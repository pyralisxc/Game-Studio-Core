# Pyralis Tabletop No-Pawn Authoring Pass

Date: 2026-06-06

Status: Authoring fix applied; proof continuation needed

## Goal

Start the second Authoring 2.0 contrast proof through native Unity authoring: no pawn route, tabletop/board/action surface, and proof assets created through the real Project/Create and Inspector flow.

## Native Unity Actions Performed

- Relaunched Game Studio Core from Unity Hub after closing stale Unity Editor processes.
- Preserved Unity scene backups when prompted by `Recovering Scene Backups`.
- Used the Pyralis Authoring Window Intent surface.
- Set:
  - World / Playfield: `Board Grid Tabletop`
  - Control Shape: `Board Seat`
  - Presentation / Runtime Lane: `Tabletop No Pawn`
- Corrected capability toggles manually:
  - unchecked `Combat`
  - checked `Tabletop / Board / Card`
- Created `Assets/TabletopNoPawnProof` through the Project window Create menu.
- Created `ProofBoardPiece` through `Create > NeonBlack > Rules > Board Piece Definition`.
- Created `ProofBoard` through `Create > NeonBlack > Rules > Board Definition`.

## Assets Created

- `Assets/TabletopNoPawnProof/ProofBoardPiece.asset`
- `Assets/TabletopNoPawnProof/ProofBoard.asset`

Both were created through Unity's native Create menu and showed guided inspectors.

## Findings

The Intent surface contains the needed tabletop/no-pawn vocabulary, but changing the high-level intent did not automatically make the capability checklist coherent. After selecting `Board Grid Tabletop`, `Board Seat`, and `Tabletop No Pawn`, `Combat` remained checked and `Tabletop / Board / Card` remained unchecked until manually corrected.

The native Create menu exposes Rules assets clearly enough for `Board Piece Definition` and `Board Definition`.

The Create menu is clipped at the bottom of this layout. `Definitions` is partially below the visible menu, and `Rules > Board Move Policy` is below the window bounds. Keyboard navigation from the clipped menu created the wrong asset once (`Board Terminal Condition`) before it was removed. This is a real authoring friction point for the tabletop proof because the route needs top-level setup definitions and may need a move policy.

## Cleanup

The mistaken `ProofBoardMovePolicy.asset` was removed because it was actually a `Board Terminal Condition Definition`, not a move policy.

## Verification

Unity refresh completed after cleanup:

`[CodexUnityValidation] Refresh complete requestId=codex-20260605-214437732 imported=Assets/TabletopNoPawnProof/ProofBoard.asset,Assets/TabletopNoPawnProof/ProofBoardPiece.asset`

No compiler error pattern was found in the checked log excerpts. Full pre-scene validation was not run because the GUI Unity Editor is open.

## Authoring Fix Applied

The authoring path was improved in the live code:

- `PyralisAuthoringIntentAdvisor.GetDefaultGoals(...)` now gives tabletop/no-pawn, board/grid, card/table, board-seat, and card-hand selections tabletop-first suggested goals: `Tabletop`, `Interaction`, `Camera`, and `UI/HUD`.
- The Intent tab now applies suggested goals when the high-level world/control/lane selectors change.
- The Intent tab now exposes a `Use Suggested` reset for the current world/control/lane so a tester can recover from exploratory manual checkbox changes without applying a preset or creating assets.
- Native `CreateAssetMenu` rule assets now have explicit authoring order: Board Definition, Board Piece Definition, Board Move Policy, Turn Order Definition, Phase Definition, then Board Terminal Condition.

Unity refresh evidence for the code fix:

`[CodexUnityValidation] Refresh complete requestId=codex-20260606-064428378 imported=Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringIntentAdvisor.cs,Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringWindow.cs,Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardDefinition.cs,Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardPieceDefinition.cs,Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardMovePolicyDefinition.cs,Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/TurnOrderDefinition.cs,Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/PhaseDefinition.cs,Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardTerminalConditionDefinition.cs utc=2026-06-06T16:44:31.3638955Z`

No `error CS`, `Compilation failed`, or `Scripts have compiler errors` patterns were found in the checked log after this refresh. The refresh helper itself timed out while attached to the open GUI Editor, so this is log-backed refresh evidence rather than a clean helper exit.

Computer Use follow-up:

- Dismissed a separate Unity `Error!` prompt so only the main Game Studio Core Editor window remained.
- Verified the live Intent tab still had `Board Grid Tabletop`, `Board Seat`, and `Tabletop No Pawn` selected.
- Used `Use Suggested`; visible top-of-list state changed away from pawn defaults with `Movement` and `Jump / Traversal` unchecked. The Unity IMGUI scroll area jumped before a clean lower-checkbox screenshot was captured, so the next proof continuation should re-check the lower rows before proceeding.

## Next Required Proof Continuation

Continue the live proof through the native authoring path:

- Re-open or refresh the Intent tab and confirm the suggested tabletop/no-pawn goals show `Tabletop`, `Interaction`, `Camera`, and `UI/HUD` checked, with pawn movement, jump/traversal, and combat unchecked unless the tester intentionally adds them.
- Use the native Project window Create menu to verify `NeonBlack > Rules > Board Move Policy` is reachable without clipped-menu guesswork.
- Create the real `ProofBoardMovePolicy.asset` through the native Create menu, then wire it from the board/tabletop rules path as the next proof asset.

Ready state: `Authoring fix applied; continue proof`.
