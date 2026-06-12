# Project Overview
- **Game Title:** Pyralis Core (Authoring System)
- **High-Level Concept:** A reflective developer intent and setup guidance system that automatically discovers gameplay capabilities and provides beginners with native Unity setup steps.
- **Players:** Developers and Technical Artists.
- **Inspiration:** Unity's own Component/Attribute system, but layered with "Semantic Intent."
- **Tone / Art Direction:** Clean, technical, and data-driven Editor Tooling.
- **Target Platform:** Unity Editor (Editor-only infrastructure).
- **Render Pipeline:** N/A (Editor Tooling).

# Game Mechanics (Authoring)
## Core Pipeline Loop
1. **Reflect**: The system scans the assembly for `[AuthoringContract]` attributes.
2. **Synthesize**: The "Brain" reflectively extracts Unity metadata (CreateAssetMenu, AddComponentMenu, RequireComponent) to build Native Action guides.
3. **Register**: Facts are cached in a central registry for all UI tabs to consume.
4. **Advise**: The Intent Tab scores these facts against the developer's current "Focus" (Goal + Axioms).

## Controls and Input Methods
- **Intent Tab Selection**: Hierarchical selection of gameplay goals.
- **Fact Explorer**: Read-only coverage view of the project's capabilities.
- **Contract Tags**: Developers tag scripts to "opt-in" to the authoring system.

# UI
- **Intent Tab**: Updated to support hierarchical goal trees (e.g., `Actor/Movement/3D`).
- **Fact Cards**: Automatically populated with "Native Action" badges derived from reflection.

# Key Asset & Context
- **Registry**: `PyralisAuthoringFactRegistry.cs` (The central hub).
- **Scanner**: `PyralisReflectiveFactScanner.cs` (The reflective brain).
- **Metadata**: `AuthoringContractAttribute.cs` (The developer API).
- **UI**: `PyralisAuthoringWindow.cs` (The IMGUI interface).

# Implementation Steps

## Phase 1: Infrastructure Unification (The "Singular Path")
1. **Unify Scanning Logic**: Merge `PyralisReflectiveFactScanner` and `ResolvedAuthoringContractRegistry` logic into a singular stream within `PyralisAuthoringFactRegistry.BuildFacts`.
   - **Assigned role**: developer
   - **Dependencies**: None
   - **Parallelizable**: No
2. **Implement Taxonomy Support**: Update `AuthoringContractAttribute` and `PyralisAuthoringFact` to support hierarchical goal paths (e.g., splitting by `/`).
   - **Assigned role**: developer
   - **Dependencies**: Step 1
   - **Parallelizable**: Yes

## Phase 2: Reflective Synthesis (The "Brain")
3. **Automate Native Actions**: Update the scanner to reflectively read `CreateAssetMenuAttribute`, `AddComponentMenu`, and `RequireComponent` from tagged types.
   - **Assigned role**: developer
   - **Dependencies**: Step 1
   - **Parallelizable**: Yes
4. **Automate Field Discovery**: Update the scanner to identify `[SerializeField]` and `[PropertyOrder]` for automatic "Assignment Field" and "Customization Moment" generation.
   - **Assigned role**: developer
   - **Dependencies**: Step 3
   - **Parallelizable**: Yes

## Phase 3: UI & Cleanup (The "Clean Codebase")
5. **Update Intent Tab Selection**: Modify `PyralisAuthoringWindow` to render the Goal selection as a hierarchical menu or tree instead of a flat list.
   - **Assigned role**: developer
   - **Dependencies**: Step 2
   - **Parallelizable**: No
6. **Deprecate Side Carts**: Identify manual facts in `PyralisConventionAuthoringFacts.cs` that are now covered by Step 3 & 4 and remove them to ensure a singular data path.
   - **Assigned role**: explorer/developer
   - **Dependencies**: Step 3, 4
   - **Parallelizable**: No
7. **Documentation & Maintenance**: Update the inline XML documentation for `AuthoringContractAttribute` to explain the new automated setup flow.
   - **Assigned role**: developer
   - **Dependencies**: All prior steps
   - **Parallelizable**: Yes

# Verification & Testing
- **Reflective Parity Test**: A unit test that compares the output of a manual "Convention" fact against the new automated "Reflective" fact for the same type (e.g., `SessionDefinition`).
- **Hierarchy Test**: Verify that a goal tagged as `Combat/Reaction` correctly appears in both the `Combat` and `Reaction` filtered views.
- **Coverage Audit**: Use the Authoring Window's "Total Facts" and "Coverage Summary" to ensure no capabilities were lost during the unification.
