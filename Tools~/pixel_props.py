"""Gera os adereços pequenos do sample: ponto de luz, fumaça e pássaros.

Uso: python Tools~/pixel_props.py
Saída: Packages/com.guavovic.parallax/Samples~/ForestDemo/Sprites/Props/
"""
import math
import os

from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "ForestDemo", "Sprites", "Props")
BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


def dot_glow(size=9):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = img.load()
    c = (size - 1) / 2.0
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - c, y - c) / c
            v = max(0.0, 1.0 - d) ** 1.5
            if v > (BAYER[y % 4][x % 4] + 0.5) / 16.0 * 0.9:
                px[x, y] = (255, 255, 255, 255)
    px[int(c), int(c)] = (255, 255, 255, 255)
    return img


def smoke_puff(size=14):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    c = size / 2.0
    for r, shade in ((size * 0.5, 120), (size * 0.38, 175), (size * 0.24, 225)):
        draw.ellipse([c - r, c - r, c + r - 1, c + r - 1], fill=(shade, shade, shade, 255))
    return img


def bird(frame):
    # asas: 0 em cima, 1 aberta, 2 embaixo, 3 aberta
    img = Image.new("RGBA", (13, 8), (0, 0, 0, 0))
    px = img.load()
    body = (10, 16, 24, 255)
    for x in range(5, 8):
        px[x, 4] = body
    px[8, 3] = body
    px[4, 4] = body
    wing = {0: [(3, 2), (2, 1), (1, 0), (9, 2), (10, 1), (11, 0)],
            1: [(3, 3), (2, 3), (1, 3), (9, 3), (10, 3), (11, 3)],
            2: [(3, 5), (2, 6), (1, 7), (9, 5), (10, 6), (11, 7)],
            3: [(3, 3), (2, 3), (1, 3), (9, 3), (10, 3), (11, 3)]}[frame]
    for x, y in wing:
        px[x, y] = body
    return img


def speech_bubble():
    # balão de fala 26x14: contorno escuro, miolo claro e uma ponta virada para a cabeça
    ink, fill = (10, 14, 22, 255), (223, 238, 240, 255)
    img = Image.new("RGBA", (26, 14), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw.rectangle([0, 0, 25, 10], fill=ink)
    draw.rectangle([1, 1, 24, 9], fill=fill)
    for x, y in ((0, 0), (25, 0), (0, 10), (25, 10)):
        img.putpixel((x, y), (0, 0, 0, 0))
    for x, y in ((1, 1), (24, 1), (1, 9), (24, 9)):
        img.putpixel((x, y), ink)
    for x, y in ((4, 10), (5, 10), (6, 10)):
        img.putpixel((x, y), fill)
    for x, y in ((3, 10), (7, 10), (4, 11), (6, 11), (4, 12), (5, 11), (5, 12)):
        img.putpixel((x, y), ink)
    for x, y in ((5, 11),):
        img.putpixel((x, y), fill)
    return img


def bubble_dot():
    img = Image.new("RGBA", (2, 2), (10, 14, 22, 255))
    return img


def main():
    os.makedirs(OUT, exist_ok=True)
    dot_glow().save(os.path.join(OUT, "dot_glow.png"))
    speech_bubble().save(os.path.join(OUT, "bubble.png"))
    bubble_dot().save(os.path.join(OUT, "bubble_dot.png"))
    smoke_puff().save(os.path.join(OUT, "smoke_puff.png"))
    for frame in range(4):
        bird(frame).save(os.path.join(OUT, "bird_%d.png" % frame))
    print("ok")


if __name__ == "__main__":
    main()
