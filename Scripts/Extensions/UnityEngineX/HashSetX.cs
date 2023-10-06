using System.Collections.Generic;

public static class HashSetX
{
	public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> toAdd)
	{
		foreach(T obj in toAdd)
			hashSet.Add(obj);
	}
}

