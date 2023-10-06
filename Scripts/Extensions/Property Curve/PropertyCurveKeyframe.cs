using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generic keyframe for a PropertyCurve.
/// </summary>
[System.Serializable]
public struct PropertyCurveKeyframe<T> {
	
	/// <summary>
	/// Gets or sets the time.
	/// </summary>
	/// <value>The time.</value>
	public float time;
	
	/// <summary>
	/// Gets or sets the value.
	/// </summary>
	/// <value>The value.</value>
	public T value;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyCurveKeyframe`1"/> struct.
	/// </summary>
	/// <param name="time">Time.</param>
	/// <param name="value">Value.</param>
	public PropertyCurveKeyframe (float _time, T _value) {
		time = _time;
		value = _value;
	}
}
