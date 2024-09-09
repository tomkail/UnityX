using UnityEngine;
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
		get => transform.position;
		set => transform.position = value;
	}
	
	public Quaternion rotation {
		get => transform.rotation;
		set => transform.rotation = value;
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
        get => _orthographic;
        set {
            _orthographic = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }


	[SerializeField]
    float _orthographicSize;
    public float orthographicSize {
        get => _orthographicSize;
        set {
            _orthographicSize = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }


	[SerializeField]
    float _fieldOfView;
    public float fieldOfView {
        get => _fieldOfView;
        set {
            _fieldOfView = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }

	[SerializeField]
    float _nearClipPlane;
    public float nearClipPlane {
        get => _nearClipPlane;
        set {
            _nearClipPlane = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }

	[SerializeField]
    float _farClipPlane;
    public float farClipPlane {
        get => _farClipPlane;
        set {
            _farClipPlane = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }
	
    // This allows using a custom screen instead of the game view. If true, you must supply values to customScreenParams; 
    public bool useCustomScreen;
    public ScreenParams customScreenParams;

    public ScreenParams screenParams {
	    get {
		    if (useCustomScreen) return customScreenParams;
		    else return ScreenParams.gameScreenParams;
	    }
    }
    public float screenWidth => screenParams.width;
    public float screenHeight => screenParams.height;
    [System.Serializable]
    public struct ScreenParams {
	    public float width;
	    public float height;

	    public static ScreenParams @default => new ScreenParams(1920, 1080);
	    public static ScreenParams gameScreenParams => new ScreenParams(gameScreenWidth, gameScreenHeight);
	    
	    // ARGH I hate this. It's necessary because screen/display don't return the values for game view in some editor contexts (using inspector windows, for example)
	    public static int gameScreenWidth {
		    get {
#if UNITY_EDITOR
			    var res = UnityEditor.UnityStats.screenRes.Split('x');
			    var width = int.Parse(res[0]);
			    if (width != 0) return width;
#endif
			    // Consider adding target displays, then replace with this.
			    // Display.displays[0].renderingWidth
			    return Screen.width;
		    }
	    }
	    public static int gameScreenHeight {
		    get {
#if UNITY_EDITOR
			    var res = UnityEditor.UnityStats.screenRes.Split('x');
			    var height = int.Parse(res[1]);
			    if (height != 0) return height;
#endif
			    // Consider adding target displays, then replace with this.
			    // Display.displays[0].renderingHeight
			    return Screen.height;
		    }
	    }

	    public ScreenParams(float width, float height) {
		    this.width = width;
		    this.height = height;
	    }
    }
    
    Rect _rect;
    public Rect rect {
        get => _rect;
        set {
            _rect = value;
            _projectionMatrixSet = false;
            _inverseProjectionMatrixSet = false;
        }
    }

    Rect clampedRect => Rect.MinMaxRect(Mathf.Clamp01(rect.xMin),Mathf.Clamp01(rect.yMin),Mathf.Clamp01(rect.xMax),Mathf.Clamp01(rect.yMax));
    public float aspect => (screenWidth * clampedRect.width)/(screenHeight * clampedRect.height);
    
	public int pixelWidth => Mathf.RoundToInt(screenWidth * clampedRect.width);

	public int pixelHeight => Mathf.RoundToInt(screenHeight * clampedRect.height);


	// https://docs.unity3d.com/ScriptReference/Camera-worldToCameraMatrix
	// "Note that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis."
	public Matrix4x4 cameraToWorldMatrix => Matrix4x4.TRS(position, rotation, new Vector3(1, 1, -1));

	public Matrix4x4 worldToCameraMatrix => cameraToWorldMatrix.inverse;

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
		_fieldOfView = defaultFieldOfView;
		_nearClipPlane = defaultNearClipPlane;
		_farClipPlane = defaultFarClipPlane;
		_orthographic = defaultOrthographic;
		_orthographicSize = defaultOrthographicSize;
        _rect = defaultRect;

        useCustomScreen = false;
        customScreenParams = ScreenParams.@default;
        
        _projectionMatrix = Matrix4x4.identity;
        _projectionMatrixSet = false;
        _inverseProjectionMatrix = Matrix4x4.identity;
        _inverseProjectionMatrixSet = false;
	}

	// SerializableCamera() {
	// 	transform = SerializableTransform.identity;
	// 	_fieldOfView = defaultFieldOfView;
	// 	_nearClipPlane = defaultNearClipPlane;
	// 	_farClipPlane = defaultFarClipPlane;
	// 	_orthographic = defaultOrthographic;
	// 	_orthographicSize = defaultOrthographicSize;
	// 	_rect = defaultRect;
	//
	// 	_projectionMatrix = Matrix4x4.identity;
	// 	_projectionMatrixSet = false;
	// 	_inverseProjectionMatrix = Matrix4x4.identity;
	// 	_inverseProjectionMatrixSet = false;
	// }

	public SerializableCamera (Camera camera) {
		Debug.Assert(camera);
		transform = new SerializableTransform(camera.transform);
		_fieldOfView = camera.fieldOfView;
		_nearClipPlane = camera.nearClipPlane;
		_farClipPlane = camera.farClipPlane;
		_orthographic = camera.orthographic;
		_orthographicSize = camera.orthographicSize;
        _rect = camera.rect;
        
        useCustomScreen = false;
        customScreenParams = ScreenParams.@default;

        _projectionMatrix = Matrix4x4.identity;
        _projectionMatrixSet = false;
        _inverseProjectionMatrix = Matrix4x4.identity;
        _inverseProjectionMatrixSet = false;
	}
	
	public SerializableCamera (Vector3 position, Quaternion rotation, float fieldOfView, float aspectRatio) {
		transform = new SerializableTransform(position, rotation);
		_fieldOfView = fieldOfView;
		_nearClipPlane = defaultNearClipPlane;
		_farClipPlane = defaultFarClipPlane;
		_orthographic = defaultOrthographic;
		_orthographicSize = defaultOrthographicSize;
        _rect = defaultRect;
        
        useCustomScreen = false;
        customScreenParams = ScreenParams.@default;
        
        _projectionMatrix = Matrix4x4.identity;
        _projectionMatrixSet = false;
        _inverseProjectionMatrix = Matrix4x4.identity;
        _inverseProjectionMatrixSet = false;
	}
	
	public SerializableCamera (Vector3 position, Quaternion rotation, float fieldOfView, float aspectRatio, float nearClipPlane, float farClipPlane) {
		transform = new SerializableTransform(position, rotation);
		_fieldOfView = fieldOfView;
		_nearClipPlane = nearClipPlane;
		_farClipPlane = farClipPlane;
		_orthographic = defaultOrthographic;
		_orthographicSize = defaultOrthographicSize;
        _rect = defaultRect;
        
        useCustomScreen = false;
        customScreenParams = ScreenParams.@default;
        
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
		transform.ApplyFrom(camera.transform);
		fieldOfView = camera.fieldOfView;
		nearClipPlane = camera.nearClipPlane;
		farClipPlane = camera.farClipPlane;
		orthographic = camera.orthographic;
		orthographicSize = camera.orthographicSize;
        rect = camera.rect;
	}

	public void ApplyFrom(SerializableCamera camera) {
		transform.ApplyFrom(camera.transform);
		fieldOfView = camera.fieldOfView;
		nearClipPlane = camera.nearClipPlane;
		farClipPlane = camera.farClipPlane;
		orthographic = camera.orthographic;
		orthographicSize = camera.orthographicSize;
        rect = camera.rect;
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
		
		// Orthographic has to be handled separately because the method below assumes that two points at different distances will have a direction different from that of the camera.
		// That said, it's just a simpler version of the same code.
		if (orthographic) {
			Vector3 p2 = new Vector3(viewportPoint.x, viewportPoint.y, 1);
			Vector3 worldPointFar = inverseProjectionMatrix.MultiplyPoint(p2);
			Vector3 point = new Vector3(worldPointFar.x, worldPointFar.y, viewportPoint.z);
			
			// Get this point relative to the camera
			point = transform.localToWorldDirectionMatrix.MultiplyPoint(point);
			return point;
		}
		// This works by tracing a ray in a direction defined by the viewport point and getting it at a distance in the direction of the camera
		else {
			Vector3 p1 = new Vector3(viewportPoint.x, viewportPoint.y, -1);
			Vector3 p2 = new Vector3(viewportPoint.x, viewportPoint.y, 1);

			Vector3 worldPointNear = inverseProjectionMatrix.MultiplyPoint(p1);
			Vector3 worldPointFar = inverseProjectionMatrix.MultiplyPoint(p2);
			// Compensate for Unity's inverted z
			worldPointNear.z = -worldPointNear.z;
			worldPointFar.z = -worldPointFar.z;

			// Create a direction that can be projected forward in world space.
			Vector3 dir = worldPointFar - worldPointNear;
			Vector3 normalizedDir = dir.normalized;

			// Get the length of the vector required to reach the target direction when using the direction we just calculated.
			float dotLength = Vector3.Dot(Vector3.forward, normalizedDir);
			// Debug.Log(dotLength);
			Vector3 point = normalizedDir * viewportPoint.z / dotLength;

			// Get this point relative to the camera
			point = transform.localToWorldDirectionMatrix.MultiplyPoint(point);
			return point;
		}
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
	
	public Vector3 ScreenToWorldPoint (Vector3 screenPoint) {
		return ViewportToWorldPoint(new Vector3(screenPoint.x * 1f/screenWidth, screenPoint.y * 1f/screenHeight, screenPoint.z));
	}
	
	public Ray ScreenPointToRay (Vector3 screenPoint) {
		return ViewportPointToRay(new Vector3(screenPoint.x * 1f/screenWidth, screenPoint.y * 1f/screenHeight, screenPoint.z));
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

	public static SerializableCamera Lerp(SerializableCamera a, SerializableCamera b, float l) {
		return new SerializableCamera() {
			transform = SerializableTransform.Lerp(a.transform, b.transform, l),
			orthographic = l < 0.5f ? a.orthographic : b.orthographic,
			orthographicSize = Mathf.Lerp(a.orthographicSize, b.orthographicSize, l),
			fieldOfView = Mathf.Lerp(a.fieldOfView, b.fieldOfView, l),
			nearClipPlane = Mathf.Lerp(a.nearClipPlane, b.nearClipPlane, l),
			farClipPlane = Mathf.Lerp(a.farClipPlane, b.farClipPlane, l),
			rect = Rect.MinMaxRect(
				Mathf.Lerp(a.rect.min.x, b.rect.min.x, l),
				Mathf.Lerp(a.rect.min.y, b.rect.min.y, l),
				Mathf.Lerp(a.rect.max.x, b.rect.max.x, l),
				Mathf.Lerp(a.rect.max.y, b.rect.max.y, l)
			)
		};
	}
	
	public static SerializableCamera LerpUnclamped(SerializableCamera a, SerializableCamera b, float l) {
		return new SerializableCamera() {
			transform = SerializableTransform.LerpUnclamped(a.transform, b.transform, l),
			orthographic = l < 0.5f ? a.orthographic : b.orthographic,
			orthographicSize = Mathf.LerpUnclamped(a.orthographicSize, b.orthographicSize, l),
			fieldOfView = Mathf.LerpUnclamped(a.fieldOfView, b.fieldOfView, l),
			nearClipPlane = Mathf.LerpUnclamped(a.nearClipPlane, b.nearClipPlane, l),
			farClipPlane = Mathf.LerpUnclamped(a.farClipPlane, b.farClipPlane, l),
			rect = Rect.MinMaxRect(
				Mathf.LerpUnclamped(a.rect.min.x, b.rect.min.x, l),
				Mathf.LerpUnclamped(a.rect.min.y, b.rect.min.y, l),
				Mathf.LerpUnclamped(a.rect.max.x, b.rect.max.x, l),
				Mathf.LerpUnclamped(a.rect.max.y, b.rect.max.y, l)
			)
		};
	}
	
	public override string ToString () {
		return $"[SerializableCamera: position={position}, rotation={rotation}, field of view={fieldOfView}, aspect={aspect}, near={nearClipPlane}, far={farClipPlane}, rect={rect}, orthSize={orthographicSize}]";
	}
}