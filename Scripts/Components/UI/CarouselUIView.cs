using System.Collections.Generic;
using UnityEngine;

// This class is used to create a carousel effect for a list of CanvasGroups.
[ExecuteAlways]
public class CarouselUIView : MonoBehaviour {
    public List<CanvasGroup> canvasGroups = new();
    public float currentTime;
    public float imageDuration = 3.0f;
    public float crossfadeDuration = 1.0f;
    
    public void Awake() {
        currentTime = 0;
        Refresh();
    }

    public CanvasGroup GetActiveItem () {
        var index = Mathf.FloorToInt(currentTime / imageDuration);
        index %= canvasGroups.Count;
        return canvasGroups[index];
    }
    
    void Update() {
        if(Application.isPlaying)
            currentTime += Time.deltaTime;
        Refresh();
    }

    void Refresh() {
        if (canvasGroups.IsNullOrEmpty()) return;
        float totalTime = imageDuration * canvasGroups.Count;
        if (canvasGroups.Count > 1) {
            for (int i = 0; i < canvasGroups.Count; i++) {
                var startTime = i * imageDuration;
                var signedDelta = SignedDeltaRepeating(0, totalTime, startTime, currentTime);
                var alpha = DoubleInverseLerp(-crossfadeDuration*0.5f, 0, imageDuration, currentTime < crossfadeDuration * 0.5f ? imageDuration : imageDuration+crossfadeDuration*0.5f, signedDelta);
                canvasGroups[i].alpha = alpha;
            }
        } else if (canvasGroups.Count == 1) {
            canvasGroups[0].alpha = 1;
        }
    }
    
    // Calculates the shortest difference between two given values.
    static float SignedDeltaRepeating(float a, float b, float val, float target) {
        b-=a;
        val-=a;
        target-=a;
        float delta = Mathf.Repeat((target - val), b);
        if (delta > b*0.5f)
            delta -= b;
        return delta;
    }
	
    // InverseLerp that works with two ranges.
    static float DoubleInverseLerp (float a, float b, float c, float d, float value) {
        Debug.Assert(a <= b);
        Debug.Assert(b <= c);
        Debug.Assert(c <= d);
        if(value >= d) return 0;
        else if(value >= c) return Mathf.InverseLerp(d, c, value);
        else if(value >= b) return 1;
        if(value >= a) return Mathf.InverseLerp(a, b, value);
        return 0;
    }
}