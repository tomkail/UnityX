using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityX.Rhythm {
	// A recording of the audio clock: (realtime, dspTime) samples plus labelled marks (e.g. MIDI arrivals).
	// Text format, one entry per line:
	//   # rhythm-clock-trace v1 buffer=<seconds>
	//   s <realtime> <dspTime>
	//   m <realtime> <label>
	public sealed class ClockTrace {
		public double bufferDuration;
		public readonly List<(double realtime, double dspTime)> samples = new();
		public readonly List<(double realtime, string label)> marks = new();

		public double StartRealtime => samples[0].realtime;
		public double EndRealtime => samples[samples.Count - 1].realtime;

		// dspTime as it read at `realtime`: the latest sample at or before it
		public double DspTimeAt(double realtime) {
			int lo = 0, hi = samples.Count - 1;
			if (realtime <= samples[0].realtime) return samples[0].dspTime;
			while (lo < hi) {
				var mid = (lo + hi + 1) / 2;
				if (samples[mid].realtime <= realtime) lo = mid; else hi = mid - 1;
			}
			return samples[lo].dspTime;
		}

		public static ClockTrace Parse(string text) {
			var trace = new ClockTrace();
			using var reader = new StringReader(text);
			string line;
			while ((line = reader.ReadLine()) != null) {
				line = line.Trim();
				if (line.Length == 0) continue;
				if (line.StartsWith("#")) {
					var bufferIndex = line.IndexOf("buffer=", StringComparison.Ordinal);
					if (bufferIndex >= 0) trace.bufferDuration = double.Parse(line.Substring(bufferIndex + 7).Split(' ')[0], CultureInfo.InvariantCulture);
					continue;
				}
				var parts = line.Split(' ');
				if (parts[0] == "s") trace.samples.Add((double.Parse(parts[1], CultureInfo.InvariantCulture), double.Parse(parts[2], CultureInfo.InvariantCulture)));
				else if (parts[0] == "m") trace.marks.Add((double.Parse(parts[1], CultureInfo.InvariantCulture), parts.Length > 2 ? string.Join(" ", parts, 2, parts.Length - 2) : ""));
			}
			if (trace.samples.Count == 0) throw new FormatException("Clock trace has no samples");
			return trace;
		}

		public string Serialize() {
			var sb = new StringBuilder();
			sb.Append("# rhythm-clock-trace v1 buffer=").AppendLine(bufferDuration.ToString("R", CultureInfo.InvariantCulture));
			foreach (var (realtime, dspTime) in samples) sb.Append("s ").Append(realtime.ToString("R", CultureInfo.InvariantCulture)).Append(' ').AppendLine(dspTime.ToString("R", CultureInfo.InvariantCulture));
			foreach (var (realtime, label) in marks) sb.Append("m ").Append(realtime.ToString("R", CultureInfo.InvariantCulture)).Append(' ').AppendLine(label);
			return sb.ToString();
		}
	}
}
