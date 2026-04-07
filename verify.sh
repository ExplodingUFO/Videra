#!/bin/bash
set -euo pipefail

CONFIGURATION="Release"
INCLUDE_NATIVE_LINUX=false
INCLUDE_NATIVE_MACOS=false
ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
ALL_PASS=true
START_TIME=$(date +%s)
TEST_VERBOSITY="q"
TEST_LOGGER_ARGS=()

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
  dotnet build "$ROOT_DIR/Videra.slnx" --configuration "$CONFIGURATION" -v q

run_check \
  "Tests" \
  "All tests passed" \
  "Some tests failed" \
  dotnet test "$ROOT_DIR/Videra.slnx" --configuration "$CONFIGURATION" -v "$TEST_VERBOSITY" "${TEST_LOGGER_ARGS[@]}"

run_check \
  "Demo Build" \
  "Demo builds" \
  "Demo build failed" \
  dotnet build "$ROOT_DIR/samples/Videra.Demo/Videra.Demo.csproj" --configuration "$CONFIGURATION" -v q

if [[ "$INCLUDE_NATIVE_LINUX" == true ]]; then
  run_check \
    "Linux Native Validation" \
    "Linux native validation passed" \
    "Linux native validation failed" \
    env VIDERA_RUN_LINUX_NATIVE_TESTS=true dotnet test "$ROOT_DIR/tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj" --configuration "$CONFIGURATION" -v "$TEST_VERBOSITY" "${TEST_LOGGER_ARGS[@]}"
fi

if [[ "$INCLUDE_NATIVE_MACOS" == true ]]; then
  run_check \
    "macOS Native Validation" \
    "macOS native validation passed" \
    "macOS native validation failed" \
    dotnet test "$ROOT_DIR/tests/Videra.Platform.macOS.Tests/Videra.Platform.macOS.Tests.csproj" --configuration "$CONFIGURATION" -v "$TEST_VERBOSITY" "${TEST_LOGGER_ARGS[@]}"
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
