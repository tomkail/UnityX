namespace UnityX.AWSBuildPipeline.Editor {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Amazon.S3.Model;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    // This is an editor window that we use for producing and uploading builds for testing and production.
    // It produces builds, performs platform specific additional tasks, and uploads builds to an S3 bucket.
    // Various input and output paths are displayed in the Info panel of this window.
    public class BuildPipelineWindow : EditorWindow {
        static AWSBuildPipelineUserSettings userSettings => AWSBuildPipelineUserSettings.Instance;
        static AWSBuildPipelineProjectSettings projectSettings => AWSBuildPipelineProjectSettings.instance;

        static bool runningPipeline;

        public static ServerHostedFileWindow uploadedFilesWindow;

        List<string> availableProfileNames;
        string connectionTestResult;

        [MenuItem("Build/Build Pipeline", false, 2400)]
        static void Init() {
            var window = GetWindow<BuildPipelineWindow>(false, "Build Pipeline");
            window.titleContent = new GUIContent("Build Pipeline");
        }

        void OnEnable() {
            availableProfileNames = AWSBuildPipelineUserSettings.GetAvailableProfileNames();
        }

        void OnFocus() {
            availableProfileNames = AWSBuildPipelineUserSettings.GetAvailableProfileNames();
        }

        void OnDisable() {
            AWSBuildPipelineUserSettings.Save();
        }

        void OnInspectorUpdate() {
            if (runningPipeline) Repaint();
        }

        void OnGUI() {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            EditorGUI.BeginChangeCheck();

            userSettings.scrollPos = EditorGUILayout.BeginScrollView(userSettings.scrollPos);

            EditorGUI.BeginDisabledGroup(runningPipeline);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            userSettings.settingsExpanded = EditorGUILayout.Foldout(userSettings.settingsExpanded, "Settings", true);
            EditorGUILayout.EndHorizontal();
            if (userSettings.settingsExpanded) {
                EditorGUI.indentLevel++;
                DrawBucketSettings();
                DrawCredentialSettings();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.Foldout(true, new GUIContent("Build Steps"), true, EditorStyles.foldout);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            foreach (var buildStep in userSettings.buildSteps) {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUI.BeginDisabledGroup(!buildStep.canBeDisabled);
                if (buildStep.canBeDisabled)
                    buildStep.enabled = EditorGUILayout.Foldout(buildStep.enabled, new GUIContent(buildStep.name), true, EditorStyles.toggle);
                else
                    EditorGUILayout.Foldout(true, new GUIContent(buildStep.name), true, EditorStyles.toggle);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                if (buildStep.expandedInSettings) {
                    EditorGUI.BeginDisabledGroup(!buildStep.enabled);
                    EditorGUI.indentLevel++;
                    buildStep.DrawSettings();
                    EditorGUI.indentLevel--;
                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUI.indentLevel--;

            DrawCommands();
            DrawInfo();

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            DrawBuild();

            if (EditorGUI.EndChangeCheck()) AWSBuildPipelineUserSettings.Save();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        void DrawBucketSettings() {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(new GUIContent("Bucket", "Saved to ProjectSettings/AWSBuildPipelineSettings.asset, shared with everyone on the project."), EditorStyles.miniBoldLabel);
            EditorGUI.BeginChangeCheck();
            projectSettings.bucketName = EditorGUILayout.TextField(new GUIContent("Bucket Name", "The S3 bucket builds are uploaded to."), projectSettings.bucketName);
            projectSettings.region = EditorGUILayout.TextField(new GUIContent("Region", "The bucket's region, e.g. eu-central-1."), projectSettings.region);
            projectSettings.serverFolderName = EditorGUILayout.TextField(new GUIContent("Folder", "The folder in the bucket that this project's builds go in. Leave empty to use '" + AWSBuildPipelineProjectSettings.GetDefaultServerFolderName() + "' (from the product name)."), projectSettings.serverFolderName);
            projectSettings.objectACL = EditorGUILayout.TextField(new GUIContent("Object ACL", "Canned ACL applied to each uploaded file, e.g. public-read. Leave empty unless ACLs are enabled on the bucket; uploads fail otherwise. Most buckets make builds public with a bucket policy instead."), projectSettings.objectACL);
            if (EditorGUI.EndChangeCheck()) projectSettings.SaveSettings();
            if (!projectSettings.isValid) EditorGUILayout.HelpBox("Set the bucket name and region.", MessageType.Error);
            EditorGUILayout.EndVertical();
        }

        void DrawCredentialSettings() {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(new GUIContent("Credentials", "Saved in EditorPrefs on this machine, never in the project."), EditorStyles.miniBoldLabel);
            userSettings.credentialSource = (AWSBuildPipelineUserSettings.CredentialSource) EditorGUILayout.EnumPopup(new GUIContent("Source", "AWS Profile reads credentials from ~/.aws (recommended: the secret stays out of Unity). Access Keys stores a key and secret in EditorPrefs."), userSettings.credentialSource);

            if (userSettings.credentialSource == AWSBuildPipelineUserSettings.CredentialSource.AWSProfile) {
                EditorGUILayout.BeginHorizontal();
                if (availableProfileNames != null && availableProfileNames.Count > 0) {
                    var index = Mathf.Max(0, availableProfileNames.IndexOf(userSettings.profileName));
                    index = EditorGUILayout.Popup("Profile", index, availableProfileNames.ToArray());
                    userSettings.profileName = availableProfileNames[index];
                } else {
                    userSettings.profileName = EditorGUILayout.TextField("Profile", userSettings.profileName);
                }
                if (GUILayout.Button("Refresh", GUILayout.Width(70))) availableProfileNames = AWSBuildPipelineUserSettings.GetAvailableProfileNames();
                EditorGUILayout.EndHorizontal();
            } else {
                userSettings.awsKeys.accessKey = EditorGUILayout.TextField("Access Key", userSettings.awsKeys.accessKey);
                userSettings.awsKeys.secretKey = EditorGUILayout.PasswordField("Secret Key", userSettings.awsKeys.secretKey);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Copy to clipboard", "Copies the key and secret as JSON, for sharing with a teammate."), GUILayout.Width(140))) {
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(userSettings.awsKeys);
                }
                if (GUILayout.Button("Paste from clipboard", GUILayout.Width(140))) {
                    try {
                        userSettings.awsKeys = JsonUtility.FromJson<S3IAMKeyParams>(GUIUtility.systemCopyBuffer);
                        GUI.changed = true;
                    } catch (Exception) {
                        Debug.LogWarning("The clipboard doesn't hold AWS keys copied from this window.");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            userSettings.GetCredentials(out var credentialsError);
            if (credentialsError != null) EditorGUILayout.HelpBox(credentialsError, MessageType.Error);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(credentialsError != null || !projectSettings.isValid);
            if (GUILayout.Button("Test Connection", GUILayout.Width(140))) _ = TestConnection();
            EditorGUI.EndDisabledGroup();
            if (connectionTestResult != null) EditorGUILayout.LabelField(connectionTestResult, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        // Lists a single key in the build folder to check the credentials can read the bucket.
        async Task TestConnection() {
            connectionTestResult = "Connecting...";
            var target = S3Target.FromSettings(out var error);
            if (target == null) {
                connectionTestResult = error;
                return;
            }
            try {
                using (var s3Client = target.CreateClient()) {
                    await s3Client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = target.bucketName, Prefix = projectSettings.ServerFolderName + "/", MaxKeys = 1 });
                }
                connectionTestResult = "Connected to " + target.bucketName + ".";
            } catch (Exception e) {
                connectionTestResult = "Failed: " + e.Message;
            }
            Repaint();
        }

        void DrawCommands() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            userSettings.commandsExpanded = EditorGUILayout.Foldout(userSettings.commandsExpanded, "Commands", true);
            EditorGUILayout.EndHorizontal();
            if (userSettings.commandsExpanded) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open uploaded files window")) {
                    if (uploadedFilesWindow == null) uploadedFilesWindow = ServerHostedFileWindow.Init();
                    uploadedFilesWindow.Show();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
        }

        void DrawInfo() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            userSettings.infoExpanded = EditorGUILayout.Foldout(userSettings.infoExpanded, "Info", true);
            EditorGUILayout.EndHorizontal();
            if (!userSettings.infoExpanded) return;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(GUI.skin.box);

            var buildTarget = userSettings.setPlatformStep.targetBuildPlatform.buildTarget;
            var buildTargetString = BuildPaths.GetBuildTargetString(buildTarget);
            var versionString = BuildInfo.Instance.version.ToString();
            var bucketName = projectSettings.bucketName;
            var region = projectSettings.region;

            if (buildTarget == BuildTarget.WebGL) {
                DrawURLPath(new GUIContent("Latest Build URL", "Path to the latest playable build"), S3Target.GetBuildURL(bucketName, region, BuildPaths.GetRelativeServerPath(BuildPaths.LatestVersionFolder, buildTargetString, "index.html")));
                DrawURLPath(new GUIContent("Version Build URL", "Path to the playable build for this version"), S3Target.GetBuildURL(bucketName, region, BuildPaths.GetRelativeServerPath(versionString, buildTargetString, "index.html")));
            } else {
                DrawURLPath(new GUIContent("Latest Build Zip", "Path to the latest build"), S3Target.GetBuildURL(bucketName, region, BuildPaths.GetRelativeServerPath(BuildPaths.LatestVersionFolder, buildTargetString, BuildPaths.GetZipFileName(versionString))));
                DrawURLPath(new GUIContent("Version Build Zip", "Path to the build for this version"), S3Target.GetBuildURL(bucketName, region, BuildPaths.GetServerRelativeBuildZipPath(versionString, buildTarget)));
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup("Build Target", buildTarget);
            EditorGUILayout.TextField("Bucket", bucketName);
            EditorGUILayout.TextField("Relative Remote Path", BuildPaths.GetRelativeServerPath(versionString, buildTargetString, "example.txt"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(2);
            DrawURLPath(new GUIContent("Remote Path Console URL", "Path to the AWS console dashboard for this version's build"), S3Target.GetAwsConsoleURL(bucketName, region, BuildPaths.GetRelativeServerPath(versionString, buildTargetString, string.Empty)));
            EditorGUILayout.Space(2);
            DrawPath(new GUIContent("Build path", "Path to the build"), BuildPaths.GetBuildPath(versionString, buildTarget) + "/");

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        void DrawBuild() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            userSettings.buildExpanded = EditorGUILayout.Foldout(userSettings.buildExpanded, "Build", true);
            EditorGUILayout.EndHorizontal();
            if (!userSettings.buildExpanded) return;

            if (!runningPipeline) {
                var col = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Build", GUILayout.Height(40))) {
                    PerformBuild();
                    GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = col;
            } else {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
                EditorGUILayout.LabelField("Build in progress!", EditorStyles.centeredGreyMiniLabel);
                foreach (var buildStep in userSettings.buildSteps) {
                    if (buildStep.enabled) buildStep.DrawProgress();
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);
        }


        void DrawURLPath(GUIContent label, string path) {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(label, path);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Copy", GUILayout.Width(42))) GUIUtility.systemCopyBuffer = path;
            if (GUILayout.Button(">", GUILayout.Width(22))) Application.OpenURL(path);
            EditorGUILayout.EndHorizontal();
        }

        void DrawPath(GUIContent label, string path) {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(label, path);
            EditorGUI.EndDisabledGroup();

            path = Path.GetDirectoryName(path);
            EditorGUI.BeginDisabledGroup(!Directory.Exists(path));
            if (GUILayout.Button(">", GUILayout.Width(22))) {
                EditorUtility.RevealInFinder(path);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        static async void PerformBuild() {
            var steps = userSettings;

            // Check we can upload before spending time on a build.
            S3Target target = null;
            if (steps.uploadToAwsBuildStep.enabled) {
                target = S3Target.FromSettings(out var error);
                if (target == null) {
                    EditorUtility.DisplayDialog("Can't upload the build", error + "\n\nFix this in the Settings section, or untick '" + steps.uploadToAwsBuildStep.name + "'.", "OK");
                    return;
                }
            }

            foreach (var buildStep in steps.buildSteps) {
                buildStep.BeginRunPipeline();
            }

            runningPipeline = true;
            try {
                steps.setBuildVersionStep.SetVersion();
                steps.setPlatformStep.SwitchToTargetPlatform();

                // Apply some settings before we start the build
                steps.createBuildStep.ApplyBuildParams(steps.setPlatformStep.targetBuildPlatform);

                AssetDatabase.SaveAssets();
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

                var buildTarget = steps.setPlatformStep.targetBuildPlatform.buildTarget;
                if (steps.createBuildStep.enabled) {
                    if (!steps.createBuildStep.Build(buildTarget, steps.runBuildStep.enabled, steps.runBuildStep.autoConnectProfiler)) return;
                }

                if (steps.uploadToAwsBuildStep.enabled) {
                    if (!await steps.uploadToAwsBuildStep.Run(target, buildTarget)) return;

                    // Run the build that's been uploaded to the server.
                    if (buildTarget == BuildTarget.WebGL && steps.runBuildStep.enabled) {
                        var buildURL = target.GetBuildURL(BuildPaths.GetRelativeServerPath(BuildPaths.LatestVersionFolder, BuildPaths.GetBuildTargetString(buildTarget), "index.html"));
                        Application.OpenURL(buildURL);
                    }
                }
            } catch (Exception e) {
                Debug.LogException(e);
            } finally {
                runningPipeline = false;
            }
        }
    }

    // Stamps BuildInfo with the version, git info, etc on every build (not just ones made from the window).
    // Runs after UnityX.Versioning's preprocessor (order 0) so BuildInfo's version is the one that ends up in PlayerSettings.
    class BuildInfoBuildPreprocessor : IPreprocessBuildWithReport {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report) {
            BuildInfo.Instance.UpdateCurrentVersion();
            EditorUtility.SetDirty(BuildInfo.Instance);
            BuildInfo.Instance.ApplyVersionToPlayerSettings();
            AssetDatabase.SaveAssets();
        }
    }
}
