# Pyralis Action Queue Runtime Parity Slice B Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a small, testable action queue that can resolve board moves, card plays, turn commands, menu actions, tactical abilities, and scripted actions through the existing action contracts.

**Architecture:** Keep the queue in `NeonBlack.Gameplay.Core.Actions` beside the existing action context/result/resolver contracts. The queue stores immutable `QueuedAction` entries, validates through registered `IActionResolver` instances at enqueue time, and processes FIFO through the first resolver that can handle each context.

**Tech Stack:** Unity 6000.4, C#, NUnit, Unity Test Runner, package asmdefs.

---

### Task 1: Runtime Queue Tests

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/CoreRulesRuntimeTests.cs`

- [ ] **Step 1: Write failing tests**

Add tests for these behaviors:

```csharp
[Test]
public void ActionQueueService_EnqueueAndResolve_ProcessesActionsInOrder()
{
    RecordingActionResolver resolver = new RecordingActionResolver();
    ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });

    Assert.That(queue.TryEnqueue(new ActionExecutionContext("action.first"), out QueuedAction first, out string firstIssue), Is.True, firstIssue);
    Assert.That(queue.TryEnqueue(new ActionExecutionContext("action.second"), out QueuedAction second, out string secondIssue), Is.True, secondIssue);

    ActionResolutionResult firstResult = queue.ResolveNext();
    ActionResolutionResult secondResult = queue.ResolveNext();

    Assert.That(firstResult.Succeeded, Is.True);
    Assert.That(secondResult.Succeeded, Is.True);
    Assert.That(resolver.ResolvedActionIds, Is.EqualTo(new[] { "action.first", "action.second" }));
    Assert.That(first.SequenceId, Is.LessThan(second.SequenceId));
    Assert.That(queue.PendingCount, Is.EqualTo(0));
}

[Test]
public void ActionQueueService_Enqueue_RejectedWhenValidationFails()
{
    RecordingActionResolver resolver = new RecordingActionResolver("action.blocked", ActionValidationResult.Failure("blocked"));
    ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });

    bool accepted = queue.TryEnqueue(new ActionExecutionContext("action.blocked"), out _, out string issue);

    Assert.That(accepted, Is.False);
    Assert.That(issue, Does.Contain("blocked"));
    Assert.That(queue.PendingCount, Is.EqualTo(0));
}

[Test]
public void ActionQueueService_Cancel_RemovesPendingAction()
{
    RecordingActionResolver resolver = new RecordingActionResolver();
    ActionQueueService queue = new ActionQueueService(new IActionResolver[] { resolver });
    Assert.That(queue.TryEnqueue(new ActionExecutionContext("action.cancel"), out QueuedAction queued, out _), Is.True);

    Assert.That(queue.TryCancel(queued.QueueId, out string issue), Is.True, issue);

    Assert.That(queue.PendingCount, Is.EqualTo(0));
    Assert.That(queue.ResolveNext().Status, Is.EqualTo(ActionResolutionStatus.Pending));
}
```

- [ ] **Step 2: Run tests to verify RED**

Run Unity EditMode tests.

Expected: tests fail because `ActionQueueService` and `QueuedAction` do not exist.

### Task 2: Queue Runtime Implementation

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/QueuedAction.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/IActionQueueService.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Actions/ActionQueueService.cs`

- [ ] **Step 1: Implement minimal runtime**

Implement:

- immutable queued action id, sequence id, context, and queued timestamp
- `PendingCount`
- `TryEnqueue(ActionExecutionContext, out QueuedAction, out string)`
- `TryCancel(string queueId, out string)`
- `ResolveNext()`
- FIFO queue semantics
- resolver validation at enqueue
- resolver resolution at processing

- [ ] **Step 2: Run targeted Unity EditMode tests**

Expected: new tests pass.

### Task 3: Docs And Parity

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`

- [ ] **Step 1: Update docs**

Document that action queue is now a foundation-level runtime service and remains sample-less until wired into tabletop and menu flows.

- [ ] **Step 2: Verify docs contract**

Run Unity EditMode tests.

Expected: docs contract stays green.

### Task 4: Final Verification

**Files:** none

- [ ] **Step 1: Run compile smoke**

Run:

```powershell
dotnet restore "Game Studio Core.slnx"
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: build succeeds with no project-code errors.

- [ ] **Step 2: Run Unity tests**

Run EditMode and PlayMode in batchmode without `-quit`.

Expected: both suites pass.
