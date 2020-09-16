using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class RandomX {
	public static Stack<UnityEngine.Random.State> seeds = new Stack<UnityEngine.Random.State>();
	public static void BeginSeed (int seed) {
		seeds.Push(UnityEngine.Random.state);
        UnityEngine.Random.InitState(seed);
	}

	public static void EndSeed () {
		UnityEngine.Random.state = seeds.Pop();	
	}

	/// <summary>
	/// Returns random true/false
	/// </summary>
	public static bool boolean {
		get { return (UnityEngine.Random.value > 0.5f); }
	}

	public static int sign {
		get { return boolean ? -1 : 1; }
	}
	
	/// <summary>
	/// Returns a random Vector2 of a circle edge 
	/// </summary>
	public static Vector2 onUnitCircle {
		get {
			float angle = RandomX.eulerAngle;
			return MathX.DegreesToVector2(angle);
		}
	}

	/// <summary>
	/// Returns a random float between 0 and 360
	/// </summary>
	public static float eulerAngle {
		get {
			return UnityEngine.Random.value*360;
		}
	}
	
	/// <summary>
	/// Returns a boolean from a chance value between 0 and 1, where 0.25 is unlikely and 0.75 is likely.
	/// </summary>
	/// <param name="chance">Chance.</param>
	public static bool Chance(float chance) {
		return chance > UnityEngine.Random.value;
	}

	public static T WeightedValue<T>(IList<T> values, Func<T, float> getWeight, float? optionalTotal = null, T defaultVal = default(T)) {
		float total = 0;
		// TODO - getWeight can be called twice, which is inefficient. If we get weight of each value for totalling, cache them and use them in the next step
		if(optionalTotal == null) {
			foreach(var value in values) {
				total += getWeight(value);
			} 
		} else total = (float)optionalTotal;

		if(total <= 0) return defaultVal;
		float currentValue = 0;
		float randomValue = UnityEngine.Random.Range(0f, total);
		for(int i = 0; i < values.Count; i++) {
			var value = values[i];
			currentValue += getWeight(value);
			if(currentValue >= randomValue) return value;
		}
		Debug.LogError("Could not find a value. Total was: "+total+" and num values was "+values.Count);
		return defaultVal;
	}

	/// <summary>
	/// Returns a random index, using the value of each array item as a weight
	/// </summary>
	/// <returns>The random index.</returns>
	/// <param name="weights">Weights.</param>
	public static int WeightedIndex(IList<float> weights){
		return RandomX.WeightedIndex(weights, weights.Sum());
	}
	
	/// <summary>
	/// Returns a random index, using the value of each array item as a weight
	/// </summary>
	/// <returns>The random index.</returns>
	/// <param name="weights">Weights.</param>
	/// <param name="total">Total.</param>
	public static int WeightedIndex(IList<float> weights, float total){
		if(weights.IsNullOrEmpty()) {
			Debug.LogError("Weights array is null or empty");
			return -1;
		}
		if(total == 0) return weights.RandomIndex();
		float currentValue = 0;
		float randomValue = UnityEngine.Random.Range(0f, total);
		for(int i = 0; i < weights.Count; i++){
			currentValue += weights[i];
			if(currentValue >= randomValue) return i;
		}
		Debug.LogError("Could not find a value. Total was: "+total+" and num values was "+weights.Count);
		return -1;
	}
}
