# Pyralis RPG Progression Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first tested RPG platform slice: owner identity, stats, XP, levels, skill points, and progression definitions.

**Architecture:** Add a new RPG capability family under `Members/Pyralis/Gameplay/Core/Rpg` for runtime contracts/state and `Data/Definitions/Rpg` for authoring assets. Keep the runtime participant-owned and actor-agnostic; do not depend on pawn controllers, scene singletons, or UI.

**Tech Stack:** Unity C#, ScriptableObject definitions, NUnit EditMode/runtime tests, existing Pyralis docs and validation patterns.

---

## File Structure

- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/RpgOwnerKind.cs` for owner categories.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/RpgOwnerKey.cs` for stable owner ids.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/StatModifier.cs` for runtime stat deltas.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/StatSheet.cs` for base values, modifiers, and lookup.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/ProgressionState.cs` for XP, level, and skill points.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/ProgressionService.cs` for per-owner progression state.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rpg/StatDefinition.cs` for stat authoring.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rpg/ProgressionCurveDefinition.cs` for level thresholds and skill point grants.
- Create tests under `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/RpgProgressionRuntimeTests.cs`.
- Create editor validation tests under `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/RpgProgressionDefinitionTests.cs`.
- Update `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RPG_SYSTEMS_ROADMAP.md` when Phase 1 moves from planned to implemented.

### Task 1: Owner Keys And Stat Sheet Runtime

**Files:**
- Create: `.../Core/Rpg/RpgOwnerKind.cs`
- Create: `.../Core/Rpg/RpgOwnerKey.cs`
- Create: `.../Core/Rpg/StatModifier.cs`
- Create: `.../Core/Rpg/StatSheet.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/RpgProgressionRuntimeTests.cs`

- [ ] **Step 1: Write failing runtime tests**

Create tests proving an owner key has value equality and a stat sheet resolves base plus modifiers:

```csharp
[Test]
public void RpgOwnerKey_UsesKindAndStableIdForEquality()
{
    var first = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
    var second = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
    var different = new RpgOwnerKey(RpgOwnerKind.Actor, "seat-1");

    Assert.That(first, Is.EqualTo(second));
    Assert.That(first, Is.Not.EqualTo(different));
}

[Test]
public void StatSheet_ReturnsBaseValuePlusMatchingModifiers()
{
    var sheet = new StatSheet();
    sheet.SetBaseValue("wisdom", 5f);
    sheet.AddModifier(new StatModifier("wisdom", 2f, "cape"));
    sheet.AddModifier(new StatModifier("strength", 10f, "sword"));

    Assert.That(sheet.GetValue("wisdom"), Is.EqualTo(7f));
    Assert.That(sheet.GetValue("strength"), Is.EqualTo(10f));
    Assert.That(sheet.GetValue("missing"), Is.EqualTo(0f));
}
```

- [ ] **Step 2: Run red check**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Tests.csproj" --filter FullyQualifiedName~RpgProgressionRuntimeTests --no-restore
```

Expected: the test assembly reports missing RPG types or Unity test execution is unavailable. If dotnet does not run Unity tests, use Unity EditMode through the project validation gate after implementation.

- [ ] **Step 3: Implement runtime types**

Use namespace `NeonBlack.Gameplay.Core.Rpg`. `RpgOwnerKey` should reject null or whitespace ids by normalizing them to an empty string and expose `IsValid`.

- [ ] **Step 4: Run green check**

Run the same targeted test command. If dotnet cannot execute Unity tests, run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected: Unity EditMode summary includes the new runtime tests passing.

### Task 2: Progression Curve And Service

**Files:**
- Create: `.../Core/Rpg/ProgressionState.cs`
- Create: `.../Core/Rpg/ProgressionService.cs`
- Create: `.../Data/Definitions/Rpg/ProgressionCurveDefinition.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/RpgProgressionRuntimeTests.cs`

- [ ] **Step 1: Write failing tests for XP and skill points**

Add tests proving XP can level an owner and grant skill points:

```csharp
[Test]
public void ProgressionService_LevelsOwnerAndGrantsSkillPoints()
{
    var curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
    curve.SetTestThresholds(new[] { 0, 100, 250 });
    curve.SetTestSkillPointGrants(new[] { 0, 1, 2 });

    var service = new ProgressionService(curve);
    var owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

    service.AddExperience(owner, 260);
    ProgressionState state = service.GetState(owner);

    Assert.That(state.Level, Is.EqualTo(3));
    Assert.That(state.Experience, Is.EqualTo(260));
    Assert.That(state.SkillPoints, Is.EqualTo(3));
}
```

- [ ] **Step 2: Run red check**

Run targeted runtime tests and confirm failure because progression types are missing.

- [ ] **Step 3: Implement progression runtime**

`ProgressionService` should keep a dictionary keyed by `RpgOwnerKey`. `AddExperience` should clamp negative awards to zero, preserve total XP, calculate the highest reached level from the curve, and grant only newly earned skill points.

- [ ] **Step 4: Run green check**

Run targeted tests or the Unity validation gate.

### Task 3: Authoring Validation

**Files:**
- Create: `.../Data/Definitions/Rpg/StatDefinition.cs`
- Modify: `.../Data/Definitions/Rpg/ProgressionCurveDefinition.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/RpgProgressionDefinitionTests.cs`

- [ ] **Step 1: Write failing editor tests**

Test invalid stat ids, duplicate thresholds, and missing level zero:

```csharp
[Test]
public void StatDefinition_RequiresStableId()
{
    var stat = ScriptableObject.CreateInstance<StatDefinition>();
    stat.SetTestId("");

    Assert.That(stat.Validate().Any(message => message.Contains("stable id")), Is.True);
}

[Test]
public void ProgressionCurveDefinition_RequiresLevelOneZeroThreshold()
{
    var curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
    curve.SetTestThresholds(new[] { 50, 100 });

    Assert.That(curve.Validate().Any(message => message.Contains("Level 1")), Is.True);
}
```

- [ ] **Step 2: Run red check**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter FullyQualifiedName~RpgProgressionDefinitionTests --no-restore
```

Expected: missing definition types or validation methods.

- [ ] **Step 3: Implement ScriptableObject definitions**

Follow existing Pyralis definition patterns: serialized private fields, public read-only properties, and a `Validate()` method returning strings for tests/editor tooling.

- [ ] **Step 4: Run green check**

Run targeted editor tests or the Unity validation gate.

### Task 4: Docs And Readiness Update

**Files:**
- Modify: `RPG_SYSTEMS_ROADMAP.md`
- Modify: `FEATURE_INVENTORY.md`
- Modify: `RUNTIME_PARITY_MATRIX.md`

- [ ] **Step 1: Update docs**

Move Phase 1 from planned to foundational code added, list the new runtime and authoring files, and keep remaining phases planned.

- [ ] **Step 2: Run docs contract tests**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter FullyQualifiedName~RpgSystemsRoadmapContractTests --no-restore
```

Expected: contract test path is clean, or Unity validation gate confirms the editor tests pass.

- [ ] **Step 3: Run project gate**

Close the Unity GUI Editor and run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected: restore/build succeeds, Unity EditMode and PlayMode XML summaries are produced, final restore/build succeeds, and residue scan passes.
