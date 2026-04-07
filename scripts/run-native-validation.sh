#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="Release"
PLATFORM="auto"
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"

print_usage() {
  cat <<'EOF'
Usage:
  ./scripts/run-native-validation.sh --platform <linux|macos|auto> [--configuration <Debug|Release>]

Examples:
  ./scripts/run-native-validation.sh --platform linux --configuration Release
  ./scripts/run-native-validation.sh --platform macos --configuration Release
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --platform)
      PLATFORM="$2"
      shift 2
      ;;
    --configuration)
      CONFIGURATION="$2"
      shift 2
      ;;
    -h|--help)
      print_usage
      exit 0
      ;;
    *)
      printf 'Unknown argument: %s\n' "$1" >&2
      print_usage >&2
      exit 2
      ;;
  esac
done

if [[ "$PLATFORM" == "auto" ]]; then
  case "$(uname -s)" in
    Linux)
      PLATFORM="linux"
      ;;
    Darwin)
      PLATFORM="macos"
      ;;
    *)
      printf 'Auto-detection only supports Linux or macOS hosts.\n' >&2
      exit 2
      ;;
  esac
fi

case "$PLATFORM" in
  linux)
    if [[ "$(uname -s)" != "Linux" ]]; then
      printf 'Linux native validation must run on a Linux host.\n' >&2
      exit 2
    fi
    if [[ -z "${DISPLAY:-}" ]]; then
      printf 'DISPLAY is not set. Start an X11 session or run this command under xvfb-run.\n' >&2
      exit 2
    fi
    bash "$ROOT_DIR/verify.sh" --configuration "$CONFIGURATION" --include-native-linux
    ;;
  macos)
    if [[ "$(uname -s)" != "Darwin" ]]; then
      printf 'macOS native validation must run on a macOS host.\n' >&2
      exit 2
    fi
    bash "$ROOT_DIR/verify.sh" --configuration "$CONFIGURATION" --include-native-macos
    ;;
  *)
    printf 'Unsupported platform value: %s\n' "$PLATFORM" >&2
    exit 2
    ;;
esac
