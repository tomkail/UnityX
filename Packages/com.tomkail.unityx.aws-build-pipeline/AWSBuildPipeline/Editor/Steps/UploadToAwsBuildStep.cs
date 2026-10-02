namespace UnityX.AWSBuildPipeline.Editor {
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.S3;
    using UnityEditor;
    using UnityEngine;

    // WebGL builds are uploaded file by file so they can be played straight from the bucket.
    // Other platforms are zipped and the zip is uploaded.
    [System.Serializable]
    public class UploadToAwsBuildStep : BuildPipelineStep {
        public bool copyIntoCurrentDirectory = true;

        public UploadToAwsBuildStep() {
            name = "Upload Build To AWS";
        }

        public override void DrawSettings() {
            copyIntoCurrentDirectory = EditorGUILayout.Toggle(new GUIContent("Copy into 'Current' directory", "Also copy the build to the '" + BuildPaths.LatestVersionFolder + "' folder, so there's a URL that always points at the latest build."), copyIntoCurrentDirectory);
        }

        // Returns true if every file was uploaded.
        public async Task<bool> Run(S3Target target, BuildTarget buildTarget) {
            var versionString = BuildInfo.Instance.version.ToString();
            var buildTargetString = BuildPaths.GetBuildTargetString(buildTarget);

            List<ServerHostedFileStatus> filesToUpload;
            if (buildTarget == BuildTarget.WebGL) {
                var buildPath = BuildPaths.GetBuildPath(versionString, buildTarget);
                CreateInfoTextFile(buildPath, BuildInfo.Instance);
                filesToUpload = GetFilesToUploadForWebGL(buildPath, versionString, buildTargetString);
            } else {
                progressTracker.Start("Zipping Build...");
                var buildDirectoryPath = BuildPaths.GetBuildDirectoryPath(versionString, buildTarget);
                CreateInfoTextFile(buildDirectoryPath, BuildInfo.Instance);
                var zipFilePath = BuildPaths.GetLocalZipPath(versionString, buildTarget);
                if (!ZipDirectory(buildDirectoryPath, zipFilePath)) {
                    progressTracker.Fail("Couldn't zip the build at " + buildDirectoryPath);
                    return false;
                }
                filesToUpload = new List<ServerHostedFileStatus> {
                    new ServerHostedFileStatus(new FileInfo(zipFilePath), buildTargetString, BuildPaths.GetServerRelativeBuildZipPath(versionString, buildTarget))
                };
            }

            using (var s3Client = target.CreateClient()) {
                if (!await UploadFiles(s3Client, filesToUpload, target, versionString, buildTargetString)) return false;
                if (copyIntoCurrentDirectory) await CopyBuildAsLatest(s3Client, target, versionString, buildTargetString);
            }
            return true;
        }

        public List<ServerHostedFileStatus> GetFilesToUploadForWebGL(string localPath, string versionString, string buildTargetString) {
            List<ServerHostedFileStatus> serverHostedFileStatusList = new List<ServerHostedFileStatus>();

            // The path to the server that we'll use as the root to upload things to
            var formattedServerPath = BuildPaths.GetRelativeServerPath(versionString, buildTargetString);

            string[] files = Directory.GetFiles(localPath, "*", SearchOption.AllDirectories);
            foreach (var filePath in files) {
                var localFilePath = filePath.Substring(localPath.Length).Replace("\\", "/");
                var remoteRelativeFilePath = formattedServerPath + localFilePath;
                var fileStatus = new ServerHostedFileStatus(new FileInfo(filePath), buildTargetString, remoteRelativeFilePath);
                var extension = Path.GetExtension(filePath);

                // FrisbeeGame.data.br
                // System defined	Content-Encoding	br
                // System defined	Content-Type	binary/octet-stream

                // FrisbeeGame.framework.js.br
                // System defined	Content-Encoding	br
                // System defined	Content-Type	application/javascript

                // FrisbeeGame.loader.js
                // System defined	Content-Type	application/javascript

                // FrisbeeGame.wasm.br
                // System defined	Content-Encoding	br
                // System defined	Content-Type	application/wasm
                {
                    if (extension == ".br") {
                        fileStatus.fileContentEncoding = "br";
                        extension = Path.GetExtension(filePath.Substring(0, filePath.Length - (".br".Length)));
                    } else if (extension == ".gz") {
                        fileStatus.fileContentEncoding = "gzip";
                        extension = Path.GetExtension(filePath.Substring(0, filePath.Length - (".gz".Length)));
                    } else if (extension == ".unityweb") {
                        // I'm not sure how this differs from "gz", but unity recommends doing it like this for .unityweb files.
                        if (PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Gzip) fileStatus.fileContentEncoding = "gzip";
                        else if (PlayerSettings.WebGL.compressionFormat == WebGLCompressionFormat.Brotli) fileStatus.fileContentEncoding = "br";
                        extension = Path.GetExtension(filePath.Substring(0, filePath.Length - (".unityweb".Length)));
                    }

                    if (extension.EndsWith(".data")) fileStatus.fileContentType = "binary/octet-stream";
                    else if (extension.EndsWith(".js")) fileStatus.fileContentType = "application/javascript";
                    else if (extension.EndsWith(".wasm")) fileStatus.fileContentType = "application/wasm";
                }
                serverHostedFileStatusList.Add(fileStatus);
            }

            return serverHostedFileStatusList;
        }

        // Returns true if every file was uploaded.
        public async Task<bool> UploadFiles(IAmazonS3 s3Client, List<ServerHostedFileStatus> serverHostedFileStatusList, S3Target target, string versionString, string buildTargetString) {
            progressTracker.Start("Uploading Build");

            // We don't need to do this, since we check if the files have changed; but it's cleaner
            var formattedServerPath = BuildPaths.GetRelativeServerPath(versionString, buildTargetString);
            await AWSUtils.DeleteDirectoryAsync(s3Client, target.bucketName, formattedServerPath);

            if (BuildPipelineWindow.uploadedFilesWindow == null) BuildPipelineWindow.uploadedFilesWindow = ServerHostedFileWindow.Init();
            BuildPipelineWindow.uploadedFilesWindow.SetFiles(target, serverHostedFileStatusList);

            foreach (var fileStatus in serverHostedFileStatusList)
                if (fileStatus.requiresCheck)
                    fileStatus.status = ServerHostedFileStatus.Status.QueuedForCheck;
            await AWSUtils.RefreshStatusOfFiles(s3Client, target, serverHostedFileStatusList, false, (progress) => { progressTracker.Update("Checking status " + (progress * 100).ToString("0") + "%", progress * 0.01f); });

            var filesToUpload = serverHostedFileStatusList.Where(x => x.requiresUpload).ToArray();
            await AWSUtils.UploadFilesAsync(target, filesToUpload, (progress) => {
                progressTracker.Update("Uploading " + (progress * 100).ToString("0") + "%", progress);
            });

            var failedCount = serverHostedFileStatusList.Count(x => x.status != ServerHostedFileStatus.Status.Uploaded);
            if (failedCount > 0) {
                progressTracker.Fail(failedCount + " files failed to upload; see the console.");
                return false;
            }
            progressTracker.Complete("Uploaded Files!");
            return true;
        }

        public async Task CopyBuildAsLatest(IAmazonS3 s3Client, S3Target target, string versionString, string buildTargetString) {
            progressTracker.Update("Copying into '" + BuildPaths.LatestVersionFolder + "'...", 1);
            var sourceDirectory = BuildPaths.GetRelativeServerPath(versionString, buildTargetString);
            var targetDirectory = BuildPaths.GetRelativeServerPath(BuildPaths.LatestVersionFolder, buildTargetString);
            await AWSUtils.CopyDirectory(s3Client, target.bucketName, sourceDirectory, target.bucketName, targetDirectory);
            progressTracker.Complete("Uploaded Files!");
        }

        static string CreateInfoTextFile(string directoryPath, BuildInfo buildInfo) {
            if (!Directory.Exists(directoryPath)) {
                Debug.LogWarning("UploadToAwsBuildStep.CreateInfoTextFile: No folder exists at " + directoryPath);
                return null;
            }

            var infoTextPath = Path.GetFullPath(Path.Combine(directoryPath, "Info.txt"));
            File.WriteAllText(infoTextPath, "Build Info\n\n" + buildInfo.ToString());
            return infoTextPath;
        }

        // Zips the contents of a folder. On macOS this uses ditto, which keeps the permissions and symlinks a .app needs to launch.
        static bool ZipDirectory(string directoryPath, string zipFilePath) {
            if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
            if (Application.platform == RuntimePlatform.OSXEditor) {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                    FileName = "/usr/bin/ditto",
                    Arguments = "-c -k --sequesterRsrc \"" + directoryPath + "\" \"" + zipFilePath + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });
                process.WaitForExit();
                return process.ExitCode == 0 && File.Exists(zipFilePath);
            } else {
                ZipFile.CreateFromDirectory(directoryPath, zipFilePath);
                return true;
            }
        }
    }
}
