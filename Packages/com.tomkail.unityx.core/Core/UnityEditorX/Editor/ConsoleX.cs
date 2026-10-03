using UnityEngine;
using UnityEditor;
using System.Collections;

public static class ConsoleX {

	public static void Clear () {
		// This simply does "LogEntries.Clear()" the long way, since UnityEditor.LogEntries is internal.
		var logEntries = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
		var clearMethod = logEntries?.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
		if(clearMethod == null) {
			Debug.LogWarning("ConsoleX.Clear: UnityEditor.LogEntries.Clear wasn't found in this Unity version.");
			return;
		}
		clearMethod.Invoke(null,null);
	}
}