using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
#endif
public class ScenePathAttribute : PropertyAttribute {
	public SceneFindMethod findMethod;
	public enum SceneFindMethod {
    	EnabledInBuild,
    	AllInBuild,
    	AllInProject
    }

    public bool useFullPath;
	public ScenePathAttribute(SceneFindMethod findMethod = SceneFindMethod.EnabledInBuild, bool useFullPath = true)
    {
		this.findMethod = findMethod;
		this.useFullPath = useFullPath;
    }
}