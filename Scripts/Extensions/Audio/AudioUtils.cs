using System.Collections.Generic;
using UnityEngine;

public static class AudioUtils {
    // I'll confess I don't really know why this is 20, but it seems to be a standard, and it sounds right.
    public const float c = 20.0f;

	/// <summary>
	/// Converts percieved linear volume in the range 0 to 1 to DB volume in the range -80 to 0.
	/// </summary>
	/// <returns>DB volume.</returns>
	/// <param name="linearVolume">Linear volume.</param>
	public static float LinearVolumeToDBVolume (float linearVolume) {
        return Mathf.Clamp(c * Mathf.Log10(linearVolume), -80.0f, 0.0f);
	}

    /// <summary>
	/// Converts DB volume in the range -80 to 0 to percieved linear volume in the range 0 to 1.
	/// </summary>
	/// <returns>Linear volume.</returns>
	/// <param name="dbVolume">DB volume.</param>
	public static float DBVolumeToLinearVolume (float dbVolume) {
        return Mathf.Pow(10.0f, dbVolume / c);
	}


    
    public static float ComputePeakAmplitude(IList<float> buffer, int startSample = 0, int length = -1, bool repeatAroundBuffer = false) {
        if(buffer.IsNullOrEmpty()) return 0;
	    // Ensure valid input and clamp length to buffer length
        if(length < 0 || length > buffer.Count) length = buffer.Count;
        startSample %= buffer.Count;
		if (startSample < 0) startSample += buffer.Count;
		if(!repeatAroundBuffer && startSample + length > buffer.Count) length = buffer.Count - startSample;
        
		float peakAmplitude = 0;
		int endSample = startSample + length;
        for (int i = startSample; i < endSample; i++) {
            float wavePeak = Mathf.Abs(buffer[i % buffer.Count]);
            if (wavePeak > peakAmplitude) {
                peakAmplitude = wavePeak;
            }
        }
        return peakAmplitude;
    }

    public static float ComputeRMS(IList<float> buffer, int startSample = 0, int length = -1, bool repeatAroundBuffer = false) {
	    if(buffer.IsNullOrEmpty()) return 0;
	    // Ensure valid input and clamp length to buffer length
	    if(length < 0 || length > buffer.Count) length = buffer.Count;
	    startSample %= buffer.Count;
	    if (startSample < 0) startSample += buffer.Count;
	    if(!repeatAroundBuffer && startSample + length > buffer.Count) length = buffer.Count - startSample;

        // sum of squares
        float sos = 0f;
        int endSample = startSample + length;
        for(int i = startSample; i < endSample; i++) {
	        var val = buffer[i % buffer.Count];
	        sos += val * val;
        }
        // return sqrt of average
        return Mathf.Sqrt( sos / length );
    }
}