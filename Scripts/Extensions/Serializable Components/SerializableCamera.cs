using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mimics the data setup of a Unity Camera
/// Used for performing operations on the transform and properties of a camera without actually needing a camera.
/// </summary>
[System.Serializable]
public struct SerializableCamera  {
	
	public const bool defaultOrthographic = false;
	public const float defaultFieldOfView = 60;
	public const float defaultOrthographicSize = 10;
	public const float defaultAspectRatio = 1;
	public const float defaultNearClipPlane = 0.01f;
	public const float defaultFarClipPlane = 1000;
	public static Rect defaultRect = new Rect(0,0,1,1);

	public SerializableTransform transform;
	
	public Vector3 position {
		get {
			return transform.position;
		} set {
			transform.position = value;
		}
	}
	
	public Quaternion rotation {
		get {
			return transform.rotation;
		} set {
			transform.rotation = value;
		}
	}

    bool _projectionMatrixSet;
    Matrix4x4 _projectionMatrix;
    public Matrix4x4 projectionMatrix {
        get {
            if(!_projectionMatrixSet) {
                _projectionMatrixSet = true;
                if(orthographic) {
                    _projectionMatrix = Matrix4x4.Ortho(-aspect * orthographicSize, aspect * orthographicSize, -orthographicSize, orthographicSize, nearClipPlane, farClipPlane);
                } else {
                    _projectionMatrix = Matrix4x4.Perspective(fieldOfView, aspect, nearClipPlane, farClipPlane);
                }
            }
            return _projectionMatrix;
        } set {
			_projectionMatrixSet = true;
			_projectionMatrix = value;
		}
    }
    bool _inverseProjectionMatrixSet;
    Matrix4x4 _inverseProjectionMatrix;
    public Matrix4x4 inverseProjectionMatrix {
        get {
            if(!_inverseProjectionMatrixSet) {
                _inverseProjectionMatrixSet = true;
                _inverseProjectionMatrix = projectionMatrix.inverse;
            }
            return _inverseProjectionMatrix;
        }
    }

    

	[SerializeField]
    bool _orthographic;
    public bool orthographic {
        get {
            return _orthographic;
        } set {
            _orthographic = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }


	[SerializeField]
    float _orthographicSize;
    public float orthographicSize {
        get {
            return _orthographicSize;
        } set {
            _orthographicSize = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }


	[SerializeField]
    float _fieldOfView;
    public float fieldOfView {
        get {
            return _fieldOfView;
        } set {
            _fieldOfView = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }

	[SerializeField]
    float _nearClipPlane;
    public float nearClipPlane {
        get {
            return _nearClipPlane;
        } set {
            _nearClipPlane = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }

	[SerializeField]
    float _farClipPlane;
    public float farClipPlane {
        get {
            return _farClipPlane;
        } set {
            _farClipPlane = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }
    Rect _rect;
    public Rect rect {
        get {
            return _rect;
        } set {
            _rect = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }

    Rect clampedRect {
        get {
            return Rect.MinMaxRect(Mathf.Clamp01(rect.xMin),Mathf.Clamp01(rect.yMin),Mathf.Clamp01(rect.xMax),Mathf.Clamp01(rect.yMax));
        }
    }
	public float aspect {
        get {
            return (screenWidth * clampedRect.width)/(screenHeight * clampedRect.height);
        }
    }


    public int pixelWidth {
        get {
            return Mathf.RoundToInt(screenWidth * clampedRect.width);
        }
    }
    public int pixelHeight {
        get {
            return Mathf.RoundToInt(screenHeight * clampedRect.height);
        }
    }

	// ARGH I hate this. It's necessary because screen/display don't return the values for game view in some editor contexts (using inspector windows, for example)
	static int screenWidth {
		get {
			#if UNITY_EDITOR
			var res = UnityEditor.UnityStats.screenRes.Split('x');
			return int.Parse(res[0]);
			#else
			// Consider adding target displays, then replace with this.
			// Display.displays[0].renderingWidth
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
			// Consider adding target displays, then replace with this.
			// Display.displays[0].renderingHeight
			return Screen.height;
			#endif
		}
	}


	// https://docs.unity3d.com/ScriptReference/Camera-worldToCameraMatrix
	// "Note that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis."
	public Matrix4x4 cameraToWorldMatrix {
		get {
			return Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1));
		}
	}
	public Matrix4x4 worldToCameraMatrix {
		get {
			return cameraToWorldMatrix.inverse;
		}
	}
	
    // Not currently used since what screen conversion code is very tested, and this is less so - kept since handy for passing to shaders. 
    public Matrix4x4 worldToCameraViewportMatrix {
        get {
            // Transform from (-0.5,-0.5)/(0.5,0.5) space to camera rect space
            var cameraToViewportMatrix = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0), Quaternion.identity, new Vector3(0.5f, 0.5f, 1));
            return cameraToViewportMatrix * projectionMatrix * worldToCameraMatrix;
        }
    }
    // Not currently used since what screen conversion code is very tested, and this is less so - kept since handy for passing to shaders. 
    public Matrix4x4 worldToCameraPixelRectMatrix {
        get {
            var viewportToScreenMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(screenWidth, screenHeight, 1));
            return viewportToScreenMatrix * worldToCameraViewportMatrix;
        }
    }

    // Not currently used since what screen conversion code is very tested, and this is less so - kept since handy for passing to shaders. 
    public Matrix4x4 worldToScreenViewportMatrix {
        get {
            // Transform from (-0.5,-0.5)/(0.5,0.5) space to (0,0)/(1,1) space
            var cameraToViewportMatrix = Matrix4x4.TRS(new Vector3(clampedRect.x, clampedRect.y, 0), Quaternion.identity, new Vector3(clampedRect.width, clampedRect.height, 1));
            return cameraToViewportMatrix * worldToCameraViewportMatrix;
        }
    }
    // Not currently used since what screen conversion code is very tested, and this is less so - kept since handy for passing to shaders. 
    public Matrix4x4 worldToScreenMatrix {
        get {
            var viewportToScreenMatrix = Matrix4x4.TRS(Vector3.one, Quaternion.identity, new Vector3(screenWidth, screenHeight, 1));
            return viewportToScreenMatrix * worldToScreenViewportMatrix;
        }
    }
	
	public static SerializableCamera identity {
		get {
			SerializableCamera serializableCamera = new SerializableCamera();
			serializableCamera.transform = SerializableTransform.identity;
			serializableCamera.fieldOfView = defaultFieldOfView;
			serializableCamera.nearClipPlane = defaultNearClipPlane;
			serializableCamera.farClipPlane = defaultFarClipPlane;
			serializableCamera.orthographic = defaultOrthographic;
			serializableCamera.orthographicSize = defaultOrthographicSize;
			serializableCamera.rect = defaultRect;
			return serializableCamera;
		}
	}

	public SerializableCamera (SerializableTransform transform) {
		this.transform = transform;
		this._fieldOfView = defaultFieldOfView;
		this._nearClipPlane = defaultNearClipPlane;
		this._farClipPlane = defaultFarClipPlane;
		this._orthographic = defaultOrthographic;
		this._orthographicSize = defaultOrthographicSize;
        this._rect = defaultRect;
        
        _projectionMatrix = Matrix4x4.identity;
        _projectionMatrixSet = false;
        _inverseProjectionMatrix = Matrix4x4.identity;
        _inverseProjectionMatrixSet = false;
	}
	
	public SerializableCamera (Camera camera) {
		Debug.Assert(camera);
		this.transform = new SerializableTransform(camera.transform);
		this._fieldOfView = camera.fieldOfView;
		this._nearClipPlane = camera.nearClipPlane;
		this._farClipPlane = camera.farClipPlane;
		this._orthographic = camera.orthographic;
		this._orthographicSize = camera.orthographicSize;
        this._rect = camera.rect;

        _projectionMatrix = Matrix4x4.identity;
        _projectionMatrixSet = false;
        _inverseProjectionMatrix = Matrix4x4.identity;
        _inverseProjectionMatrixSet = false;
	}
	
	public SerializableCamera (Vector3 position, Quaternion rotation, float fieldOfView, float aspectRatio) {
		this.transform = new SerializableTransform(position, rotation);
		this._fieldOfView = fieldOfView;
		this._nearClipPlane = defaultNearClipPlane;
		this._farClipPlane = defaultFarClipPlane;
		this._orthographic = defaultOrthographic;
		this._orthographicSize = defaultOrthographicSize;
        this._rect = defaultRect;
        
        _projectionMatrix = Matrix4x4.identity;
        _projectionMatrixSet = false;
        _inverseProjectionMatrix = Matrix4x4.identity;
        _inverseProjectionMatrixSet = false;
	}
	
	public SerializableCamera (Vector3 position, Quaternion rotation, float fieldOfView, float aspectRatio, float nearClipPlane, float farClipPlane) {
		this.transform = new SerializableTransform(position, rotation);
		this._fieldOfView = fieldOfView;
		this._nearClipPlane = nearClipPlane;
		this._farClipPlane = farClipPlane;
		this._orthographic = defaultOrthographic;
		this._orthographicSize = defaultOrthographicSize;
        this._rect = defaultRect;
        
        _projectionMatrix = Matrix4x4.identity;
        _projectionMatrixSet = false;
        _inverseProjectionMatrix = Matrix4x4.identity;
        _inverseProjectionMatrixSet = false;
	}

	public void ApplyTo (Camera camera) {
		camera.transform.position = position;
		camera.transform.rotation = rotation;
		camera.fieldOfView = fieldOfView;
		camera.nearClipPlane = nearClipPlane;
		camera.farClipPlane = farClipPlane;
		camera.orthographic = orthographic;
		camera.orthographicSize = orthographicSize;
        camera.rect = rect;
	}

	public void ApplyFrom(Camera camera) {
		this.transform.ApplyFrom(camera.transform);
		this.fieldOfView = camera.fieldOfView;
		this.nearClipPlane = camera.nearClipPlane;
		this.farClipPlane = camera.farClipPlane;
		this.orthographic = camera.orthographic;
		this.orthographicSize = camera.orthographicSize;
        this.rect = camera.rect;
	}

	public void ApplyFrom(SerializableCamera camera) {
		this.transform.ApplyFrom(camera.transform);
		this.fieldOfView = camera.fieldOfView;
		this.nearClipPlane = camera.nearClipPlane;
		this.farClipPlane = camera.farClipPlane;
		this.orthographic = camera.orthographic;
		this.orthographicSize = camera.orthographicSize;
        this.rect = camera.rect;
	}
	
    // This is different to Unity's Camera by a tiny amount when the camera's rect is small. 
    // I suspect rounding issues in Unity's code, since theirs returns 0 when the rect width is around 0.0001
	public Vector3 WorldToViewportPoint (Vector3 worldPoint) {
		Matrix4x4 viewProjectionMatrix = projectionMatrix * worldToCameraMatrix;
		Vector3 viewportPoint = viewProjectionMatrix.MultiplyPoint(worldPoint);
		return new Vector3(0.5f + (viewportPoint.x * 0.5f), 0.5f + (viewportPoint.y * 0.5f), transform.worldToLocalDirectionMatrix.MultiplyPoint(worldPoint).z);

		// to try!
		// Vector4 worldPos = new Vector4(position.x, position.y, position.z, 1.0);
		// Vector4 viewPos = camera.worldToCameraMatrix * worldPos;
		// Vector4 projPos = camera.projectionMatrix * viewPos;
		// Vector3 ndcPos = new Vector3(projPos.x / projPos.w, projPos.y / projPos.w, projPos.z / projPos.w);
		// Vector3 viewportPos = new Vector3(ndcPos.x * 0.5 + 0.5, ndcPos.y * 0.5 + 0.5, -viewPos.z);
	}

	public Vector2 WorldToScreenPoint (Vector3 worldPoint) {
        var viewportPoint = WorldToViewportPoint(worldPoint);
		return ViewportToScreenPoint(viewportPoint);
	}
	
	public Vector3 ViewportToWorldPoint (Vector3 viewportPoint) {
		viewportPoint.x = (viewportPoint.x - 0.5f) * 2;
		viewportPoint.y = (viewportPoint.y - 0.5f) * 2;
		
        Vector3 p1 = new Vector3(viewportPoint.x, viewportPoint.y, -1);
		Vector3 p2 = new Vector3(viewportPoint.x, viewportPoint.y, 1);
		
		Vector3 worldPointNear = inverseProjectionMatrix.MultiplyPoint(p1);
		Vector3 worldPointFar = inverseProjectionMatrix.MultiplyPoint(p2);
		// Compensate for Unity's inverted z
        worldPointNear.z = -worldPointNear.z;
        worldPointFar.z = -worldPointFar.z;
		
		// Create a direction that can be projected forward in world space.
		Vector3 dir = worldPointFar-worldPointNear;
		Vector3 normalizedDir = dir.normalized;
		
		// Get the length of the vector required to reach the target direction when using the direction we just calculated.
		float dotLength = Vector3.Dot (Vector3.forward, normalizedDir);
		// Debug.Log(dotLength);
		Vector3 point = normalizedDir * viewportPoint.z/dotLength;
		
		// Get this point relative to the camera
		point = transform.localToWorldDirectionMatrix.MultiplyPoint(point);
		return point;
	}

	public Ray ViewportPointToRay (Vector3 viewportPoint) {
		viewportPoint.x = (viewportPoint.x - 0.5f) * 2;
		viewportPoint.y = (viewportPoint.y - 0.5f) * 2;
		
        Vector3 p1 = new Vector3(viewportPoint.x, viewportPoint.y, -1);
		Vector3 p2 = new Vector3(viewportPoint.x, viewportPoint.y, 1);
		
		Vector3 worldPointNear = inverseProjectionMatrix.MultiplyPoint(p1);
		Vector3 worldPointFar = inverseProjectionMatrix.MultiplyPoint(p2);
		// Compensate for Unity's inverted z
        worldPointNear.z = -worldPointNear.z;
        worldPointFar.z = -worldPointFar.z;
		
		// Create a direction that can be projected forward in world space.
		Vector3 dir = worldPointFar-worldPointNear;
		Vector3 normalizedDir = dir.normalized;

		return new Ray(transform.localToWorldDirectionMatrix.MultiplyPoint(worldPointNear), transform.localToWorldDirectionMatrix.MultiplyVector(normalizedDir));
	}
	
	public Vector2 ScreenToWorldPoint (Vector3 screenPoint) {
		return ViewportToWorldPoint(Vector2.Scale(screenPoint, new Vector2(1f/screenWidth, 1f/screenHeight)));
	}
	
	public Ray ScreenPointToRay (Vector3 screenPoint) {
		return ViewportPointToRay(Vector2.Scale(screenPoint, new Vector2(1f/screenWidth, 1f/screenHeight)));
	}

    public Vector3 ScreenToViewportPoint (Vector3 screenPoint) {
		float InverseLerpUnclamped (float from, float to, float value) {
			return (value - from) / (to - from);
		}
        var x = InverseLerpUnclamped(screenWidth * clampedRect.xMin, screenWidth * clampedRect.xMax, screenPoint.x);
        var y = InverseLerpUnclamped(screenHeight * clampedRect.yMin, screenHeight * clampedRect.yMax, screenPoint.y);
		return new Vector3(x, y, screenPoint.z);
	}
    public Vector3 ViewportToScreenPoint (Vector3 viewportPoint) {
        var x = Mathf.LerpUnclamped(screenWidth * clampedRect.xMin, screenWidth * clampedRect.xMax, viewportPoint.x);
        var y = Mathf.LerpUnclamped(screenHeight * clampedRect.yMin, screenHeight * clampedRect.yMax, viewportPoint.y);
		return new Vector3(x, y, viewportPoint.z);
	}
	
	
	public Vector3[] WorldToViewportPoints (params Vector3[] input) {
		Vector3[] output = new Vector3[input.Length];
		for(int i = 0; i < output.Length; i++) {
			output[i] = WorldToViewportPoint(input[i]);
		}
		return output;
	}
	
	public Vector3[] WorldToViewportPoints (IList<Vector3> input) {
		Vector3[] output = new Vector3[input.Count];
		for(int i = 0; i < output.Length; i++) {
			output[i] = WorldToViewportPoint(input[i]);
		}
		return output;
	}
	
	
	public Vector2[] WorldToScreenPoints (params Vector3[] input) {
		Vector2[] output = new Vector2[input.Length];
		for(int i = 0; i < output.Length; i++)
			output[i] = WorldToScreenPoint(input[i]);
		return output;
	}
	public Vector2[] WorldToScreenPoints (IList<Vector3> input) {
		Vector2[] output = new Vector2[input.Count];
		for(int i = 0; i < output.Length; i++)
			output[i] = WorldToScreenPoint(input[i]);
		return output;
	}
	
	
	public Vector3[] ViewportToWorldPoints (params Vector3[] input) {
		Vector3[] output = new Vector3[input.Length];
		for(int i = 0; i < output.Length; i++)
			output[i] = ViewportToWorldPoint(input[i]);
		return output;
	}
	
	public Vector3[] ViewportToWorldPoints (IList<Vector3> input) {
		Vector3[] output = new Vector3[input.Count];
		for(int i = 0; i < output.Length; i++)
			output[i] = ViewportToWorldPoint(input[i]);
		return output;
	}


    public Rect ViewportToScreenRect (Rect rect) {
		var min = ViewportToScreenPoint(rect.min);
		var max = ViewportToScreenPoint(rect.max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}
	public Vector3 ViewportToScreenVector (Vector2 vector) {
		return ViewportToScreenPoint(vector) - ViewportToScreenPoint(Vector2.zero);
	}

	public Vector3 ViewportToWorldVector (Vector2 vector, float distance) {
		return ViewportToWorldPoint(new Vector3(0,0,distance)) - ViewportToWorldPoint(new Vector3(vector.x, vector.y, distance));
	}


    public Rect ScreenToViewportRect (Rect rect) {
		var min = ScreenToViewportPoint(rect.min);
		var max = ScreenToViewportPoint(rect.max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}
	public Vector3 ScreenToWorldVector (Vector2 vector, float distance) {
		return ScreenToWorldPoint(new Vector3(0,0,distance)) - ScreenToWorldPoint(new Vector3(vector.x, vector.y, distance));
	}
	public Vector3 ScreenToViewportVector (Vector2 vector) {
		return ScreenToViewportPoint(vector) - ScreenToViewportPoint(Vector2.zero);
	}



    public Vector2[] ScreenToViewportPoints (Vector2[] screenPoints) {
		Vector2[] viewportPoints = new Vector2[screenPoints.Length];
        for(int i = 0; i < viewportPoints.Length; i++) viewportPoints[i] = ScreenToViewportPoint(screenPoints[i]);
		return viewportPoints;
	}


	
	public float GetHorizontalFieldOfView () {
		return CameraX.GetHorizontalFieldOfView(fieldOfView, aspect);
	}
	
	public float GetFrustrumHeightAtDistance (float distance) {
		if(orthographic) return orthographicSize;
		return CameraX.GetFrustrumHeightAtDistance(distance, fieldOfView);
	}
	
	public float GetFrustrumWidthAtDistance (float distance) {
		if(orthographic) return aspect * orthographicSize;
		return CameraX.GetFrustrumWidthAtDistance(distance, fieldOfView, aspect);
	}
	
	public float GetDistanceAtFrustrumHeight (float frustumHeight) {
		Debug.Assert(!orthographic);
		return CameraX.GetDistanceAtFrustrumHeight(frustumHeight, fieldOfView);
	}
	
	public float GetDistanceAtFrustrumWidth (float frustumWidth) {

		return CameraX.GetDistanceAtFrustrumWidth(frustumWidth, fieldOfView, aspect);
	}
	
	public float GetFOVAngleAtWidthAndDistance (float frustumWidth, float distance) {
		return CameraX.GetFOVAngleAtWidthAndDistance(frustumWidth, distance, aspect);
	}
	
	public float ConvertFrustumWidthToFrustumHeight (float frustumWidth) {
		return CameraX.ConvertFrustumWidthToFrustumHeight(frustumWidth, aspect);
	}
	
	public float ConvertFrustumHeightToFrustumWidth (float frustumHeight) {
		return CameraX.ConvertFrustumHeightToFrustumWidth(frustumHeight, aspect);
	}
	
	public void DrawGizmos () {
		var cachedMatrix = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
		if(orthographic) {
			float spread = farClipPlane - nearClipPlane;
			float center = (farClipPlane + nearClipPlane)*0.5f;
			Gizmos.DrawWireCube(new Vector3(0,0,center), new Vector3(orthographicSize*2*aspect, orthographicSize*2, spread));
		} else {
			Gizmos.DrawFrustum(Vector3.zero, fieldOfView, farClipPlane, nearClipPlane, aspect);
		}
		Gizmos.matrix = cachedMatrix;
	}

	public override string ToString () {
		return string.Format ("[SerializableCamera: position={0}, rotation={1}, field of view={2}, aspect={3}, near={4}, far={5}, rect={6}, orthSize={7}]", position, rotation, fieldOfView, aspect, nearClipPlane, farClipPlane, rect, orthographicSize);
	}
}