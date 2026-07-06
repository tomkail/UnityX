# UnityX — TODO

## Making sub-modules portable
Goal: contained UnityX sub-systems that don't depend on the rest of UnityX get a namespace + asmdefs so their
portability is *visible and compiler-enforced*, as a step toward shipping them as UPM packages. Approach agreed:
inline the few recurring helpers **per module** (self-contained folders), not a shared helper folder.

### Verdict key
- ✅ **Portable** — zero UnityX deps; drop-in as-is (a required Unity *package* like UGUI/TMP is noted, not counted as a blocker).
- 🟢 **Easy** — a few trivial helpers / one small self-contained file to inline or copy.
- 🟡 **Medium** — rides a shared package (Geometry, Grid) or has broad/heavier coupling.
- 🔴 **Large** — deeply coupled; skip.
- ⚙️ **Core** — shared dependency *sink* (the thing others depend on), not a standalone-portability target.

### Recurring shared dependencies (the "hubs")
Most non-portable modules are blocked by the *same* handful of shared pieces. Making these portability-friendly first
unlocks the most modules:
- **PropertyDrawer attributes** (`[Disable]` above all, also `[AssetSaver]`, `[OnChange]`, `[Button]`, `[VisibleIf]`, `[FakeNullable]`) — `Property Drawers/*`. Runtime attribute classes, editor-only drawers.
- **`MonoSingleton<T>`** — `Components/MonoSingleton.cs` (~34 lines, self-contained). Base of GLDebug, Scene Management, Input, FPSManager, VirtualKeyboardManager.
- **`BaseEditor<T>`** — `Editor Tools/Base Editor Class/` (self-contained). Inherited by ~8 modules' custom inspectors.
- **The Geometry package** — `Point`/`PointRect`/`Polygon`/`Line`/`Triangulator` (`Extensions/Geometry/` + loose `Triangulator.cs`). Foundational for Grid, Structures, PolygonRenderer, Region, and 3 UI widgets.
- **Small `UnityEngineX`/`Collections` helpers** — `MathX`, `RectX`, `TransformX`, `ColorX`, `DebugX`, `IEnumerableX.Min/Max/IsEmpty`, `ObjectX.DestroyAutomatic`. Each trivial to inline.

Latent issues found during analysis: (1) `IEnumerableX.Min/Max` silent binding in editor drawers lacking `using System.Linq` (caused the empty-sequence crash fixed in NoiseSampler's `GraphGUI`); (2) `Color32.Compare` is referenced by the Text Effects framework but **not defined in this checkout** — a one-line helper is missing.

### Extensions/ — feature modules
| Module | Files | What it is | External deps | Verdict |
|---|---|---|---|---|
| Algorithms | 8 | Noise (Perlin/Simplex), Bezier, EasingFunction, AStar, UpscaleTools | none (shared/foundational) | ✅ |
| Range | 3 | Serializable float/int `[min,max]` range structs + drawer | none (shared/foundational) | ✅ |
| MeshBuilder | 4 | Growable vert/tri/uv/color buffers → baked `Mesh` | none | ✅ |
| FlexLayout | 4 | Flexbox-style 1D layout solver + drawers | none (own namespace) | ✅ |
| Serialized Scriptable Singleton | 1 | Generic SO persisted to EditorPrefs/PlayerPrefs | none | ✅ |
| Regex | 1 | Regex pattern consts + cached matchers | none | ✅ |
| MenuItems | 1 | Editor CONTEXT menu items for RectTransform rebuild | none | ✅ |
| Geometry | 15 | Point/PointRect/Polygon/Line/Triangle + polygon editor tool | `Triangulator` (loose file, trivial) — shared/foundational | 🟢 |
| Spring | 5 | Physically-based spring (`Spring`, `SpringHandler`) + drawers | editor-only: `RoundTo`, `IEnumerableX.Min/Max` (~3 lines) | 🟢 |
| Timer | 1 | Serializable event-driven stopwatch/countdown | `[Disable]` attr (drop) | 🟢 |
| FSM | 3 | Generic finite state machine | `DebugX` (2), `[Disable]` (~5 lines) | 🟢 |
| ValuePicker | 3 | Priority/blend value resolvers (Selector/Blender) | `DebugX.ListAsString` (1) | 🟢 |
| Property Curve | 5 | Generic keyframed curve over arbitrary types | `IsBetween` (1); drop `PolygonPropertyCurve` leaf to shed Geometry | 🟢 |
| NoiseSampler | 4 | fBm noise sampler + live-graph inspector | extract with `Algorithms/Noise`; ~20 editor lines | 🟢 |
| Spline System | 5 | Cubic-Bézier spline + scene handle editor | `Bezier` (~50 lines), editor `.ToList()` | 🟢 |
| Easer | 15 | SmoothDamp/MoveTowards/spring easers for common types | `[Disable]`, `MathX.Sign`, `QuaternionX.SmoothDamp`, `Spring.Update` (~110 lines) | 🟢 |
| Tween | 10 | Time-driven type tweens (Float/Vec/Color/…) | `Timer` (portable), `ColorX` (one tween) | 🟢 |
| Version Control | 4 | Git branch/SHA reader + version SO + build stamp | `[Info]`, `[Disable]` attrs | 🟢 |
| Undo History | 1 | Generic undo/redo stack | `IEnumerableX.IsNullOrEmpty` (1) | 🟢 |
| Texture Transform Utils | 1 | Texture rotate/flip/transpose via Blit | needs `ApplyImageOrientation` shader asset | 🟢 |
| Audio | 8 | Clip clone/trim, dB math, mic capture, WAV, FFT peer | `IsNullOrEmpty` (1), `BaseEditor` (editor); 6/8 standalone | 🟢 |
| Text (core) | 2 | TMP size/line-balancing utils + `PrettyTextLayout` | `Best`, `IsNullOrEmpty`, `RectX.Encapsulating` (all trivial) | 🟢 |
| Text Effects | 17 | TMP per-vertex/material effects framework (wobble/fader/gradient/…) | `ObjectX.DestroyAutomatic`, `ResetTransform`, `Range` struct, `GradientX.GradientType`, `MathX.Abs`, **missing `Color32.Compare`** | 🟢 |
| Serializable Components | 2 | `SerializableTransform` (portable) + `SerializableCamera` | `SerializableCamera` needs `CameraX` frustum math | 🟢 |
| GLDebug | 1 | Batched GL/Gizmo line drawing singleton | `MonoSingleton<T>` (~34 lines) + 2 shaders | 🟢 |
| Grid | 22 | 2D/3D grid + typed maps + renderer + agents | copy `Point`+`PointRect` (~780 lines) + ~20 small helpers | 🟡 |
| Structures | 7 | Point-cloud Shape/Structure + flood-fill island detectors | `Point`/`PointRect` + `TypeMap`→Grid; Island detectors nearly portable | 🟡 |
| Scene Management | 9 | ScriptableObject scene-sets + additive runtime loader | `MonoSingleton`, `BaseEditor`, `EditorSceneManagerX` | 🟡 |
| Camera | 9 | Orbit/framing camera-rig + shot generation | `SerializableCamera`+`SerializableTransform`+`CameraX` (~1300 lines) | 🔴 |

### Extensions/ — loose utility files (directly in `Extensions/`)
| File | What it is | External deps | Verdict |
|---|---|---|---|
| Triangulator.cs | Ear-clipping polygon triangulator | none — shared/foundational (Geometry, PolygonRenderer, Region, UIPolygon, GizmosX) | ✅ |
| ScaleUtils.cs | Fit/crop/stretch content into container | none (stray `Debug.Log` left in) | ✅ |
| ScaleToContainerUtils.cs | Aspect fit/fill math + Unity mode conversions | none | ✅ |
| HumanFriendlyCodeGenerator.cs | Human-readable code gen from distinct charset | none (pure .NET) | ✅ |
| WeightedBlends.cs | Generic weighted-blend helpers (bool/float/Vec3/Quat) | none | ✅ |
| VirtualKeyboardManager.cs | On-screen keyboard rect tracking/animation singleton | `MonoSingleton`, `EasingFunction`, `SLayout`, `OnGUIX`, `RectTransformX`, `ColorX`, TMP | 🔴 |

### Components/ — runtime components
| Module | Files | What it is | External deps | Verdict |
|---|---|---|---|---|
| ViewAnimator | 2 | Timeline firing callbacks on scrubbed events | none | ✅ |
| Utils (QuitGameOnKeypress) | 1 | Quit on keypress | none | ✅ |
| TextMeshPro (bg highlight) | 1 | Colored background quads behind TMP lines | none (TMP pkg) | ✅ |
| Screen (PlayerLoopUtils) | 1 | Inspect/inject Unity PlayerLoop systems | none | ✅ |
| UI Imposter | 3 | Render UGUI group to a RenderTexture | none (UGUI pkg) | ✅ |
| Transform | 4 | LockTransform + TransformCopier (+editors) | `TransformX` (2), `BaseEditor` (editor) | 🟢 |
| FPSManager | 3 | Target-FPS + rolling FPS readout singleton | `MonoSingleton<T>` | 🟢 |
| EnforceDecendentGameObjectProperties | 3 | Force tag/layer/static onto descendants | `BaseEditor` (editor) | 🟢 |
| Render Texture Creator | 2 | Create/resize a RenderTexture + preview | `TransformX.HierarchyPath` | 🟢 |
| Prototype | 2 | Pseudo-prefab pooling component | `[Disable]`, `HierarchyPath`, `BaseEditor` | 🟢 |
| HideFlags | 2 | Apply HideFlags to children | `BaseEditor` (editor); runtime portable | 🟢 |
| ChangeCheckers | 2 | Poll transform/GO changes → events | `SerializableTransform`, `BetterSendMessage` | 🟢 |
| Events (TriggerListener) | 1 | Re-broadcast collision/trigger messages | `DebugX`, `HierarchyPath`, `LayerMask.Includes` | 🟢 |
| Debugging (GUIGraph) | 1 | IMGUI oscilloscope grapher | `ColorX.WithAlpha` (1) | 🟢 |
| PolygonRenderer | 7 | Build filled/outline mesh from a `Polygon` | Geometry package + MeshBuilder + trivial helpers | 🟡 |
| Region | 2 | Polygonal region: containment/raycast/extrude mesh | Geometry package + `PolygonEditorTool` (editor) | 🟡 |
| Input | 12 | Touch/mouse/keyboard abstraction + gestures | `MonoSingleton`, `RectX`, `Best`; `ScreenX` (heavy, 1 line) | 🟡 |
| Audio | 2 | Timescale/focus-aware AudioSource wrapper | `LogicBlender`, `FloatTween`, `DebugX`, `BaseEditor`, `EditorGUILayoutX` | 🟡 |

### Components/UI/ — widget collection (60 files, ~24 widgets)
A grab-bag of independent UGUI helpers (all need the **UGUI package**, not counted). Coupling to UnityX is light —
mostly one-off `Vector2X`/`RectX`/`MathX`/`ColorX` calls and `BaseEditor` in 3 editors. Roughly **half are ✅** (e.g.
Extended Button/Slider, ExtendedCanvasScaler, Outlines, UIGradient, CanvasWorldScaler, AbsoluteRectTransformController,
CarouselUIView, EnforceImageAspectRatio, UIMonoBehaviour), and **most of the rest 🟢** (Draggable, ExtendedScrollRect,
Grid Layout, Swipe View, RoundRect, Saturation/Background-Blur, WorldSpaceUIElement, InvisibleInteractable). The **🟡**
exceptions: `Line`, `Polygon`, `RoundRectPolygonUI` (need the **Geometry** namespace), and `SLayout` (~2100-line
in-house layout-animation framework — MEDIUM by *size*, coupling is just `RectX` + optional TMP + `BaseEditor`).

### Editor Tools/ (editor-only)
| Tool | Files | What it is | External deps | Verdict |
|---|---|---|---|---|
| Base Editor Class | 1 | `BaseEditor<T>` typed inspector base | none — shared/foundational | ✅ |
| CameraUtilities | 1 | Window listing scene cameras | none | ✅ |
| DetectLeaksWindow | 1 | Live object/asset census window | none | ✅ |
| EditorTime | 1 | Editor deltaTime tracker → shader global | none | ✅ |
| SceneView | 2 | Scene-GUI callback registrar + cull helpers | none | ✅ |
| SerializedEditorSettings | 1 | Generic JSON-in-EditorPrefs settings | none | ✅ |
| Transformer | 1 | Window to offset selected transforms | none | ✅ |
| CommentComponent | 2 | Editable note component | `BaseEditor` | 🟢 |
| Icon | 5 | Assign gizmo/label icons to GameObjects | `BaseEditor` | 🟢 |
| GameLayersClassGenerator | 1 | Codegen `GameLayers.cs` from layers | `ScriptAssetCreator` | 🟢 |
| Texture Creator | 1 | Window generating solid/gradient textures | `.Abs()` (1) | 🟢 |
| FolderInspector | 2 | Custom inspector for folder assets | `EditorApplicationX`, `PathX` | 🟡 |
| Screenshot Exporter | 7 | Screenshot capture/export pipeline | collection exts, `SystemInfoX`, `CoroutineHelper` | 🟡 |

### Property Drawers/ (40 subfolders, 45 files) — 🟢 as a set
Uniform pattern: a runtime `PropertyAttribute` + an editor-only `PropertyDrawer` (shared base in `Property Drawers/Editor/`).
**~30 are pure** (Unity/.NET only): Clamp family, MinMax, SteppedRange, CurveRange, EnumButtons(+Group), Disable, Lock,
Password, Regex, Label, Info, Vector2/3Toggle, PreviewTexture, OnChange, AssetSaver, etc. **~10 use small global
`UnityEditorX`/`UnityEngineX` helpers** (`EditorGUIX`, `SerializedPropertyX`, `ReflectionX`, `EditorApplicationX`, `RectX`,
`PathX`, `OnGUIX`) — heaviest are the reflection-driven ones (`If`/`VisibleIf`/`DisableIf`, `PropertyPopup`, `Button`).
Port those few helpers once and the whole set travels.

### Extensions/ — core grab-bags (⚙️ dependency sink, not portability targets)
| Module | Files | Holds | Notes |
|---|---|---|---|
| UnityEngineX | 61 | `MathX`, `Vector2/3/4X`, `QuaternionX`, `RectX`, `BoundsX`, `TransformX`, `ColorX`, `DebugX`, `GizmosX`, `CameraX`, … | The hub — self-contained (only Unity/.NET); most depended-on folder. |
| UnityEditorX | 32 | `AssetDatabaseX`, `EditorGUIX`, `HandlesX`, `SerializedPropertyX`, `EditorApplicationX`, … | Editor-only; depends on UnityEngineX. |
| System | 9 | `StringX`, `TypeX`, `EnumX`, `PathX`, `DirectoryX`, … | Most independent (near-pure .NET). |
| Collections | 9 | `IEnumerableX`, `ListX`, `ArrayX`, `DictionaryX` + `ShuffleBag`/`ProbabilityList`/… | `ProbabilityList` → UnityEngineX; otherwise .NET. |

Net dependency direction among core: **System, UnityEngineX (base) ← Collections, UnityEditorX**. All sit in the global
namespace, which is why they act as an ambient shared core.

### Convention (proposed — prefix pending confirmation; `UnityX` recommended)
- Namespace `UnityX.<Module>` runtime / `UnityX.<Module>.Editor` for editor code. Pluralize when the module name
  equals its main type to avoid a namespace/type clash (`UnityX.Springs`, not `UnityX.Spring.Spring`). Normalize
  today's inconsistent names (`UnityX.MeshBuilder` → `UnityX.Meshes`, bare `FlexLayout` → `UnityX.Layout`).
- Two asmdefs per module: a runtime asmdef at the folder root with **empty `references`** (this is what *enforces*
  portability — it fails to compile if it touches another UnityX type), plus an editor-only asmdef
  (`includePlatforms: ["Editor"]`) in `Editor/` that references the runtime one. Set `rootNamespace` and
  `autoReferenced: true`.
- Exception: UI Imposter is not zero-reference — needs `UnityEngine.UI` (runtime) / `UnityEditor.UI` (editor).

### Packaging (later)
Each module becomes a UPM package (folder + `package.json`, id `com.unityx.<module>`; scripts under `Packages/`
require asmdefs). Distribute via GitHub git-URL (`...git?path=Packages/com.unityx.springs#v1.0.0`, Unity 6 supports
`?path=`); pin exact tags. For SemVer ranges / transitive deps, publish through OpenUPM. Recommended repo shape:
a single monorepo of packages, split per-repo only if independent release cadences are needed.

### Tasks
- [x] Confirm namespace prefix (`UnityX` vs author handle vs none). → **`UnityX`** chosen.
- [x] Scaffold **Spring** in place as the reviewable template (namespace + 2 asmdefs + Easy fix). Done:
      runtime `UnityX.Springs` (empty-`references` asmdef, enforces portability) + editor `UnityX.Springs.Editor`
      (Editor-only asmdef → references the runtime one). Inlined `MathX.RoundTo` into the editor's `GraphGUI`
      (the `.Min`/`.Max` calls now bind to `System.Linq`, so no UnityX runtime dep remains). Added
      `using UnityX.Springs;` to the two struct consumers (`SwipeView`, `Easer/SmoothDamp/SpringDamper`).
      Compiles clean. **Awaiting review before rollout.**
- [x] Modularised (namespace + asmdef(s), compiles clean; each runtime asmdef has empty `references` unless noted):
      - **Splines** — `Spline System/` → `UnityX.Splines` (+ `.Editor`). Inlined `Bezier` (internal); editor uses `System.Linq` for `.ToList()`.
      - **Noises** — `Algorithms/Noise/` → `UnityX.Noises`. Pluralised to avoid the `Noise` type/namespace clash. Moved `NoiseNormalization` enum here.
      - **NoiseSampler** — `NoiseSampler/` → `UnityX.NoiseSampler` (+ `.Editor`), references `UnityX.Noises`. Inlined `MathX.RoundTo`.
      - **Timers** — `Timer/` → `UnityX.Timers` (+ `.Editor`). Dropped `[Disable]`. **Moved `TimerDrawer` in from Tween/** (it belongs with Timer).
      - **Colors** — NEW `Colors/` module → `UnityX.Colors`. Extracted `BlendMode` + `Blend` + blend helpers + `HSLColor` out of the `ColorX` grab-bag (ColorX now references it). Consumers `AdvancedUILineRenderer` + `ColorTween` rewired to `ColorBlend`/`BlendMode`.
      - **Tween** — `Tween/` → `UnityX.Tween` (+ `.Editor`), references `UnityX.Timers` + `UnityX.Colors`. Consumers: `AudioSourceManager`, `CameraPropertiesTween`.
      - **Easer** — `Easer/` → `UnityX.Easer`, references `UnityX.Springs`. Inlined `QuaternionX.Difference/SmoothDamp` + `MathX.Sign` as `EaserMath`; dropped `[Disable]`; removed unused `using UnityEngine.UI`. Consumer: `ThumbstickUI`.
      - **Layout** — `FlexLayout/` → `UnityX.Layout` (+ `.Editor`), zero-dep. Rename also resolved the `FlexLayout.FlexLayout` type/namespace awkwardness.
      - **Meshes** — `MeshBuilder/` → `UnityX.Meshes` (was `UnityX.MeshBuilder`), zero-dep. Improvement: `ToMesh` now sets a 32-bit index buffer for >65535 verts (was a latent break). Consumer `PolygonOutlineRenderer` re-pointed to `UnityX.Meshes`.
      - **ViewAnimation** — `Components/ViewAnimator/` → `UnityX.ViewAnimation` (avoids the `ViewAnimator` type/namespace clash), zero-dep. NOTE: `ViewAnimator.Reset()` shadows Unity's editor `Reset()` message (clicking Reset / adding the component wipes animation state) — left as-is, flag for review.
      - **UIImposters** — `Components/UI/UI Imposter/` → `UnityX.UIImposters` (+ `.Editor`). Runtime references `UnityEngine.UI`; editor references `UnityEngine.UI` + `UnityEditor.UI` (extends `RawImageEditor`). No consumers.
      - **Versioning** — `Version Control/` → `UnityX.Versioning` (+ `.Editor`). Wrapped the still-global `VersionControlX`; moved `VersionBuildPreProcessor` to `.Editor`; dropped `[Disable]`/`[Info]` on `Version` (restore once PropertyDrawers is a module).
      - **StateMachines** — `FSM/` → `UnityX.StateMachines` (was `UnityX.StateMachine`; pluralised to dodge the `StateMachine` type/namespace clash). Replaced 2 `DebugX` calls with `Debug.LogError`; dropped `[Disable]` on `State.elapsedTimeInState`. No consumers.
      - **ValuePicker** — `ValuePicker/` → `UnityX.ValuePicker`. Inlined `DebugX.ListAsString` as a private helper in `LogicBlender`. Consumer `AudioSourceManager` gets `using UnityX.ValuePicker`.
      - **PropertyCurves** — `Property Curve/` → `UnityX.PropertyCurves` (pluralised; `PropertyCurve<T>` type). Inlined `IsBetween`; relocated unused `PolygonPropertyCurve` bridge to Assembly-CSharp (`Extensions/PropertyCurvePolygon/`).
      - **UI.GridLayout** — `Components/UI/Grid Layout/` → `UnityX.UI.GridLayout` (+ `.Editor`). Moved out of `namespace UnityEngine.UI` into `UnityX.UI`; renamed type `GridLayout`→`GridLayoutElement` (kills the `UnityEngine.GridLayout` Tilemap clash + editor alias); `.cs` renamed (meta GUID kept). Refs `UnityEngine.UI` (+ `UnityEditor.UI`). First of the 13 UI files to leave `UnityEngine.UI`.
      (Meshes/ViewAnimation/UIImposters/Versioning/StateMachines/ValuePicker: VERIFIED clean. PropertyCurves/UI.GridLayout: edits done, verify pending — MCP bridge down.)
### asmdef granularity — policy (not religious)
An asmdef is a *boundary you actually need*, not "one per module". Make one only when the module is (a) a
plausible standalone UPM **package**, (b) worth **compile-isolating** (changes often), or (c) a wall you want to
**enforce** (empty `references`). Everything else stays in `Assembly-CSharp`. **Do NOT asmdef single-file utilities** —
the loose one-file helpers (`ScaleUtils`, `ScaleToContainerUtils`, `WeightedBlends`, `HumanFriendlyCodeGenerator`,
`Regex`, `MenuItems`, `Serialized Scriptable Singleton`, `Undo History`, `GLDebug`, `Texture Transform Utils`, one-off
components/editor tools) either stay in Assembly-CSharp or fold into a shared `UnityX.Core`. Prefer bundling over
proliferation (e.g. all Property Drawers = one assembly, not one-per-drawer).

### Packaging candidates (cohesive, reasonable file counts) — verified file counts
**Tier 1 — easy next wins (few files, ✅/light 🟢):**
- [x] MeshBuilder (4, 0 ed) → `UnityX.Meshes` — done (+ 32-bit index buffer fix).
- [x] ViewAnimator (2, 0 ed) → `UnityX.ViewAnimation` — done.
- [x] UI Imposter (`Components/UI/UI Imposter/`, 3, 1 ed) → `UnityX.UIImposters` — done (refs UnityEngine.UI / UnityEditor.UI).
- [~] Range — DE-SCOPED as a standalone module (it's a tiny serializable min/max struct + drawer, i.e. a value type, not a package). Leave in Assembly-CSharp / fold into a future `UnityX.Core`.
- [x] FSM (3, 0 ed) → `UnityX.StateMachines` — done (pluralised to avoid the `StateMachine` clash; DebugX→Debug.LogError, dropped `[Disable]`).
- [x] ValuePicker (3, 0 ed) → `UnityX.ValuePicker` — done (inlined `DebugX.ListAsString`; consumer `AudioSourceManager`).
- [x] Property Curve (5→4, 0 ed) → `UnityX.PropertyCurves` — done. Inlined `MathX.IsBetween` (exclusive range check); relocated the unused `PolygonPropertyCurve` (a PropertyCurve↔Geometry bridge, so it can't live in either empty-refs module) out to `Extensions/PropertyCurvePolygon/` in Assembly-CSharp.
- [x] Version Control (4, 1 ed) → `UnityX.Versioning` (+ `.Editor`) — done (dropped `[Info]`/`[Disable]`).
- [~] Audio — `Extensions/Audio` (8, 3 ed) — DE-SCOPED for now: a heterogeneous utils grab-bag (WAV/mic/FFT/clip), not one cohesive feature. Revisit only if an "audio tools" package is actually wanted.

**Tier 2 — foundational, unlock the rest (bigger, higher leverage):**
- [ ] Geometry (15, 4 ed) → `UnityX.Geometry` (bundle the loose `Triangulator.cs`). Unlocks Grid, PolygonRenderer, Region, UI Line/Polygon.
- [ ] Property Drawers (86, 44 ed) → one `UnityX.PropertyDrawers` (+ `.Editor`). Shared hub — lets modules *use* `[Disable]`/`[Info]` instead of dropping them.
- [ ] Core: `UnityEngineX` + `Collections` + `System` → `UnityX.Core`; `UnityEditorX` → `UnityX.Core.Editor`. The dependency sink; removes the need to inline helpers per module.

**Tier 3 — depend on Tier 2 (do after):**
- [ ] Grid (22, 1 ed) — after Geometry.
- [ ] PolygonRenderer (7, 3 ed) + Region (2) — after Geometry + MeshBuilder.
- [ ] Structures (7) — after Geometry/Grid.
- [ ] Input (12, 0 ed) — needs a home for `MonoSingleton` + `ScreenX`.
- [ ] Text Effects (17, 0 ed) — needs the `Color32.Compare` fix + `Range`/`GradientX`.

### Point → Vector2Int migration (in progress; keep `Point`, don't delete)
Point predates Vector2Int; migrating to Unity's type. Added implicit `Point`↔`Vector2Int` conversions + a
`Vector2Int.Area()` extension (`UnityEngineX/Vector2IntX.cs`) so code migrates incrementally (mixed code compiles).
- [x] Beachhead: `Grid.size` (`Point`→`Vector2Int`, serialized field — identical {x,y} layout) + `size.area`→`size.Area()`
      in Grid.cs and the inheriting TypeMap.cs. **Verify serialization survives on a real Grid asset before proceeding.**
- [x] `Grid.cs` fully migrated off Point (only `PointRect` remains — kept type). Added `Vector2Int` direction
      extensions (`CardinalDirections`/`OrdinalDirections`/`CompassDirections` in `Vector2IntX`, offsets matching Point).
      Cascade check: only `TypeMap : Grid` subclasses it, nothing overrides the changed virtuals, and Grid's changed
      collection-returning methods have no external callers — so implicit conversions bridge consumers; should compile standalone.
- [ ] Remaining ~35 files (TypeMap, GridRenderer, agents, Structures/Shape, scattered consumers). Migrate module-by-module.
- [ ] Keep `PointRect` (→ `RectInt` is higher-friction: no `MinMaxRect`/`ToRect`, different semantics). Separate, later.
- [ ] (Opportunistic) Supply the missing `Color32.Compare` helper used by the Text Effects framework.
- [ ] Pre-existing unrelated error to resolve separately: `Grass/GrassComputeScript.cs` references `VectorFieldComponent.gridRenderer` which no longer exists (concurrent change to VectorFieldComponent).
