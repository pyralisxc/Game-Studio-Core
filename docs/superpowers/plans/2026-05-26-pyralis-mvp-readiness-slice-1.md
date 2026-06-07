# Pyralis MVP Readiness Slice 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create the active package-local MVP readiness matrix and route audit for Game Shell, Pawn-Backed Action across all three runtime lanes, and Non-Pawn Tabletop.

**Architecture:** This slice is a documentation and contract-test checkpoint. It promotes the approved MVP readiness design from the planning archive into the active Pyralis package docs, then adds source-contract coverage so the matrix keeps naming the route/lane bar that scene work depends on.

**Tech Stack:** Unity 6000.4 project, Pyralis package docs, C# Unity EditMode/source-contract tests, PowerShell validation scripts.

---

## Scope

This plan implements Slice 1 from `docs/superpowers/specs/2026-05-26-pyralis-mvp-readiness-design.md`.

It does not add menu UI, credits UI, pawn prefab changes, tabletop scene presentation changes, or new runtime behavior. Those belong to later slices after the matrix and audit make the weak spots visible.

## Files

- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
  - Owns the active MVP route/lane readiness matrix.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`
  - Aligns the current checkpoint gate with Beginner Prototype Ready through guided Unity setup.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CURRENT_STATE_AUDIT.md`
  - Records the new gate as the current pre-scene development standard.
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Tests/Editor/MvpReadinessDocsContractTests.cs`
  - Verifies the active readiness docs keep the approved MVP route/lane vocabulary.
- Verify: `Tools/Validation/Run-PreSceneValidation.ps1`
  - Preferred full validation gate after docs and tests are updated.

## Task 1: Update The Runtime Parity Matrix

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`

- [ ] **Step 1: Replace the parity-level language with MVP status language**

Update the top of `RUNTIME_PARITY_MATRIX.md` so the status vocabulary matches the approved readiness design:

```markdown
# Pyralis Runtime Parity Matrix

This matrix tracks whether Pyralis routes and runtime lanes are ready for Beginner Prototype Ready through guided Unity setup.

## MVP Status Labels

- `Ready`: authored, guided, validated, and proven.
- `Guided Needs Proof`: setup exists, but runtime proof or scene/sample proof is thin.
- `Foundation Only`: core code exists, but beginner authoring is not real yet.
- `Not Started`: missing as a platform capability.
- `Deferred`: intentionally outside the current MVP.

## Five-Part Completion Bar

Every route or runtime lane must satisfy the same bar before it can be marked `Ready`:

- `Runtime`: the code actually executes the loop.
- `Authoring`: creators can assemble the route in Unity from assets, prefabs, and components.
- `Guidance`: inspectors or docs explain what to create, assign, and leave empty.
- `Validation`: common wrong wiring produces actionable warnings before or during first Play Mode.
- `Proof`: tests, validation gates, or reference scenes prove the route works.

## MVP Route Dimensions

The MVP readiness gate tracks these dimensions:

- Game Shell
- Pawn-Backed Action / `Sprite2D`
- Pawn-Backed Action / `Billboard2_5D`
- Pawn-Backed Action / `Rigged3D`
- Non-Pawn Tabletop
```

- [ ] **Step 2: Add the route/lane MVP matrix**

Add this section before the existing capability matrix, or replace the existing `## Current Matrix` section if keeping both would make the doc confusing:

```markdown
## MVP Route Matrix

| Capability | Game Shell | Pawn Action Sprite2D | Pawn Action Billboard2_5D | Pawn Action Rigged3D | Non-Pawn Tabletop |
| --- | --- | --- | --- | --- | --- |
| Route setup profile | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Starter pack | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Scene root setup | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Session and participant setup | Foundation Only | Ready | Ready | Ready | Guided Needs Proof |
| Pawn or no-pawn correctness | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Prefab requirements | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Input ownership | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Foundation Only |
| Movement | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Camera | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Foundation Only |
| Presentation and animation | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Foundation Only |
| Health, damage, and defeat | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Combat or interaction | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Projectiles/guns | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Scoring/HUD | Foundation Only | Foundation Only | Foundation Only | Foundation Only | Foundation Only |
| Board/rules/action queue | Deferred | Deferred | Deferred | Deferred | Guided Needs Proof |
| Turns/phases | Deferred | Deferred | Deferred | Deferred | Foundation Only |
| Menu/loading/settings/credits | Guided Needs Proof | Foundation Only | Foundation Only | Foundation Only | Foundation Only |
| Scene flow | Guided Needs Proof | Foundation Only | Foundation Only | Foundation Only | Foundation Only |
| Setup flow validation | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Scene/prefab readiness validation | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Docs | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| EditMode proof | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| PlayMode proof | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Known limitations | Ready | Ready | Ready | Ready | Ready |
```

- [ ] **Step 3: Add route audit summaries**

Add this section after the MVP route matrix:

```markdown
## Route Audit Summary

### Game Shell

Current status: `Guided Needs Proof`.

Strong foundations:

- `SceneLoader`, `SceneFader`, `LoadingScreenController`, `MainMenuManager`, `SettingsManager`, and `SettingsScreen` exist.
- Guided inspectors already explain scene navigation, loading, menu, and settings wiring.
- Scene-flow docs already describe LevelData, LevelRegistry, SceneFader, and `ISceneNavigator`.

Main blockers before `Ready`:

- credits are not a first-class guided shell page yet
- shell setup does not have a single beginner route that covers boot, loading, menu, settings, credits, and game scene transition together
- PlayMode or scene validation proof for the complete shell route is thin

### Pawn-Backed Action / Sprite2D

Current status: `Guided Needs Proof`.

Strong foundations:

- 2D pawn movement, input, combat, pickup, hazard, projectile, scoring, and camera pieces exist.
- Starter-pack generation creates a spawnable pawn prefab and projectile-related assets.
- Projectile and 2D runtime tests exist for important foundations.

Main blockers before `Ready`:

- beginner route needs one clear Sprite2D prefab checklist
- scoring/HUD and scene-flow proof need to be tied to the route
- local participant and projectile setup need a beginner-readable proof path

### Pawn-Backed Action / Billboard2_5D

Current status: `Guided Needs Proof`.

Strong foundations:

- `PawnPresentationProfile` names `Billboard2_5D` as an official presentation lane.
- 3D pawn composition and camera foundations can carry the lane.
- Animation/presentation profile wiring exists.

Main blockers before `Ready`:

- beginner route needs explicit Billboard2_5D prefab and presentation guidance
- validation must catch presentation/profile/component mismatches
- proof must show movement, camera, presentation, damage/combat or interaction, and scene flow in this lane

### Pawn-Backed Action / Rigged3D

Current status: `Guided Needs Proof`.

Strong foundations:

- 3D pawn movement, traversal, camera, combat, projectile, and Animator-driven presentation foundations exist.
- Rigged 3D is an official presentation lane.
- 3D projectile launcher and movement foundations have tests.

Main blockers before `Ready`:

- beginner route needs explicit Rigged3D prefab and Animator mapping guidance
- validation must catch missing Animator/profile/component wiring
- proof must show the same action-route bar as Sprite2D and Billboard2_5D

### Non-Pawn Tabletop

Current status: `Guided Needs Proof`.

Strong foundations:

- board definitions, piece definitions, move policies, turn order, terminal conditions, action queue, board move resolver, selection bridge, and grid presenter exist.
- tabletop starter pack creates baseline rules assets.
- docs explicitly state that no pawn is required.

Main blockers before `Ready`:

- beginner route needs a single from-scratch tabletop walkthrough
- no-pawn validation must stay free of pawn/spawn false positives
- proof must show board selection, action queue, legal move resolution, turn flow, and terminal condition behavior in Unity-facing setup
```

- [ ] **Step 4: Preserve detailed feature inventory only as supporting context**

If the old capability rows are kept, place them under:

```markdown
## Supporting Capability Inventory
```

Keep the old rows useful, but make the MVP Route Matrix the first matrix readers see.

- [ ] **Step 5: Check the doc for accidental old status drift**

Run:

```powershell
rg -n "Production|Playable foundation|Foundation\\||Level \\|" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md"
```

Expected:

- No `Production` parity level section remains.
- If old capability inventory rows remain, `Playable foundation` appears only in the supporting inventory, not in the MVP route matrix.

## Task 2: Align Core Readiness Checkpoints

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`

- [ ] **Step 1: Update the purpose and product promise**

Replace the opening purpose/product promise with:

```markdown
# Pyralis Core Package Readiness Checkpoints

Date: 2026-05-26

This document is the active product-scope guardrail for Pyralis before deeper scene building begins.

The goal is **Beginner Prototype Ready through guided Unity setup**. Pyralis should not make a complete game for a creator. It should guide a creator through the assets, scene roots, prefabs, components, references, and validation required to make their own basic playable prototype.

## Product Promise

Pyralis should let a creator open Unity, choose a supported game route, and follow guided setup to build a prototype without needing to understand the internal framework architecture.

The MVP supported routes are:

- Game Shell
- Pawn-Backed Action across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`
- Non-Pawn Tabletop

The pawn-backed route is not ready unless all three official runtime lanes satisfy the same readiness bar.
```

- [ ] **Step 2: Replace the checkpoint definition with the five-part bar**

Use this exact section:

```markdown
## Checkpoint Definition Of Done

Every readiness checkpoint must meet the same five-part bar:

- `Runtime`: the code actually executes the loop.
- `Authoring`: creators can assemble the route in Unity from assets, prefabs, and components.
- `Guidance`: inspectors or docs explain what to create, assign, and leave empty.
- `Validation`: common wrong wiring produces actionable warnings before or during first Play Mode.
- `Proof`: tests, validation gates, or reference scenes prove the route works.

The active route/lane status lives in `RUNTIME_PARITY_MATRIX.md`.
```

- [ ] **Step 3: Add the three MVP route checkpoints**

Insert this before any older checkpoint sections:

```markdown
## Checkpoint 1: Game Shell MVP

Purpose: prove every Pyralis game can start from a guided boot, loading, menu, settings, credits, and scene-flow path.

Required before this checkpoint is `Ready`:

- one beginner route covering boot scene, loading scene, main menu, settings, credits, and gameplay scene transition
- guided inspectors and docs for scene navigation, settings source assignment, button/page wiring, and Build Settings
- credits page or panel support in the shell route
- validation for missing scene names, missing navigator source, missing settings source, and missing required shell UI references
- PlayMode or scene-readiness proof for menu-to-loading-to-game and return/restart flow

## Checkpoint 2: Pawn-Backed Action MVP

Purpose: prove one authoring model can create pawn-backed action prototypes in all official runtime lanes.

Official runtime lanes:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

Required before this checkpoint is `Ready`:

- beginner prefab/setup checklist for every lane
- session, participant, pawn definition, pawn prefab, input, movement, camera, presentation, health/damage, combat or interaction, projectile, scoring/HUD, and scene-flow guidance for every lane
- lane-specific validation for missing or mismatched presentation profiles, animation profiles, pawn components, launchers, cameras, and scoring/HUD dependencies
- proof that every lane can reach a small playable loop

## Checkpoint 3: Non-Pawn Tabletop MVP

Purpose: prove Pyralis supports games where participants are seats, sides, factions, cursors, or board players instead of pawn owners.

Required before this checkpoint is `Ready`:

- no-pawn setup route through `SessionDefinition`, `GameModeDefinition`, `GameSetupProfile`, and participant definitions
- board definition, pieces, move policy, action queue, turn order, selection surface, and terminal condition guidance
- validation that avoids pawn and spawn-point false positives for no-pawn routes
- proof that board selection can queue and resolve a legal move, advance or respect turn flow, and reach a terminal condition
```

- [ ] **Step 4: Preserve useful older checkpoint details as route backlog**

Move older checkpoint details under:

```markdown
## Supporting Backlog From Earlier Checkpoints
```

Keep details about tabletop, local two-player shooter, FPS/3D projectile, authoring UX, and runtime parity because they are useful. Do not let them appear as the primary readiness gate above the three MVP routes.

- [ ] **Step 5: Update the active development gate**

Replace the active development gate order with:

```markdown
## Active Development Gate

Near-term work should advance these checkpoints in order:

1. Game Shell MVP, because every future prototype needs loading, menu, settings, credits, and scene flow.
2. Pawn-Backed Action MVP across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`, because the official pawn route must be lane-honest before game scene work depends on it.
3. Non-Pawn Tabletop MVP, because tabletop support must remain a real no-pawn authoring path.
4. Friend trial and friction capture, once the three routes have proof paths.
5. Package docs alignment after each slice, so active docs never fall behind the real authoring surface.
```

## Task 3: Record The Gate In The Current State Audit

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CURRENT_STATE_AUDIT.md`

- [ ] **Step 1: Add a current-state bullet**

In the resolved/current architecture section, add this bullet near the latest pre-scene readiness bullets:

```markdown
- MVP readiness is now defined as Beginner Prototype Ready through guided Unity setup: the active gates are Game Shell, Pawn-Backed Action across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`, and Non-Pawn Tabletop; each route must satisfy runtime, authoring, guidance, validation, and proof before it can be called ready.
```

- [ ] **Step 2: Update the recommended order of attack**

Replace or prepend the recommendation list with:

```markdown
1. Promote the MVP readiness matrix and route audit into active package docs, then use it as the control panel for scene-readiness work.
2. Harden the Game Shell route first: boot/loading/menu/settings/credits, scene navigation, Build Settings guidance, and shell proof.
3. Bring Pawn-Backed Action to parity across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`; do not call the route ready while one lane is only partially proven.
4. Finish Non-Pawn Tabletop as a guided no-pawn prototype route with board selection, action queue, turn flow, and terminal condition proof.
5. Run a beginner/friend trial or simulated beginner pass and feed the friction back into inspectors, setup flow, docs, and starter packs.
```

- [ ] **Step 3: Keep older recommendations as lower-priority context**

If older recommendations still apply, keep them after the MVP list under:

```markdown
After the MVP route gates are moving, continue these standing hardening tracks:
```

Then list service ownership, primary-player assumptions, tests, networking enhancement, and asmdef enforcement as supporting tracks.

## Task 4: Add Source Contract Tests For Readiness Docs

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Tests/Editor/MvpReadinessDocsContractTests.cs`

- [ ] **Step 1: Find the package editor test assembly**

Run:

```powershell
Get-ChildItem "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay" -Recurse -Filter "*Editor.Tests.asmdef" | Select-Object FullName
```

Expected:

- One editor test assembly path is returned under the Pyralis/Gameplay package or the broader package test structure.

- [ ] **Step 2: Create the source contract test file**

Create `MvpReadinessDocsContractTests.cs` in the same editor test folder pattern used by existing source contract tests. If the exact folder is not present, create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Tests/Editor/`.

Use this complete file:

```csharp
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class MvpReadinessDocsContractTests
    {
        private static readonly string DocsRoot = Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub",
            "Members",
            "Pyralis",
            "Gameplay",
            "Docs");

        [Test]
        public void RuntimeParityMatrixNamesMvpRoutesAndRuntimeLanes()
        {
            string path = Path.Combine(DocsRoot, "RUNTIME_PARITY_MATRIX.md");
            string text = File.ReadAllText(path);

            StringAssert.Contains("Game Shell", text);
            StringAssert.Contains("Pawn-Backed Action / `Sprite2D`", text);
            StringAssert.Contains("Pawn-Backed Action / `Billboard2_5D`", text);
            StringAssert.Contains("Pawn-Backed Action / `Rigged3D`", text);
            StringAssert.Contains("Non-Pawn Tabletop", text);
        }

        [Test]
        public void RuntimeParityMatrixDefinesFivePartCompletionBar()
        {
            string path = Path.Combine(DocsRoot, "RUNTIME_PARITY_MATRIX.md");
            string text = File.ReadAllText(path);

            StringAssert.Contains("Runtime", text);
            StringAssert.Contains("Authoring", text);
            StringAssert.Contains("Guidance", text);
            StringAssert.Contains("Validation", text);
            StringAssert.Contains("Proof", text);
        }

        [Test]
        public void CoreReadinessCheckpointsPreserveBeginnerPrototypeReadyPromise()
        {
            string path = Path.Combine(DocsRoot, "CORE_PACKAGE_READINESS_CHECKPOINTS.md");
            string text = File.ReadAllText(path);

            StringAssert.Contains("Beginner Prototype Ready through guided Unity setup", text);
            StringAssert.Contains("Game Shell MVP", text);
            StringAssert.Contains("Pawn-Backed Action MVP", text);
            StringAssert.Contains("Non-Pawn Tabletop MVP", text);
            StringAssert.Contains("all three official runtime lanes", text);
        }
    }
}
```

- [ ] **Step 3: Verify new test file has a `.meta` file**

If Unity has not generated a `.meta` file yet, create one by following the repo's existing `.meta` pattern for C# scripts. Use `New-Guid` in PowerShell to generate a unique GUID:

```powershell
[guid]::NewGuid().ToString("N")
```

Expected `.meta` shape:

```yaml
fileFormatVersion: 2
guid: GENERATED_32_CHARACTER_GUID
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
```

Replace `GENERATED_32_CHARACTER_GUID` with the generated value.

- [ ] **Step 4: Run a fast source scan for the contract vocabulary**

Run:

```powershell
rg -n "Beginner Prototype Ready|Pawn-Backed Action / `Sprite2D`|Pawn-Backed Action / `Billboard2_5D`|Pawn-Backed Action / `Rigged3D`|Non-Pawn Tabletop" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs"
```

Expected:

- `RUNTIME_PARITY_MATRIX.md` and `CORE_PACKAGE_READINESS_CHECKPOINTS.md` both appear in results.

## Task 5: Run Validation

**Files:**
- Verify: project validation output under `Logs/Codex`

- [ ] **Step 1: Run focused text checks**

Run:

```powershell
rg -n "T[B]D|T[O]DO|\\?\\?|place[h]older" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CURRENT_STATE_AUDIT.md"
```

Expected:

- No output.

- [ ] **Step 2: Run the project pre-scene validation gate**

Close the Unity GUI Editor first. Then run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected:

- dotnet restore/build completes
- Unity EditMode tests complete
- Unity PlayMode tests complete
- final restore/build completes
- no generated residue is reported as blocking

- [ ] **Step 3: If the full gate cannot run, run the narrowest available checks**

If Unity is open or the full gate is blocked, run:

```powershell
dotnet restore "Game Studio Core.slnx"
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected:

- Build succeeds.

Report that Unity EditMode/PlayMode evidence is missing and name the residual risk.

## Task 6: Checkpoint And Handoff

**Files:**
- Review: all modified docs and tests

- [ ] **Step 1: Show changed files**

Run:

```powershell
git -C "." status --short
```

Expected:

- If this workspace is a Git repository, changed docs and test files are listed.
- If the command reports `fatal: not a git repository`, record that the workspace is not currently git-backed from this shell and do not attempt a commit.

- [ ] **Step 2: Commit when Git is available**

If Git is available, run:

```powershell
git -C "." add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CURRENT_STATE_AUDIT.md" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Tests/Editor/MvpReadinessDocsContractTests.cs" "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Tests/Editor/MvpReadinessDocsContractTests.cs.meta"
git -C "." commit -m "docs: define pyralis mvp readiness matrix"
```

Expected:

- Commit succeeds with the listed files.

- [ ] **Step 3: Summarize completion**

Report:

- the MVP route/lane matrix is active in package docs
- the core readiness checkpoints now use Beginner Prototype Ready through guided Unity setup
- the current state audit points future work at the MVP gates
- source contract tests protect the route/lane vocabulary
- validation evidence and any residual risk

## Self-Review Checklist

- [ ] The plan implements Slice 1 only and does not sneak in runtime feature work.
- [ ] The plan names all three official pawn runtime lanes.
- [ ] The plan keeps the matrix package-local, not only in `docs/superpowers`.
- [ ] The plan requires validation evidence before claiming completion.
- [ ] The plan accounts for the workspace currently not appearing as a Git repository from shell.
