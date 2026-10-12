using NUnit.Framework;

namespace UnityX.Rhythm.Audio.Tests {
	public class BackingTrackSyncTests {
		TestSong song;

		[SetUp]
		public void SetUp() => song = new TestSong(120);

		[Test]
		public void NothingPlaysWhilePaused() {
			Assert.IsFalse(BackingTrackSync.Resync(song.clock, 0, 10, 0.05).play);
		}

		[Test]
		public void ALeadInSchedulesTheClipForSongTimeZero() {
			song.clock.Play(-1);
			var start = BackingTrackSync.Resync(song.clock, 0, 10, 0.05);
			Assert.IsTrue(start.play);
			Assert.AreEqual(0, start.clipTime);
			Assert.AreEqual(song.clock.DspTimeAtSongTime(0), start.dspTime, 1e-9);
			Assert.AreEqual(song.clock.DspTime + 1, start.dspTime, 1e-9);
		}

		[Test]
		public void MidSongStartsFromWhereTheSongWillBe() {
			song.clock.Play(3);
			var start = BackingTrackSync.Resync(song.clock, 1, 10, 0.05);
			Assert.AreEqual(song.clock.DspTime + 0.05, start.dspTime, 1e-9);
			// Song time 3.05 is 2.05s into a clip that starts at song time 1
			Assert.AreEqual(2.05, start.clipTime, 1e-9);
			Assert.AreEqual(1, start.pitch);
		}

		[Test]
		public void PlaybackRateSetsPitchAndScalesTheDelay() {
			song.clock.SetPlaybackRate(0.5);
			song.clock.Play(3);
			var start = BackingTrackSync.Resync(song.clock, 0, 10, 0.05);
			Assert.AreEqual(0.5, start.pitch);
			Assert.AreEqual(3.025, start.clipTime, 1e-9);
		}

		[Test]
		public void PastTheEndPlaysNothing() {
			song.clock.Play(12);
			Assert.IsFalse(BackingTrackSync.Resync(song.clock, 0, 10, 0.05).play);
		}

		[Test]
		public void DriftIsHowFarTheClipIsAhead() {
			song.clock.Play(3);
			var dsp = song.clock.DspTime;
			Assert.AreEqual(0.02, BackingTrackSync.Drift(song.clock, 1, 2.02, dsp), 1e-9);
			Assert.AreEqual(-0.5, BackingTrackSync.Drift(song.clock, 1, 1.5, dsp), 1e-9);
		}
	}
}
