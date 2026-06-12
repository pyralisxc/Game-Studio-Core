# Pyralis Authoring Editor Layout

This folder is the editor-only authoring system for Pyralis. Keep it organized by distance from the product surface.

## Spine

`Spine/` contains reflective infrastructure and stable authoring truth:

- `Facts/`: fact records, convention providers, scanners, and registries.
- `Routes/`: route descriptors, proof language, capability catalog, intent advice, and route reports.
- `Validation/`: setup-flow, scene-readiness, runtime-claim, and feature-contract validation.
- `Evidence/`: scene-surface evidence snapshots and native Unity surface guidance.

Most feature developers should not need to edit the spine unless they are expanding the authoring language itself.

## Surfaces

`Surfaces/` renders or exposes authoring truth:

- `AuthoringWindow/`: the main guided Authoring Window and its UI assets.
- `Inspectors/`: shared inspector field guides and direct custom inspectors.
- `Tools/`: authoring-adjacent editor utilities, diagnostics windows, documentation helpers, and validation bridge helpers.

Surfaces should consume spine data. They should not become competing sources of route truth.

## Feature-Owned Truth

Feature-specific authoring contracts should stay beside the owning feature when practical and be discovered reflectively. Central authoring code should aggregate those contracts rather than maintaining parallel feature-id switch statements.

Feature-owned editor scripts that teach setup, draw field guides, or validate a feature's Inspector path should live under that feature's `Editor/Authoring/` folder. Keep the feature editor asmdef at the feature `Editor/` root when that preserves the existing assembly boundary. Generic feature editor utilities that are not authoring guidance should be rare and should have a clear owner-specific reason to stay outside `Authoring/`.

## Rule Of Thumb

If a file defines the authoring language, put it in `Spine`. If it renders the truth, put it in `Surfaces`. If it belongs to one gameplay capability, the feature should own it.
