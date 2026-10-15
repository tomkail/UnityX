Shader "UnityX/Unlit/Unlit Alpha" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Tint Color", Color) = (1,1,1,1)
	}

	// URP. Surface shaders don't run there, so this does by hand what the built-in subshader below gets from Unity.
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			half4 _Color;
		CBUFFER_END
		ENDHLSL

		Pass {
			Name "Unlit"
			Tags { "LightMode"="UniversalForwardOnly" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			struct Attributes {
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings {
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
				float fogFactor : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert (Attributes input) {
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				output.color = input.color;
				output.fogFactor = ComputeFogFactor(output.positionCS.z);
				return output;
			}

			half4 frag (Varyings input) : SV_Target {
				half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color * _Color;
				return half4(MixFog(c.rgb, input.fogFactor), c.a);
			}
			ENDHLSL
		}

		Pass {
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }
			ZWrite On
			ZTest LEqual
			ColorMask 0

			HLSLPROGRAM
			#pragma vertex ShadowCasterVertex
			#pragma fragment ShadowCasterFragment
			#pragma multi_compile_instancing
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
			#define UNITYX_UNLIT_URP_SHADOW_CASTER
			#include "UnlitURP.hlsl"
			ENDHLSL
		}
	}

	// Built-in render pipeline.
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        CGPROGRAM
		#pragma surface surf Unlit alpha noambient

        sampler2D _MainTex;
     	fixed4 _Color;

        #include "Unlit.cginc"

        struct Input {
            float2 uv_MainTex;
            float4 color: Color; // Vertex color
        };

        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = IN.color.rgb * c.rgb * _Color.rgb;
			o.Alpha = IN.color.a * c.a * _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
