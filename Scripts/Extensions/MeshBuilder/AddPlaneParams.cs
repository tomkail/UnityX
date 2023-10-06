using UnityEngine;

namespace UnityX.MeshBuilder
{
    public struct AddPlaneParams {
		public bool front;
		public bool back;
		public Vector3 topLeft;
		public Vector3 topRight;
		public Vector3 bottomRight;
		public Vector3 bottomLeft;
		public Vector2 uvTopLeft;
		public Vector2 uvTopRight;
		public Vector2 uvBottomRight;
		public Vector2 uvBottomLeft;
		public Color colorTopLeft;
		public Color colorTopRight;
		public Color colorBottomRight;
		public Color colorBottomLeft;

		public static AddPlaneParams standard {
			get {
				AddPlaneParams _standard = new AddPlaneParams();
				_standard.front = true;
				
                _standard.topLeft = new Vector3(-0.5f,0.5f,0f);
				_standard.topRight = new Vector3(0.5f,0.5f,0f);
				_standard.bottomRight = new Vector3(0.5f,-0.5f,0f);
				_standard.bottomLeft = new Vector3(-0.5f,-0.5f,0f);

				_standard.uvTopLeft = new Vector2(0,1);
				_standard.uvTopRight = new Vector2(1,1);
				_standard.uvBottomRight = new Vector2(1,0);
				_standard.uvBottomLeft = new Vector2(0,0);

				_standard.colorTopLeft = Color.white;
				_standard.colorTopRight = Color.white;
				_standard.colorBottomRight = Color.white;
				_standard.colorBottomLeft = Color.white;

				return _standard;
			}
		}

		// public AddPlaneParams () {
		// 	Vector2 uvTopLeft = new Vector2(0,1);
		// Vector2 uvTopRight = new Vector2(1,1);
		// Vector2 uvBottomRight = new Vector2(1,0);
		// Vector2 uvBottomLeft = new Vector2(0,0);
		// }
	}
}