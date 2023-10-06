using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Property curve for recording and accessing data of type T at a specified time.
/// Acts like a Unity AnimationCurve, but with the ability to use any type.
/// Could be used to record arbitrary data for a time rewinding system.
/// </summary>
[System.Serializable]
public abstract class PropertyCurve<T> {
	
	/// <summary>
	/// The keys, sorted by time.
	/// </summary>
	public List<PropertyCurveKeyframe<T>> keys;
	
	/// <summary>
	/// Gets the number of keys in the curve.
	/// </summary>
	/// <value>The length.</value>
	public int length {
		get {
			return keys.Count;
		}
	}
	
	/// <summary>
	/// Gets or sets the <see cref="PropertyCurve`1"/> with the specified i.
	/// </summary>
	/// <param name="i">The index.</param>
	public PropertyCurveKeyframe<T> this[int i] {
		get { 
			return keys[i]; 
		} set { 
			keys[i] = value; 
		}
	}
	
	/// <summary>
	/// Gets the times as a separate array.
	/// </summary>
	/// <value>The times.</value>
	public float[] times {
		get { 
			float[] _times = new float[length];
			for(int i = 0; i < keys.Count; i++)
				_times[i] = keys[i].time;
			return _times;
		}
	}
	
	/// <summary>
	/// Gets the values as a separate array.
	/// </summary>
	/// <value>The values.</value>
	public T[] values {
		get { 
			T[] _values = new T[length];
			for(int i = 0; i < keys.Count; i++)
				_values[i] = keys[i].value;
			return _values;
		}
	}
	
	public WrapMode postWrapMode = WrapMode.Clamp;

	public PropertyCurve(PropertyCurve<T> curve) {
		this.keys = new List<PropertyCurveKeyframe<T>>(curve.keys);
	}

	public PropertyCurve(params PropertyCurveKeyframe<T>[] keys) {
		// Done this way to avoid an AOT error on certain platforms
		this.keys = new List<PropertyCurveKeyframe<T>>();
		foreach (var key in keys) this.keys.Add(key);
	}
	
	/// <summary>
	/// Adds a key at a specified time.
	/// </summary>
	/// <param name="time">Time.</param>
	/// <param name="_value">_value.</param>
	public void AddKey(float time, T _value){
		AddKey(new PropertyCurveKeyframe<T>(time, _value));
	}
	
	/// <summary>
	/// Adds a key at a specified time.
	/// </summary>
	/// <param name="time">Time.</param>
	/// <param name="_value">_value.</param>
	public void AddKey(PropertyCurveKeyframe<T> keyframe){
		if(keys.Count == 0 || keys[keys.Count-1].time < keyframe.time){
			keys.Add(keyframe);
			return;
		} else {
			int closestIndex = ClosestIndexToTime(keyframe.time);
			if(keys[closestIndex].time == keyframe.time) {
				keys[closestIndex] = keyframe;
			} else if(keys[closestIndex].time > keyframe.time && closestIndex > 0) {
				closestIndex -= 1;
			}
			keys.Insert(closestIndex, keyframe);
		}
	}
	
	/// <summary>
	/// Removes the key at i.
	/// </summary>
	/// <param name="i">The index.</param>
	public void RemoveKey(int i){
		keys.RemoveAt(i);
	}
	
	/// <summary>
	/// Evaluates the curve at the specified time.
	/// </summary>
	/// <param name="time">Time.</param>
	public virtual T Evaluate(float time){
	
		if(keys.Count == 0){
			Debug.Log("No keys in this type exist!");
			return default (T);
		} else if(keys.Count == 1){
			//Ensures there are two values to lerp if smoothing further on
			return keys[0].value;
		}
		
		if(postWrapMode == WrapMode.Loop) {
			time %= keys.Last().time;
		}
		
		int closestIndex = ClosestIndexToTime(time);
		if(keys[closestIndex].time > time) {
			closestIndex -= 1;
			if(closestIndex == -1) {
				return keys[0].value;
			}
		}
		if(closestIndex == keys.Count-1) {
			return keys[closestIndex].value;
		}
		
		return GetSmoothedValue(keys[closestIndex].value, keys[closestIndex+1].value, Mathf.InverseLerp(keys[closestIndex].time, keys[closestIndex+1].time, time));
	}
	
	/// <summary>
	/// Sets the post wrap mode.
	/// </summary>
	/// <param name="_postWrapMode">_post wrap mode.</param>
	public virtual void SetPostWrapMode (WrapMode _postWrapMode) {
		postWrapMode = _postWrapMode;
	}
	
	/// <summary>
	/// Clears the curve.
	/// </summary>
	public void Clear() {
		keys.Clear ();
	}
	
	/// <summary>
	/// Removes the keys after and including a specified time.
	/// </summary>
	/// <param name="time">Time.</param>
	public void RemoveKeysAfter (float time) {
		for(int i = keys.Count-1; i >= 0; i--) {
			if(keys[i].time >= time) {
				keys.RemoveAt (i);
			} else if (keys[i].time < time) {
				break;
			}
		}
	}
	
	/// <summary>
	/// Removes the keys before and including a specified time.
	/// </summary>
	/// <param name="time">Time.</param>
	public void RemoveKeysBefore (float time) {
		for(int i = 0; i < keys.Count; i++) {
			if(keys[i].time <= time) {
				keys.RemoveAt (i);
			} else if (keys[i].time > time) {
				break;
			}
		}
	}
	
	/// <summary>
	/// Removes the keys between and including startTime and endTime.
	/// </summary>
	/// <param name="startTime">Start time.</param>
	/// <param name="endTime">End time.</param>
	public void RemoveKeysBetween (float startTime, float endTime) {
		for(int i = keys.Count-1; i >= 0; i--) {
			if(keys[i].time.IsBetween(startTime, endTime)) {
				keys.RemoveAt (i);
			} else if (keys[i].time < startTime) {
				break;
			}
		}
	}
	
	/// <summary>
	/// Gets a value between two keys at a normalized time.
	/// </summary>
	/// <returns>The smoothed value.</returns>
	/// <param name="key1">Key1.</param>
	/// <param name="key2">Key2.</param>
	/// <param name="time">Time.</param>
	protected abstract T GetSmoothedValue(T key1, T key2, float normalizedTime);
	
	/// <summary>
	/// Finds the index of the closest key to a time.
	/// </summary>
	/// <returns>The index to time.</returns>
	/// <param name="target">Target.</param>
	private int ClosestIndexToTime(float target) {
		int low = 0;
		int high = keys.Count - 1;
		
		if (high < 0)
			Debug.LogError("The array cannot be empty");
		while (low <= high) {
			int mid = (int)((uint)(low + high) >> 1);
			
			float midVal = keys[mid].time;
			
			if (midVal < target)
				low = mid + 1;
			else if (midVal > target)
				high = mid - 1;
			else
				return mid; // key found
		}
		if(high < 0) {
			high = 0;
		}
		return high;
	}
}