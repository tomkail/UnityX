using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public static class StringX {

	public static bool StartsWithVowel(this string s) {
		if (s.Length > 0 && "aeiou".IndexOf(s.Substring(0, 1).ToLower()) > -1 ) {
			return true; 
		} 
		return false;
	}
	
	public static string UppercaseFirstCharacter( this string s){
		if (string.IsNullOrEmpty(s)) return string.Empty;
		char[] a = s.ToCharArray();
		a[0] = char.ToUpperInvariant(a[0]);
		return new string(a);
	}

	public static string UppercaseFirstCharacterHandlingQuotes( this string s){
		if (string.IsNullOrEmpty(s)) return string.Empty;
		if (s [0] != '\"' && s [0] != '”') {
			return s.UppercaseFirstCharacter ();
		}
		var resultString = new StringBuilder(s.Length);
		resultString.Append (s [0]);
		resultString.Append (s.Substring (1).UppercaseFirstCharacter());
		return resultString.ToString();
	}

	public static string LowercaseFirstCharacter(string s){
		if (string.IsNullOrEmpty(s)) return string.Empty;
		char[] a = s.ToCharArray();
		a[0] = char.ToLowerInvariant(a[0]);
		return new string(a);
	}

	public static string SentenceCaseWithoutArticles(string s) {
		if (s.IndexOf("a ", StringComparison.OrdinalIgnoreCase) == 0) { s = s.Remove(0, 2); }
		if (s.IndexOf("the ", StringComparison.OrdinalIgnoreCase) == 0) { s = s.Remove(0, 4); }
		return UppercaseFirstCharacter (s);
	}


	/// <summary>
	/// Procedural text functions include a leading 'n ' when attached to a word that needs "an" not "a" 
	/// This is now removed: we turn "\ba\s+n\b" into "\ban\b"; and then remove "\bn\b" 
	/// </summary>
	/// <returns>The text without article altering markers.</returns>
	/// <param name="dialogueLine">Dialogue line.</param>
	public static string ResolveArticleAlteringMarkers (this string s) {
		// The following does
		//	 	/\b[aA]\s[nN]\s/  	=> 	/an /  (done below by skipping one write step)
		// then
		//		/\b[nN]\s/  		=> 	//		(done below by writing in pre-N character, then skipping the n and the space after)

		var inWord = false;
		StringBuilder outputString = new StringBuilder(s.Length); 
		for (var i = 0; i < s.Length - 1 ; i++ ){
			if (!inWord && (s[i] == 'n' || s[i] == 'N') && s[i+1]==' ') {
				// we've found an 'n'. 
				// Note we were just about to write in the "space/other" that came just before this N
				// But now we won't; we'll skip over it; and maybe skip the "n" too
				if (! (i >= 2 && s[i-1] == ' ' && ( s[i-2] == 'a' || s[i-2] == 'A' ))) {
					if (i >= 1) {
						outputString.Append(s[i - 1]); 
					}
					i += 2; 
				}
				continue; 	// Don't write in the space before either. 
			}

			inWord = Char.IsLetter(s[i]); 
			if (i > 0) {
				outputString.Append(s[i - 1]); // write in the character before this one
			}
		}
		if (s.Length >= 2) {
			outputString.Append(s[s.Length - 2]); 
		}
		if (s.Length >= 1) {
			outputString.Append(s[s.Length - 1]);
		}

		return outputString.ToString();
		
	}

	/// <summary>
	/// Trim, concatenate multiple white space, and ensure spacing before and after punctuation is correct
	/// </summary>
	/// <returns>The string without incorrect whitespaces.</returns>
	/// <param name="s">S.</param>
	public static string CorrectSpacingErrors(this string s) {
		if (string.IsNullOrEmpty(s)) return string.Empty;
		var resultString = new StringBuilder(s.Length * 2);

		bool inStringOfPunctuationCharacters = false;
		bool inQuotes = false;
		bool inSingleQuotes = false;

		bool mightHaveClosedSingleQuote = false;

		char lastCharacter = '^';

		int fullstopCounter = 0;

		for (int i = 0; i < s.Length; i++) {
			char thisChar = s [i];
			if (thisChar == '\'' || thisChar == '‘') {
				if ((lastCharacter == '^' || lastCharacter == ' ' || (inStringOfPunctuationCharacters && !inSingleQuotes)) && 
					(!inSingleQuotes ||  mightHaveClosedSingleQuote)) {
					thisChar = '\'';
					inSingleQuotes = true;
					mightHaveClosedSingleQuote = false;
				} else {
					if (inSingleQuotes) {
						// okay, take a decision. Is this closing the quote, or not? s
						if (isPunctuation(lastCharacter) || ( lastCharacter == ' ' && isPunctuation( resultString [resultString.Length - 2] ) ))
							mightHaveClosedSingleQuote = true;
					}
					
					if (inSingleQuotes && inStringOfPunctuationCharacters) {
						inSingleQuotes = false;
					}

					// we think this is an apostrophe
					thisChar = '\'';

				}
			}
			// double quotes are easier because they have to alternate
			if (thisChar == '"') {
				if (inQuotes) {
					inQuotes = false; 
					thisChar = '"';

				} else {
					thisChar = '"';
					inQuotes = true;
				}
			}

			// no spaces after open-quotes
			if ((lastCharacter == '"' || lastCharacter == '\'') && thisChar == ' ') {
				continue;
			}

			// note that open quote is counted as a word character, not a punctuation character
			if (isPunctuation(thisChar) || isQuoteMark(thisChar)) {
				// close single-quotes don't request a space after, but they don't insert one before either
				if (thisChar != '\'') {
					inStringOfPunctuationCharacters = true;

					if (lastCharacter == '\'') {
						// if we close a quote only to hit punctuation, it wasn't an apostrophe after all
						inSingleQuotes = false;
					}

				} 

				if (lastCharacter == ' ') {
					// only applies to punctuation, or quotes that aren't leading 'postrophes
					if (!isQuoteMark (thisChar) || mightHaveClosedSingleQuote) {
					
						// the last character we wrote was a ' ', so delete it. 
						resultString.Remove ((resultString.Length - 1), 1);
						lastCharacter = resultString [resultString.Length - 1];

						// we've decided we've closed a quote after all
						if (mightHaveClosedSingleQuote) {
							inSingleQuotes = false; 
							if (isPunctuation (lastCharacter)) {
								inStringOfPunctuationCharacters = true; 
							}
						}
					}
				}

			} else { 
				// add space after runs of punctuation, so we don't space out ... marks or ." 
				// note we don't consider ' to be punctuation here.
				if (inStringOfPunctuationCharacters && thisChar != ' ') {
					if (thisChar != '_') {
						// allow _ after punctuation, so we can close italics tightly; but we're still looking for a space next turn
						resultString.Append (' ');
					}
				} 

				inStringOfPunctuationCharacters = false; 

			}

			// remove basic duplication 
			if (thisChar == lastCharacter && (thisChar == ' ' || thisChar == ',' || thisChar == ':' || thisChar == ';' || thisChar == ')' || thisChar == '('))  {
				continue;
			}

			bool foundEllipsis = false;

			// slightly trickier, handle multiple full-stops but allow "..."  and "..?" and "..!"
			if (thisChar == '.' || thisChar == '!' || thisChar == '?') {

				if (lastCharacter == '.' ) {
					fullstopCounter++;
					if (i == s.Length - 1 || (s [i + 1] != '.' && s [i + 1] != '!' && s [i + 1] != '?')) {
						// we're at the end of this run of stops.
						if (fullstopCounter >= 3) {
							// is this the third stop in a row? If so, we printed the first; now append one more for the middle
							resultString.Append ('.');
							foundEllipsis = true;
						} else {
							// either there was a double stop, so treat as normal duplication, or we're still looking ahead 

							continue; 
						}
					} else {
						// basically do no duplication
						continue; 
					} 
				} else {
					fullstopCounter = 1;
				}
			}

			// don't allow weird punctuation strings
			if (!foundEllipsis && lastCharacter != thisChar && isPunctuation(lastCharacter) && isPunctuation(thisChar)) {
				// convert ,. into .  and ., into ,
				if (isTerminator(lastCharacter) != isTerminator(thisChar) && fullstopCounter <= 1) {
					fullstopCounter = 0;
					resultString.Remove ((resultString.Length - 1), 1);
					if (resultString.Length >= 2) {
						lastCharacter = resultString [resultString.Length - 1];
					} else {
						lastCharacter = '^';
					}
				} else {
					continue; 
				}
			}

			resultString.Append(thisChar);

				 
			if (inSingleQuotes && lastCharacter == '\'') {
				if (thisChar != ' ' && !inStringOfPunctuationCharacters) {
					// we printed a close quote before, and now a "real" letter
					// so it was an apostrophe 
					mightHaveClosedSingleQuote = false;
				} else if (thisChar == ' ') {
					// we might have just closed the open quote string after all
					mightHaveClosedSingleQuote = true; 
				}
			}

			lastCharacter = thisChar;

		}

		return resultString.ToString().Trim();
	}

	public static bool isPunctuation(this char thisChar) {
		return (thisChar == '.' || thisChar == '?' || thisChar == '!' || thisChar == ','
			|| thisChar == ';' || thisChar == ':');
	}

	public static bool isTerminator(this char thisChar) {
		return terminators.Contains(thisChar);
	}

	public static bool isQuoteMark(this char thisChar) {
		return ( thisChar == '”' || thisChar == '’' || thisChar == '\'' || thisChar == '"' );
	}

	/// <summary>
	///  Runs the punctuation-correction, spacing-correction and grammar adjustment tidy-up routines 
	///  required to make ink text presentable onscreen.
	/// </summary>
	/// <returns>The processed text.</returns>
	/// <param name="text">Text.</param>
	public static string ProcessText(this string text, bool terminatingPunctuation = true, bool stripSquareBrackets = false ) {
		if (string.IsNullOrEmpty(text)) return string.Empty;
		var outText = text.CorrectSpacingErrors().ResolveArticleAlteringMarkers().UppercaseFirstCharacterHandlingQuotes();
		outText = outText.EnforceTerminatingPunctuation (terminatingPunctuation);
        if (stripSquareBrackets)
		    outText = outText.StripSquareBrackets();
		return outText;


	}

    /// <summary>
    /// Removes square bracketed sections of the string. Square brackets are used as comments and shouldn't make it to the screen.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
	public static string StripSquareBrackets(this string s)
	{
		var resultString = new StringBuilder(s.Length);
		var inBrackets = false;
		for (int i = 0; i < s.Length; i++)
		{
			char thisChar = s[i];
            if (thisChar == '[')
            {
				inBrackets = true;
			} else if (thisChar == ']')
            {
				inBrackets = false;
				continue;
            }

            if (!inBrackets)
			    resultString.Append(thisChar);
		}
		return resultString.ToString();
	}

	public static string AppendEllipsis(this string text) {
		text = text.Trim (); 
		var lastChar = text [text.Length - 1];
		if (lastChar == '_') {
			return AppendEllipsis (text.Substring (0, text.Length - 1)) + "_";
			 
		} else {
			if (text.Length == 0) {
				return text; 
			} else if (!isTerminator (lastChar)) {
				if (!isPunctuation (lastChar)) {
					// no terminator? Just shove "..." on the end then
					return text + "...";
				} else {
					// end in a comma or something...
					return text.Substring (0, text.Length - 1) + "...";
				}	
 
			} else if (isTerminator (lastChar) && isTerminator (text [text.Length - 2])) {
				// two consecutive terminators is a valid ellipsis in crazy text world!? (also You... of course)
				return text;

			} else {
				var sb = new StringBuilder ();
				// eg. "You!"
				var terminator = lastChar; // '!'
				// You
				sb.Append (text.Substring (0, text.Length - 1));
				sb.Append ("..");
				// You ..!
				sb.Append (terminator); 
				return sb.ToString ();

			}	
		}
	}

	static char[] nonTerminatorsWhichThenRequireTerminators = new char[]{ '"', '”'  };
	static char[] nonTerminators = new char[]{ ')', ']', '_', ' ', '\'', '’' };
	static char[] terminators = new char[]{ '?', '!', '.' };

	public static string EnforceTerminatingPunctuation(this string s, bool requireTerminatingPunctuation) {
		
		if (string.IsNullOrEmpty(s)) return string.Empty;
		var resultString = new StringBuilder (s.Length + 1);

		// find the real terminating character, ignoring quotes and brackets
		var terminatorIndex = s.Length - 1;
		for (; terminatorIndex >= 0 ; terminatorIndex--) {
			if (nonTerminatorsWhichThenRequireTerminators.Contains(s[terminatorIndex])) {
				//  the presence of a close-quote means we now require punctuation inside that quote
				requireTerminatingPunctuation = true;
			} else if (!nonTerminators.Contains(s[terminatorIndex]))
				//  this character wasn't a post-terminator character of any kind; it's a terminator or content
				break; 
		}

		// no real text 
		if (terminatorIndex < 0)
			return s; 


		if (requireTerminatingPunctuation) {
			// want a terminator and already have one
			if (terminators.Contains (s [terminatorIndex]))
				return s; 
		
			// get the string up to and including the last real character
			resultString.Append (s.Substring (0, terminatorIndex + 1));
			// append a full stop 
			resultString.Append ('.');

		} else {
			// we don't want a terminator, but tbh we're allowed "?" and "!"
			if (s [terminatorIndex] != '.')
				return s; 

			// we're also allowed "...", "..!" and "..?"
			if (terminatorIndex >= 3 && s[terminatorIndex-1] == '.' &&  s[terminatorIndex-2] == '.' )
				return s;

			// get the string up to and excluding the last real character
			resultString.Append (s.Substring (0, terminatorIndex)); 
		}

		if (terminatorIndex < s.Length - 1) {
			// complete the string with the non-terminating cruft from the end
			resultString.Append (s.Substring (terminatorIndex + 1)); 
		}
		return resultString.ToString();


	}
	
	//Returns true if string is only white space
	public static bool IsWhiteSpace(this string s){
		foreach(char c in s){
			if(c != ' ' && c != '\t' && c != '\n') return false;
		}
		return true;
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

	//Returns true if string contains any of the listed strings
	public static bool ContainsAny(this string str, params string[] strings) {
	    foreach (string tmpString in strings) {
			if (str.Contains(tmpString)) return true;
		}
    	return false;
	}

	// return true if it contains, and sets the index in the output type variable
	public static bool Contains(this string source, string toCheck, StringComparison comp, out int index) {
		index = source.IndexOf(toCheck, comp);
		return (index >= 0);
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
	/// Lists of strings printed to a string, delimited using a given character (eg. " ", ", ", " / ". )
	/// </summary>
	/// <returns>The list of string to print.</returns>
	/// <param name="stringList">String list.</param>
	/// <param name="delimiter">Delimiter character.</param>
	public static string ListToString(List<string> stringList, string delimiter) {
		StringBuilder resultString = new StringBuilder ();
		bool doneFirst = false; 
		foreach (var word in stringList) {
			if (!doneFirst) {
				doneFirst = true;
			} else {
				resultString.Append (delimiter);
			}
			resultString.Append (word);
		}
		return resultString.ToString ();
	}

    /// <summary>
    /// Get string value after [first] a.
    /// </summary>
    public static string Before(this string value, string a, bool returnEmptyIfNotFound = true) {
		int posA = value.IndexOf(a);
		if (posA == -1) {
		    return returnEmptyIfNotFound ? string.Empty : value;
		}
		return value.Substring(0, posA);
    }

	/// <summary>
    /// Get string value after [first] a.
    /// </summary>
    public static string BeforeLast(this string value, string a, bool returnEmptyIfNotFound = true) {
		int posA = value.LastIndexOf(a);
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

	readonly static string[] Tens = { 
		"zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
	};
	readonly static string[] Units = { 
		"zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", 
		"ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
	};

	/// <summary>
	///  Generate a number in words
	/// </summary>
	/// <returns>The number as words.</returns>
	/// <param name="number">Number to convert</param>
	public static string NumberAsWords(int number, bool withLeadingConjunction = false) {
		var sb = new StringBuilder();

		if (number > 0) {
			if (withLeadingConjunction) {
				
				// we've just printed "two thousand..." 
				if (number < 100) {
					sb.Append (" and ");
				} else {
					sb.Append (", ");
				}
			}

			if (number >= 1000000) {
				
				sb.Append (StringX.NumberAsWords (Mathf.FloorToInt (number / 1000000)));
				sb.Append (" million");
				if (number % 1000 > 0) {
					sb.Append (StringX.NumberAsWords (number % 1000000, true));
				}
			} else if (number >= 1000) {
				sb.Append (StringX.NumberAsWords (Mathf.FloorToInt (number / 1000)));
				sb.Append (" thousand");
				if (number % 1000 > 0) {
					sb.Append (StringX.NumberAsWords (number % 1000, true));
				}
			} else if (number >= 100) {
				sb.Append (StringX.NumberAsWords (Mathf.FloorToInt (number / 100)));
				sb.Append (" hundred");
				if (number % 100 > 0) {
					sb.Append (StringX.NumberAsWords (number % 100, true));
				}
			} else {
				if (number >= 20) {
					sb.Append (Tens [Mathf.FloorToInt (number / 10)]);  // thirty
					if (number % 10 > 0) {
						sb.Append ("-"); 		// thirty-
					}
				}
				if (number >= 10 && number < 20) {
					sb.Append (Units [number]); // thirteen, fourteen, fifteen...
				} else if (number % 10 > 0) {
					sb.Append (Units [number % 10]);
				}
			}
		} else {
			if (!withLeadingConjunction) {
				sb.Append ("zero");
			}
		}
		return sb.ToString();
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
}
