# Pyralis Authoring Editor Layout

This folder is the editor-only authoring system for Pyralis. The graph compiles setup understanding, projections shape tab payloads, and the window renders those projections.

## Graph Inputs

- `Dependency/`: reflected setup-reference discovery for bootstrap, session, participants, pawns, authored profiles, and module-owned capabilities.
- `Evidence/`: scene-surface evidence snapshots, native Unity surface guidance, and reflected scene facts.
- `Validation/`: scene-readiness, runtime-claim, and feature-contract validation.
- `Facts/`: fact records, reflected metadata scanning, convention providers, and contract proof projectors. Facts are vocabulary/provenance input, not setup readiness truth.
- `Vocabulary/`: labels, summaries, generic proof templates, capability vocabulary, intent vocabulary, and the aggregate vocabulary registry.

## Graph And Projections

- `Graph/`: resolved setup graph, graph node/edge vocabulary, and graph compilation.
- `Routes/`: route descriptors, route analysis, and pawn-prefab readiness analysis.
- `Intent/`: intent advice, capability descriptor synthesis, capability selection, and selected-route meaning.
- `Projections/`: tab projection packets. UI and exports should render these packets rather than recomputing tab truth.
- `Exports/`: JSON serialization of projection packets and graph audit snapshots.

## User Surfaces

- `Window/`: the main guided Authoring Window and its UI assets.
- `../Inspectors/Pyralis/`: shared inspector field guides, handoffs, and direct custom inspectors.
- `../Tools/Pyralis/`: authoring-adjacent editor utilities and validation bridge helpers.

## Feature-Owned Truth

Feature-specific authoring contracts should stay beside the owning feature when practical and be discovered reflectively. Central authoring code should aggregate those contracts through dependency, validation, vocabulary, facts, and resolved graph projections rather than maintaining parallel feature-id switch statements.

Feature-owned editor scripts that teach setup, draw field guides, or validate a feature's Inspector path should live under that feature's `Editor/Inspectors/` folder. Keep the feature editor asmdef at the feature `Editor/` root when that preserves the existing assembly boundary.

## Rule Of Thumb

If a file discovers structure, put it in `Dependency` or `Evidence`. If it witnesses readiness, put it in `Validation`. If it provides wording or provenance, put it in `Vocabulary` or `Facts`. If it compiles truth, put it in `Graph`. If it shapes a tab/export payload, put it in `Projections` or `Exports`. If it renders the user experience, put it in `Window` or an inspector owner.
