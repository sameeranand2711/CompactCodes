# SPEC.md — CompactCodes V1

## Source precedence
This specification uses the Stage-0 V1 architecture below. The **October 2026 refinement** preceding it is the latest explicit user request, and therefore governs DI, single sample app and README if it differs from earlier wording. Existing code must be inspected; do not invent repo facts. Do not expand other libraries.


## October 2026 refinement — minimal DI + sample + consumer README

- **DI required:** expose a simple `IServiceCollection` registration extension in the main library, such as `AddCompactCodes(...)` if it does not clash with an existing API. Register the generator as a singleton when thread-safe; validate options before use and keep Microsoft DI dependencies minimal. No mandatory new DI package unless genuinely required by repo boundaries. Test registration, custom options, singleton behavior, and invalid config.
- **One small sample:** `samples/CompactCodes.Sample`, a tiny console app using `ServiceCollection` to register and resolve the generator, print default and custom-alphabet codes, and explain that the caller must use a DB UNIQUE constraint + retry for uniqueness. No database is required to run the demo; this is an explanation, not persistence implementation.
- **README:** after DI and sample actually compile and run, update repository `README.md` with install, DI quick start, custom configuration, output safety/entropy, URL/case rules, collision and authorization warnings, and exact `dotnet run` steps. Use the mandatory structure below.
- Do not introduce async, `Span<T>` overloads, redirect analytics, persistence, network access, or complex builders.


## Final consumer README (mandatory, generated during implementation)

The repository `README.md` is a final **project deliverable**, not an agent-pack instruction file. Do not overwrite a useful pre-existing README until its structure is understood; update it in place. Do not prewrite hypothetical API examples.

After the actual DI API and console sample compile and run, update `README.md` with this navigable structure:

1. **Overview** — what the library does and does not do.
2. **Install** — correct NuGet/project references, frameworks and package selection.
3. **Quick start** — a minimal, verified DI registration snippet followed by the actual usage snippet.
4. **Core concepts** — short, consumer-focused terms and guarantees.
5. **Configuration** — real options, defaults, limits and safe values only.
6. **Usage** — one or two realistic tasks tied to runnable sample files.
7. **Safety and limitations** — duplicate/collision/security/recovery caveats next to relevant examples.
8. **Sample app** — exact path, prerequisites, `dotnet run` command and expected output.
9. **Testing & compatibility** — .NET 8/10, how to run checks; no invented benchmark claims.
10. **Versioning/license** — only verified metadata, no invented license.

README acceptance: every referenced public API, command, path, package ID, and option must be verified against implementation and/or compiled sample. Neither imaginary snippets nor code copied from old designs qualify. All examples use dummy/demo data, never secrets. This README must be complete **last**, after DI/sample are validated. The release readiness gate fails when documentation is stale or speculative.


---

## Established Stage-0 V1 architecture (retained source)

# CompactCodes V1 — Architecture Specification

## 1. Purpose

`CompactCodes` generates compact, URL-path-safe random codes for use cases such as:

- referral codes;
- campaign links;
- short-link keys;
- invitation codes;
- public opaque identifiers where unpredictability is useful.

It is **not** a URL-shortening service.

V1 targets **.NET 8 and .NET 10** and remains small, synchronous and dependency-light.

---

## 2. Core scope decision

V1 owns only generation and validation.

It does **not** own:

- persistence;
- redirect resolution;
- database uniqueness checks;
- analytics;
- expiration of links;
- HTTP endpoints;
- retrying database insert collisions;
- authorization.

This boundary keeps the package reusable.

---

## 3. Generation strategy

Default strategy name remains:

```text
DistributedRandom
```

Implementation basis:

```text
System.Security.Cryptography.RandomNumberGenerator
```

Use cryptographically strong random selection.

Do not use:

- `System.Random`;
- timestamps;
- process IDs;
- counters;
- machine IDs;
- reversible sequential IDs as the default;
- predictable hashing of database IDs.

---

## 4. Alphabet

### Default

Base62:

```text
0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
```

### Custom alphabet

Allowed.

Validation:

- non-null;
- unique characters only;
- case preserved;
- subset of RFC 3986 unreserved ASCII characters:

```text
A-Z a-z 0-9 - . _ ~
```

V1 does not accept arbitrary Unicode alphabets for URL-path-safe generation.

Why:

- avoids normalization ambiguity;
- avoids percent-encoding surprises;
- preserves exact case semantics;
- simplifies consumers/proxies.

### Alphabet cardinality

Do not validate only character count.

The configured length and alphabet must also meet a minimum entropy-space rule.

---

## 5. Length policy

### Decision

V1 constants:

```text
absolute minimum length: 8
default preferred minimum: 10
absolute maximum length: 64
```

The public configuration describes a **minimum/preferred generation length**, not a universal uniqueness guarantee.

`DistributedRandom` V1 normally generates at the configured preferred length. The API/model leaves room for future strategies to grow length up to the configured maximum when a future coordinated strategy requires it.

### Why 8 is the hard minimum

Base62 at 8 characters provides about:

```text
47.6 bits of code-space entropy
```

That is already unsuitable as a global uniqueness guarantee at very large populations, so shorter defaults would encourage unsafe use.

### Additional entropy validation

A custom alphabet/length pair must provide at least:

```text
40 bits of nominal code-space entropy
```

The default configuration provides about:

```text
59.5 bits (Base62, length 10)
```

Applications expecting extremely large populations should select 12+ characters and still enforce persistence uniqueness where required.

---

## 6. Collision reality

For uniformly random Base62 values, approximate probability of at least one collision:

| Number generated | 8 chars | 10 chars | 12 chars |
|---:|---:|---:|---:|
| 1 million | ~0.229% | ~0.0000596% | ~0.0000000155% |
| 100 million | ~100% | ~0.594% | ~0.000155% |
| 1 billion | ~100% | ~44.9% | ~0.0155% |

These probabilities concern the **population**, not the chance that one particular call collides.

Therefore:

> CompactCodes does not guarantee global uniqueness.

If uniqueness matters:

```text
generate
  -> insert under a database unique constraint
  -> on collision generate again
```

That retry loop belongs to the consuming application/repository, not the generator.

---

## 7. Security meaning

Unpredictability is useful, but code possession is not automatically authorization.

The library documentation must state:

- generated values are not authentication tokens by default;
- generated values must not grant sensitive access merely because they are difficult to guess;
- authorization must be enforced by the consuming system;
- applications with capability-token requirements need a dedicated threat model and entropy policy.

---

## 8. API shape

V1 remains synchronous and string-returning.

Conceptual public surface:

```text
ICompactCodeGenerator
    string Generate()

CompactCodeOptions
    Alphabet
    MinimumLength
    MaximumLength
```

A custom strategy abstraction may exist only if multiple real strategies are required in V1.

Avoid abstraction for abstraction's sake.

### Deferred

As previously decided, defer:

- `Span<char>` / `TryGenerate`;
- async generation;
- persistence-aware generation.

Those can be added later without breaking the simple V1 API.

---

## 9. Maximum-length behavior

Default:

```text
MinimumLength = 10
MaximumLength = 16
```

Hard constraints:

```text
8 <= MinimumLength <= MaximumLength <= 64
```

`MaximumLength` exists as a safety/future-growth bound.

The V1 `DistributedRandom` strategy need not randomly vary output length merely because a maximum exists. It generates the preferred minimum unless a future strategy explicitly requires growth.

This avoids surprising inconsistent URL lengths.

---

## 10. Character-selection bias

The implementation must use an unbiased selection mechanism.

Prefer the framework cryptographic APIs that select from a provided choice set correctly.

Do not implement naïve:

```text
randomByte % alphabetLength
```

unless rejection sampling is used to eliminate modulo bias.

Because .NET 8/10 provides suitable `RandomNumberGenerator` APIs, custom low-level random mapping is unnecessary unless benchmarks prove a real need.

---

## 11. Case sensitivity

Case is exact.

```text
AbC123 != abc123
```

The consuming application/database must therefore use a case-sensitive comparison/collation for stored codes if the configured alphabet contains both upper and lower case.

Documentation must highlight this because a case-insensitive DB index can collapse distinct generated values.

---

## 12. Validation

Configuration validation happens at creation/startup.

Reject:

- empty alphabet;
- duplicate characters;
- reserved URL characters;
- control/Unicode characters;
- minimum < 8;
- maximum > 64;
- minimum > maximum;
- alphabet/length combination below entropy floor.

Fail fast rather than producing weak codes.

---

## 13. Thread safety

The default generator must be safe for concurrent calls from a singleton DI registration.

No shared mutable PRNG state.

Concurrency test:

- many threads/tasks;
- millions of generated values where practical;
- verify every output satisfies alphabet and length invariants;
- collision observations recorded but not treated as a generator correctness failure unless statistically anomalous.

---

## 14. Statistical tests

V1 statistical tests are regression/sanity checks, not claims of cryptographic certification.

Check:

- per-character frequency;
- position frequency;
- no impossible characters;
- no systematic positional bias;
- repeat/collision counts against broad expected bounds.

Avoid brittle tests that randomly fail because a perfectly valid random sample is imperfect.

Use generous statistical thresholds and large deterministic test campaigns where an injectable test random source is appropriate.

Production default remains cryptographic RNG.

---

## 15. Benchmarks

Measure:

- codes/second;
- allocations/code;
- length 8/10/12/16/32;
- Base62;
- smaller custom alphabet;
- parallel generation.

Do not sacrifice randomness correctness to win microbenchmarks.

---

## 16. Five-Hat architecture review

### White

Random identifiers have a finite space and collisions follow birthday mathematics. RFC 3986 defines a safe unreserved URI character set. .NET provides cryptographic random APIs.

### Red

Developers want `Generate()` to be simple. Requiring callers to understand modulo bias or entropy math defeats the purpose of the library.

### Black

The main risks are weak randomness, false uniqueness claims, case-insensitive storage and custom alphabets that collapse the search space.

### Yellow

A tiny correct package is reusable across affiliate/referral/campaign and application utilities without infrastructure coupling.

### Green

Instead of adding a database to "guarantee uniqueness," keep generation pure and require the application's existing unique constraint to arbitrate collisions.

---

## 17. Reversibility register

| Decision | Door | Notes |
|---|---|---|
| Package name `CompactCodes` | One-way after release | Check package ID before publish |
| Pure generator scope | One-way architectural boundary | Prevents service/library bloat |
| CSPRNG default | One-way safety contract | Should not be weakened |
| Base62 default | Two-way configurable | Existing consumers keep their chosen alphabet |
| Hard min 8 / hard max 64 | One-way-ish validation contract | Can relax carefully in major version |
| Preferred default 10 | Two-way default | Existing explicit config unaffected |
| Sync string API | Two-way evolution | New APIs can be additive |
| No uniqueness guarantee | One-way semantic honesty | Persistence strategies can be added separately later |

---

## 18. Project layout

Keep this project small.

```text
src/
  CompactCodes/
    Abstractions/
    Generation/
    Options/
    Validation/
    Exceptions/
    Internal/
    Registration/
    Extensions/

tests/
  CompactCodes.Tests/
  CompactCodes.StatisticalTests/
  CompactCodes.ConcurrencyTests/
  CompactCodes.Benchmarks/

samples/
  CompactCodes.Sample/
```

Avoid empty architectural folders.

---

## 19. Required test campaign

### Functional

- default code valid;
- custom alphabet;
- upper/lower distinction;
- min/max boundaries;
- invalid configuration;
- maximum enforcement.

### Security

- verify no non-CSPRNG production implementation;
- invalid/reserved characters rejected;
- tiny alphabet/entropy rejected;
- no secret/random seed exposure.

### Concurrency

- singleton generator under high parallel load;
- invariant checks under repeated calls.

### Statistical

- broad uniformity sanity tests;
- per-position distribution;
- collision observation/report.

### Integration-style sample

Demonstrate the correct persistence pattern:

```text
generate code
try INSERT with UNIQUE index
if unique violation:
    regenerate
```

The sample should make it explicit that persistence uniqueness is the application's responsibility.

---

## 20. Release gate

CompactCodes V1 may release only when:

- all alphabet/length/entropy validation tests pass;
- cryptographic RNG implementation is confirmed;
- distribution sanity tests pass;
- parallel tests pass;
- benchmark report exists;
- case-sensitivity documentation is explicit;
- uniqueness limitation is prominent;
- security-capability warning is documented;
- project-specific randomness/API reviewer returns PASS;
- no unresolved Critical/High findings remain.
