using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public static class TextMeshProUtils {
    #region Size Estimation Utilities
    // GetPreferredValues sometimes returns a value that is a tiny bit small, causing text that uses it to end up on two lines.
    // This multiplier is used to ensure that values returned always fit on one line, which is the intent of GetPreferredValues
    const float preferredWidthMultiplierFudge = 1.0025f;

    // Applying float.MaxValue to a rectTransform can cause crashes (not sure why) so we just use a very big number instead.
    const float veryLargeNumber = 10000000;
    // I long for a day when this is no longer necessary.
    static Vector2 BetterGetRenderedValues(TMP_Text textComponent, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber, bool onlyVisibleCharacters = true) {
        // If width/height is Infinity/<0 renderedSize can be NaN. In that case, use preferredValues
        var renderedSize = textComponent.GetRenderedValues(onlyVisibleCharacters);
        if(IsInvalidFloat(renderedSize.x) || IsInvalidFloat(renderedSize.y)) {
            var preferredSize = textComponent.GetPreferredValues(textComponent.text, maxWidth, maxHeight);
            // I've seen this come out as -4294967000.00 when the string has only a zero-width space (\u200B) with onlyVisibleCharacters true. In any case it makes no sense for the size to be < 0.
            preferredSize = new Vector2(Mathf.Max(preferredSize.x*preferredWidthMultiplierFudge, 0), Mathf.Max(preferredSize.y, 0));
            if(IsInvalidFloat(renderedSize.x)) renderedSize.x = preferredSize.x;
            if(IsInvalidFloat(renderedSize.y)) renderedSize.y = preferredSize.y;
        }
        
        // 1.7E+38f is half 3.40282347E+38f, which seems to be a possible return value for GetRenderedValues when maxHeight = veryLargeNumber (I guess because it's half)
        bool IsInvalidFloat (float f) {return float.IsNaN(f) || f == Mathf.Infinity || f >= 1.7E+38f  || f < 0;}
        return renderedSize;
    }
    
    // Applies the tightest bounds for the current text using GetRenderedValues
    // Note this uses sizeDelta for sizing so won't work when using anchors.
    // This is wayyyy more reliable than the actual GetRenderedValues because it won't return stupid values, as GetRenderedValues is prone to doing. 
    public static void RenderAndApplyTightSize (this TMP_Text textComponent, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber, bool onlyVisibleCharacters = true) {
        var originalRenderMode = textComponent.renderMode;
        
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxHeight);
        textComponent.ForceMeshUpdate(true);

        var renderedSize = Vector2.zero;
        if(!string.IsNullOrEmpty(textComponent.text)) renderedSize = BetterGetRenderedValues(textComponent, maxWidth, maxHeight, onlyVisibleCharacters);
        
        textComponent.renderMode = originalRenderMode;
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, renderedSize.x);
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, renderedSize.y);
        textComponent.ForceMeshUpdate(true);
    }
    
    // Gets the tightest bounds for the current text using GetRenderedValues
    // Note this uses sizeDelta for sizing so won't work when using anchors.
    // This is wayyyy more reliable than the actual GetRenderedValues because it won't return stupid values, as GetRenderedValues is prone to doing. 
    public static Vector2 RenderAndGetTightSize (this TMP_Text textComponent, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber, bool onlyVisibleCharacters = true) {
        var originalRenderMode = textComponent.renderMode;
        var originalSize = textComponent.rectTransform.rect.size;
        
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxHeight);
        textComponent.ForceMeshUpdate(true);
        
        if(string.IsNullOrEmpty(textComponent.text)) return Vector2.zero;
        // This doesn't work if the component is disabled - but it's better! I'm not even sure this function works while disabled...
        // if(textComponent.textInfo.characterCount == 0) return Vector2.zero;
        var renderedSize = BetterGetRenderedValues(textComponent, maxWidth, maxHeight, onlyVisibleCharacters);
        
        textComponent.renderMode = originalRenderMode;
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
        textComponent.ForceMeshUpdate(true);
		
        return renderedSize;
    }

    // Gets the tightest bounds for the text by updating the text and using GetRenderedValues
    // Note this uses sizeDelta for sizing so won't work when using anchors.
    // This is wayyyy more reliable than the actual GetRenderedValues because it won't return stupid values, as GetRenderedValues is prone to doing.
    public static Vector2 GetRenderedValues (this TMP_Text textComponent, string text, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber, bool onlyVisibleCharacters = true) {
        if(string.IsNullOrEmpty(text)) return Vector2.zero;
        // Setting RT size to Infinity can lead to weird results, so we use a very large number instead. 
        if(maxWidth > veryLargeNumber) maxWidth = veryLargeNumber;
        if(maxHeight > veryLargeNumber) maxHeight = veryLargeNumber;

        var originalRenderMode = textComponent.renderMode;
        var originalText = textComponent.text;
        var originalSize = textComponent.rectTransform.rect.size;
        
        textComponent.renderMode = TextRenderFlags.DontRender;
        textComponent.text = text;
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxHeight);
        textComponent.ForceMeshUpdate(true);

        // This doesn't work if the component is disabled - but it's better! I'm not even sure this function works while disabled...
        // if(textComponent.textInfo.characterCount == 0) return Vector2.zero;

        var renderedSize = BetterGetRenderedValues(textComponent, maxWidth, maxHeight, onlyVisibleCharacters);
        
        textComponent.renderMode = originalRenderMode;
        textComponent.text = originalText;
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
        textComponent.ForceMeshUpdate(true);    
		
        return renderedSize;
    }
    
    
    // Applies tight preferred values for the current text using GetTightPreferredValues
    // Note this uses sizeDelta for sizing so won't work when using anchors.
    public static void ApplyPreferredSize (this TMP_Text textComponent, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber) {
        var preferredSize = textComponent.GetPreferredValues(textComponent.text, maxWidth, maxHeight);
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
        textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
    }
    //
    // // Applies tight preferred values for the current text using GetTightPreferredValues
    // // Note this uses sizeDelta for sizing so won't work when using anchors.
    // public static void ApplyPreferredWidth (this TMP_Text textComponent, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber) {
    //     var preferredSize = GetPreferredValues(textComponent, maxWidth, maxHeight);
    //     textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
    // }
    //
    // // Applies tight preferred values for the current text using GetTightPreferredValues
    // // Note this uses sizeDelta for sizing so won't work when using anchors.
    // public static void ApplyPreferredHeight (this TMP_Text textComponent, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber) {
    //     var preferredSize = GetPreferredValues(textComponent, maxWidth, maxHeight);
    //     textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
    // }
    //
    // // A fixed version of GetPreferredValues that correctly takes into account the wrapping mode of the text; the regular one always returns width as if the text were on one line.
    // // Allows clamping the output to the input width and height in the case that the preferred values are larger than the values specified.
    // public static Vector2 GetPreferredValues (this TMP_Text textComponent, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber, bool clamped = true) {
    //     var preferredValues = textComponent.GetPreferredValues(textComponent.text, maxWidth, maxHeight);
    //     if (clamped) preferredValues = new Vector2(Mathf.Min(preferredValues.x, maxWidth), Mathf.Min(preferredValues.y, maxHeight));
    //     return preferredValues;
    // }
    //
    // // This method reproduces TMP_Text.GetPreferredWidth, but uses the wrapping mode of the text, which is always set to NoWrap in GetPreferredWidth in the original function.
    // public static float GetPreferredWidthWithCorrectWrappingMode(this TMP_Text textComponent, Vector2 margin) => GetPreferredWidth(textComponent, margin, textComponent.textWrappingMode);
    // // This method reproduces TMP_Text.GetPreferredWidth but allows passing a wrapping mode.
    // public static float GetPreferredWidth(this TMP_Text textComponent, Vector2 margin, TextWrappingModes textWrappingMode) {
    //     System.Type type = typeof(TMP_Text);
    //     
    //     float fontSize = textComponent.enableAutoSizing ? textComponent.fontSizeMax : textComponent.fontSize;
    //
    //     // Reset auto sizing point size bounds
    //     type.GetField("m_minFontSize", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(textComponent, textComponent.fontSizeMin);
    //     type.GetField("m_maxFontSize", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(textComponent, textComponent.fontSizeMax);
    //     type.GetField("m_charWidthAdjDelta", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(textComponent, 0);
    //
    //     type.GetField("m_AutoSizeIterationCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(textComponent, 0);
    //     
    //     MethodInfo methodInfo = type.GetMethod("CalculatePreferredValues", BindingFlags.NonPublic | BindingFlags.Instance);
    //     object[] parameters = { fontSize, margin, false, textWrappingMode };
    //     Vector2 result = (Vector2)methodInfo.Invoke(textComponent, parameters);
    //     return result.x;
    // }
    //
    //
    // // Width for GetPreferredValues always returns the length of the text as if it was on one line.
    // // This function additionally clamps the returned width of GetPreferredValues to the input width.
    // // It does this for both width and height, but probably doesn't need to (?) because this issue is specific to width.
    // public static Vector2 GetPreferredValues (this TMP_Text textComponent, string text, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber) {
    //     var preferredSize = textComponent.GetPreferredValues(text, maxWidth, maxHeight);
    //     preferredSize.x = Mathf.Min(preferredSize.x*preferredWidthMultiplierFudge, maxWidth);
    //     preferredSize.y = Mathf.Min(preferredSize.y, maxHeight);
    //     return preferredSize;
    // }
    
    // TMP_Text.GetPreferredWidth has a bug the overload without text uses NoWrap so the width is always the width of the text as if it were on one line.
    // Additionally, adds a tiny amount to the width, as sometimes the returned value can be slightly less than the size that actually appears to be required.
    // This is a workaround.
    
    public static Vector2 GetPreferredValues(this TMP_Text textComponent, string text, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber) {
        var preferredValues = textComponent.GetPreferredValues(text, maxWidth, maxHeight);
        // preferredValues.x *= preferredWidthMultiplierFudge;
        return preferredValues;
    }
    public static Vector2 GetPreferredValues(this TMP_Text textComponent, float maxWidth = veryLargeNumber, float maxHeight = veryLargeNumber) {
        return GetPreferredValues(textComponent, textComponent.text, maxWidth, maxHeight);
    }
    
    
    // Used by GetPrettyPreferredValues
    struct TextSizerOutput {
	    public float inputWidth;
	    public Vector2 size;
	    public float aspectRatio;
	    public float score;
    }

    // Uses the algorithm in GetPrettyPreferredValues to get the margin required for a pretty layout.
    public static Vector4 GetMarginForPrettyLayout(this TextMeshProUGUI text) {
        // If the text is justified or flush, we should expect the text to be flush with the edges and so we don't want to add any margin.
        if (text.horizontalAlignment.HasFlag(HorizontalAlignmentOptions.Justified) || text.horizontalAlignment.HasFlag(HorizontalAlignmentOptions.Flush)) return Vector4.zero;
        
        var originalMargin = text.margin;
        text.margin = Vector4.zero;
        var size = GetPrettyPreferredValues(text, text.rectTransform.rect.width);
        var delta = text.rectTransform.rect.width - size.x;
        text.margin = originalMargin;
        if(text.horizontalAlignment.HasFlag(HorizontalAlignmentOptions.Left)) return new Vector4(0,0,delta,0); 
        else if(text.horizontalAlignment.HasFlag(HorizontalAlignmentOptions.Right)) return new Vector4(delta,0,0,0); 
        else if(text.horizontalAlignment.HasFlag(HorizontalAlignmentOptions.Center) || text.horizontalAlignment.HasFlag(HorizontalAlignmentOptions.Geometry)) return new Vector4(delta*0.5f,0,delta*0.5f,0);
        else return Vector4.zero;
    }
    // This function is similar to GetPreferredValues, but it iterates over different widths to find the prettiest one - the one with the ratio of min and max line widths.
    // The height returned will always be the same as GetPreferredValues, but the width may be different.
    public static Vector2 GetPrettyPreferredValues(this TextMeshProUGUI text, float maxWidth) => text.GetPrettyPreferredValues(0, maxWidth);
    public static Vector2 GetPrettyPreferredValues(this TextMeshProUGUI text, float minWidth, float maxWidth) {
        // How much to shrink it each iteration
	    const float stepMultiplier = 0.993f;
	    // Keep iterating until we hit this
	    const float minAspectRatio = 0.8f;
	    const int maxIterations = 100;
	    
	    if (string.IsNullOrEmpty(text.text)) return Vector2.zero;
	    
	    var unoptimisedPreferredValues = text.GetPreferredValues(text.text, maxWidth);
        
        var originalRenderMode = text.renderMode;
        var originalSize = text.rectTransform.rect.size;
        
        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        text.renderMode = TextRenderFlags.DontRender;
        
        var outputs = new List<TextSizerOutput>();

        var renderedSize = unoptimisedPreferredValues;
        var inputWidth = unoptimisedPreferredValues.x;
        var lastAspectRatio = Mathf.Infinity;
        int num = 0;
        // We'll set this once we know what the line count is after getting text info for the first item.
        int lineCount = 0;
        
        do {
            if (renderedSize.x < 0) {
                Debug.LogError("Hit a weird bug where TMP RenderedWidth is " + renderedSize.x);
                return Vector2.zero;
            }

            TextSizerOutput output = new TextSizerOutput();
            output.inputWidth = inputWidth;
            output.size = renderedSize;
            output.aspectRatio = output.size.x / output.size.y;
            if (lastAspectRatio != output.aspectRatio) {
                text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, output.size.x);
                var textInfo = text.GetTextInfo(text.text);
                if (lineCount == 0) lineCount = textInfo.lineCount;
                else if (textInfo.lineCount != lineCount) break;
                output.score = GetScore(output, textInfo);
                outputs.Add(output);
                lastAspectRatio = output.aspectRatio;
                // If there's only one line we can break out right away.
                if (lineCount == 1) break;
            }

            inputWidth = Mathf.Min(inputWidth, output.size.x) * stepMultiplier;
            num++;
            if (num > maxIterations) break;
            renderedSize = text.GetPreferredValues(text.text, inputWidth);
        } while (lastAspectRatio > minAspectRatio);
        
        var best = outputs.Best(x => x.score, (other, currentBest) => other > currentBest, Mathf.NegativeInfinity);
        Debug.Assert(!outputs.IsNullOrEmpty());
        Debug.Assert(best.score > Mathf.NegativeInfinity);
        // Debug.Log($"Iterated {num} times adding {outputs.Count} outputs for input {maxWidth} which created preferred width {unoptimisedPreferredValues.x} with {lineCount} lines, found best output {best.size} with score {best.score}");
        
        text.renderMode = originalRenderMode;
        text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalSize.x);
        // text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize.y);
        text.ForceMeshUpdate(true);
        
        return best.size;
        
        // Scores layouts based on a few factors.
	    static float GetScore (TextSizerOutput output, TMP_TextInfo textInfo) {
       		if(output.size.x == 0) return Mathf.NegativeInfinity;
	    
			var minLineWidth = float.MaxValue;
			var maxLineWidth = float.MinValue;
            
            List<float> wordPunctuationScores = new List<float>();
            for (int i = 0; i < textInfo.lineCount; i++) {
                TMP_LineInfo lineInfo = textInfo.lineInfo[i];
                // can also use lineInfo.length which is similar but not at all clear what the difference is.
                var width = lineInfo.lineExtents.max.x-lineInfo.lineExtents.min.x;
                minLineWidth = Mathf.Min(minLineWidth, width);
                maxLineWidth = Mathf.Max(maxLineWidth, width);
                
                
                StringBuilder word = new StringBuilder();
                var wordIndex = 0;
                var wordHasPunctuation = false;
                for (int j = lineInfo.firstCharacterIndex; j <= lineInfo.lastCharacterIndex; j++) {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(textInfo.characterInfo[j].character);
                    if(unicodeCategory == UnicodeCategory.OtherPunctuation) wordHasPunctuation = true;
                    if (unicodeCategory == UnicodeCategory.SpaceSeparator || j == lineInfo.lastCharacterIndex) {
                        var wordPunctuationScore = 0f;
                        if (wordHasPunctuation && !(wordIndex + 1 == lineInfo.wordCount && i == textInfo.lineCount-1)) {
                            if (wordIndex+1 == lineInfo.wordCount) wordPunctuationScore = 1;
                            else wordPunctuationScore = Mathf.Lerp(0, -1, Mathf.InverseLerp(lineInfo.wordCount-4, lineInfo.wordCount-1, wordIndex+1));
                            wordPunctuationScores.Add(wordPunctuationScore);
                        }
                        // Debug.Log($"Line: {i+1} Word: {word} {wordIndex+1}/{lineInfo.wordCount}. Punc score: {wordPunctuationScore}");
                        word.Clear();
                        wordHasPunctuation = false;
                        wordIndex++;
                        continue;
                    }
                    word = word.Append(textInfo.characterInfo[j].character);
                }
			}

            
            // Score based on the ratio between the smallest and largest line.
            // Debug.Log($"size:{output.size} min:{minLineWidth} max:{maxLineWidth}");
			var comparableLineWidthScore = minLineWidth/maxLineWidth;
			if(comparableLineWidthScore > 1) comparableLineWidthScore = 1/comparableLineWidthScore;
            
            // Each word with punctuation is scored based on its position.
            // Good scores for when (especially terminating) punctuation ends a line.
            // Bad scores when (especially terminating) punctuation is very near the end of a line.
            var punctuationScore = wordPunctuationScores.Any() ? wordPunctuationScores.Sum() / wordPunctuationScores.Count : 0;
            // Debug.Log(punctuationScore);

            // var splitTextScore = TextMeshProUtils.IsAnyWordSplit(textInfo) ? -1 : 0;
            
			return comparableLineWidthScore + punctuationScore * 0.1f;

	    }
    }

    
    // This seems to get the width of the component for a given height.
    public static Vector2 GetBestFitWidth (this TMP_Text textComponent, float targetHeight, float widthStep) {
        textComponent.renderMode = TextRenderFlags.DontRender;
        var originalSize = textComponent.rectTransform.rect.size;
        
        Debug.Assert(widthStep > 1, "Width step must be larger than 1 or this will take forever/ages to execute!");
        
        float width = 0;
        textComponent.rectTransform.sizeDelta = new Vector2(width, targetHeight);
        textComponent.ForceMeshUpdate(true);

        int numIterations = 0;
        while(textComponent.isTextOverflowing) {
            width += widthStep;
			textComponent.rectTransform.sizeDelta = new Vector2(width, targetHeight);
			textComponent.ForceMeshUpdate(true);
            numIterations++;
            if(numIterations > 50) Debug.LogError("Max num iterations reached for GetBestFitWidth with targetHeight "+targetHeight+" and widthStep "+widthStep);
        }
        
        // The "tight" values for the rect - rendered width will be smaller than width used to calculate.
        var renderedSize = textComponent.GetRenderedValues(true);
		
        // Reset to how we started
        textComponent.renderMode = TextRenderFlags.Render;
        textComponent.rectTransform.sizeDelta = originalSize;
		
        return new Vector2(renderedSize.x, targetHeight);
    }
    #endregion
    

    #region Line Height Calculations
    // Returns the number of lines that would fit in a UGUI TMP component. Ignores styling tags in text, but accounts for TextStyle.
    public static float GetLineCountFromHeight(this TextMeshProUGUI textComponent) {
        float componentHeight = textComponent.rectTransform.rect.height;
        float verticalPadding = textComponent.margin.y * 2;
        return textComponent.GetLineCountFromHeight(componentHeight - verticalPadding);
    }

    // Returns the number of lines that would fit in a given height in a TMP component. Ignores styling tags in text, but accounts for TextStyle.
    public static float GetLineCountFromHeight(this TMP_Text textComponent, float height) {
        var lineSpacingToRectTransformHeight = textComponent.LineSpacingToRectTransformHeight();
        var lineHeightToRectTransformHeight = textComponent.LineHeightToRectTransformHeight();
        return (height + lineSpacingToRectTransformHeight) / (lineHeightToRectTransformHeight + lineSpacingToRectTransformHeight);
    }

    // Returns the height required for a given number of lines in a TMP component. Ignores styling tags in text, but accounts for TextStyle.
    public static float GetHeightRequiredForLineCount(this TMP_Text textComponent, float lineCount) {
        var lineSpacingToRectTransformHeight = textComponent.LineSpacingToRectTransformHeight();
        var lineHeightToRectTransformHeight = textComponent.LineHeightToRectTransformHeight();
        return lineHeightToRectTransformHeight * lineCount + lineSpacingToRectTransformHeight * (lineCount - 1);
    }
    
    // Returns the height that the spacing of a text line in TMP component would take up in a Transform. Ignores styling tags in text, but accounts for TextStyle.
    public static float LineSpacingToRectTransformHeight (this TMP_Text textComponent) {
        var fontSize = textComponent.fontSize;
        var lineSpacing = textComponent.lineSpacing;

        if (textComponent.textStyle != null) {
            if (GetFontSizeFromStyle(textComponent.textStyle, out float styleFontSize)) fontSize = styleFontSize;
            if (GetLineSpacingFromStyle(textComponent.textStyle, out float styleLineSpacing)) lineSpacing = styleLineSpacing;
        }
        
        float currentEmScale = fontSize * 0.01f * (textComponent.isOrthographic ? 1 : 0.1f);
        // lineSpacing is emLineHeight
        return currentEmScale * lineSpacing;
    }

    // Returns the height that a text line of a TMP component would take up in a Transform. Ignores styling tags in text, but accounts for TextStyle.
    public static float LineHeightToRectTransformHeight (this TMP_Text textComponent) {
        var font = textComponent.font;
        var fontSize = textComponent.fontSize;
        
        if (textComponent.textStyle != null) {
            if (GetFontFromStyle(textComponent.textStyle, out TMP_FontAsset styleFont)) font = styleFont;
            if (GetFontSizeFromStyle(textComponent.textStyle, out float styleFontSize)) fontSize = styleFontSize;
        }
        
        return fontSize * (font.faceInfo.lineHeight / font.faceInfo.pointSize);
    }
    #endregion


    #region Screen Space Calculations
    static Vector2[] fourCornersArray = new Vector2[4];
    public static Rect GetScreenRectOfTextBounds(TMP_Text tmpText) {
        // Set local corners
        {
            var bounds = tmpText.textBounds;
            float x = bounds.min.x;
            float y = bounds.min.y;
            float xMax = bounds.max.x;
            float yMax = bounds.max.y;
            fourCornersArray[0] = new Vector3(x, y, 0.0f);
            fourCornersArray[1] = new Vector3(x, yMax, 0.0f);
            fourCornersArray[2] = new Vector3(xMax, yMax, 0.0f);
            fourCornersArray[3] = new Vector3(xMax, y, 0.0f);
        }

        // Set world corners
        {
            Matrix4x4 localToWorldMatrix = tmpText.transform.localToWorldMatrix;
            for (int index = 0; index < 4; ++index) fourCornersArray[index] = localToWorldMatrix.MultiplyPoint(fourCornersArray[index]);
        }
		    
        // Set screen corners
        {
            var canvas = tmpText.canvas;
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace ? canvas.worldCamera : null;
            for (int i = 0; i < 4; i++) fourCornersArray[i] = RectTransformUtility.WorldToScreenPoint(cam, fourCornersArray[i]);
        }

        // Create rect that encapsulates screen corners
        {
            float xMin = fourCornersArray[0].x;
            float xMax = fourCornersArray[0].x;
            float yMin = fourCornersArray[0].y;
            float yMax = fourCornersArray[0].y;
            for(int i = 1; i < fourCornersArray.Length; i++) {
                var vector = fourCornersArray[i];
                xMin = Mathf.Min (xMin, vector.x);
                xMax = Mathf.Max (xMax, vector.x);
                yMin = Mathf.Min (yMin, vector.y);
                yMax = Mathf.Max (yMax, vector.y);
            }
            return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
        }
    }
    
    // Given a range of characters (inclusive) returns the screen rects that they occupy. Several rects are returned if the characters span multiple lines.
    public static IEnumerable<Rect> GetScreenRectsForCharacterRange(TMP_Text textComponent, int startCharacterIndex, int endCharacterIndex) {
        if (textComponent == null) yield break;
        var textInfo = textComponent.textInfo;
        
        int currentLineIndex = -1;
        Rect firstCharacterInLineScreenRect = Rect.zero;
        TMP_CharacterInfo lastCharacterInfo = default(TMP_CharacterInfo);

        startCharacterIndex = Mathf.Clamp(startCharacterIndex, 0, textInfo.characterCount);
        endCharacterIndex = Mathf.Clamp(endCharacterIndex, 0, textInfo.characterCount);
        
        for (int c = startCharacterIndex; c <= endCharacterIndex; c++) {
            var characterInfo = textInfo.characterInfo[c];
            if (currentLineIndex == -1) {
                currentLineIndex = characterInfo.lineNumber;
                firstCharacterInLineScreenRect = GetScreenRectForCharacter(textComponent, characterInfo);
            }
            if (characterInfo.lineNumber != currentLineIndex) {
                var currentCharacterScreenRect = GetScreenRectForCharacter(textComponent, lastCharacterInfo);
                yield return CreateEncapsulating(firstCharacterInLineScreenRect, currentCharacterScreenRect);
                    
                currentLineIndex = characterInfo.lineNumber;
                firstCharacterInLineScreenRect = GetScreenRectForCharacter(textComponent, characterInfo);
            }

            lastCharacterInfo = characterInfo;
        }

        {
            var currentCharacterScreenRect = GetScreenRectForCharacter(textComponent, lastCharacterInfo);
            yield return CreateEncapsulating(firstCharacterInLineScreenRect, currentCharacterScreenRect);
        }
    }
    
    // Given a character index returns the screen rect that it occupies.
    public static Rect GetScreenRectForCharacter(TMP_Text textComponent, TMP_CharacterInfo cInfo) {
        var m_Transform = textComponent.transform;
        var topLeft = m_Transform.TransformPoint(new Vector3(cInfo.topLeft.x, cInfo.ascender, 0));
        var bottomLeft = m_Transform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.descender, 0));
        var bottomRight = m_Transform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.descender, 0));
        var topRight = m_Transform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.ascender, 0));
        return WorldToScreenRect(textComponent, bottomLeft, topLeft, topRight, bottomRight);
    }
    
    // Given a word index returns the screen rect that it occupies.
    public static Rect GetScreenRectForWord(TMP_WordInfo wInfo) {
        var m_TextComponent = wInfo.textComponent;
        if(m_TextComponent == null) return Rect.zero;
        var m_Transform = m_TextComponent.transform;
        var m_TextInfo = m_TextComponent.textInfo;

        bool isBeginRegion = false;

        Vector3 bottomLeft = Vector3.zero;
        Vector3 topLeft = Vector3.zero;
        Vector3 bottomRight = Vector3.zero;
        Vector3 topRight = Vector3.zero;

        float maxAscender = -Mathf.Infinity;
        float minDescender = Mathf.Infinity;

        // Iterate through each character of the word
        for (int j = 0; j < wInfo.characterCount; j++)
        {
            int characterIndex = wInfo.firstCharacterIndex + j;
            TMP_CharacterInfo currentCharInfo = m_TextInfo.characterInfo[characterIndex];
            int currentLine = currentCharInfo.lineNumber;

            bool isCharacterVisible = characterIndex > m_TextComponent.maxVisibleCharacters ||
                                      currentCharInfo.lineNumber > m_TextComponent.maxVisibleLines ||
                                     (m_TextComponent.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != m_TextComponent.pageToDisplay) ? false : true;

            // Track Max Ascender and Min Descender
            maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
            minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

            if (isBeginRegion == false && isCharacterVisible)
            {
                isBeginRegion = true;

                bottomLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
                topLeft = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                // If Word is one character
                if (wInfo.characterCount == 1)
                {
                    isBeginRegion = false;

                    topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                    bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                    bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                    topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                    break;
                }
            }

            // Last Character of Word
            if (isBeginRegion && j == wInfo.characterCount - 1)
            {
                isBeginRegion = false;

                topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                break;
            }
            // If Word is split on more than one line.
            else if (isBeginRegion && currentLine != m_TextInfo.characterInfo[characterIndex + 1].lineNumber)
            {
                isBeginRegion = false;

                topLeft = m_Transform.TransformPoint(new Vector3(topLeft.x, maxAscender, 0));
                bottomLeft = m_Transform.TransformPoint(new Vector3(bottomLeft.x, minDescender, 0));
                bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, minDescender, 0));
                topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));

                break;

            }
        }
        return WorldToScreenRect(m_TextComponent, bottomLeft, topLeft, topRight, bottomRight);
    }
    
    // Utility function for other parts of this class.
    static Rect WorldToScreenRect(TMP_Text textComponent, Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight) {
        return CreateEncapsulating(RectTransformUtility.WorldToScreenPoint(textComponent.canvas.rootCanvas.worldCamera, topLeft), RectTransformUtility.WorldToScreenPoint(textComponent.canvas.rootCanvas.worldCamera, bottomRight));
    }
    #endregion
    
    
    #region Misc Utilities
    // Gets the index at which text becomes clipped in a TextMeshProUGUI component.
    public static int GetClippedCharacterIndex(TextMeshProUGUI textMeshPro) {
        string originalText = textMeshPro.text;
        string generatedText = textMeshPro.GetParsedText();

        // Find the point where the generated text differs from the original text
        int clippedIndex = FindClippedIndex(originalText, generatedText);
        //
        // if (clippedIndex >= 0)
        // {
        //     Debug.Log($"Text is clipped at index: {clippedIndex} ({originalText.Substring(clippedIndex)})");
        // }
        // else
        // {
        //     Debug.Log("Text is not clipped.");
        // }
        return clippedIndex;
        
        static int FindClippedIndex(string originalText, string generatedText)
        {
            int maxLength = Mathf.Min(originalText.Length, generatedText.Length);

            for (int i = 0; i < maxLength; i++)
            {
                if (originalText[i] != generatedText[i])
                {
                    return i;
                }
            }

            // Text is not clipped
            return -1;
        }
    }

    // Untested!
    public static float CharacterSpacingToRectTransformWidth (this TMP_Text textComponent, float emCharacterSpacing) {
        var font = textComponent.font;
        var lineHeightMultiplier = font.faceInfo.lineHeight / font.faceInfo.pointSize;
        return lineHeightMultiplier * emCharacterSpacing;
    }
    
    // Manual calculation of the width of text of TMP component, with the option to only consider visible characters.
    public static float GetTotalWidth (TMP_TextInfo textInfo, bool visibleOnly) {
        var totalWidth = 0f;
        for (int i = 0; i < textInfo.characterInfo.Length; i++) {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            if (visibleOnly && !characterInfo.isVisible) continue;
            var characterWidth = GetCharacterWidth(characterInfo);
            totalWidth += characterWidth;    
        }
        return totalWidth;
    }

    public static float GetCharacterWidth(TMP_CharacterInfo characterInfo) {
        return characterInfo.topRight.x - characterInfo.topLeft.x;
    }
    public static int GetNumCharacters(TMP_TextInfo textInfo, bool visibleOnly) {
        if(!visibleOnly) return textInfo.characterInfo.Length;
        int numCharacters = 0;
        for (int i = 0; i < textInfo.characterInfo.Length; i++) 
            if(textInfo.characterInfo[i].isVisible) 
                numCharacters++;
        return numCharacters;
    }
    
    public static bool IsAnyWordSplit (TMP_TextInfo textInfo) {
        for(int i = 0; i < textInfo.characterInfo.Length-1; i++) {
            if(textInfo.characterInfo[i].lineNumber == textInfo.characterInfo[i+1].lineNumber) continue;
            var lastCharInLine = textInfo.characterInfo[i].character;
            if(!char.IsSeparator(lastCharInLine) && !char.IsPunctuation(lastCharInLine)) {
                // Debug.LogWarning(textInfo.characterInfo[i].character +" "+ textInfo.characterInfo[i+1].character+" "+textInfo.characterInfo[i].lineNumber+" "+textInfo.characterInfo[i+1].lineNumber);
                return true;
            }
        }
        return false;
        // foreach(var line in text.textInfo.lineInfo) {
        // 	if(line.characterCount == 0) continue;
        // 	Debug.Log(line.lastCharacterIndex+" "+text.textInfo.characterInfo[line.lastCharacterIndex].character);
        // }
        // foreach(var word in text.textInfo.wordInfo) {
        // 	// if(word.characterCount == 0) continue;
        // 	Debug.Log(word.firstCharacterIndex+", "+word.lastCharacterIndex+": "+text.text.Substring(word.firstCharacterIndex, word.lastCharacterIndex-word.firstCharacterIndex+1));
        // 	// Debug.Log(word.characterCount);
        // 	if(text.textInfo.characterInfo[word.firstCharacterIndex].lineNumber != text.textInfo.characterInfo[word.lastCharacterIndex].lineNumber) {
        // 		Debug.LogWarning(text.text.Substring(word.firstCharacterIndex, word.lastCharacterIndex-word.firstCharacterIndex+1));
        // 	}
        // }	
    }
    
    public static string ReplaceUnsupportedQuoteMarks (string textString, TMP_FontAsset font) {
        if(!font.glyphLookupTable.ContainsKey('”') || !font.glyphLookupTable.ContainsKey('“')) {
            textString = textString.Replace('”', '"');
            textString = textString.Replace('“', '"');
        }
        if(!font.glyphLookupTable.ContainsKey('’')) {
            textString = textString.Replace('’', '\'');
        }
        return textString;
    }
    #endregion


    #region Private Utilities
    // Extract the font asset from the style's opening tag
    static bool GetFontFromStyle(TMP_Style style, out TMP_FontAsset font) {
        string openingTag = style.styleOpeningDefinition;
        Regex fontRegex = new Regex(@"<font=""(.*?)"">");
        Match match = fontRegex.Match(openingTag);
        if (match.Success) {
            string fontPath = match.Groups[1].Value;
            fontPath = $"{TMP_Settings.defaultFontAssetPath}{fontPath}";
            font = Resources.Load<TMP_FontAsset>(fontPath);
            return font != null;
        }
        font = null;
        return false;
    }

    // Extract the font size from the style's opening tag
    static bool GetFontSizeFromStyle(TMP_Style style, out float fontSize) {
        string openingTag = style.styleOpeningDefinition;
        Regex sizeRegex = new Regex(@"<size=(\d+)>");
        Match match = sizeRegex.Match(openingTag);
        if (match.Success && float.TryParse(match.Groups[1].Value, out fontSize)) return true;
        fontSize = 0;
        return false;
    }
    
    // Extract the line spacing from the style's opening tag
    static bool GetLineSpacingFromStyle(TMP_Style style, out float lineSpacing) {
        string openingTag = style.styleOpeningDefinition;
        Regex lineSpacingRegex = new Regex(@"<line-height=(\d+)>");
        Match match = lineSpacingRegex.Match(openingTag);
        if (match.Success && float.TryParse(match.Groups[1].Value, out lineSpacing)) return true;
        lineSpacing = 0;
        return false;
    }
    
    
    
    static Rect CreateEncapsulating (params Rect[] rects) {
        Rect rect = new Rect(rects[0]);
        for(int i = 1; i < rects.Length; i++)
            rect = Encapsulating(rect, rects[i]);
        return rect;
    }
    
    static Rect CreateEncapsulating (params Vector2[] vectors) {
        float xMin = vectors[0].x;
        float xMax = vectors[0].x;
        float yMin = vectors[0].y;
        float yMax = vectors[0].y;
        for(int i = 1; i < vectors.Length; i++) {
            var vector = vectors[i];
            xMin = Mathf.Min (xMin, vector.x);
            xMax = Mathf.Max (xMax, vector.x);
            yMin = Mathf.Min (yMin, vector.y);
            yMax = Mathf.Max (yMax, vector.y);
        }
        return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
    }
    
    static Rect Encapsulating(Rect r, Rect rect) {
        r = r.Encapsulating(rect.min);
        r = r.Encapsulating(rect.max);
        return r;
    }
	
    static Rect Encapsulating(Rect r, Vector2 point) {
        var xMin = Mathf.Min (r.xMin, point.x);
        var xMax = Mathf.Max (r.xMax, point.x);
        var yMin = Mathf.Min (r.yMin, point.y);
        var yMax = Mathf.Max (r.yMax, point.y);
        return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
    }
    #endregion
}