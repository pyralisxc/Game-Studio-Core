# Current Archetype Quality Audit - 2026-05-23

## Audit Lens

This audit grades the currently advertised Pyralis gameplay lanes against production-quality expectations: modular runtime seams, data-driven customization, inspector authoring depth, testability, deterministic behavior where practical, scene validation, and ability to scale from solo/local prototype use into larger teams, AI, multiplayer, and richer content.

This is not a content-quality grade. It is a code and tooling maturity grade.

## Overall Grade

**B for platform direction, B- for shipped mechanics maturity.**

The project has a credible platform spine: `GameplaySessionBootstrap`, participant services, runtime pattern assets, setup-profile authoring, broad guided inspectors, data definitions, and a growing test suite. The strongest work is the authoring/navigation layer and the 3D pawn/combat direction.

The main gap is no longer basic inspector coverage or direct singleton consumption in the core gameplay paths. The remaining gap is depth: setup validation now proves the first concrete scene-service layer for scoring, projectile launchers, gameplay state, camera bounds, and tabletop honesty, but prefab internals and lane-specific repair actions still need to deepen. Board/card/tabletop and turn/menu action now have starter board/action assets, but still need selection UI and sample scene wiring. Projectile combat uses the shared planner/launcher path for pawn firing through a clean authored `ProjectileDefinition` contract, with PlayMode proof for pooling/contact; enemy, trap, card, and project-owned action firing surfaces remain follow-up work.

Progress note: the first modernization pass after this audit removed the known `Camera.main` runtime lookups from 3D pawn movement, floating feedback/damage numbers, billboard-facing presentation, actor animation billboarding, enemy facing, hazard popups, camera shake, and the Cinemachine rig lens target path. Those systems now expose explicit camera fields/setters and inspector validation.

Progress note: the follow-up service pass removed active runtime singleton consumers such as `GameManager.Instance`, `SceneLoader.Instance`, `SceneFader.Instance`, `SettingsManager.Instance`, `TimeManager.Instance`, `CameraShake.Instance`, `LeaderboardManager.Instance`, `DamageNumberSpawner.Instance`, and `PlayerRegistry.Player/Motor2D` from Gameplay runtime code. `SceneGuard` remains the only intentional runtime scene-wide discovery exception, because it cleans duplicate `EventSystem` and `AudioListener` objects during scene transitions. `LeaderboardManager` no longer exposes a public singleton surface.

Progress note: the setup-flow validator now maps selected runtime patterns to first-layer scene requirements. Scoring/objective routes require both `GameModeDefinition.enableScore` and an `ISessionScoreService`; projectile routes require a `ProjectileLauncherBase`; pawn/scoring routes recommend an `IGameplayStateReader`; camera/playfield routes recommend an `ICameraBoundsProvider`; board/card/tabletop routes now verify the built-in board, move policy, terminal condition, turn order, and board-move action claims supplied by the tabletop starter pack. The deeper follow-up now also validates pawn prefab internals beyond `PawnRoot` by requiring `IPawnMotor` and `IPawnPresentationModule`, and surfaces unverified `RuntimePatternDefinition.requiredRuntimeSystems` strings in setup flow instead of silently ignoring them. Runtime-system claim verification now lives in `PyralisRuntimeSystemClaimResolver`, giving future feature slices one catalog to extend instead of growing inspector-local string checks.

Progress note: the 2026-05-24 first hardening slice removed the immediate pooling/destruction conflict inside `Projectile` by releasing through `ProjectilePoolHandle` when present. The follow-up projectile unification slice routed pawn 2D and 3D ranged/thrown weapon fire through `ProjectileFireRequest -> ProjectileFirePlanner -> ProjectileLauncher2D/3D`. The clean-break slice removed legacy inline projectile fields from `WeaponData`; ranged and thrown weapons now require `WeaponData.projectileDefinition` plus an optional `FireModeDefinition`.

Progress note: the launch-readiness cleanup pass tightened authoring truth before prefab/scene work: `Motor2D` no longer double-owns profile application beside the dedicated 2D components, projectile definition inspectors validate prefab runtime-body and physics-lane compatibility, fire-mode guidance now labels magazine/cooldown data as firing-source-owned or optional, starter authoring is named `Pawn Starter Pack`, and local multiplayer docs now explicitly distinguish PlayerInputManager local join from NGO networking.

Progress note: the foundation-ready pass moved local/offline ownership defaults into Core so fresh local Unity testing does not require a main Gameplay dependency on the optional Networking assembly. It also added runtime tests around projectile pooling/contact, respawn service resolution, duplicate seat reassignment, overflow spawn placement, and setup-flow validation for malformed runtime patterns. The Pawn Starter Pack no longer ships an empty Player 2 prefab or broken companion references, and its input, presentation, and animation profiles now match the included starter assets.

## AAA-Style Rubric

| Area | Expectation |
|---|---|
| Runtime modularity | Gameplay systems depend on explicit services, context, definitions, and interfaces rather than global scene objects. |
| Authoring | Inspector explains what to assign, validates required dependencies, offers safe repair actions, and keeps customization visible. |
| Customization | Designers can tune behavior through profiles/assets without editing code or following hidden string conventions. |
| Testability | Core mechanics can be exercised outside Play Mode where practical, with focused tests for edge cases. |
| Scalability | Systems work for N participants, AI, future networking, and non-pawn control surfaces when the pattern claims them. |
| Runtime safety | Missing optional collaborators degrade safely; required collaborators fail early with actionable validation. |

## Lane Grades

| Lane | Grade | Status |
|---|---:|---|
| Shared session, participant, setup route | **A-** | Real platform spine with route-aware scene-service validation; ownership still spans bootstrap, VContainer, and some persistent service implementations. |
| Guided authoring and inspector setup | **A-** | Concrete Gameplay `MonoBehaviour`/`ScriptableObject` surfaces now have guided editor coverage plus first-layer route/service checks protected by tests. |
| Realtime 3D pawn / brawler movement | **B** | Good pure movement model/profile path with explicit camera and optional collaborator handling; needs deeper scene repair actions and play-mode proof. |
| 2D arcade pawn / pickup / hazard loop | **B-** | Playable and performance-aware; state, bounds, hazard outcome, pickup award/spawn, and feedback now route through explicit seams, but the lane still needs scene-readiness validation and tests. |
| Melee combat / combo definitions | **B-** | Data-driven combo path exists; hitbox zones and coroutine timing are still brittle. |
| Projectile / gun combat | **B+** | Definitions, planner, launcher path, pawn firing integration, pooling lifecycle, prefab authoring validation, and PlayMode projectile coverage now align; still needs enemy/trap/card firing migration. |
| Scoring / objectives | **B-** | Useful participant score service and explicit leaderboard service path; objectives/resources/victory rules are not yet a real capability layer. |
| Camera / cursor control | **C+** | Camera rig/profile authoring and explicit camera references exist; cursor/camera-as-control-surface runtime is not yet deep. |
| Turn/menu action | **C-** | `ActionDefinition` and target rules exist; no shared turn order, action queue, command UI, or resolver runtime yet. |
| Board/card/tabletop | **C** | Starter pack now creates board, pieces, move policy, turn order, terminal conditions, and a board-move action; still needs selection UI and sample scene wiring. |

## Key Findings

### 1. The setup promise is stronger than the runtime guarantee

`GameplaySessionBootstrap` is the right scene entry point and creates/registers session, roster, spawn, input, scene, time, and camera services (`Features/Characters/GameplaySessionBootstrap.cs:44-106`). Its inspector builds a route-aware setup flow (`Editor/GameplaySessionBootstrapEditor.cs:24-35`) and the validator checks session, mode, setup profile, runtime patterns, participants, pawns, spawn points, camera rig, input manager, playfield, scoring, score service, projectile launcher, gameplay state service, camera bounds service, and tabletop contract honesty (`Editor/PyralisSetupFlowMonitor.cs`).

That is good authoring infrastructure. The first route-service gap is now closed for the currently concrete shared services: scoring routes require `GameModeDefinition.enableScore` and `ISessionScoreService`, projectile routes require `ProjectileLauncherBase`, and camera/playfield/state-heavy routes recommend their service contracts. Pawn prefabs are now checked for `PawnRoot`, `IPawnMotor`, and `IPawnPresentationModule`. `RuntimePatternDefinition.requiredRuntimeSystems` is still a string list (`Data/Definitions/RuntimePatternDefinition.cs:21-28`), but setup flow now surfaces required claims that are not covered by known bootstrap services, pawn validation, scoring validation, or projectile-launcher validation through a central resolver. The remaining gap is deeper lane validation: projectile pool compatibility, hitbox sizing, action-target UI, and project-owned tabletop services still need dedicated checks.

### 2. 3D pawn movement has the right architecture, and the first runtime safety defect is cleaned up

`Pawn3DMovementComponent` owns a pure `BrawlerMovementModel`, implements `IPawnMotor` and `IMovementModule`, applies movement/traversal profiles, and exposes movement state to sibling systems (`Features/Characters/Runtime/Shared/Components/3D/Pawn3DMovementComponent.cs:24-37`, `282-305`). That is the right modular direction.

The previous null-safety and global-camera issues in this lane have been addressed: `Pawn3DMovementComponent` now treats knockback as optional, exposes an explicit movement camera, and no longer resolves camera-relative movement through `Camera.main`. The remaining work is deeper scene-contract validation and richer prefab repair actions so setup routes can prove the correct movement, presentation, combat, traversal, and camera collaborators are present.

### 3. 2D arcade systems are feature-rich and now use explicit seams

The 2D pawn stack has useful tuning and performance choices: explicit acceleration/deceleration, screen-wrap/bounds, dead zones, dash, centralized collectible ticking, pooling, spawn-immunity, and HashSet active lists (`Pawn2DMovementComponent.cs:17-39`, `CollectibleSpawner2D.cs:65-83`).

The earlier weak point was setup coupling to `GameManager.Instance`. That has been migrated to explicit seams: `Pawn2DMovementComponent`, `CollectibleSpawner2D`, and `HazardSpawner` read `IGameplayStateReader` and `ICameraBoundsProvider`; hazards route death/outcome through `IHazardOutcomeSink`; pickups route award/spawn behavior through `IPickupAwardSink`, `IPickupSpawnSurface`, and `IPickupBurstSpawnSurface`. The remaining risk is not hidden global state, but whether the setup route can prove those collaborators are present before a creator hits Play.

### 4. Projectile combat now has a clean authored pawn-fire path

The modern path is promising: `ProjectileDefinition`, `FireModeDefinition`, `ProjectileFireRequest`, and `ProjectileFirePlanner` support authored payloads, burst, spread, hitscan/prefab delivery, action-context targeting, ownership, faction, lifetime, and impact definitions (`ProjectileDefinition.cs:10-22`, `FireModeDefinition.cs:9-22`, `ProjectileFirePlanner.cs:9-48`, `50-71`).

The largest pawn integration gap is now closed: `PawnCombatBehaviour` and `PawnCombatBehaviour2D` build `ProjectileFireRequest` values from authored `WeaponData.projectileDefinition` assets and fire them through `ProjectileLauncher3D` or `ProjectileLauncher2D`. Prefab authoring now validates the runtime body and warns on mixed 2D/3D physics lanes before scene work. The remaining projectile work is PlayMode proof and non-pawn firing surfaces such as enemies, traps, cards, and project-owned action systems.

### 5. Board/card/tabletop is currently a setup lane, not a mechanics lane

The docs correctly state that board/card/tabletop should not require fake pawns (`Docs/Setup/RUNTIME_PATTERN_COOKBOOK.md:21-24`, `35-48`). The tabletop factory creates no-pawn participants and a setup profile with board/card, turn/menu, camera/cursor, and scoring patterns (`Editor/GameplayStarterPackFactory.cs`).

But the runtime systems listed there are explicitly "project-owned": board/card rule system, deck/hand/zone services, action selection UI, turn order, and action queue (`GameplayStarterPackFactory.cs`). Searches found no concrete shared `BoardStateService`, `CardZoneService`, `TurnOrderService`, or `ActionQueue` runtime in the package. `ActionDefinition` and `ActionTargetRule` are a useful start, but this lane should be represented as "authoring contract / starter route" until the shared mechanics exist.

### 6. Scoring is useful but not yet an objectives framework

`ParticipantScoreService` supports session points, survival time, PlayerPrefs high scores, and per-participant score lookup (`Features/Scoring/Runtime/Shared/ParticipantScoreService.cs:9-17`, `81-156`). That is a good base.

It is not yet a flexible scoring/objectives layer: persistence is hardcoded to PlayerPrefs keys, score/resource/objective definitions are absent, win/loss conditions are not data-driven, and pickup feedback can apply both session points and participant score in one path (`CollectibleFeedback2D.cs:124-135`). That may be intentional, but it should be made explicit with separate session score vs participant score semantics.

### 7. Inspector coverage is broad, but deep customization needs more repair/validation

The inspector framework is strong: guided manuals, validation messages, setup steps, starter-pack buttons, and copied checklists (`PyralisInspectorGuide.cs:113-180`; `PyralisSetupFlowMonitor.cs:488-615`). This is above normal prototype tooling.

The next level is deeper validation and safe repair actions:

- Resolve selected runtime patterns into actual required components/services, not only strings.
- Validate prefab internals for lane-specific contracts: launchers, hitbox zone names, camera targets, score services, hazard colliders, pickup award/spawn surfaces, and deeper projectile pool behavior.
- Offer targeted Add/Fix buttons beyond `AddLifetimeScope` and beginner default restoration (`PyralisSetupFlowMonitor.cs:459-486`).
- Show route-specific readiness: "Projectile Combat Ready", "2D Arcade Loop Ready", "Tabletop Contract Only", etc.

## Current Risk Summary

| Risk | Impact |
|---|---|
| 2D lane depends on explicit services beyond the first validator pass | State reader and camera bounds are now surfaced in setup flow; award sinks, hazard outcome sinks, and pickup/spawn surfaces still need deeper scene/prefab validation. |
| Projectile runtime needs deeper proof beyond pawn firing | Pawn 2D/3D ranged weapons now use the shared planner/launcher path, and prefab authoring validates runtime bodies; enemy/trap/card firing and PlayMode pooling behavior still need validation. |
| Pooling and projectile lifetime behavior needs PlayMode proof | The direct destroy conflict is resolved through `ProjectilePoolHandle`, but pooled prefab reuse still needs Unity PlayMode coverage. |
| Board/card/tabletop advertised beyond runtime depth | Setup flow now labels this as a contract-only path, but users may still expect named board/card mechanics that are currently project-owned. |
| Setup validation still checks only selected scene/prefab contracts | Inspector now catches missing score services, projectile launchers, pawn motor/presentation modules, malformed runtime patterns, and unverified required-system claims, but lane-specific repair actions remain incomplete. |
| Persistent service implementations still expose some static ownership fields | The active consumers have moved to contracts, but `SceneLoader`, `SceneFader`, `TimeManager`, `CameraShake`, `SettingsManager`, and `GameManager` still carry singleton-style implementation fields for persistence or duplicate control. |
| Runtime global camera/discovery lookups are cleared from Gameplay runtime paths | `SceneGuard` is the documented exception; source contracts should keep `Camera.main` and scene-wide discovery out of gameplay systems. |

## Recommended Fix Order

### Slice 1 - Scene Contract Validation

Add a route/service validator that turns selected runtime patterns into concrete scene requirements:

- Realtime pawn: participants, pawn definitions, `PawnRoot`, spawn points, and movement/presentation modules are now checked; next validate input-module expectations and camera injection by route.
- Projectile combat: launcher component and declared launcher/runtime-system claims are now checked; next validate `ProjectileDefinition`, fire mode, projectile prefab compatibility, and impact definition.
- 2D arcade loop: game state provider and camera bounds provider are now checked; next validate pickup spawn surface, award sink, and hazard outcome sink.
- Camera/cursor: camera rig or explicit camera/cursor control surface.
- Scoring/objectives: `ParticipantScoreService` or declared custom scoring service is now checked through `ISessionScoreService`.
- Board/card/tabletop: now clearly marked as "contract-only unless custom rule services are assigned" until shared services exist.

This is the highest-leverage step because it prevents the inspector from over-promising runtime readiness.

### Slice 2 - 2D Arcade Modernization

Introduce explicit service contracts and migrate the existing 2D loop away from singleton/global state:

- `IGameplayStateReader` is now the state gate instead of direct `GameManager.Instance.CurrentState`.
- `ICameraBoundsProvider` instead of direct `Camera.main` or camera-singleton access.
- `IHazardOutcomeSink` is now the hazard death/outcome seam.
- `IPickupAwardSink`, `IPickupSpawnSurface`, and `IPickupBurstSpawnSurface` are now the pickup award and spawn seams.
- Next work is scene-readiness validation, prefab repair actions, and EditMode/PlayMode proof for missing collaborators and multi-participant scenes.

### Slice 3 - Projectile Runtime Unification

Continue moving all projectile firing through `ProjectileFireRequest -> ProjectileFirePlanner -> ProjectileLauncher2D/3D`:

- Pawn 2D/3D `WeaponData` ranged firing now builds a request instead of instantiating directly.
- `Projectile` now releases through `ProjectilePoolHandle` when present.
- Enemy, trap, card, and action-system projectile firing still need to use the same path.
- Fire mode cooldown/ammo/reload should have a reusable runtime state object.
- Add tests for burst/spread/action-target direction, pooling return, faction filtering, and planner-backed weapon fire.

### Slice 4 - 3D Pawn Safety And Test Expansion

Fix optional dependency safety and broaden movement tests:

- Null-safe knockback path or required/default knockback component.
- Camera context injection or profile assignment.
- Tests for coyote time, jump buffer, land slow, dodge cooldown, power-slide gating, top-down no-gravity, and missing optional collaborators.

### Slice 5 - Current Lane Honesty For Board/Card/Turn

Do not build a whole new archetype yet. First, make the current claim honest and useful:

- Rename status in authoring to "Board/Card/Tabletop Contract" until shared runtime services exist.
- Add minimal shared interfaces: board state, zone state, turn order, action queue/resolver.
- Provide an example validator that checks custom services are assigned when a tabletop route is selected.

## Verification Performed

- Inspected architecture docs, feature scope docs, runtime pattern cookbook, and existing product audit.
- Traced representative runtime code for session bootstrap, 3D pawn movement, 2D pawn movement, combat, projectile launchers, projectile definitions/planner, hazards, pickups, scoring, setup flow, and example pack factory.
- Searched for remaining global access patterns across Gameplay runtime C# sources:
  - direct runtime singleton consumers: 0 offenders across the protected list
  - runtime scene-wide discovery outside `SceneGuard`: 0 offenders
  - broad editor/doc guidance no longer advertises legacy projectile setup as a supported path
- Reviewed existing test coverage signals for setup flow, definitions, projectile planner, pickup seams, hazard impact utility, and authoring source contracts.

Code and tests have now changed after the original audit. The next step should be deeper lane-readiness checks and safe repair actions, followed by PlayMode proof of the 2D arcade loop and projectile pooling/runtime unification.

2026-05-24 update: treat `CURRENT_STATE_AUDIT.md` as the fresher readiness source. Projectile pooling/reuse and 2D/3D trigger contact now have Unity PlayMode proof, and the foundation gate is Unity batchmode Test Runner without `-quit`.
