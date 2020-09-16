﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// The behaviour of this class is almost the same as the original except:
/// 1. It absorbs version differences.
/// 2. It corrects the calculation of vertex list capacity.
/// </summary>
public class ModifiedShadow : Shadow
{
    protected new void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
    {
        UIVertex vt;

        // The capacity calculation of the original version seems wrong.
        var neededCpacity = verts.Count + (end - start);
        if (verts.Capacity < neededCpacity)
            verts.Capacity = neededCpacity;

        for (int i = start; i < end; ++i)
        {
            vt = verts[i];
            verts.Add(vt);

            Vector3 v = vt.position;
            v.x += x;
            v.y += y;
            vt.position = v;
            var newColor = color;
            if (useGraphicAlpha)
                newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
            vt.color = newColor;
            verts[i] = vt;
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!this.IsActive())
            return;
        
        List<UIVertex> list = new List<UIVertex>();
        vh.GetUIVertexStream(list);
        
        ModifyVertices(list);

        vh.Clear();
        vh.AddUIVertexTriangleStream(list);
    }

    public virtual void ModifyVertices(List<UIVertex> verts)
    {
    }
}
