using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// To allow an invisible button without having an image or text
/// http://answers.unity3d.com/questions/801928/46-ui-making-a-button-transparent.html#answer-851816
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class InvisibleInteractable : MaskableGraphic, ICanvasRaycastFilter
{
	public override bool Raycast(Vector2 sp, Camera eventCamera) {
		return base.Raycast(sp, eventCamera);
	}

	protected override void Awake() {
		base.Awake();
		useLegacyMeshGeneration = false;
	}

	protected override void OnPopulateMesh(VertexHelper vh) {
		vh.Clear();
	}

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
		// Note that RectTransformUtility.RectangleContainsScreenPoint doesn't work. The solution here mirrors how Image works.
		Vector2 local;
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);
	}
}