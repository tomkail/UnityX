using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Text))]
public class StackedTextEffectsController : MonoBehaviour {
    public TMP_Text sourceTextComponent => GetComponent<TMP_Text>();
    public List<TextEffectProperties> effects = new();
    List<TextEffectsController> duplicatedText = new();

    // [Button]
    void ClearViews() {
        duplicatedText = GetComponentsInChildren<TextEffectsController>().ToList();
        for (int i = 0; i < duplicatedText.Count; i++) {
            if (duplicatedText[i] != null) ObjectX.DestroyAutomatic(duplicatedText[i].gameObject);
            duplicatedText.RemoveAt(i);
            i--;
        }
    }

    void OnEnable() {
        ClearViews();
    }
    void OnDisable() {
        ClearViews();
    }

    void Update() {
        for (int i = 0; i < duplicatedText.Count; i++) {
            // Check if there are properties available for this index
            if (i >= effects.Count || duplicatedText[i] == null) {
                if (duplicatedText[i] != null) ObjectX.DestroyAutomatic(duplicatedText[i].gameObject);
                duplicatedText.RemoveAt(i);
                i--;
            } else {
                duplicatedText[i].effects = effects[i];
                TextDuplicator.CopyNonStyleProperties(sourceTextComponent, duplicatedText[i].m_TextComponent);
                // duplicatedText[i].isDirty = true;
                // duplicatedText[i].m_TextComponent.havePropertiesChanged = true;
                // duplicatedText[i].m_TextComponent.ClearMesh();
                // duplicatedText[i].m_TextComponent.SetText(duplicatedText[i].m_TextComponent.text);
                // duplicatedText[i].m_TextComponent.SetMaterialDirty();
                // duplicatedText[i].m_TextComponent.SetAllDirty();
                // duplicatedText[i].m_TextComponent.ForceMeshUpdate();
            }
        }

        if (effects.Count > duplicatedText.Count) {
            for (int i = duplicatedText.Count; i < effects.Count; i++) {
                GameObject newGameObject = new GameObject("Text Effect");
                newGameObject.transform.SetParent(transform, false);
                newGameObject.transform.ResetTransform();
                newGameObject.transform.Translate(Vector3.back * ((i + 1) * 0.001f));
                
                // newGameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                TMP_Text text = null;
                if(sourceTextComponent is TextMeshPro) text = newGameObject.AddComponent<TextMeshPro>();
                else if(sourceTextComponent is TextMeshProUGUI) text = newGameObject.AddComponent<TextMeshProUGUI>();
                TextDuplicator.CopyNonStyleProperties(sourceTextComponent, text);
                
                if (newGameObject.transform is RectTransform rectTransform) {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.sizeDelta = Vector2.zero;
                }
                
                var newComponent = newGameObject.AddComponent<TextEffectsController>();
                duplicatedText.Add(newComponent);
                
                // duplicatedText[i].effects = effects[i];
                // duplicatedText[i].isDirty = true;
            }
        }
    }
}