// URP versions of the passes the built-in Unlit surface shaders got from Unity: shadow casting (from their
// "Diffuse" fallback) and the depth prepasses. None of them sample the texture, so they work with any of the
// Unlit shaders' material properties. Include after declaring the shader's UnityPerMaterial cbuffer.
#ifndef UNITYX_UNLIT_URP_INCLUDED
#define UNITYX_UNLIT_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct UnlitURPDepthAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct UnlitURPDepthVaryings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS   : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#if defined(UNITYX_UNLIT_URP_SHADOW_CASTER)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

float3 _LightDirection;
float3 _LightPosition;

UnlitURPDepthVaryings ShadowCasterVertex(UnlitURPDepthAttributes input)
{
    UnlitURPDepthVaryings output = (UnlitURPDepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
    #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
        float3 lightDirectionWS = normalize(_LightPosition - positionWS);
    #else
        float3 lightDirectionWS = _LightDirection;
    #endif
    output.positionCS = ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS)));
    return output;
}

half4 ShadowCasterFragment(UnlitURPDepthVaryings input) : SV_Target
{
    return 0;
}
#endif

UnlitURPDepthVaryings DepthVertex(UnlitURPDepthAttributes input)
{
    UnlitURPDepthVaryings output = (UnlitURPDepthVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    return output;
}

half DepthOnlyFragment(UnlitURPDepthVaryings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    return input.positionCS.z;
}

half4 DepthNormalsFragment(UnlitURPDepthVaryings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    #if defined(_GBUFFER_NORMALS_OCT)
        float2 octNormalWS = PackNormalOctQuadEncode(normalize(input.normalWS));
        return half4(PackFloat2To888(saturate(octNormalWS * 0.5 + 0.5)), 0.0);
    #else
        return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
    #endif
}

#endif
