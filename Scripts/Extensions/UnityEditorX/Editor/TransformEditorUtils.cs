using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TransformEditorUtils
{
    [MenuItem("Tools/Transform/Position first on second")]
    private static void PositionFirstOnSecond()
    {
        Transform[] transforms = Selection.transforms;
        if (transforms.Length == 2)
        {
            Undo.RecordObject(Selection.activeTransform, "Move transform");
            if (Selection.activeTransform.position == transforms[0].position) Selection.activeTransform.position = transforms[1].position;
            else Selection.activeTransform.position = transforms[0].position;
        }
    }

    [MenuItem("Tools/Transform/Randomize Rotation", true)]
    private static bool RandomizeRotationValidator()
    {
        return Selection.activeTransform && Selection.activeTransform.childCount > 0;
    }
    [MenuItem("Tools/Transform/Randomize Rotation &%r")]
    private static void RandomizeRotation()
    {
		var trs = Selection.transforms;
	
		foreach (var tr in trs)
		{
			tr.rotation = Random.rotationUniform;
		}
    }

    [MenuItem("Tools/Transform/Move parent to average child position", true)]
    private static bool RecenterParentValidator()
    {
        return Selection.activeTransform && Selection.activeTransform.childCount > 0;
    }
    [MenuItem("Tools/Transform/Move parent to average child position &%t")]
    static void RecenterParent()
    {
        var topLevelTransforms = Selection.GetTransforms(SelectionMode.TopLevel);
        foreach(var topLevelTransform in topLevelTransforms)
            RecenterParent(topLevelTransform);
    }

    public static void RecenterParent(Transform parent)
    {
        Vector3 childrenAveragePos = parent.GetChildren().Select(x => x.position).Average();

        var localPos = childrenAveragePos;
        if (parent.parent != null)
            localPos = parent.parent.InverseTransformPoint(childrenAveragePos);

        // Change position only, don't affet rotation or scale
        Recenter(parent,
            localPos,
            parent.localRotation,
            parent.localScale);
    }

    [MenuItem("Tools/Transform/Reset parent transform")]
    private static void ResetParentTransform()
    {
        if (Selection.activeTransform && Selection.activeTransform.childCount > 0)
        {
            Undo.RecordObject(Selection.activeTransform, "Reset parent transform");
            Recenter(Selection.activeTransform);
        }
    }

    private static void Recenter(Transform parent)
    {
        Recenter(parent, Vector3.zero, Quaternion.identity, Vector3.one);
    }

    // Note that Undo.SetTransformParent isn't especially fast, and this operation can take a second or two. 
    // We might be able to optimise by changing child transforms rather than letting unity do it for us but life's too short 
    private static void Recenter(Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Recenter parent");
        var currentGroup = Undo.GetCurrentGroup();
        
        var children = parent.GetChildren();

        // Temporarily move children out (to stay with their grandparents, phew)
        foreach (var child in children)
            Undo.SetTransformParent(child, null, "Recenter parent");

        Undo.RecordObject(parent, "Recenter parent");
        parent.localPosition = localPosition;
        parent.localRotation = localRotation;
        parent.localScale = localScale;

        // Move children back again
        foreach (var child in children)
            Undo.SetTransformParent(child, parent, "Recenter parent");
        
        Undo.CollapseUndoOperations(currentGroup);
    }

    [MenuItem("Tools/Transform/Copy transform %&c", isValidateFunction: true)]
    private static bool CopyTransformValidator() { return Selection.activeTransform != null; }
    [MenuItem("Tools/Transform/Copy transform %&c")]
    private static void CopyTransform()
    {
        _copiedLocalPos = Selection.activeTransform.localPosition;
        _copiedLocalRot = Selection.activeTransform.localRotation;
        _copiedLocalScale = Selection.activeTransform.localScale;
        _hasCopiedLocalTransform = true;
    }

    [MenuItem("Tools/Transform/Paste transform %&c", isValidateFunction: true)]
    private static bool PasteTransformValidator() { return Selection.activeTransform != null && _hasCopiedLocalTransform; }
    [MenuItem("Tools/Transform/Paste transform %&v")]
    private static void PasteTransform()
    {
        foreach (Transform t in Selection.GetTransforms(SelectionMode.Editable))
        {
            Undo.RecordObject(t, "Paste transform");
            t.localPosition = _copiedLocalPos;
            t.localRotation = _copiedLocalRot;
            t.localScale = _copiedLocalScale;
        }
    }

    static Vector3 _copiedLocalPos;
    static Quaternion _copiedLocalRot;
    static Vector3 _copiedLocalScale;
    static bool _hasCopiedLocalTransform;
}