# Target Project Integration

Target projects feed PYS Authoring through evidence. They should not require package adapters.

## Allowed Inputs

- `AuthoringContractAttribute` on target project types.
- `IAuthoringValidationProvider` on target project components or assets.
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
- `RequiredFields`, `RequiredComponents`, and `RequiredInterfaces`: structure the graph can inspect.
- `Selectable`: whether Intent may select this contract.

## Validation Guidance

Use validation records to report current local readiness:

- `IssueCode`: stable machine-readable code.
- `Message`: clear human label.
- `Severity`: importance.
- `FieldPath`: field or property path when applicable.
- `TargetLabel`: Unity object or asset label when applicable.
- `ActionKind`: generic Unity action type.
- `NativeAction`: Unity-first action text.
- `SuccessCheck`: concrete condition that clears the issue.

Validation should witness local readiness. It should not invent product setup flows that contracts/reflection/dependencies cannot support.

## Vocabulary Guidance

Vocabulary providers are display-only.

They may label stable keys, categories, or target-project concepts, but they must not decide setup validity, graph ownership, proof readiness, or runtime behavior.

## Integration Checklist

1. Add `Pys.Authoring.Contracts` as an assembly reference where target runtime/editor code declares contracts or validators.
2. Do not reference `Pys.Authoring.Editor` from target runtime code.
3. Add a small contract to one low-risk target type.
4. Add validation records where local readiness is real.
5. Open `Tools/PYS/Authoring`.
6. Set Settings to the target scripts root.
7. Scan.
8. Select an Intent.
9. Confirm Overview, Guide, Map, Hygiene, and Facts project from evidence.
10. Export JSON and compare it with the visible projections.
