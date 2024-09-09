using System.Collections.Generic;
using UnityEngine;

public static class AudioSourceX {
	public static void PlayClipAtPoint2D (AudioClip clip, float volume = 1, float pitch = 1, float pan = 0) {
		var go = new GameObject("One shot audio 2D");
		var source = go.AddComponent<AudioSource>();
		source.clip = clip;
		source.volume = volume;
		source.pitch = pitch;
		source.panStereo = pan;
		source.spatialBlend = 0;
		source.Play();
		Object.Destroy(go, clip.length);
	}
}

public static class AudioClipX {
    public static AudioClip CloneAudio (AudioClip audioClip) {
        var copiedAudioClip = AudioClip.Create("Copy", audioClip.samples, audioClip.channels, audioClip.frequency, false);
        var samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);
        copiedAudioClip.SetData(samples, 0);
        return copiedAudioClip;
    }
    
    public static AudioClip ReduceAudioQuality(AudioClip clip, int targetSampleRate, int targetBitDepth, bool forceToMono = false) {
	    float[] data = new float[clip.samples * clip.channels];
	    clip.GetData(data, 0);

	    // Assuming clip.frequency is the original sample rate.
	    float resampleRatio = clip.frequency / (float)targetSampleRate;
    
	    // Calculate new sample count for the target sample rate.
	    int newSampleCount = Mathf.CeilToInt(clip.samples / resampleRatio);
    
	    // Adjust for the new number of channels if forcing mono.
	    int newChannels = forceToMono ? 1 : clip.channels;
    
	    // Create a data array to hold the downsampled audio data.
	    float[] newData = new float[newSampleCount * newChannels];

	    // Downsampling loop.
	    for (int i = 0; i < newSampleCount; i++)
	    {
		    int oldSampleIndex = Mathf.FloorToInt(i * resampleRatio) * clip.channels;

		    if (forceToMono)
		    {
			    float monoSample = 0f;
			    for (int ch = 0; ch < clip.channels; ch++)
			    {
				    monoSample += clip.samples > oldSampleIndex + ch ? data[oldSampleIndex + ch] : 0f;
			    }
			    newData[i] = monoSample / clip.channels;
		    }
		    else
		    {
			    for (int ch = 0; ch < newChannels; ch++)
			    {
				    newData[i * newChannels + ch] = clip.samples > oldSampleIndex + ch ? data[oldSampleIndex + ch] : 0f;
			    }
		    }
	    }


	    // Bit depth reduction
	    if (targetBitDepth is > 0 and < 32) {
		    float maxVal = Mathf.Pow(2, targetBitDepth) - 1;
		    float scaleFactor = maxVal / 2;

		    for (int i = 0; i < newSampleCount; i++)
			    newData[i] = Mathf.Round(newData[i] * scaleFactor) / scaleFactor;
	    }

	    AudioClip newClip = AudioClip.Create("ReducedQualityClip", newSampleCount, newChannels, targetSampleRate, false);
	    newClip.SetData(newData, 0);

	    return newClip;
    }

    public static AudioClip TrimSilence(AudioClip clip, float min, float fadeLength = 0.15f) {
		var samples = new float[clip.samples];
		clip.GetData(samples, 0);
        var trimmedData = TrimSilence(samples, clip.frequency, min, fadeLength);
		var trimmedClip = AudioClip.Create(clip.name +" (Trimmed)", trimmedData.Length, clip.channels, clip.frequency, false);
		trimmedClip.SetData(trimmedData, 0);
        return trimmedClip;
	}

    // TODO: Use a baseline volume, rather than just guessing background noise!!!
	public static float[] TrimSilence(float[] samplesToClone, float frequency, float min, float fadeLength) {
		var samples = new List<float>(samplesToClone);

		// float minVolume = 1;
		// float maxVolume = 0;
		// for (int i=0; i<samples.Count; i++) {
		// 	minVolume = Mathf.Min(minVolume, samples[i]);
		// 	maxVolume = Mathf.Max(maxVolume, samples[i]);
		// }
        
		int fadeInStartSample;
		int fadeInEndSample = 0;
		for (int i=0; i<samples.Count; i++) {
			if (Mathf.Abs(samples[i]) > min) {
				fadeInEndSample = i;
                break;
			}
		}

// 		Wav files come in two varieties. The first has a range -1 to 1 and the second has the range 0 to 255. For the first 20*log10(x) corresponds to dB FS while for the second 20*log10((x-128)/255) gives dB FS.
  
// 1 Comment
// Zhuoyi Ma
// Zhuoyi Ma on 20 Sep 2020
// You forgot to divide the x by P0= 20e-6

        fadeInStartSample = fadeInEndSample-Mathf.RoundToInt(fadeLength*frequency);
		var clampedFadeInStartSample = Mathf.Max(0,fadeInStartSample);
		for (int i=clampedFadeInStartSample; i<fadeInEndSample; i++)
            samples[i] = Mathf.InverseLerp(fadeInStartSample, fadeInEndSample, i) * samples[i];
        

        // Debug.Log("Trim start at "+(float)fadeInStartSample / frequency+" "+(float)fadeInEndSample / frequency);
		samples.RemoveRange(0, clampedFadeInStartSample);

       
        
		int fadeOutStartSample = samples.Count-1;
		int fadeOutEndSample;
		for (int i=samples.Count - 1; i>0; i--) {
			if (Mathf.Abs(samples[i]) > min) {
				fadeOutStartSample = i;
                break;
			}
		}

        fadeOutEndSample = fadeOutStartSample+Mathf.RoundToInt(fadeLength*frequency);
		var clampedFadeOutEndSample = Mathf.Min(samples.Count-1,fadeOutEndSample);
		for (int i=fadeOutStartSample; i<clampedFadeOutEndSample; i++)
            samples[i] = Mathf.InverseLerp(fadeOutEndSample, fadeOutStartSample, i) * samples[i];
        

        // float endTime = (float)i / frequency;
        // Debug.Log("Trim end at "+(startTime+endTime));
		samples.RemoveRange(clampedFadeOutEndSample, samples.Count - clampedFadeOutEndSample);

        return samples.ToArray();
	}
}