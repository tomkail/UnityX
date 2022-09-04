using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// History collection typically used as an undo stack. Uses an index pointer to track the current item, and removes items after that point when a new item is added.
/// </summary>
[System.Serializable]
public class History<T> where T : class {
	
	public T currentItem {
		get {
			if(history == null || history.Count == 0 || historyIndex < 0 || historyIndex >= history.Count) return null;
			return history[historyIndex];
		}
	}

	private int _historyIndex;
	public int historyIndex {
		get {
			return _historyIndex;
		} set {
			_historyIndex = Mathf.Clamp(value, 0, history.Count-1);
			if(OnChangeHistoryIndex != null) OnChangeHistoryIndex(currentItem);
		}
	}

	public List<T> history {get; private set;}
	public int maxHistoryItems = 100;

	public bool canStepBack {
		get {
			return !history.IsNullOrEmpty() && historyIndex > 0;
		}
	}
	
	public bool canStepForward {
		get {
			return !history.IsNullOrEmpty() && historyIndex < history.Count - 1;
		}
	}

	public delegate void OnChangeHistoryItemEvent(T historyItem);

	public event OnChangeHistoryItemEvent OnStepBack;
	public event OnChangeHistoryItemEvent OnStepForward;
	public event OnChangeHistoryItemEvent OnChangeHistoryIndex;

	public delegate void OnChangeHistoryEvent();
	public event OnChangeHistoryEvent OnChangeHistory;

	public History () {
		history = new List<T>();
		_historyIndex = -1;
	}

	public History (int maxHistoryItems) : this () {
		this.maxHistoryItems = Mathf.Clamp(maxHistoryItems, 1, int.MaxValue);
	}

	/// <summary>
	/// Adds to the history after the current index. Removes any elements found in the history after that index.
	/// </summary>
	/// <param name="state">State.</param>
	public virtual void AddToHistory (T state) {
		if(history.Count > 0 && history.Count - (historyIndex + 1) > 0) {
			history.RemoveRange(historyIndex + 1, history.Count - (historyIndex + 1));
		}
		
		if(history.Count >= maxHistoryItems) {
			history.RemoveAt (0);
			_historyIndex--;
		}

		history.Add (state);
		_historyIndex++;
		if(OnChangeHistory != null) OnChangeHistory();
	}

	/// <summary>
	/// Clears all values and resets the index.
	/// </summary>
	public virtual void Clear () {
		history.Clear();
		_historyIndex = -1;
	}

	/// <summary>
	/// Moves the current index back, and returns the value at that index.
	/// </summary>
	public virtual T StepBack () {
		if(!canStepBack) return currentItem;
		historyIndex--;
		if(OnStepBack != null) OnStepBack(currentItem);
		return currentItem;
	}

	/// <summary>
	/// Moves the current index forward, and returns the value at that index.
	/// </summary>
	public virtual T StepForward () {
		if(!canStepForward) return currentItem;
		historyIndex++;
		if(OnStepForward != null) OnStepForward(currentItem);
		return currentItem;
	}
}