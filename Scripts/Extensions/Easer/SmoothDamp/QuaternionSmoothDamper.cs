using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class QuaternionSmoothDamper : SmoothDamper<Quaternion> {
	protected QuaternionSmoothDamper () : base () {}
	public QuaternionSmoothDamper (Quaternion value) : base(value) {}
	public QuaternionSmoothDamper (Quaternion current, float smoothTime) : base(current, smoothTime) {}
	public QuaternionSmoothDamper (Quaternion target, Quaternion current, float smoothTime) : base(target, current, smoothTime) {}

	protected override Quaternion SmoothDamp (float deltaTime) {
		if(deltaTime == 0) return current;
		return QuaternionX.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	}

	protected override Quaternion GetDelta (Quaternion lastValue, Quaternion newValue) {
		return QuaternionX.Difference(newValue, lastValue);
	}
}