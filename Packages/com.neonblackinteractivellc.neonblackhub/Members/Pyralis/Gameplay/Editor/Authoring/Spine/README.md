# Authoring Spine

The spine is the infrastructure layer for Pyralis authoring. It resolves setup references, validates readiness, and compiles the graph used by the Authoring Window, inspectors, setup validation, graph projections, and docs.

Keep this layer generic and stable. Human-facing fallback wording belongs in `../Grammar/`. UI drawing belongs in `../Surfaces/`.

The desired flow is:

```text
feature-owned contracts and reflected setup references
  -> validators, evidence, and grammar defaults
      -> resolved setup graph
          -> Authoring Window, inspectors, and docs
```

Developers should be able to add a gameplay feature by declaring its authoring contract and letting reflection/validation feed the graph without editing the core window or duplicating setup logic.
