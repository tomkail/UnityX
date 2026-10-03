using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnityX.HexGrid {
	// A run of consecutive direction indices: `arcLength` directions starting at `initialDirectionIndex` and stepping
	// toward higher indices if `signedSteps` is positive, lower if negative. E.g. a sweeping attack covering three
	// neighbours. Ported from ArcadeTactics.
	// Serialization: only the two fields are persisted; the rest are derived.
	[Serializable, DataContract]
	public struct HexArc : IEquatable<HexArc> {
		[DataMember(Order = 0)]
		public int initialDirectionIndex;
		// Number of directions covered, signed by the direction of travel. 0 covers nothing.
		[DataMember(Order = 1)]
		public int signedSteps;

		public int arcLength => Math.Abs(signedSteps);
		public int directionSign => Math.Sign(signedSteps);
		public int finalDirectionIndex => initialDirectionIndex + directionSign * (arcLength - 1);
		public HexCoord initialDirection => HexCoord.Direction(initialDirectionIndex);
		public HexCoord finalDirection => HexCoord.Direction(finalDirectionIndex);

		// Covered indices in order of travel. Not normalised to 0..5; HexCoord.Direction wraps them.
		public IEnumerable<int> directionIndicesCovered {
			get { for (int i = 0; i < arcLength; i++) yield return initialDirectionIndex + i * directionSign; }
		}
		public IEnumerable<HexCoord> directionsCovered {
			get { foreach (var directionIndex in directionIndicesCovered) yield return HexCoord.Direction(directionIndex); }
		}

		public HexArc (int initialDirectionIndex, int signedSteps) {
			this.initialDirectionIndex = initialDirectionIndex;
			this.signedSteps = signedSteps;
		}

		// From an ordered run of indices: takes the first and last, so the indices between are assumed contiguous.
		public HexArc (IEnumerable<int> directionIndicesCovered) {
			initialDirectionIndex = 0;
			signedSteps = 0;
			bool first = true;
			int last = 0;
			foreach (var index in directionIndicesCovered) {
				if (first) initialDirectionIndex = index;
				last = index;
				first = false;
			}
			if (first) return;
			var delta = last - initialDirectionIndex;
			signedSteps = delta + (delta < 0 ? -1 : 1);
		}

		public bool Equals (HexArc other) => initialDirectionIndex == other.initialDirectionIndex && signedSteps == other.signedSteps;
		public override bool Equals (object obj) => obj is HexArc other && Equals(other);
		public override int GetHashCode () => unchecked(initialDirectionIndex * 397 ^ signedSteps);
		public override string ToString () => $"HexArc({initialDirectionIndex}, {signedSteps})";
	}
}
