using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ExtendedSlider), true)]
[CanEditMultipleObjects]
public class ExtendedSliderEditor : SliderEditor {

	SerializedProperty onDownProperty;
	SerializedProperty onUpProperty;
	SerializedProperty onEnterProperty;
	SerializedProperty onExitProperty;
	SerializedProperty onSelectProperty;
	SerializedProperty onDeselectProperty;
	SerializedProperty onMoveProperty;
	SerializedProperty onDragProperty;

	protected override void OnEnable()
    {
        base.OnEnable();
		onDownProperty = serializedObject.FindProperty("onDown");
		onUpProperty = serializedObject.FindProperty("onUp");
		onEnterProperty = serializedObject.FindProperty("onEnter");
		onExitProperty = serializedObject.FindProperty("onExit");
		onSelectProperty = serializedObject.FindProperty("onSelect");
		onDeselectProperty = serializedObject.FindProperty("onDeselect");
		onMoveProperty = serializedObject.FindProperty("onMove");
		onDragProperty = serializedObject.FindProperty("onDrag");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();
		EditorGUILayout.PropertyField(onDownProperty);
		EditorGUILayout.PropertyField(onUpProperty);
		EditorGUILayout.PropertyField(onEnterProperty);
		EditorGUILayout.PropertyField(onExitProperty);
		EditorGUILayout.PropertyField(onSelectProperty);
		EditorGUILayout.PropertyField(onDeselectProperty);
		EditorGUILayout.PropertyField(onMoveProperty);
		EditorGUILayout.PropertyField(onDragProperty);
        serializedObject.ApplyModifiedProperties();
    }
}