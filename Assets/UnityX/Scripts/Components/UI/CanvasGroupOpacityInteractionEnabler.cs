using UnityEngine;
using System.Collections;

/// <summary>
/// Toggles properties of a CanvasGroup based on its alpha. 
/// Useful for allowing raycasts to automatically pass through an invisible CanvasGroup.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[ExecuteInEditMode]
public class CanvasGroupOpacityInteractionEnabler : MonoBehaviour {
	private CanvasGroup canvasGroup;
	[ClampAttribute(0f,1f)]
	public float alphaThreshold = 1;
	public bool blocksRaycasts = true;
	public bool interactable = true;
	
	void OnEnable () {
		canvasGroup = GetComponent<CanvasGroup>();
	}
	
	void Update () {
        var alphaIsValid = alphaThreshold == 1 ? canvasGroup.alpha >= alphaThreshold : canvasGroup.alpha > alphaThreshold;
		canvasGroup.blocksRaycasts = blocksRaycasts && alphaIsValid;
		canvasGroup.interactable = interactable && alphaIsValid;
	}
}
