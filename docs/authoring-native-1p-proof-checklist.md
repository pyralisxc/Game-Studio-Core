# Native 1P Playable Proof (Unity-native)

**Goal:** from a fresh scene, create and run one controllable 1P pawn using only Unity-native authoring actions.

## Native workflow rules (required)

1. Create objects/assets with right-click in **Hierarchy** / **Project**.
2. Add scripts/components using **Inspector -> Add Component** search.
3. Wire references in the **Inspector** using drag-and-drop.
4. Use **Pyralis Authoring Window** as a route/validation aid, not as a scene builder.

## 1P movement proof route

### 0) Create these assets first (Project)

1. In **Project**, right-click or use **Create -> NeonBlack -> Gameplay** for each required asset.
2. Use these exact names while wiring:
   - `SessionDefinition`
   - `GameModeDefinition`
   - `GameSetupProfile`
   - `ParticipantDefinition`
   - `PawnDefinition`
   - `InputProfile`
   - `PawnMovementProfile`
   - `PawnPresentationProfile`

### Required assets (Project)

- `SessionDefinition`
- `GameModeDefinition`
- `GameSetupProfile` with at least one pawn/input movement pattern
- `ParticipantDefinition`
- `PawnDefinition`
- `InputProfile`

### 1) Scene root (Hierarchy)

1. In an empty scene, right-click in **Hierarchy** -> **Create Empty** and name it `Gameplay Root`.
2. Select `Gameplay Root`, then use **Inspector -> Add Component** to add:
   - `GameplaySessionBootstrap`
   - `PyralisGameplayLifetimeScope` (optional; keeps setup state visible while learning)
3. Set:
   - `Session Definition` -> your `SessionDefinition`
   - `Auto Create Core Services` -> On
   - `Inject Loaded Scenes On Build` -> On

### 2) Authoring chain (Project + Inspector)

On `SessionDefinition`:

1. `Default Game Mode` -> your `GameModeDefinition`
2. `Default Participants[0]` -> your `ParticipantDefinition`

On `GameModeDefinition`:

3. `Setup Profile` -> your `GameSetupProfile`

On `ParticipantDefinition`:

4. `Default Pawn` -> your `PawnDefinition`
5. `Input Profile` -> your `InputProfile`

> If your project already uses legacy `PlayerInputHandler`, you can use it in step 3 temporarily and assign it in the prefab instead of `Motor2DInputAdapter`.

> Keep `Player Input Manager` empty for this proof path. Use `PlayerInputManager` later only when you intentionally test local join.

### 3) Build the pawn prefab (Hierarchy)

1. Right-click in **Hierarchy** -> **Create Empty**, name it `PlayerPawn`.
2. Add components in this order using **Inspector -> Add Component**:
   - `PawnRoot`
   - `Motor2D`
   - `Motor2DInputAdapter`
   - `Pawn2DMovementComponent`
   - `Pawn2DPresentationComponent`
3. Save `PlayerPawn` as a prefab in **Project**.
4. On `PawnDefinition`, set `pawnPrefab` to that prefab.

### 4) Spawn + gameplay-state links

1. Right-click in **Hierarchy** -> **Create Empty**, name it `SpawnPoint_01`.
2. In `GameplaySessionBootstrap`, add `SpawnPoint_01` to `Spawn Points`.

For this minimum route, required only:

- `ParticipantDefinition.InputProfile`
- `ParticipantDefinition.DefaultPawn`
- one valid `PawnDefinition.pawnPrefab`
- at least one entry in `GameplaySessionBootstrap.Spawn Points`
- auto-created core services from `Auto Create Core Services`

Do not add optional systems yet:

- scoring
- combat
- HUD
- pickups / hazards
- networking

### 5) Optional camera bounds (only if you are explicitly proving bounds)

1. Create or assign one bounds provider (for example `CameraAspectController`).
2. Route it into your camera bounds path (`CameraRig` or other `ICameraBoundsProvider` flow).

### 6) Pre-play checklist (Authoring Window)

Keep `Gameplay Root` selected and open **NeonBlack/Gameplay/Pyralis Authoring Window**:

- **Overview -> Do Now** has no required blockers
- **Map** shows required links above as present
- **Validate** has no required setup blockers
- `GameplaySessionBootstrap` setup flow itself reports no required blockers

### 7) Play check

Press Play and confirm:

- bootstrap starts without repeated setup-blocking exceptions in Console
- one pawn spawns at `SpawnPoint_01`
- default participant input controls that pawn
- you do not need score/HUD/combat/pickup prerequisites to prove this route
- movement is repeatable for one full press-and-release cycle.

Re-run once more immediately to confirm repeatability.

## 2P extension (after 1P is stable)

1. Duplicate participant/input setup for Player 2.
2. Add a second `Spawn Point`.
3. Add and configure `PlayerInputManager` only for local join mode.

Then rerun this same proof.

## Why this route is beginner-safe

It only assumes what movement requires:

- the authoring chain (`Session -> GameMode -> SetupProfile -> Participant -> Pawn`)
- input assignment
- spawn assignment
- service bootstrap readiness

Everything else is intentionally delayed until the first route passes reliably.
