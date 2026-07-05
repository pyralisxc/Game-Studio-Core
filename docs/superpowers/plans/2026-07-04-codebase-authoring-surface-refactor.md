# Codebase Authoring Surface Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Simplify Game Studio Core's Unity-facing setup across the whole package by reducing scattered scene components, duplicate ownership, pass-through glue, and connection-hunting while preserving modular internal implementation.

**Architecture:** Refactor toward fewer visible Unity modules, clearer ScriptableObject profile ownership, and Unity-native prefab/sample/inspector workflows. Each phase is evidence-gated: first inventory, then one capability family at a time, then samples and docs. PYS remains an external observer of gameplay evidence, not a duplicate setup system.

**Tech Stack:** Unity 6000.4.0f1, C#, Unity packages, ScriptableObjects, MonoBehaviours, asmdefs, Unity Input System, Cinemachine, VContainer in Glue only, PYS Authoring as external package.

---

## Current Status

Last updated: 2026-07-05.

Completed evidence:

- Unity version has been confirmed as `6000.4.0f1`.
- PYS Authoring remains an external package dependency through `Packages/manifest.json` as `file:../../../Pys Authoring/Packages/com.pys.authoring`.
- The package-wide ownership audit exists at `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/AUTHORING_SURFACE_REFACTOR_AUDIT.md`.
- Living architecture docs and the gameplay README define the package-wide authoring-surface target instead of a pawn-only cleanup.
- The dedicated refactor changelog exists at `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/AUTHORING_SURFACE_REFACTOR_CHANGELOG.md` and records each committed slice.
- Feedback, presentation, collectibles, tabletop, route policy, profile metadata, camera, scene service, projectile, enemy, RPG item, pawn movement, traversal, combat tuning, movement/detection tuning, optional media, optional sink, and modular profile cleanup slices have been committed.
- Runtime-created object pressure has been classified in the audit as valid runtime output, reviewed setup repair, or deferred review.
- The obvious numeric/tuning `RequiredFields` false-positive scan is clean; remaining hits are primarily identifiers, definitions, UI/prefab/component references, physics masks, or concrete route services.
- Unity batch smoke resolves packages after the corrected external PYS Authoring package path.
- Focused runtime tests now protect the first wiring-report parity rules: canonical missing `SessionDefinition` rows suppress duplicate local validation rows; route-dependent camera guidance is deferred until route evidence exists; participant service and feature activation rows wait for a complete participant route; complete participant routes report concrete missing participant-service providers; PlayerInputManager plus multiple auto-join participants reports one semantic join timing issue; authored combat routes report combat feature activation; authored scoring routes report scoring, game-flow, and feedback service activation.

Remaining evidence before this goal can be called complete:

- Unity Editor compile and Unity Test Runner proof still need to run for `com.neonblackinteractivellc.neonblackhub`; batch Test Runner currently stalls on Unity Licensing Client reconnects before test discovery, so Editor Test Runner is the active proof surface.
- Package samples remain intentionally untouched; Cameron plans to hand-author samples from working code, so sample proof is not complete.
- Remaining higher-risk cleanup should focus on real ownership consolidation, stale-doc folding, and Unity proof rather than broad metadata sweeps.
- Existing unrelated local/generated changes remain outside this lane: `.plastic/*`, `Game Studio Core.slnx`, and `Tools/Validation/Run-PreSceneValidation.ps1`.

Unity Editor verification queue:

- `GameplayWiringReportBuilderTests` - proves canonical wiring report parity, route deferral, route-complete missing providers, participant join timing, and feature activation evidence.
- `FeedbackHudBindingTests` - proves the retained Feedback HUD ownership split still validates and binds through `ParticipantHealthPanel`.
- A full EditMode pass for `NeonBlack.Gameplay.Tests` after Unity imports the current package state.
- Manual Editor compile/package refresh for the external `com.pys.authoring` dependency path.

---

## Operating Rules

- Do not move, delete, or rename Unity assets without preserving matching `.meta` files.
- Do not rewrite scenes, prefabs, or ScriptableObject assets in bulk from scripts.
- Do not collapse the architecture into one large manager or one large profile.
- Do not make PYS responsible for local field errors Unity can show through inspectors or `OnValidate`.
- Prefer prefab references, serialized fields, `RequireComponent`, custom inspectors, package samples, and Unity Test Runner proof before custom framework behavior.
- Each implementation phase must be small enough to compile and test independently.
- Each phase should end with one commit.

## Target Shape

Visible Unity setup should move toward this pattern:

```text
Scene
+-- GameplaySessionBootstrap
+-- GameplayLifetimeScope
+-- Scene-owned services actually present in the scene
+-- Authored participant/pawn/camera/spawn objects

Pawn prefab
+-- PawnRoot
+-- PawnLocomotionModule
+-- PawnCombatModule
+-- PawnInteractionModule
+-- PawnPresentationModule
+-- Optional capability modules only when the capability is truly visible

Pawn module
+-- Serialized local wiring
+-- One or more assigned profiles
+-- Internal state machine / handlers / lanes
+-- Narrow command/event/reader/sink seams
```

Profile ownership should move toward this pattern:

```text
PawnDefinition
+-- PawnPresentationProfile
+-- PawnAnimationProfile
+-- PawnLocomotionProfile
+-- PawnCombatProfile
+-- PawnTraversalProfile
+-- PawnInteractionProfile
+-- PawnFeedbackProfile
```

Do not force every pawn to have every profile. Optional capability profiles should enable optional features by being assigned.

## Files And Areas

Primary architecture docs:

- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CURRENT_STATE_AUDIT.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/ARCHITECTURE_BLUEPRINT.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/README.md`

Likely runtime areas:

- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core`
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data`
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Glue`
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules`
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Presentation`
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Networking`

Likely test areas:

- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime`
- Add when needed: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor`

Likely sample areas:

- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Samples~`

## Phase Gates

Each phase has one required exit condition:

- Phase 0 exits when the repo compiles or the compile failure is documented as pre-existing.
- Phase 1 exits when every candidate script is classified as Owner, Profile, Adapter, Pass-through, Validator, Presentation, Scene Service, or Sample-only.
- Phase 2 exits when docs define the target authoring surface and no longer imply pawn-only cleanup.
- Phase 3 exits when the first capability family has fewer visible setup components without losing tests.
- Phase 4 exits when scene services stop creating setup objects to repair missing authoring.
- Phase 5 exits when validation is split into Unity-local warnings versus PYS semantic evidence.
- Phase 6 exits when package samples prove the simplified authoring path.
- Phase 7 exits when redundant docs/scripts are removed or folded into the living docs.

---

### Task 1: Baseline Compile And Package Inventory

**Files:**
- Read: `ProjectSettings/ProjectVersion.txt`
- Read: `Packages/manifest.json`
- Read: `Packages/packages-lock.json`
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/package.json`
- Read: all `*.asmdef` under `Packages/com.neonblackinteractivellc.neonblackhub`
- Produce: `docs/superpowers/plans/2026-07-04-codebase-authoring-surface-refactor.md`

- [x] **Step 1: Confirm Unity version**

Run:

```powershell
Get-Content -Path 'ProjectSettings/ProjectVersion.txt'
```

Expected: `m_EditorVersion: 6000.4.0f1`.

- [x] **Step 2: Confirm PYS remains external**

Run:

```powershell
Get-Content -Path 'Packages/manifest.json'
```

Expected: `com.pys.authoring` resolves through a `file:` path outside Game Studio Core.

- [x] **Step 3: List package assemblies**

Run:

```powershell
Get-ChildItem -Path 'Packages/com.neonblackinteractivellc.neonblackhub' -Recurse -Filter '*.asmdef' | Select-Object -ExpandProperty FullName
```

Expected: assemblies are grouped by Core, Data, Glue, Modules, Presentation, Networking, Editor, and Tests.

- [ ] **Step 4: Run the available Unity test gate**

Use Unity Test Runner from the Editor:

```text
Window > General > Test Runner > EditMode and PlayMode tests for com.neonblackinteractivellc.neonblackhub
```

Expected: current tests pass, or failures are recorded as baseline failures before refactoring starts.

- [x] **Step 5: Commit baseline plan**

Run:

```powershell
git add docs/superpowers/plans/2026-07-04-codebase-authoring-surface-refactor.md
git commit -m "docs: plan codebase authoring surface refactor"
```

Expected: one docs-only commit.

---

### Task 2: Create Ownership Inventory

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/AUTHORING_SURFACE_REFACTOR_AUDIT.md`
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core`
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data`
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Glue`
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules`
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Presentation`
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Networking`

- [x] **Step 1: Generate a component inventory**

Run:

```powershell
rg --files 'Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay' -g '*.cs'
```

Expected: list of gameplay C# files.

- [x] **Step 2: Find visible Unity components**

Run:

```powershell
rg -n 'class .*:.*MonoBehaviour|class .*:.*ScriptableObject|CreateAssetMenu|AddComponentMenu|MenuItem|CustomEditor' 'Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay'
```

Expected: list of all user-visible authoring surfaces.

- [x] **Step 3: Find pass-through and hidden setup pressure**

Run:

```powershell
rg -n 'AddComponent|new GameObject|FindObjectsByType|FindObjectOfType|DontDestroyOnLoad|InjectGameObject|InjectLoadedScene|IRuntimeValidationProvider|AuthoringContract' 'Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay'
```

Expected: list of scripts that need ownership classification.

- [x] **Step 4: Write the audit categories**

Create `AUTHORING_SURFACE_REFACTOR_AUDIT.md` with these sections:

```markdown
# Authoring Surface Refactor Audit

## Classification Rules

- Owner: owns gameplay behavior or lifecycle.
- Profile: owns authored tuning or reusable configuration.
- Adapter: converts one stable contract to another.
- Pass-through: forwards calls without owning rules.
- Validator: reports setup evidence.
- Presentation: displays state without owning gameplay rules.
- Scene Service: scene-authored service or scene-scale behavior.
- Sample-only: example content, not core package architecture.

## Owner Scripts

## Profile Assets

## Adapter Scripts

## Pass-through Candidates

## Validator Candidates

## Presentation Scripts

## Scene Services

## Sample-only Content

## First Refactor Candidates
```

Expected: the file exists and contains actual script names under each heading.

- [x] **Step 5: Commit audit**

Run:

```powershell
git add Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/AUTHORING_SURFACE_REFACTOR_AUDIT.md
git commit -m "docs: audit gameplay authoring surface ownership"
```

Expected: one docs-only commit.

---

### Task 3: Update Living Architecture For Whole-Codebase Scope

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CURRENT_STATE_AUDIT.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/ARCHITECTURE_BLUEPRINT.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/README.md`
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/AUTHORING_SURFACE_REFACTOR_AUDIT.md`

- [x] **Step 1: Add the package-wide refactor rule to architecture**

In `ARCHITECTURE_BLUEPRINT.md`, add a concise rule near the Inspector/daily surface section:

```markdown
Visible Unity setup should be smaller than the internal implementation. Prefer a few clear scene or prefab modules with assigned profiles over many sibling components that require the user to inspect code to understand ownership. Internal behavior may remain split into state machines, handlers, lanes, and private helper classes when that keeps the module readable.
```

Expected: architecture now names the visible-surface versus internal-modularity distinction.

- [x] **Step 2: Add the active cleanup lane to current state**

In `CURRENT_STATE_AUDIT.md`, add a maintenance focus entry:

```markdown
The active simplification lane is package-wide authoring surface reduction: fewer visible Unity setup components, clearer profile ownership, less pass-through glue, and stronger package samples. This applies to pawns, scene services, presentation, feedback, interaction, RPG, tabletop, networking, and editor tools.
```

Expected: current state no longer frames simplification as pawn-only.

- [x] **Step 3: Update README with the refactor target**

In `README.md`, add a short section:

```markdown
## Authoring Surface Direction

Game Studio Core should feel simple in Unity even when its internals are modular. Prefer clear scene roots, prefab modules, profile assets, and package samples over scattered setup scripts. PYS observes gameplay evidence from the package; it does not replace Unity's native Inspector, prefab, and sample workflows.
```

Expected: README communicates the direction to a new developer.

- [x] **Step 4: Run docs diff check**

Run:

```powershell
git diff --check
```

Expected: no whitespace errors.

- [x] **Step 5: Commit living doc update**

Run:

```powershell
git add Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CURRENT_STATE_AUDIT.md Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/ARCHITECTURE_BLUEPRINT.md Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/README.md
git commit -m "docs: define package-wide authoring surface direction"
```

Expected: one docs commit.

---

### Task 4: Refactor One Capability Family First

**Files:**
- Read: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/AUTHORING_SURFACE_REFACTOR_AUDIT.md`
- Modify: the first selected family under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules`
- Modify: related profiles under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data`
- Modify: related tests under `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime`

Recommended first family: Feedback or Presentation. Do not start with Locomotion, Combat, RPG, or Networking because those carry more gameplay risk.

- [x] **Step 1: Select the first family from the audit**

Use this selection rule:

```text
Pick the family with the most pass-through scripts and the least scene/prefab asset risk.
```

Expected: one family is selected and named in `AUTHORING_SURFACE_REFACTOR_AUDIT.md` under `First Refactor Candidates`.

- [ ] **Step 2: Write a focused failing test when runtime behavior changes**

Add or modify a test in `Tests/Runtime` that proves the selected family still routes profile data, events, or state correctly.

Expected: the test fails before implementation when the existing behavior cannot satisfy the new module/profile ownership rule.

- [ ] **Step 3: Merge pass-through scripts into the real owner**

Apply this rule:

```text
If a script only forwards calls, stores fallback data already owned by a profile, or exists because two systems lack a narrow contract, merge or delete it.
```

Expected: fewer visible components or clearer profile ownership in the selected family.

- [ ] **Step 4: Preserve the Unity-facing API when serialized assets depend on it**

Before renaming serialized fields or removing MonoBehaviours, search references:

```powershell
rg -n 'OldTypeName|oldFieldName' 'Packages/com.neonblackinteractivellc.neonblackhub'
```

Expected: affected scenes, prefabs, and assets are known before edits.

- [ ] **Step 5: Run tests**

Use Unity Test Runner:

```text
Window > General > Test Runner > affected EditMode/PlayMode tests
```

Expected: affected tests pass.

- [ ] **Step 6: Commit the first family refactor**

Run:

```powershell
git add Packages/com.neonblackinteractivellc.neonblackhub
git commit -m "refactor: simplify first gameplay authoring family"
```

Expected: one focused refactor commit.

---

### Task 5: Simplify Scene Services And Runtime-Created Setup

**Files:**
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Glue/SceneFlow`
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Glue/SceneServices`
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Presentation`
- Inspect/modify: `Packages/com.neonblackinteractivellc.neonblackhub/Samples~`

- [x] **Step 1: List runtime-created objects**

Run:

```powershell
rg -n 'new GameObject|AddComponent<|AddComponent\\(' 'Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Glue' 'Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Presentation' 'Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules'
```

Expected: each runtime-created object is classified as gameplay output or hidden setup repair.

- [x] **Step 2: Keep valid gameplay output**

Allowed runtime output:

```text
spawned pawns, projectiles, pooled effects, world-space popups, fade overlays, generated board/pickup views, camera focus helper transforms
```

Expected: valid runtime output remains.

- [ ] **Step 3: Replace hidden setup repair with prefab or scene references**

For hidden setup repair, replace auto-created setup objects with:

```text
serialized prefab reference, scene service reference, RequireComponent, OnValidate warning, or package sample object
```

Expected: missing setup is visible in the Inspector or sample scene.

- [ ] **Step 4: Run affected tests and Unity scene proof**

Use Unity Test Runner and a manual Play Mode pass for scenes touched by the change.

Expected: tests pass, and touched scene services still run when correctly authored.

- [ ] **Step 5: Commit scene service cleanup**

Run:

```powershell
git add Packages/com.neonblackinteractivellc.neonblackhub
git commit -m "refactor: prefer authored scene services over hidden setup"
```

Expected: one focused scene-service commit.

---

### Task 6: Split Validation Into Unity-Local And PYS-Semantic Evidence

**Files:**
- Inspect/modify: scripts implementing `IRuntimeValidationProvider`
- Inspect/modify: scripts using `Pys.Authoring.Contracts.AuthoringContractAttribute`
- Modify when needed: local custom inspectors under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor`
- Modify when needed: docs under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs`

- [ ] **Step 1: List validation providers and contracts**

Run:

```powershell
rg -n 'IRuntimeValidationProvider|GetRuntimeValidationIssues|AuthoringContract' 'Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay'
```

Expected: every validation provider and contract-visible script is known.

- [ ] **Step 2: Classify each validation message**

Use this classification:

```text
Unity-local: missing serialized field, missing sibling component, invalid number range, missing prefab reference.
PYS-semantic: route contradiction, cross-object setup requirement, participant topology, proof target, capability evidence, setup-stage readiness.
```

Expected: each provider has a clear reason to remain runtime/PYS-facing or move to local inspector/OnValidate.

- [ ] **Step 3: Move field-local warnings closer to the field**

Use `OnValidate`, `RequireComponent`, or custom inspector warnings for Unity-local issues.

Expected: PYS-visible validation gets smaller and more semantic.

- [ ] **Step 4: Keep route-level evidence in PYS contracts/providers**

Keep PYS evidence when the issue cannot be understood from one inspected object.

Expected: PYS still explains cross-object setup and proof readiness.

- [ ] **Step 5: Commit validation cleanup**

Run:

```powershell
git add Packages/com.neonblackinteractivellc.neonblackhub
git commit -m "refactor: separate local inspector validation from setup evidence"
```

Expected: one focused validation commit.

---

### Task 7: Build Package Samples As The Main Navigation Surface

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Samples~`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/README.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/README.md`

Sample targets:

```text
Samples~/Sprite2D Starter
Samples~/Local Join Starter
Samples~/Presentation Feedback Starter
Samples~/Scene Flow Starter
```

- [ ] **Step 1: Define the first sample**

Create or update `Samples~/Sprite2D Starter` with:

```text
one scene,
one pawn prefab,
one SessionDefinition,
one ParticipantDefinition,
one PawnDefinition,
one InputProfile,
one CameraRigProfile,
one minimal README.
```

Expected: the sample demonstrates the simplified authoring surface.

- [ ] **Step 2: Keep sample content package-safe**

Confirm sample content is under `Samples~` and not always imported by package consumers.

Expected: optional content stays optional.

- [ ] **Step 3: Verify import path in Unity**

Use Unity Package Manager:

```text
Window > Package Manager > Game Studio Core > Samples > Import
```

Expected: sample imports without missing scripts.

- [ ] **Step 4: Manual Play Mode proof**

Open the imported sample scene and press Play.

Expected: sample demonstrates the route it claims to demonstrate.

- [ ] **Step 5: Commit sample**

Run:

```powershell
git add Packages/com.neonblackinteractivellc.neonblackhub
git commit -m "samples: add simplified Sprite2D starter route"
```

Expected: one sample commit.

---

### Task 8: Repeat Family Refactors By Risk Order

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/AUTHORING_SURFACE_REFACTOR_AUDIT.md`
- Modify: selected family files under `Core`, `Data`, `Glue`, `Modules`, `Presentation`, or `Networking`
- Modify: focused tests under `Packages/com.neonblackinteractivellc.neonblackhub/Tests`

Recommended order:

```text
1. Feedback and simple Presentation
2. Scene Flow and Scene Services
3. Interaction and Collectibles
4. Scoring and runtime event/sink seams
5. Input routing and participant handoff
6. Locomotion and traversal profiles
7. Combat profiles and action lanes
8. Enemy behavior ownership
9. RPG optional domain
10. Tabletop optional domain
11. Networking optional extension
12. Editor tools
```

- [ ] **Step 1: Select one family**

Expected: only one family is active in the diff.

- [ ] **Step 2: Identify visible components**

Run:

```powershell
rg -n 'class .*:.*MonoBehaviour|CreateAssetMenu|AddComponentMenu|CustomEditor|MenuItem' '<selected-family-path>'
```

Expected: current Unity-facing surface is known.

- [ ] **Step 3: Identify duplicate owners**

Search for:

```powershell
rg -n 'fallback|default|auto|Forward|Router|Adapter|Manager|Service|Validate|GetRuntimeValidationIssues' '<selected-family-path>'
```

Expected: pass-through and duplicate setup candidates are known.

- [ ] **Step 4: Refactor only one ownership problem**

Apply exactly one of these actions:

```text
merge pass-through into owner,
move tunable settings into profile,
move implementation-only helper private/internal,
replace hidden setup with prefab/serialized reference,
replace direct module reference with command/event/reader/sink,
move local field warning out of PYS validation.
```

Expected: focused diff with clear before/after ownership.

- [ ] **Step 5: Verify and commit**

Run affected Unity tests, perform required manual Editor proof, then commit.

Expected: one small commit per ownership problem.

---

### Task 9: Cold-Cut Stale Docs And Obsolete Helpers

**Files:**
- Inspect/modify/delete: stale docs under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs`
- Inspect/modify/delete: obsolete editor helpers under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor`
- Inspect/modify/delete: obsolete runtime helpers under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay`

- [x] **Step 1: Find stale docs and old route language**

Run:

```powershell
rg -n 'old|legacy|deprecated|temporary|compatibility|migration|PYS embedded|embedded PYS|authoring package moved|stale note|obsolete note' 'Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs' 'Packages/com.neonblackinteractivellc.neonblackhub/README.md'
```

Expected: stale wording is known.

- [ ] **Step 2: Fold useful truth into active docs**

Keep current behavior, current architecture, current workflow, and current next direction.

Expected: no parallel stale truth remains.

- [ ] **Step 3: Remove obsolete helpers only when superseded**

Before deleting a helper, search references:

```powershell
rg -n 'TypeOrFileNameWithoutExtension' 'Packages/com.neonblackinteractivellc.neonblackhub'
```

Expected: deletion is safe or the helper remains.

- [ ] **Step 4: Run diff and test checks**

Run:

```powershell
git diff --check
```

Expected: no whitespace errors.

- [ ] **Step 5: Commit cleanup**

Run:

```powershell
git add Packages/com.neonblackinteractivellc.neonblackhub
git commit -m "docs: cold-cut stale authoring surface guidance"
```

Expected: one cleanup commit.

---

## Done Criteria

This refactor lane is done when:

- A new developer can start from package samples instead of reading architecture docs first.
- Pawn, scene service, presentation, interaction, feedback, optional domain, and editor setup surfaces are classified.
- Visible scene/prefab setup has fewer, clearer owner components.
- Optional capability profiles enable optional behavior without one giant profile.
- Runtime-created setup repair has been removed or justified as valid runtime output.
- Local field issues appear near local fields.
- PYS evidence focuses on semantic route, proof, topology, and cross-object setup truth.
- Unity Test Runner passes for affected areas.
- Manual Play Mode proof exists for changed samples and authoring flows.
- Living docs describe current behavior only.

## Execution Recommendation

Use subagent-driven execution by phase, with review after each phase. Do not execute multiple runtime refactor families in the same branch unless the first family is already merged and verified.
