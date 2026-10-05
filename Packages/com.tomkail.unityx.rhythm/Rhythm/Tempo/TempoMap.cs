using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityX.Rhythm {
	public enum TempoCurve {
		// Holds this tempo until the next point
		Step,
		// Changes tempo linearly (per beat) to reach the next point's tempo at its beat
		Linear
	}

	[Serializable]
	public struct TempoPoint {
		public double beat;
		public double bpm;
		public TempoCurve curve;
		public TempoPoint(double beat, double bpm, TempoCurve curve = TempoCurve.Step) { this.beat = beat; this.bpm = bpm; this.curve = curve; }
	}

	[Serializable]
	public struct TimeSignaturePoint {
		// Should sit on a bar line of the previous signature
		public double beat;
		public int numerator;
		public int denominator;
		public TimeSignaturePoint(double beat, int numerator, int denominator) { this.beat = beat; this.numerator = numerator; this.denominator = denominator; }
		// Bar length in beats (quarter notes)
		public double BarLength => numerator * 4.0 / denominator;
	}

	[Serializable]
	public struct SwingRegion {
		public double beat;
		// The swung note length in beats: 0.5 swings 8th notes, 0.25 swings 16ths
		public double subdivision;
		// Where the off-beat lands within each pair: 0.5 is straight, 0.667 is triplet feel, 0.75 is hard swing
		public double amount;
		public SwingRegion(double beat, double subdivision, double amount) { this.beat = beat; this.subdivision = subdivision; this.amount = amount; }
	}

	public struct BarPosition {
		public int bar;
		public double beatInBar;
		public TimeSignaturePoint signature;
	}

	// Converts between beats (quarter notes since the song started) and song time (seconds since the song started).
	// Beats passed in and returned are authored ("straight") beats; swing is applied inside the conversion.
	// Edit through the methods so the cached segments stay correct and Changed fires.
	[Serializable]
	public class TempoMap : ISerializationCallbackReceiver {
		[SerializeField] List<TempoPoint> tempoPoints = new() { new TempoPoint(0, 120) };
		[SerializeField] List<TimeSignaturePoint> timeSignatures = new() { new TimeSignaturePoint(0, 4, 4) };
		[SerializeField] List<SwingRegion> swingRegions = new();

		// The authored data, as edited by the Set*/Remove* methods and the inspector, so indices match RemoveTempoPoint etc.
		// Inspector data can be unsorted or temporarily invalid; conversions use a sanitised copy instead.
		public IReadOnlyList<TempoPoint> TempoPoints => tempoPoints;
		public IReadOnlyList<TimeSignaturePoint> TimeSignatures => timeSignatures;
		public IReadOnlyList<SwingRegion> SwingRegions => swingRegions;

		public event Action Changed;

		// Runtime copies of the authored lists: sorted, valid and never empty. Built lazily; null means rebuild.
		List<TempoPoint> points;
		List<TimeSignaturePoint> signatures;
		List<SwingRegion> swings;
		// Song time at each runtime tempo point
		double[] pointTimes;

		public TempoMap() {}
		public TempoMap(double bpm) {
			ValidateBpm(bpm);
			tempoPoints[0] = new TempoPoint(0, bpm);
		}

		public TempoMap Clone() {
			var clone = new TempoMap();
			clone.tempoPoints = new List<TempoPoint>(tempoPoints);
			clone.timeSignatures = new List<TimeSignaturePoint>(timeSignatures);
			clone.swingRegions = new List<SwingRegion>(swingRegions);
			return clone;
		}

		// Adds a tempo point, replacing any existing point at the same beat
		public void SetTempo(double beat, double bpm, TempoCurve curve = TempoCurve.Step) {
			ValidateBpm(bpm);
			Upsert(tempoPoints, new TempoPoint(beat, bpm, curve), p => p.beat);
		}
		public void RemoveTempoPoint(int index) {
			if (tempoPoints.Count <= 1) throw new InvalidOperationException("A tempo map needs at least one tempo point");
			tempoPoints.RemoveAt(index); MarkChanged();
		}
		public void SetTimeSignature(double beat, int numerator, int denominator) {
			if (numerator <= 0 || denominator <= 0) throw new ArgumentOutOfRangeException(nameof(numerator), "Time signature parts must be positive");
			Upsert(timeSignatures, new TimeSignaturePoint(beat, numerator, denominator), p => p.beat);
		}
		public void RemoveTimeSignature(int index) {
			if (timeSignatures.Count <= 1) throw new InvalidOperationException("A tempo map needs at least one time signature");
			timeSignatures.RemoveAt(index); MarkChanged();
		}
		// amount 0.5 turns swing off from this beat
		public void SetSwing(double beat, double subdivision, double amount) {
			if (subdivision <= 0) throw new ArgumentOutOfRangeException(nameof(subdivision));
			if (amount <= 0 || amount >= 1) throw new ArgumentOutOfRangeException(nameof(amount), "Swing amount must be between 0 and 1");
			Upsert(swingRegions, new SwingRegion(beat, subdivision, amount), p => p.beat);
		}
		public void RemoveSwing(int index) { swingRegions.RemoveAt(index); MarkChanged(); }

		static bool IsValidBpm(double bpm) => bpm > 0 && !double.IsInfinity(bpm);
		static void ValidateBpm(double bpm) {
			if (!IsValidBpm(bpm)) throw new ArgumentOutOfRangeException(nameof(bpm), "BPM must be positive and finite");
		}

		void Upsert<T>(List<T> list, T item, Func<T, double> beatOf) {
			var beat = beatOf(item);
			// Inspector data can hold several entries at one beat; replace them all so the new one takes effect
			list.RemoveAll(x => beatOf(x) == beat);
			list.Add(item);
			StableSort(list, beatOf);
			MarkChanged();
		}

		void MarkChanged() {
			points = null;
			Changed?.Invoke();
		}

		public void OnBeforeSerialize() {}
		// Leaves the authored lists exactly as serialized: the inspector round-trips edits through here, so sanitising
		// them would fight the user mid-edit and save the result. No Changed: this can run off the main thread.
		public void OnAfterDeserialize() => points = null;

		// Inspector and serialized data skip the Set* checks, so the runtime copy drops anything invalid
		void EnsureRuntime() {
			if (points != null) return;
			var newPoints = new List<TempoPoint>();
			if (tempoPoints != null) newPoints.AddRange(tempoPoints.Where(p => IsValidBpm(p.bpm) && !double.IsNaN(p.beat) && !double.IsInfinity(p.beat)));
			StableSort(newPoints, p => p.beat);
			// Two points at one beat would make a zero-length (divide by zero) segment; the later one wins, as with SetTempo
			for (var i = newPoints.Count - 1; i > 0; i--) {
				if (newPoints[i - 1].beat == newPoints[i].beat) newPoints.RemoveAt(i - 1);
			}
			if (newPoints.Count == 0) newPoints.Add(new TempoPoint(0, 120));

			var newSignatures = new List<TimeSignaturePoint>();
			if (timeSignatures != null) newSignatures.AddRange(timeSignatures.Where(t => t.numerator > 0 && t.denominator > 0 && !double.IsNaN(t.beat) && !double.IsInfinity(t.beat)));
			StableSort(newSignatures, t => t.beat);
			if (newSignatures.Count == 0) newSignatures.Add(new TimeSignaturePoint(0, 4, 4));

			var newSwings = new List<SwingRegion>();
			if (swingRegions != null) newSwings.AddRange(swingRegions.Where(r => r.subdivision > 0 && !double.IsInfinity(r.subdivision) && r.amount > 0 && r.amount < 1 && !double.IsNaN(r.beat)));
			StableSort(newSwings, r => r.beat);

			signatures = newSignatures;
			swings = newSwings;
			points = newPoints;
			pointTimes = new double[points.Count];
			// Song time 0 is beat 0; before the first point the first tempo applies
			pointTimes[0] = points[0].beat * 60 / points[0].bpm;
			for (var i = 1; i < points.Count; i++) pointTimes[i] = pointTimes[i - 1] + SegmentDuration(i - 1, points[i].beat - points[i - 1].beat);
		}

		// List.Sort isn't stable, and "keep the last" needs the authored order of equal beats
		static void StableSort<T>(List<T> list, Func<T, double> beatOf) {
			var sorted = list.Select((item, index) => (item, index)).OrderBy(x => beatOf(x.item)).ThenBy(x => x.index).Select(x => x.item).ToList();
			list.Clear();
			list.AddRange(sorted);
		}

		// --- Conversions -------------------------------------------------------------------------

		public double TimeAtBeat(double beat) { EnsureRuntime(); return TimeAtPerformedBeat(Swing(beat)); }
		public double BeatAtTime(double time) { EnsureRuntime(); return Unswing(PerformedBeatAtTime(time)); }
		// The musical tempo at a beat (swing doesn't change it)
		public double BpmAtBeat(double beat) { EnsureRuntime(); return BpmAtPerformedBeat(Swing(beat)); }

		public BarPosition BarAtBeat(double beat) {
			EnsureRuntime();
			var first = signatures[0];
			if (beat < first.beat || signatures.Count == 1) {
				var barIndex = (int)Math.Floor((beat - first.beat) / first.BarLength);
				return new BarPosition { bar = barIndex, beatInBar = beat - first.beat - barIndex * first.BarLength, signature = first };
			}
			var barsBefore = 0;
			for (var i = 0; i < signatures.Count; i++) {
				var signature = signatures[i];
				var end = i + 1 < signatures.Count ? signatures[i + 1].beat : double.PositiveInfinity;
				if (beat < end) {
					var barsIn = (int)Math.Floor((beat - signature.beat) / signature.BarLength);
					return new BarPosition { bar = barsBefore + barsIn, beatInBar = beat - signature.beat - barsIn * signature.BarLength, signature = signature };
				}
				// A change part-way through a bar ends that bar early
				barsBefore += (int)Math.Ceiling((end - signature.beat) / signature.BarLength - 1e-9);
			}
			throw new InvalidOperationException("Unreachable");
		}

		public double BeatAtBar(int bar) {
			EnsureRuntime();
			var first = signatures[0];
			if (bar < 0 || signatures.Count == 1) return first.beat + bar * first.BarLength;
			var barsBefore = 0;
			for (var i = 0; i < signatures.Count; i++) {
				var signature = signatures[i];
				if (i + 1 == signatures.Count) return signature.beat + (bar - barsBefore) * signature.BarLength;
				var barsHere = (int)Math.Ceiling((signatures[i + 1].beat - signature.beat) / signature.BarLength - 1e-9);
				if (bar < barsBefore + barsHere) return signature.beat + (bar - barsBefore) * signature.BarLength;
				barsBefore += barsHere;
			}
			throw new InvalidOperationException("Unreachable");
		}

		// --- Tempo on the performed beat axis ---------------------------------------------------

		// Time to advance `beats` from the start of segment i
		double SegmentDuration(int i, double beats) {
			var p = points[i];
			var slope = SegmentSlope(i);
			if (Math.Abs(slope) < 1e-12) return beats * 60 / p.bpm;
			// bpm(b) = p.bpm + slope * b, so time = integral of 60 / bpm(b) db
			return 60 / slope * Math.Log((p.bpm + slope * beats) / p.bpm);
		}

		// Beats advanced `time` seconds after the start of segment i
		double SegmentBeats(int i, double time) {
			var p = points[i];
			var slope = SegmentSlope(i);
			if (Math.Abs(slope) < 1e-12) return time * p.bpm / 60;
			return p.bpm * (Math.Exp(time * slope / 60) - 1) / slope;
		}

		// BPM change per beat within segment i
		double SegmentSlope(int i) {
			if (points[i].curve != TempoCurve.Linear || i + 1 >= points.Count) return 0;
			var next = points[i + 1];
			return (next.bpm - points[i].bpm) / (next.beat - points[i].beat);
		}

		int SegmentAtBeat(double beat) {
			var i = 0;
			while (i + 1 < points.Count && points[i + 1].beat <= beat) i++;
			return i;
		}

		double TimeAtPerformedBeat(double beat) {
			EnsureRuntime();
			if (beat <= points[0].beat) return beat * 60 / points[0].bpm;
			var i = SegmentAtBeat(beat);
			return pointTimes[i] + SegmentDuration(i, beat - points[i].beat);
		}

		double PerformedBeatAtTime(double time) {
			EnsureRuntime();
			if (time <= pointTimes[0]) return time * points[0].bpm / 60;
			var i = 0;
			while (i + 1 < points.Count && pointTimes[i + 1] <= time) i++;
			return points[i].beat + SegmentBeats(i, time - pointTimes[i]);
		}

		double BpmAtPerformedBeat(double beat) {
			if (beat <= points[0].beat) return points[0].bpm;
			var i = SegmentAtBeat(beat);
			return points[i].bpm + SegmentSlope(i) * (beat - points[i].beat);
		}

		// --- Swing -------------------------------------------------------------------------------
		// Beats are grouped in pairs of two subdivisions, aligned to beat 0. Within a pair that lies wholly inside a
		// swing region, the off-beat moves from the middle to `amount` of the way through. Pairs that straddle a region
		// boundary stay straight, so the warp is continuous and always increasing.

		public double Swing(double beat) => Warp(beat, true);
		public double Unswing(double performedBeat) => Warp(performedBeat, false);

		double Warp(double beat, bool forward) {
			EnsureRuntime();
			var region = RegionAt(beat, out var regionEnd);
			if (region == null || Math.Abs(region.Value.amount - 0.5) < 1e-12) return beat;
			var pairLength = region.Value.subdivision * 2;
			var pairStart = Math.Floor(beat / pairLength) * pairLength;
			if (pairStart < region.Value.beat - 1e-9 || pairStart + pairLength > regionEnd + 1e-9) return beat;
			var x = beat - pairStart;
			var straightMid = region.Value.subdivision;
			var swungMid = pairLength * region.Value.amount;
			var fromMid = forward ? straightMid : swungMid;
			var toMid = forward ? swungMid : straightMid;
			var warped = x < fromMid ? x * toMid / fromMid : toMid + (x - fromMid) * (pairLength - toMid) / (pairLength - fromMid);
			return pairStart + warped;
		}

		// Pair boundaries are unchanged by the warp, so looking the region up on either axis finds the same one
		SwingRegion? RegionAt(double beat, out double regionEnd) {
			regionEnd = double.PositiveInfinity;
			for (var i = swings.Count - 1; i >= 0; i--) {
				if (swings[i].beat <= beat) return swings[i];
				regionEnd = swings[i].beat;
			}
			return null;
		}
	}
}
