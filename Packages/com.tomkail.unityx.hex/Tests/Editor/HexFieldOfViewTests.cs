using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.HexGrid.Tests {
	public class HexFieldOfViewTests {
		static readonly AngleArc allAround = AngleArc.AngleFieldOfView(0, 360);

		static System.Func<HexCoord, HexSightOcclusion> Walls (params HexCoord[] walls) {
			var set = new HashSet<HexCoord>(walls);
			return c => set.Contains(c) ? HexSightOcclusion.Block : HexSightOcclusion.None;
		}

		[Test]
		public void OpenFieldSeesTheWholeRange () {
			var visible = HexFieldOfView.GetVisibleCells(HexCoord.zero, allAround, 0, 3, Walls());
			Assert.IsTrue(visible.SetEquals(HexCoord.GetPointsInRing(HexCoord.zero, 0, 3)));
		}

		[Test]
		public void ClipPlanesLimitTheRings () {
			var visible = HexFieldOfView.GetVisibleCells(HexCoord.zero, allAround, 2, 3, Walls());
			Assert.IsTrue(visible.SetEquals(HexCoord.GetPointsInRing(HexCoord.zero, 2, 3)));
			Assert.IsEmpty(HexFieldOfView.GetVisibleCells(HexCoord.zero, allAround, 3, 2, Walls()));
		}

		[Test]
		public void WallsShadowCellsBehindThem () {
			var wall = HexCoord.Direction(0);
			var visible = HexFieldOfView.GetVisibleCells(HexCoord.zero, allAround, 0, 4, Walls(wall));
			Assert.IsFalse(visible.Contains(wall));
			for (int d = 2; d <= 4; d++) Assert.IsFalse(visible.Contains(wall * d), $"distance {d}");
			Assert.IsTrue(visible.Contains(HexCoord.Direction(3) * 4));

			var withWalls = HexFieldOfView.GetVisibleCells(HexCoord.zero, allAround, 0, 4, Walls(wall), includeBlockingCells: true);
			Assert.IsTrue(withWalls.Contains(wall));
			Assert.IsFalse(withWalls.Contains(wall * 2));
		}

		[Test]
		public void MaskedCellsAreHiddenButDontBlock () {
			var masked = HexCoord.Direction(0);
			var visible = HexFieldOfView.GetVisibleCells(HexCoord.zero, allAround, 0, 3,
				c => c == masked ? HexSightOcclusion.Mask : HexSightOcclusion.None);
			Assert.IsFalse(visible.Contains(masked));
			Assert.IsTrue(visible.Contains(masked * 2));
		}

		[Test]
		public void ConeOnlySeesItsDirection () {
			var forward = HexCoord.Direction(0);
			var visible = HexFieldOfView.GetVisibleCells(HexCoord.zero, forward.Position(), 60, 0, 3, Walls());
			Assert.IsTrue(visible.Contains(forward * 3));
			Assert.IsFalse(visible.Contains(HexCoord.Direction(3)));
			Assert.IsFalse(visible.Contains(HexCoord.Direction(2) * 2));
		}

		[Test]
		public void EnclosedOriginTerminatesWithoutFarClip () {
			var ring = HexCoord.GetPointsOnRing(HexCoord.zero, 2);
			var visible = HexFieldOfView.GetVisibleCells(HexCoord.zero, allAround, 0, int.MaxValue, Walls(ring));
			Assert.IsTrue(visible.SetEquals(HexCoord.GetPointsInRing(HexCoord.zero, 0, 1)));
		}

		[Test]
		public void LineOfSight () {
			var target = HexCoord.Direction(0) * 3;
			Assert.IsTrue(HexFieldOfView.HasLineOfSight(HexCoord.zero, target, Walls()));
			Assert.IsFalse(HexFieldOfView.HasLineOfSight(HexCoord.zero, target, Walls(HexCoord.Direction(0))));
			Assert.IsTrue(HexFieldOfView.HasLineOfSight(HexCoord.zero, target, Walls(target)), "target's own occlusion is ignored");
			Assert.IsTrue(HexFieldOfView.HasLineOfSight(HexCoord.zero, target, Walls(HexCoord.Direction(1))), "off-line wall");
			Assert.IsTrue(HexFieldOfView.HasLineOfSight(target, target, Walls(target)));
		}

		[Test]
		public void AngleArcSplitsAtTheWrap () {
			var arc = AngleArc.AngleFieldOfView(0, 90);
			CollectionAssert.AreEquivalent(new[] { new Vector2(315, 360), new Vector2(0, 45) }, arc.ranges);
			Assert.IsTrue(arc.OverlapsRange(new Vector2(350, 355)));
			Assert.IsFalse(arc.OverlapsRange(new Vector2(90, 180)));
		}
	}

	public class HexSearchAndArcTests {
		[Test]
		public void ReachableIsTheRangeInOpenField () {
			var reachable = HexSearch.Reachable(HexCoord.zero, 3, _ => true);
			Assert.IsTrue(new HashSet<HexCoord>(reachable.Keys).SetEquals(HexCoord.GetPointsInRing(HexCoord.zero, 0, 3)));
			foreach (var pair in reachable) Assert.AreEqual(HexCoord.Distance(HexCoord.zero, pair.Key), pair.Value);
		}

		[Test]
		public void ReachableWalksAroundWalls () {
			// Wall off directions 0, 1 and 5 at distance 1: getting to (2,0) means going round.
			var walls = new HashSet<HexCoord> { HexCoord.Direction(0), HexCoord.Direction(1), HexCoord.Direction(5) };
			var target = HexCoord.Direction(0) * 2;
			// Shortest way round: (0,-1), (1,-2), (2,-2), (2,-1), (2,0).
			var reachable = HexSearch.Reachable(HexCoord.zero, 5, c => !walls.Contains(c));
			Assert.IsFalse(reachable.ContainsKey(HexCoord.Direction(0)));
			Assert.AreEqual(5, reachable[target]);
			Assert.IsFalse(HexSearch.Reachable(HexCoord.zero, 4, c => !walls.Contains(c)).ContainsKey(target));
		}

		[Test]
		public void HexArcCoversItsDirections () {
			var arc = new HexArc(5, 3);
			CollectionAssert.AreEqual(new[] { 5, 6, 7 }, arc.directionIndicesCovered.ToArray());
			Assert.AreEqual(HexCoord.Direction(1), arc.finalDirection);
			CollectionAssert.AreEqual(new[] { 2, 1 }, new HexArc(2, -2).directionIndicesCovered.ToArray());
			Assert.AreEqual(new HexArc(1, 3), new HexArc(new[] { 1, 2, 3 }));
			Assert.AreEqual(new HexArc(4, -2), new HexArc(new[] { 4, 3 }));
			Assert.AreEqual(new HexArc(2, 1), new HexArc(new[] { 2 }));
			Assert.AreEqual(0, new HexArc(new int[0]).arcLength);
		}

		[Test]
		public void ClosestDirectionIndexFloatAveragesAroundTheCircle () {
			Assert.AreEqual(0f, HexCoord.GetClosestDirectionIndexFloat(HexCoord.zero, HexCoord.Direction(0) * 2));
			// Halfway between directions 5 and 0 is 5.5, not 2.5.
			Assert.AreEqual(5.5f, HexCoord.GetClosestDirectionIndexFloat(HexCoord.zero, HexCoord.Diagonal(0)), 1e-4f);
			Assert.AreEqual(0.5f, HexCoord.GetClosestDirectionIndexFloat(HexCoord.zero, HexCoord.Diagonal(5)), 1e-4f);
		}

		[Test]
		public void SignedDeltaDirections () {
			Assert.AreEqual(-0.5f, HexUtils.SignedDeltaDirection(0f, 5.5f), 1e-4f);
			Assert.AreEqual(2, HexUtils.LargestSignedDeltaDirection(0, new[] { 1, 2, 5 }));
		}
	}
}
