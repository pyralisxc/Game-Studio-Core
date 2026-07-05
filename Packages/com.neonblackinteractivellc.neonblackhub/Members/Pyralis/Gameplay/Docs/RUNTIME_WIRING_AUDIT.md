# Runtime Wiring Audit

This is the current hard-cut audit lane for turning scattered runtime setup and delivery paths into one boring, readable wiring model.

The goal is not to resurrect a broad `Glue` layer. The goal is to let the codebase metamorphose into a smaller runtime wiring surface:

- Core defines shared words, contracts, and small value types.
- Data defines authored recipes and defaults.
- Modules own gameplay behavior.
- Presentation displays results.
- Wiring delivers already-owned things to already-declared receivers.
- Scene evidence inventories what exists, but does not decide what should exist.

## Executive Summary

Runtime composition has improved: modules no longer depend on Glue, Core is mostly neutral, and feature behavior is more locally owned. The remaining pressure is that several systems still express the same setup truth in different shapes.

Today, setup and delivery meaning can appear as:

- PYS authoring contracts
- local runtime validation providers
- serialized fields
- VContainer injection
- runtime services contexts
- profile receivers
- scene searches
- feature service policies
- inspector warnings
- route-specific setup prose

Those are not all wrong, but they should not all be independent truth sources.

The next architecture step is one canonical runtime address language. Existing systems should project into that language instead of inventing parallel meanings.

## Canonical Address Model

Every runtime setup relationship should be understandable as:

```text
Contract + Provider + Receiver + Package + Owner + Scope + Timing + Requiredness
```

| Term | Meaning |
| --- | --- |
| Contract | The stable thing being delivered or required. |
| Provider | The object, service, asset, profile, or module that supplies it. |
| Receiver | The object, service, asset, profile, or module that needs it. |
| Package | The payload shape or data being delivered. |
| Owner | The system that is allowed to decide this relationship exists. |
| Scope | Scene, session, participant, pawn, feature, presentation, network, or editor. |
| Timing | Authoring, startup, spawn, join, play, respawn, teardown, or editor-only. |
| Requiredness | Required, optional, fallback-free, auto-derived, or display-only. |
| Priority | Blocking setup, proof enhancer, warning, info, or cleanup pressure. |
| Explicit Source/Target | Whether the user authored the relationship directly. |
| Validation Severity | Error, warning, info, or hidden because it is not relevant to the active route. |

This vocabulary is intentionally plain. It should be easy for Unity developers to understand without knowing project history.

## Address Shapes Found Today

| Current Shape | Keep / Merge / Demote | Target Ownership |
| --- | --- | --- |
| `AuthoringContractAttribute` | Keep | Setup evidence for PYS and local contracts. |
| `IRuntimeValidationProvider` and `RuntimeValidationIssue` | Keep, then align terms | Local semantic readiness owned by the script or asset. |
| Serialized fields | Keep | Explicit user-authored source/target references. |
| VContainer `[Inject]` | Keep, restricted | Delivery/application only, owned by Wiring/Composition. |
| `GameplayRuntimeServicesContext` and pawn runtime contexts | Keep short term | Runtime delivery package; split later if it becomes too broad. |
| Profile receiver interfaces | Keep | Data-to-runtime application seams. |
| `RuntimeSceneSearch` | Demote | Inventory helper only, not authority or hidden repair. |
| Feature service policy flags | Merge | Wiring report should explain activation evidence. |
| Inspector warnings | Demote | Local projections of validation, not independent setup truth. |
| Route-specific setup prose | Move outward | PYS or owner validation should render it from typed evidence. |

## Hard-Cut Principles

1. Core is shared vocabulary only.
2. Wiring owns delivery truth, not gameplay truth.
3. Scene evidence is inventory, not authority.
4. One meaning gets one canonical issue.
5. First hard cut is conceptual: report before moving code.

The clean cut is not to rename `Glue` overnight. The clean cut is to make duplicate setup meanings visible, prove a single language, and then delete or move the old address shapes.

## Current High-Pressure Areas

### GameplayLifetimeScope

`GameplayLifetimeScope` is currently the main runtime delivery root. It composes services, injects loaded scene objects, applies runtime service contexts, resolves scene services, selects networking variants, and registers feature services.

That makes it useful, but also too easy to grow. It should become thinner after wiring truth is visible.

Target direction:

- delivery entrypoint remains here or in a renamed wiring root
- decisions move to owners
- fallback scene search becomes reportable evidence
- injection is treated as delivery, not meaning

### RuntimeSceneSearch

`RuntimeSceneSearch` is useful as an inventory scanner, but it should not decide whether a route is valid or silently supply setup meaning.

Target direction:

- scene search produces inventory rows
- wiring report classifies rows
- owners decide requiredness
- PYS/inspectors render the result

### RuntimeFeatureServicePolicy And FeatureServiceInstaller

Feature activation currently combines authored data, scene evidence, policy booleans, and service registration.

Target direction:

- feature owners declare activation evidence
- wiring report shows why a service family is active
- installers register what the report says is active
- route-specific UI setup moves out of generic service registration

### PlayerSpawner

`PlayerSpawner` is mixed ownership: lives, death, respawn timing, component toggling, pawn placement, game over, and countdown UI all appear in one scene-facing class.

Target direction:

- participant spawn remains with `ParticipantSpawnService`
- respawn/lives policy belongs to scene flow or a respawn module
- pawn lifecycle commands are explicit
- countdown UI belongs to presentation
- no behavior changes until the wiring report can explain the current setup

### ParticipantSpawnService

`ParticipantSpawnService` is mostly legitimate delivery: participant, pawn definition, spawn point, and runtime services meet here.

Target direction:

- keep as explicit spawn delivery owner
- expose provider/receiver/package rows
- avoid hidden transform fallback placement

### ParticipantInputRouter

`ParticipantInputRouter` owns local join and participant input routing. Its auto-register behavior is legitimate, but it needs clearer timing and requiredness evidence.

Target direction:

- route timing is explicit: startup auto-register versus PlayerInputManager join
- contradictions become one canonical validation row
- no first-player assumptions

### ArcadeGameFlowController

`ArcadeGameFlowController` is route-specific scene flow, not generic wiring.

Target direction:

- keep route/game-mode behavior local
- expose what it needs through canonical address rows
- do not let game-flow UI or state rules leak into generic composition

## Duplicate Meaning Targets

These are the meanings most likely to have several addresses today.

| Meaning | Current Symptoms | Canonical Target |
| --- | --- | --- |
| Missing required scene service | Inspector errors, PYS rows, scene search fallback, bootstrap warnings | One `MissingProvider` wiring row. |
| Feature service activation | Policy booleans, scene components, profile presence, registration code | One `ServiceActivation` row with evidence. |
| Runtime service handoff | VContainer injection, serialized field, runtime context, receiver interface | One `Delivery` row. |
| Camera routing | Camera profile, scene target, participant pawn, Cinemachine rig, focus helper | One `CameraFocusDelivery` row. |
| Participant join route | ParticipantDefinition, PlayerInputManager, auto-register, spawn service | One `ParticipantJoinRoute` row. |
| Respawn route | PlayerSpawner, spawn service, lives, health, scene flow, UI countdown | One `RespawnFlow` row. |
| Settings delivery | Settings menu, screen, config, runtime services | One `SettingsDelivery` row. |

## Wiring Report

The initial implementation is read-only and lives under `Glue/Wiring/Reporting`, with an editor menu at `Tools/NeonBlack/Gameplay/Wiring/Copy Selected Root Report`.

Current names:

- `GameplayWiringReport` for the data.
- `GameplayWiringReportBuilder` for collection/classification.
- `GameplayWiringHub` only if a scene-facing component becomes necessary later.

Runtime class names should stay boring and literal.

The report should contain:

- Data Intake
- Providers
- Receivers
- Deliveries
- Missing Providers
- Ambiguous Providers
- Stale Or Invalid Providers
- Timing Issues
- Cut Candidates

The report should not create objects, wire fields, choose routes, or repair scenes. It observes and classifies.

## Target Folder Shape, After Proof

Do not move folders before the report proves the shape. The likely destination is:

```text
Gameplay/
  Core/
  Data/
  Modules/
  Presentation/
  Wiring/
    Root/
    Inventory/
    Reporting/
    Validation/
    Delivery/
    RuntimeServices/
  Session/
  Participants/
  Spawning/
  SceneFlow/
  Networking/
```

If this creates too many top-level folders, keep the current `Glue` folder temporarily and build the report inside `Glue/Wiring`. The hard cut is ownership and duplicate meaning removal, not cosmetic folder churn.

## Migration Plan

### Phase A - Address Inventory And Consolidation Audit

Status: current doc phase.

Output:

- identify every address shape
- classify duplicate meanings
- document canonical address vocabulary
- avoid runtime behavior changes

### Phase B - Read-Only Wiring Report

Status: initial foundation added under `Glue/Wiring/Reporting`.

Build `GameplayWiringReport` without changing startup behavior.

The initial report observes:

- gameplay root and authored session intake
- core service providers and missing providers
- deferred session-scoped requirements when no `SessionDefinition` exists
- deferred participant-route requirements when `SessionDefinition` is missing its default game mode or participants
- participant join timing pressure
- feature service activation evidence
- `GameplayRuntimeServicesContext` and `PawnRuntimeServicesContext` receiver deliveries
- local `IRuntimeValidationProvider` issues projected into canonical wiring rows
- a read-only editor menu item at `Tools/NeonBlack/Gameplay/Wiring/Copy Selected Root Report`

Acceptance:

- report can list current providers, receivers, deliveries, missing providers, and timing contradictions
- report uses canonical terms
- no hidden setup repair
- no runtime behavior changes

### Phase C - Report Parity Against Current Runtime

Status: parity protection started.

Current protected behavior:

- missing `GameplaySessionBootstrap.sessionDefinition` setup appears once as the canonical `MissingProvider` row for `SessionDefinition`
- duplicate local `GameplaySessionBootstrap.SessionDefinition.Missing` validation rows are suppressed when the canonical missing-provider row exists
- route-dependent camera guidance is deferred while session route evidence is absent
- participant roster, spawn, input, and feature activation rows are deferred until the session asset has a default game mode and participant route

Compare report rows with existing inspector warnings, validators, and PYS exports.

Acceptance:

- same setup problems appear once with consistent wording
- canonical missing-provider rows suppress duplicate local validation rows for the same setup fact
- route-dependent optional guidance is deferred when an earlier authored data blocker prevents route inference
- participant, spawn, input, camera, and feature service rows wait until the session asset can describe a default game mode and participant route
- no lost required setup blockers
- route-specific facts remain owned by routes/modules

### Phase D - Consolidate Address Language

Make validation providers, runtime warnings, and local inspectors consume or mirror report terms.

Acceptance:

- duplicate messages are collapsed
- severity and requiredness are consistent
- local inspectors remain local integrity surfaces

### Phase E - Replace Implicit Scene Search

Demote direct scene search calls to inventory evidence used by the report.

Acceptance:

- startup does not hide missing setup behind search fallbacks
- scene evidence is inspectable
- decisions still belong to owner contracts/data

### Phase F - Move Decisions To Owners

Move remaining route, feature, respawn, camera, and settings decisions out of generic wiring.

Acceptance:

- Wiring delivers
- modules and scene-flow owners decide
- presentation only displays
- data only defines authored defaults

### Phase G - Folder Rename Or Split

Only after the model is proven, decide whether `Glue` becomes `Wiring`, splits into `Session/Participants/Spawning/SceneFlow/Wiring`, or stays as a compatibility folder with fewer responsibilities.

Acceptance:

- folder names reveal ownership
- no old bridge/adapters remain
- no behavior-changing moves are made blindly

## Do Not Touch Yet

These surfaces can be imperfect while the report is still forming:

- VContainer registration order
- networking subclass selection
- participant spawn semantics
- local join and auto-register behavior
- camera profile application
- PlayerInputManager setup
- PYS projection behavior
- arcade flow behavior
- runtime-created transient popups, effects, and focus helpers

Changing these before the wiring report reaches parity against current runtime evidence risks local patches that preserve the old confusion.

## Final Recommendation

Proceed with metamorphosis, not resurrection.

The next slice should prove `GameplayWiringReport` parity against current inspectors, validators, runtime warnings, and PYS exports. Once it can describe the current runtime honestly, use it to delete duplicate address paths and move decisions back to their owners.
