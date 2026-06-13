# Authoring Validation Model To Graph

`PyralisAuthoringValidationModel` and `PyralisAuthoringIssueAdapter` were removed after the visible Validate tab and typed validation consumers moved onto `PyralisAuthoringSetupGraphProjection`.

Keep current validation work on these paths:

- `PyralisSetupFlowValidator` and `PyralisSceneReadinessValidator` provide source evidence.
- `PyralisAuthoringSetupGraphBuilder` attaches that evidence to graph nodes.
- `PyralisAuthoringSetupGraphProjection.BuildValidationRows(...)` renders Validate rows.
- `PyralisAuthoringSetupGraphProjection.BuildTypedValidationIssues(...)` projects typed `PyralisAuthoringIssue` data.

Pawn prefab readiness text that was still useful moved to `PyralisPawnPrefabReadinessAnalysis` so route analysis can keep reporting native 2D pawn prefab issues without retaining a separate validation-card model.
