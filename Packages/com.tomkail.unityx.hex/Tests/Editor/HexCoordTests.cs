using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UnityX.HexGrid.Tests {
	public class HexCoordTests {
		static readonly HexCoord[] samples = HexCoord.GetPointsInRing(new HexCoord(2, -1), 0, 3);

		static void AssertNear (Vector2 expected, Vector2 actual, string message = null) {
			Assert.Less((expected - actual).magnitude, 1e-3f, $"{message} expected {expected} got {actual}");
		}

		static Vector2 Reflect (Vector2 point, Vector2 axis) {
			axis.Normalize();
			return 2 * Vector2.Dot(point, axis) * axis - point;
		}

		[Test]
		public void ScaleReturnsNewValueWithoutMutating () {
			var a = new HexCoord(2, -3);
			var b = a.Scale(2);
			Assert.AreEqual(new HexCoord(2, -3), a);
			Assert.AreEqual(new HexCoord(4, -6), b);
			Assert.AreEqual(new HexCoord(1, -1), a.Scale(0.5f));
			Assert.AreEqual(new HexCoord(2, -3), a);
		}

		[Test]
		public void DiagonalsLieTwoStepsAwayBeyondTheirCorner () {
			for (int c = 0; c < 6; c++) {
				var diagonal = HexCoord.Diagonal(c);
				Assert.AreEqual(2, diagonal.AxialLength(), $"corner {c}");
				// Points the same way as the corner vector.
				Assert.Greater(Vector2.Dot(diagonal.Position().normalized, HexCoord.CornerVector(c).normalized), 0.999f, $"corner {c}");
			}
			Assert.AreEqual(6, HexCoord.zero.DiagonalNeighbors().Distinct().Count());
			Assert.AreEqual(new HexCoord(3, -2) + HexCoord.Diagonal(4), new HexCoord(3, -2).DiagonalNeighbor(4));
		}

		[Test]
		public void RotateAroundMatchesSextantRotationAndKeepsDistance () {
			var center = new HexCoord(-1, 2);
			for (int i = 0; i < 6; i++)
				for (int k = -6; k <= 6; k++)
					Assert.AreEqual(center + HexCoord.Direction(i + k), (center + HexCoord.Direction(i)).RotateAround(center, k));
			foreach (var p in samples) {
				Assert.AreEqual(HexCoord.Distance(p, center), HexCoord.Distance(p.RotateAround(center, 2), center));
				Assert.AreEqual(p, p.RotateAround(center, 6));
			}
		}

		[Test]
		public void MirrorThroughCornersReflectsAcrossTheCornerAxis () {
			for (int c = 0; c < 6; c++) {
				foreach (var p in samples) {
					AssertNear(Reflect(p.Position(), HexCoord.CornerVector(c)), p.MirrorThroughCorners(c).Position(), $"corner {c} {p}");
					Assert.AreEqual(p, p.MirrorThroughCorners(c).MirrorThroughCorners(c));
				}
			}
		}

		[Test]
		public void MirrorThroughEdgesReflectsAcrossTheDirectionAxis () {
			for (int d = 0; d < 6; d++) {
				Assert.AreEqual(HexCoord.Direction(d), HexCoord.Direction(d).MirrorThroughEdges(d));
				foreach (var p in samples)
					AssertNear(Reflect(p.Position(), HexCoord.Direction(d).Position()), p.MirrorThroughEdges(d).Position(), $"direction {d} {p}");
			}
		}

		[Test]
		public void MirrorAroundCenterKeepsCenterFixed () {
			var center = new HexCoord(4, -2);
			foreach (var p in samples) {
				var offset = p - center;
				Assert.AreEqual(offset.MirrorThroughCorners(1) + center, p.MirrorThroughCorners(center, 1));
				Assert.AreEqual(offset.MirrorThroughEdges(2) + center, p.MirrorThroughEdges(center, 2));
			}
			Assert.AreEqual(center, center.MirrorThroughEdges(center, 3));
		}

		[Test]
		public void RangeIntersectionMatchesBruteForce () {
			var cases = new (HexCoord a, int ra, HexCoord b, int rb)[] {
				(HexCoord.zero, 3, new HexCoord(2, 1), 2),
				(new HexCoord(-2, 0), 4, new HexCoord(3, -3), 3),
				(HexCoord.zero, 1, new HexCoord(5, 0), 1), // disjoint
				(HexCoord.zero, 2, HexCoord.zero, 2),
			};
			foreach (var (a, ra, b, rb) in cases) {
				var expected = new HashSet<HexCoord>(HexCoord.GetPointsInRing(a, 0, ra).Where(p => HexCoord.Distance(p, b) <= rb));
				var actual = HexCoord.GetPointsInRangeIntersection(a, ra, b, rb);
				Assert.AreEqual(expected.Count, actual.Count, $"{a}/{ra} {b}/{rb}");
				Assert.IsTrue(expected.SetEquals(actual), $"{a}/{ra} {b}/{rb}");
			}
			var three = HexCoord.GetPointsInRangeIntersection((HexCoord.zero, 3), (new HexCoord(2, 0), 2), (new HexCoord(0, 2), 2));
			Assert.IsTrue(three.All(p => HexCoord.Distance(p, HexCoord.zero) <= 3 && HexCoord.Distance(p, new HexCoord(2, 0)) <= 2 && HexCoord.Distance(p, new HexCoord(0, 2)) <= 2));
			Assert.IsNotEmpty(three);
		}

		[Test]
		public void EdgeIsSharedWithNeighbourAndMapsBackToBothCells () {
			foreach (var p in samples) {
				for (int i = 0; i < 6; i++) {
					var neighbour = p.Neighbor(i);
					var edge = p.Edge(i);
					Assert.AreEqual(edge, neighbour.Edge(i + 3), $"{p} edge {i}");
					Assert.AreEqual(edge, HexCoordEdge.Between(p, neighbour));
					Assert.AreEqual(i, HexCoord.EdgeIndexBetween(p, neighbour));
					Assert.IsTrue(edge.SeparatesCoords(p, neighbour));
					CollectionAssert.AreEquivalent(new[] { p, neighbour }, edge.GetCoords(), $"{p} edge {i}");
					AssertNear((p.Position() + neighbour.Position()) * 0.5f, edge.Midpoint());
				}
				Assert.AreEqual(6, p.Edges().Distinct().Count());
			}
			Assert.AreEqual(-1, HexCoord.EdgeIndexBetween(HexCoord.zero, new HexCoord(2, 0)));
			Assert.Throws<System.ArgumentException>(() => HexCoordEdge.Between(HexCoord.zero, HexCoord.zero));
		}

		[Test]
		public void HexagonShapeIsTheRange () {
			var center = new HexCoord(1, -3);
			var hexagon = HexShapes.Hexagon(center, 3).ToList();
			Assert.AreEqual(1 + 3 * 3 * 4, hexagon.Count);
			Assert.IsTrue(new HashSet<HexCoord>(hexagon).SetEquals(HexCoord.GetPointsInRing(center, 0, 3)));
		}

		[Test]
		public void TrianglesTileTheParallelogram () {
			const int size = 4;
			var up = HexShapes.Triangle(size).ToList();
			var down = HexShapes.Triangle(size, flipped: true).ToList();
			Assert.AreEqual((size + 1) * (size + 2) / 2, up.Count);
			Assert.AreEqual(up.Count, down.Count);
			Assert.AreEqual(up.Count, up.Distinct().Count());
			// Together they cover the parallelogram, overlapping only on the anti-diagonal q+r=size.
			var parallelogram = new HashSet<HexCoord>(HexShapes.Parallelogram(HexCoord.zero, new HexCoord(size, size)));
			Assert.AreEqual((size + 1) * (size + 1), parallelogram.Count);
			Assert.IsTrue(parallelogram.SetEquals(up.Union(down)));
			Assert.IsTrue(up.Intersect(down).All(p => p.q + p.r == size));
		}

		[Test]
		public void RectangleMatchesOffsetConversion () {
			var rect = new RectInt(-1, 2, 4, 3);
			var cells = HexShapes.Rectangle(rect).ToList();
			Assert.AreEqual(12, cells.Count);
			Assert.AreEqual(12, cells.Distinct().Count());
			foreach (var cell in cells)
				Assert.IsTrue(rect.Contains(cell.ToOffset()), $"{cell} -> {cell.ToOffset()}");
		}
	}
}
