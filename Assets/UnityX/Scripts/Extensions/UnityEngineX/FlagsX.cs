﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public static class FlagsX {	
	/// <summary>
	/// Determines if any flags are set.
	/// </summary>
	/// <returns><c>true</c> if is set the specified flags; otherwise, <c>false</c>.</returns>
	/// <param name="flags">Flags.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	
	public static bool IsSet(int flagsValue, int flagValue) {
		return (flagsValue & flagValue) != 0;
	}

	public static bool AnySet(int flagsValue, params int[] flagValues) {
		return flagValues.Any(x => IsSet(flagsValue, x));
	}

	public static bool AllSet(int flagsValue, params int[] flagValues) {
		return flagValues.All(x => IsSet(flagsValue, x));
	}
	
	static int SetSingle(int flagsValue, int flagValue) {
		return flagsValue | flagValue;
	}
	public static int Set(int flagsValue, params int[] flagValues) {
		foreach(var flagValue in flagValues) flagsValue = SetSingle(flagsValue, flagValue);
		return flagsValue;
	}
	
	public static int Unset(int flagsValue, int flagValue) {
		return flagsValue & (~flagValue);
	}
	

	/// <summary>
	/// Creates a new Flag including the values provided
	/// </summary>
	/// <param name="flags">Flags.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Create<T>(params T[] flags) where T : struct {
		int[] flagValues = new int[flags.Length];
		for(int i = 0; i < flagValues.Length; i++) {
			flagValues[i] = (int)(object)(flags[i]);
		}
		return (T)(object) Create(flagValues);
	}
	
	private static int Create(params int[] flags) {
		int flagsValue = 0;
		foreach(int flag in flags) {
			if(!IsSet(flagsValue, flag)) {
				flagsValue = Set (flagsValue, flag);
			}
		}
		return flagsValue;
	}
	
	/// <summary>
	/// Flag, containing all the values
	/// </summary>
	/// <returns>The everything.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T CreateEverything<T>() where T : struct {
		return (T)(object)~0;
	}

	public static int LinearToFlagValue(int indexValue) {
		return (int)Mathf.Pow(2, indexValue);
	}
	
	public static int LinearToFlagValue<T>(T flags) where T : struct {
		return LinearToFlagValue((int)(object)flags);
	}



    public static bool Intersects (int flagsA, int flagsB) {
        return Intersection(flagsA, flagsB) != 0;
    }
    
    public static int Intersection (int flagsA, int flagsB) {
        return flagsA & flagsB;
    }
    
    public static int Union (int flagsA, int flagsB) {
        return flagsA | flagsB;
    }

    public static int Invert<T>(int flags) where T : struct {
        return (int)(object)(CreateEverything<T>()) & ~(flags);
    }

    static Dictionary<Type, Enum[]> individualFlagsCache = new Dictionary<Type, Enum[]>();
    public static IEnumerable<Enum> GetIndividualFlags(this Enum value) {
        var type = value.GetType();
        Enum[] individualFlags = null;
        if(!individualFlagsCache.TryGetValue(type, out individualFlags)) {
            individualFlags = individualFlagsCache[type] = GetFlagValues(type).ToArray();
        }
        return GetFlags(value, individualFlags);
    }

    private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
    {
        ulong bits = Convert.ToUInt64(value);
        List<Enum> results = new List<Enum>();
        for (int i = values.Length - 1; i >= 0; i--)
        {
            ulong mask = Convert.ToUInt64(values[i]);
            if (i == 0 && mask == 0L)
                break;
            if ((bits & mask) == mask)
            {
                results.Add(values[i]);
                bits -= mask;
            }
        }
        if (bits != 0L)
            return Enumerable.Empty<Enum>();
        if (Convert.ToUInt64(value) != 0L)
            return results.Reverse<Enum>();
        if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
            return values.Take(1);
        return Enumerable.Empty<Enum>();
    }

    private static IEnumerable<Enum> GetFlagValues(Type enumType) {
        ulong flag = 0x1;
        foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
        {
            ulong bits = Convert.ToUInt64(value);
            if (bits == 0L)
                //yield return value;
                continue; // skip the zero value
            while (flag < bits) flag <<= 1;
            if (flag == bits)
                yield return value;
        }
    }

    // Where flag values are:
    // 0, 1, 2, 4, 8, 16
    // Corresponding to the enum values:
    // 0, 1, 2, 3, 4, 5
    public static int FlagToEnumValue (int flagValue) {
        if(flagValue == 0) return 0;
        else if(flagValue < 0) return ~0;
        else {
            int numSteps = 1;
            while(flagValue != 1) {
                flagValue = flagValue >> 1;
                numSteps++;
            }
            return numSteps;
        }
    }
    public static int EnumToFlagValue (int enumValue) {
        return enumValue == 0 ? 0 : 1 << (enumValue-1);
    }
}