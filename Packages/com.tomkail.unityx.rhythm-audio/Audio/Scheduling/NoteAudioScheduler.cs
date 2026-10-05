using UnityEngine;
using UnityEngine.Audio;

namespace UnityX.Rhythm {
	// Plays a note source's sounds in time with a Conductor. Set Source from code; a click track is a BeatGrid.
	// Runs after the Conductor so it schedules against this frame's clock.
	[DefaultExecutionOrder(-900)]
	public class NoteAudioScheduler : MonoBehaviour {
		public Conductor conductor;
		public LaneSoundMap sounds;
		[Tooltip("How many sounds can overlap")]
		[Min(1)] public int voiceCount = 16;
		public AudioMixerGroup output;
		[Tooltip("How far ahead sounds are queued, in seconds. Long enough to cover a slow frame.")]
		public double lookAhead = 0.2;

		INoteSource source;
		AudioSource[] audioSources;

		public NoteSoundScheduler Scheduler { get; private set; }

		public INoteSource Source {
			get => source;
			set {
				source = value;
				if (Scheduler != null) Scheduler.Source = value;
			}
		}

		void Awake() => EnsureAudioSources();

		void EnsureAudioSources() {
			if (audioSources != null) return;
			audioSources = new AudioSource[Mathf.Max(1, voiceCount)];
			for (var i = 0; i < audioSources.Length; i++) {
				var audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				audioSource.outputAudioMixerGroup = output;
				audioSources[i] = audioSource;
			}
		}

		void OnDisable() {
			Scheduler?.Dispose();
			Scheduler = null;
		}

		void Update() => Tick();

		// Update calls this; tests call it directly
		public void Tick() {
			if (Scheduler == null) {
				// The conductor creates its clock in Awake, which may not have run when this is enabled
				if (conductor == null || conductor.Clock == null) return;
				EnsureAudioSources();
				Scheduler = new NoteSoundScheduler(conductor, new AudioSourceVoicePlayer(audioSources, sounds), source, lookAhead);
			}
			Scheduler.LookAhead = lookAhead;
			Scheduler.Update();
		}
	}
}
