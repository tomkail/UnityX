using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
#endif

[ExecuteAlways]
public class GameObjectChangeChecker : MonoBehaviour {
	[SerializeField, HideInInspector]
	string lastName;
	public bool useInPlayMode = true;
	#if UNITY_EDITOR
	public bool useInEditMode = true;
	#endif

	public event System.Action<GameObject> OnGameObjectChanged;
	public event System.Action<GameObject> OnNameChanged;
	public event System.Action<GameObject> OnDestroyed;

	void Update () {
		if(Application.isPlaying && !useInPlayMode) return;
		#if UNITY_EDITOR
		if(!Application.isPlaying && !useInEditMode) return;
		#endif

		if(gameObject.name != lastName) {
			lastName = gameObject.name;
			if(OnNameChanged != null) OnNameChanged(gameObject);
			if(OnGameObjectChanged != null) OnGameObjectChanged(gameObject);
			gameObject.BetterSendMessage("OnChangedName", gameObject);
			gameObject.BetterSendMessage("OnChangedGameObject", gameObject);
		}
	}

	void OnDestroy () {
		if(Application.isPlaying && !useInPlayMode) return;
		#if UNITY_EDITOR
		if(!Application.isPlaying && !useInEditMode) return;
		#endif

		if(OnDestroyed != null) OnDestroyed(gameObject);
		if(OnGameObjectChanged != null) OnGameObjectChanged(gameObject);
		gameObject.BetterSendMessage("OnDestroyed", gameObject);
		gameObject.BetterSendMessage("OnChangedGameObject", gameObject);
	}
}