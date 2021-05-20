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
}