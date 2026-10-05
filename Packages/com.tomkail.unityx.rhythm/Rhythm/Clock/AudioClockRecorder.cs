using System.IO;
using UnityEngine;

namespace UnityX.Rhythm {
	// Records (realtime, dspTime) every frame to a ClockTrace file, so clock behaviour on a real device can be replayed
	// in tests. Add it to a scene, play, and the trace is saved to Application.persistentDataPath when it stops.
	[DefaultExecutionOrder(-1001)]
	public class AudioClockRecorder : MonoBehaviour {
		public string fileName = "clock-trace.txt";
		[Tooltip("Stops after this many seconds. 0 records until disabled.")]
		public float duration = 30;

		ClockTrace trace;
		double startRealtime;

		public string LastSavedPath { get; private set; }

		void OnEnable() {
			trace = new ClockTrace { bufferDuration = new UnityAudioTimeSource().BufferDuration };
			startRealtime = Time.realtimeSinceStartupAsDouble;
		}

		void Update() {
			var realtime = Time.realtimeSinceStartupAsDouble;
			trace.samples.Add((realtime, AudioSettings.dspTime));
			if (duration > 0 && realtime - startRealtime >= duration) enabled = false;
		}

		// Records a labelled moment, e.g. when a MIDI note arrived, on the same timeline
		public void Mark(string label, double realtime) {
			trace?.marks.Add((realtime, label));
		}

		void OnDisable() => Save();

		public void Save() {
			if (trace == null || trace.samples.Count == 0) return;
			LastSavedPath = Path.Combine(Application.persistentDataPath, fileName);
			File.WriteAllText(LastSavedPath, trace.Serialize());
			Debug.Log($"Saved clock trace to {LastSavedPath}");
		}
	}
}
