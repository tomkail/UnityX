using System;
using System.Collections.Generic;
using System.Linq;

// Finds contiguous "islands" from a point cloud
public class IslandDetector<Coord> where Coord : IEquatable<Coord> {

	protected static List<Island<Coord>> islands = new List<Island<Coord>>();
	protected static HashSet<Coord> testedPoints = new HashSet<Coord>();
	protected static List<Coord> islandStartPointsToTest = new List<Coord>();

	public IEnumerable<Coord> startPoints;
	public Func<Coord, IEnumerable<Coord>> GetAdjacentPoints;
	public Func<Coord, bool> GetPointIsValid;

	public IslandDetector (IEnumerable<Coord> startPoints, Func<Coord, IEnumerable<Coord>> GetAdjacentPoints, Func<Coord, bool> GetPointIsValid) {
		this.startPoints = startPoints;
		this.GetAdjacentPoints = GetAdjacentPoints;
		this.GetPointIsValid = GetPointIsValid;
	}

	public List<Island<Coord>> FindIslands () {
		islands.Clear();
		testedPoints.Clear();
		islandStartPointsToTest.Clear();

		islandStartPointsToTest.AddRange(startPoints);
		while(islandStartPointsToTest.Count > 0) {
			Coord pointToTest = islandStartPointsToTest[0];
			Island<Coord> island = new Island<Coord>();
			TryConnectTile(island, pointToTest);
			if(island.points.Any()) islands.Add(island);
		}
		return islands;
	}
	
	void TryConnectAdjacentTiles (Island<Coord> island, Coord gridPoint) {
		var adjacentPoints = GetAdjacentPoints(gridPoint);
		foreach(Coord adjacentPoint in adjacentPoints) {
			TryConnectTile(island, adjacentPoint);
		}
	}

	void TryConnectTile (Island<Coord> island, Coord gridPoint) {
		islandStartPointsToTest.Remove (gridPoint);
		if (testedPoints.Contains(gridPoint)) return;
		
		testedPoints.Add (gridPoint);
		if(!GetPointIsValid(gridPoint)) return;

		bool alreadyCheckedInIsland = island.points.Contains(gridPoint);
		if(!alreadyCheckedInIsland) {
			island.points.Add(gridPoint);
			TryConnectAdjacentTiles(island, gridPoint);
			return;
		}

		bool markedToCheck = islandStartPointsToTest.Contains(gridPoint);
		if(!markedToCheck) {
			islandStartPointsToTest.Add (gridPoint);
			return;
		}
	}
}