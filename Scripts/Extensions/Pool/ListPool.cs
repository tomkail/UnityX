using System.Collections.Generic;

/// <summary>
/// Simple reuse pool for List<T>. Usage example:
/// To get a new list of ints:
/// 
///     var list = ListPool<int>.Get();
/// 
/// to return it to the pool:
/// 
///     ListPool<int>.Return (ref list);
/// 
/// </summary>
public class ListPool<T>
{
    /// <summary>
    /// Get a new instance of a List<T>. This will either reuse
    /// an existing List instance or it will create a brand new one.
    /// </summary>
    public static List<T> Get ()
    {
        return _instance.GetInternal ();
    }

    /// <summary>
    /// Get a new instance of a List<T>. This will either reuse
    /// an existing List instance or it will create a brand new one.
    /// An IEnumerable can be passed in order to initialise the list
    /// with some contents, similar to doing `new List<T>(elements)`.
    /// </summary>
    public static List<T> Get (IEnumerable<T> initialElements)
    {
        var list = _instance.GetInternal ();
        list.AddRange (initialElements);
        return list;
    }

    /// <summary>
    /// Return a list to the pool for future reuse.
    /// It's passed as a ref parameter since it also sets it to null.
    /// This is both for convenience and for safety, to make sure
    /// you don't accidentally keep it around when it gets handed
    /// out somewhere else later.
    /// </summary>
    public static void Return (ref List<T> list)
    {
        _instance.ReturnInternal (list);
        list = null;
    }

    List<T> GetInternal ()
    {
        if (_pool.Count > 0) {
            return _pool.Pop ();
        }

        return new List<T> ();
    }

    void ReturnInternal (List<T> list)
    {
        if (list == null)
            throw new System.ArgumentException ("Shouldn't return a null list!");
        
        list.Clear ();
        _pool.Push (list);
    }

    Stack<List<T>> _pool = new Stack<List<T>>();

    static ListPool() {
        _instance = new ListPool<T> ();
    }
    static ListPool<T> _instance;
}