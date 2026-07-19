Shader "TrippyTrip/TripScreenEffect"
{
    Properties
    {
        _Intensity ("Intensity", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "TripScreenEffect"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float4 _Drug_TintColor;
            float _Drug_SwirlStrength;
            float _Drug_NoiseStrength;
            float3 _My_Global_Curve;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 78.233);
                return frac(p.x * p.y);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = Hash(i);
                float b = Hash(i + float2(1.0, 0.0));
                float c = Hash(i + float2(0.0, 1.0));
                float d = Hash(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 centered = uv - 0.5;

                float curveAmount = length(_My_Global_Curve.xy) * 0.02;
                float swirl = _Drug_SwirlStrength;
                float angle = (length(centered) * swirl + _TimeParameters.y * (0.5 + abs(_My_Global_Curve.z))) * 2.0;

                float s = sin(angle);
                float c = cos(angle);
                float2 rotated = float2(
                    centered.x * c - centered.y * s,
                    centered.x * s + centered.y * c
                );

                float2 swirlUv = rotated + 0.5;
                swirlUv += normalize(centered + 0.0001) * curveAmount * _My_Global_Curve.z;

                float noise = Noise(uv * 18.0 + _TimeParameters.y * 4.0);
                float noiseOffset = (noise - 0.5) * 0.01 * _Drug_NoiseStrength;
                swirlUv += noiseOffset;

                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, swirlUv);
                half4 tinted = sceneColor * _Drug_TintColor;

                float noiseMix = saturate(_Drug_NoiseStrength);
                tinted.rgb = lerp(tinted.rgb, tinted.rgb + (noise - 0.5) * noiseMix, noiseMix);

                return tinted;
            }
            ENDHLSL
        }
    }
}
