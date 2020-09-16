using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (PositionLookAtAttribute))]
public class PositionLookAtDrawer : BaseAttributePropertyDrawer<PositionLookAtAttribute> {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		var vector3Rect = new Rect(position.x, position.y, position.width - 40, position.height);
		var buttonRect = new Rect(position.x + (position.width - 40), position.y, 40, position.height);


		property.vector3Value = EditorGUI.Vector3Field(vector3Rect, label, property.vector3Value);

		EditorGUI.BeginDisabledGroup(SceneView.lastActiveSceneView == null);
		if (GUI.Button(buttonRect, new GUIContent(EditorGUIUtility.IconContent("animationvisibilitytoggleon").image, "Moves the scene view camera to the point"))) 
			SceneView.lastActiveSceneView.LookAt(property.vector3Value, SceneView.lastActiveSceneView.rotation);
		EditorGUI.EndDisabledGroup();
		SerializedPropertyX.AddCopyPasteMenu(position, property);
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label);
	}
	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.Vector3;
	}
}