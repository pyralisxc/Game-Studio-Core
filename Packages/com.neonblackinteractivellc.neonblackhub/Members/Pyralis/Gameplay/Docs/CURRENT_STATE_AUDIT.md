# Pyralis Current State Audit

This is the short current-truth audit for Game Studio Core. It should stay concise enough that a new agent can read it before touching gameplay or authoring code.

## Current Architecture State

Pyralis is built around one runtime path and one reflective authoring spine:

```text
Gameplay code and contracts
  -> reflection + dependency tree + validators
    -> resolved authoring graph
      -> Overview / Intent / Guide / Map / Hygiene / Facts
```

Runtime starts from `GameplaySessionBootstrap` and composes through `PyralisGameplayLifetimeScope`. Session data is authored through `SessionDefinition`, `GameModeDefinition`, `ParticipantDefinition`, and `PawnDefinition`. The lifetime scope owns the visible service graph entrypoint; feature activation evidence lives in `PyralisRuntimeFeatureServicePolicy`; and service registration mechanics live in focused composition installers so the entrypoint stays readable.

Loaded-scene feature evidence uses one composition helper, not repeated scene-walk loops. Policy code decides whether a feature service family should be active; installer code registers authored scene services when present. Neither path should create hidden scene objects to repair setup.

The current Unity-native ownership rule is:

- Siblings describe what a pawn **is**.
- Features describe what a pawn **can do**.
- Participants describe who is **driving**.
- Pawns expose camera sockets through `PawnCameraTarget`; `CameraRigProfile.focusMode` chooses pawn, group, playfield, explicit scene target, or manual Cinemachine focus.
- Scenes own scene-scale direction and services.
- Authoring projects graph truth instead of creating presets or duplicating setup systems.

`ParticipantDefinition.inputProfile` is the authored input owner. A pawn receives input through its pawn input/movement modules; the input asset is not split across competing setup locations. For local multiplayer, Unity `PlayerInput` owns the runtime action instance paired to each controller, and `InputProfile.actions` is only the template/naming source. Camera follow is participant-owned and scene-routed: participant pawn assignment exposes the body, pawn prefabs may expose `PawnCameraTarget`, and the scene-owned Cinemachine rig routes the selected focus into Cinemachine.

`ParticipantSpawnService` is the single owner for participant pawn placement. `GameplaySessionBootstrap` owns session startup and visible service handoff; it does not store spawn points. Pawn-backed routes assign `ParticipantSpawnService.spawnPoints`; no-pawn routes can leave them empty and disable `Spawn On Register`.

Pawn-backed auto-spawn does not use hidden transform-offset fallback placement. If `Spawn On Register` is enabled, `ParticipantSpawnService` requires an authored spawn point and logs a setup error instead of inventing one. Generic scene/content spawners are optional gameplay utilities, not participant setup surfaces, and their contracts should not appear as route essentials. `PlayerSpawner` is a respawn/lives/countdown coordinator only; it delegates pawn instantiation and placement back to `ParticipantSpawnService` instead of owning separate spawn points.

Participant topology is graph evidence, not networking vocabulary. `SessionDefinition.networkMode` describes transport/authority. The graph separately infers whether the active route is `SoloLocal`, `LocalJoin`, `Networked`, or `HybridLocalNetworked` from participant count, pawn gameplay, `PlayerInputManager`, `ParticipantInputRouter.autoRegisterDefaultParticipantsWithoutPlayerInput`, `ParticipantDefinition.autoJoin`, and `ParticipantSpawnService.spawnOnRegister`. Local join routes should wait for Unity `PlayerInputManager` joins; auto-registering every default participant before controllers join is a setup contradiction that Map and Guide should expose.

## Current Authoring State

The Authoring Window has six tabs:

- **Overview**: best next action and one to three immediate moves.
- **Intent**: route steering through DNA axioms, presentation lane, and reflected capability descriptor toggles.
- **Guide**: full current-setup checklist from the shared route working projection.
- **Map**: scene/setup reality, field readiness, and Unity repair actions.
- **Hygiene**: programmer audit for graph integrity, source pressure, dependency pressure, and code ownership hotspots.
- **Facts**: compiled dictionary/vocabulary evidence.

Map owns concrete Unity scene and Inspector setup issues. Hygiene owns code/graph health. Overview, Guide, and Route Proof Trace should read the same Intent-projected setup route projection so they cannot drift. Intent remains the steering surface for what the developer wants to wire next. Map remains intentionally separate: it reports the current scene/setup graph and does not pretend Intent-selected desired setup already exists. Facts should remain read-only evidence and vocabulary.

The Authoring Window surface is consolidated around UI Toolkit for the non-Intent tabs. Overview, Guide, Map, Hygiene, and Facts build tab-specific projection packets from the resolved graph, render through the shared UI Toolkit tab renderer, and export the same projection lens. New setup intelligence should move into contracts, dependency reflection, validators, graph projection, or grammar before it reaches tab UI.

Inspectors are local integrity surfaces, not setup guides. They may show field-local contract issues, local validation warnings, an "Open Pyralis Authoring" handoff, or small asset-local utilities such as syncing an `InputProfile` from its assigned Unity Input Action Asset. They should not carry route setup cards, genre/preset buttons, or alternate first-proof guidance. Camera profile framing is now edited directly through `CameraRigProfile` fields and interpreted by Authoring/Map/Guide rather than shaped through inspector starting-point shortcuts.

Intent descriptors are strict contract and route-descriptor surfaces, not namespace guesses. Gameplay Ingredient toggles require a feature-owned contract descriptor with `SelectableIntent = true`, a semantic `CapabilityPath`, and runtime-family ownership from either explicit contract metadata or reflection-inferred runtime-surface evidence; grammar vocabulary may supply labels and generic wording, but it is not selectable and does not drive runtime-family routing. Intent also shows a read-only **Needs Contract Metadata** backlog for selectable feature contracts that match the current lane/axioms but are missing contract-owned `CapabilityPath` or runtime-family meaning that reflection cannot infer. That backlog exposes the real capacity that cannot be shaped yet without letting incomplete contracts steer the route. Route Essentials are read-only rows derived from exact `IntentRouteEssential` role tags plus the selected ingredients, DNA, lane, and participant route shape. They must not become stored Intent toggles or capability flags, and they should not be inferred from display names, broad `Setup`, `Session`, `VFX`, or `Networking` capability overlap. If a selectable contract lacks the metadata Intent needs, the graph emits `ContractMetadata.*` evidence, Hygiene surfaces it as missing contract metadata with ownership buckets, and Intent lists it as waiting instead of inventing a path or role. Runtime ownership collision checks are also contract-owned: duplicate `OwnershipClaims` values across different contracts emit `ContractMetadata.DuplicateOwnershipClaim` Hygiene evidence, while source hygiene remains an audit signal rather than formal duplicate-owner proof.

Intent also has a participant route steering control for early planning: infer from setup, solo local, two to four local players, networked, or hybrid local/networked. This control previews the desired participant topology in Guide and Overview before assets exist. It does not create participants, spawn points, input prefabs, or scene objects; once authored setup exists, Map continues to report the concrete scene/setup reality. The route analysis preserves authored participant count, desired Intent participant count, and effective route count separately so a 2P Intent preview cannot hide that `SessionDefinition.defaultParticipants` currently contains four authored seats.

Field-level setup gaps now flow through reflected graph assignment evidence. When reflection can identify an empty required field such as `ParticipantDefinition.inputProfile`, `PawnDefinition.pawnPrefab`, `PawnDefinition.movementProfile`, `PawnDefinition.presentationProfile`, or `GameModeDefinition.cameraRigProfile`, the graph emits an `AssignmentField` node with the owner, field, expected type, native Inspector action, severity, and proof role. Core setup graph nodes should not duplicate those same field assignments.

Graph nodes now carry typed projection metadata such as `SetupDomain` and stable `IssueCode` in addition to node kind, source kind, origin, work intent, severity, capability family, and contract fields. Overview, Guide, Route Proof Trace, Map, Hygiene, and JSON exports should rank, group, filter, and de-duplicate setup cards from that metadata instead of parsing display labels or guidance prose. Wording is rendering only.

Runtime validation is a local evidence witness, not a second setup brain. Feature scripts, profiles, and definitions should validate semantic rules they own through `PyralisRuntimeValidationIssue` records with stable issue codes, field paths when the issue maps to an Inspector field, native actions, severity, and success checks. Compact local `GetValidationIssues()` lists are asset-local helper surfaces only; graph-facing evidence must still enter through structured `PyralisRuntimeValidationIssue` records so the graph gets owner type, deterministic issue code, field/action metadata when known, and no anonymous prose. Reflected/dependency evidence should own field presence, required components, interfaces, and assignment paths before a validator describes the same gap. `InputProfile` now owns Input Action Asset, primary action map, gameplay action row, and supported device validity; scene readiness and core setup graph nodes do not duplicate those checks.

Feature-owned validation includes nested semantic asset chains where the owner is clear. Combat profiles surface their assigned weapons and sequences, weapons surface projectile definitions, and projectile definitions validate their own projectile prefab contract without forcing scene readiness to understand combat internals. Use this as the feature pattern: move semantic checks to the owning feature asset or script, keep dependency presence reflected, and let graph evidence carry the result.

Scene readiness is concrete scene hygiene only. It checks open-scene Unity surfaces such as missing scripts, root/service object presence, PlayerInputManager, EventSystem, AudioListener, NetworkManager/transport, and scene-visible setup contradictions. It does not relay every scene validation provider or inspect individual feature services, pawn prefab internals, feature modules, input profiles, movement profiles, presentation profiles, or similar asset/prefab contract issues. Those originate from the script/profile/definition that owns the rule and enter the graph through dependency-tree runtime validation evidence or reflected assignment evidence.

Core route setup is compiled directly by the graph from route analysis, dependency reflection, contracts, and local validators: gameplay root, session, mode, capabilities, participant topology, seats, join policy, and proof blockers. Feature-specific readiness for pawn prefab wiring, input profiles, spawn points, camera/playfield setup, scoring, HUD/UI, projectiles, tabletop, settings, enemies, RPG, and similar domains should come from feature-owned contracts, reflected assignments, dependency-tree evidence, concrete scene evidence, and local runtime validation providers. Local validators must emit structured `PyralisRuntimeValidationIssue` records with issue codes and fields; prose is for rendering, not for graph classification. A feature should not gain a central route row just because Guide or Overview needs better wording; fix the graph source or projection instead.

Participant join/spawn policy is also first-class graph evidence. Map shows a Join Policy row compiled from session/input/spawn runtime fields, and JSON exports include topology, expected join policy, spawn policy, authored participant count, desired Intent participant count, effective route count, auto-join count, auto-register state, PlayerInputManager presence, spawn-on-register state, and local-join conflict status.

Proof vocabulary is topology-aware. A pawn-backed solo route uses the 1P pawn movement proof; a pawn-backed local join or hybrid route uses the local co-op pawn join proof so Guide, Overview, and Route Proof Trace verify controller isolation, participant seating, spawn placement, and camera focus instead of calling the route a 1P proof. Generic proof templates remain valid wording fallbacks, but they are marked as grammar-owned contract metadata until a feature-owned contract declares the proof target.

Authoring vocabulary is editor-owned. Runtime-visible contract flags and attributes live in `Core/AuthoringContracts` so gameplay code can declare capabilities and axioms, while gameplay behavior seams live in `Core/RuntimeContracts`. Labels, groups, tooltips, hygiene advice, and generic wording live under `Editor/Authoring/Grammar`. Do not add UI wording or setup-card copy back into runtime-visible contract files.

Route capability guidance now lives inside `AUTHORING_MODEL.md`; there is no separate active cookbook doc. If route vocabulary needs durable wording, update the authoring model, contracts, or grammar vocabulary instead of adding another guide file.

Route-facing guidance now translates structural/interface evidence into the concrete lane action. A missing pawn motor may remain `IPawnMotor` evidence for Hygiene, but the `Sprite2D` route card should say `Add Motor2D`; a missing pawn input module should say `Add Motor2DInputAdapter`. This translation belongs in graph projection so Overview, Guide, and Route Proof Trace cannot drift.

Route shape and pawn setup are intentionally separate. `route.shape` answers whether the route has the right control owner shape, such as `SessionDefinition.defaultParticipants` and `ParticipantDefinition.defaultPawn`. It may show concise native setup for missing owner links, but it should not carry `PawnDefinition.pawnPrefab`, `ParticipantDefinition.inputProfile`, lane motor, presentation, animation, or feature-module repair instructions. Those are reflected field, prefab, component, or validator evidence and should appear as pawn setup or assignment cards instead.

Optional actor feature modules are explicit prefab composition. Pawn modules are authored through `PawnDefinition.featureModules`; enemy modules are authored through `EnemyFeatureProfile.featureModules`. When enabled modules exist, the actor prefab root should contain `ActorFeatureHost`. Runtime warns and skips installation if the host is missing instead of silently adding hidden composition.

The selected Intent lane now survives into the setup graph. This matters for partially authored routes: if a pawn prefab is not wired enough for reflection to infer 2D or 3D from concrete components, route-facing projections fall back to the Intent lane instead of showing generic lane/interface copy.

Proof cards are deliberately narrow. A proof node describes the Play Mode check and success signal for the selected first proof; it should not inherit every setup action, assignment field, or customization point from supporting contracts. Contract setup meaning still matters, but it should surface as graph evidence and route cards before the proof node rather than bloating the proof itself.

Authoring tabs export tab-specific JSON snapshots for diagnostics. Intent exports steering only: DNA axioms, presentation lane, participant route, descriptor groups, selected ingredients, metadata backlog rows, advisor rows, `gameplayIngredientGroups`, `metadataBacklogGroups`, and `routeEssentialGroups`. Map exports current setup reality only. Hygiene exports graph/code audit pressure only, with passive graph context when a setup graph exists. Hygiene rows and exports include source kind, source origin, source object, issue code, triage bucket, triage advice, ownership bucket, repair owner, ownership advice, and graph source detail; it also has focused buckets for missing contract metadata and runtime validation metadata so anonymous or weak provider output cannot quietly return. Contract source pressure separates declared runtime families from inferred runtime families so cleanup can remove policy overreach without stuffing redundant metadata into contracts. Contract metadata advice should be domain-aware: camera, combat, core setup, networking, presentation, tabletop, UI, and movement contracts should not all receive the same example path. Guide exports the Route Proof Trace that follows the same current route working projection Guide renders: current action, ordered fresh-scene steps, blockers, direct proof support, source owners, contracts, and diagnostic questions. Facts exports the compiled dictionary/cookbook: fact counts, source/confidence counts, reflected contract coverage, and dictionary fact rows. Facts deliberately excludes route-intent facts, runtime-capability routing, customization moments, proof facts, proof-support coverage, setup-action payloads, and repair instructions so it cannot become a second route guide. Each export includes a compact summary where useful so humans and agents can quickly compare route name, graph size, action counts, pressure counts, and coverage before reading the full detail. Route Proof Trace may work from an Intent-projected graph before a setup object exists, because it previews the from-scratch proof path; Map still stays bound to current scene/setup reality. Route Proof Trace deliberately separates `currentAction` from the first `orderedSteps` row so a ready foundation card does not masquerade as the next move. It excludes scene-surface repair lists, but may include concrete pawn/prefab readiness blockers as route cards when they describe the actual Unity component to add. It does not promote broad later capabilities or contract-inventory nodes as direct support for a small first proof. Hygiene separates actionable Cleanup Focus from Watch List pressure so large expected scripts remain visible without reading as failures. Cleanup Focus now includes source-ownership residue such as reflection meaning leaks, validator guide leaks, inspector route-guide leaks, export truth leaks, tab renderer logic leaks, and compatibility bridges; legacy-doc and old-owner-name findings remain Watch List audit pressure. Protective anti-fallback docs and inspector text that only hands users to Pyralis Authoring are not ownership leaks by themselves. These exports are read-only evidence for humans and agents; incorrect output should be fixed in contracts, dependency reflection, validators, graph projection, or grammar rather than patched in the export.

Export lens ownership is enforced in code. Intent exports semantic steering and runtime-family metadata without setup-action payloads. Map filters its node/edge payload to Map-owned setup reality instead of raw graph inventory. Route Proof Trace normalizes pawn-backed local join and hybrid routes to the local co-op pawn join proof, excludes scene-surface repair/enhancer rows, limits direct proof support to exact active-proof contract evidence, omits empty-route diagnostics when an Intent-projected route has ordered cards, and keeps reflected labels readable without doubled spacing. Hygiene separates setup/integrity counts from contract metadata and contract inventory pressure. Facts exports dictionary/provenance/reflected contract coverage without route-intent, runtime-capability, customization, proof, Guide, or Map repair payloads.

## Platform Health

The codebase is a usable shared gameplay foundation. The authoring graph carries real setup guidance, and the runtime setup path is understandable from contracts, dependency reflection, and local validators.

The latest Hygiene export for the Local Co-op Pawn route is clean on source/contract ownership: `HygieneRows = 0`, `ContractMetadataIssues = 0`, `CleanupFocus = 0`, `Unknown = 0`, and `ProofBlockers = 0`. Remaining dependency pressure is watchlist architecture signal, not an actionable Hygiene failure: expected composition, editor audit, scanner, grammar, and feature-module pressure should stay visible without being treated as cleanup work unless a future export promotes it into Cleanup Focus.

The main engineering risks are:

- large coordinator files that may need owner-based partials or extracted policies when they stop being readable
- presentation/animation edges accumulating too much mixed responsibility; camera is being simplified toward target routing plus Cinemachine composition
- route vocabulary that can drift if contracts, reflection, and grammar do not stay aligned
- Hygiene reporting paper health while runtime feel still needs manual Unity proof passes
- docs re-growing into stale parallel truth
- runtime-created GameObjects being misclassified: spawned gameplay objects, camera focus helpers, popups, fade overlays, and pooled effects are transient runtime output; creating scene services, camera rigs, managers, profiles, or setup objects to repair missing authoring is hidden setup and should stay out of runtime code
- descriptors or exports reintroducing behavior-affecting display-text inference instead of using typed graph metadata, contract role tags, dependency evidence, and validator records

RPG is an optional platform feature rather than mandatory engine spine: RPG service registration is feature-owned, RPG UI/panel code is feature-owned, and RPG-specific editor tooling lives with the RPG feature instead of the universal Authoring Window.

## Maintenance Focus

1. Keep one owner for each gameplay responsibility.
2. Keep runtime simple and Unity-native before adding abstraction.
3. Keep contracts semantic; reflection owns structure that code can prove.
4. Keep Authoring as graph projection, not a second setup system.
5. Keep active docs small, current, and direct.
6. Keep optional feature domains broad but locally owned; do not let a feature's service list or editor tool turn into universal core.
7. Keep generated diagnostics such as `Editor/Authoring/TempGraphs` exports out of source control; use them as evidence, not durable docs.

## Verification Posture

Use Unity Test Runner or `.\Tools\Validation\Run-PreSceneValidation.ps1` as the durable gate when runtime/editor code changes. For docs-only consolidation, use link/reference checks and `git diff --check`.

Manual Authoring Window walkthroughs are still required for user-flow confidence because static tests can prove graph shape but not whether the setup feels clear.

The default test gate should stay focused on seam protection rather than fake gameplay confidence:

- **Manual play proofs** answer whether the game actually feels and works in Unity.
- **Automated tests** protect data transfer, ownership, routing, contracts, graph projections, exports, and refactor seams.
- **Authoring exports** show what the system currently understands.

Old broad feature-domain matrices, documentation contract audits, classifier detail sweeps, and source-text tests have been removed from the active package. Add new automated tests only when they protect a current seam, data transfer, route projection, or runtime ownership rule. Do not add tests that only lock source prose, stale docs, or simulated gameplay claims that still require a Unity proof.
