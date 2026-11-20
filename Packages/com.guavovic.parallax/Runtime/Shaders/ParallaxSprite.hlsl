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

// Como ParallaxNoise, mas repete a cada "period" na horizontal: casa na emenda do loop da camada.
float ParallaxNoiseWrap(float2 p, float period)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    float x0 = fmod(fmod(i.x, period) + period, period);
    float x1 = fmod(x0 + 1.0, period);
    float a = ParallaxHash(float2(x0, i.y));
    float b = ParallaxHash(float2(x1, i.y));
    float c = ParallaxHash(float2(x0, i.y + 1.0));
    float d = ParallaxHash(float2(x1, i.y + 1.0));
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

    // Desfoque de 9 pontos com pesos de um filtro gaussiano pequeno, normalizados para somar 1 (0,2382 + 4 x 0,1191 +
    // 4 x 0,07135): com a soma abaixo de 1, o miolo opaco do sprite saía com alpha menor que 1 e deixava passar o que
    // estava atrás. A cor é ponderada pelo alpha, para o contorno não escurecer ao misturar com o transparente.
    float2 step = _MainTex_TexelSize.xy * _Blur;
    float2 offsets[9] = {
        float2(0, 0), float2(step.x, 0), float2(-step.x, 0), float2(0, step.y), float2(0, -step.y),
        step, -step, float2(step.x, -step.y), float2(-step.x, step.y)
    };
    float weights[9] = { 0.2382, 0.1191, 0.1191, 0.1191, 0.1191, 0.07135, 0.07135, 0.07135, 0.07135 };

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
