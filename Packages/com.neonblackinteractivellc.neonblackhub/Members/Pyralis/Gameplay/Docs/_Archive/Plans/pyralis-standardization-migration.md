# Pyralis Standardization & Legacy Removal Plan

## Project Overview
- **Game Title:** Neon Black Hub / Pyralis Framework
- **High-Level Concept:** A metadata-driven, modular gameplay framework using DI (VContainer) and reflective authoring to empower designers and programmers.
- **Players:** Multi-platform (Single, Local Coop, Networked).
- **Inspiration:** Modern component-based architectures, "Senior-level" Unity engineering.
- **Tone / Art Direction:** Technical, professional, documentation-heavy, developer-centric.
- **Render Pipeline:** Built-in / URP compatible.

## Standard Working System (The "Source of Truth")
The project is standardizing on **Reflective Capability-Based Authoring**.
1. **Metadata**: Every feature/service MUST be tagged with `[AuthoringContract]` using `AuthoringCapability` enums.
2. **Dependency Injection**: No singletons. All services resolved via `VContainer` in `PyralisGameplayLifetimeScope`.
3. **Composition**: Features are added to actors via `FeatureModuleDefinition` and `PawnDefinition`.
4. **Validation**: The `Authoring Window` is the primary interface for setup, using the `SetupFlowValidator` to guide users.

---

## Implementation Steps

### Phase 1: Metadata Standardization (Coverage)
**Description**: Bring all existing features up to the new metadata standard to eliminate "guesses" in the Authoring Window.
- **Action**: Update all `[AuthoringContract]` attributes. Replace `Goal = "..."` (deprecated) with `Capability = AuthoringCapability....`.
- **Action**: Populate `NativeSetup`, `AssignmentFields`, `CustomizationMoments`, and `FirstProof` for all core features (Movement, Combat, UI, Scoring).
- **Assigned role**: explorer | developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Phase 2: Legacy Infrastructure Removal
**Description**: Cut the "safety nets" used during the refactor.
- **Action**: Deprecate and delete `PlatformServiceRegistry`.
- **Action**: Move all manual service registrations from `GameplaySessionBootstrap` into the `PyralisGameplayLifetimeScope` VContainer graph.
- **Action**: Remove `ModuleId` string mappings from contracts once they are reliably discovered by type/capability.
- **Action**: Remove static `.Instance` properties from `SceneLoader`, `TimeManager`, and `ParticipantRosterService`.
- **Assigned role**: developer
- **Dependencies**: Phase 1
- **Parallelizable**: No

### Phase 3: Actor Stack Consolidation
**Description**: Ensure all playable actors follow the modular "Motor" pattern.
- **Action**: Migrate remaining legacy player scripts to the `Motor2D` / `Motor3D` component stack.
- **Action**: Standardize `PawnRoot` as the single entry point for all pawn-based gameplay.
- **Assigned role**: developer
- **Dependencies**: Phase 2
- **Parallelizable**: Yes

### Phase 4: Automated Documentation & Tooling
**Description**: Ensure the "Facts" stay true and users are guided correctly.
- **Action**: Update `PyralisDocGenerator` to exclude legacy categories and prioritize Capability-based grouping.
- **Action**: Update `START_HERE.md` and `MANUAL.md` to remove references to legacy singleton paths.
- **Assigned role**: explorer | developer
- **Dependencies**: Phase 1
- **Parallelizable**: Yes

### Phase 5: Verification & Long-term Maintenance
**Description**: Formalize the maintenance of the standard.
- **Action**: Add a "Validation Pass" to the `PyralisAuthoringWindow` that flags any `IFeatureModuleRuntime` without a contract.
- **Action**: Create a "Standard Template" script for new features to ensure developers follow the pattern.
- **Assigned role**: developer
- **Dependencies**: Phase 4
- **Parallelizable**: No

---

## Verification & Testing
- **Fact Explorer Audit**: Open the Authoring Window -> Facts tab. Total Facts should match the number of active features, and "Coverage" should be 100% (no "None recorded yet").
- **DI Validation**: Run the `SetupFlowValidator` in a clean scene. Ensure all core services resolve via the `LifetimeScope` without fallback singletons.
- **1P Movement Proof**: Run the standard movement proof in a 2D and 3D scene to verify the consolidated motor stack works across both lanes.
- **Documentation Build**: Run the `DocGenerator` and verify the output Markdown is clean, typed, and free of "Legacy" sections.
