# Pyralis Board Move Policy Checkpoint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Advance the Rules-Driven Tabletop MVP by adding Unity-authorable legal move policy primitives for board moves.

**Architecture:** Core owns actor-agnostic move policy contracts and runtime evaluation. Data owns the ScriptableObject authoring asset that creates runtime policies. Editor owns guided inspector coverage so the policy can be created and understood from Unity.

**Tech Stack:** Unity 6000.4, C#, NUnit, ScriptableObject authoring, existing Pyralis core/action/board runtime.

---

## File Structure

- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardMoveShape.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardMovePolicyContext.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/IBoardMovePolicy.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardMovePolicy.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardMovePolicyDefinition.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardMoveActionResolver.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/CoreRulesDefinitionEditors.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/CoreRulesRuntimeTests.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/CoreRulesDefinitionTests.cs`

### Task 1: Runtime Policy Contract

- [x] **Step 1: Write failing runtime tests**

Added tests proving:

- orthogonal policy rejects diagonal movement
- orthogonal policy allows orthogonal movement
- capture-capable policy captures an opposing destination piece

- [x] **Step 2: Implement runtime contract**

Added:

- `BoardMoveShape`
- `BoardMovePolicyContext`
- `IBoardMovePolicy`
- `BoardMovePolicy`

### Task 2: Resolver Integration

- [x] **Step 1: Extend board move resolver**

`BoardMoveActionResolver` now accepts an optional `IBoardMovePolicy`. Existing no-policy behavior still rejects occupied destinations. Policy-backed behavior validates shape, distance, destination, friendly occupancy, and optional opponent capture.

### Task 3: Unity Authoring Asset

- [x] **Step 1: Write failing editor tests**

Added tests proving `BoardMovePolicyDefinition` creates a matching runtime policy and validates invalid max distance.

- [x] **Step 2: Implement authoring definition**

Added `BoardMovePolicyDefinition` with Create Asset menu support, validation, and runtime policy creation.

- [x] **Step 3: Add guided inspector**

Added `BoardMovePolicyDefinitionEditor` guidance and validation display.

### Task 4: Verify

- [x] **Step 1: Import new Unity package scripts**

Unity batchmode import regenerated script projects after `.meta` files were added for new package scripts.

- [x] **Step 2: Build**

Run: `dotnet restore "Game Studio Core.slnx"; dotnet build "Game Studio Core.slnx" --no-restore`

Result: build succeeded with existing Unity/package warnings and 0 errors.

- [x] **Step 3: Unity EditMode**

Result: 207 total, 207 passed, 0 failed.

- [x] **Step 4: Unity PlayMode**

Result: 86 total, 86 passed, 0 failed.

