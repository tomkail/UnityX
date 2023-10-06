// https://github.com/anchan828/property-drawer-collection
// Copyright (C) 2014 Keigo Ando
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PasswordAttribute))]
public class PasswordDrawer : BaseAttributePropertyDrawer<PasswordAttribute> {
	
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!IsSupported(property)) {
			DrawNotSupportedGUI(position, property, label);
			return;
		}

		string password = property.stringValue;
		int maxLength = attribute.maxLength;
        position.height = 16;
        if (property.stringValue.Length > maxLength) {
            password = password.Substring(0, maxLength);
        }

		if (!attribute.useMask) {
            property.stringValue = EditorGUI.TextField(position, label, password);
        } else {
            property.stringValue = EditorGUI.PasswordField(position, label, password);
        }

        if (IsValid(property)) {
            DrawHelpBox(position);
        }
    }

    void DrawHelpBox(Rect position) {
        position.x += 10;
        position.y += 20;
        position.width -= 10;
        position.height += 8;
		EditorGUI.HelpBox(position, string.Format("Password must contain at least {0} characters!", attribute.minLength), MessageType.Error);
    }

	protected override bool IsSupported(SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.String;
	}

    bool IsValid(SerializedProperty property) {
		return property.stringValue.Length < attribute.minLength;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (IsSupported(property)) {
			if (property.stringValue.Length < attribute.minLength) {
                return base.GetPropertyHeight(property, label) + 30;
            }
        }
        return base.GetPropertyHeight(property, label);
    }
}