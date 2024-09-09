
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class WorldSpaceUIElement : UIBehaviour {
	[SerializeField] bool _updateInEditMode = true;

	[SerializeField] Camera _worldCamera;
	public Camera worldCamera {
		get {
			if(_worldCamera == null) {
				_worldCamera = Camera.main;
				Debug.Log("No camera specified. Setting to current value of Camera.main: "+(_worldCamera == null ? "Null" : _worldCamera.name), this);
			}
			return _worldCamera;
		} set {
			if(_worldCamera == value)
				return;
			_worldCamera = value;
			Refresh();
		}
	}

	[SerializeField] Transform _target;
	public Transform target {
		get => _target;
		set {
			if(_target == value)
				return;
			_target = value;
			Refresh();
		}
	}

	public Vector3 worldPosition = Vector3.zero;
	public Quaternion worldRotation = Quaternion.identity;

	Vector3 targetPositionInternal => target == null ? worldPosition : target.position;

	Quaternion targetRotationInternal => target == null ? worldRotation : target.rotation;

	public bool updatePosition = true;
	public enum RotationMode {
		None,
		Rotation,
		RotationZ
	}
	public RotationMode updateRotation;
	public Vector3 worldPointingVectorForZRotation = new(0, 0, 1);
	public bool updateScale;
	public float scaleMultiplier = 1;
	public float minScale = 0.2f;
	public float maxScale = 1f;
	
	public bool clampToScreen;
	public bool onScreen;

	public bool updateOcclusion;
	public bool occluded;
	public int occlusionMask = Physics.DefaultRaycastLayers;
	
	bool _rectTransformSet;
	RectTransform _rectTransform;
	public RectTransform rectTransform {
		get {
			if(!_rectTransformSet) {
				if(transform is RectTransform) {
					_rectTransform = transform as RectTransform;
					_rectTransformSet = true;
				} else {
					Debug.LogWarning(gameObject.name+" is not a rect transform!", this);
				}
			}
			return _rectTransform;
		}
	}

	bool _rootCanvasSet;
	Canvas _rootCanvas;
	public Canvas rootCanvas {
		get {
			if(!_rootCanvasSet) SetRootCanvas();
			return _rootCanvas;
		}
	}

	RectTransform _rootCanvasRT;
	public RectTransform rootCanvasRT {
		get {
			if(!_rootCanvasSet) SetRootCanvas();
			return _rootCanvasRT;
		}
	}

	RectTransform parentRT {
		get {
			var parent = transform.parent;
			if(parent != null) {
				if(parent is RectTransform _parentRT) return _parentRT;
				else {
					Debug.LogWarning("Parent of "+gameObject.name+" is not a rect transform!", this);
					return null;
				}
			} else {
				return rectTransform;
			}
		}
	}
	
	void SetRootCanvas () {
		Canvas parentCanvas = transform.GetComponentInParent<Canvas>();
		if(parentCanvas == null) return;
		_rootCanvas = parentCanvas.rootCanvas;
		_rootCanvasRT = _rootCanvas.GetComponent<RectTransform>();
		_rootCanvasSet = true;
	}
	
	public Vector3 GetLocalScale () {
		if(rootCanvas.renderMode == RenderMode.WorldSpace && worldCamera.orthographic) {
//			return parentCanvas.transform.InverseTransformVector(Vector3.one);
			var scale = worldCamera.orthographicSize;
			scale *= scaleMultiplier;
			return Vector3.one * Mathf.Clamp(scale, minScale, maxScale);
		} else {
			return Vector3.one * GetClampedViewportScale(worldCamera, targetPositionInternal, scaleMultiplier, minScale, maxScale);
		}
	}

	public static float GetClampedViewportScale (Camera camera, Vector3 targetPoint, float targetScale, float minViewportScale, float maxViewportScale) {
		var distanceFromCamera = 0f;
		if(camera.orthographic) {
			distanceFromCamera = Mathf.Abs(Vector3.Dot(camera.transform.position-targetPoint, camera.transform.forward));
		} else {
			distanceFromCamera = Vector3.Distance(targetPoint, camera.transform.position);
		}
		float frustrumHeight = camera.ViewportToWorldPoint(new Vector3(0,1,distanceFromCamera)).y-camera.ViewportToWorldPoint(new Vector3(1,0,distanceFromCamera)).y;
		var scale = (1f/frustrumHeight);
		scale *= targetScale;
		return Mathf.Clamp(scale, minViewportScale, maxViewportScale);
	}
	
	protected override void Awake () {
		#if UNITY_EDITOR
		if(PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
        if(!Application.isPlaying && !_updateInEditMode) return;
		#endif
		
		_rectTransform = transform as RectTransform;
		_rectTransformSet = _rectTransform != null;

		if(worldCamera == null)
			worldCamera = Camera.main;
		SetRootCanvas();
	}

	protected override void OnEnable () {
		#if UNITY_EDITOR
		if(PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
        if(!Application.isPlaying && !_updateInEditMode) return;
		#endif
		Refresh();
	}
	
	protected override void OnTransformParentChanged () {
		#if UNITY_EDITOR
		if(PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
        if(!Application.isPlaying && !_updateInEditMode) return;
		#endif
		SetRootCanvas();
		base.OnTransformParentChanged ();
	}

	// LateUpdate because we want it to come even after camera updates
	void LateUpdate () {
		#if UNITY_EDITOR
		if(PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
        if(!Application.isPlaying && !_updateInEditMode) return;
		#endif
		Refresh();
	}

	public void Refresh () {
		if(worldCamera == null || rootCanvas == null) 
			return;
		if(updateScale)
			ScaleFromDistance();
		if(updatePosition)
			SetPositionFromWorldPosition();
		if(updateRotation == RotationMode.Rotation)
			SetAngleFromRotation();
		else if(updateRotation == RotationMode.RotationZ)
			SetAngleFromRotationZ();
		if(updateOcclusion)
			CheckOcclusion();
	}

	
	public void ScaleFromDistance () {
		rectTransform.localScale = GetLocalScale();
	}

	bool TryProjectWorldPoint(Vector3 worldPosition, out Vector3 projectedCanvasPosition) {
        var _parentRT = parentRT;
		if(_parentRT == null) {
			projectedCanvasPosition = Vector3.zero;
			return false;
		}
        
		Vector3? targetPositionNullable = WorldPointToLocalPointInRectangle(rootCanvas, worldCamera, _parentRT, worldPosition);
		if(targetPositionNullable == null) {
			projectedCanvasPosition = Vector3.zero;
			return false;
		}

		projectedCanvasPosition = (Vector3) targetPositionNullable;

		if(rootCanvas.renderMode == RenderMode.WorldSpace) {
			projectedCanvasPosition = transform.parent.InverseTransformPoint(worldPosition);
			projectedCanvasPosition.z = 0;
		}

		return true;
	}
	
	public void SetPositionFromWorldPosition () {
		
		Vector3 targetPosition;
		if( !TryProjectWorldPoint(targetPositionInternal, out targetPosition) ) {
			onScreen = false;
			return;
		}
		
		rectTransform.localPosition = targetPosition;
		
		if(clampToScreen) {
			Rect smallRect = GetScreenRect(rectTransform, rootCanvas);
			Rect largeRect = GetScreenRect(rootCanvasRT, rootCanvas);
			Rect clampedRect = ClampInsideKeepSize(smallRect, largeRect);
			Vector3? canvasSpace = ScreenPointToCanvasSpace(rootCanvas, clampedRect.position + smallRect.size * 0.5f);
			if(canvasSpace != null)
				rectTransform.position = (Vector3)canvasSpace;
		}
		
		onScreen = rootCanvasRT.rect.Contains((Vector2)targetPosition);
	}

	public void SetAngleFromRotation () {
		// if(!targetRotationInternal.IsValid()) return;
		rectTransform.rotation = Quaternion.Inverse(worldCamera.transform.rotation) * targetRotationInternal;
		// float angle = Vector2X.Degrees (Vector3X.ProjectOnPlane(targetRotationInternal * Vector3.forward, new Plane(camera.transform.forward, camera.transform.position)));
		// rectTransform.localRotation = Quaternion.AngleAxis (angle, -Vector3.forward);
	}

	public void SetAngleFromRotationZ() {
		
		Vector3 projectedCentre;
		if( !TryProjectWorldPoint(targetPositionInternal, out projectedCentre) ) {
			return;
		}
		
		Vector3 projectedTip;
		if( !TryProjectWorldPoint(targetPositionInternal+worldPointingVectorForZRotation, out projectedTip) ) {
			onScreen = false;
			return;
		}

		var screenSpaceDir = projectedTip - projectedCentre;

		var angle = Vector2.SignedAngle(screenSpaceDir, Vector2.up);

		rectTransform.localRotation = Quaternion.Euler(0, 0, -angle);
	}

	void CheckOcclusion() {
		RaycastHit hit;
		Vector3 offset = targetPositionInternal-worldCamera.transform.position;
		float maxDistance = offset.magnitude;

		occluded = false;

		if(maxDistance > 0.0f) {
			var ray = offset / maxDistance;
			if(occlusionMask != 0)
				occluded = Physics.Raycast(worldCamera.transform.position, ray, out hit, maxDistance, occlusionMask);
		}
	}

	// Test clamp with this
	void _OnDrawGizmos () {
//		if(!Application.isPlaying) return;
		Vector3? targetPosition = WorldPointToLocalPointInRectangle(rootCanvas, worldCamera, targetPositionInternal);
		if(targetPosition == null) return;
		
		Rect smallRect = GetScreenRect(rectTransform, rootCanvas);
		Rect largeRect = GetScreenRect(rootCanvasRT, rootCanvas);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(smallRect.center, smallRect.size);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(largeRect.center, largeRect.size);
		Gizmos.color = Color.red;
		
		Rect clampedRect = ClampInsideKeepSize(smallRect, largeRect);
		Gizmos.DrawWireCube(clampedRect.center, clampedRect.size);	
	}

	
	static Vector3[] corners = new Vector3[4];
	static void GetScreenCorners(RectTransform rectTransform, Canvas canvas, Vector3[] fourCornersArray) {
		rectTransform.GetWorldCorners(corners);

		for (int i = 0; i < 4; i++) {
			// For Canvas mode Screen Space - Overlay there is no Camera; best solution I've found
			// is to use RectTransformUtility.WorldToScreenPoint with a null camera.
			Camera cam = null;
			if(canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
				cam = canvas.worldCamera;
			Vector3 screenCoord = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);

            fourCornersArray[i] = screenCoord;
		}
	}

	static Rect GetScreenRect(RectTransform rectTransform, Canvas canvas) {
		GetScreenCorners(rectTransform, canvas, corners);
		float xMin = float.PositiveInfinity;
		float xMax = float.NegativeInfinity;
		float yMin = float.PositiveInfinity;
		float yMax = float.NegativeInfinity;
		for (int i = 0; i < 4; i++) {
            var screenCoord = corners[i];
			if (screenCoord.x < xMin)
				xMin = screenCoord.x;
			if (screenCoord.x > xMax)
				xMax = screenCoord.x;
			if (screenCoord.y < yMin)
				yMin = screenCoord.y;
			if (screenCoord.y > yMax)
				yMax = screenCoord.y;
		}
		return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
	}

	public static Rect ClampInsideKeepSize(Rect r, Rect container) {
		Rect rect = Rect.zero;
        rect.xMin = Mathf.Max(r.xMin, container.xMin);
        rect.xMax = Mathf.Min(r.xMax, container.xMax);
        rect.yMin = Mathf.Max(r.yMin, container.yMin);
        rect.yMax = Mathf.Min(r.yMax, container.yMax);
        
        if(r.xMin < container.xMin) rect.width += container.xMin - r.xMin;
        if(r.yMin < container.yMin) rect.height += container.yMin - r.yMin;
        if(r.xMax > container.xMax) {
            rect.x -= r.xMax - container.xMax;
            rect.width += r.xMax - container.xMax;
        }
        if(r.yMax > container.yMax) {
            rect.y -= r.yMax - container.yMax;
            rect.height += r.yMax - container.yMax;
        }

        // Finally make sure we're fully contained
        rect.xMin = Mathf.Max(rect.xMin, container.xMin);
        rect.xMax = Mathf.Min(rect.xMax, container.xMax);
        rect.yMin = Mathf.Max(rect.yMin, container.yMin);
        rect.yMax = Mathf.Min(rect.yMax, container.yMax);
        return rect;
	}

	static Vector3? WorldPointToLocalPointInRectangle (Canvas canvas, Camera camera, RectTransform rectTransform, Vector3 worldPosition) {
		Vector3 screenPoint = camera.WorldToScreenPoint(worldPosition);
		if (screenPoint.z < 0) return null;
		return ScreenPointToLocalPointInRectangle(canvas, rectTransform, screenPoint);
	}
	static Vector3? WorldPointToLocalPointInRectangle (Canvas canvas, Camera camera, Vector3 worldPosition) {
		return WorldPointToLocalPointInRectangle(canvas, camera, canvas.GetComponent<RectTransform>(), worldPosition);
	}

	static Vector3? ScreenPointToLocalPointInRectangle (Canvas canvas, RectTransform rectTransform, Vector2 screenPoint) {
		Camera camera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
		Vector2 localPosition;
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out localPosition))
			return localPosition;
		else return null;
	}
	
	static Vector3? ScreenPointToCanvasSpace(Canvas canvas, Vector2 screenPoint) {
		Camera camera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
		Vector3 canvasSpace = Vector3.zero;
        var rectTransform = canvas.GetComponent<RectTransform>();
		if(RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, camera, out canvasSpace))
			return canvasSpace;
		else return null;
	}
}