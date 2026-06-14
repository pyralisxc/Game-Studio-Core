# Pickups Setup - Step-by-Step

Covers `CollectibleSpawner2D`, `Collectible2D`, and `CollectibleFeedback2D`.

---

## Before You Wire This

Start with a `SessionDefinition` assigned to `GameplaySessionBootstrap.sessionDefinition` and a `GameModeDefinition` assigned to `SessionDefinition.defaultGameMode`.

Recommended route capabilities:

- Realtime Character when pawns collect pickups
- Scoring/Objectives when pickups award points, time, currency, or progress
- Procedural Generation if pickups will be placed by generated chunks or spawn budgets

Resolve route validation before creating pickup prefabs, spawners, score hooks, or feedback objects.

---

## Concepts

- **Collectible2D** - the canonical 2D pickup implementation. It bobs in place, collects on overlap, and returns itself to the spawner pool.
- **CollectibleSpawner2D** - the canonical 2D pickup pool and spawn controller.
- **CollectibleFeedback2D** - the canonical award sink for pickup feedback and score-side effects. Pickups can resolve an `IPickupAwardSink` from an explicit override, a parent object, or active gameplay services. Its score route is explicit: assign **Score Award Source** to `ParticipantScoreService` or another `ISessionScoreAwardSink`. Audio mixer routing is required only when collect or destroy clips are assigned; score-only pickup proofs do not need audio setup.

---

## Step 1 - Create the pickup prefab

1. In the Project window, right-click -> **Create -> 2D Object -> Sprites -> Square** or use your own pickup sprite.
2. Name the prefab `Collectible_Point` or another content-facing name.
3. Add the following components to the root:
   - **SpriteRenderer**
   - **CircleCollider2D** with **Is Trigger = true**
   - **Collectible2D**
4. In the `Collectible2D` Inspector, configure:
   - **Bob Speed**
   - **Bob Height**
   - **Spawn Immunity Duration**
5. Save the prefab. The current pickup flow does not require a special pickup tag.

---

## Step 2 - Add the spawner to the scene

1. In the Hierarchy, create an empty object under your runtime scene root such as `Spawners` or `SceneSystems`.
2. Add **CollectibleSpawner2D**.

---

## Step 3 - Configure the spawner

**Collectible Prefab**

- Drag the pickup prefab into the prefab field.

**Pool**

- **Pool Size** - total instances to prewarm. Increase this for denser score-loop scenes.

**Initial Spawn**

- **Initial Collectible Count** - collectibles placed when a round begins.
- **Initial Cluster Size** - if greater than `0`, collectibles spawn in clusters.
- **Initial Cluster Radius** - radius of each cluster.

**Periodic Spawn**

- **Spawn Interval** - seconds between automatic spawns during active gameplay.
- **Minimum On Screen** - immediate refill floor for active collectibles.

**Burst Spawn**

- **Burst Count** - number of pickups dropped in one authored burst.
- **Burst Radius** - scatter radius for that burst.

**Spawn Area**

- **Spawn Margin** - inset from the camera edges.
- **Collectible Size Radius** - half the collectible size used to keep spawns fully on screen.
- **Min Distance From Player** - minimum distance from the nearest participant.

---

## Step 4 - Wire Runtime Services

`CollectibleSpawner2D` needs gameplay state and camera bounds. In the standard 2D score-loop scene, assign the spawner to the `GameManager` pickup spawner field and assign the scene `CinemachineCameraRigController` to `GameManager` Camera Bounds Source when a direct bounds source is needed. The preferred bootstrap path assigns the same rig to `GameplaySessionBootstrap > Camera Rig Controller`.

1. Select the `GameManager`.
2. Find the pickup spawner field.
3. Drag in the object that owns `CollectibleSpawner2D`.
4. Assign Camera Bounds Source to the scene `CinemachineCameraRigController` only when this direct reference is needed.

---

## Step 5 - Burst spawning from hazards

Use the explicit pickup burst surface path. Hazards receive this automatically from `GameManager` when `CollectibleSpawner2D` is assigned as the pickup spawner. Custom systems should keep an `IPickupBurstSpawnSurface` reference and call:

```csharp
pickupBurstSurface.SpawnCollectiblesAt(transform.position, count: 5, radius: 0.6f);
```

---

## Step 6 - Verify in Play Mode

1. Enter Play Mode and confirm collectibles appear at round start.
2. Walk the player over a collectible and confirm it disappears and scores.
3. If your pawn uses feature modules, confirm it has the correct collector runtime:
   - `actor.pickups.2d` with `ActorPickupCollectorFeature2D` for `Sprite2D`
   - `actor.pickups.3d` with `ActorPickupCollectorFeature3D` for `Billboard2_5D` and `Rigged3D`
4. Wait a few seconds and confirm periodic spawning refills the board.
5. Check the Console for any `[CollectibleSpawner2D]` errors about missing prefab or camera setup.

---

## Common Mistakes

| Problem | Likely cause |
|---|---|
| No collectibles appear | Prefab not assigned or pool size is `0` |
| Items never despawn | Pickup collider is not a trigger |
| No score on collection | No `IPickupAwardSink` is assigned/resolved, `CollectibleFeedback2D` Score Award Source is empty, or the collector actor does not resolve to a participant |
| Missing runtime services warning | Assign GameManager Camera Bounds Source, or assign Gameplay State Source plus Camera Bounds Source/Target Camera directly on the spawner |
| Items spawn off screen | Spawn margin or collectible size radius is too small |

---

## Modular Actor Pickup Notes

Use the shared pickup feature lane when you want actor-owned pickup behavior instead of scene-specific controller wiring.

- `PickupFeatureProfile`
  - shared authored pickup radius, auto-collect, interaction collect, and 3D overlap settings
- `ActorPickupCollectorFeature2D`
  - `2D` runtime for `Sprite2D` actor stacks
- `ActorPickupCollectorFeature3D`
  - shared runtime for `Billboard2_5D` and `Rigged3D` actor stacks

Recommended authoring:

1. Create or assign a `PickupFeatureProfile`.
2. Create a `FeatureModuleDefinition` with:
   - module id `actor.pickups.2d` for `Sprite2D`, or
   - module id `actor.pickups.3d` for `Billboard2_5D` / `Rigged3D`
3. Assign the matching runtime prefab.
4. Add that module definition to `PawnDefinition.featureModules` or the relevant actor feature profile.

Required actor surfaces:

- `Collider2D` for `actor.pickups.2d`
- `Collider` or `CharacterController` for `actor.pickups.3d`
