Shader "Parallax/Glow Sprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 4)) = 1
        _Blur ("Blur (texels)", Range(0, 6)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+10"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        // Aditivo: a luz soma ao que já está desenhado e nunca escurece nada.
        Blend SrcAlpha One

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _Intensity;
                float _Blur;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 SampleBlurred(float2 uv)
            {
                if (_Blur < 0.01)
                    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // Desfoque de 9 pontos com pesos de um filtro gaussiano pequeno.
                // A cor é ponderada pelo alpha, para o contorno não escurecer ao misturar com o transparente.
                float2 step = _MainTex_TexelSize.xy * _Blur;
                float2 offsets[9] = {
                    float2(0, 0), float2(step.x, 0), float2(-step.x, 0), float2(0, step.y), float2(0, -step.y),
                    step, -step, float2(step.x, -step.y), float2(-step.x, step.y)
                };
                float weights[9] = { 0.2270, 0.1135, 0.1135, 0.1135, 0.1135, 0.0680, 0.0680, 0.0680, 0.0680 };

                half3 color = 0;
                half alpha = 0;
                [unroll] for (int i = 0; i < 9; i++)
                {
                    half4 tap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsets[i]);
                    color += tap.rgb * tap.a * weights[i];
                    alpha += tap.a * weights[i];
                }

                return half4(color / max(alpha, 0.0001h), alpha);
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 color = SampleBlurred(input.uv) * input.color;
                color.rgb *= _Intensity;
                return color;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
