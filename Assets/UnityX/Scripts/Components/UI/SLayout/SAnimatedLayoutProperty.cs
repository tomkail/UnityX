using System;
using System.Collections.Generic;

/// <summary>
/// Base abstract class for the animation settings for a single property (e.g. X coordinate, width, or color)
/// for a single property. When an SLayoutProperty is animated, it receives an instances of this class.
/// </summary>
public abstract class SAnimatedProperty
{
	public float delay;
	public float duration;

	public abstract void Remove();
	public abstract void Animate(float lerpValue);
}

public abstract class SAnimatedLayoutProperty : SAnimatedProperty {
	public abstract SLayout GetLayout();
	public abstract void GetStart();
	public abstract void PerformAndSetEnd();
	public abstract void ResetToStart();
}

public class SAnimatedLayoutProperty<T> : SAnimatedLayoutProperty
{
	public SLayoutProperty<T> property;
	public SLayoutAnimation animation;

	public T start;
	public T end;
	
	public override void GetStart() {
		start = property.getter();
	}
	public override void PerformAndSetEnd() {
		Animate(1);
		end = property.getter();
	}
	public override void ResetToStart() {
		property.setter(start);
	}

	public override SLayout GetLayout() => property.layout;

	public override void Remove()
	{
		property.animatedProperty = null;
		property = null;
		animation = null;
		start = default(T);
		end = default(T);
		_reusePool.Push(this);
	}

	public override void Animate(float lerpValue) {
		if (!property.isValid()) {
			// We might consider also removing it from the animation too? If the animation is valid we don't need to query this all the time.
			return;
		}
		property.setter(property.Lerp(start, end, lerpValue));
	}

	public static SAnimatedLayoutProperty<T> Create(float duration, float delay, SLayoutProperty<T> layoutProperty, SLayoutAnimation anim)
	{
		SAnimatedLayoutProperty<T> animProperty = null;

		if( _reusePool.Count > 0 ) 
			animProperty = _reusePool.Pop();
		else
			animProperty = new SAnimatedLayoutProperty<T>();

		animProperty.duration = duration;
		animProperty.delay = delay;

		animProperty.animation = anim;

		// Link and back-link
		animProperty.property = layoutProperty;
		layoutProperty.animatedProperty = animProperty;

		return animProperty;
	}

	static Stack<SAnimatedLayoutProperty<T>> _reusePool = new Stack<SAnimatedLayoutProperty<T>>();
}


public class SAnimatedCustomProperty : SAnimatedProperty
{
	public SAnimatedCustomProperty(Action<float> customAnim, float duration, float delay) {
		this._customAnim = customAnim;
		this.duration = duration;
		this.delay = delay;
	}

	public override void Remove() {}
	public override void Animate(float lerpValue)
	{
		_customAnim(lerpValue);
	}

	Action<float> _customAnim;
}