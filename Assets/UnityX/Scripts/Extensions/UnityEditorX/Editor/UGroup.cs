// UGroup 1.4
// Simple Unity Editor Plugin
// Adds Menu: Edit/Group
// Places the selected objects inside a new empty go
// Adds Menu: Edit/Group...
// Grouping wizzard with optional selections (now more selections)
// (c) 2012, 2013 G. Mattner

using UnityEngine;
using UnityEditor;
using System.Linq;

public class UGroup : EditorWindow {
    static private GameObject groupParent;

    [MenuItem("Edit/Group %g", false, -999)]
    static void GroupMenu()
    {
        GroupSelectedTransforms();
    }

    static void GroupSelectedTransforms(string name = "Group", string tag = null, Vector3 position = default(Vector3), bool calcCenter = true, bool bottomY = false, bool roundToInt = false)
    {
        Transform[] currentSelection = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
        if (currentSelection.Length == 0)
            return;

		groupParent = new GameObject(name);
		Undo.RegisterCreatedObjectUndo(groupParent, "Created group"); // V 4.3+
		if(Selection.activeTransform != null) {
			groupParent.transform.SetParent(Selection.activeTransform.parent);
			groupParent.transform.SetSiblingIndex(Selection.activeTransform.GetSiblingIndex());
            if(Selection.activeTransform.parent != null && Selection.activeTransform.parent.GetComponent<RectTransform>() != null) {
                var rectTransform = Undo.AddComponent<RectTransform>(groupParent);
                rectTransform.localScale = Vector3.one;
            }
		}
		
        if (calcCenter)
        {
            position = Vector3.zero;
            foreach (Transform groupMember in currentSelection)
            {
                position += groupMember.transform.position;
            }
            if (currentSelection.Length > 0)
                position /= currentSelection.Length;
			
			if (bottomY)
			{
				foreach (Transform groupMember in currentSelection)
				{
					if (groupMember.transform.position.y < position.y)
						position.y = groupMember.transform.position.y;
					if (groupMember.GetComponent<Collider>())
					if (groupMember.GetComponent<Collider>().bounds.min.y < position.y)
						position.y = groupMember.GetComponent<Collider>().bounds.min.y;
					if (groupMember.GetComponent<Renderer>())	
					if (groupMember.GetComponent<Renderer>().bounds.min.y < position.y)
						position.y = groupMember.GetComponent<Renderer>().bounds.min.y;
						
				}
				
			}

            if( roundToInt ) {
                position.x = Mathf.RoundToInt(position.x);
                position.y = Mathf.RoundToInt(position.y);
                position.z = Mathf.RoundToInt(position.z);
            }
        }
        groupParent.transform.position = position;
        if (tag != null)
        {
            try
            {
                groupParent.tag = tag;
            }
            catch
            {
                Debug.Log("The tag '" + tag + "' is not defined, please add the tag in the tag manager!");
            }
        }
        foreach (Transform groupMember in currentSelection)
        {
			Undo.SetTransformParent(groupMember.transform, groupParent.transform, "Group");
        }
        GameObject[] newSel = new GameObject[1];
        newSel[0] = groupParent;
        Selection.objects = newSel;
    }

    [MenuItem("Edit/Group %g", true, -999)]
    [MenuItem("Edit/Group...", true, -998)]
    static bool ValidateSelection()
    {
        return Selection.activeTransform != null;
    }

    public Vector3 parentPosition = Vector3.zero;
    public bool centerPosition = false;
	public bool centerBottomY = false;
    public string groupName = "Group";
    public string gTag;

    [MenuItem("Edit/Group...", false, -998)]
    public static void Init()
    {
        var win = GetWindow(typeof(UGroup)) as UGroup;
        win.autoRepaintOnSceneChange = true;
    }

    void OnGUI()
    {
        string helpString = "Groups the selected objects into an empty parent.\nYou can specify the position for the root object \nand it's name.\nYou can also select a tag for the group.\nCheck the center option to position the group\nat the center of the selected objects.";

        GUILayout.BeginHorizontal();
        GUILayout.Space(2);
        GUILayout.Label(helpString);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        gTag = EditorGUILayout.TagField("Tag group with:", gTag);
        //GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Position parent in center of selected objects");
        centerPosition = EditorGUILayout.Toggle(centerPosition);
        GUILayout.EndHorizontal();
		if (!centerPosition)
			   GUI.enabled = false;
		GUILayout.BeginHorizontal();
        GUILayout.Label("... and at the bottom of the group (min Y)");
        centerBottomY = EditorGUILayout.Toggle(centerBottomY);
        GUILayout.EndHorizontal();
		GUI.enabled = true;
        GUILayout.BeginHorizontal();
        if (centerPosition)
            GUI.enabled = false;
        parentPosition = EditorGUILayout.Vector3Field("Parent Position", parentPosition);
        //GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUI.enabled = true;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Group Name: ");
        groupName = GUILayout.TextField(groupName);
        //GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Close"))
        {
            this.Close();
        }
        if (GUILayout.Button("Group"))
        {
            GroupSelectedTransforms(groupName, gTag, parentPosition, centerPosition, centerBottomY);
        }
        if (GUILayout.Button("Group & Close"))
		{
            GroupSelectedTransforms(groupName, gTag, parentPosition, centerPosition, centerBottomY);
            this.Close();
        }
	
        GUILayout.EndHorizontal();  
    }

	[MenuItem("Edit/Ungroup %#g", false, -997)]
    static void UngroupMenu()
    {
        Transform[] currentSelection = Selection.GetTransforms(SelectionMode.Editable);
		Transform grandfatherObject;
        GameObject parentObject;
        if (currentSelection.Length == 0)
            return;

		foreach (Transform groupMember in currentSelection)
        {
            parentObject = groupMember.parent.gameObject;
			grandfatherObject = parentObject.transform.parent;
			Undo.SetTransformParent(groupMember.transform, grandfatherObject, "Ungroup");
			groupMember.transform.SetSiblingIndex(grandfatherObject.GetSiblingIndex());
            if (parentObject.GetComponents<Component>().Length < 2)
            {
                if (parentObject.transform.childCount == 0)
                {
                    Debug.Log("Empty group object \"" + parentObject.name + "\" deleted!");
					Undo.DestroyObjectImmediate(parentObject);
                }
            }
        }
    }

    static bool CheckGroupSelection()
    {
        if (Selection.activeTransform == null)
            return false;

        if (Selection.activeTransform.parent == null)
            return false;

        return true;
    }

    [MenuItem("Edit/Ungroup %#g", true, -997)]
    static bool ValidateGroupSelection()
    {
        return CheckGroupSelection();
    }
}