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
	// - A seek back reopens the notes it moved back in front of, so they can be played again. That includes a note
	//   just hit, after even a short seek back, so a practice loop can score it again.
	// - Results are kept by NoteId, so tempo, rate and swing changes never re-judge a note.
	// - While paused nothing is missed or completed, and presses are stray hits: if you score stray hits, disable
	//   your input sources while paused. Notes whose deadline passed before the pause, including during a long frame
	//   that ended in it, are still missed. On the first Update after playback resumes, the net song time moved since
	//   the pause (seeks, scrubs) is settled, because deadlines can't be compared until the clock is playing again. If
	//   it moved by more than the scheduler's seekThreshold beyond what the frame played since resuming, it's settled
	//   like a single seek, and notes then past their deadline are skipped. Otherwise only notes already past their
	//   deadline at the pause point (left behind by a tempo edit, or entered while paused already too late) are
	//   skipped, and a note whose deadline passes in the first frame after resuming is missed.
	// - Keep calling Update while paused. If you skip it, the pause reads as playing time when you resume.
	// - Releases aren't seen while paused, so a hold that was held through a pause completes even if it was let go
	//   during the pause.
	// - A hitch longer than the scheduler's lookAhead brings notes in already past their deadline: those are skipped,
	//   not missed, as if the song had been seeked past them.
	// Call Update once per frame, after the clock has ticked: it updates the NoteScheduler too. The Judge claims its
	// NoteScheduler and is the only thing that may update it, because seek handling depends on seeing every
	// LastJump; anything else should only read its ActiveNotes and listen to its events. Dispose releases it.
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
		// Whether the clock was playing on the previous Update
		bool wasPlaying;
		// Where playback reached before a pause, until the pause is settled on resume
		double? songTimeAtPause;
		// The DSP time on the last paused Update, where Resume anchors playback
		double lastPausedDspTime;
		// Windows.WidestLate for this Update, which would otherwise loop over the grades for every note
		double late;
		// Notes that entered while paused, whose deadline can only be checked once playback resumes
		readonly HashSet<NoteId> enteredWhilePaused = new();

		public bool anyLane;
		// Extra time before a note counts as missed, in seconds, so a press timed inside its window but delivered a
		// frame later still hits it
		public double missDelay = 0.05;
		// The input latency to allow for before a note counts as missed: with negative inputLatency, a press played inside
		// the window arrives up to -inputLatency later. Falls back to the Conductor's current latency while null. A positive inputLatency never
		// shortens a deadline, so input submitted at its own time is never missed early.
		public RhythmLatency latency;

		public Judge(IBeatTimeline timeline, NoteScheduler notes, JudgementWindows windows) {
			this.timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
			this.notes = notes ?? throw new ArgumentNullException(nameof(notes));
			Windows = windows ?? throw new ArgumentNullException(nameof(windows));
			if (timeline is Conductor conductor) latency = conductor.Latency;
			notes.Claim(this);
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
			notes.Release(this);
		}

		// An active note that hasn't been hit, missed or skipped
		public bool IsOpen(NoteId id) => notes.IsActive(id) && !states.ContainsKey(id);
		public bool IsHolding(NoteId id) => states.TryGetValue(id, out var state) && state == State.Holding;

		public void Update() {
			var clock = timeline.Clock;
			// A Conductor that hasn't been initialised yet
			if (clock == null) return;
			late = Windows.WidestLate;
			notes.lookBehind = Math.Max(notes.lookBehind, RequiredLookBehind(clock));
			notes.Update(this);
			if (!clock.IsPlaying) {
				if (wasPlaying) {
					// This frame can have played right up to the pause, so the pause point is the song time now, less any
					// seek this frame. The seek is settled on resume as part of the net move, so it's counted once.
					var pausedAt = clock.SongTime - notes.LastJump;
					songTimeAtPause = pausedAt;
					MissPassedBefore(pausedAt, clock.PlaybackRate);
				}
				wasPlaying = false;
				lastPausedDspTime = clock.DspTime;
				return;
			}
			var now = clock.DspTime;
			// Whether this Update settles a pause without a seek
			var resumedInPlace = false;
			if (songTimeAtPause is double settledFrom) {
				songTimeAtPause = null;
				// Every deadline is infinite while paused, so how far song time moved is settled now. It's measured from the
				// pause rather than summed from LastJump, because a slow scrub never registers as a jump. What this frame
				// played since resuming is playback, not a seek, so it's allowed for, as NoteScheduler does.
				var rate = clock.PlaybackRate;
				var moved = clock.SongTime - settledFrom;
				var played = Math.Max(0, now - lastPausedDspTime) * rate;
				if (moved < -notes.seekThreshold) {
					Reopen(now);
					SkipUnreachable(now);
				} else if (moved > played + notes.seekThreshold) {
					SkipUnreachable(now);
				} else {
					// A note whose deadline fell in the resumed playback was playable, so the loop below misses it. Only
					// notes already past their deadline at the pause point (left behind by a tempo edit, or entered late)
					// are skipped.
					SkipLeftBehind(settledFrom, rate);
					resumedInPlace = true;
				}
			} else {
				// Only the pass for the jump's direction, so a short seek forward doesn't reopen a note that was hit early
				var jump = notes.LastJump;
				if (jump < 0) Reopen(now);
				else if (jump > 0) SkipUnreachable(now);
			}
			wasPlaying = true;
			var active = notes.ActiveNotes;
			if (enteredWhilePaused.Count > 0) {
				// As OnNoteEntered does while playing: a note that arrived already too late was never playable. Without a
				// seek, SkipLeftBehind has already judged that against the pause point.
				if (!resumedInPlace) {
					for (var i = 0; i < active.Count; i++) {
						var note = active[i];
						if (enteredWhilePaused.Contains(note.id) && !states.ContainsKey(note.id) && now > Deadline(note)) states[note.id] = State.Skipped;
					}
				}
				enteredWhilePaused.Clear();
			}
			for (var i = 0; i < active.Count; i++) {
				var note = active[i];
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
			var clock = timeline.Clock;
			if (clock == null) return;
			if (!clock.IsPlaying) {
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
			var active = notes.ActiveNotes;
			for (var i = 0; i < active.Count; i++) {
				var note = active[i];
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
			var holdReleaseEarly = Windows.holdReleaseEarly;
			var active = notes.ActiveNotes;
			for (var i = 0; i < active.Count; i++) {
				var note = active[i];
				if (!anyLane && note.note.lane != input.lane) continue;
				if (!states.TryGetValue(note.id, out var state) || state != State.Holding) continue;
				states[note.id] = State.Hit;
				if (Offset(input.dspTime, note.EndBeat) >= -holdReleaseEarly) HoldCompleted?.Invoke(note);
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

		// How much later than its timestamp a press can arrive, in real seconds
		double LateArrival {
			get {
				var current = latency != null ? latency : (timeline as Conductor)?.Latency;
				return current != null ? Math.Max(0, -current.inputLatency) : 0;
			}
		}

		// When a note stops being hittable and counts as missed
		double Deadline(NoteInstance note) {
			var end = Windows.unit == WindowUnit.Beats ? timeline.DspTimeAtBeat(note.Beat + late) : notes.DspTimeOf(note) + late;
			return end + missDelay + LateArrival;
		}

		// Deadline in song time, which unlike DSP time is still finite while paused
		double SongTimeDeadline(NoteInstance note, double rate) {
			var tempoMap = timeline.TempoMap;
			return Windows.unit == WindowUnit.Beats
				? tempoMap.TimeAtBeat(note.Beat + late) + (missDelay + LateArrival) * rate
				: tempoMap.TimeAtBeat(note.Beat) + (late + missDelay + LateArrival) * rate;
		}

		// How far back the scheduler must keep notes, in seconds of playback, for every miss to be noticed
		double RequiredLookBehind(IRhythmClock clock) {
			var seconds = late;
			if (Windows.unit == WindowUnit.Beats) {
				// A late window that started before a tempo increase lasts longer than the current tempo says, so it's
				// timed at the slowest tempo across the beats a still-open note's late window can cover
				var rate = clock.PlaybackRate;
				var tempoMap = timeline.TempoMap;
				var songTime = clock.SongTime;
				var from = tempoMap.BeatAtTime(songTime - (missDelay + LateArrival + LookBehindMargin) * rate) - late;
				var to = tempoMap.BeatAtTime(songTime);
				seconds = late * 60 / SlowestBpm(tempoMap, from, to) / rate;
			}
			return seconds + missDelay + LateArrival + LookBehindMargin;
		}

		// Tempo is linear or constant between tempo points, so its minimum is at a point or an end
		static double SlowestBpm(TempoMap tempoMap, double fromBeat, double toBeat) {
			var slowest = Math.Min(tempoMap.BpmAtBeat(fromBeat), tempoMap.BpmAtBeat(toBeat));
			var points = tempoMap.TempoPoints;
			for (var i = 0; i < points.Count; i++) {
				// Tempo points are placed on the swung beat axis
				var beat = tempoMap.Unswing(points[i].beat);
				if (beat > fromBeat && beat < toBeat) slowest = Math.Min(slowest, tempoMap.BpmAtBeat(beat));
			}
			return slowest;
		}

		// After a seek back: notes that can be played again become open, even if they were hit or missed before
		void Reopen(double now) {
			var active = notes.ActiveNotes;
			for (var i = 0; i < active.Count; i++) {
				var note = active[i];
				if (Deadline(note) > now) states.Remove(note.id);
			}
		}

		// After a seek forward: notes the song landed beyond were never playable, so they're skipped, not missed
		void SkipUnreachable(double now) {
			var active = notes.ActiveNotes;
			for (var i = 0; i < active.Count; i++) {
				var note = active[i];
				if (!states.ContainsKey(note.id) && now > Deadline(note)) states[note.id] = State.Skipped;
			}
		}

		// On resume without a seek: open notes already past their deadline at the pause point were never playable
		void SkipLeftBehind(double pausedAt, double rate) {
			var active = notes.ActiveNotes;
			for (var i = 0; i < active.Count; i++) {
				var note = active[i];
				if (!states.ContainsKey(note.id) && pausedAt > SongTimeDeadline(note, rate)) states[note.id] = State.Skipped;
			}
		}

		// On the frame that pauses: notes whose deadline playback passed before the pause were missed, even though the
		// pause makes every DSP deadline infinite. Notes that entered this frame wait for the resume check instead.
		void MissPassedBefore(double pausedAt, double rate) {
			var active = notes.ActiveNotes;
			for (var i = 0; i < active.Count; i++) {
				var note = active[i];
				if (states.ContainsKey(note.id) || enteredWhilePaused.Contains(note.id)) continue;
				if (pausedAt > SongTimeDeadline(note, rate)) {
					states[note.id] = State.Missed;
					Missed?.Invoke(note);
				}
			}
		}

		void OnNoteEntered(NoteInstance note) {
			// A note that arrives already too late (e.g. added behind the playhead) was never playable either. While
			// paused its deadline is infinite, so it's checked on resume.
			if (!timeline.Clock.IsPlaying) enteredWhilePaused.Add(note.id);
			else if (timeline.Clock.DspTime > Deadline(note)) states[note.id] = State.Skipped;
		}

		void OnNoteExited(NoteInstance note, NoteExitReason reason) {
			var wasOpen = !states.TryGetValue(note.id, out var state);
			states.Remove(note.id);
			enteredWhilePaused.Remove(note.id);
			// Song time only moves while paused through a seek or a tempo edit, so the note was skipped, not played. The
			// frame that pauses is the exception: it can have played up to the pause, and a seek in it isn't Passed.
			if (!timeline.Clock.IsPlaying && !wasPlaying) return;
			// Normally caught by the deadline first; this covers a note leaving before Update saw it pass
			if (reason == NoteExitReason.Passed && wasOpen) Missed?.Invoke(note);
			else if (reason == NoteExitReason.Passed && state == State.Holding) HoldCompleted?.Invoke(note);
		}
	}
}
