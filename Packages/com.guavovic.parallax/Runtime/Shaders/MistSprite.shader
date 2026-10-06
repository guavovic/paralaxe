Shader "Parallax/Mist Sprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _WindSpeed ("Wind Speed", Float) = 1
        _WindStrength ("Wind Strength", Float) = 0
        _Blur ("Blur (texels)", Range(0, 6)) = 0
        _Breath ("Breath (0 static, 1 full)", Range(0, 1)) = 0.6
        _NoiseScale ("Noise Scale", Float) = 5
        _Drift ("Drift (x, y per second)", Vector) = (0.04, 0.015, 0, 0)
        _Churn ("Churn Speed", Float) = 0.25
        _Wave ("Wave (texels)", Range(0, 6)) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        // Normal (SrcAlpha, OneMinusSrcAlpha) para névoa; aditivo (SrcAlpha, One) para raios e luz.
        Blend [_SrcBlend] [_DstBlend]

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
                float _WindSpeed;
                float _WindStrength;
                float _Blur;
                float _Breath;
                float _NoiseScale;
                float4 _Drift;
                float _Churn;
                float _Wave;
                float _SrcBlend;
                float _DstBlend;
            CBUFFER_END

            #include "ParallaxSprite.hlsl"

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

            static const float BayerMatrix[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

            // Bayer 4x4 no grid de texels: o que aparece e some são pixels inteiros, como o dithering da arte.
            float Bayer(float2 texel)
            {
                int2 p = int2(fmod(floor(texel), 4.0));
                return (BayerMatrix[p.y * 4 + p.x] + 0.5) / 16.0;
            }

            // Névoa que vive: duas camadas de ruído andando em direções diferentes viram uma máscara que forma e
            // desfaz bolsões; a máscara corta a arte em dithering, então a névoa "respira" sem virar borrão.
            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y;
                float2 texSize = _MainTex_TexelSize.zw;
                float wave = sin(input.uv.y * 18.0 + t * 0.9) * _Wave * _MainTex_TexelSize.x;
                float2 uv = input.uv + float2(wave, 0);

                half4 color = ParallaxSampleBlurred(uv) * input.color;

                // Ruído com período inteiro na horizontal, para a máscara casar na emenda do loop.
                float period = max(1.0, round(_NoiseScale * 2.0));
                float2 p = input.uv * float2(period, _NoiseScale);
                float a = ParallaxNoiseWrap(p + _Drift.xy * t * 10.0, period);
                float b = ParallaxNoiseWrap(p * 2.0 - _Drift.yx * t * 7.0 + t * _Churn, period * 2.0);
                float mask = saturate((a * 0.6 + b * 0.4 - 0.25) * 2.0);
                float keep = lerp(1.0, mask, _Breath);
                float visible = step(Bayer(input.uv * texSize), keep);
                color.a *= visible;
                return color;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
