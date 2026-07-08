namespace UnityX.SceneManagement {

	// Implement this on a MonoBehaviour to receive messages broadcast to a scene set via
	// RuntimeSceneSet.BroadcastMessageToIncludedScenes / RuntimeSceneSetLoader.BroadcastMessageScene.
	//
	// This replaces the old reflection + GameObject.SendMessage approach: the interface call works
	// identically in edit and play mode, is type-safe, and needs no reflection. Receivers dispatch on
	// the `message` string themselves.
	public interface ISceneSetMessageReceiver {
		void OnSceneSetMessage (string message, object argument);
	}
}
