# Pyralis Authoring Proof Tester Handoff

Date: 2026-06-05

Status: In progress

## Purpose

This handoff prepares the next agent to act as the Authoring 2.0 proof tester. The proof tester's job is to run multiple native Unity proof passes across different route families so the authoring system becomes more reflective, not more hard-coded.

The proof tester should get ahead of Cameron on general setup flow, code support, route evidence, and authoring-system gaps. Cameron remains the final validator for movement feel, route taste, customization ergonomics, asset fit, animations, input feel, art direction, and any product-shaping design choice.

## Current State

Phase 0A has reached a working checkpoint:

- `IAuthoringConventionFactProvider` and `PyralisAuthoringConventionFactRegistry` are active.
- `PyralisConventionAuthoringFacts` is the bridge for unmigrated convention facts.
- `PyralisSprite2DConventionAuthoringFactProvider` owns the first migrated 1P Sprite2D convention surface.
- `PyralisRouteIntentAuthoringFactProvider` makes Intent project-wide instead of side-scroller-only.
- The first side-scroller brawler proof reached a structural checkpoint: setup assets, prefab, scene root, spawn, camera, and Play Mode startup were created through native Unity workflow, but movement feel and final animation/art fit still need Cameron validation.

This is not a completed Authoring 2.0 phase. It is enough evidence to keep iterating with additional proof routes.

## Proof Tester Ethic

The proof tester does not create polished games for the user. It proves whether the Authoring Window, inspectors, validators, facts, contracts, runtime code, and docs can guide a Unity developer through building their own route.

Allowed:

- Create temporary or scoped proof assets when the route needs real Unity objects to test the flow.
- Leave multiple proof routes unfinished when the remaining work is clearly a Cameron/user asset, animation, input, tuning, art, or design decision.
- Fix authoring, validation, runtime, code organization, or docs gaps discovered during proof.
- Use starter values only as editable placeholders and say so plainly.
- Record each proof's unfinishedness as evidence, not as failure, when the missing piece is a valid creator decision.

Not allowed as proof evidence:

- Generated proof scenes that bypass the Authoring Window.
- One-click route factories.
- Hidden auto-wire scripts.
- Preset buttons counted as a custom authored route.
- Hard-coded route docs or code paths that only make the current proof pass.
- Claiming movement/game feel, art fit, animation quality, or route taste has passed without Cameron validation.

## Proof Portfolio Model

Keep a portfolio of route proofs instead of one golden sample. Each proof should answer a different authoring question.

| Proof | Route Family | Main Question | Current Role |
|---|---|---|---|
| Side-scroller brawler | Pawn-backed Sprite2D action | Can a beginner author the core setup chain, pawn prefab, movement profile, camera, spawn, and Play Mode startup? | First structural checkpoint; awaiting Cameron feel/art validation |
| No-pawn tabletop/card/action | Tabletop, card, action selection, turn/rules surfaces | Can the system guide a valid route where pawn fields are intentionally absent and board/action evidence matters more than movement? | Recommended second contrast proof |
| Feature-module seam | Traversal, pickups, combat, feedback, projectile, or custom effect | Can a new reusable feature become visible through contracts/facts/validation without central route edits? | Use after the no-pawn contrast or in parallel if scoped |
| Camera/world standalone | Camera, bounds, playfield, scene surfaces | Can cross-route world/camera guidance stay route-aware without auto-building the user's level? | Run when camera assumptions block route proofs |
| UI/HUD/menu/scoring | Canvas, EventSystem, presenter, score/health/session signals | Can UI-first surfaces prove visible state changes without requiring pawn setup? | Later MVP proof |
| NPC/enemy/custom object | Non-player actors or feature objects | Can authored objects enter Pyralis through contracts/facts without becoming hard-coded pawn copies? | Later contrast proof |
| Networking/authority | Ownership, local/remote participants, runtime references | Can authoring describe authority without bloating exports or local-only routes? | Later, after local evidence is stable |

The next proof should preferably be the no-pawn tabletop/card/action route because it forces the system to prove it is not secretly a pawn-route authoring tool.

## Proof Pass Protocol

Use Computer Use when Unity is open. Keep Unity full screen on the requested display when Cameron asks for it.

For each route:

1. Start from a fresh or intentionally scoped Unity scene.
2. Open the Pyralis Authoring Window.
3. Begin in Intent when no setup context exists.
4. Choose or toggle route intent through the Authoring Window instead of selecting a preset route.
5. Follow only native Unity actions: Project Create menu, Hierarchy, Inspector, Add Component, object picker, drag/drop assignment, prefab save, scene view, and Play Mode.
6. Stop and fix authoring-system gaps when the guidance becomes confusing, over-specific, missing, or falsely confident.
7. Record where facts/providers/contracts/validators should own the fix.
8. Attempt the smallest proof target the route declares.
9. Record what passed structurally, what failed technically, and what remains Cameron/user validation.
10. If code or editor registry changes are made, close GUI Unity and run `.\Tools\Validation\Run-PreSceneValidation.ps1` when practical.

## Evidence Template

Create or update a proof note under `docs/superpowers/plans` for each significant pass.

```text
Route:
Goal:
Starting state:
Authoring Window modes used:
Native Unity actions performed:
Setup assets or scene objects created:
Facts/providers/contracts consulted or missing:
Validation cards or Inspector guides shown:
Customization choices intentionally left to user:
Required Cameron assets or decisions:
First proof attempted:
Observed Play Mode result:
Runtime/editor errors:
Authoring friction:
Code/refactor gaps created:
Fixes made:
Export-footprint concerns:
Ready state:
```

Ready state should be one of:

- `Structural checkpoint`: route setup flow works enough to reach Play Mode or route-specific first proof, but Cameron validation remains.
- `Needs authoring fix`: flow exposes a facts/providers/validators/Inspector/Authoring Window gap that should be fixed before rerun.
- `Needs runtime fix`: authoring flow is understandable, but core runtime code cannot support the route yet.
- `Needs Cameron decision`: route cannot responsibly continue without art, animation, input, tuning, design, or feel direction.
- `Promotable`: route has passed live proof and Cameron validation where relevant, with tests/docs updated.

## Second Proof Recommendation

Run a no-pawn tabletop/card/action proof next.

Why:

- It tests the studio-wide promise better than another Sprite2D route.
- It proves empty pawn fields can be valid instead of warnings.
- It exercises action/rules/selection evidence, camera/cursor, UI hints, and scene surface guidance.
- It should reveal whether Intent and Overview are ranking by route ingredients rather than by hard-coded brawler expectations.

Suggested smallest proof:

- Create a session, game mode, setup profile, and runtime pattern for a tabletop/action route.
- Create one board/action/selection/rules surface using existing Pyralis data types where available.
- Author one visible or inspectable accepted/rejected action.
- Enter Play Mode and verify that the route starts without requiring a pawn prefab.
- Record any missing tabletop facts, validators, inspector handoffs, or proof targets.

Do not build final board art, full card UI, campaign rules, AI, networking, or polished visual treatment unless Cameron chooses that direction.

## Consolidation Rules For Future Agents

- Keep durable phase status in `docs/superpowers/plans/2026-06-04-pyralis-reflective-authoring-hardening-roadmap.md`.
- Keep route proof notes in `docs/superpowers/plans`.
- Keep user-facing Authoring Window behavior in `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_BLUEPRINT.md`.
- Keep asset relationship truth in `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_MODEL.md`.
- Keep active docs present/future-focused. Archive or delete stale preset-route wording when it no longer protects active migration.
- When a proof uncovers a recurring agent workflow lesson, add a short note to `C:\Users\camer\.codex\agent-context\`, but keep project architecture in the project docs.

## Checkpoint Classification

Current classification: Checkpoint reached for the first structural proof and proof-tester handoff. Overall Authoring 2.0 remains in progress.

The next checkpoint is complete when a second, different route family has a live proof note and any discovered authoring-system gaps are either fixed or classified as required follow-up before route promotion.
