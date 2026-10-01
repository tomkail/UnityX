using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityX.AWSBuildPipeline.Editor {
    // Local build output paths and remote (bucket-relative) paths.
    //
    // Local:  <project>/Builds/<version>/<BuildTarget>/...
    // Remote: <server folder>/<version or "current">/<buildtarget>/...
    public static class BuildPaths {
        public const string LatestVersionFolder = "current";

        public static string defaultDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds"));

        public static string GetBuildTargetString(BuildTarget buildTarget) {
            return buildTarget.ToString().ToLower();
        }

        // The folder everything for a build goes in.
        public static string GetBuildDirectoryPath(string versionString, BuildTarget buildTarget) {
            var buildDirectoryPath = Path.Combine(defaultDir, versionString);
            buildDirectoryPath = Path.GetFullPath(Path.Combine(buildDirectoryPath, buildTarget.ToString()));
            return buildDirectoryPath.Replace("\\", "/");
        }

        // The path passed to BuildPipeline.BuildPlayer.
        // On Windows we build an extra folder to contain the build, since additional files are placed in the same folder.
        // On OSX the .app is basically a folder in itself and on other platforms builds output to a folder
        public static string GetBuildPath(string versionString, BuildTarget buildTarget) {
            string buildDirectoryPath = GetBuildDirectoryPath(versionString, buildTarget);
            string buildPath;
            var appName = PlayerSettings.productName.Replace(" ", "_");
            if (buildTarget == BuildTarget.StandaloneWindows || buildTarget == BuildTarget.StandaloneWindows64) {
                buildPath = Path.Combine(buildDirectoryPath, appName, Path.ChangeExtension(appName, ".exe"));
            } else if (buildTarget == BuildTarget.StandaloneOSX) {
                buildPath = Path.Combine(buildDirectoryPath, Path.ChangeExtension(appName, ".app"));
            } else if (buildTarget == BuildTarget.Android) {
                buildPath = Path.Combine(buildDirectoryPath, Path.ChangeExtension(appName, EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk"));
            } else if (buildTarget == BuildTarget.WebGL) {
                buildPath = buildDirectoryPath;
            } else {
                buildPath = Path.Combine(buildDirectoryPath, appName);
            }

            return buildPath.Replace("\\", "/");
        }

        // Where a non-WebGL build is zipped to before uploading. Sits next to the build folder, not in it.
        public static string GetLocalZipPath(string versionString, BuildTarget buildTarget) {
            return Path.Combine(defaultDir, versionString, AWSBuildPipelineProjectSettings.instance.ServerFolderName + "_" + versionString + "_" + GetBuildTargetString(buildTarget) + ".zip").Replace("\\", "/");
        }

        public static string GetRelativeServerPath(string versionString, string buildTargetString, string filePath = null) {
            var path = AWSBuildPipelineProjectSettings.instance.ServerFolderName;
            if (versionString != null) path += "/" + versionString;
            if (buildTargetString != null) path += "/" + buildTargetString;
            if (filePath != null) path += "/" + filePath;
            return path;
        }

        public static string GetZipFileName(string versionString) {
            return AWSBuildPipelineProjectSettings.instance.ServerFolderName + "_" + versionString + ".zip";
        }

        public static string GetServerRelativeBuildZipPath(string versionString, BuildTarget buildTarget) {
            return GetRelativeServerPath(versionString, GetBuildTargetString(buildTarget), GetZipFileName(versionString));
        }

        // Utility to get a path relative to another path
        public static string GetRelativePath(string path, string rootPath) {
            return path.Substring(rootPath.Length).Replace("\\", "/");
        }
    }
}
