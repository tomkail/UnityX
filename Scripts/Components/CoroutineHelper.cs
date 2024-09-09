using System;
using System.Collections;
using UnityEngine;

public class CoroutineHelper : MonoSingleton<CoroutineHelper> {
	
	/// <summary>
	/// Execute the specified coroutine.
	/// </summary>
	/// <param name="coroutine">Coroutine.</param>
	public static Coroutine Execute(IEnumerator coroutine) {
		return Instance.StartCoroutine(coroutine);
	}


	/// <summary>
	/// Execute the specified coroutines.
	/// </summary>
	/// <param name="coroutines">Coroutines.</param>
	//	CoroutineHelper.Execute(new IEnumerator[] {
	//		CoroutineHelper.WaitForSeconds(0.2f),
	//		GameController.Instance.historyManager.AddToSaveHistory()
	//	});
	public static void Execute(IEnumerator[] coroutines) {
		Instance.StartCoroutine(ExecuteCR(coroutines));
	}

	public static IEnumerator ExecuteCR(IEnumerator[] coroutines) {
		foreach(IEnumerator coroutine in coroutines)
			yield return Instance.StartCoroutine(coroutine);
	}

	public static IEnumerator WaitForSeconds (float delay) {
		yield return new WaitForSeconds(delay);
	}
	
	/// <summary>
	/// Delay the specified action and delay.
	/// </summary>
	/// <param name="action">Action.</param>
	/// <param name="delay">Delay.</param>
	
	// Can be used like this!
	//	CoroutineHelper.Delay(() => {
	//		
	//	}, 1.0f);
	// or
	//	CoroutineHelper.Delay(1.0f, Method());
	public static IEnumerator Delay(Action action, float delay) {
		IEnumerator routine = DelayCR(action, delay);
		Execute(routine);
		return routine;
	}


	public static IEnumerator DelayRealtime(Action action, float delay) {
		IEnumerator routine = DelayRealtimeCR(action, delay);
		Execute(routine);
		return routine;
	}

	public static IEnumerator DelayFrame(Action action, int numFrames = 1) {
		IEnumerator routine = DelayFramesCR(action, numFrames);
		Execute(routine);
		return routine;
	}

	public static IEnumerator DelayCR (Action action, float delay) {
		yield return new WaitForSeconds(delay);
		action();
	}
	
	public static IEnumerator DelayRealtimeCR (Action action, float delay) {
		yield return new WaitForSecondsRealtime(delay);
		action();
	}
	
	private static IEnumerator DelayFramesCR (Action action, int numFrames) {
		for(int i = 0; i < numFrames; i++)
			yield return new WaitForEndOfFrame();
		action();
	}
	
	// delegate void DelayedMethod(params object[] objects); ?
}