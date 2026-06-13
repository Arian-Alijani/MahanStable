#!/usr/bin/env bash
# ============================================================================
# setup-dotnet.sh  —  نصب .NET 8 SDK در sandbox + ست‌کردن env  (CAVEMAN)
# ----------------------------------------------------------------------------
# چرا: sandbox تازه = dotnet نصب نیست. این اسکریپت در ~17s نصبش می‌کنه.
# کِی بزن: اول هر سشن که میخوای build/test/publish بگیری.
# چطور بزن (مهم! باید source بشه تا PATH تو شل فعلی بمونه):
#       source tools/setup-dotnet.sh
#   یا برای یک‌بار اجرا داخل ساب‌شل:  . tools/setup-dotnet.sh && dotnet build
#
# idempotent: اگه قبلا نصب شده، فقط env رو ست میکنه (نصب دوباره نه).
#
# تلهٔ مرگبار که حلش کردیم:  /tmp فقط 493MB و tmpfس هست → extract آرشیو
#   216MB اونجا میترکه با «No space left on device». پس TMPDIR رو میبریم home.
# ============================================================================
set -e

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export NUGET_PACKAGES="$HOME/.nuget/packages"
export TMPDIR="$HOME/tmp"            # ← کلید همه‌چی. بدون این نصب میترکه.
mkdir -p "$TMPDIR"

if [ -x "$DOTNET_ROOT/dotnet" ] && "$DOTNET_ROOT/dotnet" --version >/dev/null 2>&1; then
  echo "[setup-dotnet] قبلا نصبه: $($DOTNET_ROOT/dotnet --version) — فقط env ست شد. آماده‌ای."
  return 0 2>/dev/null || exit 0
fi

echo "[setup-dotnet] dotnet نیست → نصب .NET 8 SDK (یه ۱۷ ثانیه طول میکشه)..."
cd "$HOME"
[ -f dotnet-install.sh ] || curl -sSL -o dotnet-install.sh https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
# channel 8.0 = آخرین SDK خط 8 (الان 8.0.422). net8.0 قفلِ پروژه‌ست، بالاتر نرو.
TMPDIR="$HOME/tmp" ./dotnet-install.sh --channel 8.0 --install-dir "$DOTNET_ROOT"

echo "[setup-dotnet] تمام شد: $($DOTNET_ROOT/dotnet --version)"
echo "[setup-dotnet] حالا بزن:  dotnet restore MahanShop.sln  &&  dotnet build MahanShop.sln --no-restore"
