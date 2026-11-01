#!/bin/bash
# 用法: unity_check.sh <工程目录> <章节,逗号分隔> [额外参数，如 -noshots / -noscenes / -shot ch05-xxx]
# 编译检查 C#/Shader，生成本章场景，渲染配图到 books/ugui/src/img/，输出错误摘要。全机最多 3 个 Unity 同时跑（与 URP 书共用锁）。
PROJ="$(cd "$1" && pwd)"; CH="$2"; shift 2
U=/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity
ROOT=/Users/zsl/Documents/work/myproject/books/ugui
OUT=$ROOT/src/img; mkdir -p $OUT /tmp/urpbook_locks
LOG="$PROJ/Logs/check_$(echo $CH | tr ',' '_').log"
slot=""
while [ -z "$slot" ]; do
  for i in 1 2 3; do if mkdir /tmp/urpbook_locks/slot$i 2>/dev/null; then slot=$i; break; fi; done
  [ -z "$slot" ] && sleep 5
done
trap "rmdir /tmp/urpbook_locks/slot$slot" EXIT
"$U" -batchmode -projectPath "$PROJ" -executeMethod UGUIBook.Core.ShotRunner.Run -chapters "$CH" -outdir "$OUT" "$@" -logFile "$LOG" >/dev/null 2>&1
code=$?
echo "== unity exit $code  (log: $LOG)"
grep -E "error CS[0-9]+" "$LOG" | sort -u | head -40
echo "shots rendered: $(grep -c '\[UGUIBook\] shot' "$LOG")   scenes saved: $(grep -c '\[UGUIBook\] scene' "$LOG")"
grep -E "UGUIBOOK-ERROR|UGUIBOOK-SUMMARY|Checked [0-9]+ shaders|Aborting batchmode|Scripts have compiler errors" "$LOG" | sort -u | head -80
