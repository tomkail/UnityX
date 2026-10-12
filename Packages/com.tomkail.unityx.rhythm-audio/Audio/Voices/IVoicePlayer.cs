namespace UnityX.Rhythm {
	// Plays note sounds. AudioSourceVoicePlayer uses a pool of AudioSources; tests use a fake.
	public interface IVoicePlayer {
		// Queues the note's sound to start at dspTime (a past time starts it now). Returns null if the note has no sound.
		IVoice Play(NoteInstance note, double dspTime);
	}

	// One queued or playing sound. Once a player reuses the voice's channel for another sound, calls do nothing.
	public interface IVoice {
		double StartDspTime { get; }
		void Reschedule(double dspTime);
		void Stop();
		bool IsFinished(double dspTime);
	}
}
