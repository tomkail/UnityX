using UnityEngine;

public static class Bezier {
	public static Vector2 GetPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
		if(t <= 0) return p0;
		if(t >= 1) return p3;
        
        float t2 = t*t;
        float OneMinusT = 1f - t;
        float OneMinusT2 = OneMinusT*OneMinusT;

        float p0Scalar = OneMinusT2*OneMinusT;
        float p1Scalar = 3f * OneMinusT2 * t;
        float p2Scalar = 3f * OneMinusT * t2;
        float p3Scalar = t2*t;
        
        return new Vector2(
            p0Scalar * p0.x + p1Scalar * p1.x + p2Scalar * p2.x + p3Scalar * p3.x,
            p0Scalar * p0.y + p1Scalar * p1.y + p2Scalar * p2.y + p3Scalar * p3.y
        );

        // OneMinusT * OneMinusT * OneMinusT * p0 +
        // 3f * OneMinusT * OneMinusT * t * p1 +
        // 3f * OneMinusT * t * t * p2 +
        // t * t * t * p3;
    }
	public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		if(t <= 0) return p0;
		if(t >= 1) return p3;
        
        float t2 = t*t;
        float OneMinusT = 1f - t;
        float OneMinusT2 = OneMinusT*OneMinusT;

        float p0Scalar = OneMinusT2*OneMinusT;
        float p1Scalar = 3f * OneMinusT2 * t;
        float p2Scalar = 3f * OneMinusT * t2;
        float p3Scalar = t2*t;
        
        return new Vector3(
            p0Scalar * p0.x + p1Scalar * p1.x + p2Scalar * p2.x + p3Scalar * p3.x,
            p0Scalar * p0.y + p1Scalar * p1.y + p2Scalar * p2.y + p3Scalar * p3.y,
            p0Scalar * p0.z + p1Scalar * p1.z + p2Scalar * p2.z + p3Scalar * p3.z
        );

        // OneMinusT * OneMinusT * OneMinusT * p0 +
        // 3f * OneMinusT * OneMinusT * t * p1 +
        // 3f * OneMinusT * t * t * p2 +
        // t * t * t * p3;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        var scalar10 = 3f * oneMinusT * oneMinusT;
        var scalar21 = 6f * oneMinusT * t;
        var scalar32 = 3f * t * t;

        return new Vector3(
            scalar10 * (p1.x - p0.x) + scalar21 * (p2.x - p1.x) + scalar32 * (p3.x - p2.x),
            scalar10 * (p1.y - p0.y) + scalar21 * (p2.y - p1.y) + scalar32 * (p3.y - p2.y),
            scalar10 * (p1.z - p0.z) + scalar21 * (p2.z - p1.z) + scalar32 * (p3.z - p2.z)
        );

        // return
        //     3f * oneMinusT * oneMinusT * (p1 - p0) +
        //     6f * oneMinusT * t * (p2 - p1) +
        //     3f * t * t * (p3 - p2);
    }

	//https://stackoverflow.com/questions/29438398/cheap-way-of-calculating-cubic-bezier-length
	public static float GetRoughLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
		var chord = (p3-p0).magnitude;
		var cont_net = (p0 - p1).magnitude + (p2 - p1).magnitude + (p3 - p2).magnitude;
		return (cont_net + chord) / 2f;
	}
}