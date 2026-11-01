# UGUI 从入门到精通

《UGUI 从入门到精通：Unity 界面开发实战全解》书稿仓库（麦冬）。在线免费阅读：https://gametech.byte888.com/ugui/

- `src/`：正文 HTML（c01–c21、附录），书里的代码用 `data-file` / `data-region` 从工程里拉取。
- `project/`：配套 Unity 工程（Unity 2022.3.62f3，ugui 1.0.0，TMP 3.0.7，Built-in RP）。`Assets/UGUIBook/ChNN` 是各章代码和场景，`_Core` 是共用工具（截图、启动器、冒烟测试）。
- `site/`：网站样式和脚本；`promo/`：宣传图。
- `tools/`：生成和检查脚本，写作规范见 `tools/AUTHOR_GUIDE.md`。

## 生成

```bash
.venv/bin/python tools/build.py          # PDF + 网站 web/
.venv/bin/python tools/build.py --web    # 只生成网站
bash tools/make_zip.sh                   # 配套工程 zip
bash tools/unity_run.sh project UGUIBook.Core.DemoSmokeTest.Run -ugbSmoke 1   # Play 模式打开全部场景
node tools/sitecheck.mjs <输出目录>       # 网站截图检查（tools/node_modules 链到 ../../godot/tools/node_modules）
```

部署：在 myproject 下运行 `bash gametech/deploy.sh …`（见 gametech/GAMETECH.md）。

© 2026 麦冬
