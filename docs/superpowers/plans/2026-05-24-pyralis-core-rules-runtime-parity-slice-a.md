# Pyralis Core Rules Runtime Parity Slice A Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first core rules contracts and parity matrix so tabletop/turn/menu work has a real platform foundation before scene development.

**Architecture:** Pure rules contracts live in `NeonBlack.Gameplay.Core`; Unity-authored definitions live in `NeonBlack.Gameplay.Data`; bootstrap/service registration happens in later slices. This slice avoids scene dependencies and focuses on testable definitions, runtime state types, validation, and documentation.

**Tech Stack:** Unity 6000.4, C#, ScriptableObject definitions, NUnit/EditMode tests, existing Pyralis guided-authoring and setup documentation patterns.

---

## File Structure

- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/TurnPhase/TurnRuntimeState.cs`
  - Plain runtime state for active participant seat, round, and phase.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/TurnPhase/ITurnOrderService.cs`
  - Interface for turn services without depending on scene managers.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardCoordinate.cs`
  - Serializable board coordinate value object.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardSpaceState.cs`
  - Runtime state for one board space.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardPieceState.cs`
  - Runtime state for one board piece.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardRuntimeState.cs`
  - Runtime board state with lookup, occupancy, and move/capture primitives.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/IBoardStateService.cs`
  - Interface for future scene/runtime services.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Actions/QueuedAction.cs`
  - Plain queued action record using existing action context.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Actions/IActionQueueService.cs`
  - Interface for future immediate/queued resolution services.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/PhaseDefinition.cs`
  - ScriptableObject phase definition.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/TurnOrderDefinition.cs`
  - ScriptableObject turn order definition.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardDefinition.cs`
  - ScriptableObject rectangular board definition and starting piece placement.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardPieceDefinition.cs`
  - ScriptableObject piece definition.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/GameModeDefinition.cs`
  - Add optional `turnOrderDefinition` and `boardDefinition` references under rules.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/CoreRulesDefinitionTests.cs`
  - EditMode tests for definition validation.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/CoreRulesRuntimeTests.cs`
  - Runtime tests for board state movement/capture and turn state.
- Create `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
  - First parity matrix for runtime lane readiness.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`
  - Mark this slice as active.
- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`
  - Add the new core rules contract inventory.

---

### Task 1: Add Pure Turn And Board Runtime Contracts

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/TurnPhase/TurnRuntimeState.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/TurnPhase/ITurnOrderService.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardCoordinate.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardSpaceState.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardPieceState.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/BoardRuntimeState.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rules/Board/IBoardStateService.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/CoreRulesRuntimeTests.cs`

- [ ] **Step 1: Write failing runtime tests for board and turn primitives**

Add tests:

```csharp
[Test]
public void BoardRuntimeState_MovePiece_UpdatesOccupancy()
{
    BoardRuntimeState state = BoardRuntimeState.CreateRectangular(2, 2);
    BoardPieceState piece = new BoardPieceState("piece.p1", "pawn", ownerSeat: 0, new BoardCoordinate(0, 0));
    Assert.That(state.TryAddPiece(piece, out string addIssue), Is.True, addIssue);

    Assert.That(state.TryMovePiece("piece.p1", new BoardCoordinate(1, 0), out string moveIssue), Is.True, moveIssue);

    Assert.That(state.TryGetPieceAt(new BoardCoordinate(0, 0), out _), Is.False);
    Assert.That(state.TryGetPieceAt(new BoardCoordinate(1, 0), out BoardPieceState moved), Is.True);
    Assert.That(moved.PieceId, Is.EqualTo("piece.p1"));
}

[Test]
public void BoardRuntimeState_CapturePiece_RemovesCapturedPieceFromOccupancy()
{
    BoardRuntimeState state = BoardRuntimeState.CreateRectangular(2, 1);
    Assert.That(state.TryAddPiece(new BoardPieceState("piece.a", "pawn", 0, new BoardCoordinate(0, 0)), out _), Is.True);
    Assert.That(state.TryAddPiece(new BoardPieceState("piece.b", "pawn", 1, new BoardCoordinate(1, 0)), out _), Is.True);

    Assert.That(state.TryCapturePiece("piece.b", out string issue), Is.True, issue);

    Assert.That(state.TryGetPiece("piece.b", out BoardPieceState captured), Is.True);
    Assert.That(captured.IsCaptured, Is.True);
    Assert.That(state.TryGetPieceAt(new BoardCoordinate(1, 0), out _), Is.False);
}

[Test]
public void TurnRuntimeState_AdvanceTurn_UpdatesSeatAndRound()
{
    TurnRuntimeState state = new TurnRuntimeState(new[] { 0, 1 }, startingSeat: 0);

    Assert.That(state.ActiveSeat, Is.EqualTo(0));
    Assert.That(state.RoundIndex, Is.EqualTo(1));

    Assert.That(state.TryAdvance(out string firstIssue), Is.True, firstIssue);
    Assert.That(state.ActiveSeat, Is.EqualTo(1));
    Assert.That(state.RoundIndex, Is.EqualTo(1));

    Assert.That(state.TryAdvance(out string secondIssue), Is.True, secondIssue);
    Assert.That(state.ActiveSeat, Is.EqualTo(0));
    Assert.That(state.RoundIndex, Is.EqualTo(2));
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test "Game Studio Core.slnx" --no-restore --filter CoreRulesRuntimeTests`

Expected: compile fails because `BoardRuntimeState`, `BoardPieceState`, `BoardCoordinate`, and `TurnRuntimeState` do not exist.

- [ ] **Step 3: Implement minimal pure runtime contracts**

Implement immutable/value-style coordinate and piece/space state, plus `BoardRuntimeState` dictionaries. The core public API must include:

```csharp
public readonly struct BoardCoordinate : IEquatable<BoardCoordinate>
{
    public int X { get; }
    public int Y { get; }
}

public sealed class BoardPieceState
{
    public string PieceId { get; }
    public string PieceDefinitionId { get; }
    public int OwnerSeat { get; }
    public BoardCoordinate Coordinate { get; private set; }
    public bool IsCaptured { get; private set; }
}

public sealed class BoardRuntimeState
{
    public static BoardRuntimeState CreateRectangular(int width, int height);
    public bool TryAddPiece(BoardPieceState piece, out string issue);
    public bool TryMovePiece(string pieceId, BoardCoordinate destination, out string issue);
    public bool TryCapturePiece(string pieceId, out string issue);
    public bool TryGetPiece(string pieceId, out BoardPieceState piece);
    public bool TryGetPieceAt(BoardCoordinate coordinate, out BoardPieceState piece);
}
```

Implement `TurnRuntimeState` with:

```csharp
public sealed class TurnRuntimeState
{
    public TurnRuntimeState(IReadOnlyList<int> seats, int startingSeat = 0);
    public int ActiveSeat { get; }
    public int RoundIndex { get; }
    public int TurnIndex { get; }
    public bool TryAdvance(out string issue);
}
```

- [ ] **Step 4: Run runtime tests**

Run: `dotnet test "Game Studio Core.slnx" --no-restore --filter CoreRulesRuntimeTests`

Expected: tests compile and pass.

### Task 2: Add ScriptableObject Rule Definitions

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/PhaseDefinition.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/TurnOrderDefinition.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardPieceDefinition.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rules/BoardDefinition.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/CoreRulesDefinitionTests.cs`

- [ ] **Step 1: Write failing definition validation tests**

Add tests that create ScriptableObjects, call `GetValidationIssues()`, and assert:

```csharp
[Test]
public void BoardDefinition_GetValidationIssues_RejectsDuplicateStartingPieceIds()
{
    BoardPieceDefinition piece = ScriptableObject.CreateInstance<BoardPieceDefinition>();
    piece.pieceId = "piece.pawn";
    BoardDefinition board = ScriptableObject.CreateInstance<BoardDefinition>();
    board.boardId = "board.test";
    board.width = 2;
    board.height = 2;
    board.startingPieces = new[]
    {
        new BoardStartingPiece("piece.a", piece, 0, new BoardCoordinate(0, 0)),
        new BoardStartingPiece("piece.a", piece, 1, new BoardCoordinate(1, 0))
    };

    List<string> issues = board.GetValidationIssues();

    Assert.That(issues.Any(issue => issue.Contains("assigned more than once")), Is.True);
    Object.DestroyImmediate(board);
    Object.DestroyImmediate(piece);
}

[Test]
public void TurnOrderDefinition_GetValidationIssues_RequiresAtLeastOneSeat()
{
    TurnOrderDefinition turnOrder = ScriptableObject.CreateInstance<TurnOrderDefinition>();
    turnOrder.turnOrderId = "turn.test";
    turnOrder.participantSeats = Array.Empty<int>();

    Assert.That(turnOrder.GetValidationIssues(), Has.Some.Contains("participant seat"));
    Object.DestroyImmediate(turnOrder);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test "Game Studio Core.slnx" --no-restore --filter CoreRulesDefinitionTests`

Expected: compile fails because rule definition types do not exist.

- [ ] **Step 3: Implement definitions and validation**

Definitions must use `CreateAssetMenu` paths:

```csharp
[CreateAssetMenu(menuName = "NeonBlack/Gameplay/Rules/Board Definition", fileName = "BoardDefinition")]
[CreateAssetMenu(menuName = "NeonBlack/Gameplay/Rules/Board Piece Definition", fileName = "BoardPieceDefinition")]
[CreateAssetMenu(menuName = "NeonBlack/Gameplay/Rules/Turn Order Definition", fileName = "TurnOrderDefinition")]
[CreateAssetMenu(menuName = "NeonBlack/Gameplay/Rules/Phase Definition", fileName = "PhaseDefinition")]
```

Each definition must expose:

- stable id string
- display name
- `Sanitize()`
- `GetValidationIssues()`
- `OnValidate()` calling `Sanitize()`

`BoardDefinition` must include rectangular dimensions and `BoardStartingPiece[]`.

- [ ] **Step 4: Run definition tests**

Run: `dotnet test "Game Studio Core.slnx" --no-restore --filter CoreRulesDefinitionTests`

Expected: tests pass.

### Task 3: Link Rule Definitions To GameModeDefinition

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/GameModeDefinition.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/CoreRulesDefinitionTests.cs`

- [ ] **Step 1: Write failing validation test**

Add:

```csharp
[Test]
public void GameModeDefinition_GetValidationIssues_IncludesBoardDefinitionIssues()
{
    GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
    mode.boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
    mode.boardDefinition.boardId = string.Empty;

    List<string> issues = mode.GetValidationIssues();

    Assert.That(issues.Any(issue => issue.Contains("Board definition")), Is.True);
    Object.DestroyImmediate(mode.boardDefinition);
    Object.DestroyImmediate(mode);
}
```

- [ ] **Step 2: Run test to verify failure**

Run: `dotnet test "Game Studio Core.slnx" --no-restore --filter GameModeDefinition_GetValidationIssues_IncludesBoardDefinitionIssues`

Expected: compile fails because `GameModeDefinition.boardDefinition` does not exist.

- [ ] **Step 3: Add optional references and validation**

Under `GameModeDefinition` rules, add:

```csharp
public TurnOrderDefinition turnOrderDefinition;
public BoardDefinition boardDefinition;
```

In `GetValidationIssues()`, append nested issues:

```csharp
if (turnOrderDefinition != null)
{
    List<string> turnIssues = turnOrderDefinition.GetValidationIssues();
    for (int i = 0; i < turnIssues.Count; i++)
        issues.Add($"Turn order `{turnOrderDefinition.displayName}`: {turnIssues[i]}");
}

if (boardDefinition != null)
{
    List<string> boardIssues = boardDefinition.GetValidationIssues();
    for (int i = 0; i < boardIssues.Count; i++)
        issues.Add($"Board definition `{boardDefinition.displayName}`: {boardIssues[i]}");
}
```

- [ ] **Step 4: Run targeted test**

Run: `dotnet test "Game Studio Core.slnx" --no-restore --filter GameModeDefinition_GetValidationIssues_IncludesBoardDefinitionIssues`

Expected: targeted test passes.

### Task 4: Add Parity Matrix And Inventory Docs

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/SetupDocsContractTests.cs`

- [ ] **Step 1: Write failing docs contract test**

Add assertions that `RUNTIME_PARITY_MATRIX.md` exists and contains the lane names:

```csharp
Assert.That(File.Exists(matrixPath), Is.True);
string matrix = File.ReadAllText(matrixPath);
Assert.That(matrix, Does.Contain("2D side-scrolling shooter"));
Assert.That(matrix, Does.Contain("board/tabletop"));
Assert.That(matrix, Does.Contain("card/hand/deck"));
Assert.That(matrix, Does.Contain("Setup pattern"));
Assert.That(matrix, Does.Contain("Known limits"));
```

- [ ] **Step 2: Run docs test to verify failure**

Run: `dotnet test "Game Studio Core.slnx" --no-restore --filter SetupDocsContractTests`

Expected: fails because matrix doc is missing.

- [ ] **Step 3: Add parity matrix and doc updates**

Create a matrix with rows for:

- 2D side-scrolling shooter
- 2D arcade pickup/hazard loop
- 3D/FPS-style pawn
- 3D brawler/fighter
- projectile/guns
- turn/menu action
- board/tabletop
- card/hand/deck
- scoring/objectives
- camera/cursor control
- local multiplayer
- optional networking

Columns:

- Setup pattern
- Runtime services
- Authoring assets
- Guided inspectors
- Setup validation
- Runtime tests
- Sample/starter
- Known limits

Update inventory and roadmap to point at the new matrix and the new Slice A contracts.

- [ ] **Step 4: Run docs contract test**

Run: `dotnet test "Game Studio Core.slnx" --no-restore --filter SetupDocsContractTests`

Expected: docs contract passes.

### Task 5: Full Verification

**Files:**
- All files from Tasks 1-4.

- [ ] **Step 1: Run .NET compile/test smoke**

Run:

```powershell
dotnet restore "Game Studio Core.slnx"
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: build succeeds with 0 errors.

- [ ] **Step 2: Run Unity EditMode**

Run Unity no-quit EditMode command used in prior audits:

```powershell
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$log = Join-Path (Get-Location) "Logs\Codex\core-rules-slice-a-editmode-$timestamp.log"
$results = Join-Path (Get-Location) "Logs\Codex\core-rules-slice-a-editmode-$timestamp-results.xml"
& 'C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe' -batchmode -projectPath (Get-Location).Path -logFile $log -runTests -testPlatform EditMode -testResults $results
```

Expected: XML `<test-run ... result="Passed" failed="0" ...>`.

- [ ] **Step 3: Run Unity PlayMode**

Run:

```powershell
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$log = Join-Path (Get-Location) "Logs\Codex\core-rules-slice-a-playmode-$timestamp.log"
$results = Join-Path (Get-Location) "Logs\Codex\core-rules-slice-a-playmode-$timestamp-results.xml"
& 'C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe' -batchmode -projectPath (Get-Location).Path -logFile $log -runTests -testPlatform PlayMode -testResults $results
```

Expected: XML `<test-run ... result="Passed" failed="0" ...>`.

---

## Self-Review Notes

- This plan intentionally implements Slice A only. Board service components, setup-flow repair buttons, starter packs, movement rule packs, and sample scenes belong to later slices.
- Card runtime is documented in the design spec but not included in Slice A code except through the parity matrix, because board/turn/action contracts are the dependency path.
- This workspace did not present as a git repository during planning, so commit steps are omitted. If git metadata becomes available later, commit after each completed task group.

