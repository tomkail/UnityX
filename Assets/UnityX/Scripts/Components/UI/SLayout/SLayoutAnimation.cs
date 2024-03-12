using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Instance of an active animation. The animations currently being defined are stored
/// in a static list so that as you run the animAction code, it can detects properties that have been changed and react accordingly.
/// </summary>
public class SLayoutAnimation {

	public float duration => _duration;
	public float delay => _delay;


	public SLayoutAnimation() { }
	
	public SLayoutAnimation(float duration, Action animAction) : this() {
		_duration = _maxDuration = duration;
		_animAction = animAction;
	}
	
	public SLayoutAnimation(float duration, float delay, Action animAction) : this(duration, animAction) {
		_delay = _maxDelay = delay;
	}
	
	public SLayoutAnimation(float duration, EasingFunction.Function easingFunction, Action animAction) : this(duration, animAction) {
		_easingFunction = v => easingFunction(0, 1, v);
	}
	
	public SLayoutAnimation(float duration, float delay, EasingFunction.Function easingFunction, Action animAction) : this(duration, easingFunction, animAction) {
		_delay = _maxDelay = delay;
	}
	
	public void Start()
	{
		time = 0.0f;

		if( _animationsBeingDefined == null )
			_animationsBeingDefined = new List<SLayoutAnimation>();

		_animationsBeingDefined.Add(this);

		bool instant = _delay + _duration <= 0.0f;

		try {
			if (_animAction != null) {
				_properties = new List<SAnimatedProperty>();

				// This gathers all the properties that are being animated
				_animAction();

				// Order the properties.
				// Layout properties are sorted by transform depth so that the shallowest transforms are animated first.
				// All other properties are run afterwards.
				OrderByTransformDepth(_properties);

				static void OrderByTransformDepth(List<SAnimatedProperty> properties) {
					properties.Sort((property1, property2) => GetSortIndex(property1).CompareTo(GetSortIndex(property2)));

					static int GetSortIndex(SAnimatedProperty animatedProperty) {
						if (animatedProperty is SAnimatedLayoutProperty animatedLayoutProperty) return GetTransformDepth(animatedLayoutProperty.GetLayout().transform);
						return int.MaxValue;
					}

					static int GetTransformDepth(Transform transform) {
						if (transform.parent == null) return 0;
						else return 1 + GetTransformDepth(transform.parent);
					}
				}

				if (!instant) {
					// Layout properties are sorted against each other, so are processed as a block.
					// The order of operations ensures that the start and end values are set correctly, since performing an animation on a parent element can affect the properties of the children 
					foreach (var layoutProperty in _properties.OfType<SAnimatedLayoutProperty>()) layoutProperty.SetStartToCurrentValue();
					foreach (var layoutProperty in _properties.OfType<SAnimatedLayoutProperty>()) layoutProperty.PerformAndSetEnd();
					foreach (var layoutProperty in _properties.OfType<SAnimatedLayoutProperty>()) layoutProperty.ResetToStart();
					AnimateProperties(time);
				}
			}
		} catch (Exception e) {
			Debug.LogError(e);
			RemoveAnimFromAllProperties();
			_nonAnimatedAction = null;
			throw;
		} finally {
			_animationsBeingDefined.RemoveAt(_animationsBeingDefined.Count-1);	
		}
		
		// If it's an instant animation complete it immediately so that we don't need to process it in update.
		if (instant) CompleteImmediate();
	}
	
	public static SLayoutAnimation AnimationUnderDefinition()
	{
        if( _preventingAnim )
            return null;
		else if( _animationsBeingDefined != null && _animationsBeingDefined.Count > 0 )
			return _animationsBeingDefined[_animationsBeingDefined.Count-1];
		else
			return null;
	}
		
	// Called when setting the value of an SLayoutProperty (i.e. on an SLayout) when
	// this animation is under definition.
	public void SetupPropertyAnim<T>(SLayoutProperty<T> layoutProperty, T targetValue)
	{
		SAnimatedLayoutProperty<T> animatedProperty = layoutProperty.animatedProperty;

		// Already being animated as part of a DIFFERENT animation?
		// Cancel the animation of this property in that animation, and instead
		// animate as part of a new animation.
        // If it's already being animated in the current animation, then reuse
        // the property, but don't assign a new start value.
		if( animatedProperty != null ) {
			var existingAnim = animatedProperty.animation;
			if( existingAnim != this ) {
				existingAnim.RemovePropertyAnim(animatedProperty);
				animatedProperty = null;
			}
		}

		// Create the animated property 
		// (But only if necessary: This property may have already been set 
		// as part of this animation)
		if( animatedProperty == null ) {
			animatedProperty = SAnimatedLayoutProperty<T>.Create(_duration, _delay, layoutProperty, this);
			_properties.Add(animatedProperty);

            // Set up initial value for the animation
            // If the property has already been set up, then don't override the start point
            // in the animation. e.g. if you do:
            //
            //     l.x = -100
            //     l.Animate(0.5f, () => {
            //          l.x = 20;
            //          l.x = 100;
            //     });
            //
            // ...then animate from -100 to +100, from 20 to 100. (which would jump)
            animatedProperty.start = layoutProperty.getter();
		}

		// If animation parameter overrides exist apply them here.
		if (animParamsStack.Count > 0) {
			var animParams = animParamsStack.Peek();
			animatedProperty.duration = animParams.duration;
			animatedProperty.delay = animParams.delay;

			if (animatedProperty.duration + animatedProperty.delay-0.00001f > animatedProperty.animation._maxDuration + animatedProperty.animation._maxDelay) {
				Debug.LogWarning($"This animated property's duration {animatedProperty.duration + animatedProperty.delay} is longer than its owner animation {animatedProperty.animation._maxDuration + animatedProperty.animation._maxDelay}. This is not currently supported and the animation will not appear to complete.");
			}
		}
		
		animatedProperty.end = targetValue;
	}

	public virtual void Cancel()
	{
		RemoveAnimFromAllProperties();
		_nonAnimatedAction = null;
	}

	void RemoveAnimFromAllProperties()
	{
		if( _properties == null ) return;

		foreach(var prop in _properties)
			prop.Remove();
		
		_properties.Clear();
	}

	public void RemovePropertyAnim(SAnimatedProperty animProperty)
	{
		animProperty.Remove();

		// Could potentially make _properties a HashSet rather than List
		// to make this faster, but I think that the Remove is rare enough
		// compared to the Add that it's probably faster to keep it as a list.
		if( _properties != null )
			_properties.Remove(animProperty);
	}

	public bool isComplete => _completed;

	public void AddDelay(float extraDelay) {
		_delay += extraDelay;
		_maxDelay = Mathf.Max(_delay, _maxDelay);
	}

	public void AddDuration(float extraDuration) {
		_duration += extraDuration;
		_maxDuration = Mathf.Max(_duration, _maxDuration);
	}

	public void AddCustomAnim(Action<float> customAnim)
	{
		_properties.Add(new SAnimatedCustomProperty(customAnim, _duration, _delay));
	}

	public void AnimateProperties(float time) {
		if (_properties == null) return;
		for(int i=0; i<_properties.Count; i++) {
			var property = _properties[i];
			float lerpValue = (time-property.delay) / property.duration;
			lerpValue = _easingFunction?.Invoke(lerpValue) ?? lerpValue;
			property.Animate(lerpValue);
		}
	}

	public void CompleteImmediate()
	{
		if( _properties != null )
			foreach(var property in _properties) 
				property.Animate(1.0f);
		
		Done();
	}

	// Mark the animation as complete and call the completion action
	public void Done() 
	{
		_completed = true;

		RemoveAnimFromAllProperties();

		if( _nonAnimatedAction != null )
			_nonAnimatedAction();
	}

	// This allows setting a property within an animation definition without animating it.
	public static void StartPreventAnimation() {
		_preventingAnim = true;
	}

	public static void EndPreventAnimation() {
		_preventingAnim = false;
	}
	
	public static void WithoutAnimating(Action action) {
		
		StartPreventAnimation();
		action();
		EndPreventAnimation();
	}
	
	// This allows overriding the default animation params for a property/block of properties.
	static Stack<(float duration, float delay, Func<float, float> easingFunc)> animParamsStack = new();
	public static void StartAnimationParams(float duration, float delay) {
		// , (v) => EasingFunction.GetEasingFunction(ease)(0, 1, v)
		animParamsStack.Push((duration, delay, null));
	}

	public static void EndAnimationParams() {
		animParamsStack.Pop();
	}

	public static void WithAnimationParams(float duration, float delay, Action action) {
		StartAnimationParams(duration, delay);
		action();
		EndAnimationParams();
	}
		
	List<SAnimatedProperty> _properties;

	public float time;

	public float _duration;
	public float _delay;

	public float _maxDuration;
	public float _maxDelay;

	public delegate float EasingFunc(float t);
	public EasingFunc _easingFunction;

	public Action _animAction;
	public Action _nonAnimatedAction;
	
	bool _completed;

    static bool _preventingAnim;
	static List<SLayoutAnimation> _animationsBeingDefined;
}