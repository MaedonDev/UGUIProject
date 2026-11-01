# 《UGUI 从入门到精通：Unity 界面开发实战全解》配套工程

作者：麦冬 · 2026 年版

## 打开方式

1. 安装 **Unity 2022.3 LTS**（作者使用 2022.3.62f3，其他 2022.3.x 一般也可以）。
2. 用 Unity Hub「Add / 添加」本文件夹里的 `project` 目录并打开，首次导入需要几分钟。
3. 打开 `Assets/UGUIBook/Demos.unity`，点击运行：这是全书所有配套场景的目录，点哪个进哪个，左下角「返回目录」回来。
4. 如果中文显示为方块：菜单 **UGUIBook › Setup Project**。

## 目录结构

```
project/Assets/UGUIBook/
├── Demos.unity          全部配套场景的目录（入口）
├── _Core/               公共部分：UIKit（用代码搭 UI 的工具）、UISkin（精灵/字体/配色）、截图与场景生成框架
│   ├── Sprites/         原创程序绘制的 UI 精灵（九宫格底图、图标、物品、头像），可随意使用
│   └── Fonts/           Noto Sans SC 子集（SIL OFL 1.1，许可证见 NotoSansSC_OFL.txt）
├── Ch01/ … Ch21/        每章的代码（运行时组件在 ChNN/，编辑器代码在 ChNN/Editor/）
│   └── Scenes/          每章的配套场景
```

书中代码块标题上的路径（如 `Assets/UGUIBook/Ch06/HoldRepeatButton.cs`）就是文件在工程里的位置。

## 版本

Unity 2022.3.62f3 · com.unity.ugui 1.0.0 · com.unity.textmeshpro 3.0.7 · com.unity.inputsystem 1.14.0（Active Input Handling = Both）· 内置渲染管线 · Gamma 色彩空间。

## 许可

- 示例代码：可在你自己的项目（包括商业项目）中自由使用和修改。
- 精灵素材：作者原创，可自由使用。
- 字体：Noto Sans SC，SIL Open Font License 1.1。
- 书籍正文与配图不得转售、转载或公开传播。

Unity、TextMesh Pro 为 Unity Technologies 的商标，本工程与 Unity Technologies 无关联。

问题反馈：411827633@163.com
