using UnityEngine;

public class DrawingUtils {
	public static Vector2[] DrawRoundedCorner(Vector2 angularPoint, Vector2 p1, Vector2 p2, float radius, float degreesPerPoint) {
		if(p1 == p2 || p1 == angularPoint || angularPoint == p2) {
			Debug.LogError(angularPoint+" "+p1+" "+p2);
		}
		if(Mathf.Approximately(radius, 0) || Mathf.Approximately(degreesPerPoint, 0)) return new Vector2[] {angularPoint};
		
		//Vector 1
		float dx1 = angularPoint.x - p1.x;
		float dy1 = angularPoint.y - p1.y;

		//Vector 2
		float dx2 = angularPoint.x - p2.x;
		float dy2 = angularPoint.y - p2.y;

		//Angle between vector 1 and vector 2 divided by 2
		float angle = (Mathf.Atan2(dy1, dx1) - Mathf.Atan2(dy2, dx2)) / 2;

		// The length of segment between angular point and the
		// points of intersection with the circle of a given radius
		float tan = Mathf.Abs(Mathf.Tan(angle));
		float segment = radius / tan;

		//Check the segment
		float length1 = new Vector2(dx1, dy1).magnitude;
		float length2 = new Vector2(dx2, dy2).magnitude;

		float length = Mathf.Min(length1, length2);

		if (segment > length)
		{
			segment = length;
			radius = (float)(length * tan);
		}

		// Points of intersection are calculated by the proportion between 
		// the coordinates of the vector, length of vector and the length of the segment.
		var p1Cross = GetProportionPoint(angularPoint, segment, length1, dx1, dy1);
		var p2Cross = GetProportionPoint(angularPoint, segment, length2, dx2, dy2);

		// Calculation of the coordinates of the circle 
		// center by the addition of angular vectors.
		float dx = angularPoint.x * 2 - p1Cross.x - p2Cross.x;
		float dy = angularPoint.y * 2 - p1Cross.y - p2Cross.y;

		float L = new Vector2(dx, dy).magnitude;
		if(Mathf.Approximately(L, 0)) return new Vector2[] {angularPoint};

		float d = new Vector2(segment, radius).magnitude;

		var circlePoint = GetProportionPoint(angularPoint, d, L, dx, dy);

		//StartAngle and EndAngle of arc
		var startAngle = Vector2.SignedAngle(p1Cross-circlePoint, Vector2.up);
		var endAngle = Vector2.SignedAngle(p2Cross-circlePoint, Vector2.up);

		//Sweep angle
		var sweepAngle = Mathf.DeltaAngle(startAngle, endAngle);

		if(Mathf.Approximately(sweepAngle, 0)) return new Vector2[] {angularPoint};
		int pointsCount = Mathf.Max((int)Mathf.Abs(sweepAngle/degreesPerPoint), 1);
		Vector2[] points = new Vector2[pointsCount];

		var n = 1f/(Mathf.Max(pointsCount-1, 1));
		for (int i = 0; i < pointsCount; ++i) {
			var radians = Mathf.LerpAngle(startAngle, endAngle, i * n) * Mathf.Deg2Rad;
			var vector = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
			points[i] = circlePoint + vector * radius;
		}
		return points;
	}

	static Vector2 GetProportionPoint(Vector2 point, float segment, float length, float dx, float dy) {
		float factor = segment / length;
		return new Vector2((float)(point.x - dx * factor), (float)(point.y - dy * factor));
	}
}