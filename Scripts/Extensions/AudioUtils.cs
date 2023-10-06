using System.Collections;
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


    
    public static float ComputePeakAmplitude(float[] buffer, int offset = 0, int length = -1) {
        if(length == -1) length = buffer.Length;
		// Clamp length to buffer length
		if(offset + length > buffer.Length) {
            length = buffer.Length - offset;
        }
        // Getting a peak on the last 128 samples
        float peakAmplitude = 0;
        for (int i = 0; i < length; i++) {
            // The internet has it like this, but I don't really understand why it's not abs? Perhaps because positive and negative peaks might differ?
            float wavePeak = buffer[i] * buffer[i];
            // float wavePeak = Mathf.Abs(buffer[i]);
            if (wavePeak > peakAmplitude) {
                peakAmplitude = wavePeak;
            }
        }
        // And then you need this to bring it back into linear? Argh...
        return Mathf.Sqrt(peakAmplitude);
        // return peakAmplitude;
    }

    public static float ComputeRMS(float[] buffer, int offset = 0, int length = -1) {
		if(length == -1) length = buffer.Length;
		// Clamp length to buffer length
        if(offset + length > buffer.Length) {
            length = buffer.Length - offset;
        }

        // sum of squares
        float sos = 0f;
        float val;
        for(int i = 0; i < length; i++) {
            val = buffer[ offset ];
            sos += val * val;
            offset ++;
        }
        // return sqrt of average
        return Mathf.Sqrt( sos / length );
    }
}