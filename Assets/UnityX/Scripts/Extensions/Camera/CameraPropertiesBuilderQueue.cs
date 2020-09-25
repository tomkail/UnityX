using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Uses an ordered list of CameraProperties modifier functions to modify an instance of CameraProperties.
/// Camera properties can be edited or overwritten. 
/// This is handy for easily performing tweaks to a camera for special effects.
/// Delegate functions can also overwrite the properties for a set length of time. This could be used to mimic a multi camera setup.
/// </summary>
[System.Serializable]
public class CameraPropertiesBuilderQueue {
	public delegate void UpdateCameraPropertiesDelegate (float deltaTime);
	public delegate void ModifyCameraPropertiesDelegate (ref CameraProperties SpaceCameraProperties);
	// [SerializeField, Disable]
	private List<SetCameraPropertiesDelegateQueueItem> modifiers = new List<SetCameraPropertiesDelegateQueueItem>();

	/// <summary>
	/// Uses a sort index and a delegate to edit camera properties.
	/// </summary>
	[System.Serializable]
	private class SetCameraPropertiesDelegateQueueItem {
		public string name;
		// Lower sort indexes are executed first.
		public int sortIndex {get; private set;}
		public UpdateCameraPropertiesDelegate updateCameraPropertiesDelegate {get; private set;}
		public ModifyCameraPropertiesDelegate setCameraPropertiesDelegate {get; private set;}
		
		public SetCameraPropertiesDelegateQueueItem (int sortIndex, UpdateCameraPropertiesDelegate updateCameraPropertiesDelegate, ModifyCameraPropertiesDelegate setCameraPropertiesDelegate) {
			this.sortIndex = sortIndex;
			this.updateCameraPropertiesDelegate = updateCameraPropertiesDelegate;
			this.setCameraPropertiesDelegate = setCameraPropertiesDelegate;
		}
	}

	public void Add (UpdateCameraPropertiesDelegate updateCameraPropertiesDelegate, ModifyCameraPropertiesDelegate setCameraPropertiesDelegate) {
		modifiers.Add(new SetCameraPropertiesDelegateQueueItem(modifiers.Count, updateCameraPropertiesDelegate, setCameraPropertiesDelegate));
	}
	public void Add (UpdateCameraPropertiesDelegate updateCameraPropertiesDelegate, ModifyCameraPropertiesDelegate setCameraPropertiesDelegate, int sortIndex) {
		modifiers.Add(new SetCameraPropertiesDelegateQueueItem(sortIndex, updateCameraPropertiesDelegate, setCameraPropertiesDelegate));
		modifiers.Sort((x, y) => x.sortIndex.CompareTo(y.sortIndex));
	}

	public void Add (UpdateCameraPropertiesDelegate updateCameraPropertiesDelegate, ModifyCameraPropertiesDelegate setCameraPropertiesDelegate, int sortIndex, string name) {
		var queueItem = new SetCameraPropertiesDelegateQueueItem(sortIndex, updateCameraPropertiesDelegate, setCameraPropertiesDelegate);
		queueItem.name = name;
		modifiers.Add(queueItem);
		modifiers.Sort((x, y) => x.sortIndex.CompareTo(y.sortIndex));
	}
	
	public bool Remove (ModifyCameraPropertiesDelegate setCameraPropertiesDelegate) {
		for (int i = modifiers.Count - 1; i >= 0; i--) {
			SetCameraPropertiesDelegateQueueItem queueItem = modifiers [i];
			if (queueItem.setCameraPropertiesDelegate == setCameraPropertiesDelegate) {
				modifiers.RemoveAt(i);
				return true;
			}
		}
		return false;
	}
	
	public void Update (float deltaTime) {
		foreach(var modifier in modifiers) {
			modifier.updateCameraPropertiesDelegate(deltaTime);
		}
	}
	public void Generate (ref CameraProperties properties) {
		foreach(var modifier in modifiers) {
			modifier.setCameraPropertiesDelegate(ref properties);
		}
	}
}