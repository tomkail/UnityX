using System;
using System.Collections.Generic;
using System.Linq;
using FlexLayout;
using UnityEngine;

public static class SLayoutUtils { 
    // public enum LayoutDirection {
    //     Row, // left to right
    //     RowReverse, // right to left
    //     Column, // top to bottom
    //     ColumnReverse // bottom to top
    // }

    public enum Axis {
        X = 0,
        Y = 1,
        Both = 2,
    }
    
    #region Alignment
    // Aligns the items with the parent layout, using an anchor and a pivot to determine position. Takes transform hierarchy into account so that this function may be animated.
    public static void Align(SLayout parentLayout, IEnumerable<SLayout> layouts, Axis axis, float parentAnchor, float layoutPivot, float offset = 0) {
        if (layouts.Any(x => x == null)) {
            Debug.LogError("Null layout found in AutoLayoutWithSpacing");
            return;
        }
        
        var parentAnchorPos = Vector2.zero;
        parentAnchorPos[(int)axis] = parentLayout.targetSize[(int)axis] * parentAnchor;
        var orderedLayouts = OrderByTransformDepth(layouts);
        foreach (var layout in orderedLayouts) {
            var anchorPos = parentLayout.ConvertPositionToTarget(parentAnchorPos, layout.parent);
            var position = anchorPos[(int)axis] - layout.targetSize[(int)axis] * layoutPivot + offset;
            SetPositionOnAxis(layout, position, axis);
        }
    }
    
    // Aligns the left edge of all the items to the left edge of the parent layout. Shorthand for AlignHorizontal(parentLayout, layouts, 0f, 0f, offset);
    public static void AlignLeft(SLayout parentLayout, IEnumerable<SLayout> layouts, float offset = 0) {
        AlignHorizontal(parentLayout, layouts, 0f, 0f, offset);
    }
    
    // Aligns the right edge of all the items to the right edge of the parent layout. Shorthand for AlignHorizontal(parentLayout, layouts, 1f, 1f, offset);
    public static void AlignRight(SLayout parentLayout, IEnumerable<SLayout> layouts, float offset = 0) {
        AlignHorizontal(parentLayout, layouts, 1f, 1f, offset);
    }

    // Aligns the center of all the items to the center of the parent layout. Shorthand for AlignHorizontal(parentLayout, layouts, 0.5f, 0.5f, offset);
    public static void AlignHorizontalCenters(SLayout parentLayout, IEnumerable<SLayout> layouts, float offset = 0) {
        AlignHorizontal(parentLayout, layouts, 0.5f, 0.5f, offset);
    }

    // Aligns the x coordinate of all the layouts to the x coordinate of the parent layout, according the the anchor and pivot settings
    public static void AlignHorizontal(SLayout parentLayout, IEnumerable<SLayout> layouts, float parentAnchor, float layoutPivot, float offset = 0) {
        Align(parentLayout, layouts, Axis.X, parentAnchor, layoutPivot, offset);
    }
    
    // Aligns the center of all the items to the center of the parent layout. Shorthand for AlignVertical(parentLayout, layouts, 0.5f, 0.5f, offset);
    public static void AlignVerticalCenters(SLayout parentLayout, IEnumerable<SLayout> layouts, float offset = 0) {
        AlignVertical(parentLayout, layouts, 0.5f, 0.5f, offset);
    }
    
    // Aligns the y coordinate of all the layouts to the y coordinate of the parent layout, according the the anchor and pivot settings
    public static void AlignVertical(SLayout parentLayout, IEnumerable<SLayout> layouts, float parentAnchor, float layoutPivot, float offset = 0) {
        Align(parentLayout, layouts, Axis.Y, parentAnchor, layoutPivot, offset);
    }
    #endregion
    

    #region Layout
    // A quick and simple auto-layout function. Returns the total width of the layout, ignoring the offset.
    public static float AutoLayout(SLayout parentLayout, IList<SLayout> layouts, Axis axis, float offset, float spacing, float minPadding, float maxPadding) {
        var localLayoutPositions = new List<Vector2>();
        float axisPos = minPadding + offset;
        for (var index = 0; index < layouts.Count; index++) {
            var layout = layouts[index];
            var localLayoutPos = Vector2.zero;
            localLayoutPos[(int)axis] = axisPos;

            localLayoutPositions.Add(localLayoutPos);

            axisPos += layout.targetSize[(int)axis] + (index < layouts.Count - 1 ? spacing : 0);
        }

        ApplyLocalLayoutPositionsInTransformDepthOrder(parentLayout, layouts, localLayoutPositions, axis);
        return axisPos + maxPadding - offset;
    }



    // public static float AutoLayoutWithSpacing(FlexSLayout parentLayout, IList<SLayout> layouts, Axis axis, bool expandItemsToFill = false, float pivot = 0.5f) {
    //     return AutoLayout(
    //         parentLayout,
    //         layouts.Select(layout => new FlexSLayoutItem(Item.Fixed(layout.targetSize[(int) axis]), layout)).ToArray(),
    //         axis
    //     );
    // }

    // Automatically lays out items within the parent using a pre-determined spacing, independent of the transform hierarchy of the items.
    public static float AutoLayoutWithSpacing(SLayout parentLayout, IList<SLayout> layouts, Axis axis, float spacing, bool expandItemsToFill = false, float minPadding = 0, float maxPadding = 0, float pivot = 0.5f) {
        // return AutoLayout(
        //     new FlexSLayout(Container.Fixed(parentLayout.targetSize[(int)axis]).SetSpacing(spacing).SetPaddingMin(minPadding).SetPaddingMax(maxPadding), parentLayout),
        //     layouts.Select(layout => new FlexSLayoutItem(Item.Fixed(layout.targetSize[(int)axis]), layout)).ToArray(),
        //     axis
        // );
        
        if (layouts.Any(x => x == null)) {
            Debug.LogError("Null layout found in AutoLayoutWithSpacing");
            return 0;
        }
        
        var offset = 0f;
        if (expandItemsToFill) {
            var totalSpacingAndPadding = spacing * (layouts.Count - 1) + minPadding + maxPadding;
            var elementSize = (parentLayout.targetSize[(int)axis] - totalSpacingAndPadding) / layouts.Count;
            for (var index = 0; index < layouts.Count; index++) {
                var layout = layouts[index];
                var size = layout.targetSize;
                size[(int)axis] = elementSize;
                layout.targetSize = size;
            }
        } else {
            float totalContentSize = layouts.Sum(x => x.targetSize[(int)axis]) + spacing * (layouts.Count() - 1) + minPadding + maxPadding;
            var spareSpace = parentLayout.targetSize[(int)axis] - totalContentSize;
            offset = spareSpace * pivot;
        }

        return AutoLayout(parentLayout, layouts, axis, offset, spacing, minPadding, maxPadding);
    }

    public static float AutoLayoutXWithSpacing (SLayout parentLayout, IList<SLayout> layouts, float spacing, bool expandItemsToFill = false, float minPadding = 0, float maxPadding = 0, float pivot = 0.5f) {
        return AutoLayoutWithSpacing(parentLayout, layouts, Axis.X, spacing, expandItemsToFill, minPadding, maxPadding, pivot);
    }
    public static float AutoLayoutYWithSpacing (SLayout parentLayout, IList<SLayout> layouts, float spacing, bool expandItemsToFill = false, float minPadding = 0, float maxPadding = 0, float pivot = 0.5f) {
        return AutoLayoutWithSpacing(parentLayout, layouts, Axis.Y, spacing, expandItemsToFill, minPadding, maxPadding, pivot);
    }

    
    // Automatically lays out items to fill the parent, independent of the transform hierarchy of the items.
    // public static float AutoLayoutAndFillSpace (SLayout parentLayout, IList<SLayout> layouts, Axis axis, float minPadding = 0, float maxPadding = 0) {
    //     float totalContentSize = layouts.Sum(x => x.targetSize[(int)axis]) + minPadding + maxPadding;
    //     var spareSpace = parentLayout.targetSize[(int)axis] - totalContentSize;
    //     var spacing = spareSpace / (layouts.Count() - 1);
    //     
    //     return AutoLayout(parentLayout, layouts, axis, 0, spacing, minPadding, maxPadding);
    // }
    //
    // public static float AutoLayoutXAndFillSpace (SLayout parentLayout, IList<SLayout> layouts, float minPadding = 0, float maxPadding = 0) {
    //     return AutoLayoutAndFillSpace(parentLayout, layouts, Axis.X, minPadding, maxPadding);
    // }
    // public static float AutoLayoutYAndFillSpace (SLayout parentLayout, IList<SLayout> layouts, float minPadding = 0, float maxPadding = 0) {
    //     return AutoLayoutAndFillSpace(parentLayout, layouts, Axis.Y, minPadding, maxPadding);
    // }
    //
    //
    // // Automatically lays out items to fill the parent, independent of the transform hierarchy of the items. Adds extra spacing at the top/bottom, according to the amount of space left over.
    // public static float AutoLayoutAndFillSpaceWithSpacingAtEdge(SLayout parentLayout, IList<SLayout> layouts, Axis axis, float minPadding = 0, float maxPadding = 0, float pivot = 0.5f) {
    //     float totalContentSize = layouts.Sum(x => x.targetSize[(int)axis]);
    //     var spareSpace = parentLayout.targetSize[(int)axis] - totalContentSize;
    //     var spacing = spareSpace / layouts.Count();
    //     
    //     return AutoLayout(parentLayout, layouts, axis, spacing * pivot, spacing, minPadding, maxPadding);
    // }
    //
    // public static float AutoLayoutXAndFillSpaceWithSpacingAtEdge (SLayout parentLayout, IList<SLayout> layouts, float minPadding = 0, float maxPadding = 0, float pivot = 0.5f) {
    //     return AutoLayoutAndFillSpaceWithSpacingAtEdge(parentLayout, layouts, Axis.X, minPadding, maxPadding, pivot);
    // }
    // public static float AutoLayoutYAndFillSpaceWithSpacingAtEdge (SLayout parentLayout, IList<SLayout> layouts, float minPadding = 0, float maxPadding = 0, float pivot = 0.5f) {
    //     return AutoLayoutAndFillSpaceWithSpacingAtEdge(parentLayout, layouts, Axis.Y, minPadding, maxPadding, pivot);
    // }

    

    public static float AutoLayout(SLContainer flexLayout, IList<SLItem> layoutItems, Axis axis) {
        if (flexLayout == null) {
            Debug.LogError("AutoLayoutWithDynamicSizing can't run because parentLayout is null");
            return 0;
        }
        if (layoutItems == null || layoutItems.Count == 0) return flexLayout.totalPadding;
        
        var layoutResult = FlexLayout.FlexLayout.GetLayoutRanges(flexLayout, layoutItems.Cast<Item>().ToList());

        if(flexLayout.expandSize && flexLayout.layout != null)
            SetSizeOnAxis(flexLayout.layout, layoutResult.containerSize, axis);

        List<SLayout> layouts = new List<SLayout>();
        List<Vector2> positions = new List<Vector2>();
        for (var index = 0; index < layoutResult.ranges.Count; index++) {
            var layoutItem = layoutItems[index];
            if(layoutItem.layouts == null) continue;
            foreach (var layout in layoutItem.layouts) {
                // We might want to warn if this occurs but it doesn't affect the layout so it's not a big deal
                if (layout == null) continue;
                
                Vector2 position = layout.position;
                
                if (layoutItem.fillSpace) {
                    SetSizeOnAxis(layout, layoutResult.ranges[index].y - layoutResult.ranges[index].x, axis);
                    position[(int) axis] = layoutResult.ranges[index].x;
                }
                // If not setting size, then set position according to the pivot of the item 
                else {
                    position[(int) axis] = (Mathf.Lerp(layoutResult.ranges[index].x, layoutResult.ranges[index].y, layoutItem.fillSpacePivot) - layout.targetSize[(int) axis] * layoutItem.fillSpacePivot);
                }
                positions.Add(position);
                

                layouts.Add(layout);
            }
        }

        ApplyLocalLayoutPositionsInTransformDepthOrder(flexLayout.layout, layouts, positions, axis);
        return layoutResult.containerSize;
    }
    
    public static void FillContainer(SLayout parentLayout, IEnumerable<SLayout> layouts, Axis axis, float minPadding = 0, float maxPadding = 0) {
        AutoLayout(
            SLContainer.Fixed(parentLayout.targetSize[(int)axis]).SetPaddingMin(minPadding).SetPaddingMax(maxPadding).SetLayout(parentLayout).SetExpandSize(false), 
            new List<SLItem> {SLItem.Flexible().SetLayouts(layouts.ToArray())}, 
            axis
        );
    }
    #endregion
    
    #region Utils
    // Layout the items according to positions relative to the parentLayout. Takes transform hierarchy into account so that this function may be animated.
    public static void ApplyLocalLayoutPositionsInTransformDepthOrder(SLayout parentLayout, IList<SLayout> layouts, IList<Vector2> localLayoutPositions, Axis axis = Axis.Both) {
        if (parentLayout == null) {
            Debug.LogError("AutoLayoutWithDynamicSizing can't run because parentLayout is null");
            return;
        }
        var order = GetOrderTransformationByTransformDepth(layouts);
        var orderedLayouts = ReorderedList(layouts, order);
        var orderedLocalLayoutPositions = ReorderedList(localLayoutPositions, order);
        for (var index = 0; index < localLayoutPositions.Count; index++) {
            var layout = orderedLayouts[index];
            var localLayoutPos = orderedLocalLayoutPositions[index];
            var convertedPosition = parentLayout.ConvertPositionToTarget(localLayoutPos, layout.parent);
            if(float.IsNaN(convertedPosition.x) || float.IsNaN(convertedPosition.y)) {
                Debug.LogError($"AutoLayoutWithDynamicSizing can't run because convertedPosition is NaN. localLayoutPos is {localLayoutPos}, layout.parent is {layout.parent}");
                return;
            }
            if (axis == Axis.Both) layout.position = convertedPosition;
            else SetPositionOnAxis(layout, convertedPosition[(int)axis], axis);
        }
    }
    
    static void SetPositionOnAxis(SLayout layout, float position, Axis axis) {
        if (axis == Axis.X) layout.x = position;
        else if (axis == Axis.Y) layout.y = position;
    }
    
    static void SetSizeOnAxis(SLayout layout, float size, Axis axis) {
        if (axis == Axis.X) layout.width = size;
        else if (axis == Axis.Y) layout.height = size;
    }
    
    
    // Ordering layouts by depth ensures that animations are applied in the correct order
    static int[] GetOrderTransformationByTransformDepth(IList<SLayout> layouts) {
        return GetOrder(layouts, item => GetTransformDepth(item.transform));
        static int GetTransformDepth(Transform transform) {
            if (transform.parent == null) return 0;
            else return 1 + GetTransformDepth(transform.parent);
        }
    }
    
    
    static int[] GetOrder<T, TKey>(IList<T> list, Func<T, TKey> selector)
    {
        return list
            .Select((value, index) => new { Value = value, Index = index })
            .OrderBy(item => selector(item.Value))
            .Select(item => item.Index)
            .ToArray();
    }

    static List<T> ReorderedList<T>(IList<T> list, int[] order) {
        return order.Select(index => list[index]).ToList();
    }
    
    static IOrderedEnumerable<SLayout> OrderByTransformDepth(IEnumerable<SLayout> layouts) {
        return layouts.OrderBy(x => GetTransformDepth(x.transform));
        static int GetTransformDepth(Transform transform) {
            if (transform.parent == null) return 0;
            else return 1 + GetTransformDepth(transform.parent);
        }
    }

    #endregion
}


// Wraps layout item params and the layouts to apply the output to, if any.
// Also contains arguments defining how the layout params should be applied to the layouts
[Serializable]
public class SLContainer : Container {
    public SLayout layout;
    
    // If true, sets the size of the layouts to the space allocated for them.
    public bool expandSize = true;
    
    public new static SLContainer Fixed(float size) {
        var layoutItem = new SLContainer();
        return layoutItem.SetFixedSize(size);
    }

    public new static SLContainer Flexible(float minSize = 0, float maxSize = float.MaxValue) {
        var layoutItem = new SLContainer();
        return layoutItem.SetFlexibleSize(minSize, maxSize);
    }
    
    
    public static SLContainer Fixed(SLayout layout, SLayoutUtils.Axis axis) {
        var layoutItem = new SLContainer().SetExpandSize(false).SetLayout(layout);
        layoutItem.SetFixedSize(layout.targetSize[(int) axis]);
        return layoutItem;
    }
    
    // public new static SLContainer Flexible(SLayout layout, float minSize = 0, float maxSize = float.MaxValue) {
    //     var layoutItem = new SLContainer();
    //     return layoutItem.SetFlexibleSize(minSize, maxSize);
    // }
    // public static SLContainer Flexible(SLayout layout, SLayoutUtils.Axis axis) {
    //     var layoutItem = new SLContainer();
    //     layoutItem.SetFixedSize(layout.targetSize[(int) axis]);
    //     return layoutItem.SetExpandSize(true).SetLayout(layout);
    // }

    public SLContainer SetLayout(SLayout layout) {
        this.layout = layout;
        return this;
    }
    
    public SLContainer SetExpandSize(bool value) {
        expandSize = value;
        return this;
    }

    public new SLContainer SetFixedSize(float fixedSize) => (SLContainer)base.SetFixedSize(fixedSize);
        
    public new SLContainer SetFlexibleSize(float minSize, float maxSize) => (SLContainer)base.SetFlexibleSize(minSize, maxSize);

    public new SLContainer SetPadding(float value) => (SLContainer)base.SetPadding(value);
        
    public new SLContainer SetPadding(float minPadding, float maxPadding) => (SLContainer)base.SetPadding(minPadding, maxPadding);

    public new SLContainer SetPaddingMin(float value) => (SLContainer)base.SetPaddingMin(value);

    public new SLContainer SetPaddingMax(float value) => (SLContainer)base.SetPaddingMax(value);

    public new SLContainer SetSpacing(float value) => (SLContainer)base.SetSpacing(value);

    public new SLContainer SetSurplusOffsetPivot(float value) => (SLContainer)base.SetSurplusOffsetPivot(value);

    public new SLContainer SetSurplusSpacePaddingRatio(float value) => (SLContainer)base.SetSurplusSpacePaddingRatio(value);

    public new SLContainer SetReversed(bool value) => (SLContainer)base.SetReversed(value);
}

[Serializable]
public class SLItem : Item {
    // This may be null.
    public List<SLayout> layouts;

    // If true, sets the size of the layouts to the space allocated for them.
    public bool fillSpace = true;
    // If fillSpace is false, determines where the layouts are positioned within the space allocated for them.
    public float fillSpacePivot = 0.5f;
    
    public static SLItem Fixed(SLayout layout, SLayoutUtils.Axis axis, bool expandSize = true, float pivot = 0.5f) {
        var layoutItem = new SLItem().SetFillSpace(expandSize, pivot).SetLayouts(layout);
        layoutItem.SetFixedSize(layout.targetSize[(int) axis]);
        return layoutItem;
    }
    
    // public static FlexSLayoutItem Max(SLayoutUtils.Axis axis, params SLayout[] layouts) {
    //     var size = layouts.Max(layout => layout.targetSize[(int) axis]);
    //     layoutItemParams = Item.Fixed();
    // }
    
    public new static SLItem Fixed(float size) {
        var layoutItem = new SLItem();
        return layoutItem.SetFixedSize(size);
    }

    public new static SLItem Flexible(float minSize = 0, float maxSize = float.MaxValue, float weight = 1) {
        var layoutItem = new SLItem();
        return layoutItem.SetFlexibleSize(minSize, maxSize).SetWeight(weight);
    }
    
    public SLItem SetLayouts (params SLayout[] layouts) {
        if(layouts is {Length: > 0}) this.layouts = new List<SLayout>(layouts);
        return this;
    }
    
    public SLItem SetFillSpace(bool fillSpace, float fillSpacePivot) {
        this.fillSpace = fillSpace;
        this.fillSpacePivot = fillSpacePivot;
        return this;
    }
    
    public new SLItem SetFixedSize(float fixedSize) => (SLItem)base.SetFixedSize(fixedSize);
    public new SLItem SetFlexibleSize(float minSize, float maxSize) => (SLItem)base.SetFlexibleSize(minSize, maxSize);
    public new SLItem SetWeight(float weight) => (SLItem)base.SetWeight(weight);
}