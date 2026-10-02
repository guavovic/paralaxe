"""Gera as camadas de floresta em pixel art, com loop horizontal sem emenda.

Uso: python Tools~/pixel_forest.py
Saída: Assets/Sprites/Forest/forest_NN_nome.png (480x270, RGBA)

Cada camada é desenhada três vezes lado a lado e recortada no meio, então o loop não tem costura.
O mundo tem 17,78 unidades de largura, então 480 px dão 27 pixels por unidade.
"""
import math
import os
import random

from PIL import Image, ImageChops, ImageDraw

W, H = 480, 270
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Sprites", "Forest")
GROUND_Y = 216  # y do topo do chão, em pixels, contado de cima

BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


def hexc(value, alpha=255):
    value = value.lstrip("#")
    return tuple(int(value[i:i + 2], 16) for i in (0, 2, 4)) + (alpha,)


def mix(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(4))


def bayer(x, y):
    return (BAYER[y % 4][x % 4] + 0.5) / 16.0


class Canvas:
    """Três cópias lado a lado. Tudo que é desenhado com each() aparece nas três."""

    def __init__(self):
        self.image = Image.new("RGBA", (W * 3, H), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.image)

    def each(self, fn):
        for k in range(3):
            fn(k * W)

    def result(self):
        return self.image.crop((W, 0, W * 2, H))


def periodic(seed, harmonics=6):
    rnd = random.Random(seed)
    parts = [(k, rnd.uniform(0, math.tau), 1.0 / (0.7 + k * 0.5)) for k in range(1, harmonics + 1)]
    total = sum(p[2] for p in parts)
    return lambda x: sum(a * math.sin(math.tau * k * x / W + ph) for k, ph, a in parts) / total


def rim_light(layer, color, strength=170):
    """Contorno claro nas bordas voltadas para a luz (cima e esquerda)."""
    alpha = layer.split()[3]
    shifted = ImageChops.offset(alpha, 1, 1)
    edge = ImageChops.subtract(alpha, shifted).point(lambda v: 255 if v > 0 else 0)
    rim = Image.new("RGBA", layer.size, color[:3] + (255,))
    rim.putalpha(edge.point(lambda v: strength if v else 0))
    return Image.alpha_composite(layer, rim)


def dither_fill(size, density, color, scale=1):
    """Camada de pontos com dithering ordenado. density(x, y) devolve de 0 a 1."""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = img.load()
    for y in range(h):
        for x in range(w):
            if density(x, y) > bayer(x // scale, y // scale):
                px[x, y] = color
    return img


# --- formas ---------------------------------------------------------------------------------

def pine(d, x, base, h, w, col, rnd):
    trunk = max(1, int(w * 0.07))
    d.rectangle([x - trunk, base - h * 0.2, x + trunk, base], fill=col)
    tiers = max(4, int(h / 12))
    for i in range(tiers):
        t = i / tiers
        y_top = base - h * (0.12 + 0.88 * (i + 1) / tiers)
        y_bot = base - h * (0.12 + 0.88 * i / tiers) + h * 0.05
        half = w * 0.5 * (1.0 - t * 0.88) * rnd.uniform(0.88, 1.08)
        d.polygon([(x - half, y_bot), (x, y_top), (x + half, y_bot)], fill=col)


def round_tree(d, x, base, h, w, col, rnd):
    trunk = max(2, int(w * 0.06))
    d.polygon([(x - trunk * 1.8, base), (x - trunk, base - h * 0.5), (x + trunk, base - h * 0.5), (x + trunk * 1.8, base)], fill=col)
    for _ in range(10):
        cx = x + rnd.uniform(-w * 0.38, w * 0.38)
        cy = base - h * rnd.uniform(0.5, 0.95)
        r = w * rnd.uniform(0.14, 0.26)
        d.ellipse([cx - r, cy - r * 0.85, cx + r, cy + r * 0.85], fill=col)


def branch(d, x, y, angle, length, width, col, rnd, depth):
    x2 = x + math.cos(angle) * length
    y2 = y - math.sin(angle) * length
    d.line([(x, y), (x2, y2)], fill=col, width=max(1, int(width)))
    if depth <= 0 or length < 5:
        return
    for turn in (-1, 1):
        if rnd.random() < 0.85:
            branch(d, x2, y2, angle + turn * rnd.uniform(0.35, 0.75), length * rnd.uniform(0.62, 0.78), width * 0.68, col, rnd, depth - 1)


def dead_tree(d, x, base, h, col, rnd):
    width = max(2, h * 0.05)
    d.polygon([(x - width * 1.6, base), (x - width * 0.6, base - h * 0.45), (x + width * 0.6, base - h * 0.45), (x + width * 1.6, base)], fill=col)
    branch(d, x, base - h * 0.4, math.pi / 2 + rnd.uniform(-0.2, 0.2), h * 0.32, width * 0.7, col, rnd, 4)


def giant_tree(d, x, base, h, w, col, rnd, canopy=True):
    flare = w * 1.7
    d.polygon([(x - flare, base), (x - w * 0.55, base - h * 0.28), (x - w * 0.42, base - h), (x + w * 0.42, base - h), (x + w * 0.55, base - h * 0.28), (x + flare, base)], fill=col)
    for _ in range(4):
        side = rnd.choice((-1, 1))
        y = base - h * rnd.uniform(0.35, 0.85)
        angle = (0.0 if side > 0 else math.pi) + rnd.uniform(0.25, 0.6) * (1 if side > 0 else -1)
        length = rnd.uniform(w * 1.4, w * 2.8)
        branch(d, x + side * w * 0.3, y, angle, length, w * 0.22, col, rnd, 2)
    if canopy:
        for _ in range(7):
            cx = x + rnd.uniform(-w * 2.2, w * 2.2)
            cy = base - h * rnd.uniform(0.85, 1.02)
            r = rnd.uniform(w * 0.7, w * 1.4)
            d.ellipse([cx - r, cy - r * 0.7, cx + r, cy + r * 0.7], fill=col)


def stone_arch(d, x, base, w, h, col, brick):
    pillar = w * 0.16
    d.rectangle([x - w / 2, base - h, x - w / 2 + pillar, base], fill=col)
    d.rectangle([x + w / 2 - pillar, base - h, x + w / 2, base], fill=col)
    d.pieslice([x - w / 2, base - h - w / 2, x + w / 2, base - h + w / 2], 180, 360, fill=col)
    inner = w / 2 - pillar
    d.pieslice([x - inner, base - h - inner, x + inner, base - h + inner], 180, 360, fill=(0, 0, 0, 0))
    d.rectangle([x - inner, base - h, x + inner, base], fill=(0, 0, 0, 0))
    for yy in range(int(base - h), int(base), 7):
        d.line([(x - w / 2, yy), (x - w / 2 + pillar, yy)], fill=brick, width=1)
        d.line([(x + w / 2 - pillar, yy), (x + w / 2, yy)], fill=brick, width=1)


def broken_pillar(d, x, base, w, h, col, rnd):
    top = base - h
    d.polygon([(x - w / 2, base), (x - w / 2, top + rnd.uniform(0, 8)), (x - w / 6, top - rnd.uniform(2, 8)), (x + w / 4, top + rnd.uniform(0, 6)), (x + w / 2, top + rnd.uniform(4, 10)), (x + w / 2, base)], fill=col)


def statue(d, x, base, h, col):
    s = h / 40.0
    d.rectangle([x - 7 * s, base - 5 * s, x + 7 * s, base], fill=col)
    d.polygon([(x - 5 * s, base - 5 * s), (x - 4 * s, base - 22 * s), (x + 4 * s, base - 22 * s), (x + 5 * s, base - 5 * s)], fill=col)
    d.ellipse([x - 4 * s, base - 30 * s, x + 4 * s, base - 21 * s], fill=col)
    d.polygon([(x - 4 * s, base - 29 * s), (x - 7 * s, base - 37 * s), (x - 2 * s, base - 30 * s)], fill=col)
    d.polygon([(x + 4 * s, base - 29 * s), (x + 7 * s, base - 37 * s), (x + 2 * s, base - 30 * s)], fill=col)


# --- camadas --------------------------------------------------------------------------------

def layer_sky():
    top, mid, low = hexc("#070d1f"), hexc("#12294a"), hexc("#2c5a72")
    img = Image.new("RGBA", (W, H), top)
    px = img.load()
    for y in range(H):
        t = y / (GROUND_Y + 10)
        c0, c1, u = (top, mid, t / 0.55) if t < 0.55 else (mid, low, (t - 0.55) / 0.45)
        u = min(1.0, u)
        for x in range(W):
            levels = 7
            q = u * (levels - 1)
            base = int(q)
            frac = q - base
            a = mix(c0, c1, base / (levels - 1))
            b = mix(c0, c1, min(levels - 1, base + 1) / (levels - 1))
            px[x, y] = b if frac > bayer(x, y) else a
    draw = ImageDraw.Draw(img)
    rnd = random.Random(3)
    for _ in range(90):
        x, y = rnd.randrange(W), rnd.randrange(int(H * 0.5))
        draw.point((x, y), fill=hexc("#cfe8ff", rnd.choice((120, 190, 255))))
    # lua com halo em dithering
    cx, cy = int(W * 0.32), int(H * 0.26)
    halo = dither_fill((W, H), lambda x, y: max(0.0, 1 - math.hypot(x - cx, y - cy) / 120) ** 2 * 0.55, hexc("#bfe3ee", 200))
    img = Image.alpha_composite(img, halo)
    draw = ImageDraw.Draw(img)
    draw.ellipse([cx - 17, cy - 17, cx + 17, cy + 17], fill=hexc("#e4f4f6"))
    for ox, oy, r in ((-6, -4, 4), (5, 3, 5), (-2, 8, 3)):
        draw.ellipse([cx + ox - r, cy + oy - r, cx + ox + r, cy + oy + r], fill=hexc("#b6d3dc"))
    return img


def layer_clouds():
    noise_a, noise_b = periodic(21), periodic(22)

    def density(x, y):
        band = max(0.0, 1 - abs(y - H * 0.22) / (H * 0.16))
        v = (noise_a(x) * 0.5 + 0.5) * 0.7 + (noise_b(x * 2 + y * 3) * 0.5 + 0.5) * 0.3
        return max(0.0, (v - 0.46)) * 2.6 * band

    return dither_fill((W, H), density, hexc("#6f9bb0", 120), scale=2)


def silhouette_layer(seed, base_fn, build, color, rim=None, rim_strength=150, fade_to=None):
    rnd = random.Random(seed)
    cv = Canvas()
    items = build(rnd)

    def draw(offset):
        state = random.Random(seed + 7)
        for it in items:
            it(cv.d, offset, state)
        cv.d.rectangle([offset, base_fn - 1, offset + W, H], fill=color)

    cv.each(draw)
    img = cv.result()
    if fade_to is not None:
        px = img.load()
        for y in range(H):
            t = max(0.0, min(1.0, (y - (base_fn - 90)) / 90.0))
            for x in range(W):
                r, g, b, a = px[x, y]
                if a:
                    px[x, y] = mix((r, g, b, a), fade_to[:3] + (a,), t * 0.75 * (bayer(x, y) < t))
    if rim:
        img = rim_light(img, rim, rim_strength)
    return img


def layer_mountains():
    noise_a, noise_b = periodic(11), periodic(12)
    cv = Canvas()

    def draw(offset):
        far = [(offset, H)]
        near = [(offset, H)]
        for x in range(0, W + 2, 2):
            far.append((offset + x, H * 0.5 - (noise_a(x) * 0.5 + 0.5) * 70 - abs(noise_b(x * 3)) * 10))
            near.append((offset + x, H * 0.62 - (noise_b(x + 40) * 0.5 + 0.5) * 55))
        far.append((offset + W, H)); near.append((offset + W, H))
        cv.d.polygon(far, fill=hexc("#3b6178"))
        cv.d.polygon(near, fill=hexc("#2f5268"))

    cv.each(draw)
    img = cv.result()
    px = img.load()
    for y in range(H):
        t = max(0.0, min(1.0, (y - H * 0.45) / (GROUND_Y - H * 0.45)))
        for x in range(W):
            r, g, b, a = px[x, y]
            if a and t * 0.8 > bayer(x, y):
                px[x, y] = hexc("#4a7488")
    return rim_light(img, hexc("#9fd4d8"), 90)


def build_far_forest(rnd):
    items = []
    x = 0.0
    while x < W:
        h, w = rnd.uniform(40, 80), rnd.uniform(20, 36)
        items.append(lambda d, o, s, x=x, h=h, w=w: pine(d, o + x, GROUND_Y - 14, h, w, hexc("#2d5068"), s))
        x += rnd.uniform(10, 22)
    return items


def build_giant_trees(rnd):
    items = []
    for i in range(4):
        x = (i + rnd.uniform(0.1, 0.9)) * W / 4
        h, w = rnd.uniform(200, 260), rnd.uniform(16, 26)
        items.append(lambda d, o, s, x=x, h=h, w=w: giant_tree(d, o + x, GROUND_Y - 10, h, w, hexc("#1d3d52"), s))
    return items


def build_mid_forest(rnd):
    items = []
    x = rnd.uniform(0, 20)
    while x < W:
        pick = rnd.random()
        if pick < 0.5:
            h, w = rnd.uniform(90, 150), rnd.uniform(40, 62)
            items.append(lambda d, o, s, x=x, h=h, w=w: pine(d, o + x, GROUND_Y - 4, h, w, hexc("#142c3d"), s))
        elif pick < 0.8:
            h, w = rnd.uniform(80, 120), rnd.uniform(50, 76)
            items.append(lambda d, o, s, x=x, h=h, w=w: round_tree(d, o + x, GROUND_Y - 4, h, w, hexc("#142c3d"), s))
        else:
            h = rnd.uniform(90, 140)
            items.append(lambda d, o, s, x=x, h=h: dead_tree(d, o + x, GROUND_Y - 4, h, hexc("#142c3d"), s))
        x += rnd.uniform(26, 52)
    return items


def layer_fog(seed, y_center, thickness, color, alpha, scale=2):
    noise_a, noise_b = periodic(seed), periodic(seed + 1)

    def density(x, y):
        band = max(0.0, 1 - abs(y - y_center) / thickness)
        v = (noise_a(x) * 0.5 + 0.5) * 0.65 + (noise_b(x * 2 + y * 2) * 0.5 + 0.5) * 0.35
        return band * max(0.0, v * 0.9 - 0.3)

    return dither_fill((W, H), density, color[:3] + (alpha,), scale)


def layer_ruins():
    rnd = random.Random(61)
    stone = hexc("#0e1f2d")
    brick = hexc("#091621")
    cv = Canvas()
    lanterns = []
    spots = [0.12, 0.40, 0.68, 0.90]

    def draw(offset):
        r = random.Random(63)
        for i, frac in enumerate(spots):
            x = offset + frac * W
            kind = i % 4
            if kind == 0:
                stone_arch(cv.d, x, GROUND_Y + 2, 84, 70, stone, brick)
                cv.d.line([(x, GROUND_Y - 118), (x, GROUND_Y - 94)], fill=stone, width=1)
                cv.d.rectangle([x - 3, GROUND_Y - 94, x + 3, GROUND_Y - 86], fill=stone)
            elif kind == 1:
                broken_pillar(cv.d, x, GROUND_Y + 2, 20, 62, stone, r)
                broken_pillar(cv.d, x + 30, GROUND_Y + 2, 16, 34, stone, r)
            elif kind == 2:
                statue(cv.d, x, GROUND_Y + 2, 70, stone)
                cv.d.line([(x + 36, GROUND_Y - 60), (x + 36, GROUND_Y)], fill=stone, width=2)
                cv.d.line([(x + 30, GROUND_Y - 60), (x + 36, GROUND_Y - 64), (x + 42, GROUND_Y - 60)], fill=stone, width=1)
            else:
                stone_arch(cv.d, x, GROUND_Y + 2, 56, 52, stone, brick)
                broken_pillar(cv.d, x + 48, GROUND_Y + 2, 14, 24, stone, r)

    cv.each(draw)
    img = rim_light(cv.result(), hexc("#7fd0d4"), 130)
    draw_l = ImageDraw.Draw(img)
    positions = []
    for i, frac in enumerate(spots):
        if i % 4 == 0:
            lx, ly = int(frac * W), GROUND_Y - 84
            draw_l.rectangle([lx - 3, ly, lx + 3, ly + 7], fill=hexc("#ffcf72"))
            draw_l.rectangle([lx - 1, ly + 2, lx + 1, ly + 5], fill=hexc("#fff3c8"))
            positions.append((lx, ly + 4))
        if i % 4 == 2:
            lx, ly = int(frac * W) + 36, GROUND_Y - 62
            draw_l.rectangle([lx - 3, ly, lx + 3, ly + 7], fill=hexc("#ffcf72"))
            draw_l.rectangle([lx - 1, ly + 2, lx + 1, ly + 5], fill=hexc("#fff3c8"))
            positions.append((lx, ly + 4))
    # fogueira entre as ruínas: pedras, lenha e chamas
    fx = int(0.54 * W)
    for sx in (-9, -5, 5, 9):
        draw_l.ellipse([fx + sx - 3, GROUND_Y - 3, fx + sx + 3, GROUND_Y + 2], fill=hexc("#0b1822"))
    draw_l.line([(fx - 8, GROUND_Y), (fx + 6, GROUND_Y - 5)], fill=hexc("#2a1a14"), width=3)
    draw_l.line([(fx + 8, GROUND_Y), (fx - 6, GROUND_Y - 5)], fill=hexc("#22150f"), width=3)
    for dx, h, col in ((-4, 12, "#d8481f"), (0, 18, "#f08a2a"), (4, 11, "#d8481f")):
        draw_l.polygon([(fx + dx - 3, GROUND_Y - 4), (fx + dx, GROUND_Y - 4 - h), (fx + dx + 3, GROUND_Y - 4)], fill=hexc(col))
    draw_l.polygon([(fx - 2, GROUND_Y - 4), (fx, GROUND_Y - 4 - 11), (fx + 2, GROUND_Y - 4)], fill=hexc("#ffd77a"))
    positions.append((fx, GROUND_Y - 12))
    layer_ruins.campfire = (fx, GROUND_Y - 22)
    layer_ruins.lanterns = positions
    return img


def layer_lantern_glow():
    positions = getattr(layer_ruins, "lanterns", [])
    if not positions:
        layer_ruins()
        positions = layer_ruins.lanterns

    def density(x, y):
        best = 0.0
        for lx, ly in positions:
            d = math.hypot(x - lx, (y - ly) * 1.1)
            radius = 70 if lx == int(0.54 * W) else 46
            best = max(best, max(0.0, 1 - d / radius) ** 1.8)
        return best * 0.95

    return dither_fill((W, H), density, hexc("#ffbf5e", 255), scale=1)


def layer_ground():
    noise = periodic(29, 8)
    rnd = random.Random(33)
    soil, grass, grass_hi = hexc("#08121b"), hexc("#12383a"), hexc("#2c8a84")
    cv = Canvas()

    def top(x):
        return GROUND_Y + noise(x) * 3.5

    def draw(offset):
        pts = [(offset, H)] + [(offset + x, top(x)) for x in range(0, W + 2, 2)] + [(offset + W, H)]
        cv.d.polygon(pts, fill=soil)
        s = random.Random(34)
        for _ in range(260):
            x = s.uniform(0, W)
            h = s.randint(3, 9)
            lean = s.randint(-2, 2)
            y = top(x)
            cv.d.line([(offset + x, y + 1), (offset + x + lean, y - h)], fill=grass if s.random() < 0.7 else grass_hi, width=1)
        for _ in range(9):
            x = s.uniform(0, W)
            rw, rh = s.uniform(10, 22), s.uniform(6, 12)
            y = top(x) + 2
            cv.d.polygon([(offset + x - rw, y + 2), (offset + x - rw * 0.6, y - rh), (offset + x + rw * 0.3, y - rh * 1.1), (offset + x + rw, y + 2)], fill=hexc("#0d1c27"))
        for _ in range(7):
            x = s.uniform(0, W)
            y = top(x)
            length = s.uniform(12, 26)
            cv.d.line([(offset + x, y), (offset + x + s.uniform(-14, 14), y + length)], fill=hexc("#0b1822"), width=2)
        for _ in range(14):
            x = s.uniform(0, W)
            y = top(x)
            h = s.randint(4, 9)
            cv.d.rectangle([offset + x - 1, y - h, offset + x, y], fill=hexc("#17313a"))
            cap = hexc("#5fd3d6") if s.random() < 0.7 else hexc("#ffb457")
            cv.d.ellipse([offset + x - 4, y - h - 4, offset + x + 3, y - h + 1], fill=cap)

    cv.each(draw)
    img = cv.result()
    # sombra no solo, de cima para baixo
    px = img.load()
    for y in range(GROUND_Y + 8, H):
        t = (y - GROUND_Y - 8) / (H - GROUND_Y - 8)
        for x in range(W):
            r, g, b, a = px[x, y]
            if a and t > bayer(x, y):
                px[x, y] = (max(0, r - 4), max(0, g - 8), max(0, b - 10), a)
    return rim_light(img, hexc("#58c8c4"), 120)


def layer_vines():
    cv = Canvas()
    col, leaf = hexc("#0a2428"), hexc("#0f3a3a")

    def draw(offset):
        s = random.Random(71)
        x = 0.0
        while x < W:
            length = s.randint(30, 110)
            sway = s.uniform(2, 5)
            phase = s.uniform(0, math.tau)
            pts = [(offset + x + math.sin(phase + y * 0.09) * sway, y) for y in range(0, length)]
            cv.d.line(pts, fill=col, width=1)
            for y in range(8, length, 7):
                lx = offset + x + math.sin(phase + y * 0.09) * sway
                side = s.choice((-1, 1))
                cv.d.polygon([(lx, y), (lx + side * 5, y + 2), (lx + side * 2, y + 5)], fill=leaf)
            x += s.uniform(8, 26)
        for _ in range(8):
            cx = s.uniform(0, W)
            cv.d.ellipse([offset + cx - 30, -22, offset + cx + 30, 20], fill=col)

    cv.each(draw)
    return rim_light(cv.result(), hexc("#6fd0cf"), 110)


def layer_foreground():
    cv = Canvas()
    col = hexc("#030a0f")

    def draw(offset):
        s = random.Random(81)
        for x, w in ((10, 36), (452, 28)):
            cv.d.polygon([(offset + x - w, H), (offset + x - w * 0.6, 0), (offset + x + w * 0.6, 0), (offset + x + w, H)], fill=col)
        for _ in range(7):
            cx = s.choice((s.uniform(0, 80), s.uniform(400, 480)))
            cy = s.uniform(-6, 40)
            r = s.uniform(20, 42)
            cv.d.ellipse([offset + cx - r, cy - r * 0.6, offset + cx + r, cy + r * 0.6], fill=col)
        for _ in range(11):
            x = s.uniform(0, W)
            ang = s.uniform(-0.5, 0.5)
            for j in range(9):
                t = j / 9.0
                px_ = offset + x + math.sin(ang) * 36 * t * 3
                py_ = H - 6 - math.cos(ang) * 46 * t
                cv.d.line([(px_ - 9 * (1 - t), py_ + 3), (px_, py_), (px_ + 9 * (1 - t), py_ + 3)], fill=col, width=2)

    cv.each(draw)
    return cv.result()


def layer_fog_low():
    noise_a = periodic(91)

    def density(x, y):
        band = max(0.0, (y - H * 0.78) / (H * 0.22))
        return band * ((noise_a(x * 1.0 + y) * 0.5 + 0.5) * 0.9 + 0.1) * 0.55

    return dither_fill((W, H), density, hexc("#88c4d0", 255), scale=1)


def layer_beams():
    noise = periodic(97)

    def density(x, y):
        u = (x + y * 0.55) % 96
        band = max(0.0, 1 - abs(u - 48) / 30.0)
        fade = max(0.0, 1 - y / (H * 0.85))
        return band * fade * (0.3 + 0.25 * (noise(x) * 0.5 + 0.5))

    return dither_fill((W, H), density, hexc("#b9e6ea", 255), scale=1)


LAYERS = [
    ("sky", layer_sky),
    ("clouds", layer_clouds),
    ("mountains", layer_mountains),
    ("forest_far", lambda: silhouette_layer(5, GROUND_Y - 14, build_far_forest, hexc("#2d5068"), hexc("#8fd2d8"), 90)),
    ("fog_far", lambda: layer_fog(7, GROUND_Y - 40, 70, hexc("#8ac0cc"), 150, 1)),
    ("giant_trees", lambda: silhouette_layer(9, GROUND_Y - 10, build_giant_trees, hexc("#1d3d52"), hexc("#86d0d4"), 110)),
    ("forest_mid", lambda: silhouette_layer(13, GROUND_Y - 4, build_mid_forest, hexc("#142c3d"), hexc("#74cfd3"), 120)),
    ("fog_mid", lambda: layer_fog(17, GROUND_Y - 22, 56, hexc("#6aa6b4"), 170, 1)),
    ("ruins", layer_ruins),
    ("lantern_glow", layer_lantern_glow),
    ("ground", layer_ground),
    ("vines", layer_vines),
    ("foreground", layer_foreground),
    ("fog_low", layer_fog_low),
    ("beams", layer_beams),
]


def main():
    os.makedirs(OUT, exist_ok=True)
    for index, (name, fn) in enumerate(LAYERS):
        path = os.path.join(OUT, "forest_%02d_%s.png" % (index, name))
        fn().save(path)
        print(path)


if __name__ == "__main__":
    main()
