using UnityEngine;
using System.Collections;

[ExecuteInEditMode, DisallowMultipleComponent]
public class LockTransform : MonoBehaviour {
	public bool hideHandles = true;
	public bool editorOnly = true;
//	public bool position = true;
//	public bool rotation = true;
//	public bool scale = true;
	
	void Update () {
		if(editorOnly && Application.isPlaying) {
			enabled = false;
			return;
		}
		if(!transform.IsDefault())
			transform.ResetTransform();
	}

	void OnDrawGizmosSelected () {
		#if UNITY_EDITOR
		if(!Application.isPlaying) {
			ComponentUtilityX.MoveToTop(this);
		}
		#endif
	}
}
