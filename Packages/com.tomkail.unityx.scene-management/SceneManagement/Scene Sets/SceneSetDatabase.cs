using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

namespace UnityX.SceneManagement {
	// Every RuntimeSceneSet in the project; in the editor the list is rebuilt from the AssetDatabase.
	public class SceneSetDatabase : ScriptableObject {
		[SerializeField]
		private RuntimeSceneSet[] _sets;
		public RuntimeSceneSet[] sets {
			get {
				#if UNITY_EDITOR
				if(!Application.isPlaying) {
					CreateSetsArray();
				}
				#endif
				return _sets;
			}
		}

		#if UNITY_EDITOR
		public void CreateSetsArray () {
			var guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(RuntimeSceneSet));
			_sets = new RuntimeSceneSet[guids.Length];
			for(int i = 0; i < guids.Length; i++) {
				_sets[i] = UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]), typeof(RuntimeSceneSet)) as RuntimeSceneSet;
			}
		}
		#endif


		public RuntimeSceneSet FindSetupFromCurrent () {
			foreach(var sceneSet in sets) {
				if(sceneSet == null)
					continue;
				if(sceneSet.IsCurrentlyUniquelyLoaded())
					return sceneSet;
			}
			return null;
		}
	}
}
