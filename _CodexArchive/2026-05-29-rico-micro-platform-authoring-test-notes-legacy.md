# Rico Micro-Platform Authoring Test Notes

Date: 2026-05-29

## Goal

Use the Pyralis Authoring Window as a beginner would to create a small pawn-based Rico micro-platform test:

- one Sprite2D pawn-backed player
- optional second local player using the same pawn art with a shaded/tinted visual
- two spawn points
- a tiny platform room using Jim/Apocalyptia assets where possible
- enough setup to let Cameron test the two-player path, or enough evidence to stop and improve the tools

## Running Findings

### Bugs

- Validate tab reports "No validation issues found for the selected item" while the same authoring state still has `Scene Root [Needs Setup]` in Wire and the next step says to add spawn points to `GameplaySessionBootstrap`. For a beginner this reads as contradictory: the selected asset is valid, but the playable route is not actually ready.
- Before the tool pass, the created `Sprite2DPawnPrefab` did not include a visible `PlayerInputHandler`/`Motor2DInputAdapter` input module, and `SharedInputProfile.actions` was empty. Newly generated packs now include `SharedInputActions`, assign it to `SharedInputProfile`, and serialize it onto the 2D input adapter.
- First Play test after assigning the session spawned both pawns, but `Pawn2DMovementComponent` logged missing runtime services for gameplay state and camera bounds. The authoring path did not make it clear that spawned 2D pawns need those services injected/configured before movement works.
- The first authoring-pass flow still implied scene discovery by object search instead of "select a scene object then add required components."

### Documentation Gaps

- The starter pack includes `SharedInputProfile`, but the created `SharedSessionDefinition` still shows `Default Input Profile` as empty and the Inspector recommends adding one. A beginner is not told whether this is intentionally optional, safe for the default two-participant setup, or something they should wire before testing two-player local input.
- The Authoring Window/guide does not explain the difference between "participant has an input profile asset" and "pawn prefab has an input module that consumes that profile."
- The first fallback scene proved the backend could spawn two pawns, but it also proved that script-generated scenes are the wrong acceptance route for this test. The official retry should only count work done through Unity UI and the authoring surface.

### Tool Usage Gaps

- In the current Codex Unity layout, the Pyralis Authoring Window is narrow enough that Create tab descriptions and the "Create 2D / 3D Pawn Action" button are cramped/truncated. The starter path is still visible, but the most important beginner action is visually fighting the panel width.
- The Create action selected `SharedSessionDefinition` and the Overview route now says to add spawn points and press Play, but from this view it is not yet obvious how a beginner should create or select the required `GameplaySessionBootstrap` scene root first.
- The generated starter pack appears under `Assets/PawnStarterPack`. That is workable, but the authoring UI did not make the target folder/location obvious before creation.
- Widening the Authoring Window makes the Guide and Wire tabs much more understandable. At the narrow default, tab labels and core status text are cramped enough that the user has to already know to resize the layout.
- Wire is the first place that clearly exposes `Scene Root [Needs Setup]`; Overview and Validate do not make that blocker equally obvious.
- Before the authoring-tool pass, there was no obvious one-click "Create Scene Root From This Session" or "Finish Playable Scene" action visible from Wire/Validate after the starter pack was created.
- Participant tint exists on `ParticipantDefinition`, but the route did not reveal a beginner-facing way to make Player 2 a shaded version of Player 1. The retry should first verify whether the starter pack exposes a visibly tinted P2 identity.
- The starter-pack creator follows the current Project selection as its destination. When the selected asset is already inside a starter-pack `Definitions` folder, a second create action can generate a nested `PawnStarterPack` under that definitions folder. The UI should either ask for a target folder, default to a stable starter-pack root, or warn before nesting packs inside generated packs.
- After the new scene root repair succeeds, `GameplaySessionBootstrap` Setup Flow immediately shows missing spawn points, PlayerInputManager, projectile launcher, camera rig, and gameplay-state service. It explains what to add, but does not yet offer beginner-safe create/assign buttons for the required objects.
- Validate’s scene-root repair action and Guide’s plain object context did not always expose a direct "wire this selected scene object as gameplay root" step, so users had to infer the object-first flow from generic add-component buttons.

### Authoring Questions To Watch

- Does the Authoring Window clearly lead from starter pack to scene root?
- Does it explain whether Player 2 is a default participant, a local join participant, or both?
- Does it explain when `PlayerInputManager` is optional versus required?
- Does it teach where to make Player 2 a shaded version of Player 1?
- Does it explain how ordinary platform colliders and Jim art relate to Pyralis setup?
- Does it distinguish "ready to press Play" from "full game route is understandable"?

## Work Log

- Started from the user-approved route: Rico two-player optional micro-platform, pawn-based Sprite2D setup.
- Opened the Authoring Window in Unity and selected the Create tab as the beginner entry point.
- Used `Create 2D / 3D Pawn Action`; Unity selected `Assets/PawnStarterPack/Definitions/SharedSessionDefinition.asset`.
- Confirmed the starter pack contains two default participants, Sprite2D/Billboard2.5D/Rigged3D pawn definitions, generated pawn prefabs, and a `SharedInputProfile`.
- In Overview, the active setup follows `SharedSessionDefinition` and reports the current step as `Pawn-backed route`.
- Expanded the Authoring Window dock. Guide explains that the selected authoring chain has spawnable pawn definitions and that the scene root owns spawn point assignment.
- Wire reports the active setup and marks the Scene Root as `[Needs Setup]`.
- Validate reports no issues for the selected `SharedSessionDefinition` even though the full playable scene route is still missing a scene root.
- Inspected the generated starter assets on disk: `SharedInputProfile` exists but has no Input Actions asset assigned; `Sprite2DPawnPrefab` has `PawnRoot`, movement, presentation, health, and visual renderer components, but no visible pawn input module.
- Built a local authoring-test helper under `Assets/NeonBlack/Gameplay/AuthoringTests/RicoMicroPlatform` as an exploratory fallback, then removed it from the active project path before the official retry. The fallback generated scene is not accepted as proof for this test because it bypassed the Unity-guided authoring workflow.
- The removed fallback proved a useful bug: two pawns could spawn, but spawned 2D pawns still logged missing gameplay-state and camera-bounds services. Keep that as a runtime/setup follow-up while restarting the authoring test through the actual window.
- Authoring-tool pass started: add a direct scene-root repair action for selected sessions, make Validate surface route-level blockers, and improve the pawn starter pack's 2D input/two-player identity defaults before restarting the Rico route.
- Tool pass result: `Validate` now surfaces missing scene-root blockers for selected sessions. The first pass exposed a one-click `Create Gameplay Root From Session` action, but Cameron clarified the authoring model should guide the creator to add/select the scene object and wire required scripts/configuration rather than silently creating scene structure. The root action is now `Copy Gameplay Root Steps`.
- Starter pack result: newly generated pawn packs now include `SharedInputActions`, assign it to `SharedInputProfile`, assign the input profile to `SharedSessionDefinition`, add `Motor2DInputAdapter` to `Sprite2DPawnPrefab`, and tint `PlayerTwoDefinition` blue.
- Restart status: the official Unity-guided route proved the scene-root blocker is now visible. The next blocker is scene-surface creation/assignment: spawn points, local join input root, projectile launcher, camera/gameplay-state surfaces, and then platform room content. Setup Flow guidance now names recommended parent objects (`Spawn Points`, `Input Root`, `Camera Root`, `Game Flow Root`, `Combat Root`) and tells the creator which component/field to wire.
- Latest polish pass: scene-root recovery now consistently follows the object-first workflow.
  - `Draw Scene Root` flow now prefers wiring the currently selected scene object for `GameplaySessionBootstrap` + session assignment in one step.
  - Validate repair for `sceneRoot.bootstrap.missing` now attempts to use the selected scene object first and falls back to copying explicit setup steps.
  - Guide mode now offers `Add Gameplay Root + Assign Session` when a session is already the active setup and the selected object is a scene object.

## Latest Same-Game Attempt (2026-05-29 Fresh Restart)

- Cleaned legacy test artifacts previously generated and reopened Unity for a fresh run.
- Created a fresh untitled scene before re-testing.
- Reached the Authoring Window and Confirmed the route now exposes `Scene Root [Needs Setup]`.
- Attempted to use Project Browser and Quick Search to select `NeonBlack/Gameplay/StarterPacks/PawnStarterPack/Definitions/SharedSessionDefinition.asset` as the start point for the same Rico micro-platform test.
- Could not complete deterministic asset selection in this environment because project tree rows and search results were not exposed as stable accessibility targets.
- `Ctrl+Shift+F` opens Quick Search (`UnityEditor.Search.SearchWindow`), but results and selection state are not surfaced in `accessibility` reads in a way we can confirm selection reliably.
- Guidance-level impact: this is a beginner-friction point; users can lose flow and context before setup begins if they cannot locate and pick the intended session asset via UI steps that are exposed/validated by the tool.
