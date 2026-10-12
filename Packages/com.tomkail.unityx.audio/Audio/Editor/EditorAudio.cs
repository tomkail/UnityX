using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
 
public static class EditorAudio {
    // AudioUtil is internal, so these are looked up once and null-checked in case a Unity version renames them.
    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Audio/Bindings/AudioUtil.bindings.cs
    static readonly Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
    static readonly MethodInfo playPreviewClip = audioUtilType?.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] {typeof(AudioClip), typeof(int), typeof(bool)}, null);
    static readonly MethodInfo stopAllPreviewClips = audioUtilType?.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
    static readonly MethodInfo isPreviewClipPlaying = audioUtilType?.GetMethod("IsPreviewClipPlaying", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);

    public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false) {
        if(clip == null || playPreviewClip == null) return;
        playPreviewClip.Invoke(null, new object[] { clip, startSample, loop });
    }
 
    public static void StopAllClips() {
        stopAllPreviewClips?.Invoke(null, null);
    }
    
    public static bool IsPreviewClipPlaying() {
        return isPreviewClipPlaying != null && (bool)isPreviewClipPlaying.Invoke(null, null);
    }
}