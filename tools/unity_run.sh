#!/bin/bash
# 用法: unity_run.sh <工程目录> <静态方法全名> [额外参数]   —— 带全机锁跑一个 -executeMethod，打印日志里的错误
PROJ="$(cd "$1" && pwd)"; M="$2"; shift 2
U=/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity
LOG="$PROJ/Logs/run_${M##*.}.log"; mkdir -p /tmp/urpbook_locks
slot=""
while [ -z "$slot" ]; do
  for i in 1 2 3; do if mkdir /tmp/urpbook_locks/slot$i 2>/dev/null; then slot=$i; break; fi; done
  [ -z "$slot" ] && sleep 5
done
trap "rmdir /tmp/urpbook_locks/slot$slot" EXIT
"$U" -batchmode -projectPath "$PROJ" -executeMethod "$M" "$@" -logFile "$LOG" >/dev/null 2>&1
echo "== unity exit $?  (log: $LOG)"
grep -E "error CS[0-9]+|Exception|\[UGUIBook\]|Aborting batchmode" "$LOG" | sort -u | head -40
