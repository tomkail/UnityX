using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.HexGrid.Tests {
	public class HexPathfindingTests {
		static readonly HashSet<HexCoord> board = new HashSet<HexCoord>(HexShapes.Hexagon(HexCoord.zero, 6));

		static void AssertContiguous (List<HexCoord> path) {
			for (int i = 1; i < path.Count; i++)
				Assert.AreEqual(1, HexCoord.Distance(path[i - 1], path[i]), $"step {i}");
		}

		[Test]
		public void OpenFieldPathIsAsShortAsTheDistance () {
			var goal = new HexCoord(4, -1);
			var path = HexSearch.FindPath(HexCoord.zero, goal, board.Contains);
			Assert.AreEqual(HexCoord.zero, path[0]);
			Assert.AreEqual(goal, path[path.Count - 1]);
			Assert.AreEqual(HexCoord.Distance(HexCoord.zero, goal) + 1, path.Count);
			AssertContiguous(path);
			CollectionAssert.AreEqual(new[] { HexCoord.zero }, HexSearch.FindPath(HexCoord.zero, HexCoord.zero, board.Contains));
		}

		[Test]
		public void PathGoesAroundWallsAndMatchesBreadthFirstLength () {
			var walls = new HashSet<HexCoord>(HexCoord.GetPointsOnLine(new HexCoord(2, -3), HexCoord.Direction(1), 6));
			bool passable (HexCoord c) => board.Contains(c) && !walls.Contains(c);
			var goal = new HexCoord(4, 0);
			var path = HexSearch.FindPath(HexCoord.zero, goal, passable);
			Assert.IsNotNull(path);
			AssertContiguous(path);
			Assert.IsFalse(path.Any(walls.Contains));
			Assert.AreEqual(HexSearch.Reachable(HexCoord.zero, 20, passable)[goal] + 1, path.Count);
		}

		[Test]
		public void UnreachableGoalReturnsNull () {
			var ring = new HashSet<HexCoord>(HexCoord.GetPointsOnRing(HexCoord.zero, 2));
			bool passable (HexCoord c) => board.Contains(c) && !ring.Contains(c);
			Assert.IsNull(HexSearch.FindPath(HexCoord.zero, new HexCoord(4, 0), passable));
			Assert.IsNull(HexSearch.FindPath(HexCoord.zero, new HexCoord(2, 0), passable), "goal itself impassable");
			Assert.IsNull(HexSearch.FindPath(HexCoord.zero, new HexCoord(5, 0), board.Contains, maxCost: 4));
		}

		[Test]
		public void WeightedPathAvoidsExpensiveCells () {
			// A straight line of swamp costs 5 to enter; walking round it is cheaper.
			var swamp = new HashSet<HexCoord> { new HexCoord(1, 0), new HexCoord(2, 0), new HexCoord(3, 0) };
			float cost (HexCoord from, HexCoord to) => swamp.Contains(to) ? 5 : 1;
			var path = HexSearch.FindPath(HexCoord.zero, new HexCoord(4, 0), board.Contains, cost);
			Assert.IsFalse(path.Any(swamp.Contains));
			AssertContiguous(path);
			var costs = HexSearch.ReachableWithinCost(HexCoord.zero, 100, (a, b) => board.Contains(b) ? cost(a, b) : float.PositiveInfinity);
			var pathCost = 0f;
			for (int i = 1; i < path.Count; i++) pathCost += cost(path[i - 1], path[i]);
			Assert.AreEqual(costs[new HexCoord(4, 0)], pathCost, 1e-4f);
		}

		[Test]
		public void ReachableWithinCostRespectsBudgetAndForbiddenSteps () {
			var costs = HexSearch.ReachableWithinCost(HexCoord.zero, 2, (a, b) => 1);
			Assert.IsTrue(new HashSet<HexCoord>(costs.Keys).SetEquals(HexCoord.GetPointsInRing(HexCoord.zero, 0, 2)));
			var walled = HexSearch.ReachableWithinCost(HexCoord.zero, float.PositiveInfinity, (a, b) => board.Contains(b) ? 1 : float.PositiveInfinity);
			Assert.IsTrue(new HashSet<HexCoord>(walled.Keys).SetEquals(board));
		}
	}

	public class HexMapTests {
		[Test]
		public void BuildsFromAShapeAndLooksUpCells () {
			var map = new HexMap<int>(HexShapes.Hexagon(HexCoord.zero, 2), c => c.q * 10 + c.r);
			Assert.AreEqual(19, map.Count);
			Assert.AreEqual(11, map[new HexCoord(1, 1)]);
			Assert.IsFalse(map.Contains(new HexCoord(3, 0)));
			Assert.AreEqual(-1, map.GetValueOrDefault(new HexCoord(3, 0), -1));
			Assert.Throws<KeyNotFoundException>(() => { var _ = map[new HexCoord(3, 0)]; });
			Assert.AreEqual(6, map.Neighbors(HexCoord.zero).Count());
			Assert.AreEqual(3, map.Neighbors(new HexCoord(2, 0)).Count());
			Assert.IsTrue(new HashSet<HexCoord>(map.BorderCells()).SetEquals(HexCoord.GetPointsOnRing(HexCoord.zero, 2)));
		}

		[System.Serializable]
		class Holder { public HexMap<float> map = new HexMap<float>(); }

		[Test]
		public void SurvivesUnitySerialization () {
			var holder = new Holder();
			holder.map[new HexCoord(1, -2)] = 3.5f;
			holder.map[new HexCoord(-4, 0)] = -1f;
			var copy = JsonUtility.FromJson<Holder>(JsonUtility.ToJson(holder));
			Assert.AreEqual(2, copy.map.Count);
			Assert.AreEqual(3.5f, copy.map[new HexCoord(1, -2)]);
			Assert.AreEqual(-1f, copy.map[new HexCoord(-4, 0)]);
		}
	}
}
