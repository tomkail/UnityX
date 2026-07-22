using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor.EditorTools;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEditorInternal;

namespace UnityEditor.UI
{
    // Scene-view polygon editing for UIPolygon: shown in the Tools overlay while one is selected.
    // The offset matrix tracks the pixel-adjusted rect, so polygon space matches UI pixel space.
    [EditorTool("Edit Polygon", typeof(UIPolygon))]
    class UIPolygonEditorTool : PolygonEditorTool {
        protected override PolygonEditorInstance CreateInstance (Object target) {
            var uiPolygon = (UIPolygon)target;
            return new PolygonEditorInstance(uiPolygon.transform, Matrix4x4.Translate(uiPolygon.GetPixelAdjustedRect().position)) {
                snapInterval = 100,
                undoTarget = uiPolygon,
                GetPolygon = () => uiPolygon.polygon,
                OnPolygonChanged = _ => uiPolygon.SetVerticesDirty(),
            };
        }

        protected override void UpdateInstance (Object target, PolygonEditorInstance instance) {
            instance.offsetMatrix = Matrix4x4.Translate(((UIPolygon)target).GetPixelAdjustedRect().position);
        }
    }

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

        #pragma warning disable
        protected UIPolygon data;
        protected List<UIPolygon> datas;

        protected override void OnEnable() {
            base.OnEnable();
            SetData();

			texture = serializedObject.FindProperty("_texture");
			polygon = serializedObject.FindProperty("_polygon");
            // centreIsBoundsCentre = serializedObject.FindProperty("centreIsBoundsCentre");

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

        protected void SetData () {
            // If an object has been deleted under our feet we need to handle it gracefully.
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
            EditorGUILayout.PropertyField(polygon, new GUIContent("Polygon"), true);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("uvMode"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("uvXAngle"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("uvYAngle"));
            
            serializedObject.ApplyModifiedProperties();
        }
	}
}