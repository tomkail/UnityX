using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BaseEaser<T> {
	[SerializeField, Tooltip("The target value")]
	private T _target;
	public T target {
		get {
			return _target;
		} set {
			if(_target.Equals(value)) return;
			_target = value;
			if(OnChangeTarget != null) OnChangeTarget(target);
		}
	}
	[SerializeField, Tooltip("The current value")]
	private T _current;
	public T current {
		get {
			return _current;
		} set {
			if(_current.Equals(value)) return;
			_current = value;
			if(OnChangeCurrent != null) OnChangeCurrent(current);
		}
	}
	
	private T _delta;
	public T delta {
		get {
			return _delta;
		} protected set {
			_delta = value;
		}
	}

	public T remaining {
		get {
			return GetDelta(target, current);
		}
	}
	public System.Func<float, T> lerpFunction;

	public event System.Action<T> OnChangeTarget;
	public event System.Action<T> OnChangeCurrent;

	public virtual T Update () {
		return Update(Time.deltaTime);
	}

	public virtual T Update (float deltaTime) {
		var last = current;
		current = lerpFunction(deltaTime);
		delta = GetDelta(last, current);
		return current;
	}

	public virtual void Reset (T newDefaultValue) {
		target = current = newDefaultValue;
	}

	// Forces OnChangeCurrent event.
	public void ForceOnChangeEvent () {
		if(OnChangeCurrent != null) OnChangeCurrent(current);
	}

	public override string ToString () {
		return string.Format ("[BaseEaser] Current={0}, Target={1}", current, target);
	}

	protected abstract T GetDelta (T lastValue, T newValue);
}
