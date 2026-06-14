# Core Authoring

Runtime-visible authoring metadata lives here.

These types are allowed in player-safe assemblies because runtime and feature code declare `[AuthoringContract]` directly. Editor-only reflection, graph building, validation, grammar, and UI projection stay under `Editor/Authoring`.

Keep this folder focused on contract vocabulary and resolved metadata records. Runtime service interfaces belong in `Core/ContractInterfaces`.

Intent axioms live here as contract vocabulary so the Authoring Window can render them without owning setup routes. Editor grammar may supply fallback wording, but it should not inject feature-specific setup requirements into contract-backed descriptors.
