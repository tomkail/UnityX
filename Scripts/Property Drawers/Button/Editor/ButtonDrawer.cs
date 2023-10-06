using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonDrawer : BaseAttributePropertyDrawer<ButtonAttribute> {
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		// Gets the object that owns this instance.
		System.Object obj = ReflectionX.GetValueFromObject(property.serializedObject.targetObject, property.propertyPath.BeforeLast("."));
		MethodInfo method = obj.GetType().GetMethod(attribute.methodName, attribute.flags);

		if (method == null) {
			EditorGUI.HelpBox(position, "Method Not Found", MessageType.Error);
		} else {
			if (attribute.useValue) {
				Rect valueRect = new Rect(position.x, position.y, position.width/2f, position.height);
				Rect buttonRect = new Rect(position.x + position.width/2f, position.y, position.width/2f, position.height);

				EditorGUI.PropertyField(valueRect, property, GUIContent.none);
				if (GUI.Button(buttonRect, attribute.buttonName)) {
					foreach(Object targetObject in property.serializedObject.targetObjects) {
						System.Object _obj = ReflectionX.GetValueFromObject(targetObject, property.propertyPath.BeforeLast("."));
						method = _obj.GetType().GetMethod(attribute.methodName, attribute.flags);
						if (method != null) {
							method.Invoke(_obj, new object[]{fieldInfo.GetValue(_obj)});
						}
					}
				}
			} else {
				if (GUI.Button(position, attribute.buttonName)) {
					foreach(Object targetObject in property.serializedObject.targetObjects) {
						System.Object _obj = ReflectionX.GetValueFromObject(targetObject, property.propertyPath.BeforeLast("."));
						method = _obj.GetType().GetMethod(attribute.methodName, attribute.flags);
						if (method != null) {
							method.Invoke(_obj, null);
						}
					}
				}
			}
		}
	}

	protected override bool IsSupported (SerializedProperty property) {
		return true;
	}
}

/*
public class ButtonExample : MonoBehaviour {
	
	[Button("Method2", "Method2 button", true, BindingFlags.NonPublic | BindingFlags.Instance)] public int test1;
	
	[Button("Method2", true, BindingFlags.NonPublic | BindingFlags.Instance)] public int test2;
	
	[Button("Method1", true)] public int test3;
	
	[Button("Method4", "Method4 button", BindingFlags.NonPublic | BindingFlags.Instance)] public int test4;
	
	[Button("Method3", "Method3 button")] public int test5;
	
	[Button("Method4", BindingFlags.NonPublic | BindingFlags.Instance)] public int test6;
	
	[Button("Method3")] public int test7;
	
	public void Method1(int i) {
		Debug.Log("Public Method with Value: "+i.ToString());
	}
	
	void Method2(int i) {
		Debug.Log("Private Method with Value: "+i.ToString());
	}
	
	public void Method3() {
		Debug.Log("Public Method without Value.");
	}
	
	void Method4() {
		Debug.Log("Private Method without Value.");
	}
}*/