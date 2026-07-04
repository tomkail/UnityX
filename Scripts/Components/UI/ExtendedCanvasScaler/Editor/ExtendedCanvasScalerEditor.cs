using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ExtendedCanvasScaler), true)]
[CanEditMultipleObjects]
public class ExtendedCanvasScalerEditor : CanvasScalerEditor {

	SerializedProperty m_useCameraSizeInsteadOfScreenSize;
	SerializedProperty scaleMultiplier;

	protected override void OnEnable() {
		base.OnEnable();
		m_useCameraSizeInsteadOfScreenSize = serializedObject.FindProperty("m_useCameraSizeInsteadOfScreenSize");
		scaleMultiplier = serializedObject.FindProperty("scaleMultiplier");
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();
		EditorGUILayout.PropertyField(m_useCameraSizeInsteadOfScreenSize);
		EditorGUILayout.PropertyField(scaleMultiplier);
		serializedObject.ApplyModifiedProperties();
		base.OnInspectorGUI ();
	}	
}
