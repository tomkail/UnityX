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
   	
	void OnCollisionEnter (Collision _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnCollisionEnterEvent.Invoke(_collider);
		if(CollisionEnter != null) CollisionEnter(_collider);
	}
	
	void OnCollisionStay (Collision _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnCollisionStayEvent.Invoke(_collider);
		if(CollisionStay != null) CollisionStay(_collider);
	}

	void OnCollisionExit (Collision _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnCollisionExitEvent.Invoke(_collider);
		if(CollisionExit != null) CollisionExit(_collider);
	}


	void OnCollisionEnter2D (Collision2D _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnCollisionEnter2DEvent.Invoke(_collider);
		if(CollisionEnter2D != null) CollisionEnter2D(_collider);
	}
	
	void OnCollisionStay2D (Collision2D _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnCollisionStay2DEvent.Invoke(_collider);
		if(CollisionStay2D != null) CollisionStay2D(_collider);
	}

	void OnCollisionExit2D (Collision2D _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnCollisionExit2DEvent.Invoke(_collider);
		if(CollisionExit2D != null) CollisionExit2D(_collider);
	}
	
	
	void OnTriggerEnter (Collider _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnTriggerEnterEvent.Invoke(_collider);
		if(TriggerEnter != null) TriggerEnter(_collider);
	}
	
	void OnTriggerStay (Collider _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnTriggerStayEvent.Invoke(_collider);
		if(TriggerStay != null) TriggerStay(_collider);
	}
	
	void OnTriggerExit (Collider _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnTriggerExitEvent.Invoke(_collider);
		if(TriggerExit != null) TriggerExit(_collider);
	}
	
	
	void OnTriggerEnter2D (Collider2D _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnTriggerEnter2DEvent.Invoke(_collider);
		if(TriggerEnter2D != null) TriggerEnter2D(_collider);
	}
	
	void OnTriggerStay2D (Collider2D _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnTriggerStay2DEvent.Invoke(_collider);
		if(TriggerStay2D != null) TriggerStay2D(_collider);
	}
	
	void OnTriggerExit2D (Collider2D _collider) {
		if(ignoreLayers.Includes(_collider.gameObject.layer)) return;
		OnTriggerExit2DEvent.Invoke(_collider);
		if(TriggerExit2D != null) TriggerExit2D(_collider);
	}
}
