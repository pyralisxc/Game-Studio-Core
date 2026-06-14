# Encounter Zones Setup - Step-by-Step

Covers `ArenaZone`, `CameraZone`, and `DamageZone` for 3D scenes.

---

## Before You Wire This

Start with a `SessionDefinition` assigned to `GameplaySessionBootstrap.sessionDefinition` and a `GameModeDefinition` assigned to `SessionDefinition.defaultGameMode`.

Recommended route capabilities:

- Realtime Character
- Combat
- Camera/Cursor Control for camera zone transitions
- Scoring/Objectives if encounters award score or progression

Resolve route validation before placing arena triggers, camera zones, damage zones, or enemy spawners.

---

## Concepts

- **ArenaZone** - a combat room trigger. When the player enters, linked `EnemySpawner` objects activate and exit is blocked until all enemies are dead. Fires events on entry and clear.
- **CameraZone** - a trigger that switches the `CinemachineCameraRigController` to a `CameraRigProfile` asset when the player enters, and optionally reverts on exit. Use for framing transitions between wide establishing shots and tight combat views.
- **DamageZone** - a persistent damage volume: pits, fire floors, poison clouds, kill planes.

---

## Part A - ArenaZone

### Step 1 - Set up the arena collider

1. Create an empty GameObject. Rename it (e.g. `Arena_Kitchen_1`).
2. Add a **BoxCollider** component.
3. Set **Is Trigger = true**.
4. Size and position the collider to cover the full playable area of this combat section.

### Step 2 - Add ArenaZone

1. Add Component â†’ `ArenaZone`.

### Step 3 - Wire enemy spawners

1. Expand the **Enemy Spawners** array.
2. Drag each `EnemySpawner` GameObject that should activate for this arena into the array.

> Enemy spawners start disabled. `ArenaZone` enables them when the player enters, so enemies do not pre-spawn before the player arrives.

### Step 4 - Wire exit blockers (optional)

Exit blockers are GameObjects (wall colliders, gate meshes, invisible walls) that physically prevent the player from leaving until the arena is cleared.

1. Expand the **Exit Blockers** array.
2. Drag each blocker GameObject in.
3. Blockers start inactive and are enabled on player entry, then deactivated when the zone is cleared.

### Step 5 - Set camera profile switches (optional)

- **On Enter Camera Profile** - assign a `CameraRigProfile` asset to switch to when the player enters. Leave empty for no switch.
- **On Clear Camera Profile** - assign a `CameraRigProfile` asset to switch to when all enemies are dead. Leave empty for no switch.

### Step 6 - Wire events (optional)

- **On Entered** - fires the first time the player enters. Use for cutscene triggers, music changes, or UI banners.
- **On Cleared** - fires once all enemies are dead. Use to unlock doors, play a fanfare, or award bonus points.

### Step 7 - Verify

1. Enter Play Mode.
2. Walk into the arena collider - enemy spawners should activate and exit blockers should appear.
3. Defeat all enemies - exit blockers should deactivate and `OnCleared` should fire.

---

## Part B - CameraZone

### Step 1 - Set up the collider

1. Create an empty GameObject. Rename it (e.g. `CameraZone_Wide`).
2. Add a **BoxCollider** component. Set **Is Trigger = true**.
3. Size and position to cover the area where you want the camera profile to change.

### Step 2 - Add CameraZone

1. Add Component â†’ `CameraZone`.
2. Set the fields:
   - **On Enter Profile** - drag the `CameraRigProfile` asset to activate when the player enters.
   - **On Exit Profile** - drag the `CameraRigProfile` asset to revert to when the player exits. Leave empty for no revert.
   - **Transition Duration** - blend time in seconds (e.g. `0.5`).
   - **One Shot** - enable if this zone should only trigger once (e.g. a cinematic camera lock that should not snap back).
3. Wire **On Player Entered** and **On Player Exited** UnityEvents if you need extra logic.

> The player GameObject must have the tag `Player` for trigger detection to work.

---

## Part C - DamageZone

### Step 1 - Set up the collider

1. Create an empty GameObject. Rename it (e.g. `DamageZone_FireFloor`).
2. Add a **BoxCollider** component. Set **Is Trigger = true**.
3. Position it over the hazard area (floor, pit, spike wall, etc.).

### Step 2 - Add DamageZone

1. Add Component â†’ `DamageZone`.
2. Set the fields:

**Damage**
- **Damage Per Tick** - damage applied each interval while inside the zone (e.g. `10` for fire, `9999` for an instant-kill pit).
- **Tick Interval** - seconds between damage ticks (e.g. `0.5` for fire, `0.1` for a kill pit).
- **Knockback Force** - force applied on each tick. `0` = no knockback. Use a small value to push players away from walls.

**Targeting**
- **Targeting** - choose who takes damage:
  - `All` - everyone (environmental hazard).
  - `PlayerOnly` - only the player (enemy-placed hazard).
  - `EnemyOnly` - only enemies (player-placed hazard or friendly fire zone).

**Events** (optional)
- **On Target Entered** - fires when any valid target first enters.
- **On Target Exited** - fires when a target leaves.

### Common DamageZone recipes

| Use case | Damage Per Tick | Tick Interval | Knockback | Targeting |
|---|---|---|---|---|
| Fire floor | `5` | `0.5` | `0` | `All` |
| Poison cloud | `3` | `1.0` | `0` | `PlayerOnly` |
| Kill pit | `9999` | `0.1` | `0` | `All` |
| Spike wall | `15` | `0.25` | `3` | `PlayerOnly` |

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| ArenaZone enemies spawn before player enters | `EnemySpawner` GameObjects were manually set active - leave them inactive; `ArenaZone` enables them |
| Arena never clears | `EnemySpawner` spawning enemies without `HealthComponent` - every tracked enemy must have one |
| CameraZone has no effect | Player tag is not `Player`, or **On Enter Profile** field is empty |
| DamageZone deals no damage | `BoxCollider.Is Trigger` is off, or nothing with `HealthComponent` is entering the volume |
| Player killed instantly by fire | `Damage Per Tick` too high - reduce it or increase `Tick Interval` |
