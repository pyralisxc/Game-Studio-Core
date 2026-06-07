# Pyralis Intent Capacity Live Proof Note

Date: 2026-06-04

Status: In progress

## Route

1P Sprite2D side-view brawler, using Jim's imported sprite sheets as the intended art source.

## Goal

Verify the fleshed-out Intent tab can guide a beginner from a general 2D brawler idea toward the right route capacities before the animated pawn setup proof.

## Starting State

- Unity Editor open in a blank `Untitled` scene.
- `Pyralis Authoring` window docked and visible.
- `Assets/Jim/...` contains imported sprite sheets.
- No `.controller`, `.anim`, or `.overrideController` assets were found under `Assets`.
- Existing `Assets/PyralisProof` assets were present, but they are incomplete route remnants:
  - `BrawlerSession.asset`
  - `BrawlerGameMode.asset`
  - `BrawlerSetupProfile.asset`
  - `BrawlerCharacterPawnPattern.asset`

## Authoring Window Modes Used

- Overview
- Intent
- Inspector guidance for `BrawlerSetupProfile`

## Native Unity Actions Performed

- Opened Unity through Unity Hub.
- Activated the Unity Editor.
- Viewed the Authoring Window in a blank scene.
- Switched from Overview to Intent.
- Scrolled the Intent tab to verify the route reading and capacity shelves.
- Selected `Assets/PyralisProof/BrawlerSetupProfile.asset` in the Project window.

## Observed Result

- Blank-scene Overview correctly reported no setup context and pointed to native Unity setup actions.
- Intent displayed grouped `Capabilities I'm Considering` controls.
- Default Sprite2D brawler goals read as `2D Side-View Action + Pawn Brawler for Sprite2D`.
- Capacity shelves rendered with expanded fact rows and match labels such as `Strong match`.
- Selecting `BrawlerSetupProfile` made Active Setup follow `BrawlerSetupProfile (GameSetupProfile)`.
- The `GameSetupProfile` Inspector guided the user back to the Intent tab and showed the existing Character Pawn Gameplay capability row.

## Blockers Found

- The route is not ready for an animated pawn proof because the project does not currently contain Animator Controller or animation clip assets under `Assets`.
- The existing `PyralisProof` assets do not include the required participant, pawn definition, input profile, pawn movement profile, presentation profile, animation profile, pawn prefab, or scene root assignment.
- `BrawlerCharacterPawnPattern.asset` still has generic placeholder values (`pattern.runtime`, `Runtime Pattern`) and needs route-specific naming/content before it is useful as a beginner-facing pattern.

## Required Cameron Assets Or Decisions

- Confirm or provide the intended Animator Controller and animation clips for Jim's character art.
- Confirm which sprite sheet/character is the first brawler pawn source.
- Confirm whether the first proof should be side-view jump/gravity or top-down brawler movement.
- Cameron remains final validator for movement feel, attack readability, animation fit, collider fit, and route taste.

## Fixes Made

- Intent tab was updated before this live pass to render studio-wide capacity shelves from reflective facts.
- Tests now prove Sprite2D brawler, tabletop/no-pawn, networking/export, and unsupported-lane caution coverage at the Intent advisor level.

## Residual Risk

The Intent flow is easier to understand, but the full animated pawn proof is blocked until the animation/controller assets and route asset chain exist. Do not promote the 1P Sprite2D route from this pass alone.

## Ready State

In progress. Intent UI proof passed for the current slice; animated-pawn route proof is blocked by missing animation/controller assets and incomplete route assets.
