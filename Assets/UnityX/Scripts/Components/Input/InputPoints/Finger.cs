using UnityEngine;
using System.Collections;

[System.Serializable]
public class Finger : InputPoint {
	//The ID of the touch, as defined by Unity's Touch class. This never changes.
	public int fingerId;
	//The index of the touch, where index 0 is the active touch and index 1 is the second touch
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
	public Finger(Touch _touch) : base (_touch.position) {
		fingerId = _touch.fingerId;
		name = "Finger "+fingerId;
        Debug.Assert(fingerId >= 0, "Touch finger ID is "+fingerId);
	}
	
	public override string ToString () {
		return string.Format ("[Finger] Name {0} ID {1} State {2} Position {3}", name, fingerId, state, position);
	}
}
