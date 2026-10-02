"""Gera as camadas de floresta do sample, com loop horizontal sem emenda.

Uso: python Tools~/generate_forest_layers.py
Saída: Assets/Sprites/Generated/Forest/forest_NN_nome.png (2048x1152, RGBA)
"""
import math
import os
import random

from PIL import Image, ImageChops, ImageDraw, ImageFilter

W, H = 2048, 1152
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Sprites", "Generated", "Forest")


def hexcolor(value, alpha=255):
    value = value.lstrip("#")
    return tuple(int(value[i:i + 2], 16) for i in (0, 2, 4)) + (alpha,)


def lerp(a, b, t):
    return a + (b - a) * t


def lerp_color(a, b, t):
    return tuple(int(lerp(a[i], b[i], t)) for i in range(4))


class Tile:
    """Canvas com três cópias lado a lado. Tudo que é desenhado aparece nas três, então o recorte do meio faz loop."""

    def __init__(self):
        self.image = Image.new("RGBA", (W * 3, H), (0, 0, 0, 0))
        self.draw = ImageDraw.Draw(self.image)

    def each(self, fn):
        for k in range(3):
            fn(k * W)

    def blur(self, radius):
        self.image = self.image.filter(ImageFilter.GaussianBlur(radius))
        self.draw = ImageDraw.Draw(self.image)

    def result(self):
        return self.image.crop((W, 0, W * 2, H))


def recolor(image, color):
    """Mantém só o alpha do blur e fixa a cor, porque o blur mistura o RGB com o preto transparente e escurece as bordas."""
    alpha = image.split()[3]
    solid = Image.new("RGBA", image.size, color[:3] + (255,))
    solid.putalpha(alpha)
    return solid


def periodic_noise(seed, harmonics=7):
    rnd = random.Random(seed)
    parts = [(k, rnd.uniform(0, math.tau), 1.0 / (0.6 + k * 0.45)) for k in range(1, harmonics + 1)]
    total = sum(p[2] for p in parts)

    def noise(x):
        return sum(a * math.sin(math.tau * k * x / W + ph) for k, ph, a in parts) / total

    return noise


def vertical_gradient(top, bottom, y0=0, y1=H):
    layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(layer)
    for y in range(y0, y1):
        draw.line([(0, y), (W, y)], fill=lerp_color(top, bottom, (y - y0) / max(1, y1 - y0 - 1)))
    return layer


def pine(draw, x, base, height, width, color, rnd, tiers=7):
    trunk = max(4, int(width * 0.08))
    draw.rectangle([x - trunk, base - height * 0.2, x + trunk, base], fill=color)
    for i in range(tiers):
        t = i / tiers
        y_top = base - height * (0.15 + 0.85 * (i + 1) / tiers)
        y_bot = base - height * (0.15 + 0.85 * i / tiers) + height * 0.04
        half = width * 0.5 * (1.0 - t * 0.85) * rnd.uniform(0.9, 1.1)
        draw.polygon([(x - half, y_bot), (x, y_top), (x + half, y_bot)], fill=color)


def round_tree(draw, x, base, height, width, color, rnd):
    trunk = max(5, int(width * 0.07))
    draw.rectangle([x - trunk, base - height * 0.55, x + trunk, base], fill=color)
    for _ in range(9):
        cx = x + rnd.uniform(-width * 0.38, width * 0.38)
        cy = base - height * rnd.uniform(0.5, 0.95)
        r = width * rnd.uniform(0.16, 0.3)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=color)


def layer_sky():
    layer = vertical_gradient(hexcolor("#08101f"), hexcolor("#1f5560"), 0, int(H * 0.78))
    draw = ImageDraw.Draw(layer)
    draw.rectangle([0, int(H * 0.78), W, H], fill=hexcolor("#1f5560"))
    glow = Tile()
    for k in range(3):
        cx, cy = k * W + W * 0.32, H * 0.46
        for i in range(60):
            r = 520 * (1 - i / 60)
            alpha = int(3 + i * 0.9)
            glow.draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=hexcolor("#ffd9a0", alpha))
    glow.blur(40)
    layer = Image.alpha_composite(layer, glow.result())
    stars = ImageDraw.Draw(layer)
    rnd = random.Random(3)
    for _ in range(120):
        x, y = rnd.randrange(W), rnd.randrange(int(H * 0.45))
        stars.point((x, y), fill=hexcolor("#cfe8ff", rnd.randrange(60, 200)))
    return layer


def layer_mountains():
    tile = Tile()
    noise = periodic_noise(11)
    color_top = hexcolor("#27546a")
    color_base = hexcolor("#143347")

    def draw(offset):
        points = [(offset, H)]
        for x in range(0, W + 8, 8):
            y = H * 0.56 - (noise(x) * 0.5 + 0.5) * H * 0.2 - abs(noise(x * 2.3)) * H * 0.05
            points.append((offset + x, y))
        points.append((offset + W, H))
        tile.draw.polygon(points, fill=color_base)

    tile.each(draw)
    haze = vertical_gradient(hexcolor("#27546a", 0), hexcolor("#27546a", 190), int(H * 0.3), int(H * 0.82))
    tile.blur(3)
    base = tile.result()
    fade = Image.alpha_composite(Image.new("RGBA", (W, H), (0, 0, 0, 0)), haze)
    fade = ImageChops.multiply(fade, Image.merge("RGBA", (base.split()[3],) * 4))
    return Image.alpha_composite(base, fade)


def layer_forest(seed, base_y, height_range, width_range, spacing, color, kind="pine", glow=None):
    rnd = random.Random(seed)
    tile = Tile()
    x = rnd.uniform(0, spacing)
    items = []
    while x < W:
        items.append((x, rnd.uniform(*height_range), rnd.uniform(*width_range), rnd.random()))
        x += spacing * rnd.uniform(0.6, 1.3)

    def draw(offset):
        state = random.Random(seed + 1)
        for x, height, width, pick in items:
            fn = round_tree if (kind == "mixed" and pick > 0.55) else pine
            fn(tile.draw, offset + x, base_y, height, width, color, state) if fn is round_tree else pine(tile.draw, offset + x, base_y, height, width, color, state)
        tile.draw.rectangle([offset, base_y - 2, offset + W, H], fill=color)

    tile.each(draw)
    tile.blur(1.2)
    result = tile.result()
    if glow:
        edge = result.filter(ImageFilter.GaussianBlur(10))
        edge = ImageChops.subtract(edge.split()[3], result.split()[3])
        tint = Image.new("RGBA", (W, H), glow)
        tint.putalpha(edge.point(lambda v: min(255, v * 2)))
        result = Image.alpha_composite(tint, result)
    return result


def layer_mist(seed, y_center, color, strength, thickness):
    rnd = random.Random(seed)
    tile = Tile()
    puffs = [(rnd.uniform(0, W), rnd.uniform(-thickness, thickness), rnd.uniform(160, 420), rnd.uniform(25, 70)) for _ in range(34)]

    def draw(offset):
        for x, dy, rx, ry in puffs:
            cx, cy = offset + x, y_center + dy
            tile.draw.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], fill=color[:3] + (strength,))

    tile.each(draw)
    tile.blur(38)
    return recolor(tile.result(), color)


def layer_ground():
    tile = Tile()
    noise = periodic_noise(29, 9)
    soil = hexcolor("#0a1218")
    grass = hexcolor("#102a2c")
    rnd = random.Random(31)

    def top(x):
        return H * 0.815 + noise(x) * 10

    def draw(offset):
        points = [(offset, H)]
        for x in range(0, W + 6, 6):
            points.append((offset + x, top(x)))
        points.append((offset + W, H))
        tile.draw.polygon(points, fill=soil)
        state = random.Random(33)
        for _ in range(520):
            x = state.uniform(0, W)
            h = state.uniform(14, 46)
            lean = state.uniform(-9, 9)
            y = top(x)
            tile.draw.polygon([(offset + x - 3, y + 2), (offset + x + lean, y - h), (offset + x + 3, y + 2)], fill=grass)

    tile.each(draw)
    tile.blur(0.8)
    result = tile.result()
    shade = vertical_gradient(hexcolor('#16262c', 0), hexcolor('#000000', 150), int(H * 0.81), H)
    shade = ImageChops.multiply(shade, Image.merge('RGBA', (result.split()[3],) * 4))
    result = Image.alpha_composite(result, shade)
    glow = Tile()

    def rim(offset):
        pts = [(offset + x, top(x) - 1) for x in range(0, W + 6, 6)]
        glow.draw.line(pts, fill=hexcolor("#3b9a8a", 150), width=4)

    glow.each(rim)
    glow.blur(5)
    return Image.alpha_composite(glow.result(), result)


def layer_foreground():
    rnd = random.Random(41)
    tile = Tile()
    color = hexcolor("#03070a")
    trunks = [(W * 0.05, 80), (W * 0.93, 64)]
    leaves = [(rnd.uniform(0, W), rnd.uniform(0, H * 0.2), rnd.uniform(50, 130)) for _ in range(14)]

    def draw(offset):
        for x, width in trunks:
            tile.draw.polygon([(offset + x - width * 0.5, H), (offset + x - width * 0.38, -10), (offset + x + width * 0.38, -10), (offset + x + width * 0.5, H)], fill=color)
        for x, y, r in leaves:
            tile.draw.ellipse([offset + x - r, y - r * 0.5, offset + x + r, y + r * 0.5], fill=color)
        state = random.Random(43)
        for _ in range(26):
            x = state.uniform(0, W)
            length = state.uniform(90, 260)
            tile.draw.line([(offset + x, 0), (offset + x + state.uniform(-30, 30), length)], fill=color, width=state.randrange(2, 5))

    tile.each(draw)
    tile.blur(5)
    return tile.result()


def layer_light_beams():
    tile = Tile()
    rnd = random.Random(53)
    beams = [(rnd.uniform(0, W), rnd.uniform(90, 220)) for _ in range(7)]

    def draw(offset):
        for x, width in beams:
            tile.draw.polygon([(offset + x, 0), (offset + x + width, 0), (offset + x + width + 520, H), (offset + x + 360, H)], fill=hexcolor("#a8d8d0", 4))

    tile.each(draw)
    tile.blur(30)
    result = recolor(tile.result(), hexcolor("#a8d8d0"))
    dust = ImageDraw.Draw(result)
    state = random.Random(57)
    for _ in range(260):
        x, y = state.randrange(W), state.randrange(H)
        dust.ellipse([x, y, x + 2, y + 2], fill=hexcolor("#d8f0ff", state.randrange(20, 60)))
    return result


LAYERS = [
    ("sky", layer_sky),
    ("mountains", layer_mountains),
    ("forest_far", lambda: layer_forest(5, H * 0.74, (200, 330), (100, 160), 80, hexcolor("#163545"), "pine")),
    ("mist_far", lambda: layer_mist(7, H * 0.68, hexcolor("#7fb7c4"), 70, 40)),
    ("forest_mid", lambda: layer_forest(9, H * 0.8, (320, 520), (150, 240), 130, hexcolor("#0f2733"), "mixed", hexcolor("#3a8d92", 42))),
    ("mist_near", lambda: layer_mist(13, H * 0.76, hexcolor("#5f9fae"), 90, 55)),
    ("forest_near", lambda: layer_forest(17, H * 0.86, (480, 760), (220, 340), 240, hexcolor("#08161e"), "mixed", hexcolor("#2d7078", 36))),
    ("ground", layer_ground),
    ("foreground", layer_foreground),
    ("beams", layer_light_beams),
]


def main():
    os.makedirs(OUT, exist_ok=True)
    for index, (name, fn) in enumerate(LAYERS):
        path = os.path.join(OUT, "forest_%02d_%s.png" % (index, name))
        fn().save(path)
        print(path)


if __name__ == "__main__":
    main()
