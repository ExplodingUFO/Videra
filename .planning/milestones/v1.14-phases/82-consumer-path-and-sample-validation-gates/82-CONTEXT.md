# Phase 82: Consumer Path and Sample Validation Gates - Context

**Gathered:** 2026-04-20  
**Status:** Executed and verified  
**Mode:** Autonomous

## Phase Boundary

Phase 82 promotes the packaged consumer path and the advanced public samples from “documented references” into routine pull-request evidence. The goal is to make the public install path and the higher-level sample contracts fail fast in CI when they drift.

## Implementation Decisions

### Merge-time evidence
- **D-01:** Run packaged consumer smoke routinely on pull requests and `master`, not only by manual trigger or release flow.
- **D-02:** Add a dedicated sample-contract evidence job for the advanced public samples instead of burying those checks inside broader repository verification.

### Public-surface coverage
- **D-03:** Treat `Videra.ExtensibilitySample` and `Videra.InteractionSample` as canonical public references whose configuration/runtime contracts need explicit CI coverage.
- **D-04:** Keep support artifacts aligned with sample/consumer evidence so the same user-facing story is being tested and documented.

### Scope control
- **D-05:** Do not widen the sample surface beyond the already documented public references; verify the truth already being promised.
- **D-06:** Keep the packaged consumer path package-based, not project-reference-based, so CI continues to exercise the real consumer installation model.

## Specific Ideas

- Expand `consumer-smoke.yml` triggers to PR and push while keeping manual target selection.
- Add a `sample-contract-evidence` job to `ci.yml` that isolates advanced sample truth from the general verification noise floor.
- Update README and release docs so “green” explicitly includes packaged consumer smoke and sample-contract evidence.

## Canonical References

### Milestone and requirements
- `.planning/ROADMAP.md` — Phase 82 goal and success criteria.
- `.planning/REQUIREMENTS.md` — `CONS-01`, `CONS-02`, and `CONS-03`.

### CI and consumer/sample surfaces
- `.github/workflows/consumer-smoke.yml`
- `.github/workflows/ci.yml`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `smoke/Videra.ConsumerSmoke`
- `samples/Videra.ExtensibilitySample`
- `samples/Videra.InteractionSample`
- `tests/Videra.Core.Tests/Repository/AlphaConsumerIntegrationTests.cs`

## Existing Code Insights

### Reusable assets
- Consumer smoke already packed local packages and restored against a local feed, so it was ready to become PR evidence with workflow trigger changes.
- Integration and repository tests for the advanced samples already existed; they needed to be promoted into an explicit CI job.

### Risks carried into the phase
- The repo could still merge regressions in the packaged consumer path or advanced sample contracts because those checks were not clearly present as routine PR evidence.
- Docs claimed public reference status for the advanced samples without a dedicated CI surface proving that story every time.

## Deferred Ideas

- New public samples beyond the current minimal/extensibility/interaction set.
- Any expansion from consumer validation into a broader package discovery/install UX story.

---

*Phase: 82-consumer-path-and-sample-validation-gates*  
*Context gathered: 2026-04-20*
