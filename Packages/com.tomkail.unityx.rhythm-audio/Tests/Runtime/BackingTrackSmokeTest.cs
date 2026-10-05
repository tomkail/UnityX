using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityX.Rhythm.Audio.Tests {
	// Real audio, real frames: a backing clip following the clock through play, a rate change and a seek
	public class BackingTrackSmokeTest {
		GameObject gameObject;
		AudioClip clip;

		[TearDown]
		public void TearDown() {
			UnityEngine.Object.Destroy(gameObject);
			UnityEngine.Object.Destroy(clip);
		}

		[UnityTest]
		public IEnumerator BackingTrackStaysInStep() {
			gameObject = new GameObject("Song");
			gameObject.AddComponent<AudioListener>();
			var conductor = gameObject.AddComponent<Conductor>();
			conductor.playOnStart = false;
			var rate = AudioSettings.outputSampleRate;
			clip = AudioClip.Create("backing", rate * 8, 1, rate, false);
			var samples = new float[rate * 8];
			for (var i = 0; i < samples.Length; i++) samples[i] = 0.05f * Mathf.Sin(i * 0.05f);
			clip.SetData(samples, 0);
			var audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			audioSource.clip = clip;
			var backing = gameObject.AddComponent<BackingTrack>();
			backing.conductor = conductor;
			AudioSettings.GetDSPBufferSize(out var bufferLength, out _);
			// Clip position and dspTime both move in whole audio buffers
			var tolerance = 2.0 * bufferLength / rate + 0.005;

			yield return null;
			conductor.Clock.Play(-0.2);
			yield return WaitForSongTime(conductor, 1);
			AssertInStep(conductor, audioSource, tolerance, "after starting");

			conductor.Clock.SetPlaybackRate(0.5);
			yield return WaitForRealSeconds(0.6);
			Assert.AreEqual(0.5f, audioSource.pitch);
			AssertInStep(conductor, audioSource, tolerance, "after halving the rate");

			conductor.Clock.Seek(5);
			yield return WaitForRealSeconds(0.6);
			AssertInStep(conductor, audioSource, tolerance, "after seeking");

			conductor.Clock.Pause();
			yield return null;
			yield return null;
			Assert.IsFalse(audioSource.isPlaying);
		}

		static void AssertInStep(Conductor conductor, AudioSource audioSource, double tolerance, string when) {
			Assert.IsTrue(audioSource.isPlaying, $"Not playing {when}");
			var drift = BackingTrackSync.Drift(conductor.Clock, 0, (double)audioSource.timeSamples / audioSource.clip.frequency, AudioSettings.dspTime);
			Assert.Less(Math.Abs(drift), tolerance, $"{drift * 1000:F1}ms out {when}");
		}

		static IEnumerator WaitForSongTime(Conductor conductor, double songTime) {
			while (conductor.SongTime < songTime) yield return null;
		}

		static IEnumerator WaitForRealSeconds(double seconds) {
			var end = Time.realtimeSinceStartupAsDouble + seconds;
			while (Time.realtimeSinceStartupAsDouble < end) yield return null;
		}
	}
}
