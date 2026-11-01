#!/bin/bash
# 打包配套工程：先清空动态字体里渲染时塞进去的字形，再把 project/ 打成 zip（不含 Library/Temp/Logs 等可再生目录）
set -e
ROOT=/Users/zsl/Documents/work/myproject/books/ugui
ZIP="$ROOT/UGUI从入门到精通_配套工程.zip"
"$ROOT/tools/unity_run.sh" "$ROOT/project" UGUIBook.Core.BookSetup.ClearFontsAndExit
STAGE=$(mktemp -d)/UGUI从入门到精通_配套工程
mkdir -p "$STAGE"
cp "$ROOT/tools/README_project.md" "$STAGE/README.md"
rsync -a --exclude Library --exclude Temp --exclude Logs --exclude UserSettings --exclude obj --exclude .vs \
  --exclude '*.csproj' --exclude '*.sln' --exclude '.DS_Store' "$ROOT/project/" "$STAGE/project/"
rm -f "$ZIP"; (cd "$(dirname "$STAGE")" && zip -qr -X "$ZIP" "$(basename "$STAGE")")
echo "zip -> $ZIP ($(du -h "$ZIP" | cut -f1))"
