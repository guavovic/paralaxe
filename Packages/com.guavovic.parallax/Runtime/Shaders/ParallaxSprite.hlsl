#ifndef PARALLAX_SPRITE_INCLUDED
#define PARALLAX_SPRITE_INCLUDED

// Funções comuns dos shaders do Paralaxe. Quem inclui declara antes _MainTex, sampler_MainTex,
// _MainTex_TexelSize e _Blur.

float ParallaxHash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float ParallaxNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = ParallaxHash(i);
    float b = ParallaxHash(i + float2(1, 0));
    float c = ParallaxHash(i + float2(0, 1));
    float d = ParallaxHash(i + float2(1, 1));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// Fase pela posição no mundo: cada objeto mexe num tempo diferente, sem script por objeto.
// Sprites entram em lote já no espaço do mundo, então a fase vem do vértice e não da origem do objeto.
float ParallaxPhase(float3 positionWS)
{
    return positionWS.x * 0.37 + positionWS.y * 0.23;
}

half4 ParallaxSampleBlurred(float2 uv)
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

#endif
