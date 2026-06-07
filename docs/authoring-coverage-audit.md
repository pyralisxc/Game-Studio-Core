# Authoring Coverage Audit

Date: 2026-05-23

## 2026-05-25 Teacher-Quality Addendum

- Dynamic authoring coverage remains the guardrail: concrete inspector-visible Gameplay `MonoBehaviour` and `ScriptableObject` types are checked against `CustomEditor` coverage.
- `PyralisSetupFlowMonitor` is now the live beginner checklist. It validates route shape, scene readiness, tabletop contract state, runtime-system claims, service ownership, and common missing authoring links from the Inspector.
- Service ownership now has a verified Unity-to-runtime path: `GameplaySessionBootstrap` is the Unity-facing entry point, `PyralisGameplayLifetimeScope` owns the VContainer graph, and remaining `PlatformServiceRegistry` entries are bridged into VContainer so injection and transition-era registry consumers see the same services.
- Compatibility singleton surfaces are lifecycle-cleaned and tested. They remain duplicate-control or persistence affordances, not the beginner dependency path.
- The remaining useful authoring work is not basic coverage. It is deeper lane-specific repair actions and first-scene proof during scene development: projectile pool compatibility, pawn input/camera expectations, tabletop action surfaces, hazard outcome sinks, and creator-safe one-click repairs where the setup flow can confidently make them.

## Rule

Any Pyralis `MonoBehaviour` or `ScriptableObject` that a creator can add, load, assign, or inspect in Unity should have guided authoring. Background-only runtime helpers can skip guided authoring only when they are not intended to be placed on scene objects, prefabs, or assets directly.

## Current Snapshot

- Inspector-visible candidates found under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay`: 143
- Candidates without `CustomEditor` coverage before authoring takeover: 72
- Current concrete candidates without `CustomEditor` coverage: 0
- Current abstract base classes without direct `CustomEditor` coverage: 2
- Current Add Component / Create Asset menu candidates without `CustomEditor` coverage: 0
- Source contract status: `AuthoringSourceContractTests.PyralisEditor_Source_CoversConcreteInspectorVisibleRuntimeTypes` now dynamically checks concrete Gameplay `MonoBehaviour` and `ScriptableObject` types against `CustomEditor` coverage.
- Add Component discoverability status: high-traffic from-scratch scene components now have protected menu paths, including `GameplaySessionBootstrap`, `GameManager`, `HealthComponent`, `Hazard`, `EnemyAI`, `PlayerInputHandler`, and `ParticipantScoreService`.
- Cleaned slices so far: camera runtime stack, 2D pawn/input stack, actor feature runtime stack, pickup/scoring runtime stack, feedback/HUD runtime stack, presentation/scoring screen stack, remaining menu-facing enemy/hazard/traversal stack, combat/damage scene component stack, scene/game-flow component stack, UI/menu flow stack, hazards/spawning stack, visual/world helper stack, final P2 classification stack, full Gameplay readability cleanup
- Setup-flow validation now goes beyond asset intent for the first concrete route layer: scoring routes require `GameModeDefinition.enableScore` and an `ISessionScoreService`, projectile routes require a `ProjectileLauncherBase`, pawn/scoring routes recommend an `IGameplayStateReader`, camera/playfield routes recommend an `ICameraBoundsProvider`, tabletop routes are labeled as a runtime contract instead of a completed mechanics lane, pawn prefabs must include `PawnRoot`, `IPawnMotor`, and `IPawnPresentationModule`, and unverified `RuntimePatternDefinition.requiredRuntimeSystems` strings are surfaced to creators through a centralized runtime-system claim resolver instead of inspector-local string logic.

## Completed In This Pass

- `CinemachineCameraRigController`
- `CameraAspectController`
- `CameraOcclusionFader`
- `CameraShake`
- `CameraZone`
- `Motor2D`
- `Pawn2DMovementComponent`
- `Pawn2DPresentationComponent`
- `PawnCombatBehaviour2D`
- `PawnCombatBehaviour`
- `Motor2DInputAdapter`
- `ActorGuardInputBridge2D`
- `ActorInteractionInputBridge2D`
- `ActorAnimationDriver`
- `ActorFeatureHost`
- `ActorCombatReactionFeatureRuntime`
- `ActorStatusEffectFeatureRuntime`
- `ActorInteractionFeatureRuntime`
- `ActorFeedbackFeatureRuntime`
- `ActorFloatingFeedbackReceiver`
- `Collectible2D`
- `Collectible3D`
- `CollectibleFeedback2D`
- `ActorPickupCollectorFeature2D`
- `ActorPickupCollectorFeature3D`
- `StillnessBonus2D`
- `ParticipantFeedbackRelay`
- `ParticipantHealthPanel`
- `ParticipantTimedTextPanel`
- `ParticipantFeedbackHudPresenter`
- `ParticipantHealthHudBinder`
- `ActorShadowDriver`
- `BillboardFacing3D`
- `LeaderboardManager`
- `LeaderboardScreen`
- `EnemyAmbientFeatureRuntime`
- `EnemyReactionFeatureRuntime`
- `HazardFeedbackRuntime`
- `DamageZone2D`
- `PawnTraversalFeatureRuntime3D`
- `HitBox2D`
- `Projectile`
- `KnockbackReceiver`
- `HitFlash`
- `DamageNumber`
- `DamageNumberSpawner`
- `GameManager`
- `PlayerSpawner`
- `PlayerRegistry`
- `DifficultyManager`
- `TimeManager`
- `SettingsMenu`
- `SettingsScreen`
- `SceneFader`
- `SceneLoader`
- `SceneGuard`
- `SplashScreenController`
- `LoadingScreenController`
- `MainMenuManager`
- `DamageZone`
- `Hazard`
- `HazardSpawner`
- `Spawner`
- `SpawnTracker`
- `SpriteFlasher`
- `TextFlasher`
- `DepthSorting`
- `ArenaZone`
- `TilemapGround`
- `GrabDetector`
- `ProjectilePoolHandle`
- `ParticipantFeedbackService`
- `EnemySpawner`

These now have guided inspectors through the current guided-editor slices, Add Component menu paths where appropriate, validation hints, and editor source contracts.

Runtime claim validation now has a single editor extension point in `PyralisRuntimeSystemClaimResolver`. Future route support should add verifier rules there, then let `PyralisSetupFlowMonitor` display the resulting report.

## Readability Cleanup

- Added a source-hygiene contract for Gameplay `.cs` and `.md` files so mojibake markers do not creep back into source or durable docs.
- Cleaned inspector-facing comments and tooltips in `HazardData`, `WorldHealthBar`, and `EnemyAI` without changing runtime behavior.
- Cleaned the remaining legacy mojibake markers in Gameplay source/docs, including combat setup comments, hazard runtime comments/log messages, scoring section headers, movement helper comments, input/UI comments, and setup/audit docs.

## Remaining Priority Backlog

### P0: Add Component Menu Runtime Surfaces

Complete. Every currently detected Pyralis component under Gameplay with an `AddComponentMenu` path now has guided authoring coverage.

Keep this at zero as new Add Component surfaces are added.

### P1: Public Scene Components Without Menu Paths

These are likely to be seen on prefabs or scene objects and should either receive guided authoring or be marked as background-only:

Complete. All currently identified P1 public scene components now have guided authoring coverage.

### P2: Decide Skip Reasons

Complete for concrete scripts. The final concrete candidates were classified and covered:

- `ProjectilePoolHandle` now has a guided runtime-managed inspector and is hidden from Add Component because creators should not place it directly.
- `ParticipantFeedbackService` now has a clear Add Component path and guided service authoring.
- `EnemySpawner` now has a clear Add Component path, guided encounter-spawner authoring, and null-safe prefab/spawn-point selection.

The only remaining unmatched classes are abstract bases:

- `ProjectileLauncherBase`
- `ParticipantHudTargetBinding`

These are not direct authoring surfaces and cannot be added to scene objects directly. Their concrete subclasses have guided inspectors.

## Suggested Next Slice

Keep the dynamic authoring coverage and source-hygiene contracts green as new Gameplay `MonoBehaviour` and `ScriptableObject` types are added. The next useful pass is deeper lane validation: projectile prefab pool compatibility, pawn input/camera expectations, pickup award/spawn surfaces, hazard outcome sinks, and safe repair actions. Add those checks through shared route/claim validators before adding more inspector-only guidance.
