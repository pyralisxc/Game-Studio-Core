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

The graph is the authoring memory. Tabs are projections. Contracts and existing Unity setup evidence are the source material.

Older route reports, route proof cards, overview models, validation models, and tab-specific projections are migration scaffolding. They may remain as source inputs, fallback references, or markdown-documented migration notes while parity is proven, but visible tab behavior should move toward reading the synthesized graph projection first. Do not add new setup meaning to those older paths unless the same meaning is generated into the graph in the same slice.

When a graph projection replaces an older active path, archive the useful migration context under `Docs/_Archive/Migration/`, then delete the migrated code or tests instead of leaving a silent parallel implementation behind. The archive is a temporary map for future cleanup; it is not a second source of authoring truth.

Graph nodes also carry source-origin provenance. Use this during migration to tell whether a visible row came from user-authored setup, reflection, contract metadata, runtime evidence, spine grammar, spine fallback, or a legacy fact library. The migration target is not "no central code at all"; it is that central code only owns spine grammar and fallback behavior, while feature-specific setup meaning moves to contracts/reflection. `LegacyFact` and `SpineFallback` nodes should be counted, made visible enough for audits, and reduced only after equivalent `Contract` or `Reflection` graph coverage is tested.

Current implementation: selected setup assets and optional `RuntimePatternDefinition` assets project as `UserAuthoredSetup` graph nodes. Missing setup concepts remain `SpineGrammar`. Explicit contracts project as `Contract`; inferred or convention-derived contracts project as `Reflection`. This provenance is audit metadata, not a new authoring source.

## Why This Exists

The current authoring system is organized, but setup meaning is still interpreted by several cooperating models:

- `ResolvedAuthoringContractRegistry` resolves feature-owned `[AuthoringContract]` metadata.
- `PyralisAuthoringFactRegistry` aggregates cookbook facts.
- `PyralisSetupRouteAnalysis` interprets `GameSetupProfile.runtimeCapabilities` and optional runtime patterns.
- `PyralisSetupFlowValidator` reports setup-chain readiness.
- `PyralisSceneReadinessValidator` reports concrete scene and prefab evidence.
- `PyralisAuthoringRouteProof`, `PyralisAuthoringOverviewModel`, `PyralisAuthoringValidationModel`, `PyralisAuthoringRouteReport`, graph projections, and tab renderers each project pieces of that truth.
- `PyralisContractProofFactProjector` enriches broad route proof facts from resolved feature contracts that target the same `FirstProofTargetId`, then fills genuinely new contract-owned proof ids when no broad proof exists yet.

That is workable, but it still means feature work can require too many authoring touch points. The graph migration should reduce that pressure so normal feature development is:

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
- `PyralisAuthoringFactRegistry`
- `PyralisRuntimeCapabilityCatalog`
- `PyralisAuthoringRouteProof`
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

Current implementation: scene-readiness issues are reflected as `ValidationEvidence` graph nodes, and Validate renders required, recommended, and proof-enhancer buckets from `PyralisAuthoringSetupGraphProjection.BuildValidationRows`.
Current implementation: `PyralisAuthoringSetupGraphProjection.IsReadinessNode` is the shared readiness filter for Validate and Overview. It treats setup-chain nodes, the capability summary node, pawn/no-pawn requirements, scene-surface nodes, and validation-evidence nodes as setup health. Tabs may render those rows differently, but they should not each invent their own readiness filter.
Current implementation: the Authoring Window no longer caches or passes `PyralisAuthoringRouteReport` into visible tabs. Validate readiness buckets and detailed evidence cards are graph-backed through `PyralisAuthoringSetupGraphProjection.BuildValidationRows`. `PyralisAuthoringValidationModel` remains migration/reference code for older structured validation-card coverage until its useful tests and helpers are either folded into graph evidence or archived.

### Phase 4: Guide And Selected Context Projection

Move Guide cards and selected-object explanation onto graph projections.

Guide should rank:

- active intent nodes
- missing setup nodes
- relevant contract nodes
- nearby recommended capability nodes
- proof-supporting nodes

Selected context should explain the selected graph node or source Unity object instead of hand-writing meaning per object type wherever possible.

Current implementation: selected setup assets, bootstrap roots, pawn roots, and reflected contracts resolve through `PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow`, and Guide uses that row for role, graph id, evidence, native setup, and first check. Keep specialized selection renderers only where they expose object-specific details such as runtime pattern lane fields or GameObject component lists.
Current implementation: Guide selected-context and current-step text are graph-first. `bootstrap.root` is an explicit missing setup-chain node for blank setup contexts, so Overview and Guide can name the native Gameplay Root action without reading `PyralisAuthoringRouteReport`.
Current implementation: Facts opens with graph coverage, graph contract coverage, and graph proof coverage before the raw cookbook dictionary. The raw fact registry remains a read-only audit/reference surface, not the primary setup guidance path.

### Phase 5: Contract Enrichment

Only after the graph proves which metadata it repeatedly needs, enrich `[AuthoringContract]`.

Current implementation: `SetupNodeId` is available on `[AuthoringContract]` and normalized onto `ResolvedAuthoringContract`. Use it when a contract enriches a stable graph setup concept such as `bootstrap.root`, `session.definition`, `mode.definition`, `setup.profile`, `participant.default`, or `pawn.definition`. Do not add a `SetupNodeId` just because a component has a native setup action; HUD binders, projectile launchers, board presenters, services, and similar feature pieces should usually remain contract, evidence, requirement, or native-action nodes unless the graph has promoted that concept into the setup spine. The graph links contract nodes to declared setup nodes, selected-context projection prefers the setup node when a selected contract declares one, and cookbook facts include the setup node as a related stable id. This replaces repeated type-to-setup guessing without adding a separate mapping registry.

Current implementation: `FirstProofTargetId` remains the machine-readable proof route and `FirstProof` remains human developer guidance. Explicit `FirstProofTargetId` values win. When a contract does not declare one, `ResolvedAuthoringContractRegistry` may infer it from dependency-connected contracts only if the graph yields exactly one distinct proof route. Ambiguous routes stay blank instead of guessing. `PyralisAuthoringRouteProof` still owns broad route proof grammar such as pawn movement, tabletop action, UI/HUD, camera/cursor, generated content, and networking. `PyralisContractProofFactProjector` merges contract-owned profiles, components, assignments, customization moments, native actions, related ids, axioms, capability tags, and authoring lanes into matching broad proof facts, then fills proof facts for contract-owned proof ids that do not already exist in the broad grammar.
Current implementation: Overview guidance, current step, proof-support rows, and Do Now readiness are graph-backed. Overview no longer accepts `PyralisAuthoringRouteReport`; it reads route name, first unresolved node, readiness lanes, and proof support from `PyralisAuthoringSetupGraph` projections. The old `ResolvedAuthoringContractProofGuidance` code path was removed after graph proof edges took over visible proof support. See `Docs/_Archive/Migration/authoring-proof-guidance-to-graph.md` for the migration note.

Current implementation: `PyralisRuntimeCapabilityFamilyMap` is the shared translation layer from reflected `AuthoringCapability` flags, route lane, and world axioms into `RuntimeCapabilityFamily` rows. Intent projection and reflective contract validation consume this map instead of maintaining separate capability-family switches. Keep this as spine grammar: it describes how contract vocabulary maps to setup-family vocabulary, but it must not create assets, imply presets, or choose game content.

Current implementation: `ProfileType` is profile metadata only. It must not automatically imply `IFeatureModuleRuntime`, scene components, Unity object components, or service ownership. `RequiredInterfaces` and `RequiredInterfaceNames` describe dependency surfaces that code can implement. `RequiredComponents` and `RequiredComponentNames` describe physical Unity placement requirements that reflection cannot infer, such as actor-root, scene-root, UI-root, or runtime-prefab components. This keeps core profiles such as input, setup, camera, and settings from accidentally projecting as feature-module runtime requirements while still letting contracts own the setup truth reflection cannot see.

Current implementation: route proof graph nodes use stable proof ids from `PyralisAuthoringRouteProof.StableId` and enriched proof facts. Capability nodes may relate to proof ids through their fact `RelatedStableIds`, but graph construction must not synthesize proof ids from display labels or proof-step labels.

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

## Current Duplication To Retire Gradually

These areas should shrink as graph projections take over:

- Map-specific topology construction.
- Overview-specific proof and checklist reconstruction.
- Validate keyword/category guessing.
- Guide card duplication.
- Facts semantic tag fallback guessing.
- Selected-context hard-coded explanations.
- Route proof and capability rows that duplicate contract-backed graph data.

Do not delete these immediately. Retire them only after an equivalent graph projection is tested and visible behavior remains useful.

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

- `LegacyFact` rows identify capability/fact-card content still owned by premade libraries.
- `SpineFallback` proof rows identify generic proof grammar still waiting for feature-owned enrichment.
- `Contract` rows prove the feature-owned setup contract path is represented in the graph.
- `RuntimeEvidence` rows prove validators are still feeding concrete Unity readiness instead of being flattened into generic warnings.
- Any feature-specific card should move only when its replacement contract/reflection graph node and its proof/readiness edges are covered by tests.

## Documentation Strategy

Active docs should describe the graph as the target authoring spine once Phase 1 exists. Until tabs migrate, docs should say the graph is being introduced beside existing models.

Avoid saying the graph already powers every tab until the code does. Stale optimism is just another form of duplicated truth.

## Implementation Artifacts

The Phase 1 design and implementation plan live at:

- `docs/superpowers/specs/2026-06-12-read-only-authoring-setup-graph-design.md`
- `docs/superpowers/plans/2026-06-12-read-only-authoring-setup-graph.md`

Future agents should use this scope document for the full migration boundary and the plan document for the first implementation slice.
