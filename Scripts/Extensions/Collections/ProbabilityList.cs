using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProbabilityList<T> : IEnumerable<KeyValuePair<T, float>> {
	private List<T> values = new List<T>();
	private List<float> probabilities = new List<float>();

	public ProbabilityList () {}
	public ProbabilityList (IList<T> values, IList<float> probabilities) {
		Debug.Assert(values.Count == probabilities.Count);
		for(int i = 0; i < values.Count; i++) {
			Add(values[i], probabilities[i]);
		}
	}
	public ProbabilityList (params KeyValuePair<T, float>[] itemProbabilitySets) {
		foreach(var item in itemProbabilitySets) {
			Add(item.Key, item.Value);
		}
	}

	public void Clear () {
		values.Clear();
		probabilities.Clear();
	}

	public void Add (T item, float probability) {
		values.Add(item);
		probabilities.Add(probability);
	}

	public bool Remove (T item) {
		int index = values.IndexOf(item);
		if(index == -1) return false;
		else {
			values.RemoveAt(index);
			probabilities.RemoveAt(index);
			return true;
		}
	}

	public T GetRandom () {
		int index = RandomX.WeightedIndex(probabilities.ToArray());
		return values[index];
	}

	public T GetBest () {
		int index = probabilities.BestIndex(x => x, ((other, currentBest) => currentBest > other));
		return values[index];
	}

	/// <summary>
	/// Gets the enumerator.
	/// </summary>
	/// <returns>The enumerator.</returns>
	IEnumerator<KeyValuePair<T, float>> IEnumerable<KeyValuePair<T, float>>.GetEnumerator() {
		for (int i = 0; i < values.Count; i++) {
			yield return new KeyValuePair<T, float>(values[i], probabilities[i]);
		}
    }

	/// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>The enumerator.</returns>
    IEnumerator IEnumerable.GetEnumerator() {
		for (int i = 0; i < values.Count; i++) {
			yield return null;
	    }
    }

	/// <summary>
	/// Array operator.
	/// </summary>
	public T this[int key] {
		get {
			return values[key];
		} set {
			values[key] = value;
		}
	}
}