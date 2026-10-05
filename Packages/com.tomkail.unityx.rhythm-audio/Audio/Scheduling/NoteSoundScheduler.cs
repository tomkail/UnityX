using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	// Queues each note's sound sample-accurately, a short time ahead of the note.
	// - Re-times sounds that haven't started yet when the timeline changes (tempo, rate, seek).
	// - Cancels a sound that hasn't started when a pause or seek means it now shouldn't play.
	// - A sound that has started rings on when its note scrolls past or the song jumps forward, and is cut when its
	//   note is removed from the source or the song jumps back past it. A short jump back that leaves the note ahead
	//   lets it play again over the old sound.
	// Call Update once per frame, after the clock has ticked. NoteAudioScheduler does this for you.
	public sealed class NoteSoundScheduler : IDisposable {
		readonly IBeatTimeline timeline;
		readonly IVoicePlayer player;
		readonly NoteScheduler notes;
		readonly Dictionary<NoteId, IVoice> voices = new();
		// Sounds still ringing after their note passed, so StopAll can reach them
		readonly List<IVoice> ringing = new();
		// Notes already voiced, or skipped because they came into reach too late, so they aren't played twice
		readonly HashSet<NoteId> handled = new();
		readonly List<NoteId> scratch = new();
		bool timelineChanged;

		// How late a sound may still start, in seconds. Later than this it's skipped rather than played out of time.
		public double lateTolerance = 0.02;

		public NoteSoundScheduler(IBeatTimeline timeline, IVoicePlayer player, INoteSource source = null, double lookAhead = 0.2) {
			this.timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
			this.player = player ?? throw new ArgumentNullException(nameof(player));
			notes = new NoteScheduler(timeline, source) { lookBehind = 0.1, lookAhead = lookAhead };
			notes.NoteExited += OnNoteExited;
			timeline.TimelineChanged += OnTimelineChanged;
		}

		public INoteSource Source { get => notes.Source; set => notes.Source = value; }

		// How far ahead sounds are queued, in seconds. Long enough to cover a slow frame.
		public double LookAhead { get => notes.lookAhead; set => notes.lookAhead = value; }

		// The scheduler's own short window, e.g. to see what's about to sound
		public NoteScheduler Notes => notes;

		public int VoiceCount => voices.Count;

		public void Update() {
			notes.Update();
			var now = timeline.Clock.DspTime;
			if (timelineChanged) {
				timelineChanged = false;
				Retime(now);
			}
			if (timeline.Clock.IsPlaying) QueueNewNotes(now);
			ForgetFinishedVoices(now);
		}

		public void Dispose() {
			notes.NoteExited -= OnNoteExited;
			timeline.TimelineChanged -= OnTimelineChanged;
			StopAll();
		}

		// Stops every sound, including ones already playing
		public void StopAll() {
			foreach (var voice in voices.Values) voice.Stop();
			foreach (var voice in ringing) voice.Stop();
			voices.Clear();
			ringing.Clear();
			handled.Clear();
		}

		void QueueNewNotes(double now) {
			foreach (var note in notes.ActiveNotes) {
				if (handled.Contains(note.id)) continue;
				var dspTime = timeline.DspTimeAtBeat(note.Beat);
				if (dspTime > now + notes.lookAhead) continue;
				handled.Add(note.id);
				if (dspTime < now - lateTolerance) continue;
				var voice = player.Play(note, dspTime);
				if (voice != null) voices[note.id] = voice;
			}
		}

		void Retime(double now) {
			var playing = timeline.Clock.IsPlaying;
			scratch.Clear();
			scratch.AddRange(voices.Keys);
			foreach (var id in scratch) {
				var voice = voices[id];
				var dspTime = playing && notes.TryGetNote(id, out var note) ? timeline.DspTimeAtBeat(note.Beat) : double.NaN;
				if (voice.StartDspTime <= now) {
					// Already sounding but the note is ahead again, e.g. after a short seek back: let it ring and queue the note again
					if (dspTime > now) {
						voices.Remove(id);
						ringing.Add(voice);
					}
					continue;
				}
				if (dspTime >= now - lateTolerance) {
					voice.Reschedule(dspTime);
				} else {
					voice.Stop();
					voices.Remove(id);
				}
			}
			// Anything without a sound gets another chance, e.g. after resuming or seeking back
			handled.RemoveWhere(id => !voices.ContainsKey(id));
		}

		void ForgetFinishedVoices(double now) {
			scratch.Clear();
			foreach (var pair in voices) {
				if (pair.Value.IsFinished(now)) scratch.Add(pair.Key);
			}
			foreach (var id in scratch) voices.Remove(id);
			ringing.RemoveAll(voice => voice.IsFinished(now));
		}

		void OnNoteExited(NoteInstance note, NoteExitReason reason) {
			handled.Remove(note.id);
			if (!voices.TryGetValue(note.id, out var voice)) return;
			voices.Remove(note.id);
			if (reason == NoteExitReason.Removed || voice.StartDspTime > timeline.Clock.DspTime) voice.Stop();
			else ringing.Add(voice);
		}

		void OnTimelineChanged() => timelineChanged = true;
	}
}
