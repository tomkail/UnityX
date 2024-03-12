using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof (SpringHandler))]
public class SpringHandlerPropertyDrawer : PropertyDrawer {
	
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
		
		var time = property.FindPropertyRelative("time");
		var startValue = property.FindPropertyRelative("startValue");
		var endValue = property.FindPropertyRelative("endValue");
		var initialVelocity = property.FindPropertyRelative("initialVelocity");
		
		var springDamping = property.FindPropertyRelative("_spring").FindPropertyRelative("_damping");
		var springMass = property.FindPropertyRelative("_spring").FindPropertyRelative("_mass");
		var springStiffness = property.FindPropertyRelative("_spring").FindPropertyRelative("_stiffness");

		var showNullRect = property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue != null;
		var bodyX = position.x + EditorGUIUtility.labelWidth;
		var bodyWidth = position.width - EditorGUIUtility.labelWidth - (showNullRect ? 48+8 : 0);
		
		var cachedLabelWidth = EditorGUIUtility.labelWidth;
		Rect valueRect = new Rect(bodyX, position.y, bodyWidth*0.5f, EditorGUIUtility.singleLineHeight);
		Rect velocityRect = new Rect(bodyX+bodyWidth*0.5f+8, position.y, bodyWidth*0.5f-8, EditorGUIUtility.singleLineHeight);
		Rect nullRect = new Rect(bodyX+bodyWidth*0.5f+8, position.y, bodyWidth*0.5f-8, EditorGUIUtility.singleLineHeight);

		EditorGUI.BeginDisabledGroup(true);
		EditorGUIUtility.labelWidth = 36;
		var springValue = Spring.Value(startValue.floatValue, endValue.floatValue, initialVelocity.floatValue, time.floatValue, springMass.floatValue, springStiffness.floatValue, springDamping.floatValue);
		EditorGUI.FloatField(valueRect, new GUIContent(new GUIContent("Value")), springValue);
		var springVelocity = Spring.Velocity(startValue.floatValue, endValue.floatValue, initialVelocity.floatValue, time.floatValue, springMass.floatValue, springStiffness.floatValue, springDamping.floatValue);
		EditorGUIUtility.labelWidth = 50;
		EditorGUI.FloatField(velocityRect, new GUIContent(new GUIContent("Velocity")), springVelocity);
		EditorGUI.EndDisabledGroup();
		EditorGUIUtility.labelWidth = cachedLabelWidth;
		
		if(showNullRect && GUI.Button(new Rect(position.xMax-48, position.y, 48, EditorGUIUtility.singleLineHeight), new GUIContent("Null"))) {
			property.managedReferenceValue = null;
			EditorGUI.EndProperty();
			return;
		}
		
		property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, property.displayName, true);
		
		
		if (property.isExpanded) {
			EditorGUI.indentLevel++;
			
			Rect timeRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 1, position.width, EditorGUIUtility.singleLineHeight);
			Rect startValueRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2, position.width, EditorGUIUtility.singleLineHeight);
			Rect endValueRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3, position.width, EditorGUIUtility.singleLineHeight);
			Rect initialVelocityRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4, position.width, EditorGUIUtility.singleLineHeight);
			
			
			EditorGUI.PropertyField(timeRect, time, new GUIContent(new GUIContent("Time")));
			EditorGUI.PropertyField(startValueRect, startValue, new GUIContent(new GUIContent("Start Value")));
			EditorGUI.PropertyField(endValueRect, endValue, new GUIContent(new GUIContent("End Value")));
			EditorGUI.PropertyField(initialVelocityRect, initialVelocity, new GUIContent(new GUIContent("Initial Velocity")));
				
			var springPropertyDrawer = new SpringPropertyDrawer();
			springPropertyDrawer.Draw(new Rect(position.x, initialVelocityRect.yMax+EditorGUIUtility.standardVerticalSpacing, position.width, position.height), property.FindPropertyRelative("_spring"), label, startValue.floatValue, endValue.floatValue, initialVelocity.floatValue, time.floatValue);
			EditorGUI.indentLevel--;
		}
		
		EditorGUI.EndProperty ();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null) {
			return EditorGUIUtility.singleLineHeight;
		} else {
			if (property.isExpanded) {
				var springPropertyDrawer = new SpringPropertyDrawer();
				var springHeight = springPropertyDrawer.GetPropertyHeight(property.FindPropertyRelative("_spring"), label);
				return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 5 + springHeight;
			}
			return EditorGUIUtility.singleLineHeight;
		}
	}
}
