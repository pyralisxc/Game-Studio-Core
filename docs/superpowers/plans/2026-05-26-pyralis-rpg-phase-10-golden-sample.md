# Pyralis RPG Phase 10 Golden Sample Implementation Plan

**Goal:** Provide one small RPG route that proves the platform pieces work together before Cameron spends time placing visuals in Unity.

**Architecture:** Keep the first golden sample code-backed and scene-neutral. `RpgGoldenSampleFactory` creates the definitions and services; `RpgGoldenSampleRuntime` drives the route. Unity scenes can then bind visuals, triggers, and UI panels to the already-tested sample ids.

## Completed Slice

- Added stable golden sample ids for hub, NPC, quest, vendor, items, equipment, skill tree, meadow zone, encounter, resource, pickup, and scene ids.
- Added `RpgGoldenSampleFactory` to create the sample hub, guide dialogue, quest, vendor, equipment reward, skill tree, meadow zone, and service graph.
- Added `RpgGoldenSampleRuntime` as a small scene-neutral harness for:
  - guide dialogue;
  - quest acceptance;
  - vendor purchase;
  - meadow entry;
  - herb collection and quest completion;
  - cape reward equip;
  - wisdom skill unlock;
  - portal scene request;
  - save capture and restore.
- Added runtime proof that the complete route survives save/load, including inventory, quest completion, equipment, skill unlock, dialogue flag, open-zone travel, encounter state, and hub return scene metadata.

## Unity Handoff

Cameron can now do the visual/content pass in Unity without inventing gameplay plumbing:

- create a hub scene using `RpgGoldenSampleIds.HubSceneId` as the scene id;
- create a meadow/gameplay scene using `RpgGoldenSampleIds.MeadowSceneId`;
- place visible objects matching the sample interactable ids;
- bind UI panels to the existing RPG HUD/panel presenters;
- use the golden ids for prompts, dialogue, quest board, vendor, loadout, trainer, and meadow portal;
- treat the test as the source of truth for the route until a richer imported sample scene exists.

## Remaining Phase 10 Work

- Imported sample scenes/prefabs with placeholder art.
- Optional editor helper to instantiate a visual sample root.
- Manual Unity playthrough polish: camera, object placement, UI skin, and input feel.
