#!/bin/bash
echo "Cleaning temporary files..."
find . -name "tmpclaude-*" -type f -delete 2>/dev/null
find . -name "tmpclaude-*" -type d -exec rm -rf {} + 2>/dev/null
echo "Done."
