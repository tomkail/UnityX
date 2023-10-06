using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumFlagAttribute : PropertyAttribute {
	public EnumFlagAttribute() {}
}
