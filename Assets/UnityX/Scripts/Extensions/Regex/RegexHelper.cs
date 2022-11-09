using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class RegexHelper {
	public const string emptyOrWhiteSpace = @"^[A-Z\s]*$";

	public const string urlPattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";
	static Regex _url;
	public static Regex url {get {if(_url == null) _url = new Regex(urlPattern);return _url;}}

	public const string emailAddressPattern = @"^([a-z0-9_\.-]+)@([\da-z\.-]+)\.([a-z\.]{2,6})$";
	static Regex _emailAddress;
	public static Regex emailAddress {get {if(_emailAddress == null) _emailAddress = new Regex(emailAddressPattern);return _emailAddress;}}

	public const string hexCodePattern = @"^#?([a-f0-9]{6}|[a-f0-9]{3})$";
	static Regex _hexCode;
	public static Regex hexCode {get {if(_hexCode == null) _hexCode = new Regex(hexCodePattern);return _hexCode;}}
	
    public const string IPAddress = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
	public const string HTMLTag = @"^<([a-z]+)([^<]+)*(?:>(.*)<\/\1>|\s+\/>)$";
	public const string extractFromBrackets = @"\(([^)]*)\)";
	public const string extractFromSquareBrackets = @"\[([^\]]*)\]";
	public const string extractFromCurlyBrackets = @"\{([^\}]*)\}";
}
