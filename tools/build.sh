#!/usr/bin/env bash
# build.sh — restore + build کل solution (CAVEMAN). خودش dotnet رو ست میکنه.
# بزن:  bash tools/build.sh          (build فقط)
#       bash tools/build.sh publish  (build + publish به ~/publish_test)
set -e
cd "$(dirname "$0")/.."
source tools/setup-dotnet.sh

echo "=== restore (~16s بار اول، بعدش cache) ==="
dotnet restore MahanShop.sln

echo "=== build (~24s) ==="
dotnet build MahanShop.sln --no-restore -c Debug

if [ "$1" = "publish" ]; then
  echo "=== publish Release → ~/publish_test (~18s) ==="
  rm -rf "$HOME/publish_test"
  dotnet publish src/MahanShop.Web/MahanShop.Web.csproj -c Release -o "$HOME/publish_test" --no-restore
  echo "خروجی publish آماده در: $HOME/publish_test  (web.config برای IIS هم هست)"
fi
echo "=== DONE. اگه 0 Error دیدی، سبزه. ==="
