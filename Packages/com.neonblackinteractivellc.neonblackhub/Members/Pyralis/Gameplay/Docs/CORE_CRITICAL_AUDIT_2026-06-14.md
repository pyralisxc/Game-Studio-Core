# Pyralis Core Critical Audit - 2026-06-14

This audit is a pressure test of the current Pyralis gameplay core and authoring spine. It is intentionally critical. The goal is not to restart the architecture; the goal is to identify the remaining places where the current setup becomes hard to explain, duplicates ownership, hides runtime behavior, or would fail to scale from a small proof into higher-quality games inside the currently supported lanes.

## Executive Read

Pyralis is no longer blocked by a missing architecture. The main architecture now makes sense:

```text
Contracts and reflected code structure
  -> dependency tree
  -> resolved setup graph
  -> authoring projections, validators, inspectors, and docs
```

The runtime side also has a coherent spine:

```text
GameplaySessionBootstrap
  -> SessionDefinition
      -> GameModeDefinition
      -> ParticipantDefinition[]
          -> optional PawnDefinition
              -> pawn prefab
              -> profiles
              -> feature modules
```

The remaining risk is that several domains still have "almost one source of truth" rather than one boring source of truth. These are exactly the places where authoring starts sounding vague after the first setup chain is wired.

Highest-risk areas:

- Runtime service ownership is improved, but `GameplaySessionBootstrap`, `PyralisGameplayLifetimeScope`, scene scans, auto-created services, and string-resolved networking services still overlap.
- Scene flow is useful but split across `SceneLoader`, `SceneFader`, `SceneNavigator`, `MainMenuManager`, loading screens, and older 2D game-flow code.
- Multi-prefab and multi-scene workflows are not yet proven as clean authoring routes. A single pawn prefab path is understandable; variants, skins, loadouts, AI variants, scene changes, and content loading are not yet equally explainable.
- Route parity is honest but incomplete. `Sprite2D`, `Billboard2_5D`, `Rigged3D`, non-pawn tabletop, local join, and network MVP all have foundations, but most are still `Guided Needs Proof`.
- Authoring knows a lot, but some setup truth still comes from validators, grammar, route analysis, contracts, and scene evidence in ways that can make Overview/Guide feel less smart than the Inspector.
- Some large feature files are reasonable today, but they are future maintenance hotspots because they mix runtime behavior, setup assumptions, and presentation concerns.

## What Is Working

### The central model is sound

`SessionDefinition`, `GameModeDefinition`, `ParticipantDefinition`, `PawnDefinition`, profiles, feature modules, contracts, dependency tree, graph, and validators now line up conceptually. The setup chain is explainable without saying "run this preset."

### Participant ownership is moving the right way

`ParticipantDefinition.inputProfile` is the right owner for player/seat/faction/hand/cursor input. Keeping input off `PawnDefinition` protects non-pawn routes and avoids a common Unity framework trap where "player" always means "character prefab."

### The authoring graph is the right target

The current graph-backed model is the right abstraction. The tabs should remain projections:

- Overview: next 1-3 moves.
- Intent: desired route focus and validation lens.
- Guide: full intent-filtered route checklist.
- Map: current scene/setup reality and concrete scene issues.
- Validate: graph integrity and developer evidence.
- Facts: dictionary/provenance.

When the graph is missing context, the fix should normally be dependency reflection, contract metadata, or validator evidence, not a tab-specific workaround.

### Route readiness is being tracked honestly

`RUNTIME_PARITY_MATRIX.md` correctly marks most lanes as `Guided Needs Proof` or `Foundation Only`. That honesty matters. The framework can compile and still not be beginner-prototype-ready for a route.

## Critical Findings

### 1. Runtime service ownership is still not boring enough

Relevant files:

- `Features/Platform/Session/GameplaySessionBootstrap.cs`
- `Features/Platform/Composition/PyralisGameplayLifetimeScope.cs`
- `Features/Platform/Composition/PyralisRuntimeFeatureServicePolicy.cs`
- `Features/Characters/Runtime/Shared/Participants/ParticipantRosterService.cs`
- `Features/Characters/Runtime/Shared/Services/ParticipantSpawnService.cs`
- `Features/Input/ParticipantInputRouter.cs`

Current shape:

- `GameplaySessionBootstrap` is the visible Unity entry point.
- `PyralisGameplayLifetimeScope` is the VContainer composition root.
- Bootstrap still auto-creates missing service children.
- Bootstrap resolves networked service types by string.
- Lifetime scope registers core services, then registers feature service groups from route evidence or loaded scene components.
- Some services still support direct serialized references, injection, hierarchy discovery, and scene scanning.

Why this is a red flag:

This is functional, but it is still a lot to explain. A beginner can understand "place Bootstrap and assign SessionDefinition." A developer maintaining the engine has to understand when services are authored, auto-created, found in children, found in loaded scenes, registered by policy, injected into scene roots, or instantiated by string name.

Recommended direction:

- Keep `GameplaySessionBootstrap` as the Unity-facing entry point.
- Keep `PyralisGameplayLifetimeScope` as the service graph owner.
- Make auto-created core services an explicit bootstrap policy with graph-visible evidence: `Authored`, `AutoCreateCore`, `Missing`, or `Custom`.
- Replace string-resolved network service creation with an explicit networking installer/adapter seam when the network lane is hardened.
- Treat scene scanning in lifetime scope as `MigrationLegacy` or `DevFallback` unless a route contract explicitly declares scene component discovery as supported.
- Keep feature service registration driven by contracts, feature modules, and route evidence first; scene components should be compatibility evidence, not the normal source of truth.

### 2. Platform files moved, but assembly ownership still leaks old naming

Relevant files:

- `Features/Platform/Session/GameplaySessionBootstrap.cs`
- `Features/Characters/Runtime/NeonBlack.Gameplay.Characters.asmdef`
- `Features/Characters/Runtime/Shared/Participants/*`
- `Features/Characters/Runtime/Shared/Services/*`

Current shape:

`GameplaySessionBootstrap` now lives in `Features/Platform/Session`, which is semantically better, but its namespace remains `NeonBlack.Gameplay.Characters`. Participant roster/spawn services also still live under the Characters assembly.

Why this matters:

The folderbase now says "platform," while the assembly still says "characters." That is acceptable as a staged move to avoid breaking Unity references, but it should not become permanent invisible debt. Platform/session services are not character gameplay.

Recommended direction:

- Defer assembly migration until after current proof work is stable.
- Track a future assembly boundary cleanup: platform/session services should eventually live in a Platform or Composition assembly, with `Characters` depending on platform contracts rather than owning participant services.
- Do not move everything at once; this is a GUID/reference-sensitive Unity migration.

### 3. Scene flow has too many overlapping surfaces

Relevant files:

- `Core/SceneLoader.cs`
- `Core/SceneNavigator.cs`
- `Core/Navigation/UI/SceneFader.cs`
- `Core/Navigation/UI/LoadingScreenController.cs`
- `Core/Navigation/UI/MainMenuManager.cs`
- `Features/GameFlow/2D/GameManager.cs`
- `Data/Definitions/GameModeDefinition.cs`

Current shape:

Pyralis has direct scene loading, faded loading, a static navigator, a menu manager, loading screen controller, scene fader, scene loader, level registry/session helpers, and older 2D game flow. This gives flexibility, but it is hard to explain as one route.

Red-flag question:

"If I have a menu scene, loading scene, gameplay scene, and restart/return flow, which object owns navigation?"

Right now the answer depends on which doc or component you are looking at. That is the smell.

Recommended direction:

- Define one canonical `ISceneNavigator` authored route for the Game Shell MVP.
- Keep `SceneFader` or a future scene-flow service as the recommended concrete owner.
- Demote `SceneNavigator` static helper to utility-only, not an authoring route.
- Make `GameModeDefinition.gameplayScene` and shell/menu components connect through one scene-flow contract.
- Add validation for Build Settings scene names and duplicate scene-flow owners.
- Add a proof path for boot/menu/loading/gameplay/restart/return before calling Game Shell ready.

### 4. Multi-scene and additive scene behavior is not fully proven

Current shape:

`PyralisGameplayLifetimeScope` can inject loaded scene roots and scans all loaded scenes for components. Bootstrap can persist with `DontDestroyOnLoad`. Scene guard handles duplicate `EventSystem` and `AudioListener`.

Open questions:

- What happens when a persistent bootstrap enters a new gameplay scene with its own bootstrap?
- Which scene owns the active `SessionDefinition` after a transition?
- Does a loaded scene get injected before feature components need services?
- How do we validate additive scenes, split content scenes, UI overlay scenes, and scene-loaded feature services?
- How do persistent services cleanly reset between runs, scenes, and tests?

Recommended direction:

- Add a scene-lifecycle proof route before deeper scene work.
- Represent active session ownership through explicit graph evidence.
- Add authoring validation for duplicate bootstraps, duplicate lifetime scopes, duplicate scene-flow owners, and mismatched session assets across loaded scenes.
- Treat additive scene support as a named feature lane, not an accidental side effect of scene scanning.

### 5. Multi-prefab and variant ownership is thin

Relevant files:

- `Data/Definitions/PawnDefinition.cs`
- `Data/Definitions/ParticipantDefinition.cs`
- `Features/Characters/Runtime/Shared/Services/ParticipantSpawnService.cs`
- `Features/Spawning/3D/PlayerSpawner.cs`
- `Features/Spawning/3D/EnemySpawner.cs`
- `Features/Composition/ActorFeatureHost.cs`

Current shape:

`PawnDefinition` owns one `pawnPrefab`. `ParticipantDefinition` owns one `defaultPawn`. Spawners and enemy systems have their own prefab arrays or spawn rules. Feature modules may instantiate runtime prefabs under pawns.

This is simple and good for the first proof. It gets foggy for:

- player character select
- alternate skins
- AI variants
- difficulty-based enemy prefab variants
- weapon or loadout-driven prefab variants
- network prefab registration for multiple pawn archetypes
- scene-specific spawn pools
- Addressables or streamed prefab content

Recommended direction:

- Keep one `PawnDefinition.pawnPrefab` as the first proof path.
- Add a future `PawnVariantSet` or loadout/skin policy only when a route needs it.
- Before adding new data assets, decide whether variants belong to participant, pawn, game mode, inventory/loadout, or spawn policy.
- Authoring should distinguish "one first proof prefab" from "production variant selection."
- Add validation that can explain multiple prefab candidates without pretending the first proof needs them.

### 6. Input ownership is much cleaner, but input action mapping is still a high-friction setup point

Relevant files:

- `Data/Profiles/InputProfile.cs`
- `Data/Definitions/ParticipantDefinition.cs`
- `Features/Input/ParticipantInputRouter.cs`
- `Features/Input/2D/Motor2DInputAdapter.cs`
- `Features/Input/2D/PlayerInputHandler.cs`
- `Editor/Authoring/Surfaces/Inspectors/InputProfileEditor.cs`
- `Editor/Authoring/Spine/Validation/PyralisSceneReadinessValidator.cs`

Current shape:

The owner is right: `ParticipantDefinition.inputProfile`. The editor now has sync from Input Actions, and readiness validation checks maps/actions.

Remaining setup risk:

Input is where Unity-native flexibility and Pyralis semantic roles meet. A beginner sees an Input Action Asset, PlayerInput, PlayerInputManager, InputProfile, action rows, pawn input modules, local join, and auto-joined participants. That is a lot of pieces.

Recommended direction:

- Continue treating `InputProfile` as participant-owned.
- Keep the Input Action sync, but surface its result in Map/Validate as concrete evidence: action asset found, action map found, Move row found, row reaches pawn input module.
- Do not require `PlayerInputManager` for 1P. Keep it strictly local-join.
- Add route-specific expectations: movement first needs Move; combat route needs Attack; aim/projectile route may need Look/Aim/Fire; tabletop may need Select/Confirm/Cancel or UI input.
- Avoid adding input fields back to pawn/profile owners unless they are component-local adapters.

### 7. Camera and playfield are improved but still need route-level proof

Relevant files:

- `Data/Profiles/CameraRigProfile.cs`
- `Data/Profiles/PlayfieldProfile.cs`
- `Presentation/Camera/CinemachineCameraRigController.cs`
- `Features/Characters/2D/Pawn2DMovementComponent.cs`
- `Features/Platform/Session/GameplaySessionBootstrap.cs`

Current shape:

The intended split is right:

- `PlayfieldProfile` owns legal movement space.
- `CameraRigProfile` owns framing.
- Camera-visible bounds constrain movement only when explicitly enabled.

Remaining risk:

Camera is still a place where runtime feel and setup validation blur. A user may ask "Why can't I move?" and the cause could be Rigidbody2D, movement profile, playfield bounds, camera bounds opt-in, gravity/jump mode, input map, collider, or spawn.

Recommended direction:

- Make Map own scene-specific camera/playfield issues.
- Make Validate own graph/dependency proof that movement, camera, and bounds are connected.
- Add a route proof that intentionally tests 2D free movement, 2D bounded movement, camera framing, and explicit camera-bound movement.
- Keep Cinemachine as the high-level camera package unless a route proves a limitation.

### 8. 2D, 2.5D, and 3D parity is not yet real enough to market as equal

Current route matrix state:

- `Sprite2D`: strong foundation, still needs proof.
- `Billboard2_5D`: official lane, relies heavily on 3D stack plus presentation settings.
- `Rigged3D`: strong movement/traversal/combat foundation, still needs explicit beginner proof.

Important finding:

The lanes are not equally mature just because they all have enum values and docs. `Sprite2D` has a focused 2D stack and recent movement proof. `Rigged3D` has a rich 3D stack. `Billboard2_5D` is a presentation lane that may still need clearer runtime and authoring evidence.

Recommended direction:

- Keep `Billboard2_5D` honest as "3D logic with billboard presentation" unless it gains distinct movement/camera rules.
- Add parity tests and proof docs around the same first loop: spawn, input, movement, camera, presentation, health/damage or interaction, optional projectile/combat, HUD.
- Validate unsupported feature lanes from contracts rather than allowing vague "may work" behavior.

### 9. Tabletop/no-pawn is promising but still feels like a foundation, not a finished authoring route

Relevant files:

- `Core/Rules/Board/*`
- `Data/Definitions/Rules/*`
- `Features/Tabletop/*`
- `Core/Actions/*`
- `Data/Definitions/GameModeDefinition.cs`

Current shape:

Board state, move policies, terminal conditions, action queue, turn state, grid presenter, and selection bridge exist. This is a real core rules spine.

Missing for high-quality no-pawn games:

- card/deck/hand/zones
- richer terminal conditions
- directional/owner-relative movement
- action menus and confirm/cancel flows
- undo/preview/selection state
- board/tactics camera proof
- save/load for board state
- AI hooks
- multiplayer ownership
- authoring guide that gets a beginner from empty scene to one accepted/rejected move with visible state change

Recommended direction:

- Finish one tiny no-pawn route proof before adding named game packs.
- Keep legal rules in rules assets/services, not UI presenters.
- Use authoring graph evidence to explain why no `PawnDefinition` and no spawn point is correct.

### 10. Combat/projectiles are reusable but not yet fully route-unified

Relevant files:

- `Features/Combat/*`
- `Data/Definitions/Combat/*`
- `Features/Characters/PawnCombatBehaviour.cs`
- `Features/Characters/2D/PawnCombatBehaviour2D.cs`
- `Features/Characters/PawnProjectileModule.cs`

Current shape:

Projectile planning and 2D/3D launchers are in much better shape than older character-specific firing. Melee combat and health are still partly pawn/controller-shaped.

Red-flag questions:

- How does a trap, turret, board piece, card, or menu action fire a projectile without pretending to be a pawn?
- Where does ammo live when multiple weapons, inventory, or loadout are introduced?
- Does combat setup validation explain the difference between melee hitboxes, projectile launchers, weapon data, combat actions, and action definitions?

Recommended direction:

- Keep projectile execution actor-agnostic.
- Build future weapon/ammo/loadout policy on action context, participant/faction ownership, and source transform, not pawn-only state.
- Add validation for missing launcher, projectile definition, fire mode, impact definition, and lane mismatch.
- Do not deepen combo grammar until the first brawler route proves setup, input, animation, hitbox, damage, and feedback together.

### 11. RPG scope is broad and needs route-gated containment

Relevant files:

- `Core/Rpg/*`
- `Data/Definitions/Rpg/*`
- `Features/Rpg/*`
- `Docs/RPG_SYSTEMS_ROADMAP.md`

Current shape:

RPG has a lot of foundation: inventory, equipment, skill trees, quests, dialogue, hub, persistence, open-zone state, and sample runtime code.

Risk:

RPG can easily become a second platform inside the platform. The code volume is already significant, while authoring readiness is marked `Foundation Only`.

Recommended direction:

- Keep RPG systems participant-owned and actor-agnostic.
- Do not let RPG become mandatory in the session spine.
- Register RPG services only when route evidence asks for them.
- Require route completeness before treating RPG as product-ready: runtime, authoring, guidance, validation, proof.
- Make save/load contracts reusable for all routes, not only RPG.

### 12. Large files are manageable now but likely future hotspots

Current hotspots by line count include:

- `Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraphProjection.cs`
- `Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraphBuilder.cs`
- `Editor/Authoring/Spine/Validation/PyralisSetupFlowValidator.cs`
- `Editor/Authoring/Spine/Validation/PyralisSceneReadinessValidator.cs`
- `Features/Hazards/2D/HazardSpawner.cs`
- `Features/Characters/2D/Pawn2DMovementComponent.cs`
- `Features/Input/2D/PlayerInputHandler.cs`
- `Features/Combat/UI/WorldHealthBar.cs`
- `Features/Hazards/2D/Hazard.cs`
- `Features/Spawning/3D/PlayerSpawner.cs`
- `Features/GameFlow/2D/GameManager.cs`

Large files are not automatically bad. The authoring graph files may deserve to stay large because they centralize logic. The runtime files are riskier when they mix gameplay, setup assumptions, presentation, pooling, and debug behavior.

Recommended direction:

- Do not split files for size alone.
- Split only when a smaller policy/model/helper can be named by responsibility and tested.
- Keep authoring central if centralization avoids duplicate tab logic.
- Watch for code that combines setup validation with runtime behavior; that should usually become contract or validator evidence.

## Folderbase Findings

Current top-level package folders are sensible:

- `Core`
- `Data`
- `Editor`
- `Features`
- `Networking`
- `Presentation`
- `Tests`
- `Docs`

Remaining folderbase concerns:

- `Features/Characters` still owns participant/session services by assembly, even as platform/session concepts move out.
- `Features/GameFlow/2D` and `Core/Navigation/UI` overlap in scene/menu flow responsibilities.
- `Features/Respawn` is empty and should be removed if still empty after the current cleanup branch.
- Some `Runtime/Shared` folders are legitimate, but they need discipline so "shared" does not become a new junk drawer.
- Feature-local editor authoring is okay for field-local guidance, but route truth must remain contracts/dependency tree/graph/validators.

## Authoring-Specific Findings

### Overview and Guide need sharper graph projection, not more copy

If Overview says setup is current while the Inspector knows fields are missing, the graph is not compiling enough field evidence. Fix the evidence path:

```text
serialized fields / contracts / validators / scene evidence
  -> graph evidence
  -> Overview and Guide
```

Do not patch Overview with bespoke checks.

### Intent should remain a steering wheel

Intent should not create assets, mutate setup silently, or decide a recipe. Its correct role is:

```text
user chooses desired route focus
  -> reflected capability descriptors and axioms filter graph priorities
  -> validators expose stricter or looser expectations
  -> Overview and Guide show the relevant next setup tissue
```

Current hardcoded axiom/capability grouping is acceptable as base vocabulary, but future feature-specific intent meaning should come from contracts/reflected descriptors.

### Map versus Validate split is now clearer

Recommended durable split:

- Map owns current Unity reality: scene objects, assigned assets, missing fields, prefab component issues, duplicate cameras/listeners, physical setup problems.
- Validate owns graph integrity: proof blockers, dependencies, source origins, contract coverage, unsupported lanes, graph edges, proof reachability.

This keeps beginner-facing "what is wrong in my scene?" separate from developer-facing "why did the graph decide this?"

## AAA-Capacity Gaps Inside Current Lanes

Pyralis should not claim AAA feature depth yet. It can claim a growing shared-core foundation. To support high-quality games inside current lanes, these areas need deliberate capability growth:

### Game Shell

Needed:

- robust boot/menu/loading/gameplay/return flow
- settings, credits, pause, restart, and scene validation
- Build Settings checks
- save slot and run-state handoff
- localization-ready text surfaces

### Pawn Action

Needed:

- lane-equal movement proof across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`
- local multiplayer join proof
- camera proof
- hitbox/projectile/health feedback proof
- animation mapping support for imported controllers
- HUD proof
- respawn/checkpoint proof

### Tabletop/Rules

Needed:

- no-pawn route proof
- action menu and selection UX
- legal move preview/confirm/cancel
- richer board policies and terminal conditions
- card/deck/hand/zones
- AI and save/load hooks

### Networking

Needed:

- host/client proof
- network prefab validation for multiple pawn prefabs
- ownership transfer and authority docs
- movement/projectile replication strategy
- replicated health/score/state
- lobby/session layer later

### Content/Pipeline

Needed:

- multi-prefab variants
- Addressables or equivalent content loading strategy
- export footprint checks
- route-specific build validation
- sample/proof scenes kept disposable and separate from authoring truth

## Recommended Next Phases

### Phase 1 - Service ownership lock

Goal: make runtime ownership easier to explain.

Tasks:

- Classify every bootstrap/lifetime-scope service path as `Authored`, `AutoCreateCore`, `RouteDriven`, `DevFallback`, or `MigrationLegacy`.
- Make that classification visible in graph evidence.
- Reduce lifetime-scope scene scanning where route evidence or contracts can decide service groups.
- Decide the future explicit networking installer seam.

Done when:

- A developer can explain service ownership from `GameplaySessionBootstrap` to VContainer in one paragraph.
- Authoring can tell whether a service is authored, auto-created, missing, or route-driven.

### Phase 2 - Scene-flow and multi-scene proof

Goal: make game shell setup one route, not several possible stories.

Tasks:

- Pick the canonical scene navigator route.
- Validate Build Settings scene names and duplicate scene-flow owners.
- Prove boot/menu/loading/gameplay/restart/return.
- Add graph evidence for active session across scene transitions.

Done when:

- Game Shell can move from `Guided Needs Proof` toward `Ready`.

### Phase 3 - Multi-prefab and variant policy

Goal: make one-prefab proof stay simple while production variants have a clean future owner.

Tasks:

- Document first-proof prefab versus production variants.
- Identify whether variants belong to participant, pawn, game mode, inventory/loadout, spawn policy, or Addressables.
- Add validation for multiple pawn archetypes and network prefab registration.

Done when:

- A two-character or two-enemy setup can be explained without custom scene scripts.

### Phase 4 - Lane parity proofs

Goal: prove the official runtime lanes honestly.

Tasks:

- Sprite2D: spawn, input, movement, camera, animation/presentation, damage/interact, optional projectile/HUD.
- Billboard2_5D: same loop, with billboard-specific presentation proof.
- Rigged3D: same loop, with Animator mapping proof.
- Tabletop/no-pawn: one accepted/rejected move, turn advance, visible state change.

Done when:

- The runtime parity matrix can mark a lane `Ready` with evidence.

### Phase 5 - Authoring evidence depth

Goal: make Overview and Guide feel as smart as the best Inspector validation.

Tasks:

- Push missing-field and prefab-component checks into graph evidence.
- Keep Map scene-specific and Validate graph-specific.
- Expand contract metadata only where reflection cannot infer the setup meaning.
- Add source-contract tests that prevent tab-specific duplicate readiness logic.

Done when:

- A first-time user can wire the session chain, pawn prefab, input, and camera while Overview/Guide keep showing the next 1-3 useful moves.

### Phase 6 - Content and export readiness

Goal: prepare the engine for real projects, not only proof scenes.

Tasks:

- Decide Addressables/content-loading policy.
- Add export footprint checks once representative route exports exist.
- Make sample/proof folders clearly disposable.
- Validate route assets do not reference unrelated feature families.

Done when:

- A movement route does not accidentally pull RPG, tabletop, networking, or heavy UI assets into builds.

## Audit Verdict

Current health:

- Architecture: strong and coherent.
- Runtime ownership: improved, still too layered in service composition.
- Authoring spine: correct direction, needs deeper field/prefab evidence to feel reliably smart.
- Folderbase: mostly good, with remaining platform/characters assembly debt and scene-flow overlap.
- Supported lane parity: foundations are broad, readiness is not equal yet.
- Product readiness: beginner-prototype-ready for isolated first proofs is close; broad route-ready is still gated by proof scenes, scene flow, and lane parity.

Recommended status:

`Checkpoint reached`, not `Phase complete`.

The next cleanup should not be another architecture redesign. It should be a lock-in pass that makes ownership, scene flow, prefab variants, and lane evidence boring enough that the authoring graph can explain them without tab-specific rescue logic.

