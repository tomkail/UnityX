namespace UnityX.AWSBuildPipeline.Editor {
    using System.IO;

    public class ServerHostedFileStatus {
        public FileInfo fileInfo;
        public string fileContentEncoding; // = "br";
        public string fileContentType; // = "binary/octet-stream";

        public string buildTarget;

        // The path of the asset on the server, relative to the bucket
        public string relativePath;

        public Status status;
        public bool requiresCheck => status == ServerHostedFileStatus.Status.Unknown;
        public bool requiresUpload => status == ServerHostedFileStatus.Status.NotUploaded || status == ServerHostedFileStatus.Status.Changed;
        public bool existsOnServer => status == ServerHostedFileStatus.Status.Uploaded || status == ServerHostedFileStatus.Status.Changed;
        public AWSTaskProgress findTaskProgress;
        public AWSTaskProgress uploadTaskProgress;

        public enum Status {
            Unknown,
            QueuedForCheck,
            Checking,
            Deleting,
            NotUploaded,
            Changed,
            QueuedForUpload,
            Uploading,
            Uploaded,
        }

        public ServerHostedFileStatus(FileInfo fileInfo, string buildTarget, string relativePath) {
            this.fileInfo = fileInfo;
            this.buildTarget = buildTarget;
            this.relativePath = relativePath;
        }
    }
}
