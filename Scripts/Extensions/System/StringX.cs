using System;

public static class StringX {
	//Returns true if string is only white space
	public static bool IsWhiteSpace(this string s){
		foreach(char c in s){
			if(c != ' ' && c != '\t' && c != '\n') return false;
		}
		return true;
	}



	// return true if it contains, and sets the index in the output type variable
	public static int FirstIndexOf(this string source, string[] toChecks, StringComparison comp, ref int stringsIndex) {
		int index = -1;
		foreach(var toCheck in toChecks) {
			index = source.IndexOf(toCheck, comp);
			if(index != -1) break;
			stringsIndex++;
		}
		return index;
	}


	
	/// <summary>
	/// Contains the specified source, toCheck and comp.
	/// </summary>
	/// <param name="source">Source.</param>
	/// <param name="toCheck">To check.</param>
	/// <param name="comp">Comp.</param>
	public static bool Contains(this string source, string toCheck, StringComparison comp) {
		return source.IndexOf(toCheck, comp) >= 0;
	}

	// return true if it contains, and sets the index in the output type variable
	public static bool Contains(this string source, string toCheck, StringComparison comp, out int index) {
		index = source.IndexOf(toCheck, comp);
		return (index >= 0);
	}

	//Returns true if string contains any of the listed strings
	public static bool ContainsAny(this string str, params string[] strings) {
	    foreach (string tmpString in strings) {
			if (str.Contains(tmpString)) return true;
		}
    	return false;
	}

    
	/// <summary>
	/// Returns a truncated version of the given string. If it's not longer than the given length, it returns it unchanged.
	/// </summary>
	/// <param name="source">The string to truncate.</param>
	/// <param name="length">The maximum number of characters to allow in the string.</param>
    public static string Truncate(this string source, int length){
		if (source.Length > length) {
			source = source.Substring(0, length);
		}
		return source;
	}

    /// <summary>
    /// Get string value after [first] a.
    /// </summary>
    public static string Before(this string value, string a, bool returnEmptyIfNotFound = true) {
		int posA = value.IndexOf(a, StringComparison.Ordinal);
		if (posA == -1) {
		    return returnEmptyIfNotFound ? string.Empty : value;
		}
		return value.Substring(0, posA);
    }

	/// <summary>
    /// Get string value after [first] a.
    /// </summary>
    public static string BeforeLast(this string value, string a, bool returnEmptyIfNotFound = true) {
		int posA = value.LastIndexOf(a, StringComparison.Ordinal);
		if (posA == -1) {
		    return returnEmptyIfNotFound ? string.Empty : value;
		}
		return value.Substring(0, posA);
    }
	
	/// <summary>
	/// Get string value after [first] a.
	/// </summary>
	public static string AfterFirst(this string value, string a, bool returnEmptyIfNotFound = true) {
		int posA = value.IndexOf(a);
		if (posA == -1) {
			return returnEmptyIfNotFound ? string.Empty : value;
		}
		int adjustedPosA = posA + a.Length;
		if (adjustedPosA >= value.Length) {
			return returnEmptyIfNotFound ? string.Empty : value;
		}
		return value.Substring(adjustedPosA);
	}

    /// <summary>
    /// Get string value after [last] a.
    /// </summary>
    public static string After(this string value, string a, bool returnEmptyIfNotFound = true) {
		int posA = value.LastIndexOf(a);
		if (posA == -1) {
		    return returnEmptyIfNotFound ? string.Empty : value;
		}
		int adjustedPosA = posA + a.Length;
		if (adjustedPosA >= value.Length) {
		    return returnEmptyIfNotFound ? string.Empty : value;
		}
		return value.Substring(adjustedPosA);
    }


    

	public static string UppercaseFirstCharacter( this string s){
		if (string.IsNullOrEmpty(s)) return string.Empty;
		char[] a = s.ToCharArray();
		a[0] = char.ToUpperInvariant(a[0]);
		return new string(a);
	}

	public static string LowercaseFirstCharacter(string s){
		if (string.IsNullOrEmpty(s)) return string.Empty;
		char[] a = s.ToCharArray();
		a[0] = char.ToLowerInvariant(a[0]);
		return new string(a);
	}
}