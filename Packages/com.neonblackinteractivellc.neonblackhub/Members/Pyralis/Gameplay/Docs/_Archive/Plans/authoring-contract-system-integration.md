# Project Overview
- Game Title: Neon Black (Pyralis)
- High-Level Concept: A modular, route-based Unity framework for building action, tabletop, and RPG prototypes using guided setup and contract-backed authoring.
- Players: Single player, local multiplayer, networked multiplayer.
- Inspiration / Reference Games: Modular action frameworks, tabletop engines.
- Tone / Art Direction: Neon, modular, developer-friendly.
- Target Platform: Standalone (Windows/Mac/Linux), potentially Mobile/Web.
- Screen Orientation / Resolution: Landscape 1920x1080.
- Render Pipeline: Built-in.

# Game Mechanics
## Core Gameplay Loop
The core loop is centered around "Routes" and "Features". Creators select a route (e.g., 2D Sprite Pawn Action) and attach "Feature Modules" to their participants (Pawns). The system validates these modules against "Contracts" to ensure correct scene wiring and asset assignment.

## Controls and Input Methods
Semantic input mapping via `InputProfile`. Actions like "Jump" or "Interact" are bound to feature modules (e.g., `TopDownHopFeatureRuntime`) that consume these roles.

# UI
The system primarily manifests in the **Pyralis Authoring Window**, which uses the contract data to:
1. Provide a **Guide** for setting up features.
2. Surface **Facts** about the current project state.
3. Show **Validation Issues** for missing or misconfigured components.

# Key Asset & Context
- `AuthoringContractAttribute.cs`: Metadata for reflective discovery.
- `IAuthoringContractProvider.cs`: Interface for dynamic contract provision (to be created).
- `ResolvedAuthoringContractRegistry`: Central registry for discovered contracts.
- `FeatureModuleDefinition.cs`: Asset that defines a feature; will be refactored to use contracts for validation.
- `PyralisReflectiveContractSolver.cs`: Logic that matches scene state against contracts.

# Implementation Steps

## 1. Core Foundation: IAuthoringContractProvider & Registry Update
- **Description**: Define the `IAuthoringContractProvider` interface and update `ResolvedAuthoringContractRegistry` to support it. This ensures both attribute-based and interface-based discovery work in unison.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

## 2. Phase 1: Realtime Action Feature Contracts
- **Description**: Apply `[AuthoringContract]` to core pawn features: `Motor2D`, `Motor3D`, `Interaction`, `Pickups`, and `Combat` (Health, Reaction, Status, Feedback). Ensure `RequiredInterfaces` and `RequiredComponents` match the existing hardcoded validation in `FeatureModuleDefinition`.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## 3. Phase 2: Enemy & Hazard Feature Contracts
- **Description**: Tag `EnemyAI`, `EnemyAmbientFeatureRuntime`, `EnemyReactionFeatureRuntime`, and `HazardFeedbackRuntime` with contracts.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## 4. Phase 3: RPG System Contracts
- **Description**: Integrate the RPG systems (Stats, Inventory, Equipment, Skill Trees, Quests, Dialogue, Hubs) into the authoring model. This involves tagging the service classes and their corresponding profile/definition types.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: Yes

## 5. Phase 4: Validation Logic Refactor
- **Description**: Refactor `FeatureModuleDefinition.GetActorCompatibilityIssues()` to remove the hardcoded `switch` statement. Replace it with a call to the contract registry to perform metadata-driven validation.
- **Assigned role**: developer
- **Dependencies**: Steps 2, 3, 4
- **Parallelizable**: No

## 6. Phase 5: Testing & Debugging
- **Description**: Implement unit tests in `AuthoringContractsContractTests.cs` to verify discovery of all new contracts. Add integration tests to ensure that misconfigured features (e.g., missing a HealthComponent for Combat Reaction) correctly trigger validation issues.
- **Assigned role**: developer
- **Dependencies**: Steps 2-5
- **Parallelizable**: No

# Verification & Testing
- **Unit Tests**: Run tests for `ResolvedAuthoringContractRegistry` to ensure all tagged classes appear in the `All` list.
- **Validation Audit**: Create a test scene with various "broken" Pawn configurations and verify the **Authoring Window** (Facts/Guide tabs) reports the exact missing types defined in the contracts.
- **CLI Gate**: Run `dotnet build` and the `Run-PreSceneValidation.ps1` script to ensure no compilation errors or regressions.
