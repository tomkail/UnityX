using UnityEngine;
using UnityEngine.UI;

// IMAGE MUST HAVE IN SETTINGS:
// TEXTURE TYPE - SPRITE (2D AND UI), Full rect, readWriteEnabled
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class AlphaHitTestThresholdSetter : MonoBehaviour {
	public Image image => GetComponent<Image>();
	public float alphaThreshold = 0.1f;
	void OnValidate() {
		Refresh();
	}
	void OnEnable() {
		Refresh();
	}
	void OnDisable() {
		Refresh();
	}
	void Update() {
		Refresh();
	}
	void Refresh() {
		if(isActiveAndEnabled) image.alphaHitTestMinimumThreshold = alphaThreshold;
		// Reset to default when disabled
		else image.alphaHitTestMinimumThreshold = 0;
	}
}