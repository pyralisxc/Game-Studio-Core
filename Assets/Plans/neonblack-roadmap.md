# Project Overview
- Game Title: Neon Black (Pyralis Codebase)
- High-Level Concept: A highly decoupled, contract-based gameplay platform designed for multi-genre scalability (2D/3D Action, RPG, Tabletop).
- Players: Single player, Local/Network Multiplayer support.
- Inspiration / Reference Games: Hades, Streets of Rage, Classic RPGs.
- Tone / Art Direction: Neon/Cyberpunk (implied).
- Target Platform: PC / Console.
- Render Pipeline: Built-in (Current).
- Input System: New Input System.

# Game Mechanics
## Core Gameplay Loop
The player controls a "Pawn" in a participants-based system, engaging in combat (combos/hitboxes), interacting with RPG elements (Dialogue/Vendors/Skill Trees), and progressing through structured scenes validated by the "Setup Flow."

## Controls and Input Methods
- Character movement (3D Motor / 2D Movement).
- Combat actions (Combos, Targeting).
- Interaction (Dialogue, UI Menus).

# UI
- **RPG HUD**: HP, XP, and Status.
- **Interaction Panels**: Dialogue windows, Vendor shops, Skill tree interfaces.
- **Authoring Tools**: Editor-side Setup Flow and validation reports.

# Key Asset & Context
- `EnemyAI.cs`: Current God class for enemy behavior.
- `PawnCombatBehaviour.cs`: Core combat logic (coroutine-based combos).
- `SkillTreeService.cs` / `VendorService.cs`: RPG foundation services.
- `RpgGoldenSampleRuntime.cs`: Integration point for RPG systems.
- `PyralisSetupFlowMonitor.cs`: Editor-side orchestration.

# Implementation Steps

## Phase 1: Stabilization & Infrastructure
### 1.1 Decomposition of "God Classes"
- **Description**: Refactor `EnemyAI.cs`, `PawnCombatBehaviour.cs`, and `PyralisSetupFlowMonitor.cs` into smaller, single-responsibility components.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### 1.2 Persistence Implementation
- **Description**: Create concrete implementations for `RpgOwnerSaveData` using JSON/Binary format for local storage.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### 1.3 Networking Foundation Hardening
- **Description**: Implement basic Client-Side Prediction for `Motor3D.cs` and standardize the `NetworkManager` setup flow.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Phase 2: Content Foundations (RPG & Combat)
### 2.1 RPG "Golden Proof" Integration
- **Description**: Fully implement `RpgGoldenSampleRuntime.cs`. Connect `SkillTreeService` and `VendorService` to their respective UI Presenters using VContainer.
- **Assigned role**: developer
- **Dependencies**: Phase 1
- **Parallelizable**: No

### 2.2 Combat System Hardening
- **Description**: Transition combat combo logic from coroutines to a state-driven `CombatSequenceDefinition` to reduce brittleness.
- **Assigned role**: developer
- **Dependencies**: Phase 1.1
- **Parallelizable**: Yes

## Phase 3: Intelligence & Complexity
### 3.1 Advanced Group AI
- **Description**: Implement a `BattleManager` to coordinate enemy groups (e.g., flanking, attack tokens).
- **Assigned role**: developer
- **Dependencies**: Phase 1.1
- **Parallelizable**: Yes

### 3.2 Pluggable AI State Machine
- **Description**: Migrate `EnemyAI` logic to a modular state machine or behavior tree system for easier extension.
- **Assigned role**: developer
- **Dependencies**: Phase 1.1
- **Parallelizable**: Yes

## Phase 4: Presentation & Polish
### 4.1 UI/HUD Dependency Refactor
- **Description**: Remove singleton dependencies from UI elements (e.g., `FeedbackHud`) and inject dependencies via `VContainer`.
- **Assigned role**: developer
- **Dependencies**: Phase 2.1
- **Parallelizable**: Yes

### 4.2 PlayMode Test Expansion
- **Description**: Author PlayMode tests for the "Golden RPG Flow" and verify Combat Hitbox timing.
- **Assigned role**: explorer
- **Dependencies**: All Phases
- **Parallelizable**: No

## Phase 5: Legacy Cleanup
### 5.1 Code & Doc Maintenance
- **Description**: Remove deprecated "Legacy" folders, delete the `PlayerRegistry` bridge once replaced by VContainer registries, and purge stale documentation.
- **Assigned role**: developer
- **Dependencies**: All Phases
- **Parallelizable**: No

# Verification & Testing
- **Unit Tests**: Run existing Architecture Contract Tests to ensure no new singletons or `Camera.main` calls were introduced.
- **PlayMode Tests**: Create a test scene that runs through a full Dialogue -> Purchase -> Upgrade cycle automatically.
- **Validation**: Ensure `PyralisSetupFlowMonitor` reports 0 issues on the Golden Sample scene.
