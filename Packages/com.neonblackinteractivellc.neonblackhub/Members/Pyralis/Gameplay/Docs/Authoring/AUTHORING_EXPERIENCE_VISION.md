# Pyralis Authoring Experience Vision

This is the concise product guardrail for Pyralis authoring.

Use `AUTHORING_BLUEPRINT.md` for the detailed implementation plan, `AUTHORING_MODEL.md` for the asset chain, and this file when deciding whether new guidance still feels like the intended product.

## North Star

Authoring is the map. Unity is the workshop. Inspectors are the local knobs.

The Authoring Window guides a selected first proof, not a complete game setup. It should help the user understand what they are building, what native Unity action comes next, what blocks the proof, what can be customized now, and what should wait.

## Core Contract

The Authoring Window should not secretly build the scene or compete with the Inspector.

Users still:

- create assets through the Project window
- create scene objects through the Hierarchy
- add scripts through Inspector Add Component
- assign references through Inspector fields
- enter Play Mode to prove behavior

Authoring:

- records or infers setup intent
- compares intent against scene/project evidence
- names the current first proof
- shows the proof chain for hybrid routes without making later systems feel blocking
- recommends one next native Unity action
- separates blocking setup from customization and deferred features

## Intent, Evidence, Proof

Treat setup as three different truths:

- `Intent`: what the user is trying to build.
- `Evidence`: what Pyralis can currently see in assets, prefabs, and scenes.
- `Proof`: what has actually been attempted or passed in Play Mode.

Do not collapse these into one "ready" label. A detected object is not the same as a linked setup object, and a statically valid route is not the same as a passed Play Mode proof.

Preferred evidence ladder:

- `Missing`
- `Found candidate surface`
- `Linked to active setup`
- `Validated`
- `Play-proven`

Preferred proof language:

- `Not ready for first proof`
- `Ready to attempt first proof`
- `Play proof not run`
- `Play proof passed`
- `Play proof stale`

## Start Guided Setup

When nothing is started, prefer `Start Guided Setup` or `Choose First Proof` language over `Start New`.

The first question should be human-facing:

- Move one character
- Select one board piece
- Fire one projectile
- Update one score label
- Move one camera or cursor
- Prove one network ownership path

The tool can map that choice to route facts internally, but the user should not have to start by understanding runtime taxonomy.

## Native Action Schema

Every recommended action should be concrete:

`Verb + Unity surface + target + field/component + success check`

Examples:

- Create in Project on the opened setup folder, use Create > NeonBlack > Definitions > Session Definition, then confirm the asset appears under that folder.
- Add in Inspector on Gameplay Root, use Add Component > GameplaySessionBootstrap, then confirm Authoring shows the scene root.
- Assign in Inspector on Gameplay Root, use Session Definition, then confirm the route changes from missing session to participant setup.
- Enter in Play Mode on the current scene, use Play, then confirm the named first proof.

Every card should say where the user acts: Project, Hierarchy, Inspector, Authoring, or Play Mode.

## Customization

Inspector tooltips explain local fields.

Authoring explains route meaning and timing.

Use Authoring for customization guidance when order matters:

- what can be tuned after the first proof works
- which art/profile/field expresses taste
- what is safe to defer
- what would make Play Mode misleading

Use the Inspector for local values:

- speed
- tint
- profile references
- prefab-specific component references
- camera numbers

## Maintainability Rules

The Authoring Window should render shared models, not own feature intelligence.

New Pyralis features should contribute:

- route facts
- issue codes
- evidence detectors
- native setup actions
- first proof contributions
- proof-chain steps for hybrid routes
- customization suggestions

The central authoring system should compose those facts and present them consistently.

Guardrails:

- keep one canonical route semantic model
- prefer typed evidence/actions/issues over string matching
- keep scene scanning scoped and confidence-based
- distinguish single-route, hybrid-route, and multi-route scenes
- keep inspectors field-local
- keep optional systems visually lower than blockers
- treat intentionally empty fields as explicit success states

## Product Test

A successful Authoring experience should make a Unity user feel:

- I know what I am building.
- I know what proof I am working toward.
- I know which Unity surface to touch next.
- I understand why this field or object matters.
- I can customize without losing the setup story.
- The tool helps me learn Unity instead of hiding Unity from me.
