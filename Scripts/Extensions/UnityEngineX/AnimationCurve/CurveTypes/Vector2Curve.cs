using UnityEngine;

[System.Serializable]
public class Vector2Curve : BaseCurve<Vector2> {
	[SerializeField]
    public AnimationCurve xCurve;
	[SerializeField]
    public AnimationCurve yCurve;

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

	public Vector2Curve() {
		xCurve = new AnimationCurve();
		yCurve = new AnimationCurve();
	}

	public Vector2Curve(AnimationCurve _xCurve, AnimationCurve _yCurve) {
		xCurve = _xCurve;
		yCurve = _yCurve;
	}
	
	public void Set(AnimationCurve _xCurve, AnimationCurve _yCurve) {
		this.xCurve = _xCurve;
		this.yCurve = _yCurve;
	}
	
	public void AddKey(float time, float x, float y){
		AddKey(time, x, y);
	}

	public override void AddKey(float time, Vector2 vector){
		xCurve.AddKey(time, vector.x);
		yCurve.AddKey(time, vector.y);
	}
	
	public void AddKey(float time, Vector2 vector, float inTangent, float outTangent){
		xCurve.AddKey(new Keyframe(time, vector.x, inTangent, outTangent));
		yCurve.AddKey(new Keyframe(time, vector.y, inTangent, outTangent));
	}
	
	public void AddKeys(Keyframe xKey, Keyframe yKey){
		xCurve.AddKey(xKey);
		yCurve.AddKey(yKey);
	}
	
	public override Vector2 Evaluate(float distance){
		return new Vector2(xCurve.Evaluate(distance), yCurve.Evaluate(distance));
	}

	public override Vector2 EvaluateAtIndex(int index) {
		return new Vector2(xCurve.keys[index].value, yCurve.keys[index].value);
	}

	public override void SetPreWrapMode (WrapMode _preWrapMode) {
		xCurve.preWrapMode = _preWrapMode;
		yCurve.preWrapMode = _preWrapMode;
	}

	public override void SetPostWrapMode (WrapMode _postWrapMode) {
		xCurve.postWrapMode = _postWrapMode;
		yCurve.postWrapMode = _postWrapMode;
	}

	public override void Clear(){
		xCurve = new AnimationCurve();
		yCurve = new AnimationCurve();
	}
	
	public override void RemoveKeysBefore (float time) {
		this.xCurve.RemoveKeysBefore(time);
		this.yCurve.RemoveKeysBefore(time);
	}
	
	public override void RemoveKeysBeforeAndIncluding (float time) {
		this.xCurve.RemoveKeysBeforeAndIncluding(time);
		this.yCurve.RemoveKeysBeforeAndIncluding(time);
	}
	
	public override void RemoveKeysAfter (float time) {
		this.xCurve.RemoveKeysAfter(time);
		this.yCurve.RemoveKeysAfter(time);
	}
	
	public override void RemoveKeysAfterAndIncluding (float time) {
		this.xCurve.RemoveKeysAfterAndIncluding(time);
		this.yCurve.RemoveKeysAfterAndIncluding(time);
	}
	
	public override void RemoveKeysBetween (float startTime, float endTime) {
		this.xCurve.RemoveKeysBetween(startTime, endTime);
		this.yCurve.RemoveKeysBetween(startTime, endTime);
	}

	public override float GetLength () {
		return Mathf.Max(xCurve.length, yCurve.length);
	}
	
	public override float GetWidth () {
		return Mathf.Max(xCurve.GetWidth(), yCurve.GetWidth());
	}
	
	public override float GetHeight () {
		return Mathf.Max(xCurve.GetHeight(), yCurve.GetHeight());
	}
}