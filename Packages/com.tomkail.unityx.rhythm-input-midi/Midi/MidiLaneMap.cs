using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.Rhythm {
	// Which lane each MIDI note number plays, e.g. a drum kit's notes to a game's lanes
	[CreateAssetMenu(menuName = "UnityX/Rhythm/MIDI Lane Map")]
	public class MidiLaneMap : ScriptableObject {
		[Serializable]
		public class Entry {
			[Range(0, 127)] public int note;
			public int lane;
			[Tooltip("Only use this lane for notes on the channel below")]
			public bool matchChannel;
			[Tooltip("0-15, as Minis numbers them")]
			[Range(0, 15)] public int channel;
		}

		public List<Entry> entries = new();

		// An entry for the note's channel wins over one for the note on any channel
		public bool TryGetLane(int note, int channel, out int lane) {
			Entry best = null;
			foreach (var entry in entries) {
				if (entry.note != note) continue;
				if (entry.matchChannel) {
					if (entry.channel != channel) continue;
					best = entry;
					break;
				}
				best ??= entry;
			}
			lane = best?.lane ?? 0;
			return best != null;
		}
	}
}
