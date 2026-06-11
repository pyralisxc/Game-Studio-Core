# Project Overview
- Game Title: Neon Black (Pyralis Codebase)
- High-Level Concept: Modular gameplay framework for 2D/3D games with a focus on authoring-driven development.
- Target Architecture: Reflective, metadata-driven authoring and validation system.

# Refactor Goals
1. **Reflective Source of Truth**: Move all validation and guidance logic into `[AuthoringContract]` attributes.
2. **Zero Procedural Validation**: Replace the 300+ line `PyralisSetupFlowValidator` with a generic reflective solver.
3. **Deep Asset Hygiene**: Implement a recursive scanner to find unassigned profiles across the entire asset dependency tree.
4. **Unified Inspector**: Consolidate individual feature editors into a single reflective guidance decorator.
5. **Zero Bloat**: Delete legacy procedural code and redundant custom editors upon feature parity.

# Key Assets & Context
- `AuthoringContractAttribute.cs`: The metadata source.
- `PyralisReflectiveContractSolver.cs`: The engine that resolves metadata into validation reports.
- `PyralisSetupFlowValidator.cs`: The legacy procedural validator (to be deleted).
- `PyralisAuthoringWindow.cs`: The UI for project hygiene and setup.
- `PyralisInspectorGuide.cs`: The legacy manual inspector drawing utility (to be consolidated).

# Implementation Steps

## Phase 1: Reflective Spine Enhancement
1. **Audit & Tag**: Ensure all core gameplay components (Combat, Movement, UI) have comprehensive `[AuthoringContract]` metadata.
   - Files: `HealthComponent.cs`, `ProjectileLauncherBase.cs`, `Motor3D.cs`, etc.
   - Role: explorer | developer
2. **Upgrade Reflective Solver**: Expand `PyralisReflectiveContractSolver` to handle complex satisfaction logic (e.g., specific profile types, scene component counts).
   - Files: `PyralisReflectiveContractSolver.cs`
   - Role: developer
3. **Parity Check**: Verify the reflective solver generates the same issues as the procedural validator for a standard scene.
   - Role: developer

## Phase 2: Asset Tree Hygiene (Deep Audit)
1. **Implement Recursive Scanner**: Create a tool that traverses `SessionDefinition` -> `GameModeDefinition` -> `GameSetupProfile` -> `PawnDefinition`.
   - Files: `PyralisAssetHygieneScanner.cs` (New)
   - Role: developer
2. **Field Validation Integration**: Connect the scanner to the `AssignmentFields` property of the authoring contracts.
   - Role: developer
3. **Hygiene Tab Update**: Update the Authoring Window to display deep-asset errors (unassigned fields) alongside project-level issues.
   - Files: `PyralisAuthoringWindow.cs`, `PyralisAuthoringIntentAdvisor.cs`
   - Role: developer

## Phase 3: Unified Inspector Consolidation
1. **Global Guidance Decorator**: Implement a reflective inspector overlay that draws guidance cards for any type with a registered contract.
   - Files: `PyralisReflectiveInspectorOverlay.cs` (New)
   - Role: developer
2. **Feature Editor Migration**: Migrate specific guidance logic from custom editors to the reflective attributes.
   - Files: `ActorFeatureRuntimeGuidedEditors.cs`, `StatusEffectDefinitionEditor.cs`, etc.
   - Role: developer
3. **Custom Editor Deletion**: Delete redundant `CustomEditor` classes.
   - Role: developer

## Phase 4: Final Cleanup & Documentation
1. **Procedural Code Removal**: Delete the legacy logic in `PyralisSetupFlowValidator.cs`.
   - Role: developer
2. **Documentation Refresh**: Update the in-package documentation to reflect the new reflective workflow.
   - Files: `Packages/.../Docs/**/*`
   - Role: developer
3. **Code Maintenance**: Pass for unused namespaces, obsolete warnings, and formatting.
   - Role: developer

# Verification & Testing
1. **Golden Sample Validation**: Ensure `PyralisAuthoringWindow` reports 0 issues on a correctly configured sample scene using ONLY reflective logic.
2. **Break Test**: Intentionally unassign a profile deep in the asset tree (e.g., a Pawn's movement profile) and verify the Hygiene tab flags it immediately.
3. **Inspector Check**: Open any component with a contract (e.g., `HealthComponent`) and verify the Expert Advice and Documentation links are visible without a custom editor script.
