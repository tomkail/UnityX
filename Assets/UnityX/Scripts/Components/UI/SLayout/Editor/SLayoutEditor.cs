using UnityEditor;

[CustomEditor(typeof(SLayout)), CanEditMultipleObjects]
public class SLayoutEditor : Editor {
    public override void OnInspectorGUI () {
        var data = (SLayout) target;
        Undo.RecordObject(data.rectTransform, "Modified SLayout");
        var newRect = EditorGUILayout.RectField("Layout", data.rect);
        if( newRect != data.rect ) {
            data.rect = newRect;
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("originTopLeft"));
        serializedObject.ApplyModifiedProperties();
    }
}