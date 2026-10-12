using System.Collections.Generic;

namespace UnityX.Rhythm.JudgementTests {
	// A song, a note scheduler and a judge, recording everything the judge reports
	public sealed class JudgeRig {
		public readonly TestSong song;
		public readonly NoteScheduler notes;
		public readonly Judge judge;
		public readonly List<Judgement> judged = new();
		public readonly List<NoteInstance> missed = new();
		public readonly List<RhythmInput> strays = new();
		public readonly List<NoteInstance> holdsCompleted = new();
		public readonly List<NoteInstance> holdsDropped = new();

		public JudgeRig(INoteSource source, JudgementWindows windows = null, double bpm = 120) {
			song = new TestSong(bpm);
			notes = new NoteScheduler(song.timeline, source);
			judge = new Judge(song.timeline, notes, windows ?? new JudgementWindows());
			judge.Judged += judged.Add;
			judge.Missed += missed.Add;
			judge.StrayHit += strays.Add;
			judge.HoldCompleted += holdsCompleted.Add;
			judge.HoldDropped += (note, input) => holdsDropped.Add(note);
		}

		public void Run(double seconds) => song.Run(seconds, judge.Update);

		public double DspAtBeat(double beat) => song.timeline.DspTimeAtBeat(beat);

		public void Press(int lane, double dspTime, float velocity = 1) => judge.Submit(new RhythmInput(lane, InputPhase.Press, velocity, dspTime));
		public void Release(int lane, double dspTime) => judge.Submit(new RhythmInput(lane, InputPhase.Release, 0, dspTime));

		// Presses `offset` seconds from a beat (negative is early)
		public void PressAtBeat(int lane, double beat, double offset = 0, float velocity = 1) => Press(lane, DspAtBeat(beat) + offset, velocity);

		// Starts playing from songTime and gives the judge its first look
		public void Play(double songTime = 0) {
			song.clock.Play(songTime);
			judge.Update();
		}
	}
}
