using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RegexAttribute : PropertyAttribute {
	
    public readonly Regex regex;
    public readonly string pattern;
    public readonly string helpMessage = "String is invalid";
	public readonly bool showErrorWhenValid = true;

	public RegexAttribute (Regex regex) {
        this.regex = regex;
    }
	public RegexAttribute (string pattern) {
        this.pattern = pattern;
    }

	public RegexAttribute (string pattern, string helpMessage, bool showErrorWhenValid = true) {
        this.pattern = pattern;
        this.helpMessage = helpMessage;
		this.showErrorWhenValid = showErrorWhenValid;
    }
}