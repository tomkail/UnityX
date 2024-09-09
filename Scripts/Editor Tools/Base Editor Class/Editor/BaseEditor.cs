using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// Usage Example:

// using UnityEditor;
// using UnityEngine;
// using System.Collections;

// [CanEditMultipleObjects]
// [CustomEditor(typeof(Level))]
// public class LevelEditor : BaseEditor<Level>
// {}

[CanEditMultipleObjects]
public class BaseEditor<T> : Editor where T : UnityEngine.Object {
	
	#pragma warning disable
	protected T data;
	protected List<T> datas;
	
	public virtual void OnEnable() {
		SetData();
	}
	
	public override void OnInspectorGUI() {

		DrawDefaultInspector();
 
		if(GUI.changed && target != null) {         
			EditorUtility.SetDirty(target);
		}
	}

    public virtual void OnSceneGUI() {
	    if(datas == null || datas.Count == 0) return;
	    if (target == datas [0]) {
            OnMultiEditSceneGUI ();
        }
    }

    // Called once per SceneView refresh. You should iterate the targets/datas array to draw per object here
    protected virtual void OnMultiEditSceneGUI () {}

	protected void SetData () {
		// If an object has been deleted under our feet we need to handle it gracefully
		// (Previously it would assert)
		// This can happen if an editor script deletes an object that you previously had selected.
		if( target == null ) {
			data = null;
		} else {
			Debug.Assert(target as T != null, "Cannot cast "+target + " to "+typeof(T));
			data = (T) target;
		}

		datas = new List<T>();
		foreach(Object t in targets) {
			if( t == null ) continue;
			Debug.Assert(t as T != null, "Cannot cast "+t + " to "+typeof(T));
			datas.Add((T) t); 
		}
	}
}

