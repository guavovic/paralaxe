"""Gera os quadros do herói em pixel art (20x29; os do ataque têm 34x29).

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
    "N": (196, 214, 220, 255),  # lâmina
    "G": (120, 140, 150, 255),  # cabo
    "A": (240, 250, 250, 255),  # arco do golpe
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
    "crouch": [
        "RCCCCCCCCCCCCCCCCR",
        "..KKK........KKK..",
        "..................",
        "..................",
        "..................",
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
BOB = {"idle0": 0, "idle1": 1, "walk0": 1, "walk1": 0, "walk2": 1, "walk3": 0, "jump": 0, "fall": 0, "crouch": 4}

WIDTH, HEIGHT = 20, 29

# Os quadros do ataque são mais largos, para caber o arco do golpe à frente do corpo.
# O corpo fica na mesma posição dos outros quadros, e o pivô do sprite fica no centro dele.
ATTACK_WIDTH = 34


def build(name, width=WIDTH):
    rows = UPPER + LEGS[name]
    bob = BOB[name]
    image = Image.new("RGBA", (width, HEIGHT), PALETTE["."])
    px = image.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch == ".":
                continue
            px[x + 1, y + bob] = PALETTE[ch]
    return image


def line(px, a, b, color):
    """Linha pixel a pixel (Bresenham), sem suavizar."""
    (x0, y0), (x1, y1) = a, b
    dx, dy = abs(x1 - x0), -abs(y1 - y0)
    sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
    err = dx + dy
    while True:
        px[x0, y0] = color
        if (x0, y0) == (x1, y1):
            return
        e2 = 2 * err
        if e2 >= dy:
            err += dy
            x0 += sx
        if e2 <= dx:
            err += dx
            y0 += sy


def crescent(px, outer, inner, color, edge, dither=False, min_x=19):
    """Meia-lua do golpe: dentro do círculo de fora e fora do de dentro, à frente do corpo.
    Grossa no meio e fina nas pontas. Com dither, só metade dos pixels, para o golpe sumindo."""
    (ox, oy, orad), (ix, iy, irad) = outer, inner
    for y in range(HEIGHT):
        for x in range(min_x, ATTACK_WIDTH):
            if (x - ox) ** 2 + (y - oy) ** 2 > orad ** 2 or (x - ix) ** 2 + (y - iy) ** 2 <= irad ** 2:
                continue
            if dither and (x + y) % 2:
                continue
            if px[x, y][3] == 0:
                near_edge = (x - ox) ** 2 + (y - oy) ** 2 > (orad - 1.2) ** 2
                px[x, y] = edge if near_edge else color


def build_attack(frame, kind="slash"):
    if kind != "slash":
        return build_variation(frame, kind)
    image = build("walk1", ATTACK_WIDTH)
    px = image.load()
    if frame == 0:
        # Preparação: a lâmina sobe por trás do ombro.
        line(px, (15, 15), (16, 14), PALETTE["G"])
        line(px, (16, 13), (21, 6), PALETTE["N"])
    elif frame == 1:
        # Golpe: a meia-lua aberta à frente, com a lâmina dentro dela.
        crescent(px, (20, 16, 12), (15, 16, 11), PALETTE["A"], PALETTE["N"])
        line(px, (16, 17), (18, 17), PALETTE["G"])
        line(px, (19, 17), (26, 17), PALETTE["N"])
    else:
        # Acompanhamento: a lâmina desce e a meia-lua se desfaz.
        crescent(px, (21, 17, 11), (17, 17, 10), PALETTE["A"], PALETTE["N"], dither=True)
        line(px, (16, 18), (18, 19), PALETTE["G"])
        line(px, (19, 20), (24, 23), PALETTE["N"])
    return image


def build_variation(frame, kind):
    """Golpe subindo (segundo do combo), golpe agachado e golpe no ar."""
    body = {"up": "walk1", "crouch": "crouch", "air": "jump"}[kind]
    image = build(body, ATTACK_WIDTH)
    px = image.load()
    blade, grip = PALETTE["N"], PALETTE["G"]
    if kind == "up":
        if frame == 0:
            line(px, (15, 18), (16, 19), grip)
            line(px, (17, 20), (24, 24), blade)
        elif frame == 1:
            crescent(px, (19, 12, 12), (14, 17, 11), PALETTE["A"], blade)
            line(px, (17, 15), (18, 14), grip)
            line(px, (19, 13), (25, 7), blade)
        else:
            crescent(px, (19, 11, 11), (15, 15, 10), PALETTE["A"], blade, dither=True)
            line(px, (17, 13), (19, 9), blade)
    elif kind == "crouch":
        if frame == 0:
            line(px, (15, 19), (16, 18), grip)
            line(px, (17, 17), (21, 13), blade)
        elif frame == 1:
            crescent(px, (21, 21, 8), (17, 21, 7), PALETTE["A"], blade)
            line(px, (16, 21), (18, 21), grip)
            line(px, (19, 21), (26, 21), blade)
        else:
            crescent(px, (21, 21, 8), (18, 21, 7), PALETTE["A"], blade, dither=True)
            line(px, (17, 22), (23, 23), blade)
    else:
        if frame == 0:
            line(px, (15, 14), (16, 13), grip)
            line(px, (16, 12), (20, 4), blade)
        elif frame == 1:
            crescent(px, (19, 19, 11), (14, 14, 11), PALETTE["A"], blade, min_x=10)
            line(px, (17, 18), (18, 19), grip)
            line(px, (19, 20), (25, 26), blade)
        else:
            crescent(px, (19, 20, 10), (15, 15, 10), PALETTE["A"], blade, dither=True, min_x=10)
            line(px, (17, 20), (21, 26), blade)
    return image


def build_double_jump():
    """Pulo duplo: a capa abre e um sopro de ar sai debaixo dos pés."""
    image = build("fall")
    px = image.load()
    for x, y in ((6, 25), (8, 26), (10, 25), (12, 26), (14, 25), (9, 27), (11, 27), (7, 27), (13, 27)):
        px[x, y] = PALETTE["A"]
    return image


def main():
    os.makedirs(OUT, exist_ok=True)
    for name in LEGS:
        path = os.path.join(OUT, "hero_%s.png" % name)
        build(name).save(path)
        print(path)
    for frame in range(3):
        path = os.path.join(OUT, "hero_attack%d.png" % frame)
        build_attack(frame).save(path)
        print(path)
    for kind in ("up", "crouch", "air"):
        for frame in range(3):
            path = os.path.join(OUT, "hero_attack_%s%d.png" % (kind, frame))
            build_attack(frame, kind).save(path)
            print(path)
    path = os.path.join(OUT, "hero_jump2.png")
    build_double_jump().save(path)
    print(path)


if __name__ == "__main__":
    main()
