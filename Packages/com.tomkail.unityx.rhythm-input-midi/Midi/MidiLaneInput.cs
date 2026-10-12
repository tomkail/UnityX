using System.Collections.Generic;
using Minis;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityX.Rhythm {
	// MIDI notes as lane presses, through Minis. Picks up devices already connected when enabled and any plugged in
	// later, and lets go of them when they're removed or this is disabled.
	// Timing: Minis delivers MIDI once a frame, early in the frame, without timestamps, so each note is timed at the
	// moment it's delivered. That's up to one frame after it was played (about 16ms at 60fps). Input latency in
	// RhythmLatency can take out the average delay; the frame-to-frame spread remains.
	public class MidiLaneInput : RhythmInputSource {
		public MidiLaneMap map;
		[Tooltip("Report note-offs as releases, for hold notes. Drum kits often send note-offs that mean nothing.")]
		public bool sendReleases = true;

		readonly HashSet<MidiDevice> devices = new();

		void OnEnable() {
			foreach (var device in InputSystem.devices) {
				if (device is MidiDevice midiDevice) Add(midiDevice);
			}
			InputSystem.onDeviceChange += OnDeviceChange;
		}

		void OnDisable() {
			InputSystem.onDeviceChange -= OnDeviceChange;
			foreach (var device in devices) {
				device.onWillNoteOn -= OnNoteOn;
				device.onWillNoteOff -= OnNoteOff;
			}
			devices.Clear();
		}

		// The mapping and timing, separate from Minis so it can be tested. realtime is on the
		// Time.realtimeSinceStartupAsDouble timeline.
		public void HandleNote(int note, int channel, float velocity, bool on, double realtime) {
			if (map == null || !map.TryGetLane(note, channel, out var lane)) return;
			// A note-on with no velocity means note-off in MIDI
			if (on && velocity > 0) Submit(lane, InputPhase.Press, velocity, realtime);
			else if (sendReleases) Submit(lane, InputPhase.Release, 0, realtime);
		}

		void OnDeviceChange(InputDevice device, InputDeviceChange change) {
			if (device is not MidiDevice midiDevice) return;
			if (change is InputDeviceChange.Added or InputDeviceChange.Reconnected) Add(midiDevice);
			else if (change is InputDeviceChange.Removed or InputDeviceChange.Disconnected) Remove(midiDevice);
		}

		void Add(MidiDevice device) {
			if (!devices.Add(device)) return;
			device.onWillNoteOn += OnNoteOn;
			device.onWillNoteOff += OnNoteOff;
		}

		void Remove(MidiDevice device) {
			if (!devices.Remove(device)) return;
			device.onWillNoteOn -= OnNoteOn;
			device.onWillNoteOff -= OnNoteOff;
		}

		void OnNoteOn(MidiNoteControl note, float velocity) => HandleNote(note.noteNumber, ChannelOf(note), velocity, true, Time.realtimeSinceStartupAsDouble);
		void OnNoteOff(MidiNoteControl note) => HandleNote(note.noteNumber, ChannelOf(note), 0, false, Time.realtimeSinceStartupAsDouble);

		static int ChannelOf(MidiNoteControl note) => note.device is MidiDevice device ? device.channel : 0;
	}
}
