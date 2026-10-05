"""Gera os ícones do pacote em pixel art (16x16, salvos em 32x32 com o pixel dobrado).

Uso: python Tools~/pixel_icons.py
Saída: Packages/com.guavovic.parallax/Editor/Icons/

Mesma paleta da floresta do sample, com contorno escuro para ler bem nos temas claro e escuro.
"""
import os

from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Editor", "Icons")
SIZE = 16
SCALE = 2

OUTLINE = (11, 22, 32, 255)
DARK = (29, 61, 82, 255)
MID = (63, 116, 134, 255)
LIGHT = (159, 212, 216, 255)
WHITE = (228, 244, 246, 255)
ORANGE = (240, 160, 64, 255)
YELLOW = (255, 211, 107, 255)


def canvas():
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


def card(d, x, y, w, h, fill, top):
    """Uma camada: retângulo com contorno e uma faixa clara no alto."""
    d.rectangle([x, y, x + w - 1, y + h - 1], fill=OUTLINE)
    d.rectangle([x + 1, y + 1, x + w - 2, y + h - 2], fill=fill)
    d.line([x + 1, y + 1, x + w - 2, y + 1], fill=top)


def mountains(d, x, y, w, h, color):
    """Duas montanhas encostadas no pé de um card."""
    base = y + h - 2
    d.polygon([(x + 1, base), (x + 4, base - 3), (x + 6, base - 1), (x + 8, base - 4), (x + w - 2, base)], fill=color)


def icon_rig():
    img, d = canvas()
    card(d, 5, 1, 10, 7, DARK, MID)
    card(d, 3, 4, 10, 7, MID, LIGHT)
    card(d, 1, 8, 10, 7, LIGHT, WHITE)
    mountains(d, 1, 8, 10, 7, DARK)
    d.point((8, 10), fill=YELLOW)
    return img


def icon_layer():
    img, d = canvas()
    card(d, 1, 3, 14, 10, LIGHT, WHITE)
    mountains(d, 1, 3, 14, 10, DARK)
    d.polygon([(6, 11), (9, 7), (12, 11)], fill=MID)
    d.rectangle([10, 5, 11, 6], fill=YELLOW)
    return img


def icon_world():
    img, d = canvas()
    d.ellipse([2, 2, 13, 13], fill=OUTLINE)
    d.ellipse([3, 3, 12, 12], fill=MID)
    d.ellipse([4, 4, 8, 8], fill=LIGHT)
    for y in (6, 10):
        d.line([0, y, 15, y], fill=OUTLINE)
        d.line([1, y, 14, y], fill=WHITE)
    return img


def wind_lines(d, x0, x1):
    for y, inset in ((4, 0), (8, 2), (12, 1)):
        d.line([x0 + inset, y, x1, y], fill=MID)
        d.point((x1, y), fill=LIGHT)


def icon_wind_influencer():
    """Um corpo que levanta vento atrás de si."""
    img, d = canvas()
    wind_lines(d, 0, 7)
    d.rectangle([8, 4, 15, 12], fill=OUTLINE)
    d.rectangle([9, 5, 14, 11], fill=ORANGE)
    d.line([9, 5, 14, 5], fill=YELLOW)
    return img


def icon_wind_receiver():
    """O vento chega e empurra o corpo."""
    img, d = canvas()
    d.rectangle([0, 4, 7, 12], fill=OUTLINE)
    d.rectangle([1, 5, 6, 11], fill=ORANGE)
    d.line([1, 5, 6, 5], fill=YELLOW)
    for y in (4, 8, 12):
        d.line([9, y, 15, y], fill=MID)
        d.point((9, y), fill=LIGHT)
    d.polygon([(8, 8), (10, 6), (10, 10)], fill=MID)
    return img


def icon_profile():
    img, d = canvas()
    d.polygon([(2, 0), (11, 0), (14, 3), (14, 15), (2, 15)], fill=OUTLINE)
    d.polygon([(3, 1), (10, 1), (13, 4), (13, 14), (3, 14)], fill=WHITE)
    d.polygon([(10, 1), (13, 4), (10, 4)], fill=LIGHT)
    for y, color in ((6, DARK), (9, MID), (12, LIGHT)):
        d.rectangle([5, y, 11, y + 1], fill=color)
    return img


ICONS = {
    "ParallaxRig": icon_rig,
    "ParallaxLayer": icon_layer,
    "ParallaxWorld": icon_world,
    "ParallaxWindInfluencer2D": icon_wind_influencer,
    "ParallaxWindReceiver2D": icon_wind_receiver,
    "ParallaxProfile": icon_profile,
}


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, build in ICONS.items():
        img = build().resize((SIZE * SCALE, SIZE * SCALE), Image.NEAREST)
        path = os.path.join(OUT, name + ".png")
        img.save(path)
        print(path)


if __name__ == "__main__":
    main()
