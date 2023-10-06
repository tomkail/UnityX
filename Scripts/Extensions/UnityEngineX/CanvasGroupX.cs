using System.Collections.Generic;
using UnityEngine;

public static class CanvasGroupX {
    // Taken from https://github.com/Unity-Technologies/uGUI/blob/2019.1/UnityEngine.UI/UI/Core/Selectable.cs
    private static readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();
    public static bool CanvasGroupsAllowInteraction (GameObject gameObject) {
        // Figure out if parent groups allow interaction
        // If no interaction is alowed... then we need
        // to not do that :)
        var groupAllowInteraction = true;
        Transform t = gameObject.transform;
        while (t != null)
        {
            t.GetComponents(m_CanvasGroupCache);
            bool shouldBreak = false;
            for (var i = 0; i < m_CanvasGroupCache.Count; i++)
            {
                // if the parent group does not allow interaction
                // we need to break
                if (!m_CanvasGroupCache[i].interactable)
                {
                    groupAllowInteraction = false;
                    shouldBreak = true;
                }
                // if this is a 'fresh' group, then break
                // as we should not consider parents
                if (m_CanvasGroupCache[i].ignoreParentGroups)
                    shouldBreak = true;
            }
            if (shouldBreak)
                break;

            t = t.parent;
        }
        return groupAllowInteraction;
    }

    // Untested
    public static float CanvasGroupsAlpha (GameObject gameObject) {
        var groupAlpha = 1f;
        Transform t = gameObject.transform;
        while (t != null) {
            t.GetComponents(m_CanvasGroupCache);
            bool shouldBreak = false;
            for (var i = 0; i < m_CanvasGroupCache.Count; i++)
            {
                groupAlpha *= m_CanvasGroupCache[i].alpha;
                
                // if this is a 'fresh' group, then break
                // as we should not consider parents
                if (m_CanvasGroupCache[i].ignoreParentGroups)
                    shouldBreak = true;
            }
            if (shouldBreak)
                break;

            t = t.parent;
        }
        return groupAlpha;
    }
}
