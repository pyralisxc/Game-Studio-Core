# Project Overview
- **Game Title:** Pyralis Core (Authoring System)
- **High-Level Concept:** A system that reflectively guides the user to build games by scanning Core contracts and validating scene state against those contracts.
- **Players:** Developer-facing tool.
- **Inspiration:** Unity's own reflective inspectors, but for high-level "Game Design Intent."
- **Render Pipeline:** Built-in (as per project settings).
- **Unity Version:** 6000.4.0f1

# Game Mechanics
## Core Gameplay Loop (Authoring)
1. User selects **Axioms** in a `GameSetupProfile` (e.g., 2D, Pawn-backed, Scoring).
2. The **Reflective Scanner** finds all `[AuthoringContract]` types that match those Axioms.
3. The **Contract Solver** validates the scene/assets against the requirements defined in the attributes.
4. The **Authoring Window** displays the "Satisfaction" state and provides "Native Actions" (e.g., "Add Component") to fix issues.

# Key Asset & Context
- `AuthoringContractAttribute.cs`: The "Spine." Needs expansion to handle validation rules.
- `PyralisReflectiveFactScanner.cs`: Currently just lists facts; needs to become the solver.
- `PyralisSetupFlowMonitor.cs`: The "Old Logic" to be gutted (1,800 lines of hardcoded switches).
- `GameSetupProfile.cs`: The source of truth for active Axioms.

# Implementation Steps

## Phase 1: Metadata Enrichment
1. **Expand `AuthoringContractAttribute`**: 
   - Add `RequiredInterfaces` (Type array).
   - Add `RequiredComponents` (Type array).
   - Add `SatisfactionReason` (string) for error messages.
   - **Role:** developer | **Parallelizable:** No | **Dependencies:** None

## Phase 2: Generic Validation Engine
2. **Implement `PyralisContractSolver`**:
   - Create a static class that replaces `PyralisSetupFlowValidator`.
   - Iterate over `TypeCache.GetTypesWithAttribute<AuthoringContractAttribute>()`.
   - For each contract, perform a scene/project search for the `RequiredInterfaces/Components`.
   - **Role:** developer | **Parallelizable:** No | **Dependencies:** Step 1

## Phase 3: UI Unification
3. **Update `PyralisAuthoringWindow`**:
   - Change the "Guide" and "Setup" data sources from the old `PyralisSetupFlowStep` list to the results of the `PyralisContractSolver`.
   - **Role:** developer | **Parallelizable:** Yes | **Dependencies:** Step 2

## Phase 4: The Great Cleanup
4. **Decommission Old Code**:
   - Delete `PyralisSetupFlowStepId` enum.
   - Delete `PyralisSetupFlowMonitor.cs` (or gut its contents).
   - Remove hardcoded references in `PyralisSetupRouteAnalysis`.
   - **Role:** developer | **Parallelizable:** Yes | **Dependencies:** Step 3

# Verification & Testing
- **Reflective Scan Test:** Verify that tagging a new interface in Core with `[AuthoringContract]` immediately makes it show up in the Authoring Window without editing Editor code.
- **Validation Test:** Verify that deleting a required component from the scene causes the Reflective Solver to flag it as "Missing."
- **Axiom Filter Test:** Verify that changing the `GameSetupProfile` from 2D to 3D correctly filters the visible contracts.
