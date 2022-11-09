//	Copyright (c) 2012 Calvin Rien
//        http://the.darktable.com
//
//	This software is provided 'as-is', without any express or implied warranty. In
//	no event will the authors be held liable for any damages arising from the use
//	of this software.
//
//	Permission is granted to anyone to use this software for any purpose,
//	including commercial applications, and to alter it and redistribute it freely,
//	subject to the following restrictions:
//
//	1. The origin of this software must not be misrepresented; you must not claim
//	that you wrote the original software. If you use this software in a product,
//	an acknowledgment in the product documentation would be appreciated but is not
//	required.
//
//	2. Altered source versions must be plainly marked as such, and must not be
//	misrepresented as being the original software.
//
//	3. This notice may not be removed or altered from any source distribution.
//
//  =============================================================================
//
//  derived from Gregorio Zanon's script
//  http://forum.unity3d.com/threads/119295-Writing-AudioListener.GetOutputData-to-wav-problem?p=806734&viewfull=1#post806734

using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SavWav {

	public const string fileExtention = ".wav";
	const int HEADER_SIZE = 44;

    #if UNITY_EDITOR
    public static AudioClip SaveAssetWithPrompt (AudioClip clip, string defaultPath = "Assets") {
		if (clip == null) return null;
		var path = EditorUtility.SaveFilePanelInProject("Save AudioClip as WAV", clip.name+fileExtention, fileExtention.Substring(1), "Enter a file name for the AudioClip.", defaultPath);
		if (path == "") return null;
		if (!path.EndsWith(fileExtention, true, System.Globalization.CultureInfo.InvariantCulture)) path += fileExtention;
        using (var fileStream = CreateEmpty(path)) {
			ConvertAndWrite(fileStream, clip);
			WriteHeader(fileStream, clip);
		}
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var asset = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
		EditorGUIUtility.PingObject(asset);
		return asset;
	}

	public static AudioClip SaveAsset(string filePath, AudioClip clip) {
		if (!filePath.EndsWith(fileExtention, true, System.Globalization.CultureInfo.InvariantCulture)) filePath += fileExtention;
		var absoluteFilePath = Path.Combine(Application.dataPath, filePath);
		if(Save(absoluteFilePath, clip)) {
			var relativeFilePath = "Assets/"+filePath;
			AssetDatabase.ImportAsset(relativeFilePath, ImportAssetOptions.ForceUpdate);
			var asset = AssetDatabase.LoadAssetAtPath<AudioClip>(relativeFilePath);
			EditorGUIUtility.PingObject(asset);
			return asset;
		}
		return null;
	}
    #endif

	public static string SaveToPersistentDataPath(string fileName, AudioClip clip) {
		var filePath = Path.Combine(Application.persistentDataPath, fileName);
		if (!filePath.EndsWith(fileExtention, true, System.Globalization.CultureInfo.InvariantCulture)) filePath += fileExtention;
		if(Save(filePath, clip)) return filePath;
		else return null;
	}

	public static string SaveToTemporaryCachePath(string fileName, AudioClip clip) {
		var filePath = Path.Combine(Application.temporaryCachePath, fileName);
		if (!filePath.EndsWith(fileExtention, true, System.Globalization.CultureInfo.InvariantCulture)) filePath += fileExtention;
		if(Save(filePath, clip)) return filePath;
		else return null;
	}

	public static bool Save(string filePath, AudioClip clip) {
		if (!filePath.EndsWith(fileExtention, true, System.Globalization.CultureInfo.InvariantCulture)) {
			Debug.LogError("The file path does not end with the correct file extension.");
			return false;
		}
		// Make sure directory exists if user is saving to sub dir.
		Directory.CreateDirectory(Path.GetDirectoryName(filePath));
		// Debug.Log(filePath);
		using (var fileStream = CreateEmpty(filePath)) {
			ConvertAndWrite(fileStream, clip);
			WriteHeader(fileStream, clip);
		}
		return true; // TODO: return false if there's a failure saving the file
	}

	static FileStream CreateEmpty(string filepath) {
		var fileStream = new FileStream(filepath, FileMode.Create);
	    byte emptyByte = new byte();

	    for(int i = 0; i < HEADER_SIZE; i++) //preparing the header
	    {
	        fileStream.WriteByte(emptyByte);
	    }

		return fileStream;
	}

	static void ConvertAndWrite(FileStream fileStream, AudioClip clip) {

		var samples = new float[clip.samples];

		clip.GetData(samples, 0);

		Int16[] intData = new Int16[samples.Length];
		//converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

		Byte[] bytesData = new Byte[samples.Length * 2];
		//bytesData array is twice the size of
		//dataSource array because a float converted in Int16 is 2 bytes.

		int rescaleFactor = 32767; //to convert float to Int16

		for (int i = 0; i<samples.Length; i++) {
			intData[i] = (short) (samples[i] * rescaleFactor);
			Byte[] byteArr = new Byte[2];
			byteArr = BitConverter.GetBytes(intData[i]);
			byteArr.CopyTo(bytesData, i * 2);
		}

		fileStream.Write(bytesData, 0, bytesData.Length);
	}

	static void WriteHeader(FileStream fileStream, AudioClip clip) {

		var hz = clip.frequency;
		var channels = clip.channels;
		var samples = clip.samples;

		fileStream.Seek(0, SeekOrigin.Begin);

		Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff, 0, 4);

		Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
		fileStream.Write(chunkSize, 0, 4);

		Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave, 0, 4);

		Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt, 0, 4);

		Byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1, 0, 4);

		UInt16 one = 1;

		Byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat, 0, 2);

		Byte[] numChannels = BitConverter.GetBytes(channels);
		fileStream.Write(numChannels, 0, 2);

		Byte[] sampleRate = BitConverter.GetBytes(hz);
		fileStream.Write(sampleRate, 0, 4);

		Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
		fileStream.Write(byteRate, 0, 4);

		UInt16 blockAlign = (ushort) (channels * 2);
		fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

		UInt16 bps = 16;
		Byte[] bitsPerSample = BitConverter.GetBytes(bps);
		fileStream.Write(bitsPerSample, 0, 2);

		Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
		fileStream.Write(datastring, 0, 4);

		Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
		fileStream.Write(subChunk2, 0, 4);

//		fileStream.Close();
	}
}