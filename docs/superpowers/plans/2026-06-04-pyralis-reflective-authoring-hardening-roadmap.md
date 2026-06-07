# Pyralis Reflective Authoring Hardening Roadmap

Date: 2026-06-04

Status: In progress

## Endpoint Goal

Pyralis authoring should become reflective enough that existing features and new features can find their place in the setup model without a central author hand-writing every route sentence.

The target system is not a scene generator and not a complete-codebase scraper. It is a source-owned authoring contract pipeline:

```text
Runtime/data/source metadata
  -> reflective authoring facts
      -> route intent, setup evidence, native Unity actions, validation issues, and proof targets
          -> Authoring Window, Inspectors, validators, docs, tests, and live Unity proof notes
```

The Authoring Window remains the map. Unity remains the workshop. Inspectors remain the knobs. The authoring system should explain what a feature is, where it belongs, what native Unity action exposes it, what route or lane it supports, what can be safely ignored, and what proof would show it works.

## Studio-Wide Scope

The endpoint is studio-wide Authoring 2.0, not a 1P Sprite2D route finish line.

The 1P Sprite2D route is the first proving route because it exercises the core setup chain, but the required scope includes every MVP authoring family:

- pawn-backed action routes across `Sprite2D`, `Billboard2_5D`, and `Rigged3D` where supported
- no-pawn tabletop, board, card, cursor, camera, faction, menu, and action-selection routes
- feature-module seams such as traversal, pickups, interaction, feedback, combat, hazards, enemies, custom object effects, and future abilities
- camera/world/environment contracts including ordinary Unity colliders, bounds, cameras, layers, zones, anchors, and selectable surfaces
- UI/HUD/menu routes including Canvas, EventSystem, presenter, score, health, feedback, settings, and action-selection surfaces
- NPC/enemy/custom actor and non-player object routes
- networking/authority routes once local authoring evidence is stable

Each route family must eventually have source-owned metadata or contracts, stable ids, native Unity actions, route-aware validation, first proof targets, tests, and live proof evidence.

The recurring live gates include two kinds of proof:

- agent-driven technical proof: Codex can use Computer Use to verify native Unity workflow, wiring, validation, Play Mode startup, runtime errors, and observable behavior
- Cameron validation: Cameron is the final judge for movement feel, game feel, route taste, customization ergonomics, art/asset fit, and whether the proof represents the intended game direction

When a route requires a specific asset, art setup, input asset, animation controller, prefab, scene object, or design choice that Codex cannot responsibly create or judge, the gate must name it as a required Cameron/user asset decision instead of hiding it as a code task.

## Guidance And Enforcement Boundary

Pyralis authoring is an opinionated guide and shared-library quality gate, not a cage around Unity.

Outside the Pyralis body:

- developers can create normal Unity objects, scripts, scenes, assets, prefabs, art, UI, and experimental systems
- the Authoring Window should classify them as outside the active Pyralis setup, not as wrong
- validators should not yell about unrelated native Unity content unless it is wired into a Pyralis setup or Unity itself reports an error

Inside the Pyralis body:

- any feature, component, route, prefab, profile, or asset that claims to be part of Pyralis must attach to the authoring contract/fact/proof system
- the system can be strict about real Pyralis runtime contracts: required references, supported lanes, required components, setup fields, service registration, authority/network rules, and first proof targets
- power users can bypass the Authoring Window while building, but shared Pyralis features must return to the contract spine before they are called reusable or authoring-ready

This boundary changes scope in a healthy way: the project does not need to validate every Unity thing a developer makes, but it must make the handoff into Pyralis explicit. A new developer should be free to experiment outside the body and have a clear path to graduate useful work into the shared library.

## Why This Matters

The first authoring pass proved that Pyralis can guide a user through setup. The flaw discovered after that pass is that too much route meaning can still live in central curated guidance. That creates a long-term risk:

- a new feature can compile but stay invisible to authoring
- two features can solve the same route problem without the system noticing
- old features can be missed because no central row named them
- route guidance can drift from runtime reality
- a live Unity pass can prove only one viewable path instead of proving the setup model

The stronger path is reflective authoring: source types, feature contracts, Unity metadata, validators, and proof targets contribute structured facts that visible guidance renders.

This work is expected to reveal real code gaps and refactor needs. Those are part of the refactor, not cleanup to postpone. If a live authoring pass exposes missing runtime capacity, brittle setup ownership, awkward feature extension, hidden service assumptions, or poor customization flow, the plan should create the necessary code/refactor slice before promoting the route.

## Current Truth

Already in place:

- `PyralisAuthoringContractRegistry` discovers feature-owned `IAuthoringContractProvider` entries reflectively.
- Feature-owned contracts now drive module setup, profile/runtime/lane checks, unsupported-lane cautions, assignment fields, customization moments, and first-proof rows.
- `PyralisAuthoringFactRegistry` provides typed facts for capability cards, setup-flow rows, route proofs, issue metadata, inspector handoffs, route-family coverage, scene evidence, semantic tags, and reflection/convention facts.
- Reflection/convention facts expose selected `CreateAssetMenu`, `AddComponentMenu`, `RequireComponent`, and serialized-field metadata.
- The Authoring Window includes a read-only Facts tab.
- The native 1P Sprite2D proof route has been hardened with missing Add Component menu exposure and authoring fact tests.

Still incomplete:

- Some route relationships and proof labels are still centralized in convention/fact authorship.
- Reflection can discover Unity surfaces, but it cannot infer all product semantics by itself.
- There is not yet a formal source-owned metadata kernel for non-feature-module authoring surfaces.
- Live Computer Use validation has not yet walked the post-contract Authoring Window end to end.
- Existing route surfaces do not all have a coverage audit showing what is source-owned, reflected, centrally curated, unowned, or intentionally deferred.

## Scope And Capacity Sanity Pass

The scope is studio-wide; current capacity is not evenly studio-wide yet.

Current strongest capacity:

- core setup chain concepts: bootstrap, session, mode, setup profile, participants, optional pawn
- feature-owned contract discovery through `IAuthoringContractProvider`
- typed fact registry as a read model
- 1P Sprite2D movement as the first proving route
- broad proof target vocabulary and Facts tab visibility
- automated Unity gate coverage from the prior checkpoint

Current weaker or unproven capacity:

- live Computer Use proof has not yet validated the post-contract authoring workflow
- convention facts still rely on a central bridge file
- 2.5D and 3D route authoring need explicit route coverage audits after the first provider split
- no-pawn tabletop/card/action routes need live proof that empty pawn fields are truly valid
- feature-module seams need runtime proof, not only contract discovery
- camera/world and UI/HUD/menu are cross-cutting and can hide setup assumptions
- networking/authority should stay later until local authoring evidence is stable
- new-developer extension docs are not yet complete enough to be a finished onboarding path
- asset/design requirements and Cameron validation need to be recorded in live proof notes

Capacity rule:

- Do not claim a route is complete because the infrastructure can represent it.
- Do not delay code/refactor work that live proof reveals as necessary for route readiness.
- Do not expand to many routes at once until the convention provider spine has proven one migration cleanly.
- Treat 1P Sprite2D as the proving route, feature-module seams as the extension test, and no-pawn tabletop/card/action as the branch test.
- Treat native Unity experimentation as allowed unless it enters the Pyralis body and claims shared-library support.

## Captured Subagent Findings

The Phase 0 planning agents have been closed. Their useful findings are captured here and in the Phase 0A research memo, so future work should use this plan instead of relying on open side conversations.

| Lane | Captured Decision |
|---|---|
| Scope/current inventory | Audit route coverage by ownership: source-owned, reflected, centrally curated, unowned, or explicitly deferred. |
| Phased roadmap | Harden the spine first, then migrate route families by proof value. |
| Live/manual validation | Use recurring Computer Use promotion gates; static tests and starter packs do not prove authoring usability. |
| Metadata kernel | Add distributed convention/fact providers parallel to feature contract providers; keep the current convention file only as a bridge during migration. |
| Live route order | Start with 1P Sprite2D plus camera/world, then feature-module seams, then no-pawn tabletop/card/action. |
| Feature DoD | A feature is authoring-ready only when runtime, data, authoring metadata, validation, tests, docs, and live proof expectations are covered. |
| Phase 0A research | Keep ScriptableObjects and provider registries; avoid central route files, broad scanners, labels-as-truth, and generated indexes as source of truth. |

Subagent cleanup rule:

- Use new subagents only for bounded sidecar research or disjoint implementation slices.
- Close subagents once their useful findings are integrated.
- Do not leave architecture decisions in subagent transcripts; summarize them in active project docs.

## Scope Map

Every gameplay-facing or setup-facing surface should land in one of these authoring fact categories.

| Category | Examples | Desired Source |
|---|---|---|
| Runtime start and setup chain | `GameplaySessionBootstrap`, `SessionDefinition`, `GameModeDefinition`, `GameSetupProfile` | Reflection plus explicit setup-chain facts |
| Definitions | `PawnDefinition`, `ActionDefinition`, feature definitions, projectile/combat/status definitions | `CreateAssetMenu`, serialized fields, optional source-owned semantics |
| Profiles | movement, presentation, camera, input, combat, traversal, feature profiles | `CreateAssetMenu`, serialized fields, owning runtime/component metadata |
| Runtime components | `PawnRoot`, movement/presentation adapters, HUD binders, camera controllers, tabletop presenters | `AddComponentMenu`, `RequireComponent`, source-owned role metadata |
| Feature modules | `FeatureModuleDefinition` and feature runtimes/profiles | `IAuthoringContractProvider` |
| Route families | pawn action, tabletop/card, UI/HUD/menu, world/camera, NPC/enemy, custom object, networking | route-family facts plus live proof notes |
| Scene evidence | colliders, cameras, bounds, Canvas, EventSystem, board/selectable surfaces, pickups/hazards/enemies | scene scanners plus typed evidence state |
| Proof targets | first movement proof, accepted/rejected board action, UI event, camera framing, network ownership | proof facts plus manual Play Mode evidence |
| Native Unity actions | Project Create, Hierarchy Create Empty, Add Component, Inspector assign field, Play Mode | reflection when possible, explicit metadata when needed |
| Customization moments | art, speed, input, camera framing, UI labels, board layout, route-specific tuning | explicit feature/source-owned metadata |

## Phase 0 - Spine And Data Organization Audit

Goal: Confirm the current authoring spine is strong enough before migrating more route meaning onto it.

The concern:

The current system has a useful fact spine, but authoring meaning is spread across fact types, convention facts, route proof owners, route descriptors, scene validators, inspector handoffs, setup-flow guidance, feature contract providers, and guided editor files. That can be healthy if each owner has a clear boundary. It becomes fragile if those owners all become small central manuals with overlapping route knowledge.

Audit questions:

- Is `PyralisAuthoringFact` still the right shared record, or is it carrying too many unrelated concepts?
- Should contracts, facts, evidence, issues, proof targets, and visible guidance be more strongly separated in code?
- Which files are true source-of-truth owners, and which are renderers/adapters?
- Which data belongs beside the runtime/source type, which belongs in feature contract providers, and which belongs in central route aggregation?
- Can every visible Authoring Window claim be traced to a source, confidence level, and route/proof relationship?
- Do current folder boundaries make future feature discovery obvious, or do features need a stricter module shape?

Expected outcome:

- A spine decision before Phase 1 migration:
  - keep current fact spine with tighter boundaries
  - split the current fact model into narrower records
  - introduce a source-owned metadata layer first
  - reorganize authoring folders before deeper migration

Recommended default:

Keep the current fact registry as the read-only aggregation point, but split responsibilities more clearly before broad migration:

- source-owned metadata/contract providers declare product semantics
- reflection providers declare native Unity surfaces
- validators declare typed issues and evidence states
- proof providers declare proof targets, not proof results
- Authoring Window, Inspector guides, and docs render shared facts instead of owning route truth

Phase 0A research decision:

Use a distributed-provider architecture. Keep ScriptableObjects as the authored data spine, keep `IAuthoringContractProvider` for feature-owned contracts, keep `PyralisAuthoringFactRegistry` as a normalized read model, and add a parallel convention/fact provider layer so route meaning can move out of `PyralisConventionAuthoringFacts`.

Reference:

- `docs/superpowers/research/2026-06-04-pyralis-phase-0a-authoring-spine-research.md`

Acceptance:

- The roadmap names the actual backbone before route migration starts.
- Any deeper folder/model split is done early, while development content can still be rebuilt cleanly.
- The team can explain where a new feature should place runtime code, data assets, authoring metadata, validation, tests, and live proof notes.

Validation:

- No full Unity gate is required for a docs-only audit.
- If code/folder moves happen, run `.\Tools\Validation\Run-PreSceneValidation.ps1`.

## Phase 0A - Convention Provider Spine

Goal: Split the current central convention fact layer into a provider-indexed architecture without changing visible authoring behavior.

Deliverables:

- Add `IAuthoringConventionFactProvider`.
- Add `PyralisAuthoringConventionFactRegistry` with reflective discovery over `NeonBlack.Gameplay*` assemblies.
- Convert `PyralisConventionAuthoringFacts` into the first bridge provider so existing facts still flow through the new registry.
- Update `PyralisAuthoringFactRegistry` to consume convention facts through the convention registry.
- Add tests for provider discovery, duplicate stable ids, bridge-provider continuity, and provider failure diagnostics if available in this slice.

Architecture rules:

- `PyralisAuthoringFact` is the normalized read model, not the only source model.
- Feature contracts remain on `IAuthoringContractProvider`.
- Convention providers own source/domain-level Unity metadata and semantic route relationships.
- Reflection discovers native Unity surfaces; explicit provider metadata declares product meaning.
- Deep scene/prefab validation should stay context-triggered, not global on every selection change.

Acceptance:

- Existing Facts tab coverage remains stable.
- Existing authoring tests still pass.
- New convention providers can be added beside domains without editing the central convention monolith.
- Central route-file gravity is reduced, not relocated under a new name.
- Current checkpoint: the provider spine is active, the bridge still owns unmigrated convention facts, and `PyralisSprite2DConventionAuthoringFactProvider` owns the first migrated 1P Sprite2D convention facts without changing stable ids or visible Authoring Window behavior.

Validation:

- Run targeted EditMode tests for authoring fact registry and source contract coverage.
- Run the full pre-scene gate if code changes touch shared editor registries or tests.

## 2026-06-05 Stewardship Checkpoint

State: In progress.

This checkpoint folds the recent Intent-tab, native Unity workflow, and Apocalyptia side-scroller proof work back into the Authoring 2.0 refactor map. The live work was useful, but it should not become a separate route-pack effort or a hidden preset path.

What is real now:

- Phase 0A is effectively active: convention facts flow through `IAuthoringConventionFactProvider` entries discovered by `PyralisAuthoringConventionFactRegistry`.
- `PyralisConventionAuthoringFacts` remains the bridge for unmigrated convention facts.
- `PyralisSprite2DConventionAuthoringFactProvider` owns the first migrated 1P Sprite2D convention surface.
- `PyralisRouteIntentAuthoringFactProvider` gives the Intent tab studio-wide route-intent facts instead of making the side-scroller proof the whole product.
- The Authoring Window now has stronger Intent, Facts, semantic location, colored-word, and beginner-facing guidance surfaces, but those surfaces must keep reading from facts and providers rather than hard-coded route scripts.
- Export-footprint boundaries are now part of the authoring architecture: editor authoring contracts, facts, providers, validators, inspectors, and proof tooling must stay out of player builds, while route runtime references should not pull unrelated systems or large assets into exports.
- A native Unity side-scroller proof pass created and wired a route through ordinary Project, Hierarchy, Inspector, Add Component, object picker, prefab, scene, camera, and Play Mode actions. That proof surfaced real usability issues, but it is not route promotion yet.

What the recent live proof means:

- The old "Route Presets" framing was a product problem because it made a customizable authored path feel like a preselected setup. Current wording should treat these as editable starting points only, and route proof must not count helper buttons as evidence of a beginner-authored path.
- The Intent tab is the natural starting surface before bootstrap details, especially when the user only knows the game they want to make. It should ask from world/playfield and control shape down into movement, camera, input, combat, UI, animation, networking, and export concerns.
- Side-view 2D gravity is not the same authoring path as top-down 2D free movement. The system should guide the user to choose playfield shape and gravity semantics instead of pushing every 2D route through the same movement assumptions.
- The side-scroller proof showed that a user can reach a mostly wired setup, but movement feel and animation/art fit still require Cameron validation. Codex should name missing user assets or design choices instead of pretending code has proven them.
- Multiple unfinished proof routes are allowed and useful when the unfinished work is honestly reserved for Cameron/user art, animation, input, tuning, game-feel, or design validation. Those partial proofs should still verify setup flow, runtime support, route evidence, export boundaries, and authoring-system gaps.
- Usability gaps found during live proof are phase work. They should be converted into reflective facts, source-owned metadata, validators, inspector parity, or Authoring Window rendering improvements, not left as later cleanup.

Required near-term work before route promotion:

1. Finish the Phase 1 route coverage and residue map, including which current facts are source-owned, reflected, centrally bridged, missing, or intentionally deferred.
2. Continue splitting central convention facts into domain/route providers, starting with surfaces proven by 1P Sprite2D and then contrasting with one no-pawn route and one feature-module route.
3. Add the Phase 2 source-owned metadata kernel for product semantics that reflection cannot infer.
4. Convert live proof pain points into reusable authoring improvements:
   - ParticipantDefinition inspector parity with PawnDefinition-style guidance.
   - Camera setup broken into smaller native Unity substeps with completion evidence.
   - Project-window asset disambiguation guidance for many similar ScriptableObjects.
   - World/playfield surface guidance that explains colliders, Tilemaps, bounds, and gravity without auto-building the level.
   - Clear prefab gravity versus runtime movement-profile gravity wording.
   - Animation/Animator object-picker fallback guidance for package/public assets.
   - Valid Unity surface beacons for Project, Hierarchy, Inspector, Component, Prefab, Input, Animation, UI, and Play Mode.
5. Re-run a Computer Use proof from a clean scene through the Authoring Window and ordinary Unity workflow. Do not use generated proof folders or helper preset buttons as the evidence path.
6. Run `.\Tools\Validation\Run-PreSceneValidation.ps1` after the GUI Unity Editor is closed.
7. Add a later route-promotion build-report gate that inspects representative Unity player builds for unexpected editor assemblies, unrelated runtime modules, or large unused assets where practical.

Proof tester handoff:

- Use `docs/superpowers/plans/2026-06-05-pyralis-authoring-proof-tester-handoff.md` as the operating packet for the next agent that runs recurring Authoring 2.0 proof passes.
- The next proof should preferably be a no-pawn tabletop/card/action route because it contrasts with the side-scroller proof and tests whether the authoring system can treat empty pawn fields as valid route intent.
- The proof tester may maintain multiple structural checkpoints at once, but route promotion still requires live evidence, fixed authoring/runtime gaps, relevant tests/docs, export-footprint awareness, and Cameron validation when feel or assets matter.

Completion condition for this checkpoint:

- The project is not back at Phase 0. The spine exists, and the refactor is past the first architecture turn.
- The project is not Phase 1 complete. Route ownership and residue still need a formal matrix.
- The project is not route-promoted. The side-scroller proof needs another clean native pass and Cameron movement/art validation.

## Phase 1 - Scope Audit And Residue Map

Goal: Know exactly what is source-owned, reflected, centrally curated, missing, or intentionally deferred.

Deliverables:

- Produce a route coverage matrix for current Pyralis authoring surfaces.
- Identify all central per-route relationships in convention/fact files that should eventually move closer to source types or feature contracts.
- Tag each residue as keep, migrate, delete, or defer.
- Audit tests that currently prove discoverability and note gaps.

Acceptance:

- Existing 1P Sprite2D movement route has a complete ownership map.
- At least one non-pawn route and one feature-module route are audited for contrast.
- No central curated fact is left without an owner decision.

Validation:

- EditMode tests for fact ids, duplicate ids, native actions, and current 1P discoverability still pass.
- No Unity live pass is required in this phase unless the audit finds a route claim that needs immediate manual confirmation.

## Phase 2 - Source-Owned Metadata Kernel

Goal: Add the smallest semantic layer needed for reflective authoring to express meaning reflection cannot infer.

Principle:

Reflection should discover boring Unity facts. Source-owned metadata should declare product semantics.

Candidate shape:

```text
Unity metadata:
  CreateAssetMenu, AddComponentMenu, RequireComponent, SerializeField

Source-owned authoring semantics:
  role, route families, lane support, setup node, proof target, native action role,
  customization moments, required companions, unsupported lanes, evidence meaning
```

Deliverables:

- Introduce a minimal source-owned authoring metadata abstraction for non-feature-module surfaces.
- Keep feature modules on `IAuthoringContractProvider`.
- Ensure all generated facts carry stable id, provenance, confidence, source type, and related ids.
- Avoid broad scraper behavior that turns every public field into user guidance.

Acceptance:

- A source type can declare its own setup role without editing a central route file.
- Reflection-derived facts and explicit source-owned facts render side by side in the Facts tab with clear provenance.
- Existing visible authoring behavior remains stable for the 1P route.

Validation:

- Unit/EditMode tests prove metadata discovery.
- Tests prove explicit semantics upgrade or relate lower-confidence reflection facts instead of duplicating them.

## Phase 3 - Migrate The 1P Sprite2D Route

Goal: Convert the recently hardened native 1P movement path from centrally curated route relationship text into source-owned/reflected facts.

Initial route surface:

- `GameplaySessionBootstrap`
- `PyralisGameplayLifetimeScope`
- `SessionDefinition`
- `GameModeDefinition`
- `GameSetupProfile`
- `RuntimePatternDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `PawnMovementProfile`
- `PawnPresentationProfile`
- `PawnRoot`
- `Motor2D`
- `Motor2DInputAdapter`
- `Pawn2DMovementComponent`
- `Pawn2DPresentationComponent`

Deliverables:

- Move 1P route semantics toward source-owned metadata and feature/provider facts.
- Reduce central 1P-specific curated relationships to generic aggregation.
- Keep native Project Create, Hierarchy, Add Component, Inspector assign field, and Play Mode actions visible.

Acceptance:

- The 1P route can be reconstructed from source-owned/reflected facts.
- A residue scan finds no avoidable hard-coded 1P route relationship prose in the central convention layer.
- The Authoring Window still points to native Unity actions rather than creating the user's game for them.

Live test:

- Use Computer Use in the Unity Editor.
- Open the Authoring Window.
- Follow the native setup path for a 1P Sprite2D movement proof.
- Record every place where the guide is too specific, too vague, missing a native action, or hiding a setup assumption.
- Enter Play Mode only after the route says the first proof is ready.

## Phase 4 - Authoring-Ready Feature Definition Of Done

Goal: Make it impossible for a new feature to be "done" while invisible to authoring.

A feature is authoring-ready only when it has:

- first-class folder/module ownership under the Gameplay package when the feature is large enough to need it
- registration through existing session/bootstrap/module discovery patterns, with no hidden one-off wiring
- runtime behavior or runtime integration point
- definition/profile/prefab/scene setup path when applicable
- deterministic, safe authoring defaults
- source-owned authoring contract or metadata
- supported and unsupported route/lane declarations
- runtime, network, and authority intent where relevant
- native Unity actions for create/add/assign/customize
- validation issue codes for common setup failures
- first-proof target or explicit reason no proof applies yet
- tests proving discoverability
- docs or roadmap update when the workflow changes
- live proof note once the route is part of an MVP path

Deliverables:

- Add this DoD to active feature/authoring docs.
- Add tests that fail when public setup-facing assets/components are invisible to the authoring registry without an explicit exclusion.
- Define an exclusion path for internal helpers that should not appear in authoring.
- Ensure feature docs distinguish engineer implementation state from authoring-ready state.

Acceptance:

- New gameplay-facing feature work has a clear authoring checklist.
- Existing features can be triaged without pretending every old gap must be fixed in the same slice.
- A feature cannot be called complete only because runtime code compiles or a starter path exists.

Validation:

- Coverage tests for Create Asset menu types, Add Component menu types, and feature contracts.
- Tests must distinguish "not authoring-facing" from "forgotten".
- Live proof evidence is captured in route notes once a feature enters an MVP route.

## Phase 5 - Migrate Route Families By Proof Value

Goal: Expand reflection/source-owned coverage by routes that teach the most about the system.

Recommended order:

1. 1P Sprite2D pawn movement, because it is the current hardened proof route.
2. Tabletop/card/no-pawn route, because it proves empty pawn fields can be correct.
3. Feature-module route such as pickup, interaction, or actor feedback, because it proves optional feature placement.
4. Camera/world route, because it proves ordinary Unity scene objects can be meaningful without Pyralis owning the art.
5. UI/HUD/menu route, because it proves Canvas/EventSystem/presenter contracts.
6. NPC/enemy/custom object route, because it proves non-player actors and object effects.
7. Networking authority route, once local authoring evidence is stable.

For each route:

- map runtime/data/source surfaces
- add or migrate source-owned facts
- reduce central route-specific residue
- add tests for discoverability and stable ids
- run a live Computer Use walkthrough before calling the route authoring-ready

Acceptance:

- Each migrated route can explain required, optional, deferrable, and not-needed work from facts.
- Route guidance stays reactive to selected patterns and evidence.
- Empty or missing fields are classified route-aware instead of universally wrong.
- A route is not promoted to authoring-ready until its recurring live proof gate passes after the migration.

## Phase 6 - Reflective Validation And Evidence States

Goal: Keep intent, evidence, and proof separate everywhere.

Deliverables:

- Validators consume stable facts and issue codes instead of display strings.
- Scene-surface rows use cautious evidence states: Missing, Found candidate surface, Linked to active setup, Validated, Play-proven.
- Proof facts remain targets until a manual or automated play proof records evidence.
- Validate, Map, Overview, Inspector handoffs, and Facts tab agree on source ids.

Acceptance:

- A found collider, camera, Canvas, or presenter cannot be reported as Play-proven by static discovery alone.
- Issue cards name the source, affected field/component, native action, route relevance, and confidence when available.

Validation:

- EditMode tests for issue metadata.
- Scene readiness tests for evidence transitions.
- Live tests compare Authoring Window claims with what was actually observed.

## Phase 7 - Live Unity Proof Program

Goal: Treat live authoring passes as product evidence, not a shortcut.

Recurring promotion rule:

Every major route/domain migration must pass a live Computer Use proof gate before it is called authoring-ready. The gate is recurring, not a one-time 1P Sprite2D check. If the pass fails, fix the authoring contracts, facts, validators, setup UI, runtime assumptions, or docs, then rerun the same pass before continuing to the next route family.

Live pass rules:

- Use the actual Unity Editor with Computer Use.
- Use the Authoring Window, Project window Create menu, Hierarchy, Inspector, Add Component, object picker, scene view, and Play Mode.
- Do not count generated scenes, one-click factories, or hidden auto-wire tools as proof of authoring guidance.
- Fix authoring/code issues discovered during the pass, but keep authorship in the user's hands.

Promotion gate steps:

1. Start from a fresh or intentionally scoped native Unity setup state.
2. Use the Authoring Window as the guide, not hidden generators or one-click factories.
3. Perform Project Create, Hierarchy, Inspector, Add Component, object picker, field assignment, scene view, prefab save, and Play Mode steps as the route requires.
4. Attempt the smallest proof target declared by facts/contracts.
5. Tag every blocker, friction point, over-specific claim, missing native action, or runtime mismatch to stable ids.
6. Classify each finding as blocker, friction, wording, detectability gap, contract gap, or runtime gap.
7. Identify any required Cameron/user validation or asset/design decision separately from code work.
8. Fix code, authoring, validation, documentation, runtime capacity, or folder/refactor issues in the correct owner.
9. Rerun the same pass in the same setup context when possible.
10. Promote the route only when the pass produces a passed proof, a fixed-and-rerun proof, Cameron has validated the relevant movement/feel/customization decisions, or a clearly documented blocker keeps the route out of authoring-ready scope.

Evidence template:

```text
Route:
Goal:
Starting state:
Authoring Window mode(s) used:
Native Unity actions performed:
Customization choices left to user:
Required user/Cameron assets or decisions:
Movement/game-feel validation needed:
Setup blockers found:
Missing or over-specific facts:
Validation cards shown:
First proof attempted:
Observed result:
Fixes made:
Code/refactor gaps created:
Residual risk:
Ready state:
```

Initial recurring gate order:

1. 1P Sprite2D pawn movement plus canonical camera/world.
2. Non-pawn tabletop/card/action route plus camera/cursor and minimal UI/HUD.
3. Sprite2D feature-module seam: `actor.traversal.topdown-hop` plus `actor.pickups.2d`.
4. Camera/world framing and bounds as a standalone cross-route pass if earlier passes reveal unresolved camera assumptions.
5. UI/HUD/menu event route with scoring/session signal wiring.
6. NPC/enemy or custom object effect route.
7. Networking/authority route once local route evidence is stable.

Acceptance:

- Every MVP route has a live proof note.
- Each live pass produces either a passed proof, a fixed issue plus rerun, or a named blocker.
- The authoring contract improves because of the pass.
- No route migration can be marked authoring-ready from static tests, docs, generated scenes, or starter packs alone.
- Player movement, camera feel, interaction feel, and route taste are not considered accepted until Cameron has had the chance to validate them when they are relevant to the route.
- Required assets or design choices are named clearly instead of being silently substituted by Codex-created placeholders.

## Early Pitfalls To Watch

These are the failure modes most likely to make the project look complete before it is truly successful:

- Central route-file gravity: new providers exist, but route meaning keeps accumulating in one central file.
- Read-model overload: `PyralisAuthoringFact` becomes the only source model instead of a normalized output from narrower owners.
- Stringly typed routing: warning copy, row labels, or Inspector prose become behavior keys instead of stable ids.
- Static proof confusion: validators or Facts tab rows imply Play Mode success from discovered objects alone.
- Reflection overreach: broad scanners surface internal/editor-only types as user-facing setup work.
- Reflection silence: provider load failures, duplicate ids, missing ids, or unsupported lanes disappear without diagnostics.
- Live-test bypass: starter packs, generated scenes, one-click factories, or hidden auto-wire tools are counted as authoring evidence.
- Route tunnel vision: 1P Sprite2D becomes the goal instead of the first proving route for studio-wide migration.
- Expensive validation loops: deep scene/prefab scans run on every selection change and make the Authoring Window feel heavy.
- Feature invisibility: a runtime feature compiles but lacks authoring metadata, native setup actions, validation, proof target, or exclusion.
- Data ownership drift: definitions, profiles, prefabs, scene objects, and feature contracts start duplicating each other's responsibilities.
- Docs as source of truth: active docs manually repeat feature requirements instead of pointing to contracts/facts and explaining the mental model.
- Placeholder acceptance: Codex-created temporary assets or generic tuning are mistaken for Cameron-approved art, feel, movement, or route direction.
- Deferred refactor debt: live tests reveal code/folder/capacity problems, but the team treats them as later cleanup instead of route readiness work.
- Developer extension friction: adding a new component requires editing unrelated central files or guessing where metadata, validation, tests, and proof belong.
- Over-enforcement: the Authoring Window treats unrelated native Unity experimentation as an error instead of saying it is outside the active Pyralis setup.
- Under-enforcement: a feature is presented as reusable Pyralis work even though it has no contract, facts, validation, proof target, or extension documentation.
- Export bloat drift: editor-only authoring providers, validators, facts, inspectors, proof tooling, or unrelated route assets leak into player builds through runtime references, `Resources`, Addressables, scenes, prefabs, or always-loaded bootstrap assets.

## Phase 8 - Authoring 2.0 Completion Gate

Authoring 2.0 is complete for the current MVP scope when:

- MVP route surfaces have source-owned metadata, feature contracts, or explicit exclusion.
- Central curated route guidance has been reduced to generic aggregation and reusable product voice.
- Facts tab shows coverage and provenance clearly enough to audit missing surfaces.
- Overview, Guide, Map, Validate, Inspector handoffs, and docs render from the same fact spine where practical.
- Static validators never claim Play Mode proof.
- Each MVP route has tests for discoverability and at least one live proof note.
- Cameron/user validation has accepted movement feel, customization ergonomics, and required asset/design decisions for routes where those are product-shaping.
- Code gaps and refactors discovered by live authoring tests are either fixed in the owning phase or explicitly scoped as required follow-up before route promotion.
- New developers have a clear extension path for adding components/features: runtime owner, data assets, authoring metadata, validation, tests, docs, and proof target.
- Power users can still work directly in native Unity, while reusable Pyralis features are gated by shared-library contracts rather than informal convention.
- Authoring 2.0 editor code is separated from player builds, and representative route build reports are checked for unexpected editor assemblies, unrelated runtime modules, or large unused assets before export-footprint-sensitive routes are promoted.
- `.\Tools\Validation\Run-PreSceneValidation.ps1` passes after code changes.
- Active docs describe the current reflective model, not the transitional history.

## Later Export Footprint Gate

After the first route migrations are stable, add a recurring export-footprint sanity check beside live proof gates. The goal is not to optimize every byte early; it is to prove that route-scoped exports do not accidentally ship the authoring system or unrelated game-lane assets.

For each representative route build, inspect Unity build reports for:

- editor assemblies, authoring windows, validators, inspectors, proof tooling, or docs included in the player
- route-unrelated runtime assemblies pulled in by broad bootstrap or sample references
- large assets included through scenes, prefabs, ScriptableObjects, `Resources`, Addressables, or starter/sample content that the route does not use
- route assets that reference every possible feature instead of only the feature modules, profiles, prefabs, UI, input, art, and scenes needed for that proof

Contracts/facts should remain editor-facing metadata unless a runtime system intentionally consumes a runtime-safe contract. Runtime dependencies should be proven by actual route use, not by the fact that the Authoring Window can describe a capability.

## Near-Term Next Slice

Recommended next slice:

1. Implement Phase 0A: add the convention provider interface/registry and route the existing convention facts through it as a bridge.
2. Add guardrail tests for provider discovery, duplicate ids, and bridge-provider continuity.
3. Migrate the 1P Sprite2D route's convention facts into a domain-owned provider.
4. Run the first Computer Use pass against the current Authoring Window with the 1P Sprite2D route and record limitations using the live evidence template.

This keeps the work grounded: Phase 0A gives the backbone room to grow, the first route migration proves the provider split is real, and the live pass reveals whether the present authoring UI is actually teaching the native Unity workflow.
