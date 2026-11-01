#!/usr/bin/env python3
"""生成配套工程用的全部 UI 精灵（原创、程序绘制，可随工程自由分发）。
输出到 project/Assets/UGUIBook/_Core/Sprites/，同时写 sprites.json（九宫格边距），由 BookSpriteImporter 读取。
用法：python3 tools/gen_sprites.py
"""
import json, math, random
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter

OUT = Path(__file__).resolve().parent.parent / "project/Assets/UGUIBook/_Core/Sprites"
OUT.mkdir(parents=True, exist_ok=True)
SS = 4  # 超采样倍数
META = {}  # name -> {"border":[l,b,r,t]}
W = (255, 255, 255, 255)


def canvas(w, h):
    return Image.new("RGBA", (w * SS, h * SS), (0, 0, 0, 0))


def save(img, name, w, h, border=None):
    img = img.resize((w, h), Image.LANCZOS)
    img.save(OUT / f"{name}.png")
    META[name] = {"border": border or [0, 0, 0, 0]}


def s(v):  # 逻辑坐标 -> 超采样坐标
    return v * SS


# ---------------- 九宫格底图 ----------------
def rounded(name, size, r, outline=0, fill=W):
    im = canvas(size, size); d = ImageDraw.Draw(im)
    if outline:
        d.rounded_rectangle([0, 0, s(size) - 1, s(size) - 1], s(r), fill=W)
        d.rounded_rectangle([s(outline), s(outline), s(size - outline) - 1, s(size - outline) - 1], s(max(r - outline, 0)), fill=(0, 0, 0, 0))
    else:
        d.rounded_rectangle([0, 0, s(size) - 1, s(size) - 1], s(r), fill=fill)
    b = r + 2
    save(im, name, size, size, [b, b, b, b])


for r in (4, 8, 16, 24, 32):
    rounded(f"round_{r}", max(64, r * 2 + 8), r)
rounded("round_outline_8", 48, 8, outline=2)
rounded("round_outline_16", 64, 16, outline=3)

# 胶囊
im = canvas(128, 64); d = ImageDraw.Draw(im)
d.rounded_rectangle([0, 0, s(128) - 1, s(64) - 1], s(32), fill=W)
save(im, "pill", 128, 64, [32, 32, 32, 32])

# 软阴影（九宫格，中间实心）
def shadow(name, r, blur):
    size = 2 * (r + blur * 2) + 16
    im = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    m = Image.new("L", (size, size), 0); d = ImageDraw.Draw(m)
    p = blur * 2
    d.rounded_rectangle([p, p, size - p - 1, size - p - 1], r, fill=255)
    m = m.filter(ImageFilter.GaussianBlur(blur))
    im.putalpha(m)
    w = Image.new("RGBA", (size, size), (255, 255, 255, 0)); w.putalpha(m)
    w.save(OUT / f"{name}.png")
    b = r + p + 2
    META[name] = {"border": [b, b, b, b]}


shadow("shadow_16", 16, 10)
shadow("glow_24", 24, 14)

# 背包格子：深色圆角 + 内描边（彩色，不靠染色）
im = canvas(96, 96); d = ImageDraw.Draw(im)
d.rounded_rectangle([0, 0, s(96) - 1, s(96) - 1], s(14), fill=(46, 52, 74, 255))
d.rounded_rectangle([s(3), s(3), s(93) - 1, s(93) - 1], s(11), fill=(64, 72, 100, 255))
d.rounded_rectangle([s(6), s(6), s(90) - 1, s(90) - 1], s(9), fill=(38, 43, 62, 255))
save(im, "slot", 96, 96, [16, 16, 16, 16])

# 选中框
rounded("frame_select", 96, 16, outline=5)

# ---------------- 基本形状 ----------------
def circle(name, size, ring=0, soft=False):
    if soft:
        im = Image.new("RGBA", (size, size), (255, 255, 255, 0))
        px = im.load(); c = (size - 1) / 2
        for y in range(size):
            for x in range(size):
                t = min(1, math.hypot(x - c, y - c) / c)
                a = (1 - t) ** 2
                px[x, y] = (255, 255, 255, int(255 * a))
        im.save(OUT / f"{name}.png"); META[name] = {"border": [0, 0, 0, 0]}; return
    im = canvas(size, size); d = ImageDraw.Draw(im)
    d.ellipse([0, 0, s(size) - 1, s(size) - 1], fill=W)
    if ring:
        d.ellipse([s(ring), s(ring), s(size - ring) - 1, s(size - ring) - 1], fill=(0, 0, 0, 0))
    save(im, name, size, size)


circle("circle", 128)
circle("circle_small", 32)
circle("ring", 128, ring=12)
circle("ring_thin", 128, ring=5)
circle("circle_soft", 128, soft=True)

# 渐变条
im = Image.new("RGBA", (8, 128)); px = im.load()
for y in range(128):
    for x in range(8):
        px[x, y] = (255, 255, 255, int(255 * y / 127))
im.save(OUT / "gradient_v.png"); META["gradient_v"] = {"border": [0, 0, 0, 0]}
im = Image.new("RGBA", (128, 8)); px = im.load()
for x in range(128):
    for y in range(8):
        px[x, y] = (255, 255, 255, int(255 * x / 127))
im.save(OUT / "gradient_h.png"); META["gradient_h"] = {"border": [0, 0, 0, 0]}

# 平铺用的棋盘格 / 斜纹
im = Image.new("RGBA", (32, 32), (255, 255, 255, 255)); d = ImageDraw.Draw(im)
d.rectangle([0, 0, 15, 15], fill=(214, 220, 230, 255)); d.rectangle([16, 16, 31, 31], fill=(214, 220, 230, 255))
im.save(OUT / "checker.png"); META["checker"] = {"border": [0, 0, 0, 0]}
im = canvas(32, 32); d = ImageDraw.Draw(im)
for k in range(-2, 4):
    o = s(k * 16)
    d.polygon([(o, s(32)), (o + s(8), s(32)), (o + s(40), 0), (o + s(32), 0)], fill=(255, 255, 255, 120))
save(im, "stripes", 32, 32)

# 噪声（溶解/流光 Shader 用，灰度）
random.seed(7)
N = 128
grid = [[random.random() for _ in range(9)] for _ in range(9)]
def vnoise(x, y, f):
    gx, gy = x * f, y * f; x0, y0 = int(gx) % 8, int(gy) % 8; tx, ty = gx - int(gx), gy - int(gy)
    tx, ty = tx * tx * (3 - 2 * tx), ty * ty * (3 - 2 * ty)
    a = grid[y0][x0] * (1 - tx) + grid[y0][x0 + 1] * tx; b = grid[y0 + 1][x0] * (1 - tx) + grid[y0 + 1][x0 + 1] * tx
    return a * (1 - ty) + b * ty
im = Image.new("RGBA", (N, N)); px = im.load()
for y in range(N):
    for x in range(N):
        v = sum(vnoise(x / N, y / N, 2 ** o * 2) / 2 ** o for o in range(4)) / 1.875
        g = int(255 * v); px[x, y] = (g, g, g, 255)
im.save(OUT / "noise.png"); META["noise"] = {"border": [0, 0, 0, 0]}

# ---------------- 白色图标（染色用） ----------------
def icon(name, draw, size=128):
    im = canvas(size, size); d = ImageDraw.Draw(im)
    draw(d, lambda v: s(v * size / 100))  # 在 100×100 的逻辑网格里画
    save(im, name, size, size)


def line(d, p, pts, w):
    pts = [(p(x), p(y)) for x, y in pts]
    d.line(pts, fill=W, width=int(p(w)), joint="curve")
    r = p(w) / 2
    for x, y in (pts[0], pts[-1]):
        d.ellipse([x - r, y - r, x + r, y + r], fill=W)


def poly(d, p, pts, fill=W):
    d.polygon([(p(x), p(y)) for x, y in pts], fill=fill)


def ell(d, p, x0, y0, x1, y1, fill=W):
    d.ellipse([p(x0), p(y0), p(x1), p(y1)], fill=fill)


def rr(d, p, x0, y0, x1, y1, r, fill=W):
    d.rounded_rectangle([p(x0), p(y0), p(x1), p(y1)], p(r), fill=fill)


CLEAR = (0, 0, 0, 0)


def star_pts(cx, cy, R, r, n=5, rot=-90):
    pts = []
    for i in range(n * 2):
        a = math.radians(rot + i * 180 / n); rad = R if i % 2 == 0 else r
        pts.append((cx + rad * math.cos(a), cy + rad * math.sin(a)))
    return pts


icon("check", lambda d, p: line(d, p, [(20, 52), (42, 74), (82, 28)], 12))
icon("close", lambda d, p: (line(d, p, [(24, 24), (76, 76)], 11), line(d, p, [(76, 24), (24, 76)], 11)))
icon("plus", lambda d, p: (line(d, p, [(50, 18), (50, 82)], 12), line(d, p, [(18, 50), (82, 50)], 12)))
icon("minus", lambda d, p: line(d, p, [(18, 50), (82, 50)], 12))
icon("arrow_down", lambda d, p: line(d, p, [(22, 38), (50, 66), (78, 38)], 11))
icon("arrow_up", lambda d, p: line(d, p, [(22, 62), (50, 34), (78, 62)], 11))
icon("arrow_left", lambda d, p: line(d, p, [(62, 22), (34, 50), (62, 78)], 11))
icon("arrow_right", lambda d, p: line(d, p, [(38, 22), (66, 50), (38, 78)], 11))
icon("back", lambda d, p: (line(d, p, [(48, 22), (20, 50), (48, 78)], 10), line(d, p, [(22, 50), (82, 50)], 10)))
icon("star", lambda d, p: poly(d, p, star_pts(50, 54, 46, 19)))


def star_outline(d, p):
    poly(d, p, star_pts(50, 54, 46, 19))
    poly(d, p, star_pts(50, 54, 33, 13.5), CLEAR)


icon("star_outline", star_outline)


def heart(d, p):
    ell(d, p, 8, 14, 54, 58); ell(d, p, 46, 14, 92, 58)
    poly(d, p, [(10, 44), (90, 44), (50, 88)])


icon("heart", heart)


def home(d, p):
    poly(d, p, [(50, 12), (92, 50), (80, 50), (80, 88), (20, 88), (20, 50), (8, 50)])
    rr(d, p, 41, 62, 59, 88, 3, CLEAR)


icon("home", home)


def gear(d, p):
    teeth = 8
    for i in range(teeth):
        a = i * 2 * math.pi / teeth
        cx, cy = 50 + 36 * math.cos(a), 50 + 36 * math.sin(a)
        ca, sa = math.cos(a), math.sin(a)
        pts = [(cx + dx * ca - dy * sa, cy + dx * sa + dy * ca) for dx, dy in [(-9, -9), (9, -9), (9, 9), (-9, 9)]]
        poly(d, p, pts)
    ell(d, p, 16, 16, 84, 84); ell(d, p, 36, 36, 64, 64, CLEAR)


icon("gear", gear)


def user(d, p):
    ell(d, p, 30, 10, 70, 50)
    d.pieslice([p(12), p(56), p(88), p(132)], 180, 360, fill=W)


icon("user", user)


def search(d, p):
    ell(d, p, 10, 10, 66, 66); ell(d, p, 21, 21, 55, 55, CLEAR)
    line(d, p, [(60, 60), (86, 86)], 12)


icon("search", search)


def lock(d, p):
    d.arc([p(28), p(10), p(72), p(58)], 180, 360, fill=W, width=int(p(9)))
    line(d, p, [(32.5, 34), (32.5, 46)], 9); line(d, p, [(67.5, 34), (67.5, 46)], 9)
    rr(d, p, 18, 44, 82, 90, 8); ell(d, p, 44, 58, 56, 70, CLEAR); rr(d, p, 47, 64, 53, 80, 2, CLEAR)


icon("lock", lock)


def sound(d, p):
    poly(d, p, [(10, 36), (30, 36), (54, 14), (54, 86), (30, 64), (10, 64)])
    d.arc([p(44), p(30), p(76), p(70)], -55, 55, fill=W, width=int(p(7)))
    d.arc([p(42), p(14), p(92), p(86)], -55, 55, fill=W, width=int(p(7)))


icon("sound", sound)


def mute(d, p):
    poly(d, p, [(10, 36), (30, 36), (54, 14), (54, 86), (30, 64), (10, 64)])
    line(d, p, [(66, 38), (88, 62)], 8); line(d, p, [(88, 38), (66, 62)], 8)


icon("mute", mute)


def music(d, p):
    ell(d, p, 14, 66, 40, 88); ell(d, p, 60, 58, 86, 80)
    line(d, p, [(37, 76), (37, 20)], 7); line(d, p, [(83, 68), (83, 12)], 7)
    poly(d, p, [(33, 16), (87, 8), (87, 26), (33, 34)])


icon("music", music)


def bag(d, p):
    d.arc([p(32), p(8), p(68), p(48)], 180, 360, fill=W, width=int(p(8)))
    rr(d, p, 14, 28, 86, 92, 12)


icon("bag", bag)


def mail(d, p):
    rr(d, p, 8, 20, 92, 80, 8)
    d.line([(p(14), p(28)), (p(50), p(54)), (p(86), p(28))], fill=CLEAR, width=int(p(7)), joint="curve")


icon("mail", mail)


def bell(d, p):
    d.pieslice([p(22), p(14), p(78), p(70)], 180, 360, fill=W)
    poly(d, p, [(22, 42), (78, 42), (86, 74), (14, 74)])
    ell(d, p, 40, 74, 60, 92); ell(d, p, 44, 6, 56, 18)


icon("bell", bell)


def trophy(d, p):
    d.pieslice([p(24), p(-6), p(76), p(62)], 0, 180, fill=W)
    rr(d, p, 24, 10, 76, 30, 0)
    d.arc([p(6), p(14), p(34), p(44)], 90, 270, fill=W, width=int(p(6)))
    d.arc([p(66), p(14), p(94), p(44)], 270, 90, fill=W, width=int(p(6)))
    rr(d, p, 44, 58, 56, 78, 0); rr(d, p, 26, 78, 74, 92, 4)


icon("trophy", trophy)


def gift(d, p):
    rr(d, p, 10, 34, 90, 52, 4); rr(d, p, 16, 52, 84, 92, 4)
    rr(d, p, 45, 34, 55, 92, 0, CLEAR)
    d.arc([p(22), p(12), p(52), p(40)], 180, 20, fill=W, width=int(p(7)))
    d.arc([p(48), p(12), p(78), p(40)], 160, 360, fill=W, width=int(p(7)))


icon("gift", gift)


def chat(d, p):
    rr(d, p, 8, 12, 92, 70, 16); poly(d, p, [(24, 66), (44, 66), (20, 90)])


icon("chat", chat)


def play(d, p):
    poly(d, p, [(28, 14), (86, 50), (28, 86)])


icon("play", play)


def pause(d, p):
    rr(d, p, 22, 14, 42, 86, 4); rr(d, p, 58, 14, 78, 86, 4)


icon("pause", pause)


def refresh(d, p):
    d.arc([p(14), p(14), p(86), p(86)], 30, 330, fill=W, width=int(p(10)))
    poly(d, p, [(70, 4), (94, 30), (64, 36)])


icon("refresh", refresh)


def menu(d, p):
    for y in (24, 50, 76):
        line(d, p, [(16, y), (84, y)], 10)


icon("menu", menu)


def hand(d, p):  # 新手引导用的指示箭头
    poly(d, p, [(50, 6), (86, 46), (64, 46), (64, 94), (36, 94), (36, 46), (14, 46)])


icon("pointer", hand)


def info(d, p):
    ell(d, p, 6, 6, 94, 94); ell(d, p, 44, 20, 56, 32, CLEAR); rr(d, p, 44, 40, 56, 80, 3, CLEAR)


icon("info", info)


def warn(d, p):
    poly(d, p, [(50, 6), (96, 90), (4, 90)]); rr(d, p, 45, 34, 55, 66, 3, CLEAR); ell(d, p, 44, 72, 56, 84, CLEAR)


icon("warning", warn)

# ---------------- 彩色物品图标（背包 / 列表用） ----------------
def item(name, draw):
    im = canvas(128, 128); d = ImageDraw.Draw(im)
    draw(d, lambda v: s(v * 128 / 100))
    save(im, name, 128, 128)


def c(h):
    h = h.lstrip("#"); return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) + (255,)


def sword(d, p):
    poly(d, p, [(78, 10), (90, 10), (90, 22), (42, 70), (30, 58)], c("#cfd8e3"))
    poly(d, p, [(84, 16), (90, 10), (90, 22), (42, 70), (38, 66)], c("#9aa8b8"))
    line_c = lambda pts, w, col: d.line([(p(x), p(y)) for x, y in pts], fill=col, width=int(p(w)))
    line_c([(22, 50), (50, 78)], 9, c("#c8913a"))
    line_c([(36, 64), (16, 84)], 9, c("#6b4423"))
    ell(d, p, 8, 80, 20, 92, c("#c8913a"))


item("item_sword", sword)


def shield(d, p):
    poly(d, p, [(50, 6), (88, 18), (84, 58), (50, 94), (16, 58), (12, 18)], c("#3c6fb8"))
    poly(d, p, [(50, 16), (78, 25), (75, 56), (50, 82), (25, 56), (22, 25)], c("#5a90dd"))
    poly(d, p, star_pts(50, 48, 18, 8), c("#ffd34d"))


item("item_shield", shield)


def potion(col, col2):
    def f(d, p):
        rr(d, p, 40, 6, 60, 16, 3, c("#8a5a33"))
        rr(d, p, 42, 14, 58, 34, 2, c("#d9e6ef"))
        ell(d, p, 16, 28, 84, 96, c("#d9e6ef"))
        ell(d, p, 21, 33, 79, 91, c(col))
        d.chord([p(21), p(33), p(79), p(91)], 200, 340, fill=c("#d9e6ef"))
        ell(d, p, 30, 52, 42, 64, c(col2))
    return f


item("item_potion_red", potion("#e04848", "#ff9b9b"))
item("item_potion_blue", potion("#3d7be0", "#9cc3ff"))
item("item_potion_green", potion("#3aa865", "#9be8b6"))


def gem(d, p):
    poly(d, p, [(26, 14), (74, 14), (94, 38), (50, 92), (6, 38)], c("#2db6c9"))
    poly(d, p, [(26, 14), (74, 14), (62, 38), (38, 38)], c("#8ff0fa"))
    poly(d, p, [(6, 38), (38, 38), (50, 92)], c("#1d8ea0"))
    poly(d, p, [(62, 38), (94, 38), (50, 92)], c("#25a3b6"))


item("item_gem", gem)


def coin(d, p):
    ell(d, p, 8, 8, 92, 92, c("#d99a1e")); ell(d, p, 14, 12, 88, 86, c("#ffc93c")); ell(d, p, 26, 24, 76, 74, c("#f0ad25"))
    poly(d, p, star_pts(50, 50, 18, 8), c("#ffe38a"))


item("item_coin", coin)


def apple(d, p):
    ell(d, p, 10, 26, 58, 92, c("#e2463f")); ell(d, p, 42, 26, 90, 92, c("#e2463f"))
    ell(d, p, 22, 36, 38, 54, c("#ff8a80"))
    d.line([(p(50), p(30)), (p(54), p(10))], fill=c("#6b4423"), width=int(p(6)))
    ell(d, p, 54, 8, 80, 24, c("#5fb548"))


item("item_apple", apple)


def key(d, p):
    ell(d, p, 8, 8, 48, 48, c("#ffc93c")); ell(d, p, 18, 18, 38, 38, CLEAR)
    d.line([(p(40), p(40)), (p(88), p(88))], fill=c("#ffc93c"), width=int(p(11)))
    d.line([(p(66), p(66)), (p(78), p(54))], fill=c("#ffc93c"), width=int(p(10)))
    d.line([(p(78), p(78)), (p(90), p(66))], fill=c("#ffc93c"), width=int(p(10)))


item("item_key", key)


def scroll(d, p):
    rr(d, p, 18, 18, 82, 82, 4, c("#f3e2b8"))
    rr(d, p, 10, 10, 90, 24, 7, c("#d9b77a")); rr(d, p, 10, 76, 90, 90, 7, c("#d9b77a"))
    for y in (36, 48, 60):
        d.line([(p(28), p(y)), (p(72), p(y))], fill=c("#b08a50"), width=int(p(4)))


item("item_scroll", scroll)


def bow(d, p):
    d.arc([p(-30), p(8), p(70), p(92)], -70, 70, fill=c("#9c6a3a"), width=int(p(9)))
    d.line([(p(37), p(14)), (p(37), p(86))], fill=c("#e8e8e8"), width=int(p(3)))
    d.line([(p(18), p(50)), (p(92), p(50))], fill=c("#6b4423"), width=int(p(5)))
    poly(d, p, [(96, 50), (82, 42), (82, 58)], c("#cfd8e3"))


item("item_bow", bow)


def helmet(d, p):
    d.pieslice([p(14), p(14), p(86), p(110)], 180, 360, fill=c("#9aa8b8"))
    rr(d, p, 14, 58, 86, 72, 3, c("#7b8796")); rr(d, p, 46, 22, 54, 84, 2, c("#cfd8e3"))


item("item_helmet", helmet)


def ring_item(d, p):
    ell(d, p, 18, 30, 82, 94, c("#ffc93c")); ell(d, p, 28, 40, 72, 84, CLEAR)
    poly(d, p, [(36, 22), (64, 22), (74, 34), (50, 52), (26, 34)], c("#e0457b"))


item("item_ring", ring_item)

# ---------------- 头像 & 风景图 ----------------
AV = [("#ffb74d", "#6d4c41"), ("#81c784", "#37474f"), ("#64b5f6", "#4e342e"), ("#f48fb1", "#212121"),
      ("#ba68c8", "#5d4037"), ("#4db6ac", "#3e2723")]
for i, (bg, hair) in enumerate(AV, 1):
    im = canvas(128, 128); d = ImageDraw.Draw(im)
    p = lambda v: s(v * 128 / 100)
    d.rectangle([0, 0, s(128), s(128)], fill=c(bg))
    d.pieslice([p(14), p(70), p(86), p(142)], 180, 360, fill=c("#f5f5f5"))
    ell(d, p, 30, 18, 70, 62, c("#ffd9b8"))
    d.chord([p(28), p(12), p(72), p(52)], 180, 360, fill=c(hair))
    ell(d, p, 40, 36, 45, 41, c("#3a2a20")); ell(d, p, 55, 36, 60, 41, c("#3a2a20"))
    save(im, f"avatar_{i}", 128, 128)


def landscape(name, w0, h0, sky_top, sky_bot, layers, sun):
    k = 2  # 2 倍超采样，月亮和山脊边缘不出锯齿
    w, h = w0 * k, h0 * k
    sun = (sun[0] * k, sun[1] * k, sun[2] * k)
    im = Image.new("RGBA", (w, h)); px = im.load()
    for y in range(h):
        t = y / (h - 1)
        col = tuple(int(sky_top[k] + (sky_bot[k] - sky_top[k]) * t) for k in range(3)) + (255,)
        for x in range(w):
            px[x, y] = col
    d = ImageDraw.Draw(im)
    d.ellipse([sun[0] - sun[2], sun[1] - sun[2], sun[0] + sun[2], sun[1] + sun[2]], fill=(255, 244, 214, 255))
    rnd = random.Random(3)
    for base, amp, col in layers:
        pts = [(0, h)]
        ph = rnd.random() * 6
        for x in range(0, w + 8, 8):
            y = base * h + amp * h * (0.6 * math.sin(x / w * 7 + ph) + 0.4 * math.sin(x / w * 17 + ph * 2))
            pts.append((x, y))
        pts.append((w, h)); d.polygon(pts, fill=col)
    im = im.resize((w0, h0), Image.LANCZOS)
    im.save(OUT / f"{name}.png"); META[name] = {"border": [0, 0, 0, 0]}


landscape("bg_landscape", 1024, 512, (92, 140, 220), (255, 196, 150),
          [(0.55, 0.08, (120, 110, 170, 255)), (0.68, 0.06, (80, 88, 140, 255)), (0.82, 0.04, (46, 58, 96, 255))], (720, 250, 46))
landscape("bg_night", 1024, 512, (16, 22, 52), (70, 60, 120),
          [(0.6, 0.07, (40, 40, 80, 255)), (0.75, 0.05, (26, 28, 58, 255)), (0.88, 0.03, (14, 16, 34, 255))], (820, 110, 16))

(OUT / "sprites.json").write_text(json.dumps(META, indent=1))
print(f"[sprites] {len(META)} sprites -> {OUT}")
