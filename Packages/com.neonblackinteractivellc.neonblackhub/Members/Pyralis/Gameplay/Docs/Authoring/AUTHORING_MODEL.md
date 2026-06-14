# Pyralis Authoring Model

This is the missing map for setup. Read this before the prefab guides.

Pyralis starts from one scene object and one top-level asset.

The scene object is `GameplaySessionBootstrap`. It is the first runtime object to place in a playable scene.

The top-level asset is `SessionDefinition`. It is the first authoring asset the bootstrap reads.

Everything else hangs from that handoff: the bootstrap creates or connects runtime services, then those services read definitions, profiles, participants, pawns, feature modules, and runtime components.

## The Mental Model

`GameplaySessionBootstrap` answers: "What scene object starts Pyralis?"

`SessionDefinition` answers: "Which game session should this bootstrap start?"

Definitions answer: "What thing exists, and how does it relate to other things?"

Profiles answer: "How should that thing behave, feel, move, look, or be tuned?"

Runtime components answer: "What actually runs in the scene?"

Unity authoring is the layer where code, assets, prefabs, scenes, and Inspector guidance meet. It is not just "dragging references around." In a maintainable Unity project, authoring is the public workflow for configuring runtime behavior without editing code.

Internal product lanes such as Phyreal should consume this authoring contract before asking for new grammar. Pyralis/Game Studio Core owns reusable capability meaning: participants, pawns, routes, profiles, feature modules, input, camera, presentation lanes, interactions, facts, validation, and proof evidence. A product lane can layer its own metadata on top for product identity, governance, library adoption, world-pack release, safety, and renderer-agnostic manifest fields, but it should not fork the authoring model unless the existing contract cannot represent the needed capability.

For Pyralis, good authoring means a person can:

- select the `GameplaySessionBootstrap` scene object
- open the assigned `SessionDefinition`
- follow the chain into `GameModeDefinition`, `GameSetupProfile`, participants, pawns, profiles, and feature modules
- see validation warnings near the assets or components that need attention
- understand which fields are required, optional, route-specific, or safe to leave empty

Example:

- `PawnDefinition` says this pawn exists and points to its prefab.
- `PawnMovementProfile` says how fast it moves and jumps.
- `PawnRoot`, `Motor2D`, or `Motor3D` apply those choices at runtime.

## The Main Asset Chain

Every normal Pyralis setup starts with this scene-to-asset handoff:

```text
GameplaySessionBootstrap
  -> SessionDefinition
```

From there, the authored asset chain is:

```text
SessionDefinition
  -> GameModeDefinition
      -> GameSetupProfile
          -> Runtime Capabilities
              -> optional RuntimePatternDefinition[]
  -> ParticipantDefinition[]
      -> PawnDefinition optional
          -> pawn prefab
          -> pawn profiles
          -> feature modules
```

In plain English:

- `AuthoringContractAttribute` is the lightweight declaration placed on runtime/editor types.
- `ResolvedAuthoringContract` is the normalized contract record after attributes, merge rules, source type, setup graph node id, developer proof guidance, proof target, and fallback guidance have been resolved.
- `ResolvedAuthoringContractRegistry` is the authoritative source for feature-owned authoring contracts.
- Feature contracts are declared in feature code with `[AuthoringContract]`.
- Contracts are discovered reflectively and normalized once; validation, proof guidance, inspector handoffs, setup graph links, and capability descriptors should consume resolved contracts instead of rescanning raw attributes.
- `PyralisAuthoringGrammarRegistry` aggregates vocabulary/default facts, reflection, setup-flow facts, proof templates, inspector handoffs, convention facts, route intents, and scene evidence. It is grammar/audit input, not the operating model.
- `PyralisSetupDependencyTree` owns serialized setup/reference discovery.
- `PyralisAuthoringSetupGraph` compiles contracts, dependency-tree references, validators, grammar, and selected Unity context into the readiness/proof model consumed by the Authoring Window.
- Convention facts stay as explicit source calls from the main fact registry. Do not add a parallel provider-discovery layer unless a new spine capability genuinely needs extension-point behavior.
- `GameplaySessionBootstrap` is the runtime startup object in the scene.
- `SessionDefinition` is the whole play session.
- `GameModeDefinition` is the active mode or ruleset for that session.
- `GameSetupProfile` describes the kind of runtime the mode expects.
- `GameSetupProfile` runtime capabilities are the beginner-facing capability selections. Intent derives those rows from reflected capability descriptors that prefer contracts and dependency reflection, then use vocabulary only for fallback wording.
- Selected runtime capability families are the beginner-facing route ingredients. Optional `RuntimePatternDefinition` assets can add reusable advanced metadata such as participant embodiment, supported control surfaces, presentation/runtime lanes, and first-proof requirements when the generic capability family is not descriptive enough.
- `ParticipantDefinition` describes a player, AI, seat, team, or participant slot.
- `PawnDefinition` is used only when a participant needs an actor body.

The operating model behind those assets is:

```text
Contracts + reflection + dependency tree + scene evidence + validators + grammar
  -> resolved setup graph
      -> Overview / Intent / Guide / Map / Validate / Facts
```

`AUTHORING_BLUEPRINT.md` owns the detailed information-flow map and cleanup closure criteria. Use this file for the asset/runtime relationship model; use the blueprint when deciding where authoring information should live.

## Definitions

Definitions are identity and relationship assets.

| Asset | Owns | Points to | Use it when |
|---|---|---|---|
| `SessionDefinition` | session name, max participants, join/camera defaults | default `GameModeDefinition`, default `InputProfile`, `SettingsProfile`, participant list | every playable setup |
| `GameModeDefinition` | scene names, enabled systems, mode rules | `GameSetupProfile`, `PlayfieldProfile`, `CameraRigProfile`, required feature modules | every playable mode |
| `RuntimePatternDefinition` | optional reusable route contract plus route-facing description/setup notes, presentation/runtime lanes, and first-proof requirements | companion/cautionary runtime contracts | adding advanced metadata when a capability family alone cannot describe the route |
| `ParticipantDefinition` | display name, team, seat, auto-join defaults | default `PawnDefinition`, participant `InputProfile` | each player, AI, seat, faction, hand, or participant slot |
| `PawnDefinition` | one actor body setup | pawn prefab, pawn profiles, feature modules | only when the participant owns an actor body |
| `FeatureModuleDefinition` | reusable actor/pawn capability or ability declaration | runtime prefab, profile asset, supported presentation modes | adding modular abilities such as pickups, feedback, status, interaction, traversal, guard/reaction, or custom actor modules |
| `ActionDefinition` | target rules and action costs | none directly | reusable actions for turn/menu, cards, board moves, abilities, or commands |
| `ActorAnimationDefinition` | animation signal bindings | Animator parameters through `ActorAnimationBinding` | mapping gameplay signals to an Animator |
| `ProjectileDefinition` | projectile or hitscan payload | `ActionDefinition`, `ProjectileImpactDefinition`, projectile prefab | bullets, spells, traps, turrets, scripted shots |
| `FireModeDefinition` | firing cadence and ammo behavior | none directly | single shot, burst, spread, cooldowns, clips, reloads |
| `ProjectileImpactDefinition` | impact effects | audio/visual/camera shake values | hit/miss feedback for projectile commands |
| `CombatActionDefinition` | one combat action | hitbox slot, timing, damage, knockback | melee or authored combat moves |
| `CombatSequenceDefinition` | combo/action sequence | `CombatActionDefinition[]` | primary, secondary, aerial, or enemy attack chains |
| `StatusEffectDefinition` | reusable status effect rules | none directly | poison, burn, stun, slow, shield effects |

Rule of thumb: if the asset names a gameplay thing or links other assets together, it is probably a definition.

For pawn-authored abilities, use `FeatureModuleDefinition` assets assigned to `PawnDefinition.featureModules`. Controller profiles define the pawn's baseline body and input feel; feature modules define optional reusable capabilities that can vary per pawn archetype.

## Profiles

Profiles are tuning assets. They tell runtime components how to behave.

| Asset | Tunes | Usually assigned from |
|---|---|---|
| `GameSetupProfile` | selected runtime capabilities, resolved runtime patterns, and setup notes | `GameModeDefinition.setupProfile` |
| `InputProfile` | Input System actions, action map, gameplay action-name mapping, device/control expectations | `SessionDefinition`, `ParticipantDefinition`, `PawnDefinition` |
| `SettingsProfile` | default audio/settings values | `SessionDefinition` |
| `PlayfieldProfile` | movement bounds, depth, clamp rules | `GameModeDefinition` |
| `CameraRigProfile` | shared/split camera mode, zoom, orthographic behavior | `GameModeDefinition`, `CinemachineCameraRigController` |
| `PawnMovementProfile` | movement speeds, acceleration, jump/motion tuning | `PawnDefinition` |
| `PawnCombatProfile` | combat sequences and combat tuning | `PawnDefinition` |
| `PawnTraversalProfile` | traversal behavior such as climb, hang, ledge rules | `PawnDefinition` |
| `PawnPresentationProfile` | 2D, 2.5D, or rigged 3D presentation | `PawnDefinition` |
| `PawnAnimationProfile` | Animator controller and animation definition | `PawnDefinition` |
| `ActorFeedbackProfile` | actor feedback behavior | feature modules or feedback runtimes |
| `ActorCombatReactionProfile` | block, hurt, stagger reactions | combat reaction feature module |
| `ActorStatusEffectProfile` | starting effects and status behavior | status-effect feature module |
| `PickupFeatureProfile` | pickup collection behavior | pickup feature module |
| `InteractionFeatureProfile` | interaction radius/cooldowns | interaction feature module |
| `EnemyFeatureProfile` | enemy combat, reaction, and modules | enemy runtime setup |
| `EnemyCombatProfile` | enemy attacks and AI combat tuning | enemy runtime setup |
| `EnemyReactionProfile` | enemy hurt/stagger/camera shake feedback | enemy runtime setup |
| `EnemyAmbientFeatureProfile` | ambient enemy behavior | enemy feature module |
| `HazardImpactProfile` | damage, knockback, status effects for hazards | `HazardData` |
| `HazardFeedbackProfile` | hazard popups/flash feedback | `HazardData` |

Rule of thumb: if the asset mostly contains numbers, toggles, curves, references to effects, or tuning options, it is probably a profile.

`GameSetupProfile` is the one slightly weird case: it is called a profile because it describes setup intent, but it acts like the bridge between mode definition and runtime pattern definitions.

Runtime pattern descriptions are part of the setup manual. Fill `description` with what the pattern is for, and fill `setupNotes` with the native Unity steps a developer should follow next. Set `presentationLanes` to the runtime surfaces the pattern can guide, and set `firstProofRequirements` to the scene/service evidence the first playable proof should expose. Those fields appear in the pattern Inspector, the Authoring Window selected-object context, route validation, and any `GameSetupProfile` that references the pattern.

## Authoring Window And Inspectors

Setup-facing assets should be easy to edit, but setup intelligence should live in one central place.

The **Pyralis Authoring Window** is the central setup UI layer. It has six modes:

- **Overview**: the quick decision dashboard; shows the current setup state, the best next action, and the next one to three useful moves.
- **Intent**: the project-shape chooser; owns world/playfield, control shape, runtime lane, and grouped capability toggles. It summarizes the active intent, but it does not own the card wall.
- **Guide**: the ranked current-intent guide; shows graph-filtered route rows for the active setup, falls back to cookbook planning cards before setup exists, and includes the selected-object explainer.
- **Map**: the setup topology view; shows route links, readiness rows, scene-surface evidence, and Inspector jump targets without editable fields.
- **Validate**: the quality gate; shows issues for the selected setup object and its inferred route.
- **Facts**: the read-only cookbook and dictionary; groups all source/provenance-backed facts by kind, including facts outside the current intent, so coverage gaps stay auditable.

For the concise product north star, read `AUTHORING_EXPERIENCE_VISION.md`. For mode responsibilities, evidence model, implementation phases, and maintenance rules for this window, read `AUTHORING_BLUEPRINT.md`.

The window keeps an **Active Setup** at the top. When no setup is pinned, the active setup follows the current Unity selection whenever the selection can infer a `GameplaySessionBootstrap`, `SessionDefinition`, `GameModeDefinition`, or `GameSetupProfile`. When nothing is selected and exactly one `GameplaySessionBootstrap` exists in the open scene, the Authoring Window treats that scene root as the active setup instead of falling back to an empty state. When a loose newly-created setup asset is selected and the remembered or single scene setup still needs that asset assigned to its next field, the active setup stays on the scene setup story so the guide does not skip past the wiring step. When a setup is pinned, **Overview** and **Map** keep using that steady setup route while **Guide** continues to explain the current selection. This lets guided users click through prefabs, child objects, and component fields, or clear selection entirely, without losing the main setup story.

The native Project-window Create path should stay beginner ordered under `NeonBlack`. In `NeonBlack > Definitions`, `Session Definition` comes first because it is the first setup asset a new route needs, followed by Game Mode, Participant, Pawn, Runtime Pattern, Feature Module, Action, and Animation definitions. In `NeonBlack > Profiles`, setup-chain profiles come first: Game Setup, Input, Playfield, Camera Rig, Pawn Movement, Pawn Traversal, Pawn Presentation, Pawn Animation, Pawn Combat, then Settings before broader feature profiles. Do not move first-route setup behind preset/profile paths.

Unity's built-in dock tabs such as Project, Hierarchy, Inspector, Scene, and Game are native Editor chrome, not Pyralis UI. The authoring color system should still guide users toward those surfaces, but prefer durable semantic labels, field guidance, inspector beacons, overlays, or optional window-focus aids over unsupported Unity skin/layout hacks that repaint built-in tab text.

Surface beacons are the active form of those color hints. When guidance knows the native Unity surface for a step, it should render a compact colored beacon such as `Project`, `Hierarchy`, `Inspector`, `Play Mode`, or `Authoring`. Clicking a beacon only focuses or opens the relevant Unity surface; it must not create assets, assign fields, enter Play Mode, or make taste decisions for the user. Beacons should be driven from structured native-action data such as `PyralisAuthoringNativeAction.Surface` wherever practical so new facts, contracts, setup steps, and validators inherit the behavior without hard-coding a new UI path.

When guidance says to assign an asset to an Inspector field, drag-and-drop and the small object picker circle are both valid native Unity paths. Prefer wording that names the field and expected asset type, then lets the user choose the assignment method that fits their layout.

The Authoring Window is the guided mode. Do not add a separate beginner/advanced toggle. Power users can bypass the window and work directly in the Inspector, scenes, prefabs, code, or native Project-window asset creation paths. Inside the window, use progressive disclosure: compact route rows first, evidence and explanations in foldouts or issue cards, and route-specific next steps that point back to native Unity actions.

For the core setup chain, the **Pyralis Authoring Window** is the central guided workspace and the Inspector remains the direct Unity field editor. Inspectors should still explain the selected asset or component, but the Authoring Window should behave like an authoring service: it reads the selected context, diagnoses what setup is ready or missing, recommends the next move, and names the native Unity action and Inspector field the user should use next.

Use the Inspector when you need to edit a specific serialized field. Use the Authoring Window when you need to understand what the selected thing means in the setup route, which Project/Hierarchy/Create or Add Component action belongs next, and what to do after the field is assigned.

The authoring experience should guide Unity setup without taking authorship away. It can say "create a UI Root with Canvas and EventSystem, add `ParticipantHealthHudBinder`, then connect its labels or panels," but it should not design the HUD for the user. It can say "movement speed lives in `PawnMovementProfile` and presentation mode lives in `PawnPresentationProfile`," but it should not choose the sprite sheet, rig, or exact speed value. It can explain that a brawler route usually unlocks pawn, combat, animation, feedback, camera, input, and HUD concerns, then point to the fields and components that configure each concern.

Authoring should also keep obvious customization checkpoints visible before Play Mode. These are non-blocking Proof Enhancers, not generated setup: tune camera framing, pawn visual/collider fit, movement feel, input mappings, UI labels, board surfaces, and route-specific profiles enough that the first proof represents the user's intent instead of generic untouched defaults. The Authoring Window should name the Unity object or profile to inspect and the type of choice to make, while the Inspector remains where exact values are edited.

## Manual Authoring Validation

The validation path for this system is a Computer Use pass through the actual Unity Editor. A tester should open the Pyralis Authoring Window, follow the guidance, create assets from the Project window, place scene objects from the Hierarchy, add components through the Inspector, assign fields with drag/drop or object pickers, customize values in normal Unity fields, and enter Play Mode when the guide says the route is ready.

Do not prove the authoring model by adding a generated full-scene factory, one-click sample scene, preset, or starter-pack shortcut. That bypasses the user's decisions and hides the navigation problems the authoring system exists to solve. Active validation must come from the Authoring Window guiding native Unity workflow and generic setup capabilities.

Imported asset packs are valid proof material when they are treated as creator-owned content. For tabletop validation, use generic board, card, marker, token, tile, or little-game collections the way a normal Unity creator would: bring assets into the project, form intent in the Authoring Window, create project-local definitions/profiles, wire scene objects through the Hierarchy and Inspector, and verify one visible or inspectable state change. Do not make Pyralis authoring promote, generate, or wire package sample scenes. Disposable proof scenes can exist for Cameron's review, but they are review artifacts, not the authoring contract.

Good validation notes should record where the guide helped, where Unity navigation was naturally a learning curve, where Pyralis guidance was missing or too forceful, which code warnings or validators were improved, and which customization choices stayed in the tester's hands.

The **Intent**, **Guide**, **Overview**, **Map**, **Validate**, and **Facts** tabs are projections from the same authoring spine. The read-only resolved setup graph is built from contracts, dependency-tree route analysis, setup-flow evidence, scene-readiness evidence, selected Unity context, and generic grammar fallback wording. Foundational contracts can declare `SetupNodeId` so the graph connects reflected contracts to stable setup concepts without parallel mapping files. The graph does not create assets or apply presets; it explains what the selected intent and current setup imply. Intent owns only the creator's route choices and a compact summary. When a setup profile is active, Intent converts selected toggles into capability ingredients stored in `GameSetupProfile.runtimeCapabilities` through reflected capability descriptors produced from contracts, dependency reflection, and fallback vocabulary. Guide prefers `PyralisAuthoringSetupGraphProjection.BuildCurrentIntentGuideRows(...)` so active setup guidance comes from blockers, proof nodes, selected capabilities, and reflected contracts in the graph; fallback vocabulary only fills generic wording gaps. Overview extracts the best next one to three setup moves and can surface graph priority/proof data. Map projects current setup topology from graph rows. Validate projects graph-grouped readiness evidence while the concrete Unity checks stay in the setup-flow and scene-readiness validators. Facts shows the graph and grammar dictionary, including facts outside the current route. Genre words are summaries of selected ingredients, not source data. Intent should not apply suggested defaults, create assets, choose optional runtime contracts, wire a scene, or imply that the user has accepted a preset. For side-view brawler work, `Movement`, `Jump / Traversal`, `Combat`, `Input`, and `Animation / Presentation` are visible route-shaping capabilities because they are the first decisions a creator brings with imported art, sprites, animations, and attack feel. For top-down 2D work, free X/Y movement, bounds, targeting, camera framing, and optional hop/dash semantics outrank side-view gravity ground. For tabletop/card/UI work, no-pawn surfaces, action selection, seats/hands/factions, and visible state changes outrank pawn prefab setup. Each proof pass should improve contracts, dependency reflection, validators, and graph projection first, then let every tab benefit from the corrected reflection. The feature guide reads `GameSetupProfile.runtimeCapabilities` and optional runtime contracts, then explains the selected capabilities:

- design capabilities: checkbox/dropdown selection for route-facing capability families; optional `RuntimePatternDefinition` contracts can enrich advanced metadata
- design prompts: what world/playfield the project uses, what the user controls, which capabilities are active, and which focused proof should come next
- gameplay effect: what this capability adds to the game
- world/environment contract: how plain Unity geometry, colliders, layers, bounds, zones, anchors, and selectable surfaces affect Pyralis
- Unity setup: scene objects, components, definitions, profiles, or services to wire next
- customization: the fields and assets where the user expresses taste, art, tuning, rules, layout, and feature behavior
- recommended next options: nearby capabilities that commonly complete the selected project shape without becoming presets

The Authoring Window implementation should stay split by responsibility. Keep `PyralisAuthoringWindow` as the UI shell, active setup graph cache, selection, and mode coordinator. Put route resolution in `PyralisSetupRouteAnalysis` and `PyralisSetupDependencyTree`, graph compilation in `PyralisAuthoringSetupGraphBuilder`, graph projection in `PyralisAuthoringSetupGraphProjection`, reflected capability descriptors in `PyralisAuthoringCapabilityDescriptorRegistry`, fallback proof wording in `PyralisProofFamilyVocabulary`, capability checkbox behavior in `PyralisAuthoringCapabilitySelection`, scene-surface route wording in `PyralisAuthoringSceneSurfaceGuidance`, and pre-setup intent fallback ranking in `PyralisAuthoringIntentAdvisor`. Add new game systems to contracts, validators, dependency-tree coverage, or grammar vocabulary before adding more drawing logic to the window.

Setup flow rows should be structured facts, not just display strings. Each `PyralisSetupFlowStep` should carry a stable step id, a work intent (`Foundation`, `RequiredSetup`, `ProofEnhancer`, or `FeatureCard`), evidence, target object, and native Unity action when one is known. This lets Overview decide what to do first even when many systems are open: blockers go to `Do Now`, proof enhancers stay near the first proof, and feature-card work waits until the current proof is reliable. Do not key new feature behavior only from row labels or warning copy.

Scene readiness should use structured buckets instead of one flat warning stream:

- **Required Before Play**: issues that break or invalidate the first proof, such as missing pawn runtime contracts, wrong Input System UI module, duplicate active EventSystems/listeners, blank active SpriteRenderers, missing required network surfaces, or missing required prefab scripts.
- **Recommended Before Play**: useful setup that improves confidence but should not block every route, such as camera/audio visibility guidance when the scene has not claimed that surface yet.
- **Proof Enhancers**: route/taste checks that make the proof feel better, such as camera framing, pawn visual/collider fit, movement feel, one clear physics lane, UI labels, board layout, pickup placement, and local-input clarity.

Overview should expose a compact Play Mode checklist from those buckets: required setup, first proof target, scene visibility, input route, presentation, and physics feel. Validate owns the deeper bucketed console and should translate common Unity failures into native next actions, for example replacing `StandaloneInputModule` with `InputSystemUIInputModule`, adding one `AudioListener` to the physical camera, assigning missing SpriteRenderer sprites, or inspecting an `InputProfile` action map. These checks must guide the user's own Unity objects and fields; they must not auto-generate game content or choose the user's art, camera composition, board layout, combat shape, or menu design.

Authoring UI performance is part of the contract. Expensive graph builds, scene-surface scans, reflection/cookbook reads, and validation summaries should be cached or throttled by selection/project/hierarchy change rather than rebuilt multiple times per repaint. When a pin, selection, project, hierarchy, or intent change alters the active setup story, invalidate the relevant authoring cache explicitly.

The **Map** tab owns the first-read setup map:

```text
Scene Root -> Session -> Game Rules -> Setup Profile -> Capabilities -> Participants -> Pawn / No Pawn -> Scene Surfaces
```

Each row should show whether the current route needs that link, whether it is ready, what asset or scene object is current, and where to inspect next. Pawn status must come from the route descriptor: pawn-backed routes should ask for a `PawnDefinition`, while no-pawn tabletop, board, card, camera, cursor, and menu routes should explicitly say that empty pawn fields are correct.

The **Scene Surface Scan** is the bridge between setup assets and ordinary Unity scene content. It scans for common scene surfaces such as colliders, Tilemaps, spawn points, cameras, camera bounds providers, Canvas, EventSystem, HUD/menu presenters, scoring services, board/action selection surfaces, pickups, hazards, enemies, and zones. These rows should stay explanatory: they tell the author what exists, whether the selected route likely needs it, and what to create or inspect next. Route-irrelevant rows should read as not needed yet, not ready. Spawn points are placement evidence, not a playable Environment / Playfield surface by themselves. Side-view 2D gravity proofs need an intentional collider, Tilemap, zone, bounds provider, or other route-owned gameplay surface before Play Mode is a meaningful movement test. Top-down/free 2D, camera/cursor, tabletop, card, and UI-first paths may treat spawn/camera/selection/UI evidence differently because their world contract is not platform ground. They should not auto-build levels, choose art, or require every environment object to carry a Pyralis component.

For example, a pawn plus combat route can be described as a brawler/fighter/action route because the selected capabilities imply pawn actors, movement, combat actions, presentation, input, camera, HUD, and feedback. A tabletop route should instead say that pawn fields can stay empty and that the next useful surfaces are board/card state, action selection, camera/cursor, and UI.

Intent is the guided editor for the route selection model persisted by the `GameSetupProfile` Inspector's Runtime Capabilities section. DNA axioms describe the world contract, the presentation lane describes the runtime surface, and capability ingredient toggles describe what the author is trying to build. When a setup profile is active, those Intent choices write matching `runtimeCapabilities` entries so validation, route reading, Map, Overview, and Guide all read the same contract. Optional `RuntimePatternDefinition` assets can still enrich selected families with advanced metadata, but they are not the first-shape route selector and should be created only when the existing capability language cannot describe the game.

Environment authoring is part of the route even when the objects do not carry Pyralis scripts. Ground, walls, platforms, tilemaps, terrain, card slots, table props, board squares, rooms, and arena boundaries can remain ordinary Unity objects. Pyralis should teach which observable contracts matter:

- walkable or blocked surfaces are expressed through colliders, physics materials, layers, and ground masks
- camera, spawning, pickups, hazards, and generated content use bounds, zones, and anchors
- board/card/action routes need a selectable or addressable surface, which may be UI, raycast, board coordinates, colliders, or presenter data
- presentation systems may need sorting, depth, lane, or art-mode choices even when the object is visual only

This keeps environment work in the user's hands while still making the setup impact clear.

Background and world-art setup is intentionally broad. A route may use 2D flat sprite/PNG backdrops, Tilemaps, 2.5D layered props, 3D meshes, terrain, skyboxes, Canvas images, or future procedural chunks. Pyralis should guide only the gameplay-facing contracts: bounds, colliders, layers, anchors, zones, sorting/depth, camera framing, selectable surfaces, and generated-content placement rules. It should not require every visual background object to carry a Pyralis component.

Field explanations should name the actual selected field whenever possible. Examples:

- `defaultGameMode`: chooses the ruleset this session starts with.
- `setupProfile`: chooses which runtime route the mode expects.
- `runtimeCapabilities`: decide whether pawns, board/card UI, camera/cursor, action menus, combat, projectiles, scoring, networking, or procedural systems are expected.
- `defaultPawn`: stays empty for seats, hands, factions, menus, or camera-only participants, and is required when the selected route needs actor bodies.
- `movementProfile`: controls speed, acceleration, jump feel, braking, and similar motion tuning.
- `presentationProfile`: chooses whether the pawn is presented as Sprite2D, Billboard2_5D, Rigged3D, or another supported presentation stack.
- `Player Input Manager`: is only for local join; single-player and no-pawn routes can usually leave it empty.
- `Camera Rig Controller`: connects the bootstrap to the Pyralis Cinemachine camera flow. Sprite2D movement patterns need visible 2D bounds when the target camera or assigned camera rig profile is orthographic; Billboard2_5D and Rigged3D pawn patterns still need a camera rig for framing, but should not be blocked on orthographic 2D bounds.
- HUD/menu presenters: connect Unity Canvas objects to Pyralis services such as score, feedback, health, settings, scene flow, action selection, board selection, or turn prompts.

The shared `PyralisInspectorGuide` block is a compact field-local companion. It may show validation, short purpose text, field tooltips, and an **Open Authoring Window** action, but it should not grow into a parallel setup walkthrough. If guidance explains the whole route, chooses the next setup step, or creates/connects assets, it belongs in the Authoring Window.

The Authoring Window should keep the core setup chain understandable whenever it can infer it from the current selection, but it should not duplicate the Inspector with editable object fields:

```text
GameplaySessionBootstrap
  -> SessionDefinition
      -> GameModeDefinition
          -> GameSetupProfile
              -> RuntimePatternDefinition[]
      -> ParticipantDefinition[]
          -> PawnDefinition optional
```

When one of those core links is missing, the window should point to the native Unity workflow instead of creating or assigning the link itself: Project window right-click **Create** for assets, Hierarchy right-click **Create Empty** for scene roots, Inspector **Add Component** for scripts, and Inspector fields for reference assignment. This keeps folder ownership, asset naming, and field wiring visible to the user. The window may provide small **Inspect Asset**, **Open Guide**, or **Open Map** jumps, but field-level configuration belongs in the Inspector.

The core chain uses route-aware shared guidance:

- `RuntimePatternDefinition` explains one capability, the control surfaces and presentation lanes it supports, and the first-proof scene/service evidence it expects.
- `GameSetupProfile` combines selected capabilities and explains the next setup route.
- `GameModeDefinition` reads its setup profile before recommending camera, playfield, feature, combat, scoring, or respawn wiring.
- `SessionDefinition` explains participants, pawn/no-pawn expectations, and whether shared input is only optional.
- `GameplaySessionBootstrap` inspects the assigned session graph and reports scene-level next steps.
- `PyralisProofFamilyVocabulary`, `PyralisCapabilityVocabulary`, and `PyralisAuthoringSceneSurfaceGuidance` own reusable fallback proof, capability, and scene-surface wording. Feature-specific setup meaning belongs in contracts/reflection and should reach UI through graph evidence.

Each guided Inspector should answer compactly:

- what the asset is for
- when to use it
- what to create before it
- what to assign first
- what can safely stay empty or be customized
- what to check before pressing Play

Use the Authoring Window as the setup service and context surface. Use the Inspector guidance for field-local help, use `AUTHORING_BLUEPRINT.md` for Authoring Window product and implementation direction, and use this model when you need the longer asset-chain explanation.

## Authoring Maintainability

The current authoring direction is good: Pyralis uses definitions and profiles to describe intent, while runtime components read those assets. That is the right shape for a Unity gameplay toolkit.

The maintainability risk is duplicate setup intelligence. Route advice, first-proof copy, scene-surface wording, and validation expectations should flow from shared guidance models instead of being repeated independently in inspectors, windows, validators, and docs.

Healthy authoring should feel like this:

```text
Scene root
  -> Bootstrap
      -> SessionDefinition
          -> GameModeDefinition
              -> GameSetupProfile
                  -> RuntimePatternDefinition[]
          -> ParticipantDefinition[]
              -> PawnDefinition optional
                  -> Profiles
                  -> FeatureModuleDefinition[]
```

Unhealthy authoring feels like this:

```text
Scene objects
  -> static managers
  -> tag searches
  -> implicit single player
  -> hidden object discovery
  -> runtime behavior that is hard to predict from assets
```

Current first-scene guidance should teach the asset-driven route above.

### What Makes Unity Authoring Expensive

Unity projects become expensive to maintain when behavior is split across too many invisible places.

Common costs:

- scene references that are required but not validated
- prefabs that need exact component combinations but do not explain them
- MonoBehaviours with many unrelated serialized fields
- ScriptableObjects whose names do not explain whether they are definitions or tuning profiles
- static managers that make tests and scene reloads depend on hidden global state
- custom inspectors that show fields but do not teach the setup order
- docs that duplicate live route advice instead of pointing to shared guidance owners

For Pyralis, the highest-value authoring improvements are:

- keep `GameSetupProfile` and runtime patterns visible before prefab wiring starts
- make the Authoring Window the live setup checklist and keep `GameplaySessionBootstrap` Inspector guidance compact
- prefer explicit participant/session references over tag searches or hidden global discovery
- keep route guidance centralized in the shared authoring models
- add validation for missing scene roots, missing pawn components, invalid feature-module prefabs, and unsupported pawn/non-pawn setup combinations
- split large authoring inspectors only when there is a clear user workflow boundary

### How To Think About The Main Unity Pieces

`ScriptableObject`: an asset in the Project window. Use it for data that should survive scene reloads, be reused across prefabs/scenes, and be reviewed in source control.

Prefab: a reusable GameObject recipe. Use it for runtime composition: which components exist together, what child objects exist, and what scene object can be spawned.

Scene object: a concrete instance in one scene. Use it for scene-specific roots, spawn points, cameras, UI anchors, and one-off environmental layout.

MonoBehaviour: code that runs on a GameObject. Use it for runtime behavior, but keep design intent in assets when the behavior should be reused.

Custom Inspector: editor UI for a component or asset. Use it to make the right setup easier, not just prettier.

Validation: code that checks whether the authored setup makes sense. Use it to catch mistakes before Play Mode.

The authoring sweet spot is when ScriptableObjects explain intent, prefabs explain composition, scene objects explain placement, MonoBehaviours execute behavior, and inspectors/validation keep the whole chain understandable.

### Build Footprint Model

Authoring data and runtime dependencies are separate concerns. Editor-only authoring contracts, convention providers, facts, inspectors, validators, Authoring Window UI, and live proof tooling should not be required in player builds. They explain how to create and validate a route; they should not be part of what the exported game runs.

Player builds are shaped by runtime assemblies and by asset references from scenes, prefabs, ScriptableObjects, `Resources`, Addressables, and bootstrap paths. Keep reusable runtime systems modular, and avoid route assets that point at every possible feature "just in case." A pawn movement route should not reference tabletop, RPG, networking, or heavy UI assets unless that route actually uses them.

When adding a new reusable feature, the authoring contract can be broad and discoverable in the Editor, but the runtime prefab, profile, sample assets, and scene references should stay scoped to the routes that need that feature. Future build-report checks should verify that representative route exports do not include editor assemblies or unexpected large unrelated assets.

## Runtime Components

Runtime components live on GameObjects. They do the work.

| Component | Reads |
|---|---|
| `GameplaySessionBootstrap` | `SessionDefinition` |
| `SessionStateService` | `SessionDefinition`, active `GameModeDefinition` |
| `ParticipantRosterService` | `SessionDefinition`, `ParticipantDefinition` |
| `ParticipantSpawnService` | `ParticipantDefinition`, `PawnDefinition`, spawn points |
| `ParticipantInputRouter` | `SessionDefinition`, `InputProfile` |
| `PawnRoot` | `PawnDefinition`, active `GameModeDefinition` |
| `Motor2D`, `Pawn2DMovementComponent` | `PawnMovementProfile`, `PawnPresentationProfile` |
| `Motor3D`, `Pawn3DMovementComponent` | `PawnMovementProfile`, `PawnTraversalProfile` |
| `PawnCombatBehaviour`, `PawnCombatBehaviour2D` | `PawnCombatProfile`, combat sequence definitions |
| `ActorAnimationDriver` | `PawnAnimationProfile`, `ActorAnimationDefinition` |
| `CinemachineCameraRigController` | `CameraRigProfile`, `PlayfieldProfile`, participant roster |
| `CameraShake` | optional target transform, impact calls from combat/projectiles/hazards |
| feature runtimes | `FeatureModuleDefinition`, assigned profile asset, platform services |

## Pawn-Backed Setup

Use this for brawlers, fighters, 2D character games, shooters with actor bodies, enemy actors, or anything where a participant owns a moving body.

Minimum authoring chain:

```text
SessionDefinition
  -> GameModeDefinition
      -> GameSetupProfile
  -> ParticipantDefinition
      -> PawnDefinition
          -> Pawn prefab with PawnRoot
          -> InputProfile
          -> PawnMovementProfile
          -> PawnPresentationProfile
          -> PawnAnimationProfile optional but recommended
          -> PawnCombatProfile optional
          -> PawnTraversalProfile optional
```

Minimum scene:

- `Gameplay Root` with `GameplaySessionBootstrap`
- spawn point if the pawn should spawn into the scene
- camera root if the game needs a camera rig
- UI/scoring/playfield roots only when the mode needs them

## Non-Pawn Setup

Use this for board, card, tabletop, camera-only, menu-combat, puzzle-board, or turn-based games without actor bodies.

Minimum authoring chain:

```text
SessionDefinition
  -> GameModeDefinition
      -> GameSetupProfile
          -> Board/Card/Tabletop
          -> Turn/Menu Action
          -> Camera/Cursor Control
  -> ParticipantDefinition
```

No `PawnDefinition` is required unless the game has animated actor pieces or board pieces that should behave like pawns.

Minimum scene:

- `Gameplay Root` with `GameplaySessionBootstrap`
- `Camera Root` if the game has a board/cursor/camera view
- `UI Root` for hands, board UI, menus, turn prompts, card zones
- `Playfield Root` for board spaces, card zones, anchors, or selectors
- `Scoring Root` only if the game tracks points, victory points, timers, or resources

## Feature Module Setup

Feature modules are optional capability bundles.

The chain is:

```text
FeatureModuleDefinition
  -> profileAsset optional
  -> runtimePrefab

PawnDefinition
  -> featureModules[]
```

At runtime:

1. `PawnRoot` initializes the pawn.
2. `ActorFeatureHost` reads the feature module definitions.
3. Each module creates or uses a runtime component.
4. The runtime receives a context containing the pawn, participant, game mode, services, and profiles.

Use feature modules when you want a capability to be reusable across multiple pawns or game types.

Do not create a feature module for one-off scene wiring. Use a normal component first.

### Feature-Owned Authoring Contracts

Every reusable `FeatureModuleDefinition` module with known setup rules should have a feature-owned `[AuthoringContract]` on the runtime, interface, profile, or component type that owns the capability. The registry discovers contracts by reflection across loaded assemblies, so do not add central lists of feature module ids.

A feature contract is the source of truth for:

- stable feature id and `FeatureModuleDefinition.moduleId`
- required profile type
- required runtime prefab interfaces
- supported and unsupported `ActorPresentationMode` lanes
- unsupported-lane message
- consumed input/action roles
- native Unity setup actions
- Inspector assignment fields
- customization moments
- developer first-proof guidance and resolved proof target id

The Authoring Window, fact registry, feature-module Inspector, contract validator, and proof guidance all read this contract data. If a feature-specific rule appears in a central switch or hand-authored setup row, move it into the feature contract unless the rule is truly generic to every feature module.

New feature module checklist:

1. Add or update the runtime module, runtime prefab/component, profile, and any shared interfaces.
2. Add or update `[AuthoringContract]` on the capability owner.
3. Declare required profile, runtime interfaces/components, lane support, unsupported lane cautions, action roles, native setup, assignment fields, customization moments, and developer first-proof guidance.
4. Add Unity `.meta` files for new scripts, asmdefs, assets, prefabs, and folders.
5. Add registry/fact/validator/proof tests for the module.
6. Refresh or run Unity validation before trusting generated project files.
7. Update setup docs only when the feature changes durable authoring behavior.

Do not edit generated `.csproj` or `.sln` files to make a contract compile. Unity should generate those from asmdefs and package assets after refresh.

## Combat And Projectile Setup

Melee-style combat usually flows like this:

```text
PawnDefinition
  -> PawnCombatProfile
      -> CombatSequenceDefinition
          -> CombatActionDefinition
```

Projectile-style combat usually flows like this:

```text
ProjectileLauncher2D or ProjectileLauncher3D
  -> ProjectileDefinition
      -> ActionDefinition optional
      -> ProjectileImpactDefinition
  -> FireModeDefinition
```

Use melee combat definitions for authored hit windows, combo steps, hitbox slots, damage, and knockback.

Use projectile definitions for bullets, hitscan, spells, traps, turrets, card-triggered attacks, or scripted shots.

## Animation Setup

Animation is intentionally split:

```text
PawnDefinition
  -> PawnPresentationProfile
  -> PawnAnimationProfile
      -> Animator Controller
      -> ActorAnimationDefinition
          -> ActorAnimationBinding[]
```

The presentation profile answers what kind of visual this pawn is:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

The animation profile answers how this pawn's Animator should be driven.

The animation definition maps gameplay signals to Animator parameters.

Bring your own Animator Controller is the normal path. Find the controller in the folderbase or package where your art/controller assets live, assign the controller that the pawn visual actually uses, then map Pyralis signals such as move, jump, dash, attack, hurt, and interact to that controller's existing parameters in `PawnAnimationProfile`. Do not rename an imported or project controller just to satisfy Pyralis unless that rename also improves your project.

Input follows the same pattern. `InputProfile` should point at the project's Unity Input Action Asset, name the primary action map, and list the gameplay action rows this setup actually uses. Built-in rows such as Move, Look, Jump, Dash, Attack, Interact, Block, Sprint, Crouch, Roll, Previous, and Next unlock Pyralis features. Custom rows let creators add project-specific actions that can feed custom gameplay or animation wiring without changing Pyralis code. The 2D and 3D pawn input modules read those rows from the profile, so a project can keep its own action vocabulary while still using Pyralis movement and authoring validation.

For 2D pawns, movement actions should stay explicit. A required Move row feeds the pawn motor. Jump, Dash, Interact, Attack, and Custom rows are semantic requests. Installed feature modules can handle those requests before the input adapter falls back to built-in movement or combat behavior. When `PawnMovementProfile.allow2DJump` / `Pawn2DMovementComponent.jumpEnabled` is off, the baseline 2D route is top-down/free movement and Move drives X/Y; a Jump row can still feed a feature such as `TopDownHopFeatureRuntime`. When side-view jump is on, Move drives horizontal X while an unclaimed Jump row requests vertical jump through Rigidbody2D gravity. `PawnMovementProfile` and `Pawn2DMovementComponent` decide whether dash is allowed and how it feels, while a Dash row decides which hardware action triggers dash. Leave Dash absent for games with no keyboard/gamepad dash, map Dash to `Jump` only when a top-down 2D game intentionally wants jump-button dash instead of a hop feature, map it to `Roll` or `Dash` for a dedicated dodge, or use a Custom row when dash is part of an ability-specific input.

Keep shared scene systems out of pawn prefabs unless the prefab is intentionally self-contained. A pawn prefab can carry its movement, presentation, combat, input adapter, and feature host, but the scene/session normally owns `CinemachineCameraRigController`, the physical `Main Camera`, the Cinemachine Camera, participant roster, gameplay state, spawn points, and playfield bounds. Authoring should explain which empty prefab fields are runtime-supplied instead of forcing users to wire a camera into every pawn prefab.

## Which Asset Do I Create First?

For a pawn-backed prototype:

1. Start from the `GameplaySessionBootstrap` Setup Flow monitor.
2. Create or assign `GameSetupProfile`.
3. Choose runtime capability families that describe the route.
4. Create or assign `GameModeDefinition`.
5. Create or assign `SessionDefinition`.
6. Create or assign `ParticipantDefinition`.
7. Create `PawnDefinition` and pawn profiles.
8. Wire scene roots and prefab references only when the monitor asks for them.

For a non-pawn prototype:

1. Start from the `GameplaySessionBootstrap` Setup Flow monitor.
2. Create or assign `GameSetupProfile`.
3. Choose runtime capability families that describe the route.
4. Create or assign `GameModeDefinition`.
5. Create or assign `SessionDefinition`.
6. Create or assign `ParticipantDefinition`.
7. Add UI/camera/playfield/scoring scene roots only when the selected capabilities need them.
8. Add board/card/turn-specific runtime components as the first playable loop needs them.

Do not promote manually proven routes into active starter packs or presets. Capture reusable learning as cookbook facts, validation rules, optional route contracts, or generic setup guidance instead.

## Common Confusions

`GameSetupProfile` is not the same as `GameModeDefinition`.

- The setup profile says what runtime surfaces the game expects.
- The game mode says which scenes, systems, playfield, camera, and rules are active.

`ParticipantDefinition` is not the same as `PawnDefinition`.

- The participant is the player/AI/seat/faction.
- The pawn is an optional actor body that participant may own.

`PawnPresentationProfile` is not the same as `PawnAnimationProfile`.

- Presentation says what visual category the pawn uses.
- Animation says how gameplay signals drive the Animator.

`ActionDefinition` is broader than combat.

- It can represent card plays, menu commands, turn actions, board moves, abilities, or combat commands.

`ProjectileDefinition` does not require a character controller.

- Projectiles can be fired by pawns, enemies, traps, turrets, cards, board spaces, menus, or scripts.

## Setup Rule

If you are lost, walk the chain from top to bottom:

1. What session is running?
2. What game mode is active?
3. What setup profile describes that mode?
4. What runtime patterns are selected?
5. What participants exist?
6. Does each participant need a pawn?
7. Which profiles tune that pawn or scene system?
8. Which runtime components read those assets?
