Shader "Parallax/Ripple Sprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _WindSpeed ("Wind Speed", Float) = 1
        _WindStrength ("Wind Strength", Float) = 0.5
        _Blur ("Blur (texels)", Range(0, 6)) = 0
        _RippleStrength ("Ripple Strength (texels)", Range(0, 8)) = 1.5
        _RippleSpeed ("Ripple Speed", Float) = 1.2
        _RippleScale ("Ripple Scale", Float) = 24
        _Shimmer ("Shimmer", Range(0, 1)) = 0.15
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
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

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
                float _RippleStrength;
                float _RippleSpeed;
                float _RippleScale;
                float _Shimmer;
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

            // Ondulação para névoa, água e ar quente: as linhas deslizam para os lados em ondas,
            // com um pouco de ruído para não parecer uma senoide perfeita, e o brilho varia junto.
            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y * _RippleSpeed;
                float wave = sin(input.uv.y * _RippleScale + t) + (ParallaxNoise(float2(input.uv.y * _RippleScale * 0.5, t * 0.5)) - 0.5);
                float sway = (ParallaxNoise(float2(input.uv.x * 6.0 + t * _WindSpeed * 0.5, input.uv.y * 3.0)) - 0.5) * _WindStrength * 0.05;
                float2 uv = input.uv + float2(wave * _RippleStrength * _MainTex_TexelSize.x + sway, 0);
                half4 color = ParallaxSampleBlurred(uv) * input.color;
                color.rgb *= 1.0 + _Shimmer * wave * 0.5;
                return color;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
