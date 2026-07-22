using TMPro;

public static class TMPUtils {
    public static float GetTotalWidth (TMP_TextInfo textInfo, bool visibleOnly) {
        var totalWidth = 0f;
        for (int i = 0; i < textInfo.characterCount; i++) {
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
        for (int i = 0; i < textInfo.characterCount; i++) 
            if(textInfo.characterInfo[i].isVisible) 
                numCharacters++;
        return numCharacters;
    }
}
