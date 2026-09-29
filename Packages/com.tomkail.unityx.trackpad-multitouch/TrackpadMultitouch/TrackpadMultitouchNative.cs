using System.Runtime.InteropServices;

/// <summary>
/// Raw P/Invoke bindings to the TrackpadMultitouch.bundle native plugin (macOS only).
/// The bundle wraps the private MultitouchSupport.framework; see Source/TrackpadMultitouch.m.
///
/// The native ABI is FROZEN — Unity never unloads a native plugin, so changing the native
/// side means restarting the Editor. Keep new logic in <see cref="TrackpadTouchProvider"/>.
/// </summary>
public static class TrackpadMultitouchNative {
    /// <summary>Flat per-contact struct. Field order/types must mirror TPTouch in the .m exactly.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TPTouch {
        public int deviceIndex;   // 0-based enumerated device
        public int pathIndex;     // stable per-contact id (per device)
        public int state;         // raw MTPathStage (see TrackpadTouchState)
        public float posX, posY;  // normalized 0..1, origin bottom-left (matches Unity screen space)
        public float velX, velY;
        public float size;        // contact-area / pressure proxy
        public float z2;          // secondary z metric
        public float angle;       // ellipse orientation, radians
        public float majorAxis, minorAxis;
        public float absX, absY;  // mm on the sensor
        public float zDensity;
        public double timestamp;
    }

    // MTPathStage values observed from the framework.
    public enum State {
        NotTracking = 0, StartInRange = 1, HoverInRange = 2, MakeTouch = 3,
        Touching = 4, BreakTouch = 5, LingerInRange = 6, OutOfRange = 7,
    }

#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
    const string DLL = "TrackpadMultitouch";

    /// <summary>Ref-counted start. Returns device count (>=0) or negative on failure
    /// (-1 dlopen, -2 missing symbols, -3 no devices).</summary>
    [DllImport(DLL)] public static extern int TP_Start();
    /// <summary>Ref-counted stop; only stops once the last caller releases.</summary>
    [DllImport(DLL)] public static extern void TP_Stop();
    /// <summary>Re-enumerate to pick up hotplugged/removed trackpads. Returns current device count.</summary>
    [DllImport(DLL)] public static extern int TP_Refresh();
    [DllImport(DLL)] public static extern int TP_IsRunning();
    [DllImport(DLL)] public static extern int TP_GetDeviceCount();
    /// <summary>Copies up to maxCount contacts into out; returns number written.</summary>
    [DllImport(DLL)] public static extern int TP_PollTouches([Out] TPTouch[] outTouches, int maxCount);
    [DllImport(DLL)] public static extern int TP_IsBuiltIn(int deviceIndex);
    /// <summary>Physical sensor surface dimensions (arbitrary units; only the ratio is meaningful).
    /// Returns 1 on success.</summary>
    [DllImport(DLL)] public static extern int TP_GetSensorDimensions(int deviceIndex, out int w, out int h);
#else
    // Non-macOS stubs so the rest of the project compiles everywhere.
    public static int TP_Start() => -1;
    public static void TP_Stop() { }
    public static int TP_Refresh() => 0;
    public static int TP_IsRunning() => 0;
    public static int TP_GetDeviceCount() => 0;
    public static int TP_PollTouches(TPTouch[] outTouches, int maxCount) => 0;
    public static int TP_IsBuiltIn(int deviceIndex) => 0;
    public static int TP_GetSensorDimensions(int deviceIndex, out int w, out int h) { w = 0; h = 0; return 0; }
#endif
}
