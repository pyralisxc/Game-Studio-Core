# RPG Golden Sample Setup

The Golden RPG Sample is currently code-backed. It proves the route in runtime tests before you spend time arranging a visual Unity scene.

## What Is Already Wired In Code

- `RpgGoldenSampleFactory.CreateRuntime()` creates the sample owner, hub, guide NPC, dialogue graph, quest, vendor, cape reward, skill tree, meadow zone, and RPG services.
- `RpgGoldenSampleRuntime` drives the route:
  - talk to the guide;
  - accept the herb quest;
  - buy a potion;
  - enter the meadow;
  - collect herbs and complete the quest;
  - receive and equip the wisdom cape;
  - unlock the wisdom skill;
  - request the meadow scene;
  - capture and restore save state.

## Unity Visual Pass

Create two scenes or scene placeholders:

- `scene.golden-rpg-hub`
- `scene.golden-rpg-meadow`

In the hub scene, place simple objects for:

- `golden.talk.guide`
- `golden.quest.board`
- `golden.vendor.apothecary`
- `golden.loadout.station`
- `golden.trainer`
- `golden.portal.meadow`

Use the existing RPG HUD and panel presenters for prompts, dialogue, quest board, vendor, loadout, and trainer panels. The ids in `RpgGoldenSampleIds` are the source of truth for scene labels, trigger names, and temporary placeholder object names.

In the meadow scene, place simple objects for:

- `pickup.golden.herb-cache`
- `encounter.golden.bandits`
- `resource.golden.ore`
- `npc.golden.guide`

The current code does not force a scene layout. It gives you a verified route and stable ids so the visual pass can focus on taste, camera, spacing, and feel.

## Verification

`RpgGoldenSampleRuntimeTests` proves the whole code-backed route and save/load flow. When a visual scene is imported, add a PlayMode scene test or manual validation checklist that confirms the placed objects call the same route ids.
