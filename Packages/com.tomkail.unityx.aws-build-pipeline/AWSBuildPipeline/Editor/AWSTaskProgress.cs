using System.Threading;

namespace UnityX.AWSBuildPipeline.Editor {
    public class AWSTaskProgress {
        public float progress;
        public float bytesTransferred;
        public float bytesTotal;
        public CancellationTokenSource cancellationTokenSource;

        public AWSTaskProgress() {
            this.progress = 0;
            // Linked so a code reload also cancels this task (see AWSUtils.reloadToken).
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(AWSUtils.reloadToken);
        }
    }
}
