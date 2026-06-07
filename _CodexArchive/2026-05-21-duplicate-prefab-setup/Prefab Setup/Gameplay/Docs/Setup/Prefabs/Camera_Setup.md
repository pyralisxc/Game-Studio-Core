# Camera Setup — Step-by-Step

Covers `CinemachineCameraRigController` + `CameraRigProfile` (3D) and `CameraAspectController` (2D).

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Camera/Cursor Control for camera-owned, cursor-owned, strategy, tabletop, or menu-driven games
- Realtime Character when the camera follows pawns
- Board/Card/Tabletop when the camera frames non-pawn play

Resolve setup-profile validation before wiring Cinemachine cameras, camera profiles, target groups, or 2D aspect controllers.

---

## Concepts

- **CinemachineCameraRigController** — manages one shared or split-screen Cinemachine virtual camera per session. Used in 3D scenes only.
- **CameraRigProfile** — a ScriptableObject that defines presentation mode (shared vs split-screen), follow parameters, and zoom limits.
- **CameraAspectController** — an orthographic camera manager for 2D scenes. Ensures the defined gameplay area is always fully visible regardless of screen size or rotation.

---

## Part A — 3D Camera Setup with Cinemachine

### Step 1 — Install Cinemachine

If Cinemachine is not yet installed in your project:

1. Open **Window → Package Manager**.
2. Select **Unity Registry** from the drop-down.
3. Search for **Cinemachine** → click **Install**.

### Step 2 — Create a CameraRigProfile asset

1. Right-click in the Project window → **Create → NeonBlack → Gameplay → Profiles → Camera Rig Profile**.
2. Name it (e.g. `CameraRigProfile_Default`).
3. Set the fields:
   - **Presentation Mode** — `Shared` for single or 2-player shared screen, `SplitScreen` for side-by-side views.
   - **Use Cinemachine** — enable (required for 3D).
   - **Orthographic** — leave off for 3D perspective cameras.
   - **Lock To Playfield** — enable to clamp the camera within `PlayfieldProfile` bounds.
   - **Default Distance** — starting follow distance from the target group (e.g. `10`).
   - **Min Zoom / Max Zoom** — minimum and maximum follow distance as the group spreads apart.
   - **Follow Damping / Zoom Damping** — smoothing speed. Higher = faster tracking.
   - **Shake Amplitude / Shake Frequency** — multipliers for camera shake on hits. Match these to your `CameraRigProfile` shake calls.

### Step 3 — Set up a Cinemachine Virtual Camera in the scene

1. In the Hierarchy, right-click → **Cinemachine → CinemachineCamera** (Unity 6) or **Create → Cinemachine → Virtual Camera** (earlier versions).
2. Name it `VC_Shared`.
3. On the virtual camera, set the **Follow** and **Look At** targets to a focus point you control — `CinemachineCameraRigController` will update this focus target at runtime.

### Step 4 — Add CinemachineCameraRigController to a scene object

1. Create an empty GameObject in the scene root. Rename it `CameraRig`.
2. Add Component → `CinemachineCameraRigController`.
3. Wire the Inspector fields:
   - **Camera Rig Profile** — drag your `CameraRigProfile_Default` asset here.
   - **Playfield Profile** — drag your `PlayfieldProfile` asset here (if you use bounds clamping).
   - **Participant Roster** — drag the `ParticipantRosterService` component from your scene, or let `GameplaySessionBootstrap` / `PyralisGameplayLifetimeScope` inject it at startup.
   - **Shared Camera Behaviour** — drag the `VC_Shared` virtual camera's component here (it will update the follow target via reflection).
   - **Split Screen Camera Behaviours** — leave empty when using Shared mode.
   - **Target Camera** — drag the Main Camera. If left empty, `Camera.main` is used automatically.

### Step 5 — Connect to GameModeDefinition (optional)

If your session uses a `GameModeDefinition` asset, assign your `CameraRigProfile` and `PlayfieldProfile` inside it. The Bootstrap will call `SetGameMode()` on the controller at session start.

---

## Part B — 2D Camera Setup with CameraAspectController

### Step 1 — Attach to Main Camera

1. Select your **Main Camera** in the Hierarchy.
2. Add Component → `CameraAspectController`.

### Step 2 — Set the minimum gameplay area

The controller ensures that a minimum world rectangle is always fully visible on every screen size.

- **Min World Width (Landscape)** — world units visible horizontally in landscape. Default `19.2` = 1920 px at 100 px/unit.
- **Min World Height (Landscape)** — world units visible vertically in landscape. Default `10.8` = 1080 px at 100 px/unit.
- **Portrait Min Width / Height** — set if your game supports portrait orientation. Set both to `0` to reuse the landscape values.
- **Letterbox** — leave `false` (recommended). When `false`, wider screens simply show more world. When `true`, black bars enforce pixel-exact framing.

### Step 3 — Enable orientations in Player Settings

1. Open **Edit → Project Settings → Player → Resolution and Presentation**.
2. Enable the orientations you support (Landscape, Portrait, or both for auto-rotation).

### Step 4 — Runtime availability

`CameraAspectController.Main` is a static shortcut to the active instance. Spawners, the 2D cockroach controller, and hazard systems use it to determine on-screen bounds. No additional wiring needed.

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| Camera does not follow the player | `Participant Roster` not set, or session has not registered participants yet |
| Camera clips or shakes too aggressively | `Shake Amplitude` on `CameraRigProfile` too high — start at `1` |
| 2D sprites spawn off screen | `CameraAspectController` not on Main Camera, or `Min World Width/Height` mismatch your design size |
| Letterbox bars appearing unexpectedly | `Letterbox` is `true` on `CameraAspectController` |
| Split-screen cameras not active | `Presentation Mode` is `Shared` — switch to `SplitScreen` and assign cameras to `Split Screen Camera Behaviours` |
