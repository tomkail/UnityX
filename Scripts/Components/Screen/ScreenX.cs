//This class provides functionality for converting between pixel and percentage values, designed for GUI
using UnityEngine;
using System.Collections;

/// <summary>
/// Manages screen properties. Must be attached to a GameObject to function.
/// </summary>
[ExecuteAlways]
public class ScreenX : MonoSingleton<ScreenX> {

	public const float inchesToCentimeters = 2.54f;
	public const float centimetersToInches = 0.39370079f;

	/// <summary>
	/// The length from the bottom-left to the top-right corners. 
	/// Note: measured in pixels, rather than the standard screen diagonal unit of inches.
	/// </summary>
	public static float diagonal {
		get {
			return screen.diagonal;
		}
	}

	/// <summary>
	/// The total area of the screen
	/// </summary>
	public static float area {
		get {
			return screen.area;
		}
	}

	/// <summary>
	/// The aspect ratio
	/// </summary>
	public static float aspectRatio {
		get {
			return screen.aspectRatio;
		}
	}

	/// <summary>
	/// The reciprocal of the screen width
	/// </summary>
	public static float widthReciprocal {
		get {
			return screen.widthReciprocal;
		}
	}

	/// <summary>
	/// The reciprocal of the screen height
	/// </summary>
	public static float heightReciprocal {
		get {
			return screen.heightReciprocal;
		}
	}

	/// <summary>
	/// The reciprocal of the screen size
	/// Note: measured in pixels, rather than the standard screen diagonal unit of inches.
	/// </summary>
	public static float diagonalReciprocal {
		get {
			return screen.diagonalReciprocal;
		}
	}

	/// <summary>
	/// The inverted aspect ratio
	/// </summary>
	public static float aspectRatioReciprocal {
		get {
			return screen.aspectRatioReciprocal;
		}
	}
	
	/// <summary>
	/// The width of the screen in pixels.
	/// </summary>
	/// <value>The width.</value>
	public static float width {
		get {
			return screen.width;
		}
	}
	/// <summary>
	/// The height of the screen in pixels.
	/// </summary>
	/// <value>The height.</value>
	public static float height {
		get {
			return screen.height;
		}
	}
	
	/// <summary>
	/// The size of the screen as a Vector.
	/// </summary>
	/// <value>The size.</value>
	public static Vector2 size {
		get {
			return screen.size;
		}
	}
	
	/// <summary>
	/// The center of the screen
	/// </summary>
	/// <value>The center.</value>
	public static Vector2 center {
		get {
			return screen.center;
		}
	}
	
	/// <summary>
	/// Gets the screen rect.
	/// </summary>
	/// <value>The screen rect.</value>
	public static Rect screenRect {
		get {
			return screen.rect;
		}
	}
	
	
	/// <summary>
	/// Is device DPI unavailiable? (as it is on many devices)
	/// </summary>
	public static bool usingDefaultDPI {
		get {
			return Screen.dpi == 0;
		}
	}
	
	/// <summary>
	/// The default DPI to use in the case of default DPI
	/// </summary>
	public const int defaultDPI = 166;
	
	/// <summary>
	/// Use an override for DPI.
	/// </summary>
	public static bool usingCustomDPI;
	public static int customDPI = defaultDPI;
	
	/// <summary>
	/// The DPI of the screen
	/// </summary>
	static bool gameViewDpiMultiplierDirty = true;
	public static float dpi {
		get {
			float dpiMultiplier = 1f;
			#if UNITY_EDITOR
			if(gameViewDpiMultiplierDirty) {
				// When using a fixed game view resolution, Screen.width/height returns the size of the fixed resolution. If the fixed resolution is more than the actual game view window's size, it's scaled down.
				// Screen.dpi continues to return the dpi of the screen in this case, without taking the shrinkage into account. 
				// DPI should return the density of the game view resolution, rather than of the game view window, and so we take this into account here.
				System.Type T = System.Type.GetType("UnityEditor.PlayModeView,UnityEditor");
				System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainPlayModeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
				var gameView = (UnityEditor.EditorWindow)GetMainGameView.Invoke(null, null);
				dpiMultiplier = Mathf.Max(1, Screen.width/gameView.position.width, Screen.height/gameView.position.height);
				gameViewDpiMultiplierDirty = false;
			}
			#endif

			if(usingCustomDPI){
				return customDPI * dpiMultiplier;
			} else if(usingDefaultDPI){
				return defaultDPI * dpiMultiplier;
			} else {
				return Screen.dpi * dpiMultiplier;
			}
		}
	}

	/// <summary>
	/// The orientation of the screen last time the size was changed. Used for measuring screen size change.
	/// </summary>
	private ScreenOrientation lastScreenOrientation;
	
	/// <summary>
	/// Event called when the screen size is changed
	/// </summary>
	public delegate void OnScreenSizeChangeEvent();
	public static event OnScreenSizeChangeEvent OnScreenSizeChange;

	/// <summary>
	/// Event called when the orientation is changed
	/// </summary>
	public delegate void OnOrientationChangeEvent();
	public static event OnOrientationChangeEvent OnOrientationChange;

	public static ScreenProperties screen = new ScreenProperties();
	public static ScreenProperties viewport = new ScreenProperties();
	public static ScreenProperties inches = new ScreenProperties();
	public static ScreenProperties centimeters = new ScreenProperties();
	
	/// <summary>
	/// The width of the screen last time the size was changed. Used for measuring screen size change.
	/// </summary>
	private static int lastWidth;
	
	/// <summary>
	/// The height of the screen last time the size was changed. Used for measuring screen size change.
	/// </summary>
	private static int lastHeight;
	
	protected override void Awake () {
		base.Awake();
		StoreWidthAndHeight();
		CalculateScreenSizeProperties();
		lastScreenOrientation = Screen.orientation;
	}

	private void Update () {
		#if UNITY_EDITOR
		gameViewDpiMultiplierDirty = true;
		#endif
		CheckSizeChange();
		CheckOrientationChange();
	}
	

	public static Vector2 ScreenToViewportPoint(Vector2 screenPoint){
		return new Vector2(screenPoint.x * screen.widthReciprocal, screenPoint.y * screen.heightReciprocal);
	}

	public static Rect ScreenToViewportRect(Rect screenRect){
		return RectX.MinMaxRect(ScreenToViewportPoint(screenRect.min), ScreenToViewportPoint(screenRect.max));
	}
	
	public static Vector2 ScreenToInchesPoint(Vector2 screen){
		return new Vector2(screen.x / dpi, screen.y / dpi);
	}
	
	public static Vector2 ScreenToCentimetersPoint(Vector2 screen){
		return InchesToCentimetersPoint(ScreenToInchesPoint(screen));
	}
	
	
	public static Vector2 ViewportToScreenPoint(Vector2 viewport){
		return new Vector2(viewport.x * screenWidth, viewport.y * screenHeight);
	}
	public static Rect ViewportToScreenRect(Rect viewportRect){
		return RectX.MinMaxRect(ViewportToScreenPoint(viewportRect.min), ViewportToScreenPoint(viewportRect.max));
	}
	
	public static Vector2 ViewportToInchesPoint(Vector2 viewport){
		return ScreenToInchesPoint(ViewportToScreenPoint(viewport));
	}
	
	public static Vector2 ViewportToCentimetersPoint(Vector2 viewport){
		return ScreenToCentimetersPoint(ViewportToScreenPoint(viewport));
	}
	
	
	public static Vector2 InchesToScreenPoint(Vector2 inches){
		return new Vector2(inches.x * dpi, inches.y * dpi);
	}
	
	public static Vector2 InchesToViewportPoint(Vector2 inches){
		return ScreenToViewportPoint(InchesToScreenPoint(inches));
	}
	
	public static Vector2 InchesToCentimetersPoint(Vector2 inches){
		return inches * inchesToCentimeters;
	}
	
	
	public static Vector2 CentimetersToScreenPoint(Vector2 centimeters){
		return InchesToScreenPoint(CentimetersToInchesPoint(centimeters));
	}
	
	public static Vector2 CentimetersToViewportPoint(Vector2 centimeters){
		return InchesToViewportPoint(CentimetersToInchesPoint(centimeters));
	}
	
	public static Vector2 CentimetersToInchesPoint(Vector2 centimeters){
		return centimetersToInches * centimeters;
	}


	private void CheckSizeChange() {
		if(screenWidth != lastWidth || screenHeight != lastHeight){
			StoreWidthAndHeight();
			CalculateScreenSizeProperties();
			if(OnScreenSizeChange != null){
				OnScreenSizeChange();
			}
		}
	}
	
	private void CheckOrientationChange(){
		if(Screen.orientation != lastScreenOrientation){
			lastScreenOrientation = Screen.orientation;
			if(OnOrientationChange != null){
				OnOrientationChange();
			}
			if(Application.isPlaying) StartCoroutine(WaitForOrientationChangeAndCalculateScreen());
			else OrientationChangeAndCalculateScreen();
		}
	}
	
	//Orentation is stagged on android devices I've tested on. Wait a tiny amount of time before getting screen size.
	private IEnumerator WaitForOrientationChangeAndCalculateScreen(){
		yield return new WaitForSecondsRealtime (0.05f);
		OrientationChangeAndCalculateScreen();
	}

	void OrientationChangeAndCalculateScreen() {
		StoreWidthAndHeight();
		CalculateScreenSizeProperties();
		if(OnScreenSizeChange != null){
			OnScreenSizeChange();
		}
	}
	
	public static void CalculateScreenSizeProperties () {
		screen.CalculateScreenSizeProperties(screenWidth, screenHeight);
		viewport.CalculateScreenSizeProperties(1, 1);
		inches.CalculateScreenSizeProperties(ViewportToInchesPoint(Vector2.one));
		centimeters.CalculateScreenSizeProperties(ViewportToCentimetersPoint(Vector2.one));
	}
	
	private static void StoreWidthAndHeight () {
		lastWidth = screenWidth;
		lastHeight = screenHeight;
	}
	
	// ARGH I hate this. It's necessary because screen/display don't return the values for game view in some editor contexts (using inspector windows, for example)
	static int screenWidth {
		get {
			#if UNITY_EDITOR
			var res = UnityEditor.UnityStats.screenRes.Split('x');
			return int.Parse(res[0]);
			#else
			return Screen.width;
			#endif
		}
	}
	static int screenHeight {
		get {
			#if UNITY_EDITOR
			var res = UnityEditor.UnityStats.screenRes.Split('x');
			return int.Parse(res[1]);
			#else
			return Screen.height;
			#endif
		}
	}
}


/// <summary>
/// Unitless screen properties. 
/// Can be used to store screen properties in various unit types (Screen, Viewport, Inches)
/// </summary>
[System.Serializable]
public class ScreenProperties {
	
	/// <summary>
	/// The width of the screen last time the size was changed. Used for measuring screen size change.
	/// </summary>
	public float width {
		get; private set;
	}
	
	/// <summary>
	/// The height of the screen last time the size was changed. Used for measuring screen size change.
	/// </summary>
	public float height {
		get; private set;
	}
	
	/// <summary>
	/// The length from the bottom-left to the top-right corners. 
	/// Note: measured in pixels, rather than the standard screen diagonal unit of inches.
	/// </summary>
	public float diagonal {
		get; private set;
	}
	
	/// <summary>
	/// The total area of the screen
	/// </summary>
	public float area {
		get; private set;
	}
	
	/// <summary>
	/// The aspect ratio
	/// </summary>
	public float aspectRatio {
		get; private set;
	}
	
	/// <summary>
	/// The reciprocal of the screen width
	/// </summary>
	public float widthReciprocal {
		get; private set;
	}
	
	/// <summary>
	/// The reciprocal of the screen height
	/// </summary>
	public float heightReciprocal {
		get; private set;
	}
	
	/// <summary>
	/// The reciprocal of the screen size
	/// Note: measured in pixels, rather than the standard screen diagonal unit of inches.
	/// </summary>
	public float diagonalReciprocal {
		get; private set;
	}
	
	/// <summary>
	/// The inverted aspect ratio
	/// </summary>
	public float aspectRatioReciprocal {
		get; private set;
	}
	
	/// <summary>
	/// The size of the screen as a Vector.
	/// </summary>
	/// <value>The size.</value>
	public Vector2 size {
		get {
			return new Vector2(width, height);
		}
	}
	
	/// <summary>
	/// The center of the screen
	/// </summary>
	/// <value>The center.</value>
	public Vector2 center {
		get {
			return new Vector2(width * 0.5f, height * 0.5f);
		}
	}
	
	/// <summary>
	/// Gets the screen rect.
	/// </summary>
	/// <value>The screen rect.</value>
	public Rect rect {
		get {
			return new Rect(0, 0, width, height);
		}
	}
	
	public void CalculateScreenSizeProperties (Vector2 size){
		CalculateScreenSizeProperties(size.x, size.y);
	}
	
	public void CalculateScreenSizeProperties (float width, float height){
//		if(this.width == width && this.height == height) return;
		this.width = width;
		this.height = height;
		CalculateDiagonal();
		CalculateArea();
		CalculateAspectRatio();
		CalculateReciprocals();
	}
	
	private void CalculateDiagonal () {
		diagonal = size.magnitude;
	}
	
	private void CalculateArea () {
		area = width * height;
	}
	
	private void CalculateAspectRatio () {
		aspectRatio = (float)width/height;
	}
	
	private void CalculateReciprocals () {
		widthReciprocal = width == 0 ? 0 : 1f/width;
		heightReciprocal = height == 0 ? 0 : 1f/height;
		diagonalReciprocal = diagonal == 0 ? 0 : 1f/diagonal;
		aspectRatioReciprocal = aspectRatio == 0 ? 0 : 1f/aspectRatio;
	}

	public override string ToString() {
		return string.Format("[{0}] Width={1}, Height={2}", GetType().Name, width, height);
	}
}