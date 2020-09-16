using UnityEngine;
using System.Collections.Generic;

public static class RichTextX {

	public static string AsRichText (this string str, RichTextOptions options) {
		string text = str;
		if(options.color != null) text = ColoredRichText(text, (Color32)options.color);
		if(options.size != null) text = SizedRichText(text, (int)options.size);
		if(options.bold) text = BoldRichText(text);
		if(options.italic) text = ItalicRichText(text);
		return text;
	}

	public static string AsRichText (this string str, Color32 _color) {
		string text = str;
		text = ColoredRichText(text, _color);
		return text;
	}

	public static string AsRichText (this string str, bool _bold) {
		string text = str;
		text = BoldRichText(text);
		return text;
	}

	public static string ColoredRichText (string str, Color32 color) {
		return "<color="+color.ToHexCode()+">"+str+"</color>";
	}

	public static string SizedRichText (string text, int size) {
		return "<size="+size+">"+text+"</size>";
	}

	public static string BoldRichText (string text) {
		return "<b>"+text+"</b>";
	}

	public static string ItalicRichText (string text) {
		return "<i>"+text+"</i>";
	}
}

public class RichTextOptions {
	public Color32? color {get;set;}
	public int? size {get;set;}
	public bool bold {get;set;}
	public bool italic {get;set;}

	public RichTextOptions () : this (null, null, false, false) {}
	public RichTextOptions (Color32? _color) : this (_color, null, false, false) {}
	public RichTextOptions (Color32? _color, int? _size) : this (_color, _size, false, false) {}
	public RichTextOptions (Color32? _color, bool _bold) : this (_color, null, _bold, false) {}
	public RichTextOptions (Color32? _color, bool _bold, bool _italic) : this (_color, null, _bold, _italic) {}

	public RichTextOptions (Color32? _color, int? _size, bool _bold, bool _italic) {
		color = _color;
		size = _size;
		bold = _bold;
		italic = _italic;
	}
}