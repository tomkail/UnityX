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
	
	void OnEnable () {
		if(target == null) return;
		if(Application.isPlaying && !playMode) return;
		if(!Application.isPlaying && !editMode) return;
		Apply();
	}
	void Update () {
		if(target == null) return;
		if(Application.isPlaying && !playMode) return;
		if(!Application.isPlaying && !editMode) return;
		if(useFixedUpdate && Application.isPlaying) return;
		Apply();
	}

	void FixedUpdate () {
		if(target == null) return;
		if(Application.isPlaying && !playMode) return;
		if(!Application.isPlaying && !editMode) return;
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
