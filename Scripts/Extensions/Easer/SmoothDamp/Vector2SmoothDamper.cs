using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Vector2SmoothDamper : SmoothDamper<Vector2> {
	protected Vector2SmoothDamper () : base () {}
	public Vector2SmoothDamper (Vector2 value) : base(value) {}
	public Vector2SmoothDamper (Vector2 current, float smoothTime) : base(current, smoothTime) {}
	public Vector2SmoothDamper (Vector2 target, Vector2 current, float smoothTime) : base(target, current, smoothTime) {}

	protected override Vector2 SmoothDamp (float deltaTime) {
		if(deltaTime == 0) return current;
		return Vector2.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	}

	protected override Vector2 GetDelta (Vector2 lastValue, Vector2 newValue) {
		return newValue - lastValue;
	}
}