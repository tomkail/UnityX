using System;
using UnityEngine;

/// <summary>
/// Instance of an active animation that's created by SLayout's Animate method and automatically runs via SLayoutAnimator.
/// Allows chained animations.
/// </summary>
public class AutoSLayoutAnimation : SLayoutAnimation {
    public AutoSLayoutAnimation _chainedAnim;
    public SLayout _owner;
    public SLayout owner => _owner;
	
    bool timeIsUp => time >= _maxDelay + _maxDuration;

    public AutoSLayoutAnimation() {}
    
    public AutoSLayoutAnimation(float duration, Action animAction) : base(duration, animAction) {}

    public AutoSLayoutAnimation(float duration, float delay, Action animAction) : base(duration, delay, animAction) {}

    public AutoSLayoutAnimation(float duration, EasingFunction.Function easingFunction, Action animAction) : base(duration, easingFunction, animAction) {}

    public AutoSLayoutAnimation(float duration, float delay, EasingFunction.Function easingFunction, Action animAction) : base(duration, delay, easingFunction, animAction) {}
    
    public void Update() 
    {
        // The first frame's delta time can be huge so just skip it
        if(Time.frameCount <= 1) return;
        time += owner.timeScale * Time.unscaledDeltaTime;

        if( isComplete )
            return;

        AnimateProperties(time);

        if (timeIsUp) {
            Done();
            // Allow next anim to begin
            if( _chainedAnim != null ) {
                var nextAnim = _chainedAnim;
                _chainedAnim = null;
                SLayoutAnimator.instance.StartAnimation(nextAnim);
            }
        }
    }
	
    public override void Cancel()
    {
        base.Cancel();
        _chainedAnim = null;
    }
	
    AutoSLayoutAnimation ThenAnimateInternal(float duration, float delay, AnimationCurve customCurve, Action animAction, Action nonAnimatedAction) {
        Debug.Assert(_chainedAnim == null, "This animation already has a chained animation (called via Then...()");
        // var nextAnim = new AutoSLayoutAnimation(duration, delay, customCurve, animAction, nonAnimatedAction, _owner);
        var nextAnim = new AutoSLayoutAnimation {
            _duration = duration,
            _maxDuration = duration,
            _delay = delay,
            _maxDelay = delay,
            _animAction = animAction,
            _nonAnimatedAction = nonAnimatedAction,
            _easingFunction = customCurve == null ? null : customCurve.Evaluate,
            _owner = owner,
        };
        if (isComplete) SLayoutAnimator.instance.StartAnimation(nextAnim);
        else _chainedAnim = nextAnim;
        return nextAnim;
    }
	
    AutoSLayoutAnimation ThenAnimateInternal(float duration, float delay, EasingFunction.Ease easing, Action animAction, Action nonAnimatedAction)
    {
        Debug.Assert(_chainedAnim == null, "This animation already has a chained animation (called via Then...()");
        // var nextAnim = new AutoSLayoutAnimation(duration, delay, customCurve, animAction, nonAnimatedAction, _owner);
        var nextAnim = new AutoSLayoutAnimation {
            _duration = duration,
            _maxDuration = duration,
            _delay = delay,
            _maxDelay = delay,
            _animAction = animAction,
            _nonAnimatedAction = nonAnimatedAction,
            _easingFunction = v => EasingFunction.GetEasingFunction(easing)(0,1,v),
            _owner = owner,
        };
        if (isComplete) SLayoutAnimator.instance.StartAnimation(nextAnim);
        else _chainedAnim = nextAnim;
        return nextAnim;
    }
	

    public AutoSLayoutAnimation ThenAnimate(float duration, Action animAction)
    {
        return ThenAnimate(duration, 0.0f, animAction);
    }

    public AutoSLayoutAnimation ThenAnimate(float duration, float delay, Action animAction)
    {
        return ThenAnimateInternal(duration, delay, null, animAction, null);
    }
	
    public AutoSLayoutAnimation ThenAnimate(float duration, AnimationCurve curve, Action animAction)
    {
        return ThenAnimateInternal(duration, 0.0f, curve, animAction, null);
    }

    public AutoSLayoutAnimation ThenAnimate(float duration, float delay, AnimationCurve curve, Action animAction)
    {
        return ThenAnimateInternal(duration, delay, curve, animAction, null);
    }

    public AutoSLayoutAnimation ThenAnimate(float duration, EasingFunction.Ease easing, Action animAction)
    {
        return ThenAnimateInternal(duration, 0.0f, easing, animAction, null);
    }
    public AutoSLayoutAnimation ThenAnimate(float duration, float delay, EasingFunction.Ease easing, Action animAction)
    {
        return ThenAnimateInternal(duration, delay, easing, animAction, null);
    }

    public AutoSLayoutAnimation ThenAnimateCustom(float duration, Action<float> customAnimAction)
    {
        return ThenAnimateInternal(duration, 0, null, () => SLayout.Animatable(customAnimAction), null);
    }

    public AutoSLayoutAnimation ThenAnimateCustom(float duration, float delay, Action<float> customAnimAction)
    {
        return ThenAnimateInternal(duration, delay, null, () => SLayout.Animatable(customAnimAction), null);
    }

    public AutoSLayoutAnimation Then(Action action)
    {
        return ThenAfter(0.0f, action);
    }

    public AutoSLayoutAnimation ThenAfter(float delay, Action action)
    {
        // completion action only
        return ThenAnimateInternal(0, delay, null, null, action);
    }
}