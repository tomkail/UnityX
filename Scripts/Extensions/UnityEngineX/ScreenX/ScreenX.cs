using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Screen = UnityEngine.Device.Screen;

/// <summary>
/// Manages screen properties. Must be attached to a GameObject to function.
/// </summary>
[InitializeOnLoad]
public class ScreenX {

	public const float inchesToCentimeters = 2.54f;
	public const float centimetersToInches = 0.39370079f;
	
	/// <summary>
	/// The length from the bottom-left to the top-right corners. 
	/// Note: measured in pixels, rather than the standard screen diagonal unit of inches.
	/// </summary>
	public static float diagonal => screen.diagonal;

	/// <summary>
	/// The total area of the screen
	/// </summary>
	public static float area => screen.area;

	/// <summary>
	/// The aspect ratio
	/// </summary>
	public static float aspectRatio => screen.aspectRatio;

	/// <summary>
	/// The reciprocal of the screen width
	/// </summary>
	public static float widthReciprocal => screen.widthReciprocal;

	/// <summary>
	/// The reciprocal of the screen height
	/// </summary>
	public static float heightReciprocal => screen.heightReciprocal;

	/// <summary>
	/// The reciprocal of the screen size
	/// Note: measured in pixels, rather than the standard screen diagonal unit of inches.
	/// </summary>
	public static float diagonalReciprocal => screen.diagonalReciprocal;

	/// <summary>
	/// The inverted aspect ratio
	/// </summary>
	public static float aspectRatioReciprocal => screen.aspectRatioReciprocal;

	/// <summary>
	/// The width of the screen in pixels.
	/// </summary>
	/// <value>The width.</value>
	public static float width => screen.width;

	/// <summary>
	/// The height of the screen in pixels.
	/// </summary>
	/// <value>The height.</value>
	public static float height => screen.height;

	/// <summary>
	/// The size of the screen as a Vector.
	/// </summary>
	/// <value>The size.</value>
	public static Vector2 size => screen.size;

	/// <summary>
	/// The center of the screen
	/// </summary>
	/// <value>The center.</value>
	public static Vector2 center => screen.center;

	/// <summary>
	/// Gets the screen rect.
	/// </summary>
	/// <value>The screen rect.</value>
	public static Rect screenRect => screen.rect;


	/// <summary>
	/// Is device DPI unavailiable? (as it is on many devices)
	/// </summary>
	public static bool usingDefaultDPI => Screen.dpi == 0;

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
	// static bool gameViewDpiMultiplierDirty = true;
	public static float dpi {
		get {
			float dpiMultiplier = 1f;
			// #if UNITY_EDITOR
			// if(gameViewDpiMultiplierDirty) {
			// 	// When using a fixed game view resolution, Screen.width/height returns the size of the fixed resolution. If the fixed resolution is more than the actual game view window's size, it's scaled down.
			// 	// Screen.dpi continues to return the dpi of the screen in this case, without taking the shrinkage into account. 
			// 	// DPI should return the density of the game view resolution, rather than of the game view window, and so we take this into account here.
			// 	System.Type T = System.Type.GetType("UnityEditor.PlayModeView,UnityEditor");
			// 	System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainPlayModeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			// 	var gameView = (UnityEditor.EditorWindow)GetMainGameView.Invoke(null, null);
			// 	dpiMultiplier = Mathf.Max(1, screenWidth/gameView.position.width, screenHeight/gameView.position.height);
			// 	Debug.Log("SET M "+dpiMultiplier);
			// 	gameViewDpiMultiplierDirty = false;
			// }
			// #endif

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
	static ScreenOrientation lastScreenOrientation;
	
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

	public static ScreenRectProperties screen = new("Pixels");
	public static ScreenRectProperties viewport = new("Viewport");
	public static ScreenRectProperties inches = new("Inches");
	public static ScreenRectProperties centimeters = new("Centimeters");
	
	/// <summary>
	/// The width of the screen last time the size was changed. Used for measuring screen size change.
	/// </summary>
	static int lastWidth;
	
	/// <summary>
	/// The height of the screen last time the size was changed. Used for measuring screen size change.
	/// </summary>
	static int lastHeight;
	
	static ScreenX () {
		StoreWidthAndHeight();
		CalculateScreenSizeProperties();
		lastScreenOrientation = Screen.orientation;
		

		PlayerLoopSystem playerLoop = PlayerLoop.GetDefaultPlayerLoop();
		// Debug.Assert(PlayerLoopUtils.AddToPlayerLoop(EndOfFrameUpdate, typeof(LightgunInput), ref playerLoop, typeof(PreUpdate.NewInputUpdate), PlayerLoopUtils.AddMode.End));
		Debug.Assert(ScreenRectProperties.PlayerLoopUtils.AddToPlayerLoop(Update, typeof(ScreenX), ref playerLoop, typeof(PreUpdate.NewInputUpdate), ScreenRectProperties.PlayerLoopUtils.AddMode.End));
		PlayerLoop.SetPlayerLoop(playerLoop);
	}

	static void Update () {
		// #if UNITY_EDITOR
		// gameViewDpiMultiplierDirty = true;
		// #endif
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


	static void CheckSizeChange() {
		if(screenWidth != lastWidth || screenHeight != lastHeight){
			StoreWidthAndHeight();
			CalculateScreenSizeProperties();
			OnScreenSizeChange?.Invoke();
		}
	}

	static void CheckOrientationChange() {
		if (Screen.orientation == lastScreenOrientation) return;
		lastScreenOrientation = Screen.orientation;
		OnOrientationChange?.Invoke();
		OrientationChangeAndCalculateScreen();
	}

	static void OrientationChangeAndCalculateScreen() {
		StoreWidthAndHeight();
		CalculateScreenSizeProperties();
		OnScreenSizeChange?.Invoke();
	}
	
	public static void CalculateScreenSizeProperties () {
		screen.CalculateScreenSizeProperties(screenWidth, screenHeight);
		viewport.CalculateScreenSizeProperties(1, 1);
		inches.CalculateScreenSizeProperties(ViewportToInchesPoint(Vector2.one));
		centimeters.CalculateScreenSizeProperties(ViewportToCentimetersPoint(Vector2.one));
	}

	static void StoreWidthAndHeight () {
		lastWidth = screenWidth;
		lastHeight = screenHeight;
	}
	
	// ARGH I hate this. It's necessary because screen/display don't return the values for game view in some editor contexts (using inspector windows, for example)
	static int screenWidth {
		get {
#if UNITY_EDITOR
			var res = UnityStats.screenRes.Split('x');
			var width = int.Parse(res[0]);
			if (width != 0) return width;
#endif
			// Consider adding target displays, then replace with this.
			// Display.displays[0].renderingWidth
			return Screen.width;
		}
	}
	static int screenHeight {
		get {
#if UNITY_EDITOR
			var res = UnityStats.screenRes.Split('x');
			var height = int.Parse(res[1]);
			if (height != 0) return height;
#endif
			// Consider adding target displays, then replace with this.
			// Display.displays[0].renderingHeight
			return Screen.height;
		}
	}
}


/// <summary>
/// Unitless screen properties. 
/// Can be used to store screen properties in various unit types (Screen, Viewport, Inches)
/// </summary>
[Serializable]
public class ScreenRectProperties {
	/// <summary>
	/// A name for this way of representing the screen rect
	/// </summary>
	public string name {
		get; private set;
	}
	
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
	public Vector2 size => new(width, height);

	/// <summary>
	/// The center of the screen
	/// </summary>
	/// <value>The center.</value>
	public Vector2 center => new(width * 0.5f, height * 0.5f);

	/// <summary>
	/// Gets the screen rect.
	/// </summary>
	/// <value>The screen rect.</value>
	public Rect rect => new(0, 0, width, height);

	public ScreenRectProperties (string name) {
		this.name = name;
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

	void CalculateDiagonal () {
		diagonal = size.magnitude;
	}

	void CalculateArea () {
		area = width * height;
	}

	void CalculateAspectRatio () {
		aspectRatio = width/height;
	}

	void CalculateReciprocals () {
		widthReciprocal = width == 0 ? 0 : 1f/width;
		heightReciprocal = height == 0 ? 0 : 1f/height;
		diagonalReciprocal = diagonal == 0 ? 0 : 1f/diagonal;
		aspectRatioReciprocal = aspectRatio == 0 ? 0 : 1f/aspectRatio;
	}

	public override string ToString() {
		return $"[{GetType().Name}] Name={name} Width={width}, Height={height}";
	}
	
	
	
	
	
	
	
	
	
	
	
	
	public static class PlayerLoopUtils {
		public static void PrintPlayerLoop(PlayerLoopSystem def) {
			var sb = new StringBuilder();
			RecursivePlayerLoopPrint(def, sb, 0);
			Debug.Log(sb.ToString());
		}
		private static void RecursivePlayerLoopPrint(PlayerLoopSystem def, StringBuilder sb, int depth) {
			if (depth == 0)
				sb.AppendLine("ROOT NODE");
			else if (def.type != null) {
				for (int i = 0; i < depth; i++) 
					sb.Append("\t");
				sb.AppendLine(def.type.Name);
			}
			if (def.subSystemList != null) {
				depth++;
				foreach (var s in def.subSystemList)
					RecursivePlayerLoopPrint(s, sb, depth);
				depth--;
			}
		}
		
		public enum AddMode {
			Beginning,
			End
		}

		// Add a new PlayerLoopSystem to the PlayerLoop. Example:
		// PlayerLoopSystem playerLoop = PlayerLoop.GetDefaultPlayerLoop();
		// Debug.Assert(PlayerLoopUtils.AddToPlayerLoop(CustomUpdate, typeof(LightgunInput), ref playerLoop, typeof(PreUpdate.NewInputUpdate), PlayerLoopUtils.AddMode.End));
		// PlayerLoop.SetPlayerLoop(playerLoop);
		public static bool AddToPlayerLoop(PlayerLoopSystem.UpdateFunction function, Type ownerType, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType, AddMode addMode) {
			// did we find the type? e.g. EarlyUpdate/PreLateUpdate/etc.
			if (playerLoop.type == playerLoopSystemType) {
				// debugging
				//Debug.Log($"Found playerLoop of type {playerLoop.type} with {playerLoop.subSystemList.Length} Functions:");
				//foreach (PlayerLoopSystem sys in playerLoop.subSystemList)
				//    Debug.Log($"  ->{sys.type}");

				// resize & expand subSystemList to fit one more entry
				int oldListLength = (playerLoop.subSystemList != null) ? playerLoop.subSystemList.Length : 0;
				Array.Resize(ref playerLoop.subSystemList, oldListLength + 1);

				// prepend our custom loop to the beginning
				if (addMode == AddMode.Beginning) {
					// shift to the right, write into first array element
					Array.Copy(playerLoop.subSystemList, 0, playerLoop.subSystemList, 1, playerLoop.subSystemList.Length - 1);
					playerLoop.subSystemList[0].type = ownerType;
					playerLoop.subSystemList[0].updateDelegate = function;

				}
				// append our custom loop to the end
				else if (addMode == AddMode.End) {
					// simply write into last array element
					playerLoop.subSystemList[oldListLength].type = ownerType;
					playerLoop.subSystemList[oldListLength].updateDelegate = function;
				}

				// debugging
				//Debug.Log($"New playerLoop of type {playerLoop.type} with {playerLoop.subSystemList.Length} Functions:");
				//foreach (PlayerLoopSystem sys in playerLoop.subSystemList)
				//    Debug.Log($"  ->{sys.type}");

				return true;
			}

			// recursively keep looking
			if (playerLoop.subSystemList != null) {
				for(int i = 0; i < playerLoop.subSystemList.Length; ++i) {
					if (AddToPlayerLoop(function, ownerType, ref playerLoop.subSystemList[i], playerLoopSystemType, addMode))
						return true;
				}
			}
			return false;
		}
	}
}