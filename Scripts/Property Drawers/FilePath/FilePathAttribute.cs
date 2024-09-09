using UnityEngine;

public class FilePathAttribute : PropertyAttribute {

	/// <summary>
	/// If the path is relative to the assets folder. This is the format used by AssetDatabase.Load().
	/// Assets/MyFolder/MyFolder2
	/// </summary>
	public RelativeTo relativeTo = RelativeTo.Root;
	
	public enum RelativeTo {
		Root, // /tom/desktop/project/assets/scripts
		Project, // /assets/scripts
		Assets, // scripts
		Resources, // relative to any resources folder
		PersistentDataPath, // (on OSX) /Users/user/Library/Application Support/Company/Product
	}
	public bool showPrevNextFileControls;
	
	public FilePathAttribute () {}
	
	public FilePathAttribute (RelativeTo relativeTo){
		this.relativeTo = relativeTo;
	}
	public FilePathAttribute (RelativeTo relativeTo, bool allowScrolling){
		this.relativeTo = relativeTo;
		showPrevNextFileControls = allowScrolling;
	}
}
