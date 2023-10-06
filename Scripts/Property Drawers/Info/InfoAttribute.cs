using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InfoAttribute : PropertyAttribute {
	public string info;
	
	public InfoAttribute (string info) {
		this.info = info;
	}
}