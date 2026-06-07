# NeonBlack Gameplay Setup Guide

This is the setup entrypoint for the current Pyralis gameplay stack.

## Canonical setup

Start with:

- `CANONICAL_SETUP.md`

That document is the setup source of truth for new scenes.

## Core authoring flow

Create these assets first:

- `Create/NeonBlack/Gameplay/Profiles/Game Setup Profile`
- `Create/NeonBlack/Gameplay/Definitions/Runtime Pattern Definition`
- `Create/NeonBlack/Gameplay/Definitions/Session Definition`
- `Create/NeonBlack/Gameplay/Definitions/Game Mode Definition`
- `Create/NeonBlack/Gameplay/Definitions/Participant Definition`
- `Create/NeonBlack/Gameplay/Definitions/Pawn Definition`
- `Create/NeonBlack/Gameplay/Definitions/Actor Animation Definition`
- `Create/NeonBlack/Gameplay/Profiles/Pawn Presentation Profile`
- `Create/NeonBlack/Gameplay/Profiles/Pawn Animation Profile`
- other profiles as needed: movement, combat, traversal, camera, settings

Fast-start option:

- `Assets/Create/NeonBlack/Gameplay/Example Authoring Pack`

## Before scene wiring

Use a `GameSetupProfile` to describe the game loop before placing scene services or prefabs. Select multiple runtime patterns when the game overlaps categories, such as realtime character plus projectile combat or board/card/tabletop plus turn/menu action.

Then assign the setup profile to your `GameModeDefinition`. The inspector will summarize whether the setup requires a pawn, supports non-pawn control surfaces, uses projectile combat, or expects turn/menu style action.

## Required runtime path

New pawn-backed scenes should use:

1. `GameplaySessionBootstrap`
2. `PawnRoot` on every participant pawn prefab
3. `PawnDefinition` assigned to each participant
4. `PawnPresentationProfile` plus `PawnAnimationProfile`
5. `ActorAnimationDriver` on every animated pawn

Camera-only, board, card, checker, chess, tabletop, and turn/menu games still use `GameplaySessionBootstrap`, `SessionDefinition`, `GameModeDefinition`, and `GameSetupProfile`, but they do not need to force a `PawnRoot` or character controller unless the design needs actor pieces with pawn behavior.

## Official pawn presentation modes

- `Sprite2D`: flat 2D sprite pawn
- `Billboard2_5D`: sprite or flat visual presented in 3D space and camera-faced
- `Rigged3D`: true Animator-driven 3D model using a Generic or Humanoid rig

## Read next

- `CANONICAL_SETUP.md`
- `RUNTIME_PATTERN_COOKBOOK.md`
- `SCENE_SETUP_GUIDE.md`
- `Prefabs/README.md`
- `Prefabs/Bootstrap_Example_Setup.md`
- `Prefabs/Pawn_Setup.md`
- `Prefabs/Feature_Module_Framework_Setup.md`
- `../../Docs/NewGameTypeGuide.md`
