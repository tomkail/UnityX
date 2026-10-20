#if UNITYX_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

// URP has no GrabPass, so while any BackgroundBlurUI is enabled this pass copies each camera's colour into a
// mip-mapped global texture (_BackgroundBlurUITexture) for the blur shader to sample.
// It's enqueued from RenderPipelineManager.beginCameraRendering, so it needs no renderer feature.
class BackgroundBlurUIRenderPass : ScriptableRenderPass {
    static readonly int TextureProperty = Shader.PropertyToID("_BackgroundBlurUITexture");
    static readonly int TexelSizeProperty = Shader.PropertyToID("_BackgroundBlurUITexelSize");

    static BackgroundBlurUIRenderPass instance;

    class PassData {
        public TextureHandle texture;
    }

    BackgroundBlurUIRenderPass() {
        profilingSampler = new ProfilingSampler(nameof(BackgroundBlurUI));
        // Lets the pass read the camera colour after post-processing, which would otherwise go straight to the backbuffer.
        requiresIntermediateTexture = true;
    }

    public static void Register() {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    public static void Unregister() {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) {
        if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView) return;
        var renderer = camera.GetUniversalAdditionalCameraData().scriptableRenderer;
        if (renderer == null) return;

        instance ??= new BackgroundBlurUIRenderPass();
        // Screen space overlay canvases draw after the camera has finished, so they can blur everything it drew.
        // Other canvases draw among the camera's transparent objects (as does every canvas in the scene view), so
        // they can only blur what was drawn before those.
        instance.renderPassEvent = camera.cameraType == CameraType.Game && BackgroundBlurUI.AllEnabledOnScreenSpaceOverlayCanvases()
            ? RenderPassEvent.AfterRenderingPostProcessing
            : RenderPassEvent.BeforeRenderingTransparents;
        renderer.EnqueuePass(instance);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        var resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer) return;

        var source = resourceData.activeColorTexture;
        var desc = renderGraph.GetTextureDesc(source);
        desc.name = "_BackgroundBlurUITexture";
        desc.msaaSamples = MSAASamples.None;
        desc.bindTextureMS = false;
        desc.useMipMap = true;
        desc.autoGenerateMips = false;
        desc.filterMode = FilterMode.Trilinear;
        desc.wrapMode = TextureWrapMode.Clamp;
        desc.clearBuffer = false;
        var destination = renderGraph.CreateTexture(desc);

        renderGraph.AddBlitPass(source, destination, Vector2.one, Vector2.zero, passName: "BackgroundBlurUI Copy");

        using var builder = renderGraph.AddUnsafePass<PassData>("BackgroundBlurUI Generate Mips", out var passData, profilingSampler);
        passData.texture = destination;
        builder.UseTexture(destination, AccessFlags.ReadWrite);
        builder.AllowPassCulling(false);
        builder.AllowGlobalStateModification(true);
        builder.SetGlobalTextureAfterPass(destination, TextureProperty);
        builder.SetRenderFunc(static (PassData data, UnsafeGraphContext context) => {
            RenderTexture texture = data.texture;
            context.cmd.GenerateMips(texture);
            context.cmd.SetGlobalVector(TexelSizeProperty, new Vector4(1f / texture.width, 1f / texture.height, texture.width, texture.height));
        });
    }
}
#endif
