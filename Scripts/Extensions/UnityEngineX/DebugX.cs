using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = System.Object;

/// <summary>
/// Debug extension class
/// </summary>
public static class DebugX {
	#region
	public static bool debug = true;

	public static bool SoftAssert (Object o) {
		return SoftAssert(o != null, "Warning: SoftAssert failed because object is null");
	}
	
	public static bool SoftAssert(bool condition) {
		return SoftAssert (condition, "Warning: SoftAssert Failed!");
	}
	
	public static bool SoftAssert(bool condition, string message) {
		if (!condition)
			LogWarning (message);
		return condition;
	}
	
	public static bool SoftAssert (Object msg, Object o) {
		return SoftAssert(msg, o != null, "Warning: SoftAssert failed because object is null");
	}
	
	public static bool SoftAssert(Object msg, bool condition) {
		return SoftAssert (msg, condition, "Warning: SoftAssert Failed!");
	}
	
	public static bool SoftAssert(Object msg, bool condition, string message) {
		if (!condition)
			LogWarning (msg, message);
		return condition;
	}


	public static bool Assert (Object o) {
		return Assert(o != null, "Error: Assert failed because object is null");
	}
	
	public static bool Assert(bool condition) {
		return Assert (condition, "Error: Assert Failed!");
	}
	
	public static bool Assert(bool condition, string message) {
		if (!condition)
			LogError (message);
		return condition;
	}
	
	public static bool Assert (Object msg, Object o) {
		return Assert(msg, o != null, "Error: Assert failed because object is null");
	}
	
	public static bool Assert(Object msg, bool condition) {
		return Assert (msg, condition, "Error: Assert Failed!");
	}
	
	public static bool Assert(Object msg, bool condition, string message) {
		if (!condition)
			LogError (msg, message);
		return condition;
	}
	
	/// <summary>Returns a log for the target string, along with the current time and the log source.</summary>
	/// <param name="o">The log source.</param>
	/// <param name="item">The log item.</param>
	/// <returns>The string.</returns>
	public static string LogString (Object msg) {
		StringBuilder sb = new StringBuilder(msg == null ? "NULL" : msg.ToString());
		sb.AppendLine();
		sb.Append("(").Append(Time.unscaledTime.ToString(CultureInfo.CurrentCulture)).Append(")");
		return sb.ToString();
	}
	
	/// <summary>Returns a log for the target string, along with the current time and the log source.</summary>
	/// <param name="o">The log source.</param>
	/// <param name="item">The log item.</param>
	/// <returns>The string.</returns>
	public static string LogString (Object obj, Object msg) {
		StringBuilder sb = new StringBuilder(msg.ToString());
		sb.AppendLine();
		sb.Append("(").Append(Time.unscaledTime.ToString(CultureInfo.CurrentCulture)).Append(")");
		if(obj is Component component) sb.Append(" "+component.transform.HierarchyPath());
		else if(obj is GameObject gameObject) sb.Append(" "+gameObject.transform.HierarchyPath());
		sb.Append(" ("+obj.GetType().Name+")");
		return sb.ToString();
	}
	
	/// <summary>Logs the target string.</summary>
	/// <param name="o">The log source.</param>
	/// <param name="item">The log item.</param>
	public static void Log (Object a) {
		if(!debug) return;
		Debug.Log(LogString(a));
	}
	
	/// <summary>Logs the target string, along with the current time and the log source.</summary>
	/// <param name="o">The log source.</param>
	/// <param name="item">The log item.</param>
	public static void Log (Object obj, Object a) {
		if(!debug) return;
		var logString = LogString(obj, a);
		if(obj is UnityEngine.Object o) Debug.Log(logString, o);
		else Debug.Log(logString);
	}

	/// <summary>Logs a warning with the target string, along with the current time and the log source.</summary>
	/// <param name="o">The log source.</param>
	/// <param name="item">The log item.</param>
	public static void LogWarning (Object a) {
		if(!debug) return;
		Debug.LogWarning(LogString(a));
	}
	
	/// <summary>Logs a warning with the target string, along with the current time and the log source.</summary>
	/// <param name="o">The log source.</param>
	/// <param name="item">The log item.</param>
	public static void LogWarning (Object obj, Object a) {
		if(!debug) return;
		var logString = LogString(obj, a);
		if(obj is UnityEngine.Object o) Debug.LogWarning(logString, o);
		else Debug.LogWarning(logString);
	}
	
	/// <summary>Logs an error with the target string, along with the current time and the log source.</summary>
	/// <param name="o">The log source.</param>
	/// <param name="item">The log item.</param>
	/// <param name="color">The log color.</param>
	public static void LogError (Object a) {
		if(!debug) return;
		Debug.LogError(LogString(a));
	}
	
	/// <summary>Logs an error with the target string, along with the current time and the log source.</summary>
	/// <param name="o">The log source.</param>
	/// <param name="item">The log item.</param>
	/// <param name="color">The log color.</param>
	public static void LogError (Object obj, Object a) {
		if(!debug) return;
		var logString = LogString(obj, a);
		if(obj is UnityEngine.Object o) Debug.LogError(logString, o);
		else Debug.LogError(logString);
	}
	
	public static void LogIfNull (Object o, Object a) {
		if(a == null)
			LogError(o, "Object is null!");
	}

	public static void LogErrorOnce(string errorMessage, UnityEngine.Object context = null) {
		if( !_singleLoggedMessages.Add(errorMessage) ) 
			return;

		Debug.LogError(errorMessage, context);
	}

	public static void LogWarningOnce(string warningMessage, UnityEngine.Object context = null) {
		if( !_singleLoggedMessages.Add(warningMessage) ) 
			return;

		Debug.LogWarning(warningMessage, context);
	}

	public static void LogOnce(string message, UnityEngine.Object context = null) {
		if( !_singleLoggedMessages.Add(message) ) 
			return;

		Debug.Log(message, context);
	}

	static HashSet<string> _singleLoggedMessages = new();

	/// <summary>LogMany</summary>
	/// <param name="list">The objects to log.</param>
	public static void LogMany (params object[] list) {
		if(!debug) return;
		Debug.Log(ListAsString(list));
	}
	
	/// <summary>LogWarningMany</summary>
	/// <param name="list">The objects to log warning.</param>
	public static void LogWarningMany (params object[] list) {
		if(!debug) return;
		Debug.LogWarning(ListAsString(list));
	}
	
	/// <summary>LogErrorMany</summary>
	/// <param name="list">The objects to log error.</param>
	public static void LogErrorMany (params object[] list) {
		if(!debug) return;
		Debug.LogError(ListAsString(list));
	}
	
	/// <summary>Gets a log string from an object array.</summary>
	/// <param name="list">The list of objects to log.</param>
	/// <returns>string</returns>
	public static string ListAsString<T> (IEnumerable<T> list, Func<T, string> toString = null, bool showTypeAndCount = true, bool lineSeparated = true) {
		if(list == null) return "NULL";
		int count = 0;
		foreach(var item in list) count++;
		StringBuilder sb = new StringBuilder();
		if(showTypeAndCount) sb.AppendLine("Displaying list of "+typeof(T).Name+" with "+count +" values:");
		bool first = true;
		foreach(var item in list) {
			var itemStr = item == null ? "NULL" : toString == null ? item.ToString() : toString(item);
			if(lineSeparated) sb.AppendLine(itemStr);
			else {
				if(!first) sb.Append(", ");
				sb.Append(itemStr);
			}
			if(first) first = false;
		}
		return sb.ToString();
	}
	
	/// <summary>Gets a log string from an object array.</summary>
	/// <param name="list">The list of objects to log.</param>
	/// <returns>string</returns>
	public static string DictionaryAsString<TKey, TValue> (IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, string> toString = null, bool showTypeAndCount = true) {
		if(dictionary == null) return "NULL";
		int count = 0;
		foreach(var item in dictionary) count++;
		StringBuilder sb = new StringBuilder();
		if(showTypeAndCount) sb.AppendLine("Displaying dictionary with key "+typeof(TKey).Name+" and value "+typeof(TValue).Name+" with "+count +" values:");
		bool first = true;
		foreach(var item in dictionary) {
			if(toString != null) sb.AppendLine(toString(item));
			else {
				sb.Append("KEY: ");
				sb.Append(item.Key);
				sb.Append(", VALUE: ");
				sb.AppendLine(item.Value.ToString());
			}
			if(first) first = false;
		}
		return sb.ToString();
	}
	
	/// <summary>Clearly logs the contents of a list.</summary>
	/// <typeparam name="T">The generic list type.</typeparam>
	/// <param name="list">The list to log.</param>
	public static void LogList<T> (IEnumerable<T> list, Func<T, string> toString = null, bool showTypeAndCount = true, bool lineSeparated = true) {
		if(list == null || !list.Any()) {
			Debug.Log("List is null or empty");
			return;
		}
		#if !UNITY_WINRT
		string output = ListAsString(list, toString, showTypeAndCount, lineSeparated);
		Debug.Log(output);
		#endif
	}

	/// <summary>Clearly logs the contents of a list.</summary>
	/// <typeparam name="T">The generic list type.</typeparam>
	/// <param name="list">The list to log.</param>
	public static void LogList<T> (string log, IEnumerable<T> list, Func<T, string> toString = null, bool showTypeAndCount = true, bool lineSeparated = true) {
		#if !UNITY_WINRT
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(log);
		if(list == null || !list.Any()) {
			sb.AppendLine("List is null or empty");
		} else {
			sb.AppendLine(ListAsString(list, toString, showTypeAndCount, lineSeparated));
		}
		Debug.Log(sb.ToString());
		#endif
	}


	public static void LogDictionary<TKey,TValue> (IDictionary<TKey,TValue> dictionary, Func<KeyValuePair<TKey, TValue>, string> toString = null, bool showTypeAndCount = true) {
		if(dictionary == null || dictionary.Count == 0) {
			Debug.Log("Dictionary is null or empty");
			return;
		}
		#if !UNITY_WINRT
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("Dictionary ("+dictionary.Count+") : ");
		sb.AppendLine(DictionaryAsString(dictionary, toString, showTypeAndCount));
		Debug.Log(sb.ToString());
		#endif
	}

	/// <summary>Clearly logs the contents of a list.</summary>
	/// <typeparam name="T">The generic list type.</typeparam>
	/// <param name="dictionary">The dictionary to log.</param>
	public static void LogDictionary<TKey, TValue> (string log, Dictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, string> toString = null, bool showTypeAndCount = true) {
		#if !UNITY_WINRT
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(log);
		if(dictionary == null || !dictionary.Any()) {
			sb.AppendLine("Dictionary is null or empty");
		} else {
			sb.AppendLine(DictionaryAsString(dictionary, toString, showTypeAndCount));
		}
		Debug.Log(sb.ToString());
		#endif
	}
	
	
	#endregion
	
	// Following functions taken from UnityWiki, created by Hayden Scott-Baron (Dock) - http://starfruitgames.com
	
	/// <summary>Draws a cube in the scene window.</summary>
	/// <param name="pos">Position of the cube.</param>
	/// <param name="scale">Scale of the cube.</param>
	/// <param name="col">Color of the cube.</param>
	public static void DrawCube (Vector3 pos, Vector3 scale, Color col) {
		Vector3 halfScale = scale * 0.5f; 
		
		Vector3[] points = {
			pos + new Vector3(halfScale.x, 		halfScale.y, 	halfScale.z),
			pos + new Vector3(-halfScale.x, 	halfScale.y, 	halfScale.z),
			pos + new Vector3(-halfScale.x, 	-halfScale.y, 	halfScale.z),
			pos + new Vector3(halfScale.x, 		-halfScale.y, 	halfScale.z),			
			pos + new Vector3(halfScale.x, 		halfScale.y, 	-halfScale.z),
			pos + new Vector3(-halfScale.x, 	halfScale.y, 	-halfScale.z),
			pos + new Vector3(-halfScale.x, 	-halfScale.y, 	-halfScale.z),
			pos + new Vector3(halfScale.x, 		-halfScale.y, 	-halfScale.z),
		};
		
		Debug.DrawLine (points[0], points[1], col); 
		Debug.DrawLine (points[1], points[2], col); 
		Debug.DrawLine (points[2], points[3], col); 
		Debug.DrawLine (points[3], points[0], col); 
	}
	
	/// <summary>Draws a rect in the scene window.</summary>
	/// <param name="rect">Rect to draw.</param>
	/// <param name="col">Color of the rect.</param>
	public static void DrawRect (Rect rect, Color col) {
		Vector3 pos = new Vector3( rect.x + rect.width/2, rect.y + rect.height/2, 0.0f );
		Vector3 scale = new Vector3 (rect.width, rect.height, 0.0f );
		DrawRect (pos, scale, col); 
	}
	
	/// <summary>Draws a rect in the scene window.</summary>
	/// <param name="pos">Position of the rect.</param>
	/// <param name="scale">Scale of the rect.</param>
	/// <param name="col">Color of the rect.</param>
	public static void DrawRect (Vector3 pos, Vector3 scale, Color col) {		
		Vector3 halfScale = scale * 0.5f; 
		Vector3[] points = {
			pos + new Vector3(halfScale.x, 		halfScale.y, 	halfScale.z),
			pos + new Vector3(-halfScale.x, 	halfScale.y, 	halfScale.z),
			pos + new Vector3(-halfScale.x, 	-halfScale.y, 	halfScale.z),
			pos + new Vector3(halfScale.x, 		-halfScale.y, 	halfScale.z),	
		};
		Debug.DrawLine (points[0], points[1], col); 
		Debug.DrawLine (points[1], points[2], col); 
		Debug.DrawLine (points[2], points[3], col); 
		Debug.DrawLine (points[3], points[0], col); 
	}
	
	/// <summary>Draws a point in the scene window.</summary>
	/// <param name="pos">Position of the rect.</param>
	/// <param name="scale">Scale of the rect.</param>
	/// <param name="col">Color of the rect.</param>
	public static void DrawPoint (Vector3 pos, float scale, Color col) {
		Vector3[] points = {
			pos + (Vector3.up * scale), 
			pos - (Vector3.up * scale), 
			pos + (Vector3.right * scale), 
			pos - (Vector3.right * scale), 
			pos + (Vector3.forward * scale), 
			pos - (Vector3.forward * scale)
		}; 		
		
		Debug.DrawLine (points[0], points[1], col); 
		Debug.DrawLine (points[2], points[3], col); 
		Debug.DrawLine (points[4], points[5], col); 
		
		Debug.DrawLine (points[0], points[2], col); 
		Debug.DrawLine (points[0], points[3], col); 
		Debug.DrawLine (points[0], points[4], col); 
		Debug.DrawLine (points[0], points[5], col); 
		
		Debug.DrawLine (points[1], points[2], col); 
		Debug.DrawLine (points[1], points[3], col); 
		Debug.DrawLine (points[1], points[4], col); 
		Debug.DrawLine (points[1], points[5], col); 
		
		Debug.DrawLine (points[4], points[2], col); 
		Debug.DrawLine (points[4], points[3], col); 
		Debug.DrawLine (points[5], points[2], col); 
		Debug.DrawLine (points[5], points[3], col); 
		
	}
	
	
	
	/*public static void DrawGrid  (Vector3 pos, Point size, Vector2 cellSize) {		
	
	}*/

	/// <summary>
	/// Using a stopwatch, time how long it takes for the passed action to run, returning the number
	/// of milliseconds. Use LogTime if you want a convenient to just log it.
	/// </summary>
	public static float TimeDuration(Action action) {
		// Can't 'using' System.Diagnostics since it causes clashes with Debug class
		// Can't 'using' System since it causes clashes with Unity's Object class
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		action();

		stopwatch.Stop();

		return stopwatch.Millisecs();
	}

	public static float Millisecs(this Stopwatch stopwatch) {
		// Built in stopwatch.ElapsedMilliseconds only gives you a long rather than a fractional value,
		// but we can calculate it properly using ticks.
		var ticks = stopwatch.ElapsedTicks;
		var ticksPerSecond = Stopwatch.Frequency;
		double ticksPerMillisecond = ticksPerSecond / 1000.0;
		float millisecs = (float)(ticks / ticksPerMillisecond);
		return millisecs;
	}

	/// <summary>
	/// Time how long it takes to run a particular action, and log that time in milliseconds.
	/// Also returns the time in case you want to use it further.
	/// </summary>
	public static float LogTimeDuration(Action action) {
		var time = TimeDuration(action);
		Log(time+" ms");
		return time;
	}

	/// <summary>
	/// Time how long it takes to run a particular action, and log that time in milliseconds along with a label
	/// in the format: Your label: X ms
	/// Also returns the time in case you want to use it further.
	/// </summary>
	public static float LogTimeDuration(string label, Action action) {
		var time = TimeDuration(action);
		Log(label + ": " + time + " ms");
		return time;
	}

	/// <summary>
	/// Get a single-line short summary of the current callstack, good for appending to log lines.
	/// Doesn't include line numbers though, partly because it's expected to be usable in release
	/// when there aren't any line numbers anyway.
	/// When called within a co-routine, it doesn't bother to include all the unity callstack stuff.
	/// </summary>
	public static string ShortCallstack() {
		if( _shortCallstackTempSB == null ) _shortCallstackTempSB = new StringBuilder();
		_shortCallstackTempSB.Length = 0;

		const int framesToSkip = 1;

		var trace = new StackTrace(framesToSkip);

		// Skip actual ShortCallstack call
		for(var i=0; i<trace.FrameCount; i++) {
			var frame = trace.GetFrame(i);

			string methodName = "<UnknownMethodName>";
			string className = "<UnknownClassName>";
			var method = frame.GetMethod();

			// Don't think these things can be null but I want to be super safe with debug code right now!
			if( method != null ) {
				methodName = method.Name;
				if( method.DeclaringType != null && method.DeclaringType.Name != null ) {
					className = method.DeclaringType.Name;
				}
			}

			// Skip massive remaining co-routine callstack we don't need (lots of internal unity stuff)
			if( methodName.IndexOf("InvokeMoveNext", StringComparison.Ordinal) != -1 ) 
				break;

			if( i > 0 ) _shortCallstackTempSB.Append(" / ");

			_shortCallstackTempSB.Append(className);
			_shortCallstackTempSB.Append(".");
			_shortCallstackTempSB.Append(methodName);
			
		}

		var result = _shortCallstackTempSB.ToString();
		_shortCallstackTempSB.Length = 0;
		return result;
	}
	static StringBuilder _shortCallstackTempSB;
}