using UnityEngine;

/// <summary>
/// A tween class for Camera Properties. Saves writing out lots of code for a simple camera effect such as a smooth zoom or pan.
/// Individual properties 
/// </summary>
[System.Serializable]
public class CameraPropertiesTween : TypeTween<CameraProperties> {

	private AnimationCurve _targetPointEasingCurve;
	public AnimationCurve targetPointEasingCurve {
		get {
			return _targetPointEasingCurve != null ? _targetPointEasingCurve : easingCurve;
		} set {
			_targetPointEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}

	private AnimationCurve _distanceEasingCurve;
	public AnimationCurve distanceEasingCurve {
		get {
			return _distanceEasingCurve != null ? _distanceEasingCurve : easingCurve;
		} set {
			_distanceEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}

	private AnimationCurve _worldPitchEasingCurve;
	public AnimationCurve worldPitchEasingCurve {
		get {
			return _worldPitchEasingCurve != null ? _worldPitchEasingCurve : easingCurve;
		} set {
			_worldPitchEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}

	private AnimationCurve _worldYawEasingCurve;
	public AnimationCurve worldYawEasingCurve {
		get {
			return _worldYawEasingCurve != null ? _worldYawEasingCurve : easingCurve;
		} set {
			_worldYawEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}

	private AnimationCurve _localPitchEasingCurve;
	public AnimationCurve localPitchEasingCurve {
		get {
			return _localPitchEasingCurve != null ? _localPitchEasingCurve : easingCurve;
		} set {
			_localPitchEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}
	private AnimationCurve _localYawEasingCurve;
	public AnimationCurve localYawEasingCurve {
		get {
			return _localYawEasingCurve != null ? _localYawEasingCurve : easingCurve;
		} set {
			_localYawEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}
	private AnimationCurve _localRollEasingCurve;
	public AnimationCurve localRollEasingCurve {
		get {
			return _localRollEasingCurve != null ? _localRollEasingCurve : easingCurve;
		} set {
			_localRollEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}

	private AnimationCurve _viewportOffsetXEasingCurve;
	public AnimationCurve viewportOffsetXEasingCurve {
		get {
			return _viewportOffsetXEasingCurve != null ? _viewportOffsetXEasingCurve : easingCurve;
		} set {
			_viewportOffsetXEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}

	private AnimationCurve _viewportOffsetYEasingCurve;
	public AnimationCurve viewportOffsetYEasingCurve {
		get {
			return _viewportOffsetYEasingCurve != null ? _viewportOffsetYEasingCurve : easingCurve;
		} set {
			_viewportOffsetYEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}

	private AnimationCurve _fieldOfViewEasingCurve;
	public AnimationCurve fieldOfViewEasingCurve {
		get {
			return _fieldOfViewEasingCurve != null ? _fieldOfViewEasingCurve : easingCurve;
		} set {
			_fieldOfViewEasingCurve = value;
			SetDefaultLerpFunction();
		}
	}

	public CameraPropertiesTween () : base () {}
	public CameraPropertiesTween (CameraProperties myStartValue) : base (myStartValue) {}
	public CameraPropertiesTween (CameraProperties myStartValue, CameraProperties myTargetValue, float myLength) : base (myStartValue, myTargetValue, myLength) {}
	public CameraPropertiesTween (CameraProperties myStartValue, CameraProperties myTargetValue, float myLength, AnimationCurve myLerpCurve) : base (myStartValue, myTargetValue, myLength, myLerpCurve) {}

	protected override void SetDefaultLerpFunction () {
		lerpFunction = (start, end, lerp) => {
			CameraProperties properties = new CameraProperties();
			properties.targetPoint = Vector3.Lerp(start.targetPoint, end.targetPoint, targetPointEasingCurve.Evaluate(lerp));
			properties.distance = Mathf.Lerp(start.distance, end.distance, distanceEasingCurve.Evaluate(lerp));

			properties.worldEulerAngles.x = Mathf.LerpAngle(start.worldEulerAngles.x, end.worldEulerAngles.x, worldPitchEasingCurve.Evaluate(lerp));
			properties.worldEulerAngles.y = Mathf.LerpAngle(start.worldEulerAngles.y, end.worldEulerAngles.y, worldYawEasingCurve.Evaluate(lerp));

			properties.localEulerAngles.x = Mathf.LerpAngle(start.localEulerAngles.x, end.localEulerAngles.x, localPitchEasingCurve.Evaluate(lerp));
			properties.localEulerAngles.y = Mathf.LerpAngle(start.localEulerAngles.y, end.localEulerAngles.y, localYawEasingCurve.Evaluate(lerp));
			properties.localEulerAngles.z = Mathf.LerpAngle(start.localEulerAngles.z, end.localEulerAngles.z, localRollEasingCurve.Evaluate(lerp));

			properties.viewportOffset.x = Mathf.Lerp(start.viewportOffset.x, end.viewportOffset.x, viewportOffsetXEasingCurve.Evaluate(lerp));
			properties.viewportOffset.y = Mathf.Lerp(start.viewportOffset.y, end.viewportOffset.y, viewportOffsetYEasingCurve.Evaluate(lerp));

			properties.fieldOfView = Mathf.Lerp(start.fieldOfView, end.fieldOfView, fieldOfViewEasingCurve.Evaluate(lerp));
			return properties;
		};
	}

	protected override void SetDeltaValue (CameraProperties myLastValue, CameraProperties myCurrentValue) {}
}