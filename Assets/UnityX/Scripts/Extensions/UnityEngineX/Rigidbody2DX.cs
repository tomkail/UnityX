using UnityEngine;

public static class Rigidbody2DX {
    public static void TorqueTo(float currentAngle, float targetAngle, Rigidbody2D rb, float maxTorque, float torqueDampFactor, float offsetForgive = 0) {
        float angleDifference = Mathf.DeltaAngle(targetAngle, currentAngle);
        if (Mathf.Abs(angleDifference) < offsetForgive) return;

        float torqueToApply = maxTorque * angleDifference / 180f;
        torqueToApply -= rb.angularVelocity * torqueDampFactor;
        rb.AddTorque(torqueToApply, ForceMode2D.Force);
    }
}
