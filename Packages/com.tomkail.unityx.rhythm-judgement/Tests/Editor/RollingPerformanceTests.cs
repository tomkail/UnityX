using NUnit.Framework;

namespace UnityX.Rhythm.JudgementTests {
	public class RollingPerformanceTests {
		[Test]
		public void RollingPerformanceCoversTheLastFewSeconds() {
			var rig = new JudgeRig(new Pattern(1, new[] { new Note(0, 0), new Note(0.5, 1) }));
			using var performance = new RollingPerformance(rig.song.timeline, 1);
			performance.Watch(rig.judge);
			rig.Play(-0.2);
			rig.Run(0.1);
			rig.PressAtBeat(0, 0, -0.02);
			rig.Run(0.25);
			rig.PressAtBeat(1, 0.5, 0.04);
			rig.Run(0.6);
			var overall = performance.Overall();
			Assert.AreEqual(2, overall.hits);
			// Song time is about 0.75: beat 1 (0.5s) is past its window and the miss delay, beat 1.5 (0.75s) isn't yet
			Assert.AreEqual(1, overall.misses);
			Assert.AreEqual(0.01, overall.meanOffset, 1e-9);
			Assert.AreEqual(1, performance.ForLane(1).hits);
			// Two seconds on, the early hits have left the window
			rig.Run(2);
			Assert.AreEqual(0, performance.Overall().hits);
		}

		[Test]
		public void RollingPerformanceCanCountInBars() {
			var rig = new JudgeRig(new BeatGrid(1));
			using var performance = new RollingPerformance(rig.song.timeline, 1, RollingPerformance.Unit.Bars);
			performance.Watch(rig.judge);
			rig.Play(-0.2);
			// Hit every beat of the first two bars
			for (var beat = 0; beat < 8; beat++) {
				rig.Run(beat == 0 ? 0.2 : 0.5);
				rig.PressAtBeat(0, beat);
			}
			rig.Run(0.2);
			// At beat 7.4 (bar 1, beat 3.4), one bar back is beat 3.4, so beats 4 to 7 count
			Assert.AreEqual(4, performance.Overall().hits);
			Assert.AreEqual(1, performance.Overall().HitRate);
		}

		[Test]
		public void APauseKeepsTheWindowWhereItWas() {
			var rig = new JudgeRig(new BeatGrid(1));
			using var performance = new RollingPerformance(rig.song.timeline, 1);
			performance.Watch(rig.judge);
			rig.Play(-0.2);
			rig.Run(0.1);
			rig.PressAtBeat(0, 0);
			rig.Run(0.3);
			Assert.AreEqual(1, performance.Overall().hits);
			rig.song.clock.Pause();
			rig.Run(10);
			Assert.AreEqual(1, performance.Overall().hits);
			rig.song.clock.Resume();
			Assert.AreEqual(1, performance.Overall().hits);
		}

		[Test]
		public void HitsAheadOfTheSongAfterASeekBackNoLongerCount() {
			var rig = new JudgeRig(new BeatGrid(1));
			using var performance = new RollingPerformance(rig.song.timeline, 1);
			performance.Watch(rig.judge);
			rig.Play(-0.2);
			rig.Run(0.1);
			rig.PressAtBeat(0, 0);
			rig.Run(0.5);
			rig.PressAtBeat(0, 1);
			rig.Run(0.2);
			Assert.AreEqual(2, performance.Overall().hits);
			rig.song.clock.Seek(-0.2);
			Assert.AreEqual(0, performance.Overall().hits);
		}

		[Test]
		public void MissesLeaveTheWindowOnceItMovesPastThem() {
			var rig = new JudgeRig(new Pattern(4, new[] { new Note(0) }));
			using var performance = new RollingPerformance(rig.song.timeline, 1);
			performance.Watch(rig.judge);
			rig.Play(-0.2);
			// Beat 0 goes unplayed and is missed once its window and the miss delay have passed
			rig.Run(0.5);
			Assert.AreEqual(1, rig.missed.Count);
			Assert.AreEqual(1, performance.Overall().misses);
			rig.Run(1);
			Assert.AreEqual(0, performance.Overall().misses);
			// A miss added while paused is still timed at its note, so it leaves the window too
			rig.song.clock.Pause();
			performance.AddMiss(rig.missed[0]);
			rig.song.clock.Seek(0.5);
			Assert.AreEqual(1, performance.Overall().misses);
			rig.song.clock.Resume();
			rig.Run(1);
			Assert.AreEqual(0, performance.Overall().misses);
		}
	}
}
