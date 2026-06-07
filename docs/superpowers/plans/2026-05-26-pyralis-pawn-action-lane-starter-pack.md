# Pyralis Pawn Action Lane Starter Pack Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Pawn-Backed Action MVP route beginner-prototype ready across `Sprite2D`, `Billboard2_5D`, and `Rigged3D` by generating lane-specific starter assets and documenting the lane choice clearly.

**Architecture:** Keep the existing `GameplayStarterPackFactory` asset-generation path and expand it rather than adding a new window. Generate one shared profile set where appropriate, three `PawnPresentationProfile` assets, three lane-specific `PawnDefinition` assets, and three prefabs with the correct component stack. Keep participants pointed at the Sprite2D pawn as the safest first-play default while making the other lanes ready to swap in.

**Tech Stack:** Unity Editor asset generation, C#, `PawnRoot`, `Motor2D`, `Motor3D`, `PawnPresentationProfile`, `ActorPresentationMode`, NUnit EditMode source-contract tests.

---

## File Structure

- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/GameplayStarterPackFactory.cs`
  - Replace the single `PresentationProfile`, `SharedPawnDefinition`, and `SharedPawnPrefab` with lane-specific profile/definition/prefab assets.
  - Add helper methods for 2D and 3D pawn prefab creation.
  - Add helper method for presentation profile creation.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/Prefabs/Pawn_Setup.md`
  - Add an explicit Pawn-Backed Action MVP lane choice section.
  - Name the generated lane assets and which prefab stack each lane uses.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
  - Record starter-pack coverage for all three pawn action lanes.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`
  - Record this slice as lane-specific starter-pack coverage, with remaining proof-scene work.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/PawnActionLaneStarterPackContractTests.cs`
  - Verify the factory source creates the three lane profiles, definitions, and prefabs.
  - Verify the factory source uses both `Motor2D` and `Motor3D` stacks.
  - Verify `Pawn_Setup.md` names the Pawn-Backed Action MVP lanes and generated starter assets.

## Task 1: Add Failing Contract Tests

**Files:**
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/PawnActionLaneStarterPackContractTests.cs`

- [ ] **Step 1: Add the source-contract test**

```csharp
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class PawnActionLaneStarterPackContractTests
    {
        private static readonly string GameplayRoot = Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub",
            "Members",
            "Pyralis",
            "Gameplay");

        [Test]
        public void PawnStarterPackFactory_CreatesAssetsForAllPawnActionLanes()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Editor", "GameplayStarterPackFactory.cs"));

            StringAssert.Contains("Sprite2DPresentationProfile", source);
            StringAssert.Contains("Billboard25DPresentationProfile", source);
            StringAssert.Contains("Rigged3DPresentationProfile", source);
            StringAssert.Contains("Sprite2DPawnDefinition", source);
            StringAssert.Contains("Billboard25DPawnDefinition", source);
            StringAssert.Contains("Rigged3DPawnDefinition", source);
            StringAssert.Contains("Sprite2DPawnPrefab", source);
            StringAssert.Contains("Billboard25DPawnPrefab", source);
            StringAssert.Contains("Rigged3DPawnPrefab", source);
        }

        [Test]
        public void PawnStarterPackFactory_CreatesCorrectPrefabStacksFor2DAnd3D()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Editor", "GameplayStarterPackFactory.cs"));

            StringAssert.Contains("CreateStarterPawnPrefab2D", source);
            StringAssert.Contains("CreateStarterPawnPrefab3D", source);
            StringAssert.Contains("root.AddComponent<Motor2D>()", source);
            StringAssert.Contains("root.AddComponent<Motor3D>()", source);
            StringAssert.Contains("ActorPresentationMode.Sprite2D", source);
            StringAssert.Contains("ActorPresentationMode.Billboard2_5D", source);
            StringAssert.Contains("ActorPresentationMode.Rigged3D", source);
        }

        [Test]
        public void PawnSetupDocs_NamePawnActionMvpLaneStarterAssets()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Pawn_Setup.md"));

            StringAssert.Contains("Pawn-Backed Action MVP lane choice", docs);
            StringAssert.Contains("Sprite2DPawnPrefab", docs);
            StringAssert.Contains("Billboard25DPawnPrefab", docs);
            StringAssert.Contains("Rigged3DPawnPrefab", docs);
            StringAssert.Contains("Sprite2D", docs);
            StringAssert.Contains("Billboard2_5D", docs);
            StringAssert.Contains("Rigged3D", docs);
        }
    }
}
```

## Task 2: Generate Lane-Specific Starter Assets

**Files:**
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/GameplayStarterPackFactory.cs`

- [ ] **Step 1: Replace the single presentation/pawn/prefab path**

Create these presentation profiles:

```csharp
PawnPresentationProfile sprite2DPresentationProfile = CreatePresentationProfile(profilesFolder, "Sprite2DPresentationProfile", ActorPresentationMode.Sprite2D);
PawnPresentationProfile billboard25DPresentationProfile = CreatePresentationProfile(profilesFolder, "Billboard25DPresentationProfile", ActorPresentationMode.Billboard2_5D);
PawnPresentationProfile rigged3DPresentationProfile = CreatePresentationProfile(profilesFolder, "Rigged3DPresentationProfile", ActorPresentationMode.Rigged3D);
```

Create these pawn definitions and prefabs:

```csharp
PawnDefinition sprite2DPawnDefinition = CreatePawnDefinition(definitionsFolder, "Sprite2DPawnDefinition", inputProfile, movementProfile, combatProfile, traversalProfile, sprite2DPresentationProfile, animationProfile, CreateStarterPawnPrefab2D(prefabsFolder, "Sprite2DPawnPrefab"));
PawnDefinition billboard25DPawnDefinition = CreatePawnDefinition(definitionsFolder, "Billboard25DPawnDefinition", inputProfile, movementProfile, combatProfile, traversalProfile, billboard25DPresentationProfile, animationProfile, CreateStarterPawnPrefab3D(prefabsFolder, "Billboard25DPawnPrefab"));
PawnDefinition rigged3DPawnDefinition = CreatePawnDefinition(definitionsFolder, "Rigged3DPawnDefinition", inputProfile, movementProfile, combatProfile, traversalProfile, rigged3DPresentationProfile, animationProfile, CreateStarterPawnPrefab3D(prefabsFolder, "Rigged3DPawnPrefab"));
```

- [ ] **Step 2: Add helper methods**

Add:

```csharp
private static PawnPresentationProfile CreatePresentationProfile(string folder, string assetName, ActorPresentationMode mode)
{
    PawnPresentationProfile profile = CreateAsset<PawnPresentationProfile>(folder, assetName);
    profile.presentationMode = mode;
    profile.useSharedCamera = mode != ActorPresentationMode.Rigged3D;
    profile.rigType = mode == ActorPresentationMode.Rigged3D ? RiggedAnimationRigType.Humanoid : RiggedAnimationRigType.Generic;
    EditorUtility.SetDirty(profile);
    return profile;
}
```

Add:

```csharp
private static PawnDefinition CreatePawnDefinition(...)
```

that assigns shared input, movement, combat, traversal, presentation, animation, and prefab references.

Add `CreateStarterPawnPrefab2D` with `PawnRoot`, `Motor2D`, `ActorAnimationDriver`, `HealthComponent`, and a `SpriteRenderer` child.

Add `CreateStarterPawnPrefab3D` with `PawnRoot`, `Motor3D`, `ActorAnimationDriver`, `PawnCombatBehaviour`, and a child `Animator` placeholder object.

## Task 3: Update Beginner Docs And Readiness State

**Files:**
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/Prefabs/Pawn_Setup.md`
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`

- [ ] **Step 1: Add lane starter-pack section to Pawn_Setup.md**

Explain:

- `Sprite2D` uses `Sprite2DPawnPrefab` and the 2D stack.
- `Billboard2_5D` uses `Billboard25DPawnPrefab` and the 3D stack with billboard presentation.
- `Rigged3D` uses `Rigged3DPawnPrefab` and the 3D stack with rigged presentation.

- [ ] **Step 2: Keep status honest**

Update readiness docs to say lane-specific starter assets exist, while all three lanes still need proof-scene validation before `Ready`.

## Task 4: Verify The Slice

- [ ] **Step 1: Run source hygiene scan**

```powershell
rg -n "TBD|TODO|\\?\\?|place[h]older" Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/Prefabs/Pawn_Setup.md Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md
```

Expected: no output.

- [ ] **Step 2: Run build**

```powershell
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: build succeeds with no new errors.

- [ ] **Step 3: Run the Unity pre-scene gate**

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected: EditMode and PlayMode pass.

## Self-Review

- Spec coverage: This plan advances Pawn-Backed Action runtime lanes through authoring, guidance, validation, and starter-pack proof coverage while keeping full playable proof as remaining work.
- Placeholder scan: No unresolved implementation placeholders are present.
- Type consistency: Helper names and asset names match the contract tests and existing factory style.
