# Pyralis Adaptive Inspector Guides Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor Pyralis setup guidance so the Inspector can guide beginners along the setup path they selected instead of forcing one hardwired game flow.

**Architecture:** Extend the shared `PyralisInspectorGuide` editor layer with collapsible manual sections. Use `RuntimePatternDefinition` and `GameSetupProfile` as the path-intent source, then apply adaptive guidance to the startup chain before expanding to prefab/runtime components.

**Tech Stack:** Unity 6000.4.0f1, C#, UnityEditor custom inspectors, ScriptableObject authoring assets, NUnit editor/source-contract tests, local dotnet builds.

---

### Task 1: Shared Collapsible Guide Layer

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisInspectorGuide.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] Add a reusable `PyralisGuideSection` data type for titled beginner-guide sections.
- [ ] Add `DrawGuidedManual` so inspectors can show a compact collapsed "Guided Setup Manual" with foldout sections.
- [ ] Add source-contract coverage so future inspectors keep using the shared guide layer instead of ad hoc help boxes.

### Task 2: Path-Aware Startup Chain Guidance

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/RuntimePatternDefinitionEditor.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/GameSetupProfileEditor.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/GameModeDefinitionEditor.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/SessionDefinitionEditor.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/GameplaySessionBootstrapEditor.cs`

- [ ] Add guided manual sections for what each asset/component is, required setup, valid path choices, Unity wiring, common mistakes, and manual links.
- [ ] Make `GameSetupProfileEditor` merge selected runtime patterns into readable next-step guidance.
- [ ] Make `GameplaySessionBootstrapEditor` inspect the assigned session graph and report required/recommended/optional path messages.

### Task 3: Validation Alignment

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/SessionDefinition.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/GameModeDefinition.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] Stop treating `defaultInputProfile` as universally required.
- [ ] Treat missing `GameModeDefinition.setupProfile` as a required beginner setup issue.
- [ ] Add tests proving no-pawn/tabletop sessions are not blocked by missing pawn-style input.

### Task 4: Docs Alignment

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/START_HERE.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_MODEL.md`

- [ ] Document that the Inspector contains the path-aware setup guide.
- [ ] Explain that runtime patterns choose the guidance path, while normal fields remain editable.

### Task 5: Verification

**Commands:**
- `dotnet build NeonBlack.Gameplay.Data.csproj --no-restore`
- `dotnet build NeonBlack.Gameplay.Editor.csproj --no-restore`
- `dotnet build NeonBlack.Gameplay.Editor.Tests.csproj --no-restore`
- `Invoke-UnityValidation.ps1 -ProjectPath . -Mode Refresh -NudgePath <changed files>`

- [ ] Builds pass.
- [ ] Unity refresh/log scan reports no project compiler errors.
