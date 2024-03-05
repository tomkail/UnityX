using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// History collection typically used as an undo stack. Uses an index pointer to track the current item, and removes items after that point when a new item is added.
/// </summary>
[System.Serializable]
public class History<T> {
	public T currentItem {
		get {
			if(history == null || history.Count == 0 || historyIndex < 0 || historyIndex >= history.Count) return default;
			return history[historyIndex];
		}
	}

	int _historyIndex;
	public int historyIndex {
		get => _historyIndex;
		set {
			value = Mathf.Clamp(value, 0, history.Count-1);
			if(_historyIndex == value) return;
			_historyIndex = value;
			if(OnChangeHistoryIndex != null) OnChangeHistoryIndex(currentItem);
		}
	}

	public List<T> history {get; private set;}
	public int maxHistoryItems = 100;

	public bool canStepBack => !history.IsNullOrEmpty() && historyIndex > 0;

	public bool canStepForward => !history.IsNullOrEmpty() && historyIndex < history.Count - 1;

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
		ClearHistoryAfterHistoryIndex();
		if(history.Count >= maxHistoryItems) {
			history.RemoveAt (0);
			_historyIndex--;
		}

		history.Add (state);
		_historyIndex++;
		if(OnChangeHistory != null) OnChangeHistory();
	}

	/// <summary>
	/// Clears all redoable history; that is, all history after the current index.
	/// </summary>
	void ClearHistoryAfterHistoryIndex() {
		if(history.Count > 0 && history.Count - (historyIndex + 1) > 0) {
			history.RemoveRange(historyIndex + 1, history.Count - (historyIndex + 1));
		}
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

	
	
	/// <summary>
	/// Gets the index of the most recent occurence of the target state, or -1 if the state is not found.
	/// </summary>
	public virtual int GetMostRecentIndexOfState (T state) {
		if(!canStepBack) return -1;
		for(int i = historyIndex; i >= 0; i--) {
			if(history[i].Equals(state)) {
				return i;
			}
		}
		return -1;
	}
	
	/// <summary>
	/// If that state is found in the history, rewinds to that state. Otherwise, adds the state to the history.
	/// </summary>
	public void RewindOrAddToHistory(T state) {
		var mostRecentIndexOfState = GetMostRecentIndexOfState(state);
		if(mostRecentIndexOfState != -1) historyIndex = mostRecentIndexOfState;
		else AddToHistory(state);
	}
}