using UnityEngine;
using System.Collections;

public static class SystemInfoX {
	
	public static bool IsMacOS {
		get {
			return UnityEngine.SystemInfo.operatingSystem.Contains("Mac OS");
		}
	}
	
	public static bool IsWinOS {
		get {
			return UnityEngine.SystemInfo.operatingSystem.Contains("Windows");
		}
	}
}