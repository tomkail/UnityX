﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class QuaternionX {

	// The difference between two rotations. This can be used to get the relative rotation between two rotations.
	// var rotationRelativeToCamera = targetRotation.Difference(cameraRotation);
		
	public static Quaternion Difference (this Quaternion rotationA, Quaternion rotationB) {
		return Quaternion.Inverse(rotationB) * rotationA;
	}
	
	public static Quaternion Rotate(this Quaternion q1, Quaternion q2, Space relativeTo = Space.Self) {
		if(relativeTo == Space.World) {
			return q2 * q1;
		} else {
			return q1 * q2;
		}
	}
	
	public static Quaternion Rotate(this Quaternion q1, Vector3 eulerAngles, Space relativeTo = Space.Self) {
		return q1.Rotate(Quaternion.Euler(eulerAngles), relativeTo);
	}

	// Nabbed off the internet.
	public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion currentVelocity, float time, float maxSpeed, float deltaTime) {
		if(deltaTime == 0) return rot;

		// account for double-cover
		var Dot = Quaternion.Dot(rot, target);
		var Multi = Dot > 0f ? 1f : -1f;
		target.x *= Multi;
		target.y *= Multi;
		target.z *= Multi;
		target.w *= Multi;
		// smooth damp (nlerp approx)
		var Result = new Vector4(
			Mathf.SmoothDamp(rot.x, target.x, ref currentVelocity.x, time),
			Mathf.SmoothDamp(rot.y, target.y, ref currentVelocity.y, time),
			Mathf.SmoothDamp(rot.z, target.z, ref currentVelocity.z, time),
			Mathf.SmoothDamp(rot.w, target.w, ref currentVelocity.w, time)
		).normalized;
		// compute deriv
		var dtInv = 1f / deltaTime;
		currentVelocity.x = (Result.x - rot.x) * dtInv;
		currentVelocity.y = (Result.y - rot.y) * dtInv;
		currentVelocity.z = (Result.z - rot.z) * dtInv;
		currentVelocity.w = (Result.w - rot.w) * dtInv;
		return new Quaternion(Result.x, Result.y, Result.z, Result.w);
	}

	public static Quaternion AxisPitchYawRoll (Quaternion axis, float pitch, float yaw, float roll)  {
        var rotatedAxis = axis;
        rotatedAxis = rotatedAxis.Rotate(new Vector3(0,yaw,roll));
        rotatedAxis = rotatedAxis.Rotate(new Vector3(pitch,0,0));
		return rotatedAxis;
	}

	public static bool IsValid(this Quaternion q) {
		return !(q.x == 0 && q.y == 0 && q.z == 0 && q.w == 0);
	}
	
	public static bool IsNaN(this Quaternion q) {
		return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
	}
	
	public static Vector4 ToVector4(this Quaternion rot) {
		return new Vector4(rot.x, rot.y, rot.z, rot.w);
	}
}