#!/usr/bin/env bash
# check-js.sh — تست سبک بدون dotnet (CAVEMAN). node همیشه تو sandbox هست.
# بزن:  bash tools/check-js.sh
# 1) سینتکس همه JSهای پروژه (به‌جز lib شخص‌ثالث)  2) تست‌های JS موجود.
set -e
cd "$(dirname "$0")/.."

echo "=== node --check روی JSهای پروژه ==="
ok=0; bad=0
for f in $(find src -name "*.js" | grep -v /lib/); do
  if node --check "$f" 2>/dev/null; then ok=$((ok+1)); else bad=$((bad+1)); echo "  SYNTAX ERR: $f"; fi
done
echo "JS سینتکس: ok=$ok bad=$bad"

echo "=== اجرای تست‌های JS داخل tests/ ==="
for t in tests/*.test.js; do [ -f "$t" ] && { echo "--- $t"; node "$t"; }; done
