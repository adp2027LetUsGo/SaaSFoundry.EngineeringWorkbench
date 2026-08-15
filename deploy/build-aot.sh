#!/usr/bin/env bash
# =============================================================================
# VibeStock — NativeAOT Build Script
# Publishes all 4 Cell processes as self-contained linux-x64 NativeAOT binaries
# into the ./deploy/publish/ directory for Docker image construction.
# =============================================================================
# Usage:
#   ./deploy/build-aot.sh
#
# Prerequisites:
#   - .NET 10 SDK with Native AOT workload
#   - Linux native toolchain (clang, zlib-dev) for cross-compilation
#     OR run natively on Linux
# =============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
PUBLISH_DIR="${SCRIPT_DIR}/publish"
RUNTIME="linux-x64"
CONFIG="Release"

echo "=== VibeStock NativeAOT Build ==="
echo "Runtime: ${RUNTIME}"
echo "Config:  ${CONFIG}"
echo "Output:  ${PUBLISH_DIR}"
echo ""

cells=(
  "VibeStock.Ingestor.Cell"
  "VibeStock.System.Cell"
  "VibeStock.Bridge.Cell"
  "VibeStock.Core.Cell"
)

for cell in "${cells[@]}"; do
  echo "--- Publishing ${cell} ---"
  dotnet publish \
    "${REPO_ROOT}/src/${cell}/${cell}.csproj" \
    -c "${CONFIG}" \
    -r "${RUNTIME}" \
    /p:PublishAot=true \
    /p:SelfContained=true \
    --output "${PUBLISH_DIR}/${cell}"

  echo "    Output: ${PUBLISH_DIR}/${cell}"
  echo ""
done

echo "=== All Cells published successfully ==="
echo ""
echo "Next steps:"
echo "  cd deploy/"
echo "  docker compose build"
