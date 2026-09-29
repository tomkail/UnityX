// TrackpadMultitouch — native macOS bundle exposing raw trackpad contacts to Unity.
//
// Wraps the private MultitouchSupport.framework (loaded via dlopen, never link-time bound).
// The framework delivers contact frames on its own thread; we double-buffer under a mutex
// and Unity polls the merged snapshot from the main thread via TP_PollTouches.
//
// ABI is intentionally FROZEN and minimal: Unity never unloads a native plugin, so every
// change here costs an Editor restart. Keep all logic that can live in C# in C#.
//
// Struct layout + callback signature verified empirically on macOS 26 (Tahoe) / Apple Silicon:
//   sizeof(Finger)==96, classic non-refcon callback, MTPathStage states 3/4 = contact.
//
// Build: see build.sh (clang -bundle, arch arm64, ad-hoc signed, quarantine stripped).

#import <CoreFoundation/CoreFoundation.h>
#include <dlfcn.h>
#include <pthread.h>
#include <string.h>

// ---- Private framework types (verified layout) ---------------------------------------

typedef struct { float x, y; } MTPoint;
typedef struct { MTPoint pos, vel; } MTVector;

typedef struct {
    int32_t  frame;
    double   timestamp;
    int32_t  pathIndex;
    int32_t  state;        // MTPathStage: 3=MakeTouch, 4=Touching, 5=Break, 6=Linger, 7=OutOfRange
    int32_t  fingerID;
    int32_t  handID;
    MTVector normalized;   // pos + vel, 0..1
    float    z1;           // "size" — contact area / pressure proxy
    float    z2;           // secondary z metric (non-zero in practice; kept, not discarded)
    float    angle;        // ellipse orientation (radians)
    float    majorAxis;
    float    minorAxis;
    MTVector absolute;     // mm
    int32_t  pad[2];       // observed zero
    float    zDensity;
} Finger;

typedef void* MTDeviceRef;
typedef CFArrayRef  (*MTDeviceCreateList_f)(void);
typedef int         (*MTContactCallback_f)(MTDeviceRef device, Finger *data, int n, double ts, int frame);
typedef void        (*MTRegister_f)(MTDeviceRef, MTContactCallback_f);
typedef void        (*MTUnregister_f)(MTDeviceRef, MTContactCallback_f);
typedef void        (*MTStart_f)(MTDeviceRef, int);
typedef void        (*MTStop_f)(MTDeviceRef);
typedef int         (*MTIsBuiltIn_f)(MTDeviceRef);
typedef int         (*MTGetSurfaceDims_f)(MTDeviceRef, int32_t*, int32_t*); // sensor surface, hundredths of mm

// ---- Flat struct handed to Unity (stable ABI — must mirror the C# [StructLayout]) -----

typedef struct {
    int32_t deviceIndex;   // 0-based index into the enumerated device list
    int32_t pathIndex;     // stable per-contact id (per device — Unity composes a global id)
    int32_t state;         // raw MTPathStage
    float   posX, posY;    // normalized 0..1, origin bottom-left
    float   velX, velY;
    float   size;          // z1
    float   z2;            // secondary z
    float   angle;
    float   majorAxis, minorAxis;
    float   absX, absY;    // mm
    float   zDensity;
    double  timestamp;
} TPTouch;

// ---- State ----------------------------------------------------------------------------

#define MAX_DEVICES 8
#define MAX_TOUCHES_PER_DEVICE 16

typedef struct {
    MTDeviceRef ref;
    TPTouch     touches[MAX_TOUCHES_PER_DEVICE];
    int         count;
} DeviceSlot;

static void *g_lib = NULL;
static MTDeviceCreateList_f p_createList = NULL;
static MTRegister_f         p_register   = NULL;
static MTUnregister_f       p_unregister = NULL;
static MTStart_f            p_start       = NULL;
static MTStop_f             p_stop        = NULL;
static MTIsBuiltIn_f        p_isBuiltIn   = NULL;
static MTGetSurfaceDims_f   p_surfaceDims = NULL;

static DeviceSlot g_devices[MAX_DEVICES];
static int g_deviceCount = 0;
static int g_started = 0;
static int g_refcount = 0; // how many providers have called TP_Start without a matching TP_Stop
static pthread_mutex_t g_mutex = PTHREAD_MUTEX_INITIALIZER;

// ---- Callback (framework thread) ------------------------------------------------------

static int contactCallback(MTDeviceRef device, Finger *data, int n, double ts, int frame) {
    (void)ts; (void)frame;
    pthread_mutex_lock(&g_mutex);
    for (int d = 0; d < g_deviceCount; d++) {
        if (g_devices[d].ref != device) continue;
        int c = n; if (c > MAX_TOUCHES_PER_DEVICE) c = MAX_TOUCHES_PER_DEVICE;
        for (int i = 0; i < c; i++) {
            Finger *f = &data[i];
            TPTouch *t = &g_devices[d].touches[i];
            t->deviceIndex = d;
            t->pathIndex   = f->pathIndex;
            t->state       = f->state;
            t->posX = f->normalized.pos.x; t->posY = f->normalized.pos.y;
            t->velX = f->normalized.vel.x; t->velY = f->normalized.vel.y;
            t->size = f->z1; t->z2 = f->z2;
            t->angle = f->angle;
            t->majorAxis = f->majorAxis; t->minorAxis = f->minorAxis;
            t->absX = f->absolute.pos.x; t->absY = f->absolute.pos.y;
            t->zDensity = f->zDensity;
            t->timestamp = f->timestamp;
        }
        g_devices[d].count = c;
        break;
    }
    pthread_mutex_unlock(&g_mutex);
    return 0;
}

// ---- internal helpers -----------------------------------------------------------------

// Loads the framework and resolves symbols once. Returns 0 ok, -1 dlopen failed, -2 missing symbols.
static int ensureLoaded(void) {
    if (!g_lib) {
        g_lib = dlopen("/System/Library/PrivateFrameworks/MultitouchSupport.framework/MultitouchSupport", RTLD_NOW);
        if (!g_lib) return -1;
        p_createList = (MTDeviceCreateList_f)dlsym(g_lib, "MTDeviceCreateList");
        p_register   = (MTRegister_f)dlsym(g_lib, "MTRegisterContactFrameCallback");
        p_unregister = (MTUnregister_f)dlsym(g_lib, "MTUnregisterContactFrameCallback");
        p_start      = (MTStart_f)dlsym(g_lib, "MTDeviceStart");
        p_stop       = (MTStop_f)dlsym(g_lib, "MTDeviceStop");
        p_isBuiltIn  = (MTIsBuiltIn_f)dlsym(g_lib, "MTDeviceIsBuiltIn");
        p_surfaceDims = (MTGetSurfaceDims_f)dlsym(g_lib, "MTDeviceGetSensorSurfaceDimensions");
    }
    return (p_createList && p_register && p_start) ? 0 : -2;
}

// Enumerate devices, register the callback, and start them. Assumes symbols are loaded.
static void startDevices(void) {
    CFArrayRef devices = p_createList();
    if (!devices) { g_deviceCount = 0; return; }
    CFIndex count = CFArrayGetCount(devices);
    if (count > MAX_DEVICES) count = MAX_DEVICES;

    pthread_mutex_lock(&g_mutex);
    g_deviceCount = (int)count;
    for (CFIndex i = 0; i < count; i++) {
        g_devices[i].ref = (MTDeviceRef)CFArrayGetValueAtIndex(devices, i);
        g_devices[i].count = 0;
    }
    pthread_mutex_unlock(&g_mutex);

    for (int i = 0; i < g_deviceCount; i++) {
        p_register(g_devices[i].ref, contactCallback);
        p_start(g_devices[i].ref, 0);
    }
    CFRelease(devices);
}

static void stopDevices(void) {
    for (int i = 0; i < g_deviceCount; i++) {
        if (p_stop) p_stop(g_devices[i].ref);
        if (p_unregister) p_unregister(g_devices[i].ref, contactCallback);
    }
    pthread_mutex_lock(&g_mutex);
    for (int i = 0; i < g_deviceCount; i++) g_devices[i].count = 0;
    pthread_mutex_unlock(&g_mutex);
}

// ---- Exported C ABI -------------------------------------------------------------------

// Ref-counted: safe to call from multiple providers. Returns >=0 device count on success,
// or negative on failure: -1 dlopen failed, -2 missing symbols, -3 no devices.
int TP_Start(void) {
    int rc = ensureLoaded();
    if (rc < 0) return rc;

    if (!g_started) {
        startDevices();
        if (g_deviceCount == 0) return -3;
        g_started = 1;
    }
    g_refcount++;
    return g_deviceCount;
}

// Ref-counted: only actually stops once the last caller has released.
void TP_Stop(void) {
    if (g_refcount > 0) g_refcount--;
    if (g_refcount > 0) return;
    if (!g_started) return;
    stopDevices();
    g_started = 0;
}

// Re-enumerate to pick up hotplugged/removed trackpads. Cheap to poll; only re-registers when the
// device count actually changes. Returns the current device count.
int TP_Refresh(void) {
    if (!g_started || !p_createList) return g_deviceCount;
    CFArrayRef devices = p_createList();
    if (!devices) return g_deviceCount;
    int newCount = (int)CFArrayGetCount(devices);
    CFRelease(devices);
    if (newCount != g_deviceCount) {
        stopDevices();
        startDevices();
    }
    return g_deviceCount;
}

int TP_IsRunning(void) { return g_started; }
int TP_GetDeviceCount(void) { return g_deviceCount; }

// Copies the current merged snapshot into out (up to maxCount). Returns number written.
int TP_PollTouches(TPTouch *out, int maxCount) {
    int written = 0;
    pthread_mutex_lock(&g_mutex);
    for (int d = 0; d < g_deviceCount && written < maxCount; d++) {
        int c = g_devices[d].count;
        for (int i = 0; i < c && written < maxCount; i++) {
            out[written++] = g_devices[d].touches[i];
        }
    }
    pthread_mutex_unlock(&g_mutex);
    return written;
}

int TP_IsBuiltIn(int deviceIndex) {
    if (deviceIndex < 0 || deviceIndex >= g_deviceCount || !p_isBuiltIn) return 0;
    return p_isBuiltIn(g_devices[deviceIndex].ref) ? 1 : 0;
}

// Physical sensor surface dimensions (arbitrary units — only the ratio is meaningful).
// Returns 1 and fills outW/outH on success, 0 otherwise.
int TP_GetSensorDimensions(int deviceIndex, int *outW, int *outH) {
    if (outW) *outW = 0;
    if (outH) *outH = 0;
    if (deviceIndex < 0 || deviceIndex >= g_deviceCount || !p_surfaceDims) return 0;
    int32_t w = 0, h = 0;
    p_surfaceDims(g_devices[deviceIndex].ref, &w, &h);
    if (outW) *outW = w;
    if (outH) *outH = h;
    return (w > 0 && h > 0) ? 1 : 0;
}
