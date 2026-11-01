# 《UGUI 从入门到精通》作者指南（每位章节作者必读）

## 0. 这本书
- 书名：《UGUI 从入门到精通：Unity 界面开发实战全解》，笔名 **麦冬**，中文，A4 PDF + 网页版，面向“会一点 Unity、能看懂基础 C#”的读者，一路带到能读源码、做优化的程度。前两部分要照顾零基础读者（编辑器操作写成步骤），后面讲原理与工程。
- **版本锁定：Unity 2022.3.62f3 + com.unity.ugui 1.0.0 + com.unity.textmeshpro 3.0.7 + com.unity.inputsystem 1.14.0**（Active Input Handling = Both，场景默认用 StandaloneInputModule）。内置渲染管线（Built-in），**Gamma 色彩空间**。
- 所有 API 以工程里的源码为准：
  `<你的工程>/Library/PackageCache/com.unity.ugui@1.0.0/Runtime/`（UI/Core/*.cs、EventSystem/*.cs）
  `<你的工程>/Library/PackageCache/com.unity.textmeshpro@3.0.7/Scripts/Runtime/`
  `<你的工程>/Library/PackageCache/com.unity.inputsystem@1.14.0/InputSystem/Plugins/UI/`
  Unity 内置 Shader 官方源码（2022.3.62f3，含 `DefaultResourcesExtra/UI/UI-Default.shader`、`CGIncludes/UnityUI.cginc`）：`/Users/zsl/Documents/work/myproject/books/ugui/tools/builtin_shaders/`。自定义 UI Shader 以它为底本改写，Shader 里写全（不要 UsePass 内置 UI Shader）。
  **用到任何类/方法/属性前先 grep 源码确认存在、签名一致。** 不许凭记忆编 API。讲源码时摘最必要的片段并注明文件名（如“摘自 com.unity.ugui 1.0.0 `Runtime/UI/Core/Graphic.cs`”）。
- 卖点：**零基础起步、控件全覆盖、案例驱动、源码原理讲透、每段代码都编译通过、每张截图都是配套工程实拍、每个交互案例都有可运行的配套场景**。内容要多、要扎实、要讲透“为什么”。

## 1. 你的工作区
- 你有一份独立的 Unity 工程副本：`/Users/zsl/Documents/work/myproject/books/ugui/workers/<你的编号>/`（已导入好全部包、TMP 资源和中文字体）。
- 运行时代码放 `Assets/UGUIBook/ChNN/`（NN 两位数字），编辑器代码（配图、场景搭建、编辑器工具）放 `Assets/UGUIBook/ChNN/Editor/`。
- C# 命名空间一律 `UGUIBook.ChNN`；Shader 名一律 `UGUIBook/ChNN/名字`；材质资源放 `Assets/UGUIBook/ChNN/Materials/`（用 `s.MatAsset(...)` 创建）。
- 章节正文写到 `/Users/zsl/Documents/work/myproject/books/ugui/src/cNN.html`（一章一个文件）。
- **只动你自己负责的章节文件夹和 cNN.html。** 不要改 `_Core/`、别的章节、`00_head.html`、`build.py`。`_Core` 缺功能就在自己章节里写辅助代码；发现 `_Core` 有 bug，在最终汇报里写明。
- 公共资源（全部原创/开源，可随意用）：
  - `UISkin.Sprite("名字")` 取精灵（运行时也能用，资源在 `_Core/Resources/UISkin.asset`）。现有精灵：
    九宫格：`round_4 round_8 round_16 round_24 round_32 round_outline_8 round_outline_16 pill shadow_16 glow_24 slot frame_select`
    形状：`circle circle_small ring ring_thin circle_soft gradient_v gradient_h checker stripes noise`（noise 可读写，Shader 用）
    白色图标（用 Image.color 染色）：`check close plus minus arrow_up/down/left/right back star star_outline heart home gear user search lock sound mute music bag mail bell trophy gift chat play pause refresh menu pointer info warning`
    彩色物品：`item_sword item_shield item_potion_red/blue/green item_gem item_coin item_apple item_key item_scroll item_bow item_helmet item_ring`
    头像：`avatar_1`～`avatar_6`；风景图：`bg_landscape bg_night`（1024×512）
  - 字体：`UISkin.Instance.font`（Noto Sans SC 子集 SDF，动态，已是 TMP 默认字体）、`fontBold`、`legacyFont`（旧版 Text 用）。字符集是 GB2312 + 常用符号，生僻字/emoji 会显示方块，别用。
  - 配色：`UISkin.Instance.primary/accent/ink/muted/surface/page/dark/success/danger`。
  - `UIKit`（`_Core/Runtime/UIKit.cs`，先通读一遍）：`Canvas() EnsureEventSystem() Node() Place() Stretch() Size() Rt() Image() Panel() Card() Icon() Text() LegacyText() Button() IconButton() Toggle() Slider() Scrollbar() Input() Dropdown() ScrollView() VLayout() HLayout() Grid() Hex()`。它内部调用 Unity 的 `DefaultControls`/`TMP_DefaultControls`，结构和编辑器菜单创建的一致。
  - 需要别的图片：在自己章节里用 C# 程序生成（`new Texture2D` + SetPixels，再 `Sprite.Create`），或者用现有精灵组合。不要从网上下载任何素材。

## 2. 编译检查 + 场景 + 出图（必须跑通）
```
/Users/zsl/Documents/work/myproject/books/ugui/tools/unity_check.sh /Users/zsl/Documents/work/myproject/books/ugui/workers/<编号> ch05,ch06
/Users/zsl/Documents/work/myproject/books/ugui/tools/unity_check.sh <工程> ch05 -shot ch05-xxx     # 只拍一张（不存场景）
/Users/zsl/Documents/work/myproject/books/ugui/tools/unity_check.sh <工程> ch05 -noshots           # 只编译 + 生成 [BookScene] 场景
```
- 它会：检查 C# 编译、编译本章所有 Shader 的所有 Pass，执行本章所有 `[BookScene]` 和 `[BookShot]`，场景存到 `Assets/UGUIBook/ChNN/Scenes/`，图片输出到 `books/ugui/src/img/<id>.jpg`，层级数据输出到 `books/ugui/src/tree/<id>.txt`，最后打印 `UGUIBOOK-ERROR ...` 和 `UGUIBOOK-SUMMARY errors=N`。
- 全机最多 3 个 Unity 同时跑，脚本会自动排队（可能要等几分钟，正常）。C# 编译错误会让整个运行中止（输出 `error CS...`），先修它。
- **交付标准：`errors=0`，并且你用 Read 工具亲眼看过每一张图：布局正确、文字没截断没方块、能说明问题、好看。** 不满意就调了重拍。

### 配图与场景 API（`UGUIBook.Core.Shot`，源码 `_Core/Editor/Shot.cs`、`ShotRunner.cs`）
```csharp
using TMPro; using UGUIBook.Core; using UnityEngine; using UnityEngine.UI;
namespace UGUIBook.Ch06 { static class Shots {
  // 输出 960×540 逻辑尺寸（实际像素 ×1.5），画布参考分辨率默认 1920×1080；Scene= 同时存成可运行场景
  [BookShot("ch06-states", W = 960, H = 400, Panels = 1, Scene = "Ch06_ButtonStates")]
  static void States(Shot s) {
      // s.Root 是默认 Canvas（Overlay + CanvasScaler ScaleWithScreenSize 1920×1080 match 0.5）；s.Canvas / s.Scaler / s.Cam 可直接改
      var b = UIKit.Button(s.Root, "开始游戏", null, 400, 110, "play");
      b.Rt().Place(0, 0, 400, 110);                      // 居中放置；Place(anchor, pos, size) 指定锚点
      b.gameObject.AddComponent<PressScale>();          // 本章的运行时组件：场景里能直接用
      s.State(b, "Pressed");                            // 截图时显示为按下态（Normal/Highlighted/Pressed/Selected/Disabled）
      s.Background(UIKit.Hex("#24283B"));               // 背景色（默认 #EEF0F6）
      s.Ref(1080, 1920, 1);                             // 改参考分辨率（竖屏手机界面时配合 W=405,H=720）
      s.BeforeCapture(() => { /* 截图前执行：此时布局已按截图尺寸算好，可读 rect、调方法模拟运行时状态 */ });
      // 标注层（只出现在截图里，不进场景）：
      s.Box(b.Rt(), null, "Button");                    // 红框 + 标签
      s.Badge(b.Rt(), "1");                             // 圆形编号，默认在左上角
      s.Anchors(someRect);                              // 画出锚点（蓝色三角）和轴心（圆圈）
      s.Label(new Vector2(0, -300), "文字说明");          // 标注层坐标：原点在画面中心，单位 = 参考分辨率像素
      s.LabelAt(b.Rt(), new Vector2(1, 0.5f), new Vector2(160, 0), "右侧说明");
      s.Arrow(() => s.At(a, new Vector2(1, .5f)), () => s.At(b.Rt(), new Vector2(0, .5f)));
  }
  [BookScene("Ch06_Playground")] static void Playground(Shot s) { /* 只生成场景、不出图 */ }
}}
```
- 截图时 Overlay 画布会被临时改成 Screen Space - Camera 挂到截图相机上（看起来一样）。你自己建的 World Space / Camera 画布保持原样，相机 `s.Cam` 默认是正交相机、位置 (0,0,-10)，World Space 的图请自己摆相机（改 `s.Cam.orthographic=false` 等）。
- 多联图 `Panels = 3` 时方法会被调用 3 次，`s.Panel` 是当前格（0 开始），横向拼接；场景只存第 0 格。三联图建议 W=640,H=480 左右。
- **批处理拍图时不会进入 Play 模式**：`Awake/Start/Update/协程` 都不会执行，`Time` 不走。要拍“运行中的样子”（列表已填充、进度 60%、动画进行到一半），就把这些状态做成公开方法/属性（如 `Init()`、`SetProgress(0.6f)`、`Evaluate(t)`），在搭景代码或 `s.BeforeCapture` 里直接调用；运行时由 `Start/Update` 调同一套方法。
- 图片 id 规则：`chNN-简短英文`，全局唯一；场景名 `ChNN_英文名`。

### 配套场景的规矩（读者下载工程后会直接打开运行，必须真的能用）
- 每个**有交互的案例**都要有配套场景（`Scene = "..."` 或 `[BookScene]`），正文用 `<div class="scene">ChNN/Scenes/ChNN_Xxx.unity</div>` 标出来。场景里最好有一行提示文字（如“拖动滑块试试”）。
- 场景是把搭景代码的结果直接存盘，所以：
  - 运行时逻辑写在 MonoBehaviour 里，**在 Awake/Start 里 AddListener**；搭景时 `AddListener` 不会被序列化。若要演示“Inspector 里绑定 OnClick”，用 `UnityEditor.Events.UnityEventTools.AddPersistentListener`（只能在 Editor 代码里用）。
  - 组件之间的引用用 `[SerializeField]` 字段，在搭景代码里赋值（会存进场景）。
  - 材质必须是资源文件（`s.MatAsset("Ch14", "Gray", "UGUIBook/Ch14/Gray")`）；运行时 `new Texture2D` 生成的图不会存进场景，场景里需要的程序化图片请在组件的 Awake 里生成，或换用现有精灵。
- 预制体：需要预制体的（如框架、列表项）用 `PrefabUtility.SaveAsPrefabAsset` 在搭景代码里存到 `Assets/UGUIBook/ChNN/Prefabs/` 或 `ChNN/Resources/`（Resources.Load 用），再在场景里引用。

## 3. C# 代码规范
- 中文注释，适量、解释“为什么”，别逐行翻译代码。命名与 Unity 习惯一致（`m_` 私有字段可用可不用，但一章内统一）。
- 运行时代码不得引用 `UnityEditor`（要用就 `#if UNITY_EDITOR` 包住）。不要依赖任何第三方插件（DOTween 等只在正文提及）。
- 书中要引用某段代码时，用 `//#region 名字` 和 `//#endregion` 包起来，正文 `data-region="名字"` 引用；标记行在书里会自动隐藏。代码要完整可用，读者可以直接拷走。
- 搭景代码（Editor/Shots.cs）一般不进书，除非是在讲“用代码创建 UI”。书里展示的应该是读者在自己项目中会写的代码（运行时组件、编辑器工具）。
- 性能数据：只能写你在配套工程里**实际测出来**的（例如写一个 `[BookShot]` 或编辑器方法用 `System.Diagnostics.Stopwatch` 测，或者 `[BookScene]` 场景配一个测量脚本供读者自测），并写清“作者环境：Apple M 系列 Mac / Unity 2022.3.62f3 / 编辑器下”。测不了的写定性结论，绝不编数字、不编“官方说”。

## 4. 正文 HTML 规范（src/cNN.html）
文件结构（**第一行的 section 属性必须照写**，build.py 靠它生成目录）：
```html
<section class="chapter" id="c06" data-title="Button 与 Selectable 体系" data-sub="状态、过渡、导航与各种按钮玩法">
<div class="chap-head"><div class="num">CHAPTER 06<b>第 6 章</b></div><h2>Button 与 Selectable 体系</h2><p class="lead">一两句导语。</p></div>
<div class="learn"><div class="h">本章你会学到</div><div>……</div><div>……</div><div>……</div><div>……</div></div>

<h3 class="sec">6.1 小节标题</h3>          <!-- 原理/讲解小节：编号 章.节 -->
<p>…</p>
<h4>小标题</h4>

<h3 class="case"><span class="no">案例 6.1</span>长按连发按钮<span class="lv">入门</span></h3>   <!-- lv: 入门/进阶/高阶 -->
<p><b>目标</b>：……</p>
<ol class="steps"><li>在 Hierarchy 右键 <span class="menu">UI › Button - TextMeshPro</span>……</li></ol>   <!-- 编辑器操作步骤 -->
<pre class="code" data-file="Ch06/HoldRepeatButton.cs"></pre>                       <!-- 整个文件 -->
<pre class="code" data-file="Ch06/HoldRepeatButton.cs" data-region="tick"></pre>    <!-- 片段 -->
<pre class="code" data-lang="csharp">// 直接写在 HTML 里的短片段（如摘 UGUI 源码），注意 &lt; &gt; &amp; 转义
</pre>
<figure class="shot"><img src="img/ch06-hold.jpg"><figcaption><b>图 6-3</b>……</figcaption></figure>
<div class="tree" data-tree="ch06-hold" data-root="Canvas/Button_开始游戏" data-depth="3" data-hide="Shadow"></div>  <!-- 层级图 -->
<div class="scene">Ch06/Scenes/Ch06_Hold.unity</div>      <!-- 配套场景 -->
<div class="note tip"><b>提示</b>……</div>     <!-- note / note tip / note warn / note deep(深入) -->
<div class="box"><h4>原理：为什么……</h4><p>…</p></div>   <!-- box / box accent -->
<table>…</table>  <span class="menu">Window › Analysis › Frame Debugger</span>  <kbd>Ctrl</kbd>  <code>行内代码</code>
<div class="cmp"><div class="good"><h5>✓ 推荐</h5>…</div><div class="bad"><h5>✗ 避免</h5>…</div></div>
<div class="kv"><span>属性名</span><span>说明</span>…</div>   <!-- 两列“属性-说明”清单，讲 Inspector 字段很好用 -->
<figure><svg viewBox="0 0 600 260" width="600">…原理示意图…</svg><figcaption><b>图 6-1</b>…</figcaption></figure>
<div class="formula"><math display="block">…MathML…</math></div>

<div class="summary"><h4>本章小结</h4><ul><li>…</li></ul></div>
<div class="exercise"><h4>练习</h4><ol><li>…</li></ol></div>
</section>
```
- `data-tree` 的层级图来自同名配图搭出来的场景：`data-root` 是要显示的子树路径（从末尾按段匹配，如 `Canvas/Panel`），`data-depth` 最大深度，`data-hide` 要隐藏的节点名（逗号分隔，连同子节点），`data-comps="0"` 不显示组件名，`data-title` 改标题。讲控件结构时非常好用（比截 Hierarchy 面板清楚）。
- 案例写法：**目标 → 原理/思路（必要时配 SVG 示意图）→ 编辑器步骤（入门章节）→ 完整代码 → 关键代码讲解（引用 region 片段逐段讲）→ 效果截图 → 配套场景 → 常见坑/调参建议/扩展方向**。
- 编辑器界面（Inspector、Hierarchy、锚点预设面板、Sprite Editor 等）无法截真图，需要时用内联 SVG 画**示意图**（字段名用真实英文名，图注写“示意”）。原理图（坐标系、锚点关系、事件流程、重建流程、合批、模板缓冲等）也用 SVG。SVG 颜色：#5b4bd6(主)、#ff8a3d(强调)、#1f2436(墨)、#7a8299(灰)、#e0deef(线)、#2fb36a(绿)、#e5484d(红)；SVG 内文字 font-size ≥ 12（viewBox 600 宽时），字体 PingFang SC。图多一点，书会更好懂。
- 图号 `图 N-k`、案例号 `案例 N.k`、小节号 `N.k` 都在本章内连续编号。
- **分量要足**：每章 5–10 个案例或等量的原理小节，正文（不含代码）每章至少 9000 汉字，讲透原理和“为什么”，补充常见坑、性能提示、移动端注意事项。
- 语言：清楚、直接、像有经验的客户端程序员带新人；不灌水、不营销腔；术语首次出现给英文（如“锚点（Anchor）”）。
- **原创**：不要大段照抄 Unity 文档、博客或书籍；引用源码只摘必要片段。不确定的写“以实际版本为准”。

## 5. 预览排版
```
cd /Users/zsl/Documents/work/myproject/books/ugui && .venv/bin/python tools/build.py --only c06,c07
pdftoppm -r 50 -png -f 1 -l 6 build/preview_c06_c07.pdf <你的 scratchpad>/pv_c06   # 抽几页转图用 Read 看
```
- 检查：代码块没有溢出、图片正常、SVG 文字清楚、层级图正常、没有空白大洞。
- build.py 读代码时优先找 `project/`，再找 `workers/*/`，所以你在自己 worker 里写的代码能被预览到。

## 6. 完成后汇报（你的最终回复）
- 每章：标题、小节/案例列表、配图 id 列表、场景列表、`unity_check` 最终 `errors=0` 的输出行、预览 PDF 页数。
- 任何没做完/有疑问/发现 `_Core` 问题的地方，如实写出来。

## 7. 已知的坑（会持续补充）
- TMP 文本在批处理下由 ShotRunner 统一 `ForceMeshUpdate`；如果你在 `BeforeCapture` 里改了文字，ShotRunner 之后还会再刷新一次，不用自己处理。
- `ContentSizeFitter`/`LayoutGroup` 的结果在 `Canvas.ForceUpdateCanvases()` 之后才有效；搭景代码里读 `rect` 得到的是旧值，要读就放进 `s.BeforeCapture`。
- `Toggle.isOn` 在编辑器下切换是立即生效的（不播放渐变），`Selectable` 的状态用 `s.State()` 设置。
- `ScrollRect` 的 `normalizedPosition` 依赖 Content 尺寸，设置它要放进 `s.BeforeCapture`。
- `new Material()` 存不进场景，用 `s.MatAsset`。
- Gamma 色彩空间：UI 半透明混合结果和 Linear 不同，讲到时注明。
- 出图时 Overlay 画布被临时改成 Camera 模式（planeDistance=100），场景里的 3D 物体会挡住 UI。要在同一张图里展示 3D + “Overlay 效果”，把该画布手动设成 Camera 模式、planeDistance 很小（如 0.5）；给读者的场景仍用真正的 Overlay。
- 批处理出图时 EventSystem / GraphicRaycaster 不运行，`EventSystem.RaycastAll` 返回空。需要截“射线命中结果”的图时，写一个注释清楚的编辑器模式兜底；运行时代码路径必须用真实的 RaycastAll。
- 批处理不跑 ScrollRect.LateUpdate：设置 normalizedPosition / content 位置后，滚动条的 size/value 不会自动更新，截图前要手动同步（参考 workers/w3 的 Kit.SyncBars 思路）。
- TMP 的光标/选区只在 Play 模式创建；TMP_Dropdown 展开态可以真调 Show()（先反射补调 Start），截完清掉 m_Dropdown/m_Blocker 私有引用，否则关场景时报错。
- UIKit.Card / Panel 的圆角只支持 4/8/16/24/32，其他值会报 UISkin 错误（计入 errors）。
- 工程 MSAA 会作用到 RenderTexture；要看边缘质量的图设 camera.allowMSAA = false。
- 性能实测图每次出图都会重测，数字会变：正文里的数字必须和最后一次出图一致；最终合并时这类图不重渲（在汇报里列出性能图的 id）。
- **序列化字段不要叫 `m_Name` / `m_Script` / `m_Enabled` / `m_GameObject`**：和 UnityEngine.Object 自带的序列化字段重名，场景里会出现重复键，运行时引用变成 null（截图不进 Play 模式所以发现不了）。用 `m_NameText` 之类。
- Sequence/容器类补间要检查子补间的目标是否已销毁，否则切场景后继续写已销毁对象。
