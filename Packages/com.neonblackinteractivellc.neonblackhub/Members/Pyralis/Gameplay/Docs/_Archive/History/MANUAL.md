# Pyralis Setup Manual

This manual is the book-style index and maintained written reference for Pyralis.

Use it like a small book. Read the early chapters in order, then jump to the chapter for the thing you are wiring in Unity.

## Beginner Promise

This manual should assume the reader is smart but new.

That means chapters should explain the tiny steps:

- what to click in Unity
- what asset to create
- what the asset is for
- what field to assign
- what can be left empty
- what warning is normal
- what to check after pressing Play

Do not assume the reader already understands definitions, profiles, prefabs, roots, services, input, runtime patterns, or pawn setup.

If a setup step says "create a profile," the chapter should say which menu item creates it, what to name it, where to assign it, and how to know it worked.

Runtime pattern assets are part of the manual too. Their `description` and `setupNotes` fields should explain the game type in clear route language and tell the user what to wire next in Unity.

Custom Inspectors are the live setup checklist. Setup-facing Inspectors should use the shared `PyralisInspectorGuide` helper so users see consistent guidance for what an asset is, when to use it, what to create first, what to assign first, what can be customized safely, and how to validate the setup.

This now covers the main guided setup surfaces: `GameplaySessionBootstrap`, sessions, modes, runtime patterns, participants, pawns, pawn profiles, feature modules, feature profiles, action definitions, combat definitions, projectile definitions, enemy combat assets, status effects, animation definitions, scene-flow assets, hazard assets, input zones, and visual flash presets.

## Why This Manual Exists

Pyralis is intentionally broad. It can support pawn-backed games, camera-only games, board/card/tabletop games, projectile combat, brawlers, fighters, turn/menu selection, and hybrid setups.

That flexibility makes a wizard tempting, but a wizard is expensive to maintain while the system is still expanding. For now, the hierarchy is: Inspector guides are the live checklist, `START_HERE.md` is the first read, `CANONICAL_SETUP.md` is the technical contract, and this manual is the book/index. Future wizards should be based on that same hierarchy instead of replacing it.

## How To Read It

Read these first:

1. `START_HERE.md`
2. `AUTHORING_MODEL.md`
3. `RUNTIME_PATTERN_COOKBOOK.md`

Then read only the setup chapter for the thing you are wiring.

Use `CANONICAL_SETUP.md` as the technical contract when a first-scene chapter and implementation detail disagree.

## Part 1: First Setup Pass

| Chapter | Use it for |
|---|---|
| `START_HERE.md` | the first-scene setup path and decision flow |
| `docs/authoring-native-1p-proof-checklist.md` (repo root) | the native one-pass 1P movement proof scenario and expected pass criteria |
| `AUTHORING_MODEL.md` | definitions, profiles, participants, pawns, runtime components, and asset chains |
| `RUNTIME_PATTERN_COOKBOOK.md` | choosing overlapping runtime patterns before wiring scene objects |
| `Prefabs/Bootstrap_Example_Setup.md` | creating the gameplay root and assigning the session |
| `SCENE_SETUP_GUIDE.md` | mapping scene types to required and optional roots |

## Part 2: Actor And Camera Setup

| Chapter | Use it for |
|---|---|
| `Prefabs/Pawn_Setup.md` | pawn-backed actors, movement stacks, presentation, and animation |
| `Prefabs/Pawn_Shadow_Setup.md` | reusable shadows for 2D, 2.5D, and rigged 3D pawns |
| `Prefabs/Camera_Setup.md` | shared camera, camera-only control, board views, and camera shake |
| `Prefabs/Multiplayer_Setup.md` | local join, `PlayerInputManager`, participant slots, and split screen |

## Part 3: Combat And Action Setup

| Chapter | Use it for |
|---|---|
| `Prefabs/Combat_Definitions_Setup.md` | combat actions, combat sequences, projectiles, fire modes, and impact definitions |
| `Prefabs/Health_Combat_Setup.md` | health, hitboxes, hurtboxes, death, and damage feedback |
| `Prefabs/Feature_Module_Framework_Setup.md` | modular actor features such as pickups, status, feedback, interaction, and reactions |
| `Prefabs/Enemy_Setup.md` | enemy runtime setup and enemy profiles |

## Part 4: World, UI, And Flow Setup

| Chapter | Use it for |
|---|---|
| `Prefabs/Pickups_Setup.md` | collectible setup and pickup feature profiles |
| `Prefabs/Hazard_Difficulty_Setup.md` | hazard data, hazard impact, hazard feedback, and difficulty tuning |
| `Prefabs/Encounter_Zones_Setup.md` | encounter boundaries, zones, and triggered gameplay spaces |
| `Prefabs/Scoring_Setup.md` | score, timers, resources, victory points, and round results |
| `Prefabs/UI_HUD_Setup.md` | HUDs, world bars, menus, and UI roots |
| `Prefabs/Board_Card_Tabletop_Setup.md` | no-pawn board, card, seats, hands, turns, pieces, markers, and selection surfaces |
| `Prefabs/Settings_Setup.md` | settings manager, settings profile, audio, fullscreen, and persistent settings |
| `Prefabs/Scene_Flow_Setup.md` | fades, scene transitions, restart, and menu flow |
| `Prefabs/Respawn_Setup.md` | respawn points, lives, death recovery, and player return flow |

## Part 5: Example Game Shapes

| Chapter | Use it for |
|---|---|
| `Prefabs/Arcade_Menu_Example_Setup.md` | arcade loop with menu/HUD setup |
| `Prefabs/Brawler_Menu_Example_Setup.md` | brawler-oriented example setup |
| `Prefabs/Board_Card_Tabletop_Setup.md` | no-pawn board/card/tabletop setup example |
| `../NewGameTypeGuide.md` | planning a new game type before it has a dedicated setup chapter |

## Part 6: Reference And Maintenance

| Chapter | Use it for |
|---|---|
| `CANONICAL_SETUP.md` | exact supported setup contract and technical contract |
| `Systems/Architecture_Overview.md` | architecture and composition-root overview |
| `Systems/Migration_and_Readability_Standard.md` | cleanup rules, migration expectations, and readability standard |
| `README.md` | setup folder index |

## Chapter Template

Every setup chapter should eventually use this shape:

1. What this chapter sets up.
2. When you need it.
3. What you should already have.
4. Assets to create.
5. Scene objects to create.
6. Components to add.
7. Inspector fields to assign.
8. What can stay empty.
9. Press Play validation.
10. Common mistakes.
11. What to read next.

Keep chapters small. If a chapter starts explaining more than one system, split it.

## Wizard Rule

Do not make a wizard the only maintained setup contract.

A wizard can help create assets later, but the manual must remain the readable contract for what the wizard is doing.

Inspector guidance is the preferred middle ground before a wizard. It keeps setup knowledge in code near the asset or component it explains, which makes the guidance easier for developers and agents to maintain while features evolve.

## PDF-Friendly Rule

The source stays as linked Markdown chapters so Unity/package maintenance stays simple.

When exporting to PDF later, assemble chapters in this manual order. Avoid relying on a generated PDF as the editable source.
