using UnityEngine;

/// <summary>
/// Time helper functions
/// </summary>
public static class TimeX {

	/// <summary>
	/// When using the Damping and Lerping functions, they adjust the damping
	/// or lerping value assuming that it was designed for a particular framerate.
	/// You don't really need to change this, even if your game goes at 60FPS,
	/// so long as you use it consistently throughout the game.
	/// </summary>
	public const float kDampingLerpingExpectedFramerate = 30;
	public const float targetFrameRateDeltaTime = 1f/kDampingLerpingExpectedFramerate;
	
	/// <summary>
	/// The smallest of the deltaTime or the target frame rate deltatime. 
	/// Prevents jumps when a frame takes a long time
	/// </summary>
	public static float targetClampedDeltaTime {
		get {
			return Mathf.Min(Time.deltaTime, targetFrameRateDeltaTime);
		}
	}

	/// <summary>
	/// The smallest of the deltaTime or the target frame rate deltatime. 
	/// Prevents jumps when a frame takes a long time
	/// </summary>
	public static float targetClampedUnscaledDeltaTime {
		get {
			return Mathf.Min(Time.unscaledDeltaTime, targetFrameRateDeltaTime);
		}
	}

	/// <summary>
	/// Turn a damping value (e.g. 0.97) into a value that's framerate independent.
	/// For example:
	///    carDamping = 0.97f;
	///    carSpeed = carSpeed * TimeX.Damping(carDamping);
	/// Scales according to an assumed 30 FPS.
	/// </summary>
	public static float Damping(float damping)
	{
		return Damping(damping, Time.deltaTime);
	}

	public static float Damping(float damping, float deltaTime)
	{
		float normalDeltaTime = (1.0f / kDampingLerpingExpectedFramerate);
		float wholeFramesPassed = deltaTime / normalDeltaTime;
		return Mathf.Pow(damping, wholeFramesPassed);

	}

	/// <summary>
	/// When using small lerp value to make (for example) an object approach a target
	/// when it slows down as it gets there, this makes the lerp value framerate
	/// independent.
	/// For example:
	///     approachLerp = 0.05f;
	/// 	x = Mathf.Lerp(x, target, TimeX.Lerping(approachlerp);
	/// </summary>
	public static float Lerping(float lerping)
	{
		return Lerping(lerping, Time.deltaTime);
	}

	public static float Lerping(float lerping, float deltaTime)
	{
		return 1.0f - Damping(1.0f - lerping, deltaTime);
	}
}