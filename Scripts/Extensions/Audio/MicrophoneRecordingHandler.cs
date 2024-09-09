using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

// Handles recording from a microphone device into a buffer that can be written to an AudioClip.
// Can also be used for streaming, monitoring or live playback.

// The device and frequency cannot be changed after recording has started.
// Allows pausing and resuming.

// Recording example:
// var microphoneSampleRate = MicrophoneRecordingHandler.GetValidFrequencyForDevice(Microphone.devices[0], AudioSettings.outputSampleRate);
// var recordingHandler = new MicrophoneRecordingHandler(Microphone.devices[0], microphoneSampleRate);
// recordingHandler.StartRecording();
// In update:
// recordingHandler.Update();
// When done:
// recordingHandler.StopRecording();
// var audioClip = recordingHandler.CreateAudioClip("Voice Recording");

// Listening example (useful for streaming:
// var microphoneSampleRate = MicrophoneRecordingHandler.GetValidFrequencyForDevice(Microphone.devices[0], AudioSettings.outputSampleRate);
// var recordingHandler = new MicrophoneRecordingHandler(Microphone.devices[0], microphoneSampleRate);
// recordingHandler.StartListening();
// var microphoneInputLastPosition = recordingHandler.lastMicrophoneSamplePosition;
// In update:
// var numSamples = recordingHandler.CalculateDeltaSampleCount(ref microphoneInputLastPosition);
// if (numSamples > 0) {
//     float[] samples = new float[numSamples];
//     recordingHandler.GetRecentMicrophoneData(samples);
//     deepgramLive.SendData(DeepgramUtils.SampleDataToLiveStreamingByteArray(samples));
// }

// For live playback:
// recordingHandler = new MicrophoneRecordingHandler(Microphone.devices[0]);
// recordingHandler.StartListening();
// audioVisualizationUIView.microphoneAudioClip = recordingHandler.liveMicrophoneOutputAudioClip;
// if (Microphone.IsRecording(microphoneDeviceName) && recordingHandler.liveMicrophoneOutputAudioClip != null) {
//     currentVolume = GetAverageVolumeFromAudioClip(recordingHandler.liveMicrophoneOutputAudioClip, Microphone.GetPosition(microphoneDeviceName), averageSampleTimeLengthSeconds, sampleTimeLengthSeconds, true);
// }
// /*
// // To play back
// while (!(Microphone.GetPosition(null) > 0)) {}
// audioSource.clip = liveMicrophoneOutputAudioClip;
// audioSource.loop = true;
// audioSource.Play();
// */
public class MicrophoneRecordingHandler {
    public const int defaultRecordingFrequency = 44100;
    
    public string deviceName { get; }
    public int frequency { get; } = defaultRecordingFrequency; // 44.1kHz sample rate
    public int channels { get; private set; } = 1;
    
    
    public bool listening => Microphone.IsRecording(deviceName);
    // This can be used for live playback. You could play it back, or monitor the volume input.
    // If you use this object reference take care not to don't Destroy it while it's being recorded into! This class handles Destroying it when the recording ends.
    public AudioClip liveMicrophoneOutputAudioClip { get; private set; }
    
    public bool isRecording { private set; get; }
    public bool recordingPaused => recordingLength > 0 && !isRecording;
    public float recordingLength => (recordingAudioData == null || frequency == 0) ? 0 : recordingAudioData.Count / (float)frequency;
    public List<float> recordingAudioData { get; private set; }
    public Action OnStartRecording;
    public Action OnStopRecording;
    public Action OnClearRecording;
        
    int lastMicrophoneSamplePosition;
    
    public MicrophoneRecordingHandler(string deviceName, int frequency = defaultRecordingFrequency) {
        this.deviceName = deviceName;
        this.frequency = frequency;
    }
    
    // Starts listening to the audio clip, but does not capture the audio data in recordingAudioData.
    // It can be useful to separate listening and recording so that you can easily pause and continue recording, or start/stop recording based on the volume of the input.
    // microphoneAudioClipLengthSeconds must be longer than an update loop; if you wish to use the data it contains then you may want to control its length.
    // Avoids allocating a large audioclip so that recordings only need the memory they actually use.
    public bool StartListening(int microphoneAudioClipLengthSeconds = 1) {
        if (listening) {
            Debug.LogWarning("MicrophoneRecordingHandler.StartListening: Already listening to the microphone! Will stop and restart.");
            StopListening();
        }
        try {
            liveMicrophoneOutputAudioClip = Microphone.Start(deviceName, true, microphoneAudioClipLengthSeconds, frequency);
        } catch (Exception e) {
            Debug.LogError($"MicrophoneRecordingHandler.StartListening: {e}\n{deviceName}\n{liveMicrophoneOutputAudioClip}");
            return false;
        }
        channels = liveMicrophoneOutputAudioClip.channels;
        lastMicrophoneSamplePosition = 0;
        return true;
    }
    
    // Stop listening. This will destroy the liveMicrophoneOutputAudioClip.
    public void StopListening() {
        if (!listening) return;
        Microphone.End(deviceName);
        Object.Destroy(liveMicrophoneOutputAudioClip);
        lastMicrophoneSamplePosition = 0;
    }

    // Start or resume a recording
    public void StartRecording(float timeToAddFromListeningClip = 0) {
        isRecording = true;
        
        recordingAudioData ??= new List<float>();
        
        if(!listening && !StartListening()) {
            Debug.LogError("Failed to start listening to the microphone!");
        }
        
        OnStartRecording?.Invoke();
    }

    // Stops capturing input into the audio data array.
    // Can be used when you're done and want to create the AudioClip (CreateAudioClip())
    // Or when you want to pause the recording to be resumed later using StartRecording()
    public void StopRecording(bool alsoStopListening) {
        if (!isRecording) return;
        // Capture the last bit of audio data that was recorded between the last Update and now.
        WriteToAudioData();
        isRecording = false;
        if(alsoStopListening) StopListening();
        
        OnStopRecording?.Invoke();
    }

    // Stops, clears and starts recording.
    public void RestartRecording() {
        StopRecording(false);
        ClearRecording();
        StartRecording();
    }
    
    // You can call this to reset the recording or to clear up memory if you don't need the recording anymore (pending the garbage collector) 
    public void ClearRecording() {
        if (listening) {
            Debug.LogError("You should not clear the output buffer while recording. Call StopRecording first!");
            return;
        }

        recordingAudioData = null;
        OnClearRecording?.Invoke();
    }
    
    // Creates a new AudioClip from the recordingAudioData.
    public AudioClip CreateAudioClip(string name) {
        var audioClip = AudioClip.Create(name, recordingAudioData.Count, channels, frequency, false);
        audioClip.SetData(recordingAudioData.ToArray(), 0);
        return audioClip;
    }
    
    // Update needs to be called at least once per microphoneAudioClipLengthSeconds in order to record properly. 
    public void UpdateRecording() {
        // This can occur at the max length of the recording, and if there's an error.
        if(isRecording && !listening) {
            Debug.LogWarning("An error occurred while recording! Device name is "+deviceName);
            StopRecording(true);
        }
        
        if (isRecording) {
            WriteToAudioData();
        }
    }
    
    
    
    // Fills the samples array with most recent audio from the microphone, using liveMicrophoneOutputAudioClip.
    // Unlike liveMicrophoneOutputAudioClip, The samples will start from the most recent time.
    public void GetRecentMicrophoneData(float[] samples, int offset = 0) {
        if(liveMicrophoneOutputAudioClip == null) return;
        if (samples.Length > liveMicrophoneOutputAudioClip.samples) Debug.LogWarning("GetRecentMicrophoneData is trying to get more samples than the microphone is set to record before looping!");
        if(samples.Length == 0) return;
        int startPosition = Microphone.GetPosition(deviceName) - samples.Length + offset;
        // Ensure that start position is >= 0 and < the sample length of the audioclip
        startPosition %= liveMicrophoneOutputAudioClip.samples;
        if (startPosition < 0) startPosition += liveMicrophoneOutputAudioClip.samples;
        // Write the data to the samples array
        liveMicrophoneOutputAudioClip.GetData(samples, startPosition);
    }

    // Get the number of samples that have been recorded since the last time this method was called.
    // Designed to be called frequently, such as in an update loop - is only accurate if called at least once per microphoneAudioClipLengthSeconds.
    public int CalculateDeltaSampleCount(ref int lastMicrophoneSamplePosition) {
        if(!listening) return 0;
        int currentPos = Microphone.GetPosition(deviceName);
        int length = currentPos - lastMicrophoneSamplePosition;
        if (length < 0) length += liveMicrophoneOutputAudioClip.samples;
        lastMicrophoneSamplePosition = currentPos;
        return length;
    }

    // Writes the most recent audio data from the microphone to recordingAudioData.
    // Called each frame while capturing.
    void WriteToAudioData() {
        float[] samples = new float[CalculateDeltaSampleCount(ref lastMicrophoneSamplePosition)];
        GetRecentMicrophoneData(samples);
        recordingAudioData.AddRange(samples);
        lastMicrophoneSamplePosition = Microphone.GetPosition(deviceName);
    }
    
    
    // Ensures the frequency is within a valid range for the device
    public static int GetValidFrequencyForDevice (string recordingDeviceName, int frequency = defaultRecordingFrequency) {
        Microphone.GetDeviceCaps(recordingDeviceName, out int minFreq, out int maxFreq);
        if(minFreq == 0 && maxFreq == 0) return frequency;
        else return Mathf.Clamp(frequency, minFreq, maxFreq);
    }
}