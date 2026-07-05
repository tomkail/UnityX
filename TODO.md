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
- [ ] Roll out to the other fully-portable modules after review: UI Imposter, MeshBuilder, FlexLayout, ViewAnimator.
      **Explicitly NOT Tween or Easer yet.**
- [ ] (Opportunistic) Supply the missing `Color32.Compare` helper used by the Text Effects framework.
