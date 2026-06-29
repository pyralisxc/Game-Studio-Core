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
- Provides generic Unity package, authoring-window, setup-domain, component-role, asset-role, component-field, binding, readiness-state, and native action vocabulary for beginner-to-intermediate setup language.
- Supports target-project vocabulary providers through `IAuthoringVocabularyProvider`.
- Observes target-owned runtime validation through public instance methods such as `GetRuntimeValidationIssues`, then normalizes returned issue records into graph evidence.
- Renders and exports projection packets for Intent, Overview, Guide, Map, Hygiene, and Facts.
- Uses adaptive tab navigation plus compact and wrapped row helpers so projection tabs remain usable in compact or enlarged Unity Editor windows.
- Shows Observer-mode evidence and authoring-guide readiness in Settings after scan.
- Shows explicit first-use mode in Settings: waiting for scan, Observer, Unity Setup Available, Target Intent Ready, or Intent Selected.
- Supports tab-local projection controls for rendered packet filtering.
- Persists selected scripts root, selected Intent contract, active tab, tab-local controls, and stale scan state in Editor preferences.
- Projects Intent as goal candidates inferred from contract organization evidence instead of rendering the full contract inventory.
- Renders Intent as a compact composition workspace with toggle-row goal selection, selected intent summary, source/category/capability tags, editable feature toggles, and lane controls.
- Provides lower-priority built-in Unity setup guides as graph evidence for native no-code workflows when no target intents exist or the user explicitly enables them.
- Records built-in Unity setup package availability as graph metadata and emits normal issue evidence when a required Unity package is missing.
- Records built-in Unity setup readiness metadata from observed native scene components, component field assignments, and scoped assets, including observed/missing components, observed/missing fields, observed/missing assets, and readiness state.
- Supports developer-settable Intent composition metadata: toggles, lanes, compatible/supporting stable IDs, hover explanations, success descriptions, readiness hints, expected evidence, completion signals, and validation owner IDs.
- Projects Guide as a selected readiness path with ordered rows, row roles, readiness target, readiness, and blocking status.
- Projects built-in Unity setup readiness as non-blocking Guide evidence rows so current scene/asset reality is visible without inventing repair steps.
- Projects Overview from the active Guide path, including up to three exported next-action rows from ordered blocking Guide rows.
- Renders Guide rows grouped by route stage and setup domain while preserving the same row packet for export.
- Supports generic contract route metadata: prerequisite stable IDs, route stage/order, setup domain, readiness wording, native action kind, and success checks.
- Builds Guide from the selected contract plus its prerequisite contract closure instead of dumping the full contract inventory.
- Uses validation issue graph evidence, owner stable IDs, and related stable IDs to surface current scene/setup readiness inside the selected Guide path.
- Reports no selected Intent as an explicit blocked state.
- Preserves separate contract graph nodes when multiple contracts declare the same `StableId`; duplicate IDs are select-blocked in Intent and reported in Hygiene with source type/file provenance.
- Treats reflected fields as field evidence instead of Map assets.
- Renders Map rows grouped by current reality kind while preserving the same filtered Map packet for export.
- Adds packet-backed Map inspection actions for current scene/assets: select loaded scene objects in the Hierarchy and ping project assets when the row has enough evidence.
- Exports Facts as count totals plus typed fact rows with provenance, confidence, and source counts.
- Includes built-in Unity setup readiness state in Facts contract details.
- Filters Facts by kind and search text; exports mirror the rendered Facts rows.
- Exports filtered tab projection packets when tab-local controls change the rendered rows, including Intent's selected feature toggles, selected lane, and composition summary.
- Groups assembly reference dependency evidence by source assembly in Hygiene.
- Seeds the Hygiene dependency graph lens with graph node-kind and edge-kind groups.
- Splits Hygiene into tab-owned audit lenses for contract hygiene, dependency pressure, validation evidence quality, projection integrity, ownership/honesty, runtime flow, docs/claims, and dependency graph seed rows.
- Renders Hygiene rows grouped by severity inside the active lens while preserving lens/severity filtered export parity.

## Recent Validation

- `Tools/Validate-PysAuthoringPackage.ps1` passed.
- Unity refresh/import of `Packages/com.pys.authoring` compiled cleanly.
- PYS-specific `dotnet build` checks for `Pys.Authoring.Contracts.csproj`, `Pys.Authoring.Editor.csproj`, and `Pys.Authoring.Editor.Tests.csproj` passed with `0 Warning(s), 0 Error(s)`.
- Unity Test Runner for `Pys.Authoring.Editor.Tests` should be rerun after each package behavior change.

## Known Limits

- Intent inference is generic. It uses explicit goal, success, readiness, and evidence metadata, route terminal patterns, and fallback selectable contracts; richer grouping depends on target-project contract quality.
- Unity setup guides are generic assistance. They do not replace or outrank target-project gameplay contracts. Package availability evidence can block a selected built-in setup guide, but scene/component/asset readiness evidence is projected as observed reality rather than generated repair instructions.
- `ProofTarget` remains supported as fallback wording. New integrations should prefer success/readiness/evidence metadata because PYS infers readiness from observed evidence.
- Fresh projects without contracts primarily operate in Observer mode: Settings readiness, Facts, Hygiene, and Map are useful before Intent/Guide become authoring guidance.
- Guide path ordering is generic and metadata-driven. Richer target-project guidance depends on good contracts and validation records.
- Reflective validation observation reads public parameterless instance methods that return enumerable issue objects. Target projects own the validation model; PYS reads matching property names when present.
- Hygiene lenses report only graph-backed pressure. Empty lenses are allowed when the current scan does not contain typed evidence for that pressure.
- The window is split into tab-owned partial files and uses stock Unity IMGUI. Do not add a custom color scheme; keep emphasis to standard Unity labels, controls, disabled scopes, help boxes, compact rows, and wrapped text.

## Next Useful Work

- Use `Docs/PHASED_DEVELOPMENT_PLAN.md` as the current phased roadmap for UI/productization and validation order.
- Add target-project contracts, target-owned validation records, and optional vocabulary providers to prove integration from an external codebase.
- Export each tab after selecting an Intent and compare JSON with the visible projection.
- Add the visual dependency graph UI from the existing Hygiene dependency graph seed rows.
