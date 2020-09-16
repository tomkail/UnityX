﻿// http://www.unity3d-france.com/unity/phpBB3/viewtopic.php?f=24&t=5409
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GLDebug : MonoSingleton<GLDebug> {
    private struct Line {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public float startTime;
        public float duration;
		public int startFrame;

        public Line (Vector3 start, Vector3 end, Color color, float startTime, float duration) {
			this.start = start;
			this.end = end;
			this.color = color;
			this.startTime = startTime;
			this.duration = duration;
			startFrame = Time.frameCount;
        }
       
        public bool ShouldCull () {
			if(duration <= 0) return startFrame < Time.frameCount;
            else return Time.time - startTime > duration;
        }

        public void DrawGizmoLine () {
			Gizmos.color = color;
			Gizmos.DrawLine (start, end);
        }

        public void DrawGLLine () {
			GL.Color (color);
			GL.Vertex (start);
			GL.Vertex (end);
        }
    }

	private static Material _matZOn;
	private static Material matZOn {
		get {
			if(_matZOn == null) {
				Shader shader = Shader.Find("Debug/GLlineZOn");
				_matZOn = new Material(shader);
				_matZOn.hideFlags = HideFlags.HideAndDontSave;
//				_matZOn.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			return _matZOn;
		}
	}
	private static Material _matZOff;
	private static Material matZOff {
		get {
			if(_matZOn == null) {
				Shader shader = Shader.Find("Debug/GLlineZOff");
				_matZOn = new Material(shader);
				_matZOn.hideFlags = HideFlags.HideAndDontSave;
//				_matZOn.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			return _matZOn;
		}
	}
   
    public bool displayLines = true;
	#if UNITY_EDITOR
    public bool displayGizmos = true;
	#endif
   
    private List<Line> linesZOn = new List<Line>();
    private List<Line> linesZOff = new List<Line>();

	public IEnumerator Start() {
		if(Application.isPlaying) {
			while (true) {
				yield return null;
				CullOldLines();
			}
		}
	}

	#if UNITY_EDITOR
	void LateUpdate () {
		if(!Application.isPlaying) {
			CullOldLines();
		}
	}

    void OnDrawGizmos () {
		if (!displayGizmos) return;

		linesZOn.ForEach(x => x.DrawGizmoLine());
		linesZOff.ForEach(x => x.DrawGizmoLine());
    }
	#endif

	void CullOldLines () {
		linesZOn = linesZOn.Where (l => !l.ShouldCull ()).ToList ();
		linesZOff = linesZOff.Where (l => !l.ShouldCull ()).ToList ();
	}

    void OnPostRender () {
        if (!displayLines) return;

        matZOn.SetPass (0);
        GL.Begin (GL.LINES);
		linesZOff.ForEach(x => x.DrawGLLine());
        GL.End ();
       
        matZOff.SetPass (0);
        GL.Begin (GL.LINES);
		linesZOn.ForEach(x => x.DrawGLLine());
        GL.End ();
    }

    private static void DrawLine (Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = false)
    {
        if (duration == 0 && !Instance.displayLines) return;
        if (start == end) return;
        if (depthTest) Instance.linesZOn.Add (new Line (start, end, color, Time.time, duration));
        else Instance.linesZOff.Add (new Line (start, end, color, Time.time, duration));
    }

    /// <summary>
    /// Draw a line from start to end with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the line is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the line should start.</param>
    /// <param name="end">Point in world space where the line should end.</param>
    /// <param name="color">Color of the line.</param>
    /// <param name="duration">How long the line should be visible for.</param>
    /// <param name="depthTest">Should the line be obscured by objects closer to the camera ?</param>
    public static void DrawLine (Vector3 start, Vector3 end, Color? color = null, float duration = 0, bool depthTest = false)
    {
		DrawLine (start, end, color ?? Color.white, duration, depthTest);
    }

    /// <summary>
    /// Draw a line from start to start + dir with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the ray is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the ray should start.</param>
    /// <param name="dir">Direction and length of the ray.</param>
    /// <param name="color">Color of the ray.</param>
    /// <param name="duration">How long the ray should be visible for.</param>
    /// <param name="depthTest">Should the ray be obscured by objects closer to the camera ?</param>
    public static void DrawRay (Vector3 start, Vector3 dir, Color? color = null, float duration = 0, bool depthTest = false)
    {
            if (dir == Vector3.zero)
                    return;
            DrawLine (start, start + dir, color, duration, depthTest);
    }
   
    /// <summary>
    /// Draw an arrow from start to end with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the arrow is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the arrow should start.</param>
    /// <param name="end">Point in world space where the arrow should end.</param>
    /// <param name="arrowHeadLength">Length of the 2 lines of the head.</param>
    /// <param name="arrowHeadAngle">Angle between the main line and each of the 2 smaller lines of the head.</param>
    /// <param name="color">Color of the arrow.</param>
    /// <param name="duration">How long the arrow should be visible for.</param>
    /// <param name="depthTest">Should the arrow be obscured by objects closer to the camera ?</param>
    public static void DrawLineArrow (Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
    {
            DrawArrow (start, end - start, arrowHeadLength, arrowHeadAngle, color, duration, depthTest);
    }
   
    /// <summary>
    /// Draw an arrow from start to start + dir with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the arrow is rendered 1 frame.
    /// </summary>
    /// <param name="start">Point in world space where the arrow should start.</param>
    /// <param name="dir">Direction and length of the arrow.</param>
    /// <param name="arrowHeadLength">Length of the 2 lines of the head.</param>
    /// <param name="arrowHeadAngle">Angle between the main line and each of the 2 smaller lines of the head.</param>
    /// <param name="color">Color of the arrow.</param>
    /// <param name="duration">How long the arrow should be visible for.</param>
    /// <param name="depthTest">Should the arrow be obscured by objects closer to the camera ?</param>
    public static void DrawArrow (Vector3 start, Vector3 dir, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
    {
            if (dir == Vector3.zero)
                    return;
            DrawRay (start, dir, color, duration, depthTest);
            Vector3 right = Quaternion.LookRotation (dir) * Quaternion.Euler (0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left  = Quaternion.LookRotation (dir) * Quaternion.Euler (0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            DrawRay (start + dir, right * arrowHeadLength, color, duration, depthTest);
            DrawRay (start + dir, left  * arrowHeadLength, color, duration, depthTest);
    }
   
    /// <summary>
    /// Draw a square with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="pos">Center of the square in world space.</param>
    /// <param name="rot">Rotation of the square in euler angles in world space.</param>
    /// <param name="scale">Size of the square.</param>
    /// <param name="color">Color of the square.</param>
    /// <param name="duration">How long the square should be visible for.</param>
    /// <param name="depthTest">Should the square be obscured by objects closer to the camera ?</param>
    public static void DrawSquare (Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
            DrawSquare (Matrix4x4.TRS (pos, Quaternion.Euler (rot ?? Vector3.zero), scale ?? Vector3.one), color, duration, depthTest);
    }
    /// <summary>
    /// Draw a square with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="pos">Center of the square in world space.</param>
    /// <param name="rot">Rotation of the square in world space.</param>
    /// <param name="scale">Size of the square.</param>
    /// <param name="color">Color of the square.</param>
    /// <param name="duration">How long the square should be visible for.</param>
    /// <param name="depthTest">Should the square be obscured by objects closer to the camera ?</param>
    public static void DrawSquare (Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
            DrawSquare (Matrix4x4.TRS (pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
    }
    /// <summary>
    /// Draw a square with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="matrix">Transformation matrix which represent the square transform.</param>
    /// <param name="color">Color of the square.</param>
    /// <param name="duration">How long the square should be visible for.</param>
    /// <param name="depthTest">Should the square be obscured by objects closer to the camera ?</param>
    public static void DrawSquare (Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
    {
            Vector3
                    p_1     = matrix.MultiplyPoint3x4 (new Vector3 ( .5f, 0,  .5f)),
                    p_2     = matrix.MultiplyPoint3x4 (new Vector3 ( .5f, 0, -.5f)),
                    p_3     = matrix.MultiplyPoint3x4 (new Vector3 (-.5f, 0, -.5f)),
                    p_4     = matrix.MultiplyPoint3x4 (new Vector3 (-.5f, 0,  .5f));
           
            DrawLine (p_1, p_2, color, duration, depthTest);
            DrawLine (p_2, p_3, color, duration, depthTest);
            DrawLine (p_3, p_4, color, duration, depthTest);
            DrawLine (p_4, p_1, color, duration, depthTest);
    }
   
    /// <summary>
    /// Draw a cube with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="pos">Center of the cube in world space.</param>
    /// <param name="rot">Rotation of the cube in euler angles in world space.</param>
    /// <param name="scale">Size of the cube.</param>
    /// <param name="color">Color of the cube.</param>
    /// <param name="duration">How long the cube should be visible for.</param>
    /// <param name="depthTest">Should the cube be obscured by objects closer to the camera ?</param>
    public static void DrawCube (Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
            DrawCube (Matrix4x4.TRS (pos, Quaternion.Euler (rot ?? Vector3.zero), scale ?? Vector3.one), color, duration, depthTest);
    }
    /// <summary>
    /// Draw a cube with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="pos">Center of the cube in world space.</param>
    /// <param name="rot">Rotation of the cube in world space.</param>
    /// <param name="scale">Size of the cube.</param>
    /// <param name="color">Color of the cube.</param>
    /// <param name="duration">How long the cube should be visible for.</param>
    /// <param name="depthTest">Should the cube be obscured by objects closer to the camera ?</param>
    public static void DrawCube (Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
    {
            DrawCube (Matrix4x4.TRS (pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
    }
    /// <summary>
    /// Draw a cube with color for a duration of time and with or without depth testing.
    /// If duration is 0 then the square is renderer 1 frame.
    /// </summary>
    /// <param name="matrix">Transformation matrix which represent the cube transform.</param>
    /// <param name="color">Color of the cube.</param>
    /// <param name="duration">How long the cube should be visible for.</param>
    /// <param name="depthTest">Should the cube be obscured by objects closer to the camera ?</param>
    public static void DrawCube (Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
    {
            Vector3
                    down_1  = matrix.MultiplyPoint3x4 (new Vector3 ( .5f, -.5f,  .5f)),
                    down_2  = matrix.MultiplyPoint3x4 (new Vector3 ( .5f, -.5f, -.5f)),
                    down_3  = matrix.MultiplyPoint3x4 (new Vector3 (-.5f, -.5f, -.5f)),
                    down_4  = matrix.MultiplyPoint3x4 (new Vector3 (-.5f, -.5f,  .5f)),
                    up_1    = matrix.MultiplyPoint3x4 (new Vector3 ( .5f,  .5f,  .5f)),
                    up_2    = matrix.MultiplyPoint3x4 (new Vector3 ( .5f,  .5f, -.5f)),
                    up_3    = matrix.MultiplyPoint3x4 (new Vector3 (-.5f,  .5f, -.5f)),
                    up_4    = matrix.MultiplyPoint3x4 (new Vector3 (-.5f,  .5f,  .5f));
           
            DrawLine (down_1, down_2, color, duration, depthTest);
            DrawLine (down_2, down_3, color, duration, depthTest);
            DrawLine (down_3, down_4, color, duration, depthTest);
            DrawLine (down_4, down_1, color, duration, depthTest);
           
            DrawLine (down_1, up_1, color, duration, depthTest);
            DrawLine (down_2, up_2, color, duration, depthTest);
            DrawLine (down_3, up_3, color, duration, depthTest);
            DrawLine (down_4, up_4, color, duration, depthTest);
           
            DrawLine (up_1, up_2, color, duration, depthTest);
            DrawLine (up_2, up_3, color, duration, depthTest);
            DrawLine (up_3, up_4, color, duration, depthTest);
            DrawLine (up_4, up_1, color, duration, depthTest);
    }


	// EXTRAS
    public static void DrawCircle (Vector3 center, float radius, Color? color = null, float duration = 0, bool depthTest = false)
    {
//            float degRad = Mathf.PI / 180;
		for(float theta = 0.0f; theta < (2*Mathf.PI); theta += 0.2f)
		{
			Vector3 ci = (new Vector3(Mathf.Cos(theta) * radius + center.x, Mathf.Sin(theta) * radius + center.y, center.z));
			DrawLine (ci, ci+new Vector3(0,0.02f,0), color, duration, depthTest);
		}
    }
}