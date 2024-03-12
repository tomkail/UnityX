public static class SystemInfoX {
	
	public static bool IsMacOS => UnityEngine.SystemInfo.operatingSystem.Contains("Mac OS");

	public static bool IsWinOS => UnityEngine.SystemInfo.operatingSystem.Contains("Windows");
}