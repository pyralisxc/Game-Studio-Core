# Pyralis RPG Phase 9 Open-Zone Readiness Implementation Plan

**Goal:** Add durable open-zone state contracts before choosing terrain streaming, scene streaming, or world-partition technology.

**Architecture:** Keep open-zone state owner-scoped and backend-neutral. Pyralis owns stable zone ids, travel state, zone flags, per-zone entity snapshots, and reset policies. Projects own scene loading, terrain/streaming tech, spawn placement, and storage transport.

## Completed Slice

- Added `RpgOpenZoneService` as the runtime owner for open-zone state.
- Added `RpgZoneDefinition` with durable zone id, scene id, entrance ids, exit ids, and reset policy.
- Added travel snapshots for current zone, previous zone, last entrance, last exit, and return hub id.
- Added per-zone snapshots for:
  - flags;
  - encounters;
  - resources;
  - pickups;
  - NPC active/spawn/dialogue placement.
- Added reset policy contracts for campaign-persistent, reset-on-run, reset-on-visit, and ephemeral zones.
- Extended `RpgOwnerSaveData` so open-zone snapshots can be captured and applied alongside RPG owner state.
- Added runtime proof for zone travel restoration, per-zone state restoration, reset-on-run behavior, and owner save integration.

## Backend Boundary

Pyralis open-zone readiness does not implement terrain streaming, additive scene loading, save-file transport, encounter spawning, or resource respawn timers. It provides the state contracts those systems need.

A project/world backend should:

- map `RpgZoneDefinition.SceneId` to scenes, Addressables, streamed chunks, or generated areas;
- resolve entrance/exit ids into spawn transforms;
- decide when to capture/apply `RpgOpenZoneSnapshot`;
- apply reset policies at run, visit, or campaign boundaries;
- interpret encounter/resource/pickup/NPC ids against authored or generated content.

## Remaining Phase 9 Work

- Add authored ScriptableObject zone definitions when visual editor setup becomes the focus.
- Add scene/component adapters that push runtime trigger and spawner state into `RpgOpenZoneService`.
- Add open-zone sample content in Phase 10 with one small inspectable route.
