using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Vector3Curve : BaseCurve<Vector3> {
	[SerializeField]
    private AnimationCurve xCurve;
	[SerializeField]
    private AnimationCurve yCurve;
	[SerializeField]
    private AnimationCurve zCurve;
	
	public Keyframe[] keys {
		get {
			return xCurve.keys;
		}
	}

	public float length {
		get {
			return GetLength();
		}
	}
	
	public float width {
		get {
			return GetWidth();
		}
	}

	public float height {
		get {
			return GetHeight();
		}
	}
	
	public Vector3Curve() {
		xCurve = new AnimationCurve();
		yCurve = new AnimationCurve();
		zCurve = new AnimationCurve();
	}

	public Vector3Curve(AnimationCurve _xCurve, AnimationCurve _yCurve, AnimationCurve _zCurve) {
		xCurve = _xCurve;
		yCurve = _yCurve;
		zCurve = _zCurve;
	}

	public void Set(AnimationCurve _xCurve, AnimationCurve _yCurve, AnimationCurve _zCurve) {
		this.xCurve = _xCurve;
		this.yCurve = _yCurve;
		this.zCurve = _zCurve;
	}
	
	public void AddKey(float time, float x, float y, float z){
		xCurve.AddKey(time, x);
		yCurve.AddKey(time, y);
		zCurve.AddKey(time, z);

		//start = Mathf.Min(start, time);
		//end = Mathf.Min(end, time);
	}

	public override void AddKey(float time, Vector3 vector){
		AddKey(time, vector.x, vector.y, vector.z);
	}

	public void AddKeys(Keyframe xKey, Keyframe yKey, Keyframe zKey){
		xCurve.AddKey(xKey);
		yCurve.AddKey(yKey);
		zCurve.AddKey(zKey);

		// start = Mathf.Min(start, xKey.time);
		// end = Mathf.Min(end, xKey.time);
		// start = Mathf.Min(start, yKey.time);
		// end = Mathf.Min(end, yKey.time);
		// start = Mathf.Min(start, zKey.time);
		// end = Mathf.Min(end, zKey.time);
	}

	public override Vector3 Evaluate(float time){
		return new Vector3(xCurve.Evaluate(time), yCurve.Evaluate(time), zCurve.Evaluate(time));
	}

	public override Vector3 EvaluateAtIndex(int index) {
		return new Vector3(xCurve.keys[index].value, yCurve.keys[index].value, zCurve.keys[index].value);
	}

	public override void SetPreWrapMode (WrapMode _preWrapMode) {
		xCurve.preWrapMode = _preWrapMode;
		yCurve.preWrapMode = _preWrapMode;
		zCurve.preWrapMode = _preWrapMode;
	}

	public override void SetPostWrapMode (WrapMode _postWrapMode) {
		xCurve.postWrapMode = _postWrapMode;
		yCurve.postWrapMode = _postWrapMode;
		zCurve.postWrapMode = _postWrapMode;
	}

	public override void Clear(){
		xCurve = new AnimationCurve();
		yCurve = new AnimationCurve();
		zCurve = new AnimationCurve();
	}
	
	public override void RemoveKeysBefore (float time) {
		this.xCurve.RemoveKeysBefore(time);
		this.yCurve.RemoveKeysBefore(time);
		this.zCurve.RemoveKeysBefore(time);
	}
	
	public override void RemoveKeysBeforeAndIncluding (float time) {
		this.xCurve.RemoveKeysBeforeAndIncluding(time);
		this.yCurve.RemoveKeysBeforeAndIncluding(time);
		this.zCurve.RemoveKeysBeforeAndIncluding(time);
	}
	
	public override void RemoveKeysAfter (float time) {
		this.xCurve.RemoveKeysAfter(time);
		this.yCurve.RemoveKeysAfter(time);
		this.zCurve.RemoveKeysAfter(time);
	}
	
	public override void RemoveKeysAfterAndIncluding (float time) {
		this.xCurve.RemoveKeysAfterAndIncluding(time);
		this.yCurve.RemoveKeysAfterAndIncluding(time);
		this.zCurve.RemoveKeysAfterAndIncluding(time);
	}
	
	public override void RemoveKeysBetween (float startTime, float endTime) {
		this.xCurve.RemoveKeysBetween(startTime, endTime);
		this.yCurve.RemoveKeysBetween(startTime, endTime);
		this.zCurve.RemoveKeysBetween(startTime, endTime);
	}

	public override float GetLength () {
		return Mathf.Max(xCurve.length, yCurve.length, zCurve.length);
	}
	
	public override float GetWidth () {
		return Mathf.Max(xCurve.GetWidth(), yCurve.GetWidth(), zCurve.GetWidth());
	}
	
	public override float GetHeight () {
		return Mathf.Max(xCurve.GetHeight(), yCurve.GetHeight(), zCurve.GetHeight());
	}

	public void SetLinear() { 
		xCurve = xCurve.SetLinear();
		yCurve = yCurve.SetLinear();
		zCurve = zCurve.SetLinear();
	}
	
	/// <summary>
	/// Estimates the time where the value is closest to the target value.
	/// Useful for estimating time along a path for non-path users.
	/// </summary>
	/// <returns>The time where the value is closest to the target value.</returns>
	public float EstimateClosestTimeToValue (Vector3 vector) {
		Debug.Log ("TODO");
		return 0;
	}
	
	/// <summary>
	/// Factory that creates a Vector3Curve from a list of positions, using the distance between positions as the distance between keyframes.
	/// </summary>
	/// <param name="positions">Positions.</param>
	public static Vector3Curve Create (IList<Vector3> positions) {
		Vector3Curve positionCurve = new Vector3Curve();
		float distance = 0;
		for(int i = 0; i < positions.Count; i++) {
			if(i > 0) {
				distance += Vector3.Distance(positions[i-1], positions[i]);
			}
			positionCurve.AddKey(distance, positions[i]);
		}
		return positionCurve;
	}
}