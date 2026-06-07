# Pyralis Project-Wide Intent Upgrade Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade the Authoring Window Intent tab from route/proof-focused guidance into a project-wide, world-up capability composer that still renders from reflective facts and keeps Unity authoring in the user's hands.

**Architecture:** Keep `PyralisAuthoringWindow` as the UI shell and `PyralisAuthoringIntentAdvisor` as the ranking/read-model builder. Hardcode only the stable UI grammar and relationship vocabulary; keep concrete capabilities, route summaries, setup fields, customization moments, and proof targets in source-owned facts/providers.

**Tech Stack:** Unity Editor IMGUI, Pyralis editor-only authoring facts/providers, NUnit EditMode tests, active Pyralis setup docs.

---

### Task 1: Expand Intent Selection To Project Shape

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringIntentAdvisor.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringWindow.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/SetupFlowValidatorTests.cs`

- [x] Add editor-only intent enums for world/playfield and control shape.
- [x] Add those values to `PyralisAuthoringIntentSelection`.
- [x] Update scoring so world/control tags influence route-intent and capability rows before proof-target ranking.
- [x] Update summary copy to say "project intent" and "active focus" instead of treating proof as the top-level intent.
- [x] Add tests that side-view and top-down selections produce different primary intent readings.

### Task 2: Add Broader Source-Owned Intent Facts

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringIntentFacts.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`

- [x] Add world-up route intent facts for 2D top-down/free movement, 2.5D lane/arena, 3D space, tabletop/board/card, UI/menu-first, and hybrid/custom routes.
- [x] Use existing `GoalTags`, `LaneTags`, `RelatedStableIds`, assignment fields, customization moments, and can-wait lists instead of new runtime dependencies.
- [x] Add tests proving the facts are discoverable, unique, and project-wide rather than only 1P Sprite2D proof rows.

### Task 3: Make Intent UI Beginner-Friendly And Power-Useful

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringWindow.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringSourceContractTests.cs`

- [x] Render World / Playfield first, then Control Shape, then capability toggles.
- [x] Show a compact "Project Intent" summary separate from "Active Authoring Focus".
- [x] Rename "Recommended Path" to a project-wide label and explain that rows guide consequences, not presets.
- [x] Keep empty/conditional shelves visible with explanatory language rather than hidden or silent disabled states.
- [x] Add source-contract string tests for the new labels so regressions are visible.

### Task 4: Patch 2D Side-View Versus Top-Down Proof Readiness

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringSceneSurfaceSnapshot.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringRouteReport.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/SetupFlowValidatorTests.cs`

- [x] Keep spawn points from satisfying side-view gravity playfield evidence by themselves.
- [x] Avoid blocking top-down/no-gravity proof paths on a side-view ground/playfield row.
- [x] Update scene-surface and route-report tests with explicit movement profile setup so top-down and side-view expectations are separate.

### Task 5: Docs And Validation

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_MODEL.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_BLUEPRINT.md`

- [x] Update active docs to state that Intent is project-wide and world-up, while Overview/Validate/Proof are focused route lenses.
- [x] Preserve export-footprint guidance: facts/providers/validators remain editor-only and runtime references stay route-scoped.
- [x] Run a Unity refresh after C# edits.
- [x] Run focused authoring EditMode tests. Covered by the full pre-scene EditMode pass.
- [x] Run `.\Tools\Validation\Run-PreSceneValidation.ps1` once the Unity GUI editor is closed.

### Live Proof Notes

- [x] Computer Use live proof from a fresh scene confirmed the empty-scene Authoring Window starts in Intent, then lets the user choose Overview without being forced back into Intent.
- [x] Overview guided the native Unity path from `Gameplay Root` creation to `GameplaySessionBootstrap` assignment and then to SessionDefinition creation.
- [x] The native Create menu is consolidated under `NeonBlack`; definition menu order now follows the beginner setup spine with `Session Definition` first.
- [x] The proof exposed one remaining guidance improvement: field assignment should mention the object picker as an alternative to drag-and-drop for cramped Inspector layouts.
