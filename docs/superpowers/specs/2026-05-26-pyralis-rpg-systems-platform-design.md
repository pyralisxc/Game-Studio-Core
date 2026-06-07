# Pyralis RPG Systems Platform Design

## Intent

Pyralis should gain a reusable RPG Systems Platform: progression, inventory, equipment, skill trees, quests, NPC/dialogue hooks, hubs, persistence, and open-zone state. These systems should enrich existing action, tabletop, survival, brawler, and hybrid game routes without becoming one monolithic RPG template.

## Architecture

The RPG layer is participant-owned and actor-agnostic. Runtime ownership starts from an RPG owner key that can represent a participant, pawn, actor, board piece, faction, NPC, or custom project id. Feature systems consume stable definitions and services instead of reading scene singletons or assuming a first player.

Phase 1 creates stats, XP, levels, skill points, progression definitions, and runtime services. Later phases layer inventory, equipment, skill trees, quests, NPC hooks, hubs, persistence, and open-zone readiness on that spine.

## Capability Families

- RPG Identity, Stats, And Progression
- Inventory And Item Catalog
- Equipment And Effects
- Skill Trees
- Quests And Objectives
- NPC And Dialogue Hooks
- Hub Framework
- Persistence
- Open-Zone Readiness
- Golden RPG Sample

## Cross-Mode Requirement

The RPG systems must support side-scrolling brawlers, tabletop tactics, survival loops, hub-launched minigames, action RPGs, and open-zone prototypes. A skill unlock should be able to grant a brawler combo, chess-piece ability, spell, survival perk, hub portal, traversal move, or dialogue flag.

## Readiness Bar

Every phase must satisfy runtime, authoring, guidance, validation, and proof before moving to ready status. Tests should cover runtime behavior first, then editor validation and Unity-facing proof as authoring surfaces appear.

## First Slice

Build RPG Identity, Stats, And Progression first. It creates the state and event spine needed by inventory, equipment, skills, quests, hubs, persistence, and open-zone state.
