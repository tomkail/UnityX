namespace UnityX.AWSBuildPipeline.Editor {
    using System.Linq;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [System.Serializable]
    public class SetPlatformStep : BuildPipelineStep {
        // The chosen target is saved; BuildPlatform itself (which holds a GUIContent) is looked up from the fixed list below.
        [SerializeField]
        BuildTarget savedBuildTarget = BuildTarget.NoTarget;

        public BuildPlatform targetBuildPlatform {
            get {
                var platform = buildPlatforms.FirstOrDefault(x => x.buildTarget == savedBuildTarget);
                if (platform == null) {
                    platform = buildPlatforms.FirstOrDefault(x => x.buildTarget == EditorUserBuildSettings.activeBuildTarget) ?? buildPlatforms.FirstOrDefault(x => x.isInstalled) ?? buildPlatforms[0];
                    savedBuildTarget = platform.buildTarget;
                }
                return platform;
            }
            set => savedBuildTarget = value.buildTarget;
        }

        public static readonly List<BuildPlatform> buildPlatforms = new List<BuildPlatform>() {
            new BuildPlatform(BuildTargetGroup.WebGL, BuildTarget.WebGL, new GUIContent("WebGL", "WebGL")),
            new BuildPlatform(BuildTargetGroup.iOS, BuildTarget.iOS, new GUIContent("iOS", "iOS")),
            new BuildPlatform(BuildTargetGroup.Android, BuildTarget.Android, new GUIContent("Android", "Android")),
            new BuildPlatform(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, new GUIContent("Windows", "64bit Windows")),
            new BuildPlatform(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, new GUIContent("OSX", "OSX")),
        };

        public class BuildPlatform {
            public BuildTargetGroup buildTargetGroup;
            public BuildTarget buildTarget;
            public GUIContent label;
            public bool isInstalled => UnityEditor.BuildPipeline.IsBuildTargetSupported(buildTargetGroup, buildTarget);

            public BuildPlatform(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, GUIContent label) {
                this.buildTargetGroup = buildTargetGroup;
                this.buildTarget = buildTarget;
                this.label = label;
            }
        }

        public SetPlatformStep() {
            name = "Set Platform";
            canBeDisabled = false;
        }

        public void SwitchToTargetPlatform() {
            progressTracker.Update("Switching build target...", 0);
            var buildTarget = targetBuildPlatform.buildTarget;
            EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget);
            progressTracker.Complete("Switched build target!");
        }

        public override void DrawSettings() {
            var currentPlatform = targetBuildPlatform;
            if (!currentPlatform.isInstalled) {
                EditorGUILayout.HelpBox("Target build platform doesn't seem to be installed! Please switch to one of the ones shown below.", MessageType.Error);
            }

            GUILayout.BeginHorizontal();
            for (int i = 0; i < buildPlatforms.Count; i++) {
                BuildPlatform buildPlatform = buildPlatforms[i];
                EditorGUI.BeginDisabledGroup(!buildPlatform.isInstalled);
                if (GUILayout.Toggle(buildPlatform == currentPlatform, buildPlatform.label, BuildPipelineUtils.MiniButtonGUI(i, buildPlatforms.Count))) {
                    targetBuildPlatform = buildPlatform;
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();
        }
    }
}
