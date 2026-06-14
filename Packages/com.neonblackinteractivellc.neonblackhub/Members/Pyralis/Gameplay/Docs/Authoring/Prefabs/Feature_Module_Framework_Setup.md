# Feature Module Framework Setup

Use the shared feature-module framework whenever a gameplay system should plug into actors as authored runtime behavior instead of controller-specific code.

Designer-facing language can call these **abilities**. The implementation language is `FeatureModuleDefinition` plus an `IFeatureModuleRuntime` prefab. That keeps the authoring path concrete while still letting a pawn gain different abilities over time.

## Before You Wire This

Start with a `SessionDefinition` assigned to `GameplaySessionBootstrap.sessionDefinition` and a `GameModeDefinition` assigned to `SessionDefinition.defaultGameMode`.

Recommended route capabilities depend on the feature:

- Realtime Character for pawn-installed movement, traversal, combat, pickup, or feedback modules
- Combat or Projectile Combat for damage, status, reaction, and ranged-delivery modules
- Board/Card/Tabletop or Turn/Menu Action when a future feature module is not pawn-first

Resolve route validation before adding feature modules to `PawnDefinition`, enemy profiles, or actor feature hosts.

## Core runtime shape

- `FeatureModuleDefinition`
  - authored module id, install order, supported presentation modes, optional profile asset, and runtime prefab
- `ActorFeatureHost`
  - installs and shuts down module runtimes in deterministic order
- `ActorFeatureContext`
  - shared runtime context for pawns, enemies, and future actor types
- `IFeatureModuleRuntime`
  - neutral runtime contract for feature modules

## Pawn path

- assign feature module definitions on `PawnDefinition.featureModules`
- `PawnRoot` builds the actor feature context and initializes `ActorFeatureHost`
- module prefabs should contain one or more `IFeatureModuleRuntime` behaviours

Use this path when a pawn archetype should gain a capability without changing its baseline controller:

- create or reuse a `FeatureModuleDefinition`
- assign a runtime prefab that implements `IFeatureModuleRuntime`
- assign a module-specific profile asset when the ability needs authored tuning
- set supported presentation modes when the ability is not cross-form
- add the module asset to `PawnDefinition > Feature Modules`

Keep baseline body behavior in controller profiles (`InputProfile`, `PawnMovementProfile`, `PawnCombatProfile`, `PawnTraversalProfile`, `PawnPresentationProfile`, and `PawnAnimationProfile`). Keep optional reusable capabilities in feature modules.

## Enemy path

- assign an `EnemyFeatureProfile` to `EnemyAI`
- use `EnemyFeatureProfile.combatProfile` and `reactionProfile` for authored combat and reaction data
- assign module definitions in `EnemyFeatureProfile.featureModules`
- `EnemyAI` builds the actor feature context and initializes `ActorFeatureHost`

## Enemy reaction first-pass

Current modular enemy reaction support is provided by `EnemyReactionFeatureRuntime`.

Author with:

- `EnemyReactionProfile`
- `FeatureModuleDefinition.profileAsset`
- a runtime prefab containing `EnemyReactionFeatureRuntime`

Behavior currently handled:

- hurt reaction signal
- stagger reaction signal
- reaction lock timing
- optional hit pause
- optional camera shake
- optional knockback clear on death

## Traversal and interaction first-pass

Current modular traversal and interaction support is provided by:

- `PawnTraversalFeatureRuntime3D`
- `ActorInteractionFeatureRuntime`
- `InteractionFeatureProfile`

Recommended setup:

- assign a traversal module definition with a `PawnTraversalProfile` profile asset
- assign an interaction module definition with an `InteractionFeatureProfile` profile asset
- on `2.5D` or rigged-character pawn stacks, let `Motor3D` resolve those features through `ActorFeatureHost`

Current behavior:

- ledge probing and hang/climb flow can be owned by an installed traversal feature
- interact input can be routed through installed interaction handlers instead of directly through controller code
- if no handler consumes the interaction, the interaction feature can fall back to the shared `Interact` animation signal

## 2D pickup collection through the feature host

Current modular pickup support is provided by:

- `ActorPickupCollectorFeature2D`
- `ActorPickupCollectorFeature3D`
- `PickupFeatureProfile`

Recommended setup:

- add a pickup module definition with module id `actor.pickups.2d`
- add a pickup module definition with module id `actor.pickups.3d`
- assign a `PickupFeatureProfile` to `FeatureModuleDefinition.profileAsset`
- use a runtime prefab containing `ActorPickupCollectorFeature2D` for `Sprite2D`
- use a runtime prefab containing `ActorPickupCollectorFeature3D` for `Billboard2_5D` and `Rigged3D`
- make sure the actor root exposes the expected overlap surface for the target form:
  - `Collider2D` for `actor.pickups.2d`
  - `Collider` or `CharacterController` for `actor.pickups.3d`

Current behavior:

- overlapping `Collectible2D` items can be auto-collected through the installed pickup feature
- overlapping `Collectible3D` items can be auto-collected through the installed pickup feature for `2.5D` and rigged `3D`
- `Interact` input can collect the nearest nearby pickup when interaction collection is enabled
- pickup collection now lives in the feature-host/profile path instead of `Motor2D`

## Status effects and combat modifiers

Current modular status support is provided by:

- `ActorStatusEffectFeatureRuntime`
- `ActorStatusEffectProfile`
- `StatusEffectDefinition`

Recommended setup:

- add a status module definition with module id `actor.status`
- assign an `ActorStatusEffectProfile` to `FeatureModuleDefinition.profileAsset`
- use a runtime prefab containing `ActorStatusEffectFeatureRuntime`
- ensure the actor root exposes:
  - `HealthComponent`
  - `IActorMovementModifierReceiver`
  - `IActorCombatModifierReceiver`
  - `IActorHealthModifierReceiver`

Current behavior:

- authored starting effects can be applied automatically at feature initialization
- shared status application supports `Stun`, `Slow`, `SpeedBoost`, `DamageOverTime`, `HealOverTime`, `Poison`, `Burn`, `Shield`, `Armor`, `ArmorBreak`, `DamageBoost`, `KnockbackBoost`, and `RegenBoost`
- each `StatusEffectDefinition` can now author a `displayName`, a stack mode, and a max-stack cap for cleaner HUD messaging and more predictable reapply behavior
- movement lock, movement slow, outgoing damage scaling, outgoing knockback scaling, armor-style incoming damage reduction, and regen-rate boosts are routed through shared actor interfaces instead of controller-specific logic
- the same status lane can be consumed by pawns, enemies, `2D`, `2.5D`, and rigged `3D` actors

## Feedback event lane

Current modular actor feedback support is provided by:

- `ActorFeedbackFeatureRuntime`
- `ActorFeedbackProfile`
- `IActorFeedbackPublisher`
- `IActorFeedbackReceiver`

Recommended setup:

- add a feedback module definition with module id `actor.feedback`
- assign an `ActorFeedbackProfile` to `FeatureModuleDefinition.profileAsset`
- use a runtime prefab containing `ActorFeedbackFeatureRuntime`
- attach one or more `IActorFeedbackReceiver` behaviours under the actor when you want combat, pickup, status, or health feedback to be consumed locally

Current behavior:

- health damage, healing, and death can publish through the shared actor feedback bus
- status application can publish through the same bus
- combo confirms and pickup score collection can publish through the same bus
- UI, floating numbers, flashes, audio responses, and other presentation consumers can now attach to one shared event surface instead of reading controller-specific scripts

Concrete consumers added in the current framework pass:

- `ActorFloatingFeedbackReceiver`
  - actor-local world-space feedback for damage numbers, healing numbers, score popups, combo popups, and status popups
- `ParticipantFeedbackRelay`
  - forwards actor feedback into participant-aware shared services
- `ParticipantFeedbackService`
  - shared typed participant feedback stream for HUD and meta-UI consumers
- `ParticipantFeedbackHudPresenter`
  - HUD-facing presenter for combo, status, score, and combat alert messaging
- `ParticipantHealthHudBinder`
  - HUD-facing health binder for labels, fill bars, and reusable health panels
- `ParticipantHealthPanel`
  - reusable participant health presentation module for labels and fill bars
- `ParticipantTimedTextPanel`
  - reusable timed text module for combo, status, score, and combat alert layouts

Richer combat alerts now supported through the same lane:

- `Stagger`
  - published by actor and enemy reaction runtimes when reaction thresholds are crossed
- `GuardBreak`
  - published when a guarded actor is staggered hard enough to break defense
- `Parry`
  - published when a parry window fully negates an incoming attack
- `Finisher`
  - published when a finisher-marked combo action confirms successfully

These alerts can now be consumed by:

- `ActorFloatingFeedbackReceiver`
  - world-space popup feedback such as `Parry`, `Stagger`, `Guard Break`, and `Finisher N`
- `ParticipantFeedbackRelay`
  - participant-aware forwarding of the same combat alerts
- `ParticipantFeedbackHudPresenter`
  - HUD-facing label and timed-panel support for combat alert messaging

## Guard and reaction first-pass

Current modular guard and reaction support is provided by:

- `ActorCombatReactionFeatureRuntime`
- `ActorCombatReactionProfile`

Recommended setup:

- add a combat reaction module definition with module id `actor.combat.reaction`
- assign an `ActorCombatReactionProfile` to `FeatureModuleDefinition.profileAsset`
- use a runtime prefab containing `ActorCombatReactionFeatureRuntime`
- pair it with your input bridge on `2D` or let `Motor3D` resolve it directly on `2.5D` and rigged `3D` pawn stacks

Current behavior:

- guard state can be started and ended through the shared guard feature surface
- a short authored parry window can open when guard begins
- incoming damage can be reduced from the guarded frontal arc through the modular damage-modifier path
- successful parries can negate incoming damage, briefly lock the attacker, and publish a shared `Parry` feedback event
- hurt and stagger aftermath can apply shared animation signals and temporary reaction locks through the actor runtime
- guard break aftermath can use a dedicated authored shield-break lock duration when defense is overwhelmed

## Hazard impact modularization

Current shared hazard-impact support is provided by:

- `HazardImpactProfile`
- `HazardImpactUtility`
- `DamageZone`
- `DamageZone2D`
- `HazardData.impactProfile`

Recommended setup:

- author a reusable `HazardImpactProfile` for each hazard family
- assign that profile to `DamageZone` or `DamageZone2D` when you want zone hazards to use the shared hazard payload
- assign the same profile to `HazardData.impactProfile` when a spawned 2D hazard should use modular damage, knockback, and status effects on contact
- keep local fallback fields only for simple zones that do not need the shared hazard payload

Current behavior:

- hazards can now share one authored impact payload across `2D`, `2.5D`, and `rigged 3D` setups
- hazard impact can apply direct damage, optional knockback, and authored status effects through the same shared status runtime used by actors
- `DamageZone` and `DamageZone2D` now support the same authored hazard payload instead of each owning a separate damage vocabulary
- `HazardData` can opt into the same profile so the existing 2D hazard system gains modular damage/status behavior without losing its richer spawn and movement logic

## Hazard aftermath and feedback

Current hazard presentation feedback support is provided by:

- `HazardFeedbackProfile`
- `HazardFeedbackRuntime`
- `HazardData.feedbackProfile`

Recommended setup:

- author a `HazardFeedbackProfile` when a hazard family should own activation, explosion, bounce, collectible, or exit feedback
- assign that profile to `HazardData.feedbackProfile`
- add a `HazardFeedbackRuntime` to the hazard prefab root or child hierarchy
- add a `SpriteFlasher` when the feedback profile uses flash presets
- assign `Popup Camera` on `HazardFeedbackRuntime`, or call `SetPopupCamera` from the spawner/bootstrap path when hazards are created for a specific camera

Current behavior:

- `Slam`, `Crossing`, and `Bouncy` hazards can now trigger authored activation, explosion, bounce, collectible, and exit feedback without embedding that logic directly in the sequence data
- collectible destruction and collectible spawning can surface shared popup feedback
- explosion aftermath can now trigger authored flash and popup responses alongside the existing audio and camera shake hooks
- screen shake routes through an explicit `ICameraShakeSink` on the `Hazard` prefab or through `SetCameraShakeSink` from the setup/spawn path
- hazard SFX volume routes through an optional explicit `IGameplaySettingsApplier` on the `Hazard` prefab or through `SetSettings`; without it, hazards use the authored audio volume directly
- `HazardDataEditor` now surfaces impact and feedback profiles together so hazard families are easier to author as complete modular packages

## Validation notes

- `installOrder` controls deterministic module initialization
- `supportedPresentationModes` can filter modules per `2D`, `2.5D`, or `rigged 3D`
- if `supportedPresentationModes` is empty, the module is treated as cross-form by default
- `PawnDefinition` now reports duplicate, incompatible, and invalid feature-module assignments in the Inspector
- `EnemyFeatureProfile` now reports duplicate, incompatible, invalid, and missing-runtime-surface module assignments in the Inspector
- `GameModeDefinition` now reports duplicate or invalid required feature-module assignments in the Inspector
- runtime prefabs are now checked for required feature contracts and actor roots are checked for required collaborator surfaces before Unity scene iteration
- `actor.feedback` now warns when no `IActorFeedbackReceiver` exists in the actor hierarchy
- feedback prefabs can now surface deeper configuration warnings through `IRuntimeValidationProvider`, which catches issues like empty floating-feedback setups or HUD bridges with no bound presentation surfaces
