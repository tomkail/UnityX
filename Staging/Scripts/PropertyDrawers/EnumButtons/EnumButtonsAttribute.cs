using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumButtonsAttribute : PropertyAttribute {
	public EnumButtonsAttribute () {}
}