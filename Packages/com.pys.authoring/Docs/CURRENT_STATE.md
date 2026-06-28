# Current State

`com.pys.authoring` is an embedded Unity package linked from `Packages/manifest.json`.

## Package State

- `Pys.Authoring.Contracts` is runtime-safe and has no Editor dependencies.
- `Pys.Authoring.Editor` owns scanning, graph projection, tab projections, exports, vocabulary, Hygiene, and the window.
- `Pys.Authoring.Editor.Tests` covers current projection and export behavior.
- The package is generic. It does not contain target-project product knowledge.

## Implemented Capabilities

- Scans a selected Unity scripts folder for scripts, types, contracts, validators, asmdefs, and namespace using relationships.
- Scans current scene objects plus scoped prefab, ScriptableObject, and scene assets.
- Builds a generic graph from reflection, contracts, dependencies, validators, and Unity asset evidence.
- Provides contained vocabulary packs for Unity objects, graph edges, actions, projections, and Hygiene lenses.
- Supports target-project vocabulary providers through `IAuthoringVocabularyProvider`.
- Observes target-owned runtime validation through public instance methods such as `GetRuntimeValidationIssues`, then normalizes returned issue records into graph evidence.
- Renders and exports projection packets for Intent, Overview, Guide, Map, Hygiene, and Facts.
- Shows Observer-mode evidence and authoring-guide readiness in Settings after scan.
- Supports tab-local projection controls for rendered packet filtering.
- Persists selected Intent contract in Editor preferences.
- Projects Intent as goal candidates inferred from contract organization evidence instead of rendering the full contract inventory.
- Projects Guide as a selected proof path with ordered rows, row roles, proof target, readiness, and proof-blocking status.
- Projects Overview from the active Guide path.
- Supports generic contract route metadata: prerequisite stable IDs, route stage/order, setup domain, proof target, native action kind, and success checks.
- Builds Guide from the selected contract plus its prerequisite contract closure instead of dumping the full contract inventory.
- Uses validation issue graph evidence, owner stable IDs, and related stable IDs to surface current scene/setup readiness inside the selected Guide path.
- Reports no selected Intent as an explicit blocked state instead of proof-ready.
- Preserves separate contract graph nodes when multiple contracts declare the same `StableId`; duplicate IDs are select-blocked in Intent and reported in Hygiene with source type/file provenance.
- Treats reflected fields as field evidence instead of Map assets.
- Exports Facts as count totals plus typed fact rows with provenance, confidence, and source counts.
- Exports filtered tab projection packets when tab-local controls change the rendered rows.
- Groups assembly reference dependency evidence by source assembly in Hygiene.

## Recent Validation

- `Tools/Validate-PysAuthoringPackage.ps1` passed.
- Unity refresh/import of `Packages/com.pys.authoring` compiled cleanly.
- PYS-specific `dotnet build` checks for `Pys.Authoring.Contracts.csproj`, `Pys.Authoring.Editor.csproj`, and `Pys.Authoring.Editor.Tests.csproj` passed with `0 Warning(s), 0 Error(s)`.
- Unity Test Runner for `Pys.Authoring.Editor.Tests` should be rerun after each package behavior change.

## Known Limits

- Intent inference is generic. It uses explicit goal/proof metadata, route terminal patterns, and fallback selectable contracts; richer grouping depends on target-project contract quality.
- Fresh projects without contracts primarily operate in Observer mode: Settings readiness, Facts, Hygiene, and Map are useful before Intent/Guide become authoring guidance.
- Guide path ordering is generic and metadata-driven. Richer target-project guidance depends on good contracts and validation records.
- Reflective validation observation reads public parameterless instance methods that return enumerable issue objects. Target projects own the validation model; PYS reads matching property names when present.
- Hygiene lenses are intentionally early. They currently cover metadata, dependency pressure, projection integrity, and related structural pressure.
- The window uses IMGUI and is functional-first. UI polish can improve later without changing projection ownership.

## Next Useful Work

- Add target-project contracts, target-owned validation records, and optional vocabulary providers to prove integration from an external codebase.
- Export each tab after selecting an Intent and compare JSON with the visible projection.
- Strengthen Hygiene only when the graph already contains typed evidence for the new pressure.
