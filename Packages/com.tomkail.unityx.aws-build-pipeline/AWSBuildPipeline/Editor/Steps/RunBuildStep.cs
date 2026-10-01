namespace UnityX.AWSBuildPipeline.Editor {
    using UnityEditor;
    using UnityEngine;

    [System.Serializable]
    public class RunBuildStep : BuildPipelineStep {
        public bool autoConnectProfiler = true;

        public RunBuildStep() {
            name = "Run Build";
        }

        public override void DrawSettings() {
            autoConnectProfiler = EditorGUILayout.Toggle(new GUIContent("Auto Connect Profiler", "Tick this to auto connect your profiler when the target is launched."), autoConnectProfiler);
        }
    }
}
