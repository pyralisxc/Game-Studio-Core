# Pyralis Cleanup Consolidation Plan

## Goal

Reach a stable maintenance checkpoint where runtime ownership is obvious, the Authoring Window owns setup guidance, inspectors edit selected objects, and Hygiene points to real architecture pressure instead of duplicated explanation systems.

## Core Rule

```text
One runtime owner.
One reflective authoring spine.
No duplicate setup guides.
```

## Endpoint

This cleanup phase is complete when:

- Inspectors render fields, object-local validation, and a compact **Open Pyralis Authoring** handoff.
- Route setup, first proof, next steps, and beginner sequencing live in the Authoring Window graph projections.
- Input ownership remains `ParticipantDefinition.inputProfile`.
- Spawn ownership remains participant/session/pawn driven.
- Camera framing remains separate from movement legality.
- Movement, presentation, and animation keep distinct runtime responsibilities.
- Hygiene export shows real dependency pressure and does not report unresolved routes as failed proofs.
- EditMode and PlayMode tests pass in Unity Test Runner.

## Task Plan

### Phase 1: Baseline Hygiene Pressure

- Use `Editor/Authoring/TempGraphs/*_Hygiene_GraphSnapshot.json` as the cleanup baseline.
- Classify each top dependency-pressure file as runtime ownership, editor duplication, accepted composition pressure, compatibility pressure, or scanner noise.
- Use Hygiene pressure kinds to separate actual cleanup candidates from expected pressure:
  - `RuntimeOwnership`: inspect for scripts owning too many gameplay domains.
  - `CompatibilitySurface`: keep explicit and shrink when participant/session-native paths replace it.
  - `ReferenceAssembly`: expected pressure for focused runtime reference/context helpers; review only if gameplay decisions move into the helper.
  - `AcceptedComposition`: watch composition roots, but do not split them unless they begin owning feature behavior.
  - `PawnCoordinator`: expected pressure for explicit Unity-native pawn coordinators such as `PawnRoot`, `Motor2D`, and `Motor3D`; review only if they start constructing optional features or owning movement/combat/presentation behavior directly.
  - `FeatureModule`: expected pressure for optional `ActorFeatureHost`-installed capability modules and feature-owned contracts; review only if they become required pawn identity.
  - `AuthoredRuntimeSurface`: expected pressure for files that intentionally hold serialized runtime tuning, profile application, local validation, or gizmos; review if they duplicate graph/contract setup meaning.
  - `EditorAudit`: review for duplicate setup truth, not runtime architecture failure.
  - `GrammarVocabulary`: keep wording-only; move feature setup meaning back to contracts/reflection.
  - `ScannerImplementation`: tune false positives before treating the scanner as product risk.
- Do not add new Hygiene sections unless the export cannot explain an actionable risk.

### Phase 2: Inspector Demotion

- Keep `PyralisInspectorGuide` as a compact handoff surface, not a route guide renderer.
- Keep field-local validation such as missing colliders, missing profiles, invalid Animator parameters, and invalid numeric ranges.
- Remove or ignore inspector prose that explains whole-route setup, first proofs, beginner sequencing, or asset chains.
- Treat `InputProfileEditor` and `PawnAnimationProfileEditor` as acceptable object-local tools because they edit one asset's rows or mappings.

### Phase 3: Runtime Ownership Seams

- Keep `GameplaySessionBootstrap` as the scene entrypoint.
- Keep `PyralisGameplayLifetimeScope` as the service composition owner. **Action:** Strip `ResolveCoreComponent` autospawn logic; replace with explicit authoring/validation.
- Keep participant/session runtime files under `Features/Platform/Session`: participant identity, roster, spawn service, session state, and participant lookup are platform/session ownership, not character behavior ownership. The current asmref is an assembly-stability bridge only; do not use it as an excuse to move new pawn behavior into Platform.
- Keep `ParticipantDefinition.inputProfile` as the only authored input owner.
- Keep `ParticipantDefinition.defaultPawn -> PawnDefinition.pawnPrefab` as the spawn route.
- Keep `GameManager.playerControllers` as explicit standalone compatibility, not reflected beginner assignment guidance.
- Keep contracts from declaring their own component type as a required component; reflection already proves the source component exists.
- Keep injected runtime service override fields out of normal assignment guidance unless they are the only supported setup path.
- Keep `PlayfieldProfile` as movement-space owner and `CameraRigProfile` as framing owner.

### Phase 4: Movement, Presentation, And Camera Clarity

- Movement scripts move actors and enforce movement-space rules.
- Presentation scripts display state and forward animation/visual signals.
- Animation scripts map gameplay signals to Animator parameters.
- Camera scripts frame/follow participants and may read playfield bounds only for camera focus.
- Split large files only when there is a real owner boundary, not just because they are long.
- Keep `Pawn2DMovementComponent` as the beginner-facing 2D movement facade, with top-down/no-gravity, side-view/gravity, and movement-bounds source lanes split into partials. Presentation profile application belongs to `Pawn2DPresentationComponent`.
- Keep `Pawn2DPresentationComponent` as a beginner-facing facade until presentation pressure proves a real extraction boundary.
- Keep `PawnRoot`, `Motor3D`, and `Pawn3DMovementComponent` behavior/coordinator-focused; `PawnRootRuntimeReferences`, `Motor3DRuntimeReferences`, and `Pawn3DMovementRuntimeReferences` own sibling/context lookup for the main 3D pawn stack. `PawnRoot` owns participant initialization as the facade, `PawnRoot.Profiles` owns profile application, and `PawnRoot.Features` owns feature-module installation and actor feature context creation.
- Keep `Motor3D` as a Unity-native explicit sibling coordinator. It may ask `ActorFeatureHost` for optional traversal, guard, interaction, or status capabilities, but the pawn's core movement identity remains the visible `Motor3D`/input/movement/traversal/presentation sibling stack. Status/reaction forwarding and external traversal forwarding are source lanes, not separate gameplay owners.
- Keep `Pawn3DMovementComponent` as the 3D movement facade, with CharacterController physics, crouch/capsule handling, movement config, and profile application split into source lanes. Do not move route truth or authoring guidance into those lanes.
- Keep `Pawn3DTraversalComponent` as the traversal facade, with climb execution and hang/shimmy execution split into source lanes. Traversal profile meaning belongs in profiles/contracts; runtime behavior belongs in the component lanes.
- Keep `PawnCombatBehaviour` as the beginner-facing 3D combat facade, with sibling reference assembly in `PawnCombatRuntimeReferences` and combo/fallback sequence execution plus hit/projectile resolution split into partial source lanes.
- Keep `PawnCombatBehaviour2D` on the shared `PawnComboProcessor`; 2D combat uses the same facade/source-lane shape as 3D combat, with combo sequencing in `PawnCombatBehaviour2D.Sequences`, HitBox2D/projectile application in `PawnCombatBehaviour2D.HitResolution`, and sibling reference/launcher lookup in `PawnCombat2DRuntimeReferences`.
- Keep `WorldHealthBar` as the world-space health bar state owner; generated canvas construction belongs in `WorldHealthBar.Canvas`, while health events and fill/visibility state stay in the main component.
- Keep `PlayerInputHandler` as a direct-input compatibility/advanced handler with explicit source lanes: movement polling in the main file, action asset/profile binding in `PlayerInputHandler.Bindings`, and action callbacks in `PlayerInputHandler.ActionCallbacks`. Beginner input ownership still remains `ParticipantDefinition.inputProfile`.
- Keep `DamageZone` and `DamageZone2D` lane-specific for collider events, but shared profile impact logic belongs in `DamageZoneImpactRuntime` so 2D and 3D hazards do not drift apart.
- Keep `EnemyCombatModule` as the enemy attack execution owner, with combat-profile application, range measurement, and hitbox maintenance split into dedicated source lanes.
- Keep `EnemyAI` as the tactical state coordinator. Sibling reference caching, billboard setup, and feature-context assembly belong in `EnemyActorRuntimeReferences` so the coordinator does not own presentation/reference plumbing.
- Keep `Hazard` contracts focused on required beginner fields and leave optional modifiers conditional in wording until contract metadata can express conditional setup. `HazardRuntimeReferences` owns audio, feedback-runtime, camera-shake, settings, rigidbody, and actor-target reference discovery so sequence partials stay behavior-focused.
- Keep `ArenaZone` as the beginner-facing encounter facade. Camera profile handoff belongs in `ArenaZone.Camera`; spawner activation, exit blockers, and enemy clear tracking belong in `ArenaZone.Spawning`.
- Treat deeper `EnemyAI` and `Hazard` state/sequence extraction as later work only after behavior tests prove the code that would move.

### Phase 4A: Hygiene-Driven Source Lane Cleanup

- Keep the beginner-facing components stable while splitting source files by real owner lanes.
- `Pawn2DMovementComponent` owns the 2D movement facade; config, validation, and gizmo drawing live in dedicated source lanes.
- `Motor3D` owns frame coordination; the per-frame loop and frame substeps live in `Motor3D.Loop`.
- `Pawn3DTraversalComponent` owns traversal probing and public traversal operations; profile application and dependency references live in dedicated source lanes, while climb and hang execution stay separate.
- `EnemyCombatModule` owns enemy combat setup/ticking; attack execution, profiles, range measurement, and hitbox maintenance live in dedicated source lanes.
- `EnemyReactionFeatureRuntime` owns reaction state; feature initialization and impact sink resolution live in a feature/reference lane, while damage/death reaction behavior stays in the main file.
- `EnemyAI` owns tactical state coordination; feature installation and editor gizmos live outside the main state-machine file.
- `DamageZone` and `DamageZone2D` own Unity physics entry points only; `DamageZoneTargetRuntime` owns shared target tracking, ticking, and profile/fallback impact application.
- `WorldHealthBar` keeps the health-bar facade and serialized presentation surface; canvas construction, runtime ticking, and health-event response live in dedicated source lanes.
- `ActorFloatingFeedbackReceiver` keeps feedback event routing; popup pooling/lifetime and validation live in dedicated source lanes.
- `Pawn2DPresentationComponent` remains one beginner-facing presentation facade; animation signal mapping, sprite facing/tint, deformation, profile application, feedback, and validation are split into source lanes.

### Phase 5: Hygiene Guardrail

- Re-export Hygiene after cleanup.
- Confirm unresolved/no-route state has no proof blockers.
- Confirm contract inventory is separate from route failure.
- Keep source pressure useful by distinguishing real runtime reflection/static lookups from ordinary editor `SerializedProperty` binding.
- Keep the visible Hygiene tab and exported `cleanupFocus` ordered by cleanup usefulness: runtime ownership first, compatibility second, expected composition/reference/editor/grammar/scanner pressure after that.
- Keep exported `cleanupFocus` limited to actionable runtime ownership and compatibility pressure. Expected coordinator, feature-module, authored-surface, composition, reference, editor, grammar, and scanner pressure remains visible in `dependencyPressure` and the tab summary, but should not be treated as the next cleanup target by default.
- Treat exported `dependencyPressure` as prioritized audit inventory. Use `cleanupFocus`, `cleanupPriority`, and `cleanupFocus: true` to choose the next runtime cleanup slice instead of reading raw risk score as the plan.
- Use dependency-pressure changes to choose the next cleanup slice.

### Phase 6: Documentation And Verification

- Update active docs only with current truth.
- Keep history out of active setup guidance unless it protects a supported compatibility path.
- Validate with Unity Test Runner EditMode and PlayMode, or use `Tools/Validation/Run-PreSceneValidation.ps1` with the GUI Editor closed.
