# Pyralis Non-Pawn Tabletop MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Non-Pawn Tabletop MVP route beginner-prototype ready through a clear no-pawn quick path, protected docs contracts, and honest readiness status.

**Architecture:** Preserve the existing tabletop runtime, starter pack, setup-flow validator, and scene readiness validator. Add an explicit MVP quick path to `Board_Card_Tabletop_Setup.md` and source-contract tests that ensure the route continues to say participants can be seats/sides with empty `Default Pawn`, empty spawn points, board selection, action queue, move policy, turn order, and terminal conditions.

**Tech Stack:** Unity, C#, ScriptableObject tabletop definitions, `TabletopBoardGridPresenter`, `TabletopBoardSelectionBridge`, `ActionQueueService`, NUnit EditMode source-contract tests.

---

## File Structure

- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/Prefabs/Board_Card_Tabletop_Setup.md`
  - Add a `Non-Pawn Tabletop MVP quick path` section.
  - Name the starter-pack assets and the first Play Mode proof loop.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
  - Update Non-Pawn Tabletop strong foundations and remaining blocker language.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md`
  - Record route status after the quick-path/docs contract slice.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/NonPawnTabletopMvpContractTests.cs`
  - Verify docs preserve the no-pawn quick path.
  - Verify docs name `TabletopBoardGridPresenter`, `TabletopBoardSelectionBridge`, `ActionQueueService`, `BoardMoveActionResolver`, `TurnOrderDefinition`, and `BoardTerminalConditionDefinition`.
  - Verify setup-flow source keeps no-pawn participant and spawn-point language.

## Task 1: Add Contract Tests

- [ ] **Step 1: Add `NonPawnTabletopMvpContractTests.cs`**

```csharp
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class NonPawnTabletopMvpContractTests
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
        public void TabletopSetupDocs_DefineNoPawnMvpQuickPath()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Board_Card_Tabletop_Setup.md"));

            StringAssert.Contains("Non-Pawn Tabletop MVP quick path", docs);
            StringAssert.Contains("leave `Default Pawn` empty", docs);
            StringAssert.Contains("leave `Spawn Points` empty", docs);
            StringAssert.Contains("Tabletop Starter Pack", docs);
            StringAssert.Contains("Move Board Piece", docs);
        }

        [Test]
        public void TabletopSetupDocs_NameRuntimeProofComponents()
        {
            string docs = File.ReadAllText(Path.Combine(GameplayRoot, "Docs", "Setup", "Prefabs", "Board_Card_Tabletop_Setup.md"));

            StringAssert.Contains("TabletopBoardGridPresenter", docs);
            StringAssert.Contains("TabletopBoardSelectionBridge", docs);
            StringAssert.Contains("ActionQueueService", docs);
            StringAssert.Contains("BoardMoveActionResolver", docs);
            StringAssert.Contains("TurnOrderDefinition", docs);
            StringAssert.Contains("BoardTerminalConditionDefinition", docs);
        }

        [Test]
        public void SetupFlowSource_PreservesNoPawnParticipantAndSpawnGuidance()
        {
            string source = File.ReadAllText(Path.Combine(GameplayRoot, "Editor", "PyralisSetupFlowMonitor.cs"));

            StringAssert.Contains("No participant pawn is required for this setup route.", source);
            StringAssert.Contains("Spawn points can stay empty for no-pawn board/card/menu/camera routes.", source);
            StringAssert.Contains("Tabletop Runtime Contract", source);
            StringAssert.Contains("Assign Tabletop Selection Surface", source);
        }
    }
}
```

## Task 2: Update Beginner Tabletop Docs

- [ ] **Step 1: Add the quick path**

Add a section near the top of `Board_Card_Tabletop_Setup.md` that walks through:

- Create `Assets/Create/NeonBlack/Gameplay/Tabletop Starter Pack`.
- Assign `TabletopSessionDefinition` to `GameplaySessionBootstrap`.
- Leave participant `Default Pawn` empty.
- Leave `Spawn Points` empty.
- Add `TabletopBoardGridPresenter` and let it wire board spaces to `TabletopBoardSelectionBridge`.
- Use the generated `Move Board Piece` action, move policy, turn order, and terminal conditions for the first proof loop.

## Task 3: Update Readiness Docs

- [ ] **Step 1: Update status summaries**

Record that no-pawn authoring and validation are now strongly covered by starter-pack docs and contracts, while a packaged proof scene is still the remaining route-hardening step.

## Task 4: Verify

- [ ] **Step 1: Run scans**

```powershell
rg -n "TBD|TODO|\\?\\?|place[h]older" Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/Prefabs/Board_Card_Tabletop_Setup.md Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/CORE_PACKAGE_READINESS_CHECKPOINTS.md
```

Expected: no output.

- [ ] **Step 2: Run pre-scene validation**

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected: EditMode and PlayMode pass.

## Self-Review

- Spec coverage: The plan covers the Non-Pawn Tabletop route's authoring, guidance, validation, and proof language without pretending game-specific chess/checkers rules are part of core MVP.
- Placeholder scan: No unresolved implementation placeholders are present.
- Type consistency: Test names and component names match existing runtime/editor code.
