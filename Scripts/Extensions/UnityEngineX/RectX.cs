using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO - change new Rect() to rect.Set()!
public static class RectX {
	public static float Area(this Rect r) {
		return r.width * r.height;
	}

	//Similar to Unity's epsilon comparison, but allows for any precision.
	public static bool NearlyEqual(Rect a, Rect b, float maxDifference = 0.001f) {
		if (a == b)  { 
			return true;
		} else {
			return 
				MathX.Difference(a.x, b.x) < maxDifference && 
				MathX.Difference(a.y, b.y) < maxDifference && 
				MathX.Difference(a.xMax, b.xMax) < maxDifference &&
				MathX.Difference(a.yMax, b.yMax) < maxDifference;
	    }
	}

	public static Rect MinMaxRect (Vector2 min, Vector2 max) {
		return Rect.MinMaxRect (min.x, min.y, max.x, max.y);
	}

	/// <summary>
	/// Creates new rect that encapsulates a list of vectors.
	/// </summary>
	/// <param name="vectors">Vectors.</param>
	public static Rect CreateEncapsulating (params Vector2[] vectors) {
		float xMin = vectors[0].x;
		float xMax = vectors[0].x;
		float yMin = vectors[0].y;
		float yMax = vectors[0].y;
		for(int i = 1; i < vectors.Length; i++) {
			var vector = vectors[i];
			xMin = Mathf.Min (xMin, vector.x);
			xMax = Mathf.Max (xMax, vector.x);
			yMin = Mathf.Min (yMin, vector.y);
			yMax = Mathf.Max (yMax, vector.y);
		}
		return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
	}
	
	/// <summary>
	/// Creates new rect that encapsulates a list of vectors.
	/// </summary>
	/// <param name="vectors">Vectors.</param>
	public static Rect CreateEncapsulating (IEnumerable<Vector2> vectors) {
		var enumerator = vectors.GetEnumerator();
		enumerator.MoveNext();
		float xMin = enumerator.Current.x;
		float xMax = enumerator.Current.x;
		float yMin = enumerator.Current.y;
		float yMax = enumerator.Current.y;
		while(enumerator.MoveNext()) {
			var vector = enumerator.Current;
			xMin = Mathf.Min (xMin, vector.x);
			xMax = Mathf.Max (xMax, vector.x);
			yMin = Mathf.Min (yMin, vector.y);
			yMax = Mathf.Max (yMax, vector.y);
		}
		return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
	}

	public static Rect CreateEncapsulating (params Rect[] rects) {
		Rect rect = new Rect(rects[0]);
		for(int i = 1; i < rects.Length; i++)
			rect = rect.Encapsulating(rects[i]);
		return rect;
	}
    
	public static Rect CreateEncapsulating (IEnumerable<Rect> rects) {
		var enumerator = rects.GetEnumerator();
		enumerator.MoveNext();
		Rect rect = enumerator.Current;
		while(enumerator.MoveNext())
			rect = rect.Encapsulating(enumerator.Current);
		return rect;
	}
    public static Rect CreateFromCenter (Vector2 centerPosition, Vector2 size) {
		return CreateFromCenter(centerPosition.x, centerPosition.y, size.x, size.y);
	}
	
	public static Rect CreateFromCenter (float centerX, float centerY, float sizeX, float sizeY) {
		return new Rect(centerX - sizeX * 0.5f, centerY - sizeY * 0.5f, sizeX, sizeY);
	}

	public static Rect Create (Vector2 position, Vector2 size, Vector2 pivot) {
		return new Rect(position.x - size.x * pivot.x, position.y - size.y * pivot.y, size.x, size.y);
	}

	public static Rect Lerp (Rect rect1, Rect rect2, float lerp) {
		Vector4 newRect = Vector4.Lerp(new Vector4(rect1.x, rect1.y, rect1.width, rect1.height), new Vector4(rect2.x, rect2.y, rect2.width, rect2.height), lerp);
		return new Rect(newRect.x, newRect.y, newRect.z, newRect.w);
	}

	public static Rect Add(this Rect left, Rect right){
		return new Rect(left.x+right.x, left.y+right.y, left.width+right.width, left.height+right.height);
	}
	
	public static Rect Subtract(this Rect left, Rect right){
		return new Rect(left.x-right.x, left.y-right.y, left.width-right.width, left.height-right.height);
	}

	public static Rect CopyWithX(this Rect r, float x){
		return new Rect(x, r.y, r.width, r.height);
	}

	public static Rect CopyWithY(this Rect r, float y){
		return new Rect(r.x, y, r.width, r.height);
	}

	public static Rect CopyWithWidth(this Rect r, float width){
		return new Rect(r.x, r.y, width, r.height);
	}

	public static Rect CopyWithHeight(this Rect r, float height){
		return new Rect(r.x, r.y, r.width, height);
	}

	public static Rect CopyWithPosition(this Rect r, Vector2 position){
		return new Rect(position.x, position.y, r.width, r.height);
	}

	public static Rect CopyWithSize(this Rect r, Vector2 size){
		return new Rect(r.x, r.y, size.x, size.y);
	}
	public static Rect Inset(this Rect rect, float pixels) {
		return new Rect(
			rect.x + pixels, 
			rect.y + pixels, 
			rect.width - 2*pixels, 
			rect.height - 2*pixels
		);
	}


	/// <summary>
	/// Returns a version of the rect with position clamped between minPosition and maxPosition
	/// </summary>
	/// <returns>The position.</returns>
	/// <param name="r">The red component.</param>
	/// <param name="minSize">Minimum position.</param>
	/// <param name="maxSize">Max position.</param>
	public static Rect ClampPosition(this Rect r, Vector2 minPosition, Vector2 maxPosition) {
		r.x = Mathf.Clamp(r.x, minPosition.x, maxPosition.x);
		r.y = Mathf.Clamp(r.y, minPosition.y, maxPosition.y);
		return r;
	}

	/// <summary>
	/// Returns a version of the rect with position clamped between minSize and maxSize
	/// </summary>
	/// <returns>The position.</returns>
	/// <param name="r">The red component.</param>
	/// <param name="minSize">Minimum size.</param>
	/// <param name="maxSize">Max size.</param>
	public static Rect ClampSize(this Rect r, Vector2 minSize, Vector2 maxSize) {
		r.width = Mathf.Clamp(r.width, minSize.x, maxSize.x);
		r.height = Mathf.Clamp(r.height, minSize.y, maxSize.y);
		return r;
	}

	
	
    /// <summary>
	/// Expands (or contracts) the rect by a specified amount.
	/// So, half of x expansion will be on left, half of x expansion will be on right, etc.
	/// </summary>
	/// <param name="r">The red component.</param>
	/// <param name="expansion">Expansion.</param>
	public static Rect WithSize(this Rect r, Vector2 size){
		return WithSize (r, size, new Vector2(0.5f, 0.5f));
	}
	
	/// <summary>
	/// Expands (or contracts) the rect by a specified amount, using a pivot to control the expansion center point.
	/// </summary>
	/// <param name="r">The red component.</param>
	/// <param name="expansion">Expansion.</param>
	/// <param name="pivot">Pivot.</param>
	public static Rect WithSize(this Rect r, Vector2 size, Vector2 pivot) {
		return Create(r.center, size, pivot);
	}

    /// <summary>
	/// Expands (or contracts) the rect by a specified amount.
	/// So, half of x expansion will be on left, half of x expansion will be on right, etc.
	/// </summary>
	/// <param name="r">The red component.</param>
	/// <param name="expansion">Expansion.</param>
	public static Rect Expanded(this Rect r, Vector2 expansion){
		return Expanded (r, expansion, new Vector2(0.5f, 0.5f));
	}
	
	/// <summary>
	/// Expands (or contracts) the rect by a specified amount, using a pivot to control the expansion center point.
    /// 0,0 is bottom left and 1,1 is top right
	/// </summary>
	/// <param name="r">The red component.</param>
	/// <param name="expansion">Expansion.</param>
	/// <param name="pivot">Pivot.</param>
	public static Rect Expanded(this Rect r, Vector2 expansion, Vector2 pivot) {
        return new Rect(
            r.x + (expansion.x * -pivot.x), 
            r.y + (expansion.y * -pivot.y), 
            r.width + expansion.x, 
            r.height + expansion.y
        );
	}

    // Expands the rect to fit a target aspect ratio
    public static Rect ExpandedToFitAspectRatio (this Rect rect, float targetAspect) {
        return rect.ExpandedToFitAspectRatio(targetAspect, new Vector2(0.5f, 0.5f));
    }
    public static Rect ExpandedToFitAspectRatio (this Rect rect, float targetAspect, Vector2 pivot) {
        var rectAspect = rect.GetAspect();
		var newSize = rect.size;
        if(targetAspect > rectAspect) {
            newSize.x = rect.width + rect.width * (targetAspect - rectAspect);
        } else if(targetAspect < rectAspect) {
            newSize.y = rect.height + rect.height * (rectAspect - targetAspect);
        }
		var offset = new Vector2((newSize.x-rect.width) * -pivot.x, (newSize.y-rect.height) * -pivot.y);
		return new Rect(rect.min + offset, newSize);
    }

    public static Rect CompressToFitAspectRatio (this Rect rect, float targetAspect) {
        return rect.CompressToFitAspectRatio(targetAspect, new Vector2(0.5f, 0.5f));
    }
    public static Rect CompressToFitAspectRatio (this Rect rect, float targetAspect, Vector2 pivot) {
        var rectAspect = rect.GetAspect();
		var newSize = rect.size;
        if(targetAspect > rectAspect) {
            newSize.y = rect.height * (rectAspect / targetAspect);
        } else if(targetAspect < rectAspect) {
            newSize.x = rect.width * (targetAspect / rectAspect);
        }
		var offset = new Vector2((newSize.x-rect.width) * -pivot.x, (newSize.y-rect.height) * -pivot.y);
		return new Rect(rect.min + offset, newSize);
    }

	/// <summary>
	/// The corners of the rect, in clockwise order from the top left.
	/// </summary>
	/// <param name="r">The red component.</param>
	public static Vector2[] Corners (this Rect r) {
		return new Vector2[4] {r.TopLeft(), r.TopRight(), r.BottomRight(), r.BottomLeft()};
	}

	/// <summary>
	/// The corners of the rect, in a format that can be used by some Unity functions like RectTransform.GetWorldCorners.
	/// </summary>
	/// <param name="r">The red component.</param>
	public static Vector3[] Corners3D (this Rect r) {
		return new Vector3[4] {r.TopLeft(), r.TopRight(), r.BottomRight(), r.BottomLeft()};
	}
	
	public static Vector2 TopLeft(this Rect r){
		return new Vector2(r.x, r.y+r.height);
	}
	
	public static Vector2 TopRight(this Rect r){
		return r.max;
	}
	
	public static Vector2 BottomLeft(this Rect r){
		return r.min;
	}
	
	public static Vector2 BottomRight(this Rect r){
		return new Vector2(r.x+r.width, r.y);
	}

	/// <summary>
	/// The point on the bounding box or inside the bounding box.
    //  If the point is inside the bounding box, unmodified point position will be returned.
	/// </summary>
	/// <param name="r">The red component.</param>
	/// <param name="point">Point.</param>
	public static Vector2 ClosestPoint(this Rect rect, Vector2 point) {
		point.x = Mathf.Clamp(point.x, rect.min.x, rect.max.x);
		point.y = Mathf.Clamp(point.y, rect.min.y, rect.max.y);
		return point;
	}

    /// <summary>
	/// The point on the bounding box.
    //  If the point is inside the bounding box, point on edge of rect closest to position will be returned.
	/// </summary>
	/// <param name="r">The red component.</param>
	/// <param name="point">Point.</param>

	public static Vector2 ClosestPointOnPerimeter(this Rect r, Vector2 point) {
		point.x = Mathf.Clamp(point.x, r.xMin, r.xMax);
		point.y = Mathf.Clamp(point.y, r.yMin, r.yMax);

		var dl = Mathf.Abs(point.x - r.xMin);
		var dr = Mathf.Abs(point.x - r.xMax);
		var dt = Mathf.Abs(point.y - r.yMin);
		var db = Mathf.Abs(point.y - r.yMax);
		var m = Mathf.Min(dl, dr, dt, db);
		if(m == dt) return new Vector2(point.x, r.yMin);
		else if(m == db) return new Vector2(point.x, r.yMax);
		else if(m == dl) return new Vector2(r.xMin, point.y);
		else return new Vector2(r.xMax, point.y);
	}

    // Splats the vector in the rect.
    // Returns the point on the edge of the rect as if you'd fired a ray from the center in the vector direction
	public static Vector2 SplatVector(Rect rect, Vector2 vector)  {
		// Degenerate cases
		if( vector == Vector2.zero)
			return rect.center;

		float vecAspect = Mathf.Abs(vector.x / vector.y);
		float rectAspect = rect.size.x / rect.size.y;

		// Clamp to sides
		float scale;
		if( vecAspect > rectAspect ) {
			scale = Mathf.Abs((0.5f*rect.size.x) / vector.x);
		} 

		// Clamp to top/bottom
		else {
			scale = Mathf.Abs((0.5f*rect.size.y) / vector.y);
		}

		return (scale * vector) + rect.center;
	}

	/// <summary>
	/// Returns the point as a normalized position inside the rect, where 0,0 is the bottom left and 1,1 is the top right
	/// </summary>
	/// <returns>The normalized position inside rect.</returns>
	/// <param name="r">The red component.</param>
	/// <param name="point">Point.</param>
	public static Vector2 GetNormalizedPositionInsideRect (this Rect r, Vector2 point) {
		return new Vector2((point.x - r.x) / r.width, (point.y - r.y) / r.height);
	}

	public static Vector2 GetPointFromNormalizedPoint(this Rect r, Vector2 normalizedPoint) {
		return new Vector2(normalizedPoint.x * r.width + r.x, normalizedPoint.y * r.height + r.y);
	}
	
	public static Rect ClampInsideWithFlexibleSize(Rect r, Rect container) {
        Rect rect = Rect.zero;
        rect.xMin = Mathf.Max(r.xMin, container.xMin);
        rect.xMax = Mathf.Min(r.xMax, container.xMax);
        rect.yMin = Mathf.Max(r.yMin, container.yMin);
        rect.yMax = Mathf.Min(r.yMax, container.yMax);
        if(rect.width < 0) {
            if(rect.xMin > container.xMax) rect.x += rect.width;   
            rect.width = 0;
        }
        if(rect.height < 0) {
            if(rect.yMin > container.yMax) rect.y += rect.height;   
            rect.height = 0;
        }
        return rect;
	}
	
	public static Rect ClampInsideKeepSize(Rect r, Rect container) {
		Rect rect = Rect.zero;
        rect.xMin = Mathf.Max(r.xMin, container.xMin);
        rect.xMax = Mathf.Min(r.xMax, container.xMax);
        rect.yMin = Mathf.Max(r.yMin, container.yMin);
        rect.yMax = Mathf.Min(r.yMax, container.yMax);
        
        if(r.xMin < container.xMin) rect.width += container.xMin - r.xMin;
        if(r.yMin < container.yMin) rect.height += container.yMin - r.yMin;
        if(r.xMax > container.xMax) {
            rect.x -= r.xMax - container.xMax;
            rect.width += r.xMax - container.xMax;
        }
        if(r.yMax > container.yMax) {
            rect.y -= r.yMax - container.yMax;
            rect.height += r.yMax - container.yMax;
        }

        // Finally make sure we're fully contained
        rect.xMin = Mathf.Max(rect.xMin, container.xMin);
        rect.xMax = Mathf.Min(rect.xMax, container.xMax);
        rect.yMin = Mathf.Max(rect.yMin, container.yMin);
        rect.yMax = Mathf.Min(rect.yMax, container.yMax);
        return rect;
	}

	public static bool ContainsAny (this Rect r, params Vector2[] positions) {
		foreach(Vector2 position in positions) {
			if(r.Contains(position)) {
				return true;
			}
		}
		return false;
	}
        
    /// <summary>
    /// Returns true if the passed Rect is entirely within the bounds of the receiver rect.
    /// </summary>
    public static bool EntirelyContainsRect (this Rect r, Rect otherRect) {
        return r == otherRect || (r.Contains(otherRect.min) && r.Contains(otherRect.max));
    }

	// THIS IS THE SAME AS INTERSECT!
	public static Rect Encapsulating(this Rect r, Rect rect) {
		r = r.Encapsulating(rect.min);
		r = r.Encapsulating(rect.max);
		return r;
	}
	
	public static Rect Encapsulating(this Rect r, Vector2 point) {
		var xMin = Mathf.Min (r.xMin, point.x);
		var xMax = Mathf.Max (r.xMax, point.x);
		var yMin = Mathf.Min (r.yMin, point.y);
		var yMax = Mathf.Max (r.yMax, point.y);
		return Rect.MinMaxRect (xMin, yMin, xMax, yMax);
	}


	/// <summary>
	/// Returns true if the passed Rect is within the bounds of the receiver rect.
	/// </summary>
	public static bool Intersects(Rect r1, Rect r2) {
		return !( r1.xMax < r2.x || r1.x > r2.xMax || r1.yMax < r2.y || r1.y > r2.yMax );
	}
	
	/// <summary>
	/// Returns true if the passed Rect is within the bounds of the receiver rect.
	/// </summary>
    public static bool Intersects(float r1xMin, float r1xMax, float r1yMin, float r1yMax, float r2xMin, float r2xMax, float r2yMin, float r2yMax) {
		return !( r1xMax < r2xMin || r1xMin > r2xMax || r1yMax < r2yMin || r1yMin > r2yMax );
	}


	/// <summary>
	/// Find the rect of overlap between two other rects.
	/// Returns Rect.zero if they don't overlap.
	/// </summary>
	public static Rect Intersect(Rect r1, Rect r2) {
		if(!Intersects(r1, r2))
			return Rect.zero;

		float xMin = Mathf.Max(r1.x, r2.x);
		float xMax = Mathf.Min(r1.xMax, r2.xMax);
		float yMin = Mathf.Max(r1.y, r2.y);
		float yMax = Mathf.Min(r1.yMax, r2.yMax);

		return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
	}

	/// <summary>
	/// Find the rect of overlap between two other rects.
	/// Returns false if they don't overlap.
	/// </summary>
	public static bool Intersect(Rect r1, Rect r2, ref Rect output) {
		if(!Intersects(r1, r2)) {
            output = Rect.zero;
			return false;
        }

		float xMin = Mathf.Max(r1.x, r2.x);
		float xMax = Mathf.Min(r1.xMax, r2.xMax);
		float yMin = Mathf.Max(r1.y, r2.y);
		float yMax = Mathf.Min(r1.yMax, r2.yMax);

		output = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
		return true;
	}

	/// <summary>
	/// Find the rect of overlap between two other rects.
	/// Returns false if they don't overlap.
	/// </summary>
	public static bool Intersect(float r1xMin, float r1xMax, float r1yMin, float r1yMax, float r2xMin, float r2xMax, float r2yMin, float r2yMax, ref Rect output) {
		if( r1xMax < r2xMin || r1xMin > r2xMax || r1yMax < r2yMin || r1yMin > r2yMax )
			return false;

		float xMin = Mathf.Max(r1xMin, r2xMin);
		float xMax = Mathf.Min(r1xMax, r2xMax);
		float yMin = Mathf.Max(r1yMin, r2yMin);
		float yMax = Mathf.Min(r1yMax, r2yMax);

		output = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
		return true;
	}

	
	/// <summary>
	/// Find the closest distance between two rects.
	/// Returns 0 if they overlap.
	/// </summary>
    public static float GetClosestDistance (Rect rect1, Rect rect2) {
		var centerDiff = rect1.center - rect2.center;
		var combinedExtents = (rect1.size + rect2.size) * 0.5f;
		var delta = new Vector2(Mathf.Abs(centerDiff.x), Mathf.Abs(centerDiff.y)) - combinedExtents;
		delta.x = Mathf.Max(0, delta.x);
		delta.y = Mathf.Max(0, delta.y);
        return delta.magnitude;
    }


	/// <summary>
	/// Gets the vertices.
	/// Returns moving clockwise around from the center, starting with the top left.
	/// </summary>
	/// <returns>The vertices.</returns>
	/// <param name="rect">Rect.</param>
	public static Vector2[] GetVertices(this Rect rect) {
		Vector2[] vertices = new Vector2[4];
		Vector2 max = rect.max;
		vertices[0] = rect.min;
		vertices[1] = new Vector2(max.x, vertices[0].y);
		vertices[2] = max;
		vertices[3] = new Vector2(vertices[0].x, max.y);
		return vertices;
	}
	public static IEnumerable<Vector2> GetVerticesEnumerable(this Rect rect) {
		Vector2 min = rect.min;
		Vector2 max = rect.max;
		yield return min;
		yield return new Vector2(max.x, min.y);
		yield return max;
		yield return new Vector2(min.x, max.y);
	}

    public static float GetAspect (this Rect rect) {
        return rect.size.x/rect.size.y;
    }
}