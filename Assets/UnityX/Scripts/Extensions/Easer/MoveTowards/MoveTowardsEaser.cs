using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handy extendable wrapper for Unity's SmoothDamp functions, tucking away the several variables for each smooth damp you want to have running.
/// </summary>
[System.Serializable]
public abstract class MoveTowardsEaser<T> : BaseEaser<T> {

	private const float defaultMaxDelta = 1;

	public float maxDelta = defaultMaxDelta;

	// Used for correct Unity editor serializer initialization
	protected MoveTowardsEaser () {
		lerpFunction = MoveTowards;
		maxDelta = defaultMaxDelta;
	}

	public MoveTowardsEaser (T target, T current) {
		lerpFunction = MoveTowards;
		this.target = target;
		this.current = current;
	}

	public MoveTowardsEaser (T target, T current, float maxDelta) {
		lerpFunction = MoveTowards;
		this.target = target;
		this.current = current;
		this.maxDelta = maxDelta;
	}
	
	protected abstract T MoveTowards (float deltaTime);
}
