using UnityEngine;
using UnityEditor;
using System.Collections;

public static class ConsoleX {

//	[MenuItem ("Tools/Clear Console %#c")] // CMD + SHIFT + C
	public static void Clear () {
		// This simply does "LogEntries.Clear()" the long way:
		var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
		var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
		clearMethod.Invoke(null,null);
	}
}