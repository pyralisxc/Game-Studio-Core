# Pyralis PYS Authoring Start Here

Pyralis now uses the standalone `com.pys.authoring` package for the Authoring Window.

Open it from Unity:

```text
Tools/PYS/Authoring
```

Set the observed scripts folder to:

```text
Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay
```

Then scan. PYS builds the graph and tab projections from evidence that Pyralis scripts declare through:

- `Pys.Authoring.Contracts.AuthoringContractAttribute`
- `Pys.Authoring.Contracts.IAuthoringValidationProvider`
- reflected fields, components, interfaces, and Unity assets
- optional vocabulary providers supplied by the PYS package

Pyralis inspectors remain tactical field editors. Their handoff button opens PYS Authoring; inspectors do not own route setup, proof ordering, graph projection, or export logic.

When authoring output feels wrong, fix the evidence owner:

- missing feature meaning: update the owning Pyralis contract metadata
- missing local readiness: update the owning validation provider
- missing field/component evidence: update the owning script structure
- unclear projection behavior: fix `com.pys.authoring`, not Pyralis gameplay code
