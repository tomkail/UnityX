using System;
using UnityEngine;

/// <summary>
/// Wraps an accessor to a particular property on the SLayout (e.g. x, width, color, etc) that can be 
/// animated, so that the animation can detect when properties are changed, and cause those properties to be animated.
/// </summary>
public abstract class SLayoutProperty {
	// The reference to the layout is used to sort animatable properties and to validate animations.
	public SLayout layout;
	public enum GetMode {
		Current,
		AnimStart,
		AnimEnd,
	}
	
	public enum SetMode {
		// If within an animation block then set up an animation (or tweak the existing one). Else set the value.
		Auto,
		// Cancel any animations and set the property immediately
		Immediate,
		// If an animation exists, sets its start value. Else run Auto.
		AnimStart,
		// If an animation exists, sets its end value. Else run Auto.
		AnimEnd,
	}
}

public abstract class SLayoutProperty<T> : SLayoutProperty {
	public Func<T> getter;
	public Action<T> setter;
	public Func<bool> isValid;

	// Setting a value cancels any animations on this property.
	// If this is called within an animation block, then the animation will be set up to animate this property.
	public T value {
		get => getter();
		set {
			var currentAnim = SLayoutAnimation.AnimationUnderDefinition();
			// If we're in an animation block, then set up an animation (or tweak the existing one)
			if( currentAnim != null )
				currentAnim.SetupPropertyAnim(this, value);
			// If not, then just run the setter.
			else {
				// If this property is being animated, then remove it from the animation so that it doesn't override the new value.
				// Note that currentAnim.SetupPropertyAnim does this too.
				animatedProperty?.animation.RemovePropertyAnim(animatedProperty);
				setter(value);
			}
		}
	}

	
	public T GetProperty(GetMode getMode) {
		return getMode switch {
			GetMode.Current => value,
			GetMode.AnimStart => animatedProperty == null ? value : animatedProperty.start,
			GetMode.AnimEnd => animatedProperty == null ? value : animatedProperty.end,
			_ => value
		};
	}
	public void SetProperty(T newValue, SetMode setMode) {
		var animationUnderDefinition = SLayoutAnimation.AnimationUnderDefinition();
		if (setMode == SetMode.Auto) {
			BeginDefinedAnimationOrSetImmediate();
		} else if (setMode == SetMode.Immediate) {
			CancelAnimationAndSetImmediate();
		} else if (setMode == SetMode.AnimStart) {
			if (animatedProperty != null) animatedProperty.start = newValue;
			else BeginDefinedAnimationOrSetImmediate();
		} else if (setMode == SetMode.AnimEnd) {
			if (animatedProperty != null) animatedProperty.end = newValue;
			else BeginDefinedAnimationOrSetImmediate();
		}

		void BeginDefinedAnimationOrSetImmediate() {
			// If we're in an animation block, then set up an animation (or tweak the existing one)
			if (animationUnderDefinition != null)
				animationUnderDefinition.SetupPropertyAnim(this, newValue);
			// If not, then just run the setter.
			else CancelAnimationAndSetImmediate();
		}

		void CancelAnimationAndSetImmediate() {
			// If this property is being animated, then remove it from the animation so that it doesn't override the new value.
			// Note that currentAnim.SetupPropertyAnim does this too.
			animatedProperty?.animation.RemovePropertyAnim(animatedProperty);
			setter(newValue);
		}
	}

	public abstract T Lerp(T v0, T v1, float t);
		
	// When this property is being animated, it receives an instance
	// of a SAnimatedProperty, which contains the start value,
	// target value, the delay and the duration.
	public SAnimatedLayoutProperty<T> animatedProperty;
}

public class SLayoutFloatProperty : SLayoutProperty<float> {
	public override float Lerp(float v0, float v1, float t) => Mathf.LerpUnclamped(v0, v1, t);
}

public class SLayoutAngleProperty : SLayoutProperty<float> {
	public override float Lerp(float v0, float v1, float t) => Mathf.LerpAngle(v0, v1, t);
}

public class SLayoutColorProperty : SLayoutProperty<Color> {
	public override Color Lerp(Color v0, Color v1, float t) => Color.Lerp(v0, v1, t);
}