using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public static class FlagsX {	
	/// <summary>
	/// Determines if any flags are set.
	/// </summary>
	/// <returns><c>true</c> if is set the specified flags; otherwise, <c>false</c>.</returns>
	/// <param name="flags">Flags.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	
	public static bool HasFlag(int flagsValue, int flagValue) {
		return (flagsValue & flagValue) != 0;
	}

	public static bool AnySet(int flagsValue, params int[] flagValues) {
		return flagValues.Any(x => HasFlag(flagsValue, x));
	}

	public static bool AllSet(int flagsValue, params int[] flagValues) {
		return flagValues.All(x => HasFlag(flagsValue, x));
	}
	
	static int SetSingle(int flagsValue, int flagValue) {
		return flagsValue | flagValue;
	}
	public static int Set(int flagsValue, params int[] flagValues) {
		foreach(var flagValue in flagValues) flagsValue = SetSingle(flagsValue, flagValue);
		return flagsValue;
	}
	public static T SetFlag<T>(this T value, T flag) where T : Enum {
		var left = Caster<T, UInt64>.Cast(value);
		var right = Caster<T, UInt64>.Cast(flag);
		var result = left | right;
		return Caster<ulong, T>.Cast(result);
	}
	
	public static int Unset(int flagsValue, int flagValue) {
		return flagsValue & ~flagValue;
	}
	
	public static T UnsetFlag<T>(this T value, T flag) where T : Enum {
		var left = Caster<T, UInt64>.Cast(value);
		var right = Caster<T, UInt64>.Cast(flag);
		var result = left & ~ right;
		return Caster<ulong, T>.Cast(result);
	}

	public static T SetFlagState<T>(this T value, T flag, bool state) where T : Enum {
		var left = Caster<T, UInt64>.Cast(value);
		var right = Caster<T, UInt64>.Cast(flag);
		var result = state ? left | right : left & ~right;
		return Caster<ulong, T>.Cast(result);
	}
	
	static class Caster<TSource, TTarget> {
		public static readonly Func<TSource, TTarget> Cast = CreateConvertMethod();

		private static Func<TSource, TTarget> CreateConvertMethod()
		{
			var p = Expression.Parameter(typeof(TSource));
			var c = Expression.ConvertChecked(p, typeof(TTarget));
			return Expression.Lambda<Func<TSource, TTarget>>(c, p).Compile();
		}
	}
	
	/// <summary>
	/// Creates a new Flag including the values provided
	/// </summary>
	/// <param name="flags">Flags.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Create<T>(params T[] flags) where T : Enum {
		// OR the flags in signed-long space: Convert.ToInt64 reads the real underlying value (handling
		// negative/high-bit members like `All = ~0` that a checked ulong cast would overflow on), and
		// Enum.ToObject rebuilds for any backing type. (A `(int)(object)flags[i]` unbox would crash for
		// non-int-backed enums; a checked Caster<T,ulong> cast crashes on negative members.)
		long result = 0;
		for(int i = 0; i < flags.Length; i++) result |= Convert.ToInt64(flags[i]);
		return (T)Enum.ToObject(typeof(T), result);
	}

	static int Create(params int[] flags) {
		int flagsValue = 0;
		foreach(int flag in flags) {
			if(!HasFlag(flagsValue, flag)) {
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
	public static T CreateEverything<T>() where T : Enum {
		// All bits set for the enum's underlying type. Enum.ToObject converts -1 unchecked to the backing
		// type (0xFF for byte, ~0 for int, 0xFFFF…F for ulong, …), so this works for any backing type —
		// unlike `(T)(object)~0` (InvalidCastException for non-int enums) or a checked ulong cast
		// (OverflowException on members whose bit pattern is negative, e.g. an `All = ~0` entry).
		return (T)Enum.ToObject(typeof(T), -1L);
	}

	public static int LinearToFlagValue(int indexValue) {
		return 1 << indexValue;
	}
	
	public static int LinearToFlagValue<T>(T flags) where T : Enum {
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

	public static int Invert<T>(int flags) where T : Enum {
		// Convert.ToInt32 respects the enum's real underlying type; a plain (int)(object)
		// unbox throws InvalidCastException for non-int-backed enums. (Return type is int,
		// so this remains an int-domain helper for enums whose "everything" fits in int.)
		return Convert.ToInt32(CreateEverything<T>()) & ~(flags);
	}

	static Dictionary<Type, Enum[]> individualFlagsCache = new();
	public static IEnumerable<Enum> GetIndividualFlags(this Enum value) {
		var type = value.GetType();
		Enum[] individualFlags = null;
		if(!individualFlagsCache.TryGetValue(type, out individualFlags)) {
			individualFlags = individualFlagsCache[type] = GetFlagValues(type).ToArray();
		}
		return GetFlags(value, individualFlags);
	}

	static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
	{
		ulong bits = Convert.ToUInt64(value);
		// A value of zero decomposes to the enum's zero-named member (e.g. None = 0),
		// if one is defined. `values` only ever contains the individual non-zero flag
		// bits (GetFlagValues skips zero), so a `values[0] == 0` tail check would be
		// unreachable; look the zero member up from the enum type directly instead.
		if (bits == 0L)
		{
			foreach (var member in Enum.GetValues(value.GetType()).Cast<Enum>())
				if (Convert.ToUInt64(member) == 0L)
					return new[] { member };
			return Enumerable.Empty<Enum>();
		}
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
		return results.Reverse<Enum>();
	}

	static IEnumerable<Enum> GetFlagValues(Type enumType) {
		ulong flag = 0x1;
		foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
		{
			ulong bits = Convert.ToUInt64(value);
			if (bits == 0L)
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