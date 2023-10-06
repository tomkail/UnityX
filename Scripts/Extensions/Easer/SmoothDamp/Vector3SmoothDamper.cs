using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Vector3SmoothDamper : SmoothDamper<Vector3> {
	protected Vector3SmoothDamper () : base () {}
	public Vector3SmoothDamper (Vector3 value) : base(value) {}
	public Vector3SmoothDamper (Vector3 target, float smoothTime) : base(target, smoothTime) {}
	public Vector3SmoothDamper (Vector3 target, Vector3 current, float smoothTime) : base(target, current, smoothTime) {}

	protected override Vector3 SmoothDamp (float deltaTime) {
		if(deltaTime == 0) return current;
		return Vector3.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	}

	protected override Vector3 GetDelta (Vector3 lastValue, Vector3 newValue) {
		return newValue - lastValue;
	}
}
