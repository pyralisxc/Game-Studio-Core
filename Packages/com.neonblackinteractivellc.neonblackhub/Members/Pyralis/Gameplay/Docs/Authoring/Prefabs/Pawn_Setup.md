# Pawn Setup

This chapter is for games that need actor bodies in the scene.

Skip it for pure board games, card games, camera-only scenes, menu combat, or turn systems where participants act through UI, cursor, cards, board state, or rules instead of a spawned pawn.

## Before You Wire This

Start with a `GameSetupProfile` assigned to `GameModeDefinition.setupProfile`.

When you add a 3D pawn-stack component in Unity, use the Pyralis Authoring Window for route setup and the component's Inspector Field Guide for field-local checks before leaving the component. `Motor3D`, `Pawn3DInputModule`, `Pawn3DMovementComponent`, `Pawn3DTraversalComponent`, and `Pawn3DPresentationComponent` each explain what they do, which sibling components they expect, what can stay empty, and which missing pieces are required, recommended, or optional. Use this chapter when you want the longer written reference.

Recommended runtime patterns:

- Realtime Character when participants control pawns
- Combat if the pawn can attack, block, take damage, or trigger reactions
- Projectile Combat if the pawn can fire weapons, spells, or thrown objects
- Animation/Presentation if the pawn uses sprite, billboard, or rigged Animator presentation

Resolve setup-profile validation before building pawn prefabs or assigning pawn profiles. If the selected setup does not require a pawn, use this guide only for optional actor presentation rather than forcing a character controller into the game.

## Shared Pawn Chain

Every pawn-backed participant uses this authoring chain:

1. `PawnDefinition`
2. `PawnRoot` on the prefab
3. `PawnPresentationProfile` when the pawn has visible 2D, 2.5D, or 3D presentation
4. `PawnMovementProfile` when Pyralis movement modules drive the pawn
5. `InputProfile` when participant input drives the pawn directly
6. `PawnAnimationProfile` and `ActorAnimationDefinition` when the pawn uses Animator-driven visuals

`PawnRoot` applies movement, combat, traversal, presentation, animation, and feature-module data when those pieces are assigned.

Think of the pawn setup as two layers:

- **Core controller profiles** define the pawn's baseline body: input roles, locomotion feel, combat tuning, traversal rules, presentation, and Animator mappings.
- **Pawn abilities** are `FeatureModuleDefinition` assets added to `PawnDefinition > Feature Modules`. Use them for optional, reusable capabilities that a pawn archetype can gain or lose without editing the controller script.

Examples of pawn abilities that belong in `Feature Modules`:

- interaction
- pickup collection
- guard, parry, hurt, stagger, or combat reaction behavior
- status effects and combat modifiers
- actor feedback publishing or local feedback receivers
- 3D traversal modules such as ledge/hang/climb support
- future custom abilities such as dash variants, spells, gadgets, tools, or class-specific interactions

When adding a new ability over time, create or reuse a `FeatureModuleDefinition`, assign its runtime prefab, assign its tuning profile if it needs one, set supported presentation modes, then add it to the pawn's `Feature Modules` array. The runtime prefab must contain one or more behaviours that implement `IFeatureModuleRuntime`; `PawnRoot` installs those through `ActorFeatureHost` when the pawn initializes.

Camera wiring is scene-owned, not pawn-prefab-owned. Keep the pawn prefab reusable and spawnable; wire `CinemachineCameraRigController`, `CameraRigProfile`, the physical `Main Camera`, and the Cinemachine Camera in the scene/session setup. Pawn movement components can read camera bounds from the scene session at runtime, so empty camera fields on the pawn prefab are valid when the bootstrap camera rig is assigned. Assign camera fields on the pawn only for prefab-local tests or custom service routes.

## Pawn-Backed Action MVP lane choice

Use manual native authoring for validation passes: create the definitions, profiles, pawn prefab, and Inspector references yourself while following the Authoring Window. The route is proven when each asset and scene object is project-owned, inspectable, and wired through native Unity fields.

Expected lane-owned assets:

| Lane | Pawn definition | Prefab | Presentation profile | Runtime stack |
| --- | --- | --- | --- | --- |
| `Sprite2D` | `Sprite2DPawnDefinition` | `Sprite2DPawnPrefab` | `Sprite2DPresentationProfile` | `PawnRoot`, `Motor2D`, `Motor2DInputAdapter`, `Pawn2DMovementComponent`, `Pawn2DPresentationComponent`, `Rigidbody2D`, `PolygonCollider2D` |
| `Billboard2_5D` | `Billboard25DPawnDefinition` | `Billboard25DPawnPrefab` | `Billboard25DPresentationProfile` | `PawnRoot`, `Motor3D`, `Pawn3DInputModule`, `Pawn3DMovementComponent`, `Pawn3DTraversalComponent`, `Pawn3DPresentationComponent`, `CharacterController` |
| `Rigged3D` | `Rigged3DPawnDefinition` | `Rigged3DPawnPrefab` | `Rigged3DPresentationProfile` | `PawnRoot`, `Motor3D`, `Pawn3DInputModule`, `Pawn3DMovementComponent`, `Pawn3DTraversalComponent`, `Pawn3DPresentationComponent`, `CharacterController` |

For a clean 1P proof, create one `ParticipantDefinition`, assign its `Default Pawn`, and add one matching spawn point. Add a second participant and spawn point only when the route is intentionally testing local multiplayer. To test another lane, create a matching `PawnDefinition` and prefab for `Billboard2_5D` or `Rigged3D`, assign it to the participant's **Default Pawn**, then use the matching camera, ground, input, and presentation setup below.

## What To Assign On PawnDefinition

Always assign:

- `Pawn Prefab`

Usually assign:

- `Presentation Profile`
- `Movement Profile`
- `Input Profile`

Assign only when needed:

- `Combat Profile`
- `Traversal Profile`
- `Animation Profile`
- `Feature Modules`

If the Inspector says one of these is optional, that means the pawn can still be valid without it for some game shapes.

## 2D pawn setup

Use this stack (via Inspector -> Add Component in this order):

- `PawnRoot`
- `Motor2D`
- `Motor2DInputAdapter`
- `Pawn2DMovementComponent`
- `Pawn2DPresentationComponent`
- `Animator` on the visual object and `ActorAnimationDriver` if animated

Use `Motor2DInputAdapter` as the one supported player-input bridge for the first 2D movement proof. Do not also add a separate `2D Player Input Handler` to the same prefab unless you are intentionally building a custom direct-input route and understand which component owns input.

Choose the 2D movement route before testing controls:

| Goal | Inspector setup | What input does |
| --- | --- | --- |
| Top-down/free map movement | `PawnMovementProfile > Movement Mode = TwoD`, `Use 2D Physics` on, `Allow 2D Jump` off. On the prefab, `Pawn2DMovementComponent > Jump Enabled` off after applying the profile. | `Move` drives X/Y movement. `Jump` is available for feature modules such as top-down hop, dodge, animation-only actions, or custom ability logic. |
| Side-view/platformer movement | `Movement Mode = TwoD`, `Use 2D Physics` on, `Allow 2D Jump` on. On the prefab, tune `Jump Velocity`, `Gravity Scale`, `Ground Layer`, `Ground Check Offset`, and `Ground Check Radius`. | `Move` drives horizontal X movement. `Jump` applies vertical velocity through Rigidbody2D gravity. Vertical move input is not free map movement in this route. |

If you want a game that has both free top-down Y movement and a jump-like action, keep the baseline controller in the top-down/free route and add that jump-like action as an ability, combat action, traversal feature, or custom feature module. Use `TopDownHopFeatureRuntime` with a `TopDownHopProfile` for a Zelda/isometric-style hop that lifts the visual while the map-plane body stays grounded. Do not enable `Allow 2D Jump` unless the pawn should switch into side-view/platformer semantics.

2D dash is deliberately split between movement and input authoring:

- `PawnMovementProfile > Allow 2D Dash`, `Dash Speed`, `Dash Duration`, and `Dash Cooldown` decide whether the controller can dash and how it feels.
- `InputProfile > Dash Action` decides which hardware Input Action triggers it. Leave it empty for no keyboard/gamepad dash, set it to `Jump` for top-down dash-on-jump, set it to `Roll` or `Dash` for a dedicated dodge, or set it to another project action when your game wants dash on attack/ability input.
- Touch dash can still come from the optional authored dash zone when touch controls are enabled.

2D side-view/platformer jump is separate from dash:

- `PawnMovementProfile > Allow 2D Jump` enables the side-view route.
- `Jump Velocity 2D` and `Gravity Scale 2D` tune the jump feel.
- `Pawn2DMovementComponent > Ground Layer`, `Ground Check Offset`, and `Ground Check Radius` decide what counts as walkable ground. Put a Collider2D on the walkable level geometry, set it to a ground layer, and move the ground check to the pawn's feet.
- `InputProfile > Jump Action` decides which Input Action requests jump. Leave `Dash Action` empty unless the same pawn should also have a dash/dodge/roll ability.
- A flat PNG, sprite sheet, tilemap, terrain mesh, skybox, or canvas image can be the visual background. Pyralis only needs colliders, layers, camera bounds, spawn points, and gameplay anchors for the parts of the environment that gameplay systems read.
- Project search can be scoped to the currently selected folder. If an asset does not appear, select the `Assets` root or navigate through the folder tree before assuming the asset is missing.

Input actions are requests into the controller and ability surface:

- `Move` feeds `Motor2D` through `Motor2DInputAdapter` / `PlayerInputHandler`.
- `Jump` is first offered to installed feature modules such as `TopDownHopFeatureRuntime`; if no feature handles it, it feeds side-view jump only when the movement profile/component enables that route.
- `Dash` is first offered to installed feature modules; if no feature handles it, it feeds dash only when the movement profile/component allows dash.
- `Attack`, `Kick`, `Interact`, `Block`, guard, tools, spells, and other optional actions need a matching combat component, input bridge, or installed `FeatureModuleDefinition`.

Top-down/isometric hop feature:

- Create a `TopDownHopProfile` and tune Action Role, Duration, Height, and Cooldown.
- Create a `FeatureModuleDefinition` with module id `actor.traversal.topdown-hop`.
- Assign the `TopDownHopProfile` to `FeatureModuleDefinition > Profile Asset`.
- Assign a runtime prefab that contains `TopDownHopFeatureRuntime`.
- Add that feature definition to `PawnDefinition > Feature Modules`.
- Leave `PawnMovementProfile > Allow 2D Jump` off so Move keeps free X/Y top-down movement.

Presentation profile:

- `Presentation Mode`: `Sprite2D`
- `Sprite Default Faces Right`: set to match the art

Animation profile:

- assign the base Animator controller
- bind supported gameplay signals such as `Idle`, `Move`, `Dash`, `Death`, `Hurt`, `AttackPrimary`, `AttackSecondary`
- For a static sprite proof, keep the starting `SpriteRenderer > Sprite` assigned and wire the `PawnAnimationProfile`/`ActorAnimationDefinition` for later, but leave the Animator empty until you are ready to test animated clips.
- For PNG art, select the texture and set `Texture Type` to `Sprite (2D and UI)` before assigning it to `SpriteRenderer > Sprite`. For Aseprite art, use a visible Sprite subasset/static frame for a no-Animator proof; use the generated Aseprite prefab or Animator Controller only when the test is intentionally exercising animation.

## 2.5D pawn setup

Use this stack:

- `PawnRoot`
- `Motor3D`
- `Pawn3DInputModule`
- `Pawn3DMovementComponent`
- `Pawn3DTraversalComponent`
- `Pawn3DPresentationComponent`
- `PawnCombatBehaviour`
- `ActorAnimationDriver` if animated

Movement notes:

- assign `Movement Camera` on `Pawn3DMovementComponent` when movement should follow the gameplay camera
- leave `Movement Camera` empty only when world-axis movement is intentional
- add `KnockbackReceiver` when combat, hazards, or reactions can push the pawn; non-combat pawns can omit it safely

Presentation profile:

- `Presentation Mode`: `Billboard2_5D`
- choose billboard facing mode
- set sprite-facing direction

Animation profile:

- assign the base Animator controller
- bind locomotion, combat, traversal, hang, shimmy, and interact signals

## Rigged 3D pawn setup

Use the same stack as 2.5D, but change presentation mode:

- `Presentation Mode`: `Rigged3D`
- `Rig Type`: `Generic` or `Humanoid`

Rigged notes:

- use a standard Unity `Animator`
- assign the model as the animation driver's visual root when needed
- do not use billboard-facing behavior for rigged mode

## Animation authoring rules

- use `ActorAnimationDefinition` to declare the supported signal surface
- use `PawnAnimationProfile` to map those signals to Animator parameters, triggers, ints, and floats
- do not treat raw Animator parameter names as the source of truth
- gameplay systems should emit shared animation signals; the animation profile decides how Unity Animator reacts

## Bring Your Own Animator Controller

Use this flow when a project already has a player, enemy, or NPC Animator Controller and you want Pyralis gameplay code to drive it.

1. Create or duplicate a `PawnAnimationProfile`.
2. Find the Animator Controller in the folderbase or package where your art/controller assets live, then assign it to `Base Controller`.
3. Use the `Controller Mapping Wizard` in the profile Inspector to review readiness, signal coverage, controller parameters, and grouped validation issues.
4. Use `Append Suggestions` to add missing mappings from known parameter names, or `Replace With Suggestions` when you want a clean generated mapping pass. Suggested signal bindings respect the assigned `ActorAnimationDefinition`; add signals to the definition first if the controller supports more actions than the pawn route currently declares. Treat suggestions as a first pass: keep the controller's existing parameter vocabulary when it already fits your art pipeline.
5. Review each binding row and adjust the signal, parameter name, binding type, custom key, and value source. Parameter pickers are filtered by binding type so bool, float, int, and trigger bindings show compatible Animator parameters first.
6. If the controller uses sprite-swapping clips, select the pawn visual object and assign a valid starting sprite from the same art folder/sprite sheet to `SpriteRenderer > Sprite`. The controller, clips, and sprite frames must come from the same imported art set.
7. Assign the profile to the `PawnDefinition` and make sure the pawn prefab has an `ActorAnimationDriver`.
8. If the pawn uses `Billboard2_5D` presentation, assign `Camera Override` on `ActorAnimationDriver` / `BillboardFacing3D`, or call `SetCameraOverride` from the spawn/bootstrap code that knows the owning camera.

The profile Inspector validates the assigned controller. It reports missing Animator parameters, sprite-swapping clips with missing sprite frame references, binding type mismatches, duplicate mappings, blank parameter names, unsupported gameplay signals, and custom bindings with no custom key. These issues are grouped so setup problems, parameter mismatches, missing art references, duplicate bindings, unsupported signals, and custom-channel mistakes can be fixed without reading the raw serialized array. Partial mappings are valid: unmapped signals are ignored, while mapped signals continue to drive the controller.

Example mapping:

| Pyralis signal | Binding type | Animator parameter | Notes |
| --- | --- | --- | --- |
| `Move` | `Bool` | `IsMoving` | Set by pawn presentation from movement state. |
| `Sprint` | `Bool` | `IsSprinting` | Optional for games with sprint. |
| `Jump` | `Trigger` | `Jump` | Fired when traversal emits jump. |
| `Shimmy` | `Float` | `ShimmySpeed` | Uses signal float value. |
| `AttackPrimary` | `Trigger` or `Int` | `Attack` or `ComboStep` | Use whichever convention the controller already has. |
| `BlockLoop` | `Bool` | `Block` | Held while blocking. |
| `Hurt` | `Trigger` | `Hit` | Fired by feedback or combat reaction code. |

Current support is Animator parameter mapping: bools, floats, ints, and triggers. Direct Animator state graph editing is still the controller author's responsibility.

Blend trees are supported through float parameters. The presentation stack emits generic custom float channels that can be mapped to any Animator float parameter:

| Custom key | Meaning |
| --- | --- |
| `Speed` | Current world/planar movement speed. |
| `NormalizedSpeed` | Current movement speed normalized from 0 to 1 against the pawn move speed. |
| `MoveX` | Horizontal input or planar horizontal velocity. |
| `MoveY` | 2D vertical input, or 3D planar forward/depth velocity for common 2D blend-tree naming. |
| `MoveZ` | 3D planar forward/depth velocity for controllers that use Z naming. |
| `VelocityX` | Raw horizontal velocity. |
| `VelocityY` | Raw vertical velocity, useful for jump/fall blend logic. |
| `VelocityZ` | Raw 3D depth/forward velocity. |

If an imported controller has float parameters such as `Speed`, `MoveX`, `MoveY`, `Forward`, `VelocityY`, or `Speed01`, the profile Inspector can suggest custom float bindings for them. Review these like any other generated mapping.

The package test suite uses the included Apocalyptia player Animator Controller as an imported-controller compatibility fixture. Pyralis runtime and editor mapping code should not depend on that controller, its folder path, or prior project-specific gameplay scripts.
