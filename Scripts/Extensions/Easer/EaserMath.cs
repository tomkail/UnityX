using UnityEngine;

namespace UnityX.Easer {
	// A small subset of UnityX's UnityEngineX helpers (QuaternionX / MathX), inlined so this module's assembly
	// needs no reference to the rest of UnityX. These are faithful copies of the originals.
	internal static class EaserMath {
		// QuaternionX.Difference: the relative rotation between two rotations.
		public static Quaternion Difference (Quaternion rotationA, Quaternion rotationB) {
			return Quaternion.Inverse(rotationB) * rotationA;
		}

		// QuaternionX.SmoothDamp: double-cover-aware nlerp approximation of Mathf.SmoothDamp for rotations.
		public static Quaternion SmoothDamp (Quaternion rot, Quaternion target, ref Quaternion currentVelocity, float smoothTime, float maxSpeed, float deltaTime) {
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

		// MathX.Sign: sign of f (0 counts as positive unless allowZero is set).
		public static int Sign (float f, bool allowZero = false) {
			if(allowZero && f == 0f) return 0;
			return f >= 0f ? 1 : -1;
		}
	}
}
