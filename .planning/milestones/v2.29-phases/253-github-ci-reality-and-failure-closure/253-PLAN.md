# Phase 253: GitHub CI Reality and Failure Closure - Plan

## Goal

Observe required GitHub CI checks for PR #90 and close failures with the smallest justified code or CI-contract changes.

## Tasks

1. Poll PR #90 check rollup until checks complete or failures appear.
2. Classify each failed check as product-code issue, test/contract issue, environment issue, or flaky/noisy threshold.
3. For actionable failures, inspect logs and apply minimal scoped fixes.
4. Push fixes to the PR branch and re-observe checks.
5. Record final CI status and any residual risk.

## Success Criteria

1. Required PR checks are listed with conclusion, URL, and failure reason where applicable.
2. CI failures are classified.
3. Fixes stay scoped to the failure and preserve the v2.28 performance-hardening intent.
4. The PR reaches a mergeable state or a concrete blocker is documented.
