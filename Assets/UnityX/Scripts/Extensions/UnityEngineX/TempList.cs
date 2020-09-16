using System;
using System.Collections.Generic;

/// <summary>
/// Garbage free alternative to doing successive myList.Filter(...).Filter(...), and then extracting one result.
/// This re-uses just two temporary lists for successive filtering. It may not be as efficient as an IEnumerable
/// query where you just end up doing First() at the end (since that version probably wouldn't have to iterate the
/// whole lot), but as a naive implementation that's garbage free, it can be useful.
/// Remember:
///  - DO NOT USE if you want to save the final List result (e.g. set List on an object)
///  - DO NOT USE in multi-threaded code
/// </summary>
public static class TempListX {

    public static List<T> FilteredTempList<T>(this IList<T> fromList, Func<T, bool> filterFunc) 
    {
        var listToReturn = TempListBuffer<T>.Get(butNot:fromList);
        listToReturn.Clear();
        foreach(var val in fromList) {
            if( filterFunc(val) )
                listToReturn.Add(val);
        }
        return listToReturn;
    }

    static class TempListBuffer<T> {

        public static List<T> Get(IList<T> butNot) {
            if( butNot == _tempListA )
                return _tempListB;
            else
                return _tempListA;
        }

        static List<T> _tempListA = new List<T>();
        static List<T> _tempListB = new List<T>();
    }
}

