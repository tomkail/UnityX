using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PreviewTextureAttribute))]
public class PreviewTextureDrawer : BaseAttributePropertyDrawer<PreviewTextureAttribute> {

	protected override bool IsSupported (SerializedProperty property) {
		return property.propertyType == SerializedPropertyType.ObjectReference && GetTexture(property) != null;
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        if (IsSupported(property)) {
			return base.GetPropertyHeight(property, label) + attribute.height;
        } else {
			return base.GetPropertyHeight(property, label);
        }
    }

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		position.height = base.GetPropertyHeight(property, label);
		EditorGUI.PropertyField(position, property, label);

		Texture texture = GetTexture(property);
		if(texture != null) {
			position.y += base.GetPropertyHeight(property, label);
			position.width = attribute.width;
			position.height = attribute.height;
			Rect textureRect = GetTextureRectUV(property);
			if(textureRect == new Rect(0,0,1,1)) {
				EditorGUI.DrawPreviewTexture(position, texture, null, attribute.scaleMode);
			} else {
                Sprite sprite = (Sprite)property.objectReferenceValue;
                var aspect = sprite.rect.width/sprite.rect.height;
                position = CompressToFitAspectRatio(position, aspect);
				GUI.DrawTextureWithTexCoords(position, texture, textureRect);
			}
		}

		EditorGUI.EndProperty();
	}

	Texture GetTexture (SerializedProperty property) {
		if(property.objectReferenceValue == null) 
			return null;
		System.Type type = property.objectReferenceValue.GetType();
		if(typeof (Texture2D).IsAssignableFrom(type)) {
			return ((Texture2D)property.objectReferenceValue);
		} else if(typeof (Sprite).IsAssignableFrom(type)) {
 			return ((Sprite)property.objectReferenceValue).texture;
		} else if(typeof (RenderTexture).IsAssignableFrom(type)) {
 			return ((RenderTexture)property.objectReferenceValue);
		} else {
			return null;
		}
	}

	Rect GetTextureRectUV (SerializedProperty property) {
		if(property.objectReferenceValue == null) 
			return new Rect(0,0,1,1);
		System.Type type = property.objectReferenceValue.GetType();
		if(typeof (Sprite).IsAssignableFrom(type)) {
			Sprite sprite = (Sprite)property.objectReferenceValue;
			return new Rect(sprite.textureRect.x/sprite.texture.width, sprite.textureRect.y/sprite.texture.height, sprite.textureRect.width/sprite.texture.width, sprite.textureRect.height/sprite.texture.height);
		} else {
			return new Rect(0,0,1,1);
		}
	}

    public static Rect CompressToFitAspectRatio (Rect rect, float targetAspect) {
        var rectAspect = rect.width/rect.height;
		var newSize = rect.size;
        if(targetAspect > rectAspect) {
            newSize.y = rect.height * (rectAspect / targetAspect);
        } else if(targetAspect < rectAspect) {
            newSize.x = rect.width * (targetAspect / rectAspect);
        }
		var offset = new Vector2((newSize.x-rect.width) * -0.5f, (newSize.y-rect.height) * -0.5f);
		return new Rect(rect.min + offset, newSize);
    }
}