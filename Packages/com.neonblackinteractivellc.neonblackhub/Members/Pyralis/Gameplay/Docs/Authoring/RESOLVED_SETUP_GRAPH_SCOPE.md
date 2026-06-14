# Resolved Authoring Setup Graph Scope

This document defines the full migration scope for the Pyralis resolved authoring setup graph. Use it before implementing graph work, moving Authoring Window tabs, or adding new contract fields.

## North Star

Gameplay systems should declare their authoring meaning once, and the Authoring Window should project that meaning consistently.

```text
Gameplay systems
  -> feature-owned authoring contracts and facts
      -> read-only resolved setup graph
          -> Intent, Overview, Guide, Map, Validate, Facts, and selected-context projections
```

The graph is the authoring memory. Tabs are projections. Contracts and existing Unity setup evidence are the source material. Map, Validate, Overview, Guide, Facts, and selected-context surfaces should read graph projections instead of reconstructing route, proof, or validation meaning locally.

The old route-report and validation-card paths are archived and removed. Do not recreate `PyralisAuthoringRouteReport`, `PyralisAuthoringValidationModel`, or tab-local setup card models beside the graph. Route proof templates, capability vocabulary, route analysis, setup-flow validation, scene-readiness validation, and grammar facts still exist where they earn their keep as source inputs, fallback grammar, or audit dictionaries.

When a graph projection replaces an older active path, archive the useful migration context under `Docs/_Archive/Migration/`, then delete the migrated code or tests instead of leaving a silent parallel implementation behind. The archive is a temporary map for future cleanup; it is not a second source of authoring truth.

Graph nodes also carry source-origin provenance. Use this to tell whether a visible row came from user-authored setup, reflection, contract metadata, runtime evidence, spine grammar, or grammar fallback. The target is not "no central code at all"; it is that central code only owns spine grammar and fallback wording, while feature-specific setup meaning lives in contracts/reflection.

Current implementation: selected setup assets and optional `RuntimePatternDefinition` assets project as `UserAuthoredSetup` graph nodes. Missing setup concepts remain `SpineGrammar`. Explicit contracts project as `Contract`; inferred or convention-derived contracts project as `Reflection`. This provenance is audit metadata, not a new authoring source.

## Why This Exists

The authoring system is organized around one resolved setup graph compiled from cooperating source inputs:

- `ResolvedAuthoringContractRegistry` resolves feature-owned `[AuthoringContract]` metadata.
- `PyralisAuthoringGrammarRegistry` aggregates grammar, vocabulary, generated facts, and fallback wording for audit/projection.
- `PyralisSetupRouteAnalysis` interprets `GameSetupProfile.runtimeCapabilities` and optional runtime patterns.
- `PyralisSetupFlowValidator` reports setup-chain readiness.
- `PyralisSceneReadinessValidator` reports concrete scene and prefab evidence.
- `PyralisAuthoringSetupGraphBuilder` selects proof nodes from contract/descriptor proof targets first; `PyralisProofFamilyVocabulary` owns generic fallback proof templates; `PyralisAuthoringOverviewModel`, graph projections, and tab renderers project resolved setup graph output.
- `PyralisContractProofFactProjector` enriches broad route proof facts from resolved feature contracts that target the same `FirstProofTargetId`, then fills genuinely new contract-owned proof ids when no broad proof exists yet.

This shape keeps feature work from requiring repeated tab edits. Normal feature development should be:

```text
write gameplay system
  -> declare/update authoring contract
      -> add validation evidence only when real Unity setup needs proof
          -> graph projections inherit the meaning
```

## Non-Negotiable Boundary

The graph is read-only setup intelligence.

It may describe:

- setup nodes
- dependencies
- satisfied requirements
- missing links
- proof readiness
- native Unity actions
- evidence state
- customization checkpoints
- source contracts
- source Unity objects

It must not:

- create assets
- generate scenes
- add components
- assign fields
- choose art
- choose camera composition
- choose combat feel
- choose board layout
- apply presets
- become a route generator

Intent remains the route declaration surface. It writes explicit creator choices such as world/playfield, control shape, presentation lane, capability toggles, and proof goals. The graph explains the consequences of those choices.

## Full Migration Scope

### Phase 1: Read-Only Graph Facade

Add the graph beside the current spine.

Inputs:

- selected `GameplaySessionBootstrap`, `SessionDefinition`, `GameModeDefinition`, or `GameSetupProfile`
- `PyralisSetupRouteAnalysis`
- `ResolvedAuthoringContractRegistry`
- grammar/vocabulary defaults from `PyralisAuthoringGrammarRegistry`, `PyralisCapabilityVocabulary`, and `PyralisProofFamilyVocabulary`
- proof identity from contract/descriptor targets plus fallback proof vocabulary
- `PyralisSetupFlowValidator`
- `PyralisSceneReadinessValidator`

Outputs:

- stable graph nodes
- graph edges
- evidence states
- lookup helpers
- tests proving route/capability/contract/proof/evidence nodes exist

Visible Authoring Window behavior should not change in this phase.

### Phase 2: Map And Overview Projection

Move the easiest duplicate projections onto the graph.

Map should render graph topology:

- bootstrap
- session
- mode
- setup profile
- selected capabilities
- participants
- pawn or no-pawn surface
- scene surfaces
- feature modules
- proof target
- evidence nodes

Overview should render graph priority:

- best next unresolved node
- required setup
- proof enhancers
- later feature cards
- play-mode readiness checklist
- strongest inspect target

Keep existing visible layout unless the graph exposes a clearer ordering.

Current implementation: the visible Map tab reads `PyralisAuthoringSetupGraph` through `PyralisAuthoringSetupGraphProjection.BuildSetupMapRows`, `BuildMapConnectionRows`, `FindSceneSurfaceNodes`, and `BuildReadinessRows`. It must not read `PyralisAuthoringRouteReport`, `PyralisAuthoringValidationModel`, or route analysis directly.

### Phase 3: Validate Projection

Attach validation issues to graph node ids.

`PyralisSceneReadinessValidator` keeps its concrete Unity knowledge. The graph normalizes scene-readiness evidence into validation rows so Validate can group issues by resolved graph node id instead of by repeated string heuristics.

Good direction:

```text
scene.camera
  -> evidence: missing active camera or audio listener issue
  -> native action: inspect camera root / add camera / assign listener
```

Avoid flattening useful validator detail into generic graph warnings.

Current implementation: scene-readiness issues are reflected as deterministic `ValidationEvidence` graph nodes, and Validate renders required, recommended, and proof-enhancer buckets from `PyralisAuthoringSetupGraphProjection.BuildValidationRows`.
Current implementation: setup-flow evidence nodes remain distinct runtime evidence nodes, but their ids derive from canonical `PyralisSetupFlowGuidance.GetStableId(...)` setup ids when available, and the graph links each setup-flow evidence node back to the matching setup node. Keep this split: setup nodes describe spine grammar, evidence nodes describe the current scene/setup state.
Current implementation: `PyralisAuthoringSetupGraphProjection.IsReadinessNode` is the shared readiness filter for Validate and Overview. It treats setup-chain nodes, the capability summary node, pawn/no-pawn requirements, scene-surface nodes, and validation-evidence nodes as setup health. Tabs may render those rows differently, but they should not each invent their own readiness filter.
Current implementation: the Authoring Window no longer caches or passes `PyralisAuthoringRouteReport` into visible tabs. Validate readiness buckets and detailed evidence cards are graph-backed through `PyralisAuthoringSetupGraphProjection.BuildValidationRows`. The older `PyralisAuthoringValidationModel` card surface was removed after typed issue projection moved to `PyralisAuthoringSetupGraphProjection.BuildTypedValidationIssues`; do not reintroduce tab-local validation card models.
Current implementation: the visible Validate tab reads `PyralisAuthoringSetupGraph` through current-step and validation-row projections. Concrete setup-flow and scene-readiness validators remain graph inputs inside the builder; Validate must not call those validators or legacy route/validation models directly.
Current implementation: graph evidence can also project typed `PyralisAuthoringIssue` rows through `PyralisAuthoringSetupGraphProjection.BuildTypedValidationIssues`. Use this path for new typed validation consumers instead of adding separate validation-card infrastructure.

### Phase 4: Guide And Selected Context Projection

Move Guide cards and selected-object explanation onto graph projections.

Guide should rank:

- active intent nodes
- missing setup nodes
- relevant contract nodes
- nearby recommended capability nodes
- proof-supporting nodes

Selected context should explain the selected graph node or source Unity object instead of hand-writing meaning per object type wherever possible.

Current implementation: selected setup assets, bootstrap roots, pawn roots, reflected contracts, runtime-pattern details, and setup-profile optional route-contract details resolve through `PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow`, and Guide uses that row for role, graph id, evidence, native setup, and first check. Keep specialized selection renderers only where they expose object-specific actions such as selecting scene components, pinging targets, or filling missing RuntimePattern text without choosing route content.
Current implementation: Guide selected-context and current-step text are graph-first. `bootstrap.root` is an explicit missing setup-chain node for blank setup contexts, so Overview and Guide can name the native Gameplay Root action without reading archived route reports or recomputing setup-flow reports in the window surface. Native action detail should come from graph nodes/current-step rows; the surface helper only handles the true pre-graph bootstrap hint.
Current implementation: Guide current-intent rows use `PyralisAuthoringSetupGraphProjection.BuildCurrentIntentGuideRows(...)` only when the graph has a resolved setup context such as a Bootstrap, SessionDefinition, GameModeDefinition, GameSetupProfile, ParticipantDefinition, PawnDefinition, or Gameplay Root. Those rows are generated from first unresolved graph nodes, active proof nodes, proof support/blocker edges, selected capability nodes, and reflected contracts. `PyralisAuthoringIntentAdvisor` remains the pre-setup/fallback planning model; do not use it as the primary setup-backed Guide source.
Current implementation: Facts opens with graph coverage, graph contract coverage, and graph proof coverage before the raw cookbook dictionary. The raw fact registry remains a read-only audit/reference surface, not the primary setup guidance path.

### Phase 5: Contract Enrichment

Only after the graph proves which metadata it repeatedly needs, enrich `[AuthoringContract]`.

Current implementation: `SetupNodeId` is available on `[AuthoringContract]` and normalized onto `ResolvedAuthoringContract`. Use it when a contract enriches a stable graph setup concept such as `bootstrap.root`, `session.definition`, `mode.definition`, `setup.profile`, `participant.default`, or `pawn.definition`. Do not add a `SetupNodeId` just because a component has a native setup action; HUD binders, projectile launchers, board presenters, services, and similar feature pieces should usually remain contract, evidence, requirement, or native-action nodes unless the graph has promoted that concept into the setup spine. The graph links contract nodes to declared setup nodes, selected-context projection prefers the setup node when a selected contract declares one, and cookbook facts include the setup node as a related stable id. This replaces repeated type-to-setup guessing without adding a separate mapping registry.

Current implementation: `FirstProofTargetId` remains the machine-readable proof route and `FirstProof` remains human developer guidance. Explicit `FirstProofTargetId` values win. When a contract does not declare one, graph proof selection falls back to descriptor capability families and then to generic proof vocabulary. Ambiguous contract routes stay blank instead of guessing. `PyralisProofFamilyVocabulary` owns broad fallback proof templates such as pawn movement, tabletop action, UI/HUD, camera/cursor, generated content, and networking. `PyralisContractProofFactProjector` can enrich proof facts for graph display, but feature-specific setup requirements should still originate in contracts/reflection and graph evidence.
Current implementation: proof vocabulary templates are fallback grammar only. `PyralisProofFamilyVocabulary.GetDefaultProofTemplates()` declares stable proof ids, broad route meaning, generic Play Mode checks, and what can wait. It does not own feature-specific required profiles, runtime interfaces, scene components, or assignment fields.
Current implementation: Overview guidance, current step, proof-support rows, and Do Now readiness are graph-backed. Overview no longer accepts `PyralisAuthoringRouteReport`; it reads route name, first unresolved node, readiness lanes, and proof support from `PyralisAuthoringSetupGraph` projections. The old `ResolvedAuthoringContractProofGuidance` code path was removed after graph proof edges took over visible proof support. See `Docs/_Archive/Migration/authoring-proof-guidance-to-graph.md` for the migration note.

Current implementation: Intent projection asks `PyralisAuthoringCapabilityDescriptorRegistry` for reflected capability descriptors and selects matching `RuntimeCapabilityFamily` rows from contract `AuthoringCapability` flags, route lane, and world axioms. Fallback vocabulary may supply labels and generic wording, but it must not create assets, imply presets, or choose game content.

Current implementation: `PyralisCapabilityVocabulary` owns broad fallback capability vocabulary. `PyralisAuthoringCapabilityDescriptorRegistry` is the graph-facing capability surface: it prefers contracts/reflection, then fills generic labels and fallback wording from vocabulary. Feature-specific setup truth should live in contracts/reflection rather than hardcoded card copy.

Current implementation: `PyralisSetupDependencyTree` is the reflected setup dependency resolver for the canonical setup chain and its key child references. It reflects `GameplaySessionBootstrap.sessionDefinition`, `SessionDefinition.defaultGameMode`, `GameModeDefinition.setupProfile`, every `SessionDefinition.defaultParticipants` entry, every participant `defaultPawn`, setup profile runtime capability and runtime pattern references, mode board/turn/feature-module references, pawn prefab/profile references, and pawn feature-module references. `PyralisSetupRouteAnalysis` consumes that tree so route analysis preserves current behavior while dependency discovery moves toward serialized-reference reflection. Explicit caller-provided setup objects still win over reflected defaults; reflection fills missing context, it does not override the selected authoring object. Keep the aggregate node ids such as `participant.default` and `pawn.definition` for current tab compatibility while using indexed child nodes such as `participant.default.0` and `pawn.definition.0` for future graph-native validation parity.

Current implementation: proof readiness includes graph topology for blockers. Missing or blocked setup-chain, Unity-surface, and validation-evidence nodes emit `BlockedBy` edges from the active proof node to the required node. Capability support remains `Capability -> Proof` via `SupportsProof`; missing setup should not be represented as capability support or as an inverted blocker edge.

Current implementation: Overview lanes, first-proof summary, play-mode checklist, and best-next-action are graph projection outputs. `PyralisAuthoringOverviewModel` lives under the graph spine as a graph-native read model over those projections; Overview rows carry graph evidence states instead of setup-flow status/work-intent enums.

Current implementation: `ProfileType` is profile metadata only. It must not automatically imply `IFeatureModuleRuntime`, scene components, Unity object components, or service ownership. `RequiredInterfaces` and `RequiredInterfaceNames` describe dependency surfaces that code can implement. `RequiredComponents` and `RequiredComponentNames` describe physical Unity placement requirements that reflection cannot infer, such as actor-root, scene-root, UI-root, or runtime-prefab components. This keeps core profiles such as input, setup, camera, and settings from accidentally projecting as feature-module runtime requirements while still letting contracts own the setup truth reflection cannot see.

Current implementation: route proof graph nodes use stable proof ids from contract/descriptor proof targets and fallback proof facts. Capability nodes relate to proof ids through descriptor proof targets, but graph construction must not synthesize proof ids from display labels or proof-step labels.

Remaining candidates:

- `DependsOnNodeIds`
- `SatisfiesNodeIds`
- `EvidenceKind`
- structured native action data
- setup guidance separate from developer proof guidance
- customization guidance separate from assignment fields

Do not add contract fields because they sound complete. Add them when the graph has a real repeated derivation gap.

## Minimum Graph Vocabulary

Node kinds:

- setup chain
- capability
- contract
- proof
- scene surface
- Unity surface requirement
- assignment field
- validation evidence

Edge kinds:

- depends on
- satisfies
- recommends
- supports proof
- blocked by
- relates to

Proof support and proof blocking must stay separate. A capability, contract, or fact that helps a proof should use `SupportsProof`; a missing or invalid prerequisite should use `BlockedBy`. Do not represent positive support as a blocker edge with a friendly label.

Evidence states:

- unknown
- optional
- missing
- candidate detected
- ready
- blocked

Source origins:

- user-authored setup: selected setup assets, profile choices, and route declarations
- reflection: meaning derived directly from code shape, fields, interfaces, attributes, and Unity metadata
- contract: semantic meaning explicitly declared by `[AuthoringContract]`
- runtime evidence: setup-flow, scene-readiness, and concrete Unity object validation evidence
- spine grammar: stable Pyralis setup chain concepts and graph vocabulary
- spine fallback: generic proof guidance used before feature-owned proof coverage exists
- legacy fact: transitional premade fact/card libraries waiting to be migrated or deleted

## Example Nodes

Common stable node ids should look like:

```text
bootstrap.root
session.definition
mode.definition
setup.profile
capability.character-pawn-gameplay
capability.combat
participant.default
pawn.definition
pawn.prefab.root
scene.camera
scene.event-system
feature.contract.actor.traversal.topdown-hop
proof.1p-pawn-movement
```

Use readable, stable ids. Do not derive ids from UI labels when a durable contract id or setup concept id exists.

## Duplication To Retire Gradually

These areas should shrink as graph projections take over:

- Legacy Map migration notes and tests that predate graph projection.
- Overview-specific proof and checklist reconstruction.
- Legacy Validate keyword/category guessing and typed-card migration tests that predate graph evidence.
- Guide card duplication.
- Facts semantic tag fallback guessing.
- Selected-context hard-coded explanations.
- Route proof and capability rows that duplicate contract-backed graph data.

Do not delete real source inputs just because a tab has migrated. Retire only migrated compatibility code after an equivalent graph projection is tested and visible behavior remains useful.

## Test Strategy

Phase 1 tests should prove graph shape, not UI polish:

- a blank or null source still builds a graph with reflected contract nodes
- a `GameSetupProfile` with pawn/combat capabilities builds setup, capability, and proof nodes
- a reflected feature module contract becomes a contract node with proof target, native setup, assignment fields, and customization moments
- a `GameplaySessionBootstrap` source can attach setup-flow and scene-readiness evidence nodes
- graph nodes preserve source-origin provenance for spine grammar, legacy facts, fallback proofs, contracts, and runtime evidence
- optional `RuntimePatternDefinition` assets project as user-authored setup nodes instead of staying hidden in route analysis

Later phases should add tab-specific projection tests before deleting old logic.

Before migrating premade proof or capability cards, run a provenance sanity pass:

- `SpineGrammar` rows identify compiler grammar, vocabulary labels, or cookbook wording.
- `GrammarFallback` proof rows identify generic proof wording used when no feature-owned contract has supplied a richer proof target yet.
- `Contract` rows prove the feature-owned setup contract path is represented in the graph.
- `RuntimeEvidence` rows prove validators are still feeding concrete Unity readiness instead of being flattened into generic warnings.
- Any feature-specific card should move only when its replacement contract/reflection graph node and its proof/readiness edges are covered by tests.

## Documentation Strategy

Active docs should describe the graph as the current authoring projection spine. Migration notes belong under `Docs/_Archive/Migration/` when they still explain why an old path was removed.

Avoid active-doc language that implies a second visible-tab model is still supported. Stale caution is just another form of duplicated truth.

## Implementation Artifacts

The dated implementation artifacts have been retired from active repository docs. Future agents should use this scope document, `AUTHORING_BLUEPRINT.md`, `AUTHORING_MODEL.md`, and the graph/dependency-tree tests as the durable migration boundary.
