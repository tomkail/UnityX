using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityX.Springs.Editor {
	public static class SpringContextMenuPresets {
	    [InitializeOnLoadMethod]
	    static void SubscribeEditorEvents () {
	        EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
	        EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
	        AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
	        AssemblyReloadEvents.beforeAssemblyReload += UnsubscribeEditorEvents;
	    }

	    // Editor events outlive script assemblies, so unsubscribe before a code reload or the old handler keeps firing alongside the new one.
	    static void UnsubscribeEditorEvents () {
	        EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
	        AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeEditorEvents;
	    }

	    static IEnumerable<(string name, Spring spring)> presets {
		    get {
			    yield return ("Bouncy", Spring.bouncy);
			    yield return ("Smooth", Spring.smooth);
			    yield return ("Snappy", Spring.snappy);
		    }
	    } 
	    static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property) {
	        if (property.propertyType == SerializedPropertyType.Generic && property.type == "Spring") {
		        var propertyCopy = property.Copy();

		        foreach (var preset in presets) {
	        		var selected = propertyCopy.FindPropertyRelative("_mass").floatValue == preset.spring.mass &&
	        		                      propertyCopy.FindPropertyRelative("_stiffness").floatValue == preset.spring.stiffness &&
	        		                      propertyCopy.FindPropertyRelative("_damping").floatValue == preset.spring.damping;
	        		menu.AddItem (new GUIContent ($"Presets/{preset.name}"), selected, () => {
	        			propertyCopy.FindPropertyRelative("_mass").floatValue = preset.spring.mass;
	        			propertyCopy.FindPropertyRelative("_stiffness").floatValue = preset.spring.stiffness;
	        			propertyCopy.FindPropertyRelative("_damping").floatValue = preset.spring.damping;
				        var responseDampingProperties = Spring.PhysicalToResponseDamping(preset.spring.mass, preset.spring.stiffness, preset.spring.damping);
	        			propertyCopy.FindPropertyRelative("_response").floatValue = responseDampingProperties.response;
	        			propertyCopy.FindPropertyRelative("_dampingRatio").floatValue = responseDampingProperties.dampingRatio;
						propertyCopy.serializedObject.ApplyModifiedProperties();
	        		});
		        }
	        }
	    }
	}
}
