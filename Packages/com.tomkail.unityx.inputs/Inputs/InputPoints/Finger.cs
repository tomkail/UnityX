using UnityEngine;
using System.Collections;

namespace UnityX.Inputs {

[System.Serializable]
public class Finger : InputPoint {
	//The ID of the touch, as defined by Unity's Touch class. This never changes.
	public int fingerId;
	//Enumeration order of this touch within the current touch set (0-based); not a priority or "active" ordering.
	public int fingerArrayIndex;
	//The index of the touch, where index 0 is the active touch and index 1 is the second touch
	//public int order;
    
    // A finger that actually mirroring the mouse, while the mouse is held.
    public bool isFakeMouseFinger;

	public Finger(Vector2 position) : base (position) {
        updatedManually = true;
    }
	public Finger(MouseInput mouseInput) : base (mouseInput.position) {
		fingerId = -1;
        isFakeMouseFinger = true;
		name = "Fake mouse finger";
	}
	// Ingests an Input System EnhancedTouch touch (identified by touchId) at a screen position.
	// Kept framework-agnostic (int + Vector2) so this type doesn't reference the Input System namespace.
	public Finger(int fingerId, Vector2 position) : base (position) {
		this.fingerId = fingerId;
		name = "Finger "+fingerId;
        Debug.Assert(fingerId >= 0, "Touch finger ID is "+fingerId);
	}
	
	public override string ToString () {
		return string.Format ("[Finger] Name {0} ID {1} State {2} Position {3}", name, fingerId, state, position);
	}
}
}
