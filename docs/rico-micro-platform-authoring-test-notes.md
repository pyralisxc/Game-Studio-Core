# Rico Micro-Platform Authoring Test Notes (Fresh Restart)

Date: 2026-05-29

## Goal

Re-run the Rico micro-platform scene setup from a clean state, following the authoring flow as a first-time creator should:

1. Start from the scene root (GameplaySessionBootstrap) with an assigned session asset
2. Let the Authoring Window show the route and next required step
3. Wire required scene surfaces for a minimal first proof
4. Record bugs, authoring gaps, and tooling gaps discovered during the run

Legacy notes from the prior attempt were archived to:

- _CodexArchive/2026-05-29-rico-micro-platform-authoring-test-notes-legacy.md

## Start State Checklist (Pre-Run)

- [ ] Delete stale test setup from prior attempts:
  - Any `Assets/Temp` test roots created by prior factory runs (`PyralisStarterPackFactoryTests`).
  - Any ad-hoc "Rico" scene objects/assets intentionally created for the previous attempt.
  - Old scratch route notes or test artifacts not marked as authoritative.
- [ ] Fresh untitled scene is open.
- [ ] No Rico micro-platform objects exist in scene before this run.
- [ ] `NeonBlack/Gameplay/Pyralis Authoring Window` is open on the `Overview` path while assets are created through native Project-window Create menus.
- [ ] Confirmed existing project defaults:
  - `PyralisGameplayLifetimeScope` + `GameplaySessionBootstrap` composition pattern
  - Starter pack destination default is `Assets/NeonBlack/Gameplay/StarterPacks`
  - Route should support object-first scene wiring before prefab generation.

## Editor/Test Hygiene Notes (for this pass)

- Updated `GameplayStarterPackFactory` path behavior so creating a starter pack while a folder inside an existing starter pack is selected now targets that pack root, not a nested child folder.
- Added an edit-mode test for nested-folder selection behavior:
  - `GameplayStarterPackFactory_SelectingStarterPackSubfolder_UsesPackRootNotNestedPath`
- Still pending: manual Unity-only run to validate full scene wiring and beginner flow on a live layout.

## Scene-Wiring Notes

- Fresh run pending

## Bugs / Blocking issues

- [x] Factory path selection allowed nested folder creation when selection was inside a previous starter pack (`.../StarterPacks/<Pack>/Definitions`). This made fresh-run authoring confusing.
  - Status: fixed in `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/GameplayStarterPackFactory.cs`.
- [ ] Manual path: verify first-run authoring flow still produces a clear "next step" from `Pyralis Authoring Window` without hidden assumptions.
- [ ] Manual path: confirm route steps for 2-player micro setup can be completed using only editor UI guidance and inspector fields (no manual script-side edits).

## Documentation Gaps

- [ ] Add a first-time creator-specific flow card for "start from clean scene" and "delete stale startup artifacts" before pressing Create.
- [ ] Add explicit note that nested selection inside existing sample folders should never produce nested starter packs.
- [ ] Add a 30-second "expected control flow" screenshot flow to `AUTHORING_MODEL.md` for the native Create/Map path.

## Tooling Gaps (Unity UI / Computer Use)

- [ ] No "create scene from scratch" one-click for this test; current path still depends on consistent manual layout preparation.
- [ ] Authoring flow should keep the Add Component search pattern visible in the same action area as parent-child setup notes.
- [ ] Computer-use layout: keep one clean test layout tab preset for authoring flows and avoid cross-tab context loss.

## Exit Criteria For Fresh Try

1. Scene root exists and is wired to `SharedSessionDefinition` (or equivalent route session).
2. Route flow in Overview/Map shows no required blockers (`Missing`/`Blocked`) for the chosen micro route.
3. One minimal playable proof is reached (spawn + movement/input or equivalent first interaction).
4. Console remains free of authoring-time hard blockers for the same route.
5. Notes below are updated with each blocker and the proposed improvement.

## Post-Run Improvement Pass (Notes)

- Pending
