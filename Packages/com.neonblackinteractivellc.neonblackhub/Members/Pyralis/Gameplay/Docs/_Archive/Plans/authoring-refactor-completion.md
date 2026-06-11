# Plan: Full Codebase Authoring Refactor Completion

This plan outlines the final steps to achieve 100% 'Authored' status for the Neon Black/Pyralis package, moving from manual logic to a fully reflective, contract-backed system.

## Phase 1: Scene Evidence & Contract Bridge
*   Refactor `PyralisSceneSurfaceEvidenceFacts` to query the `PyralisAuthoringContractRegistry`.
*   Implement logic to verify if discovered scene objects satisfy the `RequiredComponents` or `RequiredInterfaces` of active contracts.
*   Update UI to show 'Contract Satisfied' when a valid GameObject is found.

## Phase 2: Input & Action Mapping
*   Apply `[AuthoringContract]` to `InputProfile`, `InputConfig`, and `ParticipantInputRouter`.
*   Add metadata to feature contracts (e.g., Combat, Interaction) specifying required Action Names.
*   Update the Validator to flag missing Action Map bindings in the Input System.

## Phase 3: Asset Generation & Tooling
*   Update `PyralisAuthoringWindow` to support a 'Generate' action for any contract.
*   Implement a factory that uses `NativeSetup` and `AssignmentFields` to instantiate and configure required prefabs/assets.
*   Add 'Fix' buttons to validation warnings that automatically create missing assets.

## Phase 4: Remaining System Coverage
*   Sweep `Gameplay/Presentation/UI` for UI Orientation and Windowing contracts.
*   Tag any remaining RPG or Combat sub-systems (e.g., Status Effect definitions, Ability presets).
*   Ensure all `ScriptableObject` definitions that act as 'Contracts' are tagged.

## Phase 5: Final Validation & Hardening
*   Resolve the string-persistence conflict in `PyralisAuthoring2_Source_KeepsAuthoringSpineOutOfPlayerBuildAssemblies`.
*   Ensure the `PyralisReflectiveFactScanner` is gated by `#if UNITY_EDITOR` while keeping attributes lightweight.
*   Run the full `Run-PreSceneValidation.ps1` suite.
