using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public abstract class SceneTask {
	public string sceneName {get; private set;}
	public bool complete {get; protected set;}
	protected AsyncOperation op;
    public float progress {
        get {
            if(complete) return 1;
            else if(op == null) return 0;
            else return op.progress;
        }
    }

	public SceneTask(string sceneName) {
		this.sceneName = sceneName;
	}

	public override string ToString () {
		return string.Format ("[{0}] SceneName:{1} complete:{2}", GetType(), sceneName, complete);
	}
}
