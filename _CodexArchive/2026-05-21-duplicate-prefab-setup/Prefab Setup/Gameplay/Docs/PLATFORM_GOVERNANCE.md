# Pyralis Platform Governance

Pyralis stays as one Unity package for now, but it is governed like a platform.

## Domain Rules

- `Core/` is for kernel concerns only: composition, lifecycle, diagnostics, contracts, save abstractions, and service registration.
- `Data/` is for cross-feature authored assets shared by multiple systems.
- `Editor/` is for shared authoring UX such as validation, setup helpers, and tooling infrastructure.
- `Features/<FeatureName>/` owns its complete gameplay slice.
- `Networking/` owns authority, replication, ownership, and backend adapters.
- `Presentation/` owns cross-feature camera, animation, audio, HUD, and VFX infrastructure.
- `Integrations/` owns package and service adapters.

## Scope Rules

- Participants are broader than pawns. A participant may control a pawn, camera, cursor, hand, board seat, faction, or menu selection.
- `PawnRoot` is the canonical character/pawn composition root, not the universal root for every game type.
- Actor-agnostic systems should depend on participant, action, target, source, owner, faction, and context contracts instead of movement-controller types.
- New genre support should begin as reusable capability families before it becomes sample-specific gameplay.
- Board, card, tactics, turn-based, projectile, and procedural systems must not be hidden inside one-off game UI or controller scripts.

## Feature Folder Template

Each new feature should follow this structure:

- `Runtime/Shared`
- `Runtime/2D`
- `Runtime/3D`
- `Data`
- `Editor`
- `Tests`
- `Docs`

Some mature features may use a smaller shape when they have no platform/runtime split yet, but new substantial features should either follow this template or document why a smaller shape is enough.

## Capability Family Rules

- Action and targeting features own action validation, target selection, action context, and resolution flow.
- Combat features own outcomes such as damage, healing, status effects, reactions, factions, and combat feedback.
- Guns and projectile features own delivery mechanics such as projectiles, hitscan, ammo, reload, pooling, spread, burst, and impact policies.
- Procedural generation features own authored generation rules, seeds, chunk/socket contracts, spawn budgets, and validation.
- Board and card features own board spaces, pieces, decks, hands, zones, turns, legal moves, costs, resources, and rule resolution.
- Presentation features own animation mapping, camera, HUD, VFX, audio, and feedback display.

When a feature spans multiple families, keep the lower-level reusable contracts in the family that owns the concept and put game-specific combinations in samples or mode definitions.

## Authoring Rules

- Designers should create major assets through guided entry points.
- Validation messages must be phrased for production authoring, not code debugging.
- Feature modules must declare network intent and authoring metadata.
- Major features should declare their capability family, authoring assets, setup docs, expected sample proof, and validation strategy before implementation.
- The canonical setup path should be easier to use than compatibility paths.

## Networking Rules

- New features must declare one network role before use.
- Gameplay code should target Pyralis networking contracts, not backend-specific APIs.
- Backend-specific implementations belong in `Networking/` or `Integrations/`.
