using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorSmoothDamper : SmoothDamper<Color> {
	protected ColorSmoothDamper () : base () {}
	public ColorSmoothDamper (Color value) : base(value) {}
	public ColorSmoothDamper (Color current, float smoothTime) : base(current, smoothTime) {}
	public ColorSmoothDamper (Color target, Color current, float smoothTime) : base(target, current, smoothTime) {}

	protected override Color SmoothDamp (float deltaTime) {
		if(deltaTime == 0) return current;
		return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	}

	private static Color SmoothDamp (Color current, Color target, ref Color currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
		return new Color(
			Mathf.SmoothDamp(current.r, target.r, ref currentVelocity.r, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(current.g, target.g, ref currentVelocity.g, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(current.b, target.b, ref currentVelocity.b, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(current.a, target.a, ref currentVelocity.a, smoothTime, maxSpeed, deltaTime)
		);
	}

	protected override Color GetDelta (Color lastValue, Color newValue) {
		return newValue - lastValue;
	}
}
