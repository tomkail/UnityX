using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class CameraShotGeneratorTools {
	const int numOrthographicIterations = 10;

	#region Point Cloud
	
	public static List<Vector3> GetPointCloudFromTargets (params GameObject[] targets) {
		return GetPointCloudFromTargetList(targets);
	}
	
	public static List<Vector3> GetPointCloudFromTargetList (IList<GameObject> targets) {
		List<Vector3> pointCloud = new List<Vector3>();
		if(targets == null || targets.Count == 0) {
			Debug.LogWarning ("Point cloud could not be created because Targets list is null or empty!");
		} else {
			for (int i = 0; i < targets.Count; i++) {
				if(targets[i] == null) continue;
				pointCloud.AddRange(GetVerticesFromGameObject(targets[i]));
			}
		}
		return pointCloud;
	}
	
	/// <summary>
	/// Gets the vertices from gameObject using the best method that can be determined from the components of the gameObject.
	/// </summary>
	/// <returns>The vertices from GameObject.</returns>
	/// <param name="meshRenderer">Mesh renderer.</param>
	public static Vector3[] GetVerticesFromGameObject (GameObject go) {
		return TryGetVerticesFromMeshFilter(go) ?? TryGetVerticesFromSpriteRenderer(go) ?? GetVerticesFromTransform(go.transform);
	}
	
	private static Vector3[] TryGetVerticesFromMeshFilter (GameObject go) {
		MeshFilter meshFilter = go.GetComponent<MeshFilter>();
		if(meshFilter != null && meshFilter.sharedMesh != null) return GetVerticesFromMeshFilter(meshFilter);
		else return null;
	}
	
	/// <summary>
	/// Gets the vertices of a mesh filter.
	/// </summary>
	/// <returns>The vertices from mesh filter.</returns>
	/// <param name="meshFilter">Mesh filter.</param>
	public static Vector3[] GetVerticesFromMeshFilter (MeshFilter meshFilter) {
		Vector3[] localVertices = meshFilter.sharedMesh.vertices;
		for(int i = 0; i < localVertices.Length; i++)
			localVertices[i] = meshFilter.transform.TransformPoint(localVertices[i]);
		return localVertices;
	}
	
	private static Vector3[] TryGetVerticesFromSpriteRenderer (GameObject go) {
		SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
		if(spriteRenderer != null && spriteRenderer.enabled && spriteRenderer.sprite != null) return GetVerticesFromSpriteRenderer(spriteRenderer);	
		else return null;
	}
	
	/// <summary>
	/// Gets the vertices from sprite renderer.
	/// Note that Unity gives sprite renderers a depth size of 0.2f by default which this function removes.
	/// </summary>
	/// <returns>The vertices from sprite renderer.</returns>
	/// <param name="spriteRenderer">Sprite renderer.</param>
	public static Vector3[] GetVerticesFromSpriteRenderer (SpriteRenderer spriteRenderer) {
		Bounds bounds = spriteRenderer.sprite.bounds;
		bounds.extents = new Vector3(bounds.extents.x, bounds.extents.y, 0);
		Vector3[] localVertices = GetVerticesFromBounds(bounds);
		for(int i = 0; i < localVertices.Length; i++)
			localVertices[i] = spriteRenderer.transform.TransformPoint(localVertices[i]);
		return localVertices;
	}

	public static Vector3[] GetVerticesFromBounds(Bounds bounds) {
		var min = bounds.min;
		var max = bounds.max;
		return new Vector3[8]{min, max, new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, min.z), new Vector3(max.x, min.y, min.z), new Vector3(min.x, max.y, max.z), new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, min.z)};
	}

	/// <summary>
	/// Creates vertices of edges from the transform data.
	/// More accurate than Renderer.bounds, as Bounds remove rotation data.
	/// </summary>
	/// <returns>The bounds.</returns>
	public static Vector3[] GetVerticesFromTransform (Transform transform) {
		Vector3 halfLocalScale = 	transform.localScale * 0.5f;
		Vector3 leftTopFront = 		transform.position + transform.rotation * new Vector3(-halfLocalScale.x, -halfLocalScale.y, halfLocalScale.z);
		Vector3 rightTopFront = 	transform.position + transform.rotation * new Vector3(halfLocalScale.x, -halfLocalScale.y, halfLocalScale.z);
		Vector3 leftTopBack = 		transform.position + transform.rotation * new Vector3(-halfLocalScale.x, -halfLocalScale.y, -halfLocalScale.z);
		Vector3 rightTopBack = 		transform.position + transform.rotation * new Vector3(halfLocalScale.x, -halfLocalScale.y, -halfLocalScale.z);
		Vector3 leftBottomFront = 	transform.position + transform.rotation * new Vector3(-halfLocalScale.x, halfLocalScale.y, halfLocalScale.z);
		Vector3 rightBottomFront = 	transform.position + transform.rotation * new Vector3(halfLocalScale.x, halfLocalScale.y, halfLocalScale.z);
		Vector3 leftBottomBack = 	transform.position + transform.rotation * new Vector3(-halfLocalScale.x, halfLocalScale.y, -halfLocalScale.z);
		Vector3 rightBottomBack = 	transform.position + transform.rotation * new Vector3(halfLocalScale.x, halfLocalScale.y, -halfLocalScale.z);
		return new Vector3[8]{leftTopFront, rightTopFront, leftTopBack, rightTopBack, leftBottomFront, rightBottomFront, leftBottomBack, rightBottomBack};
	}
	
	/// <summary>
	/// Gets a rect in viewport space from specified camera and point cloud.
	/// Quite expensive. TODO try to optimize the point cloud a bit manually before creating the viewport points.
	/// </summary>
	/// <returns>The viewport rect from point cloud.</returns>
	/// <param name="camera">Camera.</param>
	/// <param name="pointCloud">Point cloud.</param>
	public static Rect GetViewportRectFromPointCloud (SerializableCamera camera, IList<Vector3> pointCloud) {
		// Convert the world points to viewport space.
		Vector2[] viewportSpaceTargetPoints = camera.WorldToViewportPoints(pointCloud).Select(x => (Vector2)x).ToArray();
		// A camera shot frames a rectangle with flat lines. If we ever needed the rectangle to "rotate", we would switch this Rect to a Vector2[]
		return CreateEncapsulating(viewportSpaceTargetPoints);
	}
	
	#endregion
	
	
	#region Camera Framing

	/// <summary>
	/// Frame this shot in the specified cinematicCamera.
	/// </summary>
	public static SerializableCamera CreateCameraShot(CameraShotGeneratorProperties shotGeneratorProperties) {
		var camera = SerializableCamera.identity;
		CreateCameraShot(shotGeneratorProperties, ref camera);
		return camera;
	}

	static bool CreateCameraShot (CameraShotGeneratorProperties shotGeneratorProperties, ref SerializableCamera camera) {
		return CreateCameraShot(shotGeneratorProperties, ref camera, out float distance);
	}
	
	static bool CreateCameraShot (CameraShotGeneratorProperties shotGeneratorProperties, ref SerializableCamera camera, out float distanceFromTarget) {
		distanceFromTarget = 0;
		
		if(!shotGeneratorProperties.isValid) {
			shotGeneratorProperties.rotation = Quaternion.identity;
			if(!shotGeneratorProperties.isValid) {
				return false;
			}
			Debug.LogError("Shot generator properties are invalid. Using default values.");
		}
		
		camera.transform.position = Vector3.zero;
		camera.rect = shotGeneratorProperties.viewportRect;
		// camera.useCustomScreen = true;
		// 1f/Main.Instance.viewableCameraViewportArea.GetScreenRect().GetAspect()
		// var customScreenSize = ScaleToContainerUtils.Resize(new Vector2(SerializableCamera.ScreenParams.gameScreenWidth, SerializableCamera.ScreenParams.gameScreenHeight), Main.Instance.viewableCameraViewportArea.GetScreenRect().GetAspect(), ScaleToContainerUtils.ScalingMode.AspectFitWidthOnly);
		// var heightScale = SerializableCamera.ScreenParams.gameScreenHeight / Main.Instance.viewableCameraViewportArea.GetScreenRect().height;
		// Debug.Log(heightScale);
		// camera.customScreenParams = new SerializableCamera.ScreenParams(SerializableCamera.ScreenParams.gameScreenWidth, SerializableCamera.ScreenParams.gameScreenHeight * heightScale);
		// Debug.Log(camera.customScreenParams.width + " " + camera.customScreenParams.height);
		// if(camera.rect.width == 0 || camera.rect.height == 0) {
		// 	Debug.LogError("Camera rect "+camera.rect+" is invalid");
		// 	return false;
		// }
		if(camera.farClipPlane <= camera.nearClipPlane) {
			Debug.LogWarning("Camera far clip plane "+camera.farClipPlane+" is invalid");
			return false;
		}
		camera.fieldOfView = shotGeneratorProperties.fieldOfView;
		camera.transform.rotation = shotGeneratorProperties.rotation;
		camera.orthographic = shotGeneratorProperties.orthographic;
		
		
		// Frame the point cloud as if through an orthograph camera. 
		// This works nicely, but isn't as close as we'd like, so is actually used to set us up for the next step, where the stability of starting properties of the camera are important.
		FrameOrthographic(ref camera, shotGeneratorProperties);
		

		if(!shotGeneratorProperties.orthographic) {
			Vector3 closestTargetInDirection = ClosestTargetInDirection(shotGeneratorProperties.rotation * Vector3.forward, shotGeneratorProperties.pointCloud);
			FramePerspectiveOrthographicPass(ref camera, closestTargetInDirection, shotGeneratorProperties);
			// Iterate over the perspective version. The more iterations, the closer the item will hug the frame. I find that 10 is near enough to perfect.
			// The more irregular the dimentions of your object, and the more the shot shows this off (a very long object at an angle, for example), the more iterations you want.
			// Iterations hone in less and less with each pass, somewhat exponentially.
			// This isn't limitation isn't perfect in all situations, but it's pretty damned good, and should suit us fine.
			for(int i = 0; i < numOrthographicIterations; i++) {
				FramePerspective(ref camera, closestTargetInDirection, shotGeneratorProperties, out distanceFromTarget);
			}
		}
		// Offset the shot in viewport space.
		// OffsetShot(ref camera, distanceFromTarget, shotGeneratorProperties.viewportOffset);
		
		return true;
	}


	public static void FrameOrthographic (ref SerializableCamera camera, CameraShotGeneratorProperties shotGeneratorProperties) {
		Rect r = GetPointCloudRectRelativeToDirection(shotGeneratorProperties.pointCloud, shotGeneratorProperties.rotation * Vector3.forward);
		camera.transform.position = r.center;
		float closestPointCameraDistance = shotGeneratorProperties.pointCloud.Min(x => DistanceInDirection(x, shotGeneratorProperties.rotation * Vector3.forward));
		camera.transform.Translate (Vector3.forward * closestPointCameraDistance - Vector3.forward * shotGeneratorProperties.orthographicDistanceFromNearestTarget, Space.Self);
		r.size /= shotGeneratorProperties.framingRect.size;
		camera.orthographicSize = CameraX.CalculateOrthographicSize(camera.aspect, r.size, shotGeneratorProperties.scalingMode) * (1f/shotGeneratorProperties.zoom);
		camera.transform.Translate (new Vector2(camera.aspect, 1) * (Vector2.one * 0.5f-shotGeneratorProperties.framingRect.center) * (camera.orthographicSize * 2), Space.Self);
	}
	
	// Before framing perspective, we do a half-decent best fit using this function 
	public static void FramePerspectiveOrthographicPass(ref SerializableCamera camera, Vector3 closestTargetInDirection, CameraShotGeneratorProperties shotGeneratorProperties) {
		BoundingSphere boundingSphere = new BoundingSphere();
		boundingSphere.CreateFromPoints(shotGeneratorProperties.pointCloud);
		// This is to avoid NaN issues!
		if(boundingSphere.radius < 0.0001f) boundingSphere.radius = 1;

		// The distance between the closest point in the target direction and the center of the bounding sphere.
		float distanceOffset = DistanceInDirection(boundingSphere.center - closestTargetInDirection, shotGeneratorProperties.rotation * Vector3.forward);
		
		Rect r = GetPointCloudRectRelativeToDirection(shotGeneratorProperties.pointCloud, shotGeneratorProperties.rotation * Vector3.forward);
		float distanceFromTarget = GetDistanceFromTarget(camera, shotGeneratorProperties, r.size);
		camera.transform.position = boundingSphere.center;
		camera.transform.Translate (-Vector3.forward * distanceFromTarget, Space.Self);
		camera.transform.Translate (-Vector3.forward * distanceOffset, Space.Self);
	}
	private static void FramePerspective (ref SerializableCamera camera, Vector3 closestTargetInDirection, CameraShotGeneratorProperties shotGeneratorProperties, out float distanceFromTarget) {
		float closestPointCameraDistance = DistanceInDirection(closestTargetInDirection - camera.transform.position, camera.transform.forward);
		Vector2[] viewportRectVertices = GetRectVertices(GetViewportRectFromPointCloud(camera, shotGeneratorProperties.pointCloud));
		// We know that the contents at [0] and [2] are the min and max respectively from the earlier GetEdges() call that these vertices are derived from.
		Vector2 viewportRectCenter = Vector2.Lerp(viewportRectVertices[0], viewportRectVertices[2], 0.5f);
		distanceFromTarget = GetDistanceFromTarget(camera, shotGeneratorProperties, closestPointCameraDistance, viewportRectVertices);
		Vector3 worldRectCenter = camera.ViewportToWorldPoint(new Vector3(viewportRectCenter.x, viewportRectCenter.y, closestPointCameraDistance));
		camera.transform.position = worldRectCenter;
		camera.transform.Translate (-Vector3.forward * distanceFromTarget, Space.Self);
	}
	
	private static void OffsetShot (ref SerializableCamera camera, float distanceFromTarget, Vector2 offset) {
		Vector3 worldSpaceScale = camera.ViewportToWorldPoint(new Vector3(1,1,distanceFromTarget)) - camera.ViewportToWorldPoint(new Vector3(0,0,distanceFromTarget));
		Vector2 worldSpaceScreenSize = camera.transform.InverseTransformDirection(worldSpaceScale);
		Vector2 framePositionOffset = Vector3.Scale(worldSpaceScreenSize, -offset);
		camera.transform.Translate (framePositionOffset);
	}
	
	public static float GetDistanceFromTarget (SerializableCamera camera, CameraShotGeneratorProperties shotGeneratorProperties, float closestPointCameraDistance, Vector2[] viewportRectVertices) {
		Vector2[] flatTargetRectVertices = GetFlattenedTargetRectVertices(camera, closestPointCameraDistance, viewportRectVertices);
		Rect localSpaceTargetPointBounds = CreateEncapsulating(flatTargetRectVertices);
		return GetDistanceFromTarget(camera, shotGeneratorProperties, localSpaceTargetPointBounds.size);
	}

	public static float GetDistanceFromTarget (SerializableCamera camera, CameraShotGeneratorProperties shotGeneratorProperties, Vector2 targetRectSize) {
		float distanceFromWidth = 0;
		if(shotGeneratorProperties.scalingMode is CameraX.ScalingMode.AspectFit or CameraX.ScalingMode.AspectFitWidthOnly) {
			if(camera.orthographic) distanceFromWidth = targetRectSize.x;
			else distanceFromWidth = camera.GetDistanceAtFrustrumWidth(targetRectSize.x * (1f/shotGeneratorProperties.zoom));
		}
		
		float distanceFromHeight = 0;
		if(shotGeneratorProperties.scalingMode is CameraX.ScalingMode.AspectFit or CameraX.ScalingMode.AspectFitHeightOnly) {
			if(camera.orthographic) distanceFromHeight = targetRectSize.y;
			else distanceFromHeight = camera.GetDistanceAtFrustrumHeight(targetRectSize.y * (1f/shotGeneratorProperties.zoom));
		}
		Debug.LogWarning("This function has not been upgraded to support fit modes fully.");
		return Mathf.Max(distanceFromWidth, distanceFromHeight);
	}

	public static Vector3 ClosestTargetInDirection (Vector3 forwardDirection, List<Vector3> points) {
		//		return Vector3X.Average(points);
		int index = 0;
		float smallest = DistanceInDirection(points[index], forwardDirection);
		for(int i = 1; i < points.Count; i++){
			float distance = DistanceInDirection(points[i], forwardDirection);
			if (distance < smallest) {
				smallest = distance;
				index = i;
			}
		}
		return points[index];
	}

	public static float DistanceInDirection(Vector3 vectorToPoint, Vector3 direction) {
		return Vector3.Dot(direction.normalized, vectorToPoint);
	}

	public static Rect GetPointCloudRectRelativeToDirection (IList<Vector3> pointCloud, Vector3 forward) {
		Matrix4x4 inverseTransformMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(forward), Vector3.one).inverse;
		Vector2[] localFlattenedPoints = new Vector2[pointCloud.Count];
		for(int i = 0; i < pointCloud.Count; i++){
			Vector3 localPoint = inverseTransformMatrix.MultiplyPoint(pointCloud[i]);
			localFlattenedPoints[i] = localPoint;
		}
		return CreateEncapsulating(localFlattenedPoints);
	}
	
	private static Vector2[] GetFlattenedTargetRectVertices (SerializableCamera camera, float closestPointCameraDistance, Vector2[] viewportRectVertices) {
		Vector2[] flatTargetRectVertices = new Vector2[viewportRectVertices.Length];
		for(int i = 0; i < flatTargetRectVertices.Length; i++) {
			var point3D = camera.ViewportToWorldPoint(new Vector3(viewportRectVertices[i].x, viewportRectVertices[i].y, closestPointCameraDistance));
			// "Flatten" (remove depth) the points by bringing them into the local space of the camera
			flatTargetRectVertices[i] = camera.transform.InverseTransformDirection(point3D);
		}
		return flatTargetRectVertices;
	}
	
	public static Rect CreateEncapsulating (params Vector2[] vectors) {
		Rect rect = new Rect(vectors[0].x, vectors[0].y, 0, 0);
		for(int i = 1; i < vectors.Length; i++) {
			var xMin = Mathf.Min (rect.xMin, vectors[i].x);
			var xMax = Mathf.Max (rect.xMax, vectors[i].x);
			var yMin = Mathf.Min (rect.yMin, vectors[i].y);
			var yMax = Mathf.Max (rect.yMax, vectors[i].y);
			rect = Rect.MinMaxRect (xMin, yMin, xMax, yMax);
		}
		return rect;
	}
	
	public static Vector2[] GetRectVertices(Rect rect) {
		Vector2[] vertices = new Vector2[4];
		Vector2 max = rect.max;
		vertices[0] = rect.min;
		vertices[1] = new Vector2(max.x, vertices[0].y);
		vertices[2] = max;
		vertices[3] = new Vector2(vertices[0].x, max.y);
		return vertices;
	}
	#endregion
}