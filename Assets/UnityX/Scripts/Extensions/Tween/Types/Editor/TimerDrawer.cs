using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof (Timer))]
public class TimerDrawer : PropertyDrawer {

	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty (position, label, property);
		
		// Draw label
		position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

		// Calculate rects
		var repeatRect = new Rect (position.x + position.width - 60, position.y, 60, position.height);
		var contentRect = new Rect(position.x, position.y, position.width-repeatRect.width, position.height);
//		var unitRect = new Rect (position.x+55, position.y, 40, position.height);
//		var nameRect = new Rect (position.x+100, position.y, position.width-100, position.height);
		
		// Draw fields - passs GUIContent.none to each so they are drawn without labels
//		EditorGUI.PropertyField (amountRect, property.FindPropertyRelative ("state"), GUIContent.none);
//		EditorGUI.PropertyField (unitRect, property.FindPropertyRelative ("currentTime"), GUIContent.none);
//		EditorGUI.PropertyField (nameRect, property.FindPropertyRelative ("_targetTime"), GUIContent.none);


		string text = ((Timer.State)property.FindPropertyRelative ("state").enumValueIndex).ToString();

		float progress = 0;
		var currentProperty = property.FindPropertyRelative("currentTime");
		if(property.FindPropertyRelative("useTargetTime").boolValue) {
			var targetProperty = property.FindPropertyRelative("_targetTime");
			if(targetProperty.floatValue > 0) {
				text += " "+targetProperty.floatValue;
				progress = currentProperty.floatValue/targetProperty.floatValue;
			}

			EditorGUI.ProgressBar(contentRect, progress, text);

			var repeatProperty = property.FindPropertyRelative("repeatForever");
			// EditorGUI.PropertyField(repeatRect, repeatProperty);
			string repeatText = repeatProperty.boolValue ? "∞" : (property.FindPropertyRelative("targetRepeats").intValue).ToString();
			repeatProperty.boolValue = GUI.Toggle(repeatRect, repeatProperty.boolValue, repeatText);
		} else {	
			EditorGUI.PropertyField(contentRect, currentProperty);
		}

		EditorGUI.EndProperty ();
	}
}
