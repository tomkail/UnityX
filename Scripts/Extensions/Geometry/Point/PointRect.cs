using UnityEngine;
using System.Collections.Generic;

//namespace UnityX {
//	namespace Geometry {
		
		[System.Serializable]
		public struct PointRect {
			public int x, y, width, height;
			
			public int xMax {
				get {
					return width + x;
				} set {
					width = value - x;
				}
			}
			
			public int xMin {
				get {
					return x;
				} set {
					x = value;
					width = xMax - x;
				}
			}
			
			public int yMax {
				get {
					return height + y;
				}
				set {
					height = value - y;
				}
			}
			
			public int yMin {
				get {
					return y;
				} set {
					y = value;
					height = yMax - y;
				}
			}
			
			public Point min {
				get {
					return new Point(xMin, yMin);
				} set {
					xMin = value.x;
					yMin = value.y;
				}
			}

			public Point max {
				get {
					return new Point(xMax, yMax);
				} set {
					xMax = value.x + width;
					yMax = value.y + height;
				}
			}

			public Point position {
				get {
					return new Point(x, y);
				} set {
					x = value.x;
					y = value.y;
				}
			}
		
			public Point size {
				get {
					return new Point(width, height);
				} set {
					width = value.x;
					height = value.y;
				}
			}
			
            public int area {
				get {
					return width * height;
				}
			}

			public PointRect(int _x, int _y, int _width, int _height) {
				x = _x;
				y = _y;
				width = _width;
				height = _height;
			}
		
			public PointRect(float _x, float _y, float _width, float _height) : this (Mathf.RoundToInt(_x), Mathf.RoundToInt(_y), Mathf.RoundToInt(_width), Mathf.RoundToInt(_height)) {}
			
			/// <summary>
			/// Initializes a new instance of the <see cref="UnityX.Geometry.PointRect"/> class.
			/// </summary>
			/// <param name="_position">The position of the top-left corner.</param>
			/// <param name="_size">The width and height.</param>
			public PointRect(Point _position, Point _size) : this (_position.x, _position.y, _size.x, _size.y) {}
		
			public PointRect (int[] xywh) : this (xywh[0], xywh[1], xywh[2], xywh[3]) {}

			public static PointRect CreateEncapsulating (params Point[] points) {
				PointRect rect = new PointRect(points[0].x, points[0].y, 0, 0);
				for(int i = 1; i < points.Length; i++)
					rect.Encapsulate(points[i]);
				return rect;
			}

			public static PointRect CreateEncapsulating (IEnumerable<Point> points) {
				var enumerator = points.GetEnumerator();
                enumerator.MoveNext();
                PointRect rect = new PointRect(enumerator.Current.x, enumerator.Current.y, 0, 0);
                while(enumerator.MoveNext())
                    rect.Encapsulate(enumerator.Current);
                return rect;
			}

			public static PointRect MinMaxRect (Point topLeft, Point bottomRight) {
				return new PointRect(topLeft.x, topLeft.y, bottomRight.x - topLeft.x, bottomRight.y - topLeft.y);
			}

			public void Set(int _x, int _y, int _width, int _height) {
				x = _x;
				y = _y;
				width = _width;
				height = _height;
			}

			public IEnumerable<Point> GetPoints() {
				for(int y = 0; y < height; y++) {
					for(int x = 0; x < width; x++) {
						Point gridPoint = new Point(x, y);
						yield return gridPoint + position;
					}
				}
			}
		
			public static PointRect FromRect(Rect rect) {
				return new PointRect(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
			}
		
			public static Rect ToRect(PointRect pointRect) {
				return new Rect(pointRect.x, pointRect.y, pointRect.width, pointRect.height);
			}

			public override string ToString() {
				return "X: " + x + " Y: " + y + " W: " + width + " H: " + height;
			}

			public bool Contains (Point point) {
				return point.x >= this.xMin && point.x < this.xMax && point.y >= this.yMin && point.y < this.yMax;
			}
			
			public static PointRect Add(PointRect left, PointRect right){
				return new PointRect(left.x+right.x, left.y+right.y, left.width+right.width, left.height+right.height);
			}
		
		
			public static PointRect Subtract(PointRect left, PointRect right){
				return new PointRect(left.x-right.x, left.y-right.y, left.width-right.width, left.height-right.height);
			}
		
		
			public static PointRect Multiply(PointRect left, PointRect right){
				return new PointRect(left.x*right.x, left.y*right.y, left.width*right.width, left.height*right.height);
			}
			
			
			public static PointRect Divide(PointRect left, PointRect right) {
				return new PointRect(left.x/right.x, left.y/right.y, left.width/right.width, left.height/right.height);
			}

			public void Encapsulate(Point point) {
				PointRect minMaxRect = MinMaxRect (new Point(Mathf.Min(min.x, point.x), Mathf.Min(min.y, point.y)), new Point(Mathf.Max(max.x, point.x), Mathf.Max(max.y, point.y)));
				Set(minMaxRect.x, minMaxRect.y, minMaxRect.width, minMaxRect.height);
			}

			/// <summary>
			/// Clamps a point inside the rect.
			/// </summary>
			/// <param name="r">The red component.</param>
			/// <param name="point">Point.</param>
			public static Point ClampPoint(PointRect rect, Point point) {
				point.x = Mathf.Clamp(point.x, rect.min.x, rect.max.x);
				point.y = Mathf.Clamp(point.y, rect.min.y, rect.max.y);
				return point;
			}
			
			public static PointRect Clamp(PointRect r, PointRect container) {
				PointRect ret = new PointRect(0,0,0,0);
				ret.x = Mathf.Clamp(Mathf.Max(r.x, container.x), 0, int.MaxValue);
				ret.y = Mathf.Clamp(Mathf.Max(r.y, container.y), 0, int.MaxValue);
				ret.width = Mathf.Clamp(Mathf.Min(r.x + r.width, container.x + container.width) - ret.x, 0, int.MaxValue);
				ret.height = Mathf.Clamp(Mathf.Min(r.y + r.height, container.y + container.height) - ret.y, 0, int.MaxValue);
				
				int offsetX = Mathf.Clamp(r.x - Mathf.Max(ret.width, container.width), 0, int.MaxValue);
				int offsetY = Mathf.Clamp(r.y - Mathf.Max(ret.height, container.height), 0, int.MaxValue);
				ret.x -= offsetX;
				ret.y -= offsetY;
				return ret;
			}
			
			public static Rect ClampKeepSize(PointRect r, PointRect container) {
				Rect ret = new Rect();
				ret.x = Mathf.Clamp(Mathf.Max(r.x, container.x), 0, int.MaxValue);
				ret.y = Mathf.Clamp(Mathf.Max(r.y, container.y), 0, int.MaxValue);
				ret.width = Mathf.Min(r.width, container.width);
				ret.height = Mathf.Min(r.height, container.height);
				
				int offsetX = Mathf.Clamp((r.x + r.width) - (container.x + container.width), 0, int.MaxValue);
				int offsetY = Mathf.Clamp((r.y + r.height) - (container.y + container.height), 0, int.MaxValue);
				ret.x -= Mathf.Min(offsetX, ret.x);
				ret.y -= Mathf.Min(offsetY, ret.y);
				
				return ret;
			}

			public static Point[] Corners (PointRect r) {
				return new Point[4] {r.TopLeft(), r.TopRight(), r.BottomRight(), r.BottomLeft()};
			}

			public Point TopLeft(){
				return min;
			}
			
			public Point TopRight(){
				return new Point(x+width, y);
			}
			
			public Point BottomLeft(){
				return new Point(x, y+height);
			}
			
			public Point BottomRight(){
				return max;
			}

			public override bool Equals(System.Object obj) {
				// If parameter is null return false.
				if (obj == null) {
					return false;
				}
		
				// If parameter cannot be cast to PointRect return false.
				PointRect p = (PointRect)obj;
				if ((System.Object)p == null) {
					return false;
				}
		
				// Return true if the fields match:
				return (x == p.x) && (y == p.y) && (width == p.width) && (height == p.height);
			}
		
			public bool Equals(PointRect p) {
				// If parameter is null return false:
				if ((object)p == null) {
					return false;
				}
		
				// Return true if the fields match:
				return (x == p.x) && (y == p.y) && (y == p.width) && (y == p.height);
			}
		
			public override int GetHashCode() {
				unchecked // Overflow is fine, just wrap
				{
					int hash = 27;
					hash = hash * 31 + x.GetHashCode();
					hash = hash * 31 + y.GetHashCode();
					hash = hash * 31 + width.GetHashCode();
					hash = hash * 31 + height.GetHashCode();
					return hash;
				}
			}
		
			public static bool operator == (PointRect left, PointRect right) {
				if (System.Object.ReferenceEquals(left, right))
				{
					return true;
				}
		
				// If one is null, but not both, return false.
				if (((object)left == null) || ((object)right == null))
				{
					return false;
				}
				if(left.x == right.x && left.y == right.y && left.width == right.width && left.height == right.height)return true;
				return false;
			}
		
			public static bool operator != (PointRect left, PointRect right) {
				return !(left == right);
			}
		
			public static PointRect operator +(PointRect left, PointRect right) {
				return Add(left, right);
			}
		
			public static PointRect operator -(PointRect left, PointRect right) {
				return Subtract(left, right);
			}
		
			public static PointRect operator *(PointRect left, PointRect right) {
				return Add(left, right);
			}
		
			public static PointRect operator /(PointRect left, PointRect right) {
				return Subtract(left, right);
			}
		
			public static implicit operator PointRect(Rect src) {
				return FromRect(src);
			}
			
			public static implicit operator Rect(PointRect src) {
				return PointRect.ToRect(src);
			}
		}
//	}
//}