Shader "Parallax/Sway Sprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _WindSpeed ("Wind Speed", Float) = 1
        _WindStrength ("Wind Strength", Float) = 0.5
        _Blur ("Blur (texels)", Range(0, 6)) = 0
        _SwayAmount ("Sway Amount", Float) = 0.08
        _SwaySpeed ("Sway Speed", Float) = 1.3
        [Toggle] _Hanging ("Hanging (bends from the top)", Float) = 0
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
                float _SwayAmount;
                float _SwaySpeed;
                float _Hanging;
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

            // Planta e cipó: a ponta solta balança e a presa fica parada. O vento do mundo aumenta o balanço.
            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float height = lerp(input.uv.y, 1.0 - input.uv.y, _Hanging);
                float t = _Time.y * _SwaySpeed + ParallaxPhase(positionWS) * 2.0;
                float gust = ParallaxNoise(float2(t * 0.35, positionWS.x * 0.2)) - 0.5;
                float bend = (sin(t) * 0.6 + gust) * _SwayAmount * (1.0 + _WindStrength);
                positionWS.x += bend * height * height;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return ParallaxSampleBlurred(input.uv) * input.color;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
