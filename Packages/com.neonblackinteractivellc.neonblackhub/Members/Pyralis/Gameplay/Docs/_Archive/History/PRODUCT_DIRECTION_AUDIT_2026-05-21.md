# Pyralis Product Direction Audit - 2026-05-21

Status: historical snapshot. Use CURRENT_STATE_AUDIT.md, CURRENT_ARCHETYPE_QUALITY_AUDIT_2026-05-23.md, and Docs/Setup/START_HERE.md for current development status. This file is kept as product-direction history and should not be treated as the active gap list.

## Read

Pyralis is no longer best understood as a single game codebase. It is becoming a Unity gameplay platform inside Neon Black Hub: a reusable, inspector-authored toolkit for building multiple 2D, 2.5D, and 3D game types from shared participant, pawn, feature, presentation, and networking concepts.

The strongest current offer is:

- a shared gameplay package for rapidly composing arcade score loops, hazard loops, brawler combat, encounter rooms, and hybrid prototypes
- a data-driven authoring model built around `SessionDefinition`, `ParticipantDefinition`, `PawnDefinition`, `GameModeDefinition`, profiles, and feature modules
- an N-participant architecture that treats one-player and two-player as configurations rather than separate code paths
- reusable pawn stacks for `Sprite2D`, `Billboard2_5D`, and `Rigged3D`
- a growing editor-facing workflow with custom inspectors, validation, setup docs, and example authoring assets
- a future expansion path into actor-agnostic action, targeting, procedural generation, guns/projectiles, board, card, turn-based, and tabletop systems

## Direction

The project is heading toward a platform/product hybrid:

1. `Neon Black Hub` is the package wrapper and shared distribution surface.
2. `Pyralis Gameplay` is the real active product: a modular gameplay framework for Neon Black game projects.
3. The game content in `Assets/Jim/Assets` shows two likely proving grounds:
   - `La Cucarachacha`: 2D arcade/hazard/pickup/mobile score-loop energy.
   - `Apocalyptia`: side-scrolling action/platform/brawler exploration.
4. The package architecture is moving from prototype scripts to a reusable commercial toolkit.

That direction is coherent. The code, docs, asmdefs, and tests mostly reinforce the same story.

## Current Strengths

- The package compiles across runtime, editor, runtime tests, and editor tests after restoring missing `Temp/obj` assets.
- The active package has meaningful domain ownership: `Core`, `Data`, `Characters`, `Presentation`, `Networking`, and feature assemblies for combat, traversal, feedback, interaction, pickups, and scoring.
- The docs are unusually explicit about intent, setup, governance, and migration rules.
- `GameplaySessionBootstrap`, `ParticipantRosterService`, `ParticipantSpawnService`, `PawnRoot`, and `ActorFeatureHost` give the project a credible shared-core spine.
- The authored data model is real, not just theoretical. The pawn starter pack shows the intended workflow.
- The platform already has enough feature surface to support multiple playable directions: pickups, hazards, combat, status effects, enemy behavior, traversal, scoring, feedback, menus, settings, camera, and basic networking seams.

## Main Gaps

### 1. Product Positioning Is Still Blurry

The code says "multi-game gameplay platform." The assets say "we are also building actual games." The docs mostly explain architecture, not the market-facing or team-facing offer.

Recommended phrasing:

> Pyralis is Neon Black's modular Unity gameplay platform for making fast, reusable 2D/2.5D/3D action games from authored gameplay parts.

### 2. Editor UX Has Improved, But Deep Scene Repair Still Trails The Architecture

The runtime architecture is still ahead of deep scene repair actions, but the authoring surface has moved materially since this audit. Guided inspectors now cover concrete Gameplay `MonoBehaviour` and `ScriptableObject` surfaces, and `GameplaySessionBootstrap` setup flow validates the selected session/mode/setup profile chain, pawn prefab internals, scoring services, projectile launcher presence, gameplay state, camera bounds, and runtime-system claims. The next maturity jump is deeper lane-specific repair actions and Play Mode proof, not basic inspector coverage.

### 3. Compatibility Surfaces Still Pull Back Toward Single-Player

The active Gameplay runtime consumers of the protected singleton/global lookup list have been removed. The remaining concern is softer but still real: some compatibility-facing surfaces and persistent service implementations still carry one-primary-player or singleton-style ownership assumptions, so new work should keep routing through participant/session/service seams instead of reviving direct static reads.

### 4. Feature Folder Shape Is Partly Normalized, Not Fully Final

Some feature domains follow the emerging `Runtime/Shared`, `Runtime/2D`, `Runtime/3D`, `Data`, `Editor`, `Tests`, `Docs` shape. Others still use simpler `2D`, `3D`, `UI`, or root-level layouts. This is manageable, but the docs should stay honest that the folder template is the target, not universal current reality.

### 5. Game Proof Is Behind Platform Proof

The package is compiling and has architecture/test coverage, but the visible project assets do not yet show a complete shipped loop inside the package. The next major proof point should be one polished vertical slice that uses the canonical path end to end.

## Best Next Slice

Build and document one canonical playable vertical slice using Pyralis as intended.

Recommended slice:

- `GameplaySessionBootstrap`
- one `SessionDefinition`
- one or two `ParticipantDefinition` assets
- one `PawnDefinition`
- `PawnRoot`
- the 2D stack for a La Cucarachacha-style pickup/hazard score loop
- scoring, feedback, camera, settings, and one menu flow
- no new architecture unless the slice exposes a genuine missing seam

This would answer the most important product question: not "is Pyralis architecturally clean?" but "can someone build a fun, inspectable game loop with it without reverse-engineering the source?"

## Strategic Recommendation

Keep Pyralis as one package for now, but treat it as a platform with one flagship sample game.

Do not split into multiple packages yet. Do not chase every feature assembly to perfect final shape before proving the player-facing loop. The platform has crossed the line from "needs architecture rescue" into "needs product proof, editor guidance, and sample completeness."

The current north star should be:

> Make the canonical authored path easier, faster, and more obviously correct than the old compatibility paths.

## Expanded Scope Note

After the follow-up feature scope discussion, the intended direction is broader than character-controller action games.

Pyralis should support participants that control:

- pawns
- cameras
- cursors
- board seats
- card hands
- factions
- menu selections

Future feature work should move through reusable capability families first:

- action and targeting
- combat outcomes
- guns and projectiles
- procedural generation
- animation and Animator compatibility
- board, card, turn, phase, and tabletop systems

This scope is now captured in `FEATURE_DEVELOPMENT_SCOPE.md`.

## Verification Notes

Commands run:

- `dotnet restore Neonblackinteractivellc.Neonblackhub.csproj`
- `dotnet restore Neonblackinteractivellc.Neonblackhub.Editor.csproj`
- `dotnet restore Neonblackinteractivellc.Neonblackhub.Tests.csproj`
- `dotnet restore Neonblackinteractivellc.Neonblackhub.Editor.Tests.csproj`
- `dotnet build Neonblackinteractivellc.Neonblackhub.csproj --no-restore`
- `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.csproj --no-restore`
- `dotnet build Neonblackinteractivellc.Neonblackhub.Tests.csproj --no-restore`
- `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.Tests.csproj --no-restore`

Result: all four builds passed with zero warnings and zero errors after restore.

Unity Test Runner was not launched in this audit pass, so scene/import/play-mode behavior still needs Editor validation.
