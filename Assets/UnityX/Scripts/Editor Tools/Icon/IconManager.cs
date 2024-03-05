#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
#endif
using UnityEngine;

namespace UnityX.Editor.Icon {
	public static class IconManager {
		#if UNITY_EDITOR
	    private static GUIContent[] labelIcons;
	    private static GUIContent[] largeIcons;

		public static void ClearIcon (GameObject gObj) {
			EditorGUIUtility.SetIconForObject(gObj, null);
		}
	    public static Texture2D GetIcon( EditorDefaultLabelIcon icon ) {
	        if ( labelIcons == null ) labelIcons = GetTextures( "sv_label_", string.Empty, 0, 8 );
	        return labelIcons[(int) icon].image as Texture2D;
	    }
	 
	    public static Texture2D GetIcon (EditorDefaultIcon icon) {
	        if ( largeIcons == null ) largeIcons = GetTextures( "sv_icon_dot", "_pix16_gizmo", 0, 16 );
	        return largeIcons[(int) icon].image as Texture2D;
	    }

	    static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count) {
	        GUIContent[] guiContentArray = new GUIContent[count];
	 
	        var t = typeof( EditorGUIUtility );
			var mi = t.GetMethod( "IconContent", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof( string ) }, null );
	 
	        for ( int index = 0; index < count; ++index ) {
	            guiContentArray[index] = mi.Invoke( null, new object[] { baseName + (object)(startIndex + index) + postFix } ) as GUIContent;
	        }
	 
	        return guiContentArray;
	    }
		#endif
	}
}