using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Invisibly created singleton that's created when an Animate call to SLayout is made, and runs
/// all the animations so that individual SLayouts can be super lightweight, not even having an Update call.
/// You can also choose to create an SLayoutAnimator and stick it in your scene if you want.

// This singleton can be created (but doesn't need to actually do anything) in edit mode, and needs to function when Configurable Enter Play Mode is used.
// In edit mode, the singleton has flags to ensure it can't be saved into the scene.
// In play mode we use the DontDestroyOnLoad scene to ensure the singleton is never destroyed.

// There's a fairly elaborate setup to ensure that the edit mode version works correctly.
// It destroys itself whenever play mode state changes, and tries to find itself in the scene before creating a new instance.
// This resolves issues caused by the instance being created and immediately lost in the strange time between edit and play mode.
/// </summary>
public sealed class SLayoutAnimator : MonoBehaviour
{
	#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void CompileReset() {
        DestroyInstance();
        UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }
    
    static void PlayModeStateChanged (UnityEditor.PlayModeStateChange playModeStateChange) {
		DestroyInstance();
    }
	
	static void DestroyInstance () {
		if(_instance != null) {
            if(Application.isPlaying) UnityEngine.Object.Destroy(_instance.gameObject);
            else UnityEngine.Object.DestroyImmediate(_instance.gameObject);
        }
	}
    #endif

	public bool IsAnimating(SLayout target) {
		foreach(var a in _animations) {
			if( a.owner == target ) return true;
		}
		return false;
	}

	public void StartAnimation(SLayoutAnimation anim)
	{
		anim.Start();
		if( !anim.isComplete ) _animations.Add(anim);
	}

	public void AddDelay(float extraDelay)
	{
		var anim = SLayoutAnimation.AnimationUnderDefinition();
		if( anim != null ) {
			anim.AddDelay(extraDelay);
		}
	}

	public void AddDuration(float extraDuration)
	{
		var anim = SLayoutAnimation.AnimationUnderDefinition();
		if( anim != null ) {
			anim.AddDuration(extraDuration);
		}
	}

	public void Animatable(Action<float> customAnim)
	{
		var anim = SLayoutAnimation.AnimationUnderDefinition();
		if( anim != null ) {
			anim.AddCustomAnim(customAnim);
		} else {
			customAnim(1.0f);
		}
	}

	public void CancelAnimations(SLayout target)
	{
		for(int i=0; i<_animations.Count; i++) {
			var anim = _animations[i];
			if( anim.owner == target ) {
				anim.Cancel();
				_animationsToRemove.Add(anim);
			}
		}
			
		foreach(var anim in _animationsToRemove)
			_animations.Remove(anim);

		_animationsToRemove.Clear();
	}

	public void CompleteAnimations(SLayout target)
	{
		for(int i=0; i<_animations.Count; i++) {
			var anim = _animations[i];
			if( anim.owner != target ) continue;

			anim.CompleteImmediate();
		}
    }

	public static SLayoutAnimator instance {
		get {
			if( _instance == null ) {
				#if UNITY_EDITOR
				// If we're between modes, then just exit out!
				if(!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && UnityEditor.EditorApplication.isPlaying) 
					return null;
				#endif
				GameObject ownerGO = null;
				if(!Application.isPlaying) {
					ownerGO = GameObject.Find("SLayoutAnimator");
					if(ownerGO == null) {
						ownerGO = new GameObject("SLayoutAnimator");
						_instance = ownerGO.AddComponent<SLayoutAnimator>();
					} else {
						_instance = ownerGO.GetComponent<SLayoutAnimator>();
						Debug.Assert(_instance != null, "Instance of SLayoutAnimator was found by name, but no SLayoutAnimator was found! This is not currently handled.");
					}
				} else {
					ownerGO = new GameObject("SLayoutAnimator");
					_instance = ownerGO.AddComponent<SLayoutAnimator>();
				}
				
				// In play mode, use DontDestroyOnLoad
				if(Application.isPlaying) {
					DontDestroyOnLoad(ownerGO);
				}
				// In edit mode, just use HideFlags so we can see it's not going to stick around 
				else {
					ownerGO.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
				}
			}
			return _instance;
		}
	}
	
	// This resets any fields populated in edit mode when Configurable Enter Play Mode/Scene Reloading is disabled
	void OnEnable() {
		// Shouldnt ever occur, but in case a second instance is created, destroy it immediately!
		if(_instance != null) {
			Destroy(this);
			return;
		}
		_instance = this;

		_animations.Clear();
		_animationsToRemove.Clear();
	}
	void OnDisable() {
		_animations.Clear();
		_animationsToRemove.Clear();
	}
	void OnDestroy() {
		if( _instance == this ) _instance = null;
	}
	
	void Update() {
		if( _animations.Count > 0 ) {
			// Don't foreach, since a new animation could be added to the list
			// as part of the Update (due to a completion callback)
			for(int i=0; i<_animations.Count; ++i) {
				var anim = _animations[i];

				// If owner object has been deleted, stop the animation
				// (Definitely don't attempt to Update the anim since it'll likely
				//  access properties of the object that has been deleted.)
				if( !anim.canAnimate ) {
					_animationsToRemove.Add(anim);
					continue;
				}

				anim.Update();

				if( anim.isComplete )
					_animationsToRemove.Add(anim);
			}

			foreach(var anim in _animationsToRemove)
				_animations.Remove(anim);

			_animationsToRemove.Clear();
		}
	}

	List<SLayoutAnimation> _animations = new List<SLayoutAnimation>();
	List<SLayoutAnimation> _animationsToRemove = new List<SLayoutAnimation>();

	public static SLayoutAnimator _instance;
}
