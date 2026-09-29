using System;
using UnityEngine;
/// <summary>
/// Used to pan to a position in the editor
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PositionLookAtAttribute : PropertyAttribute  {
	public PositionLookAtAttribute () {}
}