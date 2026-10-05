using NUnit.Framework;
using UnityEngine;

namespace UnityX.Rhythm.Audio.Tests {
	public class NoteAudioSchedulerTests {
		GameObject gameObject;
		Conductor conductor;
		NoteAudioScheduler audioScheduler;
		ManualAudioTimeSource source;
		LaneSoundMap sounds;
		AudioClip click;

		[SetUp]
		public void SetUp() {
			gameObject = new GameObject("Song");
			conductor = gameObject.AddComponent<Conductor>();
			conductor.smoothing = Conductor.SmoothingMode.Raw;
			conductor.TempoMap = new TempoMap(120);
			source = new ManualAudioTimeSource();
			conductor.Initialize(source);
			click = AudioClip.Create("click", 4410, 1, 44100, false);
			sounds = ScriptableObject.CreateInstance<LaneSoundMap>();
			sounds.entries.Add(new LaneSoundMap.Entry { lane = 0, clip = click });
			audioScheduler = gameObject.AddComponent<NoteAudioScheduler>();
			audioScheduler.conductor = conductor;
			audioScheduler.sounds = sounds;
			audioScheduler.Source = new BeatGrid(1);
		}

		[TearDown]
		public void TearDown() {
			Object.DestroyImmediate(gameObject);
			Object.DestroyImmediate(sounds);
			Object.DestroyImmediate(click);
		}

		void Frame() {
			source.Advance(1 / 60.0);
			conductor.Tick();
			audioScheduler.Tick();
		}

		// Each test starts just before beat 0: the raw clock moves in whole audio buffers, so a frame after Play(0)
		// beat 0 can already be too late to play

		[Test]
		public void CreatesItsVoicesAndQueuesTheClickTrack() {
			conductor.Clock.Play(-0.1);
			Frame();
			var voices = gameObject.transform.Find("Voices");
			Assert.IsNotNull(voices);
			Assert.AreEqual(audioScheduler.voiceCount, voices.GetComponents<AudioSource>().Length);
			Assert.IsNotNull(audioScheduler.Scheduler);
			Assert.AreEqual(1, audioScheduler.Scheduler.VoiceCount);
			Assert.AreEqual(click, voices.GetComponents<AudioSource>()[0].clip);
		}

		[Test]
		public void ABackingTrackOnTheSameObjectGetsItsOwnAudioSource() {
			conductor.Clock.Play(-0.1);
			Frame();
			var backing = gameObject.AddComponent<BackingTrack>();
			// The pool lives on a child, so the only AudioSource here is the one BackingTrack required
			Assert.AreEqual(1, gameObject.GetComponents<AudioSource>().Length);
			Assert.AreEqual(gameObject.GetComponent<AudioSource>(), backing.AudioSource);
		}

		[Test]
		public void ChangingTheSourceWhilePlayingTakesEffect() {
			conductor.Clock.Play(-0.1);
			Frame();
			Assert.AreEqual(1, audioScheduler.Scheduler.VoiceCount);
			audioScheduler.Source = new BeatGrid(1, lane: 1);
			Frame();
			// Lane 1 has no sound, and the queued click was cancelled
			Assert.AreEqual(0, audioScheduler.Scheduler.VoiceCount);
		}
	}
}
