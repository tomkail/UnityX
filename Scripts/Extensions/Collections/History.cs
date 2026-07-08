using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A linear collection with a current-position cursor. Adding an item after the cursor discards
/// everything ahead of it — the classic browser / undo / command-history behaviour. Move the cursor
/// with StepBack, StepForward or GoToIndex.
/// </summary>
[System.Serializable]
public class History<T> where T : class {

	public List<T> items {get; private set;}
	public int maxItems = 100;

	private int _currentIndex;
	public int currentIndex => _currentIndex;

	public T currentItem {
		get {
			if(items == null || items.Count == 0 || _currentIndex < 0 || _currentIndex >= items.Count) return null;
			return items[_currentIndex];
		}
	}

	public bool canStepBack => !items.IsNullOrEmpty() && _currentIndex > 0;
	public bool canStepForward => !items.IsNullOrEmpty() && _currentIndex < items.Count - 1;

	public delegate void OnChangeItemEvent(T item);
	public event OnChangeItemEvent OnStepBack;
	public event OnChangeItemEvent OnStepForward;
	public event OnChangeItemEvent OnChangeCurrentIndex;

	public delegate void OnChangeEvent();
	public event OnChangeEvent OnChange;

	public History () {
		items = new List<T>();
		_currentIndex = -1;
	}

	public History (int maxItems) : this () {
		this.maxItems = Mathf.Clamp(maxItems, 1, int.MaxValue);
	}

	/// <summary>
	/// Adds an item after the current index, discarding any items after that index.
	/// </summary>
	public virtual void Add (T item) {
		ClearForwardInternal();

		if(items.Count >= maxItems) {
			items.RemoveAt(0);
			_currentIndex--;
		}

		items.Add(item);
		_currentIndex++;
		OnChange?.Invoke();
	}

	/// <summary>
	/// Moves the cursor to the given index (clamped to valid range) and returns the item there.
	/// </summary>
	public virtual T GoToIndex (int index) {
		if(items.IsNullOrEmpty()) return null;
		SetCurrentIndex(index);
		return currentItem;
	}

	/// <summary>
	/// Removes every item after the current index (the "redo" tail).
	/// </summary>
	public virtual void ClearForward () {
		if(ClearForwardInternal())
			OnChange?.Invoke();
	}

	/// <summary>
	/// Clears all items and resets the cursor.
	/// </summary>
	public virtual void Clear () {
		items.Clear();
		_currentIndex = -1;
		OnChange?.Invoke();
	}

	/// <summary>
	/// Moves the cursor back one and returns the item there.
	/// </summary>
	public virtual T StepBack () {
		if(!canStepBack) return currentItem;
		SetCurrentIndex(_currentIndex - 1);
		OnStepBack?.Invoke(currentItem);
		return currentItem;
	}

	/// <summary>
	/// Moves the cursor forward one and returns the item there.
	/// </summary>
	public virtual T StepForward () {
		if(!canStepForward) return currentItem;
		SetCurrentIndex(_currentIndex + 1);
		OnStepForward?.Invoke(currentItem);
		return currentItem;
	}

	// Clamps and applies the cursor, firing OnChangeCurrentIndex. Shared by GoToIndex/StepBack/StepForward.
	void SetCurrentIndex (int index) {
		_currentIndex = Mathf.Clamp(index, 0, items.Count - 1);
		OnChangeCurrentIndex?.Invoke(currentItem);
	}

	// Removes any items after the current index. Returns true if anything was removed.
	bool ClearForwardInternal () {
		int forwardCount = items.Count - (_currentIndex + 1);
		if(items.Count > 0 && forwardCount > 0) {
			items.RemoveRange(_currentIndex + 1, forwardCount);
			return true;
		}
		return false;
	}
}
