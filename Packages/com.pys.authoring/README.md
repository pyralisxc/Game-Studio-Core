# PYS Authoring

This package is embedded at `Packages/com.pys.authoring` and linked from the Unity package manifest.

Do not copy project-specific authoring code into this package.

Goal:

- observe a target Unity scripts folder
- read optional authoring contracts declared by project scripts
- read optional local validation providers declared by project scripts
- build generic graph/projection evidence
- avoid hardcoded product, project, genre, mechanic, or studio-specific knowledge

Dependency rule:

- project/runtime code may reference `Pys.Authoring.Contracts`
- project/runtime code must not reference `Pys.Authoring.Editor`
- `Pys.Authoring.Editor` may observe loaded assemblies and Unity assets
- `Pys.Authoring.Editor` must not reference project-specific runtime assemblies

## Package Surfaces

- `Pys.Authoring.Contracts`: runtime-safe attributes, validation records, and graph DTOs.
- `Pys.Authoring.Editor`: editor-only scanner, graph projection, exports, and window.

## Living Docs

- `AGENTS.md`: package-local agent instructions.
- `Docs/CURRENT_STATE.md`: current package state, limits, validation, and next work.
- `Docs/PACKAGE_BOUNDARY.md`: package identity, evidence boundaries, and embedded package path.
- `Docs/PROJECTION_CONTRACTS.md`: tab projection contracts and export rules.
- `Docs/TARGET_PROJECT_INTEGRATION.md`: how target projects feed contracts, validators, and vocabulary.
- `Docs/HYGIENE_LENSES.md`: Hygiene sub-projection ownership.
- `Docs/TESTING.md`: validation and import test expectations.

## Settings Scope

The Settings tab owns only the observation boundary:

- target scripts folder
- manual scan
- export folder display/open action

Contracts are read only from scripts inside the selected scripts folder. Out-of-scope code may appear later as external dependency evidence, but it must not contribute first-class contract truth.

## Vocabulary Rule

Vocabulary dictionaries are display-only. They can provide labels, summaries, groups, and hints for stable keys. They must not decide setup validity, runtime behavior, or graph ownership.

## Hard Boundaries

- No target-project namespace references.
- No product behavior or runtime service activation.
- No product-specific workflow assumptions unless a target codebase declares equivalent metadata through contracts or validation records.
- Script names are labels. Contracts, reflection, fields, interfaces, components, and validation records are evidence.

## Import Checklist

1. Keep the package linked from `Packages/manifest.json`.
2. Compile the package by itself.
3. Add target-project contract annotations using `Pys.Authoring.Contracts`.
4. Confirm `Pys.Authoring.Editor` can scan the target scripts root without referencing target project assemblies directly.
