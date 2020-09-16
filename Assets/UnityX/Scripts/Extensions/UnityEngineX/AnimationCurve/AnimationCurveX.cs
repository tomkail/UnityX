using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extension methods for UnityEngine.AnimationCurve.
/// Note that tangents are between 0 and Mathf.PI.
/// </summary>
public static class AnimationCurveX {
	public const float DefaultTangent = 0.63694267515f;

	/// <summary>
	/// A slow start, with a fast finish.
	/// </summary>
	/// <value>The ease in.</value>
	public static AnimationCurve easeIn {
		get {
			return AnimationCurveX.EaseIn();
		}
	} 
	static AnimationCurve _easeIn;
	public static float EvaluateEaseIn (float t) {
		if(_easeIn == null) _easeIn = AnimationCurveX.EaseIn();
		return _easeIn.Evaluate(t);
	}

	/// <summary>
	/// A fast start, with a slow finish.
	/// </summary>
	/// <value>The ease in.</value>
	public static AnimationCurve easeOut {
		get {
			return AnimationCurveX.EaseOut();
		}
	}
	static AnimationCurve _easeOut;
	public static float EvaluateEaseOut (float t) {
		if(_easeOut == null) _easeOut = AnimationCurveX.EaseOut();
		return _easeOut.Evaluate(t);
	}


	public static AnimationCurve easeInOut {
		get {
			return AnimationCurveX.EaseInOut();
		}
	}
	static AnimationCurve _easeInOut;
	public static float EvaluateEaseInOut (float t) {
		if(_easeInOut == null) _easeInOut = AnimationCurveX.EaseInOut();
		return _easeInOut.Evaluate(t);
	}

	public static AnimationCurve easeInInvert {
		get {
			return AnimationCurveX.EaseInInvert();
		}
	}
	public static AnimationCurve easeOutInvert {
		get {
			return AnimationCurveX.EaseOutInvert();
		}
	}
	 
	public static AnimationCurve bellCurve {
		get {
			return AnimationCurveX.BellCurve();
		}
	}
	public static AnimationCurve sine {
		get {
			return AnimationCurveX.Sine();
		}
	}
	
	/// <summary>
	/// Gets the width of the curve by finding the difference between the times of the first and last keyframes.
	/// This function ignores the curve's wrap mode.
	/// </summary>
	/// <param name="curve">Curve.</param>
	public static float GetWidth (this AnimationCurve curve) {
		Keyframe[] keyframes = curve.keys;
		if(keyframes.Length < 2) 
			return 0;
		else 
			return keyframes[keyframes.Length-1].time - keyframes[0].time;
	}
	
	/// <summary>
	/// Gets the height of the curve by iterating through the keys in the curve and finding the smallest and largest values and calculating the difference.
	/// Can be expensive for curves with many points.
	/// </summary>
	/// <param name="curve">Curve.</param>
	public static float GetHeight (this AnimationCurve curve) {
		Keyframe[] keyframes = curve.keys;
		float smallest = curve.keys[0].value;
		float largest = curve.keys[0].value;
		for(int i = keyframes.Length-1; i >= 0; i--) {
			if(curve.keys[i].value < smallest) {
				smallest = curve.keys[i].value;
			}
			if(curve.keys[i].value > largest) {
				largest = curve.keys[i].value;
			}
		}
		return largest - smallest;
	}
	
	/// <summary>
	/// Reverse the specified curve.
	/// Behaves a little unexpectedly in the rare case when tangents are infinity. Couldn't find a nice fix.
	/// </summary>
	/// <param name="curve">Curve.</param>
	public static AnimationCurve Reverse(AnimationCurve curve){
		Keyframe[] keys = curve.keys;
		float width = keys.Last().time;
		float startTime = keys.First().time;
		for(int i = 0; i < keys.Length; i++) {
			keys[i].time = startTime + (width - keys[i].time);
			keys[i].inTangent = -keys[i].inTangent;
			keys[i].outTangent = -keys[i].outTangent;
		}
		return new AnimationCurve(keys);
	}
	
	/// <summary>
	/// Offsets the time of all the keys in the curve.
	/// </summary>
	/// <returns>The time.</returns>
	/// <param name="curve">Curve.</param>
	/// <param name="timeOffset">Time offset.</param>
	public static AnimationCurve OffsetTime(AnimationCurve curve, float timeOffset){
		Keyframe[] keys = curve.keys;
		for(int i = 0; i < keys.Length; i++) {
			keys[i].time = keys[i].time + timeOffset;
		}
		return new AnimationCurve(keys);
	}
	
	/// <summary>
	/// Removes the keys on the curve after a time.
	/// </summary>
	/// <param name="curve">Curve.</param>
	/// <param name="time">Time.</param>
	public static void RemoveKeysAfter (this AnimationCurve curve, float time) {
		Keyframe[] keyframes = curve.keys;
		for(int i = keyframes.Length-1; i >= 0; i--) {
			if(keyframes[i].time > time) {
				curve.RemoveKey(i);
			} else if (keyframes[i].time < time) {
				break;
			}
		}
	}
	
	/// <summary>
	/// Removes the keys on the curve after and including a time.
	/// </summary>
	/// <param name="curve">Curve.</param>
	/// <param name="time">Time.</param>
	public static void RemoveKeysAfterAndIncluding (this AnimationCurve curve, float time) {
		Keyframe[] keyframes = curve.keys;
		for(int i = keyframes.Length-1; i >= 0; i--) {
			if(keyframes[i].time >= time) {
				curve.RemoveKey(i);
			} else if (keyframes[i].time < time) {
				break;
			}
		}
	}
	
	/// <summary>
	/// Removes the keys on the curve before a time.
	/// </summary>
	/// <param name="curve">Curve.</param>
	/// <param name="time">Time.</param>
	public static void RemoveKeysBefore (this AnimationCurve curve, float time) {
		Keyframe[] keyframes = curve.keys;
		int i;
		for(i = 0; i < keyframes.Length; i++) {
			if (keyframes[i].time >= time) {
				break;
			}
		}
		for(i = i-1; i >= 0; i--) {
			curve.RemoveKey(i);
		}
	}
	
	/// <summary>
	/// Removes the keys on the curve before and including a time.
	/// </summary>
	/// <param name="curve">Curve.</param>
	/// <param name="time">Time.</param>
	public static void RemoveKeysBeforeAndIncluding (this AnimationCurve curve, float time) {
		Keyframe[] keyframes = curve.keys;
		int i;
		for(i = 0; i < keyframes.Length; i++) {
			if (keyframes[i].time > time) {
				break;
			}
		}
		for(i = i-1; i >= 0; i--) {
			curve.RemoveKey(i);
		}
	}
	
	/// <summary>
	/// Removes the keys on the curve between startTime and endTime.
	/// </summary>
	/// <param name="curve">Curve.</param>
	/// <param name="startTime">Start time.</param>
	/// <param name="endTime">End time.</param>
	public static void RemoveKeysBetween (this AnimationCurve curve, float startTime, float endTime) {
		Keyframe[] keyframes = curve.keys;
		for(int i = keyframes.Length-1; i >= 0; i--) {
			if(keyframes[i].time.IsBetween(startTime, endTime)) {
				curve.RemoveKey(i);
			} else if (keyframes[i].time < startTime) {
				break;
			}
		}
	}
	
	/// <summary>
	/// Removes the keys on the curve between startTime and endTime.
	/// </summary>
	/// <param name="curve">Curve.</param>
	/// <param name="startTime">Start time.</param>
	/// <param name="endTime">End time.</param>
	public static void RemoveKeysBetweenAndIncluding (this AnimationCurve curve, float startTime, float endTime) {
		Keyframe[] keyframes = curve.keys;
		for(int i = keyframes.Length-1; i >= 0; i--) {
			if(keyframes[i].time.IsBetween(startTime, endTime)) {
				curve.RemoveKey(i);
			} else if (keyframes[i].time < startTime) {
				break;
			}
		}
	}
	
	/// <summary>
	/// Returns the value of the first key
	/// </summary>
	public static float GetFirstValue(this AnimationCurve curve){
		var len = curve.length;
		if( len > 0 ) return curve[0].value;
		return 0;
	}

	/// <summary>
	/// Returns the value of the last key
	/// </summary>
	public static float GetLastValue(this AnimationCurve curve){
		var len = curve.length;
		if( len > 0 ) return curve[curve.length-1].value;
		return 0;
	}

	/// <summary>
	/// Returns the value of the first key
	/// </summary>
	public static float GetFirstTime(this AnimationCurve curve){
		var len = curve.length;
		if( len > 0 ) return curve[0].time;
		return 0;
	}

	/// <summary>
	/// Returns the value of the last key
	/// </summary>
	public static float GetLastTime(this AnimationCurve curve){
		var len = curve.length;
		if( len > 0 ) return curve[curve.length-1].time;
		return 0;
	}
	
	/// <summary>
	/// Creates a new Animation Curve with a single point
	/// </summary>
	public static AnimationCurve Point(float value = 1f){
		return new AnimationCurve(new Keyframe[]{new Keyframe(0, value)});
	}

	public static AnimationCurve Point(float time, float value){
		return new AnimationCurve(new Keyframe[]{new Keyframe(time, value)});
	}

	/// <summary>
	/// Creates a new Animation Curve with a linear curve
	/// </summary>
	public static AnimationCurve Straight(float startTime = 0f, float endTime = 1f, float value = 1f, int points = 2) {
		Keyframe[] keys = new Keyframe[points];
		float normalizedInterval = 1f/(points-1);
		for (int i = 0; i < points; i++) {
			keys[i] = new Keyframe(Mathf.Lerp(startTime, endTime, i * normalizedInterval), value);
		}
		return new AnimationCurve(keys);
	}
	
	
	/// <summary>
	/// Creates a new Animation Curve with a sine curve of given width and height
	/// </summary>
	public static AnimationCurve Sine(float width = 1f, float height = 1f){
		height = (1/height)/0.0412275f;
		Keyframe[] ks = new Keyframe[3];
		ks[0] = new Keyframe(0, 0);
		ks[0].inTangent = 0;
		ks[0].outTangent = width/height;
		ks[1] = new Keyframe(width * 0.5f, 0);
		ks[1].inTangent = (1f/height)/(1f/-width);
		ks[1].outTangent = (1f/height)/(1f/-width);
		ks[2] = new Keyframe(width, 0);
		ks[2].inTangent = width/height;
		ks[2].outTangent = 0;
		return new AnimationCurve(ks);
	}
	
	
	
	/// <summary>
	/// Creates a new Animation Curve with a bell curve of given width and height
	/// </summary>
	public static AnimationCurve BellCurve(float width = 1f, float height = 1f, float normalizedCenter = 0.5f){
		//return new AnimationCurve(new Keyframe(0, 0), new Keyframe(width * 0.5f, height), new Keyframe(width, 0));
		Keyframe[] ks = new Keyframe[3];
		ks[0] = new Keyframe(0, 0);
		ks[0].inTangent = ks[0].outTangent = 0;
		ks[1] = new Keyframe(width * normalizedCenter, height);
		//ks[1].inTangent = ks[1].outTangent = 0;
		ks[2] = new Keyframe(width, 0);
		ks[2].inTangent = ks[1].outTangent = 0;

		return new AnimationCurve(ks);
	}
	
	

	/// <summary>
	/// Creates a new Animation Curve that eases out of given width and height
	/// </summary>
	public static AnimationCurve EaseOut(float outTangent = DefaultTangent){
		return AnimationCurveX.EaseOut(0,1,0,1, outTangent);
	}
	
	/// <summary>
	/// Creates a new Animation Curve that eases out of given width and height
	/// </summary>
	public static AnimationCurve EaseOut(float width, float height, float outTangent = DefaultTangent){
		return AnimationCurveX.EaseOut(0,width,0,height, outTangent);
	}

	/// <summary>
	/// Creates a new Animation Curve that eases out between given start and end times and heights
	/// </summary>
	public static AnimationCurve EaseOut(float startTime, float endTime, float startHeight, float endHeight, float outTangent = DefaultTangent){
		Keyframe[] ks = new Keyframe[2];
		ks[0] = new Keyframe(startTime, startHeight);
		ks[0].inTangent = 0;
		ks[0].outTangent = (endHeight-startHeight)/(endTime-startTime) * outTangent * Mathf.PI;
		ks[1] = new Keyframe(endTime, endHeight);
		ks[1].inTangent = ks[1].outTangent = 0;
		return new AnimationCurve(ks);
	}

	/// <summary>
	/// Creates a new Animation Curve that eases in between 0 and 1.
	/// </summary>
	public static AnimationCurve EaseIn(float inTangent = DefaultTangent){
		return AnimationCurveX.EaseIn(0,1,0,1,inTangent);
	}
	
	/// <summary>
	/// Creates a new Animation Curve that eases in of given width and height.
	/// </summary>
	public static AnimationCurve EaseIn(float width, float height, float inTangent = DefaultTangent){
		return AnimationCurveX.EaseIn(0,width,0,height);
	}

	/// <summary>
	/// Creates a new Animation Curve that eases in between given start and end times and heights
	/// </summary>
	public static AnimationCurve EaseIn(float startTime, float endTime, float startHeight, float endHeight, float inTangent = DefaultTangent){
		Keyframe[] ks = new Keyframe[2];
		ks[0] = new Keyframe(startTime, startHeight);
		ks[0].inTangent = ks[0].outTangent = 0;
		ks[1] = new Keyframe(endTime, endHeight);
		ks[1].inTangent = (endHeight-startHeight)/(endTime-startTime) * inTangent * Mathf.PI;
		ks[1].outTangent = 0;
		return new AnimationCurve(ks);
	}

	
	/// <summary>
	/// Creates a new Animation Curve that eases in and out of given width and height
	/// </summary>
	public static AnimationCurve EaseInOut(){
		return AnimationCurveX.EaseInOut(0,1,0,1);
	}
	
	/// <summary>
	/// Creates a new Animation Curve that eases in and out of given width and height
	/// </summary>
	public static AnimationCurve EaseInOut(float width, float height){
		return AnimationCurveX.EaseInOut(0,width,0,height);
	}

	/// <summary>
	/// Creates a new Animation Curve that eases in and out between given start and end times and heights
	/// </summary>
	public static AnimationCurve EaseInOut(float startTime, float endTime, float startHeight, float endHeight){
		Keyframe[] ks = new Keyframe[2];
		ks[0] = new Keyframe(startTime, startHeight);
		ks[0].inTangent = ks[0].outTangent = 0;
		ks[1] = new Keyframe(endTime, endHeight);
		ks[1].inTangent = ks[1].outTangent = 0;
		return new AnimationCurve(ks);
	}



	
	/// <summary>
	/// Creates a new Animation Curve that eases in of given width and height
	/// </summary>
	public static AnimationCurve EaseOutInvert(float inTangent = DefaultTangent){
		return AnimationCurveX.EaseOutInvert(0,1,1,0, inTangent);
	}
	
	
	/// <summary>
	/// Creates a new Animation Curve that eases in of given width and height
	/// </summary>
	public static AnimationCurve EaseOutInvert(float width, float height, float inTangent = DefaultTangent){
		return AnimationCurveX.EaseInInvert(0,width,height,0, inTangent);
	}

	/// <summary>
	/// Creates a new inverted Animation Curve that eases out of given width and height
	/// </summary>
	public static AnimationCurve EaseOutInvert(float startTime, float endTime, float startHeight, float endHeight, float inTangent = DefaultTangent){
		Keyframe[] ks = new Keyframe[2];
		ks[0] = new Keyframe(startTime, startHeight);
		ks[0].inTangent = ks[1].outTangent = 0;
		ks[1] = new Keyframe(endTime, endHeight);
		ks[1].outTangent = 0;
		ks[1].inTangent = -inTangent * Mathf.PI;
		return new AnimationCurve(ks);
	}

	
	/// <summary>
	/// Creates a new Animation Curve that eases in of given width and height
	/// </summary>
	public static AnimationCurve EaseInInvert(float outTangent = DefaultTangent){
		return AnimationCurveX.EaseInInvert(0,1,1,0,outTangent);
	}
	
	/// <summary>
	/// Creates a new Animation Curve that eases in of given width and height
	/// </summary>
	public static AnimationCurve EaseInInvert(float width, float height, float outTangent = DefaultTangent){
		return AnimationCurveX.EaseInInvert(0,width,height,0, outTangent);
	}

	/// <summary>
	/// Creates a new Animation Curve that eases in between given start and end times and heights
	/// </summary>
	public static AnimationCurve EaseInInvert(float startTime, float endTime, float startHeight, float endHeight, float outTangent = DefaultTangent){
		Keyframe[] ks = new Keyframe[2];
		ks[0] = new Keyframe(startTime, startHeight);
		ks[0].inTangent = 0;
		ks[0].outTangent = -outTangent * Mathf.PI;
		ks[1] = new Keyframe(endTime, endHeight);
		ks[1].outTangent = ks[1].inTangent = 0;
		return new AnimationCurve(ks);
	}

	/// <summary>
	/// Creates a new Animation Curve that eases in of given width and height
	/// </summary>
	public static AnimationCurve EaseInOutInvert(float width = 1f, float height = 1f){
		return AnimationCurveX.EaseInInvert(0,width,height,0);
	}

	/// <summary>
	/// Creates a new inverted Animation Curve that eases in and out of given width and height
	/// </summary>
	public static AnimationCurve EaseInOutInvert(float startTime, float endTime, float startHeight, float endHeight){
		Keyframe[] ks = new Keyframe[2];
		ks[0] = new Keyframe(startTime, startHeight);
		ks[0].inTangent = ks[1].outTangent = 0;
		ks[1] = new Keyframe(endTime, endHeight);
		ks[1].outTangent = ks[1].inTangent = 0;
		return new AnimationCurve(ks);
	}
	
	
	
	
	/// <summary>
	/// Makes an Animation Curve linear.
	/// </summary>
	public static AnimationCurve SetLinear(this AnimationCurve curve) { 
		AnimationCurve curve3 = new AnimationCurve(); 
		for (int count_key = 0; count_key < curve.keys.Length; ++count_key) { 
			float intangent = 0; 
			float outtangent = 0; 
			bool intangent_set = false; 
			bool outtangent_set = false; 
			Vector2 point1; 
			Vector2 point2; 
			Vector2 deltapoint; 
			Keyframe key = curve[count_key]; 
			 
			if (count_key == 0){
				intangent = 0;
				intangent_set = true;
			} 
			if (count_key == curve.keys.Length -1){
				outtangent = 0;
				outtangent_set = true;
			} 
			 
			if (!intangent_set) { 
				point1.x = curve.keys[count_key-1].time; 
				point1.y = curve.keys[count_key-1].value; 
				point2.x = curve.keys[count_key].time; 
				point2.y = curve.keys[count_key].value; 
					 
				deltapoint = point2-point1; 
				 
				intangent = deltapoint.y/deltapoint.x; 
			} 
			if (!outtangent_set) { 
				point1.x = curve.keys[count_key].time; 
				point1.y = curve.keys[count_key].value; 
				point2.x = curve.keys[count_key+1].time; 
				point2.y = curve.keys[count_key+1].value; 
					 
				deltapoint = point2-point1; 
					 
				outtangent = deltapoint.y/deltapoint.x; 
			}

			key.inTangent = intangent; 
			key.outTangent = outtangent; 
			curve3.AddKey(key); 
		}
		return curve3; 
	}

	/// <summary>
	/// Gets the curve time for value. Will only work for curves with one definite time for each value
	/// From http://stackoverflow.com/questions/25527855/animationcurve-evaluate-get-time-by-value/36795100#36795100
	/// </summary>
	/// <returns>The curve time for value.</returns>
	/// <param name="curveToCheck">Curve to check.</param>
	/// <param name="value">Value.</param>
	/// <param name="accuracy">Accuracy.</param>
	public static float ApproximateTimeForValue(this AnimationCurve curveToCheck, float value, int accuracy = 10) {

	    float startTime = curveToCheck.keys [0].time;
	    float endTime = curveToCheck.keys [curveToCheck.length - 1].time;
	    float nearestTime = startTime;
	    float step = endTime - startTime;

	    for (int i = 0; i < accuracy; i++) {

	        float valueAtNearestTime = curveToCheck.Evaluate (nearestTime);
	        float distanceToValueAtNearestTime = Mathf.Abs (value - valueAtNearestTime);

	        float timeToCompare = nearestTime + step;
	        float valueAtTimeToCompare = curveToCheck.Evaluate (timeToCompare);
	        float distanceToValueAtTimeToCompare = Mathf.Abs (value - valueAtTimeToCompare);

	        if (distanceToValueAtTimeToCompare < distanceToValueAtNearestTime) {
	            nearestTime = timeToCompare;
	            valueAtNearestTime = valueAtTimeToCompare;
	        }
	        step = Mathf.Abs(step * 0.5f) * Mathf.Sign(value-valueAtNearestTime);
	    }

	    return nearestTime;
	}

	/// <summary>
	/// Evaluates the curve as if it was mirrored from time 0 by using the absolute value of the time, optionally signing the output.
	/// </summary>
	/// <returns>The mirrored.</returns>
	/// <param name="curve">Curve.</param>
	/// <param name="time">Time.</param>
	/// <param name="sign">If set to <c>true</c> multiply by the sign of the time.</param>
	public static float EvaluateMirrored (this AnimationCurve curve, float time, bool sign = true) {
		return curve.Evaluate(Mathf.Abs(time)) * (sign ? Mathf.Sign(time) : 1);
	}
}