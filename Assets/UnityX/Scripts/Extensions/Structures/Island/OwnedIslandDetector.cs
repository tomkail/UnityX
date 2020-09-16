using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityX.Geometry;

// Finds islands "owned" by a specific property of the coord, such as the land type
public class OwnedIslandDetector<Coord, Owner> : IslandDetector<Coord> where Coord : IEquatable<Coord> {
	static new List<OwnedIsland<Coord, Owner>> islands = new List<OwnedIsland<Coord, Owner>>();
	
	public Func<Coord, Owner> GetPointOwner;
	
	public OwnedIslandDetector (IEnumerable<Coord> startPoints, Func<Coord, IEnumerable<Coord>> GetAdjacentPoints, Func<Coord, bool> GetPointIsValid, Func<Coord, Owner> GetPointOwner) : base (startPoints, GetAdjacentPoints, GetPointIsValid) {
		this.GetAdjacentPoints = GetAdjacentPoints;
		this.GetPointOwner = GetPointOwner;
		Debug.Assert(GetAdjacentPoints != null);
		Debug.Assert(GetPointOwner != null);
	}

	public new List<OwnedIsland<Coord, Owner>> FindIslands () {
		islands.Clear();
		testedPoints.Clear();
		islandStartPointsToTest.Clear();

		islandStartPointsToTest.AddRange(startPoints);
		while(islandStartPointsToTest.Count > 0) {
			Coord pointToTest = islandStartPointsToTest.First();
			var ownedIsland = CreateIsland(GetPointOwner(pointToTest));
			ConnectAdjacentTilesWithSameOwner(ownedIsland, pointToTest);
		}
		return islands;
	}

	OwnedIsland<Coord, Owner> CreateIsland (Owner owner) {
		var ownedIsland = new OwnedIsland<Coord, Owner>(owner, new List<Coord>());
		islands.Add(ownedIsland);
		return ownedIsland;
	}
	
	void ConnectAdjacentTilesWithSameOwner (OwnedIsland<Coord, Owner> island, Coord gridPoint) {
		island.points.Add(gridPoint);
		islandStartPointsToTest.Remove (gridPoint);
		
		testedPoints.Add (gridPoint);
		var validAdjacent = GetAdjacentPoints(gridPoint);
		foreach(Coord adjacentPoint in validAdjacent) {
			TryConnectTileWithSameOwner(island, adjacentPoint);
		}
	}

	void TryConnectTileWithSameOwner (OwnedIsland<Coord, Owner> island, Coord gridPoint) {
		if(testedPoints.Contains(gridPoint)) return;
		if(!GetPointIsValid(gridPoint)) return;

		bool alreadyCheckedInIsland = island.points.Contains(gridPoint);
		if(!alreadyCheckedInIsland) {
			if(island.owner.Equals(GetPointOwner(gridPoint))) {
				ConnectAdjacentTilesWithSameOwner(island, gridPoint);
				return;
			}
		}

		bool markedToCheck = islandStartPointsToTest.Contains(gridPoint);
		if(!markedToCheck) {
			islandStartPointsToTest.Add (gridPoint);
			return;
		}
	}
}