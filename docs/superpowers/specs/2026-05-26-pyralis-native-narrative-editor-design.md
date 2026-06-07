# Pyralis Native Narrative Editor Design

## Product Intent

Pyralis should provide a native narrative editor that makes NPCs and dialogue easy for beginners while staying powerful enough for RPGs, hubs, brawlers, tabletop games, survival loops, and open-zone prototypes. The goal is not just text branching. The special Pyralis value is gameplay-aware NPC interaction: dialogue can read quests, inventory, skill trees, flags, faction/world state, hub unlocks, and project-owned conditions, then trigger effects such as starting quests, granting items, opening vendors, training skills, launching portals, or setting flags.

Unity does not provide a native full narrative/dialogue editor for this shape. Unity provides useful building blocks such as ScriptableObjects, UI Toolkit, custom inspectors, Localization, Timeline, and graph/editor APIs, but Pyralis should own the authoring model and runtime spine.

## Strategy

Build Pyralis-native dialogue first. External narrative tools are optional adapters later, not foundation requirements.

This avoids forcing users to buy or learn another tool before they can make an NPC talk. It also keeps Pyralis gameplay concepts central instead of bending the RPG platform around Yarn Spinner, Ink, Pixel Crushers, or any other package.

Adapters can come later for power users:

- Yarn/Ink import or runtime bridges for teams that prefer text-script writing.
- Pixel Crushers integration for teams that already own and prefer that asset.
- Export/import paths where practical, with Pyralis conditions/effects remaining the gameplay authority.

## Phase 6A: Runtime Spine And Guided Authoring

Phase 6A should establish the durable gameplay-facing model:

- `NpcDefinition`
- `DialogueGraphDefinition`
- `DialogueNodeDefinition`
- `DialogueChoiceDefinition`
- `DialogueConditionDefinition`
- `DialogueEffectDefinition`
- `DialogueFlagDefinition` or equivalent flag contracts
- `DialogueService` / `InteractionService` runtime contracts

NPCs should have stable ids, display names, roles, tags, optional faction ids, and optional actor/participant links. Dialogue graphs should support speaker lines, player choices, conditions, effects, next-node routing, and terminal nodes. Dialogue sessions should be owner-scoped so multiple participants, board seats, actors, or hub visitors can have independent state.

Conditions should be platform-aware:

- quest started/completed/active
- item count
- skill unlocked
- dialogue flag
- project/world flag
- faction or NPC affinity placeholder
- project-owned custom condition hook

Effects should be platform-aware:

- set dialogue flag
- start quest
- report quest objective progress
- grant item
- grant XP or skill points
- open vendor/trainer/portal hook placeholders
- trigger project-owned custom event

## Phase 6B: Visual Narrative Editor

After the runtime spine and guided inspectors are useful, build a visual editor over the same data model. The graph editor should not introduce a second source of truth.

The visual editor should let creators:

- create and connect dialogue nodes
- add player choices
- attach conditions to choices or nodes
- attach effects to lines, choices, or graph completion
- inspect broken links and missing targets
- preview a dialogue session with simulated RPG state
- search by NPC, quest, item, skill, flag, or text

The first visual editor can be pragmatic and focused. It does not need to match professional writing suites on day one. Its advantage should be that everything is naturally connected to Pyralis gameplay.

## User Experience Principles

The beginner path should feel like:

1. Create an NPC.
2. Write what they say.
3. Add player choices.
4. Pick conditions from friendly dropdowns.
5. Pick effects from friendly dropdowns.
6. Preview the conversation.
7. Fix validation warnings before play.

Do not require users to understand parser syntax, external scripting languages, or event plumbing to make a useful NPC.

Advanced users should still have escape hatches through custom condition/effect ids and later external adapters.

## Architecture Boundaries

Core owns runtime contracts and state. Core must not depend on Unity authoring assets.

Data owns ScriptableObject definitions and adapts them into Core value types/interfaces.

Editor owns guided inspectors, validation display, and later graph tooling.

Runtime/gameplay features consume the dialogue spine through small contracts. Quest, inventory, progression, skill tree, hub, vendor, trainer, portal, and project-owned systems should integrate through condition/effect handlers rather than hard-coded dialogue dependencies.

## Testing

Phase 6A proof:

- runtime tests for condition checks
- runtime tests for effect dispatch
- runtime tests for owner-separated dialogue flags/session state
- runtime tests for quest/inventory/progression integration effects
- editor tests for broken node links, missing NPC ids, invalid condition targets, invalid effect targets, and duplicate ids

Phase 6B proof:

- editor tests for graph asset mutation and validation
- UI Toolkit/editor tests where feasible
- at least one preview/session test for a simple NPC conversation

## Scope Boundaries

Phase 6A should not build the full graph canvas yet. It should make the model, runtime, authoring assets, guided inspectors, validation, and tests strong enough that the graph editor has a stable foundation.

Phase 6B should build the first visual editor. Localization, voice-over pipelines, lip sync, cinematic sequencing, external tool adapters, and writer collaboration workflows are later enhancements unless a specific game demands them.

## Recommended Next Step

Create the implementation plan for Phase 6A: NPC identity, dialogue graph data, condition/effect runtime spine, guided authoring, validation, and tests. Once Phase 6A passes the pre-scene gate, plan Phase 6B as a focused visual editor slice.
