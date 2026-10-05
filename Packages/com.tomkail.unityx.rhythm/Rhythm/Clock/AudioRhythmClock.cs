using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	// The standard clock: song time is a piecewise-linear function of the (smoothed) audio clock.
	// Call Tick() once per frame before reading it. Conductor does this for you.
	public sealed class AudioRhythmClock : IRhythmClock {
		// From dspStart onwards, song time = songStart + (dsp - dspStart) * rate
		struct Segment {
			public double dspStart;
			public double songStart;
			public double rate;
		}

		readonly IAudioTimeSource timeSource;
		readonly IClockSmoother smoother;
		// Sorted by dspStart. Later segments are rate changes scheduled for the future.
		readonly List<Segment> segments = new();
		bool hasTicked;
		double lastRealtime;
		double pausedSongTime;
		double pausedRate = 1;

		public AudioRhythmClock(IAudioTimeSource timeSource, IClockSmoother smoother = null) {
			this.timeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
			this.smoother = smoother ?? new OffsetTrackingClockSmoother();
		}

		public event Action TimelineChanged;

		public bool IsPlaying { get; private set; }
		public double DspTime { get; private set; }
		public bool IsAudioStalled => smoother.IsStalled;
		public IClockSmoother Smoother => smoother;

		public double SongTime => IsPlaying ? SongTimeAtDspTime(DspTime) : pausedSongTime;
		public double PlaybackRate => IsPlaying ? SegmentAtDsp(DspTime).rate : pausedRate;

		public void Tick() {
			var realtime = timeSource.Realtime;
			DspTime = smoother.Update(realtime, timeSource.DspTime, timeSource.BufferDuration);
			// The audio device restarted on a new timeline: move the anchors with it so song time carries on
			var discontinuity = smoother.LastDiscontinuity;
			if (discontinuity != 0) {
				for (var i = 0; i < segments.Count; i++) {
					var segment = segments[i];
					segment.dspStart += discontinuity;
					segments[i] = segment;
				}
			}
			lastRealtime = realtime;
			hasTicked = true;
			// Keep a few seconds of history so slightly old input timestamps still map correctly
			while (segments.Count > 1 && segments[1].dspStart < DspTime - 5) segments.RemoveAt(0);
		}

		// Starts playing so that songTime is reached at atDspTime (default: now). Play(-2) gives a two second lead-in.
		public void Play(double songTime = 0, double? atDspTime = null) {
			EnsureTicked();
			var rate = IsPlaying ? PlaybackRate : pausedRate;
			segments.Clear();
			segments.Add(new Segment { dspStart = atDspTime ?? DspTime, songStart = songTime, rate = rate });
			IsPlaying = true;
			TimelineChanged?.Invoke();
		}

		public void Pause() {
			if (!IsPlaying) return;
			pausedSongTime = SongTime;
			pausedRate = PlaybackRate;
			IsPlaying = false;
			segments.Clear();
			TimelineChanged?.Invoke();
		}

		public void Resume(double? atDspTime = null) {
			if (IsPlaying) return;
			Play(pausedSongTime, atDspTime);
		}

		public void Seek(double songTime) {
			if (IsPlaying) {
				Play(songTime);
			} else {
				pausedSongTime = songTime;
				TimelineChanged?.Invoke();
			}
		}

		// Changes speed at atDspTime (default: now) without the song position jumping. Replaces any later rate changes.
		public void SetPlaybackRate(double rate, double? atDspTime = null) {
			if (rate <= 0) throw new ArgumentOutOfRangeException(nameof(rate), "Playback rate must be positive");
			if (!IsPlaying) {
				pausedRate = rate;
				TimelineChanged?.Invoke();
				return;
			}
			var at = Math.Max(atDspTime ?? DspTime, segments[0].dspStart);
			var songAt = SongTimeAtDspTime(at);
			segments.RemoveAll(s => s.dspStart >= at);
			segments.Add(new Segment { dspStart = at, songStart = songAt, rate = rate });
			TimelineChanged?.Invoke();
		}

		public double SongTimeAtDspTime(double dspTime) {
			if (!IsPlaying) return pausedSongTime;
			var segment = SegmentAtDsp(dspTime);
			return segment.songStart + (dspTime - segment.dspStart) * segment.rate;
		}

		// Infinity while paused: nothing is going to happen
		public double DspTimeAtSongTime(double songTime) {
			if (!IsPlaying) return double.PositiveInfinity;
			var i = 0;
			while (i + 1 < segments.Count && segments[i + 1].songStart <= songTime) i++;
			var segment = segments[i];
			return segment.dspStart + (songTime - segment.songStart) / segment.rate;
		}

		public double RealtimeToDspTime(double realtime) {
			EnsureTicked();
			return realtime + (DspTime - lastRealtime);
		}

		Segment SegmentAtDsp(double dspTime) {
			var i = 0;
			while (i + 1 < segments.Count && segments[i + 1].dspStart <= dspTime) i++;
			return segments[i];
		}

		void EnsureTicked() {
			if (!hasTicked) Tick();
		}
	}
}
