"""Gera as peças de terreno (montes, rampas, degraus, escadas) e o que dá para escalar, nos três biomas.

Uso: python Tools~/pixel_terrain.py
Saída: Packages/com.guavovic.parallax/Samples~/Demo/Sprites/Terrain/

Cada peça usa a paleta do chão do bioma e tem BELOW pixels abaixo da linha do chão, que entram no chão e escondem
a emenda. O colisor sai do contorno do próprio sprite (PolygonCollider2D), então a subida casa com a arte.
"""
import math
import os
import random
import sys

from PIL import Image, ImageChops, ImageDraw

sys.path.insert(0, os.path.dirname(__file__))
from pixel_forest import hexc  # noqa: E402

OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "Demo", "Sprites", "Terrain")
BELOW = 10


def rim(img, color, strength):
    pad = Image.new("RGBA", (img.width + 2, img.height + 2), (0, 0, 0, 0))
    pad.paste(img, (1, 1))
    alpha = pad.split()[3]
    edge = ImageChops.subtract(alpha, ImageChops.offset(alpha, 1, 1)).point(lambda v: 255 if v > 0 else 0)
    light = Image.new("RGBA", pad.size, color[:3] + (255,))
    light.putalpha(edge.point(lambda v: strength if v else 0))
    return Image.alpha_composite(pad, light).crop((1, 1, img.width + 1, img.height + 1))


def canvas(w, h):
    img = Image.new("RGBA", (w, h + BELOW), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


# --- floresta --------------------------------------------------------------------------------

FOREST = dict(soil=hexc("#08121b"), grass=hexc("#12383a"), hi=hexc("#2c8a84"), rim=hexc("#6fd0cf"))


def grassy_top(d, points, seed):
    """Faixa de grama e tufos ao longo do contorno de cima."""
    r = random.Random(seed)
    d.line(points, fill=FOREST["grass"], width=3)
    for x, y in points[::2]:
        if r.random() < 0.7:
            h = r.randint(2, 7)
            d.line([x, y, x + r.randint(-2, 2), y - h], fill=FOREST["hi"] if r.random() < 0.3 else FOREST["grass"])


def forest_hill(w, h, seed):
    img, d = canvas(w, h)
    base = h + BELOW
    # cosseno: começa e termina plano, então o herói sobe andando sem tropeçar no pé do monte
    top = [(x, h - h * (0.5 - 0.5 * math.cos(2 * math.pi * x / (w - 1)))) for x in range(0, w)]
    d.polygon([(0, base)] + top + [(w - 1, base)], fill=FOREST["soil"])
    r = random.Random(seed)
    for _ in range(w // 14):
        x = r.uniform(8, w - 8)
        y = r.uniform(h * 0.5, h + 4)
        d.ellipse([x - 3, y - 2, x + 3, y + 2], fill=hexc("#0e1c26"))
    grassy_top(d, top, seed)
    return rim(img, FOREST["rim"], 110)


def forest_step(w, h, seed):
    img, d = canvas(w, h)
    base = h + BELOW
    d.rounded_rectangle([0, 0, w - 1, base], 6, fill=FOREST["soil"])
    r = random.Random(seed)
    for _ in range(6):
        x, y = r.uniform(6, w - 6), r.uniform(10, h)
        d.rectangle([x - 4, y - 2, x + 4, y + 2], fill=hexc("#0e1c26"))
    grassy_top(d, [(x, 1) for x in range(2, w - 2)], seed)
    return rim(img, FOREST["rim"], 110)


# --- ruínas do templo ------------------------------------------------------------------------

TEMPLE = dict(floor=hexc("#0a0a18"), tile=hexc("#16163a"), edge=hexc("#2a2a52"), rim=hexc("#c4b4f0"))


def tiles(d, x0, y0, x1, y1):
    for row, y in enumerate(range(int(y0) + 8, int(y1), 9)):
        d.line([x0, y, x1, y], fill=TEMPLE["tile"])
        for x in range(int(x0) + (row % 2) * 10, int(x1), 20):
            d.line([x, y, x, y + 9], fill=TEMPLE["tile"])


def temple_stairs(steps, step_w, step_h, top_w):
    w = steps * step_w * 2 + top_w
    h = steps * step_h
    img, d = canvas(w, h)
    base = h + BELOW
    outline = [(0, base)]
    for i in range(steps):
        outline += [(i * step_w, h - i * step_h), (i * step_w, h - (i + 1) * step_h)]
    outline += [(steps * step_w, 0), (steps * step_w + top_w, 0)]
    for i in range(steps):
        x = steps * step_w + top_w + i * step_w
        outline += [(x + step_w, (i) * step_h), (x + step_w, (i + 1) * step_h)]
    outline += [(w, base)]
    d.polygon(outline, fill=TEMPLE["floor"])
    tiles(d, 0, 0, w, base)
    for i in range(steps):
        y = h - (i + 1) * step_h
        d.line([i * step_w, y, w - i * step_w, y], fill=TEMPLE["edge"])
    # juntas e bordas só dentro do contorno da escada
    mask = Image.new("L", img.size, 0)
    ImageDraw.Draw(mask).polygon(outline, fill=255)
    img.putalpha(ImageChops.multiply(img.split()[3], mask))
    return rim(img, TEMPLE["rim"], 120)


def temple_dais(w, h):
    img, d = canvas(w, h)
    d.rectangle([0, 0, w - 1, h + BELOW], fill=TEMPLE["floor"])
    tiles(d, 0, 0, w, h + BELOW)
    d.line([0, 0, w - 1, 0], fill=TEMPLE["edge"], width=2)
    for x in (6, w - 7):
        d.rectangle([x - 3, 3, x + 3, h], fill=TEMPLE["tile"])
    return rim(img, TEMPLE["rim"], 120)


# --- caverna ---------------------------------------------------------------------------------

CAVE = dict(rock=hexc("#05070f"), dark=hexc("#0a0e22"), rim=hexc("#9aa8ff"), cyan=hexc("#7de8ff"), pink=hexc("#ff7de0"))


def cave_mound(w, h, seed):
    img, d = canvas(w, h)
    base = h + BELOW
    r = random.Random(seed)
    top = []
    for x in range(0, w, 6):
        t = x / (w - 1)
        y = h - h * (0.5 - 0.5 * math.cos(2 * math.pi * t)) + (r.uniform(-1, 1) if 0.15 < t < 0.85 else 0)
        top.append((x, y))
    top.append((w - 1, h))
    d.polygon([(0, base)] + top + [(w - 1, base)], fill=CAVE["rock"])
    for x, y in top[2:-2:4]:
        c = r.choice((CAVE["cyan"], CAVE["pink"]))
        d.polygon([(x - 2, y + 1), (x, y - r.randint(4, 8)), (x + 2, y + 1)], fill=c)
    for _ in range(w // 16):
        x, y = r.uniform(6, w - 6), r.uniform(h * 0.5, h + 6)
        d.line([x, y, x + r.uniform(-6, 6), y + 4], fill=CAVE["dark"])
    return rim(img, CAVE["rim"], 120)


def cave_step(w, h, seed):
    img, d = canvas(w, h)
    base = h + BELOW
    r = random.Random(seed)
    pts = [(0, base), (2, 6), (w * 0.3, 0), (w * 0.7, 2), (w - 3, 5), (w - 1, base)]
    d.polygon(pts, fill=CAVE["rock"])
    for _ in range(5):
        x, y = r.uniform(6, w - 6), r.uniform(8, h)
        d.line([x, y, x + r.uniform(-8, 8), y + 5], fill=CAVE["dark"])
    return rim(img, CAVE["rim"], 120)


# --- escalar ---------------------------------------------------------------------------------

def climb_vine(length):
    img = Image.new("RGBA", (22, length), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    r = random.Random(5)
    for strand, phase in ((9, 0.0), (13, 1.7)):
        pts = [(strand + math.sin(phase + y * 0.08) * 2, y) for y in range(length)]
        d.line(pts, fill=hexc("#0f3a3a"), width=2)
        for y in range(4, length, 6):
            x = strand + math.sin(phase + y * 0.08) * 2
            side = r.choice((-1, 1))
            d.polygon([(x, y), (x + side * 6, y + 2), (x + side * 2, y + 5)], fill=hexc("#1c5a50"))
    return rim(img, hexc("#6fd0cf"), 110)


def climb_chain(length):
    img = Image.new("RGBA", (12, length), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    for y in range(0, length, 5):
        if (y // 5) % 2 == 0:
            d.ellipse([3, y, 8, y + 6], outline=hexc("#3a3a66"))
        else:
            d.line([6, y, 6, y + 6], fill=hexc("#3a3a66"), width=2)
    return rim(img, hexc("#c4b4f0"), 120)


def climb_roots(length):
    img = Image.new("RGBA", (24, length), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    for strand, phase in ((7, 0.3), (12, 2.1), (17, 4.0)):
        pts = [(strand + math.sin(phase + y * 0.06) * 2.5, y) for y in range(length)]
        d.line(pts, fill=hexc("#1a1d3a"), width=2)
    for y in range(10, length, 22):
        d.point((12, y), fill=hexc("#7de8ff"))
    return rim(img, hexc("#9aa8ff"), 100)


PIECES = {
    "forest_hill_small": lambda: forest_hill(150, 34, 1),
    "forest_hill_big": lambda: forest_hill(260, 62, 2),
    "forest_step": lambda: forest_step(86, 46, 3),
    "temple_stairs": lambda: temple_stairs(3, 22, 14, 70),
    "temple_dais": lambda: temple_dais(110, 30),
    "cave_mound": lambda: cave_mound(190, 50, 4),
    "cave_step": lambda: cave_step(90, 46, 5),
    "forest_climb": lambda: climb_vine(150),
    "temple_climb": lambda: climb_chain(150),
    "cave_climb": lambda: climb_roots(150),
}


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, make in PIECES.items():
        make().save(os.path.join(OUT, name + ".png"))
    print(len(PIECES), "peças")


if __name__ == "__main__":
    main()
