using System;

[System.Serializable]
public class ViewAnimationEvent {
    public string name;
    public float startTime;
    public float endTime;
    public float duration {
        get {
            return endTime-startTime;
        }
    }
    public bool isTrigger {
        get {
            return startTime == endTime;
        }
    }
    [System.NonSerialized]
    public Action onStart;
    [System.NonSerialized]
    public Action<float> onChangeProgress;
    [System.NonSerialized]
    public Action onComplete;

    protected ViewAnimationEvent() {}
    public static ViewAnimationEvent CreateAnimation (string name, float startTime, float endTime, Action<float> onChangeProgress) {
        var viewEvent = new ViewAnimationEvent();
        viewEvent.name = name;
        viewEvent.startTime = startTime;
        viewEvent.endTime = endTime;
        if(viewEvent.startTime > viewEvent.endTime) UnityEngine.Debug.LogWarning("Event starts after ending!");
        viewEvent.onChangeProgress = onChangeProgress;
        return viewEvent;
    }
    public static ViewAnimationEvent CreateAnimation (string name, float startTime, float endTime, Action<float> onChangeProgress, Action onComplete) {
        var viewEvent = CreateAnimation(name, startTime, endTime, onChangeProgress);
        viewEvent.onComplete = onComplete;
        return viewEvent;
    }
    public static ViewAnimationEvent CreateAnimation (string name, float startTime, float endTime, Action<float> onChangeProgress, Action onStart, Action onComplete) {
        var viewEvent = CreateAnimation(name, startTime, endTime, onChangeProgress);
        viewEvent.onStart = onStart;
        viewEvent.onComplete = onComplete;
        return viewEvent;
    }
    public static ViewAnimationEvent CreateEvent (string name, float time, Action onTrigger) {
        var viewEvent = new ViewAnimationEvent();
        viewEvent.name = name;
        viewEvent.startTime = time;
        viewEvent.endTime = time;
        viewEvent.onComplete = onTrigger;
        return viewEvent;
    }
    
    public bool IsPlayingAtTime (float time) {
        if(time >= startTime && time <= endTime) return true;
        else return false;
    }
}