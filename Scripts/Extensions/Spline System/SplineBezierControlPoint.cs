using UnityEngine;

namespace SplineSystem {
    [System.Serializable]
    public struct SplineBezierControlPoint {
        public enum DirectionSign {
            In = -1,
            Out = 1
        }

        public readonly DirectionSign directionSign;
        public readonly float distance;

        public static SplineBezierControlPoint In(float distance) => new (DirectionSign.In, distance);
        public static SplineBezierControlPoint InAuto(Vector3 position, Vector3 otherPosition, float normalizedControlPointDistance = 0.25f) => new (DirectionSign.In, GetAutoDistance(DirectionSign.In, position, otherPosition, normalizedControlPointDistance));
        public static SplineBezierControlPoint Out(float distance) => new (DirectionSign.Out, distance);
        public static SplineBezierControlPoint OutAuto(Vector3 position, Vector3 otherPosition, float normalizedControlPointDistance = 0.25f) => new (DirectionSign.Out, GetAutoDistance(DirectionSign.Out, position, otherPosition, normalizedControlPointDistance));
        
        public SplineBezierControlPoint WithDistance(float controlPointDistance) => new(directionSign, controlPointDistance);
        
        SplineBezierControlPoint(DirectionSign directionSign, float distance) {
            this.directionSign = directionSign;
            this.distance = distance;
        }

        public Vector3 GetPosition(SplineBezierPoint bezierPoint) => bezierPoint.position + GetDirection(bezierPoint) * distance;
        public Vector3 GetDirection(SplineBezierPoint bezierPoint) => bezierPoint.forward * (int) directionSign;
        public Quaternion GetRotation(SplineBezierPoint bezierPoint) => Quaternion.LookRotation(GetDirection(bezierPoint), bezierPoint.normal);
        
        public static float GetAutoDistanceIn(Vector3 position, Vector3 otherPosition, float normalizedControlPointDistance) => (position - otherPosition).magnitude * normalizedControlPointDistance;
        public static float GetAutoDistanceOut(Vector3 position, Vector3 otherPosition, float normalizedControlPointDistance) => (otherPosition - position).magnitude * normalizedControlPointDistance;
        
        public static float GetAutoDistance(DirectionSign directionSign, Vector3 position, Vector3 otherPosition, float normalizedControlPointDistance) {
            return directionSign switch {
                DirectionSign.In => GetAutoDistanceIn(position, otherPosition, normalizedControlPointDistance),
                DirectionSign.Out => GetAutoDistanceOut(position, otherPosition, normalizedControlPointDistance),
                _ => 0
            };
        }

        
    }
}