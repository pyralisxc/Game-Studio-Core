# Prefab Setup Guide Index

Use these guides after `Docs/Setup/START_HERE.md` and `Docs/Setup/AUTHORING_MODEL.md`. Treat `Docs/Setup/MANUAL.md` as the book-style index when you need a wider map.

These chapters should read like clear Unity implementation instructions, not shorthand reference notes. Each guide should say what to create, what to click, what to assign, what can stay empty, and how to validate the setup in Play Mode.

Every prefab guide starts with a `Before You Wire This` section. Read that section first, select matching existing runtime patterns in a `GameSetupProfile`, assign the profile to `GameModeDefinition.setupProfile`, and resolve validation before wiring scene objects in Unity. Create new runtime pattern assets only when you are defining an advanced custom setup category.

Beginner rule: do not read every guide in this folder. Pick the one for the thing you are wiring right now.

## Recommended Reading Order

1. `Bootstrap_Example_Setup.md` - create the authoring chain and startup scene root.
2. `Pawn_Setup.md` - build pawn-backed actors only when the selected setup needs pawns.
3. `Feature_Module_Framework_Setup.md` - attach reusable actor or pawn capabilities.
4. `Camera_Setup.md` - choose follow, cursor, board, menu, or camera-owned control behavior.
5. Feature-specific guides such as combat, scoring, pickups, hazards, UI, settings, scene flow, multiplayer, and respawn.

## Runtime Pattern Map

| Guide | Primary runtime patterns |
|---|---|
| `Arcade_Menu_Example_Setup.md` | Realtime Character, Scoring/Objectives, Camera/Cursor Control |
| `Brawler_Menu_Example_Setup.md` | Realtime Character, Combat, Animation/Presentation, optional Projectile Combat |
| `Camera_Setup.md` | Camera/Cursor Control, Realtime Character, Board/Card/Tabletop |
| `Combat_Definitions_Setup.md` | Combat, Projectile Combat, Turn/Menu Action |
| `Encounter_Zones_Setup.md` | Realtime Character, Combat, Camera/Cursor Control |
| `Enemy_Setup.md` | Combat, Realtime Character, Projectile Combat, Animation/Presentation |
| `Feature_Module_Framework_Setup.md` | Depends on module; select every pattern the module expects |
| `Hazard_Difficulty_Setup.md` | Realtime Character, Scoring/Objectives, Procedural Generation |
| `Health_Combat_Setup.md` | Combat, Realtime Character, Projectile Combat, Turn/Menu Action |
| `Multiplayer_Setup.md` | Realtime Character, Camera/Cursor Control, Board/Card/Tabletop |
| `Pawn_Setup.md` | Realtime Character, Combat, Projectile Combat, Animation/Presentation |
| `Pawn_Shadow_Setup.md` | Animation/Presentation, Realtime Character, Board/Card/Tabletop |
| `Pickups_Setup.md` | Realtime Character, Scoring/Objectives, Procedural Generation |
| `Respawn_Setup.md` | Realtime Character, Combat, Scoring/Objectives |
| `Scene_Flow_Setup.md` | Platform Core, Camera/Cursor Control, destination-scene pattern |
| `Scoring_Setup.md` | Scoring/Objectives, Realtime Character, Board/Card/Tabletop |
| `Settings_Setup.md` | Platform Core, Camera/Cursor Control, Realtime Character |
| `UI_HUD_Setup.md` | Scoring/Objectives, Realtime Character, Board/Card/Tabletop, Turn/Menu Action |

## Non-Pawn Games

Pyralis does not require every game to have a character controller. For board, card, tabletop, puzzle board, menu, or camera-only interaction:

- choose `Board/Card/Tabletop`, `Camera/Cursor Control`, and/or `Turn/Menu Action` runtime patterns
- avoid adding `PawnRoot` unless the game needs actor pieces with authored pawn behavior
- wire UI, cursor, camera, turn, scoring, and scene-flow guides around the setup profile rather than forcing a realtime character stack

## Unity Wiring Rule

If a guide says to place a scene service such as scoring, settings, scene flow, hazards, pickups, or UI, place it under a clear scene systems root unless the guide explicitly says the 2D score-loop `GameManager` owns that wiring for the selected flow.
