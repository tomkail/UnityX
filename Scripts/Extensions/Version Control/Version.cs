namespace UnityX.Versioning
{
    [System.Serializable]
    public struct Version {
		public int major;
		public int minor;
		public int build;
		
        [InfoAttribute("For demos or other special versions")]
		[Disable]
		public string buildType;
		[Disable]
		public string platform;
		[Disable]
		public string buildTarget;

        [Disable]
		public bool isDevelopment;

		[Disable]
		public string gitBranch;
		[Disable]
		public string gitCommitSHA;
		[Disable]
		public string buildDateTimeString;
		[Disable]
		public string inkCompileDateTimeString;

		public string ToBasicVersionString () {
			return string.Format ("{0}.{1}.{2}", major, minor, build);
		}
		public override string ToString () {
			return string.Format ("Version {0}.{1}.{2}{3} {4} {5}", major, minor, build, string.IsNullOrWhiteSpace(buildType) ? "" : " ("+buildType+")", gitBranch, gitCommitSHA);
		}
	}
}