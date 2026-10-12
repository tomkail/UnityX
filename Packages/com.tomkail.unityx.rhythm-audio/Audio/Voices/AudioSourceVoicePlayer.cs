using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityX.Rhythm {
	// Plays note sounds with AudioSource.PlayScheduled from a fixed pool. An AudioSource holds one sound at a time,
	// so a source is only reused once its sound has finished. When every source is busy, the sounding one that
	// finishes soonest is cut short; a queued sound is only taken when nothing is sounding, since its note would
	// never play at all.
	public sealed class AudioSourceVoicePlayer : IVoicePlayer {
		sealed class Channel {
			public AudioSource source;
			public double startDspTime;
			public double endDspTime;
			public int generation;
		}

		sealed class Voice : IVoice {
			readonly Channel channel;
			readonly int generation;
			readonly double length;

			public Voice(Channel channel, double startDspTime, double length) {
				this.channel = channel;
				generation = channel.generation;
				this.length = length;
				StartDspTime = startDspTime;
			}

			bool IsCurrent => channel.generation == generation;

			public double StartDspTime { get; private set; }

			public void Reschedule(double dspTime) {
				if (!IsCurrent) return;
				StartDspTime = dspTime;
				channel.startDspTime = dspTime;
				channel.endDspTime = dspTime + length;
				channel.source.SetScheduledStartTime(dspTime);
			}

			public void Stop() {
				if (!IsCurrent) return;
				channel.source.Stop();
				channel.endDspTime = double.NegativeInfinity;
				channel.generation++;
			}

			public bool IsFinished(double dspTime) => !IsCurrent || dspTime >= channel.endDspTime;
		}

		readonly List<Channel> channels = new();
		readonly LaneSoundMap sounds;
		readonly Func<double> currentDspTime;

		// currentDspTime defaults to AudioSettings.dspTime, the clock the sources actually play on
		public AudioSourceVoicePlayer(IEnumerable<AudioSource> sources, LaneSoundMap sounds, Func<double> currentDspTime = null) {
			this.sounds = sounds;
			this.currentDspTime = currentDspTime ?? (() => AudioSettings.dspTime);
			foreach (var source in sources) channels.Add(new Channel { source = source, endDspTime = double.NegativeInfinity });
			if (channels.Count == 0) throw new ArgumentException("Needs at least one AudioSource", nameof(sources));
		}

		public IVoice Play(NoteInstance note, double dspTime) {
			if (sounds == null || !sounds.TryGetSound(note.note, out var clip, out var volume)) return null;
			var channel = FreeChannel();
			channel.generation++;
			var source = channel.source;
			source.Stop();
			source.clip = clip;
			source.volume = volume * note.note.velocity;
			source.PlayScheduled(dspTime);
			var length = (double)clip.samples / clip.frequency;
			channel.startDspTime = dspTime;
			channel.endDspTime = dspTime + length;
			return new Voice(channel, dspTime, length);
		}

		Channel FreeChannel() {
			var now = currentDspTime();
			Channel sounding = null;
			Channel queued = null;
			foreach (var channel in channels) {
				if (channel.endDspTime <= now) return channel;
				if (channel.startDspTime <= now) {
					if (sounding == null || channel.endDspTime < sounding.endDspTime) sounding = channel;
				} else if (queued == null || channel.endDspTime < queued.endDspTime) {
					queued = channel;
				}
			}
			return sounding ?? queued;
		}
	}
}
