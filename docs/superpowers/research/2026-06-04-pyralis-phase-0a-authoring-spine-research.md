# Pyralis Phase 0A Authoring Spine Research

Date: 2026-06-04

Status: Research checkpoint

## Question

Can the current Pyralis authoring spine grow into true reflective authoring, or do we need to go farther back and separate the data organization patterns first?

## Short Answer

The current spine is worth keeping, but the convention layer should be split before broader route migration.

Keep:

- ScriptableObject definitions and profiles as the primary authored data spine.
- `IAuthoringContractProvider` as the feature-owned contract pattern.
- `PyralisAuthoringFactRegistry` as the read-only aggregation/read-model surface.
- The Authoring Window as route map and diagnostic UI, not the source of truth.

Change next:

- Add distributed convention/fact providers parallel to `IAuthoringContractProvider`.
- Treat `PyralisAuthoringFact` as a normalized read model, not the only source model.
- Move route meaning out of central convention files and into source/domain-owned providers.
- Add diagnostics/tests that prevent central route-file gravity, duplicate ids, silent reflection failures, and label-driven routing.
- Add an explicit guidance/enforcement boundary: native Unity experimentation stays free, while reusable Pyralis features must attach to contracts, facts, validation, and proof targets.

## Current Spine Risk

`PyralisAuthoringFact` is useful, but broad. It currently carries capability, setup-node, issue, proof, lane, native action, requirement, customization, route relevance, and relationship data. That is acceptable as an interchange/read shape. It becomes risky if every source of meaning is flattened directly into this one class with no narrower owners.

`PyralisConventionAuthoringFacts` is the clearest growth pressure point. It uses reflection to read Unity metadata, but the target types and semantic relationships are still manually listed in one central file. That means it is attribute-assisted, not truly source-owned.

## Outside Patterns

### Unity

Unity's `ScriptableObject` model supports keeping reusable data outside scene instances and saving it as project assets. This maps well to Pyralis definitions, profiles, runtime patterns, and feature module data.

Unity DOTS baking is conceptually useful because it separates authoring data from runtime data, but adopting ECS is not a Phase 0A recommendation. Borrow the separation idea, not the package.

Unity package layout supports explicit `Runtime`, `Editor`, tests, samples, docs, and assembly boundaries. That supports moving authoring providers beside feature/editor ownership instead of using a central switchboard.

Sources:

- https://docs.unity3d.com/6000.1/Documentation/Manual/class-ScriptableObject.html
- https://docs.unity.cn/Packages/com.unity.entities%401.0/manual/baking.html
- https://docs.unity3d.com/Manual/cus-layout.html

### Unreal

Unreal Gameplay Ability System is a strong conceptual cousin. Abilities carry their own requirements, costs, tags, replication policies, and activation checks. Gameplay Tags provide stable semantic classification and behavior gates.

Pyralis should borrow the idea of feature-owned declarations plus stable hierarchical ids. Central systems should query capabilities, tags, routes, lanes, authority expectations, and proof targets, not own every feature's setup story.

Sources:

- https://dev.epicgames.com/documentation/unreal-engine/using-gameplay-abilities-in-unreal-engine
- https://dev.epicgames.com/documentation/en-us/unreal-engine/using-gameplay-tags-in-unreal-engine
- https://dev.epicgames.com/documentation/en-us/unreal-engine/data-assets-in-unreal-engine

### Godot

Godot separates Nodes as behavior/scene objects from Resources as reusable data. Its editor plugin and inspector plugin models support local editor extensions without replacing the whole editor.

Pyralis should keep definitions/profiles as durable assets, native Inspectors as the field editing surface, compact Inspector guides as local help, and the Authoring Window as route-aware diagnosis.

Sources:

- https://docs.godotengine.org/en/stable/tutorials/scripting/resources.html
- https://docs.godotengine.org/en/stable/tutorials/plugins/editor/index.html
- https://docs.godotengine.org/en/stable/classes/class_editorinspectorplugin.html

### Bevy And O3DE

Bevy's plugin/reflection shape and O3DE's Gem model both reinforce modular capability ownership: a feature should bring runtime code, data, editor contribution, validation, samples/proofs, and docs together.

Pyralis should treat larger features as capability packages with local authoring providers rather than as runtime code plus later central documentation.

Sources:

- https://bevyengine.org/learn/quick-start/getting-started/ecs/
- https://bevyengine.org/learn/quick-start/getting-started/plugins/
- https://github.com/bevyengine/bevy/tree/main/examples/reflection
- https://docs.o3de.org/docs/user-guide/gems/

### VS Code And Extension Manifests

VS Code extensions declare capabilities through contribution points in the extension manifest. The workbench aggregates those contributions instead of requiring every extension to edit central UI/router code.

Pyralis should use provider registries like manifest contribution points: owners declare facts and contracts; the registry normalizes, deduplicates, reports provenance, and exposes inventory.

Sources:

- https://code.visualstudio.com/api/references/contribution-points
- https://code.visualstudio.com/api/references/activation-events

### JSON Schema, OpenAPI, Roslyn, And DI

JSON Schema separates validation keywords from annotation keywords. Annotation fields can drive documentation/form tools without changing validation behavior. Pyralis should similarly distinguish semantic annotations from behavior-changing validation rules.

OpenAPI-style extension namespaces suggest a path for project-specific or experimental metadata without bloating the core contract vocabulary.

Roslyn analyzers/source generators suggest future guardrails: tests or analyzers can fail duplicate ids, missing providers, central switchboard drift, and label-driven routing. Source generation may help build a read-only index later, but generated output should not become the source of truth.

Application-part and keyed DI patterns suggest provider inventory, explicit ordering, provenance, and lookup by stable keys such as `moduleId`, `capabilityId`, `routeFamilyId`, `proofTargetId`, and `presentationLane`.

Sources:

- https://json-schema.org/understanding-json-schema/reference/annotations
- https://json-schema.org/understanding-json-schema/reference/schema
- https://spec.openapis.org/oas/latest
- https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/
- https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md
- https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts
- https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview

## Recommended Pyralis Spine

Use a layered model:

```text
Source-owned declarations
  Feature contracts
  Domain convention providers
  Unity reflection providers
  Validator/evidence providers
  Proof target providers
        |
        v
Normalized authoring facts/read model
        |
        v
Route analysis, validators, Inspector guides, Authoring Window, docs, tests
```

The fact registry should aggregate and normalize. It should not be the only place where meaning is authored.

## Guidance Versus Enforcement

The outside patterns point to the same boundary:

- Unity editor extensions and custom inspectors support workflows without replacing the Inspector.
- Unity ScriptableObjects and Godot Resources keep authored data as reusable assets, while scene objects remain normal engine objects.
- VS Code contribution points and O3DE Gems let extensions/packages contribute to a shared system without forcing every file in the workspace to become part of that system.

For Pyralis, this means the authoring system should be strict only at the point of Pyralis participation.

Allowed outside Pyralis:

- native Unity objects, components, scenes, prefabs, scripts, art, UI, and experiments
- project-specific systems that are not claiming Pyralis route/library support
- temporary prototypes that have not entered the shared authoring body

Required inside Pyralis:

- stable ids
- source-owned contracts or convention facts
- native Unity setup actions
- supported and unsupported lane declarations
- validation and issue codes for real setup failures
- proof target or explicit exclusion
- tests and docs when a workflow becomes shared

The Authoring Window should classify unrelated Unity content as outside the active setup, not wrong. It should become strict when a developer wires that content into Pyralis or presents it as reusable Pyralis library work.

## Concrete Phase 0A Decision

Adopt a distributed-provider architecture.

1. Add `IAuthoringConventionFactProvider`.
2. Add a convention fact registry with the same reflective discovery style as `IAuthoringContractProvider`.
3. Convert the current `PyralisConventionAuthoringFacts` into a bridge provider.
4. Migrate convention facts out by domain:
   - core setup
   - pawn/1P Sprite2D
   - tabletop/no-pawn
   - camera/world
   - UI/HUD/menu
   - combat/enemy
   - feature module surfaces
5. Keep global reflection cheap and explicit; run deep scene/prefab validation lazily by context.
6. Add guardrail tests before broad migration.

## Stable ID Direction

Prefer hierarchical ids:

```text
feature.combat.melee
feature.pickups.2d
route.pawn.realtime
route.tabletop.board
proof.pawn.first-move
proof.tabletop.first-action
input.action.primary
lane.presentation.sprite2d
network.role.server-authoritative
```

Rules:

- Display text can change; stable ids should not.
- Validators, tests, proof mapping, route cards, and facts should key from ids.
- Namespaces should show ownership: `pyralis.*` for built-in facts, `project.*` for game-specific extensions, and `experimental.*` only when intentionally temporary.

## Anti-Patterns To Avoid

- One central route file that knows every feature.
- Warning copy or UI labels used as routing logic.
- Reflection without duplicate/missing/provider-failure diagnostics.
- Adding rich contract fields before Authoring Window, validators, tests, and docs all understand them.
- Full scene/prefab scans on every selection change.
- DI becoming a second hidden authoring plugin system.
- Generated indexes replacing source-owned declarations.
- Asset labels or Addressables labels becoming canonical setup truth.
- Over-enforcing unrelated native Unity work as Pyralis errors.
- Under-enforcing reusable Pyralis features that have no contract/fact/proof attachment.

## Immediate Next Slice

Implement Phase 0A as a small architecture hardening slice:

- Add the convention provider interface and registry.
- Route existing convention facts through the registry without changing visible behavior.
- Add tests for provider discovery, duplicate stable ids, and bridge-provider continuity.
- Document provider ownership rules in the active authoring docs.

After that, Phase 0B should migrate the 1P Sprite2D route's convention facts into a domain-owned provider, then run the first Computer Use live proof pass.
