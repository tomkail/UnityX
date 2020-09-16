using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputTester : MonoBehaviour {
    public float fingerSize = 50;
    Finger hoveredFinger;
    Finger selectedFinger;
    void Update () {
        hoveredFinger = InputX.Instance.fingers.Best(x => Vector2.Distance(x.position, Input.mousePosition), (other, currentBest) => other < currentBest, fingerSize, null);
        
        if(Input.GetMouseButton(0)) {
            if(Input.GetMouseButtonDown(0)) {
                if(hoveredFinger != null) {
                    selectedFinger = hoveredFinger;
                } else {
                    var finger = new Finger(Input.mousePosition);
                    InputX.Instance.fingers.Add(finger);
                    selectedFinger = finger;
                }
            }
            if(selectedFinger != null)
                selectedFinger.UpdatePosition(Input.mousePosition);
        }
        if(Input.GetMouseButtonUp(0)) {
            selectedFinger = null;
        }
        if(Input.GetMouseButtonDown(1)) {
            if(hoveredFinger != null) {
                hoveredFinger.End();
                InputX.Instance.fingers.Remove(hoveredFinger);
            }
        }
    }
}