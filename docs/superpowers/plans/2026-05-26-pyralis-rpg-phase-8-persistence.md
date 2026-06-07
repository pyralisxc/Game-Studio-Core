# Pyralis RPG Phase 8 Persistence Implementation Plan

**Goal:** Keep owner-scoped RPG state durable across scene/session boundaries without binding Pyralis to one save backend.

**Architecture:** Pyralis owns schema-versioned owner save contracts and service-level capture/restore hooks. Projects own where those contracts are stored: local JSON, PlayerPrefs, platform cloud save, profile slots, or a custom campaign backend.

## Completed Slice

- Added `RpgOwnerSaveData` as the owner-level save document with a schema version and sections for progression, inventory, equipment, quests, skill unlocks, dialogue, and hub return state.
- Added capture/restore hooks to the RPG runtime services:
  - `ProgressionService`
  - `InventoryService`
  - `QuestService`
  - `EquipmentService`
  - `SkillTreeService`
  - `DialogueService`
- Added `RpgHubReturnSnapshot` so hub return intent can record hub id, scene id, spawn point, last interactable, and requested gameplay scene.
- Added runtime proof that a populated RPG owner can round-trip into fresh services and keep inventory counts, quest progress, equipment loadout, skill unlocks, dialogue flags/session state, progression points, and hub return metadata.
- Added tolerance proof for unknown/missing data: unknown item, quest, skill, and flag ids can load without throwing; missing equipment definitions are skipped.

## Backend Boundary

Pyralis save contracts are pure runtime data. They do not write files, choose slots, encrypt payloads, sync cloud state, or decide project profile policy.

A project save backend should:

- serialize/deserialize `RpgOwnerSaveData`;
- choose save slot/profile/campaign ownership;
- resolve equippable item ids back to authored item definitions when applying equipment;
- handle compression, encryption, cloud conflicts, and platform storage rules;
- migrate old schema versions before calling `ApplyTo`.

## Remaining Phase 8 Work

- Add a concrete sample backend only when a target product chooses storage policy.
- Add broader migration helpers once schema version 2 exists.
- Extend persistence beyond owner-level RPG state into open-zone world state in Phase 9.
