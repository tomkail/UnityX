using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Toggles properties of a CanvasGroup based on its alpha. 
/// Useful for allowing raycasts to automatically pass through an invisible CanvasGroup.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[ExecuteAlways]
public class CanvasGroupOpacityInteractionEnabler : UIBehaviour {
    CanvasGroup canvasGroup => GetComponent<CanvasGroup>();
    
	[Range(0f,1f)]
	public float alphaThreshold = 1;
	public bool ignoreParentGroups;
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
        var alpha = ignoreParentGroups ? canvasGroup.alpha : CanvasGroupsAlpha(gameObject);
        var alphaIsValid = alphaThreshold == 1 ? alpha >= alphaThreshold : alpha > alphaThreshold;
        
        var newBlocksRaycasts = blocksRaycasts && alphaIsValid;
		if(canvasGroup.blocksRaycasts != newBlocksRaycasts) canvasGroup.blocksRaycasts = newBlocksRaycasts;

        var newInteractable = interactable && alphaIsValid;
		if(canvasGroup.interactable != newInteractable) canvasGroup.interactable = newInteractable;
    }
    
    static readonly List<CanvasGroup> m_CanvasGroupCache = new();
    static float CanvasGroupsAlpha (GameObject gameObject) {
        var groupAlpha = 1f;
        Transform t = gameObject.transform;
        while (t != null) {
            t.GetComponents(m_CanvasGroupCache);
            bool shouldBreak = false;
            for (var i = 0; i < m_CanvasGroupCache.Count; i++)
            {
                groupAlpha *= m_CanvasGroupCache[i].alpha;
                
                // if this is a 'fresh' group, then break
                // as we should not consider parents
                if (m_CanvasGroupCache[i].ignoreParentGroups)
                    shouldBreak = true;
            }
            if (shouldBreak)
                break;

            t = t.parent;
        }
        return groupAlpha;
    }
}