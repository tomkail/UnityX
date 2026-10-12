using System.IO;

namespace UnityX.Rhythm.Tests {
	static class Fixtures {
		public static ClockTrace LoadTrace(string name) {
#if UNITY_EDITOR
			var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>($"Packages/com.tomkail.unityx.rhythm/Tests/Editor/Fixtures/{name}");
			return ClockTrace.Parse(asset.text);
#else
			return ClockTrace.Parse(File.ReadAllText(Path.Combine(System.AppContext.BaseDirectory, "Fixtures", name)));
#endif
		}
	}
}
