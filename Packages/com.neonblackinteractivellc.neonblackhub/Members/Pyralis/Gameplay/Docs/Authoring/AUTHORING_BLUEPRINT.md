# Pyralis Authoring Blueprint

This is the canonical product and implementation blueprint for the Pyralis Authoring Window.

Read this when changing the Authoring Window, setup guidance, route validation, route scaffolding, scene-surface scanning, or Inspector handoffs. Use `AUTHORING_MODEL.md` when you need the asset relationship map behind the window.

## Documentation Role

Pyralis docs should stay current and purposeful:

- `README.md` files orient the reader and name the right next document.
- `START_HERE.md` teaches the first human setup path.
- `AUTHORING_BLUEPRINT.md` owns Authoring Window product direction, UX rules, graph projection, tab ownership, and maintenance rules.
- `AUTHORING_MODEL.md` owns the asset/runtime relationship map behind the window.
- `CANONICAL_SETUP.md` owns the technical setup contract.
- `AUTHORING_MODEL.md` owns asset/profile/runtime relationships and compact route-capability vocabulary.
- `FEATURE_DEVELOPMENT_ROADMAP.md` owns route-completeness sequencing.
- `CURRENT_STATE_AUDIT.md` owns the present platform state and highest-priority risks.

When docs disagree, prefer `AGENTS.md`, the package `README.md`, this blueprint for Authoring Window behavior, and the current code/tests for implementation truth. Update or delete stale guidance instead of preserving conflicting paths.

## North Star

The Pyralis Authoring Window should feel like a calm senior Unity teammate beside the developer.

Its job is not to replace the Inspector, build a whole scene, choose game design, or hide Unity. Its job is to keep setup understandable while the developer keeps moving.

**Explicit Authoring over Presets:** One of our core rules is to avoid "Autospawning" or "Presets." The engine should never create GameObjects behind the user's back to fix a validation error. Instead, it guides the user to the native Unity step (Hierarchy > Create) to maintain a concise and predictable codebase.

Short version: Authoring is the map, Unity is the workshop, and Inspectors are the local knobs.

The window guides a selected first proof, not a complete game setup. It can say whether setup is ready to attempt, but static discovery must not claim that Play Mode behavior has passed.

At any point, the window should answer:

1. What am I building?
2. Where am I in the setup chain?
3. What is blocking me?
4. What should I do next?
5. Where do I edit that thing?
6. What can I safely ignore for now?

The window is the guided setup surface. It should not add a beginner/advanced toggle. Guided users need clear next steps and safe defaults; power users can work directly in the Inspector, scenes, prefabs, code, and manual asset paths when they do not need the window.

## Authoring Standard

Authoring should be beginner-legible and pro-trustworthy.

Beginner-legible means the window and guides use concrete Unity language: create objects in the Hierarchy or Project window, add components through Inspector Add Component search, assign fields in the Inspector, save the scene before Play Mode, and run one small proof before expanding the route.

Pro-trustworthy means the system does not pretend missing runtime wiring is a lesson. If a runtime component, route validator, service registration, prefab, profile, or authored asset needs a reference to work, the product should supply or diagnose that reference directly. Do not add guide text that asks the user to compensate for hidden setup debt unless the missing choice is genuinely project-specific.

Guidance should stay reactive to the selected route and the developer's customization:

- explain the current grammar vocabulary and reflected contracts, not a hard-coded genre path
- show which links are required, recommended, optional, or not needed for this route
- point to the Inspector field or Unity object where the developer expresses taste
- point missing asset-chain gaps to native Project-window Create paths and Inspector assignment fields
- leave scene layout, art, tuning, controller feel, UI composition, level design, and feature selection in the developer's hands
- keep future scaffold/template tooling downstream of proven contracts, facts, validation, and live proof

When a test finds friction, classify it before fixing:

| Finding | Product response |
|---|---|
| Missing required reference, invisible prefab, null profile, or runtime service assumption | Fix code, validation, or authored asset shape |
| User must choose art, tuning, level shape, route capability, input device policy, or UI composition | Guide the Unity workflow and name the relevant fields |
| Optional system appears required before the first proof | Move it to Proof Enhancers or Optional Capabilities |
| Inspector duplicates route instructions | Keep compact field-local help and hand off to Authoring |
| Authoring loses context when selecting child assets or prefabs | Improve active-route inference, pinning, and "you are here" language |

The Authoring Window is also for experienced users. Its bar is not "can a beginner follow it eventually"; its bar is "would a Unity pro trust this route diagnosis, know exactly what changed, and still feel free to build the game their way."

## Product Contract

The Authoring Window is the central setup service for Pyralis.

The Inspector remains the direct field editor. The Authoring Window reads the current route, diagnoses readiness, explains selected context, and recommends the next useful move by naming the native Unity action and Inspector field to use.

Scene, prefab, and asset authoring must stay guidance-first. Unity scene objects and authoring assets often need project-specific folders, names, components, layers, art, input modules, camera choices, and designer intent that Pyralis should not hide. The active path is native Unity authoring: create definitions, profiles, prefabs, components, and scene objects yourself while Authoring Window cards audit, explain, show evidence, describe success, and point to the right Unity object, Create path, Inspector field, or checklist.

Do not turn scene-surface warnings into hidden one-click creation flows. The author owns the scene; Pyralis makes the setup chain legible.

## Guidance And Enforcement Boundary

Pyralis authoring should guide supported routes strongly without bounding normal Unity experimentation.

Outside the active Pyralis setup, ordinary Unity objects, scripts, scenes, assets, prefabs, art, UI, and experiments should remain valid Unity work. The Authoring Window can call them outside the active setup, found-but-not-linked, or not needed for this route, but it should not treat them as Pyralis errors unless they are wired into a Pyralis setup or Unity itself reports a native error.

Inside the Pyralis setup, the system can be strict about real contracts. If a developer wants a feature, component, route, prefab, profile, or asset to become reusable Pyralis library work, it must attach back to the shared contract/fact/proof model: stable ids, native setup actions, supported lanes, validation issues, required components or fields, first proof target, tests, and docs when the workflow changes.

Power users can bypass the Authoring Window while they experiment, but reusable Pyralis features should not become invisible side systems. The graduation path is: build freely in Unity, then connect useful work back into the Pyralis authoring spine before calling it authoring-ready.

## Export Footprint Boundary

Authoring should make setup intelligence visible in the Unity Editor without making exported games carry that editor weight. Authoring providers, grammar registries, validators, custom inspectors, proof tooling, live-test notes, and Authoring Window UI belong in `Editor` folders or editor-only assemblies unless there is a deliberate runtime reason to ship them.

Runtime code should stay modular by route. Route-specific work should not add broad always-loaded references that pull unrelated systems or large assets into a player build. Scene, prefab, ScriptableObject, `Resources`, Addressables, and bootstrap references are the export-size boundary to watch: if a route does not use a system or asset, it should not be referenced by that route's runtime setup just because the authoring model knows the system exists.

Contracts and facts are useful here because they describe setup, validation, native Unity actions, and proof targets without becoming runtime dependencies. Keep that separation intact. A later route-promotion gate should inspect Unity build reports for representative route builds and flag unexpected editor assemblies, unrelated runtime modules, or large unused assets.

## Current Implementation Shape

Keep the implementation split by responsibility:

| Owner | Responsibility |
|---|---|
| `PyralisAuthoringWindow` | UI shell, active setup state, selection, and mode coordination |
| `PyralisAuthoringRouteDescriptor` | route facts inferred from authored setup, graph vocabulary, and selected context |
| `PyralisAuthoringOverviewModel` | Overview read model for graph-projected lanes, first proof text, and Play Mode checklist |
| `PyralisAuthoringCapabilityDescriptorRegistry` | route capability descriptors built from contracts, dependency evidence, and reflected metadata |
| `PyralisCapabilityVocabulary` | generic capability labels, summaries, and native setup wording indexed by capability and runtime lane |
| `PyralisProofFamilyVocabulary` | generic proof family templates |
| `PyralisAuthoringIntentAdvisor` | pre-setup read model that ranks route-intent and graph-compatible vocabulary from selected world/playfield, control shape, lane, and goals |
| `PyralisAuthoringSetupGraph` | read-only resolved graph of setup nodes, edges, evidence, proof targets, selected context, and source contracts |
| `PyralisAuthoringSetupGraphProjection` | Map, Overview, Guide, Hygiene, reflective-contract, and selected-context projection rows derived from the resolved setup graph |
| `PyralisAuthoringSceneSurfaceGuidance` | scene-surface labels, route relevance, next-fix text, expected evidence, and success text |
| Bootstrap Inspector handoff | selected-object local integrity and a button into the graph-backed Authoring Window |
| `PyralisSceneReadinessValidator` | concrete scene-object hygiene: missing scripts, scene services, native Unity input/UI/audio/camera/network surfaces, and scene-visible proof blockers |

Add route facts, issue meaning, and setup analysis to these focused model/report classes before adding more drawing logic to the window.

The window surface is UI Toolkit-only for non-Intent tabs. `PyralisAuthoringWindow` owns the tab shell, active setup cache, selection, and mode switching. Tab-specific projection packets are built in `PyralisAuthoringSetupGraphProjection`, rendered through the shared UI Toolkit tab renderer, and exported by the same export control. Do not add new IMGUI tab renderers or tab-local data discovery paths.

The active guidance pipeline is:

```text
Gameplay code and authored assets
  -> feature contracts + reflected dependency tree + validators + grammar vocabulary
      -> resolved setup graph
          -> Map, Overview, Guide, Hygiene, Facts, selected context, inspectors, and docs
```

Do not store the same route advice separately in multiple windows, inspectors, validators, or docs. If code structure proves it, reflect it. If humans need meaning, put it in a contract. If readiness changes, project it through graph evidence. If wording is generic, put it in Grammar/Vocabulary and let visible surfaces render from graph projections.

Fallback policy is strict: grammar fallback can label or phrase something, but it cannot make a contract selectable, assign a semantic capability path, add route-essential role tags, choose a runtime family, or make a proof look feature-owned. A missing `CapabilityPath`, route role tag, or feature-owned proof target should become `ContractMetadata.*` graph evidence and a Hygiene row. Runtime-family metadata is different: if a family can be inferred from reflected type shape, implemented interfaces, required components, or concrete runtime-surface evidence, Hygiene should classify it as reflection-owned instead of asking the contract to duplicate it. Do not recover by parsing namespaces, display names, setup node ids, or generic vocabulary cards into behavior.

Behavior-affecting projection decisions must use typed graph metadata, not display text. Graph nodes should carry stable `IssueCode`, `SetupDomain`, node kind, source kind, source origin, work intent, severity, capability family, contract role tags, dependency evidence, validator fields, and native actions. Overview, Guide, Route Proof Trace, Map, Hygiene, Facts, and JSON exports may render prose, but they must not rank, filter, de-duplicate, or classify route work by searching labels or guidance sentences. Tab projections should consume the shared projection metadata resolver for group, audience, route phase, owner domain, and sort rank; do not add tab-local source-kind, source-origin, id-prefix, or label parsing as readiness truth.

Contracts may declare `CapabilityPath`, `RoleTags`, `OwnershipClaims`, `RuntimeFamilies`, and `SelectableIntent` when reflection needs stable semantic grouping for Intent, Guide, Route Proof Trace, Hygiene, or Facts. These fields are routing and ownership meaning, not setup prose. Prefer them over hardcoded Intent categories or source-shape ownership guesses, but do not use them to restate interfaces, required components, serialized fields, dependency order, or runtime-family facts that reflection/dependency evidence can already prove.

Intent may also declare a desired participant route shape, such as solo local or two local players. Treat this as a graph filter for planning the Guide/Overview route before setup exists. It should never apply a preset or mutate scene content. Concrete participant counts, join policy, spawn policy, and missing fields still come from `SessionDefinition`, `ParticipantDefinition`, `ParticipantInputRouter`, `PlayerInputManager`, `ParticipantSpawnService`, dependency reflection, and validators once the user authors the setup. Route analysis should keep authored participant count, desired Intent participant count, and effective route count distinct; Guide can explain mismatches, while Map remains honest about the current authored setup.

Intent's visible descriptor surface should not feel like a raw code inventory. Project reflected descriptors through two tab/export lenses: **Gameplay Ingredients** for goals and optional gameplay behavior the creator is steering toward, and **Route Essentials** for setup/session/participant/input-router/spawn infrastructure that may become required once the route shape is chosen. Both lenses read the same descriptor registry; do not build a second intent mapping table. Only Gameplay Ingredients are user toggles and selected descriptor ids. Route Essentials are read-only inferred rows derived from selected ingredients, DNA, lane, and participant route so infrastructure cannot masquerade as chosen gameplay intent. Infer essentials from exact route-support role tags plus selected capability overlap, DNA, lane, and participant route shape; broad capability overlap or display-name matching alone should not pull in menus, loading screens, network services, combat definitions, or optional feature services.

Missing serialized setup references should enter the graph as reflected `AssignmentField` evidence before route-facing prose describes the same gap. For example, an empty `ParticipantDefinition.inputProfile`, `PawnDefinition.pawnPrefab`, `PawnDefinition.movementProfile`, or `GameModeDefinition.cameraRigProfile` should produce a graph node that names the owner type, field, expected asset/component type, native Inspector action, severity, and proof relevance. Core route nodes describe the ownership spine; they should not duplicate field-level assignment cards that reflection can already prove.

Route-facing projections may translate low-level structural evidence into concrete lane setup actions. For example, Hygiene can expose that a pawn prefab is missing an `IPawnMotor`, but Overview, Guide, and Route Proof Trace should render the `Sprite2D` route as `Add Motor2D` because that is the native Unity action the user should take. Do this translation in graph projection, not in tab renderers or JSON exporters, so the cockpit, guide, and trace stay aligned while Map/Hygiene preserve developer evidence.

Intent lane is compiled graph context, not disposable UI state. When setup is incomplete, the graph may not yet have a `Motor2D`, `Rigidbody2D`, `Pawn3DMovementComponent`, or other concrete prefab evidence to infer the lane from. In that case route-facing projections should fall back to the selected intent lane so a `Sprite2D` proof still says `Add Motor2D` instead of generic lane/interface wording. Concrete prefab evidence can refine or challenge the lane later, but incomplete setup should not erase the creator's selected route.

Route shape is control ownership, not pawn implementation. The `route.shape` node should answer whether the route has the correct owner structure: participants exist, and pawn-backed routes have a participant default pawn. It should not own `PawnDefinition.pawnPrefab`, `ParticipantDefinition.inputProfile`, lane motor, input adapter, presentation, animation, or feature-module setup. Those belong to reflected assignment fields, prefab/component evidence, validators, and route-facing pawn setup cards.

Proof nodes are proof targets, not setup bundles. A first-proof node should explain the Play Mode test and the success signal. It should not absorb every native setup action, assignment field, or customization moment from supporting contracts. Put setup work before the proof as graph evidence; keep the proof card small enough that it reads as "what will prove this works."

## Authoring Information Flow

The Authoring Window is a graph projection surface. The graph is the single compiled view of setup truth; every tab should read that compiled view or a projection derived from it.

```text
Gameplay code / feature code
  -> AuthoringContract metadata
  -> Unity metadata and reflection
  -> serialized asset references
  -> open-scene evidence
  -> validation evidence
  -> grammar wording
      -> resolved setup graph
          -> Overview
          -> Intent
          -> Guide
          -> Map
          -> Hygiene
          -> Facts
          -> Inspector handoffs
```

| Input | Owns | Should not own |
|---|---|---|
| Gameplay code | runtime behavior, interfaces, components, profiles, definitions, feature modules | authoring prose that belongs in contracts |
| `[AuthoringContract]` | feature meaning, capability, runtime families, semantic path, role tags, relevance, proof target ids, proof guidance, native setup meaning, customization moments, unsupported lane messages | code-proven requirements that reflection can infer |
| Reflection | implemented interfaces, `[RequireComponent]`, serialized fields, `CreateAssetMenu`, `AddComponentMenu`, feature/profile links | human meaning, route taste, or proof copy |
| Dependency tree | setup/reference structure: bootstrap, session, mode, setup route, participants, pawns, prefabs, profiles, feature modules | UI labels or route guidance text |
| Scene evidence | bootstrap-scoped scene components, typed scene-surface detector results, candidate objects, linked setup state, and hygiene notes for untyped name matches | proof results, broad scene-quality policy, or feature-surface truth inferred only from type names |
| Validators | local semantic readiness, blockers, invalid combinations, severity, native action targets | persistent feature meaning, grammar wording, or references reflection can prove |
| Core route domains | graph/compiler milestones: root, lifetime scope, session, mode, route capabilities, participant topology, and join policy | feature-specific checklists, field assignments, pawn/input/spawn/camera/playfield setup, customization, or scene/prefab aggregate gates |
| Grammar/vocabulary | labels, summaries, generic proof templates, native Unity surface names | feature-specific setup truth or route decisions |
| Resolved setup graph | compiled readiness, proof targets, nodes, edges, evidence, selected context, source provenance | raw scanning or user-facing drawing details |
| Tab projections | view-specific packets for Overview, Guide, Map, Hygiene, and Facts | new route truth, validation truth, or export-only truth |
| UI tabs | projection, ranking, filtering, navigation, explanation | route truth, validation truth, or feature truth |

Runtime validation providers must emit `PyralisRuntimeValidationIssue` records, not free-form setup prose. Use reflection and the dependency tree for object references, serialized fields, required components, implemented interfaces, and assignment paths whenever code structure can prove them. Use runtime validation providers only for semantic rules that reflection cannot infer, such as "this numeric value cannot be negative," "this action row is required for this route," or "this feature profile must match the selected module contract." Each issue should carry a stable issue code, target label, native action, severity, success check, and affected field when the issue maps to an Inspector field so Overview, Guide, Map, Route Proof Trace, and Inspector handoffs all read the same graph evidence. Compact asset-local `GetValidationIssues()` helpers may exist only as implementation detail; graph-facing evidence still needs structured `PyralisRuntimeValidationIssue` records with deterministic identity instead of anonymous prose. `InputProfile`, for example, owns Input Action Asset, action-map, gameplay-row, and supported-device validity; core setup graph nodes and scene readiness should not duplicate that action-map logic. Do not classify runtime validation by parsing its message text; prose is rendering, while `IssueCode`, `FieldPath`, severity, source object, and dependency evidence are graph identity. Hygiene should surface runtime validation evidence missing stable metadata as a cleanup bucket.

Feature profiles and definitions should propagate validation for semantic child assets they own. For example, `PawnDefinition` owns pawn prefab composition readiness, combat profiles surface their assigned weapons and sequences, weapons surface projectile definitions, and projectile definitions validate their own projectile prefab contract. The authoring spine should not know how to walk pawn prefab internals, combat, RPG, tabletop, scoring, or hazard internals; it should consume the owning asset's `PyralisRuntimeValidationIssue` records and reflected dependency fields.

Scene readiness is intentionally narrow. It may inspect concrete scene surfaces that only exist in the open scene, such as missing scripts, core service object presence, `PlayerInputManager`, `EventSystem`, scene `AudioListener`, scene `NetworkManager`, and native Unity scene setup contradictions. It must not relay every scene `IRuntimeValidationProvider`, maintain a hardcoded list of feature services, inspect pawn prefab internals, or validate feature-module internals. If a scene component, prefab definition, profile, or feature module needs a setup warning, put that warning on the owner, then let dependency-tree runtime validation evidence and graph projection carry the provider record.

Core setup graph nodes are not a general feature validator. They may describe the route spine and first-proof gate, but feature-domain readiness belongs in feature-owned contracts, reflected assignment fields, scene evidence, prefab evidence, or local runtime validation providers. If a route row starts naming a feature service, presenter, projectile, board widget, score surface, RPG panel, settings menu, pawn prefab requirement, input profile action map, camera rig tuning, playfield profile, or spawn-point detail, move that evidence back to the script, contract, dependency reflection, scene/prefab evidence, or graph projection that owns it.

Route ordering is graph metadata, not English. Setup categories such as gameplay root, lifetime scope, session, game mode, route capabilities, participant topology, input, pawn definition, pawn prefab, pawn motor, spawn, camera, presentation, scene readiness, UI, settings, scoring, tabletop, networking, and feature contract should travel as `PyralisAuthoringGraphSetupDomain`. Exports should include that setup domain so humans and agents can tell whether a projection bug is upstream graph tagging or downstream rendering.

JSON exports mirror the same projections the tabs render. Intent export should describe route steering only; Map export should describe current setup reality; Hygiene export should describe graph/code audit pressure with only passive graph context; Guide's Route Proof Trace export should describe the Guide route from current action through proof; Facts export should describe the compiled dictionary/cookbook and provenance. Do not add separate export-only truth, and do not let an export leak another tab's ownership lane. If an export is wrong, fix the shared graph, projection, validator, contract, or grammar source that produced it.

The intended developer workflow is:

1. Add or edit gameplay code.
2. Add or edit the feature-owned contract when humans need meaning.
3. Let reflection and dependency-tree discovery pick up code structure.
4. Let validators emit readiness evidence.
5. Let the graph synthesize readiness and proof targets.
6. Let tabs render graph projections.

If adding a feature requires editing an Authoring Window renderer, graph projection, validator, grammar vocabulary, and a contract all at once, stop and classify why. Most feature additions should need only gameplay code plus contract metadata unless they introduce new spine grammar.

### Cleanup Closure Criteria

This refactor is complete when cleanup work stops finding new operating models and only finds missing feature contracts or missing validators.

Lock-in requires:

- Map, Overview, Guide, Hygiene, Facts, selected context, and Inspector handoffs read graph projections instead of parallel route/fact/proof models.
- Intent behaves as a graph and capability filter, not a recipe or preset system.
- Feature-specific setup guidance lives in contracts or reflected feature metadata.
- Grammar/vocabulary contains generic wording and studio-wide Unity/Pyralis language only.
- Scene evidence is collected through shared evidence models before reaching graph evidence or scene-surface rows.
- Validators preserve severity, affected object, affected field/component, native action, and success criteria when they feed graph evidence.
- Proof nodes are generated from contracts, dependency evidence, and validators first; proof vocabulary supplies only generic wording.
- Temporary cleanup-only files should be archived with removal criteria or deleted once graph-backed parity is proven.

Use this test for every cleanup candidate:

```text
Is this Pyralis grammar, feature-owned meaning, code-proven structure,
scene evidence, validation evidence, graph synthesis, or UI projection?
```

If the answer is unclear, the code probably has a boundary problem. If the answer is "UI projection," the file should not be discovering route truth.

## Core Setup Chain

The Authoring Window should keep this chain visible whenever it can infer it from the current selection:

```text
GameplaySessionBootstrap
  -> SessionDefinition
      -> GameModeDefinition
          -> reflected route capabilities
              -> grammar vocabulary
      -> ParticipantDefinition[]
          -> PawnDefinition optional
              -> pawn prefab
              -> profiles
              -> feature modules
```

The user-facing Map chain is:

```text
Gameplay Root
-> Session Definition
-> Game Mode
-> Setup Route
-> Capabilities
-> Participants
-> Pawn Setup
-> Scene Surfaces
```

Each row should show whether the route needs that link, whether it is ready, the current object or missing field, why it matters, and where to inspect next.

Pawn-backed routes should ask for a `PawnDefinition`, pawn prefab, movement/presentation profiles, and relevant spawn/camera surfaces. No-pawn routes such as tabletop, board, card, camera, cursor, menu, or faction routes should explicitly say that empty pawn fields are correct.

## Information Model

Move the window toward structured authoring facts that every mode can consume.

Core concepts:

- `Route`: the inferred game surface, such as pawn action, tabletop, action/menu, camera/cursor, scoring, networking, or a hybrid route.
- `Setup Node`: one row in the authoring chain, such as bootstrap, session, game mode, setup route, route capabilities, participants, pawn/no-pawn state, and scene surfaces.
- `Issue`: required, recommended, optional, blocked, or not-needed setup work.
- `Action`: inspect, create, assign, repair, explain, copy checklist, or open documentation.
- `Evidence`: why the tool believes something is ready, missing, optional, or not needed for this route.
- `Work Intent`: whether the row is foundation setup, required setup, a proof enhancer, or an optional capability.

The active authoring foundation is the contract/dependency-tree/graph pipeline. `PyralisAuthoringGrammarRegistry` aggregates vocabulary, reflected facts, proof templates, inspector handoffs, route intents, scene evidence, and convention facts so projections have stable ids and wording. It is an audit and grammar source, not the primary operating model. Feature-owned `[AuthoringContract]` metadata owns semantic setup meaning; `PyralisSetupDependencyTree` owns serialized setup/reference discovery; validators own local semantic readiness; `PyralisAuthoringSetupGraph` compiles those inputs into the single readiness/proof model consumed by tabs. `PyralisAuthoringIntentAdvisor` projects grammar facts into compact pre-setup planning. Intent filters the desired route shape, the graph compiles the actual participant/pawn/no-pawn ownership shape, Guide owns the graph-filtered route guide when setup exists, and Facts remains the full cookbook and dictionary.

The grammar and graph inputs should grow in this order:

1. grammar/vocabulary facts that preserve product voice - implemented for capability vocabulary
2. core setup graph nodes and native actions - implemented in graph compiler grammar and native-action vocabulary
3. route proof facts that relate capability cards, setup nodes, and first Play Mode proof - expanded for pawn, tabletop/card, action selection, NPC/enemy, custom object, UI/HUD/menu, camera/world, generated content, and networking proof anchors
4. typed validator issues with stable issue codes - started through the graph-backed Hygiene model and visible metadata block
5. inspector handoff facts for selected field/component guidance - expanded for core setup, 2D pawn/input, tabletop rules, camera/playfield, feature modules, Cinemachine camera fields, tabletop presenter fields, and camera framing customization
6. read-only Fact Explorer views that show provenance, confidence, and missing coverage - started as the Authoring Window `Facts` tab
7. read-only reflection/convention facts from Unity metadata such as `CreateAssetMenu`, `AddComponentMenu`, `RequireComponent`, and serialized fields - expanded across core setup, pawn, tabletop, action, camera, UI, custom feature, NPC/enemy, combat, projectile, and feedback surfaces
8. route-family coverage facts for broad authoring surfaces - started for pawn, NPC/enemy, custom object/feature, UI/HUD/menu, world/camera, tabletop/card, and networking routes
9. optional beginner semantic location tags - started as a top Authoring Window legend and generated fact/action badges
10. scene-evidence facts that connect route proof targets to existing scene-surface guidance rows - started for the current six Authoring Window scene surfaces
11. route-intent facts that let Intent shape the project route and Guide rank route families, contracts, cautions, and first proof targets from lane and capability toggles - started for side-view action, pawn brawler, and camera/cursor command

Reflection and convention discovery must explain and audit before it drives user guidance. Convention-derived facts should carry lower confidence until an explicit metadata attribute, validator, or manual Unity proof confirms the claim.

The only intentionally hard-coded beginner vocabulary should be studio-wide Unity/Pyralis location vocabulary: Project, Hierarchy, Inspector, Add Component, Prefab, Definition, Profile, Input, UI, Animation, Audio, Authoring, and Play Mode proof. Route-specific truths should arrive through facts, contracts, providers, validators, and metadata. New route docs or providers should not need new Authoring Window branches just to receive the same colors, tooltips, grouping, and location language.

Route-family facts are coverage anchors, not full setup completion. A route family can be named in the registry before it has complete inspector handoffs, convention coverage, scene evidence, validation issue coverage, or manual Play Mode proof. The Facts tab should make that state visible so future work expands intentionally instead of only deepening the first 2D pawn route.

Route proof facts are also proof targets, not proof results. A `proof.*` fact means the Authoring Window knows what the smallest useful Play Mode proof should be, what can wait, and which Unity surfaces are involved. It does not mean that a Computer Use walkthrough has built the scene or observed the proof. The next live Unity validation pass should use these proof facts as the checklist.

Semantic location tags are a beginner overlay, not a second authoring model. Do not hand-color prose. Add or infer tags from facts, native action surfaces, fact kinds, Unity metadata, and the shared studio-wide Unity vocabulary, then let the Authoring Window render the shared palette. The top legend is only shown when Beginner Location Tags is enabled. Inline guidance text should use the shared semantic renderer so future facts and docs inherit the same beginner location cues without hard-coded route prose.

Authoring must keep three truths separate:

- `Intent`: what the user is trying to build.
- `Evidence`: what Pyralis can see in assets, prefabs, and scenes.
- `Proof`: what has actually been attempted or passed in Play Mode.

Scene-surface rows should use cautious evidence states: `Missing`, `Found candidate surface`, `Linked to active setup`, `Validated`, and `Play-proven`. A found collider, camera, Canvas, or presenter is not the same as a passed route proof.

Every row or issue should eventually have:

- status
- reason
- target object when one exists
- affected field or component when known
- next action when safe
- evidence text that explains the diagnosis

Prefer durable issue categories or codes over keyword grouping. Text can change; issue meaning should not.

Core setup graph nodes should carry stable ids and native action metadata. The display label is for humans, not for routing behavior. Overview, Map, Hygiene, tests, and future feature contributors should prefer node ids, setup domain, work intent, and `PyralisAuthoringNativeAction` over string-matching labels or message text. Message text can still specialize a row, but it should not be the only source of meaning.

Recommended setup should not all land in the same visual priority. Use work intent to keep the flow calm:

- `Foundation`: useful visibility or defaults that keep the scene inspectable.
- `RequiredSetup`: missing links that block the named first proof.
- `ProofEnhancer`: customization or scene support that makes the first proof believable, readable, or easier to debug.
- `OptionalCapability`: optional systems, advanced route capabilities, polish, or next proof-chain work that should not compete with the current proof.

## Capability Vocabulary

The Capability Vocabulary is a guide-only discovery layer inside the Authoring Window. It lets users browse Pyralis-supported setup surfaces by capability and runtime lane without turning the window into a scene generator or preset system.

The vocabulary should use one canonical card model, not separate hardcoded trees. A card is indexed by capability tags such as movement, combat, projectiles, camera, UI/HUD, scoring, interaction, NPCs/enemies, tabletop, and networking, and by runtime-lane tags such as `Sprite2D`, `Billboard2_5D`, `Rigged3D`, tabletop/no-pawn, UI/menu, camera/cursor, and networked. Both browse modes render the same cards so generic wording does not drift.

Each card should answer:

- what this capability adds
- when to use it
- required definitions and profiles
- required scene and prefab components
- assignment fields
- customization moments
- what can wait
- first proof
- common next capabilities

Capability vocabulary cards are generic wording. The graph-facing capability surface is `PyralisAuthoringCapabilityDescriptorRegistry` in `Spine/Routes`, which prefers contracts, dependency evidence, and reflected metadata before filling generic labels and summaries from vocabulary. Feature-specific setup truth should move into feature contracts, dependency evidence, or fact projection rather than new hardcoded card prose.

The vocabulary must stay guide-only. It may select, ping, explain, or copy checklist text, but it must not create or assign assets, add components, or treat generated scaffolding as validation evidence. Users should still use native Unity surfaces: Project window asset creation, Hierarchy object creation, Inspector Add Component, Inspector field assignment, object picker, customization through serialized fields, and Play Mode proof.

The same contract/dependency-tree/graph model should expand beyond route capabilities into the whole setup path: session setup, participants, pawns, NPCs/enemies, custom interactables, pickups, hazards, camera/world bounds, UI/HUD/menus, scoring/objectives, tabletop/control surfaces, and networking.

## Mode Responsibilities

### Intent

Intent is the starting surface when no Pyralis setup context is selected or inferred. It should ask what kind of game the developer is trying to build before the user is pushed toward bootstrap, participant, pawn, prefab, scene, input, camera, or Play Mode details.

Intent should stay studio-wide:

- use world/playfield dropdowns, control-shape dropdowns, lane choices, and capability toggles as the route-shaping contract
- rank route families, contracts, cautions, and first proof targets from registry facts
- react to side-view 2D gravity, top-down/free 2D, 2.5D lane/arena, 3D, tabletop/no-pawn, card/table, UI/menu, camera/cursor, hybrid, and networking lanes
- explain what the toggles imply without creating assets or choosing design taste
- hand off to Project, Hierarchy, Inspector, Prefab, Input, Animation, UI, and Play Mode surfaces only when the route has enough declared intent
- show selectable-but-incomplete feature contracts in a read-only **Needs Contract Metadata** backlog when they match the current lane/axioms but are missing `CapabilityPath` or `RuntimeFamilies`; do not hide that capacity, and do not let it steer routes until the contract metadata is complete

Intent is not the whole setup route and not the proof itself. It names the project-wide world/control/capability shape so the developer knows what to wire next. Overview and Guide read the Intent-projected graph so the working path follows the creator's selected focus. Map reads the current setup graph so it reports only what exists in the scene/assets. Hygiene stays route-passive: it audits graph integrity, source pressure, dependency pressure, and ownership hotspots without becoming an intent-aware setup guide.

### Overview

Overview is the daily home base. It should show:

- route name
- blocking status
- one best next action
- Active Setup state
- concise readiness summary
- first playable proof target

Overview reads the Intent-projected graph, then renders the first few cards from the shared route working projection. It should feel like the compact cockpit for the current setup: one best next action, up to three `Do Now` cards, proof enhancers, and the first proof target at the bottom as the Play Mode test to run after `Do Now` clears. It should not separately scan graph nodes or maintain its own route ranking.

Organize progress into three lanes:

- `Do Now`: route-required missing or blocked work only.
- `Proof Enhancers`: setup recommended by the selected intent that can make the first proof easier to read but should not block it once `Do Now` is clear.
- `Optional Capabilities`: optional next capabilities, polish, advanced systems, and nice-to-have proof.

If the developer is tired or lost, Overview should still make the next move obvious.

### Solid Flow Test

Cameron should be able to test the Authoring Window flow with one active route without reading code:

1. Open `NeonBlack/Gameplay/Pyralis Authoring Window`.
2. Select or pin one `GameplaySessionBootstrap`, `SessionDefinition`, `GameModeDefinition`, or `SessionDefinition`/`GameModeDefinition` route.
3. Confirm Overview names the route and shows `Flow Test Status`.
4. Clear `Do Now` items before trusting Play Mode.
5. Use `Proof Enhancers` only when the first proof would be hard to read without them.
6. Leave optional capabilities alone unless the current proof specifically needs that capability.
7. Use `Inspect Best Target`, `Open Map`, and `Open Hygiene` to move between object inspection, dependency mapping, and issue triage.
8. Press Play only for the first proof named by Overview.

The flow is solid enough for product testing when a route can move from `Do Now` to a first proof without the user asking which object to inspect next.

### Guide

Guide explains the current Unity selection without losing the active setup story. It should answer:

- what this selected thing is
- why it matters in the active route
- what edits belong in the Inspector
- what to check after editing

Guide uses the same Intent-projected route working projection as Overview and Route Proof Trace, but expands the cards instead of showing only the top few. It can show the full fresh-scene path, selected-object context, and reflective contracts. It should not maintain its own route ordering or proof-bucket logic.

Guide should speak in concrete Unity steps for the selected lane. If graph evidence says a prefab lacks a required runtime interface, Guide should name the component or field the user adds for the active route, such as `Motor2D`, `Motor2DInputAdapter`, `Pawn2DPresentationComponent`, or the matching 3D lane component. Interface names are developer evidence, not beginner route copy.

### Map

Map is the current scene/setup reality map. Each row should show:

- ready, missing, blocked, recommended, optional, or not needed
- current object or missing field
- the native Unity Create/Add Component/Inspector step when the link is missing
- detected scene surfaces and selected-object wiring
- concrete field, component, prefab, scene root, or asset issues

Map should teach what currently exists and what concrete Unity setup is missing. It can read graph evidence, but it presents scene and setup issues rather than graph integrity.

Map rows should only inherit evidence that belongs to that row. Broad graph edges can relate `Gameplay Root` to scene surfaces, services, and setup cards, but a missing Canvas, pickup, hazard, or other scene surface must not demote the `Gameplay Root`, `Session Definition`, or `Game Mode` row. Put scene-surface failures under `Scene Surfaces` and `sceneSetupIssues`; put root/session/mode/pawn/input assignment failures on their matching setup rows.

Map may offer a compact read-only **Export Map JSON** button for human and agent diagnostics. That snapshot must serialize the current setup lens only: current authored route analysis, Map-owned graph nodes and edges, setup map rows, map connections, scene surfaces, and concrete scene/setup issues. It must not include Hygiene-only sections, contract metadata inventory, grammar-shaped audit nodes, or source-pressure findings; it must not become a second setup model; and it must not pretend Intent-selected desired work already exists in the scene. The export action writes the current tab view into `Editor/Authoring/TempGraphs` so issue reports and agent handoffs have one predictable diagnostic folder while generated JSON remains ignored.

### Intent Export

Intent may offer a compact read-only **Export Intent JSON** button. This snapshot serializes the steering lens only: DNA axioms, presentation lane, participant route, selected capability descriptor ids, reflected descriptor groups/subgroups, metadata backlog groups, runtime-family semantics, advisor summary, recommendations, cautions, and matching intent facts. It must not include Map setup rows, Hygiene audit rows, scene repair issues, generated setup content, required setup lists, assignment fields, or native action payloads. Intent export exists so humans and agents can inspect what the creator asked the route to focus on, and which potential ingredients are waiting on contract metadata, before judging whether Guide and Overview are honoring that focus.

### Hygiene

Hygiene should be graph integrity, not a second scene checklist. Evidence cards should include:

- severity
- stable graph node id
- evidence state
- source kind and source origin
- blocker/proof relationship when known
- graph finding and source detail

**Duplicate Owner Detection:** Hygiene is responsible for projecting competing ownership paths. Contracts that canonically own a runtime responsibility should declare a stable `OwnershipClaims` value such as `participant.spawn` or `pawn.movement.motor`. When different contracts declare the same ownership claim, the graph emits `ContractMetadata.DuplicateOwnershipClaim` evidence and Hygiene surfaces it under Missing Contract Metadata. Source dependency pressure can still warn that a file shape looks ownership-heavy, but duplicate-owner findings must come from contract/reflection graph evidence instead of filename, namespace, or display-text guessing. This prevents "systems on top of systems" and maintains a concise codebase.

Hygiene can mention source detail, but concrete Unity repair and inspection actions belong in Map or the Inspector. This keeps Hygiene useful for developers auditing the resolved graph without making it compete with the beginner route guide. Map-owned readiness nodes include setup-chain, scene-surface, Unity-surface, core-setup, scene-readiness, and runtime-validation evidence; Hygiene should filter those from its audit cards and JSON blocker rows instead of restating them as graph failures.

Hygiene may offer a compact read-only **Export Hygiene JSON** button, with the view marked as `Hygiene`, so developers and agents can inspect graph summary, Hygiene sections/rows, graph-owned proof blockers, source-origin counts, dependency pressure summaries, cleanup focus, watch-list pressure, top dependency-pressure records, and contract source pressure outside the Unity UI without scraping visible text. It should serialize the same `PyralisAuthoringHygieneProjection` the tab renders; it must not rescan package source, rebuild sections, or choose cleanup/watch rows inside the exporter. It should not include Map-only setup rows or Map-owned scene/setup repair blockers. Hygiene remains useful before a setup route exists: graph-specific sections may be empty, but dependency pressure and source audit data should still export. Contract inventory that has not been route-evaluated should be labeled as inventory, not as actionable graph failure. Graph summary counts should separate setup/integrity state from contract metadata and contract inventory pressure so missing contract fields do not read as broken scene setup. Missing contract metadata rows should include the stable issue code, a triage bucket, ownership bucket, repair owner, and short advice so exported JSON distinguishes Intent metadata decisions, route-essential paths, runtime-family decisions, proof-target gaps, duplicate ownership claims, reflection-inferred families, and support-only contracts. Contract source pressure should expose declared runtime families separately from inferred runtime families so agents can clear policy overreach without stuffing redundant data into contracts. Builder guidance should derive examples from the contract capability and role tags rather than suggesting one hardcoded movement path for every domain. `hygieneRows` should stay focused on graph-owned blocker and unvalidated-node rows; full inventory belongs in `hygieneSections` and `contractSourcePressure` so agents can inspect it without turning the primary audit into a wall of harmless contract cards. The shared export implementation should keep Map and Hygiene serialization consistent while each tab draws only the actions it owns.

Hygiene pressure kinds are not all cleanup commands. `RuntimeOwnership`, `DirectSceneQuerySurface`, and source-ownership leaks such as `ReflectionMeaningLeak`, `ValidatorGuideLeak`, `InspectorRouteGuideLeak`, `ExportTruthLeak`, `TabRendererLogicLeak`, and `CompatibilityBridge` are Cleanup Focus because they indicate behavior or route truth may be living in the wrong owner. Stale-doc and old-owner-name pressure are Watch List items unless their review hint or surrounding code proves they affect current behavior. Protective anti-fallback policy text, docs that define the current source-ownership audit, and inspector copy that only hands users to Pyralis Authoring should not be classified as ownership leaks. `PawnCoordinator`, `PawnCapabilitySibling`, `LocalPresentationSurface`, `SceneZoneSurface`, `InputRoutingSurface`, `EnemyCapabilityModule`, `ActorFeatureContext`, `SceneCameraRig`, `AuthoredDataAsset`, `HazardRuntimeSurface`, `DomainUtility`, `FeatureModule`, `AuthoredRuntimeSurface`, `GameFlowRuntimeSurface`, `AcceptedComposition`, `ReferenceAssembly`, `EditorAudit`, `GrammarVocabulary`, and `ScannerImplementation` describe expected pressure shapes unless their review hint says they have crossed ownership boundaries. The UI should separate Cleanup Focus from Watch List so expected large scripts stay visible without reading as failures.

Source ownership residue belongs in Hygiene, not Map. Reflection should expose structure only; validators should witness local semantic readiness; inspectors should stay field-local; exporters should serialize tab projections; tab renderers should render models; active docs should state current ownership directly; compatibility bridges should not quietly repair authoring setup. Source-pressure rows are advisory audit signals. Formal route invalidity still comes from contracts, dependency reflection, validators, typed graph metadata, and graph evidence.

### Route Proof Trace Export

Guide may export a compact **Route Proof Trace** JSON packet through a page-local **Export Route Trace** button. This is an editor-only, read-only diagnostic view of the current proof route, not a preset, generator, setup model, or replacement for Map/Hygiene snapshots. Map exports only its current setup snapshot, Hygiene exports only its graph/code audit snapshot, and neither should offer Route Proof Trace.

The trace should serialize the route projection's **fresh-scene setup-card path**: current route, intent focus when present as context, first proof priority, proof node, current action, ordered setup cards, proof blockers, direct proof context, supporting contracts, source owners, assignment fields, native setup actions, graph summary, and diagnostic questions for humans or agents. It may be generated from an Intent-projected graph before any concrete setup object is selected, because its job is to preview the path the user is about to create. The ordered cards should approximate the path a user would follow from an empty scene to the selected first proof: Gameplay Root, visible Lifetime Scope, Session Definition, Game Mode, route capabilities, participants, input, pawn definition, pawn prefab/runtime validation, proof enhancers when helpful, then the Play Mode proof target. Pawn-backed local join and hybrid routes use the local co-op pawn join proof even when a selected gameplay descriptor carries a generic 1P pawn movement proof hint.

The export separates route cards into three audit buckets:

- `criticalPath`: required setup cards that must be clear before the selected first proof is believable. Lifetime Scope belongs here because scene composition is real setup, not optional polish.
- `proofEnhancers`: useful cards such as camera framing, visual/collision tuning, or movement feel that make the first proof easier to judge without blocking required setup.
- `canWait`: optional, candidate, off-route, or later-route vocabulary such as scoring, tabletop, settings, or playfield cards when they are not required by the selected proof.

`orderedSteps` is the compact visible route: critical path, proof enhancers, then the final proof target. `currentAction` is the first missing or blocked route step to do now; it is intentionally separate from `orderedSteps[0]`, because `orderedSteps[0]` may be a ready foundation/context card in the full from-scratch path. The trace should not include every optional contract or broad route vocabulary item. Overview, Guide, and Route Proof Trace must all consume the same route working projection so the cockpit, expanded guide, and JSON audit cannot drift apart.

The trace must not become a broad contract-support graph. Contracts can appear as proof context, but direct support should be limited to exact active-proof contracts instead of broad contract inventory or grammar-owned proof metadata. The ordered route path should come first from setup-chain nodes, dependency-tree gaps, core setup graph nodes, prefab-readiness evidence, and runtime validation evidence. Scene-surface repair lists and scene-surface proof enhancers stay in Map, but pawn/prefab readiness blockers may appear in Guide/Trace when they translate into a concrete Unity component or field the route requires. The trace should also avoid promoting broad selected-but-later capabilities such as networking or procedural generation as direct setup cards for a local movement proof. The trace exists to answer: "What cards would Overview/Guide show, in what order, if the user had to build this proof from scratch?"

### Facts Export

Facts may offer a compact read-only **Export Facts JSON** button. This snapshot serializes the dictionary lens only: fact counts, source/confidence counts, reflected contract coverage, and dictionary fact rows. It should help humans and agents audit vocabulary, contract provenance, and missing coverage without turning Facts into setup guidance. It should not export route-intent facts, runtime-capability routing, customization moments, proof facts, proof-support coverage, first-proof instructions, assignment fields, native action payloads, or field/component repair actions. Route steering remains Intent; proof workflow remains Guide/Route Proof Trace; concrete repair work remains Map/Guide; code and graph pressure remains Hygiene.

If the trace is wrong, fix the upstream owner: gameplay contract meaning, dependency-tree reflection, validator evidence, graph projection, or generic grammar wording. Do not hardcode special trace text to make one proof look right.

### Native Creation Workflow

Creation should favor native Unity authoring steps instead of a separate preset/profile path. The guided path is the Authoring Window pointing to Project-window Create, Hierarchy Create Empty, Inspector Add Component, object picker, and Inspector wiring. Raw assets remain valid manual setup for users who already know the asset chain they want.

When a future helper action exists, it should:

- create ordinary project assets
- assign only the immediate missing reference
- select or ping the created object
- avoid choosing design taste such as art, exact tuning, level layout, or final feature list

The guided flow should use Intent, reflected setup evidence, contracts, and grammar vocabulary. It should not ask users to create or assign a separate gameplay asset for route capability metadata.

## Scene Surface Scan

The Scene Surface Scan is the bridge between setup assets and ordinary Unity scene content.

It should explain what exists, whether the route likely needs it, and what to create or inspect next. It should not require every environment object to carry a Pyralis component.

Move this scan toward route-aware detectors:

- environment and playfield detector
- camera and bounds detector
- UI, HUD, and menu detector
- scoring and objectives detector
- board, card, and action selection detector
- pawn spawn detector
- projectile detector
- networking detector
- hazards, pickups, enemies, and zones detector

Each detector should return typed evidence. A detected object means "we found a relevant surface," not "the whole route is playable." Prefer scoped, linked evidence over broad global counts. When a surface is only a candidate, say so.

## Proof Loops

Every route should recommend one small proof before asking for broad setup.

Overview should show a first playable proof target rendered from graph proof nodes with:

- setup surface
- success criteria
- proof chain for hybrid routes
- work to defer until after the proof

Route identity comes from `PyralisSetupRouteAnalysis` and the reflected dependency tree. Proof nodes are selected by `PyralisAuthoringSetupGraphBuilder` from contract/descriptor proof targets first, then `PyralisProofFamilyVocabulary` supplies generic wording. A pawn + projectile + networking setup still starts with local movement while graph edges show that projectile resolution and network ownership are intentionally next.

Examples:

| Route | First proof |
|---|---|
| Pawn action | one participant spawns one pawn and moves |
| Combat | one hit causes one visible reaction |
| Projectile | one shot spawns, travels, and resolves |
| Tabletop | one piece, card, or board action resolves |
| Action/menu | one selected command reaches its resolver |
| Scoring | one event changes score and one UI element shows it |
| Camera/cursor | one input changes view, cursor, or selection |
| Networking | local proof works before host/client ownership proof |
| Procedural | generated output is inspectable before it drives play |

This prevents the authoring path from becoming a giant tree of unproven wiring.

Use `Ready to attempt first proof` instead of `Play proof passed` until the route has actually been exercised in Play Mode. Hybrid setups should grow into proof chains: local base proof first, then route-specific proofs such as projectile, combat, scoring, tabletop action, and network ownership.

## Route Completeness

The Authoring Window exists to make routes complete and keep them complete.

A route is complete when Pyralis provides:

- mechanic runtime
- definitions and profiles
- prefab and scene setup
- Authoring Window guidance
- validation
- route scaffold or template only after manual proof
- sample scene
- first playable proof
- docs
- tests

Use this as the product gate for every game lane. A route is not done just because the runtime system exists. It is done when a Unity developer can create it, understand the setup chain, validate common mistakes, run the smallest proof, and keep going.

## Reference Products

Use these products and patterns as guidance, not as things to copy directly:

- Unity ScriptableObject authoring: keep durable design intent in assets.
- Unity UI Toolkit editor windows: the active Authoring Window surface pattern.
- Odin Inspector: polished validation and field-adjacent editor ergonomics.
- Game Creator and Adventure Creator: coherent game-creation workflows with safe defaults.
- PlayMaker and Unreal Blueprints: visible state, immediate feedback, and clear next executable steps.

Pyralis should stay Pyralis: definitions and profiles express intent, runtime components execute behavior, ordinary Unity scenes remain valid, and the Authoring Window keeps the chain understandable.

## Extension Checklist

Most gameplay feature work should not edit the Authoring Window. Before changing UI or projection code, identify the owner of the missing truth:

| Missing truth | Preferred owner |
|---|---|
| Feature identity, runtime family, role tags, proof target, supported lanes, native setup action | `[AuthoringContract]` on the feature-owned type |
| Required references, serialized fields, `CreateAssetMenu`, required components, implemented interfaces | reflection and `PyralisSetupDependencyTree` |
| Local semantic readiness, invalid values, cross-field constraints, route-specific semantic warnings | `PyralisRuntimeValidationIssue` records from the owning asset/component |
| Open-scene surfaces such as cameras, UI roots, colliders, tilemaps, spawn points, pickups, hazards, zones, and scene services | typed scene-surface detectors |
| Generic wording, labels, facts, proof prose, and inspector handoff text | grammar/vocabulary |
| Ranking, grouping, de-duplication, current action, route cards, and tab/export lenses | `PyralisAuthoringSetupGraphProjection` consuming typed graph metadata |

Projection code may translate low-level structural evidence into route-facing Unity actions, such as showing `Add Motor2D` for a Sprite2D pawn motor gap. It must not invent setup truth by parsing labels, namespaces, source origins, stable-id prefixes, or guidance prose.

## Maintenance Rules

When adding authoring behavior:

- add shared route facts to route analysis or the structured authoring model first
- keep setup intelligence out of one-off drawing code
- keep first proof, capability, recommended-next, and scene-surface wording in shared guidance owners
- keep broad route guidance in the Authoring Window, not compact inspectors
- keep field editing in the Inspector
- make no-pawn routes first-class
- label optional work as optional
- prefer native Unity Create/Add Component/Inspector wiring over Authoring Window create-and-assign actions
- update docs when the live setup path changes

The Authoring Window succeeds when a Unity developer can keep going without asking, "Which object am I supposed to touch next?"
