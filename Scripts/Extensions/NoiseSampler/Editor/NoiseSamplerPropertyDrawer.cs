using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof (NoiseSampler))]
public class NoiseSamplerPropertyDrawer : PropertyDrawer {
	Texture2D previewTexture;
	static GUISkin _editorSkin;
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		if(_editorSkin == null) _editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

//		base.OnGUI (position, property, label);
		var simpleSampler = property.GetBaseProperty<NoiseSampler>();

		EditorGUI.BeginProperty (position, label, property);

		Rect currentRect = position.CopyWithHeight(EditorGUIUtility.singleLineHeight);
		currentRect.y = EditorGUIX.DrawSerializedProperty(position, property) + EditorGUIUtility.standardVerticalSpacing;

		if(property.isExpanded) {
//			currentRect = EditorGUI.IndentedRect(currentRect);

//			var realVal = simpleSampler.Sample().value;
//			realVal = Mathf.InverseLerp(-0.5f, 0.5f, realVal);
//			var realColor = ColorX.ToGrayscaleColor(realVal);
//			var realTex = TextureX.Create(realColor);
//
//			EditorGUI.DrawPreviewTexture(currentRect, realTex);
//			currentRect.y += currentRect.height + EditorGUIUtility.standardVerticalSpacing;

			int targetPreviewTextureWidth = Mathf.Max(1, Mathf.FloorToInt(currentRect.width));
//			int targetPreviewTextureHeight = 1;
//			if(previewTexture == null || previewTexture.width != targetPreviewTextureWidth || previewTexture.height != targetPreviewTextureHeight) {
//				previewTexture = new Texture2D(targetPreviewTextureWidth, targetPreviewTextureHeight);
//			}
//			Color[] colors = new Color[previewTexture.width];
			float sampleDistance = 5;
			int numKeys = Mathf.Max(1, Mathf.FloorToInt(targetPreviewTextureWidth / sampleDistance));
			Keyframe[] keys = new Keyframe[numKeys];
			var r = 1f/(numKeys-1);
			for(int i = 0; i < numKeys; i++) {
				var offsetPos = ((r * i) - 0.5f) * numKeys;
				var val = simpleSampler.SampleAtPosition(simpleSampler.position + new Vector3(offsetPos, 0, 0)).value;
				keys[i] = new Keyframe(i, val);
//				val = Mathf.InverseLerp(-0.5f, 0.5f, val);
//				colors[i] = ColorX.ToGrayscaleColor(val);
			}
//			var existingPixels = previewTexture.GetPixels();
//			if(!existingPixels.SequenceEqual(colors)) {
//				previewTexture.SetPixels(colors);
//				previewTexture.Apply();
//			}

//			currentRect.height = 1;
//			EditorGUI.DrawPreviewTexture(currentRect, previewTexture);
//			currentRect.y += currentRect.height + EditorGUIUtility.standardVerticalSpacing;

			EditorGUI.BeginDisabledGroup(true);
			var realVal = simpleSampler.Sample().value;

			EditorGUI.FloatField(currentRect, new GUIContent("Output"), realVal);
			currentRect.y += currentRect.height + EditorGUIUtility.standardVerticalSpacing;


			currentRect.height = 30;

			AnimationCurve curve = new AnimationCurve(keys);
			EditorGUI.CurveField(currentRect, GUIContent.none, curve, Color.white, new Rect(0f, -0.5f, numKeys, 1));

			currentRect = EditorGUI.IndentedRect(currentRect);
			var style = _editorSkin.GetStyle("Grad Up Swatch");
			var tex = style.normal.background;
			GUI.DrawTexture(RectX.CreateFromCenter(currentRect.center.x, currentRect.yMax + tex.height * 0.5f, tex.width, tex.height), tex);

//			GUI.Box(RectX.CreateFromCenter(currentRect.center.x, currentRect.yMax, tex.width, tex.height), realVal.ToString("0.00"), style);

			EditorGUI.EndDisabledGroup();
		}
		EditorGUI.EndProperty ();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label) + (property.isExpanded ? ((EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 1) + 50 + EditorGUIUtility.standardVerticalSpacing) : 0);
	}
}
