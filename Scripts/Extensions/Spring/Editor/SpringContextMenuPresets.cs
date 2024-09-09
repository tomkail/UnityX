using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SpringContextMenuPresets {
    static SpringContextMenuPresets()
    {
        EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
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
