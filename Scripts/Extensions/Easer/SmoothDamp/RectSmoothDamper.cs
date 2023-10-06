using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RectSmoothDamper : SmoothDamper<Rect> {
	protected RectSmoothDamper () : base () {}
	public RectSmoothDamper (Rect value) : base(value) {}
	public RectSmoothDamper (Rect current, float smoothTime) : base(current, smoothTime) {}
	public RectSmoothDamper (Rect target, Rect current, float smoothTime) : base(target, current, smoothTime) {}

	protected override Rect SmoothDamp (float deltaTime) {
		if(deltaTime == 0) return current;
		return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	}

	private static Rect SmoothDamp (Rect current, Rect target, ref Rect currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
		Vector2 posVel = currentVelocity.position;
		Vector2 sizeVel = currentVelocity.size;
		Rect output = new Rect(
			Vector2.SmoothDamp(current.position, target.position, ref posVel, smoothTime, maxSpeed, deltaTime),
			Vector2.SmoothDamp(current.size, target.size, ref sizeVel, smoothTime, maxSpeed, deltaTime)
		);
		currentVelocity.position = posVel;
		currentVelocity.size = sizeVel;
		return output;
	}

	protected override Rect GetDelta (Rect lastValue, Rect newValue) {
		return new Rect(newValue.x - lastValue.x, newValue.y - lastValue.y, newValue.width - lastValue.width, newValue.height - lastValue.height);
	}
}