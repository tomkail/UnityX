using System;
using System.Collections.Generic;

namespace UnityX.Rhythm {
	// Where notes come from: a looping pattern, a chart, a beat grid or an arrangement of them
	public interface INoteSource {
		// Adds every note that overlaps [startBeat, endBeat) in song beats (see Note.Overlaps), in no particular order
		void GetNotes(double startBeat, double endBeat, List<NoteInstance> results);
		// The notes changed. Tempo, rate and seek changes don't move notes in beats, so they don't raise this.
		event Action Changed;
	}
}
