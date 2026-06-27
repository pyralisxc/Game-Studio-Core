# Core Authoring Contracts

Runtime-visible authoring metadata lives here.

These types are allowed in player-safe assemblies because runtime and feature code declare `[AuthoringContract]` directly. Editor-only reflection, graph building, validation, grammar, and UI projection stay under `Editor/Authoring`.

Keep this folder focused on player-safe contract flags, contract attributes, and resolved metadata records. Runtime service interfaces belong in `Core/Contracts/Runtime`.

Intent axioms, capabilities, semantic capability paths, role tags, and selectable-intent flags live here because gameplay contracts need to declare feature meaning close to the feature code. Editor-facing names, groups, tooltips, hygiene advice, documentation links, and generic wording live under Editor Authoring vocabulary. Do not add Authoring Window UI vocabulary or setup-card copy back into this runtime-visible folder.

Use contract fields only for meaning reflection cannot prove. Do not duplicate implemented interfaces, `[RequireComponent]` dependencies, serialized fields, Unity menu paths, profile types, or dependency order when the Editor authoring spine can reflect them.
