using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEditorInternal;

namespace UnityEditor.UI.Extensions
{
    /// <summary>
    /// Editor class used to edit UI Sprites.
    /// </summary>

    // [CustomEditor(typeof(AdvancedUILineRenderer), true)]
    [CanEditMultipleObjects]
    public class AdvancedUILineRendererEditor : GraphicEditor {
		private ReorderableList pointsList;
        // LineEditor lineEditor;

        #pragma warning disable
        protected AdvancedUILineRenderer data;
        protected List<AdvancedUILineRenderer> datas;

        protected override void OnEnable() {
            base.OnEnable();
            SetData();

            // lineEditor = new LineEditor(data.transform, Matrix4x4.Translate(data.GetPixelAdjustedRect().position));
            // lineEditor.snapInterval = 100;
            data.RegisterDirtyLayoutCallback(OnGraphicChange);
//			pointsList = new ReorderableList(serializedObject, points, true, true, true, true);
//			pointsList.drawHeaderCallback = (Rect rect) => {  
//    			EditorGUI.LabelField(rect, "Points");
//			};
//			pointsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
//				var element = pointsList.serializedProperty.GetArrayElementAtIndex(index);
//    			rect.y += 2;
//				EditorGUI.PropertyField(rect, element, GUIContent.none);
//    		};
        }  
    
        void OnGraphicChange () {
            // lineEditor.offsetMatrix = Matrix4x4.Translate(data.GetPixelAdjustedRect().position);
        }

        protected void SetData () {
            // If an object has been deleted under our feet we need to handle it gracefully
            // (Previously it would assert)
            // This can happen if an editor script deletes an object that you previously had selected.
            if( target == null ) {
                data = null;
            } else {
                DebugX.Assert(target as AdvancedUILineRenderer != null, "Cannot cast "+target + " to "+typeof(AdvancedUILineRenderer));
                data = (AdvancedUILineRenderer) target;
            }

            datas = new List<AdvancedUILineRenderer>();
            foreach(Object t in targets) {
                if( t == null ) continue;
                DebugX.Assert(t as AdvancedUILineRenderer != null, "Cannot cast "+t + " to "+typeof(AdvancedUILineRenderer));
                datas.Add((AdvancedUILineRenderer)t); 
            }
        }

        public override void OnInspectorGUI()
        {
        	base.OnInspectorGUI();
            serializedObject.Update();

			// EditorGUILayout.PropertyField(texture, new GUIContent("Texture"));
//			pointsList.DoLayoutList();
			// EditorGUILayoutX.DrawSerializedProperty(polygon);

            // EditorGUILayout.PropertyField(centreIsBoundsCentre);
            
            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI () {
            Undo.RecordObject(target, "Modified Line");
		    // if(lineEditor.OnSceneGUI(data.polygon)) {
            //     data.SetVerticesDirty();
            //     data.SetMaterialDirty();
            // }
        }
	}
}