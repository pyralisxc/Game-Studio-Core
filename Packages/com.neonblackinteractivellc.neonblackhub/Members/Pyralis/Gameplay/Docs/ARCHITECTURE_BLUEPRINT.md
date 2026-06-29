# NeonBlack Gameplay Architecture Blueprint

This document describes the supported architecture for NeonBlack Gameplay: a shared, modular, N-player-ready Unity gameplay package.

Use this as the runtime ownership and folderbase reference when extending gameplay systems, authoring contracts, and setup guidance.

## Goals

- Support a wide variety of game types from one shared gameplay toolkit.
- Make one-player, two-player, and future larger multiplayer modes variations of the same architecture.
- Prefer Inspector-authorable systems over hardcoded behavior forks.
- Keep "arcade", "brawler", and future modes as compositions of shared features and data profiles.
- Minimize custom framework code when stable Unity or commercial-ready open source solutions already exist.
- Support non-character games where a participant controls a camera, cursor, hand, board seat, faction, or menu selection instead of a pawn.

## Runtime Foundation

The shared-core startup path is built from these concrete building blocks:

- `GameplaySessionBootstrap`
- `SessionStateService`
- `ParticipantRosterService`
- `ParticipantSpawnService`
- `ParticipantInputRouter`
- `PawnRoot`
- `CinemachineCameraRigController`
- `SessionDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `GameModeDefinition`
- `PlayfieldProfile`
- `CameraRigProfile`
- `InputProfile`
- `SettingsProfile`
- module-owned component/profile capabilities and pawn-module interfaces
- PYS Authoring discovery for `Pys.Authoring.Contracts.AuthoringContractAttribute` metadata

New gameplay work should extend this Inspector-driven shared-core startup path and expose authoring evidence through PYS contracts or validation providers.

`SessionStateService` is the shared gameplay-active state owner. Feature systems that need to know whether gameplay is running should consume `IGameplayStateReader`. Mode-specific flow orchestrators, such as the 2D `ArcadeGameFlowController`, should expose their own flow contract for panels, scoring, and arcade transitions, then drive the shared session phase rather than implementing a second gameplay-state source.

Runtime folder ownership is part of the architecture contract. `Core` owns runtime-visible contracts and tiny shared type vocabulary only; its runtime contracts should be grouped by seam family so shared language is discoverable without becoming implementation. `Data` owns authored definitions, profiles, config, shared profile helpers, and data-backed runtime handoff contracts, including combat definitions such as weapons, sequences, projectiles, fire modes, status effects, participant handoffs, and interaction dispatch context. `Modules` own reusable gameplay capability, and broad modules should put child subsystem concepts in child folders instead of making the module root a junk drawer. `Glue` owns route/session composition, bootstrap, lifetime, participant services, input routing, service registration, and spawn coordination. `Presentation` owns camera, animation, visual, and HUD infrastructure. `Feedback` owns feedback-display gameplay modules such as damage numbers, floating feedback receivers, and participant feedback relays. Module assemblies should not reference `Glue`, ordinary module runtime assemblies should not reference VContainer, and module runtime assemblies should not reference `Presentation` for concrete animation, visual, camera, or HUD implementations. Glue composes modules through contracts, data, and explicit service registration; Presentation implements the concrete Unity-facing visual/camera controllers behind Core/Data seams. `NeonBlack.Gameplay` is a thin compatibility facade over `NeonBlack.Gameplay.Glue`, not a place to add new runtime behavior. The `.asmdef` graph should remain acyclic.

Use SOLID principles as an ownership lens when a file, module, or dependency direction is ambiguous. Single responsibility should clarify the owner of a behavior; open/closed should favor data profiles, module-owned extensions, and narrow seams over editing central switchboards; Liskov substitution should keep base contracts honest for derived/networked variants; interface segregation should keep receiver/reader/sink contracts small; and dependency inversion should move cross-module communication through stable Core/Data contracts, commands, events, state readers, handlers, or sinks. Do not apply SOLID as ceremony: avoid new abstractions, wrappers, or interfaces unless they make ownership clearer or reduce real coupling.

Namespace fan-out is also architecture evidence. A focused runtime script should import very few `NeonBlack.Gameplay.*` namespaces because broad imports usually mean the script is owning too much gameplay knowledge. Pure data/model scripts should usually need `0-1` NeonBlack Gameplay namespaces, leaf gameplay MonoBehaviours `1-2`, feature runtime modules `2-3`, and presentation scripts `1-2`. Explicit `Glue/*` composition files may import more because they wire systems together, but they still become review targets when they start carrying feature behavior instead of composition. Hygiene flags files that exceed their owner budget as `NamespaceDependencyFanout`; outside Glue, more than three NeonBlack Gameplay namespaces is audit pressure and more than five is a cleanup target. Do not hide coupling in managers to satisfy the count. Shrink the count by moving behavior to the true owner, depending on smaller contracts/data/events, or keeping cross-domain wiring in Glue.

Runtime communication should follow the same ownership discipline as PYS Authoring. PYS compiles scattered setup evidence into graph truth and then projects typed views; runtime should expose typed communication seams instead of letting modules directly know every concrete collaborator. The supported communication lanes are:

- **commands** for requested work, such as spawn participant, request attack with `ActorCombatCommand`, request interaction, submit input action, or start proof attempt
- **events/results** for facts that happened, such as participant joined, pawn spawned, combat attack started through `ActorCombatResult`, damage applied, pickup collected, score changed, or proof state changed
- **state readers** for current domain state exposed without exposing the concrete service or feature owner
- **handlers and sinks** for module-owned responders that consume stable contracts
- **state machines** for lifecycle owners that answer what mode a thing is in and which transitions are allowed
- **Glue composition** for VContainer and scene-owned wiring that connects channels, handlers, state machines, and services

VContainer wires dependencies from Glue; it is not the runtime communication model. A class should not depend on VContainer, a service locator, or a broad manager just to avoid naming the real gameplay contract. If the communication meaning is a request, use a command. If it is an observed fact, use an event. If a module needs current state, depend on a narrow reader. If a module reacts to another domain, use a handler, sink, or explicit interface owned by the stable contract surface. When Glue needs to hand scene services to authored components, use a narrow runtime-services context or an explicit configure method rather than injecting VContainer into module behavior.

Use focused state machines where lifecycle is real. Do not create one global `GlobalGameplayStateMachine`. Prefer small owners such as `SessionStateMachine`, `ParticipantStateMachine`, `PawnLocomotionStateMachine`, `CombatActionStateMachine`, and authoring/proof state flow. Each state machine should be a plain lifecycle model with explicit transitions; MonoBehaviours and services may host or observe it, but the transition rules should not be scattered across unrelated booleans, validators, and inspector guidance. State machines live in the lane that owns the meaning: session and participant flow in Glue, pawn locomotion in Character, combat action phase in Combat.

Gameplay time follows the same ownership rule. If a component changes gameplay state over time, it should inherit `GameplayTickBehaviour`, declare its `GameplayTickDomain`, and use `GameplayTickContext` for delta values. Character movement, combat windows, status effects, hazards, traversal, scoring, spawning, interaction cooldowns, and enemy decisions are gameplay tick owners. Presentation, HUD effects, camera smoothing, popup motion, depth sorting, and local input polling may use Unity `Update`, `LateUpdate`, or local `Time.deltaTime` when they only render or collect state and do not advance gameplay rules. Do not keep bridges where one module manually ticks another module's timers; the owning module should tick itself or expose a command/event/state seam.

Direct module imports are allowed only when the dependency is a genuine same-subsystem composition edge or a stable contract owned by that module. Character should not import Combat just to host combat behavior; pawn combat implementation belongs in Combat. Combat should not import Character just to read facing, airborne, or action-lock state; that flows through `IActorCombatMovementState`. Character should not import Combat just to read attack timers or combat movement multipliers; that flows through `IActorCombatMovementInfluence`. Traversal should not import Character just to coordinate the 3D pawn movement sibling; traversal-facing movement state flows through `IPawnTraversalMovementController`, traversal profile application flows through `IPawnTraversalModule`, and optional feature discovery flows through `IActorTraversalFeature`. Traversal and RPG should not import Interaction just to implement an interaction handler; `Data.Interactions.ActorInteractionContext` and `Data.Interactions.IActorInteractionHandler` are the shared handler seam, while Interaction owns the concrete dispatcher and pickup collectors. Input should not import Character, Combat, or Interaction just to drive a pawn; participant/profile handoff belongs to `Data.Participants`, and move/combat/guard/interaction requests should flow through `Core` command or receiver seams. Character, Combat, Traversal, and Enemies should drive animation through `IActorAnimationController` rather than knowing the concrete `ActorAnimationDriver`; Enemies should face billboards through `IBillboardFacingController`; Hazards should play flash feedback through `IVisualFlashPlayer`; Encounters should switch camera profiles through `ICameraRigProfileSwitcher`. Cross-subsystem behavior such as pickups awarding score, hazards applying combat reactions, enemies driving combat, UI observing session state, or scene flow reacting to gameplay outcomes should prefer commands, events, state readers, handlers, or narrow sinks before importing concrete module behavior. If a direct module import remains, it should be easy to explain in one sentence as ownership, not convenience. Do not import a module solely to name data assets that live under `Data`, to check a movement component when a `Core` state reader can answer the question, or to display feedback that can be expressed through a `Core` sink or publisher contract such as `IDamageNumberSink` or `IActorFeedbackPublisher`.

Editor ownership follows the same rule. A feature editor may validate and organize local fields, but it must not become a PYS Authoring adapter or a combined inspector for another module's implementation fields. Enemy AI editor owns enemy AI fields; enemy attack and hitbox tuning belongs to the Combat-owned `EnemyCombatModuleEditor`.

## Core Architectural Principles

### 1. N-Participant Core

NeonBlack Gameplay should be built for N participants.

`1P` and `2P` are not special architectures. They are common configurations of the same participant model.

Local multiplayer is participant topology, not networking. A local couch co-op proof uses Unity `PlayerInputManager` to pair devices to participants; a networked proof uses the networking authority path. Hybrid routes may use both, but the graph should keep these concerns separate:

- `SessionDefinition.networkMode` describes transport and authority.
- `ParticipantDefinition` describes seats/control owners.
- `ParticipantInputRouter` describes whether defaults auto-register or wait for `PlayerInput` join.
- `ParticipantSpawnService` describes whether pawns spawn when participants register or wait for manual/custom spawn.
- The authoring graph compiles these into `SoloLocal`, `LocalJoin`, `Networked`, or `HybridLocalNetworked` evidence.

### 2. Shared Capability, Data-Driven Identity

Shared systems should provide reusable capability.

Game identity should mostly come from:

- data assets,
- enabled modules,
- profile assignments,
- authored prefabs,
- mode configuration.

### 3. Inspector Is The Daily Surface

Most day-to-day design iteration should happen in:

- prefabs,
- `ScriptableObject` assets,
- custom inspectors,
- scene setup,
- package examples.

The Inspector path should be treated as product surface, not secondary tooling. If the preferred setup requires knowing hidden code rules, the authoring model is not done yet.

The maintainable path is:

- one obvious scene entrypoint,
- one obvious top-level session asset,
- visible links from session, mode, participants, pawns, module-owned components/profiles, scene evidence, and reflected contracts, with vocabulary supplying labels and generic wording only,
- validation messages near the fields that caused them,
- feature-owned authoring contracts that feed setup guidance, validation, facts, and proof targets.

Inspectors should stay tactical. They can show local field integrity, local validation messages, and asset-local utilities that operate on the inspected object. They should not launch PYS Authoring, become parallel route guides, preset pickers, proof cards, or hidden setup paths. If a user needs to understand where an object fits in the route, PYS Authoring should explain it from the graph it observes.

### 3.5. Feature Contracts Own Feature Setup Truth

Reusable module components and profiles declare their authoring requirements on the owning feature type with `Pys.Authoring.Contracts.AuthoringContractAttribute`. PYS Authoring discovers those attributes reflectively. Central authoring code lives in `com.pys.authoring`; NeonBlack Gameplay should not maintain parallel switch statements or manual lists for feature-specific profile, lane, action, runtime-interface, or proof rules.

A complete capability contract names:

- contract `Surface`, so selectable gameplay ingredients are separated from route essentials, setup glue, profiles, services, adapters, presenters, runtime components, and vocabulary-only facts
- stable capability id or setup node id when the graph needs to join evidence across sources
- required profile type
- required runtime interfaces or components
- supported and unsupported presentation lanes
- consumed action roles
- native setup actions
- assignment fields
- customization moments
- route proof target

This keeps feature ownership local while allowing PYS Authoring, inspectors, validators, facts, and proof workflow to agree on the same data.

### 4. Pawn Composition Uses Direct Module Ownership

The supported pawn stacks now implement pawn interfaces directly on focused components.

For the 3D brawler, `Motor3D` is the composition root. It coordinates focused sibling components: `Pawn3DInputModule`, `Pawn3DMovementComponent`, Traversal-owned `Pawn3DTraversalComponent`, and `Pawn3DPresentationComponent`. Each implements the corresponding pawn interface directly.

For the 2D stack, `Motor2D` is the shared 2D pawn motor surface. Focused ownership lives in `Motor2DInputAdapter`, `PlayerInputHandler`, `PawnCombatBehaviour2D`, `Pawn2DMovementComponent`, and `Pawn2DPresentationComponent`. `Motor2D` exposes neutral runtime state through `IActorMotionStateReader`, `IActorHazardImpactTarget`, facing, reaction, and movement modifier contracts so scoring, hazards, presentation, and other modules do not need to import Character internals. `PawnCombatBehaviour2D` uses the shared `PawnComboProcessor` for combo state while keeping 2D hitbox/projectile application local. `Pawn2DMovementComponent` currently keeps one beginner-facing component while naming its two internal lanes: top-down/no-gravity uses a Kinematic Rigidbody2D, and side-view/gravity uses a Dynamic Rigidbody2D. `Pawn2DPresentationComponent` follows the same facade rule: sprite facing/tint, animation signals, deformation, and feedback audio are named internal lanes until a route earns separate presenter scripts.

`Motor2DInputAdapter` is the supported Add Component surface for new player-controlled 2D pawns. `PlayerInputHandler` remains the lower-level reader behind that route and is only a direct setup surface for custom input experiments.

Shared pawn contracts live with the narrowest stable owner. Profile-application handoff contracts such as `IPawnInputModule`, `IPawnCombatModule`, `IPawnTraversalModule`, and `IActorTraversalFeature` live under `Data.Participants` because `PawnRoot` and sibling components apply authored participant/pawn profile data through them. Traversal-facing movement state also lives behind the Data-owned `IPawnTraversalMovementController` seam because traversal needs profile-backed movement coordination without importing Character. Traversal owns `Pawn3DTraversalComponent` and concrete climb/ledge/hop behavior. Combat owns the concrete pawn combat components, consumes `IActorCombatMovementState` for pawn-facing motor state, and exposes `IActorCombatMovementInfluence` when movement needs combat timing or multipliers. Presentation owns `PawnCameraTarget` and camera rigs, because those are camera-routing surfaces rather than character logic. This keeps Character, Combat, Traversal, and Presentation independently navigable while still allowing a prefab to compose them as siblings.

### 4.5. Runtime Composition Registers Core First, Features By Evidence

`GameplaySessionBootstrap` remains the scene entrypoint and serialized handoff. `GameplayLifetimeScope` owns runtime service resolution, dependency registration, and route-specific service activation. The always-on runtime spine is intentionally small:

- session definition and session state
- participant roster
- participant spawning
- participant input routing
- authored scene services such as scene loading, time, camera shake, settings, and camera rig when present
- ownership and authority services

`GameplaySessionBootstrap` starts and hands off the session; it does not own participant spawn points. `ParticipantSpawnService` owns participant pawn placement, including its `Spawn Points` array and `Spawn On Register` policy.

For solo local routes, `ParticipantInputRouter` may auto-register default participants without a `PlayerInputManager`. For local join routes, default participants act as seat templates and Unity `PlayerInputManager` should create the joined `PlayerInput` so each controller owns one participant and pawn. Leaving auto-register defaults enabled on a multi-participant local join route is a setup contradiction because every auto-join seat can spawn before devices join.

The proof should match the current Intent focus. When a selected gameplay ingredient declares a proof target, that focused proof wins; participant topology, pawn ownership, input, spawn, camera, and movement setup become prerequisites for proving that selected behavior. If no selected ingredient declares a proof target, solo pawn routes fall back to one participant, one pawn, one input path, while local join pawn routes fall back to one joined `PlayerInput` per participant, one pawn per joined seat, isolated controller input, authored spawn points, and camera focus after pawn assignment. These requirements are inferred from authored session/input/spawn graph evidence, not selected through a preset.

Feature services are not core by default. Combat, enemy, RPG, game-flow, scoring, and feedback services register when the authored route asks for them through `GameModeDefinition`, participant pawns, resolved capability contracts, or actual loaded scene components. Do not register no-op placeholder services to satisfy UI or route shape; a route either supplies the real service owner or the dependent UI reports that the service is unavailable.

Feature-specific service lists should not expand the composition root. `GameplayLifetimeScope` owns the visible service graph entrypoint; `RuntimeFeatureServicePolicy` owns route and loaded-scene activation evidence; the platform feature-service installer owns common registration mechanics; and feature installers under `Glue/ServiceRegistration/FeatureInstallers` own concrete feature registration lists when a domain is broad enough to justify its own local seam. This keeps the lifetime scope readable without putting VContainer inside feature modules or creating a second setup path.

**Strict Authoring Rule:** `GameplaySessionBootstrap` uses Unity's `RequireComponent` path to keep `GameplayLifetimeScope` visible, and runtime systems must not autospawn GameObjects or create entries to fix missing scene references. Missing core services stay null at runtime and log a clear error, while PYS Authoring Map/Guide evidence guides the user to manually add the missing objects in the scene. Hygiene can audit the graph pressure, but concrete scene repair belongs in Map. This keeps the scene hierarchy as the singular source of truth and prevents hidden systems-on-top-of-systems complexity.

Runtime creation is allowed when it is gameplay output, not setup repair. Examples that may create objects at runtime include spawned participant pawns, projectiles, pooled effects, world-space popups, fade overlays, generated board/pickup views, and camera focus helper transforms. These objects are outputs of authored systems. Runtime code should not create missing session services, camera rigs, managers, profiles, setup assets, or scene roots to make a route appear configured.

Runtime components should not carry parallel fallback payloads when a feature already has an authored data owner. Damage zones consume `HazardImpactProfile` for damage, tick cadence, knockback, targeting, and status behavior. Pawn combat consumes `CombatSequenceDefinition` and `CombatActionDefinition` for primary, secondary, and enabled aerial attack paths. Missing profile or sequence data is setup evidence for validation and PYS Authoring, not an invitation for the component to invent a simpler local behavior.

This keeps a focused proof from carrying unrelated RPG, enemy, combat, scoring, feedback, and arcade game-flow assumptions while preserving feature parity for scenes that actually use those systems.

### 4.6. Optional Feature Domains Stay Feature-Owned

Optional feature domains can be broad without becoming core engine spine. RPG is the current example:

- value/domain models and definition-facing contracts live under `Data/Rpg`,
- `Data/Definitions/Rpg` owns authored ScriptableObject assets,
- `Modules/Rpg/Runtime` owns RPG runtime services and runtime composition,
- `Modules/Rpg/UI` owns scene and panel presenters,
- `Modules/Rpg/Editor` owns RPG-specific editor tools.

PYS Authoring should discover RPG through contracts, reflection, dependency evidence, and graph projections. NeonBlack Gameplay should not carry a separate RPG setup system or restore a universal `Editor/Authoring` graph spine.

The same rule applies to other broad optional domains. `Data/Tabletop/Rules` owns board and turn value contracts that authored definitions can reference without pulling in runtime behavior. `Modules/Tabletop/Runtime` owns tabletop action resolvers, the current in-memory action queue implementation, selection bridges, and runtime service seams. Core owns the small shared action queue contract and value language in `Core/Types/Actions`; feature modules provide concrete queue/resolver behavior instead of importing an Actions module. Core only keeps stable contracts, tiny shared type vocabulary, and neutral utilities when multiple feature/data assemblies need the same value language, such as action values, movement modes, presentation lanes, animation signals, runtime service handoff contracts, and gameplay tick context contracts. Authored config assets live in `Data/Config`, participant input setup lives in `InputProfile`, runtime session context lives in `Glue/Lifetime`, and global time-scale/hit-pause ownership lives in `Glue/SceneServices/TimeScaleService`. Scene flow and menu shell navigation belong to `Glue/SceneFlow/Navigation`, not Core.

### 5. Runtime Is Transport-Agnostic

Gameplay architecture should not assume local-only or network-only ownership.

The runtime model should keep participant identity, pawn identity, and input ownership separate so networking can be added cleanly later.

### 6. Networking Is An Optional Extension Layer

`NeonBlack.Gameplay` does not reference the optional `NeonBlack.Gameplay.Networking` assembly. NGO-dependent session, authority, spawn, validation, and NetworkManager behaviour lives in the separate Networking assembly. One current shared character seam, `MovementStateSnapshot`, still uses NGO serialization types so movement replication can share DTOs with networked adapters; keep that exception narrow or move it behind a dedicated networking contract before expanding prediction/reconciliation work.

The three participant services expose protected virtual override points:

- `SessionStateService.TryStartHostIfNeeded()` - overridden by `NetworkedSessionStateService` to call `NetworkManager.StartHost()`
- `ParticipantRosterService.ResolveOwnerClientId()` - overridden by `NetworkedParticipantRosterService` to return `NetworkManager.LocalClientId`
- `ParticipantSpawnService.SpawnParticipantPawn()` / `DestroyPawnInstance()` - overridden by `NetworkedParticipantSpawnService` to call `NetworkObject.Spawn()` / `NetworkObject.Despawn()`

For local games register the base classes at bootstrap. For online games register the `Networked*` variants instead.

## Target Runtime Vocabulary

### Participant

A gameplay seat or actor in the session.

A participant may have:

- an input owner,
- a team or faction,
- a pawn,
- a camera or cursor,
- a hand, deck, board seat, or selected entity,
- score or lives,
- UI ownership.

### Pawn

The runtime entity being controlled in the world.

A pawn is configured by data and composed from shared modules.

Important: a pawn is one possible participant embodiment, not the only one. Board games, card games, menu-driven tactics, and camera-as-player games should be able to use the participant/session model without creating fake character controllers.

### Actor

A runtime entity that can receive features, actions, targeting, feedback, or ownership.

Examples:

- character pawns
- enemies
- turrets
- board pieces
- cards
- interactable scene objects
- traps
- scripted encounter objects

Use `Actor` vocabulary when a system does not require movement-controller behavior.

### Action

A player, AI, rule, or system-driven intent that can be validated and resolved.

Examples:

- punch
- fire weapon
- cast ability
- play card
- move board piece
- select menu command
- trigger trap

Actions should be able to resolve through realtime delivery, turn-based menus, board/card rules, or scripted systems.

### Session

The rules around how participants join and how the game loop runs.

Examples:

- solo local,
- couch co-op,
- versus local,
- future online co-op,
- future networked versus.

### Game Mode

The scoring, win/loss, progression, respawn, and phase logic of the experience.

Examples:

- survival pickup mode,
- arena brawler mode,
- stage clear mode,
- score attack mode.

### Playfield

The movement and spatial rules of the playable space.

Examples:

- free 2D screen bounds,
- 2.5D lane depth,
- top-down arena,
- screen wrap,
- arena lock,
- stage progression rails.

### Module-Owned Capability

A reusable behavior package that can be enabled or disabled by visible Unity composition.

Examples:

- pickups,
- hazards,
- combo combat,
- climb,
- dodge,
- inventory,
- respawn,
- shared camera rules.

### Control Surface

The thing a participant directly manipulates.

Examples:

- character controller
- camera
- cursor
- selected board piece
- card hand
- menu selection
- faction command layer

Control surfaces should route through participant ownership and input/action contracts rather than forcing all games through pawn movement.

### Route Capability

A graph-readable capability family and its participant/control-surface expectations.

Examples:

- realtime character
- projectile combat
- turn/menu action
- board/card/tabletop
- camera/cursor control
- scoring
- procedural segments
- animation mapping

Route capabilities are composable. A game mode should be able to combine realtime character, projectile combat, side-scrolling playfield, scoring, and animation mapping without pretending those are separate frameworks.

## Target Data Model

The exact class names can change, but this is the preferred shape.

### SessionDefinition

Defines:

- local or network-ready session mode,
- participant limit,
- join policy,
- shared or split presentation rules,
- authority assumptions.

### ParticipantDefinition

Defines:

- participant role,
- team or faction defaults,
- HUD ownership,
- spawn policy,
- default pawn assignment rules.

### PawnDefinition

Defines:

- pawn prefab,
- movement profile,
- combat profile,
- traversal profile,
- presentation profile,
- default module-owned capability components and profiles.

### GameModeDefinition

Defines:

- score and objective rules,
- respawn rules,
- phase flow,
- hazard or pickup enablement,
- victory and failure conditions,
- required services.

Future mode definitions may also reference turn, action, board, card, or procedural generation profiles when the game is not pawn-controller-first.

### Reflected Route Capability Graph

Defines:

- capability family,
- supported control surfaces,
- participant embodiment requirement,
- required and optional runtime systems,
- companion and cautionary capability relationships,
- first-proof vocabulary and graph proof evidence.

Route capability data is inferred from `SessionDefinition`, `GameModeDefinition`, participants, pawns, module-owned components/profiles, scene evidence, and contracts/reflection. Authoring vocabulary may label or phrase the result, but it must not invent selectable capability paths, route-essential roles, runtime families, or proof ownership. Route capability data is reusable setup vocabulary, not an exclusive game-type label or a separate gameplay/runtime data asset.

### Reflected Setup Route

Defines:

- the route capabilities inferred from session, mode, participants, pawns, module-owned components/profiles, scene evidence, and contracts/reflection,
- setup notes from graph vocabulary, facts, and reflected contracts,
- validation for missing route evidence, pawn/non-pawn mismatch, invalid module-owned capability setup, and cautionary combinations.

Future wizards, sample generators, and setup validators should read the reflected route graph before creating or inspecting scene content.

### PlayfieldProfile

Defines:

- movement space model,
- depth or lane rules,
- screen bounds or wrap,
- arena lock rules,
- spawn regions,
- camera boundary relationship.

### CameraRigProfile

Defines:

- focus mode,
- framing,
- zoom behavior,
- shake tuning,
- participant group or per-participant target routing,
- split or shared camera preferences.

### InputProfile

Defines:

- action asset reference,
- control scheme expectations,
- rebinding rules,
- touch or gamepad presentation hints.

### Module-Owned Components And Profiles

Defines:

- direct component enablement on the prefab or scene object,
- module-specific profiles and tunables,
- data references for a reusable capability owned by that module.

### Future ActionDefinition

Defines:

- action id and display name,
- cost rules,
- targeting rules,
- execution timing,
- delivery style,
- resolution effects,
- animation and feedback signals.

This is the likely shared bridge for guns, projectiles, brawler moves, turn-based commands, tactical abilities, cards, board moves, and scripted interactions.

## Controller Direction

The 3D brawler pawn is fully decomposed. `Motor3D` is the composition root - it coordinates four focused sibling components with zero gameplay logic of its own:

- `Pawn3DInputModule` - all Input System binding; produces a `FrameInput` snapshot each frame
- `Pawn3DMovementComponent` - owns `Rigged3DMovementModel`, drives `CharacterController`, implements `IPawnMotor` and `IMovementModule`
- `Pawn3DTraversalComponent` - ledge detection, climb, hang, shimmy; implements `IPawnTraversalModule`
- `Pawn3DPresentationComponent` - Animator, billboard, land squash, debug HUD; implements `IPawnPresentationModule`

Mode-specific differences are applied through profiles and optional modules.

Core pawn identity stays Unity-native and visible on the prefab. A creator inspecting a 2D or 3D pawn prefab should see the core motor, input, movement, traversal, presentation, health, combat, interaction, feedback, status, and route-specific capability pieces directly when that route requires them. Optional traversal, guard, pickup, feedback, status, and interaction behaviors are module-owned components and profiles that query or modify the explicit sibling stack instead of hiding behind a central installer.

Ownership shorthand:

- siblings describe what a pawn is,
- module-owned capabilities describe what a pawn can do,
- participants describe who is driving,
- scene camera rigs describe what is watching.

`ParticipantDefinition.inputProfile` is the authored input source of truth. `ParticipantInputRouter` may observe Unity `PlayerInput` join/leave events and apply that profile to live `PlayerInput` instances, but it should not become a second input policy owner. Camera setup follows the same split: the scene Cinemachine rig is the live drafting table, `CameraRigProfile` stores gameplay focus intent plus saved reflected Cinemachine recipe values, and pawns expose targets or sockets while zones request profile transitions.

Camera ownership follows the same Unity-native rule:

- participants own view identity and assigned pawns,
- pawns expose `PawnCameraTarget` follow/look-at sockets when the pawn route needs explicit camera focus,
- `CameraRigProfile.focusMode` chooses Participant Group, Participant Pawns, Playfield Center, Explicit Scene Target, or Manual Cinemachine,
- `CameraRigProfile` stores supported reflected Cinemachine values with mapping/provenance,
- `CinemachineCameraRigController` routes the selected gameplay target into Cinemachine and applies the saved recipe,
- Cinemachine owns composition, damping, offsets, lens behavior, blend, follow, and look-at mechanics.

Normal profile application does not create missing Cinemachine components. Missing, unsupported, or stale mappings are validation evidence for the editor/authoring path, not hidden runtime repair.

Spawn services bind pawns to participants; they do not configure Cinemachine. Camera bounds are a separate service for visible-area-aware spawning, pickups, hazards, generated content, or screen-edge movement. Bounds do not prove that the camera can follow a pawn.

Current implementation note:

- the direct 2D and 3D pawn stacks are the supported authoring path.
- movement, input, presentation, traversal, combat, interaction, feedback, pickups, and status should expose feature-owned contracts when they are reusable modules.

Examples:

- a phone brawler may use touch input presentation plus lane-depth movement plus combo combat plus pickups,
- a survival arcade mode may use free-bounds movement plus dash plus pickup scoring plus hazards,
- a technical arena mode may use free movement plus richer combat plus score attack rules.

## Playfield Versus Camera

Do not treat movement bounds as a camera-only concern.

Preferred split:

- `PlayfieldProfile` owns playable-space rules,
- `CameraRigProfile` owns focus mode and saved Cinemachine recipe values,
- `PawnCameraTarget` owns pawn follow/look-at sockets,
- the camera may read playfield data when useful to keep the view inside the authored world,
- movement modules read playfield data directly for legal movement bounds,
- camera-visible bounds constrain pawn movement only when a pawn explicitly opts into screen-edge behavior.

This avoids forcing "aspect-bound movement" to mean "camera profile."

## Shared Features Versus One-Off Mode Layers

### Likely Shared

- participant roster,
- spawning and respawning,
- turn, phase, and action-selection primitives,
- targeting and action-resolution primitives,
- health and damage primitives,
- knockback,
- hitboxes and projectiles,
- guns, ammo, reload, spread, hitscan, and reusable projectile delivery,
- inventory and equipment foundations,
- card, deck, hand, board-space, and piece primitives where reusable,
- procedural generation contracts for authored chunks, sockets, budgets, seeds, and validation,
- pickup and score primitives,
- hazard foundations,
- camera service abstractions,
- animation signal mapping and Animator tooling,
- settings and save-backed configuration,
- input ownership and routing.

### Likely Mode-Specific Or Preset-Oriented

- exact combo grammar,
- exact fighting-game move list,
- exact turn order rules for one card game,
- exact board rules for one tabletop game,
- exact touch layout presentation,
- exact scoring formula,
- exact arena progression rules,
- exact hazard themes and authored content.

If a feature appears in more than one mode, treat that as a signal it should move toward shared.

## Preferred Unity And Ecosystem Foundations

Use engine-supported systems first where they fit well.

Preferred starting points:

- Unity Input System for participant input ownership and local multiplayer,
- `PlayerInput` and `PlayerInputManager` for local join and pairing flows,
- `ScriptableObject` assets for gameplay definitions and profiles,
- editor tooling for authoring surfaces,
- Cinemachine for higher-level camera authoring if camera complexity keeps growing.

Current implementation note:

- the package manifest declares Cinemachine, Netcode for GameObjects, and Unity Transport as package dependencies; NGO gameplay behaviour remains an opt-in route isolated behind `NeonBlack.Gameplay.Networking.asmdef`, with only narrow shared serialization DTOs allowed outside the Networking folder.

Avoid building custom replacements for these unless there is a clear documented limitation.

## Unity Authoring Maintainability Rules

Unity authoring stays maintainable when scene objects are thin runtime readers and authored assets own most design intent.

Prefer:

- `ScriptableObject` definitions for identity, relationships, and reusable setup choices
- `ScriptableObject` profiles for tuning, numbers, curves, effects, and presentation choices
- prefabs for reusable runtime object composition
- one bootstrap root per playable scene
- local inspectors that show field integrity without acting as PYS Authoring adapters
- validation that catches missing references before Play Mode

Avoid:

- hidden global state as the main authoring contract
- tag searches as the preferred player/participant lookup
- scene-only wiring that cannot be recreated from definitions and profiles
- expanding singleton managers when the session or participant model should own the behavior
- adding fields to large MonoBehaviours when a profile or module-owned capability would make the choice reusable

The practical test is simple: a designer or future developer should be able to inspect a scene root, follow the assigned assets, and understand why the runtime behaves the way it does. If they must inspect several static singletons or search for objects by tag to understand the scene, the authoring path is carrying maintenance debt.

## Current Risk Areas

The architecture is coherent enough for active route development, but several areas still need disciplined cleanup as new content arrives.

Highest-risk areas:

- runtime services still include narrow static query surfaces, especially participant lookup helpers, that should keep shrinking toward explicit lifetime-scope ownership and participant/session references
- some direct scene-facing flows still need participant-native proof in Play Mode
- `CameraOcclusionFader` and a few polling/ticking systems need hot-path allocation cleanup before content density grows
- several large MonoBehaviours and editor classes remain change hotspots
- the aggregate `NeonBlack.Gameplay` assembly can still hide accidental cross-domain coupling
- deeper lane validation still needs to grow beyond the first route/service layer

These are not reasons to restart the architecture. They are the next cleanup checkpoints that make the existing direction cheaper to maintain.

## Current Refactor Targets

### Target 1: Participant Model

Replace single-active-player assumptions with a participant roster model.

This includes removing design dependence on:

- one active player registry,
- one input receiver,
- one player prefab per mode,
- tag-based player discovery as the primary path.

### Target 2: Input Ownership

Move from single-player input handling toward per-participant input ownership.

This should support:

- one local participant,
- multiple local participants,
- future network-backed participant ownership.

### Target 3: Controller Decomposition

Pawn controllers should be composed from focused components with clear responsibilities. `Motor3D` coordinates focused 3D sibling components. `Motor2D` is the 2D motor surface and delegates movement, presentation, and input adaptation to focused 2D components.

### Target 4: Mode As Data

Move mode identity out of folders and into authored definitions.

Arcade and brawler should remain example assemblies of shared parts, with reusable learning captured as contracts, dependency reflection, validation rules, authoring vocabulary, and generic setup guidance rather than entries.

### Target 5: Documentation As Source Of Truth

Keep architecture, standards, setup, and verification docs current as code changes land.

Docs should describe the supported path directly. Historical notes belong outside active setup docs unless they protect active content or a supported public contract.

### Target 5.5: Single Runtime Composition Path

The long-term runtime service ownership model should have one primary composition root.

`GameplaySessionBootstrap` remains the supported scene entrypoint, but it should feed a clear service graph rather than becoming a second service container. `GameplayLifetimeScope` should be the durable owner for dependency registration. Static singleton accessors and direct-scene query helpers such as participant lookup should shrink toward narrow facades rather than becoming a second service-location model.

This matters because Unity scenes already have enough implicit state. The gameplay platform should not add several hidden service-resolution models on top of that.

### Target 6: Actor-Agnostic Expansion

Expand the platform so participant-owned play does not require a character controller.

This includes future support for:

- camera-as-player games,
- cursor/selector-driven games,
- tactics and menu selection,
- board pieces,
- cards, hands, decks, and zones,
- turn and phase systems,
- action/targeting resolution shared by realtime and rules-driven games.

### Target 7: Capability Families Before Genre Forks

New feature work should start from reusable capability families rather than genre folders.

Examples:

- action and targeting before "RPG combat",
- projectile delivery before "shooter mode",
- board spaces and legal moves before one named board game,
- card zones and action resolution before one specific card game,
- authored segment generation before one procedural side-scroller.

## Anti-Goals

Do not:

- create a second monolithic framework under a new name,
- fork every shared system into mode-specific copies,
- keep adding singletons that imply one active player forever,
- force every game type through a pawn or character-controller model,
- hide board/card/turn/action rules inside one-off UI scripts,
- let docs drift behind the current design,
- replace mature engine packages with custom code for style reasons alone.

## Migration Mindset

This architecture should be reached incrementally.

The preferred path is:

- stabilize language and standards,
- define target shapes,
- refactor seams with high leverage,
- keep active setup guidance focused on the supported path,
- remove unsupported paths from active guidance when they do not protect active content or a public contract.
