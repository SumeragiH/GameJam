Shader "Hidden/GameJam/DarkenOutsideRegions"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "DarkenOutsideRegions"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #define MAX_REGIONS 32

            int _RegionCount;
            float _OutsideBrightness;
            int _EffectEnabled;
            float4 _RegionTypeData[MAX_REGIONS];
            float4 _RegionParamsA[MAX_REGIONS];
            float4 _RegionParamsB[MAX_REGIONS];

            float RegionCircleWeight(float2 uv, float2 center, float radius, float feather)
            {
                float edgeDistance = distance(uv, center) - radius;
                if (feather <= 0.00001)
                {
                    return edgeDistance <= 0 ? 1.0 : 0.0;
                }

                return 1.0 - saturate(smoothstep(0.0, feather, edgeDistance));
            }

            float RegionBoxWeight(float2 uv, float2 center, float2 halfSize, float cosR, float sinR, float feather, float skewTangent)
            {
                float2 delta = uv - center;
                float2 local = float2(
                    cosR * delta.x + sinR * delta.y,
                    -sinR * delta.x + cosR * delta.y
                );
                local.x -= skewTangent * local.y;
                float2 d = abs(local) - halfSize;
                float outside = max(d.x, d.y);

                if (feather <= 0.00001)
                {
                    return outside <= 0 ? 1.0 : 0.0;
                }

                return 1.0 - saturate(smoothstep(0.0, feather, outside));
            }

            float RegionSectorWeight(float2 uv, float2 center, float rangeRadius, float halfAngleRadians, float dirCos, float dirSin, float feather)
            {
                float2 delta = uv - center;
                float dist = length(delta);
                float rangeOutside = dist - rangeRadius;

                float2 direction = dist > 0.00001 ? (delta / dist) : float2(dirCos, dirSin);
                float cosHalfAngle = cos(max(halfAngleRadians, 0.00001));
                float cosToCenterDirection = dot(direction, float2(dirCos, dirSin));
                float angleOutside = cosHalfAngle - cosToCenterDirection;

                float outside = max(rangeOutside, angleOutside);
                if (feather <= 0.00001)
                {
                    return outside <= 0 ? 1.0 : 0.0;
                }

                return 1.0 - saturate(smoothstep(0.0, feather, outside));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float2 correctedUv = float2(uv.x * aspect, uv.y);
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                if (_EffectEnabled == 0)
                {
                    return color;
                }

                float insideWeight = 0.0;
                int regionCount = clamp(_RegionCount, 0, MAX_REGIONS);
                for (int i = 0; i < regionCount; i++)
                {
                    float shapeType = _RegionTypeData[i].x;
                    float2 center = _RegionParamsA[i].xy;
                    float2 size = _RegionParamsA[i].zw;
                    float cosR = _RegionParamsB[i].x;
                    float sinR = _RegionParamsB[i].y;
                    float feather = _RegionParamsB[i].z;
                    float skewTangent = _RegionParamsB[i].w;

                    float weight;
                    if (shapeType < 0.5)
                    {
                        weight = RegionCircleWeight(correctedUv, center, size.x, feather);
                    }
                    else if (shapeType < 1.5)
                    {
                        weight = RegionBoxWeight(correctedUv, center, size, cosR, sinR, feather, skewTangent);
                    }
                    else
                    {
                        weight = RegionSectorWeight(correctedUv, center, size.x, size.y, cosR, sinR, feather);
                    }

                    insideWeight = max(insideWeight, weight);
                }

                float brightness = lerp(_OutsideBrightness, 1.0, insideWeight);
                color.rgb *= brightness;
                return color;
            }
            ENDHLSL
        }
    }
}
