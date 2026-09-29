using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class LockAttribute : PropertyAttribute {
	public bool locked = true;
}