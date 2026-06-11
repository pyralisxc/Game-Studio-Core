# Pyralis Refactor & Modernization Plan

## Phase 1: Intent Tab & World DNA Upgrades
- Eliminate legacy IMGUI methods from `PyralisAuthoringWindow.cs`.
- Categorize World DNA into exclusive DropdownFields.
- Redefine Presentation Perspective Lanes in `RuntimeCapabilityLaneTag`.
- Isolate Scrolling Containers for Spine Capabilities.

## Phase 2: Dynamic Priority & Deprecation Matrices
- Update `AuthoringContractAttribute.cs` with `AuthoringPriority` and deprecation properties.
- Implement dynamic priority calculation in `PyralisReflectiveContractSolver.cs`.
- Add `DeprecatedContracts_EnforceHardDeletionDeadlines` unit test.

## Phase 3: Systematic Code Decoration & Hardening
- Audit and harden lifecycles (OnDisable cleanups).
- Complete unfinished TODOs/FIXMEs.
- Replace hardcoded 'Player'/'Enemy' tags with interface-driven checks.
- Optimize Update loops (Zero allocations).
- Decorate primary files with complete `[AuthoringContract]` attributes.

## Phase 4: Finalization
- Update documentation across the Pyralis folder.
- Bump package version in `package.json`.
- Verify compilation and run all tests.