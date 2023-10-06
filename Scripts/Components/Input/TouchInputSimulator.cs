using System;
using System.Linq;
using UnityEngine;

public class TouchInputSimulator : MonoSingleton<TouchInputSimulator> {
    public bool removeFingerOnMouseUp;
    public float fingerSize = 50;

    // Pinch _pinch;
    // Pinch pinch {
    //     get {
    //         return _pinch;
    //     } set {
    //         if(pinch == value) return;
    //         if(pinch != null && OnPinchStart != null) OnPinchStart(pinch);
    //     }
    // }
    // public Action<Pinch> OnPinchStart;
	// Finger pinchFinger1;
	// Finger pinchFinger2;
	Finger heldFinger;

    Finger hoveredFinger;
    Finger selectedFinger;

    void Update () {
        // UpdateTest();

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


        
		// if(Input.GetMouseButtonDown(1)) {
		// 	heldFinger = null;
		// 	if(pinchFinger1 != null) {
		// 		if(RectX.CreateFromCenter(pinchFinger1.position, Vector2.one * fingerSize).Contains(Input.mousePosition)) {
		// 			pinchFinger1 = null;
		// 			pinch = null;
		// 		}
		// 	}
		// 	if(pinchFinger2 != null) {
		// 		if(RectX.CreateFromCenter(pinchFinger2.position, Vector2.one * fingerSize).Contains(Input.mousePosition)) {
		// 			pinchFinger2 = null;
		// 			pinch = null;
		// 		}
		// 	}
		// }
        // if(pinchFinger1 == null && pinchFinger2 != null) {
        //     pinchFinger1 = pinchFinger2;
        //     pinchFinger2 = null;
        // }

		// if(Input.GetMouseButton(0)) {
		// 	if(Input.GetMouseButtonDown(0)) {
		// 		if(pinchFinger1 != null) {
		// 			if(RectX.CreateFromCenter(pinchFinger1.position, Vector2.one * fingerSize).Contains(Input.mousePosition)) {
		// 				heldFinger = pinchFinger1;
		// 			}
		// 		}
		// 		if(pinchFinger2 != null) {
		// 			if(RectX.CreateFromCenter(pinchFinger2.position, Vector2.one * fingerSize).Contains(Input.mousePosition)) {
		// 				heldFinger = pinchFinger2;
		// 			}
		// 		}
				
		// 		if(heldFinger == null) {
		// 			if(pinchFinger1 == null) {
		// 				pinchFinger1 = new Finger(new Touch());
		// 				pinchFinger1.Start();
		// 				pinchFinger1.UpdatePosition(Input.mousePosition);
		// 				heldFinger = pinchFinger1;

		// 				if(pinchFinger2 != null) {
		// 					pinch = new Pinch(pinchFinger1, pinchFinger2);
		// 				}
		// 			} else if(pinchFinger2 == null) {
		// 				pinchFinger2 = new Finger(new Touch());
		// 				pinchFinger2.Start();
		// 				pinchFinger2.UpdatePosition(Input.mousePosition);
		// 				heldFinger = pinchFinger2;

		// 				if(pinchFinger1 != null) {
		// 					pinch = new Pinch(pinchFinger1, pinchFinger2);
		// 				}
		// 			}
		// 		}
		// 	}

		// 	if(heldFinger != null)
		// 		heldFinger.UpdatePosition(Input.mousePosition);
		// }
		// if(Input.GetMouseButtonUp(0)) {
		// 	heldFinger = null;
		// }
		// if(pinch != null) pinch.UpdateGesture();
    }
    /*
    void UpdateTest () {
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
			pinchFinger1 = new Finger(new Touch());
			pinchFinger1.Start();
			pinchFinger1.UpdatePosition(new Vector2(0, Screen.height * 0.5f));

			pinchFinger2 = new Finger(new Touch());
			pinchFinger2.Start();
			pinchFinger2.UpdatePosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

			pinch = new Pinch(pinchFinger1, pinchFinger2);
			pinch.UpdateGesture();
		}
		if(Input.GetKeyDown(KeyCode.Alpha2)) {
			pinchFinger1 = new Finger(new Touch());
			pinchFinger1.Start();
			pinchFinger1.UpdatePosition(new Vector2(0, Screen.height * 0.5f));

			pinchFinger2 = new Finger(new Touch());
			pinchFinger2.Start();
			pinchFinger2.UpdatePosition(new Vector2(Screen.width, Screen.height * 0.5f));

			pinch = new Pinch(pinchFinger1, pinchFinger2);
			pinch.UpdateGesture();
		}
		if(Input.GetKeyDown(KeyCode.Space)) {
			// ScreenToPointOnPlane(camera, pinch.inputPoints[0].position, floorPlane, out Vector3 startWorldPoint);
			// ScreenToPointOnPlane(camera, pinch.inputPoints[1].position, floorPlane, out Vector3 endWorldPoint);
			// var worldDistanceBetweenFingers = Vector3.Distance(endWorldPoint, startWorldPoint);
			// ScreenToPointOnPlane(camera, pinch.inputPoints[0].lastPosition, floorPlane, out Vector3 lastStartWorldPoint);
			// ScreenToPointOnPlane(camera, pinch.inputPoints[1].lastPosition, floorPlane, out Vector3 lastEndWorldPoint);
			// var lastWorldDistanceBetweenFingers = Vector3.Distance(lastEndWorldPoint, lastStartWorldPoint);

			// var width = Mathf.Abs(endWorldPoint.x - startWorldPoint.x);
			// var lastWidth = Mathf.Abs(endWorldPoint.x - startWorldPoint.x);
			// var distanceToFillBothFingers = camera.GetDistanceAtFrustrumWidth(width-lastWidth);
			// Debug.Log(width+" "+lastWidth+" "+distanceToFillBothFingers);
			// cameraProperties.distance += distanceToFillBothFingers;
			// cameraProperties.targetPoint = Vector3.Lerp(startWorldPoint, endWorldPoint, 0.5f);
		}
	}
    */

	void OnGUI () {
        for (int i = 0; i < InputX.Instance.fingers.Count; i++) {
            Finger finger = InputX.Instance.fingers[i];
            GUI.Box(RectX.CreateFromCenter(ScreenToGUIPoint(finger.position), Vector2.one * fingerSize), (i+1).ToString());
        }
		// if(pinchFinger1 != null) GUI.Box(RectX.CreateFromCenter(ScreenToGUIPoint(pinchFinger1.position), Vector2.one * fingerSize), "1");
		// if(pinchFinger2 != null) GUI.Box(RectX.CreateFromCenter(ScreenToGUIPoint(pinchFinger2.position), Vector2.one * fingerSize), "2");

		// if(GUI.Button(new Rect(100, 0, 60, 60), "1")) {
		// 	pinchFinger1 = null;
		// 	pinch = null;
		// }
		// if(GUI.Button(new Rect(160, 0, 60, 60), "2")) {
		// 	pinchFinger2 = null;
		// 	pinch = null;
		// }
		// if(GUI.Button(new Rect(220, 0, 200, 60), "RESET")) {
		// 	pinchFinger1 = null;
		// 	pinchFinger2 = null;
		// 	heldFinger = null;
		// 	pinch = null;
		// 	targetPointMomentumDamper.Reset(new Vector3(0, 8, 0));
		// 	distanceMomentumDamper.Reset(8);
		// }


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
                // GUILayout.Box((pinchStartCameraSize * pinchDistanceFraction).ToString());
                // GUILayout.Box(pinchDistanceFraction.ToString());
                // GUILayout.Box(pinchStartCameraSize.ToString());

            }
		}
        // if(pinch != null) {
        //     GUILayout.Box((pinchStartCameraSize * pinchDistanceFraction).ToString());
        //     GUILayout.Box(pinch.currentPinchCenter.ToString());
        //     GUILayout.Box(pinch.startPinchDistance.ToString());
        //     GUILayout.Box(pinch.currentPinchDistance.ToString());
        //     GUILayout.Box(pinchDistanceFraction.ToString());
        //     GUILayout.Box(pinchStartCameraSize.ToString());
        // }

		Vector2 ScreenToGUIPoint (Vector2 point) {
			return new Vector2(point.x, Screen.height-point.y);
		}
	}
}
