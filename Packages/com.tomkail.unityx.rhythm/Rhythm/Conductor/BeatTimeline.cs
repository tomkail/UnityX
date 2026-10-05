using System;

namespace UnityX.Rhythm {
	// An IBeatTimeline without a MonoBehaviour, e.g. for tests or for driving a clock yourself.
	// The caller ticks the clock.
	public sealed class BeatTimeline : IBeatTimeline, IDisposable {
		TempoMap tempoMap;

		public BeatTimeline(IRhythmClock clock, TempoMap tempoMap) {
			Clock = clock ?? throw new ArgumentNullException(nameof(clock));
			this.tempoMap = tempoMap ?? throw new ArgumentNullException(nameof(tempoMap));
			Clock.TimelineChanged += RaiseTimelineChanged;
			this.tempoMap.Changed += RaiseTimelineChanged;
		}

		public IRhythmClock Clock { get; }

		public TempoMap TempoMap {
			get => tempoMap;
			set {
				if (value == null) throw new ArgumentNullException(nameof(value));
				tempoMap.Changed -= RaiseTimelineChanged;
				tempoMap = value;
				tempoMap.Changed += RaiseTimelineChanged;
				RaiseTimelineChanged();
			}
		}

		public event Action TimelineChanged;

		public void Dispose() {
			Clock.TimelineChanged -= RaiseTimelineChanged;
			tempoMap.Changed -= RaiseTimelineChanged;
		}

		void RaiseTimelineChanged() => TimelineChanged?.Invoke();
	}
}
