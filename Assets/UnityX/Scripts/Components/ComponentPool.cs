using UnityEngine;
using System.Collections.Generic;
 
[System.Serializable]
public class ComponentPool<T> where T : Component {

	#region Public Variables
	public List<T> spawnedElements {
		get {
			return spawnedElements_;
		}
	}
	#endregion
	
    #region Interface
    public ComponentPool( T prefab, Transform parent, int initialSize = 0, bool fakeMode = false ) {

        fakeMode_ = fakeMode;
        prefab_ = prefab;
        parent_ = parent;

        spawnedElements_ = new List<T>(initialSize);
        recycledElements_ = new Queue<T>( ( !fakeMode_ ) ? initialSize : 0 );
        if ( !fakeMode_ ) {
            for ( int i = 0; i < initialSize; ++i ) {
                recycledElements_.Enqueue( Allocate( Vector3.zero, Quaternion.identity ) );
            }
        }
    }

    public T Spawn( Vector3 position, Quaternion rotation ) {
        if ( fakeMode_ ) {
            T newElem = Allocate( position, rotation);
            spawnedElements_.Add(newElem);
            newElem.gameObject.SetActive(true);
            return newElem;
        }

        T newSpawnedElement;
        if(recycledElements_.Count > 0){
            newSpawnedElement = recycledElements_.Dequeue();
            newSpawnedElement.transform.position = position;
            newSpawnedElement.transform.rotation = rotation;
        }else{
            newSpawnedElement = Allocate( position, rotation);
        }
        spawnedElements_.Add(newSpawnedElement);
        GameObject newGameObject = newSpawnedElement.gameObject;
        newGameObject.SetActive(true);
        return newSpawnedElement;
    }

    public T Spawn( Vector3 position ) {
		return Spawn(position, prefab_.transform.rotation);
    }

    public T Spawn() {
		return Spawn(prefab_.transform.position, prefab_.transform.rotation);
    }

    public void Recycle( T spawnedElement ) {
        if ( fakeMode_ ) {
            if ( spawnedElements_.Remove(spawnedElement) ) {
                GameObject.Destroy(spawnedElement.gameObject);
                return;
            } else {
				Debug.LogError ("Out of range");
            }
        }

        if ( spawnedElements_.Remove(spawnedElement) ) {
            spawnedElement.gameObject.SetActive(false);
            recycledElements_.Enqueue(spawnedElement);
        } else {
			Debug.LogError ("Out of range");
        }
    }

    public void Recycle( System.Predicate<T> recyclingCondition ) {
        for ( int i = 0; i < spawnedElements_.Count; ++i ) {
            var spawnedElement = spawnedElements_[i];
            if ( recyclingCondition(spawnedElement) ) {
                Recycle(spawnedElement);
            }
        }
    }

    public void Clear () {
        for ( int i = 0; i < spawnedElements_.Count; ++i ) {
            GameObject.Destroy(spawnedElements_[i].gameObject);
        }

        T recycledElem;
        while ( recycledElements_.Count > 0 ) {
            recycledElem = recycledElements_.Dequeue();
            GameObject.Destroy(recycledElem.gameObject);
        }

        spawnedElements_.Clear();
        recycledElements_.Clear();
    }
    #endregion



    #region Private Member Functions
    T Allocate( Vector3 position, Quaternion rotation ) {
        var elem = GameObject.Instantiate(prefab_, position, rotation, parent_) as T;
        elem.name = string.Concat(elem.name, " ", nAllocatedElements_);
        DebugX.Assert(!elem.gameObject.activeSelf, "The prefab must not be active.");
        ++nAllocatedElements_;
        return elem;
    }
    #endregion



    #region Private Data Members
    T prefab_;
    Transform parent_;
    Queue<T> recycledElements_;
    List<T> spawnedElements_;
    bool fakeMode_;
    int nAllocatedElements_;
    #endregion
}