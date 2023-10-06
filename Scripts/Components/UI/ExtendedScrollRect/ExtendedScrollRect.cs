using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI {
	public class ExtendedScrollRect : ScrollRect {
		public bool routeScrollEventsToParent = false;
   		protected bool routeToParent = false;

        // The content rect is scaled, so there's some minor inaccuracy when comparing sizes which this helps mitigate.
        const float Epsilon = 0.001f;
		public new RectTransform viewRect {
			get {
				return base.viewRect;
			}
		}
		
        public Rect containerRect {
			get {
				return RectX.MinMaxRect((Vector2)viewBounds.min, (Vector2)viewBounds.max);
			}
		}


        // Content rect, also taking the scale of the content into account (as Unity's scrollrect does)
        public Rect scaledContentRect {
			get {
				return RectX.MinMaxRect((Vector2)contentBounds.min, (Vector2)contentBounds.max);
			}
		}

        public Bounds contentBounds {
			get {
				// This only runs in update when enabled. Since we want to be able to call this even when disabled, we force the bounds to be calculated here.
				if(!enabled) ForceUpdateBounds();
				return base.m_ContentBounds;
			}
		}
        
        // Note - we should stop using bounds and use contentRect and viewRect.rect instead
		public Bounds viewBounds {
			get {
				return new Bounds(viewRect.rect.center, viewRect.rect.size);
			}
		}
        
		public Vector2 freeMovementSize {
			get {
				return scaledContentRect.size-viewRect.rect.size;
			}
		}

        public Vector2 contentOffset {
			get {
				return viewRect.rect.position - (Vector2)scaledContentRect.position;
			}
		}

        // 0,0 when the bottom-left of the content matches that of the container; 1, when the top-right of the content matches that of the container.
        // When container is larger than content the axis tends to return 0.5, but is a bit unpredictable since _freeMovementSize is 0 or near it.
        public Vector2 normalizedContentOffset {
			get {
                var _contentOffset = contentOffset;
                var _freeMovementSize = freeMovementSize;
				return new Vector2(_contentOffset.x/_freeMovementSize.x, _contentOffset.y/_freeMovementSize.y);
			}
		}

        
		// These setters were last edited when I added the horizontal ones. I don't know why the vertical ones are offset by half; my guess is that it's a pivot/anchor thing and that the code for both depends on the setup, and probably wants changing!
        public float distanceToLeft {
            get {
                return contentOffset.x;
            } set {
                var x = -value;
                content.localPosition = new Vector3(x, content.localPosition.y, content.localPosition.z);
            }
        }
        public float distanceToRight {
            get {
                return freeMovementSize.x - contentOffset.x;
            } set {
                var x = value - freeMovementSize.x;
                content.localPosition = new Vector3(x, content.localPosition.y, content.localPosition.z);
            }
        }
        public float distanceToTop {
            get {
                return freeMovementSize.y - contentOffset.y;        
            } set {
                var y = value - freeMovementSize.y * 0.5f;
                content.localPosition = new Vector3(content.localPosition.x, y, content.localPosition.z);
            }
        }
        public float distanceToBottom {
            get {
                return contentOffset.y;
            } set {
                var y = value + freeMovementSize.y * 0.5f;
                content.localPosition = new Vector3(content.localPosition.x, y, content.localPosition.z);
            }
        }

        public bool canScrollLeft {
            get {
                return contentExceedsViewportX && distanceToLeft > Epsilon;
            }
        }
        public bool canScrollRight {
            get {
                return contentExceedsViewportX && distanceToRight > -Epsilon;
            }
        }
        public bool canScrollUp {
            get {
                return contentExceedsViewportY && distanceToTop > Epsilon;            
            }
        }
        public bool canScrollDown {
            get {
                return contentExceedsViewportY && distanceToBottom > -Epsilon;
            }
        }
        public bool contentExceedsViewportX {
			get {
				return freeMovementSize.x > Epsilon;
			}
		}
        public bool contentExceedsViewportY {
			get {
				return freeMovementSize.y > Epsilon;
			}
		}

		public float xMinScroll {
			get {
				return Mathf.Max(0, (contentBounds.size.x - viewRect.rect.width) * 0.5f);
			}
		}

		public float xMaxScroll {
			get {
				return -xMinScroll;
			}
		}

        public bool yContentLargerThanViewport {
			get {
				// This takes scale into account
				return ySpareSpace < 0;
				// return hScrollingNeeded;
			}
		}

		public float ySpareSpace {
			get {
				return viewRect.rect.width - contentBounds.size.y;
			}
		}

		public float yMinScroll {
			get {
				return Mathf.Max(0, (contentBounds.size.y - viewRect.rect.width) * 0.5f);
			}
		}

		public float yMaxScroll {
			get {
				return -yMinScroll;
			}
		}

        

        public Vector2 minScroll {
            get {
                var _minScroll = (Vector2)contentBounds.size - viewRect.rect.size;
                return new Vector2(Mathf.Max(0, (_minScroll.x) * 0.5f), Mathf.Max(0, (_minScroll.y) * 0.5f));
            }
        }

        public Vector2 maxScroll {
            get {
                return -minScroll;
            }
        }

        public float GetClampedAnchoredPositionX (float contentAnchoredPositionX) {
            return Mathf.Clamp(contentAnchoredPositionX, maxScroll.x, minScroll.x);
        }
        public float GetClampedAnchoredPositionY (float contentAnchoredPositionY) {
            return Mathf.Clamp(contentAnchoredPositionY, maxScroll.y, minScroll.y);
        }
        public Vector2 GetClampedAnchoredPosition (Vector2 contentAnchoredPosition) {
            return new Vector2(GetClampedAnchoredPositionX(contentAnchoredPosition.x), GetClampedAnchoredPositionY(contentAnchoredPosition.y));
        }


		private readonly Vector3[] m_Corners = new Vector3[4];
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


		[System.Serializable]
		public class ScrollRectScrollEvent : UnityEvent<PointerEventData> {}
		
		[System.Serializable]
		public class ScrollRectBeginDragEvent : UnityEvent<PointerEventData> {}
		
		[System.Serializable]
		public class ScrollRectEndDragEvent : UnityEvent<PointerEventData> {}
		
		[System.Serializable]
		public class ScrollRectDragEvent : UnityEvent<PointerEventData> {}
		
		
		public ScrollRectScrollEvent onScroll = new ScrollRectScrollEvent();
		public ScrollRectBeginDragEvent onBeginDrag = new ScrollRectBeginDragEvent();
		public ScrollRectEndDragEvent onEndDrag = new ScrollRectEndDragEvent();
		public ScrollRectDragEvent onDrag = new ScrollRectDragEvent();

		public void ForceUpdateBounds() {
			base.UpdateBounds();
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
			if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
				routeToParent = true;
			else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
				routeToParent = true;
			else
				routeToParent = false;
		
			if (routeToParent)
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
			else {
				base.OnBeginDrag (eventData);
				
				if (eventData.button != PointerEventData.InputButton.Left)
					return;
				
				if (!IsActive())
					return;
				
				onBeginDrag.Invoke(eventData);
			}	
		}
		
		public override void OnEndDrag (PointerEventData eventData) {
			if (routeToParent)
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
			else {
				base.OnEndDrag (eventData);
				if (eventData.button == PointerEventData.InputButton.Left) onEndDrag.Invoke(eventData);
			}
			routeToParent = false;
		}
		
		public override void OnDrag (PointerEventData eventData) {
			if (routeToParent) {
				ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
			} else {
				base.OnDrag (eventData);
				
				if (eventData.button == PointerEventData.InputButton.Left) onDrag.Invoke(eventData);
			}
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