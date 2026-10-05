"""Gera as camadas da caverna de cristal em pixel art, com loop horizontal sem emenda.

Uso: python Tools~/pixel_cave.py
Saída: Packages/com.guavovic.parallax/Samples~/Demo/Sprites/Cave/cave_NN_nome.png (480x270, RGBA)

São 15 camadas, uma para cada camada da floresta na mesma posição, então a cena da caverna reaproveita
a montagem da floresta trocando só as imagens. Usa as mesmas ferramentas de pixel_forest.py.
"""
import math
import os
import random
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(__file__))
from pixel_forest import (H, W, Canvas, bayer, dither_fill, hexc, mix, periodic,  # noqa: E402
                          rim_light)

OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "Demo", "Sprites", "Cave")
FLOOR_Y = 216  # mesmo chão da floresta, para o herói andar na mesma altura

RIM = hexc("#9aa8ff")
CYAN = hexc("#7de8ff")
PINK = hexc("#ff7de0")
MIST = hexc("#5a68c8")

# Cristais da camada da ponte, que a camada de brilho ilumina: (x, base, altura, cor).
CRYSTALS = [(70, 214, 48, CYAN), (196, 212, 32, PINK), (300, 214, 56, CYAN), (420, 213, 40, PINK)]


# --- formas ---------------------------------------------------------------------------------

def stalactite(d, x, top, h, w, col):
    d.polygon([(x - w / 2, top), (x + w / 2, top), (x + w * 0.12, top + h * 0.7), (x, top + h), (x - w * 0.15, top + h * 0.6)], fill=col)


def stalagmite(d, x, base, h, w, col):
    d.polygon([(x - w / 2, base), (x - w * 0.12, base - h * 0.65), (x, base - h), (x + w * 0.18, base - h * 0.55), (x + w / 2, base)], fill=col)


def column(d, x, top, base, w, col):
    d.polygon([(x - w, top), (x + w, top), (x + w * 0.55, top + 22), (x + w * 0.55, base - 22), (x + w, base), (x - w, base),
               (x - w * 0.55, base - 22), (x - w * 0.55, top + 22)], fill=col)


def crystal(d, x, base, h, light, rnd):
    """Aglomerado de cristais: pontas de 5 lados com uma face clara e outra escura."""
    dark = mix(light, hexc("#0a0e22"), 0.55)
    for dx, scale, lean in ((-0.35, 0.65, -0.25), (0.3, 0.75, 0.2), (0.0, 1.0, 0.0)):
        cx, ch, cw = x + dx * h * 0.5, h * scale, h * 0.22 * scale
        tip = (cx + lean * ch, base - ch)
        d.polygon([(cx - cw, base), (cx - cw, base - ch * 0.62), tip, (cx + cw, base - ch * 0.62), (cx + cw, base)], fill=dark)
        d.polygon([(cx - cw, base), (cx - cw, base - ch * 0.62), tip, (cx, base - ch * 0.6), (cx, base)], fill=light)


def ceiling(d, offset, top_noise, depth, col, seed):
    """Teto irregular de rocha, de 0 até perto de depth."""
    points = [(offset, 0)]
    for x in range(0, W + 2, 2):
        points.append((offset + x, depth + top_noise(x) * depth * 0.4))
    points.append((offset + W, 0))
    d.polygon(points, fill=col)


# --- camadas --------------------------------------------------------------------------------

def layer_back():
    img = Image.new("RGBA", (W, H))
    px = img.load()
    top, low = hexc("#05060f"), hexc("#0d1230")
    for y in range(H):
        t = y / H
        for x in range(W):
            px[x, y] = mix(top, low, min(1.0, t + (0.06 if bayer(x, y) < (t * 7 % 1) else 0)))
    rnd = random.Random(41)
    draw = ImageDraw.Draw(img)
    for _ in range(70):
        draw.point((rnd.randrange(W), rnd.randrange(int(H * 0.8))), fill=rnd.choice((CYAN, RIM, PINK))[:3] + (rnd.choice((90, 150, 220)),))
    return img


def silhouette(seed, col, rim_strength, draw_items, ceiling_depth=None):
    cv = Canvas()
    noise = periodic(seed)
    rnd = random.Random(seed)
    items = draw_items(rnd)

    def draw(offset):
        if ceiling_depth:
            ceiling(cv.d, offset, noise, ceiling_depth, col, seed)
        for item in items:
            item(cv.d, offset)

    cv.each(draw)
    return rim_light(cv.result(), RIM, rim_strength, skip_thin=True)


def far_walls(rnd):
    items = []
    for i in range(14):
        x = (i + rnd.uniform(0, 0.8)) * W / 14
        h, w = rnd.uniform(40, 90), rnd.uniform(14, 26)
        items.append(lambda d, o, x=x, h=h, w=w: stalactite(d, o + x, 0, h, w, hexc("#1d2350")))
        hb = rnd.uniform(30, 70)
        items.append(lambda d, o, x=x + 12, h=hb, w=w: stalagmite(d, o + x, FLOOR_Y - 20, h, w, hexc("#1d2350")))
    items.append(lambda d, o: d.rectangle([o, FLOOR_Y - 21, o + W, H], fill=hexc("#1d2350")))
    return items


def far_columns(rnd):
    items = []
    for i in range(4):
        x = (i + rnd.uniform(0.2, 0.8)) * W / 4
        w = rnd.uniform(8, 14)
        items.append(lambda d, o, x=x, w=w: column(d, o + x, 0, FLOOR_Y - 12, w, hexc("#171c40")))
    for i in range(10):
        x = rnd.uniform(0, W)
        items.append(lambda d, o, x=x, h=rnd.uniform(30, 60): stalactite(d, o + x, 0, h, 16, hexc("#171c40")))
    items.append(lambda d, o: d.rectangle([o, FLOOR_Y - 13, o + W, H], fill=hexc("#171c40")))
    return items


def big_stalactites(rnd):
    items = []
    for i in range(5):
        x = (i + rnd.uniform(0.1, 0.9)) * W / 5
        h, w = rnd.uniform(110, 160), rnd.uniform(34, 50)
        items.append(lambda d, o, x=x, h=h, w=w: stalactite(d, o + x, 0, h, w, hexc("#10152f")))
    return items


def mid_stalagmites(rnd):
    items = []
    x = rnd.uniform(0, 20)
    while x < W:
        h, w = rnd.uniform(40, 95), rnd.uniform(20, 38)
        items.append(lambda d, o, x=x, h=h, w=w: stalagmite(d, o + x, FLOOR_Y - 3, h, w, hexc("#0e1330")))
        x += rnd.uniform(34, 70)
    items.append(lambda d, o: d.rectangle([o, FLOOR_Y - 4, o + W, H], fill=hexc("#0e1330")))
    return items


def boulder(d, x, base, w, h, col):
    d.polygon([(x - w / 2, base), (x - w * 0.45, base - h * 0.6), (x - w * 0.15, base - h), (x + w * 0.25, base - h * 0.9),
               (x + w / 2, base - h * 0.4), (x + w / 2, base)], fill=col)


def layer_bridge():
    """Rochas e os cristais que brilham, na mesma posição da camada de brilho."""
    cv = Canvas()
    rnd = random.Random(61)
    col = hexc("#0a0e22")
    rocks = [(rnd.uniform(0, W), rnd.uniform(40, 70), rnd.uniform(26, 44)) for _ in range(5)]

    def draw(offset):
        for x, w, h in rocks:
            boulder(cv.d, offset + x, FLOOR_Y + 2, w, h, col)
        for x, base, h, light in CRYSTALS:
            crystal(cv.d, offset + x, base, h, light, rnd)

    cv.each(draw)
    return rim_light(cv.result(), RIM, 120, skip_thin=True)


def wrapped_distance(x, cx):
    dx = abs(x - cx) % W
    return min(dx, W - dx)


def layer_crystal_glow():
    def density(x, y):
        best = 0.0
        for cx, base, h, _ in CRYSTALS:
            dist = math.hypot(wrapped_distance(x, cx), (y - (base - h * 0.6)) * 1.2)
            best = max(best, max(0.0, 1 - dist / (h * 1.1)) ** 2.2)
        return best * 0.6

    cyan = dither_fill((W, H), lambda x, y: density(x, y) if nearest(x) == CYAN else 0.0, CYAN[:3] + (255,))
    pink = dither_fill((W, H), lambda x, y: density(x, y) if nearest(x) == PINK else 0.0, PINK[:3] + (255,))
    return Image.alpha_composite(cyan, pink)


def nearest(x):
    return min(CRYSTALS, key=lambda c: wrapped_distance(x, c[0]))[3]


def layer_ground():
    noise = periodic(71, 8)
    cv = Canvas()
    rnd = random.Random(73)
    puddles = [(rnd.uniform(0, W), rnd.uniform(26, 50)) for _ in range(4)]

    def draw(offset):
        d = cv.d
        points = [(offset, H)]
        for x in range(0, W + 2, 2):
            points.append((offset + x, FLOOR_Y + noise(x) * 3))
        points.append((offset + W, H))
        d.polygon(points, fill=hexc("#05070f"))
        for px_, w in puddles:
            x = offset + px_
            d.ellipse([x - w / 2, FLOOR_Y + 8, x + w / 2, FLOOR_Y + 14], fill=hexc("#1d2350"))
            for k in range(0, int(w) - 8, 4):
                d.point((x - w / 2 + 4 + k, FLOOR_Y + 10), fill=CYAN[:3] + (200,))
        for _ in range(10):
            x = offset + rnd.uniform(0, W)
            crystal(d, x, FLOOR_Y + 2, rnd.uniform(5, 9), rnd.choice((CYAN, PINK)), rnd)

    cv.each(draw)
    return rim_light(cv.result(), RIM, 90)


def layer_drips():
    cv = Canvas()
    rnd = random.Random(81)
    drips = [(rnd.uniform(0, W), rnd.uniform(16, 60)) for _ in range(16)]

    def draw(offset):
        for x, h in drips:
            cv.d.line([offset + x, 0, offset + x, h], fill=hexc("#0a0e22"))
            cv.d.point((offset + x, h + 3), fill=CYAN[:3] + (220,))

    cv.each(draw)
    return cv.result()


def layer_foreground():
    cv = Canvas()
    rnd = random.Random(91)
    items = []
    for i in range(3):
        x = (i + rnd.uniform(0.2, 0.8)) * W / 3
        items.append((x, rnd.uniform(60, 100), rnd.uniform(40, 60), rnd.uniform(40, 90)))
    col = hexc("#03050c")

    def draw(offset):
        for x, h, w, hb in items:
            stalactite(cv.d, offset + x, 0, h, w, col)
            stalagmite(cv.d, offset + x + W / 6, H + 4, hb, w * 0.8, col)

    cv.each(draw)
    return cv.result()


def layer_mist(seed, y_center, thickness, alpha, scale=1):
    noise_a, noise_b = periodic(seed), periodic(seed + 1)

    def density(x, y):
        band = max(0.0, 1 - abs(y - y_center) / thickness)
        v = (noise_a(x) * 0.5 + 0.5) * 0.65 + (noise_b(x * 2 + y * 2) * 0.5 + 0.5) * 0.35
        return band * max(0.0, v * 0.9 - 0.3)

    return dither_fill((W, H), density, MIST[:3] + (alpha,), scale)


def layer_spores():
    rnd = random.Random(97)
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    px = img.load()
    for _ in range(90):
        x, y = rnd.randrange(W), rnd.randrange(H)
        px[x, y] = CYAN[:3] + (rnd.choice((120, 200, 255)),)
    return img


def layer_shafts():
    noise = periodic(99)

    def density(x, y):
        u = (x - y * 0.25) % 160
        band = max(0.0, 1 - abs(u - 80) / 14.0)
        fade = max(0.0, 1 - y / (H * 0.8))
        return band * fade * (0.25 + 0.2 * (noise(x) * 0.5 + 0.5))

    return dither_fill((W, H), density, hexc("#a8d8ff", 255), scale=1)


LAYERS = [
    ("back", layer_back),
    ("mist_far", lambda: layer_mist(43, 90, 60, 60)),
    ("walls_far", lambda: silhouette(45, hexc("#1d2350"), 90, far_walls)),
    ("columns_far", lambda: silhouette(47, hexc("#171c40"), 100, far_columns, ceiling_depth=22)),
    ("fog_far", lambda: layer_mist(49, FLOOR_Y - 40, 60, 80)),
    ("big_stalactites", lambda: silhouette(51, hexc("#10152f"), 110, big_stalactites, ceiling_depth=34)),
    ("stalagmites_mid", lambda: silhouette(53, hexc("#0e1330"), 110, mid_stalagmites)),
    ("fog_mid", lambda: layer_mist(57, FLOOR_Y - 22, 50, 100)),
    ("bridge_crystals", layer_bridge),
    ("crystal_glow", layer_crystal_glow),
    ("ground", layer_ground),
    ("drips", layer_drips),
    ("foreground", layer_foreground),
    ("spores", layer_spores),
    ("shafts", layer_shafts),
]


def main():
    os.makedirs(OUT, exist_ok=True)
    for index, (name, fn) in enumerate(LAYERS):
        path = os.path.join(OUT, "cave_%02d_%s.png" % (index, name))
        fn().save(path)
        print(path)


if __name__ == "__main__":
    main()
