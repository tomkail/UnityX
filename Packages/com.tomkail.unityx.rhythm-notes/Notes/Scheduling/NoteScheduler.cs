using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	public enum NoteExitReason {
		// The note is behind the window: it has been and gone
		Passed,
		// Anything else: the source no longer has it, or a seek moved the window away from it
		Removed
	}

	// Keeps the notes near now: from lookBehind seconds ago to lookAhead seconds ahead, in playback time, so at half
	// speed the window covers half as many beats. Compares notes by NoteId, so tempo, rate, swing and seek changes only
	// move notes in and out when they really cross the window. Call Update once per frame, after the clock has ticked.
	public sealed class NoteScheduler {
		readonly IBeatTimeline timeline;
		List<NoteInstance> active = new();
		List<NoteInstance> next = new();
		HashSet<NoteId> activeIds = new();
		HashSet<NoteId> nextIds = new();
		readonly List<NoteInstance> found = new();
		readonly List<NoteInstance> exited = new();
		readonly List<NoteInstance> entered = new();

		public double lookBehind = 0.25;
		public double lookAhead = 2;

		public NoteScheduler(IBeatTimeline timeline, INoteSource source = null) {
			this.timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
			Source = source;
		}

		// Changing the source takes effect on the next Update
		public INoteSource Source { get; set; }

		// In beat order, then lane order
		public IReadOnlyList<NoteInstance> ActiveNotes => active;
		public double WindowStartBeat { get; private set; }
		public double WindowEndBeat { get; private set; }

		// Raised during Update, after ActiveNotes has been updated
		public event Action<NoteInstance> NoteEntered;
		public event Action<NoteInstance, NoteExitReason> NoteExited;

		public bool IsActive(NoteId id) => activeIds.Contains(id);

		public bool TryGetNote(NoteId id, out NoteInstance note) {
			if (activeIds.Contains(id)) {
				foreach (var candidate in active) {
					if (candidate.id == id) {
						note = candidate;
						return true;
					}
				}
			}
			note = default;
			return false;
		}

		// Infinity while paused
		public double DspTimeOf(NoteInstance note) => timeline.DspTimeAtBeat(note.Beat);

		public void Update() {
			var clock = timeline.Clock;
			var tempoMap = timeline.TempoMap;
			var rate = clock.PlaybackRate;
			var songTime = clock.SongTime;
			WindowStartBeat = tempoMap.BeatAtTime(songTime - lookBehind * rate);
			WindowEndBeat = tempoMap.BeatAtTime(songTime + lookAhead * rate);

			found.Clear();
			Source?.GetNotes(WindowStartBeat, WindowEndBeat, found);
			next.Clear();
			nextIds.Clear();
			foreach (var note in found) {
				if (nextIds.Add(note.id)) next.Add(note);
			}
			next.Sort(CompareNotes);

			exited.Clear();
			foreach (var note in active) {
				if (!nextIds.Contains(note.id)) exited.Add(note);
			}
			entered.Clear();
			foreach (var note in next) {
				if (!activeIds.Contains(note.id)) entered.Add(note);
			}

			(active, next) = (next, active);
			(activeIds, nextIds) = (nextIds, activeIds);

			foreach (var note in exited) NoteExited?.Invoke(note, note.EndBeat < WindowStartBeat ? NoteExitReason.Passed : NoteExitReason.Removed);
			foreach (var note in entered) NoteEntered?.Invoke(note);
		}

		static int CompareNotes(NoteInstance a, NoteInstance b) {
			var byBeat = a.Beat.CompareTo(b.Beat);
			if (byBeat != 0) return byBeat;
			var byLane = a.note.lane.CompareTo(b.note.lane);
			return byLane != 0 ? byLane : a.id.source.CompareTo(b.id.source);
		}
	}
}
