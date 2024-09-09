using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof (NoiseSampler))]
public class NoiseSamplerPropertyDrawer : PropertyDrawer {
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);
		if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null) {
			EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
			if(GUI.Button(new Rect(position.xMax-48, position.y, 48, position.height), new GUIContent("Create"))) {
				property.managedReferenceValue = new SpringHandler(Spring.snappy, 1, 0, 0);
			}
			EditorGUI.EndProperty();
			return;
		}
		
		var positionProp = property.FindPropertyRelative("position");


		property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, property.displayName, true);
		
		
		if (property.isExpanded) {
			EditorGUI.indentLevel++;
			
			var noiseProperties = property.FindPropertyRelative("properties");
			Rect positionRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 1, position.width, EditorGUIUtility.singleLineHeight);
			
			EditorGUI.PropertyField(positionRect, positionProp, new GUIContent(new GUIContent("Position")));

			// if (noiseProperties.isExpanded) {
				var noiseParamsPropertyDrawer = new NoiseSamplerPropertiesPropertyDrawer();
				noiseParamsPropertyDrawer.Draw(new Rect(position.x, positionRect.yMax+EditorGUIUtility.standardVerticalSpacing, position.width, position.height), property.FindPropertyRelative("properties"), label, positionProp.vector3Value);
			// }
			EditorGUI.indentLevel--;
		}
		
		EditorGUI.EndProperty ();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null) {
			return EditorGUIUtility.singleLineHeight;
		} else {
			if (property.isExpanded) {
				var noiseProperties = property.FindPropertyRelative("properties");
				var noiseHeight = 0f;
				// if (noiseProperties.isExpanded) {
					var noiseParamsPropertyDrawer = new NoiseSamplerPropertiesPropertyDrawer();
					noiseHeight = noiseParamsPropertyDrawer.GetPropertyHeight(noiseProperties, label);
				// }

				return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2 + noiseHeight;
			}
			return EditorGUIUtility.singleLineHeight;
		}
	}
}
