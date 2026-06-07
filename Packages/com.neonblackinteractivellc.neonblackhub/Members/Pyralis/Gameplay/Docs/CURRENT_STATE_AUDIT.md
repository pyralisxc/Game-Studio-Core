# NeonBlack Gameplay Current State Audit

This document captures the current architectural state of NeonBlack Gameplay after the platform realignment, assembly split, deferred cleanup pass, setup-doc consolidation, the 2026-05-22 Pyralis codebase audit, and the 2026-05-23 authoring/mechanics quality pass.

Update it whenever a major blocker changes or a formerly open refactor concern is no longer one of the top priorities.

## Summary

NeonBlack Gameplay now has:

- a real multi-assembly shared core
- a `VContainer`-backed composition root
- enforced feature ownership across the main gameplay domains
- cleaned deferred slices for Pickups, Feedback, and Scoring
- consolidated setup docs built around `Docs/Setup/CANONICAL_SETUP.md`

The platform architecture is no longer the main blocker. The highest-value ongoing work is:

- expanding Unity-side behavioral coverage
- keeping docs and editor tooling aligned with the preferred authored path
- continuing to reduce remaining single-primary-player assumptions in older compatibility surfaces without reintroducing protected singleton reads
- collapsing the remaining split between runtime registry support and the `VContainer` composition root
- fixing identified hot-path allocation risks before they become gameplay-scale performance problems

## Stable Platform Foundations

These are now true platform capabilities, not transitional goals:

- participant/session runtime through `ParticipantRosterService`, `ParticipantSpawnService`, `SessionStateService`, and `GameplaySessionBootstrap`
- authored definitions for session, participant, pawn, game mode, and feature modules
- authored profiles for movement, combat, traversal, presentation, camera, playfield, input, and settings
- feature runtime initialization through `FeatureRuntimeInitializationContext`
- actor composition through `ActorFeatureContext` and narrowed core contracts
- transport-agnostic networking ownership contracts in `Core/Contracts/Networking`
- NGO-specific behavior isolated in `NeonBlack.Gameplay.Networking`

## Assembly State

The package is no longer operating as one broad gameplay assembly.

Stable cuts now exist for:

- `Core`
- `Data`
- `Characters`
- `Composition`
- `Presentation`
- `Networking`
- `Combat`
- `Traversal`
- `Interaction`
- `Feedback`
- `Scoring`
- `Pickups`

Current priority:

- enforce the boundaries we already created
- keep extracting small contracts when a feature seam is still too concrete
- avoid re-broadening assemblies for convenience

## Resolved In The Current Architecture

These were major refactor blockers and are now resolved or intentionally stabilized:

- `GameServices` is gone from the active runtime architecture
- `GameplaySessionBootstrap` is the supported startup path
- feature runtime installation is DI-backed
- the 3D pawn is decomposed into focused modules
- the 2D pawn is decomposed into focused components, with `Motor2D` serving as the shared 2D pawn motor surface
- older `Runtime2D/`, top-level `Shared/`, and `Legacy/` folder drift has been removed from the active architecture; feature-local `Runtime/Shared` folders are intentional ownership slices
- `LegacyBrawlerPawnBridge` and `LegacyArcadePawnBridge` are no longer part of the supported path
- `PointPickup`, `PointPickupSpawner`, and `PointPickupFeedback` thin aliases have been removed
- deferred pickup, feedback, and scoring seams now use explicit contracts instead of hidden orchestration
- setup docs are consolidated around canonical naming rather than compatibility naming
- Unity Safe Mode compilation was cleared after the asmdef, editor, and test-reference stabilization pass
- active Gameplay runtime consumers of the protected singleton/global lookup list have been removed; remaining references are editor warnings or documented persistence surfaces
- concrete Gameplay MonoBehaviour and ScriptableObject authoring surfaces now have guided CustomEditor coverage enforced by source contract tests
- setup flow validates first-layer route requirements for scoring services, projectile launchers, gameplay state, camera bounds, pawn prefab internals, and runtime-system claims
- source/doc mojibake cleanup is now protected by source hygiene tests
- the 2026-05-24 first hardening slice fixed damage-zone dictionary mutation during ticking, made pooled `Projectile` instances release through `ProjectilePoolHandle` instead of always destroying themselves, repointed the package README to live setup docs, and cleaned corrupted movement-model comments
- pawn 2D and 3D ranged/thrown weapon firing now routes through `ProjectileFireRequest`, `ProjectileFirePlanner`, and `ProjectileLauncher2D/3D`; `WeaponData` uses a clean authored `ProjectileDefinition` contract instead of legacy inline projectile fields
- 3D movement now preserves the previous `MovementPhysicsFrame` until `BrawlerMovementModel.Tick` consumes grounding, then clears the accumulator inside `Pawn3DMovementComponent.ApplyMovement` before recording fresh CharacterController results
- projectile prefab delivery now initializes through `IProjectileRuntimeBody`, with 3D `Projectile` and 2D `Projectile2D` receiving the full `ProjectileSpawnCommand` so authored lifetime, range, friendly-fire, and impact definitions travel with prefab shots
- interaction and pickup feature runtimes now tolerate null initialization contexts without throwing, and interaction cooldown is spent only after a handler or fallback interaction actually succeeds
- `PawnTraversalProfile` capability flags now gate runtime behavior: jump, dodge, crouch/power-slide, climb, and hang settings are enforced by the 3D movement/traversal components instead of acting only as authoring labels
- 3D movement now resolves raw input into camera-relative planar world movement when `movementCamera` is assigned, while preserving world-axis fallback when no camera is provided; `Motor3D.CurrentVelocity` now exposes full X/Y/Z state
- `Motor2D` is no longer a duplicate `IPawnMotor` or `IPawnPresentationModule` owner; the dedicated `Pawn2DMovementComponent` and `Pawn2DPresentationComponent` now own profile application for the 2D stack
- projectile definition authoring now validates prefab runtime-body compatibility in the inspector, requiring a `Projectile` or `Projectile2D` runtime body for prefab delivery and warning when a prefab mixes 2D and 3D physics
- active setup now favors manual native authoring through `Create -> NeonBlack` asset paths, Overview guidance, and Inspector field wiring instead of starter-pack-first setup
- fire-mode guidance now distinguishes planner-consumed burst/spread fields from optional magazine data, so cooldown/ammo/reload fields do not overpromise automatic runtime behavior
- local multiplayer docs now explicitly separate `PlayerInputManager` local join from NGO-backed networking, while `Networking/README.md` documents why Netcode dependencies are package-level but networked runtime is opt-in
- local/offline ownership defaults now live in `NeonBlack.Gameplay.Core`, so the main Gameplay assembly no longer depends on the optional Networking assembly just to enter Play Mode
- projectile runtime coverage now includes PlayMode-style pooling/reuse and 2D/3D contact tests; projectile bodies reset angular velocity on return, and `Projectile2D` uses continuous collision detection by default
- `PlayerSpawner` now resolves participant services from the platform context or same-scene components and returns null with an actionable warning when no participant or prefab can be spawned, instead of attempting invalid instantiation
- hazard collectible cleanup now warns when collectible destruction is enabled without a collectible layer mask instead of silently doing no work
- participant registration now reassigns duplicate preferred seats to the next available seat, and overflow participant spawning falls back to seat-offset placement rather than clamping everyone to the last spawn point
- traversal climb/hang cleanup restores the controller, movement state, and climb zone when the component is disabled mid-action
- setup flow now includes a scene/prefab readiness gate before scene authoring: it checks authored participant seats, missing scripts on referenced roots, pawn prefab runtime interfaces, enabled feature module prefab validation, projectile prefab runtime-body/physics compatibility, and networked pawn `NetworkObject`/`NetworkManager` readiness without depending on the optional Networking assembly
- setup flow now treats invalid `RuntimePatternDefinition` assets as blocking, not ready, so empty or malformed runtime patterns cannot falsely clear the beginner checklist
- the archived starter-pack factory remains covered for legacy/internal tests, but active authoring validation no longer treats generated scaffold assets as first-route evidence
- feature runtime lifecycle hardening now destroys previously instantiated feature runtime objects during `ActorFeatureHost` reinitialization, cancels delayed projectile commands when launchers are disabled, makes feedback/traversal feature runtimes tolerate null initialization contexts, and keeps 3D traversal dependencies explicit through `RequireComponent`
- fresh Unity batch validation on 2026-05-24 passed EditMode `189/189` and PlayMode `63/63`; the working Test Runner route is batchmode without `-quit`, because Unity Test Framework 1.6 ignores command-line tests when `-quit` is specified
- networking is MVP-ready for prefab/scene setup: `SessionDefinition.networkMode` selects NGO-backed session/roster/spawn/ownership/authority services through the isolated networking assembly, setup validation covers `NetworkManager`, `UnityTransport`, pawn `NetworkObject`, and Network Prefab registration, and the spawn/authority path now uses participant-specific owner client ids
- package-facing sample metadata now points users to the real Pyralis setup path instead of Unity package template boilerplate text, and beginner-facing editor guidance now uses compatibility/fresh-start language instead of advertising new setup through legacy wording
- `CameraOcclusionFader` now uses `Physics.RaycastNonAlloc`, reusable hit/renderer/restore buffers, and cached material arrays for faded renderers instead of allocating raycast, renderer, and restore lists in `LateUpdate`
- `ParticipantInputRouter` now subscribes to `PlayerInputManager` join/leave events, supports explicit register/unregister calls, guards early lifecycle null roster wiring, and setup flow now treats multi-participant local join as requiring an assigned `PlayerInputManager`
- service lookup migration support is narrower: active feature code now resolves platform services through `GameplayPlatformContext.TryResolve` or `TryGetServices` instead of directly reaching through `GameplayPlatformContext.Current.Services`
- primary-player compatibility paths are less dominant: `PlayerRegistry` now prefers the active participant/player provider before its local static fallback, and `PlayerSpawner` no longer respawns the first participant when a specific seat is configured but missing
- clean-break pre-scene readiness now has the final authoring blockers closed for this phase: NGO participant spawning assigns ownership with `SpawnWithOwnership`, network authority compares the resolved owner client to the local client instead of treating every client as local, scene/prefab readiness scans bootstrap-referenced scene roots and key same-scene services instead of only the bootstrap hierarchy, and the package exposes its sample through `Samples~`/Package Manager metadata
- fresh Unity batch validation on 2026-05-24 passed EditMode `199/199` and PlayMode `74/74` after those changes; `dotnet restore` plus `dotnet build "Game Studio Core.slnx" --no-restore` also passed, with remaining warnings coming from Unity Test Framework and VContainer package code
- fresh Unity batch validation on 2026-05-25 passed EditMode `211/211` and PlayMode `90/90`; the tabletop starter now generates board, pieces, move policy, turn order, terminal conditions, and a board-move action, while malformed generated starter content remains archived outside the package import path
- launch-pad cleanup on 2026-05-25 archived stale active `Assets/GameplayExamplePack`, Unity `InitTestScene*.unity` scratch scenes, `Assets/Temp`, and empty package ownership folders under `_CodexArchive/2026-05-25-launchpad-active-assets-cleanup`; source contracts now keep those generated/test surfaces out of the active import path
- authoring-first tabletop readiness now separates core rule assets from Unity-playable interaction: Setup Flow names the concrete tabletop core (`BoardDefinition`, `BoardMovePolicyDefinition`, `TurnOrderDefinition`, `ActionQueueService`, `BoardMoveActionResolver`, and terminal conditions), adds a recommended `Assign Tabletop Selection Surface` step, and keeps `project-owned` runtime claims visible instead of silently marking them ready when paired with a known service name
- setup flow now has an explicit `Runtime Service Ownership` row so Unity users see that `GameplaySessionBootstrap` owns `PlatformServiceRegistry` setup and builds `PyralisGameplayLifetimeScope`; remaining static `Instance` surfaces are compatibility/persistence affordances, not the path for new service dependencies
- tabletop now has a first Unity-facing selection/action bridge: `TabletopBoardSelectionBridge` lets project-owned board presenters, card-hand UI, cursors, or menus translate piece and destination selections into `BoardMoveActionPayload` requests through `ActionQueueService` without putting legal-move rules in UI code
- tabletop now has a first scene-proof presenter: `TabletopBoardGridPresenter` builds selectable board-space GameObjects and piece views from `BoardDefinition`, initializes `TabletopBoardSelectionBridge`, and can resolve legal moves immediately for prototypes before custom board art or turn animation exists
- direct reads of `GameplayPlatformContext.Current` are now compatibility-only; runtime code should use `TryResolve`, `TryGetServices`, or `TryGetCurrent`, and source contract tests guard against new direct `Current` consumers outside the context owner
- `GameplayPlatformContext.GetServicesOrEmpty` is now compatibility-only; feature installation uses `TryGetServices` and passes a null service registry when the gameplay platform is missing so unowned runtime setup stays visible instead of receiving an anonymous `PlatformServiceRegistry`
- platform composition files no longer live under the Characters feature: `PlatformServiceRegistry` and `GameplayPlatformContext` are owned by `Core/Runtime`, while bootstrap-owned runtime composition helpers live under `Features/Platform/Composition`
- `PyralisGameplayLifetimeScope` now bridges remaining `PlatformServiceRegistry` entries into VContainer after scope-owned components and explicit contracts are registered, so `[Inject]` consumers and registry fallback resolve the same owned instances without duplicate NGO/local service registrations; destroying the owning scope also clears the active platform context or unregisters its resolver to avoid stale global state
- compatibility singleton owners now clean up after themselves during teardown: `TimeManager`, `SceneLoader`, `SceneFader`, `SettingsManager`, and `GameManager` clear their static surfaces on destruction or subsystem registration, and `PlayerSpawner` destroys its generated `DontDestroyOnLoad` countdown UI instead of leaking persistent canvases across scene/test teardown
- final pre-scene authoring audit now has docs and contracts aligned with the real code path: package quick start sends creators through START_HERE, the Authoring Window, Setup Flow, Pawn/Tabletop starter packs, and `PyralisGameplayLifetimeScope`; architecture docs name `GameplaySessionBootstrap` as the Unity-facing entry point and the lifetime scope as the VContainer graph; menu docs no longer teach direct SceneLoader singleton loading
- MVP readiness is now defined as Beginner Prototype Ready through guided Unity setup: the active gates are Game Shell, Pawn-Backed Action across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`, and Non-Pawn Tabletop; each route must satisfy runtime, authoring, guidance, validation, and proof before it can be called ready.

## Highest-Priority Remaining Issues

### 1. Keep Unity Test Runner As The Main Gate

The .NET solution build is a useful fast compile gate, and source/editor contract tests catch many structural regressions. It is not enough for foundation readiness by itself. Projectile pooling/contact, traversal cleanup, participant spawning, networking MVP wiring, generated starter prefabs, sample packaging, and prefab/script serialization now have Unity Test Runner proof, so keep that proof fresh as prefab and scene work begins.

Current focus:

- run Unity EditMode and PlayMode tests after closing the GUI Editor or through a deliberate batchmode validation run without `-quit`
- keep adding PlayMode tests for prefab behavior that cannot be proven through static/source contracts
- treat `dotnet test` as a smoke check only unless Unity Test Runner summaries are present
- after Unity batch tests, rerun `dotnet restore` before the final solution build because Unity can clear generated `Temp/obj` assets

### 2. Runtime Service Ownership Is Still Split

The package now has a real `VContainer` composition root, registry entries are bridged into that root for transition consumers, local-first defaults no longer require the Networking assembly, and compatibility singleton teardown has lifecycle coverage. Service ownership is still split between the VContainer lifetime scope, bootstrap-created persistent services, and migration support registries.

Current examples:

- `Core/Runtime/PlatformServiceRegistry.cs`
- `Core/Runtime/GameplayPlatformContext.cs`
- `Features/Platform/Composition/PyralisGameplayLifetimeScope.cs`
- `Features/Characters/GameplaySessionBootstrap.cs`
- persistent service implementations such as `SceneLoader`, `TimeManager`, `CameraShake`, `GameManager`, `SettingsManager`, and `DamageNumberSpawner`

Why it matters:

- there are still multiple setup-time places where a runtime dependency can be created
- scene reloads and multi-session tests are safer after lifecycle cleanup, but new services can still reintroduce hidden global state if they bypass the lifetime scope
- feature modules can accidentally depend on whichever service path happens to be initialized first
- the DI root is harder to reason about than it should be

Current focus:

- keep `GameplaySessionBootstrap` as the scene entrypoint, but make `PyralisGameplayLifetimeScope` the single durable service ownership path
- keep feature code behind the `GameplayPlatformContext` resolver helpers while shrinking `PlatformServiceRegistry` toward lifetime-scope-backed migration support
- keep protected static singleton reads out of Gameplay runtime code and migrate persistent service ownership toward the lifetime scope when touching nearby systems

### 3. Setup Validation Is MVP-Ready; Keep Deepening Lane-Specific Checks

The setup lane is now explicit enough to start prefab/scene setup without pretending every route is complete runtime gameplay. The supported readiness gate covers selected runtime patterns, session/mode/setup-profile links, participant seats, pawn prefab runtime interfaces, feature-module prefab validation, projectile prefab/runtime-body compatibility, scoring services, projectile launchers, referenced scene roots, missing scripts, and networking MVP wiring.

Current examples:

- `Editor/PyralisSetupFlowMonitor.cs`
- `Editor/PyralisSceneReadinessValidator.cs`
- `Editor/PyralisRuntimeSystemClaimResolver.cs`
- `Editor/GameplayStarterPackFactory.cs`
- `Networking/Runtime/PyralisNetworkSetupValidator.cs`

Networking MVP enhancement lanes:

- rollback or client-side prediction
- movement reconciliation
- projectile reconciliation
- replicated animation state
- remote input command streaming
- lobby/matchmaking/session browser flows

Current focus:

- keep prefab and scene setup moving through the Setup Flow instead of hand-wiring around it
- keep new gameplay features transport-agnostic and annotate network behavior through `FeatureModuleDefinition.networkRole`
- add scene/prefab validation whenever a feature claims `Predicted`, `Replicated`, or `ServerAuthoritative`
- prove one small host/client pawn scene after prefab creation, then expand into movement/projectile replication in deliberate slices
- for tabletop, prove the first scene with `TabletopBoardGridPresenter`, then replace its default generated visuals with project-owned board art, UI, cursor, or card-hand presenters as the game shape becomes clear

### 4. Some Compatibility Gameplay Surfaces Still Assume One Primary Player

The participant model is real, but a few older systems still think in terms of one main player for compatibility.

Current examples:

- `Features/Characters/PlayerRegistry.cs`
- parts of `Features/Respawn/3D/PlayerSpawner.cs`
- some menu-driven and 2D scene-specific flows
- legacy-facing 2D adapter and scene-flow assumptions that still need participant-native proof in Play Mode

Why it matters:

- keeps older gameplay layers from feeling fully participant-native
- makes local multiplayer harder to generalize than it should be
- encourages compatibility code to linger longer than necessary

Current focus:

- keep new respawn and lookup work participant-seat-explicit instead of falling back to "first participant" unless a compatibility path intentionally requests that behavior

### 5. Hot-Path Allocation And Polling Risks Need A Focused Pass

Most systems are not obviously wasteful, and some important paths already use pooling and non-alloc physics. The remaining concerns are concentrated enough to fix surgically.

Current examples:

- `Features/Zones/2D/DamageZone2D.cs` and `Features/Zones/3D/DamageZone.cs` now snapshot tracked targets before ticking; keep this pattern if the zone logic grows
- coroutine-heavy combat, flash, hazard, and UI feedback paths are acceptable at small scale but should be profiled before large swarm or bullet-heavy scenes; projectile launchers now cancel delayed commands on disable
- projectile prefab compatibility now has PlayMode proof around pooling/reuse and 2D/3D trigger contact behavior; max-distance miss effects and dense projectile stress behavior remain scene/profiler follow-ups

Why it matters:

- these costs scale with active cameras, occluders, participants, hazards, projectiles, and UI feedback
- visible spikes will look like game-feel problems even when the gameplay logic is correct
- Unity GC/per-frame allocation problems tend to hide until content density increases

Current focus:

- keep profiling pressure on coroutine-heavy feedback/combat paths once scenes can create realistic density
- reuse expired-target buffers or compact zone state without per-frame allocation
- add profiler notes or performance tests for projectile and feedback stress scenes before expanding content

### 6. Input Ownership Is Improved But Not Fully Normalized

The package already uses the Unity Input System and participant-aware routing, but some systems still retain direct or older assumptions.

Current example:

- `Features/Input/2D/Motor2DInputAdapter.cs`

Why it matters:

- local co-op and mixed-device flows remain more fragile than the shared core now deserves
- older input assumptions can quietly reintroduce single-player bias

### 7. Editor UX Still Trails The Runtime Architecture

The runtime seams are cleaner than the authoring experience in a few places.

Why it matters:

- a correct platform is still harder to use if the Inspector path does not strongly guide the preferred setup
- stale editor guidance can recreate old architectural habits

Current focus:

- keep setup docs, inspectors, and validation aligned with canonical types
- make preferred authored paths the only runtime path where clean-break setup is viable
- remove compatibility fields instead of documenting them as long-term migration surfaces

## Medium-Priority Issues

### 7. Large MonoBehaviours Still Carry High Change Cost

The codebase is no longer centered on one monolithic player controller, but several large files remain likely maintenance hotspots.

Current examples:

- `Features/Enemies/3D/EnemyAI.cs`
- `Editor/PyralisSetupFlowMonitor.cs`
- `Editor/GameplaySessionBootstrapEditor.cs`
- `Features/Characters/PawnCombatBehaviour.cs`
- `Features/Hazards/2D/HazardSpawner.cs`
- `Features/Combat/UI/WorldHealthBar.cs`

Why it matters:

- these files combine enough responsibility that bug fixes are likely to touch unrelated behavior
- editor files in this size range make authoring guidance expensive to keep accurate
- future agents will spend more time reacquiring context before making safe changes

Current focus:

- split only when there is a useful owner boundary, not just to reduce line count
- prefer extracting plain model/policy classes, validation helpers, and small presentation helpers
- add tests around any extracted behavior before moving more logic

### 8. Behavioral Test Coverage Is Still Light

The package now has real architecture and validation coverage, but deeper runtime/editor behavior coverage is still thinner than the platform shape.

Why it matters:

- shared-runtime changes still rely too heavily on manual confidence
- bigger refactors become slower without stronger safety rails

Current focus:

- participant lifecycle
- feature installation order
- setup validation flows
- editor-created authored assets
- Unity-side scene and prefab validation for missing scripts, service wiring, feature installation, and participant spawning

### 9. Assembly Boundaries Need Continued Enforcement

The package now has meaningful domain assemblies, but the aggregate `NeonBlack.Gameplay` assembly still references most feature assemblies for convenience.

Why it matters:

- broad aggregate references make accidental cross-domain dependencies easier
- editor and sample code can hide architecture drift until a later split becomes painful
- asmdefs only pay off if the ownership rules stay enforced

Current focus:

- keep using tests/search checks to prevent forbidden direct references
- avoid adding new dependencies to the aggregate assembly as a shortcut
- extract contracts to `Core` or `Features/Composition` only when they are genuinely shared

### 9. Deferred Compatibility Surfaces Still Need Periodic Review

The highest-risk deferred slices have been cleaned, but some older scene-facing systems still deserve occasional review so they do not drift back toward hidden coupling.

Examples:

- some respawn helpers
- some scene/menu flow helpers
- some HUD and 2D adapter surfaces

## Lower-Priority But Important

### 10. Documentation Hygiene Is Now Ongoing Maintenance

The docs are in much better shape, but the rule now is maintenance discipline, not one more giant rewrite.

Why it matters:

- stale setup docs quickly become competing truths
- the cleaner the architecture gets, the more noticeable stale wording becomes

Current source-of-truth path:

- `Docs/Setup/CANONICAL_SETUP.md`
- `Docs/Setup/SCENE_SETUP_GUIDE.md`
- feature-specific setup docs beneath `Docs/Setup/Prefabs/`

### 11. Source Encoding Hygiene Is Now Guarded

The active Gameplay source/docs had several mojibake sequences from past UTF-8/Windows encoding mismatches. The known active markers have been cleaned and source contract tests now guard Gameplay `.cs` and `.md` files.

Why it matters:

- it does not usually affect runtime behavior, but it degrades trust in the source
- it makes generated comments, XML docs, and setup guidance harder to read
- it can hide real text changes inside noisy diffs

Current focus:

- keep comments and docs plain ASCII or valid UTF-8 when touching nearby files
- avoid decorative separator comments that are easy to corrupt
- keep the source-hygiene contract green as new docs and scripts are added

## Recommended Order Of Attack

1. Promote the MVP readiness matrix and route audit into active package docs, then use it as the control panel for scene-readiness work.
2. Harden the Game Shell route first: boot/loading/menu/settings/credits, scene navigation, Build Settings guidance, and shell proof.
3. Bring Pawn-Backed Action to parity across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`; do not call the route ready while one lane is only partially proven.
4. Finish Non-Pawn Tabletop as a guided no-pawn prototype route with board selection, action queue, turn flow, and terminal condition proof.
5. Run a beginner/friend trial or simulated beginner pass and feed the friction back into inspectors, setup flow, docs, and starter packs.

After the MVP route gates are moving, continue these standing hardening tracks:

1. Collapse runtime service ownership toward the `VContainer` composition root while keeping `GameplaySessionBootstrap` as the scene entrypoint.
2. Continue reducing single-primary-player assumptions in older compatibility layers without reintroducing protected singleton reads.
3. Expand Unity-side behavioral coverage around participant flow, feature installation, authored setup validation, and scene/prefab readiness.
4. Keep networking enhancement work deliberate: host/client pawn proof first, then movement/projectile/animation replication slices.
5. Tighten editor tooling so the clean authored path is the only documented setup path.
6. Keep enforcing asmdef and runtime-boundary rules so the architecture does not drift backwards.
7. Keep `CURRENT_STATE_AUDIT.md`, `CANONICAL_SETUP.md`, and feature setup docs aligned with the real package shape.

## Success Signals

The package is moving in the right direction when:

- participant-aware flows replace one-player assumptions without special-case architecture
- new setup guides can use canonical type names only
- feature boundaries stay stable without needing to merge assemblies back together
- editor tooling points people toward the preferred path by default
- tests catch drift before refactors become risky
- scene setup can be validated without relying on hidden static state
- common authoring mistakes are caught in inspectors before Play Mode
