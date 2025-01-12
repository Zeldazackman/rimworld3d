Shader "Custom/PlantCutout3d" {
    Properties {
        _MainTex ("Main texture", 2D) = "white" {}
        _SwayHead ("Sway head", Float) = 0
        _FallColorDestination ("Fall Color Destination", 2D) = "white" {}
    }

    SubShader {
        Tags { 
            "IGNOREPROJECTOR" = "true" 
            "QUEUE" = "Transparent-100" 
            "RenderType" = "Transparent" 
        }

        Pass {
            Tags { 
                "IGNOREPROJECTOR" = "true" 
                "QUEUE" = "Transparent-100" 
                "RenderType" = "Transparent" 
            }
            
            Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 position : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _FallColorDestination;
            float _SwayHead;
            float3 _FallColorSource;
            float2 _FallTransitionRange;
            float _FallIntensity;
            int _FallBehaviorEnabled;
            int _FallGlobalControls;
            float3 _FallSrc;
            float3 _FallDst;
            float2 _FallRange;

            v2f vert(appdata v) {
                v2f o;
                
                float2 windOffset = v.vertex.xz * 4.0;
                windOffset += _SwayHead * 0.15;
                windOffset = sin(windOffset) * 0.1;
                
                float3 vertexOffset = float3(windOffset.x, 0.0, windOffset.y);
                float3 vertexPos = v.vertex.xyz + vertexOffset * v.color.w;
                
                o.position = UnityObjectToClipPos(float4(vertexPos, 1.0));
                
                float fixedZ = round(v.texcoord.z * 1000.0) / 1000.0;
                o.texcoord = float3(v.texcoord.xy, fixedZ);
                o.color = v.color;
                
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                float4 texColor = tex2D(_MainTex, i.texcoord.xy);
                
                if (texColor.a < 0.35) {
                    discard;
                }
                
                if (!_FallBehaviorEnabled) {
                    return texColor;
                }

                float maxChannel = max(texColor.r, max(texColor.g, texColor.b));
                float minChannel = min(texColor.r, min(texColor.g, texColor.b));
                float delta = maxChannel - minChannel;

                if (!(texColor.g > texColor.r && texColor.g > texColor.b && delta > 0.1)) {
                    return texColor;
                }

                float fixedZ = round(i.texcoord.z * 1000.0) / 1000.0;
                
                float noiseA = frac(sin(fixedZ * 3.0 + 5.0) * 43758.55);
                float noiseB = frac(sin(fixedZ * 2.0 + 1.0) * 43758.55);
                float noiseC = frac(sin(fixedZ * 4.0 - 3.0) * 43758.55);

                float3 targetColor;
                if (_FallGlobalControls) {
                    targetColor = _FallDst;
                } else {
                    float4 fallDestColor = tex2D(_FallColorDestination, float2(noiseA, 0.5));
                    targetColor = fallDestColor.rgb;
                }

                float2 transitionRange = _FallGlobalControls ? _FallRange : _FallTransitionRange;
                float t = (delta - transitionRange.x) / (transitionRange.y - transitionRange.x);
                float transition = saturate(t);
                transition = transition * transition * (3.0 - 2.0 * transition);
                
                float intensity = _FallIntensity * 0.47;
                float finalFactor = intensity * transition;

                float3 finalColor = lerp(texColor.rgb, targetColor, finalFactor);
                return float4(finalColor, texColor.a);
            }
            ENDCG
        }
    }
}