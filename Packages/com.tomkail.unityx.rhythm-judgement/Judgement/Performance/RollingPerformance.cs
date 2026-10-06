using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	// How the player is doing lately: hit rate, mean accuracy and early/late bias over the last few seconds or bars,
	// overall and per lane. For dynamic difficulty and feedback; what to do with it is up to the game.
	public sealed class RollingPerformance : IDisposable {
		public enum Unit {
			Seconds,
			Bars
		}

		public struct Stats {
			public int hits;
			public int misses;
			// Mean Judgement.accuracy of the hits
			public double meanAccuracy;
			// Mean signed time offset of the hits in seconds: negative means early
			public double meanOffset;

			public int Count => hits + misses;
			// 0 when nothing has happened yet
			public double HitRate => Count > 0 ? (double)hits / Count : 0;
		}

		struct Entry {
			public double dspTime;
			public int lane;
			public bool hit;
			public double accuracy;
			public double offset;
		}

		readonly IBeatTimeline timeline;
		// Misses are timed at their notes, so entries aren't strictly in time order
		readonly List<Entry> entries = new();
		double lastWindowStart = double.NegativeInfinity;
		Judge judge;

		public double window;
		public Unit unit;

		// timeline is needed to measure the window in bars, and to time misses
		public RollingPerformance(IBeatTimeline timeline, double window, Unit unit = Unit.Seconds) {
			this.timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
			if (!(window > 0)) throw new ArgumentOutOfRangeException(nameof(window), "Window must be positive");
			this.window = window;
			this.unit = unit;
		}

		public void Watch(Judge judgeToWatch) {
			Unwatch();
			judge = judgeToWatch ?? throw new ArgumentNullException(nameof(judgeToWatch));
			judge.Judged += AddHit;
			judge.Missed += AddMiss;
		}

		public void Unwatch() {
			if (judge == null) return;
			judge.Judged -= AddHit;
			judge.Missed -= AddMiss;
			judge = null;
		}

		public void Dispose() => Unwatch();

		public void AddHit(Judgement judgement) {
			entries.Add(new Entry { dspTime = judgement.input.dspTime, lane = judgement.note.note.lane, hit = true, accuracy = judgement.accuracy, offset = judgement.timeOffset });
		}

		// Timed at the note, which is when the player should have hit it
		public void AddMiss(NoteInstance note) {
			entries.Add(new Entry { dspTime = timeline.DspTimeAtBeat(note.Beat), lane = note.note.lane });
		}

		public void Clear() => entries.Clear();

		public Stats Overall() => Collect(null);
		public Stats ForLane(int lane) => Collect(lane);

		Stats Collect(int? lane) {
			Prune(WindowStartDspTime());
			var stats = new Stats();
			foreach (var entry in entries) {
				if (lane.HasValue && entry.lane != lane.Value) continue;
				if (entry.hit) {
					stats.hits++;
					stats.meanAccuracy += entry.accuracy;
					stats.meanOffset += entry.offset;
				} else {
					stats.misses++;
				}
			}
			if (stats.hits > 0) {
				stats.meanAccuracy /= stats.hits;
				stats.meanOffset /= stats.hits;
			}
			return stats;
		}

		void Prune(double start) {
			var kept = 0;
			for (var i = 0; i < entries.Count; i++) {
				if (entries[i].dspTime >= start) entries[kept++] = entries[i];
			}
			entries.RemoveRange(kept, entries.Count - kept);
		}

		double WindowStartDspTime() {
			var clock = timeline.Clock;
			// While paused nothing has a dsp time, so keep the window where it was
			if (!clock.IsPlaying) return lastWindowStart;
			if (unit == Unit.Seconds) return lastWindowStart = clock.DspTime - window;
			var tempoMap = timeline.TempoMap;
			var position = tempoMap.BarAtBeat(timeline.CurrentBeat());
			var bars = (int)Math.Ceiling(window);
			// Near the start of the song the window holds everything so far
			if (position.bar - bars < 0) return lastWindowStart = double.NegativeInfinity;
			// Whole bars back from the current position, then forward by the part of a bar not asked for
			var startBeat = tempoMap.BeatAtBar(position.bar - bars) + position.beatInBar + (bars - window) * position.signature.BarLength;
			return lastWindowStart = timeline.DspTimeAtBeat(startBeat);
		}
	}
}
