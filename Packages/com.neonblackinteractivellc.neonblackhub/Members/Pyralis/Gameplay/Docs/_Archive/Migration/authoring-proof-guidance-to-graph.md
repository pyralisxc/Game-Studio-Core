# Authoring Proof Guidance To Graph Migration

`ResolvedAuthoringContractProofGuidance` was the transitional path that mapped active feature modules to proof cards for the Overview tab.

It was removed after visible proof support moved to the resolved setup graph:

- contracts and route proof facts feed graph nodes
- capability and contract relationships feed graph proof edges
- `PyralisAuthoringSetupGraphProjection.BuildProofSupportRows` projects proof-support rows for visible tabs

The old path also carried unsupported-lane cautions and a coarse play-mode-proof state. Those concerns should be represented through graph contract nodes, graph evidence, and lane/capability facts instead of reintroducing a second proof-guidance model.

Delete this archive note once Overview, Guide, Validate, and Facts all prove parity from graph proof nodes/edges and no active migration work still references the removed helper.
