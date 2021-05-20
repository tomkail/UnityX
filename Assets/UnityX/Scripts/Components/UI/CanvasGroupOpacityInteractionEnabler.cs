using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Toggles properties of a CanvasGroup based on its alpha. 
/// Useful for allowing raycasts to automatically pass through an invisible CanvasGroup.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[ExecuteInEditMode]
public class CanvasGroupOpacityInteractionEnabler : UIBehaviour {
	private CanvasGroup canvasGroup {
        get {
            return GetComponent<CanvasGroup>();
        }
    }
	[ClampAttribute(0f,1f)]
	public float alphaThreshold = 1;
	public bool interactable = true;
	public bool blocksRaycasts = true;

    protected override void OnCanvasGroupChanged() {
        base.OnCanvasGroupChanged();
        var alphaIsValid = alphaThreshold == 1 ? canvasGroup.alpha >= alphaThreshold : canvasGroup.alpha > alphaThreshold;
        
        var newBlocksRaycasts = blocksRaycasts && alphaIsValid;
		if(canvasGroup.blocksRaycasts != newBlocksRaycasts) canvasGroup.blocksRaycasts = newBlocksRaycasts;

        var newInteractable = interactable && alphaIsValid;
		if(canvasGroup.interactable != newInteractable) canvasGroup.interactable = newInteractable;
    }
}