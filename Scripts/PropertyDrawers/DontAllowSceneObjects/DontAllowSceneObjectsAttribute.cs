using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class DontAllowSceneObjectsAttribute : PropertyAttribute {
	public DontAllowSceneObjectsAttribute () {}
}