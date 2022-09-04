using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Convenience way to create objects in the hierarchy that are a bit like prefabs, except that they
/// aren't. They exist in the scene, immediately deactivate themselves, then provide an easy way to
/// instantiate a copy in the same place in the hierarchy.
/// NEW FEATURE! If you call ReturnToPool() once you're done with the instance, then it's returned
/// to the original prototype to be reused if necessary instead of creating a brand new GameObject.
/// </summary>
[DisallowMultipleComponent]
public class Prototype : MonoBehaviour {

	public event Action<Prototype> OnPreReturnToPool;
	public event Action<Prototype> OnReturnToPool;
    [SerializeField, Disable]
    bool _inUse;
    public bool inUse {
        get {
            return _inUse;
        } private set {
            _inUse = value;
        }
    }

	public bool isOriginalPrototype {
		get {
			return _originalPrototype == null;
		}
	}

	public Prototype originalPrototype {
		get {
			return _originalPrototype;
		}
	}

	void Start() 
	{
		if( isOriginalPrototype )
			this.gameObject.SetActive(false);
	}

	void OnDestroy()
	{
        #if UNITY_EDITOR
		if(applicationQuitting) return;
        #endif
		if( _instancePool != null )
			foreach(var inst in _instancePool)  
				Destroy(inst);
		if(!isOriginalPrototype && originalPrototype != null && originalPrototype._instancePool != null) {
			originalPrototype._instancePool.Remove(this);
		}
	}

	public T Instantiate<T>(Action<T> onInstanciated)  where T : Component
	{
		var instance = Instantiate<T>();
		onInstanciated(instance);
		return instance;
	}
	
	public T Instantiate<T>(Transform parent = null)  where T : Component
	{
		Prototype instance = null;

		// Re-use instance from pool
		if( _instancePool != null && _instancePool.Count > 0 ) {
			var instanceIdx = _instancePool.Count-1;
			instance = _instancePool[instanceIdx];
            instance.transform.SetParent(parent ?? transform.parent, false);
			_instancePool.RemoveAt(instanceIdx);
			if(instance == null)
				Debug.LogError("Prototype instance for type "+typeof(T).Name+" in pool is null!");
		} 

		// Instantiate fresh instance
		else {
			instance = UnityEngine.Object.Instantiate(this, parent ?? transform.parent, false);

			var protoRT = transform as RectTransform;
			if( protoRT ) {
				var instRT  = instance.transform as RectTransform;
				instRT.anchorMin = protoRT.anchorMin;
				instRT.anchorMax = protoRT.anchorMax;
				instRT.pivot = protoRT.pivot;
				instRT.sizeDelta = protoRT.sizeDelta;
			}

			instance._originalPrototype = this;
		}

        instance.transform.localPosition = transform.localPosition;
        instance.transform.localRotation = transform.localRotation;
        instance.transform.localScale    = transform.localScale;
        instance.inUse = true;
		
		instance.gameObject.SetActive(true);

		var comp = instance.GetComponent<T>();
		if(comp == null) Debug.LogError("Cant find component "+typeof(T).Name+" when creating "+transform.HierarchyPath());
		return comp;
	}

	public void ReturnToPool()
	{
		if( isOriginalPrototype ) {
			Debug.LogError("Can't return to pool because the original prototype doesn't exist. Is this prototype the original?");
			Destroy(gameObject);
			return;
		}
			
		_originalPrototype.AddToPool(this);
	}

	public void DestroyPool()
	{
		if( _instancePool == null ) return;
		foreach(var obj in _instancePool) {
			// Not already destroyed by Unity?
			if( obj != null ) {
				#if UNITY_EDITOR
				if(!Application.isPlaying)
					DestroyImmediate(obj.gameObject);
				else
				#endif
				Destroy(obj.gameObject);
			}
		}
		_instancePool.Clear();
	}

	public T GetOriginal<T>()
	{
		if( isOriginalPrototype )
			return GetComponent<T>();
		else
			return originalPrototype.GetComponent<T>();
	}

	void AddToPool(Prototype instancePrototype)
	{
		if( !isOriginalPrototype )
			Debug.LogError("Adding "+instancePrototype.name+" to prototype pool of "+this.name+" but this appears to be an instance itself?");
		
		if( instancePrototype.OnPreReturnToPool != null )
			instancePrototype.OnPreReturnToPool(this);

		instancePrototype.gameObject.SetActive(false);
        if(instancePrototype.transform.parent != transform.parent) instancePrototype.transform.SetParent(transform.parent);
        instancePrototype.inUse = false;
		
		if( _instancePool == null ) _instancePool = new List<Prototype>();
		_instancePool.Add(instancePrototype);

		if( instancePrototype.OnReturnToPool != null )
			instancePrototype.OnReturnToPool(this);
	}

	Prototype _originalPrototype;
	[System.NonSerialized]
	List<Prototype> _instancePool;

    #if UNITY_EDITOR
    static bool applicationQuitting;
    void OnApplicationQuit () {
        applicationQuitting = true;
    }
    #endif
}