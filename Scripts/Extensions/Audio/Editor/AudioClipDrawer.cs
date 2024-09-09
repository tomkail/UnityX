using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AudioClip))]
public class AudioClipDrawer : PropertyDrawer
{
    Texture2D _PlayButtonIcon;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var audioClip = property.objectReferenceValue as AudioClip;

        property.objectReferenceValue = (AudioClip)EditorGUI.ObjectField(new Rect(position.x, position.y, position.width-32, position.height), label, property.objectReferenceValue, typeof(AudioClip), true);
        
        EditorGUI.BeginDisabledGroup(audioClip == null);
        var buttonRect = new Rect(position.xMax-24, position.y, 24, position.height);
        if(EditorAudio.IsPreviewClipPlaying()) {
            if (GUI.Button(buttonRect, stopIcon)) {
                EditorAudio.StopAllClips();
            }
        } else {
            if (GUI.Button(buttonRect, playIcon)) {
                EditorAudio.StopAllClips();
                EditorAudio.PlayClip(audioClip);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    static Texture _playIcon;
    static Texture playIcon {
        get {
            if(_playIcon == null) {
                _playIcon = EditorGUIUtility.IconContent("PlayButton").image;
            }
            return _playIcon;
        }
    }
    static Texture _pauseIcon;
    static Texture pauseIcon {
        get {
            if(_pauseIcon == null) {
                _pauseIcon = EditorGUIUtility.IconContent("PauseButton").image;
            }
            return _pauseIcon;
        }
    }
    static Texture _stopIcon;
    static Texture stopIcon {
        get {
            if(_stopIcon == null) {
                _stopIcon = EditorGUIUtility.IconContent("PreMatQuad").image;
            }
            return _stopIcon;
        }
    }
}
#endif