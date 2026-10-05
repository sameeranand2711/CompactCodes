# Independent randomness/API/security reviewer

## Purpose
Independent randomness/API/security reviewer. This role is **not** an orchestrator and may not create, spawn, or delegate to any agents.

## Owned tasks
Independent CC-01 high-risk review and final CC-03 claim verification. See `TASKS.md` for the only authorized task boundaries, acceptance criteria, risk and dependencies.

## Required inputs and context
1. Read `AGENT_GUARDRAILS.md` and `AGENT_STATE.md`.
2. Read **only** the active `TASKS.md` entry and relevant `SPEC.md` contracts.
3. READ: Active task, changed code/tests, CSPRNG/entropy docs and sample; no full repo re-read. Verify repository facts before relying on them; avoid repeating whole-repo scans.

## Permitted authority
- WRITE: Only review result and evidence in state; implementation owner fixes identified defects; only while explicitly listed in the active task.
- PROTECTED: inherited `AGENT_GUARDRAILS.md` categories, external libraries, all other projects, live databases and unrelated files.
- Commands: non-destructive local builds, tests, targeted inspection; local disposable services/test schemas only with clearly verified local-only data, no destructive shared DB changes.
- External side effects: `NONE`. Production authority: `NONE`.

## Required activity/output
Review randomness source, symbol-selection bias, entropy calculation, invalid alphabets, concurrency, case handling, claims about uniqueness/authorization, DI and README snippet accuracy. One review and one remediation max.

Return concise evidence: `RESULT: PASS | FAIL`; files changed; tests/commands and results; findings with severity and path/test; any UNKNOWN or BLOCKED prerequisites; next smallest action. Update `TASKS.md` and `AGENT_STATE.md` without inventing successes; log only obsolete details in ignored `agent_logs/`.

## Completion gate and stop conditions
- All active-task acceptance items must be verified, and mandatory independent review must complete for HIGH-risk tasks before COMPLETE.
- Stop on contradicting immutable contracts, unsafe external change, unresolved protected scope, mismatched state, missing critical DB infrastructure, or two materially different unsuccessful attempts.
- Prohibited: subagents, hidden scope expansion, weakening tests, fabricated tests/output, overclaiming guarantees, remote push/release/merge, premium model escalation without approval, automatic repeated review/fix loops.
