# Runtime Pattern Cookbook

Use this guide before wiring Unity scenes or prefabs. It helps choose the `RuntimePatternDefinition` assets that belong in a `GameSetupProfile`.

## Core Rule

A game setup can use more than one runtime pattern. Patterns describe capabilities and control surfaces, not exclusive genres.

Assign the selected patterns to a `GameSetupProfile`, then assign that setup profile to `GameModeDefinition.setupProfile`. Each pattern should also declare its presentation/runtime lanes and first-proof requirements so Overview, Guide, Map, and Validate can explain the same setup facts without text guessing.

Authoring should react to those selected patterns. A pawn action setup should prioritize pawn prefab, input, spawn, camera, and movement proof. A tabletop setup should not ask for pawn fields; it should prioritize board/card/action/cursor surfaces. A scoring setup should not block Play Mode before one score-changing event exists. The route should guide the developer's creative choices instead of forcing every game through one starter scene.

## Common Setup Recipes

| Game direction | Recommended patterns | Pawn required? |
|---|---|---|
| 2D arcade score loop | Realtime Character, Scoring/Objectives, Camera/Cursor Control | Usually |
| 2D procedural sidescroller | Realtime Character, Procedural Generation, Scoring/Objectives, Animation/Presentation | Usually |
| Brawler or action fighter | Realtime Character, Combat, Animation/Presentation, Camera/Cursor Control | Usually |
| Brawler with guns or spells | Realtime Character, Combat, Projectile Combat, Animation/Presentation | Usually |
| Twin-stick or arena shooter | Realtime Character, Projectile Combat, Combat, Scoring/Objectives | Usually |
| Turret, trap, or hazard shooter | Projectile Combat, Scoring/Objectives, Procedural Generation | Optional |
| Turn-based tactics | Board/Card/Tabletop, Turn/Menu Action, Combat, Camera/Cursor Control | Optional |
| Card battler | Board/Card/Tabletop, Turn/Menu Action, Scoring/Objectives, Camera/Cursor Control | No |
| Board game or tabletop variant | Board/Card/Tabletop, Turn/Menu Action, Camera/Cursor Control, Scoring/Objectives | No |
| Camera-only interactive scene | Camera/Cursor Control, Scoring/Objectives if needed | No |
| Menu-driven RPG combat | Turn/Menu Action, Combat, Scoring/Objectives, Animation/Presentation if actors are shown | Optional |

## Overlap Is Expected

Do not force a game into one category. Some examples:

- A tactics card battler can use board/card/tabletop play, turn/menu action, projectile-style targeting, scoring/objectives, and camera/cursor control.
- A brawler can use realtime character movement, melee combat, projectile combat, animation/presentation, scoring, and procedural encounter placement.
- A tabletop game can use no pawn controller at all, while still using participants, scoring, scene flow, settings, UI, and camera/cursor control.

## Pawn Decision

Use `PawnRoot` when the participant controls or owns an actor with authored behavior:

- movement
- combat or damage receiving
- animation
- traversal
- feature modules
- pickup collection
- presentation state

Avoid `PawnRoot` when the participant controls a board, hand of cards, cursor, camera, faction, menu, or turn state without an actor body.

## Validation Check

Before wiring scene objects:

- the `GameSetupProfile` includes every major runtime surface the scene expects
- `GameModeDefinition.setupProfile` is assigned
- pawn-required setups have a participant and pawn authoring path
- non-pawn setups include a camera, cursor, menu, board, or UI control path
- projectile-heavy setups include projectile combat
- turn/menu setups include action or targeting support

If validation complains, resolve that first. Prefab wiring should follow the selected setup, not define it by accident.
