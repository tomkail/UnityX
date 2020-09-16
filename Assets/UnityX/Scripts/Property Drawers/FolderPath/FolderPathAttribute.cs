using UnityEngine;
using System.Collections;

public class FolderPathAttribute : PropertyAttribute {
	
	/// <summary>
	/// If the path is relative to the assets folder. This is the format used by AssetDatabase.Load().
	/// Assets/MyFolder/MyFolder2
	/// </summary>
	public RelativeTo relativeTo = RelativeTo.Root;
	
	public enum RelativeTo {
		Root, // /tom/desktop/project/assets/scripts
		Project, // /assets/scripts
		Assets, // scripts
		Desktop,
		PersistentDataPath
	}

	public FolderPathAttribute () {}
	
	public FolderPathAttribute (RelativeTo relativeTo){
		this.relativeTo = relativeTo;
	}
}
