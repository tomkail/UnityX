using UnityEngine;

[System.Serializable]
public class FloatMoveTowardsEaser {
	private const float defaultMaxDelta = 1;


	
	[SerializeField, Tooltip("The target value")]
	private float _target;
	public float target {
		get {
			return _target;
		} set {
			if(_target.Equals(value)) return;
			_target = value;
			if(OnChangeTarget != null) OnChangeTarget(target);
		}
	}
	[SerializeField, Tooltip("The current value")]
	private float _current;
	public float current {
		get {
			return _current;
		} set {
			if(_current.Equals(value)) return;
			_current = value;
			if(OnChangeCurrent != null) OnChangeCurrent(current);
		}
	}
	
	private float _delta;
	public float delta {
		get {
			return _delta;
		} set {
			_delta = value;
		}
	}

	public float remaining {
		get {
			return GetDelta(target, current);
		}
	}
	
	public float maxDelta = defaultMaxDelta;

	public System.Func<float, float> lerpFunction;

	public event System.Action<float> OnChangeTarget;
	public event System.Action<float> OnChangeCurrent;


	FloatMoveTowardsEaser () {
		lerpFunction = MoveTowards;
		maxDelta = defaultMaxDelta;
	}

	public FloatMoveTowardsEaser (float value) : this(value, value) {}
	
	public FloatMoveTowardsEaser (float target, float current) {
		lerpFunction = MoveTowards;
		this.target = target;
		this.current = current;
	}

	public FloatMoveTowardsEaser (float target, float current, float maxDelta) {
		lerpFunction = MoveTowards;
		this.target = target;
		this.current = current;
		this.maxDelta = maxDelta;
	}


	public float Update () {
		return Update(Time.deltaTime);
	}

	public float Update (float deltaTime) {
		var last = current;
		current = lerpFunction(deltaTime);
		delta = GetDelta(last, current);
		return current;
	}

	public void Reset (float newDefaultValue) {
		target = current = newDefaultValue;
	}

	// Forces OnChangeCurrent event.
	public void ForceOnChangeEvent () {
		if(OnChangeCurrent != null) OnChangeCurrent(current);
	}


	float MoveTowards (float deltaTime) {
		return Mathf.MoveTowards(current, target, maxDelta * deltaTime);
	}

	float GetDelta (float lastValue, float newValue) {
		return newValue - lastValue;
	}
	
	public override string ToString () {
		return string.Format ("[BaseEaser] Current={0}, Target={1}", current, target);
	}
}