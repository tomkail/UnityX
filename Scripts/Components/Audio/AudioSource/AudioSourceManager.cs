using UnityEngine;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceManager : MonoBehaviour {

	public bool pauseWhenTimescaleIsZero = true;
	public bool pauseWhenVolumeIsZero = false;


	public bool currentlyPausedBecauseTimescaleIsZero {
		get {
			bool playing = false;
			if(canPlayBlender.TryGetValueForSource("Timescale", out playing)) return !playing;
			else return false;
		}
	}
	LogicBlender<bool> canPlayBlender = new LogicBlender<bool>(source => source == null || source.All(x => x));
	public bool startPlayingAtRandomTime;

	private AudioSource _audioSource;
	public AudioSource audioSource {
		get {
            if(_audioSource == null) {
                _audioSource = GetComponent<AudioSource>();
		        if(_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            }
			return _audioSource;
		} private set {
			_audioSource = value;
		}
	}

	// Checks if we're not playing and not at the start of the clip
	public bool paused {
		get {
			return !audioSource.isPlaying && audioSource.timeSamples != 0;
		}
	}


	// Returns if the AudioSource is playing or supposed to be playing but currently out of focus
	// When Application.runInBackground and Application.isFocused are false Unity pauses all AudioSources. 
	// In this case Update stops running, but during the last Update AudioSource.isPlaying is already marked false.
	// isPlaying fixes this by returning the state of the AudioSource the frame before we lost focus if not currently focused. 
	// The only thing this doesn't do well is pausing/unpausing when not focused, which you can do really easily by clicking the pause button in the inspector.
	public bool isPlaying {
		get {
            #if !UNITY_EDITOR && (UNITY_PS4 || UNITY_SWITCH)
            return audioSource.isPlaying;
            #else
			if(Application.isFocused || Application.runInBackground) return audioSource.isPlaying;
			else return audioSourceWasPlayingWhileFocused;
            #endif
        }
	}

	bool wasPlaying;
	bool playStateChangedThisFrame {
		get {
			return wasPlaying != audioSource.isPlaying;
		}
	}
	bool changedFocusThisFrame {
		get {
			return wasFocused != Application.isFocused;
		}
	}
	bool wasFocused;
	public bool audioSourceWasPlayingWhileFocused;

	public float normalizedTime {
		get {
			if(audioSource.clip == null) return 0;
			return (float)audioSource.timeSamples/audioSource.clip.samples;
		}
	}

	public delegate void AudioSourceEvent(AudioSourceManager sourceManager);
	public event AudioSourceEvent OnPlay;
	public event AudioSourceEvent OnPause;
	public event AudioSourceEvent OnUnPause;
	// Called on reaching the end of the clip. 
	// Note that effects might cause this sound to last for time after this.
	public event AudioSourceEvent OnStopOrFinish;

	public FloatTween volumeTween {
		get {
			return _volumeTween;
		} private set {
			_volumeTween = value;
		}
	}
	
	void Reset () {
		audioSource = GetComponent<AudioSource>();
		if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
	}

	void Awake () {
		_wasPlaying = audioSource.isPlaying;
		audioSourceWasPlayingWhileFocused = audioSource.isPlaying;
		if(audioSource.isPlaying && startPlayingAtRandomTime && audioSource.clip != null) audioSource.time = Random.Range(0, audioSource.clip.length);
		volumeTween.OnChange += OnChangeVolumeTween;
		volumeTween.Reset(audioSource.volume);
        audioSource.volume = volumeTween.currentValue;
		
		canPlayBlender.onChange += (bool canPlay) => {
			if(canPlay) audioSource.UnPause();
			else audioSource.Pause();
		};
		EnforcePauseState();
	}

    void OnChangeVolumeTween (float newVolume) {
        audioSource.volume = volumeTween.currentValue;
    }
	
	void Update () {
		volumeTween.Update(Time.unscaledDeltaTime);
	}

	void Pause () {
		canPlayBlender.Set("Manual", false);
	}

	void UnPause () {
		canPlayBlender.Remove("Manual");
	}

	void LateUpdate () {
		EnforcePauseState();
		UpdateIsPlaying();
		if(!changedFocusThisFrame && (Application.isFocused || Application.runInBackground)) {
			audioSourceWasPlayingWhileFocused = audioSource.isPlaying;
		}
		wasFocused = Application.isFocused;
		wasPlaying = audioSource.isPlaying;
	}

	void EnforcePauseState () {
		if(Time.timeScale == 0 && pauseWhenTimescaleIsZero) {
			canPlayBlender.Set("Timescale", false);
		} else {
			canPlayBlender.Remove("Timescale");
		}
		
		if(audioSource.volume == 0 && pauseWhenVolumeIsZero) {
			canPlayBlender.Set("Volume", false);
		} else {
			canPlayBlender.Remove("Volume");
		}

		// if(audioSource.isPlaying && pauseBlender.value) audioSource.Pause();
		// else if(!audioSource.isPlaying && !pauseBlender.value) audioSource.Play();
	}

	void UpdateIsPlaying () {
		if(audioSource.isPlaying != _wasPlaying) {
			_wasPlaying = audioSource.isPlaying;
			if(audioSource.isPlaying) {
				if(audioSource.timeSamples == 0) {
					if(OnPlay != null) OnPlay(this);
				} else {
					if(OnUnPause != null) OnUnPause(this);
				}
			} else {
				const float toleranceSeconds = 0.05f;
				var clip = audioSource.clip;
				int toleranceSamples = clip == null ? 0 : Mathf.CeilToInt(toleranceSeconds * clip.frequency * clip.channels);
				if(clip == null || audioSource.timeSamples == 0 || audioSource.timeSamples >= audioSource.clip.samples-toleranceSamples) {
					if(OnStopOrFinish != null) OnStopOrFinish(this);
				} else {
					if(OnPause!= null) OnPause(this);
				}
			}
		}
	}
		
	bool _wasPlaying;
	[SerializeField]
	FloatTween _volumeTween = new FloatTween();

	#if UNITY_EDITOR
	public void LogPauseBlender () {
		DebugX.Log(this, canPlayBlender.ToString());
	}
	#endif
}