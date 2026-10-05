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

		public IReadOnlyList<TempoPoint> TempoPoints => tempoPoints;
		public IReadOnlyList<TimeSignaturePoint> TimeSignatures => timeSignatures;
		public IReadOnlyList<SwingRegion> SwingRegions => swingRegions;

		public event Action Changed;

		// Song time at each tempo point, built lazily
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
			var index = list.FindIndex(x => beatOf(x) == beat);
			if (index >= 0) list[index] = item; else list.Add(item);
			list.Sort((a, b) => beatOf(a).CompareTo(beatOf(b)));
			MarkChanged();
		}

		void MarkChanged() {
			pointTimes = null;
			Changed?.Invoke();
		}

		public void OnBeforeSerialize() {}
		// Inspector and serialized data skip the Set* checks, so sanitise here. No Changed: this can run off the main thread.
		public void OnAfterDeserialize() {
			pointTimes = null;
			tempoPoints ??= new List<TempoPoint>();
			timeSignatures ??= new List<TimeSignaturePoint>();
			swingRegions ??= new List<SwingRegion>();
			tempoPoints.RemoveAll(p => !IsValidBpm(p.bpm) || double.IsNaN(p.beat) || double.IsInfinity(p.beat));
			timeSignatures.RemoveAll(s => s.numerator <= 0 || s.denominator <= 0);
			swingRegions.RemoveAll(r => !(r.subdivision > 0) || !(r.amount > 0 && r.amount < 1));
			StableSort(tempoPoints, p => p.beat);
			StableSort(timeSignatures, s => s.beat);
			StableSort(swingRegions, r => r.beat);
			// Two points at one beat would make a zero-length (divide by zero) segment; the later one wins, as with SetTempo
			for (var i = tempoPoints.Count - 1; i > 0; i--) {
				if (tempoPoints[i - 1].beat == tempoPoints[i].beat) tempoPoints.RemoveAt(i - 1);
			}
			if (tempoPoints.Count == 0) tempoPoints.Add(new TempoPoint(0, 120));
			if (timeSignatures.Count == 0) timeSignatures.Add(new TimeSignaturePoint(0, 4, 4));
		}

		// List.Sort isn't stable, and "keep the last" needs the authored order of equal beats
		static void StableSort<T>(List<T> list, Func<T, double> beatOf) {
			var sorted = list.Select((item, index) => (item, index)).OrderBy(x => beatOf(x.item)).ThenBy(x => x.index).Select(x => x.item).ToList();
			list.Clear();
			list.AddRange(sorted);
		}

		// --- Conversions -------------------------------------------------------------------------

		public double TimeAtBeat(double beat) => TimeAtPerformedBeat(Swing(beat));
		public double BeatAtTime(double time) => Unswing(PerformedBeatAtTime(time));
		// The musical tempo at a beat (swing doesn't change it)
		public double BpmAtBeat(double beat) => BpmAtPerformedBeat(Swing(beat));

		public BarPosition BarAtBeat(double beat) {
			var first = timeSignatures[0];
			if (beat < first.beat || timeSignatures.Count == 1) {
				var barIndex = (int)Math.Floor((beat - first.beat) / first.BarLength);
				return new BarPosition { bar = barIndex, beatInBar = beat - first.beat - barIndex * first.BarLength, signature = first };
			}
			var barsBefore = 0;
			for (var i = 0; i < timeSignatures.Count; i++) {
				var signature = timeSignatures[i];
				var end = i + 1 < timeSignatures.Count ? timeSignatures[i + 1].beat : double.PositiveInfinity;
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
			var first = timeSignatures[0];
			if (bar < 0 || timeSignatures.Count == 1) return first.beat + bar * first.BarLength;
			var barsBefore = 0;
			for (var i = 0; i < timeSignatures.Count; i++) {
				var signature = timeSignatures[i];
				if (i + 1 == timeSignatures.Count) return signature.beat + (bar - barsBefore) * signature.BarLength;
				var barsHere = (int)Math.Ceiling((timeSignatures[i + 1].beat - signature.beat) / signature.BarLength - 1e-9);
				if (bar < barsBefore + barsHere) return signature.beat + (bar - barsBefore) * signature.BarLength;
				barsBefore += barsHere;
			}
			throw new InvalidOperationException("Unreachable");
		}

		// --- Tempo on the performed beat axis ---------------------------------------------------

		void EnsureTimes() {
			if (pointTimes != null && pointTimes.Length == tempoPoints.Count) return;
			pointTimes = new double[tempoPoints.Count];
			// Song time 0 is beat 0; before the first point the first tempo applies
			pointTimes[0] = tempoPoints[0].beat * 60 / tempoPoints[0].bpm;
			for (var i = 1; i < tempoPoints.Count; i++) pointTimes[i] = pointTimes[i - 1] + SegmentDuration(i - 1, tempoPoints[i].beat - tempoPoints[i - 1].beat);
		}

		// Time to advance `beats` from the start of segment i
		double SegmentDuration(int i, double beats) {
			var p = tempoPoints[i];
			var slope = SegmentSlope(i);
			if (Math.Abs(slope) < 1e-12) return beats * 60 / p.bpm;
			// bpm(b) = p.bpm + slope * b, so time = integral of 60 / bpm(b) db
			return 60 / slope * Math.Log((p.bpm + slope * beats) / p.bpm);
		}

		// Beats advanced `time` seconds after the start of segment i
		double SegmentBeats(int i, double time) {
			var p = tempoPoints[i];
			var slope = SegmentSlope(i);
			if (Math.Abs(slope) < 1e-12) return time * p.bpm / 60;
			return p.bpm * (Math.Exp(time * slope / 60) - 1) / slope;
		}

		// BPM change per beat within segment i
		double SegmentSlope(int i) {
			if (tempoPoints[i].curve != TempoCurve.Linear || i + 1 >= tempoPoints.Count) return 0;
			var next = tempoPoints[i + 1];
			return (next.bpm - tempoPoints[i].bpm) / (next.beat - tempoPoints[i].beat);
		}

		int SegmentAtBeat(double beat) {
			var i = 0;
			while (i + 1 < tempoPoints.Count && tempoPoints[i + 1].beat <= beat) i++;
			return i;
		}

		double TimeAtPerformedBeat(double beat) {
			EnsureTimes();
			if (beat <= tempoPoints[0].beat) return beat * 60 / tempoPoints[0].bpm;
			var i = SegmentAtBeat(beat);
			return pointTimes[i] + SegmentDuration(i, beat - tempoPoints[i].beat);
		}

		double PerformedBeatAtTime(double time) {
			EnsureTimes();
			if (time <= pointTimes[0]) return time * tempoPoints[0].bpm / 60;
			var i = 0;
			while (i + 1 < tempoPoints.Count && pointTimes[i + 1] <= time) i++;
			return tempoPoints[i].beat + SegmentBeats(i, time - pointTimes[i]);
		}

		double BpmAtPerformedBeat(double beat) {
			if (beat <= tempoPoints[0].beat) return tempoPoints[0].bpm;
			var i = SegmentAtBeat(beat);
			return tempoPoints[i].bpm + SegmentSlope(i) * (beat - tempoPoints[i].beat);
		}

		// --- Swing -------------------------------------------------------------------------------
		// Beats are grouped in pairs of two subdivisions, aligned to beat 0. Within a pair that lies wholly inside a
		// swing region, the off-beat moves from the middle to `amount` of the way through. Pairs that straddle a region
		// boundary stay straight, so the warp is continuous and always increasing.

		public double Swing(double beat) => Warp(beat, true);
		public double Unswing(double performedBeat) => Warp(performedBeat, false);

		double Warp(double beat, bool forward) {
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
			for (var i = swingRegions.Count - 1; i >= 0; i--) {
				if (swingRegions[i].beat <= beat) return swingRegions[i];
				regionEnd = swingRegions[i].beat;
			}
			return null;
		}
	}
}
