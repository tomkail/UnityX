using System.Linq;
using Amazon.S3;
using UnityEditor;
using UnityEngine;

namespace UnityX.AWSBuildPipeline.Editor {
    // Where builds are uploaded to. Shared by everyone working on the project, so it's saved in ProjectSettings and should be committed.
    // Credentials are per-user and live in AWSBuildPipelineUserSettings instead.
    [FilePath("ProjectSettings/AWSBuildPipelineSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AWSBuildPipelineProjectSettings : ScriptableSingleton<AWSBuildPipelineProjectSettings> {
        [Tooltip("The S3 bucket builds are uploaded to.")]
        public string bucketName = string.Empty;
        [Tooltip("The bucket's region, e.g. eu-central-1.")]
        public string region = "eu-central-1";
        [Tooltip("The folder in the bucket that this project's builds go in. Defaults to the product name, lower case with underscores.")]
        public string serverFolderName = string.Empty;
        [Tooltip("Canned ACL applied to each uploaded file, e.g. public-read. Leave empty unless ACLs are enabled on the bucket; uploads fail otherwise.")]
        public string objectACL = string.Empty;

        public string ServerFolderName => string.IsNullOrWhiteSpace(serverFolderName) ? GetDefaultServerFolderName() : serverFolderName.Trim('/');
        public S3CannedACL CannedACL => string.IsNullOrWhiteSpace(objectACL) ? null : S3CannedACL.FindValue(objectACL.Trim());
        public bool isValid => !string.IsNullOrWhiteSpace(bucketName) && !string.IsNullOrWhiteSpace(region);

        public static string GetDefaultServerFolderName() {
            return new string(Application.productName.ToLowerInvariant().Select(c => char.IsWhiteSpace(c) ? '_' : c).ToArray());
        }

        public void SaveSettings() => Save(true);
    }
}
