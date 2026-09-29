using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Equivalent to BoxOutline with halfSampleCountX/Y == 1 (the 8 neighbours of a 3x3 grid). Kept as a
// distinct serialized public MonoBehaviour because it may be attached in external scenes; removing it
// would break their serialization.
public class Outline8 : ModifiedShadow
{
    public override void ModifyVertices(List<UIVertex> verts)
    {
        if (!IsActive())
            return;

        verts.Capacity = verts.Count * 9;
        var original = verts.Count;
        var count = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (!(x == 0 && y == 0))
                {
                    var next = count + original;
                    ApplyShadow(verts, effectColor, count, next, effectDistance.x * x, effectDistance.y * y);
                    count = next;
                }
            }
        }
    }
}
