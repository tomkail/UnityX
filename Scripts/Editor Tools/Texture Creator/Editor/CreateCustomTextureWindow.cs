#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Reflection;

public class CreateCustomTextureWindow : EditorWindow {
	
	// Window Properties
	private static Vector2 minWindowSize = new Vector2(280, 380);
	
	private Vector2 scrollPos = Vector2.zero;
	
	
	// Texture Type
	private static bool expandTexturePanel = true;
	
	public enum TextureType {
		SolidColor,
		Gradient,
		Shape,
		Pattern,
		Noise
	}
	
	private TextureType _textureType;
	public TextureType textureType {
		get {
			return _textureType;
		}
		set {
			if(_textureType == value)
				return;
			_textureType = value;
			currentSettings = GetSettingsFromType(_textureType);
		}
	}

	private TextureTypeSettings _currentSettings;
	public TextureTypeSettings currentSettings {
		get {
			return _currentSettings;
		}
		set {
			if(_currentSettings == value)
				return;
			if(_currentSettings != null)
				_currentSettings.OnDisable();
			_currentSettings = value;
			if(_currentSettings != null)
				_currentSettings.OnEnable();
		}
	}
	
	// Solid Color
	public SolidColorSettings solidColorSettings;
	// Gradient
	public GradientSettings gradientSettings;
	// Shape
	public ShapeSettings shapeSettings;
	// Pattern
	public PatternSettings patternSettings;
	// Noise
	public NoiseSettings noiseSettings;


	// Export
	private static bool expandExportPanel = true;
	public static int width = 64;
	public static int height = 64;
	
	
	public enum TextureExportFormat {
		PNG,
		JPEG
	}
	public TextureExportFormat textureExportFormat;
	int jpegQuality = 75;
	public Texture2D texture;
	
	
	// Preview
	private static bool expandPreviewPanel = true;
	private static bool displayActualSize = false;
	private static Vector2 textureViewportSize = new Vector2(128, 128);
	private static int maxPixelsForAutoRefresh = 16384;
	private static Texture2D displayTexture;
	
 	[MenuItem("Assets/Create/Custom Texture")]
	static void Init () {
		CreateCustomTextureWindow window = EditorWindow.GetWindow(typeof(CreateCustomTextureWindow), false, "Texture Creator") as CreateCustomTextureWindow;
		window.minSize = minWindowSize;
		window.SetUp();
	}
	
	public void SetUp () {
		solidColorSettings = new SolidColorSettings(this);
		gradientSettings = new GradientSettings(this);
		shapeSettings = new ShapeSettings(this);
		patternSettings = new PatternSettings(this);
		noiseSettings = new NoiseSettings(this);
		
		textureType = TextureType.SolidColor;
		currentSettings = GetSettingsFromType(textureType);
	}
	
	void OnDestroy () {
		if(displayTexture != null)
			MonoBehaviour.DestroyImmediate(displayTexture);
	}
	
	private TextureTypeSettings GetSettingsFromType (TextureType type) {
		if(type == TextureType.SolidColor)
			return solidColorSettings;
		else if(type == TextureType.Gradient)
			return gradientSettings;
		else if(type == TextureType.Shape)
			return shapeSettings;
		else if(type == TextureType.Pattern)
			return patternSettings;
		else if(type == TextureType.Noise)
			return noiseSettings;
		
		Debug.LogError(string.Format("TextureType {0} not recognized.", type));
		return solidColorSettings;
	}
	
	
	void OnGUI () {
		// While testing
		if(currentSettings == null) {
			solidColorSettings = new SolidColorSettings(this);
			currentSettings = solidColorSettings;
		}
		
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		GUILayout.Label("");
		EditorGUILayout.EndHorizontal();
		
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		
		
//		EditorGUILayout.Space ();
		DrawCurrentTextureTypePanel();
//		EditorGUILayout.Space ();
		DrawExportPanel();
//		EditorGUILayout.Space ();
		DrawPreviewPanel();
		
		EditorGUILayout.EndScrollView();
	}
	
	private void DrawTextureTypeToolbar () {
		textureType = (TextureType)GUILayout.Toolbar ((int)textureType, new string[] {"Color", "Gradient", "Shape", "Pattern", "Noise"});
	}
	
	void DrawCurrentTextureTypePanel () {
		DrawTextureTypeToolbar();
		EditorGUILayout.BeginVertical(GUI.skin.box);
		expandTexturePanel = EditorGUILayout.Foldout(expandTexturePanel, "Properties", EditorStyles.foldout);
		if(expandTexturePanel) {
//			EditorGUI.indentLevel++;
			currentSettings.Draw();
//			EditorGUI.indentLevel--;
		}
		
		EditorGUILayout.EndVertical();
	}
	
	void DrawExportPanel () {
		EditorGUILayout.BeginVertical(GUI.skin.box);
		
		expandExportPanel = EditorGUILayout.Foldout(expandExportPanel, "Export", EditorStyles.foldout);
		if(expandExportPanel) {
//			EditorGUI.indentLevel++;
			
//			EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
			
			width = EditorGUILayout.IntField ("Width: ", width);
			width = Mathf.Clamp(width, 1, 2000);
			
			height = EditorGUILayout.IntField ("Height: ", height);
			height = Mathf.Clamp(height, 1, 2000);
			
			EditorGUI.BeginChangeCheck();
			textureExportFormat = (TextureExportFormat)GUILayout.Toolbar ((int)textureExportFormat, new string[] {"PNG", "JPEG"});
			if(EditorGUI.EndChangeCheck()) {
				OnTextureExportFormatChanged ();
			}
			
			if(textureExportFormat == TextureExportFormat.JPEG)
				jpegQuality = EditorGUILayout.IntSlider ("JPEG Quality: ", jpegQuality, 0, 100);
			
			if(GUILayout.Button("Export")) {
				ExportUsingCurrentFormat ();
			}
			
//			EditorGUI.indentLevel--;
		}
		
		EditorGUILayout.EndVertical();
	}
	
	void DrawPreviewPanel () {
		EditorGUILayout.BeginVertical(GUI.skin.box);
		
		expandPreviewPanel = EditorGUILayout.Foldout(expandPreviewPanel, "Preview", EditorStyles.foldout);
		if(expandPreviewPanel) {
//			EditorGUI.indentLevel++;
			displayActualSize = EditorGUILayout.Toggle("Display Actual Size", displayActualSize);
			int displayWidth = Mathf.Min((int)textureViewportSize.x, width);
			int displayHeight = Mathf.Min((int)textureViewportSize.y, height);
			
			if(displayWidth * displayHeight <= maxPixelsForAutoRefresh) {
				MonoBehaviour.DestroyImmediate(displayTexture);
				displayTexture = GenerateTexture(displayWidth, displayHeight);
			} else {
				EditorGUILayout.HelpBox("Texture size is too large to update every frame.", MessageType.Info);
				if(GUILayout.Button("Refresh")) {
					MonoBehaviour.DestroyImmediate(displayTexture);
					displayTexture = GenerateTexture(displayWidth, displayHeight);
				}
			}
			
//			EditorGUILayout.LabelField(new GUIContent(displayTexture));
//			EditorGUILayout.LabelField(new GUIContent(backgroundTexture), GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
//			GUILayout.Box(backgroundTexture, GUILayout.Width(textureViewportSize.x), GUILayout.Height(textureViewportSize.y));
//			GUILayout.Box(backgroundTexture, GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
			
			if(displayActualSize) {
				EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
			} else {
				EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(textureViewportSize.x), GUILayout.Height(textureViewportSize.y));
			}
			if (Event.current.type == EventType.Repaint) {
				EditorGUI.DrawTextureTransparent(GUILayoutUtility.GetLastRect(), displayTexture);
			}
			
//			GUILayout.Box(displayTexture, GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
			
			
//			EditorGUI.indentLevel--;
		}
		
		EditorGUILayout.EndVertical();
		
	}
	
	void ExportUsingCurrentFormat () {
		switch (textureExportFormat) {
		case TextureExportFormat.PNG:
			ExportAsPNG();
			break;
		case TextureExportFormat.JPEG:
			ExportAsJPEG();
			break;
		}
	}
	
	void OnTextureExportFormatChanged () {
		
	}
	
	void ExportAsPNG() {
		GenerateTexture(width, height);
		string path = EditorUtility.SaveFilePanel("Save texture as PNG", SelectedAssetFolderPath(), "UntitledTexture.png", "png");
		
		if(path.Length == 0) {
			Debug.LogError("Texture export path was null!");
			return;
		}
		
		if(texture.format != TextureFormat.ARGB32 && texture.format != TextureFormat.RGB24){
			Texture2D newTexture = new Texture2D(texture.width, texture.height);
			newTexture.SetPixels(texture.GetPixels(0),0);
			texture = newTexture;
		}
		byte[] pngData = texture.EncodeToPNG();
		if (pngData != null) {
			File.WriteAllBytes(path, pngData);
		} else {
			Debug.LogError("Byte array was null.");
		}
		AssetDatabase.Refresh();
	
	}

	void ExportAsJPEG() {
		string path = EditorUtility.SaveFilePanel("Save texture as JPEG", SelectedAssetFolderPath(), "UntitledTexture.jpeg", "jpeg");
		
		if(path.Length == 0) {
			Debug.LogError("Texture export path was null!");
			return;
		}	
		
		if(texture.format != TextureFormat.ARGB32 && texture.format != TextureFormat.RGB24){
			Texture2D newTexture = new Texture2D(texture.width, texture.height);
			newTexture.SetPixels(texture.GetPixels(0),0);
			texture = newTexture;
		}
		byte[] jpgData = texture.EncodeToJPG();
		if (jpgData != null) {
			File.WriteAllBytes(path, jpgData);
		} else {
			Debug.LogError("Byte array was null.");
		}
		AssetDatabase.Refresh();
	}
	
	public string SelectedAssetFolderPath(){
		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj);
			if (File.Exists(path))
			{
				path = System.IO.Path.GetDirectoryName(path);
			}
			break;
		}
		return path;
	}
	
	Texture2D GenerateTexture(int width, int height) {
		if(texture != null) DestroyImmediate(texture);
		Color[] colors = currentSettings.GeneratePixels(width, height);
		texture = CreateTexture2D(width, height, colors);
		texture.Apply();
		return texture;
	}
	
	[System.Serializable]
	public abstract class TextureTypeSettings : System.Object {
		public CreateCustomTextureWindow window {get; set;}
		public abstract void OnEnable ();
		public abstract void OnDisable();
		public abstract void Draw ();
		public abstract Color[] GeneratePixels (int width, int height);
	}
	
	[System.Serializable]
	public class SolidColorSettings : TextureTypeSettings {
		
		public Color color = Color.white;
		
		public override void OnEnable () {}
		public override void OnDisable () {}
		
		public SolidColorSettings (CreateCustomTextureWindow _window) {
			window = _window;
		}
		
		public override void Draw () {
	//		EditorGUILayout.LabelField("Solid Color", EditorStyles.boldLabel);
			color = EditorGUILayout.ColorField ("Color: ", color);
		}
		
		public override Color[] GeneratePixels (int width, int height) {
			Color[] colors = new Color[width * height];
			for(int i = 0; i < colors.Length; i++) {
				colors[i] = color;
			}
			return colors;
		}
		
	}
	
	[System.Serializable]
	public class GradientSettings : TextureTypeSettings {
	
		public enum GradientType {
			Radial,
			Linear,
			Conical,
			Reflected
		}
		
		
		public GradientType gradientType;
		
		public Gradient gradient;
		
		public float angle = 0;
		public float length = 0.5f;
		
		public Vector2 gradientStartPosition = new Vector2(0.5f, 0.5f);
		public Vector2 gradientEndPosition;

		public override void OnEnable () {}
		public override void OnDisable () {}
		
		public GradientSettings (CreateCustomTextureWindow _window) {
			window = _window;
			gradient = CreateGradient(Color.white, Color.black);
		}
		
		public override void Draw () {
	//		EditorGUILayout.LabelField("Gradient", EditorStyles.boldLabel);
			
			EditorGUI.BeginChangeCheck();
			gradientType = (GradientType)EditorGUILayout.EnumPopup ("Gradient Type", gradientType);
			if(EditorGUI.EndChangeCheck()) {
				OnGradientTypeChanged ();
			}
			
			//Draw Gradient
			EditorGUI.BeginChangeCheck();

			SerializedObject serializedGradient = new SerializedObject(window);
			SerializedProperty colorGradient1 = serializedGradient.FindProperty("gradientSettings");
			SerializedProperty colorGradient = colorGradient1.FindPropertyRelative("gradient");
			EditorGUILayout.PropertyField(colorGradient, true, null);
			
			if(EditorGUI.EndChangeCheck()) {
				serializedGradient.ApplyModifiedProperties();
			}
			
			gradientStartPosition = EditorGUILayout.Vector2Field ("Gradient Start Position: ", gradientStartPosition);
			Clamp01(gradientStartPosition);
			gradientEndPosition = gradientStartPosition + Degrees2Vector2(angle) * length;
			Clamp01(gradientEndPosition);
			
			if(gradientType == GradientType.Linear || gradientType == GradientType.Radial || gradientType == GradientType.Reflected) {
				length = EditorGUILayout.FloatField ("Length: ", length);
			}
			if(gradientType == GradientType.Linear || gradientType == GradientType.Conical || gradientType == GradientType.Reflected) {
				angle = EditorGUILayout.FloatField ("Angle: ", angle);
			}
		}
		
		public override Color[] GeneratePixels (int width, int height) {
			return CreateGradient(gradientType, gradient, gradientStartPosition, gradientEndPosition, width, height);
		}
		
		private void OnGradientTypeChanged () {
			if(gradientType == GradientType.Linear) {
				
			} else if(gradientType == GradientType.Radial) {
				
			} else if(gradientType == GradientType.Conical) {
				length = 1;
			} else if(gradientType == GradientType.Reflected) {
				
			}
		}
		
		private Gradient CreateGradient(Color startColor, Color stopColor, float start = 0f, float stop = 1f){
			Gradient gradient = new Gradient();
			GradientColorKey[] colorKeys = new GradientColorKey[2];
			GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
			
			colorKeys[0].color = startColor;
			colorKeys[0].time = start;
			alphaKeys[0].alpha = startColor.a;
			alphaKeys[0].time = start;
			
			colorKeys[1].color = stopColor;
			colorKeys[1].time = stop;
			alphaKeys[1].alpha = stopColor.a;
			alphaKeys[1].time = stop;
			
			gradient.SetKeys(colorKeys, alphaKeys);
			
			return gradient;
		}
		
		private Color[] CreateGradient (GradientType gradientType, Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height) {
			switch (gradientType){
			case GradientType.Linear:
				return CreateLinearGradient(gradient, startPosition, endPosition, width, height);
			case GradientType.Radial:
				return CreateRadialGradient(gradient, startPosition, endPosition, width, height);
			case GradientType.Conical:
				return CreateConicalGradient(gradient, startPosition, endPosition, width, height);
			case GradientType.Reflected:
				return CreateReflectedGradient(gradient, startPosition, endPosition, width, height);
			default:
				return CreateConicalGradient(gradient, startPosition, endPosition, width, height);
			}
		}
		
		private Color[] CreateLinearGradient(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
			int numPixels = width * height;
			Color[] pixels = new Color[numPixels];
			float widthReciprocal = 1f/Clamp1Infinity(width-1);
			float heightReciprocal = 1f/Clamp1Infinity(height-1);
			
			for(int y = 0; y < height; y++){
				for(int x = 0; x < width; x++){
					Vector2 point = new Vector2(x * widthReciprocal, y * heightReciprocal);
					float distance = Vector2.Dot(point - endPosition, startPosition - endPosition) / ((endPosition-startPosition).sqrMagnitude);
					pixels[y * width + x] = gradient.Evaluate(distance);
				}
			}
			
			return pixels;
		}
		
		private Color[] CreateRadialGradient(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
			int numPixels = width * height;
			Color[] pixels = new Color[numPixels];
			float widthReciprocal = 1f/Clamp1Infinity(width-1);
			float heightReciprocal = 1f/Clamp1Infinity(height-1);
			float length = Vector2.Distance(startPosition, endPosition);
			
			for(int y = 0; y < height; y++){
				for(int x = 0; x < width; x++){
					float tmpRadius = Vector2.Distance(new Vector2(x * widthReciprocal, y * heightReciprocal), startPosition);
					pixels[y * width + x] = gradient.Evaluate(tmpRadius / length);
				}
			}
			
			return pixels;
		}
		
		private Color[] CreateConicalGradient(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
			int numPixels = width * height;
			Color[] pixels = new Color[numPixels];
			float widthReciprocal = 1f/Clamp1Infinity(width-1);
			float heightReciprocal = 1f/Clamp1Infinity(height-1);
			float degrees = DegreesBetween(startPosition, endPosition);
			
			for(int y = 0; y < height; y++){
				for(int x = 0; x < width; x++){
					float a = Mathf.Atan2(y * heightReciprocal - startPosition.y, x * widthReciprocal - startPosition.x);
					a += (degrees+180) * Mathf.Deg2Rad;
					a /= (Mathf.PI * 2);
					a+=0.5f;
					a = Mathf.Repeat(a,1f);
					pixels[y * width + x] = gradient.Evaluate(a);
				}
			}
			
			return pixels;
		}
		
		private Color[] CreateReflectedGradient(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
			int numPixels = width * height;
			Color[] pixels = new Color[numPixels];
			float widthReciprocal = 1f/Clamp1Infinity(width-1);
			float heightReciprocal = 1f/Clamp1Infinity(height-1);
			
			for(int y = 0; y < height; y++){
				for(int x = 0; x < width; x++){
					Vector2 point = new Vector2(x * widthReciprocal, y * heightReciprocal);
					float distance = NormalizedDistance(startPosition, endPosition, point);
					pixels[y * width + x] = gradient.Evaluate(distance);
				}
			}
			
			return pixels;
		}
		
		private float Clamp1Infinity(float value) {
			return Mathf.Clamp(value, 1, Mathf.Infinity);
		}
		
		private float DegreesBetween(Vector2 a, Vector2 b) {
			return RadiansBetween(a,b) * Mathf.Rad2Deg;
		}
		
		private float RadiansBetween(Vector2 a, Vector2 b) {
			return Mathf.Atan2(-(b.y - a.y), b.x - a.x) + (Mathf.Deg2Rad * 90);
		}
		
		private float NormalizedDistance(Vector2 a, Vector2 b, Vector2 point) {
			return (Vector2.Dot(point - a, b - a) / ((a-b).sqrMagnitude)).Abs();
		}
		
		private Vector2 Clamp01(Vector2 v){
			return Clamp(v, Vector2.zero, Vector2.one);
		}
		
		private Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max){
			v.x = Mathf.Clamp(v.x, min.x, max.x);
			v.y = Mathf.Clamp(v.y, min.y, max.y);
			return v;
		}
		
		private Vector2 Degrees2Vector2 (float degrees) {
			return Radians2Vector2(degrees * Mathf.Deg2Rad);
		}
		
		private Vector2 Radians2Vector2 (float radians) {
			float sin = Mathf.Sin(radians);
			float cos = Mathf.Cos(radians);
			return new Vector2(sin, cos);
		}
	}
	
	
	[System.Serializable]
	public class ShapeSettings : TextureTypeSettings {
		
		public enum ShapeType {
			RoundedRect
		}
		public ShapeType shapeType;
		
		public ShapeSettings (CreateCustomTextureWindow _window) {
			window = _window;
		}
		
		public override void OnEnable () {}
		public override void OnDisable () {}
		
		public override void Draw () {
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.HelpBox("Coming soon!", MessageType.Error);
			shapeType = (ShapeType)EditorGUILayout.EnumPopup ("Shape Type", shapeType);
			if(EditorGUI.EndChangeCheck()) {
				OnShapeTypeChanged ();
			}
		}
		
		public override Color[] GeneratePixels (int width, int height) {
			return new Color[width * height];
		}
		
		private void OnShapeTypeChanged () {}		
	}
	
	[System.Serializable]
	public class PatternSettings : TextureTypeSettings {
		public enum PatternType {
			Tiles,
			Grid
		}
		public PatternType patternType;
		
		public Color colorA = Color.black;
		public Color colorB = Color.white;
		
		public float tilePatternTileSizeX = 32;
		public float tilePatternTileSizeY = 32;
		
		
		public float gridPatternTileSizeX = 63;
		public float gridPatternTileSizeY = 63;
		public int gridPatternLineSize = 1;
		
		public PatternSettings (CreateCustomTextureWindow _window) {
			window = _window;
		}
		
		public override void OnEnable () {}
		public override void OnDisable () {}
		
		
		public override void Draw () {
			EditorGUI.BeginChangeCheck();
			patternType = (PatternType)EditorGUILayout.EnumPopup ("Gradient Type", patternType);
			if(EditorGUI.EndChangeCheck()) {
				OnPatternTypeChanged ();
			}
			
			EditorGUI.BeginChangeCheck();
			
			SerializedObject serializedGradient = new SerializedObject(window);
			
			if(patternType == PatternType.Grid) {
				colorA = EditorGUILayout.ColorField("Background", colorA);
				colorB = EditorGUILayout.ColorField("Lines", colorB);
				gridPatternTileSizeX = Mathf.Clamp(EditorGUILayout.FloatField("Grid Cell Width", gridPatternTileSizeX), 0, Mathf.Infinity);
				gridPatternTileSizeY = Mathf.Clamp(EditorGUILayout.FloatField("Grid Cell Height", gridPatternTileSizeY), 0, Mathf.Infinity);
				gridPatternLineSize = Mathf.Clamp(EditorGUILayout.IntField("Line Size", gridPatternLineSize), 1, 100);
			} else if(patternType == PatternType.Tiles) {
				colorA = EditorGUILayout.ColorField("Color A", colorA);
				colorB = EditorGUILayout.ColorField("Color B", colorB);
				tilePatternTileSizeX = Mathf.Clamp(EditorGUILayout.FloatField("Tile Width", tilePatternTileSizeX), 0, Mathf.Infinity);
				tilePatternTileSizeY = Mathf.Clamp(EditorGUILayout.FloatField("Tile Height", tilePatternTileSizeY), 0, Mathf.Infinity);
			}
			
			if(EditorGUI.EndChangeCheck()) {
				serializedGradient.ApplyModifiedProperties();
			}
		}
		
		public override Color[] GeneratePixels (int width, int height) {
			Color[] colors = new Color[width * height];
			if(patternType == PatternType.Grid) {
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						bool xx = (x + gridPatternLineSize * 0.5f) % gridPatternTileSizeX < gridPatternLineSize;
						bool yy = (y + gridPatternLineSize * 0.5f) % gridPatternTileSizeY < gridPatternLineSize;
						colors[y * width + x] = xx || yy ? colorA : colorB;
					}
				}
			} else if(patternType == PatternType.Tiles) {
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						bool xx = ((float)x / tilePatternTileSizeX) % 2 < 1;
						bool yy = ((float)y / tilePatternTileSizeY) % 2 < 1;
						colors[y * width + x] = xx == yy ? colorA : colorB;
					}
				}
			}
			return colors;
		}
	
		private void OnPatternTypeChanged () {}
	}

	[System.Serializable]
	public class NoiseSettings : TextureTypeSettings {
		public Gradient gradient;
		
		public enum NoiseType {
			Perlin,
			Simplex,
			Voronoi
		}
		public NoiseType noiseType;
		
		public NoiseSettings (CreateCustomTextureWindow _window) {
			window = _window;
		}
		
		public override void OnEnable () {}
		public override void OnDisable () {}
		
		public override void Draw () {
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.HelpBox("Coming soon!", MessageType.Error);
			noiseType = (NoiseType)EditorGUILayout.EnumPopup ("Noise Type", noiseType);
			if(EditorGUI.EndChangeCheck()) {
				OnNoiseTypeChanged ();
			}
		}
		
		public override Color[] GeneratePixels (int width, int height) {
			return new Color[width * height];
		}
		
		private void OnNoiseTypeChanged () {}		
	}
	
	
	private Texture2D CreateTexture2D(int width, int height, Color[] _array, FilterMode filterMode = FilterMode.Point, TextureFormat textureFormat = TextureFormat.ARGB32){
		if((width * height) != _array.Length) {
			MonoBehaviour.print("Cannot create color texture from color array because Size is ("+width+", "+height+") with area "+(width * height)+" and array size is "+_array.Length);
		}
		Texture2D tmpTexture = new Texture2D(width, height, textureFormat, false);
		tmpTexture.SetPixels(_array);
		tmpTexture.filterMode = filterMode;
		tmpTexture.wrapMode = TextureWrapMode.Clamp;
		return tmpTexture;
	}
}
#endif


/*

using UnityEngine;
using System.Collections;

public class EditorGrid {

	static EditorGrid inst = null;

	public enum Type
	{
		LightChecked,
		MediumChecked,
		DarkChecked,
		BlackChecked,
		LightSolid,
		MediumSolid,
		DarkSolid,
		BlackSolid,
		Custom
	}

	public static void Done() {
		if (inst != null) {
			inst.DestroyTexture();
			inst = null;
		}
	}

	const int textureSize = 16;

	public static void Draw(Rect rect) {
		Draw(rect, Vector2.zero);
	}

	public static void Draw(Rect rect, Vector2 offset) {
		if (inst == null) {
			inst = new EditorGrid();
			inst.InitTexture();
		}
		GUI.DrawTextureWithTexCoords(rect, inst.gridTexture, new Rect(-offset.x / textureSize, (offset.y - rect.height) / textureSize, rect.width / textureSize, rect.height / textureSize), false);
	} 

	Texture2D gridTexture = null;

	void InitTexture() {
		if (gridTexture == null) {
			gridTexture = new Texture2D(textureSize, textureSize);
			Color c0 = Color.white;
			Color c1 = new Color(0.8f, 0.8f, 0.8f, 1.0f);

			Type gridType = Type.DarkChecked;
			switch (gridType)
			{
				case Type.LightChecked:  c0 = new Color32(255, 255, 255, 255); c1 = new Color32(217, 217, 217, 255); break;
				case Type.MediumChecked: c0 = new Color32(178, 178, 178, 255); c1 = new Color32(151, 151, 151, 255); break;
				case Type.DarkChecked:   c0 = new Color32( 37,  37,  37, 255); c1 = new Color32( 31,  31,  31, 255); break;
				case Type.BlackChecked:  c0 = new Color32( 14,  14,  14, 255); c1 = new Color32(  0,   0,   0, 255); break;
				case Type.LightSolid:    c0 = new Color32(255, 255, 255, 255); c1 = c0; break;
				case Type.MediumSolid:   c0 = new Color32(178, 178, 178, 255); c1 = c0; break;
				case Type.DarkSolid:     c0 = new Color32( 37,  37,  37, 255); c1 = c0; break;
				case Type.BlackSolid:    c0 = new Color32(  0,   0,   0, 255); c1 = c0; break;
//				case Type.Custom:		 c0 = tk2dPreferences.inst.customGridColor0; c1 = tk2dPreferences.inst.customGridColor1; break;
			}

			for (int y = 0; y < gridTexture.height; ++y)
			{
				for (int x = 0; x < gridTexture.width; ++x)
				{
					bool xx = (x < gridTexture.width / 2);
					bool yy = (y < gridTexture.height / 2);
					gridTexture.SetPixel(x, y, (xx == yy)?c0:c1);
				}
			}
			gridTexture.Apply();
			gridTexture.filterMode = FilterMode.Point;
			gridTexture.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	void DestroyTexture() {
		if (gridTexture != null) {
			Object.DestroyImmediate(gridTexture);
			gridTexture = null;
		}
	}
}
*/