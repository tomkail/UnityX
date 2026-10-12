using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UnityX.HexGrid {
// Serialization: Unity serializes the private _edgeDirections via [SerializeField]. For other serializers the type
// uses the BCL [DataContract]/[DataMember] attributes (no serializer dependency), which Newtonsoft honours: it
// persists only _edgeDirections, as {"_edgeDirections":[...]}.
[System.Serializable, System.Runtime.Serialization.DataContract]
public class HexEdgeDirections {
	const int numEdges = 6;
	[SerializeField, System.Runtime.Serialization.DataMember(Name = "_edgeDirections")]
	bool[] _edgeDirections = new bool[numEdges];
	// Read-only view over the live array. Cached so repeated access (e.g. road-matching SequenceEqual)
	// doesn't copy into a new List every call; invalidated whenever _edgeDirections is reassigned (Rotate).
	// In-place edits via the indexer / Set / Reverse are reflected automatically.
	ReadOnlyCollection<bool> _directionsView;
	public ReadOnlyCollection<bool> directions => _directionsView ??= System.Array.AsReadOnly(_edgeDirections);


	public bool this[int directionIndex] {
		get {
			return this._edgeDirections[directionIndex];
		} set { 
			if(this._edgeDirections[directionIndex] == value) return;
			this._edgeDirections[directionIndex] = value;
			if(OnChangeDirections != null) OnChangeDirections(this);
		}
	}
	public delegate void OnChangeDirectionsDelegate(HexEdgeDirections edgeDirections);
	public OnChangeDirectionsDelegate OnChangeDirections;

	public int directionCount {
		get {
			int num = 0;
			foreach(var x in _edgeDirections)
				if(x) num++;
			return num;
		}
	}

	public static HexEdgeDirections All {
		get {
			return new HexEdgeDirections(new bool[]{true,true,true,true,true,true});
		}
	}
	public HexEdgeDirections () {}
	public HexEdgeDirections (IList<bool> _roadDirections) {
		Set(_roadDirections);
	}

	protected HexEdgeDirections (HexEdgeDirections model)  {
		Set(model._edgeDirections);
	}
	public HexEdgeDirections Clone () {
		return new HexEdgeDirections(this);
	}

	public void Set(HexEdgeDirections roadDirections) {
		Set(roadDirections._edgeDirections);
	}
	public void Set(IList<bool> newRoadDirections) {
		Debug.Assert(newRoadDirections.Count == numEdges);
		for(int i = 0; i < newRoadDirections.Count; i++) {
			this[i] = newRoadDirections[i];
		}
	}
	public void Add(HexEdgeDirections toAdd) {
		foreach(var d in toAdd.GetValidDirectionIndicies()) {
			this[d] = true;
		}
	}

	public void Rotate (int offset) {
		_edgeDirections = Util.GetShiftedRepeating(_edgeDirections, offset);
		_directionsView = null; // array reassigned; drop the cached view so it rewraps the new one
		if(OnChangeDirections != null) OnChangeDirections(this);
	}
	public void Reverse () {
		// _edgeDirections = 
		System.Array.Reverse(_edgeDirections);
		if(OnChangeDirections != null) OnChangeDirections(this);
	}

	public IEnumerable<int> GetValidDirectionIndicies () {
		return Util.IndexesWhere(_edgeDirections, x => x);
	}

	public bool IsSetInDirection (int directionIndex) {
		// numEdges directly instead of directions.Count, which allocated a fresh List+ReadOnlyCollection
		// on every call just to read a constant 6.
		return this[Util.Mod(directionIndex, numEdges)];
	}

	public static bool Connected (HexCoord coord, IEnumerable<int> validDirections, HexCoord targetCoord) {
        if(HexCoord.Distance(coord, targetCoord) != 1) return false;
		var directionBetweenCoords = HexCoord.GetClosestDirectionIndex(coord, targetCoord);
        foreach(var otherDirection in validDirections) {
			var otherAbsDeltaDirection = Mathf.Abs(HexUtils.SignedDeltaDirection(otherDirection, directionBetweenCoords));
			if(otherAbsDeltaDirection == 0) return true;
		}
        return false;
    }
	
	public static bool Connected (HexCoord coordA, HexEdgeDirections directionsA, HexCoord coordB, HexEdgeDirections directionsB) {
        return Connected(coordA, directionsA.GetValidDirectionIndicies(), coordB, directionsB.GetValidDirectionIndicies());
    }

	public static bool Connected (HexCoord coordA, IEnumerable<int> validDirectionsA, HexCoord coordB, IEnumerable<int> validDirectionsB) {
        if(HexCoord.Distance(coordA, coordB) != 1) return false;
		var directionBetweenCoords = HexCoord.GetClosestDirectionIndex(coordA, coordB);
        foreach(var direction in validDirectionsB) {
			var absDeltaDirection = Mathf.Abs(HexUtils.SignedDeltaDirection(direction, directionBetweenCoords));
			if(absDeltaDirection != 3) continue;
            foreach(var otherDirection in validDirectionsA) {
				var otherAbsDeltaDirection = Mathf.Abs(HexUtils.SignedDeltaDirection(otherDirection, directionBetweenCoords));
				if(otherAbsDeltaDirection == 0) return true;
            }
        }
        return false;
    }

    public override string ToString() {
        return string.Format("[{0}] Valid Directions:{1}", GetType().Name, Util.ListAsString(GetValidDirectionIndicies()));
    }
}
}