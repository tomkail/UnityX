using System;
using UnityEngine;

/// <summary>
/// Base class for a curve set.
/// </summary>
[Serializable]
public abstract class BaseCurve<T> {
	public abstract void AddKey(float time, T t);
	public abstract T Evaluate(float time);
	public abstract T EvaluateAtIndex(int index);
	public abstract void SetPreWrapMode (WrapMode _postWrapMode);
	public abstract void SetPostWrapMode (WrapMode _postWrapMode);
	public abstract void Clear();
	public abstract void RemoveKeysBefore(float time);
	public abstract void RemoveKeysBeforeAndIncluding(float time);
	public abstract void RemoveKeysAfter(float time);
	public abstract void RemoveKeysAfterAndIncluding(float time);
	public abstract void RemoveKeysBetween(float startTime, float endTime);
	public abstract float GetLength();
	public abstract float GetWidth();
	public abstract float GetHeight();
}
