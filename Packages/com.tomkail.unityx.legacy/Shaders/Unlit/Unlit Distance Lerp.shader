Shader "UnityX/Unlit/Distance Lerp" {
	Properties {
		_MainTex ("Base (RGB) Trans. (Alpha)", 2D) = "white" { }
		_Color ("Main Color", Color) = (1,1,1,1)
		_MinColor ("Color in Minimal", Color) = (1, 1, 1, 1)
		_MaxColor ("Color in Maxmal", Color) = (0, 0, 0, 0)
		_MinDistance ("Min Distance", Float) = 100
		_MaxDistance ("Max Distance", Float) = 1000
	}

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