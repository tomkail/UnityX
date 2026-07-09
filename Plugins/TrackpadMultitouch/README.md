# Trackpad Multitouch (macOS)

Turns a Mac trackpad (built-in **or** Magic Trackpad) into real multitouch input inside Unity —
positions, per-finger pressure/size, ellipse orientation, velocity, and physical mm coordinates.

It feeds a synthetic `Touchscreen` device, so anything that reads Unity's Input System
touch API (`EnhancedTouch`, and therefore this project's `InputPointManager` / pinch /
`MultitouchDraggable`) works with **no downstream changes**.

> macOS + Apple Silicon only. On other platforms the native calls are stubbed and the
> component is inert, so the project still compiles and runs everywhere.

---

## Setup

1. Add a **`Trackpad Touch Provider`** component to a GameObject in your scene
   (Add Component ▸ *UnityX/Input/Trackpad Touch Provider*).
2. Enter Play mode. Touch the trackpad — the custom inspector shows a live visualizer.
3. That's it. Touches now flow into `EnhancedTouch` / `InputPointManager`.

Requirements: **Active Input Handling** = *Input System Package* or *Both*
(Project Settings ▸ Player). No macOS permission prompt is required.

---

## How it works

```
MultitouchSupport.framework (private, dlopen'd)
   → TrackpadMultitouch.bundle   (native, polled on the main thread)
   → TrackpadTouchProvider        (maps to screen, derives phases)
   → synthetic Touchscreen        (InputSystem.QueueStateEvent)
   → EnhancedTouch → InputPointManager / pinch / MultitouchDraggable
```

- The native bundle reads raw contact frames on the framework's own thread and double-buffers
  them; C# polls a merged snapshot each frame (never touches the callback thread).
- Contacts from multiple trackpads are merged and given a unique `globalId`
  (`deviceIndex << 16 | pathIndex`) so ids never collide across devices.

---

## Using the data

**Via the Touchscreen (default, zero-config):** just read `EnhancedTouch.Touch.activeTouches`
(or use `InputPointManager`). Pressure maps to `TouchState.pressure`, contact size to `radius`.

**Via events** (richer per-contact data):

```csharp
var tp = TrackpadTouchProvider.active;          // or a serialized reference
tp.OnTouchBegan += t => Debug.Log($"began {t.touchId} @ {t.screenPosition}");
tp.OnTouchMoved += t => { /* t.velocity, t.angle, t.size, t.zDensity, t.mmPosition … */ };
tp.OnTouchEnded += t => { … };
```

**Via polling:** `tp.touches` is the list of live contacts this frame; `tp.devices` lists the
enumerated trackpads with their real sensor aspect ratio.

Each `TrackpadTouch` carries: `globalId`, `deviceIndex`, `touchId`, `phase`, `state`,
`screenPosition`, `normalizedPosition`, `velocity`, `mmPosition`, `size`, `z2`, `zDensity`,
`angle`, `majorAxis`, `minorAxis`.

---

## Settings reference

| Field | Purpose |
|---|---|
| **Feed Touchscreen** | Synthesize the `Touchscreen` device (leave on for `InputPointManager`). |
| **Pressure Scale** | Native `size` value mapped to full (1.0) `TouchState.pressure`. |
| **Map To Full Screen** / **Target Rect** | Which screen region the trackpad maps onto. |
| **Aspect Mode** | `Stretch` (fill, distorts), `Fit` (letterbox, undistorted), `Fill` (cover, undistorted). |
| **Auto Detect Input Aspect** | Fill `Input Aspect` from the trackpad's real sensor dimensions on start. |
| **Input Aspect** | Trackpad width/height ratio (built-in Force Touch measures ~1.61). |
| **Flip X / Flip Y** | Mirror an axis if a device reports a flipped surface. |
| **Lock Cursor While Running** | Capture the OS cursor so it doesn't drift/fight the touches. |
| **Device Filter** | `All`, `BuiltInOnly`, or `ExternalOnly` — ignore stray contacts from the other pad. |
| **Device Refresh Interval** | Seconds between hotplug checks (a Magic Trackpad connected after Play). 0 disables. |
| **Draw Debug GUI** | On-screen touch dots, status overlay, and an outline of the mapped region. |

### Aspect modes

The trackpad is ~1.61:1; your game view usually isn't. `Stretch` uses the whole surface and the
whole view but distorts angles/rotation. `Fit` and `Fill` preserve trackpad proportions (so a
circle stays a circle) — `Fit` letterboxes (some screen unreachable), `Fill` overscans (trackpad
edges map off-screen). Default is `Stretch`.

### Cursor capture

When **Lock Cursor While Running** is on, the game "captures" the cursor on start so trackpad
motion doesn't move the OS pointer. Press **Esc** to release (use the Editor UI), then **click**
back into the Game view to re-capture. Releasing also drops all live touches so they don't leak.

---

## Building / rebuilding the native bundle

Source is in `Source/`. To rebuild:

```sh
Source/build.sh        # clang -bundle, arm64, ad-hoc signed, quarantine stripped
```

or click **Rebuild Native Bundle** in the inspector.

> ⚠️ Unity **never unloads a native plugin** once loaded. After any rebuild you must
> **restart the Editor** for the new binary to take effect.

The `.bundle` import settings should be **macOS ▸ Editor + Standalone**. `build.sh` ad-hoc signs
the bundle and strips the `com.apple.quarantine` attribute so Gatekeeper won't refuse to load it
("bundle is damaged"). For distributed player builds, re-sign with your Developer ID team in an
Xcode post-build step (notarization does **not** reject private-API use — only App Store review does).

## Tests

Edit-mode tests for the aspect-fit math live in `Tests/` (assembly `UnityX.TrackpadMultitouch.Tests`).
Run them via **Window ▸ General ▸ Test Runner ▸ EditMode**.

The package is split into assemblies matching the UnityX convention: `UnityX.TrackpadMultitouch`
(runtime), `UnityX.TrackpadMultitouch.Editor`, and `UnityX.TrackpadMultitouch.Tests`.

---

## Limitations

- **macOS trackpad gestures** (Mission Control, swipe between spaces, zoom) are handled globally
  by the OS and **can't be suppressed per-app**. Disable the interfering ones in
  System Settings ▸ Trackpad while using this. Raw touches still arrive regardless.
- True Force-Touch *click* pressure isn't exposed (it's a separate `NSEvent` channel). `size` /
  `zDensity` are continuous contact signals and are almost always what you want.
- Built on the **private** `MultitouchSupport.framework` (loaded via `dlopen`, degrades gracefully
  if absent). Verified working on macOS 26 (Tahoe) / Apple Silicon.

---

## Troubleshooting

| Symptom | Fix |
|---|---|
| Inspector says "framework dlopen failed" | The private framework is missing/moved on this macOS. |
| "bundle is damaged and can't be opened" | Re-run `Source/build.sh` (re-signs + strips quarantine). |
| No touches in `InputPointManager` | Check Active Input Handling includes the Input System; ensure the provider is enabled and captured. |
| Touches feel distorted | Set Aspect Mode to `Fit` or `Fill`. |
| Ids climb forever | Expected — `touchId`/`globalId` are per-touch ids, not counts. Watch the live *count* instead. |
