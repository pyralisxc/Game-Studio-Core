# Combat Definitions Setup

Use shared combat definitions to author combat types once and reuse them across `2D`, `2.5D`, and `rigged 3D` pawn stacks.

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Combat
- Realtime Character for brawlers, fighters, and action games
- Projectile Combat when combat actions fire projectiles or hitscan commands
- Turn/Menu Action when combat is selected through menus, tactics UI, cards, or command queues

Resolve setup-profile validation before creating combat definitions, assigning pawn combat profiles, or wiring hitboxes/projectiles.

## Core Assets

The shared combat layer now has three authoring levels:

- `WeaponData`
  - per-weapon stats such as damage, knockback, cooldown, hitbox zone, and projectile setup
- `CombatActionDefinition`
  - one authored move such as jab, kick, launcher, finisher, projectile burst, or special
- `CombatSequenceDefinition`
  - an ordered chain of authored moves for one input lane

`PawnCombatProfile` is the composition point that assigns the sequences to a pawn.
`EnemyCombatProfile` is the composition point that assigns enemy attack selection and timing.

## Supported Combat Lanes

- `Primary`
- `Secondary`
- `Aerial`
- `Special`

Current runtime integration is strongest on `Primary`, `Secondary`, and `Aerial`. The asset model is already neutral enough to grow further.

## Step 1

Create or reuse `WeaponData` assets for each move family:

- jab weapon
- kick weapon
- aerial weapon
- launcher weapon
- finisher weapon
- projectile weapon

## Step 2

Create `CombatActionDefinition` assets.

Recommended examples:

- `Action_Jab1`
- `Action_Jab2`
- `Action_Finisher`
- `Action_Kick1`
- `Action_AerialSlash`
- `Action_ProjectileBurst`

Important fields:

- `Input Type`
- `Archetype`
- `Animation Signal`
- `Combo Step`
- `Requires Hit Confirm For Next Branch`
- `Finisher Resets Combo`
- `Combo Window`
- `Cooldown Override`
- `Fallback Hit Box Zone`
- `Weapon`

## Step 3

Create `CombatSequenceDefinition` assets for each lane.

Examples:

- `Sequence_Primary_Combo`
- `Sequence_Secondary_Combo`
- `Sequence_Aerial_Combo`

Assign ordered `CombatActionDefinition` assets to the sequence.

Example primary chain:

1. `Action_Jab1`
2. `Action_Jab2`
3. `Action_Finisher`

## Step 4

Assign the sequences on `PawnCombatProfile`:

- `Primary Sequence`
- `Secondary Sequence`
- `Aerial Sequence`

You can still assign base fallback weapons:

- `Attack Weapon`
- `Kick Weapon`
- `Aerial Weapon`

These remain useful when a lane has no authored sequence yet.

## Step 5

Assign the `PawnCombatProfile` to the pawn through `PawnDefinition`.

This keeps combat modular and profile-driven instead of controller-driven.

## Enemy Setup

For enemies, keep the same authored mindset:

- assign `EnemyAttack` assets for each enemy move
- optionally assign an `EnemyCombatProfile` to `EnemyAI`
- use `ActorAnimationDriver` bindings when you want enemy attacks to run through shared animation signals instead of raw animator triggers

Recommended `EnemyAttack` fields when using shared animation:

- `Animation Signal`
- `Use Custom Animation Key`
- `Custom Animation Key`
- `Animation Step`

`EnemyAI` still supports `animatorTrigger` as a fallback, so you can move enemies over incrementally while keeping the combat data modular.

## Hit Confirm Behavior

`PawnCombatBehaviour` now listens for `HitBox` hit-confirm events.

That means:

- a sequence step can require a real hit before the next branch opens
- a whiff can naturally fail to branch
- a finisher can explicitly reset the combo chain

This is the shared foundation for:

- jab strings
- kick strings
- launcher chains
- finisher chains
- projectile follow-ups
- future special actions

## Practical Pattern By Lane

Recommended defaults:

- `2D`
  - blob shadow
  - authored combat sequences on `PawnCombatBehaviour2D`
  - `HitBox2D`
  - `PlayerInputHandler` actions for `Attack` and `Kick`
- `2.5D`
  - authored combat sequences on `PawnCombatBehaviour`
  - `HitBox`
  - `ActorAnimationDriver`
- `rigged 3D`
  - authored combat sequences on `PawnCombatBehaviour`
  - `HitBox`
  - rigged animator mappings

## Impact Feedback Services

Combat impact feedback is explicit. Components that can freeze time or shake the camera expose service fields instead of searching for global singletons:

- assign `TimeManager` or another `IHitPauseSink` to hitboxes, projectiles, projectile launchers, or reaction runtimes when hit pause is enabled
- assign `CameraShake` or another `ICameraShakeSink` when camera shake is enabled
- use runtime setters from spawn/bootstrap code when the owning camera or feedback service is created dynamically

## Common Mistakes

| Problem | Likely cause |
|---|---|
| Combo never advances | action requires hit confirm, but the move never lands or the wrong hitbox zone is assigned |
| Wrong animation plays | `Animation Signal` or `Combo Step` does not match the animator binding setup |
| Sequence always restarts at step one | combo window is too short, or branch was never allowed |
| Finisher never resets cleanly | final action is not marked as `Finisher Resets Combo` and the sequence is not set to reset after the final action |
| 2D attacks never fire | `Player` input map is missing `Attack` or `Kick`, or the pawn is missing `PawnCombatBehaviour2D` |
