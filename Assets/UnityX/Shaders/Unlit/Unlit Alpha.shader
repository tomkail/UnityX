Shader "UnityX/Unlit/Unlit Alpha" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Tint Color", Color) = (1,1,1,1)
	}
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
 