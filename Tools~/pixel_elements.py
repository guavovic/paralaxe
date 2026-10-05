"""Gera os elementos interativos do sample: o que o herói quebra e as plataformas em que sobe.

Uso: python Tools~/pixel_elements.py
Saída: Packages/com.guavovic.parallax/Samples~/ForestDemo/Sprites/Elements/

Cada elemento tem contorno escuro e luz de contorno no alto, como as silhuetas do cenário.
"""
import os
import random

from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "ForestDemo", "Sprites", "Elements")

OUTLINE = (6, 10, 16, 255)


def hexc(value, alpha=255):
    value = value.lstrip("#")
    return tuple(int(value[i:i + 2], 16) for i in (0, 2, 4)) + (alpha,)


def outlined(size, draw_body):
    """Desenha o corpo, depois põe o contorno escuro em volta (1 px) e devolve a imagem."""
    w, h = size
    body = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw_body(ImageDraw.Draw(body), body)
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    src, dst = body.load(), out.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3]:
                continue
            if any(0 <= x + dx < w and 0 <= y + dy < h and src[x + dx, y + dy][3] for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                dst[x, y] = OUTLINE
    out.alpha_composite(body)
    return out


def pot(w, h):
    clay, dark, light = hexc("#7a4a35"), hexc("#4e2c20"), hexc("#c08060")

    def draw(d, img):
        d.ellipse([1, h * 0.25, w - 2, h - 2], fill=clay)
        d.rectangle([w * 0.3, 1, w * 0.7 - 1, h * 0.35], fill=clay)
        d.rectangle([w * 0.22, 1, w * 0.78 - 1, 2], fill=light)
        d.line([2, h * 0.55, w - 3, h * 0.55], fill=dark)
        d.line([3, h * 0.45, 3, h * 0.75], fill=light)
    return outlined((w, h), draw)


def crate():
    wood, dark, light = hexc("#5c4630"), hexc("#3a2b1d"), hexc("#9a7a52")

    def draw(d, img):
        d.rectangle([1, 1, 14, 14], fill=wood)
        d.rectangle([1, 1, 14, 2], fill=light)
        d.line([1, 1, 14, 14], fill=dark)
        d.line([14, 1, 1, 14], fill=dark)
        d.rectangle([1, 7, 14, 8], fill=dark)
    return outlined((16, 16), draw)


def mushroom():
    cap, glow, stem = hexc("#2a9aa0"), hexc("#7de8e0"), hexc("#c8d8d0")

    def draw(d, img):
        d.rectangle([6, 7, 9, 13], fill=stem)
        d.pieslice([1, 1, 14, 12], 180, 360, fill=cap)
        for x, y in ((4, 4), (8, 3), (11, 5), (6, 6)):
            d.point((x, y), fill=glow)
        d.line([3, 6, 12, 6], fill=glow)
    return outlined((16, 14), draw)


def crystal(color_light, color_dark, h):
    w = 14

    def draw(d, img):
        for dx, scale, lean in ((-3, 0.65, -2), (3, 0.75, 2), (0, 1.0, 0)):
            cx, ch = w / 2 + dx, (h - 2) * scale
            tip = (cx + lean, h - 1 - ch)
            d.polygon([(cx - 2, h - 1), (cx - 2, h - 1 - ch * 0.6), tip, (cx + 2, h - 1 - ch * 0.6), (cx + 2, h - 1)], fill=color_dark)
            d.polygon([(cx - 2, h - 1), (cx - 2, h - 1 - ch * 0.6), tip, (cx, h - 1 - ch * 0.58), (cx, h - 1)], fill=color_light)
    return outlined((w, h), draw)


def geode():
    rock, light, inner = hexc("#2a2f5a"), hexc("#9aa8ff"), hexc("#ff7de0")

    def draw(d, img):
        d.polygon([(1, 11), (2, 5), (6, 1), (12, 2), (15, 6), (15, 11)], fill=rock)
        d.line([3, 5, 6, 2], fill=light)
        d.line([6, 2, 11, 2], fill=light)
        d.polygon([(6, 8), (8, 5), (10, 8), (8, 10)], fill=inner)
    return outlined((17, 13), draw)


def ledge(top, top_light, rock, seed):
    w, h = 64, 14
    rnd = random.Random(seed)

    def draw(d, img):
        d.polygon([(1, 3), (w - 2, 3), (w - 4, 9), (w - 12, h - 1), (10, h - 1), (3, 9)], fill=rock)
        d.rectangle([1, 2, w - 2, 4], fill=top)
        d.line([2, 2, w - 3, 2], fill=top_light)
        for _ in range(10):
            x = rnd.randrange(3, w - 3)
            d.point((x, 1), fill=top_light)
    return outlined((w, h), draw)


def main():
    os.makedirs(OUT, exist_ok=True)
    items = {
        "pot_small": pot(10, 12),
        "pot_big": pot(14, 16),
        "crate": crate(),
        "mushroom": mushroom(),
        "crystal_cyan": crystal(hexc("#7de8ff"), hexc("#2f7f9a"), 18),
        "crystal_pink": crystal(hexc("#ff7de0"), hexc("#8f3a80"), 15),
        "geode": geode(),
        "ledge_forest": ledge(hexc("#2f6a52"), hexc("#74cfa0"), hexc("#142c3d"), 3),
        "ledge_cave": ledge(hexc("#1d2350"), hexc("#9aa8ff"), hexc("#10152f"), 5),
    }
    for name, image in items.items():
        image.save(os.path.join(OUT, name + ".png"))
    print(len(items), "elementos")


if __name__ == "__main__":
    main()
