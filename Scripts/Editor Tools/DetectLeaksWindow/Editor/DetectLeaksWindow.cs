using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DetectLeaksWindow : EditorWindow {
    public static int numObjects;
	private static bool _showEditorResources = false;
	public static bool showEditorResources {
		get {
			return _showEditorResources;
			} set {
				if(_showEditorResources == value) return;
				_showEditorResources = value;
				Refresh();
			}
	}
	public static LeakDetectionType<GameObject> gameObjects = new LeakDetectionType<GameObject>();
	public static LeakDetectionType<Material> materials = new LeakDetectionType<Material>();
	public static LeakDetectionType<Texture> textures = new LeakDetectionType<Texture>();
	public static LeakDetectionType<Sprite> sprites = new LeakDetectionType<Sprite>();
	public static LeakDetectionType<Mesh> meshes = new LeakDetectionType<Mesh>();
	public static LeakDetectionType<AudioSource> sounds = new LeakDetectionType<AudioSource>();

	private static Vector2 scrollPosition;

	public class LeakDetectionType<T> where T : Object {
		public bool _show = false;
		public bool show {
			get {
				return _show;
			} set {
				if(_show == value) return;
				_show = value;
				if(show) Refresh();
			}
		}
		public int quantity;
		public string allString;
        public List<GUIContent> all = new List<GUIContent>();
		public Vector2 scrollPosition;

		public void Refresh () {
            var type = typeof(T);
			Object[] objects = DetectLeaksWindow.showEditorResources ? Resources.FindObjectsOfTypeAll(type) : Object.FindObjectsOfType<T>();
			quantity = objects.Length;

            all.Clear();
			// allString = string.Empty;
	        for (int i = 0; i < objects.Length; i++) {
                var name = (objects[i].name == string.Empty ? "UNNAMED "+type.Name : objects[i].name);
                var guiContent = new GUIContent(name);
                if(type == typeof(Texture)) {
                    guiContent.image = objects[i] as Texture;
                } else if(type == typeof(Sprite)) {
                    guiContent.image = (objects[i] as Sprite).texture;
                }
                all.Add(guiContent);
				// allString += (i.ToString() + ". " + + "\n");
				// if(allString.Length > 15000) {
				// 	allString += "...";
				// 	break;
				// }
	        }
		}

		public void OnGUI () {
			GUILayout.BeginVertical(GUI.skin.box);
			show = EditorGUILayout.Foldout(show, typeof(T).Name + (show ? " ("+quantity+")" : ""));
			if(show) {
				if(GUILayout.Button("Refresh")) Refresh();
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
				// EditorGUILayout.TextArea(allString, GUILayout.ExpandHeight(true));
				foreach(var x in all) {
                    EditorGUILayout.LabelField(x);    
                }
                EditorGUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}
	}
	[MenuItem ("Window/Debugging/Detect Leaks")]
	public static void CreateWindow () {
//		EditorApplication.update += Update;
		EditorWindow.GetWindow <DetectLeaksWindow>(false, "Detect Leaks", true);
	}

	void OnFocus () {
//		Refresh();
	}

//	static void Update(){}

	static void Refresh(){
		gameObjects.Refresh();
		materials.Refresh();
		textures.Refresh();
		sprites.Refresh();
		meshes.Refresh();
		sounds.Refresh();
    }

    void OnGUI() {
    	autoRepaintOnSceneChange = true;
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		showEditorResources = GUILayout.Toggle(showEditorResources, "Show Editor Resources");
        if (GUILayout.Button("Unload Unused Assets")) {
            Resources.UnloadUnusedAssets();
        }

        if (GUILayout.Button("Mono Garbage Collect")) {
            System.GC.Collect();
        }

        GUILayout.BeginVertical(GUI.skin.box);
		gameObjects.OnGUI();
		materials.OnGUI();
		textures.OnGUI();
		sprites.OnGUI();
        meshes.OnGUI();
		sounds.OnGUI();
		GUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
    }
}
