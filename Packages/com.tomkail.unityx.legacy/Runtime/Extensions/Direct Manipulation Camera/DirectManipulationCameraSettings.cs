using UnityEngine;

public class DirectManipulationCameraSettings : ScriptableObject {
	public float mouseWheelSpeed = 2f;
	public float OSXDefaultScrollSpeedMultipler = 1f;
	[Space]
	public float keyboardPanSpeed = 1f;
	[Space]
	public float momentumSmoothTime = 0.2f;
	public float maxViewportSpaceMomentum = Mathf.Infinity;
}