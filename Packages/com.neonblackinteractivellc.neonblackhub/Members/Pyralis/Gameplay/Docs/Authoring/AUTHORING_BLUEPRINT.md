# Pyralis Authoring Blueprint

This is the canonical product and implementation blueprint for the Pyralis Authoring Window.

Read this when changing the Authoring Window, setup guidance, route validation, route scaffolding, scene-surface scanning, or Inspector handoffs. Use `AUTHORING_MODEL.md` when you need the asset relationship map behind the window.

## Documentation Role

Pyralis docs should stay current and purposeful:

- `README.md` files orient the reader and name the right next document.
- `START_HERE.md` teaches the first human setup path.
- `AUTHORING_EXPERIENCE_VISION.md` owns the concise product north star: Authoring is the map, Unity is the workshop, and Inspectors are the local knobs.
- `AUTHORING_BLUEPRINT.md` owns Authoring Window product direction, UX rules, implementation phases, and maintenance rules.
- `AUTHORING_MODEL.md` owns the asset/runtime relationship map behind the window.
- `CANONICAL_SETUP.md` owns the technical setup contract.
- `FEATURE_DEVELOPMENT_ROADMAP.md` owns route-completeness sequencing.
- `CURRENT_STATE_AUDIT.md` owns the present platform state and highest-priority risks.
- `CORE_PACKAGE_READINESS_CHECKPOINTS.md` owns near-term readiness gates and validation workflow.
- dated audits and old refactor notes are historical records unless an active README names them as current.

When these docs disagree, prefer the active README reading order, this blueprint for Authoring Window behavior, and the current code/tests for implementation truth. Update the stale doc instead of preserving conflicting guidance.

## North Star

The Pyralis Authoring Window should feel like a calm senior Unity teammate beside the developer.

Its job is not to replace the Inspector, build a whole scene, choose game design, or hide Unity. Its job is to keep setup understandable while the developer keeps moving.

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

- explain the current `GameSetupProfile.runtimePatterns`, not a hard-coded genre path
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
| Optional system appears required before the first proof | Move it to Proof Enhancers or Feature Cards |
| Inspector duplicates route instructions | Keep compact field-local help and hand off to Authoring |
| Authoring loses context when selecting child assets or prefabs | Improve active-route inference, pinning, and "you are here" language |

The Authoring Window is also for experienced users. Its bar is not "can a beginner follow it eventually"; its bar is "would a Unity pro trust this route diagnosis, know exactly what changed, and still feel free to build the game their way."

## Product Contract

The Authoring Window is the central setup service for Pyralis.

The Inspector remains the direct field editor. The Authoring Window reads the current route, diagnoses readiness, explains selected context, and recommends the next useful move by naming the native Unity action and Inspector field to use.

Scene, prefab, and asset authoring must stay guidance-first. Unity scene objects and authoring assets often need project-specific folders, names, components, layers, art, input modules, camera choices, and designer intent that Pyralis should not hide. The active path is native Unity authoring: create definitions, profiles, runtime patterns, prefabs, components, and scene objects yourself while Authoring Window cards audit, explain, show evidence, describe success, and point to the right Unity object, Create path, Inspector field, or checklist.

Do not turn scene-surface warnings into hidden one-click creation flows. The author owns the scene; Pyralis makes the setup chain legible.

## Guidance And Enforcement Boundary

Pyralis authoring should guide supported routes strongly without bounding normal Unity experimentation.

Outside the active Pyralis setup, ordinary Unity objects, scripts, scenes, assets, prefabs, art, UI, and experiments should remain valid Unity work. The Authoring Window can call them outside the active setup, found-but-not-linked, or not needed for this route, but it should not treat them as Pyralis errors unless they are wired into a Pyralis setup or Unity itself reports a native error.

Inside the Pyralis setup, the system can be strict about real contracts. If a developer wants a feature, component, route, prefab, profile, or asset to become reusable Pyralis library work, it must attach back to the shared contract/fact/proof model: stable ids, native setup actions, supported lanes, validation issues, required components or fields, first proof target, tests, and docs when the workflow changes.

Power users can bypass the Authoring Window while they experiment, but reusable Pyralis features should not become invisible side systems. The graduation path is: build freely in Unity, then connect useful work back into the Pyralis authoring spine before calling it authoring-ready.

## Export Footprint Boundary

Authoring 2.0 should make setup intelligence visible in the Unity Editor without making exported games carry that editor weight. Authoring providers, fact registries, validators, custom inspectors, proof tooling, live-test notes, and Authoring Window UI belong in `Editor` folders or editor-only assemblies unless there is a deliberate runtime reason to ship them.

Runtime code should stay modular by route. A route migration should not add broad always-loaded references that pull unrelated systems or large assets into a player build. Scene, prefab, ScriptableObject, `Resources`, Addressables, and bootstrap references are the export-size boundary to watch: if a route does not use a system or asset, it should not be referenced by that route's runtime setup just because the authoring model knows the system exists.

Contracts and facts are useful here because they describe setup, validation, native Unity actions, and proof targets without becoming runtime dependencies. Keep that separation intact. A later route-promotion gate should inspect Unity build reports for representative route builds and flag unexpected editor assemblies, unrelated runtime modules, or large unused assets.

## Current Implementation Shape

Keep the implementation split by responsibility:

| Owner | Responsibility |
|---|---|
| `PyralisAuthoringWindow` | UI shell, active setup state, selection, and mode coordination |
| `PyralisAuthoringRouteDescriptor` | route facts inferred from setup patterns and selected context |
| `PyralisAuthoringRouteProof` | first playable proof names, success criteria, and deferred work for each route family |
| `PyralisAuthoringRouteReport` | selected-object diagnosis, validation issues, target jumps, and route report rows |
| `PyralisAuthoringOverviewSnapshot` | Overview counts, next-step summary, and readiness buckets |
| `PyralisAuthoringCapabilitySelection` | Design Capabilities picker backed by existing `RuntimePatternDefinition` assets |
| `PyralisRuntimeCapabilityCatalog` | guide-only capability cards indexed by game-goal and runtime-lane tags |
| `PyralisAuthoringIntentAdvisor` | Cookbook-to-intent read model that ranks route-intent, capability, contract, and proof facts from selected world/playfield, control shape, lane, and goals for Guide cards and other tab projections |
| `PyralisAuthoringCapabilityGuidance` | selected capability, recommended next, environment, and route-intent guidance rows |
| `PyralisAuthoringSceneSurfaceGuidance` | scene-surface labels, route relevance, next-fix text, expected evidence, and success text |
| `PyralisAuthoringFeatureAdvisor` | composes route-aware feature advice from shared guidance models |
| `PyralisSetupFlowMonitor` | bootstrap/setup-flow readiness checks that should stay aligned with the window |
| `PyralisSceneReadinessValidator` | scene and prefab evidence checks |

Add route facts, issue meaning, and setup analysis to these focused model/report classes before adding more drawing logic to the window.

The active guidance pipeline is:

```text
Runtime contracts
  -> Definition/Profile assets
      -> Route analysis
          -> Shared authoring guidance models
              -> Authoring Window, validation cards, inspector handoffs, and docs
```

Do not store the same route advice separately in multiple windows, inspectors, validators, or docs. If wording such as first proof, route intent, recommended next systems, or scene-surface success criteria needs to change, update the shared guidance owner first and let the visible surfaces render from it.

## Core Setup Chain

The Authoring Window should keep this chain visible whenever it can infer it from the current selection:

```text
GameplaySessionBootstrap
  -> SessionDefinition
      -> GameModeDefinition
          -> GameSetupProfile
              -> RuntimePatternDefinition[]
      -> ParticipantDefinition[]
          -> PawnDefinition optional
              -> pawn prefab
              -> profiles
              -> feature modules
```

The user-facing Map chain is:

```text
Scene Root
-> Session
-> Game Rules
-> Setup Recipe
-> Capabilities
-> Participants
-> Pawn / No Pawn
-> Scene Surfaces
```

Each row should show whether the route needs that link, whether it is ready, the current object or missing field, why it matters, and where to inspect next.

Pawn-backed routes should ask for a `PawnDefinition`, pawn prefab, movement/presentation profiles, and relevant spawn/camera surfaces. No-pawn routes such as tabletop, board, card, camera, cursor, menu, or faction routes should explicitly say that empty pawn fields are correct.

## Information Model

Move the window toward structured authoring facts that every mode can consume.

Core concepts:

- `Route`: the inferred game surface, such as pawn action, tabletop, action/menu, camera/cursor, scoring, networking, or a hybrid route.
- `Setup Node`: one row in the authoring chain, such as bootstrap, session, game mode, setup profile, runtime patterns, participants, pawn/no-pawn state, and scene surfaces.
- `Issue`: required, recommended, optional, blocked, or not-needed setup work.
- `Action`: inspect, create, assign, repair, explain, copy checklist, or open documentation.
- `Evidence`: why the tool believes something is ready, missing, optional, or not needed for this route.
- `Work Intent`: whether the row is foundation setup, required setup, a proof enhancer, or a feature card.

The first Authoring 2.0 foundation is `PyralisAuthoringFactRegistry`. It is a read-only typed fact spine used to keep guide cards, future validation issues, inspector handoffs, and setup rows on stable ids instead of display text. Current runtime capability cards now expose `PyralisAuthoringFact` records with provenance, confidence, native Unity actions, route relevance, first proof, required definitions/profiles/components, assignment fields, customization moments, deferrable work, and supported/unsupported lanes. Setup-flow rows now contribute setup-node facts through `PyralisSetupFlowGuidance.GetAuthoringFacts`, including stable setup ids, work intent, native Unity action metadata, and related capability ids for the 2D pawn movement route. Route proof facts now include stable first-proof anchors for pawn movement, board/card action, action selection, NPC/enemy behavior, custom object effects, UI/HUD/menu events, camera/cursor/world framing, generated content, and network ownership. Route-family coverage facts name the broad authoring surfaces the system must support: pawn actors, NPC/enemy actors, custom objects/features, UI/HUD/menu routes, world/camera routes, tabletop/card routes, and networking authority routes. Scene-evidence facts now bridge the existing scene-surface guidance rows to route/proof ids for environment/playfield, camera/bounds, UI/HUD/menus, scoring/objectives, board/action selection, and pickups/hazards/enemies surfaces. Validate cards now carry and display typed `PyralisAuthoringIssue` companions through `PyralisAuthoringIssueAdapter`, preserving stable issue codes, severity, work intent, evidence state, affected field/component, reason, and native Unity action metadata for setup, scene-surface, and prefab-readiness issues. Inspector handoff facts now link native Inspector fields for core setup, 2D pawn/input, tabletop board/turn rules, camera/playfield profiles, required feature modules, feature module profile/runtime/network fields, Cinemachine camera fields, tabletop presenter fields, and camera framing customization back to route families and first-proof facts. Reflection/convention facts now flow through `IAuthoringConventionFactProvider` entries discovered by `PyralisAuthoringConventionFactRegistry`; `PyralisConventionAuthoringFacts` remains the bridge for unmigrated convention facts, `PyralisSprite2DConventionAuthoringFactProvider` owns the first migrated 1P Sprite2D convention surface, and `PyralisRouteIntentAuthoringFactProvider` owns studio-wide Game Type intent facts such as 2D side-view action, pawn brawler, and camera/cursor command. `PyralisAuthoringIntentAdvisor` now projects those facts into a compact route-choice summary for Intent and ranked current-intent cards for Guide: world/playfield, control shape, lane, grouped capability toggles, tooltips, recommendations, first-proof summaries, assignment/customization foldouts, and lane cautions. Intent chooses the route shape; Guide owns the card wall; Facts remains the full cookbook and dictionary. Those facts expose selected `CreateAssetMenu`, `AddComponentMenu`, `RequireComponent`, serialized-field, route-intent, lane, and goal metadata across core setup, pawn, tabletop, action, camera, UI, custom feature, NPC/enemy, combat, projectile, and feedback surfaces as read-only native Unity guidance with explicit or convention-derived confidence. Beginner semantic location tags have started through `PyralisAuthoringSemanticTag`, giving the Authoring Window an optional top legend strip and fact/action badges for Project, Hierarchy, Inspector, Component, Prefab, Definition, Profile, Input, UI, Animation, Audio, and Play Mode. The Authoring Window now includes a read-only `Facts` tab that groups registry facts by kind, shows coverage counts, provenance, native actions, requirement lists, customization moments, semantic location badges, and related stable ids.

The registry should grow in this order:

1. hand-authored guide-card facts that preserve product voice - implemented for the Runtime Capability Catalog
2. setup-flow facts for core setup nodes and native actions - implemented for current setup step ids
3. route proof facts that relate capability cards, setup nodes, and first Play Mode proof - expanded for pawn, tabletop/card, action selection, NPC/enemy, custom object, UI/HUD/menu, camera/world, generated content, and networking proof anchors
4. typed validator issues with stable issue codes - started through the Validate model adapter and visible metadata block
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

`PyralisSetupFlowStep` rows should carry stable ids and native action metadata. The display label is for humans, not for routing behavior. Overview, Map, Validate, tests, and future feature contributors should prefer the step id, work intent, and `PyralisAuthoringNativeAction` over string-matching labels or message text. Message text can still specialize a row, but it should not be the only source of meaning.

Recommended setup should not all land in the same visual priority. Use work intent to keep the flow calm:

- `Foundation`: useful visibility or defaults that keep the scene inspectable.
- `RequiredSetup`: missing links that block the named first proof.
- `ProofEnhancer`: customization or scene support that makes the first proof believable, readable, or easier to debug.
- `FeatureCard`: optional systems, advanced route capabilities, polish, or next proof-chain work that should not compete with the current proof.

## Runtime Capability Catalog

The Runtime Capability Catalog is a guide-only discovery layer inside the Authoring Window. It lets users browse Pyralis-supported setup surfaces by both game goal and runtime lane without turning the window into a scene generator.

The catalog should use one canonical card model, not separate hardcoded trees. A card is indexed by game-goal tags such as movement, combat, projectiles, camera, UI/HUD, scoring, interaction, NPCs/enemies, tabletop, and networking, and by runtime-lane tags such as `Sprite2D`, `Billboard2_5D`, `Rigged3D`, tabletop/no-pawn, UI/menu, camera/cursor, and networked. Both browse modes render the same cards so guidance does not drift.

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

Runtime capability cards are the first fact-backed card surface. The visible card remains guide-only, but its underlying `PyralisAuthoringFact` is the maintainable contract future Authoring 2.0 surfaces should read from. Do not add new capability cards as isolated UI prose; add or update the shared card/fact model so Overview, Map, Validate, inspectors, docs, and future fact explorer views can converge on the same stable id and setup meaning.

The catalog must stay guide-only for the first implementation. It may select, ping, explain, or copy checklist text, but it must not create or assign assets, add components, or treat generated scaffolding as validation evidence. Users should still use native Unity surfaces: Project window asset creation, Hierarchy object creation, Inspector Add Component, Inspector field assignment, object picker, customization through serialized fields, and Play Mode proof.

The same guide-card model should later expand beyond runtime patterns into the whole setup path: session setup, participants, pawns, NPCs/enemies, custom interactables, pickups, hazards, camera/world bounds, UI/HUD/menus, scoring/objectives, tabletop/control surfaces, and networking.

## Mode Responsibilities

### Intent

Intent is the starting surface when no Pyralis setup context is selected or inferred. It should ask what kind of game the developer is trying to build before the user is pushed toward bootstrap, participant, pawn, prefab, scene, input, camera, or Play Mode details.

Intent should stay studio-wide:

- use world/playfield dropdowns, control-shape dropdowns, lane choices, and capability toggles, not preset setup profiles
- rank route families, contracts, cautions, and first proof targets from registry facts
- react to side-view 2D gravity, top-down/free 2D, 2.5D lane/arena, 3D, tabletop/no-pawn, card/table, UI/menu, camera/cursor, hybrid, and networking lanes
- explain what the toggles imply without creating assets or choosing design taste
- hand off to Project, Hierarchy, Inspector, Prefab, Input, Animation, UI, and Play Mode surfaces only when the route has enough declared intent

Intent is not the whole setup flow and not the proof itself. It names the project-wide world/control/capability shape so Overview, Map, Validate, Facts, and native Unity Inspectors can focus the next route proof with clearer context.

### Overview

Overview is the daily home base. It should show:

- route name
- blocking status
- one best next action
- Active Setup state
- You Are Here chain
- selected context
- concise readiness summary
- first playable proof card

Organize progress into three lanes:

- `Do Now`: required missing or blocked work only.
- `Proof Enhancers`: route-specific setup that can make the first proof easier to read but should not block it once `Do Now` is clear.
- `Feature Cards`: optional next capabilities, polish, advanced systems, and nice-to-have proof.

If the developer is tired or lost, Overview should still make the next move obvious.

### Solid Flow Test

Cameron should be able to test the Authoring Window flow with one active route without reading code:

1. Open `NeonBlack/Gameplay/Pyralis Authoring Window`.
2. Select or pin one `GameplaySessionBootstrap`, `SessionDefinition`, `GameModeDefinition`, or `GameSetupProfile`.
3. Confirm Overview names the route and shows `Flow Test Status`.
4. Clear `Do Now` items before trusting Play Mode.
5. Use `Proof Enhancers` only when the first proof would be hard to read without them.
6. Leave `Feature Cards` alone unless the current proof specifically needs that capability.
7. Use `Inspect Best Target`, `Open Map`, and `Open Validate` to move between object inspection, dependency mapping, and issue triage.
8. Press Play only for the first proof named by Overview.

The flow is solid enough for product testing when a route can move from `Do Now` to a first proof without the user asking which object to inspect next.

### Guide

Guide explains the current Unity selection without losing the active setup story. It should answer:

- what this selected thing is
- why it matters in the active route
- what edits belong in the Inspector
- what to check after editing

Guide should not repeat the whole setup manual. If it starts explaining the entire route, that content belongs in Overview, Map, or docs.

### Map

Map is the dependency map. Each row should show:

- ready, missing, blocked, recommended, optional, or not needed
- current object or missing field
- why the route needs it
- where to inspect it
- the native Unity Create/Add Component/Inspector step when the link is missing

Map should teach the mental model. It should not become a second Inspector.

### Validate

Validate should be triage, not a wall of warnings. Issue cards should include:

- severity
- affected object
- affected field or component when known
- plain-English problem
- why it matters
- exact next inspection target
- expected surface, found evidence, and success criteria for scene/prefab audits
- safe fix button when possible

Validation should point to real Unity objects whenever possible. Current target jumps should prefer the pawn prefab, feature runtime prefab, projectile prefab, projectile definition, scene root, or affected asset over the bootstrap when the issue evidence names a more precise target.

### Native Creation Workflow

Creation should favor native Unity authoring steps instead of a separate preset/profile path. The guided path is the Authoring Window pointing to Project-window Create, Hierarchy Create Empty, Inspector Add Component, object picker, and Inspector wiring. Raw assets remain valid manual setup for users who already know the asset chain they want.

When a future helper action exists, it should:

- create ordinary project assets
- assign only the immediate missing reference
- select or ping the created object
- avoid choosing design taste such as art, exact tuning, level layout, or final feature list

New `RuntimePatternDefinition` assets are manual authoring work. The guided flow should choose existing patterns.

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

Overview should show a first playable proof card rendered from `PyralisAuthoringRouteProof` with:

- setup surface
- success criteria
- proof chain for hybrid routes
- work to defer until after the proof

Route identity comes from `PyralisSetupRouteAnalysis`: it owns canonical route facts such as Pawn Action, Projectile, Scoring, and Networking. `PyralisAuthoringRouteProof` composes those facts into a first proof plus later proof-chain steps, so a pawn + projectile + networking setup still starts with local movement while showing that projectile resolution and network ownership are intentionally next.

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
- Unity UI Toolkit editor windows: long-term fit for a persistent dashboard.
- Odin Inspector: polished validation and field-adjacent editor ergonomics.
- Game Creator and Adventure Creator: coherent game-creation workflows with safe defaults.
- PlayMaker and Unreal Blueprints: visible state, immediate feedback, and clear next executable steps.

Pyralis should stay Pyralis: definitions and profiles express intent, runtime components execute behavior, ordinary Unity scenes remain valid, and the Authoring Window keeps the chain understandable.

## Implementation Phases

### Phase 1: Structured Authoring Facts

Introduce or consolidate structured models for setup nodes, issues, actions, and evidence.

Expected result:

- less keyword-based categorization
- fewer duplicated route checks
- clearer UI rows across all modes
- tests can validate authoring diagnosis without opening the window

### Phase 2: Overview As Keep-Going Screen

Refactor Overview around `Do Now`, `Proof Enhancers`, and `Feature Cards`.

Expected result:

- the next useful step is obvious
- recommended items do not compete with blockers
- optional systems stop feeling like required setup

### Phase 3: Better Guide And Map Separation

Make Guide selection-local and Map route-global.

Expected result:

- Guide explains the selected object
- Map explains the dependency chain
- neither duplicates Inspector field editing

### Phase 4: Diagnosis-First Validate

Replace text grouping with structured issue cards.

Expected result:

- validation becomes triage
- each issue points to the affected object or field
- safe fix actions become easier to add without brittle string matching

### Phase 5: Detector-Based Scene Surfaces

Move scene scanning into route-aware detectors with evidence. Keep route relevance and user-facing detector wording in `PyralisAuthoringSceneSurfaceGuidance` so Overview, Validate, scene-surface rows, and docs do not drift.

Expected result:

- fewer false positives
- project-owned equivalents are easier to support
- scene surfaces explain partial readiness instead of only present/missing counts

### Phase 6: Proof Loop Guidance

Add first-proof recommendations to route analysis, feature advice, and Overview through `PyralisAuthoringRouteProof`.

Expected result:

- the developer knows what to test before expanding setup
- starter routes stay playable instead of becoming paperwork

### Phase 7: Window Architecture Hardening

Shrink `PyralisAuthoringWindow` toward a UI shell and mode coordinator.

Move behavior into focused drawers, presenters, analyzers, detectors, and action handlers.

Expected result:

- lower maintenance cost
- easier route additions
- better test coverage
- a future UI Toolkit migration becomes mechanical rather than conceptual

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
