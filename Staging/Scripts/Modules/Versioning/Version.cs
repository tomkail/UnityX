using System;

namespace UnityX.Versioning
{
    [Serializable]
    public struct Version {
		public int major;
		public int minor;
		public int build;
		
		public string buildType;
		public string platform;
		public string buildTarget;

		public bool isDevelopment;

		public string gitBranch;
		public string gitCommitSHA;
		public string buildDateTimeString;
		public string inkCompileDateTimeString;

		public string ToBasicVersionString () {
			return string.Format ("{0}.{1}.{2}", major, minor, build);
		}
		public override string ToString () {
			return string.Format ("Version {0}.{1}.{2}{3} {4} {5}", major, minor, build, string.IsNullOrWhiteSpace(buildType) ? "" : " ("+buildType+")", gitBranch, gitCommitSHA);
		}
	}
}