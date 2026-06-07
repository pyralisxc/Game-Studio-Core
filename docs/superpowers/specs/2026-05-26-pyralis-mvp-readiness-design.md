# Pyralis MVP Readiness Design

Date: 2026-05-26

## Purpose

Pyralis is moving from internal framework development into scene development. Before it is offered to friends, collaborators, or studio members as a real authoring path, the package needs a clear MVP readiness bar.

The goal is not to make Pyralis create complete games automatically. The goal is to make Pyralis reliable enough that a beginner can create a basic playable prototype from scratch by following guided Unity setup: Inspectors, setup flow, starter packs, docs, validation messages, prefabs, and reusable runtime components.

This design defines the readiness target, route gates, runtime parity requirements, and implementation order for that platform checkpoint.

## Product Promise

Pyralis MVP readiness means:

> A beginner can open Unity, choose a supported game route, and Pyralis tells them what assets, scene roots, prefabs, components, and references they need. They still make their own game, art, levels, rules, tuning, and style, but the framework keeps them from getting lost in wiring.

Pyralis should guide creators into building their own games rather than handing them a finished game. Samples and demos can exist as references, but the product surface is the authoring system plus reusable gameplay scripts.

## Readiness Target

The target is **Beginner Prototype Ready through guided Unity setup**.

This is stronger than creator-assisted readiness. It should not require Cameron or Codex to hover over every setup step.

It is weaker than public toolkit readiness. It does not require a polished windowed no-code app, marketplace-quality sample library, online documentation site, or full support for every genre.

The first MVP should prove that guided Unity authoring can create normal playable prototypes across the core routes Pyralis claims to support.

## Core Principle

Pyralis should be route-driven and runtime-lane honest.

Routes describe the kind of game setup a creator is making. Runtime lanes describe the supported presentation/control implementation inside that route.

No route should be called ready unless it has:

- a real runtime path
- a Unity authoring path
- guided inspectors or setup docs
- setup or scene validation
- proof through tests, validation gates, or a small reference scene

No pawn-backed route should be called ready if only one runtime lane works. The official pawn runtime lanes are:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

All three must meet the MVP bar before the pawn-backed route is considered ready.

## MVP Routes

### Route A: Game Shell

The game shell is the front door every project needs.

This route covers:

- boot scene
- loading scene
- main menu
- settings
- credits
- scene fades and transitions
- new game or play flow
- restart and return-to-menu flow
- Build Settings guidance
- explicit scene navigation service assignment
- validation for missing scene names, navigators, panels, settings services, and buttons

The shell should not require a custom menu controller for the basic flow. Creators should be able to assemble the route from Pyralis components and connect their UI.

Credits are part of the MVP shell. They can be a simple panel/page at first; they do not need a complex scrolling or localization system in the first pass.

### Route B: Pawn-Backed Action

Pawn-backed action is the route for games where a participant controls an actor body.

This route covers:

- 2D character games
- 2.5D or billboard action games
- rigged 3D pawn games
- brawlers
- shooters
- fighters
- platform/action prototypes
- enemy or NPC actor bodies where the same setup concepts apply

This route must support all official runtime lanes:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

Each lane must be able to prove the same core authoring promise:

- session setup
- participant setup
- pawn definition setup
- pawn prefab setup
- input ownership
- movement
- camera
- animation or presentation
- health, damage, and defeat
- combat or interaction
- projectiles or guns where the route selects projectile combat
- scoring or HUD where the route selects scoring/objectives
- scene flow back to the shell
- setup validation for common mistakes

The first MVP does not need genre-specific move lists, advanced combo grammar, procedural levels, polished enemy AI, or production weapon inventories. It does need enough core mechanics to make a normal beginner prototype playable in every official pawn lane.

### Route C: Non-Pawn Tabletop

Non-pawn tabletop proves Pyralis is not only a character-controller framework.

This route covers:

- chess-like board games
- checkers-like board games
- tactical boards
- simple tabletop turn games
- camera/cursor-controlled board surfaces
- participants as seats, sides, factions, hands, or cursors

This route must not require fake pawns. A participant can exist without a `PawnDefinition`.

The MVP route should support:

- no-pawn session setup
- participants as seats or sides
- board definition
- board pieces
- move policy
- action queue
- turn order
- selection surface
- terminal or win conditions
- a minimal board presenter or a guided project-owned presenter path
- validation that does not ask for pawn prefabs or spawn points when the route is no-pawn

The first MVP does not need full official chess completeness, chess AI, cinematic capture animations, notation, clocks, or replay. It does need a guided playable board prototype path.

## Five-Part Completion Bar

Every MVP route and lane must satisfy this bar before being marked ready.

### Runtime Path

The runtime code must actually execute the loop. It is not enough for a setup pattern to name the idea.

Examples:

- menu buttons navigate through an `ISceneNavigator`
- pawn prefabs spawn and move
- projectiles fire and resolve impact
- board selections become queued actions
- terminal conditions can end a tabletop game

### Authoring Path

Creators must be able to assemble the route in Unity.

Examples:

- `CreateAssetMenu` entries exist
- starter packs create useful baseline assets
- components appear under understandable Add Component menus
- required scene roots are named and explained
- prefab component requirements are visible

### Guidance Path

Inspectors and docs must explain what to do next.

Guidance should answer:

- what this asset or component is for
- when to use it
- what to create first
- which fields are required
- which fields are optional
- what can safely stay empty
- what common mistake the creator is about to make

### Validation Path

Common setup mistakes must be caught before or during first Play Mode.

Validation should cover:

- missing scene names
- missing scene navigator source
- missing settings source
- missing session or mode links
- selected route without required runtime systems
- pawn route with incomplete prefab stack
- no-pawn route incorrectly asking for pawn/spawn setup
- runtime lane mismatch between pawn definition, prefab, presentation profile, and component stack
- projectile route without compatible launcher/projectile body
- tabletop route without board, move policy, action queue, turn order, selection bridge, or terminal condition where required

### Proof Path

Each route must have proof that the route works.

Acceptable proof:

- EditMode tests for pure/runtime logic
- PlayMode tests for Unity lifecycle and scene/prefab behavior
- source contract tests for authoring coverage
- scene readiness validation
- a small package sample or internal reference scene after the reusable path exists

The proof should match the risk. Runtime services and rules need tests. Visual authoring flows can use small reference scenes plus validation.

## Runtime Parity Matrix

Create and maintain an active parity matrix for MVP readiness.

The matrix should track each capability against each route or lane using these statuses:

- `Ready`: authored, guided, validated, and proven.
- `Guided Needs Proof`: setup exists, but runtime proof or sample proof is thin.
- `Foundation Only`: core code exists, but beginner authoring is not real yet.
- `Not Started`: missing as a platform capability.
- `Deferred`: intentionally outside MVP.

Initial matrix dimensions:

- Game Shell
- Pawn-Backed Action / `Sprite2D`
- Pawn-Backed Action / `Billboard2_5D`
- Pawn-Backed Action / `Rigged3D`
- Non-Pawn Tabletop

Initial capability rows:

- route setup profile
- starter pack
- scene root setup
- session and participant setup
- pawn or no-pawn correctness
- prefab requirements
- input ownership
- movement
- camera
- presentation and animation
- health, damage, and defeat
- combat or interaction
- projectiles/guns
- scoring/HUD
- board/rules/action queue
- turns/phases
- menu/loading/settings/credits
- scene flow
- setup flow validation
- scene/prefab readiness validation
- docs
- EditMode proof
- PlayMode proof
- known limitations

The matrix should live in package-local Pyralis docs, not only in this planning archive, once implementation starts.

## Explicit Non-Goals For This MVP

These are not part of the first readiness bar:

- production `.io` networking
- matchmaking, lobby, or session browser
- rollback, prediction, reconciliation, or remote input streaming
- replicated animation polish
- procedural endless side-scroller generation
- full official chess implementation
- chess AI
- cinematic replay systems
- full card/deck/hand gameplay
- visual node graph authoring
- a polished windowed no-code authoring app
- marketplace-quality public documentation

These are valid future directions. They should not block MVP guided Unity setup.

## Current Foundation To Reuse

The readiness pass should build on current Pyralis systems:

- `GameplaySessionBootstrap`
- `PyralisGameplayLifetimeScope`
- `SessionDefinition`
- `GameModeDefinition`
- `GameSetupProfile`
- `RuntimePatternDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `PawnRoot`
- `PawnPresentationProfile`
- `PawnAnimationProfile`
- `ActorAnimationDriver`
- `ParticipantRosterService`
- `ParticipantSpawnService`
- `ParticipantInputRouter`
- `SceneLoader`
- `SceneFader`
- `LoadingScreenController`
- `MainMenuManager`
- `SettingsManager`
- `SettingsScreen`
- `ActionDefinition`
- `ActionQueueService`
- `BoardDefinition`
- `BoardMovePolicyDefinition`
- `BoardMoveActionResolver`
- `TurnOrderDefinition`
- `TabletopBoardSelectionBridge`
- `TabletopBoardGridPresenter`
- `ProjectileDefinition`
- `ProjectileFirePlanner`
- `ProjectileLauncher2D`
- `ProjectileLauncher3D`
- `ParticipantScoreService`
- guided inspector helpers
- setup flow monitor
- scene readiness validator
- project validation gate

The pass should not reintroduce global singleton-first setup, tag-search-first setup, or first-player assumptions as the beginner path.

## Implementation Order

### Slice 1: Readiness Matrix And Route Audit

Create the active package-local MVP readiness matrix. Audit the three MVP routes and all pawn runtime lanes against the five-part completion bar.

Output:

- matrix doc
- route status summary
- top blockers for each route/lane
- updated current-state audit

### Slice 2: Game Shell MVP

Harden the shell route first because every future prototype needs it.

Output:

- guided boot/loading/menu/settings/credits setup path
- credits panel/page support
- scene flow validation
- docs and inspector guidance aligned with the real setup
- PlayMode or scene validation proof for the shell route

### Slice 3: Pawn Runtime Lane Parity

Bring `Sprite2D`, `Billboard2_5D`, and `Rigged3D` to the same MVP authoring bar.

Output:

- route/lane-specific setup guidance
- starter pack or starter-pack variants where needed
- prefab readiness checks for each lane
- movement, input, camera, presentation, health/damage, combat/interaction, projectile, scoring/HUD proof
- explicit limitations per lane

### Slice 4: Non-Pawn Tabletop MVP

Finish the no-pawn guided route enough to make a basic board prototype from scratch.

Output:

- tabletop starter pack path
- board presenter or project-owned presenter guidance
- selection/action queue/turn/win flow proof
- no-pawn validation that avoids pawn/spawn false positives
- docs for simple board prototype setup

### Slice 5: Friend Trial And Friction Capture

Run the beginner path with a collaborator or simulated beginner pass.

Output:

- friction log
- confusing labels or missing guidance
- validation gaps
- doc gaps
- final MVP readiness checklist

### Slice 6: Package Docs Alignment

Update the active Pyralis docs so the source of truth reflects the readiness outcome.

Output:

- `CURRENT_STATE_AUDIT.md`
- setup docs
- feature inventory
- runtime parity/readiness matrix
- roadmap/checkpoint docs

## Success Criteria

Pyralis reaches this MVP checkpoint when:

- a beginner can build the game shell route from guided Unity setup
- a beginner can build a pawn-backed action prototype in `Sprite2D`
- a beginner can build a pawn-backed action prototype in `Billboard2_5D`
- a beginner can build a pawn-backed action prototype in `Rigged3D`
- a beginner can build a no-pawn tabletop prototype
- Setup Flow and inspectors tell the creator what is missing
- validation catches common wrong wiring before the creator has to debug code
- docs describe the supported path, not aspirational features
- tests or validation evidence prove the routes
- known limitations are visible instead of hidden

## Review Notes

This design intentionally keeps special game mechanics out of MVP. Official chess, replay, endless procedural generation, advanced combo systems, and production networking should be built later as game-specific or expansion slices after the core authoring routes are trustworthy.

The readiness bar is strict for the three official pawn lanes. A route cannot be marked ready because `Sprite2D` works while `Billboard2_5D` or `Rigged3D` still only has partial setup proof.
