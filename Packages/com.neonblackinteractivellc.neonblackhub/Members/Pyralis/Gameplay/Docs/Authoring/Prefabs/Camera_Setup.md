# Camera Setup - Step-by-Step

Covers the single supported Pyralis camera route: `CinemachineCameraRigController` + `CameraRigProfile`.

---

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

Recommended runtime patterns:

- Camera/Cursor Control for camera-owned, cursor-owned, strategy, tabletop, or menu-driven games
- Realtime Character when the camera follows pawns
- Board/Card/Tabletop when the camera frames non-pawn play

Resolve setup-profile validation before wiring Cinemachine cameras, camera profiles, target groups, or 2D bounds framing.

---

## Concepts

- **CinemachineCameraRigController** - manages one shared or split-screen Cinemachine Camera per session and provides visible 2D bounds through `ICameraBoundsProvider`.
- **CameraRigProfile** - a ScriptableObject that defines presentation mode (shared vs split-screen), follow parameters, profile-driven camera offset, pitch/yaw/roll, and zoom limits.
- **Physical Target Camera** - the Unity `Camera` that renders the Game view. This is usually the scene `Main Camera`, keeps the `MainCamera` tag, and has `Cinemachine Brain`.
- **Cinemachine Camera** - the Cinemachine component that composes/follows the view. Assign it to `Shared Camera Behaviour`; do not treat it as the physical render camera.
- **Normal shared-camera count** - one enabled physical render `Camera` plus one Cinemachine Camera. Multiple enabled physical cameras are valid only when you intentionally author split screen, overlay cameras, render textures, minimaps, or another advanced route.

Unity's normal Cinemachine route keeps or creates one real Unity Camera and adds `Cinemachine Brain` to that camera. The separate Cinemachine Camera GameObject does not render the Game view; it controls the real camera. Do not delete the default `Main Camera` for the normal Pyralis shared-camera proof unless you are intentionally replacing it with another single physical Unity Camera.

---

## Cinemachine Camera Setup

### Step 1 - Install Cinemachine

If Cinemachine is not yet installed in your project:

1. Open **Window -> Package Manager**.
2. Select **Unity Registry** from the drop-down.
3. Search for **Cinemachine** -> click **Install**.

### Step 2 - Create a CameraRigProfile asset

1. Right-click in the Project window -> **Create -> NeonBlack -> Profiles -> Camera Rig Profile**.
2. Name it (e.g. `CameraRigProfile_Default`).
3. Set the fields:
   - **Presentation Mode** - `Shared` for single or 2-player shared screen, `SplitScreen` for side-by-side views.
   - **Use Cinemachine** - enable (required for 3D).
   - **Orthographic** - enable for 2D movement, 2D camera bounds, board/top-down views, or any route where bounded framing should be measured in world units. Leave it off for 3D perspective cameras.
   - **Lock To Playfield** - enable to clamp the camera within `PlayfieldProfile` bounds.
   - **Use Profile Transform** - enable when the profile should place and rotate the shared Cinemachine Camera at runtime. Disable it when you want to hand-place and hand-rotate the Cinemachine Camera in the scene.
   - **Follow Offset** - world offset from the runtime shared follow focus to the Cinemachine Camera. For a clean 2D proof use `0, 0, -10`. For angled 3D/2.5D, raise Y and move back on Z.
   - **View Euler Angles** - pitch, yaw, and roll in Unity degrees. For a normal 2D orthographic proof use `0, 0, 0`; for angled follow, tune pitch/yaw here.
   - **Default Distance** - starting follow distance from the target group (e.g. `10`).
   - **Min Zoom / Max Zoom** - minimum and maximum follow distance as the group spreads apart.
   - **Follow Damping / Zoom Damping** - smoothing speed. `0` means snap/no lag; higher values smooth but still catch up faster as the number increases.
   - **Shake Amplitude / Shake Frequency** - multipliers for camera shake on hits. Match these to your `CameraRigProfile` shake calls.

### Step 3 - Set up a Cinemachine Camera in the scene

1. Keep or create exactly one enabled physical Unity Camera for a shared proof, usually the default `Main Camera`.
2. Use **GameObject -> Cinemachine -> Cinemachine Camera**. It can live under `Camera Root` for a tidy proof scene or alongside it if your scene organization prefers that; the durable contract is the Inspector reference you assign in Step 4.
3. Name it `CinemachineCamera` or `VC_Shared`.
4. Leave **Tracking Target** empty for the first pass. This should look empty in Edit Mode. `CinemachineCameraRigController` creates a shared focus target and assigns it to the Cinemachine Camera at runtime after the participant roster has spawned pawns.
5. Check the Hierarchy after creation. Unity usually adds or updates `Cinemachine Brain` on the existing `Main Camera`. If a new extra GameObject with a physical `Camera` component appeared and you do not need it, disable or remove that extra physical camera before Play Mode.

### Step 4 - Add CinemachineCameraRigController to a scene object

1. Create an empty GameObject in the scene root. Rename it `Camera Root`.
2. Add Component -> `CinemachineCameraRigController`.
3. Wire the Inspector fields:
   - **Camera Rig Profile** - drag your project-owned camera profile asset here. If the object picker shows duplicate names, drag the profile directly from the intended setup folder.
   - **Playfield Profile** - drag your `PlayfieldProfile` asset here (if you use bounds clamping).
   - **Participant Roster** - drag the `ParticipantRosterService` component from your scene, or let `GameplaySessionBootstrap` / `PyralisGameplayLifetimeScope` inject it at startup.
   - **Shared Camera Behaviour** - drag the `CinemachineCamera` / `VC_Shared` component here. This is the Cinemachine/virtual camera component, not the physical Unity Camera.
   - **Split Screen Camera Behaviours** - leave empty when using Shared mode.
   - **Target Camera** - drag the physical Unity Camera here, usually `Main Camera`. Verify that it keeps the **MainCamera** tag and has **Cinemachine Brain**; Unity usually adds the Brain when you create a Cinemachine Camera, so use **Inspector -> Add Component** only if it is missing. If the field is empty, the rig only auto-resolves a Camera under the same rig object; it does not search `Camera.main`. Assign the field explicitly when your camera lives elsewhere in the scene.
   - **Target Camera Projection** - for 2D proofs, select the physical Target Camera and set **Camera -> Projection** to **Orthographic**, unless the assigned `CameraRigProfile` is already orthographic. Pyralis uses this to decide whether the rig can provide 2D movement bounds.
   - **2D Bounds Framing** - for 2D proofs, leave **Enforce Minimum Visible Area 2D** on when the camera must always show at least a designed world rectangle. The rig previews `CameraRigProfile > Orthographic Size` in Edit Mode and Play Mode, then this clamp can raise the effective size. Lower the min world width/height, or turn enforcement off, when you want the profile size to zoom closer.

### Step 5 - Connect to GameModeDefinition (optional)

If your session uses a `GameModeDefinition` asset, assign your `CameraRigProfile` and `PlayfieldProfile` inside it. The Bootstrap will call `SetGameMode()` on the controller at session start.

### Step 6 - Connect bounds-aware systems

`CinemachineCameraRigController` implements the shared camera bounds service used by 2D movement, pickup spawning, and hazard spawning.

1. Drag the `Camera Root` object from the Hierarchy into `GameplaySessionBootstrap > Camera Rig Controller`. Unity assigns the `CinemachineCameraRigController` component from that object.
2. Leave `Camera Bounds Source` empty unless you are intentionally using a custom `ICameraBoundsProvider`.
3. If a legacy or direct component asks for Camera Bounds Source, assign the same `CinemachineCameraRigController`.

### Step 7 - Enable orientations in Player Settings for 2D

1. Open **Edit -> Project Settings -> Player -> Resolution and Presentation**.
2. Enable the orientations you support (Landscape, Portrait, or both for auto-rotation).

---

## Common mistakes

| Problem | Likely cause |
|---|---|
| Unsure whether to delete Main Camera | Do not delete it for the normal route. Keep one physical Unity Camera, usually `Main Camera`, with `Cinemachine Brain`; add separate Cinemachine Camera GameObjects for composition |
| Cinemachine Camera has no Tracking Target before Play | Expected for the Pyralis shared-camera route. The rig assigns a runtime focus target after participants spawn |
| Cinemachine Camera still has no Tracking Target after Play, or the camera does not follow the player | `Shared Camera Behaviour` is not the Cinemachine Camera, `Target Camera` is not the physical Main Camera with Cinemachine Brain, `Participant Roster` is not set/injected, `GameplaySessionBootstrap > Camera Rig Controller` is not assigned, or the session has not registered participants yet |
| Game view still shows the default Main Camera angle | The physical Main Camera is rendering without Cinemachine Brain or was not assigned as Target Camera; keep Main Camera tagged `MainCamera`, add Cinemachine Brain, and assign it to `CinemachineCameraRigController > Target Camera` |
| Camera follows but feels delayed | Set `CameraRigProfile > Follow Damping` to `0` for immediate follow, then retest in Play Mode |
| Orthographic Size does not zoom closer | `Camera Root > Enforce Minimum Visible Area 2D` is probably raising the live camera size to fit Min World Width/Height. Lower those fields or turn enforcement off for tighter zoom |
| Pitch/yaw changes do nothing | If `Use Profile Transform` is enabled, tune `CameraRigProfile > View Euler Angles`. If you want to rotate the Cinemachine Camera transform directly, disable `Use Profile Transform` first |
| Scene has three cameras after setup | You likely have one Cinemachine Camera plus two physical `Camera` components. Keep one enabled physical render camera for the shared proof, usually `Main Camera` with `Cinemachine Brain`, and disable or remove accidental extra physical cameras |
| Camera clips or shakes too aggressively | `Shake Amplitude` on `CameraRigProfile` too high - start at `1` |
| 2D sprites spawn off screen | `CinemachineCameraRigController` is not assigned as the camera rig/bounds provider, or 2D Bounds Framing world sizes do not match your design size |
| Letterbox bars appearing unexpectedly | `Letterbox 2D` is enabled on `CinemachineCameraRigController` |
| Split-screen cameras not active | `Presentation Mode` is `Shared` - switch to `SplitScreen` and assign cameras to `Split Screen Camera Behaviours` |
