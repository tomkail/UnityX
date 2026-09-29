using System;
using UnityEngine;
/// <summary>
/// Used to pan to a position in the editor
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PositionHandleAttribute : PropertyAttribute  {
	public bool showName;
	public PositionHandleAttribute (bool showName = true) {
		this.showName = showName;
	}
}