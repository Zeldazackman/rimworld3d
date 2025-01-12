Shader "Custom/PospoHdr"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Exposure ("Exposure", Range(0, 10)) = 1
        _Contrast ("Contrast", Range(0, 3)) = 1
        _Saturation ("Saturation", Range(0, 3)) = 1
        _Bloom ("Bloom", Range(0, 2)) = 0.5
        _BloomThreshold ("Bloom Threshold", Range(0, 2)) = 0.5
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
            float _Exposure;
            float _Contrast;
            float _Saturation;
            float _Bloom;
            float _BloomThreshold;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Invertimos las coordenadas UV en Y
                o.uv = float2(v.uv.x, 1.0 - v.uv.y);
                o.uv = TRANSFORM_TEX(o.uv, _MainTex);
                return o;
            }
            
            float3 ACESFilm(float3 x)
            {
                float a = 2.51f;
                float b = 0.03f;
                float c = 2.43f;
                float d = 0.59f;
                float e = 0.14f;
                return saturate((x*(a*x+b))/(x*(c*x+d)+e));
            }

            float3 Saturation(float3 color, float adjustment)
            {
                float luminance = dot(color, float3(0.2126, 0.7152, 0.0722));
                return lerp(float3(luminance, luminance, luminance), color, adjustment);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 col = tex2D(_MainTex, i.uv).rgb;
                
                col *= _Exposure;
                
                col = pow(col, _Contrast);
                
                float brightness = dot(col, float3(0.2126, 0.7152, 0.0722));
                float3 bloomColor = col * step(_BloomThreshold, brightness);
                col += bloomColor * _Bloom;
                
                col = Saturation(col, _Saturation);
                
                col = ACESFilm(col);
                
                return float4(col, 1);
            }
            ENDCG
        }
    }
}