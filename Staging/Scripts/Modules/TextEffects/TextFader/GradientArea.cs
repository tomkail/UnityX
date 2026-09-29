using UnityEngine;

namespace UnityX.TextEffects {

public class GradientArea : MonoBehaviour {
    public GradientX.GradientType gradientType = GradientX.GradientType.Conical;
    public Gradient gradient;

    void OnDrawGizmosSelected() {
        var matrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        if (transform is RectTransform rectTransform) {
            Gizmos.DrawWireCube(rectTransform.rect.center, rectTransform.rect.size);
        } else {
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        Gizmos.matrix = matrix;
    }

    public Color EvaluateAtPosition (Vector3 position) {
        if (gradient == null) return default;
        var matrix = transform.worldToLocalMatrix;
        if (transform is RectTransform rectTransform) {
            var rect = rectTransform.rect;
            matrix = Matrix4x4.TRS(rect.center, Quaternion.identity, new Vector3(rect.width, rect.height, 1)).inverse * transform.worldToLocalMatrix;
        }

        var normalizedPosition = matrix.MultiplyPoint3x4(position);
        normalizedPosition += Vector3.one * 0.5f;
        switch (gradientType){
            case GradientX.GradientType.Linear:
                return EvaluateLinearGradientAtPosition(normalizedPosition, gradient);
            case GradientX.GradientType.Radial:
                return EvaluateRadialGradientAtPosition(normalizedPosition, gradient);
            case GradientX.GradientType.Conical:
                return EvaluateConicalGradientAtPosition(normalizedPosition, gradient);
            case GradientX.GradientType.Reflected:
                return EvaluateReflectedGradientAtPosition(normalizedPosition, gradient);
            default:
                return EvaluateConicalGradientAtPosition(normalizedPosition, gradient);
        }
    }

    static Color EvaluateLinearGradientAtPosition(Vector2 normalizedPosition, Gradient gradient) {
        return gradient.Evaluate(normalizedPosition.x);
    }

    static Color EvaluateRadialGradientAtPosition(Vector2 normalizedPosition, Gradient gradient) {
        return gradient.Evaluate((normalizedPosition - Vector2.one * 0.5f).magnitude*2);
    }
    static Color EvaluateConicalGradientAtPosition(Vector2 normalizedPosition, Gradient gradient) {
        // TODO: conical gradient not implemented — falls back to a linear (along-X) gradient. NOTE: Conical is the default mode, so this fallback is used by default.
        return gradient.Evaluate(normalizedPosition.x);
    }
    static Color EvaluateReflectedGradientAtPosition(Vector2 normalizedPosition, Gradient gradient) {
        return gradient.Evaluate(Mathf.Abs(normalizedPosition.x - 0.5f)*2);
    }

    static float Clamp1Infinity(float value) {
        return Mathf.Max(value, 1);
    }

    static float NormalizedDistance(Vector2 a, Vector2 b, Vector2 point) {
        return (Vector2.Dot(point - a, b - a) / ((a-b).sqrMagnitude)).Abs();
    }
}
}
