# Game Studio Core Agent Instructions

Use this project file as the local instruction anchor for the Game Studio Core project root. It complements Cameron's global Codex contract; follow the more specific project instruction when they differ.

## Required Skills

- Use `game-studio-core-unity-agent` for all Game Studio Core, NeonBlack Gameplay, Pyralis, shared mechanics, package maintenance, setup validation, and 2D/2.5D/3D runtime-lane work.
- Pair it with `unity-project-stewardship`, `agentic-project-stewardship`, `library-first-feature-development`, and `long-running-process-stewardship` when their triggers apply.

## Project Context

Pyralis lives at:

`Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay`

Before product, architecture, or package-maintenance decisions, read the relevant local docs:

- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\README.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\CURRENT_STATE_AUDIT.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\ARCHITECTURE_BLUEPRINT.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\Setup\AUTHORING_MODEL.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\Setup\CANONICAL_SETUP.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\Setup\START_HERE.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\FEATURE_DEVELOPMENT_SCOPE.md`
- `Packages\com.neonblackinteractivellc.neonblackhub\Members\Pyralis\Gameplay\Docs\FEATURE_DEVELOPMENT_ROADMAP.md`

## Development Expectations

- Treat gameplay mechanics as shared platform capabilities by default.
- Check how mechanics apply to `Sprite2D`, `Billboard2_5D`, `Rigged3D`, non-pawn participants, and networking/authority when relevant.
- Prefer Unity packages, Unity ecosystem packages, and credible free/open packages before writing custom infrastructure.
- Treat code, folder, package, docs, and validation maintenance as part of each slice, not as deferred cleanup.
- Skip Unity generated/cache/build files for normal context. Treat `.meta` files as identity/reference files: preserve and create them with assets/scripts, inspect them only when GUID/reference/import behavior matters, and never blanket-ignore them during moves or package changes.
- Keep `GameplaySessionBootstrap`, `PyralisGameplayLifetimeScope`, participant/session services, authored definitions/profiles, setup flow, and runtime patterns as the current source of truth.
- Do not reintroduce hidden singleton service lookups, first-player assumptions, or compatibility bridges unless preserving committed content requires it.
- Keep active docs focused on present truth and intended direction. Remove stale legacy/history commentary from setup and architecture docs unless it protects active migration, shipped compatibility, or project data; move useful history to an archive, changelog, migration note, or audit file.

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

When validating the Pyralis authoring system, use Computer Use to drive the Unity Editor like a beginner-to-adaptable user would: open the Authoring Window, follow its guidance, use the Project window Create menu, Hierarchy, Inspector, Add Component, object picker, scene view, and Play Mode. The point is to prove the user can create and customize their own setup from the guidance.

Do not satisfy authoring validation by adding one-off scene generators, factory menu items, hidden auto-wire scripts, or generated "proof scenes" that bypass the Authoring Window and native Unity workflow. Those shortcuts can be useful only as separate developer tooling after the real authoring path has been manually proven, and they must not be treated as evidence that the authoring guide works.

Fix authoring/code issues discovered during the Computer Use pass, but keep the product behavior user-authored: guidance should point to the Unity object, asset, field, or component to customize rather than choosing the user's map, art, layout, camera framing, NPC content, combat shape, or quest structure for them.

## Checkpoints

At meaningful checkpoints, classify the state as `In progress`, `Checkpoint reached`, `Phase complete`, `Project complete`, or `Blocked`.

For phase/project completion, provide a check-out audit covering product state, code/architecture state, maintenance/folderbase state, docs/standards state, verification evidence, residual risks, optional enhancements, and the recommended next move.
