using NUnit.Framework;
using UnityEngine;

namespace UnityX.Rhythm.Audio.Tests {
	public class AudioSourceVoicePlayerTests {
		GameObject gameObject;
		AudioSource[] sources;
		LaneSoundMap sounds;
		AudioClip kick;
		AudioClip snare;
		AudioClip accent;
		double now;

		[SetUp]
		public void SetUp() {
			gameObject = new GameObject("Voices");
			sources = new[] { gameObject.AddComponent<AudioSource>(), gameObject.AddComponent<AudioSource>() };
			// 0.1s each
			kick = AudioClip.Create("kick", 4410, 1, 44100, false);
			snare = AudioClip.Create("snare", 4410, 1, 44100, false);
			accent = AudioClip.Create("accent", 4410, 1, 44100, false);
			sounds = ScriptableObject.CreateInstance<LaneSoundMap>();
			sounds.entries.Add(new LaneSoundMap.Entry { lane = 0, clip = kick, volume = 0.5f });
			sounds.entries.Add(new LaneSoundMap.Entry { lane = 1, clip = snare });
			sounds.entries.Add(new LaneSoundMap.Entry { lane = 1, matchData = true, data = 1, clip = accent });
			now = 100;
		}

		[TearDown]
		public void TearDown() {
			Object.DestroyImmediate(gameObject);
			Object.DestroyImmediate(sounds);
			Object.DestroyImmediate(kick);
			Object.DestroyImmediate(snare);
			Object.DestroyImmediate(accent);
		}

		AudioSourceVoicePlayer CreatePlayer() => new(sources, sounds, () => now);

		static NoteInstance NoteOn(int lane, int data = 0, float velocity = 1) => new(new NoteId(0, 0, lane, 0), new Note(0, lane, 0, velocity, data));

		[Test]
		public void LaneSoundMapPrefersAnEntryForTheNotesData() {
			Assert.IsTrue(sounds.TryGetSound(new Note(0, 1), out var clip, out _));
			Assert.AreEqual(snare, clip);
			Assert.IsTrue(sounds.TryGetSound(new Note(0, 1, 0, 1, 1), out clip, out _));
			Assert.AreEqual(accent, clip);
			Assert.IsFalse(sounds.TryGetSound(new Note(0, 5), out _, out _));
		}

		[Test]
		public void PlaysTheLaneClipAtVolumeTimesVelocity() {
			var voice = CreatePlayer().Play(NoteOn(0, velocity: 0.5f), 100.2);
			Assert.IsNotNull(voice);
			Assert.AreEqual(kick, sources[0].clip);
			Assert.AreEqual(0.25f, sources[0].volume, 1e-6f);
			Assert.AreEqual(100.2, voice.StartDspTime);
			Assert.IsFalse(voice.IsFinished(100.25));
			Assert.IsTrue(voice.IsFinished(100.3));
		}

		[Test]
		public void LanesWithoutASoundPlayNothing() {
			Assert.IsNull(CreatePlayer().Play(NoteOn(5), 100.2));
		}

		[Test]
		public void OverlappingSoundsUseSeparateSources() {
			var player = CreatePlayer();
			player.Play(NoteOn(0), 100.2);
			player.Play(NoteOn(1), 100.25);
			Assert.AreEqual(kick, sources[0].clip);
			Assert.AreEqual(snare, sources[1].clip);
		}

		[Test]
		public void FinishedSourcesAreReused() {
			var player = CreatePlayer();
			var first = player.Play(NoteOn(0), 100);
			now = 100.2;
			player.Play(NoteOn(1), 100.3);
			Assert.AreEqual(snare, sources[0].clip);
			// The first voice's source belongs to another sound now, so stopping it does nothing to that sound
			Assert.IsTrue(first.IsFinished(now));
			first.Stop();
			Assert.IsFalse(player.Play(NoteOn(0), 100.3) == null);
			Assert.AreEqual(kick, sources[1].clip);
		}

		[Test]
		public void WhenEverySourceIsBusyTheOneFinishingSoonestIsTaken() {
			var player = CreatePlayer();
			var early = player.Play(NoteOn(0), 100.2);
			var late = player.Play(NoteOn(0), 100.25);
			player.Play(NoteOn(1), 100.26);
			Assert.AreEqual(snare, sources[0].clip);
			Assert.IsTrue(early.IsFinished(now));
			Assert.IsFalse(late.IsFinished(now));
		}

		[Test]
		public void StoppingFreesTheSource() {
			var player = CreatePlayer();
			var voice = player.Play(NoteOn(0), 100.2);
			voice.Stop();
			Assert.IsTrue(voice.IsFinished(now));
			player.Play(NoteOn(1), 100.3);
			Assert.AreEqual(snare, sources[0].clip);
		}

		[Test]
		public void ReschedulingMovesTheEnd() {
			var voice = CreatePlayer().Play(NoteOn(0), 100.2);
			voice.Reschedule(100.5);
			Assert.AreEqual(100.5, voice.StartDspTime);
			Assert.IsFalse(voice.IsFinished(100.55));
		}
	}
}
