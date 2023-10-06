using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class FPSManager : MonoSingleton<FPSManager> {
    public FPSSettings settings;
    public FPSDebugSettings debugSettings;
    public float averageFPS = 0.0f;
    public float maxFPS = 0.0f;
    public float minFPS = 0.0f;

    
    public float averageFrameTime {
        get {
            return 1f/averageFPS;
        }
    }
    public float targetFrameTime {
        get {
            var _averageFrameTime = averageFrameTime;
            if(Application.targetFrameRate <= 0) {
                return _averageFrameTime;
            }
            // If the difference between the average and target frame times is significantly different to the target frame time, then return the average frame rate instead
            var _targetFrameTime = 1f/Application.targetFrameRate;
            var difference = Mathf.Abs(_averageFrameTime - _targetFrameTime);
            if(difference > _targetFrameTime * 0.25f) {
                // Debug.Log("Returning _averageFrameTime because average "+_targetFrameTime+" "+_averageFrameTime+" "+difference);
                return _averageFrameTime;
            } else {
                return _targetFrameTime;
            }
        }
    }

    void OnEnable () {
        RefreshTargetFrameRate();
        SetInitialFPS();
    }
    void Update() {
        RefreshTargetFrameRate();
        RefreshAverageFPS();
    }

    void RefreshTargetFrameRate () {
        #if UNITY_EDITOR
        // VSync must be disabled in editor for targetFrameRate to work
        if(QualitySettings.vSyncCount != 0)
            QualitySettings.vSyncCount = 0;
        #endif
        if(Application.targetFrameRate != settings.targetFrameRate)
            Application.targetFrameRate = settings.targetFrameRate;
    }

    void SetInitialFPS () {
		averageFPS = settings.targetFrameRate;
		maxFPS = settings.targetFrameRate;
		minFPS = settings.targetFrameRate;
    }

	private void RefreshAverageFPS () {
		// Add this frame to the deltaTimes
		deltaTimes.Add (Time.unscaledDeltaTime);
		RemoveOldDeltaTimes ();

        int numDeltaTimes = deltaTimes.Count;

		averageFPS = 0.0f;
		maxFPS = 0.0f;
		minFPS = 0.0f;

		if (numDeltaTimes > 0)
		{
			// Calculate the min, max and mean frame rates.
			float minDeltaTime = float.MaxValue;
			float maxDeltaTime = float.MinValue;
			float averageDeltaTime = 0.0f;
			foreach (float currDeltaTime in deltaTimes)
			{
				minDeltaTime = Mathf.Min (minDeltaTime, currDeltaTime);
				maxDeltaTime = Mathf.Max (maxDeltaTime, currDeltaTime);
				averageDeltaTime += currDeltaTime;
			}
			averageDeltaTime /= numDeltaTimes;

			averageFPS = 1.0f / averageDeltaTime;
			maxFPS = 1.0f / minDeltaTime;
			minFPS = 1.0f / maxDeltaTime;
		}
	}

	private void RemoveOldDeltaTimes () {
		// Remove old times.
		float totalTime = 0.0f;
		for (int currTimeIndex = deltaTimes.Count - 1; currTimeIndex >= 0; --currTimeIndex) {
			totalTime += deltaTimes[currTimeIndex];
			if (totalTime > debugSettings.fpsGraphHistoryTime) {
				deltaTimes.RemoveRange(0, currTimeIndex);
				break;
			}
		}
	}

	private void OnGUI()
	{
        var show = false;
        if(Application.isEditor) {
            if(debugSettings.showInEditor) show = true;
        } else {
            if(Debug.isDebugBuild && debugSettings.showInDevBuilds) show = true;
            else if (!Debug.isDebugBuild && debugSettings.showInReleaseBuilds) show = true;
        }
		if(show) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("FPS");
            sb.Append("TAR: ");
            sb.AppendLine(string.Format("{0:n1}",settings.targetFrameRate));
            sb.Append("AVG: ");
            sb.AppendLine(string.Format("{0:n1}",averageFPS));
            sb.Append("MAX: ");
            sb.AppendLine(string.Format("{0:n1}",maxFPS));
            sb.Append("MIN: ");
            sb.AppendLine(string.Format("{0:n1}",minFPS));
            GUI.Label (debugSettings.fpsPos, sb.ToString());
        }
	}
	
	private List<float> deltaTimes = new List<float>();
}