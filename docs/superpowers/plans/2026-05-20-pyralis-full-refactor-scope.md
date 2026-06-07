# Pyralis Full Refactor Scope Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finish the Pyralis gameplay refactor into a stable, multi-assembly, designer-friendly platform with clean domain ownership, no legacy service-locator/runtime-discovery architecture, and normalized 2D/3D pawn stacks.

**Architecture:** The package remains one Unity package, but internal ownership becomes strict: `Core` owns transport-agnostic infrastructure and contracts, `Characters` owns participant/session/pawn runtime, `Features` owns domain logic, and later domain assemblies split along already-clean seams. The refactor continues by cutting assemblies only after the contracts, namespaces, and runtime seams are already clean enough to support one-way references.

**Tech Stack:** Unity 6, URP, VContainer, Netcode for GameObjects adapters, Input System, Addressables, Localization, Unity Test Framework, `dotnet build` verification.

---

## File Structure Scope

### Already stabilized enough to build on

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/Runtime/Shared/`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/2D/`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/3D/`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Composition/`
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/`

### Remaining structure targets

- Create stable assembly ownership for:
  - `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Presentation/`
  - `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Networking/`
  - `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Integrations/`
  - feature-domain asmdefs under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/`
- Normalize feature folders toward:
  - `Runtime/Shared`
  - `Runtime/2D`
  - `Runtime/3D`
  - `Data`
  - `Editor`
  - `Tests`
  - `Docs`

---

### Task 1: Finish Core Namespace And Ownership Normalization

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/**/*.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/**/*.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/**/*.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] **Step 1: Write or extend failing architecture tests for remaining namespace drift**
- [ ] **Step 2: Run editor/runtime builds or focused tests to confirm the current namespace mismatch still exists**
- [ ] **Step 3: Rename remaining `NeonBlack.Gameplay.Shared.Core.*` namespaces to `NeonBlack.Gameplay.Core.*` and update imports**
- [ ] **Step 4: Re-run builds/tests until the namespace enforcement passes**
- [ ] **Step 5: Commit**

### Task 2: Finish Characters Namespace And Topology Ownership

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/**/*.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Networking/Characters/**/*.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/PawnSystemTests.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/ParticipantRuntimeTests.cs`

- [ ] **Step 1: Add failing enforcement for remaining `NeonBlack.Gameplay.Characters` ownership gaps**
- [ ] **Step 2: Move all participant/session/pawn namespaces off `Shared.Participants` and onto `NeonBlack.Gameplay.Characters`**
- [ ] **Step 3: Align networking character adapters to the new `Characters` namespace without reintroducing cross-assembly cycles**
- [ ] **Step 4: Re-run runtime tests/builds for bootstrap, participant, and pawn flows**
- [ ] **Step 5: Commit**

### Task 3: Normalize Feature Composition Namespace Ownership

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Composition/*.cs`
- Modify: feature runtimes consuming `ActorFeatureContext` or `FeatureRuntimeInitializationContext`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/PlatformKernelTests.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] **Step 1: Add or extend failing tests that assert composition types use `NeonBlack.Gameplay.Features.Composition`**
- [ ] **Step 2: Rename `Shared.Features` imports/usages to `Features.Composition`**
- [ ] **Step 3: Re-run feature host and initialization context tests**
- [ ] **Step 4: Commit**

### Task 4: Create Presentation Ownership Boundary

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Presentation/NeonBlack.Gameplay.Presentation.asmdef`
- Move/Modify:
  - `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Animation/ActorAnimationDriver.cs`
  - `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Camera/CinemachineCameraRigController.cs`
  - `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Visuals/**/*`
  - presentation-facing definitions/profiles if they have no feature-runtime ownership
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] **Step 1: Add failing test asserting the Presentation asmdef exists**
- [ ] **Step 2: Extract presentation-owned runtime types into the `Presentation` domain and update namespaces/imports**
- [ ] **Step 3: Update asmdef references so gameplay features consume presentation contracts rather than concrete misc ownership**
- [ ] **Step 4: Re-run runtime/editor builds**
- [ ] **Step 5: Commit**

### Task 5: Create Combat Domain Assembly

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Combat/NeonBlack.Gameplay.Feature.Combat.asmdef`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Combat/**/*.cs`
- Modify: consumers in `Characters`, `Enemies`, `Hazards`, `Feedback`, `Pickups`
- Test: combat-related tests and validation tests

- [ ] **Step 1: Add failing test asserting the Combat asmdef exists**
- [ ] **Step 2: Identify remaining concrete dependencies Combat exports to sibling domains**
- [ ] **Step 3: Extract any tiny missing contracts to `Core` or `Features.Composition` where required**
- [ ] **Step 4: Add the Combat asmdef with one-way references only**
- [ ] **Step 5: Re-run full build matrix**
- [ ] **Step 6: Commit**

### Task 6: Create Traversal Domain Assembly

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Traversal/NeonBlack.Gameplay.Feature.Traversal.asmdef`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Traversal/**/*.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Zones/**/*Climb*`
- Test: traversal runtime and validation tests

- [ ] **Step 1: Add failing test asserting the Traversal asmdef exists**
- [ ] **Step 2: Cut remaining direct references between traversal code and non-traversal character internals**
- [ ] **Step 3: Add the Traversal asmdef with only approved dependencies**
- [ ] **Step 4: Re-run builds and targeted traversal scenarios**
- [ ] **Step 5: Commit**

### Task 7: Create Feedback, Scoring, Interaction, and Pickups Assemblies

**Files:**
- Create:
  - `Features/Feedback/NeonBlack.Gameplay.Feature.Feedback.asmdef`
  - `Features/Scoring/NeonBlack.Gameplay.Feature.Scoring.asmdef`
  - `Features/Interaction/NeonBlack.Gameplay.Feature.Interaction.asmdef`
  - `Features/Pickups/NeonBlack.Gameplay.Feature.Pickups.asmdef`
- Modify: each corresponding feature folder and cross-domain imports
- Test: feature and editor tests covering their validation/runtime seams

- [ ] **Step 1: Add failing tests asserting each asmdef exists**
- [ ] **Step 2: Extract minimal contracts for publisher/query seams where needed**
- [ ] **Step 3: Add asmdefs one domain at a time, verifying after each**
- [ ] **Step 4: Commit after each domain or pair of low-risk domains**

### Task 8: Create GameFlow, Enemies, Zones, Camera, Respawn, and Integrations Assemblies

**Files:**
- Create:
  - `Features/GameFlow/NeonBlack.Gameplay.Feature.GameFlow.asmdef`
  - `Features/Enemies/NeonBlack.Gameplay.Feature.Enemies.asmdef`
  - `Features/Zones/NeonBlack.Gameplay.Feature.Zones.asmdef`
  - `Features/Camera/NeonBlack.Gameplay.Feature.Camera.asmdef` or roll camera runtime into `Presentation` after final decision
  - `Features/Respawn/NeonBlack.Gameplay.Feature.Respawn.asmdef`
  - `Integrations/NeonBlack.Gameplay.Integrations.asmdef`
- Modify: cross-domain references and remaining scene/config seams
- Test: encounter/camera/gameflow/editor validation tests

- [ ] **Step 1: Decide whether camera runtime lives under `Presentation` or `Feature.Camera` and lock that choice**
- [ ] **Step 2: Add failing tests for the chosen asmdefs**
- [ ] **Step 3: Split domains in dependency order: Enemies -> Zones -> GameFlow -> Respawn -> Integrations**
- [ ] **Step 4: Re-run the full build matrix after each cut**
- [ ] **Step 5: Commit**

### Task 9: Physically Normalize Feature Folder Shape

**Files:**
- Modify/move across `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/**/*`
- Test: editor architecture enforcement tests
- Docs: feature template docs

- [ ] **Step 1: Add failing test for normalized feature folder presence on migrated domains**
- [ ] **Step 2: Move per-feature data, editor, tests, and docs beside the owning feature**
- [ ] **Step 3: Remove stray feature files from `Data/` or top-level mixed locations where ownership is now clear**
- [ ] **Step 4: Re-run builds and folder enforcement tests**
- [ ] **Step 5: Commit**

### Task 10: Finish Networking Isolation And Backend Boundary

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Networking/**/*.cs`
- Modify: any feature/runtime code still touching NGO specifics directly
- Test: networking-related runtime tests

- [ ] **Step 1: Add failing search/enforcement test for forbidden direct NGO use outside networking ownership**
- [ ] **Step 2: Finish moving transport-agnostic contracts to `Core.Contracts.Networking`**
- [ ] **Step 3: Ensure only `Networking/` owns backend-aware adapters**
- [ ] **Step 4: Re-run build matrix**
- [ ] **Step 5: Commit**

### Task 11: Finish Authoring And Validation Framework Alignment

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/**/*.cs`
- Modify: inspectors, validation dashboard, and setup windows
- Test: editor tests for validation and wizard generation
- Docs: setup guides and authoring docs

- [ ] **Step 1: Add failing tests for required asmdef presence, folder normalization, and forbidden runtime discovery**
- [ ] **Step 2: Update validation/editor tooling to understand the final domain-owned file layout**
- [ ] **Step 3: Add or refresh setup flows for session, pawn, mode, and feature module assets**
- [ ] **Step 4: Re-run editor tests/builds**
- [ ] **Step 5: Commit**

### Task 12: Final Docs And Historical Cleanup

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/**/*.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/README.md`

- [ ] **Step 1: Remove active-guidance references to deleted bridges, removed service locators, and old namespace ownership**
- [ ] **Step 2: Keep historical notes only where they are explicitly labeled as history**
- [ ] **Step 3: Update architecture docs to show the final asmdef map and feature template**
- [ ] **Step 4: Commit**

---

## Verification Gates

Run after every assembly split wave:

- `dotnet build Neonblackinteractivellc.Neonblackhub.csproj --no-restore`
- `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.csproj --no-restore`
- `dotnet build Neonblackinteractivellc.Neonblackhub.Tests.csproj --no-restore`
- `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.Tests.csproj --no-restore`

Add Unity batch refresh when asmdefs move:

- `\"C:\\Program Files\\Unity\\Hub\\Editor\\6000.4.0f1\\Editor\\Unity.exe\" -batchmode -projectPath \"C:\\Users\\camer\\Desktop\\Game Studio\\Game Studio Core\" -quit`

---

## Final Acceptance Criteria

- `Core`, `Data`, `Characters`, `Networking`, and `Presentation` have clean ownership and matching namespaces.
- `Combat`, `Traversal`, `Feedback`, `Scoring`, `Interaction`, `Pickups`, `GameFlow`, `Enemies`, `Zones`, `Camera`/`Presentation`, `Respawn`, and `Integrations` each have stable asmdef ownership where justified.
- No active runtime code depends on `GameServices`.
- No feature-layer runtime code uses scene-wide discovery for platform wiring.
- `Motor2D` remains only as compatibility facade, not monolithic owner.
- `ActorFeatureContext` remains narrow and contract-based.
- Docs describe the current architecture rather than deleted bridges or service locators.
- Full build matrix passes cleanly.

---

## Recommended Execution Order

1. Core namespace cleanup completion
2. Characters namespace/topology normalization
3. Composition namespace normalization
4. Presentation assembly cut
5. Combat assembly cut
6. Traversal assembly cut
7. Feedback / Scoring / Interaction / Pickups assembly cuts
8. Enemies / Zones / GameFlow / Respawn / Integrations cuts
9. Folder normalization
10. Networking isolation pass
11. Authoring/test enforcement pass
12. Final docs cleanup

Plan complete and saved to `docs/superpowers/plans/2026-05-20-pyralis-full-refactor-scope.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

Which approach?
