# Pyralis Current State Audit

This is the short current-truth audit for Game Studio Core. It should stay concise enough that a new agent can read it before touching gameplay or authoring code.

## Current Architecture State

Pyralis is now built around one runtime path and one reflective authoring spine:

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

`ParticipantDefinition.inputProfile` is the authored input owner. A pawn receives input through its pawn input/movement modules; the input asset is not split across competing setup locations. Camera follow is no longer treated as camera-bounds readiness: participant pawn assignment exposes the body, pawn prefabs may expose `PawnCameraTarget`, and the scene-owned Cinemachine rig routes the selected focus into Cinemachine.

## Current Authoring State

The Authoring Window has six tabs:

- **Overview**: best next action and one to three immediate moves.
- **Intent**: route steering through DNA axioms, presentation lane, and reflected capability descriptor toggles.
- **Guide**: full current-setup checklist from the shared route working projection.
- **Map**: scene/setup reality, field readiness, and Unity repair actions.
- **Hygiene**: programmer audit for graph integrity, source pressure, dependency pressure, and code ownership hotspots.
- **Facts**: compiled dictionary/vocabulary evidence.

Map owns concrete Unity scene and Inspector setup issues. Hygiene owns code/graph health. Overview, Guide, and Route Proof Trace should read the same Intent-projected setup route projection so they cannot drift. Intent remains the steering surface for what the developer wants to wire next. Map remains intentionally separate: it reports the current scene/setup graph and does not pretend Intent-selected desired setup already exists. Facts should remain read-only evidence and vocabulary.

Intent descriptors now prefer contract-authored `CapabilityPath`, `RoleTags`, and `SelectableIntent` over hand-authored category mappings. The descriptor registry also infers broad semantic paths and role tags from source type, namespace, capability flags, lane, category, module id, profile type, and setup node id so the whole codebase can become readable without hand-editing every contract. Use explicit contract fields only when reflection cannot infer the intended semantic grouping. Do not use them to duplicate structure the dependency tree already proves.

Field-level setup gaps now flow through reflected graph assignment evidence. When reflection can identify an empty required field such as `ParticipantDefinition.inputProfile`, `PawnDefinition.pawnPrefab`, `PawnDefinition.movementProfile`, `PawnDefinition.presentationProfile`, or `GameModeDefinition.cameraRigProfile`, the graph emits an `AssignmentField` node with the owner, field, expected type, native Inspector action, severity, and proof role. Setup-flow cards should not duplicate those same field assignments.

Authoring vocabulary is editor-owned. Runtime-visible contract flags and attributes live in `Core/Authoring` so gameplay code can declare capabilities and axioms, but labels, groups, tooltips, hygiene advice, and generic wording live under `Editor/Authoring/Grammar`. Do not add UI wording or setup-card copy back into runtime-visible contract files.

Route capability guidance now lives inside `AUTHORING_MODEL.md`; there is no separate active cookbook doc. If route vocabulary needs durable wording, update the authoring model, contracts, or grammar vocabulary instead of adding another guide file.

Route-facing guidance now translates structural/interface evidence into the concrete lane action. A missing pawn motor may remain `IPawnMotor` evidence for Hygiene, but the `Sprite2D` route card should say `Add Motor2D`; a missing pawn input module should say `Add Motor2DInputAdapter`. This translation belongs in graph projection so Overview, Guide, and Route Proof Trace cannot drift.

Route shape and pawn setup are intentionally separate. `route.shape` answers whether the route has the right control owner shape, such as `SessionDefinition.defaultParticipants` and `ParticipantDefinition.defaultPawn`. It should not carry `PawnDefinition.pawnPrefab`, `ParticipantDefinition.inputProfile`, lane motor, presentation, animation, or feature-module repair instructions. Those are reflected field, prefab, component, or validator evidence and should appear as pawn setup or assignment cards instead.

Optional actor feature modules are explicit prefab composition. Pawn modules are authored through `PawnDefinition.featureModules`; enemy modules are authored through `EnemyFeatureProfile.featureModules`. When enabled modules exist, the actor prefab root should contain `ActorFeatureHost`. Runtime warns and skips installation if the host is missing instead of silently adding hidden composition.

The selected Intent lane now survives into the setup graph. This matters for partially authored routes: if a pawn prefab is not wired enough for reflection to infer 2D or 3D from concrete components, route-facing projections fall back to the Intent lane instead of showing generic lane/interface copy.

Proof cards are deliberately narrow. A proof node describes the Play Mode check and success signal for the selected first proof; it should not inherit every setup action, assignment field, or customization point from supporting contracts. Contract setup meaning still matters, but it should surface as graph evidence and route cards before the proof node rather than bloating the proof itself.

Map and Hygiene export tab-specific JSON snapshots for diagnostics. Map exports current setup reality only. Hygiene exports graph/code audit pressure only, with passive graph context when a setup graph exists. Guide exports the Route Proof Trace that follows the same current route working projection Guide renders: current action, ordered fresh-scene steps, blockers, direct proof support, source owners, contracts, and diagnostic questions. Each export includes a compact `summary` block so humans and agents can quickly compare route name, graph size, action counts, and pressure counts before reading the full detail. Route Proof Trace may work from an Intent-projected graph before a setup object exists, because it previews the from-scratch proof path; Map still stays bound to current scene/setup reality. Route Proof Trace deliberately separates `currentAction` from the first `orderedSteps` row so a ready foundation card does not masquerade as the next move. It excludes scene-surface repair lists, but may include concrete pawn/prefab readiness blockers as route cards when they describe the actual Unity component to add. It does not promote broad later capabilities or contract-inventory nodes as direct support for a small first proof. Hygiene separates actionable Cleanup Focus from Watch List pressure so large expected scripts remain visible without reading as failures. These exports are read-only evidence for humans and agents; incorrect output should be fixed in contracts, dependency reflection, validators, graph projection, or grammar rather than patched in the export.

## Current Health

The codebase is in a solid consolidation phase. The authoring graph is carrying real setup guidance, and the runtime setup path is increasingly understandable from contracts, dependency reflection, and validators.

The biggest remaining risks are not the basic architecture. They are:

- large coordinator files that may need owner-based partials or extracted policies when they stop being readable
- presentation/animation edges accumulating too much mixed responsibility; camera is being simplified toward target routing plus Cinemachine composition
- route vocabulary that can drift if contracts, reflection, and grammar do not stay aligned
- Hygiene reporting paper health while runtime feel still needs manual Unity proof passes
- docs re-growing into stale parallel truth

RPG is now treated as an optional platform feature rather than mandatory engine spine: RPG service registration is feature-owned, RPG UI/panel code is feature-owned, and RPG-specific editor tooling lives with the RPG feature instead of the universal Authoring Window.

## Current Cleanup Focus

1. Keep reducing duplicate ownership paths.
2. Keep runtime simple and Unity-native before adding abstraction.
3. Keep contracts semantic; remove anything reflection can infer.
4. Keep Authoring as graph projection, not a second setup system.
5. Keep docs small and current.
6. Keep optional feature domains broad but locally owned; do not let a feature's service list or editor tool turn into universal core.

## Verification Posture

Use Unity Test Runner or `.\Tools\Validation\Run-PreSceneValidation.ps1` as the durable gate when runtime/editor code changes. For docs-only consolidation, use link/reference checks and `git diff --check`.

Manual Authoring Window walkthroughs are still required for user-flow confidence because static tests can prove graph shape but not whether the setup feels clear.

The default test gate should stay focused on seam protection rather than fake gameplay confidence:

- **Manual play proofs** answer whether the game actually feels and works in Unity.
- **Automated tests** protect data transfer, ownership, routing, contracts, graph projections, exports, and refactor seams.
- **Authoring exports** show what the system currently understands.

Old broad feature-domain matrices, documentation contract audits, classifier detail sweeps, and source-text tests have been removed from the active package. Add new automated tests only when they protect a current seam, data transfer, route projection, or runtime ownership rule. Do not add tests that only lock source prose, stale docs, or simulated gameplay claims that still require a Unity proof.
