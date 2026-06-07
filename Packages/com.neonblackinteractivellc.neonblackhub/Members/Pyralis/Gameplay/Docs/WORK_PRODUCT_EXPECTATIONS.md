# NeonBlack Gameplay Work Product Expectations

This document defines the default quality bar for all NeonBlack Gameplay module work.

It exists to keep NeonBlack Gameplay commercially minded, modular, readable, and easy to extend across many game types without multiplying one-off tools or legacy paths.

## Primary Priorities

1. Create high-quality, in-depth tools by building on commercial-ready open source libraries and stable engine packages when possible.
2. Prefer improving or reusing an existing tool over creating a new tool.
3. Keep C# and Markdown readable for beginner-to-adept users.
4. Keep docs and files current by updating or removing old and legacy content as work evolves.
5. Prefer modularity and in-engine customization over code-centered truths.

## Additional Standing Priorities

6. Architect for N participants by default. One-player and two-player are configurations, not separate architectural tracks.
7. Keep mode identity data-driven. "Arcade", "brawler", and future game types should primarily differ by composition, profiles, and enabled modules.
8. Prefer Unity-first and ecosystem-first solutions before writing custom framework code.
9. Keep runtime package boundaries clean so shared code can stay shared.
10. Ship tools with enough editor UX, docs, and examples that a teammate can use them without reverse-engineering the source.
11. Treat character-controller gameplay as one supported surface, not the only supported surface. Pyralis should also support participants embodied as cameras, cursors, hands, board seats, factions, and menu selections.
12. Expand features through reusable capability families before genre-specific implementations.

## Default Decision Rules

When choosing between two approaches, prefer the option that:

- reduces bespoke framework code,
- increases Inspector authoring,
- improves reuse across multiple genres,
- preserves a clean migration path,
- is easier to document and test,
- avoids locking the project into a single-player assumption.
- avoids locking the project into a character-controller assumption unless the feature is explicitly pawn-specific.

## Capability-Family Policy

Major new work should declare the capability family it belongs to before implementation.

Current intended capability families:

- platform core: participants, sessions, turns, phases, targeting, action resolution, save/state, and authority contracts
- character and pawn gameplay: movement, traversal, brawler mechanics, pawn presentation, pickups, hazards, encounters, and respawn
- action and targeting: reusable action selection, targeting rules, costs, queues, and resolution contexts
- combat: damage, healing, status effects, teams/factions, reactions, and delivery adapters for brawler, fighter, shooter, tactics, menus, and cards
- guns and projectiles: projectile, hitscan, ammo, reload, pooling, spread, burst, area effects, and faction filtering
- procedural generation: authored chunks, sockets, budgets, seeds, validation, and inspectable generated content
- board, card, and tabletop systems: boards, pieces, decks, hands, zones, turns, legal moves, resources, and action stacks
- presentation: animation mapping, camera, HUD, VFX, audio, and feedback
- samples and templates: playable examples that prove setup paths without becoming hidden framework requirements

If a proposed feature cannot name its capability family, pause and clarify whether it is platform core, reusable feature runtime, presentation, integration, sample content, or game-specific behavior.

## Library And Package Policy

Prefer this order of operations:

1. Use a mature Unity first-party package if it solves the problem well.
2. Use a well-maintained open source library with a commercial-friendly license.
3. Enhance an existing NeonBlack Gameplay tool if the gap is small or medium.
4. Build a new NeonBlack Gameplay tool only when the need is real and reuse is likely.

Before adopting a new external library, document:

- problem being solved,
- why existing NeonBlack Gameplay and Unity tools are not enough,
- maintenance health,
- license suitability for commercial use,
- version and upgrade risk,
- expected package owner in NeonBlack Gameplay.

Avoid adopting libraries that are:

- abandoned,
- poorly documented,
- hard to debug,
- loosely licensed for commercial work,
- narrower than the problem they claim to solve.

## Tool Creation Policy

Do not create a new tool if the need can be met by:

- a profile or data asset,
- a module added to an existing tool,
- a better editor surface for an existing component,
- a thin wrapper around a stable Unity package already in use.

Create a new tool only when:

- it represents a distinct reusable capability,
- it will likely serve multiple game modes,
- it has a clear owner and maintenance path,
- it cannot be modeled cleanly as configuration on an existing tool.

New tools should not assume a pawn, controller, camera, or UI input path unless that assumption is the point of the tool. Prefer accepting participant, action, target, owner, source, and context data through narrow contracts.

## Inspector-First Authoring Policy

The Inspector is the preferred surface for day-to-day design work.

That means NeonBlack Gameplay tools should favor:

- `ScriptableObject` profiles and definitions,
- clear serialized fields,
- strong tooltips and custom editors where they genuinely help,
- prefabs as assembly points,
- mode and feature composition through data and components.

Avoid hiding important configuration in:

- hardcoded constants,
- static gameplay state,
- stringly-typed scene assumptions,
- setup steps that only exist in a developer's memory.

## Modularity Policy

Shared systems should own reusable capability.

Mode-specific layers should mostly answer:

- which modules are enabled,
- which profiles are assigned,
- what rules are tuned,
- what presentation is used.

Avoid cloning a system into multiple folders just because two game modes use it differently.

Instead prefer:

- shared primitives,
- adapter-level composition,
- feature modules,
- profile-driven behavior.

## Multiplayer Policy

NeonBlack Gameplay should assume N participants even when a game only uses one.

Do not tightly couple these concepts:

- participant,
- input owner,
- pawn,
- player slot,
- team,
- network connection.

One-player and two-player games should be authored by configuring the participant model, not by introducing special-case architecture.

## Readability Policy

Code should be easy to scan and explain.

Prefer:

- short classes with focused jobs,
- descriptive names,
- comments for non-obvious behavior,
- docs that explain purpose and usage,
- examples that reflect real NeonBlack Gameplay patterns.

Avoid:

- giant God classes,
- undocumented editor magic,
- hidden runtime coupling,
- stale comments,
- docs that describe an architecture that no longer exists.

## Documentation Policy

Docs are part of the work product, not an afterthought.

Every meaningful refactor should update the relevant Markdown in `Members/Pyralis/Gameplay/Docs/`.

If a doc becomes inaccurate:

- update it immediately, or
- delete/archive it if it no longer serves a current purpose.

Prefer a few accurate docs over many stale docs.

## Refactor Hygiene Policy

When enhancing or replacing a tool:

- preserve working functionality where possible,
- define a migration path,
- mark legacy paths clearly,
- remove dead documentation,
- avoid keeping duplicate systems alive longer than necessary.

Do not leave "temporary" parallel systems undocumented.

## Minimum Definition Of Done

A NeonBlack Gameplay tool or major enhancement is not done until it has:

- a clear purpose,
- a reusable runtime shape,
- Inspector-facing configuration where appropriate,
- updated docs,
- obvious setup guidance,
- reduced or at least not increased architectural duplication,
- no unnecessary new custom framework layer.

## Current Preferred Packages And Directions

These are preferred starting points for refactor work unless a documented exception exists:

- Unity Input System for participant input ownership and local multiplayer flows,
- `ScriptableObject`-driven data and profiles,
- Unity editor tooling for authoring surfaces,
- shared runtime services that avoid single-player-only assumptions,
- modular composition over adapter forks.
- data-driven animation signal mapping over Animator-controller-specific gameplay logic.
- inspectable, seeded procedural systems over opaque generation.
- action/targeting contracts that can serve realtime, turn-based, card, board, and menu-driven play.

See `FEATURE_DEVELOPMENT_SCOPE.md` for the broader feature expansion scope.
