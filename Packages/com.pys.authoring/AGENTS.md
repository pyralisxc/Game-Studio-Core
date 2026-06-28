# PYS Authoring Agent Instructions

This file is the local instruction anchor for `Packages/com.pys.authoring`.

## Start Here

Read these small living docs before package architecture or behavior changes:

- `README.md`
- `Docs/CURRENT_STATE.md`
- `Docs/PACKAGE_BOUNDARY.md`
- `Docs/PROJECTION_CONTRACTS.md`
- `Docs/TARGET_PROJECT_INTEGRATION.md`
- `Docs/TESTING.md`

## Core Rule

PYS Authoring owns generic authoring machinery. Target projects own product meaning.

Do not add project, genre, studio, gameplay, route, proof-scene, or product-specific assumptions to this package. Product meaning may enter only as observed evidence from target project contracts, reflection, dependency edges, validation records, scene or asset names, or vocabulary providers.

## Ownership Model

- `Runtime` contains runtime-safe contract DTOs, attributes, validation records, generic action kinds, graph DTOs, and neutral enums.
- `Editor` contains scanner, graph projection, tab projections, exports, vocabulary, Hygiene, and the Authoring window.
- `Tests/Editor` validates contract resolution, projection packet shape, UI/export parity helpers, and generic scanner/projection behavior.
- `Docs` holds present-tense package truth, not old-path history.

Runtime code may reference `Pys.Authoring.Contracts`.

Runtime code must not reference `Pys.Authoring.Editor`.

`Pys.Authoring.Editor` must not reference target project assemblies directly.

## Projection Motto

Every tab renders a projection packet, and export saves that same projection packet.

If a tab needs information that is not in its projection packet, add typed evidence upstream or extend the projection. Do not let UI or export code compute a second truth.

## Current Workflow Model

- Settings chooses the observation scope.
- Intent selects a contract-backed authoring goal.
- Guide projects the selected proof path from contract setup steps, validation issues, metadata gaps, and success checks.
- Overview summarizes the active Guide path and next action.
- Map shows current scene, prefab, and asset evidence.
- Hygiene shows structural and ownership pressure.
- Facts shows evidence counts only.

## Editing Rules

- Prefer generic Unity terms and stable vocabulary keys.
- Keep vocabulary display-only.
- Keep validation records structured enough for projections and exports.
- Keep docs short and current. Delete stale old-path wording from active docs.
- Preserve `.meta` files for Unity package assets.

## Validation

Run:

```powershell
& "Packages/com.pys.authoring/Tools/Validate-PysAuthoringPackage.ps1"
```

Then use Unity refresh/import or Unity Test Runner as appropriate. Preferred test suite: `Pys.Authoring.Editor.Tests`.

## Handoff Rule

At any meaningful checkpoint, update `Docs/CURRENT_STATE.md` if package capabilities, residual risks, validation evidence, or next required work changed.
