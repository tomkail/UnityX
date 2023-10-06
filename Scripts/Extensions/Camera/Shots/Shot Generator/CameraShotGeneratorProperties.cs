using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CameraShotGeneratorProperties {
	public Rect viewportRect = new Rect(0,0,1,1);
	public List<Vector3> pointCloud = new List<Vector3>();
	public Quaternion rotation = Quaternion.identity;

	public bool orthographic = false;
	public float fieldOfView = 60;
	
	public float zoom = 1f;
	
	public bool fitHorizontally = true;
	public bool fitVertically = true;
	
	public bool isValid {
		get {
			if(pointCloud.Count == 0) return false;
			if(!fitHorizontally && !fitVertically) return false;
			if(fieldOfView <= 0) return false;
			if(rotation.x == 0 && rotation.y == 0 && rotation.z == 0 && rotation.w == 0) return false;
			return true;
		}
	}
	
	public CameraShotGeneratorProperties () {}
	
	public SerializableCamera ToShot () {
		return ToShot(Camera.main);
	}

	public SerializableCamera ToShot (Camera camera) {
		var sCamera = new SerializableCamera(camera);
		sCamera.orthographic = false;
		CameraShotGeneratorTools.CreateCameraShot(this, ref sCamera);
		return sCamera;
	}
	
	public override string ToString () {
		return string.Format ("[CameraShotGeneratorProperties: fieldOfView={0}, zoom={1}, rotation={2}, fitHorizontally={3}, fitVertically={4}, isValid={5}]", fieldOfView, zoom, rotation, fitHorizontally, fitVertically, isValid);
	}
}
