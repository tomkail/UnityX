using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using Spring = UnitySpring.ClosedForm.Spring;
using EasyButtons;
using UnityEditor;

[RequireComponent(typeof(RectTransform))]
public class SwipeView : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {
	RectTransform rectTransform => (RectTransform)transform;
	
	bool routeToParent = false;
	
	// Settings
	// The amount that the velocity of the drag is smoothed
	const float smoothedDragVelocitySmoothTime = 0.1f;
	const float snapSpringDamping = 29f;
	const float snapSpringMass = 1f;
	const float snapSpringStiffness = 250f;
	
	public bool interactable = true;
	
	// Is typically the object on which this script is attached. Pivots/anchors on this element are ignored.
	[SerializeField] RectTransform _viewport;
	public RectTransform viewport => _viewport != null ? _viewport : (RectTransform)transform;
	// The bit that moves! Pivots/anchors on this element are ignored.
	public RectTransform content;
	
	
	[Space]
	// Determines where we frame the targets, and where we measure distance on the pages to the framing point from.
	// With a pivot of 0, the left/bottom edge of the current page will snap to the left/bottom edge of the viewport.
	// There's an argument for using the pivots of the viewport/pages instead, but pivots are also used for scaling those objects, can be confusing to work with, and the typical use case involves setting them both at the same time to work in the way this system is set up.
	public Vector2 pivot = new Vector2(0.5f, 0.5f);
	public ValidDragAxis axis = ValidDragAxis.Horizontal;
	public enum ValidDragAxis {
		Horizontal,
		Vertical,
		Auto,
	}
	

	[Space]
	public SwipeMode swipeMode = SwipeMode.AdjacentThreshold;
	public enum SwipeMode {
		// Finds the page closest to where the swipe's velocity would take the container.
		// Feels natural, but may not work well for large distances where a large swipe is required.
		Velocity,
		// Tries to pick the page adjacent to targetPage in the direction of the swipe, but only if the swipe magnitude exceeds adjacentThresholdInInches.
		// Works best for when swipe items further than the adjacent pages are not visible.
		AdjacentThreshold,
		// A custom resolver is used to determine the target page.
		Custom
	}
	public float adjacentThresholdInInches = 0.1f;
	public delegate RectTransform CustomSwipeResolver(Vector2 velocity);
	public CustomSwipeResolver customSwipeResolver;


	public enum ClampType {
		// The user isn't restricted from dragging the container beyond the edges of the pages (although note all modes snaps to the closest item once released)
		// This mode is mostly only useful when combined with the CanDragInDirection delegate
		Unclamped,
		// Pulls back when the user moves beyond the edges of the pages
		Elastic,
		// Doesn't allow the user to push beyond the edges of the pages
		Clamped
	}
	public ClampType clampType = ClampType.Elastic;
	// Allows overriding the ability to drag. Useful for preventing dragging in certain directions, or for preventing dragging on certain pages.
	public delegate bool CanDragInDirectionDelegate(Vector2 pointerDeltaInTargetDirection);
	public CanDragInDirectionDelegate CanDragInDirection;
	
	
	[Space]
	// The bits that we move the container in order to frame within the viewport. Pivots/anchors on these elements are ignored.
	public List<RectTransform> pages = new List<RectTransform>();
	// Instead of a global pivot, you can set a pivot for each page individually. See Pivot field for more info.
	public List<Vector2> pagePivots = new List<Vector2>();
	
	// The page we're aiming for. The spring moves the container to frame this page.
	[SerializeField, Disable]
	RectTransform _targetPage;
	public RectTransform targetPage {
		get => _targetPage;
		set {
			if (_targetPage == value) return;
			var previousPage = _targetPage;
			if (isActiveAndEnabled) {
				if(_targetPage != null) {
					_targetPage.BroadcastMessage("OnUnsetAsTargetPage", SendMessageOptions.DontRequireReceiver);
				}
			}
			_targetPage = value;
			if (isActiveAndEnabled) {
				SetUpSpring();
				if (_targetPage != null) {
					_targetPage.BroadcastMessage("OnSetAsTargetPage", SendMessageOptions.DontRequireReceiver);
				}

				if (OnChangeTargetPage != null) OnChangeTargetPage(previousPage, _targetPage);
			}
		}
	}
	public int targetPageIndex => pages.IndexOf(targetPage);
	public delegate void OnChangeTargetPageDelegate(RectTransform previousPage, RectTransform newPage);
	public OnChangeTargetPageDelegate OnChangeTargetPage;
	
	public delegate void OnCompleteInteractionDelegate(RectTransform previousPage, RectTransform newPage);
	public OnCompleteInteractionDelegate OnCompleteInteraction;
	
	// The page that's currently closest to the focal point.
	// This is not necessarily the same as the target page, as the target page is the page that the spring is moving towards.
	public RectTransform closestPage => pages.Best(page => Mathf.Abs(GetNormalizedDistance(page)), (other, currentBest) => other < currentBest, Mathf.Infinity, null);
	public int closestPageIndex => pages.IndexOf(closestPage);

	public DragAxis currentDragAxis { get; private set; }
	public enum DragAxis {
		Horizontal,
		Vertical,
		Unknown
	}
	
	public bool dragging { get; private set; }
	// The page when we started dragging
	public RectTransform targetPageWhenDragStarted { get; private set; }
	
	Vector2 m_PointerStartLocalCursor;
	Vector2 smoothedDragVelocityVelocity;
	Vector2 smoothedDragVelocity;
	
	// Spring that snaps us back to the current page
	Spring snapSpringDamperX = new Spring();
	Spring snapSpringDamperY = new Spring();
	
	protected override void OnEnable () {
		base.OnEnable();
		snapSpringDamperX.stiffness = snapSpringDamperY.stiffness = snapSpringStiffness;
		snapSpringDamperX.mass = snapSpringDamperY.mass = snapSpringMass;
		snapSpringDamperX.damping = snapSpringDamperY.damping = snapSpringDamping;
		SetUpSpring();
	}

	void Update () {
		if(!Application.isPlaying) return;
		// Set this so that we have an axis to use for the various functions that rely on it before actually dragging.
		if(axis == ValidDragAxis.Horizontal) currentDragAxis = DragAxis.Horizontal;
		else if(axis == ValidDragAxis.Vertical) currentDragAxis = DragAxis.Vertical;
		
		if(dragging) smoothedDragVelocity = Vector2.SmoothDamp(smoothedDragVelocity, Vector2.zero, ref smoothedDragVelocityVelocity, smoothedDragVelocitySmoothTime);
		
		if(pages.IsNullOrEmpty()) {
			if(targetPage != null) targetPage = null;
		} else {
			if(!dragging) {
				var newAnchoredPosition = content.anchoredPosition;
				if(axis == ValidDragAxis.Horizontal || axis == ValidDragAxis.Auto) newAnchoredPosition.x = snapSpringDamperX.Evaluate(Time.deltaTime);
				if(axis == ValidDragAxis.Vertical || axis == ValidDragAxis.Auto) newAnchoredPosition.y = snapSpringDamperY.Evaluate(Time.deltaTime);
				content.anchoredPosition = newAnchoredPosition;
			}
		}
	}
	
	// Gets the distance from the target page to the focal point along the current axis, according to pivot settings
	public float GetNormalizedDistance (RectTransform page) {
		if(page == null) {
			Debug.Log("Page is null!");
			return -1;
		}
		var vectorToPage = GetPageVectorToViewportPivot(page);
		var normalizedDistanceFromPivot = vectorToPage[(int)currentDragAxis]/viewport.rect.size[(int)currentDragAxis];
		// Debug.Log(string.Format("{0} is {1} from being current page", page, normalizedDistanceFromPivot), this);
		return normalizedDistanceFromPivot;
	}
	
	public float GetNormalizedDistance (int pageIndex) {
		if(!pages.ContainsIndex(pageIndex)) {
			Debug.Log($"Index {pageIndex} does not point to a page! There are {pages.Count} pages", this);
			return 0;
		}
		var page = pages[pageIndex];
		return GetNormalizedDistance(page);
	}

	public Vector2 GetPivotForPage(RectTransform page) {
		var index = pages.IndexOf(page);
		return pagePivots != null && pagePivots.ContainsIndex(index) ? pagePivots[index] : pivot;
	}
	
	// Gets the vector from a page to the focal point, according to pivot settings.  
	public Vector2 GetPageVectorToViewportPivot (RectTransform page) {
		if (page == null) {
			Debug.Log("Page is null!");
			return Vector2.zero;
		}

		var pagePivot = GetPivotForPage(page);
		var pos = page.TransformPoint(new Vector3(Mathf.Lerp(page.rect.xMin, page.rect.xMax, pagePivot.x), Mathf.Lerp(page.rect.yMin, page.rect.yMax, pagePivot.y), 0));
		var localPos = viewport.InverseTransformPoint(pos);
		localPos += (Vector3)((viewport.pivot-pagePivot) * viewport.rect.size);
		return localPos;
	}
	
	// Given a page, gets the anchored position for the container to frame it according to pivot settings.
	public Vector2 GetContainerAnchoredPositionForPage(RectTransform page) {
		if (page == null) {
			Debug.Log("Page is null!");
			return Vector2.zero;
		}
		var pagePivot = GetPivotForPage(page);
		var pos = page.TransformPoint(new Vector3(Mathf.Lerp(page.rect.xMin, page.rect.xMax, pagePivot.x), Mathf.Lerp(page.rect.yMin, page.rect.yMax, pagePivot.y), 0));
		var matrix = Matrix4x4.TRS(content.position, content.rotation, content.parent.lossyScale);
		var localPos = matrix.inverse.MultiplyPoint3x4(pos);
		localPos -= (Vector3)content.GetLocalToAnchoredPositionOffset();
		localPos += (Vector3)((rectTransform.pivot-pagePivot) * viewport.rect.size);
		return -localPos;
	}
	
	public void GoToPageImmediate(RectTransform page) {
		GoToPageImmediate(pages.IndexOf(page));
	}
	
	[Button]
	public void GoToPageImmediate (int pageIndex) {
		if(!pages.ContainsIndex(pageIndex)) {
			Debug.LogWarning(string.Format("Index {0} does not point to a page! There are {1} pages", pageIndex, pages.Count), this);
			return;
		}
		targetPage = pages[pageIndex];
		content.anchoredPosition = GetContainerAnchoredPositionForPage(targetPage);
		SetUpSpring();
	}

	public void GoToPageSmooth(RectTransform page) {
		GoToPageSmooth(pages.IndexOf(page));
	}

	[Button]
	public void GoToPageSmooth (int pageIndex) {
		if(!pages.ContainsIndex(pageIndex)) {
			Debug.Log(string.Format("Index {0} does not point to a page! There are {1} pages", pageIndex, pages.Count), this);
			return;
		}
		targetPage = pages[pageIndex];
	}
	
	

	public void OnPointerDown(PointerEventData eventData) {}

	public void OnPointerUp(PointerEventData eventData) {}
	
	public void OnInitializePotentialDrag(PointerEventData eventData) {
		if (!interactable || eventData.button != PointerEventData.InputButton.Left || !IsActive()) {
			ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
		}
		// Judging by how ScrollRect works, we should stop the springs when this occurs.
	}
	
	public void OnBeginDrag(PointerEventData eventData) {
		if (!interactable || eventData.button != PointerEventData.InputButton.Left || !IsActive()) routeToParent = true;
		else {
			if (axis != ValidDragAxis.Horizontal && Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y)) routeToParent = true;
			else if (axis != ValidDragAxis.Vertical && Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y)) routeToParent = true;
			else routeToParent = false;
		}

		if (routeToParent) {
			ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
			return;
		}
		
		m_PointerStartLocalCursor = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
		
		targetPageWhenDragStarted = targetPage;
		dragging = true;
		
		if(axis == ValidDragAxis.Horizontal) currentDragAxis = DragAxis.Horizontal;
		else if(axis == ValidDragAxis.Vertical) currentDragAxis = DragAxis.Vertical;
		else currentDragAxis = DragAxis.Unknown;
	}

	public void OnDrag(PointerEventData eventData) {
		if (routeToParent) {
			ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
			return;
		}
		if (!interactable || eventData.button != PointerEventData.InputButton.Left || !IsActive()) return;

		Vector2 localCursor;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position, eventData.pressEventCamera, out localCursor))
			return;

		var localPointerDelta = localCursor - m_PointerStartLocalCursor;
		m_PointerStartLocalCursor = localCursor;

		if(currentDragAxis == DragAxis.Unknown && localPointerDelta.magnitude > 0) {
			if(Mathf.Abs(localPointerDelta.x) > Mathf.Abs(localPointerDelta.y) && (axis == ValidDragAxis.Horizontal || axis == ValidDragAxis.Auto)) {
				currentDragAxis = DragAxis.Horizontal;
			} else if(axis == ValidDragAxis.Vertical || axis == ValidDragAxis.Auto) {
				currentDragAxis = DragAxis.Vertical;
			}
		}
		if (currentDragAxis != DragAxis.Unknown) {
			var distanceFromMinEdge = content.anchoredPosition[(int)currentDragAxis];
			var distanceFromMaxEdge = -(content.rect.size[(int)currentDragAxis] + content.anchoredPosition[(int)currentDragAxis]-viewport.rect.size[(int)currentDragAxis]);

			var distanceFromCenter = viewport.InverseTransformPoint(content.transform.TransformPoint(content.rect.center))[(int)currentDragAxis];
			distanceFromMinEdge = (distanceFromCenter + viewport.rect.size[(int)currentDragAxis] * 0.5f - content.rect.size[(int)currentDragAxis] * 0.5f);
			distanceFromMaxEdge = -(distanceFromCenter - viewport.rect.size[(int)currentDragAxis] * 0.5f + content.rect.size[(int)currentDragAxis] * 0.5f);

			var pointerDeltaInTargetDirection = new Vector2();
			if (clampType == ClampType.Elastic) {
				var movingAgainstEdgeMultiplier = 1f;
				if(Mathf.Sign(distanceFromMinEdge - distanceFromMaxEdge) == Mathf.Sign(localPointerDelta[(int)currentDragAxis])) {
					var distanceFromEdge = Mathf.Max(distanceFromMinEdge, distanceFromMaxEdge);
					movingAgainstEdgeMultiplier = Mathf.InverseLerp(500, 0, distanceFromEdge);
					
					// This was an attempt to use the existing spring to move the container back to the edge, but it felt less smooth.
					// snapSpringDamper.Reset();
					// snapSpringDamper.startValue = 0;
					// snapSpringDamper.endValue = Mathf.Max(0, distanceFromEdge);
					// snapSpringDamper.Evaluate(Time.deltaTime);
					// if(pointerDelta[(int) dragAxis] > 0) pointerDelta[(int) dragAxis] = Mathf.Max(pointerDelta[(int) dragAxis] - Mathf.Sign(pointerDelta[(int) dragAxis]) * Mathf.Abs(snapSpringDamper.currentValue), 0);
					// else pointerDelta[(int) dragAxis] = Mathf.Min(pointerDelta[(int) dragAxis] - Mathf.Sign(pointerDelta[(int) dragAxis]) * Mathf.Abs(snapSpringDamper.currentValue), 0);
					// movingAgainstEdgeMultiplier = 1;
				}
				pointerDeltaInTargetDirection[(int)currentDragAxis] = localPointerDelta[(int)currentDragAxis] * movingAgainstEdgeMultiplier;
			} else if (clampType == ClampType.Clamped) {
				pointerDeltaInTargetDirection[(int)currentDragAxis] = localPointerDelta[(int)currentDragAxis];
				if (Mathf.Sign(pointerDeltaInTargetDirection[(int)currentDragAxis]) < 0)
                    pointerDeltaInTargetDirection[(int)currentDragAxis] = -Mathf.Min(-pointerDeltaInTargetDirection[(int)currentDragAxis], -distanceFromMaxEdge);
				else if (Mathf.Sign(pointerDeltaInTargetDirection[(int)currentDragAxis]) > 0)
                    pointerDeltaInTargetDirection[(int)currentDragAxis] = Mathf.Min(pointerDeltaInTargetDirection[(int)currentDragAxis], -distanceFromMinEdge);
			} else {
				pointerDeltaInTargetDirection[(int)currentDragAxis] = localPointerDelta[(int)currentDragAxis];
			}
			
			if(CanDragInDirection == null || CanDragInDirection(pointerDeltaInTargetDirection))
				content.anchoredPosition += pointerDeltaInTargetDirection;
			else {
				localPointerDelta = Vector2.zero;
			}
		}
		smoothedDragVelocity += ScreenX.ScreenToInchesPoint(localPointerDelta);
	}

	public void OnEndDrag(PointerEventData eventData) {
		if (routeToParent) {
			ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
			return;
		}
		if (!interactable || eventData.button != PointerEventData.InputButton.Left || !IsActive()) return;

		dragging = false;

		if (pages.Count == 0) {
			targetPage = null;
			return;
		}

		if (currentDragAxis != DragAxis.Unknown) {
			if (swipeMode == SwipeMode.Velocity) targetPage = GetCurrentPageFromVelocity(smoothedDragVelocity);
			else if (swipeMode == SwipeMode.AdjacentThreshold) targetPage = GetCurrentPageFromDirectionAndThreshold(smoothedDragVelocity, adjacentThresholdInInches);
			else if (swipeMode == SwipeMode.Custom && customSwipeResolver != null) targetPage = customSwipeResolver(smoothedDragVelocity);
		}

		SetUpSpring();

		smoothedDragVelocity = smoothedDragVelocityVelocity = Vector2.zero;
		
		OnCompleteInteraction?.Invoke(targetPageWhenDragStarted, targetPage);
	} 
	
	// Finds the page closest to where the swipe's velocity would take the container.
	// Feels natural, but may not work well for large distances where a large swipe is required.
	public RectTransform GetCurrentPageFromVelocity(Vector2 smoothedDragVelocityInInches) {
		Vector3[] corners = new Vector3[4];
		viewport.GetScreenCorners(viewport.GetComponentInParent<Canvas>(), corners);
		var viewportScreenSize = corners[2]-corners[0];
		var viewportInchSize = ScreenX.ScreenToInchesPoint(viewportScreenSize);
		var viewportSpaceVelocity = viewport.rect.size[(int)currentDragAxis] * (smoothedDragVelocityInInches[(int)currentDragAxis]/viewportInchSize[(int)currentDragAxis]);
		return pages.Best(page => Mathf.Abs(-viewportSpaceVelocity - GetPageVectorToViewportPivot(page)[(int)currentDragAxis]), (other, currentBest) => other < currentBest, Mathf.Infinity, null);
	}
	
	// If the swipe has enough magnitude, pick the page adjacent to targetPage in the direction of the swipe.
	// If not, return to the target page or use the closest page if useClosestPageIfThresholdNotMet is true.
	public RectTransform GetCurrentPageFromDirectionAndThreshold(Vector2 smoothedDragVelocityInInches, float swipeVelocityThresholdInInches, bool useClosestPageIfThresholdNotMet = true) {
		var dragSpeedInches = Mathf.Abs(smoothedDragVelocityInInches[(int)currentDragAxis]);
		if (dragSpeedInches > swipeVelocityThresholdInInches) {
			var orderedPages = pages.OrderBy(page => GetPageVectorToViewportPivot(page)[(int)currentDragAxis]).ToArray();
			var newPageIndex = orderedPages.IndexOf(targetPage) - (int)Mathf.Sign(smoothedDragVelocityInInches[(int)currentDragAxis]);
			if(orderedPages.ContainsIndex(newPageIndex)) return orderedPages[newPageIndex];
		}
		return useClosestPageIfThresholdNotMet ? closestPage : targetPage;
	}

	// Returns the interpolated index of the current position in the list of pages.
	public float GetInterpolatedCurrentPageIndex() {
		List<float> anchoredPositions = new List<float>(pages.Count);
		foreach (var page in pages) anchoredPositions.Add(GetContainerAnchoredPositionForPage(page)[(int)currentDragAxis]);
		return MathX.FindIndexPosition(anchoredPositions, content.anchoredPosition[(int)currentDragAxis]);
	}
	
	public float GetNormalizedProgress() {
		return GetInterpolatedCurrentPageIndex() / (pages.Count - 1);
	}
	
	
	
	void SetUpSpring() {
		if(targetPage == null) return;

		var targetAnchoredPos = GetContainerAnchoredPositionForPage(targetPage);
			
		snapSpringDamperX.Reset();
		snapSpringDamperX.startValue = content.anchoredPosition.x;
		snapSpringDamperX.endValue = targetAnchoredPos.x;
		
		snapSpringDamperY.Reset();
		snapSpringDamperY.startValue = content.anchoredPosition.y;
		snapSpringDamperY.endValue = targetAnchoredPos.y;
	}
	
	#if UNITY_EDITOR
	void OnDrawGizmos() {
		Gizmos.DrawSphere(viewport.rect.center, 10);
		foreach (var page in pages) {
			var pagePivot = GetPivotForPage(page);
			var pos = page.TransformPoint(new Vector3(Mathf.Lerp(page.rect.xMin, page.rect.xMax, pagePivot.x), Mathf.Lerp(page.rect.yMin, page.rect.yMax, pagePivot.y), 0));
			Gizmos.DrawSphere(pos, 10);
			Handles.Label(pos, $"{GetPageVectorToViewportPivot(page)}");
		}
	}
	#endif
}