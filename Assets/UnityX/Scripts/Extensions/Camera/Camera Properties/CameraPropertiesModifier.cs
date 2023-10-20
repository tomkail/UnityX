using UnityEngine;

/// <summary>
/// Modifies selected camera properties using a blend mode.
/// </summary>
[System.Serializable]
public class CameraPropertiesModifier {
	
	public enum Mode {
		Additive,
		Multiply,
		Override
	}

	[EnumFlagAttribute]
	public CameraProperties.CameraPropertiesAxis modifiers = CameraProperties.CameraPropertiesAxis.WorldYaw;
	public Mode mode = Mode.Override;

	public CameraProperties properties = new CameraProperties();

	public CameraPropertiesModifier () {}
	public CameraPropertiesModifier (CameraPropertiesModifier toClone) {
		CopyFrom(toClone);
	}

	public void CopyFrom (CameraPropertiesModifier toClone) {
		modifiers = toClone.modifiers;
		mode = toClone.mode;
		properties = toClone.properties;
	}

	public void ModifyWithStrength (ref CameraProperties propertiesToModify, float strength) {
		if(strength == 0 && mode != Mode.Multiply) return;
		
		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.TargetPoint)) {
			if(mode == Mode.Additive) {
				propertiesToModify.targetPoint += properties.targetPoint * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.targetPoint = Vector3.Scale(propertiesToModify.targetPoint, properties.targetPoint * strength);
			} else if(mode == Mode.Override) {
				propertiesToModify.targetPoint = Vector3.Lerp(propertiesToModify.targetPoint, properties.targetPoint, strength);
			}
		}
		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.Axis)) {
			if(mode == Mode.Additive) {
				propertiesToModify.axis = propertiesToModify.axis * Quaternion.Euler(properties.targetPoint * strength);
			} else if(mode == Mode.Multiply) {

			} else if(mode == Mode.Override) {
				propertiesToModify.axis = Quaternion.Slerp(propertiesToModify.axis, properties.axis, strength);
			}
		}
		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.Distance)) {
			if(mode == Mode.Additive) {
				propertiesToModify.distance += properties.distance * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.distance *= properties.distance * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.distance = Mathf.Lerp(propertiesToModify.distance, properties.distance, strength);
			}
		}

		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.WorldPitch)) {
			if(mode == Mode.Additive) {
				propertiesToModify.worldEulerAngles.x += properties.worldEulerAngles.x * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.worldEulerAngles.x *= properties.worldEulerAngles.x * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.worldEulerAngles.x = Mathf.LerpAngle(propertiesToModify.worldEulerAngles.x, properties.worldEulerAngles.x, strength);
			}
		}

		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.WorldYaw)) {
			if(mode == Mode.Additive) {
				propertiesToModify.worldEulerAngles.y += properties.worldEulerAngles.y * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.worldEulerAngles.y *= properties.worldEulerAngles.y * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.worldEulerAngles.y = Mathf.LerpAngle(propertiesToModify.worldEulerAngles.y, properties.worldEulerAngles.y, strength);
			}
		}

		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.LocalPitch)) {
			if(mode == Mode.Additive) {
				propertiesToModify.localEulerAngles.x += properties.localEulerAngles.x * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.localEulerAngles.x *= properties.localEulerAngles.x * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.localEulerAngles.x = Mathf.LerpAngle(propertiesToModify.localEulerAngles.x, properties.localEulerAngles.x, strength);
			}
		}

		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.LocalYaw)) {
			if(mode == Mode.Additive) {
				propertiesToModify.localEulerAngles.y += properties.localEulerAngles.y * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.localEulerAngles.y *= properties.localEulerAngles.y * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.localEulerAngles.y = Mathf.LerpAngle(propertiesToModify.localEulerAngles.y, properties.localEulerAngles.y, strength);
			}
		}

		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.LocalRoll)) {
			if(mode == Mode.Additive) {
				propertiesToModify.localEulerAngles.z += properties.localEulerAngles.z * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.localEulerAngles.z *= properties.localEulerAngles.z * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.localEulerAngles.z = Mathf.Lerp(propertiesToModify.localEulerAngles.z, properties.localEulerAngles.z, strength);
			}
		}

		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.HorizontalViewportOffset)) {
			if(mode == Mode.Additive) {
				propertiesToModify.viewportOffset.x += properties.viewportOffset.x * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.viewportOffset.x *= properties.viewportOffset.x * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.viewportOffset.x = Mathf.Lerp(propertiesToModify.viewportOffset.x, properties.viewportOffset.x, strength);
			}
		}

		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.VerticalViewportOffset)) {
			if(mode == Mode.Additive) {
				propertiesToModify.viewportOffset.y += properties.viewportOffset.y * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.viewportOffset.y *= properties.viewportOffset.y * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.viewportOffset.y = Mathf.Lerp(propertiesToModify.viewportOffset.y, properties.viewportOffset.y, strength);
			}
		}

		if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.FieldOfView)) {
			if(mode == Mode.Additive) {
				propertiesToModify.fieldOfView += properties.fieldOfView * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.fieldOfView *= properties.fieldOfView * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.fieldOfView = Mathf.Lerp(propertiesToModify.fieldOfView, properties.fieldOfView, strength);
			}
		}
		
        if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.Orthographic)) {
            propertiesToModify.orthographic = properties.orthographic;
		}
        
        if(modifiers.HasFlag(CameraProperties.CameraPropertiesAxis.OrthographicSize)) {
			if(mode == Mode.Additive) {
				propertiesToModify.orthographicSize += properties.orthographicSize * strength;
			} else if(mode == Mode.Multiply) {
				propertiesToModify.orthographicSize *= properties.orthographicSize * strength;
			} else if(mode == Mode.Override) {
				propertiesToModify.orthographicSize = Mathf.Lerp(propertiesToModify.orthographicSize, properties.orthographicSize, strength);
			}
		}
	}
}