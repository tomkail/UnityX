using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
#endif
public class SceneAttribute : PropertyAttribute {
	public int selectedValue = 0;

	public SceneAttribute() {}
}