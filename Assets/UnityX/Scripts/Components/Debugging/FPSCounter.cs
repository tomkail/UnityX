using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour {
	public static FPSCounter inst;
	public float frequency = 0.5f;
 
	public int fps { get; protected set; }
	public int targetFPS = 30;
	public Gradient colors;
	public Vector2 normalizedPosition;

	void Awake () {
		inst = this;
	}

	private void Start() {
		StartCoroutine(FPS());
	}

	private IEnumerator FPS() {
		for(;;){
			// Capture frame-per-second
			int lastFrameCount = Time.frameCount;
			float lastTime = Time.realtimeSinceStartup;
			yield return new WaitForSeconds(frequency);
			float timeSpan = Time.realtimeSinceStartup - lastTime;
			int frameCount = Time.frameCount - lastFrameCount;
			fps = Mathf.RoundToInt(frameCount / timeSpan);
		}
	}

	void OnGUI () {
		colors = GradientX.Create (new GradientColorKey[] {
			new GradientColorKey (Color.red, 0.0f),
			new GradientColorKey (Color.yellow, 0.4f),
			new GradientColorKey (Color.green, 0.5f),
			new GradientColorKey (Color.cyan, 0.75f),
			new GradientColorKey (Color.white, 1f),
		});
		GUI.color = colors.Evaluate ((float)fps / (targetFPS * 2));
		Rect rect = new Rect (normalizedPosition.x * Screen.width, normalizedPosition.y * Screen.height, 50, 30);
//		if (rect.x + rect.width > Screen.width) {
//			rect.x += Screen.width - (rect.x + rect.width);
//		} else if (rect.y + rect.height > Screen.height) {
//			rect.y += Screen.height - (rect.y + rect.height);
//		}
		GUI.Box (rect, "");
		GUI.Box (rect, "");
		GUI.Box (rect, "");
		GUI.Box (rect, "");
		GUI.Box (rect, fps.ToString () + " fps");
	}
}