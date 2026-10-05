using System;
using UnityEngine;

namespace UnityX.Rhythm {
	// Plays this GameObject's AudioSource clip in step with a Conductor: starts it on play, follows pauses, seeks and
	// playback rate (as pitch, see BackingTrackSync), and re-syncs if it drifts. Rate changes reach the clip within a
	// frame rather than sample-accurately, by changing pitch in place without a restart. Unity caps pitch at 3, so a
	// rate above 3 can't be followed: the clip falls behind and restarts each time it drifts too far. The clip
	// shouldn't loop.
	[DefaultExecutionOrder(-900)]
	[RequireComponent(typeof(AudioSource))]
	public class BackingTrack : MonoBehaviour {
		public Conductor conductor;
		[Tooltip("The song time, in seconds, at which the clip's first sample plays")]
		public double clipStartSongTime;
		[Tooltip("Re-sync when the clip is further than this from the clock, in seconds. Never less than one and a half audio buffers.")]
		public double resyncThreshold = 0.03;
		[Tooltip("How far ahead a start is scheduled, so it lands sample-accurately")]
		public double startDelay = 0.05;

		AudioSource audioSource;
		Conductor watched;
		// Unity's AudioSource.pitch range
		const float MaxPitch = 3;

		bool needsResync = true;
		double scheduledDspTime;
		double bufferThreshold = -1;

		// How many times the clip has been (re)started, e.g. to spot a track that keeps losing sync
		public int ResyncCount { get; private set; }

		public AudioSource AudioSource => audioSource != null ? audioSource : audioSource = GetComponent<AudioSource>();

		void OnEnable() => needsResync = true;

		void OnDisable() {
			Watch(null);
			AudioSource.Stop();
		}

		void Update() => Tick();

		// Update calls this; tests call it directly
		public void Tick() {
			var clip = AudioSource.clip;
			if (conductor == null || conductor.Clock == null || clip == null) return;
			Watch(conductor);
			var clock = conductor.Clock;
			if (!clock.IsPlaying) {
				if (AudioSource.isPlaying) AudioSource.Stop();
				needsResync = true;
				return;
			}
			var pitch = TargetPitch(clock);
			var pitchWrong = Math.Abs(AudioSource.pitch - pitch) > 1e-4f;
			// Once it has been playing a moment, its position can be checked against the clock
			var settled = AudioSource.isPlaying && clock.DspTime > scheduledDspTime + 0.1;
			if (needsResync || pitchWrong) {
				// TimelineChanged carries no payload. A rate change leaves the clip where the clock says it should be, so
				// only the pitch has to follow, and the drift check mops up the frame it took; a seek or play doesn't.
				if (settled && IsInStep(clock, clip)) {
					AudioSource.pitch = pitch;
					needsResync = false;
				} else {
					needsResync = true;
				}
			} else if (settled && !IsInStep(clock, clip)) {
				needsResync = true;
			}
			if (needsResync) Resync(clip);
		}

		static float TargetPitch(IRhythmClock clock) => Mathf.Min((float)clock.PlaybackRate, MaxPitch);

		bool IsInStep(IRhythmClock clock, AudioClip clip) {
			var drift = BackingTrackSync.Drift(clock, clipStartSongTime, (double)AudioSource.timeSamples / clip.frequency, AudioSettings.dspTime);
			return Math.Abs(drift) <= ResyncThreshold;
		}

		// The clip position and dspTime both move in whole audio buffers, so with a large buffer they can disagree by
		// more than resyncThreshold while perfectly in step, and every false resync leaves a gap
		double ResyncThreshold {
			get {
				if (bufferThreshold < 0) {
					AudioSettings.GetDSPBufferSize(out var bufferLength, out _);
					bufferThreshold = 1.5 * bufferLength / AudioSettings.outputSampleRate;
				}
				return Math.Max(resyncThreshold, bufferThreshold);
			}
		}

		void Resync(AudioClip clip) {
			needsResync = false;
			AudioSource.Stop();
			var start = BackingTrackSync.Resync(conductor.Clock, clipStartSongTime, (double)clip.samples / clip.frequency, startDelay);
			if (!start.play) return;
			AudioSource.pitch = TargetPitch(conductor.Clock);
			AudioSource.timeSamples = Math.Min(clip.samples - 1, (int)Math.Round(start.clipTime * clip.frequency));
			AudioSource.PlayScheduled(start.dspTime);
			scheduledDspTime = start.dspTime;
			ResyncCount++;
		}

		void Watch(Conductor target) {
			if (watched == target) return;
			if (watched != null) watched.TimelineChanged -= OnTimelineChanged;
			watched = target;
			if (watched != null) watched.TimelineChanged += OnTimelineChanged;
		}

		void OnTimelineChanged() => needsResync = true;
	}
}
