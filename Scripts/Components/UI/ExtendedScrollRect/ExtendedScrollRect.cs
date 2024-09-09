using System;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI {
	public class ExtendedScrollRect : ScrollRect {
		public bool routeUnusedAxisDragEventsToParent = true;
   		protected bool routedUnusedAxisDragToParent;
	    
	    // The edges that allow the drag to be rerouted to the parent when exceeding bounds.
	    [SerializeField] int excessDragRerouteEdges;
	    public int ExcessDragRerouteEdges {
		    get => excessDragRerouteEdges;
		    set => excessDragRerouteEdges = value;
	    }
	    // public bool CurrentlyRoutingExcessDragToParent => routingExcessDragToParent;
	    bool currentlyRoutingExcessDragToParent = false;
	    bool routedExcessDragToParent = false;
	    

	    bool ExcessDragIsReroutable(Vector2 excessDrag) {
		    if (horizontal) {
			    if(excessDrag.x < 0 && IsEdgeFlagSet(excessDragRerouteEdges, RectTransform.Edge.Left)) return true;
			    else if(excessDrag.x > 0 && IsEdgeFlagSet(excessDragRerouteEdges, RectTransform.Edge.Right)) return true;
		    }
		    if (vertical) {
			    if(excessDrag.y > 0 && IsEdgeFlagSet(excessDragRerouteEdges, RectTransform.Edge.Top)) return true;
			    else if(excessDrag.y < 0 && IsEdgeFlagSet(excessDragRerouteEdges, RectTransform.Edge.Bottom)) return true;
		    }
		    return false;
	    }
	    public static bool IsEdgeFlagSet (int excessDragRerouteEdges, RectTransform.Edge edge) {
		    return (excessDragRerouteEdges & EnumToFlagValue((int)edge+1)) != 0;
		    static int EnumToFlagValue (int enumValue) {
			    return enumValue == 0 ? 0 : 1 << (enumValue-1);
		    }
	    }


	    public bool routeScrollEventsToParent;
	    
        public bool dragging { get; private set; }

        // The content rect is scaled, so there's some minor inaccuracy when comparing sizes which this helps mitigate.
        const float Epsilon = 0.001f;
        // The viewport rect transform. Uses this component's rect transform if none is specified.
		public new RectTransform viewRect => base.viewRect;

		public Rect containerRect => RectX.MinMaxRect(viewBounds.min, viewBounds.max);


		// Content rect, also taking the scale of the content into account (as Unity's scrollrect does)
        public Rect scaledContentRect => RectX.MinMaxRect(contentBounds.min, contentBounds.max);

        public Bounds contentBounds {
			get {
				// Actually it's best we just do this all the time, else we have to remember to call ForceUpdateBounds() when we want to get the bounds.
				ForceUpdateBounds();
				// This only runs in update when enabled. Since we want to be able to call this even when disabled, we force the bounds to be calculated here.
				// if(!enabled) ForceUpdateBounds();
				return m_ContentBounds;
			}
		}
        
        // Note - we should stop using bounds and use contentRect and viewRect.rect instead
		public Bounds viewBounds => new(viewRect.rect.center, viewRect.rect.size);

		public Vector2 freeMovementSize => scaledContentRect.size-viewRect.rect.size;

		// This is the offset from the pivot point to the side of the content rect that matches the pivot
        public Vector2 contentOffset => viewRect.rect.position - scaledContentRect.position;

        // 0,0 when the bottom-left of the content matches that of the container; 1, when the top-right of the content matches that of the container.
        // When container is larger than content the axis tends to return 0.5, but is a bit unpredictable since _freeMovementSize is 0 or near it.
        public Vector2 normalizedContentOffset {
			get {
                var _contentOffset = contentOffset;
                var _freeMovementSize = freeMovementSize;
				return new Vector2(_contentOffset.x/_freeMovementSize.x, _contentOffset.y/_freeMovementSize.y);
			}
		}
        
        public Vector2 CalculateOffset() => InternalCalculateOffset(viewBounds, contentBounds);
        internal static Vector2 InternalCalculateOffset(Bounds viewBounds, Bounds contentBounds) {
	        Vector2 offset = Vector2.zero;
	        
	        Vector2 min = contentBounds.min;
	        Vector2 max = contentBounds.max;

	        {
		        // min.x += delta.x;
		        // max.x += delta.x;

		        float maxOffset = viewBounds.max.x - max.x;
		        float minOffset = viewBounds.min.x - min.x;

		        if (minOffset < -0.001f)
			        offset.x = minOffset;
		        else if (maxOffset > 0.001f)
			        offset.x = maxOffset;
	        }


	        {
		        // min.y += delta.y;
		        // max.y += delta.y;

		        float maxOffset = viewBounds.max.y - max.y;
		        float minOffset = viewBounds.min.y - min.y;

		        if (maxOffset > 0.001f)
			        offset.y = maxOffset;
		        else if (minOffset < -0.001f)
			        offset.y = minOffset;
	        }

	        return offset;
        }

        
		

        
        public Vector2 contentBottomLeftAnchoredPosition => content.GetAnchoredPositionForTargetAnchorAndPivot(viewRect, new Vector2(0, 0), new Vector2(0, 0));
        public Vector2 contentTopRightAnchoredPosition => content.GetAnchoredPositionForTargetAnchorAndPivot(viewRect, new Vector2(1, 1), new Vector2(1, 1));

		// Get and set the distance of the content from the left edge, in the anchored space of the content.
		// These values are signed as if they were signed distance fields - Moving the content outside of the viewport results in positive values, and when the content is inside the viewport the values are negative. 
		public float signedAnchoredDistanceFromLeftEdge {
			get => contentBottomLeftAnchoredPosition.x - content.anchoredPosition.x;
			set => content.anchoredPosition = new Vector2(contentBottomLeftAnchoredPosition.x - value, content.anchoredPosition.y);
		}

		public float signedAnchoredDistanceFromRightEdge {
			get => content.anchoredPosition.x - contentTopRightAnchoredPosition.x;
			set => content.anchoredPosition = new Vector2(contentTopRightAnchoredPosition.x + value, content.anchoredPosition.y);
		}
		public float signedAnchoredDistanceFromBottomEdge {
			get => contentBottomLeftAnchoredPosition.y - content.anchoredPosition.y;
			set => content.anchoredPosition = new Vector2(content.anchoredPosition.x, contentBottomLeftAnchoredPosition.y - value);
		}

		public float signedAnchoredDistanceFromTopEdge {
			get => content.anchoredPosition.y - contentTopRightAnchoredPosition.y;
			set => content.anchoredPosition = new Vector2(content.anchoredPosition.x, contentTopRightAnchoredPosition.y + value);
		}
		
		
		public bool canScrollLeft => contentSizeExceedsViewportX && signedAnchoredDistanceFromLeftEdge > Epsilon;

		public bool canScrollRight => contentSizeExceedsViewportX && signedAnchoredDistanceFromRightEdge > -Epsilon;

		public bool canScrollUp => contentSizeExceedsViewportY && signedAnchoredDistanceFromTopEdge > Epsilon;

		public bool canScrollDown => contentSizeExceedsViewportY && signedAnchoredDistanceFromBottomEdge > -Epsilon;

		public bool contentSizeExceedsViewportX => freeMovementSize.x > Epsilon;

		public bool contentSizeExceedsViewportY => freeMovementSize.y > Epsilon;
		
		
        public float GetClampedAnchoredPositionX (float contentAnchoredPositionX) {
	        var min = contentBottomLeftAnchoredPosition.x;
	        var max = contentTopRightAnchoredPosition.x;
	        if(min > max) (min, max) = (max, min);
	        return Mathf.Clamp(contentAnchoredPositionX, min, max);
        }
        public float GetClampedAnchoredPositionY (float contentAnchoredPositionY) {
	        var min = contentBottomLeftAnchoredPosition.y;
	        var max = contentTopRightAnchoredPosition.y;
	        if(min > max) (min, max) = (max, min);
	        return Mathf.Clamp(contentAnchoredPositionY, min, max);
        }
        public Vector2 GetClampedAnchoredPosition (Vector2 contentAnchoredPosition) {
            return new Vector2(GetClampedAnchoredPositionX(contentAnchoredPosition.x), GetClampedAnchoredPositionY(contentAnchoredPosition.y));
        }
        
        // Gets the anchored position for the content of the scroll rect so that a world position is framed at the pivot point.
        // To make the scroll rect show child object in its vertical center, we might call: 
        // scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, scrollRect.GetAnchoredPositionForWorldPoint(childRT.TransformPoint(childRT.rect.center), new Vector2(0.5f, 0.5f).y);
        public Vector2 GetAnchoredPositionForWorldPoint (Vector3 worldPoint, Vector2 pivot) {
	        var transformedPoint = content.InverseTransformPoint(worldPoint);
	        var targetPos = -(Vector2) transformedPoint;
	        targetPos += Rect.NormalizedToPoint((viewport == null ? (RectTransform) transform : viewport).rect, pivot);
	        targetPos += content.GetLocalToAnchoredPositionOffset();
	        return targetPos;
        }


        readonly Vector3[] m_Corners = new Vector3[4];
		public Bounds GetContentBounds() {
            if (content == null) return new Bounds();
            content.GetWorldCorners(m_Corners);
            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
            return InternalGetContentBounds(m_Corners, ref viewWorldToLocalMatrix);
        }

        internal static Bounds InternalGetContentBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix) {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }


		[Serializable]
		public class ScrollRectScrollEvent : UnityEvent<PointerEventData> {}
		
		[Serializable]
		public class ScrollRectBeginDragEvent : UnityEvent<PointerEventData> {}
		
		[Serializable]
		public class ScrollRectEndDragEvent : UnityEvent<PointerEventData> {}
		
		[Serializable]
		public class ScrollRectDragEvent : UnityEvent<PointerEventData> {}
		
		
		public ScrollRectScrollEvent onScroll = new();
		public ScrollRectBeginDragEvent onBeginDrag = new();
		public ScrollRectEndDragEvent onEndDrag = new();
		public ScrollRectDragEvent onDrag = new();

		public void ForceUpdateBounds() {
			UpdateBounds();
		}

		/// <summary>
		/// Always route initialize potential drag event to parents
		/// </summary>
		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
			base.OnInitializePotentialDrag(eventData);
		}

		public override void OnScroll (PointerEventData eventData) {
			if (routeScrollEventsToParent)
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
			else {
				base.OnScroll(eventData);
				if (IsActive()) onScroll.Invoke(eventData);
			}
		}
		
		public override void OnBeginDrag (PointerEventData eventData) {
			if (routeUnusedAxisDragEventsToParent && !horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
				routedUnusedAxisDragToParent = true;
			else if (routeUnusedAxisDragEventsToParent && !vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
				routedUnusedAxisDragToParent = true;
			else
				routedUnusedAxisDragToParent = false;

			if (routedUnusedAxisDragToParent)
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
			else {
				base.OnBeginDrag (eventData);
				
				if (eventData.button != PointerEventData.InputButton.Left)
					return;
				
				if (!IsActive())
					return;
				
				onBeginDrag.Invoke(eventData);
				dragging = true;
			}
		}
		
		public override void OnEndDrag (PointerEventData eventData) {
			if (routedUnusedAxisDragToParent || routedExcessDragToParent)
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
			else {
				base.OnEndDrag (eventData);
				if (eventData.button == PointerEventData.InputButton.Left) onEndDrag.Invoke(eventData);
			}
			routedUnusedAxisDragToParent = false;
			routedExcessDragToParent = false;
			currentlyRoutingExcessDragToParent = false;
			dragging = false;
		}

		
		public override void OnDrag (PointerEventData eventData) {
			var excessDrag = GetAmountOfExcessMovement(eventData);
			var excessDragReroutable = ExcessDragIsReroutable(excessDrag);
			var canRouteExcessDragToParent = !routedUnusedAxisDragToParent && excessDragReroutable;
			if (!currentlyRoutingExcessDragToParent && canRouteExcessDragToParent) {
				// var parent = ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
				// var reroutableParent = parent.GetComponent<IReroutableDrag>();
				
				// We need to start a drag each time we first reroute the drag to the parent (which might happen several times in a drag if you move up and down)
				// The reason for this is that some drag systems (such as ScrollRect) use delta from the drag start point, rather than the mouse delta.
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
				
				// if(reroutableParent != null)reroutableParent.AcceptReroutedDrag(eventData);
				routedExcessDragToParent = true;
				currentlyRoutingExcessDragToParent = true;
			}

			if (currentlyRoutingExcessDragToParent && !canRouteExcessDragToParent) {
				currentlyRoutingExcessDragToParent = false;
				// If this is used, then the parent will stop dragging, which means that it might continue with inertia, or snap back to a position, or whatefver
				// We want the nested drag items to continue being dragged.
				// This does mean that more BeginDrag events are fired than EndDrag events, but that's not normally a problem. 
				// ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
			}

			if (routedUnusedAxisDragToParent || (currentlyRoutingExcessDragToParent && excessDragReroutable)) {
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
			} else {
				base.OnDrag (eventData);
				
				if (eventData.button == PointerEventData.InputButton.Left) onDrag.Invoke(eventData);
			}
		}

		Vector2 GetAmountOfExcessMovement(PointerEventData eventData) {
			Type type = typeof(ScrollRect);
			FieldInfo fieldInfo = type.GetField("m_PointerStartLocalCursor", BindingFlags.NonPublic | BindingFlags.Instance);
			var m_PointerStartLocalCursor = (Vector2)fieldInfo.GetValue(this);
			MethodInfo calculateOffsetMethodInfo = type.GetMethod("CalculateOffset", BindingFlags.NonPublic | BindingFlags.Instance);

			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out var localCursor))
				return Vector2.zero;

			UpdateBounds();
			
			var pointerDelta = localCursor - m_PointerStartLocalCursor;
			Vector2 position = m_ContentStartPosition + pointerDelta;

			return (Vector2)calculateOffsetMethodInfo.Invoke(this, new object[] {position - content.anchoredPosition});
		}

		protected override void OnDisable() {
			dragging = false;
			base.OnDisable();
		}
		// #if UNITY_EDITOR
		// protected override void Reset() {
		// 	this.viewport = this.transform.Find("Viewport").transform as RectTransform;
		// 	this.content = this.transform.Find("Viewport/Content").transform as RectTransform;
		// 	base.Reset();
		// }
		// #endif
	}
}