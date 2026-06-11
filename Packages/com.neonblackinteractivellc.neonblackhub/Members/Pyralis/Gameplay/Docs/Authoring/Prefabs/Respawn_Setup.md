# Respawn Setup

Covers `PlayerSpawner` for the 3D death-and-respawn cycle.

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Realtime Character when participants use pawns
- Combat when respawn follows defeat or health depletion
- Scoring/Objectives when lives, checkpoints, or failure states affect the game mode

Resolve setup-profile validation before placing spawn points, participant spawn services, or respawn objects.

## Concepts

There are two spawn layers in NeonBlack Gameplay:

| Layer | What it does | Where to set it up |
|---|---|---|
| `GameplaySessionBootstrap` spawn points | Places pawns in the world when a session starts or a participant joins | assign spawn point `Transform`s on `GameplaySessionBootstrap` |
| `PlayerSpawner` | Watches for death, waits a delay, then moves or recreates the tracked pawn | add `PlayerSpawner` to the scene |

`PlayerSpawner` now supports two runtime styles:

- direct scene-object or prefab respawn
- participant-aware respawn through `ParticipantRosterService` and `ParticipantSpawnService`

## Step 1

Create an empty scene object named `PlayerSpawner` and add the `PlayerSpawner` component.

## Step 2

Choose how the tracked pawn should be resolved.

Direct object path:

- assign **Current Player** to reuse an existing scene object
- or leave **Current Player** empty and assign **Player Prefab**

Participant-aware path:

- assign **Participant Spawn Service**
- assign **Participant Roster Service**
- set **Target Seat Index** to the participant seat this spawner should track

When participant services are present, `PlayerSpawner` resolves the targeted participant first instead of assuming one global player.

## Step 3

Assign one or more respawn points to **Spawn Points**.

- if **Randomise Spawn Point** is off, the first point is used
- if **Randomise Spawn Point** is on, a random point is used
- if the array is empty, the spawner uses its own `Transform.position`

## Step 4

Configure the main timing values:

- **Respawn Delay**: seconds between death and respawn
- **Respawn Shield**: seconds of invulnerability after respawn

## Step 5

Configure the optional lives system:

- **Starting Lives**: `0` means infinite lives
- positive values use a limited-lives flow
- when lives run out, `PlayerSpawner` fires **On Game Over** instead of respawning

## Step 6

Optional countdown UI:

- **Show Countdown**
- **Countdown Format**
- **Countdown Font Size**
- **Countdown Color**

The countdown label is created at runtime on a screen-space overlay canvas.

## Step 7

Configure health restoration:

- **Respawn HP Fraction**: fraction of max health restored on respawn

## Step 8

Optional events:

- **On Before Respawn**
- **On After Respawn**
- **On Game Over**

## Step 9

Verify in Play Mode:

1. Kill the tracked pawn.
2. Confirm the pawn is disabled and the countdown appears if enabled.
3. Confirm the pawn respawns at a valid spawn point.
4. Confirm the respawn HP fraction and shield behave as expected.
5. If using limited lives, confirm the out-of-lives path fires **On Game Over**.

## Common Mistakes

| Problem | Likely cause |
|---|---|
| Spawner does nothing on death | the tracked pawn has no `HealthComponent`, so `PlayerSpawner` cannot subscribe to `OnDeath` |
| Wrong participant respawns | `Target Seat Index` does not match the intended roster seat |
| Respawn creates the wrong pawn | participant services are missing, so the spawner falls back to `Player Prefab` |
| Player respawns in the wrong place | spawn points are missing or attached to moving scene objects |
| Countdown is invisible | countdown is disabled or its color blends into the background |
