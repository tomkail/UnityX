Shader "TextureProcessor/ApplyImageOrientation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Orientation ("Orientation", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _Orientation;

            v2f vert(appdata v){
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Adjust UVs based on the orientation
                float2 uv = v.uv;
                switch (_Orientation)
                {
                   case 0: // Normal
                       break;
                   case 1: // Rotate90
                       uv = float2(1.0 - uv.y, uv.x);
                       break;
                   case 2: // Rotate180
                       uv = float2(1.0 - uv.x, 1.0 - uv.y);
                       break;
                   case 3: // Rotate270
                       uv = float2(uv.y, 1.0 - uv.x);
                       break;
                   case 4: // FlipHorizontal
                       uv = float2(1.0 - uv.x, uv.y);
                       break;
                   case 5: // Transpose
                       uv = float2(1.0 - uv.y, 1.0 - uv.x);
                       break;
                   case 6: // FlipVertical
                       uv = float2(uv.x, 1.0 - uv.y);
                       break;
                   case 7: // Transverse
                       uv = float2(uv.y, uv.x);
                       break;
                   default: // Unknown or any other value
                       break;
                }
                o.uv = TRANSFORM_TEX(uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}