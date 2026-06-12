# Read-Only Authoring Setup Graph Design

## Goal

Create a read-only resolved setup graph for Pyralis authoring so gameplay systems can declare setup meaning through contracts and the Authoring Window can eventually render every tab from one shared model.

## Current Problem

Pyralis authoring is much cleaner than before, but several models still interpret the same setup independently:

- `ResolvedAuthoringContractRegistry` resolves feature-owned contracts.
- `PyralisAuthoringFactRegistry` aggregates cookbook facts.
- `PyralisSetupRouteAnalysis` interprets setup profile capabilities and optional runtime patterns.
- `PyralisSetupFlowValidator` produces setup-chain readiness rows.
- `PyralisSceneReadinessValidator` produces concrete Unity scene and prefab evidence.
- Overview, Map, Validate, Guide, Facts, and selected-context renderers project those models separately.

That means a gameplay developer can still need to update contracts, facts, proof routing, route analysis, map logic, overview logic, validation wording, and docs when adding a feature. The desired future is that the developer updates gameplay code plus its authoring contract, and the graph makes the setup meaning available to every tab.

## Architecture

Phase 1 introduces a read-only graph facade under the authoring spine:

```text
Selection / GameSetupProfile / GameplaySessionBootstrap
  -> existing contracts, facts, route analysis, setup-flow report, scene readiness report
      -> PyralisAuthoringSetupGraph
          -> future tab projections
```

The graph is not a new registry and not a recipe engine. It is a resolver/projection layer over existing truth. It does not create assets, wire scenes, apply presets, mutate intent, choose art, or choose game feel.

## Core Model

The first graph model needs three small concepts:

- `PyralisAuthoringGraphNode`: a stable setup concept such as `bootstrap.root`, `setup.profile`, `capability.pawn-action`, `feature.contract.actor.traversal.topdown-hop`, `scene.camera`, or `proof.1p-pawn-movement`.
- `PyralisAuthoringGraphEdge`: a relationship between nodes such as `DependsOn`, `Satisfies`, `Recommends`, `BlocksProof`, or `RelatesTo`.
- `PyralisAuthoringSetupGraph`: a read-only container with route context, nodes, edges, and lookup helpers.

Minimum node fields:

- stable id
- label
- node kind
- source kind
- evidence state
- capability family
- typed authoring capability
- proof target id
- guidance
- native setup text
- assignment fields
- customization moments
- blocking reason
- source contract
- source object

## Phase 1 Scope

Phase 1 should add the graph without migrating the visible Authoring Window:

1. Create graph model types under `Editor/Authoring/Spine/Graph`.
2. Build graph nodes from existing route analysis, runtime capability cards, resolved contracts, setup-flow rows, scene readiness rows, and route proof data.
3. Add editor tests proving a simple pawn route and reflected feature contract produce expected nodes and edges.
4. Update active docs to name the resolved graph as the next authoring spine without claiming all tabs have migrated.

## Out Of Scope For Phase 1

- No tab renderer rewrite.
- No Intent behavior change.
- No asset generation.
- No scene creation.
- No automatic field assignment.
- No contract enrichment beyond what existing metadata can already resolve.
- No removal of route reports, setup-flow validator, scene-readiness validator, or fact registry.

## Migration Path

After Phase 1 proves the graph:

1. Move Map topology to graph nodes and edges.
2. Move Overview next-step and play-mode checklist projection to graph evidence.
3. Attach Validate issues to graph node ids while preserving detailed Unity validator messages.
4. Move Guide cards and selected-context explanation to graph projections.
5. Enrich `[AuthoringContract]` only for data the graph repeatedly needs and cannot derive cleanly, such as setup node ids, dependency ids, satisfied node ids, evidence kind, or structured native actions.

## Guardrail

The graph may describe setup, dependencies, evidence, missing links, proof readiness, native Unity actions, and customization checkpoints. It must not create content, choose design, wire taste, or become a preset system.

## Success Criteria

Phase 1 is successful when:

- A graph can be built for a selected `GameSetupProfile`, `GameModeDefinition`, `SessionDefinition`, or `GameplaySessionBootstrap`.
- The graph contains stable nodes for setup chain, selected capability families, proof target, reflected feature contracts, setup-flow evidence, and scene-readiness evidence when those sources are available.
- Existing authoring UI behavior remains unchanged.
- Existing smoke validation passes.
- Tests prove graph coverage for at least one pawn-backed route and one reflected feature-module contract.
