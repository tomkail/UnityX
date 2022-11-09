using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Data for generating a camera shot.
/// </summary>
[System.Serializable]
public struct CameraProperties {
	public const float defaultDistance = 10;

	public static CameraProperties @default {
		get {
			return new CameraProperties(Vector3.zero);
		}
	}

	/// <summary>
	/// The various "axis" of control allowed by camera properties.
	/// </summary>
	[Flags]
	public enum CameraPropertiesAxis {
		None = 0,
		All = ~0,
		TargetPoint = 1 << 0,
		Axis = 1 << 1,
		Distance = 1 << 2,
		WorldPitch = 1 << 3,
		WorldYaw = 1 << 4,
		LocalPitch = 1 << 5,
		LocalYaw = 1 << 6,
		LocalRoll = 1 << 7,
		HorizontalViewportOffset = 1 << 8,
		VerticalViewportOffset = 1 << 9,
		FieldOfView = 1 << 10,
		Orthographic = 1 << 11,
		OrthographicSize = 1 << 12,
	}
	
	// The point the camera revolves around
	public Vector3 targetPoint;
	// The base axis that everything else is derived from. If you're building a setup for a game without a flat ground this can be useful.
	public Quaternion axis;
	// The distance from the target in forward direction
	public float distance;
	// X: Pitch Y: Yaw  (relative to axis)
	public Vector2 worldEulerAngles;
	// X: Pitch Y: Yaw Z: Roll
	public Vector3 localEulerAngles;
	public Vector2 viewportOffset;
	public float fieldOfView;
	public bool orthographic;
	public float orthographicSize;

	// Convenience accessor
	public float yaw {
		get {
			return worldEulerAngles.y;
		}
		set {
			worldEulerAngles.y = value;
		}
	}

	// Convenience accessor
	public float pitch {
		get {
			return worldEulerAngles.x;
		}
		set {
			worldEulerAngles.x = value;
		}
	}

	// Convenience accessor
	public float localYaw {
		get {
			return localEulerAngles.y;
		}
		set {
			localEulerAngles.y = value;
		}
	}

	// Convenience accessor
	public float localPitch {
		get {
			return localEulerAngles.x;
		}
		set {
			localEulerAngles.x = value;
		}
	}

	// Convenience accessor
	public float localRoll {
		get {
			return localEulerAngles.z;
		}
		set {
			localEulerAngles.z = value;
		}
	}

	// Convenience accessor
	public float horizontalViewportOffset {
		get {
			return viewportOffset.x;
		}
		set {
			viewportOffset.x = value;
		}
	}

	// Convenience accessor
	public float verticalViewportOffset {
		get {
			return viewportOffset.y;
		}
		set {
			viewportOffset.y = value;
		}
	}

	public Quaternion rotation {
		get {
			var eulerAngles = (Vector3)worldEulerAngles + localEulerAngles;
			if(eulerAngles == Vector3.zero) return axis;
			return axis * Quaternion.Euler(eulerAngles);
		}
	}

	public Vector3 forward {
		get {
			return rotation * Vector3.forward;
		}
	}

	public Vector3 right {
		get {
			return rotation * Vector3.right;
		}
	}

	public Vector3 up {
		get {
			return rotation * Vector3.up;
		}
	}



	/// <summary>
	/// Position of the camera before any viewport offset. Useful for setting
	/// the camera position directly rather than using pitch and yaw.
	/// </summary>
	public Vector3 basePosition {
		get {
			// Can't use forward since it includes localRotation
			var worldRotation = axis * Quaternion.Euler(worldEulerAngles);
			var offsetFromTarget = -distance * (worldRotation * Vector3.forward);
			return targetPoint + offsetFromTarget;
		}
		set {
			var baseToTarget = targetPoint - value;
			var direction = Quaternion.Inverse(axis) * baseToTarget;
			worldEulerAngles = GetPitchAndYaw(direction);
			distance = baseToTarget.magnitude;
		}
	}

	public CameraProperties (Vector3 targetPoint) {
		this.axis = Quaternion.identity;

		this.targetPoint = targetPoint;
		this.distance = defaultDistance;
		this.worldEulerAngles = Vector2.zero;
		this.localEulerAngles = Vector3.zero;
		this.viewportOffset = Vector2.zero;
		this.fieldOfView = SerializableCamera.defaultFieldOfView;
		this.orthographic = SerializableCamera.defaultOrthographic;
		this.orthographicSize = SerializableCamera.defaultOrthographicSize;
	}

	// Moves the target point, but translation in the the direction of the camera is handled using distance.
	// Handy if you have a fixed ground plane you don't want the targetPoint going, for example.
	public void TranslateUsingDistance (Plane floorPlane, Vector3 translation) {
		var cameraPropertiesWithOffsetTargetPoint = this;
		cameraPropertiesWithOffsetTargetPoint.targetPoint += translation;
		var distanceToFloor = floorPlane.GetDistanceToPointInDirection(cameraPropertiesWithOffsetTargetPoint.basePosition, cameraPropertiesWithOffsetTargetPoint.rotation * Vector3.forward);
		targetPoint = cameraPropertiesWithOffsetTargetPoint.basePosition + cameraPropertiesWithOffsetTargetPoint.rotation * Vector3.forward * distanceToFloor;
		distance = distanceToFloor;
		// Clamp it again to fix floating point errors
		targetPoint = floorPlane.ClosestPointOnPlane(targetPoint);
	}

	/// <summary>
	/// Final position of camera. Unfortunately you can't get this without knowing the aspect
	/// ratio of the camera you're using, since the viewport position is affected by this.
	/// If you don't care, you can use the simple basePosition property.
	/// </summary>
	public Vector3 PositionWithViewportAspectRatio(float aspect) {
		float halfHeight = 0f;
		if(orthographic) {
			halfHeight = orthographicSize * 2;
		} else {
			var distanceInDirection = distance;
			halfHeight = distanceInDirection * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad) * 2;
		}
		float halfWidth  = aspect * halfHeight;
		var viewportScaledOffset = new Vector3(halfWidth * viewportOffset.x, halfHeight * viewportOffset.y, 0.0f);

		var finalRotation = rotation;
		var localUp    = finalRotation * Vector3.up;
		var localRight = finalRotation * Vector3.right;
		var localOffset = viewportScaledOffset.x * localRight + viewportScaledOffset.y * localUp;
		return basePosition + localOffset;
	}

	public static CameraProperties OrbitingPoint (Vector3 targetPoint, Quaternion axis, Vector2 worldEulerAngles, float distance) {
		var cameraProperties = new CameraProperties();
		cameraProperties.axis = axis;
		cameraProperties.targetPoint = targetPoint;
		cameraProperties.worldEulerAngles = worldEulerAngles;
		cameraProperties.distance = distance;
		return cameraProperties;
	}

	public static CameraProperties FromTo (Vector3 originPoint, Vector3 targetPoint) {
		var cameraProperties = new CameraProperties();
		cameraProperties.axis = Quaternion.identity;
		cameraProperties.targetPoint = targetPoint;
		cameraProperties.basePosition = originPoint;
		return cameraProperties;
	}

	public static CameraProperties FromTo (Vector3 originPoint, Vector3 targetPoint, Quaternion axis) {
		var cameraProperties = new CameraProperties();
		cameraProperties.axis = axis;
		cameraProperties.targetPoint = targetPoint;
		cameraProperties.basePosition = originPoint;
		return cameraProperties;
	}
	
	public static CameraProperties Lerp (CameraProperties start, CameraProperties end, float lerp) {
		if(lerp <= 0) return start;
		else if(lerp >= 1) return end;
		return LerpUnclamped(start, end, lerp);
	}

	public static CameraProperties LerpUnclamped (CameraProperties start, CameraProperties end, float lerp) {
		float LerpAngleUnclamped (float a, float b, float t) {
			float delta = Mathf.Repeat((b-a), 360);
			if(delta > 180)
				delta -= 360;
			return a + delta * t;
		}

		CameraProperties properties = new CameraProperties();

		properties.axis = Quaternion.SlerpUnclamped(start.axis, end.axis, lerp);

		properties.targetPoint = Vector3.LerpUnclamped(start.targetPoint, end.targetPoint, lerp);
		properties.basePosition = Vector3.LerpUnclamped(start.basePosition, end.basePosition, lerp);
		// properties.distance = Mathf.LerpUnclamped(start.distance, end.distance, lerp);

		// properties.worldEulerAngles.x = LerpAngleUnclamped(start.worldEulerAngles.x, end.worldEulerAngles.x, lerp);
		// properties.worldEulerAngles.y = LerpAngleUnclamped(start.worldEulerAngles.y, end.worldEulerAngles.y, lerp);

		properties.localEulerAngles.x = LerpAngleUnclamped(start.localEulerAngles.x, end.localEulerAngles.x, lerp);
		properties.localEulerAngles.y = LerpAngleUnclamped(start.localEulerAngles.y, end.localEulerAngles.y, lerp);
		properties.localEulerAngles.z = LerpAngleUnclamped(start.localEulerAngles.z, end.localEulerAngles.z, lerp);

		properties.viewportOffset.x = Mathf.LerpUnclamped(start.viewportOffset.x, end.viewportOffset.x, lerp);
		properties.viewportOffset.y = Mathf.LerpUnclamped(start.viewportOffset.y, end.viewportOffset.y, lerp);

		properties.fieldOfView = Mathf.LerpUnclamped(start.fieldOfView, end.fieldOfView, lerp);
		
		properties.orthographic = lerp < 0.5 ? start.orthographic : end.orthographic;
		properties.orthographicSize = Mathf.LerpUnclamped(start.orthographicSize, end.orthographicSize, lerp);
		return properties;
	}
	
	// Used for smoothdamp
	[System.Serializable]
	public struct CameraPropertiesMaxSpeed {
		public float axis;
		public float targetPoint;
		public float distance;
		public Vector2 worldEulerAngles;
		public Vector3 localEulerAngles;
		public Vector2 viewportOffset;
		public float fieldOfView;
		public float orthographicSize;

		public static CameraPropertiesMaxSpeed Infinity {
			get {
				var maxSpeed = new CameraPropertiesMaxSpeed();
				maxSpeed.axis = Mathf.Infinity;
				maxSpeed.targetPoint = Mathf.Infinity;
				maxSpeed.distance = Mathf.Infinity;
				maxSpeed.worldEulerAngles = new Vector2(Mathf.Infinity, Mathf.Infinity);
				maxSpeed.localEulerAngles = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
				maxSpeed.viewportOffset = new Vector2(Mathf.Infinity, Mathf.Infinity);
				maxSpeed.fieldOfView = Mathf.Infinity;
				maxSpeed.orthographicSize = Mathf.Infinity;
				return maxSpeed;
			}
		}
	}
	public static CameraProperties SmoothDamp (CameraProperties start, CameraProperties end, ref CameraProperties velocity, float smoothTime, float deltaTime) {
		return SmoothDamp(start, end, ref velocity, smoothTime, CameraPropertiesMaxSpeed.Infinity, deltaTime);
	}
	public static CameraProperties SmoothDamp (CameraProperties start, CameraProperties end, ref CameraProperties velocity, float smoothTime, CameraPropertiesMaxSpeed maxSpeed, float deltaTime) {
		if(deltaTime <= 0) return start;
		CameraProperties properties = new CameraProperties();

		properties.axis = QuaternionSmoothDamp(start.axis, end.axis, ref velocity.axis, smoothTime, maxSpeed.axis, deltaTime);

		properties.targetPoint = Vector3.SmoothDamp(start.targetPoint, end.targetPoint, ref velocity.targetPoint, smoothTime, maxSpeed.targetPoint, deltaTime);
		properties.distance = Mathf.SmoothDamp(start.distance, end.distance, ref velocity.distance, smoothTime, maxSpeed.distance, deltaTime);
		
		properties.worldEulerAngles.x = Mathf.SmoothDampAngle(start.worldEulerAngles.x, end.worldEulerAngles.x, ref velocity.worldEulerAngles.x, smoothTime, maxSpeed.worldEulerAngles.x, deltaTime);
		properties.worldEulerAngles.y = Mathf.SmoothDampAngle(start.worldEulerAngles.y, end.worldEulerAngles.y, ref velocity.worldEulerAngles.y, smoothTime, maxSpeed.worldEulerAngles.y, deltaTime);
		
		properties.localEulerAngles.x = Mathf.SmoothDampAngle(start.localEulerAngles.x, end.localEulerAngles.x, ref velocity.localEulerAngles.x, smoothTime, maxSpeed.localEulerAngles.x, deltaTime);
		properties.localEulerAngles.y = Mathf.SmoothDampAngle(start.localEulerAngles.y, end.localEulerAngles.y, ref velocity.localEulerAngles.y, smoothTime, maxSpeed.localEulerAngles.y, deltaTime);
		properties.localEulerAngles.z = Mathf.SmoothDampAngle(start.localEulerAngles.z, end.localEulerAngles.z, ref velocity.localEulerAngles.z, smoothTime, maxSpeed.localEulerAngles.z, deltaTime);

		properties.viewportOffset.x = Mathf.SmoothDamp(start.viewportOffset.x, end.viewportOffset.x, ref velocity.viewportOffset.x, smoothTime, maxSpeed.viewportOffset.x, deltaTime);
		properties.viewportOffset.y = Mathf.SmoothDamp(start.viewportOffset.y, end.viewportOffset.y, ref velocity.viewportOffset.y, smoothTime, maxSpeed.viewportOffset.y, deltaTime);

		properties.fieldOfView = Mathf.SmoothDamp(start.fieldOfView, end.fieldOfView, ref velocity.fieldOfView, smoothTime, maxSpeed.fieldOfView, deltaTime);
		
		properties.orthographic = end.orthographic;
		properties.orthographicSize = Mathf.SmoothDamp(start.orthographicSize, end.orthographicSize, ref velocity.orthographicSize, smoothTime, maxSpeed.orthographicSize, deltaTime);
		return properties;
	}

	static Quaternion QuaternionSmoothDamp(Quaternion rot, Quaternion target, ref Quaternion currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
		if(deltaTime == 0) return rot;

		// account for double-cover
		var dot = Quaternion.Dot(rot, target);
		var sign = dot > 0f ? 1f : -1f;
		target.x *= sign;
		target.y *= sign;
		target.z *= sign;
		target.w *= sign;
		// smooth damp (nlerp approx)
		var Result = new Vector4(
			Mathf.SmoothDamp(rot.x, target.x, ref currentVelocity.x, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(rot.y, target.y, ref currentVelocity.y, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(rot.z, target.z, ref currentVelocity.z, smoothTime, maxSpeed, deltaTime),
			Mathf.SmoothDamp(rot.w, target.w, ref currentVelocity.w, smoothTime, maxSpeed, deltaTime)
		).normalized;
		// compute deriv
		var dtInv = 1f / deltaTime;
		currentVelocity.x = (Result.x - rot.x) * dtInv;
		currentVelocity.y = (Result.y - rot.y) * dtInv;
		currentVelocity.z = (Result.z - rot.z) * dtInv;
		currentVelocity.w = (Result.w - rot.w) * dtInv;
		return new Quaternion(Result.x, Result.y, Result.z, Result.w);
	}

	public static CameraProperties WeightedBlend(IEnumerable<CameraProperties> allProperties, IList<float> weights) {
		// Normalise weights so they add up to 1.0
		float totalWeight = weights.Sum();
		for(int i=0; i<weights.Count; i++)
			weights[i] = weights[i] / totalWeight;
		
		CameraProperties blended = new CameraProperties();

		blended.axis = WeightedBlends.WeightedBlend(allProperties, p => p.axis, weights);

		blended.targetPoint = WeightedBlends.WeightedBlend(allProperties, p => p.targetPoint, weights);
		blended.distance = WeightedBlends.WeightedBlend(allProperties, p => p.distance, weights);

		blended.worldEulerAngles.x = WeightedBlends.WeightedBlendAngle(allProperties, p => p.worldEulerAngles.x, weights);
		blended.worldEulerAngles.y = WeightedBlends.WeightedBlendAngle(allProperties, p => p.worldEulerAngles.y, weights);

		blended.localEulerAngles.x = WeightedBlends.WeightedBlendAngle(allProperties, p => p.localEulerAngles.x, weights);
		blended.localEulerAngles.y = WeightedBlends.WeightedBlendAngle(allProperties, p => p.localEulerAngles.y, weights);
		blended.localEulerAngles.z = WeightedBlends.WeightedBlendAngle(allProperties, p => p.localEulerAngles.z, weights);

		blended.viewportOffset.x = WeightedBlends.WeightedBlend(allProperties, p => p.viewportOffset.x, weights);
		blended.viewportOffset.y = WeightedBlends.WeightedBlend(allProperties, p => p.viewportOffset.y, weights);

		blended.fieldOfView = WeightedBlends.WeightedBlend(allProperties, p => p.fieldOfView, weights);

		blended.orthographic = WeightedBlends.WeightedBlend(allProperties.Select(p => p.orthographic), weights);
		blended.orthographicSize = WeightedBlends.WeightedBlend(allProperties, p => p.orthographicSize, weights);

		return blended;
	}

	public void Reset () {
		targetPoint = Vector3.zero;
		distance = 1;
		viewportOffset.x = viewportOffset.y = worldEulerAngles.x = worldEulerAngles.y = localEulerAngles.x = localEulerAngles.y = localEulerAngles.z = 0;
		fieldOfView = 60;
		orthographicSize = 10;
	}

	public void ApplyTo(ref SerializableCamera camera) {
		var finalRotation = rotation;
		camera.rotation = finalRotation;

		camera.fieldOfView = fieldOfView;
		
		camera.orthographic = orthographic;
		camera.orthographicSize = orthographicSize;
		// FOV is vertical
		camera.position = PositionWithViewportAspectRatio(camera.aspect);
	}

	public void ApplyTo(Camera camera) {
		var serCam = new SerializableCamera(camera);
		ApplyTo(ref serCam);
		serCam.ApplyTo(camera);
	}

	public bool HasNaN () {
		if(Vector3X.HasNaN(targetPoint)) return true;
		if(QuaternionX.IsNaN(axis)) return true;
		if(float.IsNaN(distance)) return true;
		if(Vector3X.HasNaN(worldEulerAngles)) return true;
		if(Vector3X.HasNaN(localEulerAngles)) return true;
		if(Vector3X.HasNaN(viewportOffset)) return true;
		if(float.IsNaN(orthographicSize)) return true;
		if(float.IsNaN(fieldOfView)) return true;
		return false;
	}

	public bool IsValid () {
		if(Vector3X.HasNaN(targetPoint)) return false;
		if(QuaternionX.IsNaN(axis)) return false;
		if(float.IsNaN(distance) || distance < 0) return false;
		if(Vector3X.HasNaN(worldEulerAngles)) return false;
		if(Vector3X.HasNaN(localEulerAngles)) return false;
		if(Vector3X.HasNaN(viewportOffset)) return false;
		if(orthographic && (float.IsNaN(orthographicSize) || orthographicSize <= 0)) return false;
		if(!orthographic && (float.IsNaN(fieldOfView) || fieldOfView <= 0)) return false;
		return true;
	}

	float GetPitch (Vector3 v){
		float len = Mathf.Sqrt((v.x * v.x) + (v.z * v.z));    // Length on xz plane.
		return -Mathf.Atan2(v.y, len) * Mathf.Rad2Deg;
	}

	float GetYaw (Vector3 v)  {
		return(Mathf.Atan2(v.x, v.z)) * Mathf.Rad2Deg;
	}

	Vector2 GetPitchAndYaw (Vector3 v)  {
		return new Vector2(GetPitch(v), GetYaw(v));
	}

	public override string ToString() {
		return string.Format("{0}: targetPoint: {1}, axis: {2}, distance: {3}, worldEulerAngles: {4}, localEulerAngles: {5}, viewportOffset: {6}, fieldOfView: {7}, orthographic: {8}, orthographicSize: {9}", GetType(), targetPoint, axis, distance, worldEulerAngles, localEulerAngles, viewportOffset, fieldOfView, orthographic, orthographicSize);
	}


	static class WeightedBlends {
		public static bool WeightedBlend(IEnumerable<bool> values, IList<float> weights) {
			float sum = 0;
			float total = 0;
			int i = 0;
			foreach(var value in values) {
				total += weights[i];
				if(value) sum += weights[i];
				i++;
			}
			return (sum/total) > 0.5f;
		}

		public static float WeightedBlend(IEnumerable<float> values, IList<float> weights) {
			return WeightedBlend(values, f => f, weights);
		}

		public static float WeightedBlend<T>(IEnumerable<T> values, Func<T, float> selector, IList<float> weights) {
			return WeightedBlend(
				values, selector, weights, 
				(float total, float val, float weight) => total + weight * val,
				total => total
			);
		}

		public static Vector3 WeightedBlend(IEnumerable<Vector3> values, IList<float> weights) {
			return WeightedBlend(values, v => v, weights);
		}

		public static Vector3 WeightedBlend<T>(IEnumerable<T> values, Func<T, Vector3> selector, IList<float> weights) {
			return WeightedBlend(
				values, selector, weights, 
				(Vector3 total, Vector3 val, float weight) => total + weight * val,
				total => total
			);
		}


		public static SerializableTransform WeightedBlend(IEnumerable<SerializableTransform> values, IList<float> weights) {
			return new SerializableTransform(WeightedBlend(values.Select(x => x.position), weights), WeightedBlend(values.Select(x => x.rotation), weights), Vector3.one);
		}

		public static float WeightedBlendAngle(IEnumerable<float> values, IList<float> weights) {
			return WeightedBlend(
				values, v => v, weights,
				(Vector2 totalDirection, float angle, float weight) => totalDirection + weight * WithDegrees(angle),
				totalDirection => Mathf.Atan2(-totalDirection.y, totalDirection.x) * Mathf.Rad2Deg
			);
		}

		public static float WeightedBlendAngle<T>(IEnumerable<T> values, Func<T, float> selector, IList<float> weights) {
			return WeightedBlend(
				values, selector, weights,
				(Vector2 totalDirection, float angle, float weight) => totalDirection + weight * WithDegrees(angle),
				totalDirection => Mathf.Atan2(-totalDirection.y, totalDirection.x) * Mathf.Rad2Deg
			);
		}


		struct WeightedAxes {
			public Vector3 forward;
			public Vector3 up;
		}
		public static Quaternion WeightedBlend<T>(IEnumerable<T> values, Func<T, Quaternion> selector, IList<float> weights) {
			return WeightedBlend(values, selector, weights, (WeightedAxes axes, Quaternion q, float weight) => {
				if(weight == 0) return axes;
				var forward = q * Vector3.forward;
				var up      = q * Vector3.up;
				axes.forward += weight * forward;
				axes.up      += weight * up;
				return axes;
			}, axes => {
				if(axes.forward == Vector3.zero || axes.up == Vector3.zero) return Quaternion.identity;
				return Quaternion.LookRotation(axes.forward.normalized, axes.up.normalized);
			});
		}

		public static Quaternion WeightedBlend(IEnumerable<Quaternion> values, IList<float> weights) {
			return WeightedBlend(values, q => q, weights);
		}

		static U WeightedBlend<T, U, TAccum>(IEnumerable<T> values, Func<T, U> selector, IList<float> weights, Func<TAccum, U, float, TAccum> accumFunc, Func<TAccum, U> resultFunc) {
			TAccum accumulatedTotal = default(TAccum);

			int i=0;
			foreach(T fromObj in values) {
				var weight = weights[i];
				var val = selector(fromObj);
				accumulatedTotal = accumFunc(accumulatedTotal, val, weight);
				i++;
			}

			return resultFunc(accumulatedTotal);
		}
		
		static T Identity<T>(T t) {
			return t;
		}

		static Vector2 WithDegrees(float degrees) {
			var rad = degrees * Mathf.Deg2Rad;
			return new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
		}
	}
	

	public void DrawGizmos (float nearClipPlane, float farClipPlane, float aspect) {
		var cachedMatrix = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(basePosition, rotation, Vector3.one);
		if(orthographic) {
			float spread = farClipPlane - nearClipPlane;
			float center = (farClipPlane + nearClipPlane)*0.5f;
			Gizmos.DrawWireCube(new Vector3(0,0,center), new Vector3(orthographicSize*2*aspect, orthographicSize*2, spread));
		} else {
			Gizmos.DrawFrustum(Vector3.zero, fieldOfView, farClipPlane, nearClipPlane, aspect);
		}
		Gizmos.matrix = cachedMatrix;
	}








	public override bool Equals(System.Object obj) {
		return obj is CameraProperties && this == (CameraProperties)obj;
	}
	
	public bool Equals(CameraProperties p) {
		return 
		axis == p.axis && 
		targetPoint == p.targetPoint && 
		distance == p.distance && 
		worldEulerAngles == p.worldEulerAngles && 
		localEulerAngles == p.localEulerAngles && 
		viewportOffset == p.viewportOffset && 
		fieldOfView == p.fieldOfView && 
		orthographic == p.orthographic && 
		orthographicSize == p.orthographicSize;
	}

	public override int GetHashCode() {
		unchecked // Overflow is fine, just wrap
		{
			int hash = 27;
			hash = hash * axis.GetHashCode();
			hash = hash * targetPoint.GetHashCode();
			hash = hash * distance.GetHashCode();
			hash = hash * worldEulerAngles.GetHashCode();
			hash = hash * localEulerAngles.GetHashCode();
			hash = hash * viewportOffset.GetHashCode();
			hash = hash * fieldOfView.GetHashCode();
			hash = hash * orthographic.GetHashCode();
			hash = hash * orthographicSize.GetHashCode();
			return hash;
		}
	}

	public static bool operator == (CameraProperties left, CameraProperties right) {
		return left.Equals(right);
	}

	public static bool operator != (CameraProperties left, CameraProperties right) {
		return !(left == right);
	}
}