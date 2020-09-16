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

		public static bool ClearIcon( GameObject gObj ) {
			return SetIcon(gObj, null);
		}
	    public static bool SetIcon( GameObject gObj, LabelIcon icon ) {
	        if ( labelIcons == null ) labelIcons = GetTextures( "sv_label_", string.Empty, 0, 8 );
	        return SetIcon( gObj, labelIcons[(int)icon].image as Texture2D );
	    }
	 
	    public static bool SetIcon( GameObject gObj, Icon icon ) {
	        if ( largeIcons == null ) largeIcons = GetTextures( "sv_icon_dot", "_pix16_gizmo", 0, 16 );
			return SetIcon( gObj, largeIcons[(int)icon].image as Texture2D );
	    }
	 
		public static bool SetIcon( GameObject gObj, Texture2D texture ) {
	        if(GetIcon(gObj) == texture) return false;
			var ty = typeof( EditorGUIUtility );
	        var mi = ty.GetMethod( "SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static );
	        mi.Invoke( null, new object[] { gObj, texture } );
			return true;
	    }

		public static Texture2D GetIcon(GameObject gObj) {
	        var ty = typeof(EditorGUIUtility);
	        var mi = ty.GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
	        var texture = mi.Invoke( null, new object[] {gObj});
			if(texture == null) return null;
			else return (Texture2D)texture;
	    }
	 
	    private static GUIContent[] GetTextures( string baseName, string postFix, int startIndex, int count ) {
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