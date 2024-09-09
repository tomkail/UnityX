using System;
using System.Collections.Generic;
using UnityEngine;

public static class DictionaryX {
    public static bool TryGetValueAsType<TKey, TValue>(this IDictionary<TKey, object> dictionary, TKey key, out TValue value, TValue defaultValue = default) {
        if (dictionary != null && dictionary.TryGetValue(key, out object tempValue)) {
            if (tempValue is TValue castedValue) {
                value = castedValue;
                return true;
            } else {
                // Special case for enums, which may be stored as strings
                if (typeof(TValue).IsEnum) {
                    if (tempValue is string stringValue) {
                        if (Enum.TryParse(typeof(TValue), stringValue, out object enumValue)) {
                            value = (TValue)enumValue;
                            return true;
                        }
                    }
                }
                Debug.LogWarning("Failed to cast value to type: " + typeof(TValue).Name + " for key: " + key + " with value: " + tempValue + "");
            }
        }
        value = defaultValue;
        return false;
    }
}