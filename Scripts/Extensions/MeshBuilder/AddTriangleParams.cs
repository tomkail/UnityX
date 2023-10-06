using UnityEngine;

namespace UnityX.MeshBuilder
{
    public struct AddTriangleParams {
		public bool front;
		public bool back;
		public Vector3 topLeft;
		public Vector3 topRight;
		public Vector3 bottom;
		public Vector2 uvTopLeft;
		public Vector2 uvTopRight;
		public Vector2 uvBottom;

		public Color colorTopLeft;
		public Color colorTopRight;
		public Color colorBottom;

		public static AddTriangleParams standard {
			get {
				AddTriangleParams _standard = new AddTriangleParams();
				_standard.front = true;
				_standard.uvTopLeft = new Vector2(0,1);
				_standard.uvTopRight = new Vector2(1,1);
				_standard.uvBottom = new Vector2(0.5f,0);

				_standard.colorTopLeft = Color.white;
				_standard.colorTopRight = Color.white;
				_standard.colorBottom = Color.white;

				return _standard;
			}
		}
	}
}