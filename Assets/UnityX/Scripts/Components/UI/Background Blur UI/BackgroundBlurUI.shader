Shader "Hidden/BackgroundBlurUI"
{

    Properties
    {
        // Internally enforced by MAX_RADIUS
        _BlurRadius("Blur Radius", Range(0, 64)) = 1
        _BlurStrength("Blur Strength", Range(0, 1)) = 1
        _BlurSigma("Blur Sigma", Range(0, 1)) = 1
        
        // see Stencil in UI/Default
        [HideInInspector][PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
        [HideInInspector]_UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
        #include "UnityCG.cginc"
        #pragma multi_compile LITTLE_KERNEL MEDIUM_KERNEL BIG_KERNEL
        #include "BackgroundBlurUI_Shared.cginc"
        
        uniform sampler2D _GrabTexture;
        uniform float4 _GrabTexture_TexelSize;
        uniform float _BlurSigma;
        
        half4 frag_horizontal(v2f i) : COLOR
        {
            if(i.color.a == 0) discard;
            float2 dir = float2(1, 0);
            pixel_info pinfo;
            pinfo.tex = _GrabTexture;
            pinfo.uv = i.uvgrab;
            pinfo.texelSize = _GrabTexture_TexelSize;
            if(_BlurSigma == 0) return tex2D(pinfo.tex, pinfo.uv);
            half4 blurred = GaussianBlur(pinfo, _BlurSigma, dir);
            half4 pixel_raw = tex2D(_MainTex, i.uvmain);
            return half4(blurred.rgb, blurred.a*pixel_raw.a);
        }
        
        half4 frag_vertical(v2f i) : COLOR
        {
            if(i.color.a == 0) discard;
            float2 dir = float2(0, 1);
            pixel_info pinfo;
            pinfo.tex = _GrabTexture;
            pinfo.uv = i.uvgrab;
            pinfo.texelSize = _GrabTexture_TexelSize;
            if(_BlurSigma == 0) return tex2D(pinfo.tex, pinfo.uv);
            half4 blurred = GaussianBlur(pinfo, _BlurSigma, dir);
            half4 pixel_raw = tex2D(_MainTex, i.uvmain);
            return half4(blurred.rgb, blurred.a*pixel_raw.a);
        }
	    ENDCG
        
        SubShader
        {

            GrabPass {}

            Pass
            {
                Name "UIBlur_Y"
                Tags{ "LightMode" = "Always" }

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag_vertical
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile __ IS_BLUR_ALPHA_MASKED
                #pragma multi_compile __ IS_SPRITE_VISIBLE
                #pragma multi_compile __ UNITY_UI_ALPHACLIP
            ENDCG
            }

            GrabPass {}
            
            Pass
            {
                Name "UIBlur_X"
                Tags{ "LightMode" = "Always" }


            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag_horizontal
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile __ IS_BLUR_ALPHA_MASKED
                #pragma multi_compile __ IS_SPRITE_VISIBLE
                #pragma multi_compile __ UNITY_UI_ALPHACLIP
				#pragma shader_feature MAKE_DESATURATED
            ENDCG
            }

        }
    }
    Fallback "UI/Default"
}
