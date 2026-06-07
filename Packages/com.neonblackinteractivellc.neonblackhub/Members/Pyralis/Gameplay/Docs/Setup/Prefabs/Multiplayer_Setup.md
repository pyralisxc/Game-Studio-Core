# Multiplayer Setup - Step-by-Step

Covers two different multiplayer routes:

- Local multiplayer through `SessionDefinition`, `PlayerInputManager`, and multiple spawn points.
- Networked MVP setup through `SessionDefinition.networkMode`, Unity Netcode for GameObjects, and Unity Transport.

Do not mix those routes accidentally. Local multiplayer is for one Unity process with multiple local participants. NGO networking is for host/client/server authority and network-spawned pawns.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Realtime Character for local co-op or versus pawn play
- Camera/Cursor Control when participants control a camera, cursor, menu, faction, or tabletop seat
- Board/Card/Tabletop for local shared-board or shared-card games

Resolve setup-profile validation before assigning participant slots, player input, split-screen settings, spawn points, or camera behavior.

---

## How Local Multiplayer Works

NeonBlack Gameplay uses Unity's `PlayerInputManager` for local split-join. Each player presses a button to join, Unity creates a `PlayerInput` component for them, and `ParticipantInputRouter` maps it to the correct `ParticipantDefinition` slot. `ParticipantSpawnService` then spawns that participant's pawn at the next available spawn point.

No NGO session is involved in this guide. This is local-only multiplayer: couch co-op, shared screen, or split-screen through Unity Input System participants.

The package includes Unity Netcode for GameObjects and Unity Transport. Those dependencies do not make this local route networked. Use the NGO route only when the game has explicit server/client authority, network ownership, and replicated spawn rules.

## How NGO Multiplayer Works

Pyralis does not write the low-level network transport itself for the MVP. Unity Netcode for GameObjects and Unity Transport own connection and transport behavior. Pyralis owns the authoring chain around them: session intent, participant ownership, authority, roster/session services, spawn service adapters, setup guidance, and validation.

The supported networking MVP uses `SessionDefinition.networkMode`:

- `LocalOnly` - local/offline services
- `NetcodeHost` - NGO host path
- `NetcodeClient` - NGO client path
- `NetcodeServer` - NGO dedicated/server path

When the session selects an NGO mode, `GameplaySessionBootstrap` uses the networking assembly's session, roster, spawn, ownership, and authority services instead of the local service lane.

Required scene wiring:

- one `NetworkManager`
- `UnityTransport` assigned as the Network Transport
- pawn prefabs include `NetworkObject`
- pawn prefabs are registered in Network Prefabs
- feature modules that are networked declare their `Network Role`, `Replication Policy Id`, and ownership/server/prediction flags

This MVP is ready for prefab and scene setup. It proves network-aware session startup, participant ownership metadata, participant-specific pawn ownership, local-authority checks against the resolved owner client, and server-side pawn spawn/despawn. It does not yet claim rollback, movement reconciliation, projectile reconciliation, remote input command streaming, lobby/matchmaking, relay, dedicated fleet orchestration, or replicated animation state.

## Network Chain MVP Quick Path

1. Create or open a `SessionDefinition`.
2. Set **Network Mode** to `NetcodeHost`, `NetcodeClient`, or `NetcodeServer`.
3. Keep **Local First** off for networked sessions. `SessionDefinition.Sanitize()` will force this off for NGO modes, but authors should still treat it as a local-only option.
4. Add one `NetworkManager` to the scene.
5. Add `UnityTransport` to the same object or another scene object and assign it to `NetworkManager.NetworkConfig.NetworkTransport`.
6. Add `NetworkObject` to every pawn prefab spawned by networked participants.
7. Register every network-spawned pawn prefab in `NetworkManager.NetworkConfig.Prefabs`.
8. Assign the networked `SessionDefinition` to `GameplaySessionBootstrap`.
9. Open the `GameplaySessionBootstrap` guided inspector or run scene readiness validation before pressing Play.

When this path is clean, the scene is ready for a host/client prototype. Game-specific replication such as board-state replay synchronization, beat-em-up projectile reconciliation, or arena match flow should layer on top of the participant/session/authority services instead of bypassing them.

---

## Step 1 - Configure SessionDefinition For Multiple Players

1. Open your `SessionDefinition` asset.
2. Set the following fields:

| Field | Description |
|---|---|
| **Max Participants** | Maximum number of players allowed, such as `2` for 2-player or `4` for 4-player. |
| **Default Participants** | Add one `ParticipantDefinition` per player slot. Each pawn-backed participant needs a `PawnDefinition` that points to its pawn prefab. |
| **Shared Camera By Default** | `true` means all players share one camera that follows the group centroid. `false` means each player gets their own camera. |
| **Allow Split Screen** | `true` means each player's view occupies a portion of the screen when `Shared Camera By Default` is off. |
| **Allow Late Join** | `true` means players can join after the session has started. |

For a 2-player game with a shared camera:

- **Max Participants**: `2`
- **Shared Camera By Default**: checked
- **Allow Split Screen**: unchecked

For a 2-player game with split-screen:

- **Max Participants**: `2`
- **Shared Camera By Default**: unchecked
- **Allow Split Screen**: checked

---

## Step 2 - Create A ParticipantDefinition For Each Player

You need one `ParticipantDefinition` per player seat, each with its own `PawnDefinition` assigned for pawn-backed routes.

1. Right-click in the Project window and choose **Create > NeonBlack > Definitions > Participant Definition**.
2. Name it, for example `ParticipantDef_P1` or `ParticipantDef_P2`.
3. Assign a `PawnDefinition` that points to the player pawn prefab.
4. Open your `SessionDefinition`, expand **Default Participants**, and add both `ParticipantDefinition` assets.

---

## Step 3 - Add Multiple Spawn Points In The Scene

Each pawn-backed participant needs its own spawn point so players do not overlap.

1. In the Hierarchy, create two or more empty GameObjects named `SpawnPoint_P1`, `SpawnPoint_P2`, and so on.
2. Position them apart from each other, typically side-by-side or at opposite ends of the arena.
3. On your `GameplaySessionBootstrap`, expand the **Spawn Points** array and drag both Transforms in.

`ParticipantSpawnService` assigns spawn points to participants in order: participant 0 gets the first point, participant 1 gets the second, and so on.

---

## Step 4 - Add And Configure PlayerInputManager

`PlayerInputManager` manages local join and instantiation of player input components.

1. In the Hierarchy, right-click and choose **Create Empty**. Rename it `PlayerInputManager`.
2. Add the `PlayerInputManager` component.
3. Set the fields:
   - **Joining Enabled**: check this to allow players to join by pressing a button.
   - **Player Prefab**: leave this empty unless your setup has a dedicated input prefab. The Bootstrap and spawn service instantiate pawns from `PawnDefinition` assets.
   - **Max Player Count**: the Bootstrap sets this from `SessionDefinition.GetEffectiveMaxParticipants()`.
   - **Split Screen**: the Bootstrap configures this from `SessionDefinition.allowSplitScreen`.
4. On your `GameplaySessionBootstrap`, drag this `PlayerInputManager` component into the **Player Input Manager** field.

The Bootstrap configures `maxPlayerCount` and `splitScreen` on `PlayerInputManager` at `Awake`, so do not duplicate those values manually unless you are overriding the bootstrap path.

`ParticipantInputRouter` subscribes to this manager's player joined/left events. Multi-participant local join scenes should assign the manager on `GameplaySessionBootstrap`; single-player, board, card, menu, or custom-input scenes can leave it empty unless they intentionally use Unity local join.

---

## Step 5 - Set Up The Shared Camera

If players share one camera, `CinemachineCameraRigController` can track the centroid of all active participant pawns and zoom out as they spread apart.

1. Follow [Camera_Setup.md](Camera_Setup.md) for the 3D Cinemachine setup.
2. In your `CameraRigProfile`:
   - **Presentation Mode**: `Shared`
   - **Min Zoom**: tight enough to frame one player, such as `5`
   - **Max Zoom**: wide enough to frame players at their maximum spread, such as `18`

No additional wiring is needed when the camera rig reads the participant roster at `LateUpdate`.

---

## Step 6 - Set Up Split-Screen Cameras

If using split-screen:

1. Create one **GameObject -> Cinemachine -> Cinemachine Camera** per player, named `VC_P1`, `VC_P2`, and so on.
2. On `CinemachineCameraRigController`:
   - Leave **Shared Camera Behaviour** empty or assign `VC_P1`.
   - Expand **Split Screen Camera Behaviours**, add one slot per player, and assign the CinemachineCamera components.
3. In your `CameraRigProfile`, set **Presentation Mode** to `SplitScreen`.
4. In `SessionDefinition`, set **Allow Split Screen** to `true` and **Shared Camera By Default** to `false`.

---

## Step 7 - Set Up PlayerSpawner For Respawn

For each player that can die and respawn:

1. Add a `PlayerSpawner` for each player or use one `PlayerSpawner` per player root.
2. Assign spawn point Transforms to the **Spawn Points** array.
3. Enable **Randomise Spawn Point** if players should respawn at different points each time.

See [Respawn_Setup.md](Respawn_Setup.md) for the full `PlayerSpawner` field reference.

---

## Step 8 - Verify In Play Mode

1. Enter Play Mode.
2. Player 1 joins automatically if the first slot is populated from `SessionDefinition.defaultParticipants`.
3. Player 2 joins by pressing a button on a second gamepad or another configured device.
4. Confirm both pawns spawn at separate spawn points.
5. Confirm the shared camera frames both players, or split-screen shows separate views.

---

## Runtime Join Flow

```text
Player presses button on controller 2
  -> PlayerInputManager creates a PlayerInput component for device 2
  -> ParticipantInputRouter receives the join event
  -> Assigns device 2 to the next unoccupied ParticipantDefinition slot
  -> ParticipantSpawnService spawns that participant's pawn at the next spawn point
  -> CinemachineCameraRigController updates the tracked group to include the new pawn
```

---

## Common Mistakes

| Problem | Likely cause |
|---|---|
| Second player never joins | **Joining Enabled** is off on `PlayerInputManager`, or no second device is connected. |
| Both players spawn at the same point | Only one spawn point is assigned on `GameplaySessionBootstrap`. |
| Second player has no input | `SessionDefinition.defaultParticipants` only has one entry. Add a second `ParticipantDefinition`. |
| Split-screen does not activate | `SessionDefinition.allowSplitScreen` is off, or `CameraRigProfile.presentationMode` is still `Shared`. |
| Camera does not zoom out for 2 players | `CameraRigProfile.maxZoom` is too low. Increase it to fit both players. |
| Player 2 uses the same pawn as Player 1 | Both `ParticipantDefinition` assets reference the same `PawnDefinition`. Create separate ones if you want different characters. |
| Networked scene still acts local | `SessionDefinition.networkMode` is still `LocalOnly`, or the scene is using `PlayerInputManager` expectations instead of the NGO route. |
| Host/client scene fails readiness | Missing `NetworkManager`, missing `UnityTransport`, missing pawn `NetworkObject`, or pawn prefab not registered in Network Prefabs. |
| Every participant seems locally controlled | Gameplay code is checking host/client status directly instead of using Pyralis participant authority services. |
