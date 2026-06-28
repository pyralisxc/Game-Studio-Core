# Target Project Integration

Target projects feed PYS Authoring through evidence. They should not require package adapters.

## Allowed Inputs

- `AuthoringContractAttribute` on target project types.
- Target-owned runtime validation methods such as public instance `GetRuntimeValidationIssues` methods on observed components.
- `IAuthoringVocabularyProvider` for display-only labels.
- Reflected Unity structure such as serialized fields, required components, implemented interfaces, scene objects, prefabs, and assets.
- Source dependency evidence such as asmdef references and namespace usings.

## Contract Guidance

Use contracts to describe stable authoring meaning:

- `StableId`: durable target-project ID.
- `DisplayName`: user-facing label.
- `Category`: broad grouping.
- `CapabilityPath`: slash-delimited capability path.
- `Surface`: whether this is a goal, setup, profile, component, service, presenter, adapter, or vocabulary-only item.
- `SetupSteps`: generic steps Guide may project for selected Intent paths.
- `SuccessChecks`: checks Guide may show as proof target verification.
- `PrerequisiteStableIds`: stable IDs that must be resolved before this contract's proof path.
- `RouteStage` and `RouteOrder`: generic ordering hints for Guide rows inside a selected dependency closure.
- `SetupDomain`: broad setup domain label for Guide grouping and export rows.
- `ProofTarget`: contract-owned proof label when the proof target differs from the display name.
- `NativeActionKind`: generic Unity action kind for setup rows.
- `RequiredFields`, `RequiredComponents`, and `RequiredInterfaces`: structure the graph can inspect.
- `Selectable`: whether Intent may select this contract.

Intent is not a full contract inventory. PYS treats explicit goal/proof evidence as the primary Intent candidate source, then falls back to terminal route contracts, then to selectable contracts only when no stronger organization pattern exists. Use `Surface = Goal`, `ProofTarget`, and `SuccessChecks` for user-facing authoring goals. Use `PrerequisiteStableIds`, `RouteStage`, `RouteOrder`, and `SetupDomain` for the supporting setup/profile/component path that Guide should expand after an Intent is selected.

## Validation Guidance

Target projects own local validation. PYS observes validation evidence; it does not require target projects to implement a PYS adapter interface.

By default, PYS looks for public parameterless instance methods named `GetRuntimeValidationIssues` on observed components. The method should return an enumerable of issue objects. PYS reflects these optional public properties when present:

- `IssueCode`: stable machine-readable code.
- `Message`: clear human label.
- `Severity`: importance.
- `FieldPath`: field or property path when applicable.
- `TargetLabel`: Unity object or asset label when applicable.
- `ActionKind`: generic Unity action type.
- `NativeAction`: Unity-first action text.
- `SuccessCheck`: concrete condition that clears the issue.
- `OwnerStableId` or `RelatedStableIds`: optional stable IDs that let Guide attach current scene/setup readiness to the selected route dependency closure.

Validation should witness local readiness. It should not invent product setup flows that contracts/reflection/dependencies cannot support.

## Vocabulary Guidance

Vocabulary providers are display-only.

They may label stable keys, categories, or target-project concepts, but they must not decide setup validity, graph ownership, proof readiness, or runtime behavior.

## Integration Checklist

1. Add `Pys.Authoring.Contracts` as an assembly reference where target runtime/editor code declares contracts.
2. Do not reference `Pys.Authoring.Editor` from target runtime code.
3. Add a small contract to one low-risk target type.
4. Add target-owned runtime validation records where local readiness is real.
5. Open `Tools/PYS/Authoring`.
6. Set Settings to the target scripts root.
7. Scan.
8. Select an Intent.
9. Confirm Overview, Guide, Map, Hygiene, and Facts project from evidence.
10. Export JSON and compare it with the visible projections.
