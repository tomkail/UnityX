using TMPro;

public class WordHighlightTextEffect : BaseTextMeshProEffect
{
    protected override void OnPreRenderText(TMP_TextInfo textInfo) {
        if (textInfo.characterCount == 0) return;
    }
}