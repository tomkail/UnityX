using UnityEngine;
using System.IO;
using System.Collections;

public enum ScreenshotExportFormat {
	PNG,
	JPEG
}

/// <summary>
/// Screenshot exporter.
/// </summary>
public static class ScreenshotExporter {
	
	/// <summary>
	/// Export the specified exportSettings.
	/// </summary>
	/// <param name="exportSettings">Export settings.</param>
	public static void Export (ScreenshotExportSettings exportSettings) {
		if(exportSettings.directoryPath == string.Empty) {
			Debug.LogError("Export file path is empty!");
			return;
		}
		if(exportSettings.texture == null) {
			Debug.LogError("Texture was null!");
			return;
		}
		CheckPathExists(exportSettings.directoryPath);
		string filePath = exportSettings.filePath;
		#if UNITY_EDITOR
		if(exportSettings.openSavePrompt) {
			filePath = UnityEditor.EditorUtility.SaveFilePanel("Save texture as "+exportSettings.exportFormat.ToString(), exportSettings.directoryPath, exportSettings.fileName+"."+exportSettings.fileExtension, exportSettings.fileExtension);
		}
		#endif
		if(filePath.Length == 0) {
			Debug.LogError("Texture export path was null!");
			return;
		}
		ValidateTextureFormat(ref exportSettings.texture);
		byte[] textureByteData = exportSettings.textureByteData;
		SaveEncodedTextureDataToFile(filePath, textureByteData);
		#if UNITY_EDITOR
		if(exportSettings.importToUnity)
			UnityEditor.AssetDatabase.ImportAsset(exportSettings.filePath, UnityEditor.ImportAssetOptions.ForceUpdate);
		#endif
	}
	
	private static void CheckPathExists (string path) {
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
	}
	
	private static void SaveEncodedTextureDataToFile (string path, byte[] textureData) {
		if (textureData != null) {
			File.WriteAllBytes(path, textureData);
		} else {
			Debug.LogError("Byte array was null.");
		}
	}

	private static void ValidateTextureFormat (ref Texture2D texture) {
		if(texture.format != TextureFormat.ARGB32 && texture.format != TextureFormat.RGB24){
			Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
			newTexture.SetPixels(texture.GetPixels());
			newTexture.Apply();
			texture = newTexture;
//			MonoBehaviour.DestroyImmediate(newTexture);
		}
	}
}

/// <summary>
/// Screenshot export settings.
/// </summary>
public class ScreenshotExportSettings {
	
	public Texture2D texture;

	// The file directory
	private string _filePath;
	public string directoryPath {
		get {
			return _filePath;
		} set {
			_filePath = value;
			while(_filePath.EndsWith(Path.DirectorySeparatorChar.ToString())){
				_filePath = _filePath.Substring(0, _filePath.Length-1);
			}
		}
	}
	
	public string fileName;
	public string fileExtension {
		get {
			switch (exportFormat) {
			case ScreenshotExportFormat.PNG:
				return "png";
			case ScreenshotExportFormat.JPEG:
				return "jpeg";
			default:
				return "";
			}
		}
	}

	// The file path, relative to (including) unity's Assets folder.
	public string filePath {
		get {
			return directoryPath+"/"+fileName+"."+fileExtension;
		}
	}
	
	public byte[] textureByteData {
		get {
			switch (exportFormat) {
			case ScreenshotExportFormat.PNG:
				return texture.EncodeToPNG();
			case ScreenshotExportFormat.JPEG:
				return texture.EncodeToJPG(jpegQuality);
			default:
				return null;
			}
		}
	}
	
	public ScreenshotExportFormat exportFormat;
	public int jpegQuality = 75;
	
	#if UNITY_EDITOR
	public bool openSavePrompt;
	public bool importToUnity;
	#endif
	
	public ScreenshotExportSettings () {}
	
	public ScreenshotExportSettings (Texture2D texture, string filePath, string fileName, ScreenshotExportFormat exportFormat) {
		this.texture = texture;
		this.directoryPath = filePath;
		this.fileName = fileName;
		this.exportFormat = exportFormat;
	}
	
	public ScreenshotExportSettings (Texture2D texture, string filePath, string fileName, ScreenshotExportFormat exportFormat, int jpegQuality) {
		this.texture = texture;
		this.directoryPath = filePath;
		this.fileName = fileName;
		this.exportFormat = exportFormat;
		this.jpegQuality = jpegQuality;
	}
	
	#if UNITY_EDITOR
	public ScreenshotExportSettings (Texture2D texture, string filePath, string fileName, ScreenshotExportFormat exportFormat, bool openSavePrompt) {
		this.texture = texture;
		this.directoryPath = filePath;
		this.fileName = fileName;
		this.exportFormat = exportFormat;
		this.openSavePrompt = openSavePrompt;
		this.importToUnity = filePath.StartsWith("Assets");
	}
	
	public ScreenshotExportSettings (Texture2D texture, string filePath, string fileName, ScreenshotExportFormat exportFormat, int jpegQuality, bool openSavePrompt) {
		this.texture = texture;
		this.directoryPath = filePath;
		this.fileName = fileName;
		this.exportFormat = exportFormat;
		this.jpegQuality = jpegQuality;
		this.openSavePrompt = openSavePrompt;
		this.importToUnity = filePath.StartsWith("Assets");
	}
	#endif
}