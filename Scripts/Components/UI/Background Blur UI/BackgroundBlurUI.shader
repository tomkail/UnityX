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
	    ENDCG
        
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
                #pragma multi_compile __ UNITY_UI_ALPHACLIP
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
                #pragma multi_compile __ UNITY_UI_ALPHACLIP
                ENDCG
            }
        }
    }
    Fallback "UI/Default"
}
