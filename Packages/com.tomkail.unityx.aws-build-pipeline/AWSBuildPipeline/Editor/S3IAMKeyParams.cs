namespace UnityX.AWSBuildPipeline.Editor {
    [System.Serializable]
    public struct S3IAMKeyParams {
        public string accessKey;
        public string secretKey;
        public bool isUndefined => string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey);

        public S3IAMKeyParams(string accessKey, string secretKey) {
            this.accessKey = accessKey;
            this.secretKey = secretKey;
        }
    }
}
