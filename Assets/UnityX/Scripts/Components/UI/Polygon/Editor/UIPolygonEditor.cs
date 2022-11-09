using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEditorInternal;

namespace UnityEditor.UI
{
    /// <summary>
    /// Editor class used to edit UI Sprites.
    /// </summary>

    [CustomEditor(typeof(UIPolygon), true)]
    [CanEditMultipleObjects]
    public class UIPolygonEditor : GraphicEditor {
		SerializedProperty texture;
		SerializedProperty polygon;
        // SerializedProperty centreIsBoundsCentre;
		private ReorderableList pointsList;
        PolygonEditorHandles polygonEditor;

        #pragma warning disable
        protected UIPolygon data;
        protected List<UIPolygon> datas;

        protected override void OnEnable() {
            base.OnEnable();
            SetData();

			texture = serializedObject.FindProperty("_texture");
			polygon = serializedObject.FindProperty("_polygon");
            // centreIsBoundsCentre = serializedObject.FindProperty("centreIsBoundsCentre");
            polygonEditor = new PolygonEditorHandles(data.transform, Matrix4x4.Translate(data.GetPixelAdjustedRect().position));
            polygonEditor.snapInterval = 100;
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

        void OnDisable() {
            // polygonEditor.Destroy();
        }

    
        void OnGraphicChange () {
            polygonEditor.offsetMatrix = Matrix4x4.Translate(data.GetPixelAdjustedRect().position);
        }

        protected void SetData () {
            // If an object has been deleted under our feet we need to handle it gracefully
            // (Previously it would assert)
            // This can happen if an editor script deletes an object that you previously had selected.
            if( target == null ) {
                data = null;
            } else {
                DebugX.Assert(target as UIPolygon != null, "Cannot cast "+target + " to "+typeof(UIPolygon));
                data = (UIPolygon) target;
            }

            datas = new List<UIPolygon>();
            foreach(Object t in targets) {
                if( t == null ) continue;
                DebugX.Assert(t as UIPolygon != null, "Cannot cast "+t + " to "+typeof(UIPolygon));
                datas.Add((UIPolygon)t); 
            }
        }

        public override void OnInspectorGUI()
        {
        	base.OnInspectorGUI();
            serializedObject.Update();

			EditorGUILayout.PropertyField(texture, new GUIContent("Texture"));
//			pointsList.DoLayoutList();
            EditorGUILayout.PropertyField(polygon, new GUIContent("Polygon"), true);
			// EditorGUILayoutX.DrawSerializedProperty(polygon);
            // EditorGUILayoutX.DrawSerializedProperty(polygon);
            // base.OnInspectorGUI();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("uvMode"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("uvXAngle"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("uvYAngle"));
            
            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI () {
            Undo.RecordObject(target, "Modified Polygon");
            polygonEditor.drawPolygon = Selection.activeGameObject == data.gameObject;
		    if(polygonEditor.OnSceneGUI(data.polygon)) {
                polygon.SetValue(data.polygon);
                data.SetVerticesDirty();
            }
        }
	}
}