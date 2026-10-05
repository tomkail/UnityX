using System.Collections.Generic;
using NUnit.Framework;

namespace UnityX.Rhythm.Tests {
	public class BeatEventTrackerTests {
		[Test]
		public void ReportsEachCrossingOnce() {
			var tracker = new BeatEventTracker(1);
			var crossed = new List<long>();
			tracker.Advance(-0.2, crossed.Add);
			tracker.Advance(0.1, crossed.Add);
			tracker.Advance(0.9, crossed.Add);
			tracker.Advance(3.05, crossed.Add);
			CollectionAssert.AreEqual(new long[] { 0, 1, 2, 3 }, crossed);
		}

		[Test]
		public void FirstAdvanceOnlyRecords() {
			var tracker = new BeatEventTracker(0.5);
			var crossed = new List<long>();
			tracker.Advance(10.2, crossed.Add);
			CollectionAssert.IsEmpty(crossed);
		}

		[Test]
		public void BackwardsResyncsWithoutFiring() {
			var tracker = new BeatEventTracker(1);
			var crossed = new List<long>();
			tracker.Advance(5.5, crossed.Add);
			tracker.Advance(1.2, crossed.Add);
			tracker.Advance(2.1, crossed.Add);
			CollectionAssert.AreEqual(new long[] { 2 }, crossed);
		}

		[Test]
		public void SmallBackwardWobblesAreIgnored() {
			var tracker = new BeatEventTracker(1);
			var crossed = new List<long>();
			tracker.Advance(0.9, crossed.Add);
			tracker.Advance(1.01, crossed.Add);
			tracker.Advance(0.99, crossed.Add);
			tracker.Advance(1.02, crossed.Add);
			tracker.Advance(2.0, crossed.Add);
			CollectionAssert.AreEqual(new long[] { 1, 2 }, crossed);
		}

		[Test]
		public void PrimeReportsALineExactlyAtTheStart() {
			var tracker = new BeatEventTracker(0.5);
			var crossed = new List<long>();
			tracker.Prime(2.0);
			tracker.Advance(2.0, crossed.Add);
			tracker.Advance(2.4, crossed.Add);
			CollectionAssert.AreEqual(new long[] { 4 }, crossed);
		}

		[Test]
		public void IgnoresInvalidInput() {
			var tracker = new BeatEventTracker(1);
			var crossed = new List<long>();
			tracker.Advance(0.5, crossed.Add);
			tracker.Advance(double.NaN, crossed.Add);
			tracker.Advance(double.PositiveInfinity, crossed.Add);
			tracker.interval = 0;
			tracker.Advance(3, crossed.Add);
			CollectionAssert.IsEmpty(crossed);
		}

		[Test]
		public void CatchUpIsCapped() {
			var tracker = new BeatEventTracker(1) { maxCatchUp = 4 };
			var crossed = new List<long>();
			tracker.Advance(0, crossed.Add);
			tracker.Advance(100.5, crossed.Add);
			CollectionAssert.AreEqual(new long[] { 97, 98, 99, 100 }, crossed);
		}
	}
}
