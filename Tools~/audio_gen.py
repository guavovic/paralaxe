"""Gera as músicas, os ambientes e os efeitos sonoros do sample, tudo por código.

Uso: python Tools~/audio_gen.py
Saída: Packages/com.guavovic.parallax/Samples~/Demo/Audio/{Music,Ambience,Sfx}/*.wav (mono, 22 050 Hz, 16 bits)
Precisa do numpy.

Músicas: calmas, sem bateria, lentas, poucos instrumentos, notas graves e médias. Três por bioma, cada uma com tom,
escala, progressão e instrumentos próprios, e com a ponta emendada no começo para repetir sem corte.
"""
import math
import os
import wave

import numpy as np

SR = 22050
OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "Demo", "Audio")
RNG = np.random.default_rng(7)


def save(path, signal, peak=0.85):
    signal = np.asarray(signal, dtype=np.float64)
    m = np.max(np.abs(signal)) or 1.0
    data = (signal / m * peak * 32767).astype(np.int16)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())


def t_axis(seconds):
    return np.arange(int(seconds * SR)) / SR


def midi(note):
    return 440.0 * 2 ** ((note - 69) / 12)


def env(n, attack, release, sustain=1.0):
    """Envelope linear de ataque e soltura, em segundos."""
    e = np.full(n, sustain)
    a = min(n, int(attack * SR))
    r = min(n - a, int(release * SR))
    if a:
        e[:a] = np.linspace(0, sustain, a)
    if r:
        e[n - r:] = np.linspace(sustain, 0, r)
    return e


def lowpass(x, cutoff):
    spec = np.fft.rfft(x)
    freqs = np.fft.rfftfreq(len(x), 1 / SR)
    spec *= 1 / (1 + (freqs / cutoff) ** 4)
    return np.fft.irfft(spec, len(x))


def bandpass(x, low, high):
    spec = np.fft.rfft(x)
    freqs = np.fft.rfftfreq(len(x), 1 / SR)
    spec *= (1 / (1 + (freqs / high) ** 4)) * (1 - 1 / (1 + (freqs / low) ** 4))
    return np.fft.irfft(spec, len(x))


def reverb(x, seconds=3.2, wet=0.35, damp=2500):
    """Convolução com uma resposta sintética: ruído com decaimento exponencial, abafado nos agudos."""
    n = int(seconds * SR)
    ir = RNG.standard_normal(n) * np.exp(-np.arange(n) / (SR * seconds / 6.5))
    ir = lowpass(ir, damp)
    ir /= np.sqrt(np.sum(ir ** 2))
    size = len(x) + n
    fft_size = 1 << (size - 1).bit_length()
    y = np.fft.irfft(np.fft.rfft(x, fft_size) * np.fft.rfft(ir, fft_size), fft_size)[:size]
    out = np.zeros(size)
    out[:len(x)] += x * (1 - wet)
    out += y * wet * 0.6
    return out


def loop_seam(x, length, fade=4.0):
    """Corta em length segundos e põe o que vem depois (rabo do reverb e notas soando) por cima do começo, com fade
    cruzado: quando a música repete, o fim continua no começo sem corte."""
    n = int(length * SR)
    f = int(fade * SR)
    body = x[:n].copy()
    tail = x[n:n + f]
    if len(tail) < f:
        tail = np.pad(tail, (0, f - len(tail)))
    ramp = np.linspace(0, 1, f)
    body[:f] = x[:f] * ramp + tail * (1 - ramp)
    return body


# --- instrumentos ----------------------------------------------------------------------------

def pad(freqs, seconds, attack=2.5, release=3.0, bright=0.25):
    t = t_axis(seconds)
    out = np.zeros(len(t))
    for f in freqs:
        for detune in (-0.12, 0.0, 0.12):
            ff = f * 2 ** (detune / 12)
            vib = 1 + 0.002 * np.sin(2 * math.pi * 0.25 * t + RNG.uniform(0, 6))
            out += np.sin(2 * math.pi * ff * t * vib) + bright * np.sin(4 * math.pi * ff * t) * 0.5
    return out * env(len(t), attack, release) / max(1, len(freqs) * 3)


def bass(f, seconds, attack=0.8, release=2.0):
    t = t_axis(seconds)
    s = np.sin(2 * math.pi * f * t) + 0.3 * np.sin(4 * math.pi * f * t) + 0.1 * np.sin(6 * math.pi * f * t)
    return s * env(len(t), attack, release) * 0.9


def pluck(f, seconds, brightness=0.5):
    """Karplus-Strong: corda dedilhada, grave e média."""
    n = int(seconds * SR)
    period = max(2, int(SR / f))
    buf = RNG.uniform(-1, 1, period)
    buf = lowpass(np.concatenate([buf, buf]), 1500 + 3000 * brightness)[:period]
    out = np.zeros(n)
    decay = 0.996
    for i in range(n):
        j = i % period
        out[i] = buf[j]
        buf[j] = decay * 0.5 * (buf[j] + buf[(j + 1) % period])
    return out * env(n, 0.003, 0.4)


def bell(f, seconds, glass=False):
    t = t_axis(seconds)
    ratios = (1, 2.76, 5.4) if glass else (1, 2.0, 3.01)
    amps = (1, 0.35, 0.12)
    s = sum(a * np.sin(2 * math.pi * f * r * t) * np.exp(-t * (1.2 + r * 0.8)) for r, a in zip(ratios, amps))
    return s * env(len(t), 0.004, 0.3) * 0.7


def bowed(f, seconds):
    """Corda arqueada: ataque lento, vibrato e harmônicos que diminuem."""
    t = t_axis(seconds)
    vib = 1 + 0.004 * np.sin(2 * math.pi * 4.8 * t) * np.clip(t / 1.2, 0, 1)
    s = sum((0.7 ** k) * np.sin(2 * math.pi * f * (k + 1) * t * vib) for k in range(5))
    return s * env(len(t), 1.2, 1.5) * 0.35


def place(track, sound, at):
    i = int(at * SR)
    if i < 0:
        sound, i = sound[-i:], 0
    if i >= len(track):
        return
    end = min(len(track), i + len(sound))
    track[i:end] += sound[:end - i]


# --- músicas ---------------------------------------------------------------------------------

SCALES = {
    "dorian": [0, 2, 3, 5, 7, 9, 10],
    "aeolian": [0, 2, 3, 5, 7, 8, 10],
    "lydian": [0, 2, 4, 6, 7, 9, 11],
    "phrygian": [0, 1, 3, 5, 7, 8, 10],
    "phrygian_dom": [0, 1, 4, 5, 7, 8, 10],
    "pentatonic": [0, 3, 5, 7, 10],
}


def chord(root, scale, degree):
    s = SCALES[scale]
    notes = []
    for k in (0, 2, 4):
        idx = degree + k
        notes.append(root + s[idx % len(s)] + 12 * (idx // len(s)))
    return notes


def song(root, scale, progression, bpm, length, lead, seed, drone=False, bell_color=None):
    """root em MIDI (grave). lead: 'pluck', 'bowed' ou 'glass'."""
    rng = np.random.default_rng(seed)
    beat = 60.0 / bpm
    bar = beat * 4
    total = length + 6
    track = np.zeros(int(total * SR))
    chord_len = bar * 2
    t = 0.0
    i = 0
    while t < length + chord_len:
        degree = progression[i % len(progression)]
        notes = chord(root + 12, scale, degree)
        place(track, pad([midi(n) for n in notes], chord_len + 2.5) * 0.55, t)
        bass_note = root + SCALES[scale][degree % len(SCALES[scale])]
        place(track, bass(midi(bass_note), chord_len + 1.5) * (0.5 if drone else 0.4), t)
        if drone:
            place(track, bass(midi(root - 12), chord_len + 2) * 0.25, t)
        # melodia esparsa, entre a região média e a média-grave, com pausas
        s = SCALES[scale]
        b = 0.0
        while b < chord_len - beat:
            if rng.random() < 0.55:
                deg = int(rng.integers(0, len(s) + 3))
                note = root + 24 + s[deg % len(s)] + 12 * (deg // len(s))
                note = min(note, root + 34)
                f = midi(note)
                when = t + b + rng.uniform(-0.03, 0.03)
                if lead == "pluck":
                    place(track, pluck(f, 3.5, 0.35) * 0.55, when)
                elif lead == "bowed":
                    place(track, bowed(f, beat * 3) * 0.6, when)
                else:
                    place(track, bell(f, 4.0, glass=True) * 0.45, when)
                if bell_color and rng.random() < 0.15:
                    place(track, bell(midi(note - 12), 5.0, glass=bell_color == "glass") * 0.3, when + beat)
            b += beat * rng.choice([1, 2, 2, 3])
        t += chord_len
        i += 1
    return loop_seam(reverb(track, 3.6, 0.42), length)


SONGS = {
    "forest": [
        dict(root=38, scale="dorian", progression=[0, 3, 4, 3], bpm=62, lead="pluck", seed=11),
        dict(root=45, scale="aeolian", progression=[0, 5, 3, 4], bpm=58, lead="pluck", seed=12),
        dict(root=41, scale="lydian", progression=[0, 1, 0, 4], bpm=56, lead="pluck", seed=13, bell_color="warm"),
    ],
    "temple": [
        dict(root=40, scale="phrygian_dom", progression=[0, 1, 0, 6], bpm=54, lead="bowed", seed=21, drone=True, bell_color="warm"),
        dict(root=38, scale="phrygian", progression=[0, 6, 5, 1], bpm=52, lead="bowed", seed=22, drone=True),
        dict(root=36, scale="phrygian_dom", progression=[0, 3, 1, 0], bpm=56, lead="bowed", seed=23, drone=True, bell_color="warm"),
    ],
    "cave": [
        dict(root=37, scale="lydian", progression=[0, 1, 4, 1], bpm=54, lead="glass", seed=31, drone=True),
        dict(root=40, scale="pentatonic", progression=[0, 2, 3, 1], bpm=50, lead="glass", seed=32, drone=True),
        dict(root=35, scale="aeolian", progression=[0, 5, 2, 6], bpm=52, lead="glass", seed=33, drone=True, bell_color="glass"),
    ],
}


# --- ambientes -------------------------------------------------------------------------------

def noise(seconds):
    return RNG.standard_normal(int(seconds * SR))


def slow_lfo(seconds, rate, phase=0.0):
    t = t_axis(seconds)
    return 0.5 + 0.5 * np.sin(2 * math.pi * rate * t + phase)


def ambience_forest(seconds=20):
    t = t_axis(seconds)
    wind = lowpass(noise(seconds), 500) * (0.4 + 0.6 * slow_lfo(seconds, 0.1)) * 0.8
    leaves = bandpass(noise(seconds), 1500, 5000) * (0.2 + 0.8 * slow_lfo(seconds, 0.1, 0.5) ** 3) * 0.15
    crickets = np.zeros(len(t))
    for k in range(3):
        f = 4300 + 300 * k
        gate = (np.sin(2 * math.pi * (14 + k * 2) * t) > 0.6).astype(float)
        bursts = (np.sin(2 * math.pi * (0.45 + 0.1 * k) * t + k) > 0.2).astype(float)
        crickets += np.sin(2 * math.pi * f * t) * gate * bursts * 0.05
    owl = np.zeros(len(t))
    for at in (6.0, 15.5):
        tt = t_axis(0.9)
        hoot = np.sin(2 * math.pi * 330 * tt) * env(len(tt), 0.08, 0.5) * 0.15
        place(owl, hoot, at)
        place(owl, hoot * 0.8, at + 0.8)
    return loop_seam(np.concatenate([wind + leaves + crickets + owl, np.zeros(SR * 4)]), seconds, 2.0)


def ambience_temple(seconds=20):
    t = t_axis(seconds)
    wind = bandpass(noise(seconds), 200, 900) * (0.3 + 0.7 * slow_lfo(seconds, 0.07)) * 0.5
    rumble = lowpass(noise(seconds), 120) * 0.6
    crackle = np.zeros(len(t))
    for at in RNG.uniform(0, seconds, 220):
        n = int(RNG.uniform(0.004, 0.015) * SR)
        burst = RNG.standard_normal(n) * np.exp(-np.arange(n) / (n / 4)) * RNG.uniform(0.2, 0.9)
        place(crackle, burst, at)
    crackle = bandpass(crackle, 800, 6000) * 0.5
    return loop_seam(np.concatenate([wind + rumble + crackle, np.zeros(SR * 4)]), seconds, 2.0)


def ambience_cave(seconds=20):
    t = t_axis(seconds)
    hum = (np.sin(2 * math.pi * 55 * t) + 0.6 * np.sin(2 * math.pi * 55.4 * t)) * 0.2
    air = lowpass(noise(seconds), 300) * (0.3 + 0.7 * slow_lfo(seconds, 0.05)) * 0.4
    drips = np.zeros(len(t) + SR * 2)
    for at in RNG.uniform(0, seconds, 14):
        f = RNG.uniform(900, 1600)
        tt = t_axis(0.25)
        drop = np.sin(2 * math.pi * f * tt * (1 + 1.5 * np.exp(-tt * 30))) * np.exp(-tt * 18) * 0.3
        place(drips, drop, at)
    drips = reverb(drips, 2.5, 0.6)[:len(t)]
    return loop_seam(np.concatenate([hum + air + drips, np.zeros(SR * 4)]), seconds, 2.0)


# --- efeitos ---------------------------------------------------------------------------------

def step(kind, variant):
    rng = np.random.default_rng(100 + variant + {"grass": 0, "stone": 10, "rock": 20}[kind])
    n = int(0.12 * SR)
    x = rng.standard_normal(n) * np.exp(-np.arange(n) / (SR * 0.025))
    if kind == "grass":
        x = bandpass(x, 900, 5000) * 0.8 + bandpass(rng.standard_normal(n), 2500, 7000) * np.exp(-np.arange(n) / (SR * 0.04)) * 0.3
    elif kind == "stone":
        tt = np.arange(n) / SR
        x = bandpass(x, 300, 3000) + np.sin(2 * math.pi * rng.uniform(140, 180) * tt) * np.exp(-tt * 40) * 0.8
    else:
        tt = np.arange(n) / SR
        x = lowpass(x, 1500) + np.sin(2 * math.pi * rng.uniform(90, 120) * tt) * np.exp(-tt * 35)
    return x


def whoosh(seconds, f0, f1, amp=1.0):
    n = int(seconds * SR)
    x = RNG.standard_normal(n)
    out = np.zeros(n)
    chunks = 24
    size = n // chunks
    for c in range(chunks):
        f = f0 + (f1 - f0) * c / chunks
        seg = x[c * size:(c + 1) * size]
        out[c * size:(c + 1) * size] = bandpass(seg, f * 0.6, f * 1.6)
    return out * np.sin(np.linspace(0, math.pi, n)) ** 1.5 * amp


def thud(seconds=0.18, f=70):
    tt = t_axis(seconds)
    return np.sin(2 * math.pi * f * tt * (1 + 0.5 * np.exp(-tt * 40))) * np.exp(-tt * 22) + lowpass(RNG.standard_normal(len(tt)), 600) * np.exp(-tt * 40) * 0.5


def knock(f, seconds=0.25, noise_amt=0.4):
    tt = t_axis(seconds)
    return (np.sin(2 * math.pi * f * tt) + 0.5 * np.sin(2 * math.pi * f * 2.3 * tt)) * np.exp(-tt * 25) + bandpass(RNG.standard_normal(len(tt)), 500, 4000) * np.exp(-tt * 50) * noise_amt


def crash(base, seconds=0.7, glass=False):
    out = np.zeros(int(seconds * SR))
    for k in range(9):
        f = base * RNG.uniform(1, 3.5)
        at = RNG.uniform(0, 0.12)
        piece = bell(f, 0.5, glass=True) if glass else knock(f, 0.3, 0.6)
        place(out, piece * RNG.uniform(0.3, 0.8), at)
    tt = t_axis(seconds)
    out += bandpass(RNG.standard_normal(len(tt)), 1000, 8000) * np.exp(-tt * 9) * (0.4 if glass else 0.6)
    return out


def layer(*sounds):
    """Soma sons de durações diferentes (o mais curto completa com silêncio)."""
    n = max(len(x) for x in sounds)
    return sum(np.pad(x, (0, n - len(x))) for x in sounds)


def chain_clink(variant):
    rng = np.random.default_rng(300 + variant)
    out = np.zeros(int(0.25 * SR))
    for k in range(3):
        place(out, bell(rng.uniform(1800, 2600), 0.2, glass=True) * rng.uniform(0.3, 0.7), rng.uniform(0, 0.08))
    return out


def rustle(variant):
    rng = np.random.default_rng(400 + variant)
    n = int(0.22 * SR)
    x = bandpass(rng.standard_normal(n), 1500, 6000) * np.sin(np.linspace(0, math.pi, n)) * (0.5 + 0.5 * np.abs(np.sin(np.linspace(0, 9, n))))
    return x


SFX = {
    "jump": lambda: whoosh(0.22, 600, 1600, 0.6),
    "air_jump": lambda: whoosh(0.25, 900, 2400, 0.6),
    "land": lambda: thud(),
    "swing": lambda: whoosh(0.18, 2500, 900, 0.8),
    "hit_clay": lambda: knock(380),
    "hit_wood": lambda: knock(220, 0.3, 0.5),
    "hit_crystal": lambda: bell(1100, 0.6, glass=True),
    "break_clay": lambda: crash(300),
    "break_wood": lambda: crash(160),
    "break_crystal": lambda: crash(900, 0.9, glass=True),
    "cut": lambda: layer(whoosh(0.16, 4000, 2000, 0.5), rustle(9) * 0.6),
    "portal": lambda: layer(whoosh(1.4, 180, 500, 0.9), reverb(bell(midi(50), 2.0), 2.0, 0.6)[:int(1.4 * SR)] * 0.3),
}


def main():
    for biome, songs in SONGS.items():
        for i, params in enumerate(songs):
            save(os.path.join(OUT, "Music", "%s_%d.wav" % (biome, i + 1)), song(length=56, **params), 0.8)
            print("música", biome, i + 1)
    save(os.path.join(OUT, "Ambience", "forest.wav"), ambience_forest(), 0.7)
    save(os.path.join(OUT, "Ambience", "temple.wav"), ambience_temple(), 0.7)
    save(os.path.join(OUT, "Ambience", "cave.wav"), ambience_cave(), 0.7)
    for kind in ("grass", "stone", "rock"):
        for v in range(4):
            save(os.path.join(OUT, "Sfx", "step_%s_%d.wav" % (kind, v)), step(kind, v), 0.6)
    for v in range(3):
        save(os.path.join(OUT, "Sfx", "climb_vine_%d.wav" % v), rustle(v), 0.5)
        save(os.path.join(OUT, "Sfx", "climb_chain_%d.wav" % v), chain_clink(v), 0.5)
    for name, make in SFX.items():
        save(os.path.join(OUT, "Sfx", name + ".wav"), make(), 0.8)
    print("ok")


if __name__ == "__main__":
    main()
