using UnityEditor;
using UnityEngine;

namespace UnityX.AWSBuildPipeline.Editor {
    [System.Serializable]
    public class BuildPipelineStep {
        public string name;
        public bool canBeDisabled = true;
        public bool enabled = true;
        public bool expandedInSettings = true;
        // Transient build-run state, not part of the saved pipeline settings.
        [System.NonSerialized]
        BuildStageProgressTracker _progressTracker;
        public BuildStageProgressTracker progressTracker => _progressTracker ?? (_progressTracker = new BuildStageProgressTracker());

        public virtual void BeginRunPipeline() {
            progressTracker.Reset();
        }

        public virtual void DrawSettings() {
        }

        public virtual void DrawProgress() {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(name);
            BuildPipelineUtils.ProgressBar(progressTracker.label, progressTracker.progress);
            GUILayout.EndHorizontal();
        }
    }
}
