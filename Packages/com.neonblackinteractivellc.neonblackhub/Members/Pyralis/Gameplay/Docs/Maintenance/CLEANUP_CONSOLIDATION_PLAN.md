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
- Do not add new Hygiene sections unless the export cannot explain an actionable risk.

### Phase 2: Inspector Demotion

- Keep `PyralisInspectorGuide` as a compact handoff surface, not a route guide renderer.
- Keep field-local validation such as missing colliders, missing profiles, invalid Animator parameters, and invalid numeric ranges.
- Remove or ignore inspector prose that explains whole-route setup, first proofs, beginner sequencing, or asset chains.
- Treat `InputProfileEditor` and `PawnAnimationProfileEditor` as acceptable object-local tools because they edit one asset's rows or mappings.

### Phase 3: Runtime Ownership Seams

- Keep `GameplaySessionBootstrap` as the scene entrypoint.
- Keep `PyralisGameplayLifetimeScope` as the service composition owner.
- Keep `ParticipantDefinition.inputProfile` as the only authored input owner.
- Keep `ParticipantDefinition.defaultPawn -> PawnDefinition.pawnPrefab` as the spawn route.
- Keep `GameManager.playerControllers` only as explicit standalone compatibility.
- Keep `PlayfieldProfile` as movement-space owner and `CameraRigProfile` as framing owner.

### Phase 4: Movement, Presentation, And Camera Clarity

- Movement scripts move actors and enforce movement-space rules.
- Presentation scripts display state and forward animation/visual signals.
- Animation scripts map gameplay signals to Animator parameters.
- Camera scripts frame/follow participants and may read playfield bounds only for camera focus.
- Split large files only when there is a real owner boundary, not just because they are long.
- Keep `Pawn2DPresentationComponent` as a beginner-facing facade until presentation pressure proves a real extraction boundary.
- Keep `Motor3D` as a coordinator until feature fallback routing earns a narrower adapter.
- Plan a later behavior-tested convergence where `PawnCombatBehaviour2D` uses the shared `PawnComboProcessor` instead of carrying its own combo runtime.
- Treat `EnemyAI` and `Hazard` as later extraction targets only after tests prove the state/sequence behavior that would move.

### Phase 5: Hygiene Guardrail

- Re-export Hygiene after cleanup.
- Confirm unresolved/no-route state has no proof blockers.
- Confirm contract inventory is separate from route failure.
- Use dependency-pressure changes to choose the next cleanup slice.

### Phase 6: Documentation And Verification

- Update active docs only with current truth.
- Keep history out of active setup guidance unless it protects a supported compatibility path.
- Validate with Unity Test Runner EditMode and PlayMode, or use `Tools/Validation/Run-PreSceneValidation.ps1` with the GUI Editor closed.
