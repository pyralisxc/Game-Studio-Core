# Project Overview
- **Game Title**: Pyralis (Neon Black Framework)
- **High-Level Concept**: A high-fidelity, metadata-driven gameplay framework for Unity, focused on "Zero-Friction Authoring" through reflective discovery and DI-driven architecture.
- **Players**: Developers and Designers.
- **Tone / Art Direction**: Professional, technical, documentation-heavy, developer-centric (Cyberpunk/Neon UI for tooling).
- **Target Platform**: Unity Editor / Multi-platform Runtime.
- **Render Pipeline**: Built-in / URP.

# Phased Implementation Plan: The Road to Zero-Friction Authoring

This plan outlines the steps to move Pyralis from its current transitional state to a fully hardened, legacy-free, and self-documenting engine.

## Phase 1: Metadata Standardization & Fact Discovery
**Description**: Ensure every feature, service, and component is correctly tagged with the new `AuthoringContract` metadata. This turns the codebase into a "searchable database" for the Editor.
- **Action**: Audit and update all core classes with `[AuthoringContract]`.
    - Populate `Capability`, `Relevance`, `ExpertAdvice`, and `DocumentationURL`.
    - Define `AssignmentFields` to guide the UI on which Inspector fields matter most.
- **Action**: Implement the `PyralisAuthoringFactRegistry`.
    - Ensure it reflectively discovers all contracts at domain reload.
    - Store "Provenance" (where the fact came from) to aid in debugging.
- **Action**: Build the "Hygiene" Validator.
    - A tool that flags any `IFeatureModuleRuntime` or `IGameService` missing a contract.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Phase 2: UI Toolkit Migration & Authoring UX
**Description**: Complete the transition of the Authoring Window to UI Toolkit, focusing on "Semantic Intent" and "Setup Mapping."
- **Action**: Finalize `PyralisAuthoringWindow.uxml` and `.uss`.
    - Implement the **Intent Tab**: A clear dashboard for DNA (Axioms), Spine (Capabilities), and Lane (Presentation).
    - Implement the **Map Tab**: A visual "Setup Chain" (Bootstrap -> Session -> Game Mode -> Pawn).
- **Action**: Integrated "Issue Cards".
    - Replace the generic error log with actionable "Cards" that link directly to the field or object needing repair.
- **Action**: Expert Advice Integration.
    - Dynamically display `ExpertAdvice` strings when a user selects a component in the Hierarchy.
- **Assigned role**: developer
- **Dependencies**: Phase 1
- **Parallelizable**: Yes

## Phase 3: Logic Hardening & "Golden Path" Proofs
**Description**: Flush out the current feature set (RPG, Tabletop, Combat) to ensure they are 100% functional, VContainer-compliant, and proven.
- **Action**: Standardize the RPG Spine.
    - Ensure `DialogueService`, `QuestService`, and `InventoryService` are fully decoupled and use constructor injection.
- **Action**: Finalize Tabletop Rules.
    - Complete the "Named Move Policies" and "Terminal Conditions" for board games.
- **Action**: Create "First Playable Proofs" (Scenes).
    - **1P Movement Proof**: Standard side-scroller/brawler.
    - **Tabletop Proof**: A 1-vs-1 grid move and capture scenario.
    - **RPG Hub Proof**: A NPC -> Dialogue -> Quest -> Reward loop.
- **Action**: Implement Automated Proof-Checkers.
    - A button in the Editor that runs a headless Play Mode test to verify the "First Proof" defined in the `AuthoringContract`.
- **Assigned role**: explorer | developer
- **Dependencies**: Phase 2
- **Parallelizable**: Yes

## Phase 4: Legacy & Preset Removal
**Description**: Clean up the "refactor debt" and remove all preset content to ensure a single, maintainable path for future development.
- **Action**: Deprecate and Delete `PlatformServiceRegistry`.
    - Migrate all remaining consumers to VContainer `IObjectResolver` or direct constructor injection.
- **Action**: Remove all "Quick-Select" presets and hardcoded starter setups.
    - Ensure the system relies entirely on reflective discovery and user customization.
- **Action**: Singleton Purge.
    - Remove static `.Instance` properties from `SceneLoader`, `TimeManager`, and `ParticipantRosterService`.
- **Action**: Standardize Folder Structure.
    - Move all scripts into the governed `Features/[Name]/Runtime/` and `Editor/` structure.
- **Assigned role**: developer
- **Dependencies**: Phase 3
- **Parallelizable**: No

## Phase 5: Automated Documentation & Sustained Maintenance
**Description**: Ensure the project remains "self-documenting" so that documentation never drifts from reality again.
- **Action**: Finalize `PyralisDocGenerator`.
    - A utility that reads the `FactRegistry` and outputs `README.md` files for every feature folder.
    - Updates `MANUAL.md` and `START_HERE.md` based on active system capabilities.
- **Action**: XML Documentation Pass.
    - Audit all public APIs for proper `<summary>`, `<param>`, and `<returns>` tags.
- **Action**: Create "Standard Feature Template".
    - A script template for new Feature Modules that includes the `AuthoringContract` boilerplate and DI setup.
- **Assigned role**: developer
- **Dependencies**: Phase 4
- **Parallelizable**: Yes

# Key Asset & Context
- `PyralisAuthoringWindow.cs`: Target for UI unification.
- `AuthoringContractAttribute.cs`: The core metadata bridge.
- `PyralisGameplayLifetimeScope.cs`: The DI composition root.
- `PyralisDocGenerator.cs`: The documentation automation engine.
- `PyralisAuthoringFactRegistry.cs`: The reflective discovery hub.

# Verification & Testing
- **Fact Coverage Test**: Run the Hygiene Validator; it must report 100% coverage with 0 "None recorded yet" entries.
- **DI Audit**: Use the VContainer debugger in every proof scene; there should be 0 "fallback" or "singleton" resolutions.
- **Doc Integrity Check**: Change a `Relevance` string in an attribute, run the `DocGenerator`, and verify the README updates instantly.
- **Play Proofs**: All "First Playable Proofs" must pass in one click from the Authoring Window.
