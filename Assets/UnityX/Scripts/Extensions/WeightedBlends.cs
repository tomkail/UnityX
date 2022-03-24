using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public static class WeightedBlends {
	public static bool WeightedBlend(IEnumerable<bool> values, IList<float> weights) {
		float sum = 0;
		float total = 0;
		int i = 0;
		foreach(var value in values) {
			total += weights[i];
			if(value) sum += weights[i];
			i++;
		}
		return (sum/total) > 0.5f;
	}

	public static float WeightedBlend(IEnumerable<float> values, IList<float> weights) {
		return WeightedBlend(values, f => f, weights);
	}

	public static float WeightedBlend<T>(IEnumerable<T> values, Func<T, float> selector, IList<float> weights) {
		return WeightedBlend(
			values, selector, weights, 
			(float total, float val, float weight) => total + weight * val,
			total => total
		);
	}

	public static Vector3 WeightedBlend(IEnumerable<Vector3> values, IList<float> weights) {
		return WeightedBlend(values, v => v, weights);
	}

	public static Vector3 WeightedBlend<T>(IEnumerable<T> values, Func<T, Vector3> selector, IList<float> weights) {
		return WeightedBlend(
			values, selector, weights, 
			(Vector3 total, Vector3 val, float weight) => total + weight * val,
			total => total
		);
	}


	public static SerializableTransform WeightedBlend(IEnumerable<SerializableTransform> values, IList<float> weights) {
		return new SerializableTransform(WeightedBlend(values.Select(x => x.position), weights), WeightedBlend(values.Select(x => x.rotation), weights), Vector3.one);
	}

	public static float WeightedBlendAngle(IEnumerable<float> values, IList<float> weights) {
		return WeightedBlend(
			values, v => v, weights,
			(Vector2 totalDirection, float angle, float weight) => totalDirection + weight * WithDegrees(angle),
			totalDirection => Mathf.Atan2(-totalDirection.y, totalDirection.x) * Mathf.Rad2Deg
		);
	}

	public static float WeightedBlendAngle<T>(IEnumerable<T> values, Func<T, float> selector, IList<float> weights) {
		return WeightedBlend(
			values, selector, weights,
			(Vector2 totalDirection, float angle, float weight) => totalDirection + weight * WithDegrees(angle),
			totalDirection => Mathf.Atan2(-totalDirection.y, totalDirection.x) * Mathf.Rad2Deg
		);
	}


	struct WeightedAxes {
		public Vector3 forward;
		public Vector3 up;
	}
	public static Quaternion WeightedBlend<T>(IEnumerable<T> values, Func<T, Quaternion> selector, IList<float> weights) {
		return WeightedBlend(values, selector, weights, (WeightedAxes axes, Quaternion q, float weight) => {
			if(weight == 0) return axes;
			var forward = q * Vector3.forward;
			var up      = q * Vector3.up;
			axes.forward += weight * forward;
			axes.up      += weight * up;
			return axes;
		}, axes => {
			if(axes.forward == Vector3.zero || axes.up == Vector3.zero) return Quaternion.identity;
			return Quaternion.LookRotation(axes.forward.normalized, axes.up.normalized);
		});
	}

	public static Quaternion WeightedBlend(IEnumerable<Quaternion> values, IList<float> weights) {
		return WeightedBlend(values, q => q, weights);
	}

	static U WeightedBlend<T, U, TAccum>(IEnumerable<T> values, Func<T, U> selector, IList<float> weights, Func<TAccum, U, float, TAccum> accumFunc, Func<TAccum, U> resultFunc) {
		TAccum accumulatedTotal = default(TAccum);

		int i=0;
		foreach(T fromObj in values) {
			var weight = weights[i];
			var val = selector(fromObj);
			accumulatedTotal = accumFunc(accumulatedTotal, val, weight);
			i++;
		}

		return resultFunc(accumulatedTotal);
	}
	
	static T Identity<T>(T t) {
		return t;
	}

	static Vector2 WithDegrees(float degrees) {
		var rad = degrees * Mathf.Deg2Rad;
		return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
	}
}