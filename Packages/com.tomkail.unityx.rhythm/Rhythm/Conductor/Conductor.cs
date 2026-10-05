using System;
using UnityEngine;

namespace UnityX.Rhythm {
	// Pairs a clock with a tempo map: where we are in the music this frame, and beat/bar/subdivision events.
	// Runs early in the frame so everything else reads this frame's position.
	[DefaultExecutionOrder(-1000)]
	public class Conductor : MonoBehaviour {
		public enum SmoothingMode {
			OffsetTracking,
			Regression,
			Raw
		}

		[SerializeField] TempoMap tempoMap = new TempoMap();
		[SerializeField] RhythmLatency latency;
		public SmoothingMode smoothing = SmoothingMode.OffsetTracking;
		public bool playOnStart = true;
		[Tooltip("Seconds before beat 0 when playing on start")]
		public double leadIn = 1;
		[Tooltip("Subdivision events per beat. 0 turns them off.")]
		[Min(0)] public int subdivisionsPerBeat;
		[Tooltip("How early the Scheduled events fire, in seconds, so audio can be queued sample-accurately")]
		public double lookAheadTime = 0.1;

		public AudioRhythmClock Clock { get; private set; }
		public RhythmLatency Latency { get => latency; set => latency = value; }
		public double SongTime => Clock.SongTime;
		// This frame's position in authored beats
		public double Beat { get; private set; }
		public BarPosition BarPosition { get; private set; }

		public TempoMap TempoMap {
			get => tempoMap;
			set => tempoMap = value ?? throw new ArgumentNullException(nameof(value));
		}

		public event Action<BeatEvent> BeatCrossed;
		public event Action<BeatEvent> BarCrossed;
		public event Action<BeatEvent> SubdivisionCrossed;
		// Fire lookAheadTime early, carrying the exact dsp time, for scheduling audio
		public event Action<BeatEvent> BeatScheduled;
		public event Action<BeatEvent> BarScheduled;

		readonly BeatEventTracker beatTracker = new(1);
		readonly BeatEventTracker barTracker = new(1);
		readonly BeatEventTracker subdivisionTracker = new(1);
		readonly BeatEventTracker scheduledBeatTracker = new(1);
		readonly BeatEventTracker scheduledBarTracker = new(1);

		// Called automatically in Awake with Unity's clocks. Call it yourself first to use another time source (e.g. in tests).
		public void Initialize(IAudioTimeSource timeSource = null) {
			if (Clock != null) Clock.TimelineChanged -= OnTimelineChanged;
			Clock = new AudioRhythmClock(timeSource ?? new UnityAudioTimeSource(), CreateSmoother());
			Clock.TimelineChanged += OnTimelineChanged;
		}

		void Awake() {
			if (Clock == null) Initialize();
		}

		void Start() {
			if (playOnStart && !Clock.IsPlaying) Clock.Play(-leadIn);
		}

		void Update() => Tick();

		// Advances the clock and raises this frame's events. Update calls it; tests call it directly.
		public void Tick() {
			Clock.Tick();
			Beat = tempoMap.BeatAtTime(Clock.SongTime);
			BarPosition = tempoMap.BarAtBeat(Beat);
			if (!Clock.IsPlaying) return;

			beatTracker.Advance(Beat, index => BeatCrossed?.Invoke(CreateEvent(index, index)));
			barTracker.Advance(BarPosition.bar, index => BarCrossed?.Invoke(CreateEvent(index, tempoMap.BeatAtBar((int)index))));
			if (subdivisionsPerBeat > 0) {
				subdivisionTracker.interval = 1.0 / subdivisionsPerBeat;
				subdivisionTracker.Advance(Beat, index => SubdivisionCrossed?.Invoke(CreateEvent(index, index * subdivisionTracker.interval)));
			}

			var aheadBeat = BeatAtDspTime(Clock.DspTime + lookAheadTime);
			scheduledBeatTracker.Advance(aheadBeat, index => BeatScheduled?.Invoke(CreateEvent(index, index)));
			scheduledBarTracker.Advance(tempoMap.BarAtBeat(aheadBeat).bar, index => BarScheduled?.Invoke(CreateEvent(index, tempoMap.BeatAtBar((int)index))));
		}

		public double DspTimeAtBeat(double beat) => Clock.DspTimeAtSongTime(tempoMap.TimeAtBeat(beat));
		public double BeatAtDspTime(double dspTime) => tempoMap.BeatAtTime(Clock.SongTimeAtDspTime(dspTime));

		// The beat being heard now, allowing for audio output latency. Use this for visuals.
		public double AudibleBeat => BeatAtDspTime(Clock.DspTime - (latency != null ? latency.audioOutputLatency : 0));

		// How close a moment is to the beat grid, e.g. for "move on the beat" checks. gridInterval is in beats.
		public BeatPhase GetBeatPhase(double dspTime, double gridInterval = 1) {
			var beat = BeatAtDspTime(dspTime);
			var nearest = Math.Round(beat / gridInterval) * gridInterval;
			return new BeatPhase { nearestBeat = nearest, beatOffset = beat - nearest, timeOffset = dspTime - DspTimeAtBeat(nearest) };
		}

		BeatEvent CreateEvent(long index, double beat) {
			return new BeatEvent { index = index, beat = beat, dspTime = DspTimeAtBeat(beat), bar = tempoMap.BarAtBeat(beat) };
		}

		// Play and seek move the song position, so record the new position without reporting what was skipped
		void OnTimelineChanged() {
			var beat = tempoMap.BeatAtTime(Clock.SongTime);
			var aheadBeat = Clock.IsPlaying ? BeatAtDspTime(Clock.DspTime + lookAheadTime) : beat;
			Prime(beatTracker, beat);
			Prime(barTracker, tempoMap.BarAtBeat(beat).bar);
			subdivisionTracker.interval = subdivisionsPerBeat > 0 ? 1.0 / subdivisionsPerBeat : 1;
			Prime(subdivisionTracker, beat);
			Prime(scheduledBeatTracker, aheadBeat);
			Prime(scheduledBarTracker, tempoMap.BarAtBeat(aheadBeat).bar);
		}

		static void Prime(BeatEventTracker tracker, double position) {
			tracker.Reset();
			tracker.Advance(position, _ => {});
		}

		IClockSmoother CreateSmoother() {
			return smoothing switch {
				SmoothingMode.Raw => new RawClockSmoother(),
				SmoothingMode.Regression => new RegressionClockSmoother(),
				_ => new OffsetTrackingClockSmoother()
			};
		}
	}
}
