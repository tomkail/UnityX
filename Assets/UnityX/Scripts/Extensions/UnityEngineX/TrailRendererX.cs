using UnityEngine;
using System.Collections;

public static class TrailRendererX {
	
	/// <summary>
	/// Reset the trail so it can be moved without streaking
	/// </summary>
	public static void Clear(this TrailRenderer trail) {
		CoroutineHelper.Execute(ClearTrail(trail));
	}
	
	/// <summary>
	/// Coroutine to reset a trail renderer trail
	/// </summary>
	/// <param name="trail"></param>
	/// <returns></returns>
	static IEnumerator ClearTrail(TrailRenderer trail) {
		if(trail == null) {
			Debug.LogError("Attempted to clear TrailRenderer, but trail is null.");
			yield break;
		}
		// Don't do this if we're already clearing the trail.
		if(trail.time == -1) {
			Debug.LogWarning("Attempted to clear TrailRenderer twice.");
			yield break;
		}
		var trailTime = trail.time;
		trail.time = -1;
		yield return new WaitForEndOfFrame();
		if(trail != null)
			trail.time = trailTime;
	}        
}