using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace UnityX.Versioning.Editor {
    // This script updates all the version number fields in PlayerSettings to match the VersionManager script's version.
    // It also updates some fields on VersionManager based on the build settings, such as if the build is a Development build.
    [InitializeOnLoad]
    public class VersionBuildPreProcessor : IPreprocessBuildWithReport {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport buildReport) {
            // The asset is per-project state (it's rewritten on every build), so it lives in the project's own
            // Resources folder rather than in this package. Without one, leave PlayerSettings' version alone instead
            // of stamping the default 0.0.0.
            var versionSO = Resources.Load<CurrentVersionSO>(nameof(CurrentVersionSO));
            if (versionSO == null) {
                Debug.LogWarning($"No {nameof(CurrentVersionSO)} asset in a Resources folder; the build's version numbers aren't being set from it. Create one (Assets > Create > UnityX > Current Version) to enable this.");
                return;
            }

            // Set versions in PlayerSettings
            string versionString = versionSO.version.ToBasicVersionString();
            PlayerSettings.bundleVersion = versionString;
            // Version 1.2.45 would come out as 102045. Allows for 99 minors and 999 builds.
            var bundleVersionCode = (versionSO.version.major * 100000) + (versionSO.version.minor * 1000) + versionSO.version.build;

            {
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
            }
            
            {
                // Allow for 99 upload attempts too!
                PlayerSettings.macOS.buildNumber = bundleVersionCode.ToString()+"00";
            }
            
            // Nintendo Switch version stamping disabled in Unity 6000.5: the NMETA-based PlayerSettings.Switch
            // API (displayVersion, releaseVersion, NMETAOverrideFullPath, etc.) was removed/obsoleted with no
            // 1:1 replacement. If shipping on Switch, reimplement against the current Switch metadata workflow.
            // {
            //     PlayerSettings.Switch.displayVersion = versionString;
            //     if(System.IO.File.Exists(PlayerSettings.Switch.NMETAOverrideFullPath)) {
            //         var text = System.IO.File.ReadAllText(PlayerSettings.Switch.NMETAOverrideFullPath);
            //         var startIndex = text.IndexOf("<DisplayVersion>") + "<DisplayVersion>".Length;
            //         var endIndex = text.IndexOf("</DisplayVersion>");
            //         text = text.Remove(startIndex, endIndex-startIndex);
            //         text = text.Insert(startIndex, versionString);
            //         System.IO.File.WriteAllText(PlayerSettings.Switch.NMETAOverrideFullPath, text);
            //     }
            // }
            
            UpdateCurrentVersion(versionSO);
            EditorUtility.SetDirty(versionSO);
            AssetDatabase.SaveAssets();
        }

        public void UpdateCurrentVersion(CurrentVersionSO currentVersion) {
            if(currentVersion == null) return;
            if(VersionControlX.gitDirectory != null) {
                currentVersion.version.gitBranch = VersionControlX.GetGitBranch();
                currentVersion.version.gitCommitSHA = VersionControlX.GetGitSHA();
            }
            currentVersion.version.buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
            currentVersion.version.isDevelopment = Debug.isDebugBuild;
            currentVersion.version.buildDateTimeString = System.DateTime.Now.ToString("u");
        }
    }
}