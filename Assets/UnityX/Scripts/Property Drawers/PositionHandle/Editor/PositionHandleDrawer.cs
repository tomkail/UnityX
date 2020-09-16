using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof (PositionHandleAttribute))]
public class PositionHandleDrawer : BaseAttributePropertyDrawer<PositionHandleAttribute> {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}
		EditorGUI.BeginProperty(position, label, property);
		var vector3Rect = new Rect(position.x, position.y, position.width - 80, position.height);
		var lookButtonRect = new Rect(position.x + (position.width - 80), position.y, 40, position.height);
		var setButtonRect = new Rect(position.x + (position.width - 40), position.y, 40, position.height);

		property.vector3Value = EditorGUI.Vector3Field(vector3Rect, label, property.vector3Value);

		GUISkin _editorSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
		EditorGUI.BeginDisabledGroup(SceneView.lastActiveSceneView == null);
		if (GUI.Button(lookButtonRect, new GUIContent(_editorSkin.GetStyle("VisibilityToggle").onNormal.background, "Moves the scene view camera to the point"))) 
			SceneView.lastActiveSceneView.LookAt(property.vector3Value, SceneView.lastActiveSceneView.rotation);
		EditorGUI.EndDisabledGroup();
		EditorGUI.BeginDisabledGroup(SceneView.lastActiveSceneView == null);
		if (GUI.Button(setButtonRect, new GUIContent(_editorSkin.GetStyle("OL Plus").normal.background, "Sets the point to the scene view camera focal point"))) 
			property.vector3Value = SceneView.lastActiveSceneView.pivot;
		EditorGUI.EndDisabledGroup();

		SceneGUIDrawer.DrawOnce(this, () => {
			try {
				property.serializedObject.Update();
				if(Tools.pivotRotation == PivotRotation.Global) {
					property.vector3Value = Handles.PositionHandle(property.vector3Value, Quaternion.identity);
				} else {
					Transform transform = null;
					if(property.serializedObject.targetObject is Component) transform = ((Component)property.serializedObject.targetObject).transform;
					if(transform == null) property.vector3Value = Handles.PositionHandle(property.vector3Value, Quaternion.identity);
					else property.vector3Value = Handles.PositionHandle(property.vector3Value, transform.rotation);
				}
				property.serializedObject.ApplyModifiedProperties();
				
				if(attribute.showName) {
					Handles.BeginGUI ();
					Vector3 midPointScreenPoint = HandleUtility.WorldToGUIPointWithDepth (property.vector3Value);
					if (midPointScreenPoint.z > 0) {
						var gc = new GUIContent(property.displayName);
						var style = EditorStyles.centeredGreyMiniLabel;
						midPointScreenPoint.y += 20;
						var rect = RectX.CreateFromCenter (midPointScreenPoint, new Vector2(200, 16));
						GUI.Label (rect, gc, style);
					}
					Handles.EndGUI ();
				}
			} catch {}
			// This can not work for a few reasons. Use this for debugging
			// catch (System.Exception e) {
				//Debug.LogWarning(e);
			// }
		});
		
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return EditorGUI.GetPropertyHeight(property, label);
	}
	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.Vector3;
	}
}