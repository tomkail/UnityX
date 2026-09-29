using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using ISTouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// Polls the TrackpadMultitouch native plugin each frame, maps trackpad contacts into screen
/// space, and feeds them to a synthetic <see cref="Touchscreen"/> device. Because downstream code
/// reads EnhancedTouch, InputPointManager / MultitouchDraggable light up with no changes.
///
/// Also publishes the full per-contact data (velocity, ellipse, pressure, mm) via <see cref="touches"/>
/// and the <see cref="onTouchBegan"/>/<see cref="onTouchMoved"/>/<see cref="onTouchEnded"/> events,
/// for consumers that want more than the Touchscreen carries.
///
/// macOS only; on other platforms the native calls are stubbed and this component is inert.
/// </summary>
[AddComponentMenu("UnityX/Input/Trackpad Touch Provider")]
public class TrackpadTouchProvider : MonoBehaviour {
    const int MaxTouches = 32;

    [Header("Touch Feed")]
    [Tooltip("Feed a synthetic Touchscreen so EnhancedTouch (and InputPointManager) pick the contacts up.")]
    public bool feedTouchscreen = true;
    [Tooltip("Native 'size' value that maps to full (1.0) Touchscreen pressure.")]
    public float pressureScale = 2f;

    [Header("Coordinate Mapping")]
    [Tooltip("Map the trackpad onto the entire game view (its current pixel size — not the physical " +
             "display; rescales if the window resizes). Uncheck to confine touches to targetRect instead.")]
    public bool mapToFullScreen = true;
    [Tooltip("Screen rect (pixels) the trackpad's 0..1 surface maps onto when not mapping to full screen.")]
    public Rect targetRect = new Rect(0, 0, 1920, 1080);

    public enum AspectMode {
        Stretch, // fill the target rect exactly (distorts if aspects differ)
        Fit,     // preserve trackpad aspect, letterbox inside the target (some screen unreachable)
        Fill,    // preserve trackpad aspect, cover the target (trackpad edges map off-screen)
    }
    [Tooltip("How the trackpad surface maps onto the target rect when their aspect ratios differ. " +
             "Stretch distorts; Fit/Fill preserve trackpad proportions using inputAspect.")]
    public AspectMode aspectMode = AspectMode.Stretch;
    [Tooltip("Fill inputAspect from the trackpad's real sensor dimensions when it starts (recommended).")]
    public bool autoDetectInputAspect = true;
    [Tooltip("Trackpad surface aspect ratio (width / height). Auto-filled when autoDetectInputAspect is on " +
             "(built-in Force Touch trackpad measures ~1.61). Only used when aspectMode != Stretch.")]
    public float inputAspect = 1.6f;
    [Tooltip("Mirror the X axis (in case a device reports a flipped surface).")]
    public bool flipX = false;
    [Tooltip("Mirror the Y axis.")]
    public bool flipY = false;

    [Header("Cursor")]
    [Tooltip("While the trackpad is running (touchpad mode), lock+hide the OS cursor so mouse movement " +
             "doesn't drift or fight the trackpad touches. Restored when it stops.")]
    public bool lockCursorWhileRunning = true;

    public enum DeviceFilter { All, BuiltInOnly, ExternalOnly }
    [Header("Devices")]
    [Tooltip("Which trackpads feed touches. Use BuiltInOnly / ExternalOnly to ignore stray contacts from " +
             "the other pad (e.g. hands resting on the laptop while using a Magic Trackpad).")]
    public DeviceFilter deviceFilter = DeviceFilter.All;
    [Tooltip("Seconds between checks for a hotplugged/removed trackpad (e.g. connecting a Magic Trackpad " +
             "after Play starts). 0 disables polling.")]
    public float deviceRefreshInterval = 1.5f;

    [Header("Debug")]
    public bool drawDebugGUI = false;

    // ---- events (fired from Update, main thread) --------------------------------------
    public event System.Action<TrackpadTouch> OnTouchBegan;
    public event System.Action<TrackpadTouch> OnTouchMoved;
    public event System.Action<TrackpadTouch> OnTouchEnded;

    /// <summary>Most-recently-enabled provider, for convenient access. Null when none is running.</summary>
    public static TrackpadTouchProvider active { get; private set; }

    /// <summary>One live trackpad contact with everything the sensor reports.</summary>
    public struct TrackpadTouch {
        public int globalId;         // stable, unique across both trackpads
        public int deviceIndex;
        public int rawPathIndex;
        public int touchId;          // synthetic Touchscreen id (0 if not feeding)
        public TrackpadMultitouchNative.State state;
        public ISTouchPhase phase;   // derived Unity phase
        public Vector2 screenPosition;
        public Vector2 normalizedPosition;
        public Vector2 velocity;
        public Vector2 mmPosition;
        public float size, z2, zDensity;
        public float angle, majorAxis, minorAxis;
    }

    /// <summary>Static info about one enumerated trackpad, resolved on start.</summary>
    public struct DeviceInfo {
        public int index;
        public bool builtIn;
        public int sensorWidth, sensorHeight; // arbitrary units; only the ratio is meaningful
        public float aspect;
    }

    readonly List<TrackpadTouch> _touches = new List<TrackpadTouch>();
    /// <summary>Live contacts this frame (contact phases only).</summary>
    public IReadOnlyList<TrackpadTouch> touches => _touches;

    readonly List<DeviceInfo> _devices = new List<DeviceInfo>();
    public IReadOnlyList<DeviceInfo> devices => _devices;

    public int deviceCount { get; private set; }
    public bool running { get; private set; }
    public bool captured => _captured;
    public string status { get; private set; } = "not started";

    TrackpadMultitouchNative.TPTouch[] _buffer = new TrackpadMultitouchNative.TPTouch[MaxTouches];
    Touchscreen _touchscreen;
    // globalId -> synthetic Touchscreen touchId (nonzero; only populated when feeding the Touchscreen)
    readonly Dictionary<int, int> _touchIds = new Dictionary<int, int>();
    // globalId -> last seen contact; the source of truth for liveness (independent of the Touchscreen feed).
    readonly Dictionary<int, TrackpadTouch> _lastById = new Dictionary<int, TrackpadTouch>();
    int _nextTouchId = 1;
    bool _appFocused = true;
    float _refreshTimer;
    // We own the "is the game capturing input" state rather than reading Cursor.lockState, because the
    // editor's Esc-unlock doesn't reliably update lockState (it can still read Locked while visually free).
    bool _captured;

    static int GlobalId(TrackpadMultitouchNative.TPTouch t) => (t.deviceIndex << 16) | (t.pathIndex & 0xFFFF);

    static bool IsContact(int state) =>
        state == (int)TrackpadMultitouchNative.State.MakeTouch ||
        state == (int)TrackpadMultitouchNative.State.Touching;

    bool PassesDeviceFilter(int deviceIndex) {
        if (deviceFilter == DeviceFilter.All) return true;
        bool builtIn = deviceIndex >= 0 && deviceIndex < _devices.Count && _devices[deviceIndex].builtIn;
        return deviceFilter == DeviceFilter.BuiltInOnly ? builtIn : !builtIn;
    }

    void OnEnable() {
        int rc = TrackpadMultitouchNative.TP_Start();
        deviceCount = TrackpadMultitouchNative.TP_GetDeviceCount();
        running = rc >= 0;
        status = rc >= 0 ? $"running ({deviceCount} device(s))"
               : rc == -1 ? "framework dlopen failed"
               : rc == -2 ? "missing framework symbols"
               : "no multitouch devices";

        ResolveDevices();
        if (active != null && active != this)
            Debug.LogWarning("[TrackpadTouchProvider] Another provider is already active; multiple providers feed the " +
                             "same Touchscreen and will conflict. Use a single provider.", this);
        active = this;

        if (feedTouchscreen && running) {
            _touchscreen = InputSystem.GetDevice<Touchscreen>() as Touchscreen;
            if (_touchscreen == null || _touchscreen.name != "TrackpadMultitouch")
                _touchscreen = InputSystem.AddDevice<Touchscreen>("TrackpadMultitouch");
        }

        // In touchpad mode the trackpad drives touches, so capture (lock+hide) the OS cursor on start.
        if (lockCursorWhileRunning && running) SetCaptured(true);
    }

    void OnDisable() {
        EndAllTouches();
        TrackpadMultitouchNative.TP_Stop();
        running = false;
        status = "stopped";
        if (_touchscreen != null) { InputSystem.RemoveDevice(_touchscreen); _touchscreen = null; }
        if (active == this) active = null;

        if (_captured) SetCaptured(false);
    }

    // The framework delivers frames regardless of app focus, so drop touches when we lose focus —
    // otherwise using the trackpad in another app would leak touches into Unity.
    void OnApplicationFocus(bool focused) {
        _appFocused = focused;
        if (!focused) EndAllTouches();
    }

    void ResolveDevices() {
        _devices.Clear();
        for (int i = 0; i < deviceCount; i++) {
            int w = 0, h = 0;
            bool builtIn = false;
            // Guard against a stale bundle missing newer symbols (native plugins don't hot-reload,
            // so a rebuild that added TP_GetSensorDimensions isn't live until the Editor restarts).
            try {
                TrackpadMultitouchNative.TP_GetSensorDimensions(i, out w, out h);
                builtIn = TrackpadMultitouchNative.TP_IsBuiltIn(i) != 0;
            } catch (System.EntryPointNotFoundException) {
                status += " (stale bundle — restart Editor)";
            }
            _devices.Add(new DeviceInfo {
                index = i, builtIn = builtIn,
                sensorWidth = w, sensorHeight = h,
                aspect = h > 0 ? (float)w / h : 0f,
            });
        }
        if (autoDetectInputAspect && _devices.Count > 0 && _devices[0].aspect > 0f)
            inputAspect = _devices[0].aspect;
    }

    // Capture = game owns the cursor (locked+hidden) and trackpad touches feed the game.
    // Released = cursor free + touches ignored, so you can use the Editor UI.
    void SetCaptured (bool captured) {
        _captured = captured;
        Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !captured;
        // In touchpad mode the trackpad is the only intended pointer. Disable the Mouse/Pen devices while
        // captured: with pointerBehavior SingleMouseOrPenButMultiTouchAndTrack a *locked* mouse still feeds the
        // UI module a phantom pointer (position (-1,-1)) that contends with the synthetic touches and steals
        // per-finger drag/end delivery — leaving stuck "phantom finger" trackers in UI consumers. Re-enabled on
        // release so the click-to-recapture (and normal mouse use) works again.
        SetDeviceEnabled(Mouse.current, !captured);
        SetDeviceEnabled(Pen.current, !captured);
    }

    static void SetDeviceEnabled (InputDevice device, bool enabled) {
        if (device == null) return;
        if (enabled && !device.enabled) InputSystem.EnableDevice(device);
        else if (!enabled && device.enabled) InputSystem.DisableDevice(device);
    }

    void Update() {
        if (!running || !_appFocused) return;

        PollDeviceChanges();

        // Touches only affect the game while captured. Esc releases (so you can use the Editor UI without
        // trackpad touches leaking into the game); click back into the view to re-capture. We read the
        // Input System devices directly (they see the key/click regardless of the editor's own Esc-unlock).
        if (lockCursorWhileRunning) {
            if (_captured) {
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                    SetCaptured(false);
            } else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
                SetCaptured(true);
            }
            if (!_captured) { EndAllTouches(); return; }
        }

        int n = TrackpadMultitouchNative.TP_PollTouches(_buffer, MaxTouches);
        _touches.Clear();

        // Track which globalIds are contacting this frame so we can end the rest.
        var seen = HashSetPool();
        for (int i = 0; i < n; i++) {
            var raw = _buffer[i];
            if (!IsContact(raw.state)) continue;
            if (!PassesDeviceFilter(raw.deviceIndex)) continue;
            int gid = GlobalId(raw);
            seen.Add(gid);

            bool isNew = !_lastById.ContainsKey(gid);
            var phase = isNew ? ISTouchPhase.Began : ISTouchPhase.Moved;
            Vector2 screenPos = ToScreen(raw.posX, raw.posY);

            int touchId = 0;
            if (feedTouchscreen && _touchscreen != null)
                touchId = QueueTouch(gid, screenPos, raw);

            var t = new TrackpadTouch {
                globalId = gid, deviceIndex = raw.deviceIndex, rawPathIndex = raw.pathIndex, touchId = touchId,
                state = (TrackpadMultitouchNative.State)raw.state, phase = phase,
                screenPosition = screenPos, normalizedPosition = new Vector2(raw.posX, raw.posY),
                velocity = new Vector2(raw.velX, raw.velY), mmPosition = new Vector2(raw.absX, raw.absY),
                size = raw.size, z2 = raw.z2, zDensity = raw.zDensity,
                angle = raw.angle, majorAxis = raw.majorAxis, minorAxis = raw.minorAxis,
            };
            _touches.Add(t);
            _lastById[gid] = t;
            if (isNew) OnTouchBegan?.Invoke(t); else OnTouchMoved?.Invoke(t);
        }

        // End any previously-live touch that is no longer contacting.
        if (_lastById.Count > 0) {
            _endBuffer.Clear();
            foreach (var kv in _lastById) if (!seen.Contains(kv.Key)) _endBuffer.Add(kv.Key);
            foreach (var gid in _endBuffer) EndTouch(gid);
        }
        // (No primary-touch bookkeeping: every contact is fed as non-primary — see QueueTouch.)
        ReleaseHashSet(seen);
    }

    void PollDeviceChanges() {
        if (deviceRefreshInterval <= 0f) return;
        _refreshTimer += Time.unscaledDeltaTime;
        if (_refreshTimer < deviceRefreshInterval) return;
        _refreshTimer = 0f;
        try {
            int n = TrackpadMultitouchNative.TP_Refresh();
            if (n != deviceCount) { deviceCount = n; ResolveDevices(); }
        } catch (System.EntryPointNotFoundException) {
            deviceRefreshInterval = 0f; // stale bundle without TP_Refresh — stop polling until restart
        }
    }

    // Returns the assigned synthetic touchId.
    int QueueTouch(int gid, Vector2 screenPos, TrackpadMultitouchNative.TPTouch raw) {
        bool freshId = !_touchIds.TryGetValue(gid, out int id);
        if (freshId) { id = _nextTouchId++; if (_nextTouchId > (1 << 20)) _nextTouchId = 1; _touchIds[gid] = id; }
        InputSystem.QueueStateEvent(_touchscreen, new TouchState {
            touchId = id,
            position = screenPos,
            phase = freshId ? ISTouchPhase.Began : ISTouchPhase.Moved,
            pressure = Mathf.Clamp01(raw.size / Mathf.Max(0.0001f, pressureScale)),
            radius = new Vector2(raw.majorAxis, raw.minorAxis),
            // Deliberately never flag a primary touch. Unity's InputSystemUIInputModule folds the primary touch
            // into its shared "pointer" (which in touchpad mode is also the locked, frozen mouse), and once a
            // second touch exists the primary finger stops receiving per-finger OnDrag/OnEndDrag — leaving a
            // stuck tracker in UI consumers. Presenting every contact as non-primary makes them all behave as
            // independent, cleanly-tracked touch pointers.
            isPrimaryTouch = false,
        });
        return id;
    }

    void EndTouch(int gid) {
        if (_touchscreen != null && _touchIds.TryGetValue(gid, out int id))
            InputSystem.QueueStateEvent(_touchscreen, new TouchState { touchId = id, phase = ISTouchPhase.Ended, isPrimaryTouch = false });
        _touchIds.Remove(gid);

        if (_lastById.TryGetValue(gid, out var last)) {
            last.phase = ISTouchPhase.Ended;
            _lastById.Remove(gid);
            OnTouchEnded?.Invoke(last);
        }
    }

    void EndAllTouches() {
        if (_lastById.Count == 0 && _touchIds.Count == 0) { _touches.Clear(); return; }
        _endBuffer.Clear();
        foreach (var kv in _lastById) _endBuffer.Add(kv.Key);
        foreach (var kv in _touchIds) if (!_lastById.ContainsKey(kv.Key)) _endBuffer.Add(kv.Key);
        foreach (var gid in _endBuffer) EndTouch(gid);
        _touches.Clear();
    }

    Vector2 ToScreen(float nx, float ny) {
        // MT normalized origin is bottom-left, matching Unity screen space — no Y flip by default.
        if (flipX) nx = 1f - nx;
        if (flipY) ny = 1f - ny;
        Rect target = mapToFullScreen ? new Rect(0, 0, Screen.width, Screen.height) : targetRect;
        Rect r = AspectMapped(target);
        return new Vector2(r.x + nx * r.width, r.y + ny * r.height);
    }

    /// <summary>The sub-rect of 'target' the trackpad's 0..1 surface maps into, honouring aspectMode.</summary>
    public Rect AspectMapped(Rect target) => ComputeAspectRect(target, aspectMode, inputAspect);

    /// <summary>Pure aspect-fit math (no instance state), so it can be unit-tested directly.</summary>
    public static Rect ComputeAspectRect(Rect target, AspectMode mode, float inputAspect) {
        if (mode == AspectMode.Stretch || inputAspect <= 0f || target.height <= 0f)
            return target;

        float targetAspect = target.width / target.height;
        // Fit: input wider than target → width-bound (letterbox top/bottom); else height-bound.
        // Fill: swap the comparison so we cover instead of fit.
        bool widthBound = mode == AspectMode.Fit ? inputAspect > targetAspect
                                                 : inputAspect < targetAspect;
        if (widthBound) {
            float h = target.width / inputAspect;
            return new Rect(target.x, target.y + (target.height - h) * 0.5f, target.width, h);
        } else {
            float w = target.height * inputAspect;
            return new Rect(target.x + (target.width - w) * 0.5f, target.y, w, target.height);
        }
    }

    // --- tiny pooling so Update() doesn't allocate a HashSet every frame ---
    static readonly Stack<HashSet<int>> _hsPool = new Stack<HashSet<int>>();
    readonly List<int> _endBuffer = new List<int>();
    static HashSet<int> HashSetPool() { var hs = _hsPool.Count > 0 ? _hsPool.Pop() : new HashSet<int>(); hs.Clear(); return hs; }
    static void ReleaseHashSet(HashSet<int> hs) => _hsPool.Push(hs);

    void OnGUI() {
        if (!drawDebugGUI) return;
        GUI.Label(new Rect(10, 10, 600, 20),
            $"Trackpad: {status} | live: {_touches.Count} | aspect {inputAspect:0.00}" + (_captured ? "" : " | RELEASED (click to capture)"));

        // Gizmo: outline the region the trackpad actually maps into (after aspect fit/fill).
        Rect target = mapToFullScreen ? new Rect(0, 0, Screen.width, Screen.height) : targetRect;
        DrawScreenRectOutline(AspectMapped(target), new Color(0.3f, 0.8f, 1f, 0.7f));
        if (!mapToFullScreen)
            DrawScreenRectOutline(target, new Color(1f, 1f, 1f, 0.25f)); // the unclipped target too

        foreach (var t in _touches) {
            var p = new Vector2(t.screenPosition.x, Screen.height - t.screenPosition.y);
            var box = new Rect(p.x - 25, p.y - 25, 50, 50);
            GUI.Box(box, $"{t.touchId}\np{t.size:0.00}");
        }
    }

    // Draws a 1px outline of a screen-space rect (bottom-left origin) in GUI space (top-left origin).
    static void DrawScreenRectOutline(Rect screenRect, Color color) {
        float y = Screen.height - screenRect.yMax; // flip to GUI top-left
        var r = new Rect(screenRect.x, y, screenRect.width, screenRect.height);
        var prev = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, 1), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x, r.yMax - 1, r.width, 1), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x, r.y, 1, r.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMax - 1, r.y, 1, r.height), Texture2D.whiteTexture);
        GUI.color = prev;
    }
}
