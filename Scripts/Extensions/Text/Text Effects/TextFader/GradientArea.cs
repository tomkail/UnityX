using UnityEngine;

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
        // Not done
        return gradient.Evaluate(normalizedPosition.x);
    }
    static Color EvaluateReflectedGradientAtPosition(Vector2 normalizedPosition, Gradient gradient) {
        return gradient.Evaluate(Mathf.Abs(normalizedPosition.x - 0.5f)*2);
    }
    

    Color[] CreateGradient (GradientX.GradientType gradientType, Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height) {
        switch (gradientType){
            case GradientX.GradientType.Linear:
                return CreateLinearGradient(gradient, startPosition, endPosition, width, height);
            case GradientX.GradientType.Radial:
                return CreateRadialGradient(gradient, startPosition, endPosition, width, height);
            case GradientX.GradientType.Conical:
                return CreateConicalGradient(gradient, startPosition, endPosition, width, height);
            case GradientX.GradientType.Reflected:
                return CreateReflectedGradient(gradient, startPosition, endPosition, width, height);
            default:
                return CreateConicalGradient(gradient, startPosition, endPosition, width, height);
        }
    }
		
    Color[] CreateLinearGradient(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
        int numPixels = width * height;
        Color[] pixels = new Color[numPixels];
        float widthReciprocal = 1f/Clamp1Infinity(width-1);
        float heightReciprocal = 1f/Clamp1Infinity(height-1);
			
        for(int y = 0; y < height; y++){
            for(int x = 0; x < width; x++){
                Vector2 point = new Vector2(x * widthReciprocal, y * heightReciprocal);
                float distance = Vector2.Dot(point - endPosition, startPosition - endPosition) / ((endPosition-startPosition).sqrMagnitude);
                pixels[y * width + x] = gradient.Evaluate(distance);
            }
        }
			
        return pixels;
    }
		
    Color[] CreateRadialGradient(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
        int numPixels = width * height;
        Color[] pixels = new Color[numPixels];
        float widthReciprocal = 1f/Clamp1Infinity(width-1);
        float heightReciprocal = 1f/Clamp1Infinity(height-1);
        float length = Vector2.Distance(startPosition, endPosition);
			
        for(int y = 0; y < height; y++){
            for(int x = 0; x < width; x++){
                float tmpRadius = Vector2.Distance(new Vector2(x * widthReciprocal, y * heightReciprocal), startPosition);
                pixels[y * width + x] = gradient.Evaluate(tmpRadius / length);
            }
        }
			
        return pixels;
    }
		
    Color[] CreateConicalGradient(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
        int numPixels = width * height;
        Color[] pixels = new Color[numPixels];
        float widthReciprocal = 1f/Clamp1Infinity(width-1);
        float heightReciprocal = 1f/Clamp1Infinity(height-1);
        float degrees = DegreesBetween(startPosition, endPosition);
			
        for(int y = 0; y < height; y++){
            for(int x = 0; x < width; x++){
                float a = Mathf.Atan2(y * heightReciprocal - startPosition.y, x * widthReciprocal - startPosition.x);
                a += (degrees+180) * Mathf.Deg2Rad;
                a /= (Mathf.PI * 2);
                a+=0.5f;
                a = Mathf.Repeat(a,1f);
                pixels[y * width + x] = gradient.Evaluate(a);
            }
        }
			
        return pixels;
    }
		
    Color[] CreateReflectedGradient(Gradient gradient, Vector2 startPosition, Vector2 endPosition, int width, int height){
        int numPixels = width * height;
        Color[] pixels = new Color[numPixels];
        float widthReciprocal = 1f/Clamp1Infinity(width-1);
        float heightReciprocal = 1f/Clamp1Infinity(height-1);
			
        for(int y = 0; y < height; y++){
            for(int x = 0; x < width; x++){
                Vector2 point = new Vector2(x * widthReciprocal, y * heightReciprocal);
                float distance = NormalizedDistance(startPosition, endPosition, point);
                pixels[y * width + x] = gradient.Evaluate(distance);
            }
        }
			
        return pixels;
    }
		
    static float Clamp1Infinity(float value) {
        return Mathf.Clamp(value, 1, Mathf.Infinity);
    }
		
    static float DegreesBetween(Vector2 a, Vector2 b) {
        return RadiansBetween(a,b) * Mathf.Rad2Deg;
    }
		
    static float RadiansBetween(Vector2 a, Vector2 b) {
        return Mathf.Atan2(-(b.y - a.y), b.x - a.x) + (Mathf.Deg2Rad * 90);
    }
		
    static float NormalizedDistance(Vector2 a, Vector2 b, Vector2 point) {
        return (Vector2.Dot(point - a, b - a) / ((a-b).sqrMagnitude)).Abs();
    }
		
    static Vector2 Radians2Vector2 (float radians) {
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(sin, cos);
    }
}