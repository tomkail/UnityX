using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Handles moving, scaling and rotating UI objects using multitouch.
// Should be just about 1-1, as you'd expect on a touch screen, although because it applies deltas there's a bit of "slippage" if you manipulate the same object for a while/rapidly.
// To test multitouch with a mouse editor you can comment out the marked line in OnEndDrag.
public class MultitouchDraggable : Selectable, IBeginDragHandler, IEndDragHandler, IDragHandler {
	public RectTransform rectTransform => (RectTransform)transform;
	[SerializeField]
	RectTransform _target;
	public RectTransform target {
		get {
			return _target == null ? rectTransform : _target;
		} set {
			_target = value;
		}
	}

	[Space]
	public bool canRotate = true;
	[Space]
	public float minScale = 0.5f;
	public float maxScale = 2f;
	public float targetScaleElasticity = 1f;

	// This is set true before a drag begins so that isHolding is true as soon as the hold starts. When any drag ends, or a mouse up event occurs it's unset.
	public bool isPointerDown;
	public bool isHolding {
		get {
			return dragInputs.Any() || isPointerDown;
		}
	}
	List<DragInput> dragInputs = new List<DragInput>();
	[System.Serializable]
	public class DragInput {
		public int pointerId;
		public Vector2 screenPos;
		public Vector2 lastScreenPos;
		public Vector2 deltaScreenPos => screenPos - lastScreenPos;

		public DragInput (PointerEventData eventData) {
			pointerId = eventData.pointerId;
			screenPos = lastScreenPos = eventData.position;
		}

		public void UpdateDrag (PointerEventData eventData) {
			lastScreenPos = screenPos;
			screenPos = eventData.position;
		}

		public void ClearDeltas () {
			lastScreenPos = screenPos;
		}
	}
	
	protected override void OnDisable () {
		base.OnDisable();
		dragInputs.Clear();
	}


	void LateUpdate () {
		if(dragInputs.Count > 0) {
			// This really wants to use whatever camera the PointerEventData used, but it's this in pretty much all cases I ever deal with.
			var camera = GetComponentInParent<Canvas>().rootCanvas.worldCamera;
			
			if(dragInputs.Count == 1) {
				RectTransformUtility.ScreenPointToWorldPointInRectangle(target, dragInputs[0].screenPos, camera, out Vector3 newWorldPos);
				RectTransformUtility.ScreenPointToWorldPointInRectangle(target, dragInputs[0].lastScreenPos, camera, out Vector3 lastWorldPos);
				var translation = newWorldPos - lastWorldPos;
				target.position += translation;
			} else if(dragInputs.Count == 2) {
				DoPinch(dragInputs[0], dragInputs[1]);
				DoPinch(dragInputs[1], dragInputs[0]);
				// Perform the pinch gesture using one finger as a static pivot and the other finger's delta movement
				void DoPinch (DragInput pivotFinger, DragInput movingFinger) {
					RectTransformUtility.ScreenPointToWorldPointInRectangle(target, pivotFinger.lastScreenPos, camera, out Vector3 worldPivotPos);
					ScreenPointToNormalizedPointInRectangle(target, pivotFinger.lastScreenPos, camera, out Vector2 normalizedPivotFingerScreenPos);

					// Rotate
					if(canRotate) {
						var deltaAngle = Vector2.SignedAngle(Vector2.up, movingFinger.screenPos-pivotFinger.lastScreenPos) - Vector2.SignedAngle(Vector2.up, movingFinger.lastScreenPos-pivotFinger.lastScreenPos);
						target.RotateAround(worldPivotPos, new Vector3(0,0,1), deltaAngle);
					}
					
					// Scale + Movement
					ScreenPointToNormalizedPointInRectangle(target, movingFinger.lastScreenPos, camera, out Vector2 normalizedLastFingerPoint);
					ScreenPointToNormalizedPointInRectangle(target, movingFinger.screenPos, camera, out Vector2 normalizedFingerPoint);
					var lastDistanceFromPivot = Vector2.Distance(normalizedLastFingerPoint, normalizedPivotFingerScreenPos);
					var delta = SignedDistanceInDirection(normalizedFingerPoint, normalizedLastFingerPoint, normalizedPivotFingerScreenPos-normalizedFingerPoint);
					float SignedDistanceInDirection (Vector2 fromVector, Vector2 toVector, Vector2 direction) {
						Vector2 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
						return Vector2.Dot(toVector-fromVector, normalizedDirection);
					}
					if(delta != 0 && lastDistanceFromPivot != 0) {
						var targetScale = target.localScale.x * (1+(delta/lastDistanceFromPivot));
						targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
						ScaleAround(target, worldPivotPos, Vector3.one * targetScale);
					}
				}
			}
			foreach(var dragInput in dragInputs) {
				dragInput.ClearDeltas();
			}
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (eventData.button != PointerEventData.InputButton.Left)
			return;
		
		isPointerDown = true;
	}
	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (eventData.button != PointerEventData.InputButton.Left)
			return;
		
		isPointerDown = false;
	}
	
	public void OnBeginDrag(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Left || !IsActive()) return;
		
		var dragInput = dragInputs.FirstOrDefault(x => x.pointerId == eventData.pointerId);
		if(dragInput != null) {
			#if UNITY_EDITOR
			dragInputs.RemoveRange(1,dragInputs.Count-1);
			// This code path is used for testing in editor, where the second click is treated as a new input and the first is turned into a static input point.
			dragInput.pointerId = 0;
			var pointerStartLocalCursor = Vector2.zero;
			dragInputs.Add(new DragInput(eventData));
			#endif
			Debug.LogWarning("Drag started but input tracker was found!");
		} else {
			var pointerStartLocalCursor = Vector2.zero;
			dragInputs.Add(new DragInput(eventData));
		}
	}

	public void OnDrag(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Left || !IsActive()) return;

		var dragInput = dragInputs.FirstOrDefault(x => x.pointerId == eventData.pointerId);
		if(dragInput == null) {
			Debug.LogWarning("Drag occurred but input tracker was not found!");
		} else {
			dragInput.UpdateDrag(eventData);
		}
	}

	public void OnEndDrag(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Left || !IsActive()) return;
		
		isPointerDown = false;
		var dragInput = dragInputs.FirstOrDefault(x => x.pointerId == eventData.pointerId);
		if(dragInput != null) {
			// Remove this to vaguely test multitouch in editor, with this ended drag used as the first of two fingers.
			dragInputs.Remove(dragInput);
		} else {
			Debug.LogWarning("Drag ended but no input tracker found!");
		}
	}






	// UTILS

	/// <summary>
	/// Scales the target around an arbitrary point by scaleFactor.
	/// This is relative scaling, meaning using  scale Factor of Vector3.one
	/// will not change anything and new Vector3(0.5f,0.5f,0.5f) will reduce
	/// the object size by half.
	/// The pivot is in world space.
	/// Scaling is applied to localScale of target.
	/// </summary>
	/// <param name="target">The object to scale.</param>
	/// <param name="pivot">The point to scale around in space of target.</param>
	/// <param name="scaleFactor">The factor with which the current localScale of the target will be multiplied with.</param>
	public static void ScaleAroundRelative(Transform target, Vector3 pivot, Vector3 scaleFactor)
	{
		// pivot
		var pivotDelta = target.position - pivot;
		pivotDelta.Scale(scaleFactor);
		target.position = pivot + pivotDelta;
	
		// scale
		var finalScale = target.localScale;
		finalScale.Scale(scaleFactor);
		target.localScale = finalScale;
	}
	
	/// <summary>
	/// Scales the target around an arbitrary pivot.
	/// This is absolute scaling, meaning using for example a scale factor of
	/// Vector3.one will set the localScale of target to x=1, y=1 and z=1.
	/// The pivot is in world space.
	/// Scaling is applied to localScale of target.
	/// </summary>
	/// <param name="target">The object to scale.</param>
	/// <param name="pivot">The point to scale around in the space of target.</param>
	/// <param name="scaleFactor">The new localScale the target object will have after scaling.</param>
	public static void ScaleAround(Transform target, Vector3 pivot, Vector3 newScale)
	{
		// pivot
		Vector3 pivotDelta = target.position - pivot; // diff from object pivot to desired pivot/origin
		Vector3 scaleFactor = new Vector3(
			newScale.x / target.localScale.x,
			newScale.y / target.localScale.y,
			newScale.z / target.localScale.z );
		pivotDelta.Scale(scaleFactor);
		target.position = pivot + pivotDelta;
	
		//scale
		target.localScale = newScale;
	}
	
	public static bool ScreenPointToNormalizedPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 normalizedPosition) {
		normalizedPosition = default;
		if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out var localPosition)) return false;
		var r = rect.rect;
		normalizedPosition = new Vector2((localPosition.x - r.x) / r.width, (localPosition.y - r.y) / r.height);
		normalizedPosition += rect.pivot-(Vector2.one * 0.5f);
		return true;
	}
}
