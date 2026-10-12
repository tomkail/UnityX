using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.HexGrid.Tests {
	// The pure maths behind the inspector diagrams: orientation must match HexCoord's direction convention, hit
	// testing must land on the side drawn, and the arc / map helpers must agree with the runtime types.
	public class HexDiagramTests {
		static readonly Vector2 centre = new Vector2(100f, 50f);
		const float radius = 30f;

		[Test]
		public void SidesFaceTheMatchingHexDirection () {
			for (int i = 0; i < 6; i++) {
				var world = HexCoord.Direction(i).Position().normalized;
				var expected = new Vector2(world.x, -world.y); // GUI space is y-down
				Assert.That(Vector2.Distance(expected, HexDiagram.SideNormal(i)), Is.LessThan(1e-4f), $"side {i}");
			}
		}

		[Test]
		public void SideEndpointsAreCornersEitherSideOfTheMidpoint () {
			for (int i = 0; i < 6; i++) {
				HexDiagram.SideEndpoints(centre, radius, i, out var a, out var b);
				Assert.AreEqual(radius, Vector2.Distance(a, b), 1e-3f, "a pointy hexagon's side equals its circumradius");
				Assert.That(Vector2.Distance((a + b) * 0.5f, HexDiagram.SideMidpoint(centre, radius, i)), Is.LessThan(1e-3f));
			}
		}

		[Test]
		public void SideAtPointHitsTheSideUnderThePoint () {
			for (int i = 0; i < 6; i++) {
				Assert.AreEqual(i, HexDiagram.SideAtPoint(centre, radius, HexDiagram.SideMidpoint(centre, radius, i)), $"midpoint of {i}");
				// Just inside each end of the side still belongs to it.
				HexDiagram.SideEndpoints(centre, radius, i, out var a, out var b);
				var mid = HexDiagram.SideMidpoint(centre, radius, i);
				Assert.AreEqual(i, HexDiagram.SideAtPoint(centre, radius, Vector2.Lerp(mid, a, 0.9f)));
				Assert.AreEqual(i, HexDiagram.SideAtPoint(centre, radius, Vector2.Lerp(mid, b, 0.9f)));
			}
			Assert.AreEqual(-1, HexDiagram.SideAtPoint(centre, radius, centre), "dead zone at the centre");
			Assert.AreEqual(-1, HexDiagram.SideAtPoint(centre, radius, centre + Vector2.right * radius * 2f), "well outside");
			Assert.AreEqual(0, HexDiagram.SideAtPoint(centre, radius, centre + Vector2.right * radius * 0.6f));
			Assert.AreEqual(1, HexDiagram.SideAtPoint(centre, radius, centre + new Vector2(0.5f, -0.866f) * radius * 0.6f), "up-right on screen is side 1");
		}

		[Test]
		public void FitRadiusKeepsTheHexagonInsideTheRect () {
			var rect = new Rect(0, 0, 58, 58);
			float r = HexDiagram.FitRadius(rect, 2f);
			for (int i = 0; i < 6; i++) Assert.IsTrue(rect.Contains(HexDiagram.Corner(rect.center, r, i)));
			Assert.AreEqual(27f, r, 1e-3f);
		}

		static int[] Covered (int start, int steps) {
			var covered = HexDiagram.ArcCoverage(start, steps);
			return Enumerable.Range(0, 6).Where(i => covered[i]).ToArray();
		}

		[Test]
		public void ArcCoverageWrapsAndFollowsTheSign () {
			CollectionAssert.AreEqual(new[] { 0, 1, 5 }, Covered(5, 3));
			CollectionAssert.AreEqual(new[] { 0, 1 }, Covered(1, -2));
			CollectionAssert.IsEmpty(Covered(3, 0));
			CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5 }, Covered(0, 8));
			CollectionAssert.AreEqual(new[] { 4 }, Covered(-2, 1), "negative start wraps");
		}

		[Test]
		public void StepsToReachStretchesTheArcInItsDirectionOfTravel () {
			Assert.AreEqual(3, HexDiagram.StepsToReach(1, 1, 3));
			Assert.AreEqual(6, HexDiagram.StepsToReach(1, 2, 0), "reaching back past the start goes the long way round");
			Assert.AreEqual(-3, HexDiagram.StepsToReach(1, -1, 5));
			Assert.AreEqual(3, HexDiagram.StepsToReach(4, 0, 6), "an empty arc grows counter-clockwise");
			Assert.AreEqual(0, HexDiagram.StepsToReach(2, 1, 2), "clicking a lone start empties the arc");
			Assert.AreEqual(1, HexDiagram.StepsToReach(2, 3, 2), "clicking the start of a longer arc shrinks it to the start");
			// Whatever we clicked is now the final side.
			for (int target = 0; target < 6; target++) {
				var arc = new HexArc(4, HexDiagram.StepsToReach(4, -2, target));
				if (arc.signedSteps != 0) Assert.AreEqual(target, HexDiagram.Mod(arc.finalDirectionIndex, 6));
			}
		}

		[Test]
		public void ReversedCoversTheSameSidesFromTheOtherEnd () {
			var arc = new HexArc(5, 3);
			var reversed = HexDiagram.Reversed(arc);
			Assert.AreEqual(new HexArc(1, -3), reversed);
			CollectionAssert.AreEqual(HexDiagram.ArcCoverage(5, 3), HexDiagram.ArcCoverage(reversed.initialDirectionIndex, reversed.signedSteps));
			Assert.AreEqual(arc, HexDiagram.Reversed(reversed));
			Assert.AreEqual(new HexArc(2, 0), HexDiagram.Reversed(new HexArc(2, 0)));
		}

		[Test]
		public void RotatedMatchesHexEdgeDirectionsRotate () {
			var sides = new[] { true, false, true, true, false, false };
			foreach (var offset in new[] { 1, -1, 3, 7 }) {
				var directions = new HexEdgeDirections(sides);
				directions.Rotate(offset);
				CollectionAssert.AreEqual(directions.directions.ToArray(), HexDiagram.Rotated(sides, offset), $"offset {offset}");
			}
			CollectionAssert.AreEqual(new[] { false, true, false, false, false, false }, HexDiagram.Rotated(new[] { true, false, false, false, false, false }, 1), "+1 is counter-clockwise: side 0 moves to 1");
		}

		[Test]
		public void SidesToStringListsSetSides () {
			Assert.AreEqual("0, 2, 4", HexDiagram.SidesToString(new[] { true, false, true, false, true, false }));
			Assert.AreEqual("none", HexDiagram.SidesToString(new bool[6]));
		}

		[Test]
		public void OverriddenIndicesFlagsEveryEarlierDuplicate () {
			var a = new HexCoord(1, 0);
			var b = new HexCoord(0, 1);
			var coords = new List<HexCoord> { a, b, a, HexCoord.zero, a };
			CollectionAssert.AreEquivalent(new[] { 0, 2 }, HexDiagram.OverriddenIndices(coords));
			CollectionAssert.IsEmpty(HexDiagram.OverriddenIndices(new List<HexCoord> { a, b }));
		}

		[Test]
		public void NextFreeCoordSpiralsOutFromTheOrigin () {
			Assert.AreEqual(HexCoord.zero, HexDiagram.NextFreeCoord(new HashSet<HexCoord>()));
			var used = new HashSet<HexCoord> { HexCoord.zero };
			var next = HexDiagram.NextFreeCoord(used);
			Assert.AreEqual(1, HexCoord.Distance(HexCoord.zero, next));
			var filled = new HashSet<HexCoord>(HexShapes.Hexagon(HexCoord.zero, 1));
			var beyond = HexDiagram.NextFreeCoord(filled);
			Assert.IsFalse(filled.Contains(beyond));
			Assert.AreEqual(2, HexCoord.Distance(HexCoord.zero, beyond));
		}

		[Test]
		public void PreviewPlacementRoundTripsAndFits () {
			var coords = HexShapes.Hexagon(new HexCoord(3, -2), 3).ToList();
			var rect = new Rect(10, 20, 300, 96);
			float scale = HexDiagram.FitCoords(coords, rect, 14f, out var origin);
			Assert.That(scale, Is.GreaterThan(0f).And.LessThanOrEqualTo(14f));
			foreach (var coord in coords) {
				var p = origin + HexDiagram.CoordToDiagram(coord) * scale;
				Assert.IsTrue(rect.Contains(p), $"{coord} at {p}");
				Assert.AreEqual(coord, HexDiagram.DiagramToCoord(p, origin, scale));
				Assert.AreEqual(coord, HexDiagram.DiagramToCoord(p + new Vector2(0.3f, -0.4f) * scale, origin, scale), "anywhere well inside the cell");
			}
		}

		[Test]
		public void AngleRangeValidationAndTotals () {
			Assert.IsTrue(HexDiagram.IsValidAngleRange(new Vector2(0, 360)));
			Assert.IsFalse(HexDiagram.IsValidAngleRange(new Vector2(-10, 20)));
			Assert.IsFalse(HexDiagram.IsValidAngleRange(new Vector2(30, 20)));
			Assert.IsFalse(HexDiagram.IsValidAngleRange(new Vector2(300, 400)));
			var arc = new AngleArc(-30, 60); // split at the wrap into (330, 360) and (0, 60)
			Assert.AreEqual(90f, HexDiagram.TotalDegrees(arc.ranges), 1e-4f);
			Assert.AreEqual(10f, HexDiagram.TotalDegrees(new[] { new Vector2(0, 10), new Vector2(50, 40) }), 1e-4f, "invalid ranges don't count");
		}
	}
}
