using NUnit.Framework;
using UnityEngine;
using AspectMode = TrackpadTouchProvider.AspectMode;

// Edit-mode tests for TrackpadTouchProvider.ComputeAspectRect — the pure aspect-fit math.
// Runnable via the Test Runner window (Window ▸ General ▸ Test Runner ▸ EditMode).
public class TrackpadMultitouchAspectTests {
    const float Eps = 1e-3f;

    static void AssertRect(Rect expected, Rect actual, string msg) {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(Eps), msg + " (x)");
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(Eps), msg + " (y)");
        Assert.That(actual.width, Is.EqualTo(expected.width).Within(Eps), msg + " (width)");
        Assert.That(actual.height, Is.EqualTo(expected.height).Within(Eps), msg + " (height)");
    }

    static readonly Rect Square = new Rect(0, 0, 100, 100);
    static readonly Rect Wide = new Rect(0, 0, 200, 100); // aspect 2.0

    [Test]
    public void Stretch_ReturnsTargetUnchanged() {
        AssertRect(Square, TrackpadTouchProvider.ComputeAspectRect(Square, AspectMode.Stretch, 2f),
            "Stretch should return the target rect unchanged");
    }

    [Test]
    public void Fit_InputWiderThanTarget_Letterboxes() {
        AssertRect(new Rect(0, 25, 100, 50),
            TrackpadTouchProvider.ComputeAspectRect(Square, AspectMode.Fit, 2f),
            "Fit (input 2.0 into square) should letterbox top/bottom");
    }

    [Test]
    public void Fit_InputNarrowerThanTarget_Pillarboxes() {
        AssertRect(new Rect(50, 0, 100, 100),
            TrackpadTouchProvider.ComputeAspectRect(Wide, AspectMode.Fit, 1f),
            "Fit (input 1.0 into 2:1) should pillarbox left/right");
    }

    [Test]
    public void Fill_InputWiderThanTarget_OverscansHorizontally() {
        AssertRect(new Rect(-50, 0, 200, 100),
            TrackpadTouchProvider.ComputeAspectRect(Square, AspectMode.Fill, 2f),
            "Fill (input 2.0 into square) should cover, overflowing left/right");
    }

    [Test]
    public void Fill_InputNarrowerThanTarget_OverscansVertically() {
        AssertRect(new Rect(0, -50, 200, 200),
            TrackpadTouchProvider.ComputeAspectRect(Wide, AspectMode.Fill, 1f),
            "Fill (input 1.0 into 2:1) should cover, overflowing top/bottom");
    }

    [TestCase(AspectMode.Fit)]
    [TestCase(AspectMode.Fill)]
    public void PreservesInputAspect(AspectMode mode) {
        var r = TrackpadTouchProvider.ComputeAspectRect(Wide, mode, 1.6f);
        Assert.That(r.width / r.height, Is.EqualTo(1.6f).Within(Eps), $"{mode} must preserve inputAspect");
    }

    [Test]
    public void NonPositiveInputAspect_FallsBackToTarget() {
        AssertRect(Square, TrackpadTouchProvider.ComputeAspectRect(Square, AspectMode.Fit, 0f),
            "Non-positive inputAspect should fall back to the target");
    }
}
