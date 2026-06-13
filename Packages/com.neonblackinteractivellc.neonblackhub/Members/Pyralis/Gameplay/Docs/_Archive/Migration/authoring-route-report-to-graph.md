# Authoring Route Report To Graph

`PyralisAuthoringRouteReport` was removed after visible setup diagnosis moved onto graph-backed projections.

Keep current route guidance on these paths:

- `PyralisSetupRouteAnalysis` resolves the selected setup route and reflected dependency context.
- `PyralisSetupFlowValidator` reports concrete setup-chain readiness.
- `PyralisSceneReadinessValidator` reports scene and prefab evidence.
- `PyralisAuthoringSetupGraphBuilder` synthesizes route, contract, proof, setup-flow, and scene-readiness evidence into graph nodes.
- `PyralisAuthoringSetupGraphProjection` projects current step, Overview, Map, Validate, Guide selected context, typed issues, and proof-support rows.

The only reusable pawn-prefab checks from the old route report live in `PyralisPawnPrefabReadinessAnalysis`. Do not reintroduce a tab-local route report; add setup meaning to contracts, route analysis, validators, or graph projections so every tab reads the same resolved setup graph.
