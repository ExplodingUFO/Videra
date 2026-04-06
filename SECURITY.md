# Security Policy

## Supported Versions

Videra is still evolving quickly. Security fixes are applied to the latest code on `master`.

If you are evaluating or deploying Videra, use the latest published commit or release tag rather than older snapshots.

## Reporting a Vulnerability

Do not open a public issue for suspected security vulnerabilities.

Preferred channel: GitHub private vulnerability reporting for this repository.

If private vulnerability reporting is not available, contact the maintainer through a non-public GitHub contact channel rather than a public issue or pull request.

Include:

- A clear description of the issue
- Affected components and platforms
- Reproduction steps or proof of concept
- Any suggested mitigations

We aim to:

- Acknowledge receipt within 72 hours
- Provide an initial triage update within 5 business days
- Share a remediation plan or next-step status within 30 calendar days when the report is actionable

We will review reports, confirm impact, and coordinate a fix before public disclosure where appropriate. Please do not discuss suspected vulnerabilities in public issues, PRs, or discussions until the maintainers confirm disclosure timing.

## Scope

Security reports are especially useful for:

- Native interop and platform host boundaries
- Dependency or package supply-chain concerns
- Unsafe file handling or model import parsing bugs
- Privilege, sandbox, or code execution issues

General bugs, rendering defects, and feature requests should go through the normal issue tracker.
