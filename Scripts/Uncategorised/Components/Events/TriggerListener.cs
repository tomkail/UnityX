using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TriggerListener : MonoBehaviour {

	public LayerMask ignoreLayers;

	public delegate void CollisionEnterEvent (Collision _collider);
	public event CollisionEnterEvent CollisionEnter;
	
	public delegate void CollisionStayEvent (Collision _collider);
	public event CollisionStayEvent CollisionStay;
	
	public delegate void CollisionExitEvent (Collision _collider);
	public event CollisionExitEvent CollisionExit;
	
	public delegate void CollisionEnter2DEvent (Collision2D _collider);
	public event CollisionEnter2DEvent CollisionEnter2D;
	
	public delegate void CollisionStay2DEvent (Collision2D _collider);
	public event CollisionStay2DEvent CollisionStay2D;
	
	public delegate void CollisionExit2DEvent (Collision2D _collider);
	public event CollisionExit2DEvent CollisionExit2D;
	
	
	public delegate void TriggerEnterEvent (Collider _collider);
   	public event TriggerEnterEvent TriggerEnter;

   	public delegate void TriggerStayEvent (Collider _collider);
   	public event TriggerStayEvent TriggerStay;

   	public delegate void TriggerExitEvent (Collider _collider);
   	public event TriggerExitEvent TriggerExit;

   	public delegate void TriggerEnter2DEvent (Collider2D _collider);
   	public event TriggerEnter2DEvent TriggerEnter2D;

   	public delegate void TriggerStay2DEvent (Collider2D _collider);
   	public event TriggerStay2DEvent TriggerStay2D;

   	public delegate void TriggerExit2DEvent (Collider2D _collider);
   	public event TriggerExit2DEvent TriggerExit2D;
 	
 	
	public CollisionEvent OnCollisionEnterEvent = new CollisionEvent();
	public CollisionEvent OnCollisionStayEvent = new CollisionEvent();
	public CollisionEvent OnCollisionExitEvent = new CollisionEvent();
	public Collision2DEvent OnCollisionEnter2DEvent = new Collision2DEvent();
	public Collision2DEvent OnCollisionStay2DEvent = new Collision2DEvent();
	public Collision2DEvent OnCollisionExit2DEvent = new Collision2DEvent();
	
	public TriggerEvent OnTriggerEnterEvent = new TriggerEvent();
	public TriggerEvent OnTriggerStayEvent = new TriggerEvent();
	public TriggerEvent OnTriggerExitEvent = new TriggerEvent();
	public Trigger2DEvent OnTriggerEnter2DEvent = new Trigger2DEvent();
	public Trigger2DEvent OnTriggerStay2DEvent = new Trigger2DEvent();
	public Trigger2DEvent OnTriggerExit2DEvent = new Trigger2DEvent();
	
	[System.Serializable]
	public class CollisionEvent : UnityEvent<Collision> {}
	
	[System.Serializable]
	public class Collision2DEvent : UnityEvent<Collision2D> {}
	
	[System.Serializable]
	public class TriggerEvent : UnityEvent<Collider> {}
	
	[System.Serializable]
	public class Trigger2DEvent : UnityEvent<Collider2D> {}
	
	
   	void Start () {
		if(GetComponent<Collider>() == null && GetComponent<Collider2D>() == null) {
			DebugX.LogError(this, "No collider attached to "+transform.HierarchyPath());
			enabled = false;
		}
   	}
   	
	// Shared handler body: ignore-layer gate, fire the UnityEvent, then the C# event.
	void Dispatch<T> (T collider, int layer, UnityEvent<T> unityEvent, System.Action rawEvent) {
		if(ignoreLayers.Includes(layer)) return;
		unityEvent.Invoke(collider);
		rawEvent?.Invoke();
	}

	void OnCollisionEnter (Collision c)   => Dispatch(c, c.gameObject.layer, OnCollisionEnterEvent, () => CollisionEnter?.Invoke(c));
	void OnCollisionStay  (Collision c)   => Dispatch(c, c.gameObject.layer, OnCollisionStayEvent,  () => CollisionStay?.Invoke(c));
	void OnCollisionExit  (Collision c)   => Dispatch(c, c.gameObject.layer, OnCollisionExitEvent,  () => CollisionExit?.Invoke(c));

	void OnCollisionEnter2D (Collision2D c) => Dispatch(c, c.gameObject.layer, OnCollisionEnter2DEvent, () => CollisionEnter2D?.Invoke(c));
	void OnCollisionStay2D  (Collision2D c) => Dispatch(c, c.gameObject.layer, OnCollisionStay2DEvent,  () => CollisionStay2D?.Invoke(c));
	void OnCollisionExit2D  (Collision2D c) => Dispatch(c, c.gameObject.layer, OnCollisionExit2DEvent,  () => CollisionExit2D?.Invoke(c));

	void OnTriggerEnter (Collider c) => Dispatch(c, c.gameObject.layer, OnTriggerEnterEvent, () => TriggerEnter?.Invoke(c));
	void OnTriggerStay  (Collider c) => Dispatch(c, c.gameObject.layer, OnTriggerStayEvent,  () => TriggerStay?.Invoke(c));
	void OnTriggerExit  (Collider c) => Dispatch(c, c.gameObject.layer, OnTriggerExitEvent,  () => TriggerExit?.Invoke(c));

	void OnTriggerEnter2D (Collider2D c) => Dispatch(c, c.gameObject.layer, OnTriggerEnter2DEvent, () => TriggerEnter2D?.Invoke(c));
	void OnTriggerStay2D  (Collider2D c) => Dispatch(c, c.gameObject.layer, OnTriggerStay2DEvent,  () => TriggerStay2D?.Invoke(c));
	void OnTriggerExit2D  (Collider2D c) => Dispatch(c, c.gameObject.layer, OnTriggerExit2DEvent,  () => TriggerExit2D?.Invoke(c));
}
