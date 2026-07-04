# Authoring Surface Refactor Changelog

This file records the package-wide authoring-surface refactor for review. It is intentionally separate from package release notes.

Scope rules:

- Do not modify `Assets/` during this lane unless Cameron explicitly asks.
- Do not auto-generate samples, scenes, prefabs, or ScriptableObject assets.
- Commit small verified slices as work proceeds.
- Prefer clean compatibility cuts when references allow.
- Keep PYS focused primarily on codebase quality, system health, ownership pressure, route semantics, topology, proof readiness, and cross-object setup truth.

## 2026-07-04

### Baseline Planning And Audit

Commits:

- `559bf90b` - `docs: plan codebase authoring surface refactor`
- `82914e22` - `docs: audit gameplay authoring surface ownership`
- `cc5ca80f` - `docs: define package-wide authoring surface direction`
- `9a3f2f28` - `docs: codify authoring surface refactor defaults`
- `53bbb15d` - `docs: keep agent guidance project-level`

Changes:

- Added the phased package-wide refactor plan.
- Added the ownership audit that classifies current code by owner/profile/adapter/pass-through/validator/presentation/scene-service/sample-only pressure.
- Updated living docs so the authoring-surface direction is package-wide rather than pawn-only.
- Updated `AGENTS.md` with durable project-management rules only; sequence-specific refactor guidance stays in the refactor plan and audit.

Verification:

- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because these changes were docs-only.

Notes:

- Existing unrelated local/generated changes remain outside this refactor lane: `.plastic/*`, `Game Studio Core.slnx`, and `Tools/Validation/Run-PreSceneValidation.ps1`.
