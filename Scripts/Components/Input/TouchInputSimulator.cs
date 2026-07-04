using System;
using System.Linq;
using UnityEngine;

public class TouchInputSimulator : MonoSingleton<TouchInputSimulator> {
    public bool removeFingerOnMouseUp;
    public float fingerSize = 50;

	Finger heldFinger;

    Finger hoveredFinger;
    Finger selectedFinger;

    void Update () {

        hoveredFinger = InputX.Instance.fingers.Best(x => Vector2.Distance(x.position, Input.mousePosition), (other, currentBest) => other < currentBest, fingerSize, null);
        

        if(Input.GetMouseButton(0)) {
            if(Input.GetMouseButtonDown(0)) {
                if(hoveredFinger != null) {
                    selectedFinger = hoveredFinger;
                } else {
                    var touch = new Touch();
                    touch.phase = TouchPhase.Began;
                    touch.fingerId = InputX.Instance.fingers.Count;
                    touch.position = Input.mousePosition;
                    var finger = new Finger(touch);
                    finger.updatedManually = true;
                    InputX.Instance.AddFinger(finger);
                    selectedFinger = finger;
                }
            }
            if(selectedFinger != null) {
                selectedFinger.UpdatePosition(Input.mousePosition);
            }
        }
        foreach(var finger in InputX.Instance.fingers) {
            if(finger.updatedManually)
                finger.UpdateState();
        }
        if(Input.GetMouseButtonUp(0)) {
            if(selectedFinger != null) {
                if(removeFingerOnMouseUp) {
                    selectedFinger.End();
                    InputX.Instance.RemoveFinger(selectedFinger);
                }
                selectedFinger = null;
            }
        }
        if(Input.GetMouseButtonDown(1)) {
            if(hoveredFinger != null) {
                hoveredFinger.End();
                InputX.Instance.RemoveFinger(hoveredFinger);
            }
        }
    }

	void OnGUI () {
        for (int i = 0; i < InputX.Instance.fingers.Count; i++) {
            Finger finger = InputX.Instance.fingers[i];
            GUI.Box(RectX.CreateFromCenter(ScreenToGUIPoint(finger.position), Vector2.one * fingerSize), (i+1).ToString());
        }

        if(InputX.Instance.pinches.Any()) {
            int i = 0;
            foreach(var pinch in InputX.Instance.pinches) {
                GUILayout.Window(i, new Rect(0,0,100,100), (int id) => {
                    GUILayout.Box("Point 1: "+pinch.inputPoint1.position.ToString());
                    GUILayout.Box("Point 2: "+pinch.inputPoint2.position.ToString());
                    GUILayout.Box("Center: "+pinch.currentPinchCenter.ToString());
                    GUILayout.Box("Start Distance: "+pinch.startPinchDistance.ToString());
                    GUILayout.Box("Distance: "+pinch.currentPinchDistance.ToString());
                }, "Pinch "+(i+1));
            }
		}

		Vector2 ScreenToGUIPoint (Vector2 point) {
			return new Vector2(point.x, Screen.height-point.y);
		}
	}
}
