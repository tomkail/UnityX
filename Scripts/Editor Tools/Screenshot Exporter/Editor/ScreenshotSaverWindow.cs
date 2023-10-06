#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class ScreenshotSaverWindowProperties {
	public string folderRoot = "Screenshots";
	
	public bool openSavePrompt = false;
	public ScreenshotExportFormat exportFormat = ScreenshotExportFormat.PNG;
	public int jpegQuality = 75;
	
	public KeyCode captureKeycode = KeyCode.F5;
	
	
	public ScreenshotSaverTextureFormat textureFormat = ScreenshotSaverTextureFormat.RGB24;
	
	public List<ScreenshotResolution> resolutions;
	public int currentResolutionIndex;
	
	
	public ScreenshotResolution currentResolution {
		get {
			if(resolutions.IsNullOrEmpty()) {
				CreateDefaultResolutions();
			}
			return !resolutions.ContainsIndex(currentResolutionIndex) ? null : resolutions[currentResolutionIndex];
			
		}
	}
	
	public ScreenshotResolution customResolution {
		get {
			if(resolutions.IsNullOrEmpty()) {
				CreateDefaultResolutions();
			}
			return resolutions[0].name != "Custom" ? null : resolutions[0];
		} set {
			if(resolutions[0].name == "Custom")
				resolutions[0] = value;
		}
	}
	
	private void CreateDefaultResolutions () {
		resolutions = new List<ScreenshotResolution>();
		resolutions.Add (new ScreenshotResolution("Custom", 0, 0));
		resolutions.Add (new ScreenshotResolution("Game View", 0, 0));
		resolutions.Add (CommonScreenshotResolutions.Create1280x720());
		resolutions.Add (CommonScreenshotResolutions.Create1920x1080());
		resolutions.Add (CommonScreenshotResolutions.Create3840x2160());
		resolutions.Add (CommonScreenshotResolutions.Create7680x4320());
		resolutions.Add (CommonScreenshotResolutions.CreateiPhone4());
		resolutions.Add (CommonScreenshotResolutions.CreateiPhone5());
		resolutions.Add (CommonScreenshotResolutions.CreateiPhone6());
		currentResolutionIndex = 2;
	}
}

public class ScreenshotSaverWindow : EditorWindow {
	static ScreenshotSaverWindow Instance;
	private string propertiesKey {
		get {
			return Application.productName+"_ScreenshotSaverProperties";
		}
	}
	private ScreenshotSaverWindowProperties properties;
	
	private string folderRoot {
		get {
			return properties.folderRoot;
		} set {
			if(properties.folderRoot == value) return;
			properties.folderRoot = value;
		}
	}
	
	private string defaultPath {
		get {
			return Directory.GetCurrentDirectory()+"/"+folderRoot;
		}
	}
	
	private string automaticFileName {
		get {
			return string.Format("{0}_{1}", Application.productName, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		}
	}
	
	private string automaticFilePathAndName {
		get {
			return defaultPath+"/"+automaticFileName;
		}
	}
	
	private bool openSavePrompt {
		get {
			return properties.openSavePrompt;
		} set {
			if(properties.openSavePrompt == value) return;
			properties.openSavePrompt = value;
			SaveProperties();
		}
	}
	
	public ScreenshotSaverTextureFormat textureFormat {
		get {
			return properties.textureFormat;
		} set {
			if(properties.textureFormat == value) return;
			properties.textureFormat = value;
			SaveProperties();
		}
	}
	
	public ScreenshotExportFormat exportFormat {
		get {
			return properties.exportFormat;
		} set {
			if(properties.exportFormat == value) return;
			properties.exportFormat = value;
			SaveProperties();
		}
	}
	public int jpegQuality {
		get {
			return properties.jpegQuality;
		} set {
			if(properties.jpegQuality == value) return;
			properties.jpegQuality = value;
			SaveProperties();
		}
	}
	
	private bool useAllCameras = true;
	private List<Camera> cameras = new List<Camera>();

	private bool retinaGameViewResolution = true;

	private List<ScreenshotResolution> resolutions {
		get {
			return properties.resolutions;
		} set {
			if(properties.resolutions == value) return;
			properties.resolutions = value;
			SaveProperties();
		}
	}
	
	private ScreenshotResolution currentResolution {
		get {
			return properties.currentResolution;
		}
	}

	private int currentResolutionIndex {
		get {
			return properties.currentResolutionIndex;
		} set {
			if(properties.currentResolutionIndex == value) return;
			properties.currentResolutionIndex = value;
			SaveProperties();
		}
	}
	private ScreenshotResolution customResolution {
		get {
			return properties.customResolution;
		} set {
			if(properties.customResolution == value) return;
			properties.customResolution = value;
			SaveProperties();
		}
	}
	
	private KeyCode captureKeycode {
		get {
			return properties.captureKeycode;
		} set {
			if(properties.captureKeycode == value) return;
			properties.captureKeycode = value;
			SaveProperties();
		}
	}
	
	private Vector2 scrollPosition = Vector2.zero;
	private bool expandCameraSection = true;
	private bool expandFormatSection = true;
	private bool expandResolutionSection = true;
	private bool expandExportSection = true;
	
	[MenuItem("Window/Screenshotter %t", false, 2400)]
	static void Init () {
		ScreenshotSaverWindow window = EditorWindow.GetWindow(typeof(ScreenshotSaverWindow), false, "Screenshot Saver") as ScreenshotSaverWindow;
		window.minSize = new Vector2(260, 320);
		window.titleContent = new GUIContent("Screenshot Saver");
		window.TryLoadProperties();
	}
	
    ScreenshotSaverWindow () {
        Instance = this;
        EditorApplication.update += GameUpdate;
    }

	public void TryLoadProperties () {
		if(properties != null) return;
		if(EditorPrefs.HasKey(propertiesKey)) {
			string propertiesJSONString = EditorPrefs.GetString(propertiesKey);
			properties = JsonUtility.FromJson<ScreenshotSaverWindowProperties>(propertiesJSONString);
		} else {
			properties = new ScreenshotSaverWindowProperties();
			SaveProperties();
		}
	}
	
	void SaveProperties () {
		string propertiesJSONString = JsonUtility.ToJson(properties);
		EditorPrefs.SetString(propertiesKey, propertiesJSONString);
	}
	
	void OnCaptureScreenshot (Texture2D screenshot) {
		ScreenshotExportSettings exportSettings = null;
		string fileName = "";
		fileName = string.Format("{0}_{1}", Application.productName, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		
		if(exportFormat == ScreenshotExportFormat.JPEG) {
			exportSettings = new ScreenshotExportSettings(screenshot, defaultPath, fileName, exportFormat, jpegQuality, openSavePrompt);
		} else if(exportFormat == ScreenshotExportFormat.PNG) {
			exportSettings = new ScreenshotExportSettings(screenshot, defaultPath, fileName, exportFormat, openSavePrompt);
		}
		ScreenshotExporter.Export(exportSettings);
	}
	
	void Update () {
		TryLoadProperties();
	}

    static void GameUpdate () {
		Instance.TryLoadProperties();
		if(!Application.isPlaying || ScreenshotCapturer.capturingScreenshot || Instance.cameras.Count == 0) return;
		// KeyDown never works here. This is nice in some ways since you can hold to make a movie!
        if(Input.GetKey(Instance.captureKeycode))
			Instance.CaptureScreenshot();
    }
	
	void OnGUI () {
		Repaint();
		
		TryLoadProperties();
		
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		GUILayout.Label("");
		if(GUILayout.Button ("Reset", EditorStyles.toolbarButton)) {
			properties = new ScreenshotSaverWindowProperties();
			SaveProperties();
		}
		
		EditorGUILayout.EndHorizontal();
		
		DrawCapturePanel();

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		DrawCaptureKeycodePanel();
		DrawCameraPanel();
		DrawFormatPanel();
		DrawResolutionPanel();
		DrawExportPanel();
		
		EditorGUILayout.EndScrollView();
	}
	
	private void DrawCapturePanel () {
		GUILayout.BeginVertical(GUI.skin.box);
		
		if(!Application.isPlaying || ScreenshotCapturer.capturingScreenshot || cameras.Count == 0) {
			if(!Application.isPlaying) EditorGUILayout.HelpBox("Screenshots can only be taken in play mode.", MessageType.Info);
			else if(!Application.isPlaying) EditorGUILayout.HelpBox("Capturing...", MessageType.Info);
			else if(!Application.isPlaying) EditorGUILayout.HelpBox("No cameras selected.", MessageType.Info);
			GUI.enabled = false;
		}
		if(GUILayout.Button("Capture Screenshot", GUILayout.MinHeight(40))) {
			CaptureScreenshot();
		}
		GUI.enabled = true;
		
		GUILayout.EndVertical();
	}

	void DrawCaptureKeycodePanel () {
		GUILayout.BeginVertical(GUI.skin.box);

		captureKeycode = (KeyCode)EditorGUILayout.EnumPopup("Capture Keycode", captureKeycode);
		if(SystemInfoX.IsMacOS && KeyCodeIsFKey(captureKeycode)) {
			EditorGUILayout.HelpBox("CMD+Shift+"+captureKeycode, MessageType.Info);
		}

		GUILayout.EndVertical();
	}

	private string[] GetResolutionNames () {
		string[] names = new string[resolutions.Count];
		for(int i = 0; i < names.Length; i++) {
			names[i] = resolutions[i].name;
		}
		return names;
	}

	void SetScreenWidthAndHeightFromEditorGameViewViaReflection(ref int screenHeight, ref int screenWidth) {
		var gameView = GetMainGameView();
		var prop = gameView.GetType().GetProperty("targetSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		var gvsize = prop.GetValue(gameView, new object[0]{});
		var gvSizeType = gvsize.GetType();

		screenHeight = (int)((Single)gvSizeType.GetField("x", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(gvsize));
		screenWidth = (int)((Single)gvSizeType.GetField("y", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(gvsize));
	}

	UnityEditor.EditorWindow GetMainGameView() {
		System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
		System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		System.Object Res = GetMainGameView.Invoke(null,null);
		return (UnityEditor.EditorWindow)Res;
	}

	private void DrawResolutionPanel () {
		EditorGUILayout.BeginVertical(GUI.skin.box);
		expandResolutionSection = EditorGUILayout.Foldout(expandResolutionSection, "Resolution");

		if(currentResolution.name == "Game View") {
			if(EditorGUIUtility.pixelsPerPoint > 1)
				retinaGameViewResolution = EditorGUILayout.Toggle("Retina", retinaGameViewResolution);
			RefreshGameViewResolution();
		}

		if(expandResolutionSection) {
			EditorGUI.indentLevel++;
			EditorGUI.BeginChangeCheck();
			ScreenshotResolution lastResolution = currentResolution;
			currentResolutionIndex = EditorGUILayout.Popup (currentResolutionIndex, GetResolutionNames());
			if(EditorGUI.EndChangeCheck()) {
				OnResolutionTypeChanged (lastResolution, currentResolution);
			}
			int captureWidth = currentResolution.width;
			int captureHeight = currentResolution.height;
			EditorGUI.BeginChangeCheck();
			captureWidth = EditorGUILayout.IntField("Width", captureWidth);
			captureHeight = EditorGUILayout.IntField("Height", captureHeight);
			if(EditorGUI.EndChangeCheck()) {
				OnResolutionChanged (captureWidth, captureHeight);
			}
			EditorGUI.indentLevel--;
		}
		
		GUILayout.EndVertical();
	}
	
	private void DrawCameraPanel () {
		GUILayout.BeginVertical(GUI.skin.box);
		expandCameraSection = EditorGUILayout.Foldout(expandCameraSection, "Cameras");
		if(expandCameraSection) {
			EditorGUI.indentLevel++;
			useAllCameras = EditorGUILayout.Toggle("Use All Cameras", useAllCameras);
			if(useAllCameras) {
				cameras = Camera.allCameras.ToList();
			} else {
				DrawCustomCameraSelectionList();
			}
			if(cameras.IsNullOrEmpty()) {
				EditorGUILayout.HelpBox("Must be capturing at least one camera", MessageType.Warning);
			}
			EditorGUI.indentLevel--;
		}
		GUILayout.EndVertical();
	}
	
	private void DrawFormatPanel () {
		GUILayout.BeginVertical(GUI.skin.box);
		expandFormatSection = EditorGUILayout.Foldout(expandFormatSection, "Format");
		if(expandFormatSection) {
			EditorGUI.indentLevel++;
			textureFormat = (ScreenshotSaverTextureFormat)EditorGUILayout.EnumPopup("Texture Format", textureFormat);
			EditorGUILayout.HelpBox(ScreenshotSaverTextureFormatUtils.FormatToInfoMessage(textureFormat), MessageType.None);
			EditorGUI.indentLevel--;
		}
		GUILayout.EndVertical();
	}
	
	private void DrawCustomCameraSelectionList() {
		Camera[] gameCameras = Camera.allCameras;
		gameCameras = gameCameras.OrderBy(camera => camera.depth).ToArray();
		for(int i = 0; i < gameCameras.Length; i++) {
			EditorGUI.BeginChangeCheck();
			string name = gameCameras[i].gameObject.name+" ("+(gameCameras[i].depth)+")";
			if(gameCameras[i] == Camera.main) {
				name += " (Main)";
			} else if(gameCameras[i] == Camera.current) {
				name += " (Current)";
			}
			bool include = EditorGUILayout.Toggle(name, cameras.Contains(gameCameras[i]));
			if(EditorGUI.EndChangeCheck()) {
				if(include) {
					cameras.Add (gameCameras[i]);
				} else {
					cameras.Remove (gameCameras[i]);
				}
			}
		}
	}
	
	void DrawExportPanel () {
		EditorGUILayout.BeginVertical(GUI.skin.box);
		expandExportSection = EditorGUILayout.Foldout(expandExportSection, "Export");
		if(expandExportSection) {
			EditorGUI.indentLevel++;
			
			GUILayout.BeginVertical(GUI.skin.box);
			exportFormat = (ScreenshotExportFormat)GUILayout.Toolbar ((int)exportFormat, new string[] {"PNG", "JPEG"});
			
			if(exportFormat == ScreenshotExportFormat.JPEG)
				jpegQuality = EditorGUILayout.IntSlider ("JPEG Quality: ", jpegQuality, 0, 100);
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical(GUI.skin.box);
			folderRoot = EditorGUILayout.TextField ("Folder Root", folderRoot);
			openSavePrompt = EditorGUILayout.Toggle ("Open Save Prompt", openSavePrompt);
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.BeginHorizontal();
			
			EditorGUILayout.SelectableLabel (defaultPath, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
			if(EditorGUIUtility.systemCopyBuffer == defaultPath){
 				GUI.enabled = false;
 			}
			if(GUILayout.Button(EditorGUIUtility.systemCopyBuffer == defaultPath ? "Copied" : "Copy")) {
				EditorGUIUtility.systemCopyBuffer = defaultPath;
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			if(Directory.Exists(defaultPath)) {
				if(GUILayout.Button("Open Folder")) {
					EditorUtility.RevealInFinder(defaultPath);
				}
			} else {
				if(GUILayout.Button("Create Folder")) {
					Directory.CreateDirectory(defaultPath);
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}
	
	private void OnResolutionTypeChanged (ScreenshotResolution lastResolution, ScreenshotResolution currentResolution) {
	}
	
	private void OnResolutionChanged (int width, int height) {
		customResolution.width = width;
		customResolution.height = height;
		currentResolutionIndex = 0;
	}
	
	private void RefreshGameViewResolution () {
		foreach(var resolution in resolutions) {
			if(resolution.name == "Game View") {
				SetScreenWidthAndHeightFromEditorGameViewViaReflection(ref resolution.width, ref resolution.height);
				if(EditorGUIUtility.pixelsPerPoint > 1 && !retinaGameViewResolution) {
					resolution.width = Mathf.RoundToInt(resolution.width * 0.5f);
					resolution.height = Mathf.RoundToInt(resolution.height * 0.5f);
				}
				break;
			}
		}
	}
	
	private bool KeyCodeIsFKey (KeyCode keycode) {
		return (keycode == KeyCode.F1 || keycode == KeyCode.F2 || keycode == KeyCode.F3 || keycode == KeyCode.F4 || keycode == KeyCode.F5 || keycode == KeyCode.F6 || keycode == KeyCode.F7 || keycode == KeyCode.F8 || keycode == KeyCode.F9 || keycode == KeyCode.F10 || keycode == KeyCode.F11 || keycode == KeyCode.F12 || keycode == KeyCode.F13 || keycode == KeyCode.F14 || keycode == KeyCode.F15);
	}
	
	private void CaptureScreenshot () {
		if(currentResolution.name == "Game View") RefreshGameViewResolution();
		var properties = new ScreenshotCapturer.ScreenshotCapturerProperties(currentResolution.width, currentResolution.height, cameras, textureFormat, OnCaptureScreenshot);
		ScreenshotCapturer.CaptureScreenshot(properties);
	}
}
#endif