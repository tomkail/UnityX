using UnityEngine;

namespace UnityX.Rhythm {
	// Reads Unity's real-time and audio clocks
	public sealed class UnityAudioTimeSource : IAudioTimeSource {
		public double Realtime => Time.realtimeSinceStartupAsDouble;
		public double DspTime => AudioSettings.dspTime;
		public double BufferDuration {
			get {
				AudioSettings.GetDSPBufferSize(out var bufferLength, out _);
				var sampleRate = AudioSettings.outputSampleRate;
				return sampleRate > 0 ? bufferLength / (double)sampleRate : 0.02;
			}
		}
	}
}
