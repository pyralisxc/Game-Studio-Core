# Authoring Surfaces

Surfaces are editor UI projections of the authoring spine.

- `AuthoringWindow/` owns the guided workspace shell.
- `Inspectors/` owns field-local inspector guidance and direct custom editors.
- `Tools/` owns editor utilities, diagnostics windows, and bridge helpers.

Surfaces may guide, render, focus Unity windows, and explain next steps. They should not create hidden setup truth that validators, facts, and docs cannot see, and they should not generate proof scenes or preset content for authoring validation.
