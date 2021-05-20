using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class WorldSpaceUIElement : UIBehaviour {
	[SerializeField]
	private bool _updateInEditMode = true;

	[SerializeField]
	private Camera _camera;
	public new Camera camera {
		get {
			if(_camera == null) {
				_camera = Camera.main;
				DebugX.Log(this, "No camera specified. Setting to current value of Camera.main: "+(_camera == null ? "Null" : _camera.transform.HierarchyPath()));
			}
			return _camera;
		} set {
			if(_camera == value)
				return;
			_camera = value;
			Refresh();
		}
	}

	[SerializeField]
	private Transform _target;
	public Transform target {
		get {
			return _target;
		} set {
			if(_target == value)
				return;
			_target = value;
			Refresh();
		}
	}

	public Vector3 worldPosition = Vector3.zero;
	public Quaternion worldRotation = Quaternion.identity;

	private Vector3 targetPositionInternal {
		get {
			return target == null ? worldPosition : target.position;
		}
	}

	private Quaternion targetRotationInternal {
		get {
			return target == null ? worldRotation : target.rotation;
		}
	}
	
	public bool updatePosition = true;
	public enum RotationMode {
		None,
		Rotation,
		RotationZ
	}
	public RotationMode updateRotation;
	public Vector3 worldPointingVectorForZRotation = new Vector3(0, 0, 1);
	public bool updateScale = false;
	public float scaleMultiplier = 1;
	public float minScale = 0.2f;
	public float maxScale = 1f;
	
	public bool clampToScreen;
	public bool onScreen;

	public bool updateOcclusion = false;
	public bool occluded;
	public int occlusionMask = Physics.DefaultRaycastLayers;
	
	RectTransform _rectTransform;
	public RectTransform rectTransform {
		get {
			if(_rectTransform == null) rectTransform = this.GetRectTransform();
			return _rectTransform;
		} private set {
			_rectTransform = value;
		}
	}

	Canvas _rootCanvas;
	public Canvas rootCanvas {
		get {
			if(_rootCanvas == null) SetRootCanvas();
			return _rootCanvas;
		} private set {
			_rootCanvas = value;
		}
	}

	RectTransform _rootCanvasRT;
	public RectTransform rootCanvasRT {
		get {
			if(_rootCanvasRT == null) SetRootCanvas();
			return _rootCanvasRT;
		} private set {
			_rootCanvasRT = value;
		}
	}

	RectTransform parentRT {
		get {
			if(rectTransform == null) {
				DebugX.LogWarning(this, transform.HierarchyPath()+" is not a rect transform!");
				return null;
			}
			RectTransform parentRT = rectTransform;
			if(transform.parent != null) {
				if(transform.parent is RectTransform) {
					parentRT = (RectTransform)transform.parent;
				} else {
					DebugX.LogWarning(this, "Parent of "+transform.HierarchyPath()+" is not a rect transform!");
					return null;
				}
			}
			return parentRT;
		}
	}
	
	void SetRootCanvas () {
		Canvas parentCanvas = this.GetParentCanvas();
		rootCanvas = parentCanvas.rootCanvas;
		rootCanvasRT = rootCanvas.GetComponent<RectTransform>();
	}
	
	public Vector3 GetLocalScale () {
		float scale = 1;
		if(rootCanvas.renderMode == RenderMode.WorldSpace && camera.orthographic) {
//			return parentCanvas.transform.InverseTransformVector(Vector3.one);
			scale = camera.orthographicSize;
		} else {
			var distanceFromCamera = 0f;
			if(camera.orthographic) {
				distanceFromCamera = Vector3X.DistanceInDirection(targetPositionInternal, camera.transform.position, camera.transform.forward);
			} else {
				distanceFromCamera = Vector3.Distance(targetPositionInternal, camera.transform.position);
			}
			float frustrumHeight = camera.GetFrustrumHeightAtDistance(distanceFromCamera);
			scale = (1f/frustrumHeight);
		}
		scale *= scaleMultiplier;
		float clampedScale = Mathf.Clamp(scale, minScale, maxScale);
		return Vector3.one * clampedScale;
	}
	
	protected override void Awake () {
		#if UNITY_EDITOR
		if(UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
        if(!Application.isPlaying && !_updateInEditMode) return;
		#endif
		rectTransform = this.GetRectTransform();
		if(camera == null)
			camera = Camera.main;
		SetRootCanvas();
	}

	protected override void OnEnable () {
		#if UNITY_EDITOR
		if(UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
        if(!Application.isPlaying && !_updateInEditMode) return;
		#endif
		Refresh();
	}
	
	protected override void OnTransformParentChanged () {
		#if UNITY_EDITOR
		if(UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
        if(!Application.isPlaying && !_updateInEditMode) return;
		#endif
		SetRootCanvas();
		base.OnTransformParentChanged ();
	}

	// LateUpdate because we want it to come even after camera updates
	private void LateUpdate () {
		#if UNITY_EDITOR
		if(UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) return;
        if(!Application.isPlaying && !_updateInEditMode) return;
		#endif
		Refresh();
	}

	public void Refresh () {
		if(camera == null || rootCanvas == null) 
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
        if(parentRT == null) {
			projectedCanvasPosition = Vector3.zero;
			return false;
		}
        
		Vector3? targetPositionNullable = rootCanvas.WorldPointToLocalPointInRectangle(camera, parentRT, worldPosition);
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
			Rect smallRect = rectTransform.GetScreenRect(rootCanvas);
			Rect largeRect = rootCanvasRT.GetScreenRect(rootCanvas);
			Rect clampedRect = RectX.ClampInsideKeepSize(smallRect, largeRect);
			Vector3? canvasSpace = rootCanvas.ScreenPointToCanvasSpace(clampedRect.position + smallRect.size * 0.5f);
			if(canvasSpace != null)
				rectTransform.position = (Vector3)canvasSpace;
		}
		
		onScreen = rootCanvasRT.rect.Contains((Vector2)targetPosition);
	}

	public void SetAngleFromRotation () {
		if(!targetRotationInternal.IsValid()) return;
		var rot = targetRotationInternal.Difference(camera.transform.rotation);
		rectTransform.rotation = rot;
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

		var angle = Vector2X.Degrees(screenSpaceDir.XY());

		rectTransform.localRotation = Quaternion.Euler(0, 0, -angle);
	}

	void CheckOcclusion() {
		RaycastHit hit;
		Vector3 offset = targetPositionInternal-camera.transform.position;
		float maxDistance = offset.magnitude;

		occluded = false;

		if(maxDistance > 0.0f) {
			var ray = offset / maxDistance;
			if(occlusionMask != 0)
				occluded = Physics.Raycast(camera.transform.position, ray, out hit, maxDistance, occlusionMask);
		}
	}

	// Test clamp with this
	private void _OnDrawGizmos () {
//		if(!Application.isPlaying) return;
		Vector3? targetPosition = rootCanvas.WorldPointToLocalPointInRectangle(camera, targetPositionInternal);
		if(targetPosition == null) return;
		
		Rect smallRect = rectTransform.GetScreenRect(rootCanvas);
		Rect largeRect = rootCanvasRT.GetScreenRect(rootCanvas);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(smallRect.center, smallRect.size);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(largeRect.center, largeRect.size);
		Gizmos.color = Color.red;
		
		Rect clampedRect = RectX.ClampInsideKeepSize(smallRect, largeRect);
		Gizmos.DrawWireCube(clampedRect.center, clampedRect.size);	
	}
}
