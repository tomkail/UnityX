using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.HexGrid {
	// One value per cell, for data every cell has: terrain, height, fog, flow fields. Backed by a dictionary, so any
	// map shape works and cells off the map are simply absent.
	//
	// Unity serializes it (as parallel key/value lists), so it can be an inspector field when T is serializable.
	// Not the right home for entities that move around: those should own their position, with a coord -> entities
	// lookup kept alongside and updated in the one place that moves them.
	[Serializable]
	public class HexMap<T> : IEnumerable<KeyValuePair<HexCoord, T>>, ISerializationCallbackReceiver {
		Dictionary<HexCoord, T> cells = new Dictionary<HexCoord, T>();

		[SerializeField] List<HexCoord> serializedCoords = new List<HexCoord>();
		[SerializeField] List<T> serializedValues = new List<T>();

		public HexMap () {}

		// A map over `coords`, e.g. HexShapes.Hexagon(HexCoord.zero, 5), with each cell's value from `create`.
		public HexMap (IEnumerable<HexCoord> coords, Func<HexCoord, T> create) {
			foreach (var coord in coords) cells[coord] = create(coord);
		}

		public int Count => cells.Count;
		public IEnumerable<HexCoord> Coords => cells.Keys;
		public IEnumerable<T> Values => cells.Values;

		// Throws KeyNotFoundException for a cell off the map; use TryGetValue or GetValueOrDefault to check.
		public T this[HexCoord coord] {
			get => cells[coord];
			set => cells[coord] = value;
		}

		public bool Contains (HexCoord coord) => cells.ContainsKey(coord);
		public bool TryGetValue (HexCoord coord, out T value) => cells.TryGetValue(coord, out value);
		public T GetValueOrDefault (HexCoord coord, T fallback = default) => cells.TryGetValue(coord, out var value) ? value : fallback;
		public bool Remove (HexCoord coord) => cells.Remove(coord);
		public void Clear () => cells.Clear();

		// Neighbours of `coord` that are on the map, in direction order.
		public IEnumerable<HexCoord> Neighbors (HexCoord coord) {
			for (int i = 0; i < 6; i++) {
				var neighbor = coord.Neighbor(i);
				if (cells.ContainsKey(neighbor)) yield return neighbor;
			}
		}

		// Cells on the map whose edge faces off it.
		public IEnumerable<HexCoord> BorderCells () {
			foreach (var coord in cells.Keys) {
				for (int i = 0; i < 6; i++) {
					if (cells.ContainsKey(coord.Neighbor(i))) continue;
					yield return coord;
					break;
				}
			}
		}

		public IEnumerator<KeyValuePair<HexCoord, T>> GetEnumerator () => cells.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();

		void ISerializationCallbackReceiver.OnBeforeSerialize () {
			serializedCoords.Clear();
			serializedValues.Clear();
			foreach (var pair in cells) {
				serializedCoords.Add(pair.Key);
				serializedValues.Add(pair.Value);
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize () {
			cells = new Dictionary<HexCoord, T>(serializedCoords.Count);
			int count = Mathf.Min(serializedCoords.Count, serializedValues.Count);
			for (int i = 0; i < count; i++) cells[serializedCoords[i]] = serializedValues[i];
		}
	}
}
