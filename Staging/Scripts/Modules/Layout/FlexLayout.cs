using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityX.Layout {
    /// <summary>
    /// A 1D flexbox-style layout solver. Given a <see cref="Container"/> and a list of <see cref="Item"/>s,
    /// <see cref="GetLayoutRanges{TContainer,TItem}"/> resolves each item's size and position along a single
    /// axis (call it once per axis to build a 2D layout).
    ///
    /// Items are either fixed (a set size) or flexible (grow to fill leftover space, bounded by min/max and
    /// shared out by weight, like CSS flex-grow). The container is likewise fixed or flexible; a flexible
    /// container shrink-wraps its content between its own min and max size. Any leftover space is distributed
    /// according to the container's <see cref="Container.SurplusMode"/>.
    ///
    /// <code>
    /// var container = Container.Fixed(300).SetPadding(8).SetSpacing(10);
    /// var items = new[] {
    ///     Item.Fixed(50),                        // fixed 50 units
    ///     Item.Flexible(minSize: 20, weight: 1), // grows to fill remaining space
    ///     Item.Flexible(minSize: 20, weight: 2), // grows twice as fast as the item above
    /// };
    /// FlexLayout.Result result = FlexLayout.GetLayoutRanges(container, items);
    /// // result.ranges[i] is the (start, end) position of item i; result.containerSize is the total extent.
    /// </code>
    /// </summary>
    public static class FlexLayout {
        /// <summary>The result of a layout pass.</summary>
        [Serializable]
        public class Result {
            /// <summary>Total extent consumed along the axis, including padding.</summary>
            public float containerSize;
            /// <summary>Per-item positions in item order, where x is the start edge and y is the end edge.</summary>
            public List<Vector2> ranges;
        }

        [Flags]
        public enum InvalidSettingsType {
            None = 0,
            ContainerNull = 1 << 0,
            ContainerAndChildBothFlexible = 1 << 1,
            ItemsNull = 1 << 2,
        }
        /// <summary>
        /// Checks for settings that <see cref="GetLayoutRanges{TContainer,TItem}"/> can't handle. Returns true if
        /// a problem is found, with the specific problems flagged in <paramref name="invalidSettingsType"/>.
        /// </summary>
        public static bool DetectInvalidSettings<TContainer, TItem>(TContainer layoutParams, IList<TItem> items, out InvalidSettingsType invalidSettingsType) where TContainer : Container where TItem : Item {
            invalidSettingsType = 0;

            if (layoutParams == null) {
                invalidSettingsType |= InvalidSettingsType.ContainerNull;
                return true;
            }

            if (items == null) {
                invalidSettingsType |= InvalidSettingsType.ItemsNull;
                return true;
            }

            if (layoutParams.flexible && layoutParams.maxSize == float.MaxValue && items.Any(i => i.flexible && i.maxSize == float.MaxValue)) {
                invalidSettingsType |= InvalidSettingsType.ContainerAndChildBothFlexible;
                return true;
            }
            return false;
        }

        static void TryThrowExceptionForInvalidSettings<TContainer, TItem>(TContainer layoutParams, IList<TItem> items) where TContainer : Container where TItem : Item {
            if(!DetectInvalidSettings(layoutParams, items, out InvalidSettingsType invalidSettings)) return;
            if(invalidSettings.HasFlag(InvalidSettingsType.ContainerNull)) throw new ArgumentException("GetLayoutRanges can't run because the container is null");
            if(invalidSettings.HasFlag(InvalidSettingsType.ItemsNull)) throw new ArgumentException("GetLayoutRanges can't run because the items list is null");
            if(invalidSettings.HasFlag(InvalidSettingsType.ContainerAndChildBothFlexible)) {
                throw new ArgumentException($"When using a flexible container with an infinite max size, all flexible items must have a finite max size.");
            }
        }

        /// <summary>
        /// Resolves the size and position of each item along the axis.
        /// Throws <see cref="ArgumentException"/> if the settings are invalid (see <see cref="DetectInvalidSettings{TContainer,TItem}"/>).
        /// </summary>
        public static Result GetLayoutRanges<TContainer, TItem>(TContainer layoutParams, IList<TItem> items) where TContainer : Container where TItem : Item {
            TryThrowExceptionForInvalidSettings(layoutParams, items);
            Vector2 surplusOffsetPadding = Vector2.zero;
            float totalItemSpacing = layoutParams.spacing;
            
            float availableFlexibleSpace = (layoutParams.flexible ? layoutParams.maxInnerSize : layoutParams.fixedInnerSize);
            
            float totalFixedItemSize = items.Where(i => !i.flexible).Sum(i => i.fixedSize);
            availableFlexibleSpace -= totalFixedItemSize;
            
            float totalMinFlexibleItemSize = items.Where(i => i.flexible).Sum(i => i.minSize);
            availableFlexibleSpace -= totalMinFlexibleItemSize;
                
            float totalFixedSpacing = layoutParams.spacing * Math.Max(0, items.Count - 1);
            availableFlexibleSpace -= totalFixedSpacing;

            float totalMarginMin = items.Sum(i => i.marginMin);
            float totalMarginMax = items.Sum(i => i.marginMax);
            availableFlexibleSpace -= totalMarginMin + totalMarginMax;
            
            // Final resolved size for each item, indexed by position. Flexible items start at their min size
            // and grow below; fixed items keep their fixed size. Indexing by position (rather than a dictionary
            // keyed by item reference) means the same Item instance can safely appear more than once in the list.
            var itemSizes = new float[items.Count];
            for (var i = 0; i < items.Count; i++)
                itemSizes[i] = items[i].flexible ? items[i].minSize : items[i].fixedSize;

            while (availableFlexibleSpace > 0) {
                // Get the total weight of the flexible items that can still grow
                float totalWeight = 0f;
                for (var i = 0; i < items.Count; i++) {
                    if (items[i].flexible && itemSizes[i] < items[i].maxSize) totalWeight += items[i].weight;
                }

                if (totalWeight == 0) break;

                float spaceAllocatedThisIteration = 0;

                for (var i = 0; i < items.Count; i++) {
                    var item = items[i];
                    if (!item.flexible || itemSizes[i] >= item.maxSize)
                        continue;

                    float weightFraction = item.weight / totalWeight;
                    float spaceForThisItem = weightFraction * availableFlexibleSpace;
                    float spaceActuallyUsed = Math.Min(spaceForThisItem, item.maxSize - itemSizes[i]);

                    itemSizes[i] += spaceActuallyUsed;
                    spaceAllocatedThisIteration += spaceActuallyUsed;
                }

                // Reduce the available space by the space that was allocated in this iteration
                availableFlexibleSpace -= spaceAllocatedThisIteration;
            }

            // The total size of the content and fixed spacing. Any additional space can be used for extra spacing or padding.
            float totalFlexibleItemSize = 0f;
            for (var i = 0; i < items.Count; i++)
                if (items[i].flexible) totalFlexibleItemSize += itemSizes[i];
            float contentSizeAndFixedSpacing = totalFixedItemSize + totalFlexibleItemSize + totalFixedSpacing;
            // The space that's left after the content and fixed spacing is taken into account to be used for extra spacing or padding.
            // Can be negative, if the content is larger than the maximum size of the container.
            var flexibleSpacing = 0f;
            if (layoutParams.flexible) {
                flexibleSpacing = layoutParams.minInnerSize - contentSizeAndFixedSpacing - totalMarginMin - totalMarginMax;
                var maxFlexibleSpacing = Mathf.Min(0, layoutParams.maxInnerSize - contentSizeAndFixedSpacing - totalMarginMin - totalMarginMax);
                flexibleSpacing = Mathf.Max(flexibleSpacing, maxFlexibleSpacing);
            } else {
                flexibleSpacing = layoutParams.fixedInnerSize - contentSizeAndFixedSpacing - totalMarginMin - totalMarginMax;
            }


            if (layoutParams.surplusMode == Container.SurplusMode.Offset) {
                surplusOffsetPadding.x = flexibleSpacing * layoutParams.surplusOffsetPivot;
                surplusOffsetPadding.y = flexibleSpacing * (1f - layoutParams.surplusOffsetPivot);
            } else if (layoutParams.surplusMode == Container.SurplusMode.Space) {
                // When surplusSpacePaddingRatio is 1 we're effectively pretending there are 2 zero-size items at the start and end of the list.
                var fakeItemCountForFlexibleSpacing = (items.Count - 1) + layoutParams.surplusSpacePaddingRatio * 2;
                // Guard against a zero/negative denominator (e.g. a single item with space-between, or an empty
                // list), which would otherwise yield NaN/Infinity and corrupt every range. In that case there
                // are no gaps to distribute into, so the surplus is simply left unused (items sit at the start).
                if (fakeItemCountForFlexibleSpacing > 0) {
                    var flexibleItemSpacing = flexibleSpacing / fakeItemCountForFlexibleSpacing;
                    surplusOffsetPadding.x = flexibleItemSpacing * layoutParams.surplusSpacePaddingRatio;
                    surplusOffsetPadding.y = flexibleItemSpacing * layoutParams.surplusSpacePaddingRatio;
                    totalItemSpacing += flexibleItemSpacing;
                }
            }
            
            var ranges = new List<Vector2>();
            var currentItemPosition = 0f;
            for (var index = 0; index < items.Count; index++) {
                var item = items[index];
                float itemSize = itemSizes[index];
                if (layoutParams.reversed) {
                    // When reversed, we first account for marginMax as it's now at the 'start'.
                    currentItemPosition -= item.marginMax;
                    ranges.Add(new Vector2(currentItemPosition - itemSize, currentItemPosition));
                    currentItemPosition -= itemSize;
                    // Only apply spacing if it's not the last item (now the first visually).
                    if (index < items.Count-1) currentItemPosition -= totalItemSpacing;
                    currentItemPosition -= item.marginMin;
                } else {
                    // Apply marginMin before positioning the item.
                    currentItemPosition += item.marginMin;
                    ranges.Add(new Vector2(currentItemPosition, currentItemPosition + itemSize));
                    currentItemPosition += itemSize;
                    // Apply spacing and marginMax if it's not the last item.
                    if (index < items.Count-1) currentItemPosition += totalItemSpacing + item.marginMax;
                }
            }

            float itemOffset;
            if (layoutParams.reversed) {
                itemOffset = layoutParams.paddingMax + -currentItemPosition + surplusOffsetPadding.x;
            } else {
                itemOffset = layoutParams.paddingMin + surplusOffsetPadding.x;
            }
            for (var index = 0; index < ranges.Count; index++) {
                ranges[index] += Vector2.one * itemOffset;
            }

            var totalSizeConsumedIncludingPadding = surplusOffsetPadding.y;
            if (layoutParams.reversed) totalSizeConsumedIncludingPadding += (items.Count > 0 ? (ranges.First().y + items.First().marginMax) : 0) + layoutParams.paddingMin;
            else totalSizeConsumedIncludingPadding += (items.Count > 0 ? (ranges.Last().y + items.Last().marginMax) : 0) + layoutParams.paddingMax;

            return new Result {
                containerSize = totalSizeConsumedIncludingPadding,
                ranges = ranges
            };
        }
    }

    /// <summary>Sizing fields shared by <see cref="Container"/> and <see cref="Item"/>.</summary>
    [Serializable]
    public class LayoutElement {
        // When true the element is flexible (uses minSize/maxSize); when false it's a fixed size (uses fixedSize).
        public bool flexible;

        // The size used when not flexible.
        public float fixedSize;

        // The minimum and maximum size used when flexible.
        public float minSize;
        public float maxSize;
    }

    /// <summary>The area that <see cref="Item"/>s are laid out within: its size, padding, spacing and surplus behaviour.</summary>
    [Serializable]
    public class Container : LayoutElement {
        // Inner size = outer size minus padding; this is the space actually available to the items.
        public float fixedInnerSize => fixedSize - totalPadding;
        public float minInnerSize => minSize - totalPadding;
        public float maxInnerSize => maxSize - totalPadding;

        // Padding reserved inside the container, before (min) and after (max) the items.
        public float paddingMin;
        public float paddingMax;
        public float totalPadding => paddingMin + paddingMax;

        // The fixed spacing between adjacent items. Extra spacing may be added when surplusMode is Space.
        public float spacing;

        // Describes what happens to leftover space when the items don't fill the container.
        public SurplusMode surplusMode = SurplusMode.Offset;
        public enum SurplusMode {
            // Surplus becomes padding placed around the items, positioned by surplusOffsetPivot.
            Offset,
            // Surplus is distributed as extra space between (and optionally around) the items, controlled by surplusSpacePaddingRatio.
            Space,
        }


        // Used when surplusMode is Offset. Aligns the items within the leftover space, like flexbox justify-content:
        // 0 = start, 0.5 = centre, 1 = end. Relative to layout direction, so it is not flipped by 'reversed'.
        public float surplusOffsetPivot = 0.5f;

        // Used when surplusMode is Space. Corresponds to flexbox's justify-content space options:
        // 0 = space-between, 0.5 = space-around, 1 = space-evenly.
        public float surplusSpacePaddingRatio;

        // When reversed, the layout runs from the max edge to the min edge, starting with the last item rather than the first.
        // Note that surplusOffsetPivot is not reversed.
        public bool reversed;
        
        public static Container Fixed(float size) {
            var layoutItem = new Container();
            return layoutItem.SetFixedSize(size);
        }

        public static Container Flexible(float minSize = 0, float maxSize = float.MaxValue) {
            var layoutItem = new Container();
            return layoutItem.SetFlexibleSize(minSize, maxSize);
        }

        public Container SetFixedSize(float fixedSize) {
            flexible = false;
            this.fixedSize = fixedSize;
            return this;
        }
        
        public Container SetFlexibleSize(float minSize, float maxSize) {
            flexible = true;
            this.minSize = minSize;
            this.maxSize = maxSize;
            return this;
        }

        public Container SetPadding(float value) {
            paddingMin = paddingMax = value;
            return this;
        }
        
        public Container SetPadding(float minPadding, float maxPadding) {
            paddingMin = minPadding;
            paddingMax = maxPadding;
            return this;
        }

        public Container SetPaddingMin(float value) {
            paddingMin = value;
            return this;
        }

        public Container SetPaddingMax(float value) {
            paddingMax = value;
            return this;
        }

        public Container SetSpacing(float value) {
            spacing = value;
            return this;
        }

        public Container SetSurplusOffsetPivot(float value) {
            surplusMode = SurplusMode.Offset;
            surplusOffsetPivot = value;
            return this;
        }

        public Container SetSurplusSpacePaddingRatio(float value) {
            surplusMode = SurplusMode.Space;
            surplusSpacePaddingRatio = value;
            return this;
        }

        public Container SetReversed(bool value) {
            reversed = value;
            return this;
        }
    }

    /// <summary>
    /// An item to be laid out. Either fixed (a set size) or flexible (grows to fill leftover space, bounded by
    /// min/max and shared out by weight, like CSS flex-grow). Margins add space around the item without counting
    /// toward its size.
    /// </summary>
    [Serializable]
    public class Item : LayoutElement {
        // Present in flexbox; might be worth adding as an upgrade.
        // public int order;

        // Relative growth rate among flexible items, like CSS flex-grow. Only used when flexible.
        public float weight;

        // Margin before (min) and after (max) the item. Adds to the space around the item without counting toward its size.
        public float marginMin;
        public float marginMax;
        public float totalMargin => marginMin + marginMax;
        
        public static Item Fixed(float size) {
            var layoutItem = new Item();
            return layoutItem.SetFixedSize(size);
        }

        public static Item Flexible(float minSize = 0, float maxSize = float.MaxValue, float weight = 1) {
            var layoutItem = new Item();
            return layoutItem.SetFlexibleSize(minSize, maxSize).SetWeight(weight);
        }
        
        public Item SetFixedSize(float fixedSize) {
            flexible = false;
            this.fixedSize = fixedSize;
            return this;
        }
        
        public Item SetFlexibleSize(float minSize, float maxSize) {
            flexible = true;
            this.minSize = minSize;
            this.maxSize = maxSize;
            return this;
        }
        
        public Item SetWeight(float weight) {
            this.weight = weight;
            return this;
        }
        
        public Item SetMargin(float value) {
            marginMin = marginMax = value;
            return this;
        }
        
        public Item SetMargin(float minPadding, float maxPadding) {
            marginMin = minPadding;
            marginMax = maxPadding;
            return this;
        }

        public Item SetMarginMin(float value) {
            marginMin = value;
            return this;
        }

        public Item SetMarginMax(float value) {
            marginMax = value;
            return this;
        }
    }
}