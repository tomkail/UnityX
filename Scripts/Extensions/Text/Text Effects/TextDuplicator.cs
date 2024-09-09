using System.Linq;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Text))]
public class TextDuplicator : MonoBehaviour
{
    public TMP_Text m_TextComponent;
    public TMP_Text duplicated;

    void Update() {
        if (duplicated == null) {
            // duplicated = Object.Instantiate<TMP_Text>(m_TextComponent, transform, true);
                
        }

        CopyNonStyleProperties(m_TextComponent, duplicated);
    }

    public static void CopyNonStyleProperties(TMP_Text source, TMP_Text dest) {
        if(dest.alignment != source.alignment) dest.alignment = source.alignment;
        if(dest.font != source.font) dest.font = source.font;
        if(dest.margin != source.margin) dest.margin = source.margin;
        if(dest.text != source.text) dest.text = source.text;
        if(dest.characterSpacing != source.characterSpacing) dest.characterSpacing = source.characterSpacing;
        if (!dest.fontFeatures.SequenceEqual(source.fontFeatures)) {
            dest.fontFeatures.Clear();
            dest.fontFeatures.AddRange(source.fontFeatures);
        }
        if(dest.extraPadding != source.extraPadding) dest.extraPadding = source.extraPadding;
        if(dest.fontSize != source.fontSize) dest.fontSize = source.fontSize;
        if(dest.fontStyle != source.fontStyle) dest.fontStyle = source.fontStyle;
        if(dest.fontWeight != source.fontWeight) dest.fontWeight = source.fontWeight;
        if(dest.horizontalAlignment != source.horizontalAlignment) dest.horizontalAlignment = source.horizontalAlignment;
        if(dest.horizontalMapping != source.horizontalMapping) dest.horizontalMapping = source.horizontalMapping;
        if(dest.ignoreVisibility != source.ignoreVisibility) dest.ignoreVisibility = source.ignoreVisibility;
        if(dest.isOverlay != source.isOverlay) dest.isOverlay = source.isOverlay;
        if(dest.lineSpacing != source.lineSpacing) dest.lineSpacing = source.lineSpacing;
        if(dest.overflowMode != source.overflowMode) dest.overflowMode = source.overflowMode;
        if(dest.paragraphSpacing != source.paragraphSpacing) dest.paragraphSpacing = source.paragraphSpacing;
        if(dest.renderMode != source.renderMode) dest.renderMode = source.renderMode;
        if(dest.richText != source.richText) dest.richText = source.richText;
        if(dest.spriteAsset != source.spriteAsset) dest.spriteAsset = source.spriteAsset;
        if(dest.styleSheet != source.styleSheet) dest.styleSheet = source.styleSheet;
        if(dest.textStyle != source.textStyle) dest.textStyle = source.textStyle;
        if(dest.verticalAlignment != source.verticalAlignment) dest.verticalAlignment = source.verticalAlignment;
        if(dest.verticalMapping != source.verticalMapping) dest.verticalMapping = source.verticalMapping;
        if(dest.wordSpacing != source.wordSpacing) dest.wordSpacing = source.wordSpacing;
        if(dest.characterWidthAdjustment != source.characterWidthAdjustment) dest.characterWidthAdjustment = source.characterWidthAdjustment;
        if(dest.enableAutoSizing != source.enableAutoSizing) dest.enableAutoSizing = source.enableAutoSizing;
        if(dest.fontSizeMin != source.fontSizeMin) dest.fontSizeMin = source.fontSizeMin;
        if(dest.fontSizeMax != source.fontSizeMax) dest.fontSizeMax = source.fontSizeMax;
        if(dest.geometrySortingOrder != source.geometrySortingOrder) dest.geometrySortingOrder = source.geometrySortingOrder;
        if(dest.lineSpacingAdjustment != source.lineSpacingAdjustment) dest.lineSpacingAdjustment = source.lineSpacingAdjustment;
        if(dest.maxVisibleCharacters != source.maxVisibleCharacters) dest.maxVisibleCharacters = source.maxVisibleCharacters;
        if(dest.maxVisibleWords != source.maxVisibleWords) dest.maxVisibleWords = source.maxVisibleWords;
        if(dest.maxVisibleLines != source.maxVisibleLines) dest.maxVisibleLines = source.maxVisibleLines;
        if(dest.overrideColorTags != source.overrideColorTags) dest.overrideColorTags = source.overrideColorTags;
        if(dest.pageToDisplay != source.pageToDisplay) dest.pageToDisplay = source.pageToDisplay;
        if(dest.parseCtrlCharacters != source.parseCtrlCharacters) dest.parseCtrlCharacters = source.parseCtrlCharacters;
        if(dest.tintAllSprites != source.tintAllSprites) dest.tintAllSprites = source.tintAllSprites;
        if(dest.autoSizeTextContainer != source.autoSizeTextContainer) dest.autoSizeTextContainer = source.autoSizeTextContainer;
        if(dest.mappingUvLineOffset != source.mappingUvLineOffset) dest.mappingUvLineOffset = source.mappingUvLineOffset;
        if(dest.useMaxVisibleDescender != source.useMaxVisibleDescender) dest.useMaxVisibleDescender = source.useMaxVisibleDescender;
        if(dest.isRightToLeftText != source.isRightToLeftText) dest.isRightToLeftText = source.isRightToLeftText;
    }
}