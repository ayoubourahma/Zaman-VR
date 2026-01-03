Shader "URP/Water_VR_Simple"
{
    Properties
    {
        // Color
        _BaseColor       ("Base Color (RGB)", Color) = (0.06,0.22,0.20,1)
        _ReflectionTint  ("Reflection Tint", Color) = (0.25,0.35,0.45,1)

        // Transparency controls
        _Transparency    ("Transparency (0=opaque, 1=clear)", Range(0,1)) = 0.70
        _AlphaDeep       ("Alpha Deep (opacity)", Range(0,1)) = 0.55
        _AlphaShallow    ("Alpha Shallow (opacity)", Range(0,1)) = 0.18
        _ShoreFade       ("Depth Fade / Shore (m)", Range(0.05,8)) = 0.8

        // Waves
        _WaveHeight      ("Wave Height (m)", Range(0,1)) = 0.03
        _WaveFreq1       ("Wave Freq 1", Range(0.1,10)) = 3.0
        _WaveFreq2       ("Wave Freq 2", Range(0.1,10)) = 4.6
        _WaveSpeed1      ("Wave Speed 1", Range(0,5)) = 0.9
        _WaveSpeed2      ("Wave Speed 2", Range(0,5)) = 1.6
        _Tiling          ("World Tiling", Range(0.01,5)) = 0.9

        // Optics
        _RefractStrength ("Refraction Strength", Range(0,0.1)) = 0.015
        _FresnelPower    ("Fresnel Power", Range(0.5,8)) = 3.2

        // Reflection source
        _EnvCube         ("Environment Cubemap", Cube) = "" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _FOG_FRAGMENT

            // URP core includes XR helpers like UnityStereoTransformScreenSpaceTex
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Scene textures (URP Asset: Opaque Texture + Depth Texture must be enabled)
            TEXTURE2D_X(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D_X(_CameraDepthTexture);  SAMPLER(sampler_CameraDepthTexture);

            // Simple environment reflection (assign a cubemap or a baked reflection probe to a cubemap)
            TEXTURECUBE(_EnvCube);             SAMPLER(sampler_EnvCube);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _ReflectionTint;

                float _Transparency;
                float _AlphaDeep;
                float _AlphaShallow;
                float _ShoreFade;

                float _WaveHeight;
                float _WaveFreq1, _WaveFreq2;
                float _WaveSpeed1, _WaveSpeed2;
                float _Tiling;

                float _RefractStrength;
                float _FresnelPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Height(float2 xz, float t)
            {
                // Cheap, stable world-space ripples
                return
                    sin(xz.x * _WaveFreq1 + t * _WaveSpeed1) * _WaveHeight +
                    sin(xz.y * _WaveFreq2 + t * _WaveSpeed2) * (_WaveHeight * 0.6);
            }

            float3 WaterNormalFromHeight(float3 posWS)
            {
                // Finite difference gradient from the same height field
                float t = _Time.y;
                float2 w  = posWS.xz * _Tiling;
                float  h0 = Height(w, t);

                // eps in "height-space" — keep small to avoid noisy normals in VR
                float  eps = 0.05;
                float  hx = Height(w + float2(eps, 0), t);
                float  hz = Height(w + float2(0, eps), t);

                // Convert back to world-space step for cross product
                float3 dx = float3(eps / _Tiling, hx - h0, 0);
                float3 dz = float3(0, hz - h0, eps / _Tiling);

                return normalize(cross(dz, dx)); // points mostly up
            }

            float2 ScreenUV(float4 screenPos)
            {
                float2 uv = (screenPos.xy / screenPos.w);

                #if UNITY_UV_STARTS_AT_TOP
                    uv.y = 1.0 - uv.y;
                #endif

                // XR-safe transform (single-pass instanced)
                return UnityStereoTransformScreenSpaceTex(uv);
            }

            Varyings Vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);

                // Vertex displacement (needs enough mesh density)
                float t = _Time.y;
                float2 w = posWS.xz * _Tiling;
                posWS.y += Height(w, t);

                OUT.positionWS = posWS;
                OUT.positionCS = TransformWorldToHClip(posWS);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 Frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float3 N = WaterNormalFromHeight(IN.positionWS);
                float3 V = SafeNormalize(GetWorldSpaceViewDir(IN.positionWS));

                // Fresnel
                float fres = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                // Screen UVs
                float2 uvSS = ScreenUV(IN.screenPos);

                // Refraction from opaque texture (cheap, VR-friendly if subtle)
                float2 refrUV = uvSS + N.xz * _RefractStrength;
                float3 sceneCol = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, refrUV).rgb;

                // Depth fade: compare scene depth vs water surface depth (0 deep -> 1 shallow)
                float rawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uvSS).r;
                float scene01  = Linear01Depth(rawDepth, _ZBufferParams);
                float surf01   = Linear01Depth(IN.screenPos.z / IN.screenPos.w, _ZBufferParams);

                float diff01 = max(scene01 - surf01, 0.0);

                // This maps "meters-ish" to 0..1 in a controllable way
                float depthFade = saturate(diff01 / max(_ShoreFade * 0.1, 1e-4));

                // Environment reflection
                float3 R = reflect(-V, N);
                float3 env = SAMPLE_TEXTURECUBE(_EnvCube, sampler_EnvCube, R).rgb * _ReflectionTint.rgb;

                // Color: refracted scene + fresnel reflection + slight water tint
                float3 col = lerp(sceneCol, env, fres);
                col = lerp(col, _BaseColor.rgb, 0.25);

                // New transparency model:
                // depthFade: 0 (deep) -> 1 (shallow)
                float alphaDepth = lerp(_AlphaDeep, _AlphaShallow, depthFade);

                // _Transparency: 0=opaque, 1=clear => invert for alpha multiplier
                float alpha = saturate(alphaDepth * (1.0 - _Transparency));

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
