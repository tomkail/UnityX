using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// On start dragging event delegate.
/// </summary>
public delegate void OnStartDraggingEvent();
/// <summary>
/// On stop dragging event delegate.
/// </summary>
public delegate void OnStopDraggingEvent();

public class Draggable : Selectable, IBeginDragHandler, IEndDragHandler, IDragHandler {
	public RectTransform rectTransform {
		get {
			return (RectTransform)transform;
		}
	}
	public PointerEventData lastPointerEventData;

	/// <summary>
	/// Occurs when a drag starts.
	/// </summary>
	public event OnStartDraggingEvent OnStartDragging;
	/// <summary>
	/// Occurs when a drag stops.
	/// </summary>
	public event OnStopDraggingEvent OnStopDragging;

	public Action<Draggable, PointerEventData> OnDragged;
	public Action<Draggable, PointerEventData> OnClicked;
	
	// Container this object resides inside
	public RectTransform viewRect {
		get {
			return (RectTransform)transform.parent;
		}
	}

	/// If the target position is controlled here or elsewhere
	// public bool setTargetPositionAutomatically = true;
	
	public bool held;
	public bool dragging;
	
	
	[Space]
	/// <summary>
	/// The drag velocity.
	/// </summary>
	public Vector2 dragTranslateAxis = Vector2.one;
	
	/// <summary>
	/// The speed that the object follows the mouse.
	/// </summary>
	public float dragVelocitySmoothTime = 0.1f;
	
	/// <summary>
	/// The maximum drag velocity.
	/// </summary>
	public float maxDragVelocity = Mathf.Infinity;
	
	
	/// <summary>
	/// The drag velocity.
	/// </summary>
	public Vector3 dragRotateAxis = Vector3.one;
	
	/// <summary>
	/// The magnitude of rotation from a drag, linked to the current drag velocity
	/// </summary>
	public float dragVelocityRotationMagnitude = 0.01f;
	/// <summary>
	/// The maximum rotation of the from a drag.
	/// </summary>
	public float maxDragVelocityRotation = 30f;
	
	
	
	
	/// <summary>
	/// The drag offset.
	/// </summary>
	private Vector2 dragOffset;
	
	/// <summary>
	/// The drag start position.
	/// </summary>
	public Vector2 m_ContentStartPosition;
	public Vector2 m_PointerStartLocalCursor;
	
	/// <summary>
	/// The drag target location. Includes dragOffset.
	/// </summary>
	public Vector2 dragTargetPosition;
	
	/// <summary>
	/// The drag velocity.
	/// </summary>
	[SerializeField]
	private Vector2 _dragVelocity;
	private Vector2 dragVelocity {
		get {
			return _dragVelocity;
		} set {
			if(_dragVelocity == value) return;
			_dragVelocity = value;
		}
	}
	
	public float distanceFromTarget => Vector2.Distance(rectTransform.anchoredPosition, dragTargetPosition);
	/// <summary>
	/// Updates the drag.
	/// </summary>
	public void UpdateDrag () {
		CalculateDragVelocity();
		ApplyTransformChangesFromDragVelocity();
	}
	
	/// <summary>
	/// Updates the drag.
	/// </summary>
	// public void UpdateDrag (Vector3 targetPosition) {
	// 	SetDragTargetPosition(targetPosition, true);
	// 	CalculateDragVelocity();
	// 	ApplyTransformChangesFromDragVelocity();
	// }
	
	private void ApplyTransformChangesFromDragVelocity () {
		TranslateWithDragVelocity (dragVelocity);
		RotateWithDragVelocity (dragVelocity, dragVelocityRotationMagnitude, maxDragVelocityRotation);
	}
	
	void StopDragging (bool revert = false) {
		dragging = false;
		dragVelocity = Vector2.zero;
		dragOffset = Vector2.zero;
		if(revert) {
			dragTargetPosition = m_PointerStartLocalCursor;
		}
		if(OnStopDragging != null) OnStopDragging();
	}
	
	/// <summary>
	/// Sets the drag target position.
	/// </summary>
	/// <param name="_newPosition">_new position.</param>
	/// <param name="_applyDragOffset">If set to <c>true</c> apply drag offset.</param>
	public void SetDragTargetPosition (Vector2 _newPosition, bool _applyDragOffset) {
		dragTargetPosition = _newPosition;
		if(_applyDragOffset) dragTargetPosition -= dragOffset;
	}
	
	/// <summary>
	/// Sets the position.
	/// </summary>
	/// <param name="_newPosition">_new position.</param>
	public void SetPositionImmediate (Vector2 _newPosition) {
		dragVelocity = Vector2.zero;
		rectTransform.anchoredPosition = dragTargetPosition = _newPosition;
	}
		
	/// <summary>
	/// Gets the position from the drag space mode.
	/// </summary>
	/// <returns>The position.</returns>
	/// <param name="space">Space.</param>
	protected virtual Vector3 GetPosition (Vector2 screenPosition) {
		var canvas = transform.GetComponentInParent<Canvas>();
		Vector3 point;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), screenPosition, canvas.worldCamera, out point);
		return point;
	}
	
	protected virtual void CalculateDragVelocity () {
		if(dragVelocitySmoothTime == 0) {
			SetPositionImmediate(dragTargetPosition);
		} else {
			var __dragVelocity = dragVelocity;
			Vector2.SmoothDamp(rectTransform.anchoredPosition, dragTargetPosition, ref __dragVelocity, dragVelocitySmoothTime, maxDragVelocity, Time.unscaledDeltaTime);
			dragVelocity = __dragVelocity;
		}
	}
	
	protected virtual void TranslateWithDragVelocity (Vector2 _dragVelocity) {
		_dragVelocity = Vector2.Scale (_dragVelocity, dragTranslateAxis);
		rectTransform.anchoredPosition += _dragVelocity * Time.deltaTime;
	}
	
	protected virtual void RotateWithDragVelocity (Vector3 _dragVelocity, float _dragVelocityRotationMagnitude, float _maxRotateStep) {
		_dragVelocity = -Vector3.Scale (_dragVelocity, (Vector3)dragRotateAxis);
		float step = _dragVelocity.magnitude * _dragVelocityRotationMagnitude;
		step = Mathf.Clamp (step, 0, _maxRotateStep);
		step *= Time.unscaledDeltaTime;
		Vector3 newDir = Vector3.RotateTowards(Vector3.forward, _dragVelocity.normalized, step, 0.0f);
		transform.rotation = Quaternion.LookRotation(newDir);
	}
	
	protected override void OnEnable () {
		base.OnEnable();
		SetPositionImmediate(rectTransform.anchoredPosition);
	}
	
	protected override void OnDisable () {
		base.OnDisable();
		StopDragging();
	}

	protected virtual void LateUpdate () {
		if(!Application.isPlaying) return;
		UpdateDrag();
	}

	public override void OnPointerDown(PointerEventData eventData) {
		held = true;
		base.OnPointerDown(eventData);
	}
	public override void OnPointerUp(PointerEventData eventData) {
		held = false;
		base.OnPointerUp(eventData);
	}
	
	public void OnBeginDrag(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Left || !IsActive()) return;
		lastPointerEventData = eventData;
		
		m_PointerStartLocalCursor = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
		m_ContentStartPosition = rectTransform.anchoredPosition;
		
		dragging = true;
		// m_PointerStartLocalCursor = rectTransform.anchoredPosition;
		
		// dragOffset = (_startPosition - m_PointerStartLocalCursor);
		// dragOffset = new Vector3(dragOffset.x, dragOffset.y, 0);
		if(OnStartDragging != null) OnStartDragging();
	}

	public void OnDrag(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Left || !IsActive()) return;
		lastPointerEventData = eventData;

		Vector2 localCursor;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
			return;

		var pointerDelta = localCursor - m_PointerStartLocalCursor;
		Vector2 position = m_ContentStartPosition + pointerDelta;

		if (position != dragTargetPosition) {
			dragTargetPosition = position;
		}

		if(OnDragged !=null) OnDragged(this, eventData);
	}

	public void OnEndDrag(PointerEventData eventData) {
		if (eventData.button != PointerEventData.InputButton.Left || !IsActive()) return;
		lastPointerEventData = eventData;
		StopDragging();
	}

	public void OnPointerClick(PointerEventData eventData) {
		if(dragging) return;
		// lastPointerEventData = eventData;
		// StopDragging();
		if(OnClicked != null) OnClicked(this, eventData);
		
		// Debug.Log(name + " Game Object Clicked!");
	}
}
