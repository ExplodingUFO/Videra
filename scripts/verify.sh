#!/bin/bash
set -euo pipefail

CONFIGURATION="Release"
INCLUDE_NATIVE_LINUX=false
INCLUDE_NATIVE_LINUX_XWAYLAND=false
INCLUDE_NATIVE_MACOS=false
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
ALL_PASS=true
START_TIME=$(date +%s)
TEST_VERBOSITY="q"
TEST_LOGGER_ARGS=()
DOTNET_CMD="${DOTNET_CMD:-}"
DOTNET_ROOT_DIR="$ROOT_DIR"

if [[ -z "$DOTNET_CMD" ]]; then
  if command -v dotnet >/dev/null 2>&1; then
    DOTNET_CMD="dotnet"
  elif command -v dotnet.exe >/dev/null 2>&1; then
    DOTNET_CMD="dotnet.exe"
  else
    printf 'dotnet CLI is not available in PATH.\n' >&2
    exit 2
  fi
fi

if [[ "$DOTNET_CMD" == *.exe ]]; then
  if command -v cygpath >/dev/null 2>&1; then
    DOTNET_ROOT_DIR="$(cygpath -w "$ROOT_DIR")"
  elif command -v wslpath >/dev/null 2>&1; then
    DOTNET_ROOT_DIR="$(wslpath -w "$ROOT_DIR")"
  fi
fi

if [[ "${VIDERA_VERBOSE_TEST_LOGS:-false}" == "true" ]]; then
  TEST_VERBOSITY="m"
  TEST_LOGGER_ARGS=(--logger "console;verbosity=detailed")
fi

print_step() {
  printf '\n=== %s ===\n' "$1"
}

run_check() {
  local title="$1"
  local success_message="$2"
  local failure_message="$3"
  shift 3

  print_step "$title"
  if "$@"; then
    printf '  [PASS] %s\n' "$success_message"
  else
    printf '  [FAIL] %s\n' "$failure_message"
    ALL_PASS=false
  fi
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --configuration)
      CONFIGURATION="$2"
      shift 2
      ;;
    --include-native-linux)
      INCLUDE_NATIVE_LINUX=true
      shift
      ;;
    --include-native-linux-xwayland)
      INCLUDE_NATIVE_LINUX_XWAYLAND=true
      shift
      ;;
    --include-native-macos)
      INCLUDE_NATIVE_MACOS=true
      shift
      ;;
    *)
      printf 'Unknown argument: %s\n' "$1" >&2
      exit 2
      ;;
  esac
done

run_check \
  "Build ($CONFIGURATION)" \
  "Build succeeded" \
  "Build failed" \
  "$DOTNET_CMD" build "$DOTNET_ROOT_DIR/Videra.slnx" --configuration "$CONFIGURATION" -v q

run_check \
  "Tests" \
  "All tests passed" \
  "Some tests failed" \
  "$DOTNET_CMD" test "$DOTNET_ROOT_DIR/Videra.slnx" --configuration "$CONFIGURATION" -v "$TEST_VERBOSITY" "${TEST_LOGGER_ARGS[@]}"

run_check \
  "Demo Build" \
  "Demo builds" \
  "Demo build failed" \
  "$DOTNET_CMD" build "$DOTNET_ROOT_DIR/samples/Videra.Demo/Videra.Demo.csproj" --configuration "$CONFIGURATION" -v q

run_check \
  "Surface Charts Demo Build" \
  "Surface charts demo builds" \
  "Surface charts demo build failed" \
  "$DOTNET_CMD" build "$DOTNET_ROOT_DIR/samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj" --configuration "$CONFIGURATION" -v q

if [[ "$INCLUDE_NATIVE_LINUX" == true ]]; then
  run_check \
    "Linux X11 Native Validation" \
    "Linux X11 native validation passed" \
    "Linux X11 native validation failed" \
    env VIDERA_RUN_LINUX_NATIVE_TESTS=true VIDERA_EXPECT_LINUX_DISPLAY_SERVER=X11 "$DOTNET_CMD" test "$DOTNET_ROOT_DIR/tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj" --configuration "$CONFIGURATION" -v "$TEST_VERBOSITY" "${TEST_LOGGER_ARGS[@]}"
fi

if [[ "$INCLUDE_NATIVE_LINUX_XWAYLAND" == true ]]; then
  run_check \
    "Linux XWayland Native Validation" \
    "Linux XWayland native validation passed" \
    "Linux XWayland native validation failed" \
    env VIDERA_RUN_LINUX_NATIVE_TESTS=true VIDERA_EXPECT_LINUX_DISPLAY_SERVER=XWayland "$DOTNET_CMD" test "$DOTNET_ROOT_DIR/tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj" --configuration "$CONFIGURATION" -v "$TEST_VERBOSITY" "${TEST_LOGGER_ARGS[@]}"
fi

if [[ "$INCLUDE_NATIVE_MACOS" == true ]]; then
  run_check \
    "macOS Native Validation" \
    "macOS native validation passed" \
    "macOS native validation failed" \
    "$DOTNET_CMD" test "$DOTNET_ROOT_DIR/tests/Videra.Platform.macOS.Tests/Videra.Platform.macOS.Tests.csproj" --configuration "$CONFIGURATION" -v "$TEST_VERBOSITY" "${TEST_LOGGER_ARGS[@]}"
fi

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))
MINUTES=$((DURATION / 60))
SECONDS=$((DURATION % 60))

printf '\n=== Summary ===\n'
printf '  Duration: %02d:%02d\n' "$MINUTES" "$SECONDS"

if [[ "$ALL_PASS" == true ]]; then
  printf '  All checks passed!\n'
  exit 0
else
  printf '  Some checks failed.\n'
  exit 1
fi
