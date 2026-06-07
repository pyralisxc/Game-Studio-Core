# RPG Open-Zone Setup

Pyralis open-zone support is a durable state spine, not a terrain streaming system. Use it when a game needs zones, entrances, exits, resources, encounters, pickups, NPC placement, or hub/world return state to survive scene changes or save/load.

## What Pyralis Owns

- `RpgZoneDefinition` for stable zone ids, scene ids, entrance ids, exit ids, and reset policy.
- `RpgOpenZoneService` for owner-scoped travel and per-zone state.
- Snapshots for zone flags, encounters, resources, pickups, NPC state, and travel state.
- Reset scopes for new visit, new run, or full reset.
- Integration with `RpgOwnerSaveData`.

## What The Project Owns

- Terrain, additive scene loading, Addressables, generated world chunks, or custom streaming technology.
- Mapping zone ids and scene ids to actual loaded content.
- Mapping entrance/exit ids to spawn transforms.
- Encounter spawning, resource timers, pickup GameObjects, and NPC placement.
- When to reset run-scoped or visit-scoped zones.

## Basic Flow

1. Register zones:

```csharp
zones.RegisterZone(new RpgZoneDefinition(
    "zone.meadow",
    "Meadow",
    "scene.meadow",
    RpgZoneResetPolicy.CampaignPersistent,
    new[] { "entrance.hub" },
    new[] { "exit.cave" }));
```

2. Enter and exit zones:

```csharp
zones.EnterZone(owner, "zone.meadow", "entrance.hub", "hub.rpg-proof", out _);
zones.ExitZone(owner, "exit.cave", "zone.cave", out _);
```

3. Record durable state:

```csharp
zones.SetEncounterState(owner, "zone.meadow", "encounter.bandits", RpgZoneEntityStatus.Cleared);
zones.SetResourceState(owner, "zone.meadow", "resource.ore-01", 0, depleted: true);
zones.SetPickupState(owner, "zone.meadow", "pickup.chest-01", RpgZoneEntityStatus.Collected);
zones.SetNpcState(owner, "zone.meadow", "npc.elder", "spawn.market", active: false, "dialogue.after-quest");
```

4. Capture through `RpgOwnerSaveData` when saving:

```csharp
RpgOwnerSaveData save = RpgOwnerSaveData.Capture(
    owner,
    progression,
    inventory,
    equipment,
    quests,
    skills,
    dialogue,
    hubReturn,
    zones);
```

5. Apply after loading:

```csharp
save.ApplyTo(
    progression,
    inventory,
    equipment,
    quests,
    skills,
    dialogue,
    itemResolver,
    zones);
```

Use `RpgZoneResetPolicy.ResetOnRun` for roguelike/survival run spaces, `ResetOnVisit` for areas that rebuild every visit, and `CampaignPersistent` for authored story or open-world zones.
