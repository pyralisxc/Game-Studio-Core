# Pyralis Player Experience Phase 7A Hub Interaction Spine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first hub interaction spine for the Player Experience Framework, with HUD-ready interaction results and Unity authoring validation.

**Architecture:** Add core hub contracts and a `HubInteractionService` under the RPG core namespace so runtime behavior remains UI-stack-neutral. Add ScriptableObject definitions under RPG data definitions, guided inspectors under the existing RPG editor tooling, and runtime/editor tests proving available, locked, hidden, panel, dialogue, scene, and malformed authoring paths.

**Tech Stack:** Unity 6000.4, C#, NUnit EditMode/PlayMode, ScriptableObject authoring, existing Pyralis RPG services and validation gate.

---

### File Structure

- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubInteractionKind.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubInteractionAvailability.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubConditionKind.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubEffectKind.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/PlayerPanelRoute.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubInteractionStatus.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubInteractionCondition.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubInteractionEffect.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubInteractable.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubDefinitionModel.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubPromptPayload.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubNotificationPayload.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubInteractionResult.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/IHubDefinition.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/IDialogueGraphResolver.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/IHubConditionResolver.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/IHubEffectSink.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/HubInteractionService.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rpg/HubConditionDefinition.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rpg/HubEffectDefinition.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rpg/HubInteractableDefinition.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rpg/HubDefinition.cs`
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/RpgDefinitionEditors.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/RpgHubInteractionRuntimeTests.cs`
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/RpgHubDefinitionTests.cs`
- Modify generated csproj files so fast build sees new files before Unity regenerates them.
- Modify RPG roadmap, runtime parity matrix, feature inventory, and feature development roadmap docs.

### Task 1: Runtime Tests

- [ ] **Step 1: Write failing runtime tests**

Create tests for:

- available interactions return unlocked prompt payloads
- locked interactions can be shown with locked prompt text
- hidden locked interactions are omitted from available interaction queries
- selected interactions can request dialogue, panel routes, and scene navigation without loading a scene
- invalid owners or ids return explicit status and issue

- [ ] **Step 2: Patch runtime test csproj**

Add `RpgHubInteractionRuntimeTests.cs` to `NeonBlack.Gameplay.Tests.csproj`.

- [ ] **Step 3: Run red build**

Run `dotnet build "Game Studio Core.slnx" --no-restore`.

Expected: fails because hub runtime types do not exist.

### Task 2: Runtime Implementation

- [ ] **Step 1: Add core hub enums and payload models**

Implement interaction kind, availability, condition/effect kind, panel route, status, prompt payload, notification payload, and interaction result types.

- [ ] **Step 2: Add core hub models and contracts**

Implement `HubInteractionCondition`, `HubInteractionEffect`, `HubInteractable`, `HubDefinitionModel`, `IHubDefinition`, `IDialogueGraphResolver`, `IHubConditionResolver`, and `IHubEffectSink`.

- [ ] **Step 3: Add `HubInteractionService`**

Implement query and selection behavior using existing `InventoryService`, `QuestService`, `SkillTreeService`, `DialogueService`, optional resolvers, and effect sinks.

- [ ] **Step 4: Patch core csproj**

Add all new Core/Rpg files to `NeonBlack.Gameplay.Core.csproj`.

- [ ] **Step 5: Run green build**

Run `dotnet build "Game Studio Core.slnx" --no-restore`.

Expected: build succeeds.

### Task 3: Authoring Tests

- [ ] **Step 1: Write failing editor definition tests**

Create tests for:

- hub id is required
- duplicate interactable ids are flagged
- portal/minigame interactions require scene id
- NPC dialogue interactions require dialogue graph id
- panel-only interactions require a panel route
- condition/effect target ids are validated when required

- [ ] **Step 2: Patch editor test csproj**

Add `RpgHubDefinitionTests.cs` to `NeonBlack.Gameplay.Editor.Tests.csproj`.

- [ ] **Step 3: Run red build**

Run `dotnet build "Game Studio Core.slnx" --no-restore`.

Expected: fails because hub definition assets do not exist.

### Task 4: Authoring Implementation

- [ ] **Step 1: Add ScriptableObject definitions**

Implement `HubDefinition`, `HubInteractableDefinition`, `HubConditionDefinition`, and `HubEffectDefinition` with sanitize, runtime conversion, and validation.

- [ ] **Step 2: Add guided inspectors**

Extend `RpgDefinitionEditors.cs` with `HubDefinitionEditor` and guided authoring copy.

- [ ] **Step 3: Patch data/editor csproj files**

Add new data files to `NeonBlack.Gameplay.Data.csproj` and keep editor file already included.

- [ ] **Step 4: Run green build**

Run `dotnet build "Game Studio Core.slnx" --no-restore`.

Expected: build succeeds.

### Task 5: Docs

- [ ] **Step 1: Update RPG roadmap**

Mark Phase 7 as foundational code added and describe the hub interaction spine plus HUD-ready payloads.

- [ ] **Step 2: Update parity and inventory docs**

List new core/data/editor assets and update Phase 7 MVP status to Foundation Only.

- [ ] **Step 3: Update feature development roadmap**

Add the Phase 7A plan path and status.

### Task 6: Full Validation

- [ ] **Step 1: Run full pre-scene validation**

Run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected: restore/build passes, Unity EditMode passes, Unity PlayMode passes, final restore/build passes, residue scan clean.
