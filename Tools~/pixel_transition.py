"""Gera os blocos de transição da floresta para a caverna, camada por camada.

Uso: python Tools~/pixel_transition.py
Saída: Packages/com.guavovic.parallax/Samples~/Demo/Sprites/Transition/forest_to_cave_NN.png

Cada bloco começa igual à floresta na borda esquerda e termina igual à caverna na direita, misturando as duas
em dithering no meio. Como floresta e caverna são loops sem emenda, as bordas do bloco casam com os vizinhos.
"""
import glob
import math
import os
import sys

from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from pixel_forest import bayer, periodic  # noqa: E402

BASE = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "Demo", "Sprites")
OUT = os.path.join(BASE, "Transition")


def blend(left, right, seed):
    w, h = left.size
    noise = periodic(seed, 5)
    out = Image.new("RGBA", (w, h))
    a, b, px = left.load(), right.load(), out.load()
    for y in range(h):
        # a costura ondula um pouco na vertical, para não parecer uma linha reta
        wobble = noise(y * 3) * 0.12
        for x in range(w):
            t = min(1.0, max(0.0, (x / (w - 1) - 0.15) / 0.7 + wobble))
            t = t * t * (3 - 2 * t)
            px[x, y] = b[x, y] if t > bayer(x, y) else a[x, y]
    return out


def main():
    os.makedirs(OUT, exist_ok=True)
    forest = sorted(glob.glob(os.path.join(BASE, "Forest", "forest_*.png")))
    cave = sorted(glob.glob(os.path.join(BASE, "Cave", "cave_*.png")))
    for index, (f, c) in enumerate(zip(forest, cave)):
        image = blend(Image.open(f).convert("RGBA"), Image.open(c).convert("RGBA"), 200 + index)
        path = os.path.join(OUT, "forest_to_cave_%02d.png" % index)
        image.save(path)
    print(len(forest), "blocos de transição")


if __name__ == "__main__":
    main()
