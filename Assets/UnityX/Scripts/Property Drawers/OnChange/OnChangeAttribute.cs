using UnityEngine;

public class OnChangeAttribute : PropertyAttribute {
	public string[] callbackNames;

	public OnChangeAttribute(params string[] callbackNames) {
		this.callbackNames = callbackNames;
	}
}