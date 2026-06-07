# Pyralis Board Offset Move Policy Checkpoint

**Goal:** Advance the Rules-Driven Tabletop MVP by making fixed jump/offset board movement authorable from Unity.

**Why:** Common tabletop and chess-like games need more than straight lines and adjacent moves. Knight-style moves, checker-style jumps, and custom tactical leaps should be expressible as reusable data before scene building begins.

## Scope

- [x] Add failing runtime tests for allowed and rejected offset moves.
- [x] Add failing editor tests for authored offset arrays and missing-offset validation.
- [x] Add `BoardMoveShape.Offset`.
- [x] Extend `BoardMovePolicy` with optional allowed offsets while preserving existing constructor callers.
- [x] Add `BoardMoveOffset` and `BoardMovePolicyDefinition.allowedOffsets` for Unity ScriptableObject authoring.
- [x] Update guided inspector copy so beginners know when and how to use offsets.
- [x] Update readiness, parity, roadmap, and inventory docs.

## Verification

- [x] Red build observed before production changes: tests failed because `BoardMoveShape.Offset`, `allowedOffsets`, and `BoardMoveOffset` did not exist.
- [x] Fast compiler pass after implementation: `dotnet build "Game Studio Core.slnx" --no-restore` succeeded with 0 warnings and 0 errors.

## Remaining Follow-Up

- Owner-relative forward movement is still a later slice for pawn/checker direction rules.
- Bundled named presets can now be built on top of shape and offset primitives instead of custom code.
