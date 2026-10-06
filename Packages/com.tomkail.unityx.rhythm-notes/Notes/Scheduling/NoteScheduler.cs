using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	public enum NoteExitReason {
		// The note is behind the window: it has been and gone. A hitch counts: the song kept playing.
		Passed,
		// The source no longer has it, or a seek back moved the window away from it
		Removed,
		// A seek forward jumped the window past it, so it was never played
		Skipped
	}

	// Keeps the notes near now: from lookBehind seconds ago to lookAhead seconds ahead, in playback time, so at half
	// speed the window covers half as many beats. Compares notes by NoteId, so tempo, rate, swing and seek changes only
	// move notes in and out when they really cross the window.
	// Call Update once per frame, after the clock has ticked, including while paused: seeks are measured between
	// Updates, so a skipped frame makes them harder to tell from playback.
	// Something that depends on seeing every Update, such as a Judge, can Claim the scheduler. Only its owner can then
	// update it; everything else should only read ActiveNotes and listen to the events.
	public sealed class NoteScheduler {
		readonly IBeatTimeline timeline;
		List<NoteInstance> active = new();
		List<NoteInstance> next = new();
		HashSet<NoteId> activeIds = new();
		HashSet<NoteId> nextIds = new();
		readonly List<NoteInstance> found = new();
		readonly List<NoteInstance> exited = new();
		readonly List<NoteInstance> entered = new();
		static readonly Comparison<NoteInstance> compareNotes = CompareNotes;
		bool hasLast;
		bool wasPlaying;
		double lastSongTime;
		double lastDspTime;
		double lastRate;

		public double lookBehind = 0.25;
		public double lookAhead = 2;
		// How far song time must move from where playback would have taken it to count as a seek, in seconds
		public double seekThreshold = 0.05;

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
		// How far song time jumped in the last Update beyond normal playback, in seconds of song: positive for a seek
		// forward, negative for a seek back, 0 otherwise. A Play or Resume that starts behind the paused song time (a
		// lead-in) is negative too: song time really did move back.
		public double LastJump { get; private set; }

		// Raised during Update, after ActiveNotes has been updated
		public event Action<NoteInstance> NoteEntered;
		public event Action<NoteInstance, NoteExitReason> NoteExited;

		// Whatever has claimed the scheduler, or null
		public object Owner { get; private set; }

		public bool IsActive(NoteId id) => activeIds.Contains(id);

		public void Claim(object owner) {
			if (owner == null) throw new ArgumentNullException(nameof(owner));
			if (Owner != null && !ReferenceEquals(Owner, owner)) throw new InvalidOperationException($"This NoteScheduler is already owned by a {Owner.GetType().Name}");
			Owner = owner;
		}

		public void Release(object owner) {
			if (ReferenceEquals(Owner, owner)) Owner = null;
		}

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
		public double EndDspTimeOf(NoteInstance note) => timeline.DspTimeAtBeat(note.EndBeat);

		public void Update() => Update(null);

		// caller must be the Owner, if there is one
		public void Update(object caller) {
			if (Owner != null && !ReferenceEquals(caller, Owner)) {
				throw new InvalidOperationException($"This NoteScheduler is owned by its {Owner.GetType().Name}, which updates it. Views should only read ActiveNotes and listen to NoteEntered and NoteExited.");
			}
			var clock = timeline.Clock;
			var tempoMap = timeline.TempoMap;
			var rate = clock.PlaybackRate;
			var songTime = clock.SongTime;
			var dspTime = clock.DspTime;
			var isPlaying = clock.IsPlaying;
			// The range playback alone could have taken song time to since the last Update. A hitch moves both, a seek
			// only one. It's a range because a pause, resume or rate change can fall anywhere in the frame.
			double minSongTime, maxSongTime;
			// DSP time going back means the audio device restarted and the clock carried song time across it. With
			// nothing to measure against, a seek made in that same frame isn't seen either.
			if (!hasLast || dspTime < lastDspTime) {
				minSongTime = maxSongTime = songTime;
			} else {
				var elapsed = dspTime - lastDspTime;
				minSongTime = maxSongTime = lastSongTime;
				if (wasPlaying && isPlaying) {
					minSongTime += elapsed * Math.Min(lastRate, rate);
					maxSongTime += elapsed * Math.Max(lastRate, rate);
				} else if (wasPlaying || isPlaying) {
					// Played for anything from none of the frame to all of it
					maxSongTime += elapsed * (wasPlaying ? lastRate : rate);
				}
			}
			LastJump = songTime > maxSongTime + seekThreshold ? songTime - maxSongTime : songTime < minSongTime - seekThreshold ? songTime - minSongTime : 0;
			// The nearest allowed song time, so a note that would have passed under any allowed advance still passes
			var expectedSongTime = Math.Clamp(songTime, minSongTime, maxSongTime);
			var expectedStartBeat = tempoMap.BeatAtTime(expectedSongTime - lookBehind * rate);
			WindowStartBeat = tempoMap.BeatAtTime(songTime - lookBehind * rate);
			WindowEndBeat = tempoMap.BeatAtTime(songTime + lookAhead * rate);
			hasLast = true;
			wasPlaying = isPlaying;
			lastSongTime = songTime;
			lastDspTime = dspTime;
			lastRate = rate;

			found.Clear();
			Source?.GetNotes(WindowStartBeat, WindowEndBeat, found);
			next.Clear();
			nextIds.Clear();
			foreach (var note in found) {
				if (nextIds.Add(note.id)) next.Add(note);
			}
			next.Sort(compareNotes);

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

			foreach (var note in exited) NoteExited?.Invoke(note, ExitReason(note, expectedStartBeat));
			foreach (var note in entered) NoteEntered?.Invoke(note);
		}

		NoteExitReason ExitReason(NoteInstance note, double expectedStartBeat) {
			if (note.EndBeat >= WindowStartBeat) return NoteExitReason.Removed;
			// Still inside where the window would have been without the jump: the seek carried it away
			return LastJump > 0 && note.EndBeat >= expectedStartBeat ? NoteExitReason.Skipped : NoteExitReason.Passed;
		}

		static int CompareNotes(NoteInstance a, NoteInstance b) {
			var byBeat = a.Beat.CompareTo(b.Beat);
			if (byBeat != 0) return byBeat;
			var byLane = a.note.lane.CompareTo(b.note.lane);
			return byLane != 0 ? byLane : a.id.source.CompareTo(b.id.source);
		}
	}
}
