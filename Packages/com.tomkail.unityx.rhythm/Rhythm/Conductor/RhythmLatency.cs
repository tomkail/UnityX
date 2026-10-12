using UnityEngine;

namespace UnityX.Rhythm {
	// Latency offsets for a device or setup. Both follow one convention: actual = raw + offset.
	[CreateAssetMenu(menuName = "UnityX/Rhythm/Latency", fileName = "RhythmLatency")]
	public class RhythmLatency : ScriptableObject {
		[Tooltip("Seconds from audio being scheduled to it being heard. Visuals read the clock this far in the past so they match what's heard.")]
		public double audioOutputLatency;
		[Tooltip("Added to input timestamps to get when the player actually played. Negative when inputs arrive late.")]
		public double inputLatency;

		public double CorrectInputDspTime(double rawDspTime) => rawDspTime + inputLatency;
	}
}
