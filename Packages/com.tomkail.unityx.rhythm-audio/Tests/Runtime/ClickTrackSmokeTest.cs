using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityX.Rhythm.Audio.Tests {
	// Real audio, real frames: a click track at 120bpm for a couple of seconds
	public class ClickTrackSmokeTest {
		GameObject gameObject;
		AudioClip click;
		LaneSoundMap sounds;

		[TearDown]
		public void TearDown() {
			UnityEngine.Object.Destroy(gameObject);
			UnityEngine.Object.Destroy(sounds);
			UnityEngine.Object.Destroy(click);
		}

		[UnityTest]
		public IEnumerator ClickTrackPlaysOnTheBeat() {
			gameObject = new GameObject("Song");
			gameObject.AddComponent<AudioListener>();
			var conductor = gameObject.AddComponent<Conductor>();
			conductor.playOnStart = false;
			conductor.TempoMap = new TempoMap(120);

			var rate = AudioSettings.outputSampleRate;
			click = AudioClip.Create("click", rate / 4, 1, rate, false);
			var samples = new float[rate / 4];
			for (var i = 0; i < samples.Length; i++) samples[i] = 0.1f * Mathf.Sin(i * 0.1f);
			click.SetData(samples, 0);
			sounds = ScriptableObject.CreateInstance<LaneSoundMap>();
			sounds.entries.Add(new LaneSoundMap.Entry { lane = 0, clip = click });

			var audioScheduler = gameObject.AddComponent<NoteAudioScheduler>();
			audioScheduler.conductor = conductor;
			audioScheduler.sounds = sounds;
			audioScheduler.Source = new BeatGrid(1);
			var audioSources = gameObject.GetComponents<AudioSource>();

			yield return null;
			conductor.Clock.Play(-0.5);
			var lastSongTime = double.NegativeInfinity;
			var heard = new HashSet<long>();
			var worstStartError = 0.0;
			while (conductor.SongTime < 2.2) {
				yield return null;
				Assert.GreaterOrEqual(conductor.SongTime, lastSongTime, "Song time went backwards");
				lastSongTime = conductor.SongTime;
				foreach (var audioSource in audioSources) {
					if (!audioSource.isPlaying || audioSource.timeSamples <= 0) continue;
					// When this sound actually started, on the audio clock
					var started = AudioSettings.dspTime - (double)audioSource.timeSamples / rate;
					var beat = Math.Round(conductor.BeatAtDspTime(started));
					heard.Add((long)beat);
					worstStartError = Math.Max(worstStartError, Math.Abs(started - conductor.DspTimeAtBeat(beat)));
				}
			}
			CollectionAssert.IsSupersetOf(heard, new long[] { 0, 1, 2, 3 });
			// dspTime and timeSamples both move in whole audio buffers, so allow two buffers
			AudioSettings.GetDSPBufferSize(out var bufferLength, out _);
			Assert.Less(worstStartError, 2.0 * bufferLength / rate + 0.005, $"Clicks started up to {worstStartError * 1000:F1}ms off the beat");
		}
	}
}
