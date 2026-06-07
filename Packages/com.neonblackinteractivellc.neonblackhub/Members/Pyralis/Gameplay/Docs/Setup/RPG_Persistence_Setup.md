# RPG Persistence Setup

Pyralis provides owner-scoped save contracts, not a final storage backend. This keeps the gameplay package usable for local prototypes, campaign slots, cloud saves, and project-owned account systems.

## What Pyralis Owns

- `RpgOwnerSaveData` as the schema-versioned owner save document.
- Snapshot contracts for progression, inventory, equipment, quest progress, skill unlocks, dialogue flags/session state, and hub return metadata.
- Capture/restore methods on the RPG runtime services.
- Tolerant restore behavior for missing or unknown data.

## What The Project Owns

- Save slot, profile, campaign, or account ownership.
- File path, PlayerPrefs key, cloud-save record, or database row layout.
- Serialization format and transport.
- Compression, encryption, cloud conflict policy, and backup policy.
- Migration from older schema versions before applying state.
- Resolving saved equipment item ids back to authored `IEquippableItem` definitions.

## Basic Flow

1. Build the runtime services for the owner.
2. Capture state before leaving a hub, saving a checkpoint, or closing the session:

```csharp
RpgOwnerSaveData save = RpgOwnerSaveData.Capture(
    owner,
    progression,
    inventory,
    equipment,
    quests,
    skills,
    dialogue,
    hubReturn);
```

3. Store `save` through the project backend.
4. Load and migrate the document if needed.
5. Apply it to fresh runtime services:

```csharp
save.ApplyTo(
    progression,
    inventory,
    equipment,
    quests,
    skills,
    dialogue,
    itemId => itemCatalog.ResolveEquippable(itemId));
```

If the resolver cannot find an equipment item, that slot is skipped. Unknown inventory, quest, skill, and dialogue flag ids are preserved so projects can load saves before all content bundles are available.
