# Project Overview
- **Game Title**: Neon Black (Pyralis Codebase Alignment)
- **High-Level Concept**: A comprehensive modernization and documentation pass for the Pyralis gameplay framework. The goal is to move from a "working codebase" to a "professional-grade platform" where every script is self-documenting, every mechanic is extensible, and every developer path is clearly paved.
- **Players**: Single player, Local/Network Multiplayer support.
- **Inspiration / Reference Games**: Modular engines (Unreal/Unity ECS patterns), highly decoupled combat systems (Hades, Streets of Rage).
- **Tone / Art Direction**: Technical excellence, professional structure.
- **Target Platform**: PC / Console.
- **Render Pipeline**: Built-in.
- **Input System**: New Input System.

# Game Mechanics
## Core Gameplay Loop
The Pyralis framework coordinates "Participants" (Players/AI) through "Sessions" (Levels/Matches). It manages the lifecycle of "Pawns" (Characters) and their "Features" (Abilities/Logic) using a contract-based dependency model.

## Controls and Input Methods
- **Input Routing**: Semantic mapping of Input Actions to gameplay roles.
- **Customization**: Input profiles allow per-pawn or per-session control overrides.

# UI
- **Authoring Window**: The primary workspace for designers to define "Intent" and "Route."
- **Setup Flow**: Real-time validation and checklist for scene readiness.
- **Metadata Overlays**: Inline documentation and "Expert Advice" surfaced directly in the Unity Inspector.

# Key Asset & Context
- `AuthoringContractAttribute.cs`: The core metadata engine that powers reflective discovery.
- `SessionDefinition.cs` / `GameModeDefinition.cs` / `PawnDefinition.cs`: The "Big Three" orchestration assets.
- `HealthComponent.cs` / `HitBox.cs`: Core combat logic providers.
- `Motor2D.cs` / `Motor3D.cs`: Foundation movement providers.
- `PyralisSetupFlowValidator.cs`: The validator coordinating the whole setup experience.

# Implementation Steps

## Phase 1: Core Orchestration & Metadata Foundation
- **Description**: Apply deep `[AuthoringContract]` decoration to the "Big Three" definitions and their supporting profiles. Move from magic strings to `nameof()` for all assignment fields.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Phase 2: Mechanic Alignment (Combat & Movement)
- **Description**: Review logic in `HealthComponent`, `HitBox`, and `Motor` scripts. Ensure they offer maximum customization via profiles. Add complete contract metadata including "Expert Advice" and "First Proof" steps.
- **Assigned role**: developer
- **Dependencies**: Phase 1
- **Parallelizable**: Yes

## Phase 3: RPG & Modular Systems Hardening
- **Description**: Align `SkillTreeService`, `ProgressionService`, and `FeatureModuleDefinition`. Ensure they are correctly categorized in the Authoring Advisor and have clear documentation links.
- **Assigned role**: developer
- **Dependencies**: Phase 1
- **Parallelizable**: Yes

## Phase 4: Reflective Advisor Integration
- **Description**: Update the `PyralisAuthoringIntentAdvisor` and `PyralisReflectiveContractSolver` to fully utilize the new metadata (Priority, Axioms, Lanes) to provide "Perfect Routes" for designers.
- **Assigned role**: developer
- **Dependencies**: Phase 2, Phase 3
- **Parallelizable**: No

## Phase 5: Documentation & Manual Refresh
- **Description**: Crawl the `Docs/Setup/` folder. Update the Markdown manuals to reflect the current state of the framework. Ensure all `DocumentationURL` fields in code point to the correct files.
- **Assigned role**: explorer
- **Dependencies**: All prior phases
- **Parallelizable**: No

# Verification & Testing
- **Contract Integrity**: Run `PyralisReflectiveContractSolver` checks to ensure 100% decoration coverage on primary systems.
- **Validation**: Ensure `PyralisSetupFlowValidator` correctly identifies missing requirements for new contracts.
- **Proof Run**: Execute the `RpgHubProof.unity` scene and verify that the Authoring Window correctly guides the setup from scratch.
