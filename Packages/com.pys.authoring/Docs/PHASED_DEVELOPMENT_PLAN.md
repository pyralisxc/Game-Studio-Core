# PYS Authoring Phased Development Plan

PYS Authoring is currently an internal alpha package with a strong projection architecture and a functional IMGUI window. The next development target is not more graph abstraction. The next target is making the existing projections understandable, actionable, and trustworthy for a Unity developer in the first minute of use.

Core rule:

```text
compiled evidence graph
-> tab-owned projection packets
-> tab-owned rendering and controls
-> exports that mirror the rendered packet
```

PYS remains a generic Unity observer package. It must not reference target-project assemblies or encode target-project product meaning.

## Current Audit

Strengths:

- The package boundary is correct: `Pys.Authoring.Contracts` is runtime-safe, and `Pys.Authoring.Editor` owns scanning, graph building, projections, exports, vocabulary, Hygiene, and the window.
- The projection model is coherent: Settings, Intent, Overview, Guide, Map, Hygiene, and Facts each have an explicit ownership contract.
- Exports mirror rendered projection packets, including filtered rows.
- Duplicate contract `StableId` handling is graph-backed and visible through Intent/Hygiene.
- Target validation is observed reflectively instead of requiring target projects to implement a PYS-specific validation adapter.
- Built-in Unity setup guides compile into normal graph evidence and stay lower priority than target contracts.
- Hygiene now has real lenses instead of a single warning dump.
- Facts is a useful evidence ledger with provenance and confidence labels.

Weaknesses:

- The window is now split into tab-owned partial files, but the projection UI still needs continued hands-on Unity review against real compact and expanded Editor layouts.
- Intent is a compact composition workspace, but richer interactive grouping will depend on more target-project contract metadata.
- Overview communicates a small next-action set from Guide, but its usefulness depends on route metadata quality.
- Guide rows are grouped and scannable, but binding-level readiness and richer native action hints still need more evidence.
- Map has packet-backed select/ping inspection actions and compact filters, but still needs richer issue drilldown for daily scene auditing.
- Hygiene has correct lenses, adaptive lens navigation, severity grouping, and compact rows, but still needs a real Dependency Graph view.
- Facts has kind/search filtering and compact evidence rows, but still needs real-world stress review on large projects.
- Built-in Unity setup guides record package availability, missing-package issues, native scene component evidence, component field assignment evidence, scoped asset evidence, and Unity-native vocabulary labels for setup fields/bindings/readiness. They do not yet inspect window/action availability or complex binding graphs such as Timeline track bindings.
- Full Unity validation is currently blocked by unrelated target-project compile errors, so PYS package validation is clean but project-wide Unity compile is not clean.

## Phase 1: First-Run And Settings UX

Goal: make the first scan state obvious to a fresh Unity developer.

Scope:

- Turn Settings into a clear first-run/control surface.
- Show scan root, stale state, scan command, export folder, and evidence readiness.
- Show mode explicitly: `Observer`, `Unity Setup Available`, or `Target Intent Ready`.
- Show counts as grouped evidence: contracts, validation methods, scene/prefab/assets, graph issues.
- Keep Settings free of setup instructions and audit findings.

Files:

- `Editor/Window/PysAuthoringWindow.cs`
- likely new `Editor/Window/PysAuthoringWindow.Settings.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/PROJECTION_CONTRACTS.md`
- `Docs/CURRENT_STATE.md`

Acceptance:

- A fresh project with no contracts communicates that Facts, Hygiene, and Map are useful immediately.
- A project with no target Intent candidates communicates that Unity Setup Guides are available but lower priority.
- Scan stale state persists and is visually clear.
- No target-project product meaning appears in Settings.

Validation:

- PYS package validation script.
- PYS contracts/editor/tests builds.
- Manual Unity window scan of `Packages/com.pys.authoring`.

## Phase 2: Window Decomposition

Goal: reduce UI maintenance risk before adding more UI behavior.

Scope:

- Split the window by tab ownership using partial classes or small tab renderer classes.
- Keep packet construction in projection builders, not the window.
- Keep rendered-packet filtering close to each tab renderer.
- Centralize small UI helpers for counts, status rows, metadata lines, action labels, and export buttons.

Files:

- `Editor/Window/PysAuthoringWindow.cs`
- create `Editor/Window/PysAuthoringWindow.Settings.cs`
- create `Editor/Window/PysAuthoringWindow.Intent.cs`
- create `Editor/Window/PysAuthoringWindow.Overview.cs`
- create `Editor/Window/PysAuthoringWindow.Guide.cs`
- create `Editor/Window/PysAuthoringWindow.Map.cs`
- create `Editor/Window/PysAuthoringWindow.Hygiene.cs`
- create `Editor/Window/PysAuthoringWindow.Facts.cs`
- create `Editor/Window/PysAuthoringWindow.Shared.cs`

Acceptance:

- No projection behavior changes.
- Exports remain byte-for-byte equivalent for the same rendered packets where ordering is unchanged.
- Each tab file owns only its tab rendering and tab-local controls.
- `PysAuthoringWindow.cs` owns lifecycle, tab routing, scan orchestration, and shared state only.

Validation:

- Existing projection/export tests pass.
- PYS editor build passes with zero warnings.

## Phase 3: Intent Compact Workspace

Goal: make Intent feel like selected-goal steering, not a contract inventory.

Scope:

- Replace large metadata presentation with a compact selected-intent workspace.
- Render candidate selection as a concise dropdown/list with source and disabled status.
- Render `IntentToggles` as checkboxes or toggle rows.
- Render `IntentLanes` as popup or segmented choices when values are present.
- Render hover explanations as tooltips/help text attached to controls.
- Keep source labels clear: `Target Contract` vs `Built-In Unity Setup`.
- Keep target contracts ordered before built-in Unity setup rows.

Files:

- `Editor/Window/PysAuthoringWindow.Intent.cs`
- `Editor/Projections/AuthoringProjectionModels.cs`
- `Editor/Projections/AuthoringProjectionBuilder.cs`
- `Editor/Exports/ProjectionJsonExporter.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/PROJECTION_CONTRACTS.md`

Acceptance:

- Intent does not display a full contract inventory.
- Duplicate `StableId` rows are still disabled/select-blocked.
- Built-in Unity setup guides show only when no target intents exist or the user enables them.
- Exported Intent packet matches the rendered Intent state.

Validation:

- Tests cover toggle/lane rendering data and export parity.
- Manual scan confirms no selected Intent displays `No intent selected`, not a ready state.

## Phase 4: Overview Next Actions

Goal: make Overview the next-small-step surface.

Scope:

- Promote Overview from one `NextAction` string to the next 1-3 projected actions.
- Derive rows from the ordered blocking Guide rows when an Intent is selected.
- Fall back to Map/current scene/setup issue summaries only when no Intent path exists.
- Keep Hygiene details out of Overview.

Files:

- `Editor/Projections/AuthoringProjectionModels.cs`
- `Editor/Projections/AuthoringProjectionBuilder.cs`
- `Editor/Exports/ProjectionJsonExporter.cs`
- `Editor/Window/PysAuthoringWindow.Overview.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/PROJECTION_CONTRACTS.md`

Acceptance:

- Overview shows no more than three next actions.
- The first action matches the first ordered blocking Guide row.
- No selected Intent does not report readiness.
- Exported Overview packet matches rendered rows.

Validation:

- Tests cover selected Intent, no Intent, ready path, and Map fallback.
- Manual scan confirms Overview remains small.

## Phase 5: Guide Route Presentation

Goal: make Guide a real Unity authoring path.

Scope:

- Group Guide rows by `RouteStage` and `SetupDomain`.
- Show blocking rows first within stage, with completed/readiness rows visually secondary.
- Render action kind labels as badges or compact prefixes.
- Keep `NativeAction`, `SuccessCheck`, and source owner visible without overwhelming the row.
- Add tab controls: blocking only, required plus optional, show validation inline, collapse completed.

Files:

- `Editor/Window/PysAuthoringWindow.Guide.cs`
- `Editor/Projections/AuthoringProjectionModels.cs`
- `Editor/Projections/AuthoringProjectionBuilder.cs`
- `Editor/Exports/ProjectionJsonExporter.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/PROJECTION_CONTRACTS.md`

Acceptance:

- Guide starts from selected contract plus dependency closure only.
- Rows are ordered by route metadata and grouped for scanning.
- Validation issue rows stay attached to the selected route.
- Built-in Unity setup guides use the same row model as target contracts.
- Exported Guide packet matches active filters.

Validation:

- Tests cover route stage grouping data and filtered export parity.
- Manual scan confirms the path can be followed without reading raw graph terms.

## Phase 6: Map Inspection Surface

Goal: make Map show current scene/setup reality clearly.

Scope:

- Improve Map rows for scene objects, prefabs, and assets.
- Add grouping by kind and source path.
- Add issue count emphasis.
- Add native Unity actions where safe: select hierarchy object, ping asset, open inspector/focus object when object references are available.
- Keep desired setup and future route instructions out of Map.

Files:

- `Editor/Scanning/UnityObjectScanner.cs`
- `Editor/Scanning/UnityAssetScanner.cs`
- `Editor/Projections/AuthoringProjectionModels.cs`
- `Editor/Projections/AuthoringProjectionBuilder.cs`
- `Editor/Window/PysAuthoringWindow.Map.cs`
- `Editor/Exports/ProjectionJsonExporter.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/PROJECTION_CONTRACTS.md`

Acceptance:

- Reflected fields are never shown as assets.
- Scene/prefab/asset reality is separated from desired setup.
- Issue counts are visible and exportable.
- Native object actions work only when the rendered packet exposes safe scene-object or asset navigation fields.
- Native scene/component/field/asset readiness evidence is shown as observed reality, not generated setup repair.

Validation:

- Tests cover row kind grouping and export parity.
- Manual Unity check validates ping/select actions.

## Phase 7: Hygiene Productization

Goal: make Hygiene useful as an audit workbench instead of a warning wall.

Scope:

- Give each Hygiene lens a focused header, count summary, severity filter, and grouped rows.
- Make Contract Hygiene, Dependency Pressure, Validation Evidence, Projection Integrity, Ownership & Honesty, Runtime Flow, Docs & Claims, and Dependency Graph visibly distinct.
- Promote high-severity and duplicate `StableId` rows.
- Add source path navigation where safe.
- Keep Hygiene from giving setup walkthroughs.

Files:

- `Editor/Hygiene/HygieneProjection.cs`
- `Editor/Window/PysAuthoringWindow.Hygiene.cs`
- `Editor/Exports/HygieneJsonExporter.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/HYGIENE_LENSES.md`

Acceptance:

- Duplicate `StableId` row lists all source types/files.
- Assembly references stay grouped by source assembly.
- Validation owner and expected-evidence honesty rows remain graph-backed.
- Exported Hygiene packet matches selected lens and severity filter.

Validation:

- Tests cover each lens and rendered export parity.
- Manual scan confirms Hygiene is readable under noisy graphs.

## Phase 8: Visual Dependency Graph Lens

Goal: turn existing graph edge seed rows into an actual dependency graph inspection view.

Scope:

- Start with folder/module/edge-kind graph view, not class-level chaos.
- Render node groups for assemblies, namespaces, contracts, validators, scene/prefab/assets, and issue groups.
- Allow drilldown by edge kind.
- Keep this inside Hygiene as a lens unless it becomes large enough to justify its own tab.

Files:

- `Editor/Hygiene/HygieneProjection.cs`
- `Editor/Window/PysAuthoringWindow.Hygiene.cs`
- possible `Editor/Window/HygieneDependencyGraphView.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/HYGIENE_LENSES.md`

Acceptance:

- Visual graph uses compiled graph evidence only.
- Dependency graph seed rows include node-kind and edge-kind groups.
- It does not invent desired setup or proof/readiness state.
- Textual Hygiene export remains the source packet for export parity.

Validation:

- Tests cover grouped edge packets.
- Manual UI check confirms graph is readable at package scale.

## Phase 9: Built-In Unity Setup Readiness

Goal: make generic Unity setup guides useful for beginner-to-intermediate native workflows without outranking gameplay contracts.

Scope:

- Extend existing package availability facts for Cinemachine, Timeline, VFX Graph, Input System, UI, and render pipeline features.
- Extend observed scene evidence for common native setup objects: Camera, AudioSource, AudioListener, Canvas, EventSystem, Animator, PlayableDirector, ParticleSystem, VisualEffect.
- Extend built-in setup expected evidence into deeper binding-level readiness rows when observed.
- Keep static built-in setup guides as lower-priority graph evidence.

Files:

- `Editor/UnitySetup/BuiltInUnitySetupCatalog.cs`
- `Editor/Scanning/UnityObjectScanner.cs`
- `Editor/Scanning/UnityAssetScanner.cs`
- `Editor/Hygiene/DependencyGraphProjection.cs`
- `Editor/Projections/AuthoringProjectionBuilder.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/AUTHORING_MODEL.md`

Acceptance:

- Fresh projects get useful Unity-native guidance.
- Existing gameplay contracts always outrank built-in Unity setup guides in Intent.
- Gameplay scripts that reference Unity systems benefit from vocabulary labels without PYS claiming gameplay meaning.

Validation:

- Tests cover package/component evidence mapping.
- Manual fresh-project scan checks Camera, UI Canvas, Audio Source, Timeline, and Cinemachine flows.

## Phase 10: Facts Ledger Usability

Goal: make Facts a navigable evidence library.

Scope:

- Add grouping by kind, source path, confidence, and source count.
- Add search/filter text.
- Add compact evidence row summaries.
- Keep Facts free of guidance, next actions, and audit judgment.

Files:

- `Editor/Window/PysAuthoringWindow.Facts.cs`
- `Editor/Projections/AuthoringProjectionModels.cs`
- `Editor/Projections/AuthoringProjectionBuilder.cs`
- `Editor/Exports/ProjectionJsonExporter.cs`
- `Tests/Editor/ProjectionBuilderTests.cs`
- `Docs/PROJECTION_CONTRACTS.md`

Acceptance:

- Facts can answer where information came from.
- Built-in Unity setup guides, reflected fields, validators, contracts, assets, and issues are distinguishable.
- Exported Facts packet matches active filters.

Validation:

- Tests cover filters and export parity.
- Manual scan confirms large fact sets remain navigable.

## Phase 11: External Project Trial Matrix

Goal: validate PYS as a general Unity package, not just against itself or one target project.

Trial projects:

- PYS package scanning itself.
- Fresh Unity project with no contracts.
- Small Unity project with native Camera/UI/Audio setup and no gameplay contracts.
- Small scripted project with sparse contracts.
- Contract-rich project with validation methods.
- Large contract-rich target export as a stress case, without adding target-specific PYS code.

Acceptance:

- Fresh project: Settings, Facts, Hygiene, Map, and Unity Setup Guides are useful.
- Sparse project: Intent falls back honestly without pretending it knows more than it does.
- Contract-rich project: Intent, Overview, and Guide produce a coherent selected readiness route.
- Contract-rich target export: duplicate IDs, validation, route metadata, and current scene reality remain separated and honest.

Validation:

- Save representative exports under `Temp/PysAuthoringExports` for review.
- Compare UI packet state with exported JSON for each tab.
- Run package validation and PYS builds after each trial-driven fix.

## Phase 12: Code Maintenance And Living Docs

Goal: keep PYS maintainable as a product-grade package.

Scope:

- Remove stale terms and outdated guidance from active docs.
- Keep public fallback API documented only where it still exists.
- Maintain package boundary docs.
- Keep window files small and tab-owned.
- Add targeted tests before projection behavior changes.
- Avoid broad compatibility bridges and target-project adapters.

Files:

- `Docs/AUTHORING_MODEL.md`
- `Docs/CURRENT_STATE.md`
- `Docs/PROJECTION_CONTRACTS.md`
- `Docs/TARGET_PROJECT_INTEGRATION.md`
- `Docs/HYGIENE_LENSES.md`
- `Docs/TESTING.md`
- package tests and window/projection files touched by each phase

Acceptance:

- Active docs describe current truth only.
- No active docs preserve stale history as implementation guidance.
- No PYS package C# file imports target-project namespaces.
- Every changed projection has tests and export parity coverage.

Validation:

- `rg` scans for stale terms and target-project references.
- PYS package validation script.
- PYS contracts/editor/tests builds.
- Unity refresh/import when Unity project compile is not blocked by unrelated target code.

## Recommended Execution Order

1. Phase 1: First-Run And Settings UX.
2. Phase 2: Window Decomposition.
3. Phase 3: Intent Compact Workspace.
4. Phase 4: Overview Next Actions.
5. Phase 5: Guide Route Presentation.
6. Phase 7: Hygiene Productization.
7. Phase 10: Facts Ledger Usability.
8. Phase 6: Map Inspection Surface.
9. Phase 9: Built-In Unity Setup Readiness.
10. Phase 8: Visual Dependency Graph Lens.
11. Phase 11: External Project Trial Matrix.
12. Phase 12: Code Maintenance And Living Docs after every phase.

The first coherent implementation slice completed Settings first-use mode, tab-owned window decomposition, compact Intent rendering, Overview next-action packet rows, Guide route grouping, Map kind grouping, Hygiene severity grouping, Facts kind/search filtering, adaptive tab navigation, and compact/wrapped projection rows for small and large Editor windows. Follow-up slices added packet-backed Map select/ping inspection actions, package-availability issue evidence, native scene/component/field readiness evidence, scoped asset readiness evidence, Unity-native setup vocabulary, and node/edge dependency graph seed rows. The next coherent implementation slice should continue with binding-level native setup readiness, richer visual Hygiene rendering, and external project trials.
