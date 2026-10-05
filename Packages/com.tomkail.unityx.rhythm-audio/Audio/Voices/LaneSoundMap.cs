using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.Rhythm {
	// Which sound each lane plays, optionally different per Note.data (e.g. accents, or left and right hands)
	[CreateAssetMenu(menuName = "UnityX/Rhythm/Lane Sound Map")]
	public class LaneSoundMap : ScriptableObject {
		[Serializable]
		public class Entry {
			public int lane;
			[Tooltip("Only use this sound for notes whose data matches")]
			public bool matchData;
			public int data;
			public AudioClip clip;
			[Range(0, 1)] public float volume = 1;
		}

		public List<Entry> entries = new();

		// An entry matching the note's lane and data wins over one for the lane alone
		public bool TryGetSound(Note note, out AudioClip clip, out float volume) {
			Entry best = null;
			foreach (var entry in entries) {
				if (entry.lane != note.lane || entry.clip == null) continue;
				if (entry.matchData) {
					if (entry.data != note.data) continue;
					best = entry;
					break;
				}
				best ??= entry;
			}
			clip = best?.clip;
			volume = best?.volume ?? 0;
			return best != null;
		}
	}
}
