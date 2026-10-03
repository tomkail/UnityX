# CoreCLR readiness audit

Audit of all 567 C# files against Unity's [Path to CoreCLR 2026 upgrade guide](https://discussions.unity.com/t/path-to-coreclr-2026-upgrade-guide/1714279). Unity 6.8 removes Mono and domain reload. Project is on 6000.5.1f1.

## Summary

**Clean:** no BinaryFormatter, ManagedDebugger, AppDomain, Assembly.Load/Location, Encoding.GetEncoding, Mono P/Invoke, Registry, WebClient, IntPtr→int conversions, or the explicit-interface/override resolution hazard. Bundled AWSSDK DLLs target netstandard2.0 (fine). Trackpad native interop marshalling is correct.

**The real work is domain-reload removal.** `ProjectSettings/EditorSettings.asset` has `m_EnterPlayModeOptionsEnabled: 0`, so the project has never run without domain reload and none of the static state below has been exercised that way.

Recommended order:
1. Fix the **High** items below with manual `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` resets (works on 6000.5 today).
2. Turn on Enter Play Mode Options with domain reload disabled, play twice, and fix what breaks.
3. On 6.8, swap trivial resets for `[AutoStaticsCleanup]`.

**Generic-class caveat:** `[RuntimeInitializeOnLoadMethod]` isn't invoked on methods of generic classes. `MonoSingleton<T>`, `ScriptableSingleton<T>`, `SerializedScriptableSingleton<T>`, `MonoInstancer<T>` and `SAnimatedProperty<T>` need a non-generic reset hub (e.g. a static session counter that the generic class checks), unless `[AutoStaticsCleanup]` is confirmed to support generics.

**Editor event caveat:** UnityEditor's static events (`EditorApplication.update`, `SceneView.duringSceneGui`, `Selection.selectionChanged`, `ObjectChangeEvents.changesPublished`, ...) outlive the script assembly. A `+=` with no unsubscribe keeps the old assembly alive and makes its handler fire alongside the new one after each recompile. `-=` before `+=` in the new assembly doesn't remove the old delegate. Use named methods and unsubscribe in `AssemblyReloadEvents.beforeAssemblyReload`.

---

## 1. Static state that leaks across play sessions — High

**Done:** every row below now has a reset. Non-generic classes use `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`. Generic classes compare against the new `PlaySession.id` (core; scene-management has an inlined `MonoSingletonSession`). Both are stand-ins for `[AutoStaticsCleanup]` until 6.8.

Each of these breaks on the second play session without domain reload.

| Location | Problem | Fix |
|---|---|---|
| core `UnityEngineX/ApplicationX.cs:5` | `isApplicationQuitting` is set in OnApplicationQuit (fires on exit play) and never reset, so every later session thinks it's quitting | SubsystemRegistration reset |
| core `UnityEngineX/MonoSingleton.cs:7-8` | `_Instance`/`searched` cached in edit mode → destroyed object returned in play, never re-searched | generic: session-counter hub |
| scene-management `SceneManagement/MonoSingleton.cs:11-12` | Same as above (separate copy of MonoSingleton) | same |
| legacy `Runtime/Components/Prototype/Prototype.cs:167` | `applicationQuitting` never reset → OnDestroy early-returns, pools leak, "instance in pool is null!" errors | SubsystemRegistration reset |
| core `UnityEngineX/ScreenX/ScreenX.cs:102-103,130,136` | `OnScreenSizeChange`/`OnOrientationChange` subscribers accumulate; DPI override persists | manual reset that nulls events, resets DPI, recomputes sizes (not AutoStaticsCleanup — it'd zero `screen`/`viewport`) |
| core `UnityEngineX/OnGUIX.cs:158` | `drawActions` closures over destroyed objects | Clear() on reset |
| legacy `Runtime/Components/GUIDrawer.cs:6` | Same pattern | Clear() on reset |
| core `SceneViewTools/Editor/SceneGUIDrawer.cs:8-9` | `drawActions`/`drawOnceActions` persist; `drawOnceActions` is also never cleared (existing bug, DrawOnce draws forever) | clear `drawOnceActions` after drawing; clear both on play-mode change |
| core `UnityEngineX/DebugX.cs:16,178` | `LogOnce` set never cleared (silent after session 1); runtime `debug=false` sticks | reset |
| core `UnityEngineX/ReflectionX.cs:193` | `s_FieldInfoFromPaths` keyed by object instance: unbounded, pins destroyed objects (leaks even today) | key by `(Type, path)` |
| legacy `Runtime/Components/Debugging/GUIGraph.cs:218` | `lastSampleTime` from old session > new `Time.unscaledTime` → graphs freeze | reset to null |
| legacy `Runtime/Extensions/Serialized Scriptable Singleton/SerializedScriptableSingleton.cs:10,18,20` | `OnCreateOrLoad` subscribers accumulate; `_Instance` carries runtime mutations | generic: hub |
| legacy `Runtime/Components/MonoInstancer.cs:8,30-32` | `CompileReset()` handles reset correctly but nothing calls it | wire via hub or require subclasses to call it |
| screenshot-exporter `ScreenshotCapturer.cs:73,83` | `_capturingScreenshot` stuck true if play exits mid-capture → all later captures refused | reset + try/finally |
| screenshot-exporter `Editor/ScreenshotSaverWindow.cs:66,204-206` | constructor does `EditorApplication.update += GameUpdate`, never removed → N× per frame | move to OnEnable/OnDisable |
| slayouts `SLayoutAnimation.cs:348-349` | `_preventingAnim`/`_animationsBeingDefined` corrupted forever after an exception in an anim action | try/finally + reset |
| hex `Hex/HexCoord.cs:1094,1103` | process-wide `_offsetLayout` and `OnChangeOffsetLayout` subscribers persist | reset to OddR / null |
| scene-management `Scene Set Loader/RuntimeSceneSetLoader.cs:49` | `GetLoadedSceneSets` delegate persists; also called without null check at `RuntimeSceneSetLoadTask.cs:100` | reset + null check |
| aws-build-pipeline `Editor/BuildPipelineWindow.cs:19` | `runningPipeline` stuck true if recompile interrupts `async void PerformBuild` | reset in `[InitializeOnLoadMethod]`; cancel uploads on `beforeAssemblyReload` |

## 2. Static state — Medium/Low

- **ScriptableObject singletons with a `CreateInstance` fallback** keep the fallback (and its runtime mutations) across sessions and ignore a real asset added later: legacy `ScriptableSingleton.cs:7` (generic), aws `BuildInfo.cs:11`, versioning `CurrentVersionSO.cs:5`.
- **SLayoutAnimator.cs:48**: `DestroyInstance()` should also null `_instance`.
- **Registries**, self-balancing via OnEnable/OnDisable (cheap insurance): camera-properties `CameraPropertiesModifierZone.cs:5` (also make readonly), trackpad `TrackpadTouchProvider.cs:76`, `SAnimatedProperty.cs:76` pool. region `Region.cs:18` `activeRegions` is never added to, so it's dead code.
- **Begin/End stacks** keep entries after an exception: `OnGUIX.cs:7-10`, `GizmosX.cs:50,61`, `HandlesX.cs:38,48`, `RandomX.cs:7`, `EditorGUIX.cs:11`.
- **Scratch lists pinning Unity refs**: `CanvasGroupX.cs:6`, `UI/CanvasX.cs:130`, `UI/EventSystemX.cs:7`, legacy `CanvasGroupOpacityInteractionEnabler.cs:41`, splines `Spline.cs:295`, grid-maps `RadialGrid3Agent.cs:20-21` (make these locals).
- **Native objects in statics**: these need destroying, not just nulling. Temp mesh lists: `GizmosX.cs:35`, `HandlesX.cs:21`, geometry `PolygonGizmos.cs:15` (grows unbounded with no SceneView open), region `RegionEditor.cs:242`. ui-imposters `UIImposterRenderer.cs:43-44` holds a HideAndDontSave Camera and Canvas. Destroy on `beforeAssemblyReload`.
- **Editor-only**: `SelectionX.cs:25-33` events, `EditorSceneManagerX.cs:52` `OnChangeSceneAssets`. `SmartEditorWindow.cs:5,8`: `_subscribed`/`_visible` are static, so shared across all windows (existing bug); make them instance fields.
- **Should be const/readonly**: `HexCoord.cs:399-401`, Noise/SimplexNoise tables (public and mutable), `SerializableCamera.cs:16`, `BasePolygonRenderer.cs:62`, `InputPoint.cs:46`, SpringPropertyDrawer/NoiseSampler drawer statics, `ServerHostedFileWindow.cs:21-22`, `AudioPeerEditor.cs:6`, `HumanFriendlyCodeGenerator.cs:11`.

## 3. Static constructors / `[InitializeOnLoad]` / unbalanced editor event subscriptions

**Done:** each site now subscribes from `[InitializeOnLoadMethod]` with `-=` before `+=`, and unsubscribes on `AssemblyReloadEvents.beforeAssemblyReload`. ScreenX installs its player loop entry explicitly (editor load and SubsystemRegistration), builds on `GetCurrentPlayerLoop()`, and removes its entry before reload (new `PlayerLoopUtils.RemoveFromPlayerLoop`). EditorSceneManagerX fills its scene lists lazily. SmartEditorWindow's flags are per-window.

All need: named handler, `-=` before `+=`, and unsubscribe on `AssemblyReloadEvents.beforeAssemblyReload`. Prefer `[InitializeOnLoadMethod]` over a static constructor (CoreCLR runs cctors lazily, maybe off the main thread).

| Location | Event |
|---|---|
| core `ScreenX.cs:153-162` | PlayerLoop insertion. It also rebuilds from `GetDefaultPlayerLoop()`, which wipes other packages' systems, and in builds it only runs on first touch. Move to explicit init, use `GetCurrentPlayerLoop()`, remove the existing entry first |
| core `SelectionX.cs:50` | `Selection.selectionChanged` |
| core `GizmosX.cs:16`, `HandlesX.cs:13`, `SceneGUIDrawer.cs:24` | `SceneView.duringSceneGui` (lazy cctor) |
| core `SceneViewUtility.cs:21` | `SceneView.beforeSceneGui` |
| core `SaveAllOnEnterPlayMode.cs:27` | `playModeStateChanged` via a lambda, so saves run N times |
| core `ExtendedScriptableObjectDrawer.cs:26-27` | `selectionChanged`, `beforeAssemblyReload` |
| core `SmartEditorWindow.cs:13-15` | subscribes in the instance constructor (runs during deserialization); move to OnEnable |
| core `EditorSceneManagerX.cs:54-56` | `AssetDatabase.FindAssets` in cctor, which can run mid-import; make lazy |
| legacy `EditorTime.cs:21-24` | `EditorApplication.update` |
| legacy `PrettyTextLayout.cs:28` | `EditorApplication.update` (duplicate within a tick) |
| legacy `GameLayersClassGenerator.cs:82` | `delayCall` |
| geometry `PolygonGizmos.cs:17-24` | `SceneView.duringSceneGui` via an anonymous lambda in the cctor, so it can never be removed |
| hex `Components/Editor/GridChangeReapplier.cs:15`, `HexGridLayoutGuard.cs:14` | `ObjectChangeEvents.changesPublished` |
| springs `Editor/SpringContextMenuPresets.cs:8-10` | `contextualPropertyMenu`, so duplicate Presets menu items |

## 4. Other runtime differences

| Location | Problem | Fix |
|---|---|---|
| core `MathX.cs:473` `HashString` | `string.GetHashCode()` is randomized per process on CoreCLR, so the documented "deterministic" result breaks. No callers in repo, but it's public API | stable hash (FNV-1a) |
| audio `SaveWav.cs:127` | `(short)(sample * 32767)`: Mono wraps out-of-range values (loud clicks), CoreCLR saturates, so output differs | clamp explicitly |
| core `Editor/ConsoleX.cs:9` | `Type.GetType("...,UnityEditor.dll")` is not a valid assembly name, so likely null → NRE | `typeof(Editor).Assembly.GetType(...)` |
| core `EditorWindowX.cs:16`, `HierarchyX.cs:12` | `Type.GetType("...,UnityEditor")` relies on facade forwarding | same pattern |
| aws `AWSUtils.cs`, `UploadToAwsBuildStep.cs`, `BuildPipelineWindow.cs:291` | in-flight async uploads pin the old assembly across a recompile; nothing ever cancels the existing CTS | static CTS cancelled on `beforeAssemblyReload` |
| core `DebugX.cs:457-492` `ShortCallstack` | frame filtering tuned to Mono frame names | verify output after switch |
| audio `Editor/EditorAudio.cs:9-27`, legacy `ExtendedScrollRect.cs:345`, `ScreenshotSaverWindow.cs:313` | reflection on Unity internals; may change in 6.8 | cache + null-check |
| trackpad `TrackpadMultitouchNative.cs` | verify `.bundle` resolution under the CoreCLR editor (native probing differs) | test |
| noise | if any project bakes noise into assets, re-validate bit-exactness (JIT float codegen differs) | test |

## 5. Existing bugs found in passing (not CoreCLR-specific)

**Done:** AStar, HumanFriendlyCodeGenerator and the trackpad refcount are fixed.

- pathfinding `AStar.cs:526-527`: inner loop tests and removes `[findIndex]` instead of `[currTestIndex]`, so with tied costs the wrong entry is checked and duplicates accumulate.
- legacy `HumanFriendlyCodeGenerator.cs:43`: `GenerateSeeded` overwrites the seeded bytes with crypto randomness, so it's never reproducible. Line 21 `GetInt32(0, Length - 1)` treats the exclusive upper bound as inclusive, so the last allowed character is never chosen.
- trackpad `TrackpadTouchProvider.cs:140,166`: `TP_Stop()` is called even when `TP_Start()` failed, which unbalances the process-wide native refcount.
- `MonoSingleton` exists in both core and scene-management.
