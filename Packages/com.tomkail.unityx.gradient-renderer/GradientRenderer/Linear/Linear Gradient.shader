Shader "Linear Gradient"
{
    Properties
    {
        [PerRendererData]_StartColor ("Start Color", Color) = (1,1,1,1)
        [PerRendererData]_EndColor ("End Color", Color) = (1,1,1,0)
        [PerRendererData]_StartDistance ("Start Distance", Float) = 0
        [PerRendererData]_EndDistance ("End Distance", Float) = 1
        [PerRendererData]_OffsetX ("OffsetX", Float) = 0
        [PerRendererData]_OffsetY ("OffsetY", Float) = 0
        [PerRendererData]_Degrees ("Degrees", float) = 0
        [PerRendererData]_Power ("Power", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        // ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _StartColor)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _EndColor)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _StartDistance)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _EndDistance)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _OffsetX)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _OffsetY)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _Power)
            UNITY_DEFINE_INSTANCED_PROP(fixed, _Degrees)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float InvLerp (float a, float b, float v) {
                return (v-a)/(b-a);
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 startColor = UNITY_ACCESS_INSTANCED_PROP(Props, _StartColor);
                fixed4 endColor = UNITY_ACCESS_INSTANCED_PROP(Props, _EndColor);
                fixed startDistance = UNITY_ACCESS_INSTANCED_PROP(Props, _StartDistance);
                fixed endDistance = UNITY_ACCESS_INSTANCED_PROP(Props, _EndDistance);
                fixed offsetX = UNITY_ACCESS_INSTANCED_PROP(Props, _OffsetX);
                fixed offsetY = UNITY_ACCESS_INSTANCED_PROP(Props, _OffsetY);
                fixed power = UNITY_ACCESS_INSTANCED_PROP(Props, _Power);
                fixed degrees = UNITY_ACCESS_INSTANCED_PROP(Props, _Degrees);
                
                float rads = radians(degrees);
                float2 dir = float2(sin(rads), cos(rads));
                float d = dot(i.uv-0.5 + float2(offsetX, offsetY), dir);
                float g = InvLerp(startDistance-0.5, endDistance-0.5, d);

                g = saturate(g);
                g = pow(g, power);
                float4 color = lerp(startColor, endColor, g);
                if(color.a <= 0) discard;
                return color;
            }
            ENDCG
        }
    }
}