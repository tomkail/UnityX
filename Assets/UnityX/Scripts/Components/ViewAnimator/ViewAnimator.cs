using UnityEngine;
using System.Linq;
using System.Collections.Generic;

// Can be used to fire off events on a timeline.
// Designed for fine control over animations that need to play in sequence
public class ViewAnimator : MonoBehaviour {
    public ViewAnimationEvent AddEvent (ViewAnimationEvent animEvent) {
        if(animEvent == null) return null;
        if(animEvent.endTime < _animationTime) Debug.LogWarning("ViewAnimationEvent '"+animEvent.name+"' end time ("+animEvent.endTime+") is in the past! Current time is "+_animationTime);
        
        bool inserted = false;
        for(int i = 0; i < animationEvents.Count; i++) {
            if(animEvent.startTime < animationEvents[i].startTime) {
                animationEvents.Insert(i, animEvent);
                inserted = true;
                break;
            }
        }
        if(!inserted) animationEvents.Add(animEvent);

        if(CheckEvent(_lastAnimationTime, animationTime, animEvent)) {
            RemoveEvent(animEvent);
            return null;
        } else {
            return animEvent;
        }
    }
    public void RemoveEvent (ViewAnimationEvent animEvent) {
        if(animEvent == null) return;
        if(animationEvents.Remove(animEvent)) {
            animationEventHistory.Add(animEvent);
        }
    }
    float _lastAnimationTime = -1;
    [SerializeField]
    float _animationTime;
    public float animationTime {
        get {
            return _animationTime;
        } set {
            if(_animationTime == value) return;
            _lastAnimationTime = _animationTime;
            _animationTime = value;
            CheckEvents(_lastAnimationTime, animationTime);
        }
    }
    
    public bool isPlayingOrHasFutureEvents {
        get {
            return animationEvents.Count != 0;
        }
    }
    public bool isCurrentlyPlayingAnyAnimation {
        get {
            for(int i = animationEvents.Count-1; i >= 0; i--) {
                var currentEvent = animationEvents[i];
                if(currentEvent.IsPlayingAtTime(animationTime))
                    return true;
            }
            return false;
        }
    }

    [SerializeField]
    List<ViewAnimationEvent> animationEvents = new List<ViewAnimationEvent>();
    // This is just for debugging
    [SerializeField]
    List<ViewAnimationEvent> animationEventHistory = new List<ViewAnimationEvent>();

    public void Clear () {
        animationEvents.Clear();
        animationEventHistory.Clear();
    }
    
    public void Reset () {
        animationEvents.Clear();
        animationEventHistory.Clear();
        _lastAnimationTime = -1;
        _animationTime = 0;
    }

    public IEnumerable<ViewAnimationEvent> GetCurrentAndFutureAnimationEvents () {
        for(int i = animationEvents.Count-1; i >= 0; i--) {
            yield return animationEvents[i];
        }
    }

    public float GetFuturemostAnimationEventTime () {
        if(animationEvents.Count != 0) return animationEvents.Max(x => x.endTime);
        else if(animationEventHistory.Count != 0) return animationEventHistory.Max(x => x.endTime);
        else return 0;
    }

    public void CancelCurrentAndFutureEvents (bool completeEvents) {
        for(int i = animationEvents.Count-1; i >= 0; i--) {
            var currentEvent = animationEvents[i];
            CancelEvent(currentEvent, completeEvents);
        }
    }
    
    public void CancelFutureEvents (bool completeEvents) {
        for(int i = animationEvents.Count-1; i >= 0; i--) {
            var currentEvent = animationEvents[i];
            if(animationTime < currentEvent.startTime) {
                CancelEvent(currentEvent, completeEvents);
            }
        }
    }
    public void CancelCurrentlyPlayingEvents (bool completeEvents) {
        for(int i = animationEvents.Count-1; i >= 0; i--) {
            var currentEvent = animationEvents[i];
            if(currentEvent.IsPlayingAtTime(animationTime)) {
                CancelEvent(currentEvent, completeEvents);
            }
        }
    }
    
    void CheckEvents (float lastAnimationTime, float animationTime) {
        if(animationEvents.Count == 0) return;
        // This generates a small amount of garbage! We could cache it in the class, but I'm slightly wary of this function being called recursively and breaking entirely.
        // It shouldn't ever be called recursively though, so perhaps that's moot.
        List<ViewAnimationEvent> animationEventsToCheck = new List<ViewAnimationEvent>(animationEvents);
        for(int i = 0; i < animationEventsToCheck.Count; i++) {
            var animEvent = animationEventsToCheck[i];
            // This can fire when events are removed as a result of an event in this loop. I'm not sure what the best behaviour is here!
            // Debug.Assert(animationEvents.Contains(animEvent));
            if(CheckEvent(lastAnimationTime, animationTime, animEvent)) {
                animationEventsToCheck.RemoveAt(i);
                // It's possible we've removed the event as a result of completing it - if we clear the view animator, for example. In that case, don't add it back!
                RemoveEvent(animEvent);
                i--;
            }
        }
    }

    static bool CheckEvent (float lastAnimationTime, float animationTime, ViewAnimationEvent animEvent) {
        if(animationTime >= animEvent.startTime && lastAnimationTime < animEvent.startTime) {
            if(animEvent.onStart != null) animEvent.onStart();
        }

        if(animationTime >= animEvent.startTime) {
            if(animEvent.onChangeProgress != null) {
                var progress = 1f;
                // If end time == startTime InverseLerp returns 0, so default to 1!
                if(animEvent.startTime < animEvent.endTime) progress = Mathf.InverseLerp(animEvent.startTime, animEvent.endTime, animationTime);
                animEvent.onChangeProgress(progress);
            }
        }
        
        if(animationTime >= animEvent.endTime && lastAnimationTime < animEvent.endTime) {
            if(animEvent.onComplete != null) animEvent.onComplete();
            return true;
        }
        return false;
    }

    void CancelEvent (ViewAnimationEvent animEvent, bool completeEvents) {
        RemoveEvent(animEvent);
        if(completeEvents && animEvent.onComplete != null) animEvent.onComplete();
    }
}
