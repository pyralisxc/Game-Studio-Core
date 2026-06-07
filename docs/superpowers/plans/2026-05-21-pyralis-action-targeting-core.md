# Pyralis Action Targeting Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first actor-agnostic Action + Targeting foundation so future guns, projectiles, tactics, cards, board games, menus, and brawler moves can share validation and resolution vocabulary.

**Architecture:** Keep runtime-neutral structs, enums, and service contracts in `Core/Actions`. Put designer-authored `ActionDefinition` assets in `Data/Definitions/Actions` so they can be referenced by future features without creating feature-to-feature coupling. Add tests that prove targeting validation works without requiring `PawnRoot`, movement components, or character controllers.

**Tech Stack:** Unity 6, C#, `ScriptableObject` definitions, NUnit runtime/editor tests, existing `NeonBlack.Gameplay.Core` and `NeonBlack.Gameplay.Data` asmdefs.

---

## File Structure

- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionTargetKind.cs`
  - Owns target-kind vocabulary: none, self, actor, point, direction, area, board space, card, zone, custom.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionExecutionTiming.cs`
  - Owns immediate, queued, turn, reaction, channel, and scripted timing vocabulary.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionResolutionStatus.cs`
  - Owns success/failure/pending/canceled/rejected resolution result states.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionTargetRule.cs`
  - Serializable authored/runtime rule for allowed target kinds, required target count, range, team filters, line-of-sight flags, and empty-target behavior.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionTargetDescriptor.cs`
  - Lightweight runtime target payload that can describe actors, world points, directions, ids, and custom payloads without depending on pawn controllers.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionExecutionContext.cs`
  - Runtime request context: action id, source object, owner object, participant object, faction, targets, and optional custom payload.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionValidationResult.cs`
  - Small success/failure value used before action execution.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionResolutionResult.cs`
  - Small outcome value used after action execution.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/IActionResolver.cs`
  - Contract for future feature runtimes that validate and resolve actions.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Actions/ActionDefinition.cs`
  - Designer-authored action asset with id, display name, family, timing, cooldown, cost, targeting rule, and validation.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/PlatformKernelTests.cs`
  - Add runtime tests for target rule validation and action context without requiring a pawn.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`
  - Add editor tests for `ActionDefinition` sanitation and validation.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_SCOPE.md`
  - Mark Action + Targeting core as started.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`
  - Add the new first-slice action/targeting surface.

---

### Task 1: Runtime Action Vocabulary

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionTargetKind.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionExecutionTiming.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionResolutionStatus.cs`

- [ ] **Step 1: Add enums**

```csharp
namespace NeonBlack.Gameplay.Core.Actions
{
    public enum ActionTargetKind
    {
        None,
        Self,
        Actor,
        WorldPoint,
        Direction,
        Area,
        BoardSpace,
        Card,
        Zone,
        Custom
    }
}
```

```csharp
namespace NeonBlack.Gameplay.Core.Actions
{
    public enum ActionExecutionTiming
    {
        Immediate,
        Queued,
        Turn,
        Reaction,
        Channel,
        Scripted
    }
}
```

```csharp
namespace NeonBlack.Gameplay.Core.Actions
{
    public enum ActionResolutionStatus
    {
        Succeeded,
        Failed,
        Pending,
        Canceled,
        Rejected
    }
}
```

- [ ] **Step 2: Run compile**

Run: `dotnet build Neonblackinteractivellc.Neonblackhub.csproj --no-restore`

Expected: build succeeds.

### Task 2: Target Rules And Runtime Payloads

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionTargetRule.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionTargetDescriptor.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionExecutionContext.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionValidationResult.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionResolutionResult.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/IActionResolver.cs`

- [ ] **Step 1: Add target rule and payload code**

Add serializable target and result types that depend only on Unity primitives and Core faction vocabulary.

- [ ] **Step 2: Add runtime tests**

Add tests proving:

- a self-only rule accepts one self target
- an actor rule rejects the wrong target kind
- a no-target action can validate without a pawn or controller

- [ ] **Step 3: Run runtime tests compile**

Run: `dotnet build Neonblackinteractivellc.Neonblackhub.Tests.csproj --no-restore`

Expected: build succeeds.

### Task 3: Authored ActionDefinition

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Actions/ActionDefinition.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] **Step 1: Add authored action definition**

Create a `ScriptableObject` with:

- `actionId`
- `displayName`
- `actionFamily`
- `executionTiming`
- `cooldown`
- `resourceCost`
- `targetRule`
- `notes`
- `Sanitize()`
- `GetValidationIssues()`

- [ ] **Step 2: Add editor tests**

Add tests proving:

- sanitation fills empty ids/names and clamps cooldown/cost
- validation flags missing target rules when a target is required
- no-target actions can be valid without a pawn setup

- [ ] **Step 3: Run editor tests compile**

Run: `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.Tests.csproj --no-restore`

Expected: build succeeds.

### Task 4: Docs And Verification

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_SCOPE.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`

- [ ] **Step 1: Update docs**

Document that Action + Targeting core has started and list the available runtime/authored types.

- [ ] **Step 2: Run full build matrix**

Run:

```powershell
dotnet build Neonblackinteractivellc.Neonblackhub.csproj --no-restore
dotnet build Neonblackinteractivellc.Neonblackhub.Editor.csproj --no-restore
dotnet build Neonblackinteractivellc.Neonblackhub.Tests.csproj --no-restore
dotnet build Neonblackinteractivellc.Neonblackhub.Editor.Tests.csproj --no-restore
```

Expected: all four builds pass with zero errors.

---

## Self-Review

- Spec coverage: covers the first Action + Targeting slice from `FEATURE_DEVELOPMENT_SCOPE.md`; guns/projectiles, animation, procedural generation, and tabletop systems remain future slices.
- Placeholder scan: no `TBD`, `TODO`, or vague implementation-only steps remain.
- Type consistency: runtime types use `NeonBlack.Gameplay.Core.Actions`; authored definition uses `NeonBlack.Gameplay.Data.Definitions`.
