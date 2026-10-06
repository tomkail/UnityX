using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	// Matches presses to notes and reports hits, misses and stray hits.
	// - A press goes to the nearest open note in its lane (any lane with anyLane) inside the widest window. Outside
	//   every window, it's a stray hit and the notes stay open.
	// - A note whose window has passed is missed. Notes the song jumped over (a seek forward) are skipped instead:
	//   neither hit nor missed.
	// - Holds are judged on the press, then complete when the song reaches their end, or drop if released earlier
	//   than holdReleaseEarly before it.
	// - A seek back reopens the notes it moved back in front of, so they can be played again.
	// - Results are kept by NoteId, so tempo, rate and swing changes never re-judge a note.
	// Call Update once per frame, after the clock has ticked: it updates the NoteScheduler too.
	public sealed class Judge : IDisposable {
		enum State {
			Hit,
			Holding,
			Missed,
			Skipped
		}

		// Covers a frame's delay in noticing a note has passed, so its look-behind always holds the note long enough
		const double LookBehindMargin = 0.05;

		readonly IBeatTimeline timeline;
		readonly NoteScheduler notes;
		readonly Dictionary<NoteId, State> states = new();
		readonly List<IRhythmInputSource> sources = new();
		readonly List<NoteId> scratch = new();

		public bool anyLane;
		// Extra time before a note counts as missed, in seconds, so a press timed inside its window but delivered a
		// frame later still hits it
		public double missDelay = 0.05;

		public Judge(IBeatTimeline timeline, NoteScheduler notes, JudgementWindows windows) {
			this.timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
			this.notes = notes ?? throw new ArgumentNullException(nameof(notes));
			Windows = windows ?? throw new ArgumentNullException(nameof(windows));
			notes.NoteEntered += OnNoteEntered;
			notes.NoteExited += OnNoteExited;
		}

		public JudgementWindows Windows { get; set; }
		public NoteScheduler Notes => notes;

		public event Action<Judgement> Judged;
		public event Action<NoteInstance> Missed;
		public event Action<RhythmInput> StrayHit;
		public event Action<NoteInstance> HoldCompleted;
		public event Action<NoteInstance, RhythmInput> HoldDropped;

		public void Attach(IRhythmInputSource source) {
			source.InputReceived += Submit;
			sources.Add(source);
		}

		public void Detach(IRhythmInputSource source) {
			if (sources.Remove(source)) source.InputReceived -= Submit;
		}

		public void Dispose() {
			foreach (var source in sources) source.InputReceived -= Submit;
			sources.Clear();
			notes.NoteEntered -= OnNoteEntered;
			notes.NoteExited -= OnNoteExited;
		}

		// An active note that hasn't been hit, missed or skipped
		public bool IsOpen(NoteId id) => notes.IsActive(id) && !states.ContainsKey(id);
		public bool IsHolding(NoteId id) => states.TryGetValue(id, out var state) && state == State.Holding;

		public void Update() {
			var clock = timeline.Clock;
			notes.lookBehind = Math.Max(notes.lookBehind, RequiredLookBehind());
			notes.Update();
			var now = clock.DspTime;
			if (notes.LastJump < 0) Reopen(now);
			else if (notes.LastJump > 0) SkipUnreachable(now);
			if (!clock.IsPlaying) return;
			foreach (var note in notes.ActiveNotes) {
				if (!states.TryGetValue(note.id, out var state)) {
					if (now > Deadline(note)) {
						states[note.id] = State.Missed;
						Missed?.Invoke(note);
					}
				} else if (state == State.Holding && now >= notes.EndDspTimeOf(note)) {
					states[note.id] = State.Hit;
					HoldCompleted?.Invoke(note);
				}
			}
		}

		public void Submit(RhythmInput input) {
			if (!timeline.Clock.IsPlaying) {
				if (input.IsPress) StrayHit?.Invoke(input);
				return;
			}
			if (input.IsPress) Press(input);
			else Release(input);
		}

		void Press(RhythmInput input) {
			var windows = Windows;
			var widestEarly = windows.WidestEarly;
			var widestLate = windows.WidestLate;
			NoteInstance best = default;
			var found = false;
			var bestOffset = 0.0;
			foreach (var note in notes.ActiveNotes) {
				if (!anyLane && note.note.lane != input.lane) continue;
				if (states.ContainsKey(note.id)) continue;
				var offset = Offset(input.dspTime, note.Beat);
				if (offset < -widestEarly || offset > widestLate) continue;
				if (!found || Math.Abs(offset) < Math.Abs(bestOffset)) {
					best = note;
					bestOffset = offset;
					found = true;
				}
			}
			if (!found) {
				StrayHit?.Invoke(input);
				return;
			}
			var grade = windows.GradeFor(bestOffset);
			states[best.id] = best.note.length > 0 ? State.Holding : State.Hit;
			var timeOffset = input.dspTime - notes.DspTimeOf(best);
			var beatOffset = timeline.BeatAtDspTime(input.dspTime) - best.Beat;
			Judged?.Invoke(new Judgement(best, input, grade, windows.grades[grade].name, timeOffset, beatOffset, windows.AccuracyFor(bestOffset)));
		}

		// Releases outside a hold don't count against the player
		void Release(RhythmInput input) {
			foreach (var note in notes.ActiveNotes) {
				if (!anyLane && note.note.lane != input.lane) continue;
				if (!states.TryGetValue(note.id, out var state) || state != State.Holding) continue;
				states[note.id] = State.Hit;
				if (Offset(input.dspTime, note.EndBeat) >= -Windows.holdReleaseEarly) HoldCompleted?.Invoke(note);
				else HoldDropped?.Invoke(note, input);
				return;
			}
		}

		// Hit minus note, in the windows' unit
		double Offset(double dspTime, double noteBeat) {
			return Windows.unit == WindowUnit.Beats
				? timeline.BeatAtDspTime(dspTime) - noteBeat
				: dspTime - timeline.DspTimeAtBeat(noteBeat);
		}

		// When a note stops being hittable and counts as missed
		double Deadline(NoteInstance note) {
			var late = Windows.WidestLate;
			var end = Windows.unit == WindowUnit.Beats ? timeline.DspTimeAtBeat(note.Beat + late) : notes.DspTimeOf(note) + late;
			return end + missDelay;
		}

		// How far back the scheduler must keep notes, in seconds of playback, for every miss to be noticed
		double RequiredLookBehind() {
			var late = Windows.WidestLate;
			if (Windows.unit == WindowUnit.Beats) {
				var clock = timeline.Clock;
				var bpm = timeline.TempoMap.BpmAtBeat(timeline.CurrentBeat());
				late = late * 60 / bpm / clock.PlaybackRate;
			}
			return late + missDelay + LookBehindMargin;
		}

		// After a seek back: notes that can be played again become open, even if they were hit or missed before
		void Reopen(double now) {
			scratch.Clear();
			foreach (var note in notes.ActiveNotes) {
				if (states.ContainsKey(note.id) && Deadline(note) > now) scratch.Add(note.id);
			}
			foreach (var id in scratch) states.Remove(id);
		}

		// After a seek forward: notes the song landed beyond were never playable, so they're skipped, not missed
		void SkipUnreachable(double now) {
			foreach (var note in notes.ActiveNotes) {
				if (!states.ContainsKey(note.id) && now > Deadline(note)) states[note.id] = State.Skipped;
			}
		}

		void OnNoteEntered(NoteInstance note) {
			// A note that arrives already too late (e.g. added behind the playhead) was never playable either
			if (timeline.Clock.IsPlaying && timeline.Clock.DspTime > Deadline(note)) states[note.id] = State.Skipped;
		}

		void OnNoteExited(NoteInstance note, NoteExitReason reason) {
			var wasOpen = !states.TryGetValue(note.id, out var state);
			states.Remove(note.id);
			// Normally caught by the deadline first; this covers a note leaving before Update saw it pass
			if (reason == NoteExitReason.Passed && wasOpen) Missed?.Invoke(note);
			else if (reason == NoteExitReason.Passed && state == State.Holding) HoldCompleted?.Invoke(note);
		}
	}
}
