# Rico Native Authoring Test Notes

This is an audit log for the manual Rico/native-authoring validation passes, not the current setup guide. Use `START_HERE.md`, `CANONICAL_SETUP.md`, and the `Prefabs/*_Setup.md` guides for current authoring instructions; keep this file only for evidence, regressions, and lessons that already fed the active docs/editor guidance.

Date: 2026-05-30, continued 2026-05-31

Update: 2026-06-02 manual Computer Use pass focused on a no-Animator Rico setup with a PNG background, separate walkable ground collider, Cinemachine framing, and horizontal-plus-jump movement.

## Goal

Use the Pyralis Authoring window and normal Unity workflows to build a tiny 2D pawn test from scratch:

- start each manual pass from a clean disposable proof folder, deleting prior `Assets/Pyralis*Proof` or starter-pack scratch content before the next run
- create assets from the Project window Create menu
- add or inspect scene objects through the Hierarchy and Inspector
- wire references through object fields and picker/search UI
- reach a spawned 1P pawn, then verify movement before adding 2P extras

Authoring validation must stay manual-through-Unity: use Computer Use to follow the Authoring Window, Project window, Hierarchy, Inspector, object picker, scene tools, and Play Mode. Do not count generated proof scenes, factory menu items, or hidden auto-wire shortcuts as evidence that the authoring system is followable.

## What Worked

- Computer Use could use the normal Unity Project/Hierarchy/Inspector route to add `1 level test v1-2.png` from Jim's Apocalyptia folder as a visible SpriteRenderer background.
- Creating a separate `Walkable Ground` scene object, adding `Box Collider 2D` through Inspector -> Add Component, and sizing it as a platform was followable. This is the right guidance shape: background art stays visual, gameplay ground is ordinary Unity collider/layer authoring.
- A static `Rico Static Visual` object with `SpriteRenderer` could be created through native Unity Add Component, keeping the no-Animator proof concept understandable.
- The corrected no-Animator proof worked once the static Rico art was assigned to the spawned pawn prefab's `SpriteVisual` child rather than a loose scene object: the runtime clone showed Rico in Scene and Game view, over the PNG background, with the camera framing the spawned pawn.
- The authored camera route is understandable when broken into concrete checks: `Main Camera` is tagged `MainCamera`, is orthographic, and owns the `Cinemachine Brain`; the separate `CinemachineCamera` is assigned to `Camera Root > Shared Camera Behaviour`; `Camera Root > Target Camera` points back to `Main Camera`.
- The spawned pawn runtime root switched to `Rigidbody2D: Dynamic` with gravity when the active movement profile/prefab route had side-view jump enabled, which makes the horizontal-plus-jump proof distinguishable from the older kinematic top-down starter route.
- Project search showed the derived `RicoFrontStatic` asset after selecting the `Assets` root and clearing the old scoped search.
- A fresh manual route created through native Unity UI reached a ready-to-play pawn-backed setup: `Scratch Gameplay Root` + `ZeroSessionDefinition` + `ZeroGameModeDefinition` + `ZeroGameSetupProfile` + `RicoProofPattern` + `ZeroPlayerOneParticipant` + `ZeroRicoPawnDefinition` + spawn point + Cinemachine camera rig.
- The Authoring Window correctly treated `Scratch Gameplay Root` as the active setup while linked assets were selected, and it stayed useful after the route crossed from asset wiring into scene objects.
- Setting `ZeroSessionDefinition > Max Participants` to `1` cleared the local-join/`PlayerInputManager` warning for a 1P proof without requiring a PlayerInputManager.
- Play Mode spawned `Rico Pawn Prefab Source(Clone)` from the native setup route.
- The Project window Create path can create the Pawn Starter Pack from `Create > NeonBlack > Pawn Starter Pack`.
- A fresh manual route can start from an empty `Gameplay Root`: adding `GameplaySessionBootstrap` through Inspector -> Add Component immediately promotes the scene object as the active setup and guides the next Project-window asset.
- After assigning `FreshSessionDefinition`, `FreshGameModeDefinition`, `FreshGameSetupProfile`, and `RicoProofPattern`, the Authoring Window stayed on the scene `Gameplay Root` as the active setup while linked assets were selected, so the setup story no longer got lost during field-level asset wiring.
- Dragging assets from the Project panel into Inspector fields worked reliably for `GameplaySessionBootstrap.sessionDefinition`, `SessionDefinition.defaultGameMode`, `GameModeDefinition.setupProfile`, and `GameSetupProfile.runtimePatterns` once the correct asset and field were visible.
- The corrected Project-window flow works when the user opens the destination folder first: `Assets/PyralisManualProof/PawnStarterPack` was created under the intended folder after the Project content pane/breadcrumb showed `PyralisManualProof`.
- The `GameplaySessionBootstrap` Inspector is now simpler: normal fields plus an `Open Pyralis Authoring` handoff button.
- Using the object picker for `Session Definition` worked more reliably than dragging when the Inspector target was narrow or partially obscured.
- Generated starter content now creates a visible 2D pawn prefab with a child `SpriteVisual` renderer, a generated sprite, one `ActorAnimationDriver`, and one `Motor2DInputAdapter`.
- A scene built through the native flow reached Play Mode with two spawned starter pawns from `SharedSessionDefinition`.
- A fresh 1P scene built through native Unity UI reached Play Mode with exactly one spawned `Sprite2DPawnPrefab(Clone)` after the starter session defaulted to Player One only.
- The starter route now produces a visible 2P variant through participant tint instead of spawning two identical pawns.
- Authoring retained the `Gameplay Root` setup story when `Spawn Point 1` was selected after the helper-context fix, so Overview stayed on the pawn route instead of resetting to first-step guidance.

## Issues Found

- Project search scope was confusing again: the search stayed scoped to the previously active scratch folder until the `Assets` root was selected. Authoring should tell users to check the Project breadcrumb/search scope before concluding that art or setup assets are missing.
- Dragging the Rico Aseprite asset from `Assets/Jim/Assets/Apocalyptia/2D Sprites/Player/Rico` into the Scene did not create an obvious static sprite object. The `.meta` contains many sprite subassets, but the normal Sprite picker did not discover them by name during this pass.
- Exporting a no-Animator static Rico frame required creating a derived PNG (`Assets/PyralisAuthoringScratchRoot_20260602/DerivedArt/RicoFrontStatic.png`) and setting its import type to Sprite. This is acceptable as an art-pipeline workaround, but authoring should not pretend Aseprite root assets always drag in as static sprites.
- The Sprite picker did not refresh while already open after the PNG import/settings changed; closing and reopening the picker was necessary before retesting.
- Dragging the imported `RicoFrontStatic` Project thumbnail into the selected SpriteRenderer field/Scene did not assign or instantiate reliably through Computer Use, even though the asset appeared in Project search. This remains a manual verification gap for the exact static-sprite assignment step.
- The existing scratch scene was not a clean from-empty pass; it already contained `Gameplay Root`, `SpawnPointC1`, `Camera Root`, and `CinemachineCamera` from the prior route. Treat this pass as a focused finish/configuration pass, not full from-scratch proof evidence.
- The loose scene object `Rico Static Visual` was a misleading proof artifact. It did not validate the pawn stack because the spawned pawn still used its own prefab visual child. Static-art validation must inspect the pawn prefab or runtime clone child `SpriteVisual`.
- Duplicate `Sprite2DPawnPrefab` / `SharedSessionDefinition` results from `Assets/PawnStarterPack` and `Assets/PyralisAuthoringScratchRoot_20260602/PawnStarterPack` made it easy to configure one folder while the scene played the other. Authoring guidance should tell users to check the Project breadcrumb/path and the assigned `SessionDefinition -> Participant -> PawnDefinition -> Pawn Prefab` chain.
- The derived `DerivedArt/RicoFrontStatic.png` repeatedly behaved like a default texture after refresh/import attempts. For this proof, using an already Sprite-imported pawn art asset was more reliable than teaching users to fight importer state in a scratch `DerivedArt` folder.
- The scratch test accidentally nested `PyralisScratchFromZero` under the previously active Project folder. The native Create path is still the right source of truth, but Authoring should keep reminding the user to open/check the parent folder before creating assets.
- Guidance like "drag it into SessionDefinition > Default Game Mode" is too implicit during native setup because the Inspector may still be showing the bootstrap or another linked asset. Shared setup guidance should say "select/open the SessionDefinition asset, then assign Default Game Mode."
- The camera step works better as a short sequence than as one dense sentence: create `Camera Root`, add `CinemachineCameraRigController`, create or choose a Cinemachine camera, verify Main Camera Brain, assign Shared Camera Behaviour / Target Camera explicitly, then drag the rig to the bootstrap.
- The `PlayerInputManager` warning hid the actual 1P fix. For a 1P proof, the concrete action is `SessionDefinition > Max Participants = 1` and `Bootstrap > Player Input Manager` stays empty; `PlayerInputManager` belongs to local join.
- The scratch Play Mode spawn appeared in the hierarchy, but Game view framing made the pawn look tiny/off-center. Manual held-input validation is still needed before claiming movement feel.
- Manual movement testing showed Rico could face/change direction but the transform did not move. Root-cause inspection found the 2D movement stack exits before `MovePosition` when the assigned camera rig cannot provide orthographic 2D bounds. Authoring had accepted "camera rig assigned" without proving the rig/profile/target camera was usable for 2D bounds.
- Selecting `FreshSessionDefinition` originally made the active setup become the asset instead of the scene `Gameplay Root`, which made the Authoring story feel detached from the actual setup root until linked-asset setup inference was added.
- The instruction "drag it into SessionDefinition > Default Game Mode" assumes the user realizes they must select/open the `SessionDefinition` asset before assigning the field. The next-step language should explicitly name "select the Session Definition asset, then assign its Default Game Mode field" when the current Inspector is still on the bootstrap.
- The dense `Create -> NeonBlack -> Profiles` menu makes it easy to create the wrong profile type. During the native pass, `Hazard Feedback Profile` was accidentally created when trying to create a `Game Setup Profile`; Authoring should name the expected Inspector type after creation so users can catch this quickly.
- Project search filters can remain active from earlier work. If the Project pane is filtered, the user's chosen folderbase and available assets are harder to reason about; Authoring guidance should remind users to clear Project search when the expected folder contents are not visible.
- The first regenerated `Sprite2DPawnPrefab` had a `SpriteRenderer` child but no sprite, so the pawn could spawn invisibly.
- The same prefab had duplicate `ActorAnimationDriver` components because the factory added one explicitly after required components had already added one.
- The generated sprite initially imported as a multi-sprite texture, so `LoadAssetAtPath<Sprite>` returned null and the prefab still had no sprite assigned.
- Importing the generated sprite in the middle of starter-pack creation caused unsaved object-reference fields in generated definitions and profiles to serialize as null.
- A blank-scene Play Mode check is not enough for a micro-platform test: the 2D pawn uses Rigidbody2D gravity, so the guide must either teach platform/ground creation before movement verification or offer a top-down/no-gravity pawn setup.
- The first successful Play Mode spawn still logged missing input actions because the generated input profile and prefab input module were not wired to `Assets/InputSystem_Actions.inputactions`.
- The scene bootstrap registered gameplay services in the platform context, but 2D movement and input components could still require manual Inspector sources unless they resolved the context themselves.
- `GameplaySessionBootstrap` created persistent child services that then called `DontDestroyOnLoad` on non-root GameObjects, creating warnings in an otherwise simple first-scene proof.
- The Unity D3D device reset crash during a Play Mode attempt made the unsaved scene route fragile. The guide should prompt developers to save the scene before entering Play Mode.
- The Authoring Validate tab could say `Asset validation` while a scene bootstrap was selected, which read like the wrong route context.
- When selected on an intermediate asset, the Authoring window can report missing scene/session context even when the scene route exists. It needs clearer "you are inspecting part of the route" language or better chain discovery.
- Computer Use can click/focus the Game view and send key pulses, but it is not a perfect held-key movement simulator. Movement validation should still include either a human-held input pass or a targeted PlayMode check that drives input for several frames.
- With nothing selected, Authoring can start the user correctly, but after creating `Gameplay Root` it did not repaint promptly from Unity selection/hierarchy changes.
- After adding `GameplaySessionBootstrap`, Authoring sometimes stayed on `No setup context` until the Inspector handoff button was clicked.
- Project-window starter-pack creation is the right professional path, but users can create assets in the wrong folder if the guidance does not explicitly say to check the selected Project folder and breadcrumb first.
- In Unity's Project window, selecting a folder tile is not the same as opening that folder as the active Create destination. The first fresh pass selected `PyralisManualProof` but created `Assets/PawnStarterPack` at the parent `Assets` level.
- The generated Pawn Starter Pack selected a session with two default participants, so a first "1P" proof spawned two pawns unless the user knew to remove Player Two.
- Overview and Validate could disagree: Overview still asked for spawn points while Validate reported no selected-item issues.
- Selecting a scene helper such as `Spawn Point 1` should not make the user lose the active setup story.
- After exiting Play Mode during the fresh pass, selecting `Spawn Point 1` briefly made Authoring report `No setup context` until scene-helper setup discovery was added.

## Fixes Made During This Pass

- Added an opt-in 2D side-view jump path to `PawnMovementProfile` and `Pawn2DMovementComponent`: `Allow 2D Jump`, jump velocity, gravity scale, max fall speed, ground layer, ground check offset, and ground check radius.
- `Motor2D` now exposes `Jump()`, and `PlayerInputHandler` binds the `InputProfile` Jump action row to real jump while keeping Dash as a separate optional action row.
- 2D movement authoring guidance now separates top-down dash-on-jump from side-view jump, and the inspector explains ground layers/ground checks.
- `InputProfile` validation now treats Jump as route-dependent instead of globally required for every player-owned input profile.
- Pawn setup docs now explain PNG/static sprite import requirements, Aseprite static-frame friction, Project search scope, and background art versus gameplay colliders.
- Authoring primary actions now name the field owner explicitly for the core chain: select/open `SessionDefinition`, `GameModeDefinition`, or `GameSetupProfile`, then assign the named field.
- Runtime pattern guidance now prefers existing pattern assets for first proofs and frames new `RuntimePatternDefinition` creation as advanced route-taxonomy work.
- 1P/local-join guidance now tells users to set `SessionDefinition > Max Participants` to `1` and leave `Bootstrap > Player Input Manager` empty for a 1P proof.
- Cinemachine setup guidance is now broken into concrete native steps and no longer implies the user must add a Brain if Unity already did.
- Pawn-backed first-proof validation now requires the assigned camera rig to provide 2D bounds through either an orthographic `CameraRigProfile` or an orthographic Target Camera before Authoring calls the route ready for Play Mode.
- Disposable scratch folders from prior authoring passes were removed from `Assets` so the next from-scratch validation does not inherit generated assets or scene setup.
- The Authoring Window now resolves selected linked assets back to the scene `GameplaySessionBootstrap` that owns them when possible. This covers the session definition, game mode, setup profile, runtime patterns, participants, pawn definitions, pawn prefab, input/profile assets, and core game-mode references.
- The Active Setup bar now explains when the selected object is already linked into the active scene setup, so users understand why the scene root remains the setup story while they inspect a field owner.
- `GameplayStarterPackFactory` now generates a starter 2D sprite before creating the definition/profile graph.
- The generated starter sprite imports as a single sprite and is assigned to `SpriteVisual`.
- The starter 2D prefab wires `Pawn2DPresentationComponent.spriteRenderer`, `ActorAnimationDriver.spriteRenderer`, and `ActorAnimationDriver.visualRoot`.
- The starter 2D prefab uses `EnsureComponent<ActorAnimationDriver>` instead of adding a duplicate.
- The starter 2D prefab includes `Motor2DInputAdapter`.
- The Pawn Starter Pack now assigns `Assets/InputSystem_Actions.inputactions` to `SharedInputProfile`, the 2D starter prefab, and the 3D starter input modules when the project action asset exists.
- The Pawn Starter Pack now assigns `SharedSessionDefinition.defaultInputProfile`.
- `PlayerInputHandler` and `Pawn2DMovementComponent` can resolve runtime gameplay state/camera services from `GameplayPlatformContext`, reducing manual hidden wiring.
- The default starter 2D pawn uses zero gravity and frozen rotation so a blank-scene top-down movement test is possible before adding platforms.
- Participant tint now affects 2D presentation, and the generated Player Two definition uses a darker tint.
- Persistent gameplay services no longer call `DontDestroyOnLoad` while parented under the bootstrap root.
- Stale legacy/migration wording was removed from the current 2D controller/input comments.
- The Authoring Window repaints on Unity selection, hierarchy, project, and inspector updates so it reacts to native workflow steps without requiring an Inspector handoff click.
- Empty-selection starter guidance now names the setup map rather than stale `SessionDefinition` wording.
- Bootstrap/session guidance now tells users to select the destination Project folder and check the Project breadcrumb before using native Create paths.
- Bootstrap/session guidance now says to open the destination folder and check the Project content pane/breadcrumb before using native Create paths, avoiding the selected-folder-tile ambiguity.
- The Pawn Starter Pack now keeps `PlayerTwoDefinition` as an optional asset but assigns only `PlayerOneDefinition` to the starter session for a clean 1P first proof.
- Pawn-backed route guidance now reads assigned `GameplaySessionBootstrap.Spawn Points` and default participant count, then distinguishes missing spawn points, participant/spawn mismatch, and ready-to-play states.
- The Validate tab now validates the active setup context instead of only the raw selected object, so it matches Overview when a spawn point or other helper object is selected.
- The Authoring Window can now resolve a referenced spawn point back to the `GameplaySessionBootstrap` that owns it, and falls back to the only scene bootstrap when that is the unambiguous setup route.
- Source-contract tests now require the helper-context route discovery hooks and the starter-pack selected-folder regression check.

## Verification Notes

- `dotnet build NeonBlack.Gameplay.Characters.csproj --no-restore` passed with 0 warnings/errors after the jump/input changes.
- `dotnet build NeonBlack.Gameplay.Editor.csproj --no-restore` passed with 0 warnings/errors after the inspector/doc changes.
- Computer Use verified the corrected runtime clone after the active prefab route was fixed: `Sprite2DPawnPrefab(Clone)` spawned, `SpriteVisual > Animator` was present but disabled, `SpriteVisual > SpriteRenderer > Sprite` was assigned, the Game view showed the Rico static sprite over the PNG background, and the root `Rigidbody2D` was `Dynamic` with gravity scale `3`.
- Computer Use verified the camera setup in the live scene: `Main Camera` had `MainCamera` tag, orthographic projection, and `Cinemachine Brain`; `Camera Root` referenced `Main Camera` as Target Camera and the separate `CinemachineCamera` as Shared Camera Behaviour.
- Unity refresh attached to the open Editor after the code/doc changes and reported a fresh `[CodexUnityValidation] Refresh complete` marker with no common compiler/test error patterns.
- Unity refresh attached after importing `RicoFrontStatic.png` and after updating its `.meta` import settings; both refreshes completed with no common compiler/test error patterns. The first import logged a transient asset-version import retry, then imported successfully.
- Computer Use manually added the PNG background, created `Walkable Ground`, added `Box Collider 2D`, and created `Rico Static Visual` with `SpriteRenderer`. Static Rico sprite assignment remains unresolved in the live Unity scene.
- Unity refresh validation passed after the linked-asset context fix, with a fresh `[CodexUnityValidation] Refresh complete` marker, Tundra build success, and no common compiler/test error patterns.
- Computer Use retest after the linked-asset context fix showed `FreshSessionDefinition`, `FreshGameModeDefinition`, and `FreshGameSetupProfile` selected while Active Setup stayed on `Gameplay Root`.
- Unity refresh validation passed after each code/import slice, with fresh Tundra build success and no common compiler/test error patterns.
- Computer Use Play Mode verification showed two visible pawns spawning from the native scene route.
- Computer Use retest after the 2026-05-31 fixes showed Authoring retaining the remembered `Gameplay Root` active setup while `Spawn Point 1` was selected, and both Overview and Validate reported the same one-spawn/two-participant mismatch.
- Unity refresh validation passed after the 2026-05-31 fixes with fresh Tundra build success and no common compiler/test error patterns.
- Computer Use fresh-start pass created `Assets/PyralisManualProof`, opened that folder, created the Pawn Starter Pack through Project right-click Create, added `GameplaySessionBootstrap` through Inspector Add Component, assigned `SharedSessionDefinition`, added one spawn point, entered Play Mode, and observed exactly one visible starter pawn spawned.
- Computer Use retest after the helper-context fix showed `Spawn Point 1` selected while Authoring still reported `Remembered Setup: Gameplay Root (GameplaySessionBootstrap)` and the ready-to-play pawn-backed route.
- `dotnet build NeonBlack.Gameplay.Editor.Tests.csproj --no-restore` passed after restore/refresh.
- After the final fixes, the Console only showed Unity AI Assistant service noise from the local environment during the checked run; the prior starter input and `DontDestroyOnLoad` warnings were no longer reproduced.
- 1P spawn is verified from the native UI route. 1P movement still needs stronger verification with a held-key/manual pass or a PlayMode input simulation because Computer Use key pulses are too brief for confident motion proof.

## Next Verification Step

Finish the no-Animator Rico pass from a saved scene before claiming it is ready for Cameron movement testing:

- assign `RicoFrontStatic` or a visible Rico Sprite subasset to `Rico Static Visual > SpriteRenderer > Sprite`
- wire the static visual onto the actual pawn prefab/`Pawn2DPresentationComponent.spriteRenderer`, not only a loose scene object
- set the pawn movement profile to side-view 2D jump, omit the Dash action row unless the game needs hardware dash, and verify the Jump action row maps to `Jump`
- set `Pawn2DMovementComponent > Ground Layer` to the layer used by `Walkable Ground`, then place the ground check at the pawn feet
- confirm Main Camera has `MainCamera` tag + Cinemachine Brain, the separate Cinemachine Camera is assigned as Shared Camera Behaviour, and the physical Main Camera is assigned as Target Camera
- enter Play Mode and verify the spawned pawn is visible over the PNG background, in camera frame, and ready for Cameron to test held horizontal movement plus jump

After that, run the broader saved-scene input pass:

- a visible 1P pawn spawns at `P1 Spawn`
- the pawn remains in camera view
- held keyboard movement changes the pawn position
- Authoring guidance names the save-before-play and movement-validation steps without requiring duplicate Inspector setup text
- optional follow-up: add Player Two back to `SessionDefinition.defaultParticipants`, add a second spawn point, and verify the tinted 2P variant only after the 1P proof is reliable
