"""Gera os detalhes espalhados pelos cenários e os bichos que se mexem, nas cores de cada profundidade.

Uso: python Tools~/pixel_details.py
Saída: Packages/com.guavovic.parallax/Samples~/Demo/Sprites/Details/

Cada imagem é recortada no próprio conteúdo, com a base embaixo, para o ParallaxScatter espalhar pela camada.
A luz de contorno vem do alto e da esquerda, como nas camadas.
"""
import math
import os
import random
import sys

from PIL import Image, ImageChops, ImageDraw

sys.path.insert(0, os.path.dirname(__file__))
from pixel_forest import bayer, dead_tree, giant_tree, hexc, mix, pine, round_tree, statue, broken_pillar, branch  # noqa: E402
from pixel_cave import CYAN, PINK, RIM, boulder, column, crystal, stalactite, stalagmite  # noqa: E402

OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "Demo", "Sprites", "Details")
VIOLET = hexc("#b38cff")


def rim(img, color, strength):
    """Luz de contorno em imagem de qualquer tamanho (a das camadas assume a largura do loop)."""
    pad = Image.new("RGBA", (img.width + 2, img.height + 2), (0, 0, 0, 0))
    pad.paste(img, (1, 1))
    alpha = pad.split()[3]
    edge = ImageChops.subtract(alpha, ImageChops.offset(alpha, 1, 1)).point(lambda v: 255 if v > 0 else 0)
    light = Image.new("RGBA", pad.size, color[:3] + (255,))
    light.putalpha(edge.point(lambda v: strength if v else 0))
    return Image.alpha_composite(pad, light).crop((1, 1, img.width + 1, img.height + 1))


def piece(w, h, draw, rim_color=None, rim_strength=120):
    """Desenha numa tela w x h com a base em y = h, aplica o contorno e recorta no conteúdo (base mantida)."""
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw(ImageDraw.Draw(img), img)
    if rim_color:
        img = rim(img, rim_color, rim_strength)
    box = img.getbbox()
    if box is None:
        return img
    left, top, right, _ = box
    return img.crop((left, top, right, h))


def hanging(w, h, draw, rim_color=None, rim_strength=120):
    """Como piece, mas pendurado: o topo fica em y = 0."""
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw(ImageDraw.Draw(img), img)
    if rim_color:
        img = rim(img, rim_color, rim_strength)
    box = img.getbbox()
    if box is None:
        return img
    left, _, right, bottom = box
    return img.crop((left, 0, right, bottom))


# --- floresta --------------------------------------------------------------------------------

def far_trees():
    col, light = hexc("#2d5068"), hexc("#8fd2d8")
    out = []
    for i in range(5):
        r = random.Random(500 + i)
        h = r.uniform(45, 90)

        def draw(d, img, r=r, h=h, i=i):
            for k in range(r.randint(1, 3)):
                x = 40 + k * r.uniform(10, 18)
                hh = h * r.uniform(0.7, 1.0)
                if (i + k) % 3 == 2:
                    round_tree(d, x, 120, hh * 0.8, hh * 0.55, col, r)
                else:
                    pine(d, x, 120, hh, hh * 0.42, col, r)
        out.append(piece(110, 120, draw, light, 90))
    return out


def giant_trunks():
    col, light = hexc("#1d3d52"), hexc("#86d0d4")
    out = []
    for i in range(3):
        r = random.Random(520 + i)
        out.append(piece(200, 270, lambda d, img, r=r, i=i: giant_tree(d, 100, 262, r.uniform(200, 250), r.uniform(14, 22), col, r, canopy=i != 1),
                         light, 110))
    return out


def mid_trees():
    col, light = hexc("#142c3d"), hexc("#74cfd3")
    out = []
    for i in range(6):
        r = random.Random(540 + i)
        kind = i % 3

        def draw(d, img, r=r, kind=kind):
            if kind == 0:
                pine(d, 60, 170, r.uniform(100, 150), r.uniform(40, 60), col, r)
            elif kind == 1:
                round_tree(d, 60, 170, r.uniform(80, 120), r.uniform(50, 74), col, r)
            else:
                dead_tree(d, 60, 170, r.uniform(90, 140), col, r)
        out.append(piece(120, 170, draw, light, 120))
    return out


def ruins():
    stone, light = hexc("#0e1f2d"), hexc("#74cfd3")
    out = []
    r = random.Random(560)
    out.append(piece(40, 90, lambda d, img: broken_pillar(d, 20, 90, 16, 70, stone, r), light, 110))
    out.append(piece(40, 90, lambda d, img: broken_pillar(d, 20, 90, 20, 44, stone, r), light, 110))
    out.append(piece(40, 70, lambda d, img: statue(d, 20, 70, 56, stone), light, 110))

    def tomb(d, img):
        d.rectangle([4, 14, 22, 40], fill=stone)
        d.pieslice([4, 4, 22, 24], 180, 360, fill=stone)
        d.line([13, 16, 13, 30], fill=hexc("#091621"))
        d.line([9, 20, 17, 20], fill=hexc("#091621"))
    out.append(piece(26, 40, tomb, light, 110))
    return out


def ground_plants():
    """Plantas na altura do herói: arbustos, samambaias, capim, pedras, um tronco caído e cogumelos."""
    leaf, leaf_hi, soil = hexc("#12383a"), hexc("#2c8a84"), hexc("#08121b")
    light = hexc("#6fd0cf")
    out = {}

    def bush(seed):
        r = random.Random(seed)

        def draw(d, img):
            for _ in range(9):
                cx, cy, rr = r.uniform(10, 34), r.uniform(10, 20), r.uniform(5, 9)
                d.ellipse([cx - rr, cy - rr * 0.8, cx + rr, cy + rr * 0.8], fill=leaf)
            for _ in range(6):
                d.point((r.uniform(6, 38), r.uniform(4, 14)), fill=leaf_hi)
            d.rectangle([4, 22, 40, 26], fill=leaf)
        return piece(44, 26, draw, light, 120)

    def fern(seed):
        r = random.Random(seed)

        def draw(d, img):
            for k in range(r.randint(5, 7)):
                ang = math.pi / 2 + (k - 3) * r.uniform(0.28, 0.38)
                length = r.uniform(14, 24)
                pts = [(20 + math.cos(ang) * length * t + math.cos(ang) ** 3 * 6 * t * t, 30 - math.sin(ang) * length * t + 7 * t * t)
                       for t in [j / 8 for j in range(9)]]
                d.line(pts, fill=leaf, width=1)
                for j in range(1, 8):
                    x, y = pts[j]
                    d.line([x, y, x + 2, y - 2], fill=leaf_hi if j % 3 == 0 else leaf)
                    d.line([x, y, x - 2, y - 1], fill=leaf)
        return piece(40, 30, draw, light, 100)

    def grass(seed):
        r = random.Random(seed)

        def draw(d, img):
            for _ in range(18):
                x = r.uniform(2, 30)
                h = r.uniform(5, 16)
                lean = r.uniform(-4, 4)
                d.line([x, 20, x + lean, 20 - h], fill=leaf_hi if r.random() < 0.3 else leaf)
        return piece(32, 20, draw, light, 80)

    def rock(seed):
        r = random.Random(seed)
        w, h = r.uniform(12, 26), r.uniform(8, 14)
        return piece(30, 16, lambda d, img: boulder(d, 15, 16, w, h, hexc("#0c1a24")), light, 120)

    def log(d, img):
        d.rounded_rectangle([2, 8, 62, 18], 5, fill=hexc("#0f1e24"))
        d.ellipse([56, 7, 66, 19], fill=hexc("#1c3238"))
        d.ellipse([59, 10, 63, 16], fill=hexc("#0f1e24"))
        for x in range(8, 52, 9):
            d.line([x, 10, x + 5, 10], fill=hexc("#1c3238"))
        d.line([20, 8, 26, 2], fill=leaf, width=1)
        d.ellipse([24, 0, 30, 4], fill=leaf)

    def glow_shrooms(seed, cap):
        r = random.Random(seed)

        def draw(d, img):
            for k in range(r.randint(2, 4)):
                x = 6 + k * r.uniform(5, 8)
                h = r.uniform(4, 10)
                d.line([x, 14, x, 14 - h], fill=hexc("#c8d8d0"))
                d.pieslice([x - 3, 14 - h - 3, x + 3, 14 - h + 3], 180, 360, fill=cap)
        return piece(34, 14, draw)

    def flowers(seed, petal):
        r = random.Random(seed)

        def draw(d, img):
            for _ in range(r.randint(3, 5)):
                x, h = r.uniform(3, 23), r.uniform(5, 12)
                d.line([x, 14, x, 14 - h], fill=leaf)
                d.point((x - 1, 14 - h), fill=petal)
                d.point((x + 1, 14 - h), fill=petal)
                d.point((x, 13 - h), fill=petal)
        return piece(26, 14, draw)

    out["bush"] = [bush(600 + i) for i in range(4)]
    out["fern"] = [fern(610 + i) for i in range(3)]
    out["grass"] = [grass(620 + i) for i in range(3)]
    out["rock"] = [rock(630 + i) for i in range(3)]
    out["log"] = [piece(68, 20, log, light, 110)]
    out["glowshroom"] = [glow_shrooms(640, hexc("#7de8e0")), glow_shrooms(641, hexc("#ffb36b"))]
    out["flower"] = [flowers(650, hexc("#f6e08a")), flowers(651, hexc("#b8f0ff")), flowers(652, hexc("#ff9ac8"))]
    return out


def vines():
    col, leaf, light = hexc("#0a2428"), hexc("#0f3a3a"), hexc("#6fd0cf")
    out = []
    for i in range(4):
        r = random.Random(700 + i)
        length = r.randint(40, 140)

        def draw(d, img, r=r, length=length):
            for strand in range(r.randint(1, 3)):
                x0 = 12 + strand * r.uniform(4, 8)
                sway, phase = r.uniform(2, 5), r.uniform(0, math.tau)
                ln = length * r.uniform(0.6, 1.0)
                pts = [(x0 + math.sin(phase + y * 0.09) * sway, y) for y in range(int(ln))]
                d.line(pts, fill=col)
                for y in range(6, int(ln), 6):
                    lx = x0 + math.sin(phase + y * 0.09) * sway
                    side = r.choice((-1, 1))
                    d.polygon([(lx, y), (lx + side * 5, y + 2), (lx + side * 2, y + 5)], fill=leaf)
        out.append(hanging(36, 150, draw, light, 110))
    return out


def foreground_forest():
    """Silhuetas grandes e escuras bem perto da câmera."""
    col = hexc("#030a0f")
    out = []

    def big_fern(seed):
        r = random.Random(seed)

        def draw(d, img):
            for k in range(6):
                ang = math.pi / 2 + (k - 2.5) * 0.32
                length = r.uniform(80, 130)
                pts = [(80 + math.cos(ang) * length * t, 140 - math.sin(ang) * length * t + 30 * t * t) for t in [j / 12 for j in range(13)]]
                d.line(pts, fill=col, width=3)
                for j in range(1, 12):
                    x, y = pts[j]
                    size = 12 * (1 - j / 13)
                    d.polygon([(x, y), (x + size, y - size * 0.6), (x + size * 0.3, y + 2)], fill=col)
                    d.polygon([(x, y), (x - size, y - size * 0.4), (x - size * 0.3, y + 2)], fill=col)
        return piece(160, 140, draw)

    def trunk(d, img):
        d.polygon([(10, 280), (22, 0), (58, 0), (74, 280)], fill=col)
        r = random.Random(730)
        branch(d, 50, 90, 0.4, 60, 6, col, r, 3)

    def branch_top(d, img):
        r = random.Random(731)
        d.line([0, 4, 150, 24], fill=col, width=7)
        for x in range(10, 150, 16):
            d.ellipse([x - 14, 10 + x * 0.12, x + 14, 34 + x * 0.12], fill=col)
            branch(d, x, 20 + x * 0.13, -math.pi / 2 + r.uniform(-0.4, 0.4), r.uniform(14, 30), 2, col, r, 2)

    out.append(big_fern(720))
    out.append(big_fern(721))
    out.append(piece(84, 280, trunk))
    out.append(hanging(170, 90, branch_top))
    return out


def butterfly(frame, color):
    def draw(d, img):
        span = (4, 1)[frame]
        d.line([4, 2, 4, 5], fill=hexc("#0a1418"))
        d.rectangle([4 - span, 1, 3, 3], fill=color)
        d.rectangle([5, 1, 4 + span, 3], fill=color)
        d.point((4 - span, 4), fill=mix(color, hexc("#ffffff"), 0.5))
        d.point((4 + span, 4), fill=mix(color, hexc("#ffffff"), 0.5))
    img = Image.new("RGBA", (9, 6), (0, 0, 0, 0))
    draw(ImageDraw.Draw(img), img)
    return img


def leaf_particle():
    img = Image.new("RGBA", (4, 3), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.polygon([(0, 1), (2, 0), (3, 1), (1, 2)], fill=hexc("#5fb08a"))
    return img


# --- caverna ---------------------------------------------------------------------------------

def cave_columns():
    col = hexc("#171c40")
    return [piece(40, 200, lambda d, img, w=w: column(d, 20, 0, 200, w, col), RIM, 90) for w in (8, 11, 14)]


def cave_stalactites():
    col = hexc("#10152f")
    out = []
    for i in range(4):
        r = random.Random(800 + i)
        h, w = r.uniform(70, 160), r.uniform(22, 48)

        def draw(d, img, r=r, h=h, w=w):
            stalactite(d, 30, 0, h, w, col)
            if r.random() < 0.6:
                stalactite(d, 30 + r.uniform(-16, 16), 0, h * 0.5, w * 0.5, col)
        out.append(hanging(60, 170, draw, RIM, 110))
    return out


def cave_stalagmites():
    col = hexc("#0e1330")
    out = []
    for i in range(4):
        r = random.Random(820 + i)
        h, w = r.uniform(40, 95), r.uniform(18, 36)
        out.append(piece(50, 100, lambda d, img, h=h, w=w, r=r: (stalagmite(d, 25, 100, h, w, col),
                                                                  stalagmite(d, 25 + r.uniform(-14, 14), 100, h * 0.45, w * 0.5, col)), RIM, 110))
    return out


def cave_crystals():
    out = []
    for i, (color, h) in enumerate(((CYAN, 30), (PINK, 24), (VIOLET, 34), (CYAN, 16), (PINK, 40), (VIOLET, 18))):
        r = random.Random(840 + i)
        out.append(piece(60, 50, lambda d, img, c=color, h=h, r=r: crystal(d, 30, 50, h, c, r)))
    return out


def cave_ground_bits():
    out = {}

    def shrooms(seed, cap):
        r = random.Random(seed)

        def draw(d, img):
            for k in range(r.randint(2, 4)):
                x = 6 + k * r.uniform(6, 9)
                h = r.uniform(6, 14)
                d.line([x, 18, x - 1, 18 - h], fill=hexc("#8a96c8"))
                d.pieslice([x - 5, 18 - h - 4, x + 3, 18 - h + 4], 180, 360, fill=cap)
                d.point((x - 2, 18 - h - 2), fill=hexc("#ffffff"))
        return piece(36, 18, draw)

    def bones(d, img):
        bone = hexc("#a8acc8")
        d.ellipse([2, 6, 12, 14], fill=bone)
        d.point((5, 9), fill=hexc("#05070f"))
        d.point((9, 9), fill=hexc("#05070f"))
        d.line([12, 12, 28, 10], fill=bone, width=2)
        d.line([16, 14, 24, 6], fill=bone, width=1)

    def rock(seed):
        r = random.Random(seed)
        w, h = r.uniform(14, 30), r.uniform(8, 16)
        return piece(34, 18, lambda d, img: boulder(d, 17, 18, w, h, hexc("#0a0e22")), RIM, 110)

    out["shroom"] = [shrooms(860, hexc("#7de8ff")), shrooms(861, hexc("#ff7de0")), shrooms(862, VIOLET)]
    out["bones"] = [piece(30, 16, bones, RIM, 60)]
    out["rock"] = [rock(870 + i) for i in range(3)]
    return out


def cave_roots():
    col = hexc("#0a0e22")
    out = []
    for i in range(3):
        r = random.Random(880 + i)

        def draw(d, img, r=r):
            for strand in range(r.randint(2, 4)):
                x0 = 14 + strand * r.uniform(3, 7)
                ln = r.uniform(30, 90)
                pts = [(x0 + math.sin(y * 0.15 + strand) * 2, y) for y in range(int(ln))]
                d.line(pts, fill=col, width=1)
                d.point((pts[-1][0], ln + 2), fill=CYAN[:3] + (230,))
        out.append(hanging(40, 100, draw, RIM, 80))
    return out


def foreground_cave():
    col = hexc("#03050c")
    out = []
    for i in range(3):
        r = random.Random(900 + i)
        h, w = r.uniform(110, 180), r.uniform(50, 80)
        out.append(hanging(100, 200, lambda d, img, h=h, w=w: stalactite(d, 50, 0, h, w, col)))
    for i in range(2):
        r = random.Random(910 + i)
        out.append(piece(120, 120, lambda d, img, r=r: boulder(d, 60, 120, r.uniform(80, 110), r.uniform(70, 110), col)))
    return out


def bat(frame):
    img = Image.new("RGBA", (11, 6), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    body = hexc("#1a1d3a")
    d.ellipse([4, 2, 6, 4], fill=body)
    if frame == 0:
        d.polygon([(4, 3), (0, 0), (2, 3)], fill=body)
        d.polygon([(6, 3), (10, 0), (8, 3)], fill=body)
    elif frame == 1:
        d.line([0, 3, 4, 3], fill=body)
        d.line([6, 3, 10, 3], fill=body)
    else:
        d.polygon([(4, 3), (0, 5), (2, 3)], fill=body)
        d.polygon([(6, 3), (10, 5), (8, 3)], fill=body)
    d.point((4, 2), fill=PINK)
    d.point((6, 2), fill=PINK)
    return img


def jelly(frame, color):
    """Esporo que flutua e pulsa, como uma água-viva pequena."""
    img = Image.new("RGBA", (9, 11), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    squeeze = (0, 1)[frame]
    d.pieslice([1 + squeeze, 1, 7 - squeeze, 7], 180, 360, fill=color[:3] + (220,))
    d.point((3, 2), fill=hexc("#ffffff"))
    for x in (2, 4, 6):
        d.line([x, 4, x + (frame * 2 - 1) * (x - 4) // 2, 9], fill=color[:3] + (150,))
    return img


# --- ruínas do templo -------------------------------------------------------------------------

TEMPLE_RIM = hexc("#c4b4f0")


def temple_column(d, x, base, h, w, col, broken):
    d.rectangle([x - w / 2 - 3, base - 5, x + w / 2 + 3, base], fill=col)
    top = base - h
    if not broken:
        d.rectangle([x - w / 2 - 4, top - 6, x + w / 2 + 4, top], fill=col)
        d.rectangle([x - w / 2, top, x + w / 2, base - 5], fill=col)
    else:
        d.polygon([(x - w / 2, base - 5), (x - w / 2, top), (x - w / 6, top - 8), (x + w / 5, top + 5), (x + w / 2, top + 12),
                   (x + w / 2, base - 5)], fill=col)


def temple_spires():
    col = hexc("#34355c")
    out = []
    for i in range(3):
        r = random.Random(1000 + i)
        h, w = r.uniform(70, 130), r.uniform(10, 18)

        def draw(d, img, h=h, w=w):
            d.rectangle([30 - w / 2, 140 - h * 0.75, 30 + w / 2, 140], fill=col)
            d.polygon([(30 - w / 2 - 2, 140 - h * 0.75), (30, 140 - h), (30 + w / 2 + 2, 140 - h * 0.75)], fill=col)
        out.append(piece(60, 140, draw, TEMPLE_RIM, 100))
    return out


def temple_columns(col, rim_strength, count, seed, tall):
    out = []
    canvas_h = int(tall[1]) + 12
    for i in range(count):
        r = random.Random(seed + i)
        h = r.uniform(*tall)
        w = r.uniform(12, 22)
        out.append(piece(60, canvas_h, lambda d, img, h=h, w=w, i=i: temple_column(d, 30, canvas_h, h, w, col, i % 2 == 1),
                         TEMPLE_RIM, rim_strength))
    return out


def temple_statues():
    col = hexc("#0f102a")
    return [piece(40, 80, lambda d, img, h=h: statue(d, 20, 80, h, col), TEMPLE_RIM, 110) for h in (48, 66)]


def temple_ground_bits():
    out = {}
    stone, dark = hexc("#1a1a3a"), hexc("#0c0c22")

    def urn(seed):
        r = random.Random(seed)
        w, h = r.randint(10, 14), r.randint(14, 20)

        def draw(d, img):
            d.ellipse([1, h * 0.3, w - 1, h - 1], fill=hexc("#4a3a5e"))
            d.rectangle([w * 0.3, 1, w * 0.7, h * 0.4], fill=hexc("#4a3a5e"))
            d.rectangle([w * 0.22, 1, w * 0.78, 2], fill=hexc("#8a74a8"))
            d.line([2, h * 0.6, w - 3, h * 0.6], fill=hexc("#c08a3c"))
        return piece(w + 2, h + 1, draw, TEMPLE_RIM, 90)

    def rubble(seed):
        r = random.Random(seed)

        def draw(d, img):
            for _ in range(r.randint(3, 5)):
                x, w, h = r.uniform(4, 30), r.uniform(5, 12), r.uniform(4, 9)
                d.polygon([(x - w / 2, 16), (x - w / 2 + 1, 16 - h), (x + w / 2, 16 - h + 2), (x + w / 2, 16)],
                          fill=stone if r.random() < 0.6 else dark)
        return piece(36, 16, draw, TEMPLE_RIM, 110)

    def brazier(d, img):
        col = hexc("#14142e")
        d.polygon([(4, 14), (20, 14), (16, 20), (8, 20)], fill=col)
        d.rectangle([10, 20, 14, 30], fill=col)
        d.rectangle([6, 30, 18, 32], fill=col)
        for dx, h, c in ((-3, 9, "#d8541f"), (0, 14, "#f08a2a"), (3, 8, "#d8541f")):
            d.polygon([(12 + dx - 3, 14), (12 + dx, 14 - h), (12 + dx + 3, 14)], fill=hexc(c))
        d.polygon([(10, 14), (12, 6), (14, 14)], fill=hexc("#ffd88a"))

    out["urn"] = [urn(1100 + i) for i in range(3)]
    out["rubble"] = [rubble(1110 + i) for i in range(3)]
    out["brazier"] = [piece(24, 33, brazier, TEMPLE_RIM, 80)]
    return out


def temple_banners():
    out = []
    for i, (cloth, trim) in enumerate(((hexc("#3a1e3c"), hexc("#c08a3c")), (hexc("#24264e"), hexc("#a8a0d8")))):
        r = random.Random(1120 + i)
        length = r.randint(60, 110)

        def draw(d, img, cloth=cloth, trim=trim, length=length):
            d.rectangle([2, 0, 24, 3], fill=hexc("#0c0d24"))
            d.polygon([(4, 3), (22, 3), (22, length * 0.8), (13, length), (4, length * 0.8)], fill=cloth)
            d.line([6, 8, 20, 8], fill=trim)
            d.line([6, length * 0.7, 20, length * 0.7], fill=trim)
            d.rectangle([11, 18, 15, 26], outline=trim)
        out.append(hanging(26, 120, draw, TEMPLE_RIM, 70))
    for i in range(2):
        r = random.Random(1130 + i)
        length = r.randint(40, 100)

        def chain(d, img, length=length):
            for y in range(0, length, 4):
                d.ellipse([3, y, 6, y + 4], outline=hexc("#0c0d24"))
        out.append(hanging(10, 110, chain, TEMPLE_RIM, 80))
    return out


def temple_foreground():
    col = hexc("#05050e")
    out = [piece(70, 290, lambda d, img: temple_column(d, 35, 290, 280, 44, col, False)),
           piece(70, 200, lambda d, img: temple_column(d, 35, 200, 150, 40, col, True))]

    def arch_top(d, img):
        d.rectangle([0, 0, 180, 18], fill=col)
        d.pieslice([10, -40, 170, 70], 0, 180, fill=col)
        d.pieslice([30, -30, 150, 50], 0, 180, fill=(0, 0, 0, 0))
    out.append(hanging(180, 80, arch_top))
    return out


def temple_gate():
    """Entrada do templo no fim das ruínas: moldura de pedra, escada descendo no escuro e duas chamas."""
    stone, dark = hexc("#22234a"), hexc("#06060f")

    def draw(d, img):
        d.rectangle([0, 20, 70, 96], fill=stone)
        d.rectangle([4, 10, 66, 20], fill=stone)
        d.polygon([(0, 10), (35, 0), (70, 10)], fill=stone)
        d.rectangle([16, 34, 54, 96], fill=dark)
        d.pieslice([16, 22, 54, 48], 180, 360, fill=dark)
        for k in range(5):
            d.line([18 + k * 2, 70 + k * 5, 52 - k * 2, 70 + k * 5], fill=hexc("#14142e"))
        for x in (8, 62):
            d.rectangle([x - 2, 40, x + 2, 50], fill=hexc("#14142e"))
            d.polygon([(x - 3, 40), (x, 30), (x + 3, 40)], fill=hexc("#f08a2a"))
            d.polygon([(x - 1, 40), (x, 34), (x + 1, 40)], fill=hexc("#ffd88a"))
        d.line([35, 4, 35, 8], fill=hexc("#c08a3c"))
    return piece(70, 97, draw, TEMPLE_RIM, 120)


def temple_ledge():
    img = Image.new("RGBA", (64, 12), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, 63, 7], fill=hexc("#22234a"))
    d.rectangle([0, 0, 63, 1], fill=hexc("#8a7cc0"))
    for x in range(0, 64, 16):
        d.line([x, 2, x, 7], fill=hexc("#14142e"))
    d.rectangle([6, 8, 12, 11], fill=hexc("#14142e"))
    d.rectangle([51, 8, 57, 11], fill=hexc("#14142e"))
    return img


def moth(frame):
    img = Image.new("RGBA", (7, 5), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    wing = hexc("#ffe0b0")
    d.line([3, 1, 3, 3], fill=hexc("#3a2a1a"))
    if frame == 0:
        d.rectangle([0, 0, 2, 2], fill=wing)
        d.rectangle([4, 0, 6, 2], fill=wing)
    else:
        d.rectangle([1, 2, 2, 3], fill=wing)
        d.rectangle([4, 2, 5, 3], fill=wing)
    return img


def ember():
    img = Image.new("RGBA", (3, 3), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.point((1, 1), fill=hexc("#ffd88a"))
    for p in ((0, 1), (2, 1), (1, 0), (1, 2)):
        d.point(p, fill=hexc("#f08a2a", 180))
    return img


def halo(color, radius):
    """Brilho redondo em dithering, mais denso no centro, para o material que pulsa."""
    size = radius * 2 + 1
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = img.load()
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - radius, y - radius) / radius
            if max(0.0, 1 - d) ** 1.6 * 0.8 > bayer(x, y):
                px[x, y] = color[:3] + (255,)
    return img


def save(name, img):
    img.save(os.path.join(OUT, name + ".png"))


def main():
    os.makedirs(OUT, exist_ok=True)
    groups = {
        "forest_far_tree": far_trees(),
        "forest_giant": giant_trunks(),
        "forest_mid_tree": mid_trees(),
        "forest_ruin": ruins(),
        "forest_vine": vines(),
        "forest_fg": foreground_forest(),
        "cave_column": cave_columns(),
        "cave_stalactite": cave_stalactites(),
        "cave_stalagmite": cave_stalagmites(),
        "cave_crystal": cave_crystals(),
        "cave_root": cave_roots(),
        "cave_fg": foreground_cave(),
    }
    for kind, images in ground_plants().items():
        groups["forest_" + kind] = images
    for kind, images in cave_ground_bits().items():
        groups["cave_" + kind] = images
    groups["temple_spire"] = temple_spires()
    groups["temple_colossus"] = temple_columns(hexc("#22234a"), 110, 3, 1010, (150, 220))
    groups["temple_wall_column"] = temple_columns(hexc("#17183a"), 120, 3, 1020, (80, 130))
    groups["temple_near_column"] = temple_columns(hexc("#0f102a"), 120, 4, 1030, (60, 120))
    groups["temple_statue"] = temple_statues()
    for kind, images in temple_ground_bits().items():
        groups["temple_" + kind] = images
    groups["temple_hanging"] = temple_banners()
    groups["temple_fg"] = temple_foreground()

    for prefix, images in groups.items():
        for i, img in enumerate(images):
            save("%s_%d" % (prefix, i), img)

    for frame in (0, 1):
        save("butterfly_cyan_%d" % frame, butterfly(frame, hexc("#7de8e0")))
        save("butterfly_amber_%d" % frame, butterfly(frame, hexc("#ffb36b")))
        save("jelly_cyan_%d" % frame, jelly(frame, CYAN))
        save("jelly_pink_%d" % frame, jelly(frame, PINK))
    for frame in (0, 1, 2):
        save("bat_%d" % frame, bat(frame))
    save("leaf", leaf_particle())
    save("temple_gate", temple_gate())
    save("temple_ledge", temple_ledge())
    save("ember", ember())
    for frame in (0, 1):
        save("moth_%d" % frame, moth(frame))
    for name, color in (("cyan", CYAN), ("pink", PINK), ("violet", VIOLET), ("amber", hexc("#ffb36b"))):
        save("glow_halo_%s" % name, halo(color, 22))


if __name__ == "__main__":
    main()
