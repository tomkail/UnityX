using System.Collections;
using System.Collections.Generic;
public interface IPoolable {
	void ResetValues ();
}

// Pool of objects. Last-in-first-out.
// Also consider adding an upper limit and using a queue to ensures that objects are cycled out even if they're not manually returned
public class Pool<T> where T : class, IPoolable, new() {
	/// <summary>
    /// Get a new instance of a T. This will either reuse
    /// an existing class instance or it will create a brand new one.
    /// </summary>
    public static T Get ()
    {
        return _instance.GetInternal ();
    }
    
    public static void ReturnAll (ref List<T> list) {
        for(int i = 0; i < list.Count; i++) {
			var hit = list[i];
            Return(ref hit);
		}
		list.Clear();
    }

    public static void ReturnAll (ref T[] array) {
        for(int i = 0; i < array.Length; i++) {
			var hit = array[i];
            Return(ref hit);
		}
    }
    /// <summary>
    /// Return a class to the pool for future reuse.
    /// It's passed as a ref parameter since it also sets it to null.
    /// This is both for convenience and for safety, to make sure
    /// you don't accidentally keep it around when it gets handed
    /// out somewhere else later.
    /// </summary>
    public static void Return (ref T obj)
    {
        _instance.ReturnInternal (obj);
        obj = null;
    }

    public static void Clear ()
    {
        _instance._pool.Clear();
    }

    T GetInternal ()
    {
        if (_pool.Count > 0) {
            return _pool.Pop ();
        }

        return new T ();
    }

    void ReturnInternal (T obj)
    {
        if (obj == null)
            throw new System.ArgumentException ("Shouldn't return a null object!");
        
        obj.ResetValues ();
        _pool.Push (obj);
    }

    Stack<T> _pool = new Stack<T>();

    static Pool() {
        _instance = new Pool<T> ();
    }
    static Pool<T> _instance;
}
