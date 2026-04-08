#!/bin/bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "Cleaning temporary files..."
find "$repo_root" -name "tmpclaude-*" -type f -delete 2>/dev/null
find "$repo_root" -name "tmpclaude-*" -type d -prune -exec rm -rf {} + 2>/dev/null
echo "Done."
