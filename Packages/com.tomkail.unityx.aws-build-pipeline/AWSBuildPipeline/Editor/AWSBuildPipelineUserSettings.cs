using System;
using System.Collections.Generic;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using UnityEditor;
using UnityEngine;

namespace UnityX.AWSBuildPipeline.Editor {
    // Per-user settings for the build pipeline window, including AWS credentials.
    // Stored as JSON in EditorPrefs (never in the project) so credentials can't be committed by accident.
    [Serializable]
    public class AWSBuildPipelineUserSettings {
        // The key the pre-package build pipeline window used, so settings (and keys) saved by it carry over.
        static string prefsKey => $"BuildPipelineEditorWindowSettings Settings ({Application.productName})";

        public enum CredentialSource {
            // A named profile from ~/.aws/credentials or ~/.aws/config. The secret never touches Unity's prefs.
            AWSProfile,
            // An access key and secret stored in EditorPrefs on this machine.
            AccessKeys,
        }

        public CredentialSource credentialSource = CredentialSource.AWSProfile;
        public string profileName = "default";
        public S3IAMKeyParams awsKeys;

        public Vector2 scrollPos;
        public bool settingsExpanded = true;
        public bool commandsExpanded = false;
        public bool infoExpanded = false;
        public bool buildExpanded = true;

        public SetPlatformStep setPlatformStep = new SetPlatformStep();
        public SetBuildVersionStep setBuildVersionStep = new SetBuildVersionStep();
        public CreateBuildStep createBuildStep = new CreateBuildStep();
        public UploadToAwsBuildStep uploadToAwsBuildStep = new UploadToAwsBuildStep();
        public RunBuildStep runBuildStep = new RunBuildStep();

        public IEnumerable<BuildPipelineStep> buildSteps {
            get {
                yield return setPlatformStep;
                yield return setBuildVersionStep;
                yield return createBuildStep;
                yield return uploadToAwsBuildStep;
                yield return runBuildStep;
            }
        }

        static AWSBuildPipelineUserSettings _Instance;
        public static AWSBuildPipelineUserSettings Instance {
            get {
                if (_Instance == null) _Instance = Load();
                return _Instance;
            }
        }

        static AWSBuildPipelineUserSettings Load() {
            var settings = new AWSBuildPipelineUserSettings();
            if (!EditorPrefs.HasKey(prefsKey)) return settings;
            var data = EditorPrefs.GetString(prefsKey);
            try {
                JsonUtility.FromJsonOverwrite(data, settings);
                // Settings saved before profiles were supported only had keys; keep using them.
                if (!data.Contains("\"" + nameof(credentialSource) + "\"") && !settings.awsKeys.isUndefined)
                    settings.credentialSource = CredentialSource.AccessKeys;
            } catch (Exception e) {
                Debug.LogError("Build pipeline settings couldn't be parsed and were reset: " + e.Message);
                settings = new AWSBuildPipelineUserSettings();
            }
            return settings;
        }

        public static void Save() {
            if (_Instance != null) EditorPrefs.SetString(prefsKey, JsonUtility.ToJson(_Instance));
        }

        // Returns null and sets error if credentials can't be resolved.
        public AWSCredentials GetCredentials(out string error) {
            error = null;
            if (credentialSource == CredentialSource.AWSProfile) {
                if (string.IsNullOrWhiteSpace(profileName)) {
                    error = "Choose an AWS profile.";
                    return null;
                }
                if (new CredentialProfileStoreChain().TryGetAWSCredentials(profileName, out var credentials)) return credentials;
                error = $"AWS profile '{profileName}' wasn't found in ~/.aws/credentials or ~/.aws/config. Run `aws configure --profile {profileName}` to create it.";
                return null;
            } else {
                if (awsKeys.isUndefined) {
                    error = "Enter an AWS access key and secret.";
                    return null;
                }
                return new BasicAWSCredentials(awsKeys.accessKey, awsKeys.secretKey);
            }
        }

        public static List<string> GetAvailableProfileNames() {
            var names = new List<string>();
            try {
                foreach (var profile in new CredentialProfileStoreChain().ListProfiles())
                    if (!names.Contains(profile.Name)) names.Add(profile.Name);
            } catch (Exception e) {
                Debug.LogWarning("Couldn't read AWS profiles: " + e.Message);
            }
            names.Sort();
            return names;
        }
    }
}
