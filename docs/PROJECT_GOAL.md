# Project Goal Command

Use this as the starting command for the next full-cycle improvement pass:

`/goal Make the entire Game Studio Core repository creator-ready end-to-end across all packages, systems, lanes, and workflows: editor tooling, authoring UX, runtime patterns, package composition, docs, tests, and sample content (not just one lane). Improve authoring windows, scene setup docs, runtime setup pathways, package/runtime quality, project templates/samples, and Unity workflow ergonomics so every active route can be built from scratch in-editor with native workflows (Hierarchy/Project create, Inspector Add Component search, Inspector drag/drop wiring). Include all gameplay systems and official lanes (`Game Shell`, `Pawn-Backed Action` in `Sprite2D`/`Billboard2_5D`/`Rigged3D`, `Non-Pawn Tabletop/Card`, `Network Chain`, `RPG/Progression`, and dependent feature modules), validated end-to-end for beginner-adept users before any route is treated as production-ready.`

Acceptance criteria for this goal:

- The goal applies across the project and all relevant runtime routes, not a single lane.
- The goal applies to every package and major feature surface in this repository, including shared tool surfaces and workflow tooling that support multiple gameplay lanes.
- The workflow stays native to Unity: create in Hierarchy/Project, add components via Inspector search, and wire references with Inspector drag-and-drop.
- Every active lane gets a clean route-specific proof (`1P`, `1-2P`, networked, non-pawn, etc.) before optional features are added.
- Blockers in authoring flow, setup docs, validation, runtime behavior, or editor tooling are logged with severity and concrete remediation.
- Documentation, setup guidance, and editor tool text stay aligned with the actual runtime contract and route-appropriate requirements.
- Route-level fixes are reusable unless explicitly route-specific, so the same usability improvements work across the whole project.
- Project-wide quality gates include editor UI behavior, onboarding experience, runtime assembly checks, and sample/asset organization, so no single lane or module can pass while another core surface is out of alignment.
