# Pyralis Board Terminal Condition Checkpoint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Advance the Rules-Driven Tabletop MVP by adding Unity-authorable board terminal conditions for baseline win/loss states.

**Architecture:** Core owns actor-agnostic terminal-condition evaluation. Data owns the ScriptableObject authoring asset that creates runtime evaluators. GameMode validation links terminal conditions into the authored rules surface. Editor owns guided inspector coverage.

**Tech Stack:** Unity 6000.4, C#, NUnit, ScriptableObject authoring, existing Pyralis board runtime.

---

## File Structure

- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardTerminalConditionKind.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardTerminalEvaluationResult.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/IBoardTerminalCondition.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardTerminalCondition.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardTerminalConditionDefinition.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/GameModeDefinition.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/CoreRulesDefinitionEditors.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/CoreRulesRuntimeTests.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/CoreRulesDefinitionTests.cs`

### Task 1: Runtime Terminal Conditions

- [x] **Step 1: Write failing runtime tests**

Added tests proving:

- side-eliminated returns the configured winning seat
- objective-occupied returns the occupying seat when no winner override is assigned

- [x] **Step 2: Implement runtime evaluator**

Added:

- `BoardTerminalConditionKind`
- `BoardTerminalEvaluationResult`
- `IBoardTerminalCondition`
- `BoardTerminalCondition`

### Task 2: Unity Authoring Asset

- [x] **Step 1: Write failing editor tests**

Added tests proving:

- `BoardTerminalConditionDefinition` creates a runtime condition
- invalid side-eliminated seat setup reports validation issues
- `GameModeDefinition` includes terminal-condition validation issues

- [x] **Step 2: Implement authoring definition**

Added `BoardTerminalConditionDefinition` with Create Asset menu support, validation, and runtime condition creation.

- [x] **Step 3: Add GameMode linkage**

Added `GameModeDefinition.boardTerminalConditions` and validation for null, duplicate, and invalid terminal conditions.

- [x] **Step 4: Add guided inspector**

Added `BoardTerminalConditionDefinitionEditor` guidance and validation display.

### Task 3: Verify

- [x] **Step 1: Unity import**

Unity batchmode import regenerated script projects and compiled the new package scripts.

- [x] **Step 2: Build**

Run: `dotnet restore "Game Studio Core.slnx"; dotnet build "Game Studio Core.slnx" --no-restore`

Result: build succeeded with existing Unity/package warnings and 0 errors.

- [x] **Step 3: Unity EditMode**

Result: 209 total, 209 passed, 0 failed.

- [x] **Step 4: Unity PlayMode**

Result: 88 total, 88 passed, 0 failed.

