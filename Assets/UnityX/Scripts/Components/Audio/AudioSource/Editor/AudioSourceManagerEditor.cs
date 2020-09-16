﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioSourceManager)), CanEditMultipleObjects]
public class AudioSourceManagerEditor : BaseEditor<AudioSourceManager> {

	public override void OnInspectorGUI () {
		serializedObject.Update();
		this.Repaint();
//		base.OnInspectorGUI ();
		
		var audioSources = data.gameObject.GetComponents<AudioSource>();
		if(audioSources != null) {
			if(audioSources.Length == 1) serializedObject.FindProperty("_audioSource").objectReferenceValue = audioSources[0];
			else if(audioSources.Length > 1) EditorGUILayout.PropertyField(serializedObject.FindProperty("_audioSource"));
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty("startPlayingAtRandomTime"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseWhenTimescaleIsZero"));
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
		if(!Application.isPlaying) {
			EditorGUILayoutX.ProgressBar("Not in Play Mode", 0);
		} else if(data.audioSource == null) {
			EditorGUILayoutX.ProgressBar("No Clip", 0);
		} else if(data.audioSource.isPlaying) {
			EditorGUILayoutX.ProgressBar("Playing", data.normalizedTime);
			if(GUILayout.Button("Pause")) {
				data.audioSource.Pause();
			}
		} else if(data.paused) {
			EditorGUILayoutX.ProgressBar("Paused", data.normalizedTime);
			if(GUILayout.Button("Resume")) {
				data.audioSource.Play();
			}
		} else {
			EditorGUILayoutX.ProgressBar("Stopped", data.normalizedTime);
			if(GUILayout.Button("Play")) {
				data.audioSource.Play();
			}
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
		if(Application.isPlaying) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_volumeTween"));
			if(GUILayout.Button("Log pause blender")) {
				data.LogPauseBlender();
			}
		}
		serializedObject.ApplyModifiedProperties();
	}
}
