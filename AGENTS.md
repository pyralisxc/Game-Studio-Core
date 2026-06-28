# Game Studio Core Agent Instructions

Use this project file as the local instruction anchor for the Game Studio Core project root. It complements Cameron's global Codex contract; follow the more specific project instruction when they differ.

## Required Skills

- Use `game-studio-core-unity-agent` for all Game Studio Core, NeonBlack Gameplay, Pyralis, shared mechanics, package maintenance, setup validation, and 2D/2.5D/3D runtime-lane work.
- Pair it with `unity-project-stewardship`, `agentic-project-stewardship`, `library-first-feature-development`, and `long-running-process-stewardship` when their triggers apply.

## Project Context

Pyralis lives at:

`Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay`

Before product, architecture, or package-maintenance decisions, start with the small living-doc set:

- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\README.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\CURRENT_STATE_AUDIT.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\ARCHITECTURE_BLUEPRINT.md`

Read deeper only when changing that surface:

- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\Authoring\START_HERE.md` for the PYS Authoring integration path.
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\Authoring\AUTHORING_BLUEPRINT.md` for Pyralis/PYS ownership boundaries.
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\Authoring\AUTHORING_MODEL.md` for PYS contract and validation evidence rules.
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\Authoring\CANONICAL_SETUP.md` for setup-chain contract details.
- `Packages\com.pys.authoring\Docs\PROJECTION_CONTRACTS.md` for PYS tab behavior and export rules.
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\FEATURE_DEVELOPMENT_ROADMAP.md` for current feature expansion priorities.

Do not use dated audits, migration notes, or old setup guides as source truth. If onboarding density rises, fold the current rule into this file, the package README, or the relevant living doc, then delete the stale source.

## Development Expectations

- Keep the core motto visible in every architecture choice:
  - Unity owns engine behavior.
  - Pyralis owns gameplay meaning.
  - Reflection discovers structure.
  - Dependency analysis discovers setup relationships.
  - Validators witness local semantic readiness.
  - The graph compiles understanding.
  - PYS Authoring renders guidance.
- Treat gameplay mechanics as shared platform capabilities by default.
- Check how mechanics apply to `Sprite2D`, `Billboard2_5D`, `Rigged3D`, non-pawn participants, and networking/authority when relevant.
- Prefer Unity packages, Unity ecosystem packages, and credible free/open packages before writing custom infrastructure.
- Treat code, folder, package, docs, and validation maintenance as part of each slice, not as deferred cleanup.
- Skip Unity generated/cache/build files for normal context. Treat `.meta` files as identity/reference files: preserve and create them with assets/scripts, inspect them only when GUID/reference/import behavior matters, and never blanket-ignore them during moves or package changes.
- Keep `GameplaySessionBootstrap`, `PyralisGameplayLifetimeScope`, participant/session services, authored definitions/profiles, PYS contracts/reflection, dependency evidence, validators, and PYS graph projections as the current source of truth.
- Do not reintroduce hidden singleton service lookups, first-player assumptions, or compatibility bridges unless preserving committed content requires it.
- Keep the runtime simple and Unity-native: explicit pawn sibling components describe what a pawn is, direct module-owned components describe optional capabilities, and `ParticipantDefinition` owns input for the participant driving the pawn.
- Keep active docs focused on present truth and intended direction. Remove stale legacy/history commentary from setup and architecture docs unless it protects active migration, shipped compatibility, or project data; move useful history to an archive, changelog, migration note, or audit file.

### PYS Authoring Proof Semantics For Pyralis

For PYS Authoring while observing Pyralis, a proof is not the earliest technical Play Mode baseline. A proof is the successful Play Mode attempt that satisfies the selected Intent. If no Intent exists, or the authored setup has moved beyond the selected Intent, the active proof should come from the next meaningful Map/topology-derived route.

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

Intent, Overview, and Guide form the user-driven development path workbench. Map is the scene/setup reality workbench. Facts and Hygiene are codebase/system audit workbenches with separate ownership. Route Proof Trace is Guide's exported route projection, not a separate tab or proof engine. PYS owns these projections; Pyralis supplies target-project contracts, validation records, and Unity evidence.

## Validation

Preferred project gate:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Close the GUI Unity Editor before running the full gate. Treat `Logs\Codex` XML summaries as Unity test evidence. If the full gate cannot run, explain why and name the residual risk.

## Project Path Portability

Keep shared project files portable for other team members. Active docs, tests, editor tools, and package code should use project-relative paths such as `.\Tools\Validation\Run-PreSceneValidation.ps1`, Unity project paths such as `Assets/...` and `Packages/...`, or runtime-derived paths such as `Application.dataPath`.

Do not commit or rely on machine-local absolute paths, `.codex`, Unity layout paths, or a specific developer's desktop folder. Unity-generated local state is intentionally ignored by both `.gitignore` and `ignore.conf`: `Library`, `Temp`, `Logs`, `UserSettings`, generated `.csproj` / `.sln` files, private files, and IDE folders should not be treated as shared project content.

### Authoring Validation Protocol

When validating the Pyralis authoring path, use Computer Use to drive the Unity Editor like a beginner-to-adaptable user would: open PYS Authoring from `Tools/PYS/Authoring`, follow its guidance, use the Project window Create menu, Hierarchy, Inspector, Add Component, object picker, scene view, and Play Mode. The point is to prove the user can create and customize their own setup from the guidance.

Do not satisfy authoring validation by adding one-off scene generators, factory menu items, hidden auto-wire scripts, or generated "proof scenes" that bypass the Authoring Window and native Unity workflow. Those shortcuts can be useful only as separate developer tooling after the real authoring path has been manually proven, and they must not be treated as evidence that the authoring guide works.

Fix authoring/code issues discovered during the Computer Use pass, but keep the product behavior user-authored: guidance should point to the Unity object, asset, field, or component to customize rather than choosing the user's map, art, layout, camera framing, NPC content, combat shape, or quest structure for them.

## Checkpoints

At meaningful checkpoints, classify the state as `In progress`, `Checkpoint reached`, `Phase complete`, `Project complete`, or `Blocked`.

For phase/project completion, provide a check-out audit covering product state, code/architecture state, maintenance/folderbase state, docs/standards state, verification evidence, residual risks, optional enhancements, and the recommended next move.
