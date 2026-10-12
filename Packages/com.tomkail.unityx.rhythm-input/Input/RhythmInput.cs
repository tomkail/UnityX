using System;

namespace UnityX.Rhythm {
	public enum InputPhase {
		Press,
		Release
	}

	// One press or release on a lane, timed on the audio clock with input latency already applied
	public readonly struct RhythmInput {
		public readonly int lane;
		public readonly InputPhase phase;
		// 0-1, e.g. MIDI velocity or analog pressure. 1 for plain buttons.
		public readonly float velocity;
		public readonly double dspTime;

		public RhythmInput(int lane, InputPhase phase, float velocity, double dspTime) {
			this.lane = lane;
			this.phase = phase;
			this.velocity = velocity;
			this.dspTime = dspTime;
		}

		public bool IsPress => phase == InputPhase.Press;

		public override string ToString() => $"{phase} lane {lane} at {dspTime:F4} (velocity {velocity:F2})";
	}

	public interface IRhythmInputSource {
		event Action<RhythmInput> InputReceived;
	}
}
