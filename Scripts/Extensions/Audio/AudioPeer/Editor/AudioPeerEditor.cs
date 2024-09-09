using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioPeer)), CanEditMultipleObjects]
public class AudioPeerEditor : BaseEditor<AudioPeer> {
    // SerializedProperty _renderTextureProperty;
	// void OnEnable() {
		// _renderTextureProperty = serializedObject.FindProperty("rt");
	// }
	public static string editorPrefsKey = "Audio Peer Editor Visualization Mode";
	static AudioPeerEditorDrawerSettings settings = new AudioPeerEditorDrawerSettings();
	public class AudioPeerEditorDrawerSettings {
		public VisualisationMode visualisationMode;
	}
	
	public enum VisualisationMode {
		// SamplesLeft,
		// SamplesRight,
		// SamplesStereo,
		AudioBand8,
		AudioBandBuffer8,
		AudioBand64,
		AudioBandBuffer64,
	}

	public override bool RequiresConstantRepaint() {
		return true;
	}

	public static void SaveSettings (AudioPeerEditorDrawerSettings settings) {
		string data = JsonUtility.ToJson(settings);
		EditorPrefs.SetString(editorPrefsKey, data);
    }

	public static void LoadSettings (AudioPeerEditorDrawerSettings settings) {
		if(!EditorPrefs.HasKey(editorPrefsKey)) return;
		string data = EditorPrefs.GetString(editorPrefsKey);
		JsonUtility.FromJsonOverwrite(data, settings);
    }

	public override bool HasPreviewGUI() {return true;}

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
		if(Event.current.type == EventType.Repaint) {
			// if(settings.visualisationMode == VisualisationMode.SamplesLeft) {
			// 	DrawSpectrum(data._audioBand);
			// }
			// if(settings.visualisationMode == VisualisationMode.SamplesRight) {
			// 	DrawSpectrum(data._audioBand);
			// }
			// if(settings.visualisationMode == VisualisationMode.SamplesStereo) {
			// 	DrawSpectrum(data);
			// }
			if(settings.visualisationMode == VisualisationMode.AudioBand8) {
				DrawSpectrum(data._audioBand);
			}
			else if(settings.visualisationMode == VisualisationMode.AudioBandBuffer8) {
				DrawSpectrum(data._audioBandBuffer);
			}
			else if(settings.visualisationMode == VisualisationMode.AudioBand64) {
				DrawSpectrum(data._audioBand64);
			}
			else if(settings.visualisationMode == VisualisationMode.AudioBandBuffer64) {
				DrawSpectrum(data._audioBandBuffer64);
			}
			
			void DrawSpectrum (float[] audioBand) {
				var reciprocal = 1f/audioBand.Length;
				for(int i = 0; i < audioBand.Length; i++) {
					var height = r.height * audioBand[i];
					EditorGUI.DrawRect(new Rect(r.x+r.width*reciprocal*i, r.yMax-height, r.width*reciprocal, r.height * audioBand[i]), Color.HSVToRGB(i / (float)audioBand.Length, 1, 1));
				}
			}
		}
    }

	public override void OnPreviewSettings() {
		LoadSettings(settings);
		settings.visualisationMode = (VisualisationMode)EditorGUILayout.EnumPopup(settings.visualisationMode, EditorStyles.toolbarDropDown, GUILayout.Width(120));
		SaveSettings(settings);
	}
}