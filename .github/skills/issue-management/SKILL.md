---
name: issue-management
description: Create, refine, triage, and verify GitHub Issues for FileManager. Use when asked to open, organize, split, prioritize, or close development tasks.
---
# FileManager Issue Management

## Before creating
1. Read `Docs/PRD.md`, `Docs/ARCHITECTURE.md`, and relevant code/tests.
2. Search existing open AND closed issues and PRs for duplicates. Prefer updating/linking an existing issue.
3. Verify whether the capability is already implemented. Do not infer status from an old issue or the roadmap alone.
4. Decide whether an issue is necessary; trivial follow-ups may belong in an existing PR.

## Issue format
Title: `[Type] Verb + specific outcome` (types: Feature, Bug, Refactor, Test, Docs, Chore).
Body sections, in order:
- **Context / Problem**: concrete current behavior and why it matters; distinguish confirmed facts from assumptions.
- **Scope**: smallest independently reviewable change; list affected layers when known.
- **Acceptance Criteria**: 2–6 observable checkboxes; include failure/edge cases when applicable.
- **Out of Scope**: explicit boundaries only if useful.
- **Validation**: exact tests or manual verification expected.
- **Related**: links to existing issues, PRs, docs, or dependencies where relevant.

Do not invent file paths, endpoints, APIs, test results, or completion claims. Ask for missing product decisions only if necessary. Keep issues concise and self-contained. Do not use vague acceptance criteria such as "works correctly".

## Triage
- Priority: P0 = blocks safety/reliability/usable core; P1 = near-term product; P2 = later enhancement.
- Size: S = focused and newcomer-friendly; M = moderate; L = split if independently deliverable.
- Apply `good first issue` only when requirements are unambiguous, setup is straightforward, and the change is low risk.
- Apply `help wanted` only when outside contributions are welcome and there is sufficient guidance.
- Use existing repository labels where possible; propose missing labels rather than assuming they exist.

## FileManager invariants
- Physical files are source of truth; the application owns metadata and virtual organization.
- Indexing, hashing, duplicate detection, monitoring and enrichment never modify, move, or delete source files.
- Any future physical-file change requires explicit user initiation and confirmation.
- Preserve Core / Application / Infrastructure / API / CLI boundaries; React consumes the API.
- Safety-related changes need automated regression tests.

## Lifecycle
1. Draft or refine issue; cross-link dependencies.
2. Implement on a branch, link the PR, and verify acceptance criteria.
3. Confirm relevant tests/CI and required review; do not mark tests as passed without evidence.
4. Close only after merged implementation or documented resolution; leave a clear explanation if closed without code.
5. Never merge or close merely because an AI agent generated code.

## Token efficiency
Prefer brief issues and references to existing architecture documents over copying long policies. Avoid splitting simple tasks into artificial subtasks.
