using System;
using UnityEngine;

namespace UnityX.Rhythm {
	// Turns presses into RhythmInputs on a Conductor's clock, applying its input latency once.
	// Use it directly for custom input such as touch zones, calling Submit; InputActionLaneInput and MidiLaneInput
	// build on it.
	public class RhythmInputSource : MonoBehaviour, IRhythmInputSource {
		public Conductor conductor;

		public event Action<RhythmInput> InputReceived;

		// realtime is when the press happened, on the Time.realtimeSinceStartupAsDouble timeline, which input event
		// timestamps use. Dropped if the conductor has no clock yet.
		public void Submit(int lane, InputPhase phase, float velocity, double realtime) {
			if (conductor == null || conductor.Clock == null) return;
			var dspTime = conductor.Clock.RealtimeToDspTime(realtime);
			if (conductor.Latency != null) dspTime = conductor.Latency.CorrectInputDspTime(dspTime);
			InputReceived?.Invoke(new RhythmInput(lane, phase, Mathf.Clamp01(velocity), dspTime));
		}

		// For input already on the audio clock with latency applied, e.g. an auto-player hitting notes exactly
		public void SubmitAtDspTime(int lane, InputPhase phase, float velocity, double dspTime) {
			InputReceived?.Invoke(new RhythmInput(lane, phase, Mathf.Clamp01(velocity), dspTime));
		}
	}
}
