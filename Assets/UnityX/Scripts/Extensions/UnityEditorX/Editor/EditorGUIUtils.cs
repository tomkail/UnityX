#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class EditorGUIUtils {
    public static void ShowSpritePickerForLabel (Sprite currentSprite, string prefixLabel, string searchFilterLabel, System.Action<Sprite> SetFunc, float maxWidth) {
        var searchFilter = "t:Sprite l:"+searchFilterLabel;
        GUILayout.BeginHorizontal();
        if(GUILayout.Button(prefixLabel, GUILayout.Width(maxWidth-44))) {
            EditorGUIUtility.ShowObjectPicker<Sprite>(currentSprite, false, searchFilter, -1);
        }
        
        EditorGUI.BeginDisabledGroup(currentSprite == null);
        if(GUILayout.Button("<", EditorStyles.miniButtonLeft, GUILayout.Width(18))) {
            var validSprites = AssetDatabase.FindAssets(searchFilter);
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(currentSprite));
            var index = System.Array.IndexOf(validSprites, guid);
            var assetPath = AssetDatabase.GUIDToAssetPath(index == -1 ? validSprites.Random() : validSprites.GetRepeating(index-1));
            SetFunc(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath));
        }
        if(GUILayout.Button(">", EditorStyles.miniButtonRight, GUILayout.Width(18))) {
            var validSprites = AssetDatabase.FindAssets(searchFilter);
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(currentSprite));
            var index = System.Array.IndexOf(validSprites, guid);
            var assetPath = AssetDatabase.GUIDToAssetPath(index == -1 ? validSprites.Random() : validSprites.GetRepeating(index+1));
            SetFunc(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath));
        }
        EditorGUI.EndDisabledGroup();
        // if(GUILayout.Button("?", GUILayout.Width(18))) {
        //     var validSprites = AssetDatabase.FindAssets(searchFilter).AsEnumerable();
        //     if(currentSprite != null) {
        //         var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(currentSprite));
        //         validSprites = validSprites.Where(x => x != guid);
        //     }
        //     var assetPath = AssetDatabase.GUIDToAssetPath(validSprites.Random());
        //     SetFunc(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath));
        // }
        GUILayout.EndHorizontal();
    }

    
    // Utils for dealing with sprites in editor code
    public static Texture2D GetTexture (SerializedProperty property) {
        if(property.objectReferenceValue == null) 
            return null;
        System.Type type = property.objectReferenceValue.GetType();
        if(typeof (Texture2D).IsAssignableFrom(type)) {
            return ((Texture2D)property.objectReferenceValue);
        } else if(typeof (Sprite).IsAssignableFrom(type)) {
            return ((Sprite)property.objectReferenceValue).texture;
        } else {
            return null;
        }
    }
    public static Rect GetTextureRectUV (SerializedProperty property) {
        if(property.objectReferenceValue == null) 
            return new Rect(0,0,1,1);
        System.Type type = property.objectReferenceValue.GetType();
        if(typeof (Sprite).IsAssignableFrom(type)) {
            Sprite sprite = (Sprite)property.objectReferenceValue;
            var spriteRect = sprite.textureRect.width == 0 ? sprite.rect : sprite.textureRect;
            return new Rect(spriteRect.x/sprite.texture.width, spriteRect.y/sprite.texture.height, spriteRect.width/sprite.texture.width, spriteRect.height/sprite.texture.height);
        } else {
            return new Rect(0,0,1,1);
        }
    }
    public static float GetTextureAspect (SerializedProperty property) {
        if(property.objectReferenceValue == null) 
            return 1f;
        System.Type type = property.objectReferenceValue.GetType();
        if(typeof (Texture2D).IsAssignableFrom(type)) {
            var texture = ((Texture2D)property.objectReferenceValue);
            return texture.width/(float)texture.height;
        } else if(typeof (Sprite).IsAssignableFrom(type)) {
            Sprite sprite = (Sprite)property.objectReferenceValue;
            var spriteRect = sprite.textureRect.width == 0 ? sprite.rect : sprite.textureRect;
            return spriteRect.width/spriteRect.height;
        } else {
            return 1;
        }
    }
    public static Rect CompressToFitAspectRatio (Rect rect, float targetAspect, Vector2 pivot) {
        var rectAspect = rect.size.x/rect.size.y;
        var newSize = rect.size;
        if(targetAspect > rectAspect) {
            newSize.y = rect.height * (rectAspect / targetAspect);
        } else if(targetAspect < rectAspect) {
            newSize.x = rect.width * (targetAspect / rectAspect);
        }
        var offset = new Vector2((newSize.x-rect.width) * -pivot.x, (newSize.y-rect.height) * -pivot.y);
        return new Rect(rect.min + offset, newSize);
    }
}
#endif