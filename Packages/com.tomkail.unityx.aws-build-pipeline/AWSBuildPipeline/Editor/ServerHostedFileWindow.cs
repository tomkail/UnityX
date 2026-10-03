namespace UnityX.AWSBuildPipeline.Editor {
    using System.Linq;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.Threading.Tasks;

    // Lists a set of local files and their state on the server, and lets you upload/delete them individually.
    // The build pipeline opens this while uploading a build so you can watch progress.
    public class ServerHostedFileWindow : EditorWindow {
        protected S3Target target;
        protected List<ServerHostedFileStatus> allFileStatuses = new List<ServerHostedFileStatus>();
        protected ServerHostedFileStatus[] fileStatuses = new ServerHostedFileStatus[0];

        Vector2 fileListPosition;
        string searchString;


        const float fileFieldHeight = 20;
        const float extraHeight = 9;
        const float spacing = 5;

        public static ServerHostedFileWindow Init() {
            var window = (ServerHostedFileWindow) EditorWindow.GetWindow(typeof(ServerHostedFileWindow), false, "Uploaded Files", true);
            window.minSize = new Vector2(800, 600);
            return window;
        }

        public void SetFiles(S3Target target, List<ServerHostedFileStatus> fileStatuses) {
            this.target = target;
            this.allFileStatuses = fileStatuses;
            RefreshValidFileStatuses();
        }

        void OnInspectorUpdate() {
            Repaint();
        }

        void OnGUI() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField(fileStatuses.Count(x => x.status == ServerHostedFileStatus.Status.Uploaded) + "/" + fileStatuses.Length + " uploaded files");
            EditorGUILayout.LabelField(fileStatuses.Count(x => x.status == ServerHostedFileStatus.Status.QueuedForUpload) + " queued");
            EditorGUILayout.LabelField(fileStatuses.Count(x => x.status == ServerHostedFileStatus.Status.Uploading) + " uploading");

            bool changed = DrawSearchBar(ref searchString);
            if (changed) RefreshValidFileStatuses();

            EditorGUILayout.EndHorizontal();

            if (target == null) {
                EditorGUILayout.HelpBox("No files yet. This window lists the files of a build while the build pipeline uploads it.", MessageType.Info);
                return;
            }

            DrawToolbar();
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            DrawFilesInDirectory(ref fileListPosition, 100);
            EditorGUILayout.EndVertical();
        }

        protected void RefreshValidFileStatuses() {
            if (allFileStatuses != null)
                fileStatuses = allFileStatuses.Where(x => SearchStringMatch(x.relativePath, searchString)).OrderByDescending(x => x.fileInfo.LastWriteTime).ToArray();
        }

        static bool StringContains(string str, string toCheck, StringComparison comp) {
            if (toCheck.Length == 0) return false;
            return str.IndexOf(toCheck, comp) >= 0;
        }

        static GUIStyle searchTextFieldStyle;
        static GUIStyle searchCancelButtonStyle;

        static bool DrawSearchBar(ref string searchString) {
            if (searchTextFieldStyle == null) searchTextFieldStyle = EditorStyles.toolbarSearchField;
            if (searchCancelButtonStyle == null) searchCancelButtonStyle = GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? EditorStyles.toolbarButton;

            var lastString = searchString;
            searchString = GUILayout.TextField(searchString ?? string.Empty, searchTextFieldStyle);
            if (GUILayout.Button("", searchCancelButtonStyle)) {
                searchString = string.Empty;
            }

            return lastString != searchString;
        }

        static bool SearchStringMatch(string content, string searchString) {
            return string.IsNullOrWhiteSpace(searchString) || StringContains(content, searchString, StringComparison.OrdinalIgnoreCase);
        }

        static void DrawLoadableFile(S3Target target, ServerHostedFileStatus fileStatus) {
            var lastWriteTime = fileStatus.fileInfo.LastWriteTime;

            EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(fileFieldHeight));

            EditorGUILayout.LabelField(fileStatus.buildTarget, GUILayout.Width(80), GUILayout.Height(fileFieldHeight - 5));
            EditorGUI.BeginDisabledGroup(fileStatus.status != ServerHostedFileStatus.Status.Uploaded);
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow")), EditorStyles.miniButton, GUILayout.Width(24))) {
                Application.OpenURL(target.GetAwsConsoleURL(fileStatus.relativePath));
            }

            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("BuildSettings.Web.Small")), EditorStyles.miniButton, GUILayout.Width(24))) {
                Application.OpenURL(target.GetBuildURL(fileStatus.relativePath));
            }

            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_Folder Icon")), EditorStyles.miniButton, GUILayout.Width(24))) {
                EditorUtility.RevealInFinder(fileStatus.fileInfo.FullName);
            }

            EditorGUILayout.LabelField(fileStatus.relativePath, GUILayout.Width(780));

            EditorGUILayout.LabelField(new GUIContent(BuildPipelineUtils.FormatMegabytes(fileStatus.fileInfo.Length)), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(52), GUILayout.Height(fileFieldHeight - 5));
            EditorGUILayout.LabelField(new GUIContent(lastWriteTime.ToShortDateString() + " " + lastWriteTime.ToShortTimeString()), EditorStyles.miniBoldLabel, GUILayout.Width(102), GUILayout.Height(fileFieldHeight - 5));



            EditorGUI.BeginDisabledGroup(!fileStatus.requiresUpload);
            if (GUILayout.Button("Upload", EditorStyles.miniButton, GUILayout.Width(90), GUILayout.Height(fileFieldHeight - 5))) {
                _ = AWSUtils.UploadFileAsync(target.CreateClient(), target, fileStatus);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!fileStatus.existsOnServer);
            if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(90), GUILayout.Height(fileFieldHeight - 5))) {
                _ = AWSUtils.DeleteFileAsync(target.CreateClient(), target, fileStatus);
            }

            EditorGUI.EndDisabledGroup();

            if (fileStatus.status == ServerHostedFileStatus.Status.NotUploaded) {
                GUILayout.Label("Not uploaded", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            } else if (fileStatus.status == ServerHostedFileStatus.Status.Changed) {
                GUILayout.Label("Changed", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            } else if (fileStatus.status == ServerHostedFileStatus.Status.QueuedForCheck) {
                GUILayout.Label("Queued for check", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            } else if (fileStatus.status == ServerHostedFileStatus.Status.Checking) {
                GUILayout.Label("Checking...", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            } else if (fileStatus.status == ServerHostedFileStatus.Status.Deleting) {
                GUILayout.Label("Deleting...", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            } else if (fileStatus.status == ServerHostedFileStatus.Status.Uploaded) {
                GUILayout.Label("Uploaded", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            } else if (fileStatus.status == ServerHostedFileStatus.Status.QueuedForUpload) {
                GUILayout.Label("Queued for upload", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            } else if (fileStatus.status == ServerHostedFileStatus.Status.Uploading) {
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(100));
                EditorGUI.ProgressBar(rect, fileStatus.uploadTaskProgress.progress, "");
                GUI.Label(rect, "Uploading...", EditorStyles.centeredGreyMiniLabel);
            } else if (fileStatus.status == ServerHostedFileStatus.Status.Unknown) {
                GUILayout.Label("???", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            } else {
                GUILayout.Label(fileStatus.status.ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(100));
            }

            EditorGUILayout.EndHorizontal();
        }




        public void DrawToolbar() {
            EditorGUILayout.BeginHorizontal();

            var numRefreshing = fileStatuses.Count(x => x.status == ServerHostedFileStatus.Status.Checking || x.status == ServerHostedFileStatus.Status.QueuedForCheck);
            if (GUILayout.Button(numRefreshing == 0 ? "Refresh file states" : "Refreshing " + numRefreshing + " files...")) {
                _ = RecheckFileList();
            }

            var filesToUpload = fileStatuses.Where(x => x.requiresUpload).ToArray();
            EditorGUI.BeginDisabledGroup((numRefreshing > 0 || filesToUpload.Length == 0) && !Event.current.control);
            if (GUILayout.Button("Upload " + filesToUpload.Length + " files missing from server (" + BuildPipelineUtils.FormatMegabytes(filesToUpload.Sum(x => x.fileInfo.Length)) + ")")) {
                var startTime = Time.realtimeSinceStartup;
                _ = AWSUtils.UploadFilesAsync(target, filesToUpload, null, () => {
                    var endTime = Time.realtimeSinceStartup;
                    EditorUtility.DisplayDialog("Upload complete!", "Took " + (endTime - startTime) + "s", "Ok!");
                });
            }

            EditorGUI.EndDisabledGroup();

            var filesToDelete = fileStatuses.Where(x => x.existsOnServer).ToArray();
            EditorGUI.BeginDisabledGroup((numRefreshing > 0 || filesToDelete.Length == 0) && !Event.current.control);
            if (GUILayout.Button("Delete " + filesToDelete.Length + " files on server")) {
                if (EditorUtility.DisplayDialog("Delete files from the server?", "This deletes " + filesToDelete.Length + " files from " + target.bucketName + ". It can't be undone.", "Delete", "Cancel")) {
                    var startTime = Time.realtimeSinceStartup;
                    _ = AWSUtils.DeleteFiles(target, filesToDelete, () => {
                        var endTime = Time.realtimeSinceStartup;
                        EditorUtility.DisplayDialog("Delete complete!", "Took " + (endTime - startTime) + "s", "Ok!");
                    });
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        public void DrawFilesInDirectory(ref Vector2 scrollPosition, int maxNumToShow = 1) {
            if (fileStatuses == null || fileStatuses.Length == 0) return;

            maxNumToShow = Mathf.Min(maxNumToShow, fileStatuses.Length);
            float m_ItemHeight = fileFieldHeight;
            float scrollRectHeight = (maxNumToShow * m_ItemHeight) + ((maxNumToShow - 1) * spacing) + extraHeight;
            int numToShow = Mathf.CeilToInt(scrollRectHeight / m_ItemHeight);
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(false));
            int firstIndex = (int) (scrollPosition.y / m_ItemHeight);
            firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, fileStatuses.Length - numToShow));
            if (firstIndex * m_ItemHeight > 0) GUILayout.Space(firstIndex * m_ItemHeight);
            var lastIndex = Mathf.Min(fileStatuses.Length, firstIndex + numToShow);
            for (int i = firstIndex; i < lastIndex; i++) {
                DrawLoadableFile(target, fileStatuses[i]);
            }

            var numItemsOffScreenAtEnd = Mathf.Max(0, fileStatuses.Length - firstIndex - numToShow);
            if (numItemsOffScreenAtEnd * m_ItemHeight > 0) GUILayout.Space(numItemsOffScreenAtEnd * m_ItemHeight);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }



        public async Task RecheckFileList() {
            foreach (var fileStatus in fileStatuses)
                fileStatus.status = ServerHostedFileStatus.Status.QueuedForCheck;
            float time = Time.realtimeSinceStartup;
            using (var s3Client = target.CreateClient()) {
                await AWSUtils.RefreshStatusOfFiles(s3Client, target, fileStatuses);
            }
            Debug.Log("Took " + (Time.realtimeSinceStartup - time) + " to refresh state of " + fileStatuses.Length + " files");
            RefreshValidFileStatuses();
        }
    }
}
