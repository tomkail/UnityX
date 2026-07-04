namespace UnityEngine.UI {
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class GridLayoutItem : MonoBehaviour {
		public RectTransform rectTransform => (RectTransform) transform;

		public GridLayout gridLayout;
		public Vector2 gridCoordinate;

		DrivenRectTransformTracker drivenRectTransformTracker;

		void OnEnable() {
			Refresh();
		}

		void OnDisable() {
			drivenRectTransformTracker.Clear();
		}

		void Update() {
			Refresh();
		}

		void Refresh() {
			drivenRectTransformTracker.Clear();

			if (gridLayout == null) return;

			gridLayout.ApplyToRectTransform(rectTransform, gridCoordinate);

			drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta | DrivenTransformProperties.AnchoredPosition3D);
		}
	}
}