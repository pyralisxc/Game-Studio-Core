# Pawn Setup

This file is the current source of truth for participant pawn setup.

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Realtime Character when participants control pawns
- Combat if the pawn can attack, block, take damage, or trigger reactions
- Projectile Combat if the pawn can fire weapons, spells, or thrown objects
- Animation/Presentation if the pawn uses sprite, billboard, or rigged Animator presentation

Resolve setup-profile validation before building pawn prefabs or assigning pawn profiles. If the selected setup does not require a pawn, use this guide only for optional actor presentation rather than forcing a character controller into the game.

## Shared pawn chain

Every playable pawn now uses the same authoring chain:

1. `PawnDefinition`
2. `PawnPresentationProfile`
3. `PawnAnimationProfile`
4. `ActorAnimationDefinition`
5. `PawnRoot` on the prefab
6. `ActorAnimationDriver` on the prefab

`PawnRoot` still applies movement, combat, traversal, and presentation data. The clean-slate change is that animation is now routed through `ActorAnimationDriver`.

## Required assets

Assign these on `PawnDefinition`:

- `Movement Profile`
- `Combat Profile`
- `Traversal Profile`
- `Presentation Profile`
- `Animation Profile`

## 2D pawn setup

Use this stack:

- `PawnRoot`
- `Motor2D`
- `Motor2DInputAdapter`
- `ActorAnimationDriver`

Presentation profile:

- `Presentation Mode`: `Sprite2D`
- `Sprite Default Faces Right`: set to match the art

Animation profile:

- assign the base Animator controller
- bind supported gameplay signals such as `Idle`, `Move`, `Dash`, `Death`, `Hurt`, `AttackPrimary`, `AttackSecondary`

## 2.5D pawn setup

Use this stack:

- `PawnRoot`
- `Motor3D`
- `Pawn3DInputModule`
- `Pawn3DMovementComponent`
- `Pawn3DTraversalComponent`
- `Pawn3DPresentationComponent`
- `PawnCombatBehaviour`
- `ActorAnimationDriver`

Presentation profile:

- `Presentation Mode`: `Billboard2_5D`
- choose billboard facing mode
- set sprite-facing direction

Animation profile:

- assign the base Animator controller
- bind locomotion, combat, traversal, hang, shimmy, and interact signals

## Rigged 3D pawn setup

Use the same stack as 2.5D, but change presentation mode:

- `Presentation Mode`: `Rigged3D`
- `Rig Type`: `Generic` or `Humanoid`

Rigged notes:

- use a standard Unity `Animator`
- assign the model as the animation driver's visual root when needed
- do not use billboard-facing behavior for rigged mode

## Animation authoring rules

- use `ActorAnimationDefinition` to declare the supported signal surface
- use `PawnAnimationProfile` to map those signals to Animator parameters, triggers, ints, and floats
- do not treat raw Animator parameter names as the source of truth
- gameplay systems should emit shared animation signals; the animation profile decides how Unity Animator reacts
