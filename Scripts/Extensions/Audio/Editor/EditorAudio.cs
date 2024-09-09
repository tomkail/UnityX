using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
 
public static class EditorAudio {
    static Type audioUtilType {
        get {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            return unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        }
    }
    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Audio/Bindings/AudioUtil.bindings.cs
    public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false) {
        if(clip == null) return;
        MethodInfo method = audioUtilType.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof(AudioClip), typeof(int), typeof(bool)}, null);
        method.Invoke(null, new object[] { clip, startSample, loop });
    }
 
    public static void StopAllClips() {
        MethodInfo method = audioUtilType.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public, null, new Type[] {}, null);
        method.Invoke( null, new object[] { });
    }
    
    public static bool IsPreviewClipPlaying() {
        MethodInfo method = audioUtilType.GetMethod("IsPreviewClipPlaying", BindingFlags.Static | BindingFlags.Public, null, new Type[] {}, null);
        return (bool)method.Invoke( null, new object[] { });
    }
}