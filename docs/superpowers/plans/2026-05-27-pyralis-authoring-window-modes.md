# Pyralis Authoring Window Modes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn the Pyralis Authoring Window into a mode-based setup UI layer that centralizes setup intelligence without duplicating Inspector field editing.

**Architecture:** Keep `PyralisAuthoringRouteReport` and `PyralisSetupRouteAnalysis` as the shared setup brain. The current product direction uses `Overview`, `Guide`, `Map`, and `Validate` modes only. Asset and scene creation belong to native Unity Project/Hierarchy Create menus and Inspector assignment fields, with the Authoring Window naming those steps instead of owning a Create tab.

**Tech Stack:** Unity Editor IMGUI, C#, NUnit source contract tests, Pyralis setup docs.

---

### Task 1: Contract And Docs

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringSourceContractTests.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_MODEL.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/START_HERE.md`

- [ ] **Step 1: Write the failing source contract**

Add assertions that `PyralisAuthoringWindow.cs` contains `Overview`, `Guide`, `Map`, `Validate`, `DrawGuideMode`, `DrawMapMode`, `DrawValidateMode`, and native workflow guidance, and does not contain `EditorGUILayout.ObjectField` or Authoring Window create-and-assign actions.

- [ ] **Step 2: Run the source contract script**

Run a PowerShell source check against `PyralisAuthoringWindow.cs`.
Expected: FAIL until the mode methods and labels exist.

- [ ] **Step 3: Update docs to define the product boundary**

State that the Authoring Window is the setup UI layer with `Overview`, `Guide`, `Map`, and `Validate` modes. State that Inspectors are field editors with compact field-local help and native Unity Create/Add Component/field assignment remains the creation path.

### Task 2: Authoring Window Modes

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringWindow.cs`

- [ ] **Step 1: Add mode state**

Add an enum with `Overview`, `Guide`, `Map`, and `Validate`, a selected mode field, and a toolbar.

- [ ] **Step 2: Split the existing UI into mode methods**

Move route report and next action flow into `DrawGuideMode`, setup topology/status into `DrawMapMode`, and validation issue rendering into `DrawValidateMode`. Creation guidance should point to native Unity menus and Inspector fields, not `DrawCreateMode`.

- [ ] **Step 3: Keep the no-second-Inspector rule**

Do not add `EditorGUILayout.ObjectField` or broad property drawers to the Authoring Window. Use labels, statuses, and explicit `Inspect Asset` jumps only.

### Task 3: Verification

**Files:**
- Verify: `Game Studio Core.slnx`
- Verify: Unity Editor refresh through `Invoke-UnityValidation.ps1`

- [ ] **Step 1: Run source checks**

Run PowerShell checks for mode labels/methods, docs alignment, and no `EditorGUILayout.ObjectField`.

- [ ] **Step 2: Build**

Run `dotnet build "Game Studio Core.slnx" --no-restore`.
Expected: 0 warnings, 0 errors.

- [ ] **Step 3: Refresh Unity**

Run `Invoke-UnityValidation.ps1 -Mode Refresh` with changed files.
Expected: fresh Tundra build success and no compiler/test error patterns.
