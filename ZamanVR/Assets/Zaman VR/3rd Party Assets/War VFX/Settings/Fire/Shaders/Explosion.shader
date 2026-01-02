Shader "URP/ExplosionFlipbook"
{
    Properties
    {
        _Explosion_Tex("Explosion_Tex", 2D) = "white" {}
        [HideInInspector] _Explosion_Tex_ST("Explosion_Tex ST", Vector) = (1,1,0,0)
    }

    SubShader
    {
        // URP tags
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            // Core URP includes (provides _Time, matrix transforms, etc.)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ---- Per-material (SRP Batcher compatible) ----
            CBUFFER_START(UnityPerMaterial)
                float4 _Explosion_Tex_ST;
            CBUFFER_END

            // ---- Textures/Samplers ----
            TEXTURE2D(_Explosion_Tex);
            SAMPLER(sampler_Explosion_Tex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv0        : TEXCOORD0; // xy = UV0, z used by your shader as flipbook frame index (0..63)
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            // Helper: apply _ST tiling/offset like TRANSFORM_TEX
            float2 ApplyST(float2 uv, float4 st) { return uv * st.xy + st.zw; }

            Varyings vert (Attributes v)
            {
                Varyings o;

                // Transform object to clip space
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);

                // Base UV with material tiling/offset
                float2 baseUV = ApplyST(v.uv0.xy, _Explosion_Tex_ST);

                // ----- Flipbook UV Animation (matches your original) -----
                // 8x8 grid = 64 tiles
                const float cols = 8.0;
                const float rows = 8.0;
                const float total = cols * rows;

                // Speed term was 0.0 in your original (Time * 0). Kept for parity.
                float anim = _Time.y * 0.0;

                // Original stored frame index in uv0.z, scaled by 63 (0..63).
                float currentIndex = round(fmod(anim + (v.uv0.z * 63.0), total));
                currentIndex += (currentIndex < 0.0) ? total : 0.0;

                // Compute tile x/y (top-to-bottom Y as in your code)
                float tileX = round(fmod(currentIndex, cols));
                float tileY = round(fmod((currentIndex - tileX) / cols, rows));
                tileY = (rows - 1.0) - tileY; // invert Y to read tiles top->bottom

                float2 tileSize = float2(1.0 / cols, 1.0 / rows);
                float2 offset   = float2(tileX * tileSize.x, tileY * tileSize.y);

                // Final flipbook UV
                o.uv = baseUV * tileSize + offset;

                o.color = v.color;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Sample flipbook texture
                half4 tex = SAMPLE_TEXTURE2D(_Explosion_Tex, sampler_Explosion_Tex, i.uv);

                // Your Built-in surface shader wrote Emission=rgb and Alpha=tex.a * vertexColor.a
                half alpha = tex.a * i.color.a;

                // Keep RGB unlit; (optionally multiply by vertex color rgb if desired)
                return half4(tex.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
