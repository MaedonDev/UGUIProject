#!/usr/bin/env python3
"""组装《UGUI 从入门到精通》：PDF 版 + 网页版。

章节文件 src/cNN.html 里的特殊写法：
  <pre class="code" data-file="Ch05/Foo.cs"></pre>                      整个文件（来自配套 Unity 工程 Assets/UGUIBook/）
  <pre class="code" data-file="Ch05/Foo.cs" data-region="name"></pre>   只取 //#region name ... //#endregion
  <pre class="code" data-file="..." data-lines="12-40"></pre>           只取行号范围
  <pre class="code" data-lang="csharp">...片段...</pre>                   直接写在 HTML 里的片段（csharp/hlsl/text）
  <div class="tree" data-tree="ch05-xxx" data-root="Canvas/Panel" data-depth="3" data-comps="1"></div>
                                                                        配图 ch05-xxx 搭出来的层级（Hierarchy 面板样式）
  <div class="scene">Ch05/Scenes/Ch05_Foo.unity</div>                   配套场景路径
用法：
  .venv/bin/python tools/build.py                # 全书 PDF + 网页
  .venv/bin/python tools/build.py --only c05,c06 # 只预览这几章 -> build/preview_c05_c06.pdf
  .venv/bin/python tools/build.py --web          # 只生成网站 web/（tools/build_site.py，GameTech 专栏 /ugui/）
"""
import html, re, shutil, subprocess, sys
from pathlib import Path
from pygments import highlight
from pygments.formatters import HtmlFormatter
from pygments.lexers import get_lexer_by_name

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "src"
BUILD = ROOT / "build"
CODE_ROOTS = [ROOT / "project/Assets/UGUIBook"]
TREE_DIR = SRC / "tree"
BOOK = "UGUI 从入门到精通"
FULL_TITLE = "UGUI 从入门到精通：Unity 界面开发实战全解"
PDF_NAME = "UGUI从入门到精通_麦冬.pdf"
ZIP_NAME = "UGUI从入门到精通_配套工程.zip"
CHROME = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"

PARTS = [  # (在哪一章之前, 部分序号, 标题, 副标题, 要点)
    ("c01", "第一部分", "入门", "从零做出第一个界面", ["认识 UGUI 与第一个界面", "Canvas 与屏幕适配", "RectTransform 与锚点", "Image 与精灵", "文字：Text 与 TextMeshPro"]),
    ("c06", "第二部分", "控件", "每一个内置控件的用法与扩展", ["Button 与 Selectable", "Toggle、Slider 与 Scrollbar", "InputField 与 Dropdown", "ScrollRect 与遮罩", "自动布局"]),
    ("c11", "第三部分", "原理", "事件、重建与渲染：读懂 UGUI 源码", ["事件系统", "Graphic、Canvas 与重建", "自定义 Graphic 与网格效果", "UI Shader、遮罩与模板"]),
    ("c15", "第四部分", "实战", "动效、框架与常见游戏界面", ["UI 动效", "UI 框架与界面管理", "游戏 HUD", "列表、背包与无限滚动", "适配、多语言与 3D 混合"]),
    ("c20", "第五部分", "精通", "性能优化、调试与工程化", ["性能优化", "调试、工具与技巧"]),
    ("ax", "附录", "速查", "随手翻的工具页", ["常用 API 速查", "事件接口与执行顺序", "常见问题排查清单", "术语表"]),
]

FMT = HtmlFormatter(nowrap=True)


def find_code(rel):
    for r in CODE_ROOTS:
        p = r / rel
        if p.exists():
            return p
    raise SystemExit(f"[build] 找不到代码文件 {rel}（在 {', '.join(map(str, CODE_ROOTS))}）")


def lang_of(name):
    ext = name.rsplit(".", 1)[-1].lower()
    return {"cs": "csharp", "shader": "hlsl", "hlsl": "hlsl", "cginc": "hlsl", "compute": "hlsl", "json": "json"}.get(ext, "text")


def strip_markers(lines):
    return [l for l in lines if not re.match(r"\s*//\s*#(end)?region\b", l)]


def dedent(lines):
    ind = min((len(l) - len(l.lstrip()) for l in lines if l.strip()), default=0)
    return [l[ind:] for l in lines]


def code_block(attrs, inner):
    a = dict(re.findall(r'data-([\w-]+)="([^"]*)"', attrs))
    title = a.get("title")
    if "file" in a:
        p = find_code(a["file"])
        lines = p.read_text(encoding="utf-8-sig").splitlines()
        if "region" in a:
            name = a["region"]
            try:
                s = next(i for i, l in enumerate(lines) if re.match(rf"\s*//\s*#region\s+{re.escape(name)}\s*$", l))
            except StopIteration:
                raise SystemExit(f"[build] {a['file']} 里没有 //#region {name}")
            e = next(i for i in range(s + 1, len(lines)) if re.match(r"\s*//\s*#endregion", lines[i]))
            lines = dedent(lines[s + 1:e])
        elif "lines" in a:
            x, y = map(int, a["lines"].split("-"))
            lines = dedent(lines[x - 1:y])
        lines = strip_markers(lines)
        lang = a.get("lang", lang_of(p.name))
        if title is None:
            title = "Assets/UGUIBook/" + a["file"] + (f"（节选：{a['region']}）" if "region" in a else "")
        code = "\n".join(lines).strip("\n")
    else:
        lang = a.get("lang", "csharp")
        code = html.unescape(inner).strip("\n")
    body = highlight(code, get_lexer_by_name(lang), FMT) if lang != "text" else html.escape(code)
    cap = f'<div class="code-cap">{html.escape(title)}</div>' if title else ""
    return f'<div class="codebox">{cap}<pre class="code">{body}</pre></div>'


def tree_block(attrs):
    a = dict(re.findall(r'data-([\w-]+)="([^"]*)"', attrs))
    f = TREE_DIR / (a["tree"] + ".txt")
    if not f.exists():
        raise SystemExit(f"[build] 没有层级数据 {f}（先跑 unity_check 出图）")
    rows = []
    for line in f.read_text().splitlines():
        d, name, comps = line.split("|", 2)
        rows.append((int(d), name, comps))
    root = a.get("root", "Canvas")
    hide = set(filter(None, a.get("hide", "").split(",")))
    maxd = int(a.get("depth", "9"))
    show_comps = a.get("comps", "1") == "1"
    # 找到 root 路径对应的行
    path = root.split("/")
    start, base = None, 0
    stack = []
    for i, (d, name, _) in enumerate(rows):
        stack = stack[:d] + [name.lstrip("!")]
        if stack[-len(path):] == path:   # 路径按段从末尾匹配，"Panel/List" 能匹配 "Canvas/Panel/List"
            start, base = i, d
            break
    if start is None:
        raise SystemExit(f"[build] 层级 {a['tree']} 里找不到 {root}")
    out = []
    skip_below = None
    for d, name, comps in rows[start:]:
        if d < base or (d == base and out):
            break
        if skip_below is not None and d > skip_below:
            continue
        skip_below = None
        clean = name.lstrip("!")
        if clean in hide:
            skip_below = d
            continue
        if d - base > maxd:
            continue
        off = name.startswith("!")
        c = ""
        if show_comps and comps:
            keep = [x for x in comps.split(",") if x not in ("RectTransform",)]
            if keep:
                c = f"<small>{html.escape(' · '.join(keep))}</small>"
        indent = "&nbsp;&nbsp;&nbsp;" * (d - base)
        out.append(f'<div class="r{" off" if off else ""}">{indent}<span class="ic"></span>{html.escape(clean)}{c}</div>')
    title = a.get("title", "Hierarchy")
    return f'<div class="tree"><div class="tt">{html.escape(title)}</div>{"".join(out)}</div>'


def scene_block(inner):
    p = inner.strip()
    if not (ROOT / "project/Assets/UGUIBook" / p).exists() and not any((r / p).exists() for r in CODE_ROOTS):
        print(f"[build] 警告：配套场景不存在 {p}")
    return f'<div class="scene"><code>Assets/UGUIBook/{html.escape(p)}</code></div>'


def expand(s):
    s = re.sub(r'<pre class="code"([^>]*)>(.*?)</pre>', lambda m: code_block(m.group(1), m.group(2)), s, flags=re.S)
    s = re.sub(r'<div class="tree"([^>]*)></div>', lambda m: tree_block(m.group(1)), s)
    s = re.sub(r'<div class="scene">(.*?)</div>', lambda m: scene_block(m.group(1)), s)
    return s


def part_html(p):
    _, no, title, sub, items = p
    lis = "".join(f"<li>{i}</li>" for i in items)
    return f'''<section class="part"><div class="grid"></div><div class="ring"></div>
<div class="pn">{no}</div><h2>{title}</h2><div class="ps">{sub}</div><ul>{lis}</ul></section>\n'''


def chapter_files(only=None):
    files = sorted(SRC.glob("c[0-9][0-9].html")) + ([SRC / "ax.html"] if (SRC / "ax.html").exists() else [])
    if only:
        files = [f for f in files if f.stem in only]
    return files


def meta_of(text):
    m = re.search(r'<section class="chapter[^"]*" id="(\w+)" data-title="([^"]*)" data-sub="([^"]*)"', text)
    return m.groups() if m else None


def toc_html(metas, pages, link=None):
    front = f'<a href="index.html#front">写在前面</a>' if link else "写在前面"
    rows = [f'<li><span class="n">{front}</span><span class="t"></span><span class="p">{pages.get("front", "")}</span></li>']
    part_at = {p[0]: p for p in PARTS}
    for cid, title, sub in metas:
        if cid in part_at:
            p = part_at[cid]
            rows.append(f'<li class="pt">{p[1]} · {p[2]}<small>{p[3]}</small></li>')
        n = "附录" if cid == "ax" else f"第 {int(cid[1:])} 章"
        t = f'<a href="{cid}.html">{title}</a>' if link else title
        rows.append(f'<li><span class="n">{n}</span><span class="t">{t}<small>{sub}</small></span>'
                    f'<span class="p">{pages.get(cid, "")}</span></li>')
    return "<ol>" + "\n".join(rows) + "</ol>"


def load_chapters(only=None):
    files = chapter_files(only)
    texts = [f.read_text() for f in files]
    metas = [meta_of(t) for t in texts]
    for f, m in zip(files, metas):
        if m is None:
            raise SystemExit(f"[build] {f.name} 缺少 <section class=\"chapter\" id=.. data-title=.. data-sub=..>")
    return texts, metas


def front_html():
    f = SRC / "front.html"
    return f.read_text() if f.exists() else ""


def assemble(only=None, pages=None):
    head = (SRC / "00_head.html").read_text()
    tail = (SRC / "99_tail.html").read_text() if not only else "</body></html>"
    texts, metas = load_chapters(only)
    if only:
        head = re.sub(r"<!--FRONT-->.*?<!--/FRONT-->", "", head, flags=re.S)
    head = head.replace("<!--FRONTMATTER-->", front_html())
    head = head.replace("<!--TOC-->", toc_html(metas, pages or {}))
    body = []
    part_at = {p[0]: p for p in PARTS}
    for (cid, _, _), t in zip(metas, texts):
        if cid in part_at and not only:
            body.append(part_html(part_at[cid]))
        body.append(expand(t))
    out = head + "\n".join(body) + tail
    pdf_img()
    out = out.replace("</style>", PDF_FONT_CSS + "</style>", 1)
    return out.replace('src="img/', 'src="build/pdfimg/').replace("url('img/", "url('build/pdfimg/"), metas


# PDF 里“PingFang SC”换成配套工程里的 Noto Sans SC 静态子集（SIL OFL）：PingFang 是可变字体，Chrome 会把它嵌成几百个 Type 3 字体（约 17MB）；
# 子集里没有的字自动回退到系统字体。网页版不受影响。
_F = "project/Assets/UGUIBook/_Core/Fonts/"
PDF_FONT_CSS = (f'@font-face {{ font-family: "PingFang SC"; src: url("{_F}NotoSansSC-Regular.ttf"); font-weight: 100 499; }}\n'
                f'@font-face {{ font-family: "PingFang SC"; src: url("{_F}NotoSansSC-Bold.ttf"); font-weight: 500 900; }}\n')

PDF_IMG_W = 1400   # PDF 用的截图最宽 1400px（A4 正文宽约 200dpi），让 PDF 小于 24MB（海外镜像 Cloudflare 单文件上限 25MB）


def pdf_img():
    """src/img → build/pdfimg：宽图缩到 PDF_IMG_W、JPEG 质量 80；只处理比缓存新的文件"""
    from PIL import Image
    out = BUILD / "pdfimg"
    out.mkdir(parents=True, exist_ok=True)
    for f in (SRC / "img").iterdir():
        if f.name.startswith(("_", ".")):
            continue
        dst = out / f.name
        if dst.exists() and dst.stat().st_mtime >= f.stat().st_mtime:
            continue
        im = Image.open(f)
        if f.suffix.lower() != ".jpg" or im.width <= PDF_IMG_W:
            shutil.copy2(f, dst)
            continue
        im.convert("RGB").resize((PDF_IMG_W, round(im.height * PDF_IMG_W / im.width)), Image.LANCZOS).save(dst, quality=80, optimize=True)


def to_pdf(html_path, pdf_path):
    subprocess.run([CHROME, "--headless=new", "--disable-gpu", "--no-pdf-header-footer", "--virtual-time-budget=20000",
                    f"--print-to-pdf={pdf_path}", f"file://{html_path}"], stderr=subprocess.DEVNULL, check=True)


def find_pages(pdf, metas):
    n = int(re.search(r"Pages:\s+(\d+)", subprocess.run(["pdfinfo", pdf], capture_output=True, text=True).stdout).group(1))
    txt = subprocess.run(["pdftotext", "-layout", str(pdf), "-"], capture_output=True, text=True).stdout.split("\f")
    pages = {}
    for i, t in enumerate(txt[:n], 1):
        if "front" not in pages and i > 1 and "写在前面" in t[:300]:
            pages["front"] = i
        for cid, _, _ in metas:
            key = "APPENDIX" if cid == "ax" else rf"CHAPTER\s+{cid[1:]}"
            if cid not in pages and re.search(rf"\b{key}\b", t):
                pages[cid] = i
    return pages, n


# ---------------------------------------------------------------- 网页版
def main():
    only = None
    if "--web" in sys.argv:
        import build_site
        build_site.main()
        return
    if "--only" in sys.argv:
        only = set(sys.argv[sys.argv.index("--only") + 1].split(","))
    BUILD.mkdir(exist_ok=True)
    if only:
        out, _ = assemble(only)
        tag = "_".join(sorted(only))
        hp = ROOT / f"preview_{tag}.html"; hp.write_text(out)
        pdf = BUILD / f"preview_{tag}.pdf"
        to_pdf(hp, pdf); hp.unlink()
        print("[build] preview ->", pdf)
        return
    out, metas = assemble()
    hp = ROOT / "ugui.html"; hp.write_text(out)
    pdf = ROOT / PDF_NAME
    to_pdf(hp, pdf)
    pages, n = find_pages(pdf, metas)
    out, _ = assemble(pages=pages)
    hp.write_text(out)
    to_pdf(hp, pdf)
    pages2, n = find_pages(pdf, metas)
    if pages2 != pages:
        out, _ = assemble(pages=pages2); hp.write_text(out); to_pdf(hp, pdf)
    import pikepdf
    with pikepdf.open(pdf, allow_overwriting_input=True) as p:
        p.save(pdf, compress_streams=True, object_stream_mode=pikepdf.ObjectStreamMode.generate, recompress_flate=True)
    print(f"[build] {pdf} ({n} 页, {pdf.stat().st_size / 1e6:.1f} MB)")
    missing = [c for c, _, _ in metas if c not in pages2]
    if missing:
        print("[build] 警告：目录里找不到页码的章节", missing)
    import build_site   # 网站（GameTech 专栏 /ugui/）
    build_site.main()


if __name__ == "__main__":
    main()
