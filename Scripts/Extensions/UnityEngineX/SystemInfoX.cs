using UnityEngine;

public static class SystemInfoX {

	public static bool IsMacOS {
		get {
			return SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;
		}
	}

	public static bool IsWinOS {
		get {
			return SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows;
		}
	}
}
