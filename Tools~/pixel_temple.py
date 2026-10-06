"""Gera as 15 camadas das ruínas de templo, no mesmo esquema de camadas da floresta (é para onde ela vira).

Uso: python Tools~/pixel_temple.py
Saída: Packages/com.guavovic.parallax/Samples~/Demo/Sprites/Temple/temple_NN_nome.png (480x270, RGBA)

Crepúsculo violeta, pedra em lilás e índigo, tochas âmbar como destaque. Mesmo chão da floresta, para o herói
andar na mesma altura, e mesmos fatores de parallax por índice.
"""
import math
import os
import random
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(__file__))
from pixel_forest import (GROUND_Y, H, W, Canvas, bayer, broken_pillar, dither_fill, hexc, layer_fog, mix,  # noqa: E402
                          periodic, rim_light, stone_arch, statue)

OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "Demo", "Sprites", "Temple")

RIM = hexc("#c4b4f0")
WARM = hexc("#ffb062")
FLAME = hexc("#ffd88a")

# Braseiros da camada de ruínas próximas, que a camada de brilho ilumina: frações da largura.
BRAZIERS = [0.16, 0.47, 0.81]


# --- formas ---------------------------------------------------------------------------------

def column(d, x, base, h, w, col, broken=None):
    """Coluna com capitel e base; com broken, o topo é quebrado em degraus."""
    d.rectangle([x - w / 2 - 3, base - 5, x + w / 2 + 3, base], fill=col)
    top = base - h
    if broken is None:
        d.rectangle([x - w / 2 - 4, top - 6, x + w / 2 + 4, top], fill=col)
        d.rectangle([x - w / 2, top, x + w / 2, base - 5], fill=col)
    else:
        d.polygon([(x - w / 2, base - 5), (x - w / 2, top + broken[0]), (x - w / 6, top + broken[1]),
                   (x + w / 5, top + broken[2]), (x + w / 2, top + broken[3]), (x + w / 2, base - 5)], fill=col)


def ziggurat(d, x, base, w, h, col, steps=5):
    for i in range(steps):
        ww = w * (1 - i / (steps + 1))
        hh = h / steps
        d.rectangle([x - ww / 2, base - hh * (i + 1), x + ww / 2, base - hh * i], fill=col)
    d.rectangle([x - w * 0.06, base - h - 10, x + w * 0.06, base - h], fill=col)


def spire(d, x, base, h, w, col):
    d.rectangle([x - w / 2, base - h * 0.75, x + w / 2, base], fill=col)
    d.polygon([(x - w / 2 - 2, base - h * 0.75), (x, base - h), (x + w / 2 + 2, base - h * 0.75)], fill=col)
    for k in range(3):
        y = base - h * (0.2 + k * 0.18)
        d.rectangle([x - 1, y, x + 1, y + 4], fill=(0, 0, 0, 0))


def aqueduct(d, offset, base, top, col, span=40):
    d.rectangle([offset, top, offset + W, top + 8], fill=col)
    x = 0
    while x < W:
        d.rectangle([offset + x, top, offset + x + 8, base], fill=col)
        x += span
    for x in range(0, W, span):
        d.pieslice([offset + x + 8, top + 4, offset + x + span, top + 4 + (span - 8)], 0, 180, fill=col)


def giant_head(d, x, base, h, col):
    s = h / 100.0
    d.polygon([(x - 34 * s, base), (x - 30 * s, base - 60 * s), (x - 22 * s, base - 92 * s), (x, base - 100 * s),
               (x + 22 * s, base - 92 * s), (x + 30 * s, base - 60 * s), (x + 34 * s, base)], fill=col)
    # olhos e boca vazados, para a luz de contorno desenhar o rosto
    d.rectangle([x - 16 * s, base - 64 * s, x - 6 * s, base - 58 * s], fill=(0, 0, 0, 0))
    d.rectangle([x + 6 * s, base - 64 * s, x + 16 * s, base - 58 * s], fill=(0, 0, 0, 0))
    d.rectangle([x - 10 * s, base - 34 * s, x + 10 * s, base - 30 * s], fill=(0, 0, 0, 0))


def brazier(d, x, base, col):
    d.polygon([(x - 7, base - 16), (x + 7, base - 16), (x + 4, base - 10), (x - 4, base - 10)], fill=col)
    d.rectangle([x - 2, base - 10, x + 2, base - 2], fill=col)
    d.rectangle([x - 6, base - 2, x + 6, base], fill=col)


def flame(d, x, y):
    for dx, h, c in ((-3, 9, "#d8541f"), (0, 14, "#f08a2a"), (3, 8, "#d8541f")):
        d.polygon([(x + dx - 3, y), (x + dx, y - h), (x + dx + 3, y)], fill=hexc(c))
    d.polygon([(x - 2, y), (x, y - 8), (x + 2, y)], fill=FLAME)


# --- camadas --------------------------------------------------------------------------------

def layer_sky():
    top, mid, low = hexc("#090a1c"), hexc("#1d1e44"), hexc("#4c3d66")
    img = Image.new("RGBA", (W, H), top)
    px = img.load()
    levels = 7
    for y in range(H):
        t = y / (GROUND_Y + 10)
        c0, c1, u = (top, mid, t / 0.55) if t < 0.55 else (mid, low, (t - 0.55) / 0.45)
        u = min(1.0, u)
        q = u * (levels - 1)
        base = int(q)
        frac = q - base
        a = mix(c0, c1, base / (levels - 1))
        b = mix(c0, c1, min(levels - 1, base + 1) / (levels - 1))
        for x in range(W):
            px[x, y] = b if frac > bayer(x, y) else a
    draw = ImageDraw.Draw(img)
    rnd = random.Random(301)
    for _ in range(110):
        x, y = rnd.randrange(W), rnd.randrange(int(H * 0.55))
        draw.point((x, y), fill=hexc("#e8dcff", rnd.choice((110, 180, 255))))
    # lua grande e baixa, com halo violeta
    cx, cy = int(W * 0.7), int(H * 0.34)
    halo = dither_fill((W, H), lambda x, y: max(0.0, 1 - math.hypot(x - cx, y - cy) / 140) ** 2 * 0.5, hexc("#cdb8f0", 200))
    img = Image.alpha_composite(img, halo)
    draw = ImageDraw.Draw(img)
    draw.ellipse([cx - 26, cy - 26, cx + 26, cy + 26], fill=hexc("#f2e8ff"))
    for ox, oy, r in ((-9, -6, 6), (8, 5, 7), (-3, 12, 4), (12, -10, 3)):
        draw.ellipse([cx + ox - r, cy + oy - r, cx + ox + r, cy + oy + r], fill=hexc("#cfc0ea"))
    return img


def layer_clouds():
    noise_a, noise_b = periodic(311), periodic(312)

    def density(x, y):
        band = max(0.0, 1 - abs(y - H * 0.3) / (H * 0.14))
        v = (noise_a(x) * 0.5 + 0.5) * 0.7 + (noise_b(x * 2 + y * 3) * 0.5 + 0.5) * 0.3
        return max(0.0, (v - 0.48)) * 2.6 * band

    return dither_fill((W, H), density, hexc("#9a86c0", 120), scale=2)


def layer_far_temples():
    """Montanhas baixas e templos em degraus muito longe."""
    noise = periodic(321)
    cv = Canvas()
    rnd = random.Random(322)
    temples = [(rnd.uniform(0, W), rnd.uniform(70, 120), rnd.uniform(40, 70)) for _ in range(3)]

    def draw(offset):
        line = [(offset, H)] + [(offset + x, H * 0.62 - (noise(x) * 0.5 + 0.5) * 40) for x in range(0, W + 2, 2)] + [(offset + W, H)]
        cv.d.polygon(line, fill=hexc("#3c3d66"))
        for x, w, h in temples:
            ziggurat(cv.d, offset + x, H * 0.62 + 6, w, h, hexc("#45466f"))

    cv.each(draw)
    img = cv.result()
    px = img.load()
    for y in range(H):
        t = max(0.0, min(1.0, (y - H * 0.45) / (GROUND_Y - H * 0.45)))
        for x in range(W):
            if px[x, y][3] and t * 0.8 > bayer(x, y):
                px[x, y] = hexc("#575882")
    return rim_light(img, RIM, 90)


def layer_spires():
    cv = Canvas()
    rnd = random.Random(331)
    spires = [(i * W / 6 + rnd.uniform(0, 40), rnd.uniform(70, 130), rnd.uniform(10, 18)) for i in range(6)]
    col = hexc("#34355c")

    def draw(offset):
        aqueduct(cv.d, offset, GROUND_Y - 14, GROUND_Y - 70, col)
        for x, h, w in spires:
            spire(cv.d, offset + x, GROUND_Y - 14, h, w, col)
        cv.d.rectangle([offset, GROUND_Y - 15, offset + W, H], fill=col)

    cv.each(draw)
    return rim_light(cv.result(), RIM, 100, skip_thin=True)


def layer_colossi():
    """Cabeças e colunas colossais, como as árvores gigantes da floresta."""
    cv = Canvas()
    rnd = random.Random(341)
    col = hexc("#22234a")
    items = []
    for i in range(4):
        x = (i + rnd.uniform(0.15, 0.85)) * W / 4
        items.append((x, i % 2, rnd.uniform(150, 220)))

    def draw(offset):
        for x, kind, h in items:
            if kind == 0:
                giant_head(cv.d, offset + x, GROUND_Y - 8, h * 0.6, col)
            else:
                column(cv.d, offset + x, GROUND_Y - 8, h, 26, col, broken=(0, -8, 6, 14))
        cv.d.rectangle([offset, GROUND_Y - 9, offset + W, H], fill=col)

    cv.each(draw)
    return rim_light(cv.result(), RIM, 110, skip_thin=True)


def layer_walls():
    """Muros com arcos e escadarias, no meio."""
    cv = Canvas()
    rnd = random.Random(351)
    col, brick = hexc("#17183a"), hexc("#10112c")
    pieces = []
    x = rnd.uniform(0, 30)
    while x < W:
        pieces.append((x, rnd.random(), rnd.uniform(60, 110)))
        x += rnd.uniform(60, 100)

    def draw(offset):
        for x, pick, h in pieces:
            if pick < 0.45:
                stone_arch(cv.d, offset + x, GROUND_Y - 3, 70, h * 0.7, col, brick)
            elif pick < 0.75:
                w = 50
                cv.d.rectangle([offset + x - w / 2, GROUND_Y - h, offset + x + w / 2, GROUND_Y], fill=col)
                for k in range(5):
                    cv.d.rectangle([offset + x - w / 2 + k * 10, GROUND_Y - h - 6, offset + x - w / 2 + k * 10 + 6, GROUND_Y - h], fill=col)
            else:
                for k in range(6):
                    cv.d.rectangle([offset + x - 30 + k * 6, GROUND_Y - (k + 1) * 8, offset + x + 30, GROUND_Y], fill=col)
        cv.d.rectangle([offset, GROUND_Y - 4, offset + W, H], fill=col)

    cv.each(draw)
    return rim_light(cv.result(), RIM, 120, skip_thin=True)


def layer_near_ruins():
    """Colunas, estátuas e braseiros perto do herói. Guarda onde ficam as chamas para a camada de brilho."""
    rnd = random.Random(361)
    col = hexc("#0f102a")
    cv = Canvas()

    def draw(offset):
        r = random.Random(362)
        for i, frac in enumerate(BRAZIERS):
            x = offset + frac * W
            brazier(cv.d, x, GROUND_Y + 2, col)
            if i == 0:
                column(cv.d, x - 40, GROUND_Y + 2, 90, 16, col, broken=(0, -6, 4, 10))
                statue(cv.d, x + 42, GROUND_Y + 2, 64, col)
            elif i == 1:
                column(cv.d, x + 34, GROUND_Y + 2, 120, 18, col)
                broken_pillar(cv.d, x - 36, GROUND_Y + 2, 18, 30, col, r)
            else:
                stone_arch(cv.d, x - 50, GROUND_Y + 2, 60, 58, col, hexc("#0a0b20"))
                column(cv.d, x + 30, GROUND_Y + 2, 70, 14, col, broken=(4, -4, 2, 12))

    cv.each(draw)
    img = rim_light(cv.result(), hexc("#e0b0a0"), 120)
    d = ImageDraw.Draw(img)
    for frac in BRAZIERS:
        flame(d, int(frac * W), GROUND_Y - 14)
    return img


def layer_torch_glow():
    def density(x, y):
        best = 0.0
        for frac in BRAZIERS:
            lx, ly = frac * W, GROUND_Y - 22
            dist = min(abs(x - lx), W - abs(x - lx))
            d = math.hypot(dist, (y - ly) * 1.1)
            best = max(best, max(0.0, 1 - d / 62) ** 1.8)
        return best * 0.95

    return dither_fill((W, H), density, WARM, scale=1)


def layer_ground():
    noise = periodic(371, 8)
    cv = Canvas()
    floor, tile, moss = hexc("#0a0a18"), hexc("#16163a"), hexc("#2a3f46")

    def top(x):
        return GROUND_Y + noise(x) * 1.5

    def draw(offset):
        d = cv.d
        d.polygon([(offset, H)] + [(offset + x, top(x)) for x in range(0, W + 2, 2)] + [(offset + W, H)], fill=floor)
        # lajes: linhas de junta e uma borda clara no alto
        d.line([(offset + x, top(x)) for x in range(0, W + 2, 2)], fill=tile, width=2)
        for row, y in enumerate(range(GROUND_Y + 8, H, 10)):
            d.line([offset, y, offset + W, y], fill=tile)
            for x in range((row % 2) * 12, W, 24):
                d.line([offset + x, y, offset + x, y + 10], fill=tile)
        s = random.Random(372)
        for _ in range(70):
            x = s.uniform(0, W)
            h = s.randint(2, 6)
            d.line([offset + x, top(x), offset + x + s.randint(-1, 1), top(x) - h], fill=moss)
        for _ in range(12):
            x = s.uniform(0, W)
            d.line([offset + x, GROUND_Y + 10, offset + x + s.uniform(-6, 6), GROUND_Y + 30], fill=hexc("#06060f"))

    cv.each(draw)
    return rim_light(cv.result(), RIM, 80)


def layer_chains():
    cv = Canvas()
    col, cloth = hexc("#0c0d24"), hexc("#3a1e3c")
    rnd = random.Random(381)
    items = [(rnd.uniform(0, W), rnd.randint(30, 120), rnd.random(), rnd.uniform(2, 4), rnd.uniform(0, math.tau)) for _ in range(9)]
    beams = [rnd.uniform(0, W) for _ in range(6)]

    def draw(offset):
        for x, length, pick, sway, phase in items:
            if pick < 0.5:
                for y in range(0, length, 4):
                    cv.d.ellipse([offset + x - 1, y, offset + x + 1, y + 4], outline=col)
            elif pick < 0.8:
                w = 14
                cv.d.polygon([(offset + x - w / 2, 0), (offset + x + w / 2, 0), (offset + x + w / 2, length * 0.7),
                              (offset + x, length * 0.7 + 8), (offset + x - w / 2, length * 0.7)], fill=cloth)
                cv.d.line([offset + x - w / 2 + 2, 6, offset + x + w / 2 - 2, 6], fill=hexc("#c08a3c"))
            else:
                pts = [(offset + x + math.sin(phase + y * 0.1) * sway, y) for y in range(length)]
                cv.d.line(pts, fill=hexc("#0f2a2c"))
        for cx in beams:
            cv.d.rectangle([offset + cx - 40, 0, offset + cx + 40, 10], fill=col)

    cv.each(draw)
    return rim_light(cv.result(), RIM, 100)


def layer_foreground():
    cv = Canvas()
    col = hexc("#05050e")

    def draw(offset):
        s = random.Random(391)
        column(cv.d, offset + 20, H + 6, H + 10, 40, col)
        column(cv.d, offset + 455, H + 6, 120, 34, col, broken=(0, -10, 8, 20))
        for _ in range(6):
            x = s.uniform(0, W)
            r = s.uniform(14, 30)
            cv.d.polygon([(offset + x - r, H), (offset + x - r * 0.6, H - r * 0.9), (offset + x + r * 0.3, H - r),
                          (offset + x + r, H - r * 0.4), (offset + x + r, H)], fill=col)
        cv.d.rectangle([offset, 0, offset + W, 8], fill=col)

    cv.each(draw)
    return cv.result()


def layer_dust():
    noise = periodic(397)

    def density(x, y):
        band = max(0.0, (y - H * 0.75) / (H * 0.25))
        return band * ((noise(x * 1.0 + y) * 0.5 + 0.5) * 0.9 + 0.1) * 0.5

    return dither_fill((W, H), density, hexc("#c8b2e0", 255), scale=1)


def layer_beams():
    noise = periodic(399)

    def density(x, y):
        u = (x - y * 0.45) % 120
        band = max(0.0, 1 - abs(u - 60) / 26.0)
        fade = max(0.0, 1 - y / (H * 0.9))
        return band * fade * (0.28 + 0.22 * (noise(x) * 0.5 + 0.5))

    return dither_fill((W, H), density, hexc("#ffd9b0", 255), scale=1)


LAYERS = [
    ("sky", layer_sky),
    ("clouds", layer_clouds),
    ("far_temples", layer_far_temples),
    ("spires", layer_spires),
    ("fog_far", lambda: layer_fog(401, GROUND_Y - 40, 70, hexc("#a493c8"), 150, 1)),
    ("colossi", layer_colossi),
    ("walls", layer_walls),
    ("fog_mid", lambda: layer_fog(403, GROUND_Y - 22, 56, hexc("#8676ae"), 170, 1)),
    ("near_ruins", layer_near_ruins),
    ("torch_glow", layer_torch_glow),
    ("ground", layer_ground),
    ("chains", layer_chains),
    ("foreground", layer_foreground),
    ("dust", layer_dust),
    ("beams", layer_beams),
]


def main():
    os.makedirs(OUT, exist_ok=True)
    for index, (name, fn) in enumerate(LAYERS):
        fn().save(os.path.join(OUT, "temple_%02d_%s.png" % (index, name)))
    print(len(LAYERS), "camadas")


if __name__ == "__main__":
    main()
