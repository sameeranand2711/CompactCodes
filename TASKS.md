# TASKS.md

Tasks are sequential. `NOT_STARTED → IN_PROGRESS → COMPLETE`; `BLOCKED` is a checkpoint, not approval to bypass a gate. High-risk tasks require targeted independent review to be counted complete; reviewers may review a related high-risk diff as one bounded pass. No next dependent task starts without its gates unless an explicit human-approved exception is recorded here; a deferred review never counts as passed.

**Human-approved exception (2026-10-05):** Defer all independent reviews until CC-03/end-of-work review. CC-02 and subsequent implementation tasks may proceed while earlier review gates remain pending. Keep affected task statuses `IN_PROGRESS` until their required reviews pass.

**Independent review result (2026-10-05):** `FAIL`. No Critical/High security defect was found in the randomness, validation, DI, or documentation paths, but release evidence is incomplete: the required benchmark report is absent, and the required collision observation/report is not implemented in the test campaign. See `AGENT_STATE.md` for commands and evidence. One bounded implementation-owner remediation pass remains.

**Six Thinking Hats review (2026-10-05):** Completed on the review branch. The architecture and V1 API remain accepted; Blue-hat release decision is `NO-GO` until the two existing evidence gaps are remediated and targeted verification passes. No additional blocking finding was introduced.

## Shared task boundaries
- **Default external side effects:** `NONE` for every task.
- **Production authority:** `NONE` for every task.
- **PROTECTED:** inherit `AGENT_GUARDRAILS.md`, including live data, secrets, infra, unrelated libraries, and other active tasks.
- A branch-local commit after completed tasks is authorized; remote push/PR/publish/merge are not.
- File paths refer to conventional structure; inspect real repository locations first and adapt only within the same logical scope. Record deviations in `AGENT_STATE.md`.

## CC-01 — Small secure code generator and configuration contract

- **Status:** `IN_PROGRESS`
- **Owner:** `agent-01-builder-docs`
- **Dependencies:** None
- **Risk:** `HIGH`
- **External side effects:** `NONE`
- **Production authority:** `NONE`

**READ:** `SPEC.md`; existing CompactCodes source and tests; package references

**WRITE:** `src/CompactCodes/**`, `tests/**` limited to generation/validation/concurrency/statistics

**PROTECTED:** `AGENT_GUARDRAILS.md` default areas, all unrelated components and external systems.

**In scope:** Implement/finish Base62 default, custom alphabet validation, 8/10/64 length policy, >=40 bits nominal entropy, `DistributedRandom` with cryptographic randomness, unbiased symbol choice, exact case and sync string API. No database or redirect service.

**Out of scope:** unrelated refactoring, new domain features, production/shared infra, external writes and speculative new abstractions.

**Outputs:** Correct library code and targeted tests.

**Acceptance criteria:** No `System.Random`, no modulo bias, invalid alphabets/limits rejected; concurrent generation adheres to format; no uniqueness/authorization claim.

**Validation:** Build .NET 8/10; unit, high-parallel and robust statistical sanity tests (no brittle zero-collision assertions); independent cryptography review required.

**Definition of done:**
- [x] Implemented required observable behavior and outputs; acceptance satisfied.
- [x] READ/WRITE/PROTECTED and command boundaries respected.
- [x] No external/production side effects or unapproved dependencies.
- [x] Tests/validations pass without weakening tests or disabling checks.
- [ ] Correct risk-based independent review completed when required; no unaddressed blocker.
- [x] `AGENT_STATE.md` updated; scope changes recorded; local task checkpoint/commit when authorized.

## CC-02 — Minimal DI registration and console demo

- **Status:** `COMPLETE`
- **Owner:** `agent-01-builder-docs`
- **Dependencies:** CC-01
- **Risk:** `MEDIUM`
- **External side effects:** `NONE`
- **Production authority:** `NONE`

**READ:** Implemented real public API, DI registration conventions, `SPEC.md`

**WRITE:** `src/CompactCodes/**` limited to DI extensions, `samples/CompactCodes.Sample/**`, and DI tests

**PROTECTED:** `AGENT_GUARDRAILS.md` default areas, all unrelated components and external systems.

**In scope:** Use `IServiceCollection` with minimal new abstractions. Wire thread-safe singleton generator and options. Create one tiny console app using DI to generate default and custom-alphabet codes; explain persistence UNIQUE constraint without implementing persistence.

**Out of scope:** unrelated refactoring, new domain features, production/shared infra, external writes and speculative new abstractions.

**Outputs:** Compiling/runnable DI console sample and small DI tests.

**Acceptance criteria:** DI lifetime/options validated; console output visibly respects alphabet/length; sample has no database, broker, network or framework bloat.

**Validation:** Compile and run sample, exercise valid/invalid options and repeated DI registration.

**Definition of done:**
- [x] Implemented required observable behavior and outputs; acceptance satisfied.
- [x] READ/WRITE/PROTECTED and command boundaries respected.
- [x] No external/production side effects or unapproved dependencies.
- [x] Tests/validations pass without weakening tests or disabling checks.
- [x] Correct risk-based independent review completed when required; no unaddressed blocker.
- [x] `AGENT_STATE.md` updated; scope changes recorded; local task checkpoint/commit when authorized.

## CC-03 — Final README, local packaging and independent security review

- **Status:** `IN_PROGRESS`
- **Owner:** `agent-01-builder-docs + agent-02-independent-review`
- **Dependencies:** CC-02
- **Risk:** `HIGH`
- **External side effects:** `NONE`
- **Production authority:** `NONE`

**READ:** Verified public APIs, tests and compiled sample, existing README/metadata

**WRITE:** Repository `README.md`, relevant docs/release metadata, targeted changes routed to implementation owner if review fails

**PROTECTED:** `AGENT_GUARDRAILS.md` default areas, all unrelated components and external systems.

**In scope:** Update well-structured README only after demo is verified; show real installation, DI, custom alphabet, config constraints, entropy/collision math, non-authorization guarantee and exact run command. Independently review RNG, bias, API and README claims once; one remediation pass only.

**Out of scope:** unrelated refactoring, new domain features, production/shared infra, external writes and speculative new abstractions.

**Outputs:** Accurate README and review PASS/FAIL report; ready for human release review.

**Acceptance criteria:** All README snippets work; CSPRNG/entropy/URL-safe case rules verified; no claims of global uniqueness; security reviewer passes with no Critical/High findings; no remote publish/merge.

**Validation:** Final builds/tests/statistical sanity, sample run, local package inspection and one targeted independent review; at most one remediation and verification.

**Definition of done:**
- [x] Implemented required observable behavior and outputs; acceptance satisfied.
- [x] READ/WRITE/PROTECTED and command boundaries respected.
- [x] No external/production side effects or unapproved dependencies.
- [x] Tests/validations pass without weakening tests or disabling checks.
- [ ] Correct risk-based independent review completed when required; no unaddressed blocker.
- [x] `AGENT_STATE.md` updated; scope changes recorded; local task checkpoint/commit when authorized.
