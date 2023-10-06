using UnityEngine;
using System;
using System.Collections.Generic;

public static class MathX {

	public static float Min(IEnumerable<float> values) {
		return values.Min(x => x);
	}

	public static float Max(IEnumerable<float> values) {
		return values.Max(x => x);
	}

	/// <summary>
	/// Can be iterated using foreach(var sign in MathX.signs)
	/// </summary>
	public static int[] signs = new int[] {-1, +1};

	// https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
	public static int Mod(this int a, int n) {
        if (n == 0) throw new ArgumentOutOfRangeException("n", "(a mod 0) is undefined.");

        //puts a in the [-n+1, n-1] range using the remainder operator
        int remainder = a%n;

        //if the remainder is less than zero, add n to put it in the [0, n-1] range if n is positive
        //if the remainder is greater than zero, add n to put it in the [n-1, 0] range if n is negative
        if ((n > 0 && remainder < 0) || (n < 0 && remainder > 0)) return remainder + n;
        return remainder;
    }

	/// <summary>
	/// Repeat the specified value around a min and max.
	/// Note that unlike Unity's Repeat function, this function returns max when val == max instead of min.
	/// </summary>
	/// <param name="val">Value.</param>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	public static int RepeatInclusive (int val, int min, int max) {
		int range = max - min;
		if (val > max) 
			val -= range * Mathf.CeilToInt((val-max) / range);
		else if (val < min) 
			val += range * Mathf.CeilToInt((-val+min) / range);
		return val;
	}
	
	/// <summary>
	/// Repeat the specified value around a min and max.
	/// Note that unlike Unity's Repeat function, this function returns max when val == max instead of min.
	/// </summary>
	/// <param name="val">Value.</param>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	public static float RepeatInclusive (float val, float min, float max) {
		float range = max - min;
		if (val > max) 
			val -= range * Mathf.CeilToInt((val-max) / range);
		else if (val < min) 
			val += range * Mathf.CeilToInt((-val+min) / range);
		return val;
	}
	
	// Repeats around a range so that output is always between a and b.
    // RepeatInRange(1,4,5) = 2;
    // RepeatInRange(1,4,0) = 3;

    // Not-inclusive of b, so:
    // RepeatInRange(1,4,4) = 1; rather than 4
    public static float RepeatInRange (float a, float b, float val) {
        if(a == b) return val;
        b-=a;
        val-=a;
        val = Mathf.Repeat(val,b);
        val+=a;
        return val;
    }

    // Calculates the shortest difference between two given angles.
    public static float SignedDeltaRepeating(float a, float b, float val, float target) {
        b-=a;
        val-=a;
        target-=a;
        float delta = Mathf.Repeat((target - val), b);
        if (delta > b*0.5f)
            delta -= b;
        return delta;
    }



	/// <summary>
	/// Returns the reciprocal of a number.
	/// </summary>
	public static float Reciprocal (this float f) {
		return 1f/f;
	}
	
	/// <summary>
	/// Returns true is the float is a whole number
	/// </summary>
	public static bool IsWhole (this float f) {
		return f.IsMultipleOf(1);
	}

	/// <summary>
	/// Returns true is the float is positive (more than or equal to zero)
	/// </summary>
	public static bool IsPositive (this float f) {
		return (f >= 0);
	}
	
	/// <summary>
	/// Determines if the float is even.
	/// </summary>
	/// <returns><c>true</c> if the specified f is even; otherwise, <c>false</c>.</returns>
	/// <param name="f">F.</param>
	public static bool IsEven (this int f) {
		return f % 2 == 0;
	}
	
	/// <summary>
	/// Determines if the float is odd.
	/// </summary>
	/// <returns><c>true</c> if the specified float is odd; otherwise, <c>false</c>.</returns>
	/// <param name="f">F.</param>
	public static bool IsOdd (this int f) {
		return f % 2 == 1;
	}
	
	
	/// <summary>
	/// Determines if the float is a multiple of val.
	/// </summary>
	/// <returns><c>true</c> if is multiple of the specified f val; otherwise, <c>false</c>.</returns>
	/// <param name="f">F.</param>
	/// <param name="val">Value.</param>
	public static bool IsMultipleOf (this float f, float val) {
		return f % val == 0;
	}
	
	

	
	/// <summary>
	/// Returns a value indicating the sign of a number.
	/// Where allowZero is true, returns 0 when f is 0.
	/// </summary>
	public static int Sign (this float f, bool allowZero = false) {
		if( allowZero && f == 0.0f ) return 0;
		return f.IsPositive() ? 1 : -1;
	}

    //COMPARISON 

    //Similar to Unity's epsilon comparison, but allows for any precision.
    public static bool NearlyEqual(float a, float b, float maxDifference = 0.001f) {
		if (a == b)  { 
			return true;
		} else {
			return MathX.Difference(a, b) < maxDifference;
	    }
	}

	/// <summary>
	/// Determines if f is more than a, and less than to b.
	/// </summary>
	/// <returns><c>true</c> if f is more than a, and less than to b.; otherwise, <c>false</c>.</returns>
	/// <param name="f">F.</param>
	/// <param name="a">A.</param>
	/// <param name="b">B.</param>
	public static bool IsBetween(this float f, float min, float max) {
		return f > min && f < max;
	}
	
	/// <summary>
	/// Determines if f is equal or more than a, and less than or equal to b.
	/// </summary>
	/// <returns><c>true</c> if f is equal or more than a, and less than or equal to b.; otherwise, <c>false</c>.</returns>
	/// <param name="f">F.</param>
	/// <param name="a">A.</param>
	/// <param name="b">B.</param>
	public static bool IsBetweenInclusive(this float f, float min, float max) {
		return f >= min && f <= max;
	}
	
    /// <summary>
	/// Absolute difference between a and b. Similar to Vector3.Distance
	/// </summary>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	public static float LargestDifference(params float[] values) {
        if(values.IsNullOrEmpty()) return 0;
		float min = values.First();
        float max = values.First();
        for(int i = 0; i < values.Length; i++) {
            min = Mathf.Min(min, values[i]);
            max = Mathf.Max(max, values[i]);
        }
        return max-min;
	}

    /// <summary>
	/// Absolute difference between a and b. Similar to Vector3.Distance
	/// </summary>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	public static float Difference(float a, float b) {
		return Mathf.Abs(a - b);
	}

	/// <summary>
	/// Absolute difference between a and b.
	/// </summary>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	public static int Difference(int a, int b) {
		return Mathf.Abs(a - b);
	}


	/// <summary>
	/// Finds the average value from a list
	/// </summary>
	/// <param name="values">The list from which to find the average</param>
	/// <returns>A float equal to the average value of the list</returns>
	public static float Average(this IList<float> values){
		return values.Sum()/values.Count;
	}

	/// <summary>
	/// Returns the total of the specified values.
	/// </summary>
	/// <param name="values">Values.</param>
	public static float Sum(this IList<float> values){
		float total = 0;
		int count = values.Count;
		for(int i = 0; i < count; i++)
			total += values[i];
		return total;
	}

	//Absolute value as extension method
	public static float Abs(this float f) {
		return Mathf.Abs(f);
	}

	// ANGLES
	
	/// <summary>
	/// Calculates degrees as a fraction of value and max. For example, DegreesFromRange(1, 4) would return 90.
	/// </summary>
	/// <param name="val">Value.</param>
	/// <param name="max">Max.</param>
	public static float DegreesFromRange (float val, float max) {
		return (val / max) * 360f;
	}

	public static float RadiansFromRange (float val, float max) {
		return (val / max) * 2 * Mathf.PI;
	}

	/// <summary>
	/// Wraps a number around 360 from -180 to 180
	/// </summary>
	/// <returns>The degrees.</returns>
	/// <param name="degrees">Degrees.</param>
	public static float WrapDegrees (float degrees) {
		return MathX.RepeatInclusive(degrees, -180, 180);
	}
	
	/// <summary>
	/// Returns the difference between two degrees, wrapping values where appropriate.
	/// </summary>
	/// <param name="val">Value.</param>
	/// <param name="max">Max.</param>
	public static float AbsDeltaDegrees (float a, float b) {
		return Mathf.DeltaAngle(a, b).Abs ();
	}

	/// <summary>
	/// Wraps an angle around 360 and clamps it between min and max
	/// </summary>
	/// <returns>The degrees.</returns>
	/// <param name="degrees">Degrees.</param>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	public static float ClampDegrees (float degrees, float min, float max) {
		return Mathf.Clamp (MathX.WrapDegrees(degrees), min, max);
	}

	public static float LerpAngleUnclamped (float a, float b, float t) {
		float delta = Mathf.Repeat((b-a), 360);
		if(delta > 180)
			delta -= 360;
		return a + delta * t;
	}

	/// <summary>
	/// Standard clamp, but for doubles rather than floats
	/// </summary>
	public static double Clamp(double x, double min, double max) {
		if( x < min ) x = min;
		if( x > max ) x = max;
		return x;
	}

	public static double InverseLerp(double a, double b, double val) {
		if( b == a ) return 0.0;
		return Clamp((val-a) / (b-a), 0.0, 1.0);
	}
	
	/// <summary>
	/// Angle in degrees to normalized direction as Vector2
    /// 0 returns up, 90 returns right
	/// </summary>
	/// <returns>The vector2.</returns>
	/// <param name="degrees">Degrees.</param>
	public static Vector2 DegreesToVector2 (float degrees) {
		return RadiansToVector2(degrees * Mathf.Deg2Rad);
	}
	
	/// <summary>
	/// Angle in degrees to normalized direction as Vector2
	/// </summary>
	/// <returns>The vector2.</returns>
	/// <param name="radians">Radians.</param>
	public static Vector2 RadiansToVector2 (float radians) {
		float sin = Mathf.Sin(radians);
		float cos = Mathf.Cos(radians);
		return new Vector2(sin, cos);
	}


	//Oscellates a value between 0 and 1, where a value of 0 yields 0, 0.5 yields 1, and 1 returns to 0.
	public static float Oscellate (float value) {
		return 1f-(Mathf.Cos(Mathf.PI * 2 * value) + 1f) * 0.5f;
	}

	//Oscellates a value between min and max, where a value of 0 yields min, 0.5 yields max, and 1 returns to min.
	public static float Oscellate (float value, float min, float max) {
		return Mathf.Lerp(min, max, Oscellate(value));
	}

	// Inverse lerp, but allowing for values <0 and >1
	public static float InverseLerpUnclamped (float from, float to, float value) {
		return (value - from) / (to - from);
	}

	//CLAMP
	
	// Clamps the scale of a value, keeping its original sign. The sign of unsignedMax is arbitrary. ClampMagnitude(-3, 1) = -1, ClampMagnitude(3, 1) = 1 ClampMagnitude(-3, -1) = -1
	public static float ClampMagnitude (float value, float unsignedMax) {
		return Mathf.Min(Mathf.Abs(value), Mathf.Abs(unsignedMax)) * MathX.Sign(value);
	}
	public static float Clamp0Infinity(float value) {
		return Mathf.Clamp(value, 0, Mathf.Infinity);
	}

	public static float Clamp1Infinity(float value) {
		return Mathf.Clamp(value, 1, Mathf.Infinity);
	}
	
	public static int Clamp0Infinity(int value) {
		return Mathf.Clamp(value, 0, int.MaxValue);
	}
	
	public static int Clamp1Infinity(int value) {
		return Mathf.Clamp(value, 1, int.MaxValue);
	}
	
	/// <summary>
	/// Extension method to round a float to an int.
	/// </summary>
	/// <returns>The to int.</returns>
	/// <param name="val">Value.</param>
	public static int RoundToInt(this float val) {
		return Mathf.RoundToInt(val);
	}

	/// <summary>
	/// Rounds a value to a specified power of ten. 
	/// Decimal places can be positive or negative
	/// USED AS SO: (123.456f).RoundTo(-2); = 100;
	/// USED AS SO: (123.456f).RoundTo(2); = 123.46;
	/// </summary>
	/// <returns>The to.</returns>
	/// <param name="newNum">New number.</param>
	/// <param name="decimalPlaces">Decimal places.</param>
	public static float RoundTo(this float newNum, int decimalPlaces) {
		return Mathf.Round(newNum*Mathf.Pow(10,decimalPlaces)) / Mathf.Pow(10,decimalPlaces);
	}

	/// <summary>
	/// Rounds a value to a specified number of significant digits
	/// USED AS SO: (123.456f).RoundTo(2); = 120;
	/// </summary>
	/// <returns>The to sig.</returns>
	/// <param name="d">D.</param>
	/// <param name="digits">Digits.</param>
	public static float RoundToSig(this float d, int digits) {
		if(d == 0) return 0;
		float scale = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Abs(d))) + 1);
		return scale * (d / scale).RoundTo(digits);
	}

	/// <summary>
	/// Rounds to an interval of nearestValue.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="newNum">Value.</param>
	/// <param name="nearestValue">Nearest value.</param>
	public static float RoundToNearest(this float newNum, float nearestValue){
		return Mathf.Round(newNum/nearestValue)*nearestValue;
	}

	/// <summary>
	/// Rounds to an interval of nearestValue.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="newNum">Value.</param>
	/// <param name="nearestValue">Nearest value.</param>
	public static int RoundToNearestInt(this float newNum, int nearestValue){
		return Mathf.RoundToInt(Mathf.Round(newNum/nearestValue)*nearestValue);
	}

	/// <summary>
	/// Ceils to an interval of nearestValue.
	/// </summary>
	/// <returns>The Ceiled value.</returns>
	/// <param name="newNum">Value.</param>
	/// <param name="nearestValue">Nearest value.</param>
	public static float CeilToNearest(this float newNum, float nearestValue){
		return Mathf.Ceil(newNum/nearestValue)*nearestValue;
	}

	/// <summary>
	/// Ceils to an interval of nearestValue.
	/// </summary>
	/// <returns>The Ceiled value.</returns>
	/// <param name="newNum">Value.</param>
	/// <param name="nearestValue">Nearest value.</param>
	public static int CeilToNearestInt(this float newNum, int nearestValue){
		return Mathf.CeilToInt(Mathf.Ceil(newNum/nearestValue)*nearestValue);
	}

	/// <summary>
	/// Floors to an interval of nearestValue.
	/// </summary>
	/// <returns>The Floored value.</returns>
	/// <param name="newNum">Value.</param>
	/// <param name="nearestValue">Nearest value.</param>
	public static float FloorToNearest(this float newNum, float nearestValue){
		return Mathf.Floor(newNum/nearestValue)*nearestValue;
	}

	/// <summary>
	/// Floors to an interval of nearestValue.
	/// </summary>
	/// <returns>The Floored value.</returns>
	/// <param name="newNum">Value.</param>
	/// <param name="nearestValue">Nearest value.</param>
	public static int FloorToNearestInt(this float newNum, int nearestValue){
		return Mathf.FloorToInt(Mathf.Floor(newNum/nearestValue)*nearestValue);
	}

	//STRING -> INT HASHING
	// <summary>
	// Turns a string into a value 0 - 1 in a deterministic but essentially random way
	// </summary>
	public static float HashString(string sourceString) {
		return (float) Math.Abs(Math.Cos (sourceString.GetHashCode ()));
	}
	
	//INTERPOLATE FUNCTIONS.

	//Also known as Linear Dodge
	public static float LerpAdditive(float float1, float float2, float lerp = 1f){
		return Mathf.Lerp(float1, float1 + float2, lerp);
	}

	//Multiply
	public static float LerpMultiply(float float1, float float2, float lerp = 1f){
		return Mathf.Lerp(float1, float1 * float2, lerp);
	}

	//Screen
	public static float LerpScreen(float float1, float float2, float lerp = 1f){
		return Mathf.Lerp(float1, 1 - ((1 - float1) * (1 - float2)), lerp);
	}

	//This doesn't quite mirror what photoshop do - I think they do both techniques (multiply and screen - thats what this is) at the same time, while we do one or the other.
	public static float LerpOverlay(float float1, float float2, float lerp = 1f){
		if(float2 < 0.5f){
			return Mathf.Lerp(float1, float1 * (float2 * 2f), lerp);
		} else {
			return Mathf.Lerp(float1, 1f - ((1f - float1) * (1f - float2 * 2f)), lerp);
		}
	}

	//Lighten
	public static float LerpLighten(float float1, float float2, float lerp = 1f){
		return Mathf.Lerp(float1, Mathf.Max(float1, float2), lerp);
	}
	
	//Darken
	public static float LerpDarken(float float1, float float2, float lerp = 1f){
		return Mathf.Lerp(float1, Mathf.Min(float1, float2), lerp);
	}

	//Difference
	public static float LerpDifference(float float1, float float2, float lerp = 1f){
		return Mathf.Lerp(float1, Mathf.Max(float1, float2) - Mathf.Min(float1, float2), lerp);
	}


	// Lerp doubles
	public static double LerpD(double v1, double v2, double lerp) {
		if( lerp < 0.0 ) lerp = 0.0;
		if( lerp > 1.0 ) lerp = 1.0;
		return (1.0-lerp)*v1 + lerp*v2;
	}

	public static bool QuadraticSolver(float a, float b, float c, out float x1, out float x2) {
	    //Quadratic Formula: x = (-b +- sqrt(b^2 - 4ac)) / 2a
	    //Calculate the inside of the square root
	    float insideSquareRoot = (b * b) - 4 * a * c;
	    if (insideSquareRoot < 0) {
	        //There is no solution
	        x1 = float.NaN;
	        x2 = float.NaN;
	        return false;
	    } else {
	        //Compute the value of each x
	        //if there is only one solution, both x's will be the same
	        float sqrt = Mathf.Sqrt(insideSquareRoot);
	        x1 = (-b + sqrt) / (2 * a);
	        x2 = (-b - sqrt) / (2 * a);
			return true;
	    }
	}
	
	// Like Mathf.SmoothDamp, but for large changes in scale on a power curve. e.g. can smoothly lerp between 0.1x zoom to 1000x zoom with 
	// sensible acceleration and deceleration.
	public static double SmoothDampPowerCurveD(double current, double target, ref float scaleSpeed, float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = -1)
	{
		if( deltaTime == -1 ) deltaTime = Time.deltaTime;

		// Catch bug where (scaled time == 0) causes NaN in SmoothDamp
		if( deltaTime == 0 ) deltaTime = Time.unscaledDeltaTime;

		// E.g. need to multiply by 2x to get to target (so ratio is 2)
		var ratioToTarget = target / current;

		// E.g. how many times do we need to multiply by e to get to target (e.g. exactly once = 1.0)
		const double baseZoom = 2.0;
		var scaleStepsToTarget = (float)System.Math.Log(ratioToTarget, baseZoom);

		// Smooth damp this "number of times" -> smooth damp the "distance" to target
		var newScaleStepsToTarget = Mathf.SmoothDamp(scaleStepsToTarget, 0.0f, ref scaleSpeed, smoothTime, maxSpeed, Time.unscaledDeltaTime);

		// Convert "number of multiplies" back to a ratio after the smooth damp.
		var newRatioToTarget = System.Math.Pow(baseZoom, newScaleStepsToTarget);

		// And get final scale
		return target / newRatioToTarget;
	}




	/// <summary>
	/// I like to scale values by a random factor which can be expressed as a multiple of the value - ie 1 means a random scale between half and double the input
	/// You can't just multiply by (1 + Random.Range(-factor,factor)) since while half is 0.5, double is 2.
	/// This function returns a multipler which is in the correct scale.

	/// Example - MathX.LinearMultipler(Mathf.PerlinNoise(x, y), 0.5f);
	/// here the noise returns between 0 and 1, and the scalar is 0.5, meaning it'll return between 0.75f and 1.5f
	/// </summary>
	/// <returns>The multipler.</returns>
	/// <param name="normalizedValue">Normalized value between 0 and 1 where 0.5 always returns 1. This is normally a random number.</param>
	/// <param name="strength">Strength of the multiplier. 1 would return between 0.5 and 2 (half/double), 0.5 returns between 0.666 and 1.5, 2 returns between 0.33 and 3, 3 returns between 0.25 and 4 </param>
	public static float LinearMultipler (float normalizedValue, float strength) {
		float power = Mathf.LerpUnclamped(-1, 1, normalizedValue);
		return Mathf.Pow(1+strength, power);
	}

	//https://stackoverflow.com/questions/24887799/get-the-x-y-coordinate-on-a-square-based-on-the-square-center-angle
	public static Vector2 CircleToSquare (float radians) {
		var circleCoords = MathX.RadiansToVector2(radians);
		var u = Mathf.Max(Mathf.Abs(circleCoords.x), Mathf.Abs(circleCoords.y));
		
		var xp = circleCoords.x / u;
		var yp = circleCoords.y / u;

		return new Vector2(xp, yp);
	}
	
	public static float DoubleInverseLerp (float a, float b, float c, float d, float value) {
		Debug.Assert(a <= b);
		Debug.Assert(b <= c);
		Debug.Assert(c <= d);
		if(value >= d) return 0;
		else if(value >= c) return Mathf.InverseLerp(d, c, value);
		else if(value >= b) return 1;
		if(value >= a) return Mathf.InverseLerp(a, b, value);
		return 0;
	}






	public static float Remap(this float val, float aIn1, float aIn2, float aOut1, float aOut2) {
		float t = (val - aIn1) / (aIn2 - aIn1);
		return aOut1 + (aOut2 - aOut1) * t;
	}
	public static float RemapClamped(this float val, float aIn1, float aIn2, float aOut1, float aOut2) {
		float t = (val - aIn1) / (aIn2 - aIn1);
		if (t > 1f) return aOut2;
		if(t < 0f) return aOut1;
		return aOut1 + (aOut2 - aOut1) * t;
	}
}