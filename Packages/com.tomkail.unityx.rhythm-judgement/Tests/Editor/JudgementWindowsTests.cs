using NUnit.Framework;

namespace UnityX.Rhythm.JudgementTests {
	public class JudgementWindowsTests {
		[Test]
		public void DefaultsArePerfectGreatGood() {
			var windows = new JudgementWindows();
			CollectionAssert.AreEqual(new[] { "Perfect", "Great", "Good" }, windows.grades.ConvertAll(g => g.name));
			Assert.AreEqual(WindowUnit.Seconds, windows.unit);
			Assert.AreEqual(0.1, windows.WidestEarly);
			Assert.AreEqual(0.1, windows.WidestLate);
		}

		[Test]
		public void TheFirstGradeContainingTheOffsetWins() {
			var windows = new JudgementWindows();
			Assert.AreEqual(0, windows.GradeFor(0));
			Assert.AreEqual(0, windows.GradeFor(-0.025));
			Assert.AreEqual(1, windows.GradeFor(0.03));
			Assert.AreEqual(2, windows.GradeFor(-0.09));
			Assert.AreEqual(-1, windows.GradeFor(0.11));
		}

		[Test]
		public void EarlyAndLateCanDiffer() {
			var windows = new JudgementWindows { grades = { } };
			windows.grades.Clear();
			windows.grades.Add(new JudgementGrade("Hit", 0.05, 0.15));
			Assert.AreEqual(0, windows.GradeFor(0.12));
			Assert.AreEqual(-1, windows.GradeFor(-0.12));
			Assert.AreEqual(0.05, windows.WidestEarly);
			Assert.AreEqual(0.15, windows.WidestLate);
		}

		[Test]
		public void AccuracyFallsToZeroAtTheWidestEdgeOnEachSide() {
			var windows = new JudgementWindows();
			windows.grades.Clear();
			windows.grades.Add(new JudgementGrade("Hit", 0.05, 0.2));
			Assert.AreEqual(1, windows.AccuracyFor(0));
			Assert.AreEqual(0.5, windows.AccuracyFor(-0.025), 1e-9);
			Assert.AreEqual(0.5, windows.AccuracyFor(0.1), 1e-9);
			Assert.AreEqual(0, windows.AccuracyFor(0.3));
		}
	}
}
