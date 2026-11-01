#!/usr/bin/env python3
"""生成《UGUI 从入门到精通》网站：books/ugui/web/（GameTech 专栏 /ugui/，免费公开）

  .venv/bin/python tools/build_site.py      # build.py 全量构建时最后也会调用它

页面（网址用描述性英文词，见 tools/slugs.json）：
  index.html                 封面、配套工程怎么用、目录、写在前面
  chapters/<章>.html         各章正文（书的 HTML + 代码块复制 / 查看完整文件）
  cases/<案例>.html          每个案例一页（对应具体的搜索词）
  cases.html                 案例目录
  source.html                配套工程源码浏览 + 下载（PDF、完整工程、按章打包）
  code/ChNN/xxx.cs.html      单个源文件的高亮页
外观统一走 GameTech 外壳（gametech/shell.py）：顶栏、配色变量 --gt-*、深浅色、作者卡片、SEO 标签。
"""
import hashlib, html, json, re, shutil, sys, zipfile
from pathlib import Path
from pygments import highlight
from pygments.formatters import HtmlFormatter
from pygments.lexers import get_lexer_by_name

ROOT = Path(__file__).resolve().parent.parent
SRC = ROOT / "src"
CODE = ROOT / "project/Assets/UGUIBook"
SITE = ROOT / "site"
DIST = ROOT / "web"
FMT = HtmlFormatter(nowrap=True)
SITE_TITLE = "UGUI 从入门到精通"
SITE_URL = "https://gametech.byte888.com/ugui/"
COVER = SITE_URL + "img/cover-hero.jpg"
PDF_DL, ZIP_DL = "download/ugui-from-beginner-to-expert.pdf", "download/ugui-companion-project.zip"
DEFAULT_DESC = ("《UGUI 从入门到精通》免费在线版：Unity 2022.3 界面开发从第一个按钮讲到源码原理、UI 框架和性能优化。"
                "21 章 122 个案例，附 119 个可运行场景的完整 Unity 工程和 PDF 免费下载。")
ICP = '<a href="https://beian.miit.gov.cn/" target="_blank" rel="noopener">京ICP备2025143660号-1</a>'

sys.path.insert(0, str(ROOT / "tools"))
import build as B  # noqa: E402  复用书的 PARTS、章节元数据、tree / scene 块、代码定位
sys.path.insert(0, str(ROOT.parent.parent / "gametech"))
import shell  # noqa: E402  GameTech 专栏统一外壳

SLUGS = json.loads((ROOT / "tools/slugs.json").read_text(encoding="utf-8"))
LANG = {"cs": "csharp", "shader": "hlsl", "hlsl": "hlsl", "cginc": "hlsl", "json": "json"}


def esc(s):
    return html.escape(s, quote=True)


def lang_of(name):
    return LANG.get(name.rsplit(".", 1)[-1].lower(), "text")


def hl(code, lang):
    return html.escape(code) if lang == "text" else highlight(code, get_lexer_by_name(lang), FMT)


def ver(p):
    return hashlib.md5(p.read_bytes()).hexdigest()[:8]


def strip_tags(s):
    return re.sub(r"\s+", " ", re.sub(r"<[^>]+>", " ", s)).strip()


# ---------------- 正文块 ----------------
def code_block(attrs, inner):
    a = dict(re.findall(r'data-([\w-]+)="([^"]*)"', attrs))
    links = ""
    if "file" in a:
        rel = a["file"]
        lines = B.find_code(rel).read_text(encoding="utf-8-sig").splitlines()
        if "region" in a:
            s = next(i for i, l in enumerate(lines) if re.match(rf"\s*//\s*#region\s+{re.escape(a['region'])}\s*$", l))
            e = next(i for i in range(s + 1, len(lines)) if re.match(r"\s*//\s*#endregion", lines[i]))
            lines = B.dedent(lines[s + 1:e])
        elif "lines" in a:
            x, y = map(int, a["lines"].split("-"))
            lines = B.dedent(lines[x - 1:y])
        lines = B.strip_markers(lines)
        lang = a.get("lang", lang_of(rel))
        cap = a.get("title") or ("Assets/UGUIBook/" + rel + (f"（节选：{a['region']}）" if "region" in a else ""))
        code = "\n".join(lines).strip("\n")
        links = f'<div class="cb-links"><a href="code/{esc(rel)}.html">完整文件</a><a href="src/{esc(rel)}" download>下载</a></div>'
    else:
        lang = a.get("lang", "csharp")
        code = html.unescape(inner).strip("\n")
        cap = a.get("title")
    head = f'<div class="code-cap">{esc(cap)}</div>' if cap else ""
    return (f'<div class="codebox">{head}<div class="cb-tools"><button class="cb-copy" title="复制">复制</button></div>'
            f'<div class="cb-pane on">{links}<pre class="code">{hl(code, lang)}</pre></div></div>')


def expand(s):
    s = re.sub(r'<pre class="code"([^>]*)>(.*?)</pre>', lambda m: code_block(m.group(1), m.group(2)), s, flags=re.S)
    s = re.sub(r'<div class="tree"([^>]*)></div>', lambda m: B.tree_block(m.group(1)), s)
    s = re.sub(r'<div class="scene">(.*?)</div>', lambda m: B.scene_block(m.group(1)), s)
    return s


def add_alt(text):
    """图片用所在 figure 的图注做 alt（搜索引擎和读屏软件靠它理解图片），并延迟加载"""
    def fig(m):
        cap = re.search(r"<figcaption[^>]*>(.*?)</figcaption>", m.group(0), re.S)
        alt = strip_tags(cap.group(1))[:120] if cap else ""
        return re.sub(r'<img\b(?![^>]*\balt=)', f'<img alt="{esc(alt)}"', m.group(0)) if alt else m.group(0)
    text = re.sub(r"<figure\b.*?</figure>", fig, text, flags=re.S)
    return re.sub(r'<img\b(?![^>]*\bloading=)', '<img loading="lazy" decoding="async"', text)


def add_ids(text):
    """h3.sec / h3.case 加锚点，返回 (文本, [(id, 标题, 类)])"""
    secs, n = [], [0]

    def rep(m):
        n[0] += 1
        sid = f"s{n[0]}"
        t = re.sub(r"\s*(入门|进阶|高阶)$", "", strip_tags(m.group(2)))
        secs.append((sid, t, "case" if "case" in m.group(1) else "sec"))
        return f'<h3 class="{m.group(1)}" id="{sid}">{m.group(2)}</h3>'
    return re.sub(r'<h3 class="(sec|case[^"]*)">(.*?)</h3>', rep, text, flags=re.S), secs


# ---------------- 页面骨架 ----------------
def chapters():
    out = []
    for f in B.chapter_files():
        t = f.read_text(encoding="utf-8")
        cid, title, sub = B.meta_of(t)
        label = "附录" if cid == "ax" else f"第 {int(cid[1:])} 章"
        out.append({"id": cid, "title": title, "sub": sub, "text": t, "label": label,
                    "url": f"chapters/{SLUGS['chapters'][cid]}.html"})
    return out


def nav_html(chs, current=None, sections=None):
    part_at = {p[0]: p for p in B.PARTS}
    rows = ['<a class="nav-home" href="index.html">封面与目录</a>']
    for c in chs:
        if c["id"] in part_at:
            p = part_at[c["id"]]
            rows.append(f'<div class="nav-part">{p[1]} · {p[2]}</div>')
        n = "附" if c["id"] == "ax" else f"{int(c['id'][1:])}"
        rows.append(f'<a class="nav-ch{" on" if c["id"] == current else ""}" href="{c["url"]}"><span>{n}</span>{esc(c["title"])}</a>')
        if c["id"] == current and sections:
            rows.append('<div class="nav-sec">' + "".join(f'<a href="#{sid}" class="{k}">{esc(t)}</a>' for sid, t, k in sections) + "</div>")
    rows.append('<div class="nav-part">资源</div><a class="nav-ch" href="cases.html"><span>☰</span>案例目录</a>'
                '<a class="nav-ch" href="source.html"><span>↓</span>源码、工程与 PDF 下载</a>')
    return "\n".join(rows)


VER = {}
PDF_PAGES = 0


def page(title, body, nav, *, depth=0, cls="", desc=None, path=None, crumb=None):
    """depth：页面在几层子目录里（chapters/ 是 1）；path：相对 /ugui/ 的地址；crumb：面包屑最后一级（首页不传）"""
    crumbs = [("GameTech", "https://gametech.byte888.com/"), (SITE_TITLE, SITE_URL)]
    if crumb and path is not None:
        crumbs.append((crumb, SITE_URL + path))
    body = add_alt(body).replace(shell.ad("chapter-end"), shell.author_box() + shell.ad("chapter-end"), 1)
    search = '<div class="search"><input type="search" placeholder="搜索章节、案例…" aria-label="搜索本书"><div class="results"></div></div>'
    out = f"""<!doctype html>
<html lang="zh-CN"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>{esc(title)}</title>
{shell.seo(title, desc or DEFAULT_DESC, SITE_URL + path if path is not None else None, COVER, crumbs=crumbs, article=bool(crumb))}
<link rel="icon" href="assets/icon.svg">
{shell.head()}
<link rel="stylesheet" href="assets/book.css?v={VER['book.css']}"><link rel="stylesheet" href="assets/site.css?v={VER['site.css']}">
</head><body class="{cls}">
{shell.topbar(SITE_TITLE, "index.html", slot=search)}
<aside class="nav">{nav}</aside>
<main class="page-main">{body}
<footer class="foot">{shell.foot_links()}© 2026 麦冬。全书免费阅读；书中示例代码与配套素材可自由用于你的项目（含商业项目）。{ICP}</footer>
</main>
<script src="assets/site.js?v={VER['site.js']}"></script>
{shell.scripts()}
</body></html>
"""
    if depth:
        up = "../" * depth
        out = re.sub(r'(href|src)="(?!https?:|mailto:|#|/|\.\./|data:)([^"]+)"', lambda m: f'{m.group(1)}="{up}{m.group(2)}"', out)
    return out


def write(rel, text):
    p = DIST / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(text, encoding="utf-8")


def pager(prev, nxt, fmt):
    return ('<nav class="pager">' + (f'<a href="{prev[0]}">← {esc(prev[1])}</a>' if prev else "<span></span>")
            + (f'<a href="{nxt[0]}">{esc(nxt[1])} →</a>' if nxt else "<span></span>") + "</nav>")


# ---------------- 章节与案例 ----------------
ALL_CASES = []


def split_cases(text, c):
    """找出本章的案例 / 实验（h3.case 到下一个 h3 / 小结 / 练习），在标题后加“单独打开”链接"""
    cases = []
    for m in re.finditer(r'<h3 class="case[^"]*" id="(s\d+)">(.*?)</h3>', text, flags=re.S):
        no = re.search(r'<span class="no">(案例|实验)\s*([\d.]+)</span>', m.group(2))
        if not no:
            continue
        kind, num = no.groups()
        if num not in SLUGS["cases"]:
            raise SystemExit(f"[site] tools/slugs.json 里缺少 {kind} {num} 的网址名")
        start = m.end()
        nxt = re.search(r'<h3[ >]|<div class="summary">|<div class="exercise">', text[start:])
        end = start + nxt.start() if nxt else len(text)
        name = strip_tags(re.sub(r'<span class="(no|lv)">.*?</span>', "", m.group(2)))
        lv = re.search(r'<span class="lv">(.*?)</span>', m.group(2))
        goal = re.search(r"<p><b>目标</b>[:：](.*?)</p>", text[start:end], flags=re.S)
        cases.append({"kind": kind, "num": num, "slug": SLUGS["cases"][num], "name": name, "lv": lv.group(1) if lv else "",
                      "goal": strip_tags(goal.group(1)) if goal else "", "chapter": c, "sid": m.group(1),
                      "h3": m.group(0), "html": text[start:end]})
    for cs in cases:
        link = (f'<a class="case-link" href="cases/{cs["slug"]}.html" title="单独打开：{cs["kind"]} {cs["num"]} {esc(cs["name"])}">'
                f'单独打开{cs["kind"]} {cs["num"]} ↗</a>')
        text = text.replace(cs["h3"], cs["h3"].replace("</h3>", link + "</h3>"), 1)
    return text, cases


def chapter_pages(chs, search):
    for i, c in enumerate(chs):
        text, secs = add_ids(expand(c["text"]))
        text, cases = split_cases(text, c)
        ALL_CASES.extend(cases)
        text = re.sub(r'(<div class="chap-head">.*?)<h2>(.*?)</h2>', r'\1<h1>\2</h1>', text, count=1, flags=re.S)  # 章名是页面的 h1
        prev = (chs[i - 1]["url"], chs[i - 1]["title"]) if i else None
        nxt = (chs[i + 1]["url"], chs[i + 1]["title"]) if i + 1 < len(chs) else None
        body = f'<article class="book">{text}</article>{shell.ad("chapter-end")}{pager(prev, nxt, None)}'
        lead = re.search(r'<p class="lead">(.*?)</p>', c["text"], flags=re.S)
        desc = f"{c['label']} {c['title']}：{c['sub']}。{strip_tags(lead.group(1)) if lead else ''}"[:150]
        title = (f"{c['title']}（{c['label']}）- Unity UGUI 教程 · {SITE_TITLE}" if c["id"] != "ax"
                 else f"附录 {c['title']} - Unity UGUI 速查 · {SITE_TITLE}")
        write(c["url"], page(title, body, nav_html(chs, c["id"], secs), depth=1, desc=desc, path=c["url"], crumb=f"{c['label']} {c['title']}"))
        search.append({"t": f"{c['label']} {c['title']}", "s": c["sub"], "u": c["url"]})
        search += [{"t": t, "s": c["label"], "u": f"{c['url']}#{sid}"} for sid, t, k in secs]


def case_pages(chs):
    for i, cs in enumerate(ALL_CASES):
        c = cs["chapter"]
        same = [x for x in ALL_CASES if x["chapter"] is c]
        others = "".join(f'<li{" class=on" if x is cs else ""}><a href="cases/{x["slug"]}.html">{x["kind"]} {x["num"]} {esc(x["name"])}</a></li>' for x in same)
        prev = (f'cases/{ALL_CASES[i - 1]["slug"]}.html', f'{ALL_CASES[i - 1]["kind"]} {ALL_CASES[i - 1]["num"]} {ALL_CASES[i - 1]["name"]}') if i else None
        nx = ALL_CASES[i + 1] if i + 1 < len(ALL_CASES) else None
        nxt = (f'cases/{nx["slug"]}.html', f'{nx["kind"]} {nx["num"]} {nx["name"]}') if nx else None
        h1 = re.sub(r'<a class="case-link".*?</a>', "", cs["h3"]).replace("<h3", "<h1", 1).replace("</h3>", "</h1>")
        body = (f'<article class="book case-page"><div class="crumb"><a href="index.html">{SITE_TITLE}</a> / '
                f'<a href="{c["url"]}">{c["label"]} {esc(c["title"])}</a> / {cs["kind"]} {cs["num"]}</div>'
                f'{h1}{cs["html"]}'
                f'<div class="case-foot"><p>本{cs["kind"]}出自《{SITE_TITLE}》{c["label"]}「{esc(c["title"])}」。'
                f'<a href="{c["url"]}#{cs["sid"]}">回到{c["label"]}阅读上下文 →</a></p>'
                f'<h4>本章全部案例</h4><ol class="case-list">{others}</ol>'
                f'<p class="muted">全书 {len(ALL_CASES)} 个案例：<a href="cases.html">案例目录</a> · '
                f'完整 Unity 工程（含本案例的配套场景）：<a href="source.html">源码、工程与 PDF 下载</a></p></div>'
                f'</article>{shell.ad("chapter-end")}{pager(prev, nxt, None)}')
        title = f"{cs['name']} - Unity UGUI 实现（{cs['kind']} {cs['num']}）· {SITE_TITLE}"
        desc = f"Unity UGUI {cs['kind']} {cs['num']}：{cs['name']}。{cs['goal']}附完整 C# 代码和可运行的配套场景。"[:155]
        write(f"cases/{cs['slug']}.html", page(title, body, nav_html(chs, c["id"]), depth=1, desc=desc,
                                               path=f"cases/{cs['slug']}.html", crumb=f"{cs['kind']} {cs['num']} {cs['name']}"))


def cases_index(chs):
    rows, cur = [], None
    for cs in ALL_CASES:
        if cs["chapter"] is not cur:
            if cur is not None:
                rows.append("</ol>")
            cur = cs["chapter"]
            rows.append(f'<h3 class="sec"><a href="{cur["url"]}">{cur["label"]} {esc(cur["title"])}</a></h3><ol class="case-list">')
        lv = f' <span class="lv-tag">{esc(cs["lv"])}</span>' if cs["lv"] else ""
        goal = f'<small>{esc(cs["goal"][:60])}{"…" if len(cs["goal"]) > 60 else ""}</small>' if cs["goal"] else ""
        rows.append(f'<li><a href="cases/{cs["slug"]}.html"><b>{cs["kind"]} {cs["num"]}</b> {esc(cs["name"])}</a>{lv}{goal}</li>')
    rows.append("</ol>")
    n = len(ALL_CASES)
    body = (f'<article class="book"><h1 class="page-title">Unity UGUI 案例目录（{n} 个）</h1>'
            f'<p class="muted">主菜单、设置页、背包、聊天、轮播、摇杆、血条飘字、小地图、排行榜、性能实验……每个案例都有完整 C# 代码和可运行的配套场景。</p>'
            f'{"".join(rows)}</article>')
    desc = f"Unity UGUI 案例大全：{n} 个案例，覆盖控件、屏幕适配、事件、自定义 Graphic、UI Shader、动效、UI 框架、HUD、循环列表与性能优化，附完整代码。"
    write("cases.html", page(f"Unity UGUI 案例大全（{n} 个，附源码）· {SITE_TITLE}", body, nav_html(chs), desc=desc, path="cases.html"))


# ---------------- 首页 ----------------
def index_page(chs):
    front = (SRC / "front.html").read_text(encoding="utf-8")
    front = re.sub(r"^\s*<section[^>]*>|</section>\s*$", "", front.strip())
    part_at = {p[0]: p for p in B.PARTS}
    toc = ['<ol class="site-toc">']
    for c in chs:
        if c["id"] in part_at:
            p = part_at[c["id"]]
            toc.append(f'<li class="pt">{p[1]} · {p[2]}<small>{esc(p[3])}</small></li>')
        n_cases = sum(1 for x in ALL_CASES if x["chapter"] is c)
        badge = f"<em>{n_cases} 个案例</em>" if n_cases else ""
        toc.append(f'<li><a href="{c["url"]}"><span class="n">{c["label"]}</span><span class="t">{esc(c["title"])}<small>{esc(c["sub"])}</small></span>{badge}</a></li>')
    toc.append("</ol>")
    n_scenes = len(list(CODE.glob("Ch*/Scenes/*.unity")))
    hero = f"""
<section class="hero">
  <div class="hero-text">
    <div class="edition">UNITY 2022.3 LTS · UGUI · TEXTMESH PRO</div>
    <h1>UGUI <em>从入门到精通</em></h1>
    <p class="sub">Unity 界面开发实战全解 · 麦冬 著</p>
    <p class="tag">从第一个按钮讲起：Canvas、屏幕适配、RectTransform、全部内置控件、自动布局、事件系统、源码原理、自定义 Graphic 与 UI Shader、
    UI 框架、游戏 HUD、循环列表与背包、性能优化与调试技巧。全书<b>免费阅读</b>，配套 Unity 工程和 PDF 免费下载。</p>
    <div class="cta"><a class="btn" href="{chs[0]['url']}">开始阅读</a><a class="btn ghost" href="source.html">下载工程与 PDF</a></div>
    <div class="chips"><span>21 章 + 附录</span><span>{len(ALL_CASES)} 个案例</span><span>{n_scenes} 个可运行场景</span><span>Unity 2022.3</span></div>
  </div>
  <figure class="shot hero-demo"><img src="img/cover-hero.jpg" alt="用 UGUI 搭出的游戏界面：头像血条、货币、背包格子与设置面板"><figcaption>封面这张游戏界面也是配套工程用 UGUI 实际渲染的</figcaption></figure>
</section>"""
    howto = f"""
<section class="howto">
  <div><b>按顺序读</b>五个部分由浅入深：入门 → 控件 → 原理 → 实战 → 精通。有 UGUI 经验可以从第三部分“原理”读起。</div>
  <div><b>打开配套场景</b>正文里标着“▶ 配套场景”的地方，在工程里都有对应的场景；打开 Demos.unity 运行，所有场景一键直达。</div>
  <div><b>代码可直接用</b>代码块右上角可复制，“完整文件”查看工程里的原文件。书里的代码就是从工程里直接取出来的，全部编译通过。</div>
  <div><b>离线阅读</b>整本书的 PDF（{PDF_PAGES} 页）和完整 Unity 工程在“<a href="source.html">源码、工程与 PDF 下载</a>”里，都免费。</div>
</section>"""
    body = f'{hero}{howto}<section class="index-toc"><h2 id="toc">目录</h2>{"".join(toc)}</section><section class="book front-inline">{front}</section>'
    write("index.html", page(f"{SITE_TITLE}：Unity UGUI 界面开发教程（免费在线阅读，附源码与 PDF）", body, nav_html(chs), cls="home", path=""))


# ---------------- 源码与下载 ----------------
def code_page(rel, p, chs):
    code = "\n".join(B.strip_markers(p.read_text(encoding="utf-8-sig").splitlines()))
    m = re.match(r"Ch(\d+)", rel)
    ch = next((c for c in chs if m and c["id"] == f"c{int(m.group(1)):02d}"), None)
    where = f"第 {int(m.group(1))} 章「{ch['title']}」" if ch else "公共代码（_Core）"
    name = rel.split("/")[-1]
    kind = "UI Shader" if name.endswith(".shader") else "编辑器脚本" if "/Editor/" in rel else "C# 脚本"
    back = f'<a href="{ch["url"]}">回到{ch["label"]}</a>' if ch else ""
    body = (f'<article class="book codepage"><div class="crumb"><a href="source.html">源码</a> / {esc(rel)}</div>'
            f'<h1 class="code-title">{esc(name)} <small>{esc(where)} · {kind}</small></h1>'
            f'<div class="codebox"><div class="code-cap">Assets/UGUIBook/{esc(rel)}</div><div class="cb-tools"><button class="cb-copy">复制</button></div>'
            f'<div class="cb-pane on"><div class="cb-links"><a href="src/{esc(rel)}" download>下载</a>{back}</div>'
            f'<pre class="code">{hl(code, lang_of(name))}</pre></div></div></article>')
    title = f"{name} - Unity UGUI {kind}源码（{where}）· {SITE_TITLE}"
    desc = f"《{SITE_TITLE}》{where}配套源码 {rel}（{kind}），可直接复制、下载，用于 Unity 2022.3 项目；对应章节有逐段讲解。"[:155]
    write(f"code/{rel}.html", page(title, body, nav_html(chs), depth=rel.count("/") + 1, desc=desc, path=f"code/{rel}.html", crumb=name))


def source_page(chs):
    rows = []
    groups = ["_Core"] + sorted(p.name for p in CODE.glob("Ch*") if p.is_dir())
    for g in groups:
        items = []
        for p in sorted((CODE / g).rglob("*")):
            if not p.is_file() or p.suffix not in (".cs", ".shader"):
                continue
            rel = p.relative_to(CODE).as_posix()
            code_page(rel, p, chs)
            items.append(f'<li><a class="f" href="code/{esc(rel)}.html">{esc(rel)}</a><a href="src/{esc(rel)}" download>下载</a></li>')
        ch = next((c for c in chs if g.startswith("Ch") and c["id"] == f"c{int(g[2:]):02d}"), None)
        title = "公共部分（_Core：UIKit、UISkin、截图与场景框架）" if g == "_Core" else f'{ch["label"]} {ch["title"]}' if ch else g
        rows.append(f'<section class="src-group"><h3>{esc(title)}<a class="zip" href="download/{g}.zip" download>打包下载 {g}.zip</a></h3><ul>{"".join(items)}</ul></section>')
    pdf_mb = (ROOT / B.PDF_NAME).stat().st_size / 1e6
    zip_mb = (ROOT / B.ZIP_NAME).stat().st_size / 1e6
    intro = f"""<article class="book"><h1 class="page-title">源码、工程与 PDF 下载</h1>
<div class="dl-main"><div><b>完整配套 Unity 工程</b><p>Unity 2022.3 LTS（作者用 2022.3.62f3）。全书 270 个 C# 脚本、13 个 UI Shader、原创精灵素材和 119 个配套场景；
用 Unity Hub 打开 project 文件夹，运行 Assets/UGUIBook/Demos.unity 就能按章节打开所有场景。</p>
<a class="btn" href="{ZIP_DL}" download="UGUI从入门到精通_配套工程.zip">下载配套工程（zip）</a><small>{zip_mb:.1f} MB · 示例代码和素材可自由用于你的项目（含商业项目）</small></div>
<div><b>整本书 PDF</b><p>{PDF_PAGES} 页，A4 排版，和网页版内容相同，适合离线阅读和打印。目录带页码，代码和配图都在。</p>
<a class="btn ghost" href="{PDF_DL}" download="UGUI从入门到精通_麦冬.pdf">下载 PDF</a><small>{pdf_mb:.1f} MB · 免费，欢迎分享本页链接</small></div></div>
<p class="muted">下面按章节列出工程里的每个源文件：点文件名在线查看，也可以单个下载，或整章打包下载。</p></article>"""
    write("source.html", page(f"源码、工程与 PDF 下载 · {SITE_TITLE}", intro + "".join(rows), nav_html(chs),
                              desc=f"《{SITE_TITLE}》免费下载：完整配套 Unity 工程（119 个可运行场景、全部 C# 与 UI Shader 源码）和整本书 PDF（{PDF_PAGES} 页），源码也可在线逐个查看。",
                              path="source.html"))
    return groups


def downloads(groups):
    dl = DIST / "download"
    dl.mkdir(parents=True, exist_ok=True)
    for g in groups:
        with zipfile.ZipFile(dl / f"{g}.zip", "w", zipfile.ZIP_DEFLATED) as z:
            for p in sorted((CODE / g).rglob("*")):
                if p.is_file() and p.suffix in (".cs", ".shader"):
                    z.write(p, f"Assets/UGUIBook/{p.relative_to(CODE).as_posix()}")
    shutil.copy2(ROOT / B.ZIP_NAME, DIST / ZIP_DL)
    shutil.copy2(ROOT / B.PDF_NAME, DIST / PDF_DL)


# ---------------- 主流程 ----------------
def book_css():
    """书的正文样式：src/00_head.html 去掉打印分页、封面、分部页、目录页"""
    css = re.search(r"<style>(.*?)</style>", (SRC / "00_head.html").read_text(encoding="utf-8"), re.S).group(1)
    css = re.sub(r"@page[^{]*\{(?:[^{}]|\{[^{}]*\})*\}", "", css)
    css = "\n".join(l for l in css.split("\n") if not re.match(r"\s*\.(cover|part|toc)\b", l))
    return "/* 书的正文样式（由 build_site.py 从 src/00_head.html 生成，去掉了打印专用部分；屏幕上的调整见 site.css） */\n" + css


def main():
    global PDF_PAGES
    import subprocess
    PDF_PAGES = int(re.search(r"Pages:\s+(\d+)", subprocess.run(["pdfinfo", ROOT / B.PDF_NAME], capture_output=True, text=True).stdout).group(1))
    if DIST.exists():
        shutil.rmtree(DIST)
    (DIST / "assets").mkdir(parents=True)
    (DIST / "assets/book.css").write_text(book_css(), encoding="utf-8")
    for f in SITE.iterdir():
        shutil.copy2(f, DIST / "assets" / f.name)
    for name in ("book.css", "site.css", "site.js"):
        VER[name] = ver(DIST / "assets" / name)
    shutil.copytree(SRC / "img", DIST / "img", ignore=shutil.ignore_patterns("_*", ".*"))
    for p in CODE.rglob("*"):
        if p.is_file() and p.suffix in (".cs", ".shader"):
            d = DIST / "src" / p.relative_to(CODE)
            d.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(p, d)
    chs = chapters()
    search = []
    chapter_pages(chs, search)
    case_pages(chs)
    cases_index(chs)
    search += [{"t": f"{cs['kind']} {cs['num']} {cs['name']}", "s": cs["chapter"]["label"] + " · 单独页面", "u": f"cases/{cs['slug']}.html"} for cs in ALL_CASES]
    index_page(chs)
    groups = source_page(chs)
    (DIST / "assets/search.json").write_text(json.dumps(search, ensure_ascii=False), encoding="utf-8")
    downloads(groups)
    n = sum(1 for _ in DIST.rglob("*.html"))
    print(f"[site] {DIST}（{len(chs)} 章，{len(ALL_CASES)} 个案例页，共 {n} 个页面）")


if __name__ == "__main__":
    main()
