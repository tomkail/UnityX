using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Toggles properties of a CanvasGroup based on its alpha. 
/// Useful for allowing raycasts to automatically pass through an invisible CanvasGroup.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[ExecuteAlways]
public class CanvasGroupOpacityInteractionEnabler : UIBehaviour {
	private CanvasGroup canvasGroup {
        get {
            return GetComponent<CanvasGroup>();
        }
    }
	[ClampAttribute(0f,1f)]
	public float alphaThreshold = 1;
	public bool ignoreParentGroups = false;
	public bool interactable = true;
	public bool blocksRaycasts = true;

    void Update () {
        if(ignoreParentGroups) return;
        Refresh();
    }
    protected override void OnCanvasGroupChanged() {
        if(!ignoreParentGroups) return;
        base.OnCanvasGroupChanged();
        Refresh();
    }

    void Refresh () {
        var alpha = ignoreParentGroups ? canvasGroup.alpha : CanvasGroupX.CanvasGroupsAlpha(gameObject);
        var alphaIsValid = alphaThreshold == 1 ? alpha >= alphaThreshold : alpha > alphaThreshold;
        
        var newBlocksRaycasts = blocksRaycasts && alphaIsValid;
		if(canvasGroup.blocksRaycasts != newBlocksRaycasts) canvasGroup.blocksRaycasts = newBlocksRaycasts;

        var newInteractable = interactable && alphaIsValid;
		if(canvasGroup.interactable != newInteractable) canvasGroup.interactable = newInteractable;
    }
}