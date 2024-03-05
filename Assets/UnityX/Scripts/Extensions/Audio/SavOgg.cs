using System.IO;
using UnityEngine;
using Utilities.Encoding.OggVorbis;

public static class SavOgg {
	
    public const string fileExtension = ".ogg";
	
    public static string SaveToTemporaryCachePath(string fileName, AudioClip clip) {
        var filePath = Path.Combine(Application.temporaryCachePath, fileName);
        if (!filePath.EndsWith(fileExtension, true, System.Globalization.CultureInfo.InvariantCulture)) filePath += fileExtension;
        if(Save(filePath, clip)) return filePath;
        else return null;
    }
	

    public static bool Save(string filePath, AudioClip clip) {
        if (!filePath.EndsWith(fileExtension, true, System.Globalization.CultureInfo.InvariantCulture)) {
            Debug.LogError("The file path does not end with the correct file extension.");
            return false;
        }
        // Make sure directory exists if user is saving to sub dir.
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllBytes(filePath, clip.EncodeToOggVorbis());
        return true;
    }
}