using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CameraShotGeneratorProperties {
	public static CameraShotGeneratorProperties DefaultPerspective =>
		new() {
			viewportRect = new Rect(0, 0, 1, 1),
			pointCloud = new List<Vector3>(),
			rotation = Quaternion.identity,
			orthographic = false,
			orthographicDistanceFromNearestTarget = 1,
			fieldOfView = 60,
			zoom = 1,
			scalingMode = CameraX.ScalingMode.AspectFit,
			framingRect = new Rect(0,0,1,1)
		};
	
	public static CameraShotGeneratorProperties DefaultOrthographic =>
		new() {
			viewportRect = new Rect(0, 0, 1, 1),
			pointCloud = new List<Vector3>(),
			rotation = Quaternion.identity,
			orthographic = true,
			orthographicDistanceFromNearestTarget = 1,
			fieldOfView = 60,
			zoom = 1,
			scalingMode = CameraX.ScalingMode.AspectFit,
			framingRect = new Rect(0,0,1,1)
		};
	
	public static CameraShotGeneratorProperties FromCamera (Camera camera) {
		return new CameraShotGeneratorProperties {
			viewportRect = camera.rect,
			pointCloud = new List<Vector3>(),
			rotation = camera.transform.rotation,
			orthographic = camera.orthographic,
			orthographicDistanceFromNearestTarget = 1,
			fieldOfView = camera.fieldOfView,
			zoom = 1,
			scalingMode = CameraX.ScalingMode.AspectFit,
			framingRect = new Rect(0,0,1,1)
		};
	}
	
	public Rect viewportRect;
	public List<Vector3> pointCloud;
	public Quaternion rotation;

	public bool orthographic;
	// How far back the camera should physically be from the nearest target.
	// Doesn't affect how the image actually looks, unless negative in which case the camera will be inside the target.
	public float orthographicDistanceFromNearestTarget;
	
	public float fieldOfView;
	
	public float zoom;

	// Determines how the rect created by the point cloud is fit inside the camera frame.
	public CameraX.ScalingMode scalingMode;
	
	// Determines where the rect point cloud is framed.
	// Uses normalized space relative to the camera rect, ie:
	// (0,0,1,1) fills the viewport, (0,0,0.5,0.5) frames the object in the bottom left quadrant.
	public Rect framingRect;
	
	public bool isValid {
		get {
			if(pointCloud.Count == 0) return false;
			if(!orthographic && fieldOfView <= 0) return false;
			if(rotation is {x: 0, y: 0, z: 0, w: 0}) return false;
			return true;
		}
	}

	public SerializableCamera ToShot () {
		return CameraShotGeneratorTools.CreateCameraShot(this);
	}
	
	public override string ToString () {
		return $"[CameraShotGeneratorProperties: fieldOfView={fieldOfView}, zoom={zoom}, rotation={rotation}, scalingMode={scalingMode}, framingRect={framingRect}, isValid={isValid}]";
	}
}
