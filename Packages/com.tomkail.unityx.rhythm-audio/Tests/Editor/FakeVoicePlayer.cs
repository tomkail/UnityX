using System.Collections.Generic;

namespace UnityX.Rhythm.Audio.Tests {
	// Records what would have been played. Every sound lasts `length` seconds.
	public sealed class FakeVoicePlayer : IVoicePlayer {
		public sealed class FakeVoice : IVoice {
			public NoteInstance note;
			public double length;
			public bool stopped;
			public int reschedules;
			public double StartDspTime { get; set; }
			public void Reschedule(double dspTime) { StartDspTime = dspTime; reschedules++; }
			public void Stop() => stopped = true;
			public bool IsFinished(double dspTime) => stopped || dspTime >= StartDspTime + length;
		}

		public readonly List<FakeVoice> voices = new();
		public double length = 0.1;

		public IVoice Play(NoteInstance note, double dspTime) {
			var voice = new FakeVoice { note = note, StartDspTime = dspTime, length = length };
			voices.Add(voice);
			return voice;
		}
	}
}
