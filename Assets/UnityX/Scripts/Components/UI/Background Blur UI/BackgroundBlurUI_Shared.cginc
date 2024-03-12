#define PI 3.14159265
#ifdef MEDIUM_KERNEL
    #define KERNEL_SIZE 35
#elif BIG_KERNEL
    #define KERNEL_SIZE 127
#else //LITTLE_KERNEL
    #define KERNEL_SIZE 7
#endif

#include "UnityCG.cginc"

struct appdata_t
{
    float4 vertex   : POSITION;
    float4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex   : SV_POSITION;
    fixed4 color    : COLOR;
    float2 texcoord  : TEXCOORD0;
    float4 worldPosition : TEXCOORD1;
    float4  mask : TEXCOORD2;
    UNITY_VERTEX_OUTPUT_STEREO

    float4 uvgrab : TEXCOORD3;
};

v2f vert(appdata_t v)
{
    v2f OUT;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
    float4 vPosition = UnityObjectToClipPos(v.vertex);
    OUT.worldPosition = v.vertex;
    OUT.vertex = vPosition;

    float2 pixelSize = vPosition.w;
    pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

    float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
    float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
    OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
    OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));


    if (_UIVertexColorAlwaysGammaSpace)
    {
        if(!IsGammaSpace())
        {
            v.color.rgb = UIGammaToLinear(v.color.rgb);
        }
    }

    OUT.color = v.color * _Color;
    
    #if UNITY_UV_STARTS_AT_TOP
    float scale = -1.0;
    #else
    float scale = 1.0;
    #endif
    OUT.uvgrab.xy = (float2(OUT.vertex.x, OUT.vertex.y*scale) + OUT.vertex.w) * 0.5;
    OUT.uvgrab.zw = OUT.vertex.zw;
    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
    
    return OUT;
}

half4 GrabPixel(sampler2D tex, float4 uv)
{
    half4 pixel = tex2Dproj(tex, UNITY_PROJ_COORD(uv));
    return half4(pixel.rgb, 1);
}

half4 GrabPixelXY(sampler2D tex, float4 uv, float4 texelSize, half kernelx, half kernely)
{
    half4 pixel = tex2Dproj(
        tex,
        UNITY_PROJ_COORD(
            float4(
                uv.x + texelSize.x * kernelx,
                uv.y + texelSize.y * kernely,
                uv.z,
                uv.w)
        )
    );
    return half4(pixel.rgb, 1);
}

struct pixel_info
{
    sampler2D tex;
    float2 uv;
    float4 texelSize;
};

float gauss(float x, float sigma)
{
    return  1.0f / (2.0f * PI * sigma * sigma) * exp(-(x * x) / (2.0f * sigma * sigma));
}

float gauss(float x, float y, float sigma)
{
    return  1.0f / (2.0f * PI * sigma * sigma) * exp(-(x * x + y * y) / (2.0f * sigma * sigma));
}

float4 GaussianBlur(pixel_info pinfo, float sigma, float2 dir)
{
    float4 o = 0;
    float sum = 0;
    float2 uvOffset;
    float weight;
	
    for(int kernelStep = - KERNEL_SIZE / 2; kernelStep <= KERNEL_SIZE / 2; ++kernelStep)
    {
        uvOffset = pinfo.uv;
        uvOffset.x += ((kernelStep) * pinfo.texelSize.x) * dir.x;
        uvOffset.y += ((kernelStep) * pinfo.texelSize.y) * dir.y;
        // uvOffset.x = saturate(uvOffset.x);
        // uvOffset.y = saturate(uvOffset.y);
        weight = gauss(kernelStep, sigma) + gauss(kernelStep+1, sigma);
        // float4 sum = GrabPixel(tex, pinfo.uv);
        o += tex2D(pinfo.tex, uvOffset) * weight;
        sum += weight;
    }
    o *= (1.0f / sum);
    return o;
}