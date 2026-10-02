namespace UnityX.AWSBuildPipeline.Editor {
    using UnityEditor;
    using UnityEngine;

    [System.Serializable]
    public class SetBuildVersionStep : BuildPipelineStep {
        public bool autoIncrementBuildVersion = false;

        public SetBuildVersionStep() {
            name = "Set Version";
        }

        public void SetVersion() {
            if (autoIncrementBuildVersion) {
                BuildInfo.Instance.version.build++;
                EditorUtility.SetDirty(BuildInfo.Instance);
                BuildInfo.Instance.ApplyVersionToPlayerSettings();
                AssetDatabase.SaveAssets();
            }
        }

        public override void DrawSettings() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(BuildInfo.Instance.version.ToString() + (autoIncrementBuildVersion ? " (+1)" : string.Empty), GUILayout.Width(120));
            autoIncrementBuildVersion = GUILayout.Toggle(autoIncrementBuildVersion, "Auto-Increment Build", GUILayout.Width(160));
            if (GUILayout.Button("Increment Minor", GUILayout.Width(120))) {
                BuildInfo.Instance.version.minor++;
                BuildInfo.Instance.version.build = 0;
                EditorUtility.SetDirty(BuildInfo.Instance);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.ObjectField(BuildInfo.Instance, typeof(BuildInfo), false, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
        }
    }
}
