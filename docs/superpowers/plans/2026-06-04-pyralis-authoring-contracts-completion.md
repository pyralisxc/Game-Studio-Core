# Pyralis Authoring Contracts Completion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the Pyralis authoring contracts refactor so feature-owned reflective contracts become the source of truth for authoring facts, validation, setup guidance, proof prompts, and durable docs.

**Architecture:** Keep explicit contracts beside each feature, discover them reflectively through `PyralisAuthoringContractRegistry`, and generate central authoring surfaces from those contracts. Central files should aggregate, adapt, and display contracts, not remember feature-specific module rules.

**Tech Stack:** Unity 6000.4, C#, asmdefs, ScriptableObject authoring, Unity Editor inspectors/windows, Unity EditMode/PlayMode tests, `.\Tools\Validation\Run-PreSceneValidation.ps1`.

---

## Current Baseline

Phase 0 is complete and validated.

- `PyralisAuthoringContract`, `IAuthoringContractProvider`, and `PyralisAuthoringContractRegistry` exist.
- Top Down Hop and Actor Interaction have feature-owned reflective providers.
- Contract facts appear in the Authoring Window.
- `FeatureModuleDefinitionEditor` can show matched contract metadata.
- The full pre-scene validation gate passed with 344/344 EditMode and 166/166 PlayMode.

## Completion Definition

This refactor is complete when:

- Every reusable `FeatureModuleDefinition` module with known profile/runtime/lane requirements has a feature-owned contract provider.
- Feature-specific profile, runtime interface, lane, action, assignment, and proof metadata no longer lives in central `FeatureModuleDefinition` switches unless it is truly generic.
- The Authoring Window generates setup/proof guidance from contracts rather than hand-authored parallel lists.
- Contract coverage is tested at the registry, fact, validation, and authoring-surface levels.
- Active docs describe the reflective authoring model as present truth.
- `.\Tools\Validation\Run-PreSceneValidation.ps1` passes with clean residue.

---

## Phase 1: Contract Coverage Expansion

Status: Complete. Validated with `.\Tools\Validation\Run-PreSceneValidation.ps1` on 2026-06-03: EditMode 360/360 passed, PlayMode 166/166 passed, residue scan clean.

**Intent:** Give every obvious reusable module a feature-owned contract.

**Primary write areas:**

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/**/Editor/*AuthoringContractProvider.cs`
- Feature editor `.asmdef` files beside providers
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`

**Candidate modules:**

- `actor.traversal.3d`
- `actor.pickups.2d`
- `actor.pickups.3d`
- `actor.feedback`
- `actor.combat.reaction`
- `actor.status`
- `enemy.reaction`
- `enemy.ambient`
- scoring or HUD feature modules that are represented as feature modules

**Tasks:**

- [x] Inventory all module ids currently validated by `FeatureModuleDefinition.GetValidationIssues()` and `AppendRuntimeContractIssues()`.
- [x] For each module id, identify owning feature folder, profile type, runtime prefab interface requirements, supported lanes, unsupported lanes, consumed action roles, native setup steps, assignment fields, customization moments, and first proof target.
- [x] Add one `IAuthoringContractProvider` per owning feature editor assembly.
- [x] Add missing feature editor asmdefs and `.meta` files where a feature has runtime code but no editor assembly.
- [x] Update `AuthoringContractsContractTests` so each contract is discoverable by module id and contributes a `FeatureContract` fact.

**Acceptance criteria:**

- `PyralisAuthoringContractRegistry.All` contains contracts for all inventoried feature modules.
- No provider is registered by a central hardcoded list.
- Unity imports all new provider assemblies.

---

## Phase 2: Contract-Driven Validation Cleanup

Status: Complete. Validated with `.\Tools\Validation\Run-PreSceneValidation.ps1` on 2026-06-03: EditMode 361/361 passed, PlayMode 166/166 passed, residue scan clean.

**Intent:** Delete duplicated feature-specific validation from central definitions.

**Primary write areas:**

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/FeatureModuleDefinition.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/FeatureModuleDefinitionEditor.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

**Tasks:**

- [x] Move profile mismatch checks into `PyralisFeatureModuleContractValidator`.
- [x] Move runtime interface checks into contract-driven validation.
- [x] Move lane support/unsupported-lane messaging into contract-driven validation.
- [x] Keep only generic validation in `FeatureModuleDefinition`: missing runtime prefab, missing `IFeatureModuleRuntime`, network metadata, empty category, generic runtime validation provider checks.
- [x] Keep actor-root compatibility checks in `FeatureModuleDefinition` only when they depend on a specific authored actor root rather than the feature definition itself.
- [x] Update tests so feature-specific assertions call the contract validator or inspector validation path, not `FeatureModuleDefinition.GetValidationIssues()`.

**Acceptance criteria:**

- `FeatureModuleDefinition` no longer needs one switch case per migrated module for definition-level rules.
- Contract-driven validation reports the same or clearer actionable messages.
- Tests prove profile, lane, and runtime-interface mismatches for representative modules.

---

## Phase 3: Authoring Window Generation

Status: Complete. Validated with `.\Tools\Validation\Run-PreSceneValidation.ps1` on 2026-06-04: EditMode 362/362 passed, PlayMode 166/166 passed, residue scan clean.

**Intent:** Turn contracts into real authoring guidance, not just facts.

**Primary write areas:**

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringWindow.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactRegistry.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringRouteReport.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringSourceContractTests.cs`

**Tasks:**

- [x] Add a contract-backed feature module section to the Authoring Window that groups contracts by category.
- [x] Show required profile, runtime interfaces, supported lanes, unsupported lanes, consumed actions, native setup actions, and first proof target.
- [x] Link contract setup actions to native Unity surfaces: Project window asset creation, Inspector assignment, Hierarchy/Add Component, object picker, and Play Mode proof.
- [x] Replace duplicated hand-authored feature module guidance rows with contract-derived rows where equivalent.
- [x] Add tests that assert the Authoring Window surfaces `FeatureContract` groups and native setup actions.

**Acceptance criteria:**

- Authors can inspect the contract-backed setup recipe for each covered module without reading code.
- The Facts tab and practical setup views agree on the same contract data.
- No contract-derived guidance is duplicated as a second manual truth unless the manual text adds distinct product context.

---

## Phase 4: Proof Workflow Integration

Status: Complete. Validated with `.\Tools\Validation\Run-PreSceneValidation.ps1` on 2026-06-04: EditMode 367/367 passed, PlayMode 166/166 passed, residue scan clean.

**Intent:** Use contracts to decide what proof the author should attempt first.

**Primary write areas:**

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringRouteProof.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringOverviewSnapshot.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringRouteReport.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/*`

**Tasks:**

- [x] Map `PyralisAuthoringContract.FirstProofTargetId` to route proof cards.
- [x] When a setup includes required feature modules, surface their proof targets as proof enhancers or blockers according to route readiness.
- [x] Add validation metadata that distinguishes “proof target exists” from “proof passed in Play Mode.”
- [x] Show unsupported lanes as explicit proof cautions.
- [x] Add tests for Top Down Hop, Interaction, and at least one non-pawn or 3D feature proof path.

**Acceptance criteria:**

- The Authoring Window can explain what to prove first for a selected module.
- Proof guidance is generated from contracts and route state.
- Unsupported lane messaging appears before Play Mode confusion.

---

## Phase 5: Documentation And Source Truth Cleanup

Status: Complete. Active docs now describe feature-owned reflective contracts, required provider fields, first-proof mapping, and the new feature module checklist.

**Intent:** Make active docs describe the new truth directly.

**Primary write areas:**

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_MODEL.md`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/CANONICAL_SETUP.md`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/ARCHITECTURE_BLUEPRINT.md`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_SCOPE.md`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`

**Tasks:**

- [x] Document the feature-owned contract provider pattern.
- [x] Document when to add a provider and which fields are required.
- [x] Remove stale manual-registry and legacy-compatibility language.
- [x] Add a short “new feature module checklist” that includes provider, asmdef references, `.meta`, tests, docs, and validation.
- [x] Keep historical context only in archive/audit docs if it still helps future migration review.

**Acceptance criteria:**

- A future agent can add a feature module contract without reverse-engineering the refactor.
- Active docs contain present-tense source truth.
- No docs tell agents to edit generated `.csproj` files.

---

## Phase 6: Folderbase And Dead-Code Shrink Pass

Status: Complete. Validated with `.\Tools\Validation\Run-PreSceneValidation.ps1` on 2026-06-04: EditMode 367/367 passed, PlayMode 166/166 passed, residue scan clean.

**Intent:** Harvest the shrinkage the refactor enables.

**Primary write areas:**

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/**`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/**`
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/**`

**Tasks:**

- [x] Search for duplicated profile/runtime/lane/action/proof strings now owned by contracts.
- [x] Delete or consolidate duplicate hand-authored guidance.
- [x] Remove empty directories and stale `.meta` files created by intermediate refactors.
- [x] Keep generated `.csproj` and `.sln` files out of source edits.
- [x] Run residue scan through the project gate.

**Acceptance criteria:**

- Central authoring files shrink or become simpler aggregators.
- Empty/pass-through folders are removed.
- Tests continue to protect the reflective model.

---

## Phase 7: Final Completion Gate

Status: Complete. Validated on 2026-06-04 with `dotnet build "Game Studio Core.slnx" --no-restore` and `.\Tools\Validation\Run-PreSceneValidation.ps1`: EditMode 367/367 passed, PlayMode 166/166 passed, residue scan clean.

**Intent:** Prove this is complete as a durable authoring-system refactor.

**Tasks:**

- [x] Run `dotnet build "Game Studio Core.slnx" --no-restore` after Unity has regenerated project files.
- [x] Close the GUI Unity Editor.
- [x] Run `.\Tools\Validation\Run-PreSceneValidation.ps1`.
- [x] Confirm EditMode and PlayMode XML summaries under `Logs\Codex`.
- [x] Confirm residue scan is clean.
- [x] Write a phase/project check-out audit covering product state, code architecture state, folderbase state, docs state, validation evidence, residual risks, and optional enhancements.

**Acceptance criteria:**

- `.\Tools\Validation\Run-PreSceneValidation.ps1` passes.
- Contract coverage is complete for current known reusable modules.
- Remaining work, if any, is optional enhancement rather than required refactor work.

**Check-out audit:**

- Product state: Pyralis authoring now treats feature-owned reflective contracts as the source of truth for reusable module setup, required profiles, runtime interfaces, supported lanes, unsupported lanes, native setup actions, customization moments, and first proof targets.
- Code and architecture state: Contract coverage exists for the current known reusable modules, central validation keeps only generic definition concerns plus actor-root compatibility, proof guidance is contract-backed, and repeated runtime/profile setup prose is consolidated through `PyralisAuthoringContractGuideText`.
- Maintenance/folderbase state: No empty gameplay folders or missing `.cs.meta` files were found, generated `.csproj` and `.sln` files were kept out of source edits, and source tests now guard against reintroducing duplicated hard-coded authoring guidance in the cleaned editor paths.
- Docs and standards state: Active setup, architecture, scope, roadmap, and phase-plan docs describe the reflective authoring model as current truth.
- Verification evidence: `dotnet build "Game Studio Core.slnx" --no-restore` passed with 0 warnings and 0 errors. `.\Tools\Validation\Run-PreSceneValidation.ps1` passed with EditMode 367/367 and PlayMode 166/166. XML summaries were written to `Logs\Codex\pre-scene-editmode-20260604-093246-results.xml` and `Logs\Codex\pre-scene-playmode-20260604-093351-results.xml`.
- Known residual risks: The full gate still reports existing third-party Unity Test Framework and VContainer warnings during its final build stage; no project errors or validation residue were reported.
- Optional enhancements: Future work can add richer Computer Use authoring-path validation, more feature contracts as new modules appear, and deeper UI polish for the Authoring Window. These are enhancements, not required completion work for this refactor.
