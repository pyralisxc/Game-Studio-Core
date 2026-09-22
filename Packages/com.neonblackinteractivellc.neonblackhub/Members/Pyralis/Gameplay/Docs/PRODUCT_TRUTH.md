# Game Studio Core Product Truth

This document is the durable product-direction contract for NeonBlack Gameplay. Change it only when product direction changes. Current migration state, temporary debt, and implementation progress belong in `CURRENT_STATE_AUDIT.md`.

## Mission

Game Studio Core is a Unity-native game construction platform.

Unity owns engine behavior and native authoring surfaces. NeonBlack Gameplay owns reusable gameplay meaning, session ownership, capability composition, runtime contracts, and rule/data handoff. PYS Authoring observes and guides the authored route; it does not replace Unity. Development Intelligence may analyze the resulting technical truth; it does not define product meaning.

## Universal Spine

The platform-wide route is:

`Composition Root -> Session -> Mode / Rule Composition -> Participants -> Route-Specific Capabilities`

Participants are the universal session ownership concept. A participant can represent a human, AI-controlled seat, command owner, or other session actor. Seat, input device, pawn, network client, team, camera focus, and presentation are attachments to or properties of that participant; none of them defines participant identity.

Pawn gameplay is one route branch, not the universal definition of gameplay.

Examples:

- Character route: Participant -> Input -> Pawn -> Movement / Presentation -> optional Combat / Traversal / Interaction.
- Tabletop route: Participant -> Selection -> Board State -> Turn / Phase State -> Action Resolution -> Presentation.
- Cursor or UI route: Participant -> Input / Selection Surface -> Actions -> State / Presentation.
- RPG route: Participant -> Interaction / Action Route -> RPG-owned state and persistence.
- Future routes may omit pawn, playfield, camera, or direct input entirely when those concepts do not apply.

## Core Vocabulary

Core stays tiny and neutral.

`ParticipantId` belongs in Core because participant identity is needed by cross-feature contracts, events, scoring, actions, networking, and non-pawn routes. `ParticipantHandle` stays above Core because it carries runtime/session state and Unity-facing attachments.

Do not introduce a platform-wide `GameplayOwnerId` unless a concrete cross-feature owner exists that cannot be modeled as a participant. Feature-specific owner keys may remain local to their feature.

## Capability Composition

Capabilities come before genres.

Combat, interaction, traversal, dialogue, scoring, tabletop, RPG, hazards, feedback, camera, movement, and future systems should compose as capability families rather than requiring genre forks.

The platform must not grow a central feature switchboard. New capabilities should not require adding another permanent `GameModeDefinition.enableX` field plus matching branches in global composition code.

Feature owners should declare typed activation evidence, requirements, providers, receivers, scope, timing, and requiredness. Runtime wiring delivers the declared relationships.

## Runtime Wiring

There is one canonical runtime address language:

`Contract + Provider + Receiver + Package + Owner + Scope + Timing + Requiredness`

Runtime wiring owns delivery, not gameplay meaning.

- Scene search is inventory evidence only.
- VContainer delivers dependencies; it does not decide gameplay meaning.
- Feature owners decide when their capability is required or active.
- The wiring report, validators, PYS projections, and DI analysis should project the same typed relationships rather than invent parallel truth.
- `RuntimeSceneSearch` must not silently activate service families or repair missing setup in the target architecture.

## Game Mode

`GameModeDefinition` is the authored composition of high-level rules and route intent.

It is not the permanent registry of every feature the studio may ever create.

Camera, playfield, pawn, board, combat, scoring, respawn, dialogue, and other capability-specific requirements should only become required when the chosen route/capability needs them.

## Validation

`RuntimeValidationIssue` is the canonical validation evidence model.

Strings, Inspector help, PYS guidance, wiring rows, exports, and diagnostics should be projections of typed validation evidence rather than independent validation systems.

## Authoring

The Unity-facing experience should stay simpler than the internal implementation.

- Prefer Unity Inspector, prefabs, ScriptableObjects, Input System, Cinemachine, Animator, Test Runner, and mature ecosystem packages.
- PYS Authoring guides and explains route evidence; it does not auto-build the user's game.
- Runtime code may create gameplay output such as pawns, projectiles, effects, board views, or camera focus helpers.
- Runtime code must not create hidden setup repair objects such as missing managers, profiles, roots, camera rigs, or services.

## Legacy Policy

Retired games are not compatibility constraints on Game Studio Core.

`Members/Public` and other retired game content may be removed completely. Do not preserve old serialized fields, compatibility adapters, API shapes, scene assumptions, or package layout solely because retired content once referenced them.

Current core/package consumers explicitly kept in scope may justify compatibility. Historical games do not.

## Growth Order

1. Shed retired game content and compatibility-only surfaces.
2. Move neutral participant identity into Core and converge cross-feature ownership on typed participant identity.
3. Complete the runtime wiring hard cut so scene discovery is inventory-only and wiring becomes boring delivery.
4. Replace central feature-switch activation with owner-declared capability evidence.
5. Shrink `GameModeDefinition` toward route/rule composition and make pawn/camera/playfield requirements conditional.
6. Align tabletop, RPG, actions, scoring, networking, and other routes with the universal participant spine.
7. Make structured validation the single source projected into Inspector, PYS, wiring reports, and diagnostics.
8. Harden with Unity Test Runner, manual authoring/play proofs, architecture boundary tests, and CI when the repository is portable enough for it.

## Acceptance Standard

A refactor is successful when:

- a new capability can be added without editing a global feature catalog;
- a non-pawn game route can use the session/participant spine without fake pawn/camera/playfield setup;
- runtime wiring can explain why every required provider/receiver relationship exists;
- no scene scan silently decides platform meaning;
- participant identity remains typed across shared contracts;
- PYS and diagnostics report the same underlying evidence the runtime uses;
- Unity-native setup remains understandable to a beginner and adaptable for advanced projects;
- retired games impose zero compatibility requirements on the core.
