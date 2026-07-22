using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class TransformCopier : MonoBehaviour {
	/// <summary>
	/// The target to mirror.
	/// </summary>
	public Transform target;
	public bool position = true;
	public bool rotation = true;
	public bool useFixedUpdate = false;
	public bool playMode = true;
	public bool editMode = true;
	
	// Shared guard: no target, or the current play/edit mode is disabled, means we must not copy.
	bool ShouldCopy () {
		if(target == null) return false;
		if(Application.isPlaying && !playMode) return false;
		if(!Application.isPlaying && !editMode) return false;
		return true;
	}

	void OnEnable () {
		if(!ShouldCopy()) return;
		Apply();
	}
	void Update () {
		if(!ShouldCopy()) return;
		if(useFixedUpdate && Application.isPlaying) return;
		Apply();
	}

	void FixedUpdate () {
		if(!ShouldCopy()) return;
		if(!useFixedUpdate) return;
		Apply();
	}

	public void Apply () {
		if(position)
			transform.position = target.position;
		if(rotation)
			transform.rotation = target.rotation;
	}
}
