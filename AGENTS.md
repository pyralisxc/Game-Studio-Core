# Game Studio Core Agent Instructions

Use this project file as the local instruction anchor for the Game Studio Core project root. It complements Cameron's global Codex contract; follow the more specific project instruction when they differ.

## Project Context

NeonBlack Gameplay lives at:

`Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay`

Before product, architecture, or package-maintenance decisions, start with the small living-doc set:

- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\README.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\CURRENT_STATE_AUDIT.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\ARCHITECTURE_BLUEPRINT.md`

Read deeper only when changing that surface:

- The installed/source `com.pys.authoring` package docs for PYS Authoring package behavior, projection contracts, exports, and integration rules.
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\FEATURE_DEVELOPMENT_ROADMAP.md` for current feature expansion priorities.

Do not use dated audits, migration notes, or old setup guides as source truth. If onboarding density rises, fold the current rule into this file, the package README, or the relevant living doc, then delete the stale source.

## Development Expectations

- Keep the core motto visible in every architecture choice:
  - Unity owns engine behavior.
  - NeonBlack Gameplay owns gameplay meaning.
  - Reflection discovers structure.
  - Dependency analysis discovers setup relationships.
  - Validators witness local semantic readiness.
  - The graph compiles understanding.
  - PYS Authoring observes gameplay contract evidence from its own package lane.
- Treat PYS Authoring primarily as a codebase-quality and system-health observer. It can also help expose setup guidance, route proof, and Unity evidence, but it must not become a replacement for Unity's native Inspector, prefab, scene, package sample, or Test Runner workflows.
- Treat gameplay mechanics as shared platform capabilities by default.
- Check how mechanics apply to `Sprite2D`, `Billboard2_5D`, `Rigged3D`, non-pawn participants, and networking/authority when relevant.
- Prefer Unity packages, Unity ecosystem packages, and credible free/open packages before writing custom infrastructure.
- Treat code, folder, package, docs, and validation maintenance as part of each slice, not as deferred cleanup.
- Commit small, coherent slices as work proceeds. Prefer one commit per verified docs update, audit, refactor family, or behavior change.
- Skip Unity generated/cache/build files for normal context. Treat `.meta` files as identity/reference files: preserve and create them with assets/scripts, inspect them only when GUID/reference/import behavior matters, and never blanket-ignore them during moves or package changes.
- Do not modify the project `Assets/` folder for this refactor lane unless Cameron explicitly asks. Work in the package, docs, tests, and tooling surfaces first.
- Do not create package samples automatically for this refactor lane. Cameron will hand-author samples from the working codebase. Agents may document the desired sample shape and keep the package ready for samples, but should not generate sample scenes, prefabs, or sample assets by default.
- Keep `GameplaySessionBootstrap`, `GameplayLifetimeScope`, participant/session services, authored definitions/profiles, PYS contracts/reflection, dependency evidence, validators, and PYS graph projections as the current source of truth.
- Game Studio Core consumes `com.pys.authoring` as an installed external package; do not recreate or restore an embedded `Packages\com.pys.authoring` source folder in this project.
- Do not reintroduce hidden singleton service lookups, first-player assumptions, or compatibility bridges unless preserving committed content requires it.
- Keep the runtime simple and Unity-native: explicit pawn sibling components describe what a pawn is, direct module-owned components describe optional capabilities, and `ParticipantDefinition` owns input for the participant driving the pawn.
- Keep active docs focused on present truth and intended direction. Remove stale legacy/history commentary from setup and architecture docs unless it protects active migration, shipped compatibility, or project data; move useful history to an archive, changelog, migration note, or audit file.

### Authoring Surface Refactor Defaults

Use `docs\superpowers\plans\2026-07-04-codebase-authoring-surface-refactor.md` and `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\AUTHORING_SURFACE_REFACTOR_AUDIT.md` as the active execution map for the package-wide simplification lane.

Default decisions for that lane:

- Start with Feedback plus simple Presentation before higher-risk systems such as locomotion, combat, RPG, tabletop, networking, or editor tooling.
- Reduce visible Unity setup before reducing internal implementation. A few clear modules with assigned profiles are preferable to many small scene components that require code navigation to understand.
- Keep internal code modular where it clarifies ownership: state machines, handlers, private lanes, readers, sinks, and small helpers are valid implementation details.
- Use profiles for reusable authored variation, not as one giant storage object and not for every local field.
- Treat pass-through scripts, duplicate fallback fields, and scripts that only bridge unclear ownership as merge/delete/refactor candidates.
- Prefer clean compatibility cuts when references allow. Do not keep bridges for superseded paths unless serialized scenes, prefabs, package consumers, or committed user content still depend on them.
- Keep runtime-created objects only when they are gameplay output, such as spawned pawns, projectiles, effects, popups, generated board views, or camera focus helpers. Do not create hidden setup repair objects such as missing managers, services, profiles, roots, or camera rigs.
- Split validation by ownership: Unity-local field/component errors belong near the Inspector through `RequireComponent`, `OnValidate`, custom inspectors, or prefab structure; PYS-visible evidence should focus on codebase quality, ownership pressure, route semantics, topology, proof readiness, and cross-object setup truth.

### PYS Authoring Proof Semantics For NeonBlack Gameplay

For PYS Authoring while observing NeonBlack Gameplay, a proof is not the earliest technical Play Mode baseline. A proof is the successful Play Mode attempt that satisfies the selected Intent. If no Intent exists, or the authored setup has moved beyond the selected Intent, the active proof should come from the next meaningful Map/topology-derived route.

Prerequisites are not proof completion. Movement, input, camera, session, participant, or pawn setup can be required setup for a combat/traversal/presentation proof, but they must not replace the chosen proof target.

Before changing PYS Authoring tabs, projections, or exports, state the affected surface's projection contract:

- what question it answers
- what payload it may show
- what payload it must not show
- whether it may show native Unity actions
- whether it may show desired Intent
- whether it may show current scene/setup reality
- whether it may show code/ownership audit pressure
- whether its export saves the same projection the UI renders

Intent, Overview, and Guide form the user-driven development path workbench. Map is the scene/setup reality workbench. Facts and Hygiene are codebase/system audit workbenches with separate ownership. Route Proof Trace is Guide's exported route projection, not a separate tab or proof engine. PYS owns these projections; NeonBlack Gameplay supplies target-project contracts, validation records, and Unity evidence.

## Validation

Use Unity Test Runner as the preferred project validation surface. Run the relevant EditMode or PlayMode tests from the Unity Editor when validating gameplay, authoring, package, or editor-tool changes.

Manual Unity proof remains first-class evidence. For gameplay feel, authoring usability, scene setup, camera behavior, input, and proof-route confidence, validate in the Unity Editor rather than replacing that work with generated scenes or command-line-only checks.

Command-line validation scripts may be used as optional diagnostics when they are helpful, but they are not the default source of project truth.

## Project Path Portability

Keep shared project files portable for other team members. Active docs, tests, editor tools, and package code should use project-relative paths, Unity project paths such as `Assets/...` and `Packages/...`, or runtime-derived paths such as `Application.dataPath`.

Do not commit or rely on machine-local absolute paths, `.codex`, Unity layout paths, or a specific developer's desktop folder. Unity-generated local state is intentionally ignored by both `.gitignore` and `ignore.conf`: `Library`, `Temp`, `Logs`, `UserSettings`, generated `.csproj` / `.sln` files, private files, and IDE folders should not be treated as shared project content.

### Authoring Validation Protocol

When validating the gameplay authoring path, use Computer Use to drive the Unity Editor like a beginner-to-adaptable user would: open PYS Authoring from `Tools/PYS/Authoring`, follow its guidance, use the Project window Create menu, Hierarchy, Inspector, Add Component, object picker, scene view, and Play Mode. The point is to prove the user can create and customize their own setup from the guidance.

Do not satisfy authoring validation by adding one-off scene generators, factory menu items, hidden auto-wire scripts, or generated "proof scenes" that bypass the Authoring Window and native Unity workflow. Those shortcuts can be useful only as separate developer tooling after the real authoring path has been manually proven, and they must not be treated as evidence that the authoring guide works.

Fix authoring/code issues discovered during the Computer Use pass, but keep the product behavior user-authored: guidance should point to the Unity object, asset, field, or component to customize rather than choosing the user's map, art, layout, camera framing, NPC content, combat shape, or quest structure for them.
