using UnityEngine;

namespace UnityX.Rhythm {
	// A pattern saved as an asset. Play a runtime copy, so playing never edits the asset.
	[CreateAssetMenu(menuName = "UnityX/Rhythm/Pattern")]
	public class PatternAsset : ScriptableObject {
		[SerializeField] Pattern pattern = new();

		// The authored pattern. Edit this only in editor tools.
		public Pattern Pattern => pattern;

		public Pattern CreateRuntimeCopy() => pattern.Clone();

		void OnValidate() => pattern.NotifyChanged();
	}
}
