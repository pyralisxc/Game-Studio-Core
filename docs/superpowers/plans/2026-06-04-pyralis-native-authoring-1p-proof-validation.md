# Pyralis Native 1P Authoring Proof Validation

## Status

Checkpoint reached. Automated native-path hardening is implemented and validated on 2026-06-04. A real GUI click-through remains explicitly tracked because Computer Use was not callable in this session.

## Intent

Prove the completed reflective authoring contracts refactor supports the required native Unity 1P movement authoring route without relying on scene generators, starter-pack shortcuts, or hidden one-click setup.

## Scope

- Validate the route described by `docs/authoring-native-1p-proof-checklist.md`.
- Keep the Authoring Window as route guidance only.
- Keep actual authorship in native Unity surfaces: Project Create menus, Hierarchy Create Empty, Inspector Add Component, Inspector field assignment, prefab saving, and Play Mode.

## Implemented Hardening

- `PawnRoot` now exposes an explicit Add Component path: `NeonBlack/Gameplay/Characters/Pawn Root`.
- `PyralisGameplayLifetimeScope` now exposes an explicit Add Component path: `NeonBlack/Gameplay/Setup/Pyralis Gameplay Lifetime Scope`.
- `PyralisConventionAuthoringFacts` now exposes native create/add-component facts for the complete minimum 1P movement checklist:
  - `SessionDefinition`
  - `GameModeDefinition`
  - `GameSetupProfile`
  - `ParticipantDefinition`
  - `PawnDefinition`
  - `InputProfile`
  - `PawnMovementProfile`
  - `PawnPresentationProfile`
  - `GameplaySessionBootstrap`
  - `PyralisGameplayLifetimeScope`
  - `PawnRoot`
  - `Motor2D`
  - `Motor2DInputAdapter`
  - `Pawn2DMovementComponent`
  - `Pawn2DPresentationComponent`
- `SetupFlowValidatorTests` now protects the native Add Component menu paths and fact-registry coverage for this route.

## Validation Evidence

- `dotnet build "Game Studio Core.slnx" --no-restore` passed on 2026-06-04.
- `.\Tools\Validation\Run-PreSceneValidation.ps1` passed on 2026-06-04.
- EditMode: 369/369 passed.
- PlayMode: 166/166 passed.
- Residue scan: clean.
- XML summaries:
  - `Logs\Codex\pre-scene-editmode-20260604-103202-results.xml`
  - `Logs\Codex\pre-scene-playmode-20260604-103407-results.xml`

## Remaining Validation

- Perform the actual Unity Editor GUI pass when Computer Use or a human tester is available:
  - open `NeonBlack/Gameplay/Pyralis Authoring Window`
  - create the checklist assets through Project Create menus
  - create `Gameplay Root` and `PlayerPawn` through Hierarchy
  - add all required components through Inspector Add Component
  - assign references in the Inspector
  - save `PlayerPawn` as a prefab
  - enter Play Mode and confirm one pawn spawns and moves

## Completion Criteria

- Full pre-scene validation passes.
- The Authoring Window and fact registry expose every native action needed by the checklist.
- The GUI pass has either been completed or remains explicitly tracked as a manual validation residual.

## Check-out Audit

- Product state: the native 1P movement checklist now has explicit Unity-visible create/add-component coverage for its minimum asset and component path.
- Code/architecture state: Add Component menu metadata is present for `PawnRoot` and `PyralisGameplayLifetimeScope`; convention facts now expose the full minimum route to the reflective authoring/fact layer.
- Maintenance/folderbase state: no generated scene, starter pack, or proof-scene shortcut was added; the change stays in runtime metadata, authoring facts, tests, and this plan.
- Docs/standards state: this plan records that automated readiness is complete and that true GUI walkthrough evidence is still a separate validation surface.
- Verification evidence: full pre-scene gate passed with EditMode 369/369, PlayMode 166/166, and clean residue.
- Known residual risk: the actual Unity GUI click-through was not completed because no callable Computer Use tool was available in this session.
- Recommended next move: run the real Editor walkthrough with Computer Use or Cameron at the keyboard, then patch any guidance friction found during the click path.
