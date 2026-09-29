using System.Collections.Generic;

// A fairly efficient way to update a list in such a way that produces a list of the items added and removed
public class DoubleBufferChangeHashSet<T> {
    public HashSet<T> last = new HashSet<T>();
    public HashSet<T> current = new HashSet<T>();

    public HashSet<T> removed = new HashSet<T>();
    public HashSet<T> added = new HashSet<T>();

    public delegate void OnChangeDelegate(HashSet<T> removed, HashSet<T> added);
    public event OnChangeDelegate OnChange;

    public void Clear() {
        current.Clear();
        last.Clear();
        removed.Clear();
        added.Clear();
    }
    
    // Update the current list, using an action to populate the current list
    public void Repopulate (System.Action<HashSet<T>> PopulateListAction) {
        SwapLists(ref last, ref current);

        current.Clear();
        PopulateListAction(current);
        
		removed.Clear();
        added.Clear();
		foreach(var item in IEnumerableX.GetRemoved(last, current)) removed.Add(item);
		foreach(var item in IEnumerableX.GetAdded(last, current)) added.Add(item);

        if(removed.Count > 0 || added.Count > 0) {
            if(OnChange != null)
                OnChange(last, current);
        }
    }

    // Double buffer list ref swapping
    void SwapLists (ref HashSet<T> last, ref HashSet<T> current) {
        var temp = current;
        current = last;
        last = temp;
    }
}