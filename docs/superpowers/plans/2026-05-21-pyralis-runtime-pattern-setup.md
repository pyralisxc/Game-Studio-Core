# Pyralis Runtime Pattern Setup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add composable runtime setup pattern assets so Pyralis can describe overlapping game setup recipes without forcing every game through a pawn or one exclusive genre.

**Architecture:** Add small data-layer enums plus `RuntimePatternDefinition` and `GameSetupProfile` ScriptableObjects. Keep validation plain C# and editor-testable. Update the example authoring pack and durable docs after the data model is protected by tests.

**Tech Stack:** Unity 6000.4.0f1, C#, ScriptableObject authoring assets, NUnit editor tests, package asmdefs, local dotnet project builds.

---

### Task 1: Runtime Pattern Validation Tests

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] **Step 1: Write failing tests for the desired API**

Add tests that create `RuntimePatternDefinition` and `GameSetupProfile` assets, then assert validation behavior for well-formed patterns, missing ids, impossible pawn/non-pawn surface combinations, duplicate profiles, conflicts, and compatible overlap.

- [ ] **Step 2: Run the editor test project build and confirm RED**

Run: `dotnet build NeonBlack.Gameplay.Editor.Tests.csproj --no-restore`

Expected: fail with missing `RuntimePatternDefinition`, `RuntimeControlSurface`, `ParticipantEmbodimentRequirement`, `RuntimeCapabilityFamily`, or `GameSetupProfile`.

### Task 2: Runtime Pattern Data Model

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/RuntimeControlSurface.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/ParticipantEmbodimentRequirement.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/RuntimeCapabilityFamily.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/RuntimePatternDefinition.cs`
- Create: matching `.meta` files for new Unity assets
- Modify: `NeonBlack.Gameplay.Data.csproj`

- [ ] **Step 1: Implement enums**

Create enums for control surfaces, participant embodiment requirements, and capability families using the names from the approved spec.

- [ ] **Step 2: Implement `RuntimePatternDefinition`**

Add fields for stable id, display name, description, capability family, supported control surfaces, participant embodiment requirement, required runtime systems, optional runtime systems, recommended companions, cautionary companions, and setup notes.

Add:

- `Sanitize()`
- `SupportsControlSurface(RuntimeControlSurface surface)`
- `Recommends(RuntimePatternDefinition pattern)`
- `ConflictsWith(RuntimePatternDefinition pattern)`
- `GetValidationIssues()`

- [ ] **Step 3: Sync the local data csproj**

Add the new data files to `NeonBlack.Gameplay.Data.csproj` so local dotnet builds include them before Unity regenerates project files.

- [ ] **Step 4: Run the editor test project build**

Run: `dotnet build NeonBlack.Gameplay.Editor.Tests.csproj --no-restore`

Expected: still fail only on `GameSetupProfile` tests.

### Task 3: Game Setup Profile Data Model

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Profiles/GameSetupProfile.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Profiles/GameSetupProfile.cs.meta`
- Modify: `NeonBlack.Gameplay.Data.csproj`

- [ ] **Step 1: Implement `GameSetupProfile`**

Add setup name, summary, runtime pattern array, setup notes, `Sanitize()`, `HasPattern(string patternId)`, and `GetValidationIssues()`.

Validation should flag:

- empty setup profile
- null pattern slots
- duplicate pattern ids
- child pattern validation issues
- declared conflicts between selected patterns

- [ ] **Step 2: Sync the local data csproj**

Add the new profile file to `NeonBlack.Gameplay.Data.csproj`.

- [ ] **Step 3: Run the editor test project build and confirm GREEN**

Run: `dotnet build NeonBlack.Gameplay.Editor.Tests.csproj --no-restore`

Expected: pass.

### Task 4: Example Authoring Pack

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/GameplayExampleAssetFactory.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] **Step 1: Write failing source-level test**

Add a test that reads `GameplayExampleAssetFactory.cs` and asserts it creates `RuntimePatternDefinition` and `GameSetupProfile` sample assets.

- [ ] **Step 2: Run editor tests and confirm RED**

Run: `dotnet build NeonBlack.Gameplay.Editor.Tests.csproj --no-restore`

Expected: fail because the factory does not yet create runtime pattern/profile assets.

- [ ] **Step 3: Update example factory**

Create canonical pattern assets for realtime character, projectile combat, turn/menu action, board/card/tabletop, and camera/cursor control. Create a sample game setup profile that combines realtime character and projectile combat, then assign recommended/cautionary references where useful.

- [ ] **Step 4: Run editor tests and confirm GREEN**

Run: `dotnet build NeonBlack.Gameplay.Editor.Tests.csproj --no-restore`

Expected: pass.

### Task 5: Documentation

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_SCOPE.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/ARCHITECTURE_BLUEPRINT.md`

- [ ] **Step 1: Update docs**

Document `RuntimePatternDefinition`, `GameSetupProfile`, composable overlap, and the fact that non-pawn participant surfaces are first-class.

- [ ] **Step 2: Run source/docs sanity checks**

Run: `rg -n "RuntimePatternDefinition|GameSetupProfile|composable|overlap" Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs`

Expected: docs mention the new model in the durable product/architecture files.

### Task 6: Final Verification

**Files:**
- All files touched in prior tasks

- [ ] **Step 1: Build data assembly**

Run: `dotnet build NeonBlack.Gameplay.Data.csproj --no-restore`

Expected: pass.

- [ ] **Step 2: Build editor assembly**

Run: `dotnet build NeonBlack.Gameplay.Editor.csproj --no-restore`

Expected: pass.

- [ ] **Step 3: Build editor tests**

Run: `dotnet build NeonBlack.Gameplay.Editor.Tests.csproj --no-restore`

Expected: pass.

- [ ] **Step 4: Build aggregate gameplay assembly**

Run: `dotnet build NeonBlack.Gameplay.csproj --no-restore`

Expected: pass or report only pre-existing warnings.

- [ ] **Step 5: Scan duplicate Unity meta GUIDs under Gameplay**

Run:

```powershell
$root = 'Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay'
$guids = Get-ChildItem -Path $root -Recurse -Filter '*.meta' | ForEach-Object {
    $line = Select-String -Path $_.FullName -Pattern '^guid: ' | Select-Object -First 1
    if ($line) { [PSCustomObject]@{ Guid = $line.Line.Substring(6); Path = $_.FullName } }
}
$dupes = $guids | Group-Object Guid | Where-Object { $_.Count -gt 1 }
$dupes | ForEach-Object { $_.Group | Format-Table -AutoSize }
if ($dupes) { exit 1 }
```

Expected: exit 0 with no duplicate groups.

