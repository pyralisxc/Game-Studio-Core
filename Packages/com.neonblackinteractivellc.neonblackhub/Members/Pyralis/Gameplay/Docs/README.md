# NeonBlack Gameplay Docs

This folder intentionally stays small. Active docs describe supported architecture, setup, verification, and roadmap truth. Extra audits, phase plans, historical notes, and duplicate setup guides belong outside the active doc set unless they protect current work.

## Living Docs

- `CURRENT_STATE_AUDIT.md` - current health, risks, Hygiene baseline, and verification posture.
- `ARCHITECTURE_BLUEPRINT.md` - runtime ownership, folderbase, vocabulary, and system boundaries.
- `FEATURE_DEVELOPMENT_ROADMAP.md` - current feature expansion priorities.

PYS Authoring package documentation lives under `Packages/com.pys.authoring/Docs`, not in this gameplay package.

## Maintenance Rule

If a new rule is important for every agent, put it in `AGENTS.md`.

If a new rule is important for humans opening the package, put it in `README.md`.

If a new rule belongs to one subsystem, update that subsystem's living doc. Do not add a new markdown file unless it will remain a durable source of truth.
