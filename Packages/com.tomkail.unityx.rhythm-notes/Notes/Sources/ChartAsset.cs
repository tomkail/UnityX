using UnityEngine;

namespace UnityX.Rhythm {
	// A chart saved as an asset, with the tempo map it was charted against. Play a runtime copy, so playing never
	// edits the asset.
	[CreateAssetMenu(menuName = "UnityX/Rhythm/Chart")]
	public class ChartAsset : ScriptableObject {
		[SerializeField] TempoMap tempoMap = new(120);
		[SerializeField] Chart chart = new();

		// The authored data. Edit these only in editor tools.
		public TempoMap TempoMap => tempoMap;
		public Chart Chart => chart;

		public Chart CreateRuntimeChart() => chart.Clone();
		public TempoMap CreateRuntimeTempoMap() => tempoMap.Clone();

		void OnValidate() => chart.NotifyChanged();
	}
}
