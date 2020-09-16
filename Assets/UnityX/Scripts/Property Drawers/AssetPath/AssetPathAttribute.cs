using UnityEngine;
using System;

public class AssetPathAttribute : PropertyAttribute {

	public Type assetType;
	public bool isResourcePath;
	public bool onlyLoadResourcePathsInEditMode;

	public AssetPathAttribute (Type assetType, bool isResourcePath, bool onlyLoadResourcePathsInEditMode) {
		this.assetType = assetType;
		this.isResourcePath = isResourcePath;
		this.onlyLoadResourcePathsInEditMode = onlyLoadResourcePathsInEditMode;
	}
}
