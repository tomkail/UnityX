using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows the speed of an animation to be sped up or slowed down independently
/// of any scalars that exist using the standard Time.timeScale. This is useful
/// since often UI is used within pause menus.
/// When this component is added to a Canvas object, any SLayouts within the
/// hierarchy beneath this object will be affected by it.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class SLayoutCanvasTimeScalar : MonoBehaviour {

	public enum TimeType {
		Scaled,
		Unscaled
	}

	/// <summary>
	/// When Scaled, the the timeScale is in addition to any Time.timeScale.
	/// When Unscaled, it acts entirely independently.
	/// </summary>
	public TimeType timeType;

	/// <summary>
	/// 1 is normal, 0 is pause, 0.5 is half speed.
	/// </summary>
	public float timeScaleMultiplier = 1;

	// The calculated time scale used by SLayout
	public float timeScale {
		get {
			if(timeType == TimeType.Scaled) return timeScaleMultiplier * Time.timeScale;
			else return timeScaleMultiplier;
		}
	}
}