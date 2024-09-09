using TMPro;
using UnityEngine;

public struct TextRevealAnimatorCalculatedParams {
    public float totalWidth;
    public float totalWidthMinusLast;
    public int numCharacters;

    public float normalizedEffectStart;
    public float normalizedEffectEnd;
    public float normalizedEffectWidth;
    
    public float normalizedEffectStartMinusOne;
    public float normalizedEffectEndMinusOne;
    public float normalizedEffectWidthMinusOne;

    

    public static TextRevealAnimatorCalculatedParams Calculate(TMP_TextInfo textInfo, float animationProgress, float characterEffectWidth) {
        // if (textInfo.characterCount == 0) return;
        var calculatedParams = new TextRevealAnimatorCalculatedParams();
        
        calculatedParams.totalWidth = TMPUtils.GetTotalWidth(textInfo, true);
        calculatedParams.numCharacters = TMPUtils.GetNumCharacters(textInfo, true);

        calculatedParams.normalizedEffectWidth = characterEffectWidth / calculatedParams.numCharacters;
        calculatedParams.normalizedEffectStart = Mathf.Lerp(-calculatedParams.normalizedEffectWidth, 1, animationProgress);
        calculatedParams.normalizedEffectEnd = Mathf.Lerp(0, 1 + calculatedParams.normalizedEffectWidth, animationProgress);
        
        calculatedParams.normalizedEffectWidthMinusOne = characterEffectWidth / (calculatedParams.numCharacters - 1);
        calculatedParams.normalizedEffectStartMinusOne = Mathf.Lerp(-calculatedParams.normalizedEffectWidthMinusOne, 1, animationProgress);
        calculatedParams.normalizedEffectEndMinusOne = Mathf.Lerp(0, 1 + calculatedParams.normalizedEffectWidthMinusOne, animationProgress);

        calculatedParams.totalWidthMinusLast = calculatedParams.totalWidth;
        for (var index = textInfo.characterInfo.Length - 1; index >= 0; index--) {
            var characterInfo = textInfo.characterInfo[index];
            if (characterInfo.isVisible) {
                calculatedParams.totalWidthMinusLast -= TMPUtils.GetCharacterWidth(characterInfo);
                break;
            }
        }

        return calculatedParams;
    }

    public float GetStrength(float currentWidth) {
        var normalizedCurrentWidth = currentWidth / totalWidth;
        return Mathf.InverseLerp(normalizedEffectEnd, normalizedEffectStart, normalizedCurrentWidth);
    }
    
    // This is a bit of a hack. In some cases we don't want to evaluate using the start and end of the text, but rather the start and end-minus-the-last-letter.
    // This is useful when we only want our effect to cover the entire letter rather than the start and end of it.
    // We may want to use the center of the letter instead, and change the range to go from half letter to total width minus half a letter.
    public float GetStrengthMinusOne(float currentWidth) {
        var normalizedCurrentWidth = currentWidth / totalWidthMinusLast;
        return Mathf.InverseLerp(normalizedEffectEndMinusOne, normalizedEffectStartMinusOne, normalizedCurrentWidth);
    }

    
    // public void X(TMP_TextInfo textInfo) {
    //     var currentCharacterDistance = 0f;
    //     foreach (var characterInfo in textInfo.characterInfo) {
    //         if (!characterInfo.isVisible) continue;
    //
    //         var characterWidth = TMPUtils.GetCharacterWidth(characterInfo);
    //
    //         var effectLength = effectEnd - effectStart;
    //         var distanceFromCenter = Mathf.Abs(currentCharacterDistance - Mathf.Lerp(effectEnd, effectStart, 0.5f));
    //         var heightOffset = distanceFromCenter / effectLength * 2;
    //         heightOffset = 1 - Mathf.Clamp01(heightOffset);
    //         heightOffset = Mathf.Sin(heightOffset * Mathf.PI * 0.5f);
    //
    //         currentCharacterDistance += characterWidth;
    //     }
    // }
}