using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexLayout {
    public static class FlexLayout {
        [System.Serializable]
        public class Result {
            public float containerSize;
            public List<Vector2> ranges;
        }
        
        public static Result GetLayoutRanges(Container layoutParams, List<Item> items) {
            Vector2 surplusOffsetPadding = Vector2.zero;
            float totalItemSpacing = layoutParams.spacing;
            Dictionary<Item, float> flexItemSizes = null;
            
            float totalFixedItemSize = items.Where(i => !i.flexible).Sum(i => i.fixedSize);
            float totalMinFlexibleItemSize = items.Where(i => i.flexible).Sum(i => i.minSize);
            float totalFixedSpacing = layoutParams.spacing * (items.Count - 1);
            float availableFlexibleSpace = (layoutParams.flexible ? layoutParams.maxInnerSize : layoutParams.fixedInnerSize) - totalFixedItemSize - totalMinFlexibleItemSize - totalFixedSpacing;

            // Map to hold final sizes for each flexible item
            flexItemSizes = items.Where(i => i.flexible).ToDictionary(i => i, i => i.minSize);

            while (availableFlexibleSpace > 0) {
                // Get the total weight of the flexible items that can still grow
                float totalWeight = items.Where(i => i.flexible && flexItemSizes[i] < i.maxSize).Sum(i => i.weight);

                if (totalWeight == 0) break;

                float spaceAllocatedThisIteration = 0;

                foreach (var item in items.Where(i => i.flexible)) {
                    if (flexItemSizes[item] >= item.maxSize)
                        continue;

                    float weightFraction = item.weight / totalWeight;
                    float spaceForThisItem = weightFraction * availableFlexibleSpace;
                    float spaceActuallyUsed = Math.Min(spaceForThisItem, item.maxSize - flexItemSizes[item]);

                    flexItemSizes[item] += spaceActuallyUsed;
                    spaceAllocatedThisIteration += spaceActuallyUsed;
                }

                // Reduce the available space by the space that was allocated in this iteration
                availableFlexibleSpace -= spaceAllocatedThisIteration;
            }

            // The total size of the content and fixed spacing. Any additional space can be used for extra spacing or padding.
            float contentSizeAndFixedSpacing = totalFixedItemSize + flexItemSizes.Values.Sum() + totalFixedSpacing;
            // The space that's left after the content and fixed spacing is taken into account to be used for extra spacing or padding.
            // Can be negative, if the content is larger than the maximum size of the container.
            var flexibleSpacing = 0f;
            if (layoutParams.flexible) {
                flexibleSpacing = layoutParams.minInnerSize - contentSizeAndFixedSpacing;
                var maxFlexibleSpacing = Mathf.Min(0, layoutParams.maxInnerSize - contentSizeAndFixedSpacing);
                flexibleSpacing = Mathf.Max(flexibleSpacing, maxFlexibleSpacing);
            } else {
                flexibleSpacing = layoutParams.fixedInnerSize - contentSizeAndFixedSpacing;
            }


            if (layoutParams.surplusMode == Container.SurplusMode.Offset) {
                surplusOffsetPadding.x = flexibleSpacing * layoutParams.surplusOffsetPivot;
                surplusOffsetPadding.y = flexibleSpacing * (1f - layoutParams.surplusOffsetPivot);
            } else if (layoutParams.surplusMode == Container.SurplusMode.Space) {
                // When justifySpacePaddingRatio is 1 we're effectively pretending there are 2 zero-size items at the start and end of the list.
                var fakeItemCountForFlexibleSpacing = (items.Count - 1) + layoutParams.surplusSpacePaddingRatio * 2;
                var flexibleItemSpacing = flexibleSpacing / fakeItemCountForFlexibleSpacing;
                surplusOffsetPadding.x = flexibleItemSpacing * layoutParams.surplusSpacePaddingRatio;
                surplusOffsetPadding.y = flexibleItemSpacing * layoutParams.surplusSpacePaddingRatio;
                totalItemSpacing += flexibleItemSpacing;
            }
            
            var ranges = new List<Vector2>();

            var currentItemPosition = 0f;
            for (var index = 0; index < items.Count; index++) {
                var item = items[index];
                float itemSize = flexItemSizes != null && flexItemSizes.TryGetValue(item, out var flexibleSize) ? flexibleSize : item.fixedSize;
                if (layoutParams.reversed) {
                    ranges.Add(new Vector2(currentItemPosition - itemSize, currentItemPosition));
                    currentItemPosition -= itemSize + (index < items.Count-1 ? totalItemSpacing : 0);
                } else {
                    ranges.Add(new Vector2(currentItemPosition, currentItemPosition + itemSize));
                    currentItemPosition += itemSize + (index < items.Count-1 ? totalItemSpacing : 0);
                }
            }

            var itemOffset = 0f;
            if (layoutParams.reversed) {
                itemOffset = layoutParams.paddingMax + -currentItemPosition + surplusOffsetPadding.x;
            } else {
                itemOffset = layoutParams.paddingMin + surplusOffsetPadding.x;
            }
            for (var index = 0; index < ranges.Count; index++) {
                ranges[index] += Vector2.one * itemOffset;
            }

            var totalSizeConsumedIncludingPadding = (layoutParams.reversed ? ranges.First().y + layoutParams.paddingMin : ranges.Last().y + layoutParams.paddingMax) + surplusOffsetPadding.y;

            return new Result() {
                containerSize = totalSizeConsumedIncludingPadding,
                ranges = ranges
            };
        }
    }

    public class LayoutElement {
        // If the item is flexible
        public bool flexible;

        // If the item isn't flexible, this is the size that is used.
        public float fixedSize;

        // Min/Max sizes for flexible items.
        public float minSize;

        public float maxSize;
    }

    // Define the properties of the container that LayoutItemParams are laid out in.
    [Serializable]
    public class Container : LayoutElement {
        public float fixedInnerSize => fixedSize - totalPadding;
        public float minInnerSize => minSize - totalPadding;
        public float maxInnerSize => maxSize - totalPadding;

        public float paddingMin;
        public float paddingMax;
        public float totalPadding => paddingMin + paddingMax;

        // The fixed spacing between the elements. Extra spacing may be added if justifyMode is Space.
        public float spacing = 0f;

        // Describes what happens to extra space when the items don't fill the container.
        public SurplusMode surplusMode = SurplusMode.Offset;
        public enum SurplusMode {
            // Offset adds extra padding (at the start or end depending on the justifyPivot setting).
            Offset,
            // Space adds extra space between the items.
            Space,
        }


        // When using SurplusMode.Offset
        // This corresponds to flexbox's justify-content flex-start/flex-right/center options.
        // Set to 0 to add the spacing at the start, 0.5 for center, and 1 at the end.
        // Note the use of "Start and end" vs "min and max". They are relative to direction - if direction is reversed, 0 is the right edge instead of the left edge.
        public float surplusOffsetPivot = 0.5f;

        // When using SurplusMode.Space
        // This corresponds to flexbox's justify-content space options.
        // Set to 0 for space-between, 0.5 for space-around, and 1 for space-evenly.
        public float surplusSpacePaddingRatio = 0f;

        // When reversed, the layout starts at the max rather than the min, and starts with the last layout item rather than the first.
        // Note that surplusOffsetPivot is not reversed.
        public bool reversed = false;
        
        public static Container Fixed(float size) {
            var layoutItem = new Container();
            return layoutItem.SetFixedSize(size);
        }

        public static Container Flexible(float minSize = 0, float maxSize = float.MaxValue) {
            var layoutItem = new Container();
            return layoutItem.SetFlexibleSize(minSize, maxSize);
        }

        public Container SetFixedSize(float fixedSize) {
            this.flexible = false;
            this.fixedSize = fixedSize;
            return this;
        }
        
        public Container SetFlexibleSize(float minSize, float maxSize) {
            this.flexible = true;
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
            this.reversed = value;
            return this;
        }
    }

// LayoutItemParams determine the sizes of the layouts when using GetLayoutRanges, which is the base of other layout functions.
// It allows fixed and flexible sizes.
// Flexible is similar to CSS Flexbox and can have a min/max size and a weight.
    [System.Serializable]
    public class Item : LayoutElement {
        // This is present in flexbox, and it might be an upgrade to consider.
        // public int order;

        // This is similar to flex-grow in CSS.
        public float weight;

        public static Item Fixed(float size) {
            var layoutItem = new Item();
            return layoutItem.SetFixedSize(size);
        }

        public static Item Flexible(float minSize = 0, float maxSize = float.MaxValue, float weight = 1) {
            var layoutItem = new Item();
            return layoutItem.SetFlexibleSize(minSize, maxSize).SetWeight(weight);
        }
        
        public Item SetFixedSize(float fixedSize) {
            this.flexible = false;
            this.fixedSize = fixedSize;
            return this;
        }
        
        public Item SetFlexibleSize(float minSize, float maxSize) {
            this.flexible = true;
            this.minSize = minSize;
            this.maxSize = maxSize;
            return this;
        }
        
        public Item SetWeight(float weight) {
            this.weight = weight;
            return this;
        }
    }
}