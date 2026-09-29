using System.Collections.Generic;

public static class HashSetX
{
	public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> toAdd)
	{
		hashSet.UnionWith(toAdd);
	}
}

