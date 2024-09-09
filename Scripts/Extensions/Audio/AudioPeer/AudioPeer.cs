using UnityEngine;
using UnityEngine.Audio;

// From https://www.youtube.com/watch?v=KqVsu07oAEM&ab_channel=PeerPlay
// We might also want to look at this plugin - https://github.com/keijiro/unity-audio-spectrum
[RequireComponent (typeof (AudioSource))]
public class AudioPeer : MonoBehaviour {
	AudioSource _audioSource;

	const FFTWindow fftWindow = FFTWindow.Blackman;

	//Microphone input
	public AudioClip _audioclip;
	public bool _useMicrophone;
	public string _selectedMicrophoneDevice;
	//different input use different audio mixer group (master/microphone)
	public AudioMixerGroup _mixerGroupMicrophone, _mixerGroupMaster;

	private float[] _samplesLeft;
	private float[] _samplesRight;
	[HideInInspector]
	public float[] _samplesStereo;
	public int _samplesLength = 512;

	//audio 8
	private float[] _freqBand = new float[8];
	private float[] _bandBuffer = new float[8];
	private float[] _bufferDecrease = new float[8];
	private float[] _freqBandHighest = new float[8];

	//audio 64
	private float[] _freqBand64 = new float[64];
	private float[] _bandBuffer64 = new float[64];
	private float[] _bufferDecrease64 = new float[64];
	private float[] _freqBandHighest64 = new float[64];

	[HideInInspector]
	public float[] _audioBand, _audioBandBuffer;

	//audio 64
	[HideInInspector]
	public float[] _audioBand64, _audioBandBuffer64;

	[HideInInspector]
	public float _Amplitude, _AmplitudeBuffer;
	private float _AmplitudeHighest;
	public float _audioProfile;

	public enum Channel { Stereo, Left, Right }
	public Channel channel = Channel.Stereo;

	// Use this for initialization
	void Start () {
		_samplesLeft = new float[_samplesLength];
		_samplesRight = new float[_samplesLength];
		_samplesStereo = new float [_samplesLength];

		_audioBand = new float[8];
		_audioBandBuffer = new float[8];
		_audioBand64 = new float[64];
		_audioBandBuffer64 = new float[64];

		
		// _mixerGroupMicrophone = Resources.Load<AudioMixerGroup>("Audios/AudioMixer/Microphone");


		_audioSource = GetComponent<AudioSource> ();
		AudioProfile (_audioProfile);

		// _audioclip = Resources.Load<AudioClip> ("Audios/audioclip_1");

		//Microphone input

		if (_useMicrophone) {

			if (Microphone.devices.Length > 0) {

				_selectedMicrophoneDevice = Microphone.devices[0];
				_audioSource.outputAudioMixerGroup = _mixerGroupMicrophone;
				_audioSource.clip = Microphone.Start (_selectedMicrophoneDevice, true, 10, AudioSettings.outputSampleRate);

			} else {
				_useMicrophone = false;
			}

		}

		if (!_useMicrophone) {
			_audioSource.outputAudioMixerGroup = _mixerGroupMaster;
			_audioSource.clip = _audioclip;
		}

		_audioSource.Play ();
	}

	// Update is called once per frame
	void Update () {
		GetSpectrumAudioSource ();
		GetStereoSpectrumAudioSource();
		MakeFrequencyBands ();
		MakeFrequencyBands64 ();
		BandBuffer (_freqBand, _bandBuffer, _bufferDecrease);
		BandBuffer (_freqBand64, _bandBuffer64, _bufferDecrease64);
		CreateAudioBands ();
		CreateAudioBands64 ();
		GetAmplitude (); //Get Average Amplitude

	}

	void AudioProfile (float audioProfile) {
		for (int i = 0; i < 8; i++) {
			_freqBandHighest[i] = audioProfile;
		}
	}

	void GetAmplitude () {
		float _CurrentAmplitude = 0;
		float _CurrentAmplitudeBuffer = 0;
		for (int i = 0; i < 8; i++) {
			_CurrentAmplitude += _audioBand[i];
			_CurrentAmplitudeBuffer += _audioBandBuffer[i];
		}

		if (_CurrentAmplitude > _AmplitudeHighest) {

			_AmplitudeHighest = _CurrentAmplitude;
		}

		_Amplitude = _CurrentAmplitude / _AmplitudeHighest;
		_AmplitudeBuffer = _CurrentAmplitudeBuffer / _AmplitudeHighest;
	}

	void CreateAudioBands () {
		for (int i = 0; i < 8; i++) {
			if (_freqBand[i] > _freqBandHighest[i]) {
				_freqBandHighest[i] = _freqBand[i];
			}

			_audioBand[i] = (_freqBand[i] / _freqBandHighest[i]);
			_audioBandBuffer[i] = (_bandBuffer[i] / _freqBandHighest[i]);
		}
	}

	void CreateAudioBands64 () {
		for (int i = 0; i < 64; i++) {
			if (_freqBand64[i] > _freqBandHighest64[i]) {
				_freqBandHighest64[i] = _freqBand64[i];
			}

			_audioBand64[i] = (_freqBand64[i] / _freqBandHighest64[i]);
			_audioBandBuffer64[i] = (_bandBuffer64[i] / _freqBandHighest64[i]);
		}
	}

	void GetSpectrumAudioSource () {
		// if(_audioSource.clip.channels == 1) {
		// } else if(_audioSource.clip.channels == 2) {
			_audioSource.GetSpectrumData (_samplesLeft, 0, fftWindow);
			_audioSource.GetSpectrumData (_samplesRight, 1, fftWindow);
		// }
	}

	void GetStereoSpectrumAudioSource(){
		for(int i = 0; i < _samplesLength; i++){
			_samplesStereo[i] = _samplesLeft[i] + _samplesRight[i];
		}
	}

	// The buffer smoothes out the frequency bands, which are typically very volitile. 
	// If the current value is higher than the buffer, immediately set the buffer to the current value and reset the bufferDecrease.
	// If it's lower, than reduce the value of the buffer, and increase bufferDecrease so that it'll fall off more rapidly over time.
	static void BandBuffer (float[] freqBand, float[] bandBuffer, float[] bufferDecrease) {
		for (int g = 0; g < bandBuffer.Length; g++) {
			if (freqBand[g] > bandBuffer[g]) {
				bandBuffer[g] = freqBand[g];
				bufferDecrease[g] = 0.005f;
			}

			if (freqBand[g] < bandBuffer[g]) {
				bandBuffer[g] -= bufferDecrease[g];
				bufferDecrease[g] *= 1.2f;
			}
		}
	}

	void MakeFrequencyBands () {

		/*
         * hertz = first need to know the amount of hertz of the song
         * hertz / 512 (bands) = amount of hertz per sample
         * 
         * 7 bands channels:
         * 20-60 hertz
         * 60-250 hertz
         * 250-500 hertz
         * 500-2000 hertz
         * 2000-4000 hertz
         * 4000-6000 hertz
         * 6000-20000 hertz
         * 
         * @@@@@@@@@@EXAMPLE@@@@@@@@@@@@
         * hertz = 22050
         * hertz per sample = 22050 / 512 = 43 hertz per sample
         *         * Frequancy bands:
         * counter |frequancy sample| amount of hertz per sample * amount of boxes | Range
         * 0       | 2              | 43 hertz * 2 = 86 hertz                      | 0 - 86
         * 1       | 4              | 43 hertz * 4 =  172 hertz                    | 87 - 258
         * 2       | 8              | 43 hertz * 8 =  344 hertz                    | 259 - 600
         * 3       | 16             | 43 hertz * 16 =  688 hertz                   | 601 - 1288
         * 4       | 32             | 43 hertz * 32 =  1376 hertz                  | 1289 - 2665
         * 5       | 64             | 43 hertz * 64 =  2752 hertz                  | 2666 - 5418
         * 6       | 128            | 43 hertz * 128 =  5504 hertz                 | 5419 - 10922
         * 7       | 256            | 43 hertz * 256 =  11008 hertz                | 10923 - 21930
         *           =
         *           510. This is less than 512 so we'll add the remaining 2 samples to the last band.
         * @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
         * 
         * 
         * 
         * Frequancy bands:
         * boxes
         * counter | frequancy sample | amount of hertz per sample * amount of boxes
         * 0       | 2                | 
         * 1       | 4                | 
         * 2       | 8                | 
         * 3       | 16               | 
         * 4       | 32               | 
         * 5       | 64               | 
         * 6       | 128              | 
         * 7       | 256              | 
         * 
  
         */
		
		int count = 0;
		for (int i = 0; i < 8; i++) {
			float average = 0;
			int sampleCount = (int) Mathf.Pow (2, i) * 2;

			if (i == 7) {
				//510 is less than 512, so we add the last 2 samples to it so not to waste them
				sampleCount += 2;
			}
			for (int j = 0; j < sampleCount; j++) {
				if (channel == Channel.Stereo) {
					average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
				}
				else if (channel == Channel.Left) {
					average += (_samplesLeft[count] * (count + 1));
				}
				else if (channel == Channel.Right) {
					average += (_samplesRight[count]) * (count + 1);
				}
				count++;

			}

			average /= count;

			_freqBand[i] = average * 10;
		}
	}

	void MakeFrequencyBands64 () {

		/*
			*
			*0 - 15 = 1 sample  =16
			*16 - 31 = 2 sample =32
			*32 - 39 = 4 sample =32
			*40 - 47 = 6 sample =48
			*48 - 55 = 16 sample =128
			*56 - 63 = 32 sample =256
			*512
			*/

		int count = 0;
		int sampleCount = 1;
		int power = 0;

		for (int i = 0; i < 64; i++) {
			float average = 0;

			if (i == 16 || i == 32 || i == 40 || i == 48 || i == 56) {

				power++;
				sampleCount = (int) Mathf.Pow (2, power);

				if (power == 3) {
					sampleCount -= 2; //40-47 = 6
				}
			}

			for (int j = 0; j < sampleCount; j++) {
				if (channel == Channel.Stereo) {
					average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
				}
				else if (channel == Channel.Left) {
					average += (_samplesLeft[count] * (count + 1));
				}
				else if (channel == Channel.Right) {
					average += (_samplesRight[count]) * (count + 1);
				}
				count++;

			}

			average /= count;

			_freqBand64[i] = average * 10;
		}
	}
}