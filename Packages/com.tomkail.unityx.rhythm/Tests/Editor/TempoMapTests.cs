using NUnit.Framework;

namespace UnityX.Rhythm.Tests {
	public class TempoMapTests {
		const double Tolerance = 1e-9;

		[Test]
		public void ConstantTempoConvertsBothWays() {
			var map = new TempoMap(120);
			Assert.AreEqual(0.5, map.TimeAtBeat(1), Tolerance);
			Assert.AreEqual(4, map.BeatAtTime(2), Tolerance);
			Assert.AreEqual(-0.5, map.TimeAtBeat(-1), Tolerance, "Negative beats (lead-in) use the first tempo");
			Assert.AreEqual(120, map.BpmAtBeat(10), Tolerance);
		}

		[Test]
		public void StepChangeTakesEffectAtItsBeat() {
			var map = new TempoMap(120);
			map.SetTempo(4, 60);
			Assert.AreEqual(2, map.TimeAtBeat(4), Tolerance);
			Assert.AreEqual(3, map.TimeAtBeat(5), Tolerance);
			Assert.AreEqual(5, map.BeatAtTime(3), Tolerance);
			Assert.AreEqual(120, map.BpmAtBeat(3.99), Tolerance);
			Assert.AreEqual(60, map.BpmAtBeat(4), Tolerance);
		}

		[Test]
		public void LinearRampIsExactBothWays() {
			var map = new TempoMap(60);
			map.SetTempo(0, 60, TempoCurve.Linear);
			map.SetTempo(8, 180);
			Assert.AreEqual(120, map.BpmAtBeat(4), Tolerance);
			// time = 60/k * ln(bpm(b)/bpm0), k = 15 bpm per beat
			Assert.AreEqual(4 * System.Math.Log(3), map.TimeAtBeat(8), Tolerance);
			for (var beat = -2.0; beat < 20; beat += 0.37) {
				Assert.AreEqual(beat, map.BeatAtTime(map.TimeAtBeat(beat)), 1e-9, $"Round trip at beat {beat}");
			}
		}

		[Test]
		public void SettingTempoAtExistingBeatReplacesIt() {
			var map = new TempoMap(120);
			map.SetTempo(0, 90);
			Assert.AreEqual(1, map.TempoPoints.Count);
			Assert.AreEqual(90, map.BpmAtBeat(0), Tolerance);
		}

		[Test]
		public void ChangedFiresOnEdits() {
			var map = new TempoMap(120);
			var count = 0;
			map.Changed += () => count++;
			map.SetTempo(4, 100);
			map.SetTimeSignature(8, 3, 4);
			map.SetSwing(0, 0.5, 0.6);
			Assert.AreEqual(3, count);
		}

		[Test]
		public void CloneIsIndependent() {
			var map = new TempoMap(120);
			var clone = map.Clone();
			clone.SetTempo(0, 60);
			Assert.AreEqual(120, map.BpmAtBeat(0), Tolerance);
		}

		[Test]
		public void InvalidValuesThrow() {
			var map = new TempoMap(120);
			Assert.Throws<System.ArgumentOutOfRangeException>(() => map.SetTempo(0, 0));
			Assert.Throws<System.ArgumentOutOfRangeException>(() => map.SetTimeSignature(0, 0, 4));
			Assert.Throws<System.ArgumentOutOfRangeException>(() => map.SetSwing(0, 0.5, 1));
			Assert.Throws<System.InvalidOperationException>(() => map.RemoveTempoPoint(0));
		}
	}

	public class TimeSignatureTests {
		[Test]
		public void FourFourBars() {
			var map = new TempoMap(120);
			var position = map.BarAtBeat(9.5);
			Assert.AreEqual(2, position.bar);
			Assert.AreEqual(1.5, position.beatInBar, 1e-9);
			Assert.AreEqual(-1, map.BarAtBeat(-0.5).bar);
			Assert.AreEqual(8, map.BeatAtBar(2), 1e-9);
		}

		[Test]
		public void SignatureChangeCountsBarsAcrossIt() {
			var map = new TempoMap(120);
			map.SetTimeSignature(8, 3, 4);
			map.SetTimeSignature(14, 6, 8);
			Assert.AreEqual(1, map.BarAtBeat(7).bar);
			Assert.AreEqual(2, map.BarAtBeat(8).bar);
			Assert.AreEqual(3, map.BarAtBeat(11).bar);
			Assert.AreEqual(3, map.BarAtBeat(11).signature.numerator);
			// 6/8 bars are 3 quarter notes long
			Assert.AreEqual(4, map.BarAtBeat(14).bar);
			Assert.AreEqual(5, map.BarAtBeat(17.5).bar);
			Assert.AreEqual(0.5, map.BarAtBeat(17.5).beatInBar, 1e-9);
			for (var bar = 0; bar < 8; bar++) Assert.AreEqual(bar, map.BarAtBeat(map.BeatAtBar(bar)).bar, $"Bar {bar}");
		}
	}

	public class SwingTests {
		[Test]
		public void EighthSwingMovesOffBeats() {
			var map = new TempoMap(60);
			map.SetSwing(0, 0.5, 2.0 / 3);
			Assert.AreEqual(0, map.Swing(0), 1e-9);
			Assert.AreEqual(2.0 / 3, map.Swing(0.5), 1e-9, "Triplet feel");
			Assert.AreEqual(1, map.Swing(1), 1e-9, "Beats stay put");
			Assert.AreEqual(2.0 / 3, map.TimeAtBeat(0.5), 1e-9);
			Assert.AreEqual(0.5, map.BeatAtTime(2.0 / 3), 1e-9);
		}

		[Test]
		public void SwingInvertsAndKeepsOrder() {
			var map = new TempoMap(100);
			map.SetSwing(2, 0.25, 0.7);
			map.SetSwing(6.1, 0.5, 0.5);
			var previous = double.NegativeInfinity;
			for (var beat = -1.0; beat < 10; beat += 0.01) {
				var swung = map.Swing(beat);
				Assert.Greater(swung, previous, $"Increasing at {beat}");
				Assert.AreEqual(beat, map.Unswing(swung), 1e-9, $"Inverts at {beat}");
				previous = swung;
			}
		}

		[Test]
		public void PairsStraddlingARegionEdgeStayStraight() {
			var map = new TempoMap(60);
			map.SetSwing(0.5, 0.5, 0.75);
			// The pair [0,1) starts before the region, so it isn't swung
			Assert.AreEqual(0.5, map.Swing(0.5), 1e-9);
			Assert.AreEqual(1.75, map.Swing(1.5), 1e-9);
		}
	}
}
