using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OnGUIX : MonoBehaviour {

	public static Stack<Matrix4x4> matricies = new Stack<Matrix4x4>();
	public static Stack<Color> colors = new Stack<Color>();
	public static Stack<Color> contentColors = new Stack<Color>();
	public static Stack<Color> backgroundColors = new Stack<Color>();

	public static void BeginMatrix (Matrix4x4 matrix) {
		matricies.Push(GUI.matrix);
		GUI.matrix = matrix;
	}

	public static void EndMatrix () {
		GUI.matrix = matricies.Pop();	
	}

	public static void BeginColor (Color color) {
		colors.Push(GUI.color);
		GUI.color = color;
	}

	public static void EndColor () {
		GUI.color = colors.Pop();	
	}

	public static void BeginContentColor (Color contentColor) {
		contentColors.Push(GUI.contentColor);
		GUI.contentColor = contentColor;
	}

	public static void EndContentColor () {
		GUI.contentColor = contentColors.Pop();	
	}

	public static void BeginBackgroundColor (Color backgroundColor) {
		backgroundColors.Push(GUI.backgroundColor);
		GUI.backgroundColor = backgroundColor;
	}

	public static void EndBackgroundColor () {
		GUI.backgroundColor = backgroundColors.Pop();	
	}

    public static void DrawLine(Rect rect) { DrawLine(rect, GUI.contentColor, 1.0f); }
    public static void DrawLine(Rect rect, Color color) { DrawLine(rect, color, 1.0f); }
    public static void DrawLine(Rect rect, float width) { DrawLine(rect, GUI.contentColor, width); }
    public static void DrawLine(Rect rect, Color color, float width) { DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB) { DrawLine(pointA, pointB, GUI.contentColor, 1.0f); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color) { DrawLine(pointA, pointB, color, 1.0f); }
    public static void DrawLine(Vector2 pointA, Vector2 pointB, float width) { DrawLine(pointA, pointB, GUI.contentColor, width); }
	
	static Vector3 offset = new Vector3(0, -0.5f, 0); // Compensate for line width	
	static Matrix4x4 guiTransMat = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
	static Matrix4x4 guiTransMatInv = Matrix4x4.TRS(-offset, Quaternion.identity, Vector3.one);
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
    	if(width <= 0 || pointA == pointB || color.a == 0) return;

        // Save the current GUI matrix, since we're going to make changes to it.
        Matrix4x4 matrix = GUI.matrix;

		// GUI.matrix = Matrix4x4.identity;
       
		/*
		float angle = Mathf.Atan2 (pointB.y - pointA.y, pointB.x - pointA.x) * 180f / Mathf.PI;
		float length = Vector2.Distance(pointA, pointB);

		GUIUtility.RotateAroundPivot (angle, pointA);
		GUI.DrawTexture (new Rect (pointA.x, pointA.y, length, width), lineTex);
		GUI.matrix = matrix;
		return;
		*/
 
        // Store current GUI color, so we can switch it back later,
        // and set the GUI color to the color parameter
        Color savedColor = GUI.color;
        GUI.color = color;

	    var delta = (Vector3)(pointB-pointA);
		Quaternion guiRot = Quaternion.FromToRotation(Vector2.right, delta);
		Matrix4x4 guiRotMat = Matrix4x4.TRS((Vector3)pointA, guiRot, new Vector3(delta.magnitude, width, 1));
		GUI.matrix = guiTransMatInv * guiRotMat * guiTransMat;
        // Finally, draw the actual line.
        // We're really only drawing a 1x1 texture from pointA.
        // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
        //  render with the proper width, length, and angle.
		GUI.DrawTexture(new Rect(0, 0, 1, 1), Texture2D.whiteTexture);   // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.matrix = matrix;
        GUI.color = savedColor;
    }

	public static void DrawCircle (Vector2 center, float radius, Color color, float width, int numPoints = 20) {
		var step = Mathf.PI * 2f / (numPoints-1);
		int i = 0;
		Vector2 lastOffset = MathX.RadiansToVector2(i * step) * radius;
    	for(i = 1; i < numPoints; i++) {
			var offset = MathX.RadiansToVector2(i * step) * radius;
			DrawLine(center + lastOffset, center + offset, color, width);
			lastOffset = offset;
    	}
    }

	public static void DrawWireBox (Rect rect, float width) {
		DrawLine(rect.TopLeft(), rect.TopRight(), width);
		DrawLine(rect.TopRight(), rect.BottomRight(), width);
		DrawLine(rect.BottomRight(), rect.BottomLeft(), width);
		DrawLine(rect.BottomLeft(), rect.TopLeft(), width);
	}

    public static void DrawLine(Vector2[] points, Color color, float width) {
        for(var i = 0; i < points.Length-1; i++) {
			DrawLine(points[i], points[i+1], color, width);
    	}
    }
    public static void DrawPolygon(Vector2[] points, Color color, float width) {
        for(var i = 0; i < points.Length; i++) {
			DrawLine(points[i], i+1 == points.Length ? points[0] : points[i+1], color, width);
    	}
    }

    public static void DrawSprite (Rect rect, Sprite sprite) {
        if(sprite == null) return;
        var aspect = sprite.rect.width/sprite.rect.height;
        rect = rect.CompressToFitAspectRatio(aspect);
        var texRect = new Rect(sprite.rect.x/sprite.texture.width, sprite.rect.y/sprite.texture.height, sprite.rect.width/sprite.texture.width, sprite.rect.height/sprite.texture.height);
        GUI.DrawTextureWithTexCoords(rect, sprite.texture, texRect, true);
    }
    

	public static Rect ViewportToGUIRect (Rect rect) {
		var screenRect = ScreenX.ViewportToScreenRect(rect);
		return ScreenToGUIRect(screenRect);
	}
	
	public static Vector2 ViewportToGUIPoint (Vector2 point) {
		return ScreenToGUIPoint(ScreenX.ViewportToScreenPoint(point));
	}

	public static Rect ScreenToGUIRect (Rect rect) {
		rect.center = ScreenToGUIPoint(rect.center);
		return rect;
	}

	public static Vector2 ScreenToGUIPoint (Vector2 point) {
		return new Vector2(point.x, Screen.height-point.y);
	}
	public static Vector2[] ScreenToGUIPoints (Vector2[] points) {
        Vector2[] guiPoints = new Vector2[points.Length];
		for(var i = 0; i < points.Length; i++) {
			guiPoints[i] = ScreenToGUIPoint(points[i]);
    	}
        return guiPoints;
    }

//	public class DrawAction {
//		public object obj;
//		public System.Action action;
//
//		public DrawAction (object obj, System.Action action) {
//			this.obj = obj;
//			this.action = action;
//		}
//	}

	static Dictionary<object, System.Action> drawActions = new Dictionary<object, System.Action>();
	public static void StartDrawing (object obj, System.Action drawAction) {
		if(drawActions.ContainsKey(obj)) drawActions[obj] = drawAction;
		else drawActions.Add(obj, drawAction);
	}

	public static void StopDrawing (object obj) {
		if(drawActions.ContainsKey(obj)) drawActions.Remove(obj);
	}

	void OnGUI () {
//		GUI.Box(new Rect(0,60,10,100), "");
		//Debug.Log(drawActions.Count);
		foreach(var drawAction in drawActions) {
			drawAction.Value();
//			Debug.Log(123);
//			GUI.Box(new Rect(20,60,10,100), "");
//			GUILayout.Label(moveModel.targetPoint.ToString());
//			GUILayout.Label(moveModel.strength.ToString());
//			GUILayout.EndArea();
		}
//		drawActions.Clear();
	}
}
