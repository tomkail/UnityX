using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	// Sources laid out on the song timeline, editable while playing, e.g. "switch pattern at the next bar".
	// Each region plays its source with the source's beat 0 at the region's start, and keeps the notes that start
	// inside the region. Each region's notes get the region's ID as their NoteId.source, so the same pattern in two
	// regions gives different notes.
	public sealed class Arrangement : INoteSource {
		public sealed class Region {
			internal Region(int id, INoteSource source, double startBeat, double endBeat) {
				Id = id;
				Source = source;
				StartBeat = startBeat;
				EndBeat = endBeat;
			}

			public int Id { get; }
			public INoteSource Source { get; }
			public double StartBeat { get; }
			// Positive infinity for a region that never ends
			public double EndBeat { get; internal set; }
		}

		readonly List<Region> regions = new();
		readonly List<NoteInstance> regionNotes = new();
		int nextId = 1;

		public event Action Changed;

		public IReadOnlyList<Region> Regions => regions;

		public Region Add(INoteSource source, double startBeat, double endBeat = double.PositiveInfinity) {
			var region = CreateRegion(source, startBeat, endBeat);
			Changed?.Invoke();
			return region;
		}

		public void Remove(Region region) {
			if (!regions.Remove(region)) return;
			region.Source.Changed -= OnSourceChanged;
			Changed?.Invoke();
		}

		public void SetEnd(Region region, double endBeat) {
			if (!(endBeat > region.StartBeat)) throw new ArgumentOutOfRangeException(nameof(endBeat), "A region must end after it starts");
			region.EndBeat = endBeat;
			Changed?.Invoke();
		}

		// Ends whatever is playing at beat, drops anything queued from beat onwards, and plays source from beat
		public Region SwitchAt(double beat, INoteSource source) {
			for (var i = regions.Count - 1; i >= 0; i--) {
				var region = regions[i];
				if (region.StartBeat >= beat) {
					region.Source.Changed -= OnSourceChanged;
					regions.RemoveAt(i);
				} else if (region.EndBeat > beat) {
					region.EndBeat = beat;
				}
			}
			var added = CreateRegion(source, beat, double.PositiveInfinity);
			Changed?.Invoke();
			return added;
		}

		public void Clear() {
			foreach (var region in regions) region.Source.Changed -= OnSourceChanged;
			regions.Clear();
			Changed?.Invoke();
		}

		public void GetNotes(double startBeat, double endBeat, List<NoteInstance> results) {
			foreach (var region in regions) {
				if (region.StartBeat >= endBeat) continue;
				var end = Math.Min(endBeat, region.EndBeat);
				if (end <= startBeat) continue;
				regionNotes.Clear();
				region.Source.GetNotes(startBeat - region.StartBeat, end - region.StartBeat, regionNotes);
				foreach (var note in regionNotes) {
					// Only notes that start inside the region; a hold may carry on past its end
					if (note.Beat < 0) continue;
					results.Add(note.Offset(region.StartBeat).WithId(note.id.WithSource(region.Id)));
				}
			}
		}

		Region CreateRegion(INoteSource source, double startBeat, double endBeat) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (!(endBeat > startBeat)) throw new ArgumentOutOfRangeException(nameof(endBeat), "A region must end after it starts");
			var region = new Region(nextId++, source, startBeat, endBeat);
			regions.Add(region);
			source.Changed += OnSourceChanged;
			return region;
		}

		void OnSourceChanged() => Changed?.Invoke();
	}
}
