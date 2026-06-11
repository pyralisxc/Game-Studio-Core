# Authoring Spine

The spine is the infrastructure layer for Pyralis authoring. It holds the reflective truth used by the Authoring Window, inspectors, setup validation, route reports, and docs.

Keep this layer generic and stable. Route-specific customization belongs in route/fact/provider data. UI drawing belongs in `../Surfaces/`.

The desired flow is:

```text
feature-owned contracts and conventions
  -> facts, evidence, validation, and proof targets
      -> Authoring Window, inspectors, route reports, and docs
```

Developers should be able to add a gameplay feature by declaring its authoring contract and facts without editing the core window or duplicating setup logic.
