"""Gera os quadros do herói em pixel art (20x28).

Uso: python Tools~/pixel_hero.py
Saída: Packages/com.guavovic.parallax/Samples~/ForestDemo/Sprites/Hero/hero_NOME.png
"""
import os

from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "..", "Packages", "com.guavovic.parallax", "Samples~", "ForestDemo", "Sprites", "Hero")

PALETTE = {
    ".": (0, 0, 0, 0),
    "K": (10, 14, 22, 255),     # corpo
    "W": (223, 238, 240, 255),  # máscara
    "E": (6, 9, 13, 255),       # olhos
    "C": (24, 52, 68, 255),     # capa
    "R": (52, 104, 120, 255),   # luz da capa
    "H": (232, 244, 242, 255),  # chifres
    "S": (150, 190, 196, 255),  # sombra da máscara
}

# Cabeça e tronco, iguais em todos os quadros. 18 linhas.
UPPER = [
    "....H........H....",
    "....HH......HH....",
    ".....HH....HH.....",
    ".....HWWWWWWH.....",
    "....KWWWWWWWWK....",
    "...KKWWEWWEWWKK...",
    "...KKWWEWWEWWKK...",
    "...KKWWWWWWWWKK...",
    "....KKSWWWWSKK....",
    ".....KKKKKKKK.....",
    "....CCKKKKKKCC....",
    "...CCCKKKKKKCCC...",
    "..RCCCKKKKKKCCCR..",
    "..RCCCCKKKKCCCCR..",
    ".RCCCCCKKKKCCCCCR.",
    ".RCCCCCCKKCCCCCCR.",
    ".RCCCCCCCCCCCCCCR.",
    ".RCCCCCCCCCCCCCCR.",
]

LEGS = {
    "idle0": [
        "..CCCCCCCCCCCCCC..",
        "...CCCCCCCCCCCC...",
        "....CCC....CCC....",
        ".....KK....KK.....",
        ".....KK....KK.....",
        "....KKK....KKK....",
        "..................",
        "..................",
        "..................",
        "..................",
    ],
    "idle1": [
        ".RCCCCCCCCCCCCCCR.",
        "..CCCCCCCCCCCCCC..",
        "...CCC......CCC...",
        ".....KK....KK.....",
        ".....KK....KK.....",
        "....KKK....KKK....",
        "..................",
        "..................",
        "..................",
        "..................",
    ],
    "walk0": [
        "..CCCCCCCCCCCCCC..",
        "...CCCCCCCCCCCC...",
        "....CCCC...CCC....",
        "....KK......KK....",
        "...KK........KK...",
        "..KKK.........KKK.",
        "..................",
        "..................",
        "..................",
        "..................",
    ],
    "walk1": [
        "..CCCCCCCCCCCCCC..",
        "...CCCCCCCCCCCC...",
        "....CCC....CCC....",
        ".....KK....KK.....",
        ".....KK....KK.....",
        "......K....K......",
        "..................",
        "..................",
        "..................",
        "..................",
    ],
    "walk2": [
        "..CCCCCCCCCCCCCC..",
        "...CCCCCCCCCCCC...",
        "....CCC...CCCC....",
        "....KK......KK....",
        "...KK........KK...",
        ".KKK.........KKK..",
        "..................",
        "..................",
        "..................",
        "..................",
    ],
    "walk3": [
        "..CCCCCCCCCCCCCC..",
        "...CCCCCCCCCCCC...",
        "....CCC....CCC....",
        ".....KK....KK.....",
        ".....KK....KK.....",
        ".....K......K.....",
        "..................",
        "..................",
        "..................",
        "..................",
    ],
    "jump": [
        "..RCCCCCCCCCCCCR..",
        "...CCCCCCCCCCCC...",
        "....CCCCCCCCCC....",
        "....KKK....KKK....",
        ".....KK....KK.....",
        "..................",
        "..................",
        "..................",
        "..................",
        "..................",
    ],
    "fall": [
        ".RCCCCCCCCCCCCCCR.",
        "RCCCCCCCCCCCCCCCCR",
        "..CCCCCCCCCCCCCC..",
        "...KK........KK...",
        "...KK........KK...",
        "..................",
        "..................",
        "..................",
        "..................",
        "..................",
    ],
}

# Quanto o corpo sobe ou desce em cada quadro, em pixels.
BOB = {"idle0": 0, "idle1": 1, "walk0": 1, "walk1": 0, "walk2": 1, "walk3": 0, "jump": 0, "fall": 0}

WIDTH, HEIGHT = 20, 29


def build(name):
    rows = UPPER + LEGS[name]
    bob = BOB[name]
    image = Image.new("RGBA", (WIDTH, HEIGHT), PALETTE["."])
    px = image.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch == ".":
                continue
            px[x + 1, y + bob] = PALETTE[ch]
    return image


def main():
    os.makedirs(OUT, exist_ok=True)
    for name in LEGS:
        path = os.path.join(OUT, "hero_%s.png" % name)
        build(name).save(path)
        print(path)


if __name__ == "__main__":
    main()
