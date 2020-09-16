using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SLayout)), CanEditMultipleObjects]
public class SLayoutEditor : BaseEditor<SLayout> {
	public override void OnInspectorGUI () {
        Undo.RecordObject(data.rectTransform, "Modified SLayout");
		var newRect = EditorGUILayout.RectField("Layout", data.rect);
		if( newRect != data.rect ) {
			data.rect = newRect;
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty("originTopLeft"));
		serializedObject.ApplyModifiedProperties();
	}
}