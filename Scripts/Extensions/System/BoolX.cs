using System;

public static class BoolX {

	/// <summary>
	/// Returns bool based on int value, as defined by C#.
	/// </summary>
	/// <returns>The bool.</returns>
	/// <param name="_bool">If set to <c>true</c> _bool.</param>
	public static bool ToBool(this int _int) {
		return Convert.ToBoolean(_int);
	}
		
	/// <summary>
	/// Returns int based on boolean value, as defined by C#. False is 0, True is 1.
	/// </summary>
	/// <returns>The int.</returns>
	/// <param name="_bool">If set to <c>true</c> _bool.</param>
	public static int ToInt(this bool _bool) {
		return Convert.ToInt32(_bool);
	}
}