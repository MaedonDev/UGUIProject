# 全书大纲（各章作者据此写作；可在本章范围内增补案例，但不要写到别人的章节里去，需要时写“见第 N 章”）

读者画像：会一点 Unity 基本操作（或完全没做过 UI）的 C# 初学者 → 想读懂源码、做性能优化的中高级开发者。前两部分照顾小白（编辑器操作步骤要写清楚），后三部分讲原理与工程。

## 第一部分 入门：从零做出第一个界面
**c01 认识 UGUI：做出第一个界面**
- Unity 的三套 UI（UGUI / UI Toolkit / IMGUI）是什么、各用在哪、本书为什么讲 UGUI；com.unity.ugui 包与源码位置
- 新建 Canvas 时自动生成了什么（Canvas、Canvas Scaler、Graphic Raycaster、EventSystem）；Scene 视图 2D 模式、Rect Tool（T）、Game 视图分辨率设置
- 层级顺序 = 绘制顺序（后面的盖住前面的）；GameObject 激活与 SetActive
- 案例：主菜单（背景 + 标题 + 三个按钮）；点击计数器（Inspector 里绑 OnClick 与代码 AddListener 两种方式）；面板切换（主菜单 ↔ 设置页）；退出按钮（Application.Quit 与编辑器下的处理）
- 新手最常见的五个坑：没 EventSystem、被透明图片挡住、Raycast Target、Canvas 在相机后面、按钮在 Scene 里看不到
- 配套工程怎么用（目录结构、UIKit 是什么、场景怎么打开）
**c02 Canvas 与屏幕适配**
- 三种 Render Mode（Overlay / Camera / World Space）的区别与选择；Sorting Layer / Order in Layer；Plane Distance；Pixel Perfect
- Canvas Scaler 三种模式；Scale With Screen Size 的 Match 原理（对数插值公式 + 图）；Reference Pixels Per Unit；Expand / Shrink
- 同一界面在 16:9 / 4:3 / 21:9 / 竖屏下的表现（多联图对比不同 Match）
- 多个 Canvas 叠放、嵌套 Canvas（Override Sorting）、Additional Shader Channels
- 案例：多分辨率适配对比；World Space 告示牌/头顶名字；Camera 模式下让 3D 物体/粒子夹在两层 UI 之间
**c03 RectTransform 与锚点**
- Rect、anchorMin/anchorMax、pivot、anchoredPosition、sizeDelta、offsetMin/offsetMax、rect 之间的换算（公式 + 示意图）
- 锚点预设 16 种 + Shift/Alt；锚点重合 vs 分开（固定尺寸 vs 拉伸）；Raw Edit / Blueprint 模式
- 代码设置：SetSizeWithCurrentAnchors、SetInsetAndSizeFromParentEdge、ForceUpdateRectTransforms
- 坐标转换：屏幕 ↔ UI 本地（RectTransformUtility.ScreenPointToLocalPointInRectangle / WorldToScreenPoint）
- 案例：App 布局（顶栏 + 底栏 + 自适应内容区）；跟随鼠标且不出屏幕的 Tooltip；刘海屏 SafeArea 适配组件；九宫格位置的角标/红点
**c04 Image、RawImage 与精灵**
- 纹理导入为 Sprite (2D and UI)、Pixels Per Unit、Sprite Editor 设九宫格边框
- Image 四种 Type：Simple / Sliced / Tiled / Filled（Fill Method/Origin/Clockwise）；Preserve Aspect；Set Native Size；Pixels Per Unit Multiplier
- Raycast Target、alphaHitTestMinimumThreshold（不规则按钮，需要 Read/Write）
- RawImage 与 UV Rect；Image 与 RawImage 怎么选；Sprite Atlas 初识（详见第 20 章）
- 案例：九宫格对话框；技能冷却（Radial 360）；带延迟白条的血条；不规则形状按钮；无限滚动背景（UV Rect）；头像框
**c05 文字：Text 与 TextMeshPro**
- 旧版 Text 与 TMP 对比；SDF 原理（为什么放大不糊、描边便宜）；字体资源 Font Asset Creator 各参数（采样大小、Padding、图集、Render Mode、Static/Dynamic、字符集）
- 中文字体方案：动态字体 + Fallback、静态常用字库、多图集
- 排版：对齐、自动换行、溢出模式、Auto Size（及其代价）、行距字距、Margins
- 富文本：color/size/b/i/u/s/mark/sprite/link/gradient/voffset/nobr 等
- 材质预设：描边、阴影（Underlay）、发光；同一字体多种样式
- 案例：创建中文字体；描边/投影标题；图文混排（Sprite Asset 表情）；可点击链接（TMP_TextUtilities.FindIntersectingLink）；打字机效果（maxVisibleCharacters）；数字跳动

## 第二部分 控件：每一个内置控件的用法与扩展
**c06 Button 与 Selectable 体系**
- Selectable 状态机（Normal/Highlighted/Pressed/Selected/Disabled）与源码；Transition：Color Tint / Sprite Swap / Animation，Color Multiplier、Fade Duration
- Navigation（键盘/手柄）、interactable 与 CanvasGroup.interactable
- onClick 的绑定方式、闭包陷阱（循环里 AddListener）、RemoveListener
- 案例：按钮五种状态对比图；长按连发；双击；防连点（冷却）；按下缩放 + 音效反馈；单选按钮组；手柄导航菜单；自定义 Selectable（页签按钮）
**c07 Toggle、Slider 与 Scrollbar**
- Toggle 结构、isOn、onValueChanged、SetIsOnWithoutNotify；ToggleGroup 与 allowSwitchOff；页签切换
- Slider 结构（Background/Fill Area/Handle Slide Area）、方向、wholeNumbers、只读进度条；Scrollbar 的 size/numberOfSteps
- 案例：设置界面（音量/画质/开关）；iOS 风格开关；Tab 页签；加载进度条；双滑块价格区间；Slider 与输入框双向同步
**c08 InputField 与 Dropdown**
- TMP_InputField 结构（Text Area/Placeholder/Caret）、Content Type、Line Type、Character Limit、事件（onValueChanged/onEndEdit/onSubmit/onSelect/onDeselect）
- 输入验证（Content Type、onValidateInput、自定义 TMP_InputValidator）；移动端软键盘要点；密码可见切换
- TMP_Dropdown 结构（Template 模板）、动态增删选项、带图标选项、事件
- 案例：登录表单（校验与错误提示）；聊天输入（回车发送、保持焦点）；搜索框实时过滤列表；带国旗图标的语言选择；数字步进器
**c09 ScrollRect 与遮罩**
- ScrollRect 结构（Viewport/Content/Scrollbar）、Movement Type、Inertia、Deceleration Rate、Scroll Sensitivity、normalizedPosition
- Content 尺寸与 ContentSizeFitter + LayoutGroup；Mask 与 RectMask2D 初识（原理见第 14 章）
- 滚动到指定项、分页吸附（PageView + 指示点）、嵌套滚动（横竖方向冲突的解决）、下拉刷新
- 案例：商城列表；滚动到第 N 项；轮播图 + 圆点；横竖嵌套；下拉刷新/上拉加载
**c10 自动布局**
- 布局系统两阶段（先算尺寸、再定位置）、ILayoutElement/ILayoutController/ILayoutGroup/ILayoutSelfController，min/preferred/flexible
- Horizontal/Vertical/Grid Layout Group 全部参数；Child Control Size × Force Expand 组合表；Layout Element；Content Size Fitter；Aspect Ratio Fitter
- 常见坑：Fitter 嵌套警告、父子争夺控制、布局一帧延迟、LayoutRebuilder.ForceRebuildLayoutImmediate
- 案例：自适应聊天气泡；表单布局；按宽度自动算列数的网格；自定义流式布局 FlowLayoutGroup（标签云）；可折叠分组列表

## 第三部分 原理：事件、重建与渲染
**c11 事件系统**
- 三件套：EventSystem、InputModule、Raycaster；一次点击从输入到回调的全流程（读源码：StandaloneInputModule.Process）；PointerEventData 字段
- 全部事件接口表；ExecuteEvents 与向上冒泡（GetEventHandler）；拖拽（IBeginDrag/IDrag/IEndDrag/IDrop、dragThreshold、IInitializePotentialDragHandler）
- blocksRaycasts/CanvasGroup；EventSystem.IsPointerOverGameObject；PhysicsRaycaster 让 3D 物体响应；EventTrigger 组件（为什么不推荐大量用）；Input System 包的 InputSystemUIInputModule；多点触控
- 案例：可拖拽窗口；拖放背包格子（交换/丢弃）；虚拟摇杆；点击空白关闭弹窗；点击 3D 物体；手势（双指缩放）；事件冒泡与拦截
**c12 Graphic、Canvas 与重建（源码原理）**
- Graphic/MaskableGraphic/Image/Text 继承图；CanvasRenderer；CanvasUpdateRegistry 两个队列（Layout、Graphic）与 PerformUpdate 顺序
- SetLayoutDirty / SetVerticesDirty / SetMaterialDirty 各由什么触发；Rebuild 流程；Canvas.willRenderCanvases
- Canvas 的合批：Native 侧如何把子元素合并、Depth 计算、什么会打断合批（材质、纹理、重叠插入）
- VertexHelper、UIVertex；IMaterialModifier、IMeshModifier；IClipper/IClippable
- 实验（在配套工程里实测，写清环境）：改颜色 / 改位置 / 改文字 / 改 sprite 分别触发哪种重建，统计次数和耗时
**c13 自定义 Graphic 与网格效果**
- OnPopulateMesh：无图圆角矩形、圆环/扇形进度、多边形雷达图（属性图）、折线图、贝塞尔连线
- BaseMeshEffect：顶点渐变、Outline/Shadow 原理与为什么贵、镜像、倾斜；TMP 逐字顶点动画（波浪、抖动）
- 让自定义 Graphic 支持 Mask/RectMask2D、raycast 精确命中（IsRaycastLocationValid）
- 案例：上述每一项都做成组件
**c14 UI Shader、遮罩与模板**
- UI/Default Shader 逐段讲解（Stencil 属性、UNITY_UI_CLIP_RECT、UNITY_UI_ALPHACLIP、_TextureSampleAdd、ColorMask、顶点色）
- 自定义 UI Shader：灰度（禁用态图标）、流光、溶解、波纹/扭曲、HSV 调色；保持 Mask/RectMask2D 兼容的要点
- Mask 原理（模板缓冲写入/测试、嵌套层级位）vs RectMask2D 原理（裁剪矩形、Softness），对比表；软遮罩
- 新手引导挖洞（镂空遮罩 + 事件穿透给高亮按钮）；圆形头像；模糊背景（降采样 RenderTexture 方案）
- Shader 名统一 `UGUIBook/Ch14/xxx`

## 第四部分 实战：动效、框架与常见游戏界面
**c15 UI 动效**
- Animator 做 UI 动画的问题（每帧标脏、常驻开销）；Animation vs 代码补间
- 手写轻量 Tween（Update 驱动、缓动函数、链式、Unscaled Time）；缓动曲线图
- CanvasGroup 淡入淡出；弹窗弹出回弹；列表依次入场；按钮呼吸；数字滚动；飞金币（贝塞尔路径飞向图标）；屏幕转场（擦除/溶解）；Toast 提示
- DOTween 等第三方库只提及思路，不放其代码
**c16 UI 框架与界面管理**
- 为什么需要框架；UIManager：层级（Background/Normal/Popup/Toast/Guide/Top）、界面栈与返回键、打开/关闭生命周期、预制体加载（Resources 实现，讨论 Addressables）、缓存与销毁策略
- 弹窗队列；通用确认框（回调）；Toast；MVP/简单数据绑定（Observable 属性）；事件中心
- 编辑器工具：按命名规则自动生成 UI 绑定代码
- 案例：一个可直接用的小框架 + 示例界面（主界面 → 背包 → 确认框）
**c17 游戏 HUD 实战**
- 头顶血条跟随（WorldToScreenPoint、屏幕外/背后处理、缩放）；伤害飘字（对象池）；屏幕边缘目标指示箭头
- 小地图（图标方案 + RenderTexture 方案）；技能按钮（冷却、充能层数、长按蓄力）；受击方向指示；Buff 栏
- 对话框（打字机 + 选项）；任务追踪
**c18 列表、背包与无限滚动**
- 一万项列表为什么卡；虚拟化/循环列表原理
- 实现循环列表：固定高度、可变高度、网格；数据驱动 + 复用；跳转到指定项
- 聊天列表（可变高度、新消息自动滚到底）；背包（网格虚拟化、排序筛选、拖拽、物品详情 Tips）；排行榜（我的排名固定行）
**c19 适配、多语言与 3D 混合**
- 横竖屏切换、不同比例的布局切换策略；Safe Area 深入
- 多语言：本地化表（CSV/JSON）+ LocalizedText 组件 + 字体切换 + 文本长度差异的排版对策；简述 Unity Localization 包
- UI 中显示 3D 模型（RenderTexture 角色展示 + 拖拽旋转）；粒子特效与 UI 的排序（Camera 模式 + sortingOrder）
- World Space UI 交互；UI 跟随 3D 物体的另一种做法

## 第五部分 精通：性能优化、调试与工程化
**c20 性能优化**
- 工具：Profiler 的 UI / UI Details 模块、Frame Debugger、Overdraw 视图、Memory Profiler；先测再改
- 四大开销：重建（Layout/Graphic）、合批（Batch 打断）、填充率（Overdraw/半透明）、射线检测；每类给可复现的实验（在配套工程实测，写清机器与版本，不编数字）
- 动静分离 Canvas；避免深层 Layout 嵌套；隐藏 UI 的方法对比（SetActive / CanvasGroup / Canvas.enabled / 移出屏幕 / 缩放 0）
- Sprite Atlas 与合批；TMP 优化；Raycast Target 清理；Mask vs RectMask2D 的开销；对象池；Animator 开销；全屏界面关闭 3D 相机；移动端注意事项
- 一页优化清单
**c21 调试、工具与技巧**
- “点了没反应”排查流程图；EventSystem 的 Inspector 预览；运行时显示鼠标下所有射线命中的调试工具
- 编辑器扩展：批量替换字体/精灵、检查丢失引用、一键关闭非交互元素的 Raycast Target、UI 预制体规范检查、高清截图工具
- 技巧合集（30 条左右，短小实用）；UGUI 与 UI Toolkit 的取舍与迁移思路
**ax 附录**
- 常用 API 速查（RectTransform、Graphic、Selectable、ScrollRect、LayoutRebuilder、RectTransformUtility、TMP）
- 事件接口与执行顺序表；重建流程图；常见问题 FAQ；术语表；快捷键
