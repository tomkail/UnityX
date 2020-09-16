using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class RegexHelper {
	public const string emptyOrWhiteSpace = @"^[A-Z\s]*$";
	public const string URL = @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$";
	public const string emailAddress = @"^([a-z0-9_\.-]+)@([\da-z\.-]+)\.([a-z\.]{2,6})$";
	public const string hexCode = @"^#?([a-f0-9]{6}|[a-f0-9]{3})$";
	public const string IPAddress = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
	public const string HTMLTag = @"^<([a-z]+)([^<]+)*(?:>(.*)<\/\1>|\s+\/>)$";
	public const string extractFromBrackets = @"\(([^)]*)\)";
	public const string extractFromSquareBrackets = @"\[([^\]]*)\]";
	public const string extractFromCurlyBrackets = @"\{([^\}]*)\}";
}
