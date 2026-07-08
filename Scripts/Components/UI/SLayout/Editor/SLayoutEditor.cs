using UnityEngine;
using UnityEditor;

namespace UnityX.SLayouts.Editor {

	[CustomEditor(typeof(SLayout)), CanEditMultipleObjects]
	public class SLayoutEditor : UnityEditor.Editor {

		SLayout data => (SLayout)target;

		public override void OnInspectorGUI () {
			Undo.RecordObject(data.rectTransform, "Modified SLayout");
			var newRect = EditorGUILayout.RectField("Layout", data.rect);
			if (newRect != data.rect) {
				data.rect = newRect;
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("originTopLeft"));
			serializedObject.ApplyModifiedProperties();
		}
	}
}
