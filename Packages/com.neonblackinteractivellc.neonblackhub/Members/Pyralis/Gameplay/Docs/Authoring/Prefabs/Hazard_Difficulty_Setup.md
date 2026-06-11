# Hazards and Difficulty Setup - Step-by-Step

Covers `HazardSpawner`, `DifficultyManager`, `HazardData`, and hazard prefab creation for 2D scenes.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Realtime Character for pawn-dodging hazard loops
- Scoring/Objectives when hazards affect score, survival time, rewards, or fail states
- Procedural Generation if hazards will be placed through generated chunks or spawn budgets

Resolve setup-profile validation before creating hazard data, hazard prefabs, difficulty curves, or spawners.

---

## Concepts

- **HazardData** - a ScriptableObject per hazard type, defining visuals, damage, timing, and FX.
- **HazardSpawner** - manages per-type object pools and spawns hazards on the screen during play. Reads all timing from `DifficultyManager`.
- **DifficultyManager** - controls how hard the game is over time: spawn rate, hazard count, shadow/warning durations. Supports Linear, Exponential, Steps, and Wave modes.

---

## Step 1 - Create HazardData assets

1. Right-click in the Project window → **Create → NeonBlack → Gameplay → Hazards → Hazard Data**.
2. Create one asset per unique hazard type (e.g. `HazardData_Falling`, `HazardData_Sliding`).
3. Fill in the Inspector for each:
   - **Damage** - damage dealt on contact.
   - **Slam Duration** - seconds the hazard stays fully visible after landing. `0` uses the DifficultyManager value.
   - **Retract Duration** - seconds to fade out after landing. `0` uses the DifficultyManager value.
   - **Slam Sprite / Shadow Sprite / Warning Sprite** - the sprite assets shown in each animation phase.
   - **Hit FX Prefab** - optional particle spawned on contact.
   - **Hit SFX** - optional audio clip played on contact.
4. Assign each `HazardData` asset to the **Hazard Data** field on your hazard prefab's `Hazard` component.

---

## Step 2 - Set up hazard prefabs

Each hazard prefab needs:
- A `SpriteRenderer` for the hazard body.
- A `Collider2D` (set as trigger) for player detection.
- A `Hazard` component, with `HazardData` assigned.

1. Create or open a hazard prefab in Prefab Mode.
2. Add the `Hazard` component to the root.
3. Drag the appropriate `HazardData` asset into **Hazard Data**.
4. Ensure the `Collider2D` is set to **Is Trigger = true**.
5. Assign **Camera Shake Sink** when the hazard data enables screen shake.
6. Assign **Settings Source** to `SettingsManager` or another `IGameplaySettingsApplier` when hazard audio should follow the player's SFX volume.
7. Save the prefab.

---

## Step 3 - Add DifficultyManager to the scene

1. On a dedicated scene systems object, or on your 2D `GameManager` when using that flow, add Component → `DifficultyManager`.
2. Choose a **Difficulty Mode**:

**Linear** - spawn interval decreases steadily over time.
- **Initial Spawn Interval** - seconds between spawns at game start (e.g. `3`).
- **Min Spawn Interval** - floor; difficulty stops scaling here (e.g. `0.5`).
- **Ramp Duration** - seconds to go from initial to min interval (e.g. `120`).

**Exponential** - faster early ramp, plateaus near minimum.
- Same fields as Linear, plus **Exponent Factor**.

**Steps** - jumps to the next difficulty tier at each threshold.
- Add tiers in the **Steps** array with a **Time Threshold** and **Spawn Interval**.

**Wave** - named waves each with their own timing, hazard counts, and duration. Best for boss-style variety.
- Add entries in the **Waves** array. Each `WaveEntry` has:
  - **Wave Name** - label for readability.
  - **Spawn Interval** - seconds between spawns.
  - **Shadow Duration** - seconds the shadow is visible before the warning flash.
  - **Warning Flash Duration** - seconds the warning flashes before the slam.
  - **Min / Max Hazards** - on-screen count floor and ceiling.
  - **Min / Max Spawn Count** - hazards per burst.
  - **Min / Max Duration** - how long this wave lasts before switching.
- **Wave Pattern Mode** - `Random`, `Sequential`, or `Weighted`.

---

## Step 4 - Add HazardSpawner to the scene

1. Create an empty child under your scene systems root or existing `Spawners` object. Rename it `HazardSpawner`.
2. Add Component → `HazardSpawner`.
3. Wire the Inspector:
   - **Difficulty Manager** - drag the `DifficultyManager` component from Step 3.
   - **Hazard Entries** - add one entry per hazard type:
     - **Prefab** - drag the hazard prefab.
     - **Weight** - relative spawn frequency. `3` spawns three times as often as an entry with `1`.
     - **Pool Size** - pre-pooled instances (e.g. `3-5` per type).
     - **Spawn Size Radius** - half the sprite's largest dimension in world units. Prevents spawning partially off-screen.

---

## Step 5 - Start spawning

`HazardSpawner` begins spawning when `StartSpawning()` is called. `GameManager` calls this automatically and provides gameplay state, camera bounds, hazard outcome, and pickup burst services.

If you are using `HazardSpawner` outside of `GameManager`, call:

```csharp
hazardSpawner.StartSpawning();
```

Call `StopSpawning()` to pause (e.g. game paused, player dead).

---

## Step 6 - Verify in Play Mode

1. Enter Play Mode.
2. Start a session (or manually call `StartSpawning()` from a test button).
3. Observe hazards appearing. Their shadow, warning flash, and slam should follow your timing values.
4. Walk the player into a hazard and confirm damage is applied.

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| No hazards appear | `DifficultyManager` not wired on `HazardSpawner`, or `StartSpawning()` never called |
| Hazards spawn but deal no damage | `Collider2D` is not a trigger, or `HazardData` not assigned on the prefab |
| Pool exhausted warnings in Console | `Pool Size` is too low for your spawn frequency - increase it |
| All hazard types look the same | Same `HazardData` assigned to multiple prefabs by accident - check each prefab |
| Warning flash never shows | `Shadow Duration` + `Warning Flash Duration` both `0` - set at least one > `0` |
