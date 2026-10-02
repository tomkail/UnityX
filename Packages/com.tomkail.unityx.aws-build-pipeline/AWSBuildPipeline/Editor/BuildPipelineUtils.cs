using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityX.AWSBuildPipeline.Editor {
    public static class BuildPipelineUtils {
        public static void ProgressBar (string label, float value) {
            Rect r = EditorGUILayout.BeginVertical();
            EditorGUI.ProgressBar(r, value, label);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.EndVertical();
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        public static GUIStyle MiniButtonGUI (int index, int length) {
            if(length < 2) return EditorStyles.miniButton;
            else if(index == 0) return EditorStyles.miniButtonLeft;
            else if(index == length-1) return EditorStyles.miniButtonRight;
            else return EditorStyles.miniButtonMid;
        }

        public static string FormatMegabytes (long bytes) {
            return (bytes / (1024.0 * 1024.0)).ToString("0.##") + "MB";
        }

        public static void DeleteAllContents (DirectoryInfo directoryInfo, bool alsoDeleteFolder = true) {
            if(!directoryInfo.Exists) return;
            foreach(FileInfo file in directoryInfo.GetFiles()) file.Delete();
            foreach(DirectoryInfo subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);
            if(alsoDeleteFolder) directoryInfo.Delete(true);
        }
    }

    public class BuildStageProgressTracker {
        public string label;
        public float progress;
        public State state;
        public enum State {
            Idle,
            Working,
            Complete,
            Failed,
            Skipped
        }

        public void Reset () {
            label = "";
            progress = 0;
            state = State.Idle;
        }
        public void Start (string label) {
            this.label = label;
            state = State.Working;
        }
        public void Update (string label, float progress) {
            this.label = label;
            this.progress = progress;
            state = State.Working;
        }
        public void Complete (string label) {
            this.label = label;
            progress = 1;
            state = State.Complete;
        }
        public void Fail (string label) {
            this.label = label;
            progress = 1;
            state = State.Failed;
        }
        public void Skip () {
            label = "Skipped";
            progress = 1;
            state = State.Skipped;
        }
    }
}
