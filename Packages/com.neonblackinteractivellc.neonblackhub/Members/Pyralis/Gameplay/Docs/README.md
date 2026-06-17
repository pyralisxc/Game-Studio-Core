# Pyralis Gameplay Docs

This folder intentionally stays small. Active docs describe current truth only; old audits, phase plans, migration notes, and duplicate setup guides should be deleted or folded into the living docs below.

## Living Docs

- `CURRENT_STATE_AUDIT.md` - current health, risks, cleanup focus, and verification posture.
- `ARCHITECTURE_BLUEPRINT.md` - runtime ownership, folderbase, vocabulary, and system boundaries.
- `FEATURE_DEVELOPMENT_ROADMAP.md` - current feature expansion priorities.
- `Authoring/START_HERE.md` - human first-setup path.
- `Authoring/AUTHORING_BLUEPRINT.md` - Authoring Window behavior, tab ownership, graph projection, and hygiene rules.
- `Authoring/AUTHORING_MODEL.md` - asset/profile/runtime relationship map.
- `Authoring/CANONICAL_SETUP.md` - technical setup-chain contract.
- `Authoring/ROUTE_CAPABILITY_COOKBOOK.md` - compact route-capability vocabulary reference.

## Maintenance Rule

If a new rule is important for every agent, put it in `AGENTS.md`.

If a new rule is important for humans opening the package, put it in `README.md`.

If a new rule belongs to one subsystem, update that subsystem's living doc. Do not add a new markdown file unless it will remain a durable source of truth.
