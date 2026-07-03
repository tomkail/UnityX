#if UNITY_EDITOR
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
			EditorGUIUtility.SetIconForObject( gObj, texture );
			return true;
	    }

		public static Texture2D GetIcon(GameObject gObj) {
			return EditorGUIUtility.GetIconForObject( gObj );
	    }

	    private static GUIContent[] GetTextures( string baseName, string postFix, int startIndex, int count ) {
	        GUIContent[] guiContentArray = new GUIContent[count];
	        for ( int index = 0; index < count; ++index ) {
	            guiContentArray[index] = EditorGUIUtility.IconContent( baseName + (startIndex + index) + postFix );
	        }
	        return guiContentArray;
	    }
		#endif
	}
}
