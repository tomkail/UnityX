using System;
using System.Collections.Generic;

public class OwnedIsland<Coord, T> : Island<Coord> where Coord : IEquatable<Coord> {
	public T owner;
	public OwnedIsland (T owner, List<Coord> islandPoints) : base (islandPoints) {
		this.owner = owner;
	}

    public override string ToString() {
		return string.Format("[{0}] Owner={1} List={2}", GetType().Name, owner, DebugX.ListAsString(points));
	}

/*
	public Polygon GetOutline () {
		var solver = new OutlineSolver(this);
		return solver.GetOutline();
	}
	class OutlineSolver<Coord> {
		OwnedIsland island;
		List<Vector2> polygonPoints = new List<Vector2>();
		Coord startEdge = default(Coord);

		public OutlineSolver (Island island) {
			this.island = island;
		}

		public Polygon GetOutline () {
			Coord startDirection = Point.zero;
			foreach(var point in island.points) {
				if(point.x == island.pointBounds.xMin || point.x == island.pointBounds.xMax || point.y == island.pointBounds.yMin || point.y == island.pointBounds.yMax) {
					if(point.x == island.pointBounds.xMin) {
						startEdge.x = point.x;
						startDirection = Vector2.right;
					} else if(point.x == island.pointBounds.xMax) {
						startEdge.x = point.x + 1;
						startDirection = Vector2.right;
					}
					if(point.y == island.pointBounds.yMin) {
						startEdge.x = point.y;
						startDirection = Vector2.up;
					} else if(point.y == island.pointBounds.yMax) {
						startEdge.y = point.y + 1;
						startDirection = Vector2.up;
					}
					startEdge = point;
					break;
				}
			}

			foreach(var direction in Point.CardinalDirections()) {
				Point newEdge = startEdge + direction;
				int numContacts = CornerPointTouchingPoint(newEdge);
				if(numContacts > 0 && numContacts < 4) {
					startDirection = direction;
					break;
				}
			}
			polygonPoints.Add(startEdge);
			RecursivelyFindNextPoint(startEdge + startDirection, startDirection);
			return new Polygon(polygonPoints.ToArray());
		}
		public void RecursivelyFindNextPoint (Point currentEdge, Point currentDirection) {
			polygonPoints.Add(currentEdge);
			var cardinalPoints = new Point[] {
				new Point(-1, 0),
				new Point(0, 1),
				new Point(1, 0)
			};
			foreach(var direction in cardinalPoints) {
				var relativeDirection = GetRelativeDirection(direction, currentDirection);
				Debug.Assert(-currentDirection != relativeDirection);
				Point newEdge = currentEdge + relativeDirection;
				int numContacts = CornerPointTouchingPoint(newEdge);
				if(numContacts > 0 && numContacts < 4) {
					currentDirection = relativeDirection;
					if(newEdge == startEdge) return;
					else {
						RecursivelyFindNextPoint(newEdge, currentDirection);
						break;
					}
				}
			}
		}

		public int CornerPointTouchingPoint (Point cornerPoint) {
			int numPoints = 0;
			var ordinal = Point.OrdinalDirections();
			foreach(var offset in ordinal) {
				Point point = new Point(((Vector2)cornerPoint - Vector2X.half) + (Vector2)offset * 0.5f);
				if(island.points.Contains(point)) 
					numPoints++;
			}
			return numPoints;
		}
		// Theres gotta be a better way to do this :/
		public Point GetRelativeDirection (Vector2 direction, Vector2 relativeTo) {
			var degrees = Vector2X.Degrees(relativeTo);
			return Vector2X.Rotate(direction, degrees);
		}
	} */
}
