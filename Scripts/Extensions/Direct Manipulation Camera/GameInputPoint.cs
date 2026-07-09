using System;
using UnityEngine;

[System.Serializable]
public class GameInputPoint {
    public string name;
    public InputPoint inputPoint {
        get {
            if (finger != null) return finger;
            else if (mouseInput != null) return mouseInput;
            return null;
        }
    }

    public float timeDown {
        get {                
            if(inputPoint is MouseInput) return ((MouseInput)inputPoint).leftButton.activeTime;
            else if(inputPoint is Finger) return inputPoint.activeTime;
            else return 0f;
        }
    }
    public int framesDown {
        get {                
            if(inputPoint is MouseInput) return ((MouseInput)inputPoint).leftButton.activeFrames;
            else if(inputPoint is Finger) return inputPoint.activeFrames;
            else return -1;
        }
    }


    MouseInput mouseInput;
    Finger finger;

    

    public enum GameInputPointTarget {
        None,
        // If the touch starts on UI or when not interactive, for example.
        Invalid,
        UI,
        Camera,
        World,
        Interactable,
    }
    [SerializeField]
    GameInputPointTarget _target;
    public GameInputPointTarget target {
        get {
            return _target;
        } set {
            if(_target == value) return;
            var lastTarget = _target;
            _target = value;
            RefreshName();
            if(OnChangeTarget != null) OnChangeTarget(this, lastTarget, _target);
        }
    }
    public Action<GameInputPoint, GameInputPointTarget, GameInputPointTarget> OnChangeTarget;

    public GameInputPoint (Finger inputPoint) {
        this.finger = inputPoint;
        RefreshName();
    }
    public GameInputPoint (MouseInput inputPoint) {
        this.mouseInput = inputPoint;
        RefreshName();
    }

    void RefreshName () {
        name = "Input Point "+target+" "+GetInputSourceString();
    }

    public string GetInputSourceString () {
        return mouseInput == null ? ("Finger "+finger.fingerId+ "("+finger.fingerArrayIndex+")") : "Mouse input";
    }

    // public bool DownForMoreThan (float minTime) {
    //     return timeDown > minTime/* && framesDown * FPSManager.Instance.targetFrameTime > minTime*/;
    // }
}