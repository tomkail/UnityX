using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector for <see cref="TrackpadTouchProvider"/>: live status, device readout, an in-inspector
/// touch visualizer, a live per-touch data list, and a one-click native-bundle rebuild.
/// </summary>
[CustomEditor(typeof(TrackpadTouchProvider))]
public class TrackpadTouchProviderEditor : Editor {
    static Texture2D _dot;
    static GUIStyle _bannerStyle;
    bool _showTouchData = true;

    // A plain style (not derived from EditorStyles) so it carries no hover/active state — the banner
    // renders identically whether or not the mouse is over it.
    static GUIStyle BannerStyle() {
        if (_bannerStyle != null) return _bannerStyle;
        _bannerStyle = new GUIStyle {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(8, 8, 0, 0),
        };
        _bannerStyle.normal.textColor = Color.white;
        _bannerStyle.hover.textColor = Color.white;
        _bannerStyle.active.textColor = Color.white;
        _bannerStyle.focused.textColor = Color.white;
        return _bannerStyle;
    }

    // Repaint continuously in play mode so the live visualizer animates.
    public override bool RequiresConstantRepaint() => Application.isPlaying;

    public override void OnInspectorGUI() {
        var p = (TrackpadTouchProvider)target;

        DrawStatusBanner(p);
        EditorGUILayout.Space(2);

        DrawFilteredInspector(p);

        if (Application.isPlaying) {
            EditorGUILayout.Space(6);
            DrawDevices(p);
            EditorGUILayout.Space(4);
            DrawVisualizer(p);
            EditorGUILayout.Space(4);
            DrawTouchData(p);
        }

        EditorGUILayout.Space(8);
        DrawTools();
        DrawHelp();
    }

    // Draws the serialized fields (with their [Header]/[Tooltip] decorators) but hides fields that
    // don't apply given the current mode: inputAspect when auto-detecting, targetRect when full-screen.
    void DrawFilteredInspector(TrackpadTouchProvider p) {
        serializedObject.Update();
        var prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren)) {
            enterChildren = false;
            if (prop.name == "m_Script") continue;
            if (prop.name == "inputAspect" && p.autoDetectInputAspect) continue;
            if (prop.name == "targetRect" && p.mapToFullScreen) continue;
            EditorGUILayout.PropertyField(prop, true);
        }
        serializedObject.ApplyModifiedProperties();
    }

    // ---- sections ---------------------------------------------------------------------

    void DrawStatusBanner(TrackpadTouchProvider p) {
        Color bg; string msg; MessageType type;
        if (!Application.isPlaying) {
            bg = new Color(0.4f, 0.4f, 0.4f); msg = "Enter Play mode to start the trackpad."; type = MessageType.Info;
        } else if (!p.running) {
            bg = new Color(0.6f, 0.2f, 0.2f); msg = $"Not running — {p.status}"; type = MessageType.Error;
        } else if (!p.captured) {
            bg = new Color(0.6f, 0.5f, 0.15f); msg = "Released — click into the Game view to capture the trackpad."; type = MessageType.Warning;
        } else {
            bg = new Color(0.18f, 0.5f, 0.25f); msg = $"● Running — {p.deviceCount} device(s), {p.touches.Count} live touch(es). Esc to release."; type = MessageType.None;
        }

        var rect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, bg);
        GUI.Label(rect, msg, BannerStyle());
        if (type == MessageType.Error) EditorGUILayout.HelpBox(p.status, MessageType.Error);
    }

    void DrawDevices(TrackpadTouchProvider p) {
        if (p.devices == null || p.devices.Count == 0) return;
        EditorGUILayout.LabelField("Devices", EditorStyles.boldLabel);
        using (new EditorGUI.IndentLevelScope()) {
            foreach (var d in p.devices) {
                string dims = d.sensorWidth > 0 ? $"{d.sensorWidth}×{d.sensorHeight} (aspect {d.aspect:0.000})" : "dimensions unavailable";
                EditorGUILayout.LabelField($"Device {d.index}", $"{(d.builtIn ? "built-in" : "external")} · {dims}");
            }
        }
    }

    void DrawVisualizer(TrackpadTouchProvider p) {
        EditorGUILayout.LabelField("Live Touches", EditorStyles.boldLabel);

        float aspect = p.inputAspect > 0.1f ? p.inputAspect : 1.6f;
        float width = EditorGUIUtility.currentViewWidth - 40f;
        float height = Mathf.Clamp(width / aspect, 60f, 260f);
        var pad = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(true));
        pad = new Rect(pad.x + 4, pad.y, pad.width - 8, pad.height);

        EditorGUI.DrawRect(pad, new Color(0.12f, 0.12f, 0.14f));
        DrawBorder(pad, new Color(0.3f, 0.3f, 0.35f));

        var labelStyle = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
        foreach (var t in p.touches) {
            // normalized origin is bottom-left; GUI is top-left → flip Y.
            float x = pad.x + t.normalizedPosition.x * pad.width;
            float y = pad.y + (1f - t.normalizedPosition.y) * pad.height;
            float pressure = Mathf.Clamp01(t.size / Mathf.Max(0.0001f, p.pressureScale));
            float r = Mathf.Lerp(7f, 26f, pressure);
            var col = Color.Lerp(new Color(0.3f, 0.8f, 1f), new Color(1f, 0.4f, 0.3f), pressure);

            var prev = GUI.color;
            GUI.color = col;
            GUI.DrawTexture(new Rect(x - r, y - r, r * 2, r * 2), Dot());
            GUI.color = prev;
            GUI.Label(new Rect(x - r, y - 8, r * 2, 16), t.touchId.ToString(), labelStyle);
        }

        if (p.touches.Count == 0) {
            var hint = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            GUI.Label(pad, "(touch the trackpad)", hint);
        }
    }

    void DrawTouchData(TrackpadTouchProvider p) {
        _showTouchData = EditorGUILayout.Foldout(_showTouchData, $"Touch Data ({p.touches.Count})", true);
        if (!_showTouchData) return;
        using (new EditorGUI.IndentLevelScope()) {
            foreach (var t in p.touches) {
                EditorGUILayout.LabelField($"#{t.touchId} (dev {t.deviceIndex})",
                    $"pos {t.normalizedPosition.x:0.00},{t.normalizedPosition.y:0.00} · press {t.size:0.00} · {Mathf.Rad2Deg * t.angle:0}° · vel {t.velocity.magnitude:0.0}");
            }
        }
    }

    void DrawTools() {
        using (new EditorGUILayout.HorizontalScope()) {
            if (GUILayout.Button("Rebuild Native Bundle")) RebuildBundle();
            if (GUILayout.Button("Reveal Source")) {
                string dir = Path.Combine(Application.dataPath, "UnityX/Plugins/TrackpadMultitouch/Source");
                EditorUtility.RevealInFinder(dir);
            }
        }
    }

    void DrawHelp() {
        EditorGUILayout.HelpBox(
            "Native plugins never unload — after Rebuild Native Bundle you must restart the Editor for it to take effect.\n" +
            "macOS trackpad gestures (Mission Control, swipes) can't be suppressed per-app; disable them in System Settings ▸ Trackpad while using this.",
            MessageType.Info);
    }

    // ---- helpers ----------------------------------------------------------------------

    void RebuildBundle() {
        string script = Path.Combine(Application.dataPath, "UnityX/Plugins/TrackpadMultitouch/Source/build.sh");
        if (!File.Exists(script)) { Debug.LogError($"build.sh not found at {script}"); return; }
        try {
            var psi = new System.Diagnostics.ProcessStartInfo("/bin/bash", $"\"{script}\"") {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            if (proc.ExitCode == 0) {
                Debug.Log($"[TrackpadMultitouch] Rebuilt bundle.\n{stdout}");
                EditorUtility.DisplayDialog("Bundle rebuilt",
                    "The native bundle was rebuilt. Restart the Unity Editor for the new binary to load (native plugins never hot-reload).", "OK");
            } else {
                Debug.LogError($"[TrackpadMultitouch] build.sh failed (exit {proc.ExitCode}).\n{stdout}\n{stderr}");
            }
        } catch (System.Exception e) {
            Debug.LogError($"[TrackpadMultitouch] Rebuild failed: {e.Message}");
        }
    }

    static void DrawBorder(Rect r, Color c) {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), c);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), c);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), c);
        EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), c);
    }

    static Texture2D Dot() {
        if (_dot != null) return _dot;
        const int s = 64;
        _dot = new Texture2D(s, s, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
        float rad = s * 0.5f;
        var center = new Vector2(rad, rad);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++) {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float a = Mathf.Clamp01(rad - d); // ~1px anti-aliased edge
                _dot.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        _dot.Apply();
        return _dot;
    }
}
