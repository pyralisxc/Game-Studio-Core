# Authoring Surfaces

Surfaces are editor UI projections of the cached resolved setup graph.

- `AuthoringWindow/` owns the guided workspace shell.
- `Inspectors/` owns field-local inspector guidance and direct custom editors.
- `Tools/` owns authoring-adjacent editor utilities, diagnostics windows, documentation helpers, and bridge helpers.

Surfaces may guide, render, focus Unity windows, and explain next steps. They should not create hidden setup truth that contracts, dependency analysis, validators, grammar, and docs cannot see, and they should not generate proof scenes or preset content for authoring validation.
