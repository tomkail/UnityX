using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Instance of an active animation that's created by SLayout's Animate method. The animations currently being defined are stored
/// in a static list so that as you run the animAction code, it can detects properties that have been changed and react accordingly.
/// </summary>
public class SLayoutAnimation {

	public float duration { get { return _duration; } }
	public float delay { get { return _delay; } }

	public SLayoutAnimation () {
		time = 0.0f;
	}
	public SLayoutAnimation(float duration, float delay, EasingFunction.Function easingFunction, Action animAction, Action nonAnimatedAction, SLayout owner)
	{
		_duration = _maxDuration = duration;
		_delay = _maxDelay = delay;
		_animAction = animAction;
		_nonAnimatedAction = nonAnimatedAction;
		_easingFunction = easingFunction;
		_owner = owner;
		time = 0.0f;
	}

	public void Start()
	{
		time = 0.0f;

		if( _animationsBeingDefined == null )
			_animationsBeingDefined = new List<SLayoutAnimation>();

		_animationsBeingDefined.Add(this);

		bool instant = _delay + _duration <= 0.0f;

		if( _animAction != null ) {
			_properties = new List<SAnimatedProperty>();

			_animAction();

			// Rewind animation back to beginning
			// But only if our duration > 0
			if( !instant ) {
				foreach(var property in _properties)
					property.Start();
			}
		}

		_animationsBeingDefined.RemoveAt(_animationsBeingDefined.Count-1);	

		// Duration = zero? Done already
		if( instant ) Done();
	}

	public SLayoutAnimation Then(Action action)
	{
		return ThenAfter(0.0f, action);
	}

	public SLayoutAnimation ThenAfter(float delay, Action action)
	{
		// completion action only
		return ThenAnimateInternal(0, delay, null, null, action);
	}

	public SLayoutAnimation ThenAnimate(float duration, Action animAction)
	{
		return ThenAnimate(duration, 0.0f, animAction);
	}

	public SLayoutAnimation ThenAnimate(float duration, float delay, Action animAction)
	{
		return ThenAnimateInternal(duration, delay, null, animAction, null);
	}
	
	public SLayoutAnimation ThenAnimate(float duration, AnimationCurve curve, Action animAction)
	{
		return ThenAnimateInternal(duration, 0.0f, curve, animAction, null);
	}

	public SLayoutAnimation ThenAnimate(float duration, float delay, AnimationCurve curve, Action animAction)
	{
		return ThenAnimateInternal(duration, delay, curve, animAction, null);
	}

	public SLayoutAnimation ThenAnimate(float duration, EasingFunction.Ease easing, Action animAction)
	{
		return ThenAnimateInternal(duration, 0.0f, easing, animAction, null);
	}
	public SLayoutAnimation ThenAnimate(float duration, float delay, EasingFunction.Ease easing, Action animAction)
	{
		return ThenAnimateInternal(duration, delay, easing, animAction, null);
	}

	public SLayoutAnimation ThenAnimateCustom(float duration, Action<float> customAnimAction)
	{
        return ThenAnimateInternal(duration, 0, null, () => SLayout.Animatable(customAnimAction), null);
	}

	public SLayoutAnimation ThenAnimateCustom(float duration, float delay, Action<float> customAnimAction)
	{
        return ThenAnimateInternal(duration, delay, null, () => SLayout.Animatable(customAnimAction), null);
	}

    SLayoutAnimation ThenAnimateInternal(float duration, float delay, AnimationCurve customCurve, Action animAction, Action nonAnimatedAction) {
        Debug.Assert(_chainedAnim == null, "This animation already has a chained animation (called via Then...()");
        // var nextAnim = new SLayoutAnimation(duration, delay, customCurve, animAction, nonAnimatedAction, _owner);
		var nextAnim = new SLayoutAnimation() {
			_duration = duration,
			_maxDuration = duration,
			_delay = delay,
			_maxDelay = delay,
			_animAction = animAction,
			_nonAnimatedAction = nonAnimatedAction,
			_customCurve = customCurve,
			_owner = owner,
		};
        if (this.isComplete) SLayoutAnimator.instance.StartAnimation(nextAnim);
        else _chainedAnim = nextAnim;
        return nextAnim;
	}
	
    SLayoutAnimation ThenAnimateInternal(float duration, float delay, EasingFunction.Ease easing, Action animAction, Action nonAnimatedAction)
    {
        Debug.Assert(_chainedAnim == null, "This animation already has a chained animation (called via Then...()");
        // var nextAnim = new SLayoutAnimation(duration, delay, customCurve, animAction, nonAnimatedAction, _owner);
		var nextAnim = new SLayoutAnimation() {
			_duration = duration,
			_maxDuration = duration,
			_delay = delay,
			_maxDelay = delay,
			_animAction = animAction,
			_nonAnimatedAction = nonAnimatedAction,
			_easingFunction = EasingFunction.GetEasingFunction(easing),
			_owner = owner,
		};
        if (this.isComplete) SLayoutAnimator.instance.StartAnimation(nextAnim);
        else _chainedAnim = nextAnim;
        return nextAnim;
    }

	public bool canAnimate {
		get {
			// If owner is removed/deleted then the animation is cancelled.
			// However, even if properties have all been removed we still allow
			// the animation to remain active, since it may have a completion callback etc
			if( owner == null || owner.Equals(null) )  return false;
			return true;
		}
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
	public void SetupPropertyAnim<T>(SLayoutProperty<T> layoutProperty)
	{
		SAnimatedProperty<T> animatedProperty = layoutProperty.animatedProperty;

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
			animatedProperty = SAnimatedProperty<T>.Create(_duration, _delay, layoutProperty, this);
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
	}

	public void Cancel()
	{
		RemoveAnimFromAllProperties();
		_nonAnimatedAction = null;
		_chainedAnim = null;
	}

	void RemoveAnimFromAllProperties()
	{
		if( _properties == null ) return;

		foreach(var prop in _properties)
			prop.Remove();
		
		_properties.Clear();
	}

	void RemovePropertyAnim(SAnimatedProperty animProperty)
	{
		animProperty.Remove();

		// Could potentially make _properties a HashSet rather than List
		// to make this faster, but I think that the Remove is rare enough
		// compared to the Add that it's probably faster to keep it as a list.
		if( _properties != null )
			_properties.Remove(animProperty);
	}

	public bool isComplete {
		get {
			return _completed;
		}
	}

	public SLayout owner {
		get {
			return _owner;
		}
	}

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

	bool timeIsUp {
		get {
			return time >= _maxDelay + _maxDuration;
		}
	}
	
	public void Update() 
	{
		// The first frame's delta time can be huge so just skip it
		if(Time.frameCount <= 1) return;
		time += owner.timeScale * Time.unscaledDeltaTime;

		if( isComplete )
			return;

		if( _properties != null ) {
			// Properties may be removed during this loop
			for(int i=0; i<_properties.Count; i++) {
				var property = _properties[i];
				float lerpValue = 0.0f;
				if( time > property.delay ) {
					lerpValue =  Mathf.Clamp01((time-property.delay) / property.duration);

					// TODO: Allow different curves?
					if( _easingFunction != null ) {
						lerpValue = _easingFunction(0.0f, 1.0f, lerpValue);
					} else if( _customCurve != null ) {
						lerpValue = _customCurve.Evaluate(lerpValue);
					} else {
						lerpValue = Mathf.SmoothStep(0.0f, 1.0f, lerpValue);
					}

					property.Animate(lerpValue);
				}
			}
		}

		if( timeIsUp )
			Done();
	}

	public void CompleteImmediate()
	{
		if( _properties != null )
			foreach(var property in _properties) 
				property.Animate(1.0f);
		
		Done();
	}

    public static void StartPreventAnimation()
    {
        _preventingAnim = true;
    }

    public static void EndPreventAnimation()
    {
        _preventingAnim = false;
    }

	void Done() 
	{
		_completed = true;

		RemoveAnimFromAllProperties();

		if( _nonAnimatedAction != null )
			_nonAnimatedAction();

		// Allow next anim to begin
		if( _chainedAnim != null ) {
			var nextAnim = _chainedAnim;
			_chainedAnim = null;
			SLayoutAnimator.instance.StartAnimation(nextAnim);
		}
	}
		
	List<SAnimatedProperty> _properties;

	public float time;

	public float _duration;
	public float _delay;

	public float _maxDuration;
	public float _maxDelay;

	public EasingFunction.Function _easingFunction;
	public AnimationCurve _customCurve;

	public Action _animAction;
	public Action _nonAnimatedAction;
	public SLayoutAnimation _chainedAnim;
	bool _completed;
	public SLayout _owner;

    static bool _preventingAnim;
	static List<SLayoutAnimation> _animationsBeingDefined;
}