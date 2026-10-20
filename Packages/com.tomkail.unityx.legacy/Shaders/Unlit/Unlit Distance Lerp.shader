Shader "UnityX/Unlit/Distance Lerp" {
	Properties {
		_MainTex ("Base (RGB) Trans. (Alpha)", 2D) = "white" { }
		_Color ("Main Color", Color) = (1,1,1,1)
		_MinColor ("Color in Minimal", Color) = (1, 1, 1, 1)
		_MaxColor ("Color in Maxmal", Color) = (0, 0, 0, 0)
		_MinDistance ("Min Distance", Float) = 100
		_MaxDistance ("Max Distance", Float) = 1000
	}

	// URP. Surface shaders don't run there, so this does by hand what the built-in subshader below gets from Unity.
	// That subshader's surface pass writes alpha 1 (surface shaders do unless told keepalpha), so it covers its
	// empty _Color pass completely; this one draws just the surface pass's result.
	SubShader {
		Tags { "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
		LOD 200

		Pass {
			Name "Unlit"
			Tags { "LightMode"="UniversalForwardOnly" }
			ZWrite On
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				half4 _Color;
				half4 _MinColor;
				half4 _MaxColor;
				float _MinDistance;
				float _MaxDistance;
			CBUFFER_END

			struct Attributes {
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings {
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 positionWS : TEXCOORD1;
				float fogFactor : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert (Attributes input) {
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
				output.positionCS = TransformWorldToHClip(output.positionWS);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				output.fogFactor = ComputeFogFactor(output.positionCS.z);
				return output;
			}

			half4 frag (Varyings input) : SV_Target {
				half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
				float dist = distance(_WorldSpaceCameraPos, input.positionWS);
				half weight = saturate( (dist - _MinDistance) / (_MaxDistance - _MinDistance) );
				half4 distanceColor = lerp(_MinColor, _MaxColor, weight);
				return half4(MixFog(c.rgb * distanceColor.rgb * c.a, input.fogFactor), 1);
			}
			ENDHLSL
		}
	}

	// Built-in render pipeline.
	SubShader {
		Lighting Off
		ZWrite On
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		Tags {"Queue" = "Transparent"}
		Color[_Color]
		Pass {

		}

		LOD 200

		CGPROGRAM
		#pragma surface surf NoLighting
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		float _MaxDistance;
		float _MinDistance;
		half4 _MinColor;
		half4 _MaxColor;

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			float dist = distance(_WorldSpaceCameraPos, IN.worldPos);
			half weight = saturate( (dist - _MinDistance) / (_MaxDistance - _MinDistance) );
			half4 distanceColor = lerp(_MinColor, _MaxColor, weight);

			o.Albedo = c.rgb * distanceColor.rgb * c.a;
			o.Alpha = c.a;
		}

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		ENDCG
	}
}
