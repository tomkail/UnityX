using System;
using UnityEngine;

namespace UnityX.Rhythm {
	// Plays this GameObject's AudioSource clip in step with a Conductor: starts it on play, follows pauses, seeks and
	// playback rate (as pitch, see BackingTrackSync), and re-syncs if it drifts. Rate changes reach the clip within a
	// frame rather than sample-accurately. The clip shouldn't loop.
	[DefaultExecutionOrder(-900)]
	[RequireComponent(typeof(AudioSource))]
	public class BackingTrack : MonoBehaviour {
		public Conductor conductor;
		[Tooltip("The song time, in seconds, at which the clip's first sample plays")]
		public double clipStartSongTime;
		[Tooltip("Re-sync when the clip is further than this from the clock, in seconds")]
		public double resyncThreshold = 0.03;
		[Tooltip("How far ahead a start is scheduled, so it lands sample-accurately")]
		public double startDelay = 0.05;

		AudioSource audioSource;
		Conductor watched;
		bool needsResync = true;
		double scheduledDspTime;

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
			if (AudioSource.pitch != (float)clock.PlaybackRate) needsResync = true;
			// Once it has been playing a moment, check it against the clock. Both move in whole audio buffers.
			if (!needsResync && AudioSource.isPlaying && clock.DspTime > scheduledDspTime + 0.1) {
				var dspTime = AudioSettings.dspTime;
				var drift = BackingTrackSync.Drift(clock, clipStartSongTime, (double)AudioSource.timeSamples / clip.frequency, dspTime);
				if (Math.Abs(drift) > resyncThreshold) needsResync = true;
			}
			if (needsResync) Resync(clip);
		}

		void Resync(AudioClip clip) {
			needsResync = false;
			AudioSource.Stop();
			var start = BackingTrackSync.Resync(conductor.Clock, clipStartSongTime, (double)clip.samples / clip.frequency, startDelay);
			if (!start.play) return;
			AudioSource.pitch = (float)start.pitch;
			AudioSource.timeSamples = Math.Min(clip.samples - 1, (int)Math.Round(start.clipTime * clip.frequency));
			AudioSource.PlayScheduled(start.dspTime);
			scheduledDspTime = start.dspTime;
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
