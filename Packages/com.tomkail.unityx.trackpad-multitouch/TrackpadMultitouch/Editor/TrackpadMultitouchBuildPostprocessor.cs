#if UNITY_EDITOR_OSX
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// After a macOS standalone build, strips the quarantine attribute from the bundled
/// TrackpadMultitouch.bundle and ad-hoc signs it, so Gatekeeper doesn't refuse to load it
/// ("bundle is damaged"). For notarized distribution you should still re-sign the whole .app
/// with your Developer ID team — this just makes local/dev builds run out of the box.
/// </summary>
public class TrackpadMultitouchBuildPostprocessor : IPostprocessBuildWithReport {
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report) {
        if (report.summary.platform != BuildTarget.StandaloneOSX) return;

        string app = report.summary.outputPath; // the .app bundle
        string bundle = Path.Combine(app, "Contents/PlugIns/TrackpadMultitouch.bundle");
        if (!Directory.Exists(bundle)) {
            Debug.LogWarning($"[TrackpadMultitouch] Bundle not found in build at {bundle}; skipping post-build signing.");
            return;
        }

        Run("/usr/bin/xattr", $"-dr com.apple.quarantine \"{bundle}\"");
        int code = Run("/usr/bin/codesign", $"--force --sign - \"{bundle}\"");
        if (code == 0)
            Debug.Log("[TrackpadMultitouch] Post-build: ad-hoc signed the bundle. For notarized distribution, " +
                      "re-sign the .app with your Developer ID team.");
        else
            Debug.LogWarning($"[TrackpadMultitouch] Post-build codesign returned {code}; the bundle may not load on other machines.");
    }

    static int Run(string exe, string args) {
        try {
            var psi = new System.Diagnostics.ProcessStartInfo(exe, args) {
                UseShellExecute = false, RedirectStandardError = true, CreateNoWindow = true,
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            string err = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            if (proc.ExitCode != 0 && !string.IsNullOrEmpty(err))
                Debug.LogWarning($"[TrackpadMultitouch] {Path.GetFileName(exe)}: {err.Trim()}");
            return proc.ExitCode;
        } catch (System.Exception e) {
            Debug.LogWarning($"[TrackpadMultitouch] Failed to run {exe}: {e.Message}");
            return -1;
        }
    }
}
#endif
