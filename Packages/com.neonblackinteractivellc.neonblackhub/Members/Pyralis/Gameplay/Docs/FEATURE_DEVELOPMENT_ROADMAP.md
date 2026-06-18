# Pyralis Feature Development Roadmap

This roadmap tracks active expansion priorities. It is not a history log, audit archive, or exhaustive feature inventory.

## North Star

New gameplay work should follow this loop:

```text
Implement gameplay feature
  -> declare semantic contract
    -> expose real dependencies through Unity references/components
      -> let reflection and validators feed the graph
        -> verify the Authoring Window can guide the setup
```

Avoid adding guide-only systems, hidden runtime auto-wiring, or duplicated setup registries.

## Current Priority Order

1. **First playable pawn route**
   - Session, participant, pawn, input, movement, camera, spawn, and Play Mode proof should stay the reference flow.
   - Supported presentation lanes: `Sprite2D`, `Billboard2_5D`, and `Rigged3D`.

2. **Gameplay ownership simplification**
   - Keep input owned by `ParticipantDefinition`.
   - Keep physical pawn identity in explicit sibling components.
   - Keep optional capabilities in feature modules.
   - Remove duplicate or hidden paths that hide missing authored setup.

3. **Authoring graph quality**
   - Intent filters graph guidance.
   - Overview and Guide show the forward path.
   - Map shows scene/setup reality.
   - Hygiene shows graph/code health.
   - Facts shows read-only vocabulary and evidence.

4. **Presentation and camera cleanup**
   - Keep scene camera ownership clear.
   - Pawns expose targets/sockets, not scene camera rigs.
   - Split large presentation or feedback scripts only at real ownership boundaries.

5. **Route expansion**
   - Add combat, interaction, traversal, RPG, tabletop, strategy, and scoring features through the same contract/reflection/graph loop.
   - Do not add a new setup route system for a genre.

## Done Criteria For A New Feature

- Runtime code compiles and has a clear single owner.
- Setup fields are visible in Unity-native Inspectors.
- Contract metadata explains human meaning only.
- Dependency tree or validators can prove required setup.
- Authoring graph shows readiness and blockers.
- Overview/Guide/Map/Hygiene/Facts use existing projections.
- Relevant edit/play tests or manual Unity proof passes cover the behavior.

## Documentation Rule

When feature work changes durable setup, update one of the living docs. Do not add a new roadmap, audit, plan, or setup guide unless it replaces an existing living doc.
