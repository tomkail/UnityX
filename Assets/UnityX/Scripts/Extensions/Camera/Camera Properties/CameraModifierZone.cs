using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraModifierZone : MonoBehaviour {
    public static List<CameraModifierZone> all = new List<CameraModifierZone>();
    public Transform target;
    [SerializeField]
    bool testStrength = false;
    public int sortIndex = 0;
    [Range(0,1)]
    public float strengthMultiplier = 1;
    public CameraPropertiesModifier modifier = new CameraPropertiesModifier();
    public float radius = 5f;

    [Space]
    public bool active;
    public bool inRange;
    [Range(0,1)]
    public float totalStrength;

    public float inRangeStrength;
    public float inRangeSmoothSpeed = 1;

    void OnEnable () {
        all.Add(this);
    }
    void OnDisable () {
        all.Remove(this);
    }
    
    protected virtual void LateUpdate () {
        inRange = active && Vector3.Distance(target.position, transform.position) < radius;
        if(!Application.isPlaying) {
        } else {
            inRangeStrength = Mathf.MoveTowards(inRangeStrength, inRange ? 1 : 0, inRangeSmoothSpeed * Time.deltaTime);
        }

        if(!testStrength) {
            totalStrength = strengthMultiplier * Mathf.SmoothStep(0, 1, inRangeStrength);
        }
    }

    public virtual void ModifyCameraProperties (ref CameraProperties properties) {
        modifier.properties.yaw = SignedDegreesAgainstDirection(Vector3.forward, transform.position-target.position, Vector3.up);
        modifier.ModifyWithStrength(ref properties, totalStrength);
    }

    float SignedDegreesAgainstDirection (Vector3 a, Vector3 b, Vector3 direction) {
		Vector3 normalizedDirection = direction.sqrMagnitude == 1 ? direction : direction.normalized;
		Vector3 projectedA = Vector3.ProjectOnPlane(a, normalizedDirection).normalized;
		Vector3 projectedB = Vector3.ProjectOnPlane(b, normalizedDirection).normalized;
		return Vector3.SignedAngle(projectedA, projectedB, direction);
	}

    private void OnDrawGizmosSelected () {
        Gizmos.color = new Color(0,1,0,0.5f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}