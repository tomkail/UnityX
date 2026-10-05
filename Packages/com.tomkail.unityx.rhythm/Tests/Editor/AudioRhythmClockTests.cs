using NUnit.Framework;

namespace UnityX.Rhythm.Tests {
	public class AudioRhythmClockTests {
		FakeAudioTimeSource source;
		AudioRhythmClock clock;

		[SetUp]
		public void SetUp() {
			source = new FakeAudioTimeSource();
			// Raw keeps these tests exact; smoothing has its own tests
			clock = new AudioRhythmClock(source, new RawClockSmoother());
			clock.Tick();
		}

		void Advance(double seconds) {
			source.Advance(seconds);
			clock.Tick();
		}

		[Test]
		public void PlaysWithLeadIn() {
			clock.Play(-1);
			Assert.IsTrue(clock.IsPlaying);
			Assert.AreEqual(-1, clock.SongTime, 1e-9);
			var startDsp = clock.DspTime;
			Assert.AreEqual(startDsp + 1, clock.DspTimeAtSongTime(0), 1e-9);
			Advance(source.BufferDuration * 10);
			Assert.AreEqual(-1 + source.BufferDuration * 10, clock.SongTime, 1e-9);
		}

		[Test]
		public void PauseFreezesAndResumeContinues() {
			clock.Play(0);
			Advance(source.BufferDuration * 20);
			clock.Pause();
			var pausedAt = clock.SongTime;
			Advance(1);
			Assert.AreEqual(pausedAt, clock.SongTime, 1e-12);
			Assert.IsTrue(double.IsPositiveInfinity(clock.DspTimeAtSongTime(pausedAt + 1)));
			clock.Resume();
			Assert.AreEqual(pausedAt, clock.SongTime, 1e-9);
			Advance(source.BufferDuration * 4);
			Assert.AreEqual(pausedAt + source.BufferDuration * 4, clock.SongTime, 1e-9);
		}

		[Test]
		public void SeekJumps() {
			clock.Play(0);
			clock.Seek(30);
			Assert.AreEqual(30, clock.SongTime, 1e-9);
		}

		[Test]
		public void RateChangeKeepsPositionAndScalesSpeed() {
			clock.Play(0);
			Advance(source.BufferDuration * 43);
			var before = clock.SongTime;
			clock.SetPlaybackRate(0.5);
			Assert.AreEqual(before, clock.SongTime, 1e-9, "No jump");
			Advance(source.BufferDuration * 10);
			Assert.AreEqual(before + source.BufferDuration * 5, clock.SongTime, 1e-9);
			Assert.AreEqual(0.5, clock.PlaybackRate, 1e-12);
		}

		[Test]
		public void FutureRateChangeAppliesAtItsTime() {
			clock.Play(0);
			var changeAt = clock.DspTime + 2;
			clock.SetPlaybackRate(2, changeAt);
			Assert.AreEqual(1, clock.PlaybackRate, 1e-12);
			Assert.AreEqual(2, clock.SongTimeAtDspTime(changeAt), 1e-9);
			Assert.AreEqual(4, clock.SongTimeAtDspTime(changeAt + 1), 1e-9);
			Assert.AreEqual(changeAt + 1, clock.DspTimeAtSongTime(4), 1e-9);
			for (var song = 0.0; song < 6; song += 0.3) Assert.AreEqual(song, clock.SongTimeAtDspTime(clock.DspTimeAtSongTime(song)), 1e-9);
		}

		[Test]
		public void TimelineChangedFiresOnPlayPauseSeekAndRate() {
			var count = 0;
			clock.TimelineChanged += () => count++;
			clock.Play(0);
			clock.SetPlaybackRate(0.75);
			clock.Seek(3);
			clock.Pause();
			Assert.AreEqual(4, count);
		}

		[Test]
		public void MapsRealtimeTimestampsOntoTheAudioClock() {
			clock.Play(0);
			Advance(0.5);
			var realtime = source.Realtime - 0.01;
			Assert.AreEqual(clock.DspTime - 0.01, clock.RealtimeToDspTime(realtime), 1e-9);
		}

		[Test]
		public void SongTimeCarriesOnWhenTheAudioDeviceResets() {
			clock.Play(0);
			Advance(1);
			var before = clock.SongTime;
			source.DspOffset = 5 - source.Realtime;
			Advance(1 / 60.0);
			Assert.AreEqual(before, clock.SongTime, 0.05, "No jump in the music");
			Assert.Less(clock.DspTime, 10, "Clock is on the new audio timeline");
			var after = clock.SongTime;
			Advance(0.5);
			Assert.AreEqual(after + 0.5, clock.SongTime, 0.05, "Keeps advancing at normal speed");
		}

		[Test]
		public void InvalidRateThrows() {
			Assert.Throws<System.ArgumentOutOfRangeException>(() => clock.SetPlaybackRate(0));
		}
	}
}
