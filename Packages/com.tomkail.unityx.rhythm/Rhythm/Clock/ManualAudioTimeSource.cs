using System;

namespace UnityX.Rhythm {
	// An audio clock you drive by hand, for tests and replays: dspTime advances in whole buffers, like Unity's
	public sealed class ManualAudioTimeSource : IAudioTimeSource {
		public double Realtime { get; set; }
		public double BufferDuration { get; set; } = 1024 / 44100.0;
		// Offset between the audio clock and real time
		public double DspOffset { get; set; } = 100;
		// While stalled, dspTime stops advancing
		public bool Stalled { get; set; }
		double stalledDspTime;

		public double DspTime => Stalled ? stalledDspTime : Math.Floor((Realtime + DspOffset) / BufferDuration) * BufferDuration;

		public void Advance(double seconds) {
			if (!Stalled) stalledDspTime = DspTime;
			Realtime += seconds;
		}

		public void Stall() { stalledDspTime = DspTime; Stalled = true; }
		// Resumes where the audio clock stopped, so the gap is lost from the audio timeline
		public void Unstall() {
			DspOffset = stalledDspTime + BufferDuration - Realtime;
			Stalled = false;
		}
	}
}
