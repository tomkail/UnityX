Shader "Hidden/BackgroundBlurUI"
{

    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    Category
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        CGINCLUDE		
        #pragma target 3.0
        
        #include "UnityCG.cginc"
        #include "UnityUI.cginc"

        #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
        #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
        
        
        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _TextureSampleAdd;
        float4 _ClipRect;
        float4 _MainTex_ST;
        float _UIMaskSoftnessX;
        float _UIMaskSoftnessY;
        int _UIVertexColorAlwaysGammaSpace;
        sampler2D _WeightTexture;
        uniform int _KernelSize;
        uniform float _Strength;
        uniform int _StepSize = 2;
                
        #include "BackgroundBlurUI_Shared.cginc"
        
        uniform sampler2D _GrabTexture;
        uniform float4 _GrabTexture_TexelSize;
              
        half4 frag_horizontal(v2f i) : COLOR
        {
            if(i.color.a == 0) discard;
            pixel_info pinfo;
            pinfo.tex = _GrabTexture;
            pinfo.uv = i.uvgrab;
            pinfo.texelSize = _GrabTexture_TexelSize;
            if(_KernelSize == 0) return tex2D(pinfo.tex, pinfo.uv);
            half4 blurred = GaussianBlur(pinfo, int2(1, 0), _KernelSize, _WeightTexture, _StepSize);
            return half4(blurred.rgb, 1);
        }
        
        half4 frag_vertical(v2f i) : COLOR
        {
            if(i.color.a == 0) discard;
            pixel_info pinfo;
            pinfo.tex = _GrabTexture;
            pinfo.uv = i.uvgrab;
            pinfo.texelSize = _GrabTexture_TexelSize;
            if(_KernelSize == 0) return tex2D(pinfo.tex, pinfo.uv);
            half4 blurred = GaussianBlur(pinfo, int2(0, 1), _KernelSize, _WeightTexture, _StepSize);
            half4 pixel_raw = tex2D(_MainTex, i.texcoord);
            return half4(blurred.rgb, 1);
        }

        // URP has no GrabPass. BackgroundBlurUI enqueues a render pass that copies the camera colour into this
        // mip-mapped texture, and this blurs it in a single pass: it reads the mip level where the blur's standard
        // deviation is small enough for a (2 * _BlurTapRadius + 1)^2 tap Gaussian to cover it.
        sampler2D _BackgroundBlurUITexture;
        float4 _BackgroundBlurUITexelSize;
        uniform float _BlurSigma;
        uniform float _BlurTapRadius;

        half4 frag_urp(v2f i) : SV_Target
        {
            if(i.color.a == 0) discard;
            float2 uv = i.screenPos.xy / i.screenPos.w;
            int tapRadius = max((int)_BlurTapRadius, 1);
            // Taps reach 2.5 standard deviations either side of the pixel at the chosen mip level.
            float lod = max(0, log2(_BlurSigma * 2.5 / tapRadius));
            float texelScale = exp2(lod);
            float sigma = max(_BlurSigma / texelScale, 1e-3);
            float2 tapStep = _BackgroundBlurUITexelSize.xy * texelScale;

            float4 colorSum = 0;
            float weightSum = 0;
            [loop] for (int y = -tapRadius; y <= tapRadius; y++)
            {
                [loop] for (int x = -tapRadius; x <= tapRadius; x++)
                {
                    float2 coord = uv + float2(x, y) * tapStep;
                    // Skip taps outside the screen. This avoids banding at the edges of the screen.
                    float inBounds = step(0.0, coord.x) * step(coord.x, 1.0) * step(0.0, coord.y) * step(coord.y, 1.0);
                    float weight = exp(-(x * x + y * y) / (2.0 * sigma * sigma)) * inBounds;
                    colorSum += tex2Dlod(_BackgroundBlurUITexture, float4(coord, 0, lod)) * weight;
                    weightSum += weight;
                }
            }
            return half4(colorSum.rgb / max(weightSum, 1e-5), 1);
        }
	    ENDCG

        // URP.
        SubShader
        {
            Tags { "RenderPipeline"="UniversalPipeline" }

            Pass
            {
                Name "UIBlur"

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag_urp
                ENDCG
            }
        }

        // Built-in render pipeline.
        SubShader
        {
            GrabPass {}

            Pass
            {
                Name "UIBlur_Y"

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag_vertical
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile __ IS_BLUR_ALPHA_MASKED
                #pragma multi_compile __ IS_SPRITE_VISIBLE
                ENDCG
            }

            GrabPass {}
            
            Pass
            {
                Name "UIBlur_X"

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag_horizontal
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile __ IS_BLUR_ALPHA_MASKED
                #pragma multi_compile __ IS_SPRITE_VISIBLE
                ENDCG
            }
        }
    }
    Fallback "UI/Default"
}
