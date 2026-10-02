using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityX.Versioning;
#endif

namespace UnityX.AWSBuildPipeline {
    // This is populated on build, and allows us to check the version and other info we might need after the build is created to help find bugs.
    // Lives at Resources/BuildInfo.asset in the project.
    public class BuildInfo : ScriptableObject {
        private static BuildInfo _Instance;
        public static BuildInfo Instance {
            get {
                if(_Instance == null) _Instance = Resources.Load<BuildInfo>(typeof(BuildInfo).Name);
                if(_Instance == null){
                    Debug.LogWarning("No instance of " + typeof(BuildInfo).Name + " found, using default values");
                    _Instance = ScriptableObject.CreateInstance<BuildInfo>();
                }
                return _Instance;
            }
        }
        public Version version;
        [System.Serializable]
        public struct Version {
            public int major;
            public int minor;
            public int build;

            public override string ToString () {
                return string.Format ("{0}.{1}.{2}", major, minor, build);
            }
        }

        [Space]
        // Unity's BuildTarget enum
        public string buildTarget;
        public static string BuildPlatform => Instance.buildTarget;

        [Space]
        public string builderComputerName;
        public string builderUsername;

        [Space]
        public string gitBranch;
        public string gitCommitSHA;

        [Space]
        public string buildDateTimeString;

        [Space]
        public string miscInfo;

        protected virtual void OnEnable() {
            if( _Instance == null )
                _Instance = this;
        }

        protected virtual void OnDisable () {
            if( _Instance == this )
                _Instance = null;
        }



        #if UNITY_EDITOR
        public void UpdateCurrentVersion() {
            gitBranch = VersionControlX.GetGitBranch();
            gitCommitSHA = VersionControlX.GetGitSHA();

            builderComputerName = SystemInfo.deviceName;
            builderUsername = System.Environment.UserName;

            buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();

            buildDateTimeString = System.DateTime.Now.ToString();

            miscInfo = string.Empty;
            miscInfo += "Caching: "+PlayerSettings.WebGL.dataCaching+"\n";
            miscInfo += "CompressionFormat: "+PlayerSettings.WebGL.compressionFormat+"\n";
            miscInfo += "Decompression Fallback: "+PlayerSettings.WebGL.decompressionFallback+"\n";
            miscInfo += "Exceptions: "+PlayerSettings.WebGL.exceptionSupport+"\n";
            miscInfo += "Code Optimisation: "+EditorUserBuildSettings.GetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.WebGL), "CodeOptimization");
        }

        public void ApplyVersionToPlayerSettings () {
            // Set versions in PlayerSettings
            string versionString = version.ToString();
            PlayerSettings.bundleVersion = versionString;
            // Version 1.2.45 would come out as 102045. Allows for 99 minors and 999 builds.
            var bundleVersionCode = (version.major * 100000) + (version.minor * 1000) + version.build;

            {
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            }

            {
                // Allow for 99 upload attempts too!
                PlayerSettings.macOS.buildNumber = bundleVersionCode.ToString()+"00";
            }

            PlayerSettings.iOS.buildNumber = version.build.ToString();
        }
        #endif



        public override string ToString () {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format (Application.productName+" Version {0}.{1}.{2}", version.major, version.minor, version.build));
            sb.AppendLine(Debug.isDebugBuild?"Development Build":"Production Build");
            sb.AppendLine("Build Target: "+buildTarget);
            sb.AppendLine("Build DateTime: "+buildDateTimeString);
            sb.AppendLine("Builder: "+builderUsername+" ("+builderComputerName+")");
            sb.AppendLine("Unity Version: "+Application.unityVersion);
            sb.AppendLine("Git Branch: "+gitBranch);
            sb.AppendLine("Git SHA: "+gitCommitSHA);
            sb.AppendLine("Misc Info:\n"+miscInfo);

            return sb.ToString();
        }
    }
}
