using System;
using UnityEngine;
#if UNITY_EDITOR
#endif

[AttributeUsage(AttributeTargets.Field)]
public class InfoAttribute : PropertyAttribute {
	public string info;
	
	public InfoAttribute (string info) {
		this.info = info;
	}
}