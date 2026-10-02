using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace UnityX.AWSBuildPipeline.Editor {
    // A bucket plus the credentials used to reach it.
    public class S3Target {
        public readonly AWSCredentials credentials;
        public readonly string bucketName;
        public readonly string regionString;
        // Only applied when ACLs are enabled on the bucket; null otherwise.
        public readonly S3CannedACL cannedACL;

        public S3Target(AWSCredentials credentials, string bucketName, string regionString, S3CannedACL cannedACL) {
            this.credentials = credentials;
            this.bucketName = bucketName;
            this.regionString = regionString;
            this.cannedACL = cannedACL;
        }

        // Returns null and sets error if the project settings or the user's credentials aren't set up.
        public static S3Target FromSettings(out string error) {
            var projectSettings = AWSBuildPipelineProjectSettings.instance;
            if (!projectSettings.isValid) {
                error = "Set the bucket name and region.";
                return null;
            }
            var credentials = AWSBuildPipelineUserSettings.Instance.GetCredentials(out error);
            if (credentials == null) return null;
            return new S3Target(credentials, projectSettings.bucketName.Trim(), projectSettings.region.Trim(), projectSettings.CannedACL);
        }

        public AmazonS3Client CreateClient() {
            return new AmazonS3Client(credentials, new AmazonS3Config {
                RegionEndpoint = RegionEndpoint.GetBySystemName(regionString),
            });
        }

        public string GetAwsConsoleURL(string relativePath) => GetAwsConsoleURL(bucketName, regionString, relativePath);
        public string GetBuildURL(string relativePath) => GetBuildURL(bucketName, regionString, relativePath);

        public static string GetAwsConsoleURL(string bucketName, string regionString, string relativePath) {
            return "https://s3.console.aws.amazon.com/s3/buckets/" + bucketName + "?region=" + regionString + "&prefix=" + relativePath + "&showversions=false";
        }

        public static string GetBuildURL(string bucketName, string regionString, string relativePath) {
            return "https://" + bucketName + ".s3." + regionString + ".amazonaws.com/" + relativePath;
        }
    }
}
