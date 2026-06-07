# Multiplayer Setup — Step-by-Step

Covers local 2-player (and up to 4-player) setup using `SessionDefinition`, `PlayerInputManager`, and multiple spawn points.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Realtime Character for local co-op or versus pawn play
- Camera/Cursor Control when participants control a camera, cursor, menu, faction, or tabletop seat
- Board/Card/Tabletop for local shared-board or shared-card games

Resolve setup-profile validation before assigning participant slots, player input, split-screen settings, spawn points, or camera behavior.

---

## How local multiplayer works

NeonBlack Gameplay uses Unity's **Player Input Manager** for local split-join. Each player presses a button to join, Unity creates a `PlayerInput` component for them, and `ParticipantInputRouter` maps it to the correct `ParticipantDefinition` slot. `ParticipantSpawnService` then spawns that participant's pawn at the next available spawn point.

No networking is involved — this is local-only multiplayer (couch co-op / shared screen or split-screen).

---

## Step 1 — Configure SessionDefinition for multiple players

1. Open your `SessionDefinition` asset.
2. Set the following fields:

| Field | Description |
|---|---|
| **Max Participants** | Maximum number of players allowed (e.g. `2` for 2-player, `4` for 4-player). |
| **Default Participants** | Expand the array. Add one `ParticipantDefinition` per player slot. Each participant needs a `PawnDefinition` that points to their pawn prefab. |
| **Shared Camera By Default** | `true` = all players share one camera that follows the group centroid. `false` = each player gets their own camera. |
| **Allow Split Screen** | `true` = when `Shared Camera By Default` is off, each player's view occupies a portion of the screen. |
| **Allow Late Join** | `true` = players can join after the session has started. |

For a 2-player game with a shared camera:
- **Max Participants**: `2`
- **Shared Camera By Default**: ✓ (checked)
- **Allow Split Screen**: unchecked

For a 2-player game with split-screen:
- **Max Participants**: `2`
- **Shared Camera By Default**: unchecked
- **Allow Split Screen**: ✓ (checked)

---

## Step 2 — Create a ParticipantDefinition for each player

You need one `ParticipantDefinition` per player seat, each with its own `PawnDefinition` assigned.

1. Right-click in the Project window → **Create → NeonBlack → Gameplay → Definitions → Participant Definition**.
2. Name it (e.g. `ParticipantDef_P1`, `ParticipantDef_P2`).
3. Assign a `PawnDefinition` (which points to your player pawn prefab) to each.
4. Open your `SessionDefinition` → expand **Default Participants** → add both `ParticipantDefinition` assets.

---

## Step 3 — Add multiple spawn points in the scene

Each player needs their own spawn point so they do not overlap.

1. In the Hierarchy, create two (or more) empty GameObjects named `SpawnPoint_P1`, `SpawnPoint_P2`, etc.
2. Position them apart from each other — typically side-by-side or at opposite ends of the arena.
3. On your `GameplaySessionBootstrap`, expand the **Spawn Points** array and drag both Transforms in.

`ParticipantSpawnService` assigns spawn points to participants in order — participant 0 gets the first point, participant 1 gets the second, and so on.

---

## Step 4 — Add and configure PlayerInputManager

`PlayerInputManager` manages local join and instantiation of the player input components.

1. In the Hierarchy, right-click → **Create Empty**. Rename it `PlayerInputManager`.
2. Add Component → `PlayerInputManager`.
3. Set the fields:
   - **Joining Enabled** — check this to allow players to join by pressing a button.
   - **Player Prefab** — leave this empty. The Bootstrap and spawn service instantiate pawns from `PawnDefinition` assets, not from the PlayerInputManager prefab slot. If you have an existing PlayerInput prefab for your setup, drag it here.
   - **Max Player Count** — the Bootstrap sets this automatically from `SessionDefinition.GetEffectiveMaxParticipants()`. You do not need to set it manually.
   - **Split Screen** — the Bootstrap configures this from `SessionDefinition.allowSplitScreen`. You do not need to set it manually.
4. On your `GameplaySessionBootstrap`, drag this `PlayerInputManager` component into the **Player Input Manager** field.

> The Bootstrap configures `maxPlayerCount` and `splitScreen` on `PlayerInputManager` at `Awake` — no manual duplication needed.

---

## Step 5 — Set up the shared camera for 2-player (shared view)

If both players share one camera, `CinemachineCameraRigController` automatically tracks the centroid of both pawns and zooms out as they spread apart.

1. Follow [Camera_Setup.md](Camera_Setup.md) for the 3D Cinemachine setup.
2. In your `CameraRigProfile`:
   - **Presentation Mode**: `Shared`
   - **Min Zoom**: set tight enough to frame a single player (e.g. `5`)
   - **Max Zoom**: set wide enough to frame both players at their maximum spread (e.g. `18`)

No additional wiring needed — `CinemachineCameraRigController` reads the participant roster at `LateUpdate` each frame.

---

## Step 6 — Set up split-screen cameras (optional)

If using split-screen:

1. Create one Cinemachine virtual camera per player, named `VC_P1`, `VC_P2`.
2. On `CinemachineCameraRigController`:
   - Leave **Shared Camera Behaviour** empty (or assign `VC_P1`).
   - Expand **Split Screen Camera Behaviours** → add one slot per player → drag `VC_P1`, `VC_P2`.
3. In your `CameraRigProfile`: set **Presentation Mode** to `SplitScreen`.
4. In `SessionDefinition`: set **Allow Split Screen** to `true` and **Shared Camera By Default** to `false`.

---

## Step 7 — Set up PlayerSpawner for 2-player respawn

For each player that can die and respawn:

1. Add a `PlayerSpawner` for each player **or** use a single `PlayerSpawner` per player on their own root object.
2. Assign both spawn point Transforms to the **Spawn Points** array on each spawner so they share the same respawn positions.
3. Enable **Randomise Spawn Point** if you want players to respawn at different points each time.

See [Respawn_Setup.md](Respawn_Setup.md) for the full `PlayerSpawner` field reference.

---

## Step 8 — Verify in Play Mode

1. Enter Play Mode.
2. Player 1 joins automatically (the first slot is populated from `SessionDefinition.defaultParticipants`).
3. Player 2 joins by pressing a button on a second gamepad (or keyboard, if your Input Actions allow it).
4. Confirm both pawns spawn at separate spawn points.
5. Confirm the shared camera frames both players, or split-screen shows separate views.

---

## How join detection works at runtime

```
Player presses button on controller 2
  → PlayerInputManager creates a PlayerInput component for device 2
  → ParticipantInputRouter receives the join event
  → Assigns device 2 to the next unoccupied ParticipantDefinition slot
  → ParticipantSpawnService spawns that participant's pawn at the next spawn point
  → CinemachineCameraRigController updates the tracked group to include the new pawn
```

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| Second player never joins | **Joining Enabled** is off on `PlayerInputManager`, or no second device is connected |
| Both players spawn at the same point | Only one spawn point assigned on `GameplaySessionBootstrap` |
| Second player has no input | `SessionDefinition.defaultParticipants` only has one entry — add a second `ParticipantDefinition` |
| Split-screen does not activate | `SessionDefinition.allowSplitScreen` is off, or `CameraRigProfile.presentationMode` is still `Shared` |
| Camera does not zoom out for 2 players | `CameraRigProfile.maxZoom` is too low — increase it to fit both players |
| Player 2 uses the same pawn as Player 1 | Both `ParticipantDefinition` assets reference the same `PawnDefinition` — create separate ones if you want different characters |
