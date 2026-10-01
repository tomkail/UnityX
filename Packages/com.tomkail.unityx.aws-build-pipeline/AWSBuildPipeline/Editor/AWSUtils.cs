// Originally based on code copyright 2019 The Gamedev Guru (http://thegamedev.guru)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
namespace UnityX.AWSBuildPipeline.Editor {
    using Amazon.S3;
    using Amazon.S3.Transfer;
    using Amazon.S3.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    // Note on AWS SDK v4: collections on responses are null rather than empty when there's nothing in them,
    // and value-type properties (IsTruncated, LastModified, etc) are nullable.
    public static class AWSUtils {
        // When uploading too much at once (I had around 1100 when I first noticed the error), AWS seems to throw an error. I've fixed it by limiting the number of files uploaded in one go.
        // I suspect the error was actually caused by the size of the files, rather than the quantity, but this works.
        const int maxSimultaneousRequests = 500;

        static string AsFolderPrefix(string keyPrefix) => keyPrefix.TrimEnd('/') + "/";

        // Deletes all objects under a folder, recursively.
        public static async Task DeleteDirectoryAsync(IAmazonS3 s3Client, string bucketName, string keyPrefix) {
            var listRequest = new ListObjectsV2Request {
                BucketName = bucketName,
                Prefix = AsFolderPrefix(keyPrefix)
            };
            ListObjectsV2Response response;
            do {
                // Each page holds at most 1000 keys, which is also the most DeleteObjects accepts.
                response = await s3Client.ListObjectsV2Async(listRequest);
                if (response.S3Objects != null && response.S3Objects.Count > 0) {
                    await s3Client.DeleteObjectsAsync(new DeleteObjectsRequest {
                        BucketName = bucketName,
                        Objects = response.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
                    });
                }
                listRequest.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated == true);
        }

        // Copies every object under one folder into another. Objects keep their metadata (Content-Type, Content-Encoding).
        public static async Task CopyDirectory(IAmazonS3 s3Client, string sourceBucket, string sourceKey, string destinationBucket, string destinationKey) {
            sourceKey = AsFolderPrefix(sourceKey);
            destinationKey = AsFolderPrefix(destinationKey);
            try {
                var listRequest = new ListObjectsV2Request {
                    BucketName = sourceBucket,
                    Prefix = sourceKey
                };
                ListObjectsV2Response response;
                do {
                    response = await s3Client.ListObjectsV2Async(listRequest, CancellationToken.None);
                    if (response.S3Objects != null) {
                        await ForEachThrottled(response.S3Objects, obj => s3Client.CopyObjectAsync(new CopyObjectRequest {
                            SourceBucket = sourceBucket,
                            SourceKey = obj.Key,
                            DestinationBucket = destinationBucket,
                            DestinationKey = destinationKey + obj.Key.Substring(sourceKey.Length)
                        }));
                    }
                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated == true);
            } catch (AmazonS3Exception e) {
                Debug.LogError(e);
            }
        }



        public static ServerHostedFileStatus.Status GetStatusFromLocalAndRemoteFiles(ServerHostedFileStatus fileStatus, GetObjectMetadataResponse obj) {
            if (obj == null) return ServerHostedFileStatus.Status.NotUploaded;
            // S3 infers a content type for files where we don't set one, so only compare it when we do.
            var contentTypeChanged = !string.IsNullOrEmpty(fileStatus.fileContentType) && fileStatus.fileContentType != obj.Headers.ContentType;
            var remoteLastModified = obj.LastModified?.ToUniversalTime();
            if (
                fileStatus.fileContentEncoding != obj.Headers.ContentEncoding ||
                contentTypeChanged ||
                fileStatus.fileInfo.Length != obj.Headers.ContentLength ||
                (remoteLastModified.HasValue && fileStatus.fileInfo.LastWriteTimeUtc > remoteLastModified.Value)
            ) {
                return ServerHostedFileStatus.Status.Changed;
            } else {
                return ServerHostedFileStatus.Status.Uploaded;
            }
        }



        public static async Task RefreshStatusOfFiles(IAmazonS3 s3Client, S3Target target, IList<ServerHostedFileStatus> fileStatuses, bool forceCheck = false, Action<float> onUpdateProgress = null) {
            await ForEachThrottled(fileStatuses, fileStatus => CheckFileUploadedStatus(s3Client, target, fileStatus, forceCheck, () => {
                if (onUpdateProgress != null) onUpdateProgress(fileStatuses.Sum(x => x.findTaskProgress == null ? 0 : x.findTaskProgress.progress) / fileStatuses.Count);
            }));
        }

        public static async Task CheckFileUploadedStatus(IAmazonS3 s3Client, S3Target target, ServerHostedFileStatus fileStatus, bool forceCheck = false, Action onComplete = null) {
            if (fileStatus.status == ServerHostedFileStatus.Status.Unknown || fileStatus.status == ServerHostedFileStatus.Status.QueuedForCheck || forceCheck) {
                fileStatus.status = ServerHostedFileStatus.Status.Checking;
                var remoteFileMetadata = await FindRemoteFile(s3Client, target.bucketName, fileStatus);
                fileStatus.status = GetStatusFromLocalAndRemoteFiles(fileStatus, remoteFileMetadata);
            }

            if (onComplete != null) onComplete();
        }

        // Returns null if the file isn't on the server.
        public static async Task<GetObjectMetadataResponse> FindRemoteFile(IAmazonS3 s3Client, string bucketName, ServerHostedFileStatus fileStatus) {
            fileStatus.findTaskProgress = new AWSTaskProgress();
            GetObjectMetadataResponse response = null;
            try {
                response = await s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest {
                    BucketName = bucketName,
                    Key = fileStatus.relativePath
                });
            } catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound) {
            } catch (Exception e) {
                // Treated as missing, so the file gets uploaded again (and the upload reports anything that's really wrong).
                Debug.LogError("Couldn't check " + fileStatus.relativePath + " on the server: " + e.Message);
            }

            fileStatus.findTaskProgress.progress = 1;
            return response;
        }



        public static async Task DeleteFiles(S3Target target, IEnumerable<ServerHostedFileStatus> files, Action OnComplete = null) {
            files = files.ToList();
            Debug.Log("Deleting " + files.Count() + " files...");
            foreach (var fileStatus in files) {
                fileStatus.status = ServerHostedFileStatus.Status.Deleting;
            }

            using (var s3Client = target.CreateClient()) {
                await ForEachThrottled(files, fileStatus => DeleteFileAsync(s3Client, target, fileStatus));
            }

            Debug.Log("Deleted " + files.Count() + " files");
            if (OnComplete != null) OnComplete();
        }

        public static async Task DeleteFileAsync(IAmazonS3 s3Client, S3Target target, ServerHostedFileStatus fileStatus) {
            try {
                fileStatus.status = ServerHostedFileStatus.Status.Deleting;
                await s3Client.DeleteObjectAsync(new DeleteObjectRequest {
                    BucketName = target.bucketName,
                    Key = fileStatus.relativePath
                });
                fileStatus.status = ServerHostedFileStatus.Status.NotUploaded;
            } catch (Exception e) {
                fileStatus.status = ServerHostedFileStatus.Status.Unknown;
                Debug.LogError("Couldn't delete " + fileStatus.relativePath + " from the server: " + e.Message);
            }
        }



        public static async Task UploadFilesAsync(S3Target target, IEnumerable<ServerHostedFileStatus> files, Action<float> OnUpdateProgress, Action OnComplete = null) {
            files = files.ToList();
            foreach (var fileStatus in files) {
                fileStatus.status = ServerHostedFileStatus.Status.QueuedForUpload;
            }

            var totalBytes = files.Sum(x => x.fileInfo.Length);

            void ProgressCallback(UploadProgressArgs uploadProgressArgs) {
                if (OnUpdateProgress != null && totalBytes > 0) OnUpdateProgress(files.Sum(x => x.uploadTaskProgress == null ? 0 : x.uploadTaskProgress.bytesTransferred) / totalBytes);
            }

            using (var s3Client = target.CreateClient())
            using (var transferUtility = new TransferUtility(s3Client)) {
                await ForEachThrottled(files, fileStatus => UploadFileAsync(transferUtility, target, fileStatus, ProgressCallback));
            }

            if (OnComplete != null) OnComplete();
        }

        public static async Task UploadFileAsync(IAmazonS3 s3Client, S3Target target, ServerHostedFileStatus fileStatus, Action<UploadProgressArgs> progressCallback = null) {
            using (var transferUtility = new TransferUtility(s3Client)) {
                await UploadFileAsync(transferUtility, target, fileStatus, progressCallback);
            }
        }

        public static async Task UploadFileAsync(TransferUtility transferUtility, S3Target target, ServerHostedFileStatus fileStatus, Action<UploadProgressArgs> progressCallback = null) {
            fileStatus.uploadTaskProgress = new AWSTaskProgress();
            try {
                fileStatus.status = ServerHostedFileStatus.Status.Uploading;

                var transferUtilityRequest = new TransferUtilityUploadRequest {
                    BucketName = target.bucketName,
                    FilePath = fileStatus.fileInfo.FullName,
                    Key = fileStatus.relativePath,
                };
                // This only works when ACLs are enabled on the bucket
                if (target.cannedACL != null) transferUtilityRequest.CannedACL = target.cannedACL;
                if (!string.IsNullOrEmpty(fileStatus.fileContentEncoding)) transferUtilityRequest.Headers.ContentEncoding = fileStatus.fileContentEncoding;
                if (!string.IsNullOrEmpty(fileStatus.fileContentType)) transferUtilityRequest.Headers.ContentType = fileStatus.fileContentType;

                transferUtilityRequest.UploadProgressEvent += (object sender, UploadProgressArgs args) => {
                    fileStatus.uploadTaskProgress.progress = args.PercentDone / 100f;
                    fileStatus.uploadTaskProgress.bytesTransferred = args.TransferredBytes;
                    fileStatus.uploadTaskProgress.bytesTotal = args.TotalBytes;
                    if (progressCallback != null) progressCallback(args);
                };

                await transferUtility.UploadAsync(transferUtilityRequest, fileStatus.uploadTaskProgress.cancellationTokenSource.Token);
                fileStatus.status = ServerHostedFileStatus.Status.Uploaded;
            } catch (Exception e) {
                fileStatus.status = ServerHostedFileStatus.Status.NotUploaded;
                Debug.LogError("Couldn't upload " + fileStatus.relativePath + ": " + e.Message);
            }
        }



        // Runs an async action for each item, with at most maxSimultaneousRequests running at once.
        static async Task ForEachThrottled<T>(IEnumerable<T> items, Func<T, Task> action) {
            using (var semaphore = new SemaphoreSlim(maxSimultaneousRequests)) {
                await Task.WhenAll(items.Select(async item => {
                    await semaphore.WaitAsync();
                    try {
                        await action(item);
                    } finally {
                        semaphore.Release();
                    }
                }));
            }
        }
    }
}
