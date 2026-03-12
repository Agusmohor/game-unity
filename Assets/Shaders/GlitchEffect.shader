Shader "Custom/URP/VHSGlitch"
{
    Properties
    {
        _Intensity("Intensity", Range(0, 1)) = 0.85
        _ScanlineStrength("Scanline Strength", Range(0, 2)) = 0.35
        _JitterAmount("Horizontal Jitter", Range(0, 0.1)) = 0.012
        _DriftSpeed("Tracking Drift Speed", Range(0, 8)) = 1.5
        _RollSpeed("Roll Speed", Range(0, 4)) = 0.65
        _RollInterval("Roll Interval", Range(0.5, 8)) = 2.4
        _RollChance("Roll Chance", Range(0, 1)) = 0.45
        _RollWidth("Roll Width", Range(0.01, 0.4)) = 0.08
        _RollDistortion("Roll Distortion", Range(0, 0.2)) = 0.045
        _ColorBleed("RGB Split", Range(0, 0.03)) = 0.003
        _NoiseAmount("Noise Amount", Range(0, 1)) = 0.12
        _StaticBandStrength("Static Band Strength", Range(0, 1)) = 0.22
        _BottomStaticStrength("Bottom Static Strength", Range(0, 2)) = 0.9
        _FlickerStrength("Flicker Strength", Range(0, 1)) = 0.05
        _VignetteStrength("Vignette Strength", Range(0, 1)) = 0.18
        _Tint("Tint", Color) = (0.92, 1.0, 0.96, 1.0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "VHSGlitch"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);

            CBUFFER_START(UnityPerMaterial)
                float _Intensity;
                float _ScanlineStrength;
                float _JitterAmount;
                float _DriftSpeed;
                float _RollSpeed;
                float _RollInterval;
                float _RollChance;
                float _RollWidth;
                float _RollDistortion;
                float _ColorBleed;
                float _NoiseAmount;
                float _StaticBandStrength;
                float _BottomStaticStrength;
                float _FlickerStrength;
                float _VignetteStrength;
                float4 _Tint;
            CBUFFER_END

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionHCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float3 SampleSource(float2 uv, float colorOffset)
            {
                // Small RGB separation to simulate chroma misalignment.
                float r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(colorOffset, 0)).r;
                float g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).g;
                float b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(colorOffset, 0)).b;
                return float3(r, g, b);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;
                float time = _Time.y * _DriftSpeed;
                float intensity = saturate(_Intensity);
                float rawTime = _Time.y;

                // Bias the effect toward the top and bottom borders, with only
                // occasional interference bands in the center of the screen.
                float edgeMaskTop = 1.0 - smoothstep(0.0, 0.22, uv.y);
                float edgeMaskBottom = smoothstep(0.78, 1.0, uv.y);
                float edgeMask = saturate(edgeMaskTop + edgeMaskBottom);

                float midEvent = step(0.82, Hash21(float2(floor(time * 0.7), 4.17)));
                float midCenter = 0.35 + Hash21(float2(floor(time * 0.37), 9.13)) * 0.3;
                float midBand = smoothstep(0.12, 0.0, abs(uv.y - midCenter)) * midEvent;

                float effectMask = saturate(edgeMask * 0.95 + midBand * 0.65);

                float sliceIndex = floor(uv.y * 90.0 + floor(time * 5.0));
                float sliceNoise = Hash21(float2(sliceIndex, floor(time * 3.0)));
                float sliceMask = step(0.72, sliceNoise);
                float sliceShift = (Hash21(float2(sliceIndex * 1.37, floor(time * 7.0))) - 0.5) * _JitterAmount * 7.0;

                float thinSliceIndex = floor(uv.y * 220.0 + floor(time * 11.0));
                float thinSliceNoise = Hash21(float2(thinSliceIndex * 0.73, floor(time * 9.0)));
                float thinSliceMask = step(0.88, thinSliceNoise);
                float thinSliceShift = (Hash21(float2(thinSliceIndex * 2.11, floor(time * 13.0))) - 0.5) * _JitterAmount * 12.0;

                float trackingCenter = frac(time * 0.23);
                float trackingBand = smoothstep(0.12, 0.0, abs(uv.y - trackingCenter));
                float trackingShift = sin(uv.y * 320.0 + time * 18.0) * _JitterAmount * 4.0 * trackingBand;

                float rollInterval = max(_RollInterval, 0.001);
                float rollCycle = floor(rawTime / rollInterval);
                float rollCycleTime = frac(rawTime / rollInterval);
                float rollSeed = Hash21(float2(rollCycle, 19.37));
                float rollTrigger = step(1.0 - _RollChance, rollSeed);
                float rollDuration = lerp(0.24, 0.52, Hash21(float2(rollCycle, 7.11)));
                float rollProgress = saturate(rollCycleTime / max(rollDuration, 0.001));
                float rollActive = rollTrigger * step(rollCycleTime, rollDuration);
                float rollCenter = 1.0 - saturate(rollProgress * _RollSpeed);
                float rollDistance = abs(uv.y - rollCenter);
                float rollBand = smoothstep(_RollWidth, 0.0, rollDistance) * rollActive;
                float rollPhase = (uv.y - rollCenter) / max(_RollWidth, 0.0001);
                float rollWave = sin(rollPhase * 18.0 + rawTime * 28.0) * _RollDistortion;
                float rollSkew = sin(uv.y * 90.0 + rawTime * 8.0) * _JitterAmount * 1.8;

                float glitchShift = (sliceMask * sliceShift) + (thinSliceMask * thinSliceShift) + trackingShift;
                glitchShift += (rollWave + rollSkew) * rollBand;
                uv.x += glitchShift * intensity * effectMask;
                uv = saturate(uv);

                float harshBand = saturate(sliceMask + thinSliceMask + trackingBand + rollBand * 1.2) * effectMask;
                float bleed = _ColorBleed * (0.5 + harshBand + rollBand * 0.8) * intensity;
                float3 color = SampleSource(uv, bleed);

                float scan = sin((uv.y + time * 0.05) * _ScreenParams.y * 1.35);
                float scanlines = 1.0 - ((scan * 0.5 + 0.5) * _ScanlineStrength * 0.24 * intensity);
                float denseScan = sin((uv.y + rawTime * 0.021) * _ScreenParams.y * 2.6 + sin(rawTime * 3.5) * 4.0);
                float denseScanlines = 1.0 - ((denseScan * 0.5 + 0.5) * _ScanlineStrength * 0.1 * intensity);

                // Animated grain and static noise.
                float grain = Noise(uv * _ScreenParams.xy * 0.27 + time * 24.0) - 0.5;
                float staticNoise = Noise(float2(uv.x * _ScreenParams.x * 0.08, uv.y * _ScreenParams.y * 1.8 - time * 18.0)) - 0.5;
                float tearNoise = (Hash21(float2(sliceIndex, floor(time * 15.0))) - 0.5) * sliceMask;
                float snow = Hash21(floor(uv * _ScreenParams.xy * float2(1.0, 0.6) + rawTime * 60.0)) - 0.5;
                float bottomMask = smoothstep(0.72, 1.0, uv.y);
                float bottomStatic = (staticNoise * 0.8 + snow * 1.25) * bottomMask * _BottomStaticStrength;
                color += grain * _NoiseAmount * 0.14 * intensity;
                color += staticNoise * harshBand * _StaticBandStrength * 0.2 * intensity;
                color += tearNoise * (_StaticBandStrength + rollBand * 0.5) * 0.24 * intensity;
                color += bottomStatic * 0.22 * intensity;

                float rollLumaDrop = lerp(1.0, 0.72 + sin(rollPhase * 7.0 + rawTime * 35.0) * 0.08, rollBand);
                color *= rollLumaDrop;

                // Occasional darker dropout lines inside thin tearing bands.
                float dropoutMask = step(0.94, Hash21(float2(thinSliceIndex * 3.1, floor(time * 17.0)))) * thinSliceMask;
                color *= 1.0 - dropoutMask * 0.22 * intensity * effectMask;

                // Analog luminance instability.
                float flicker = 1.0 - (_FlickerStrength * intensity * effectMask * (0.5 + 0.5 * sin(_Time.y * 16.0 + uv.y * 12.0)));

                // Slight desaturation helps the horror/VHS look feel older.
                float luma = dot(color, float3(0.299, 0.587, 0.114));
                color = lerp(float3(luma, luma, luma), color, 0.7);
                color = lerp(color, color * scanlines, 0.55);
                color *= denseScanlines;
                color *= flicker;
                color *= _Tint.rgb;
                color *= lerp(0.92, 0.68, bottomMask * 0.85);

                // Soft lens darkening on the frame edges.
                float2 centered = input.uv * 2.0 - 1.0;
                float vignette = 1.0 - dot(centered, centered) * _VignetteStrength * 0.52;
                color *= saturate(vignette);

                return half4(saturate(color), 1.0);
            }
            ENDHLSL
        }
    }
}
