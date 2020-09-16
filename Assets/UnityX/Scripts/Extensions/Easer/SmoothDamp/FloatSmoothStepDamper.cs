using UnityEngine;
/// <summary>
/// Eases smoothly using an easing curve and an elasticity when the target changes.
/// </summary>
[System.Serializable]
public class FloatSmoothStepDamper {
	
    [SerializeField, Tooltip("The target value")]
	private float _target;
	public float target {
		get {
			return _target;
		} set {
			if(float.IsNaN(value)) return;
			if(_target.Equals(value)) return;
			_target = value;
			if(OnChangeTarget != null) OnChangeTarget(target);
		}
	}

    
    [SerializeField, Tooltip("The current value")]
	private float _currentEasingPosition;
	public float currentEasingPosition {
		get {
			return _currentEasingPosition;
		} private set {
			if(_currentEasingPosition.Equals(value)) return;
			_currentEasingPosition = value;
		}
	}

	[SerializeField, Tooltip("The eased value"), Disable]
	private float _current;
	public float current {
		get {
			return _current;
		} private set {
			if(_current.Equals(value)) return;
			_current = value;
			if(OnChangeCurrent != null) OnChangeCurrent(_current);
		}
	}

	public System.Func<float, float> lerpFunction;

	public event System.Action<float> OnChangeTarget;
	public event System.Action<float> OnChangeCurrent;
	
    
    public float smoothSpeed = 1;
    // Decreasing this makes changing direction faster
    public float elasticitySmoothTime = 0.15f;
	
    float currentVelocity;
    float velocityDampVelocity;
    // EaseInOut
	static AnimationCurve defaultEasing {
        get {
            Keyframe[] ks = new Keyframe[2];
            ks[0] = new Keyframe(0, 0);
            ks[0].inTangent = ks[0].outTangent = 0;
            ks[1] = new Keyframe(1, 1);
            ks[1].inTangent = ks[1].outTangent = 0;
            return new AnimationCurve(ks);
        }
    }
	AnimationCurve easing = defaultEasing;
    
    
	protected FloatSmoothStepDamper () {
		lerpFunction = SmoothDamp;

	}
	public FloatSmoothStepDamper (float current) {
		lerpFunction = SmoothDamp;
		// this.initial = 
        this.current = this.target = current;
	}

	public FloatSmoothStepDamper (float current, float smoothSpeed) {
		lerpFunction = SmoothDamp;
		// this.initial = 
        this.current = this.target = current;
        this.smoothSpeed = smoothSpeed;
    }

	public FloatSmoothStepDamper (float target, float current, float smoothSpeed) {
		lerpFunction = SmoothDamp;
		this.target = target;
		this.current = current;
        this.smoothSpeed = smoothSpeed;
	}

	protected float SmoothDamp (float deltaTime) {
        var targetVelocity = 0;
		if(current == target) {
			targetVelocity = 0;
		} else {
			targetVelocity = (target-current).Sign();
		}
		if(deltaTime > 0) {
			currentVelocity = Mathf.SmoothDamp(currentVelocity, targetVelocity, ref velocityDampVelocity, elasticitySmoothTime, Mathf.Infinity, deltaTime);
			currentEasingPosition += currentVelocity * smoothSpeed * deltaTime;
			currentEasingPosition = Mathf.Clamp01(currentEasingPosition);
			current = easing.Evaluate(currentEasingPosition);
		}
		return current; 
	}

	public virtual float Update () {
		return Update(Time.deltaTime);
	}

	public virtual float Update (float deltaTime) {
		var last = current;
		current = lerpFunction(deltaTime);
		return current;
	}

	public virtual void Reset (float newDefaultValue) {
		target = currentEasingPosition = newDefaultValue;
		velocityDampVelocity = 0;
		current = easing.Evaluate(currentEasingPosition);
	}

	// Forces OnChangeCurrent event.
	public void ForceOnChangeEvent () {
		if(OnChangeCurrent != null) OnChangeCurrent(current);
	}

	public override string ToString () {
		return string.Format ("[BaseEaser] Current={0}, Target={1}", current, target);
	}
}