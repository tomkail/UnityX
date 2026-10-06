using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.Rhythm {
	public enum WindowUnit {
		// Real seconds, the same at any tempo or playback rate
		Seconds,
		// Beats, so the windows tighten as the tempo rises
		Beats
	}

	[Serializable]
	public class JudgementGrade {
		public string name;
		[Tooltip("How early a hit can be and still get this grade, as a positive amount")]
		public double early;
		[Tooltip("How late a hit can be and still get this grade")]
		public double late;

		public JudgementGrade() {}

		public JudgementGrade(string name, double early, double late) {
			this.name = name;
			this.early = early;
			this.late = late;
		}

		public bool Contains(double offset) => offset >= -early && offset <= late;
	}

	// The grades a hit can get, narrowest first: a hit gets the first grade whose window contains it
	[Serializable]
	public class JudgementWindows {
		public WindowUnit unit = WindowUnit.Seconds;
		public List<JudgementGrade> grades = new() {
			new JudgementGrade("Perfect", 0.025, 0.025),
			new JudgementGrade("Great", 0.06, 0.06),
			new JudgementGrade("Good", 0.1, 0.1)
		};
		[Tooltip("How long before a hold's end it can be released and still count as completed, in the same unit")]
		public double holdReleaseEarly = 0.1;

		// The furthest a hit can be from its note and still count
		public double WidestEarly {
			get {
				var widest = 0.0;
				foreach (var grade in grades) widest = Math.Max(widest, grade.early);
				return widest;
			}
		}

		public double WidestLate {
			get {
				var widest = 0.0;
				foreach (var grade in grades) widest = Math.Max(widest, grade.late);
				return widest;
			}
		}

		// The grade for an offset (hit minus note, in this unit), or -1 if it's outside every window
		public int GradeFor(double offset) {
			for (var i = 0; i < grades.Count; i++) {
				if (grades[i].Contains(offset)) return i;
			}
			return -1;
		}

		// 1 for a hit dead on its note, falling to 0 at the edge of the widest window on that side
		public double AccuracyFor(double offset) {
			var edge = offset < 0 ? WidestEarly : WidestLate;
			return edge > 0 ? Math.Clamp(1 - Math.Abs(offset) / edge, 0, 1) : offset == 0 ? 1 : 0;
		}
	}
}
