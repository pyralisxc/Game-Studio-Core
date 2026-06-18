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

The current Unity-native ownership rule is:

- Siblings describe what a pawn **is**.
- Features describe what a pawn **can do**.
- Participants describe who is **driving**.
- Scenes own scene-scale direction and services.
- Authoring projects graph truth instead of creating presets or duplicating setup systems.

`ParticipantDefinition.inputProfile` is the authored input owner. A pawn receives input through its pawn input/movement modules; the input asset is not split across competing setup locations.

## Current Authoring State

The Authoring Window has six tabs:

- **Overview**: best next action and one to three immediate moves.
- **Intent**: route steering through DNA axioms, presentation lane, and capability toggles.
- **Guide**: full current-setup checklist from the shared route working projection.
- **Map**: scene/setup reality, field readiness, and Unity repair actions.
- **Hygiene**: programmer audit for graph integrity, source pressure, dependency pressure, and code ownership hotspots.
- **Facts**: compiled dictionary/cookbook.

Map owns concrete Unity scene and Inspector setup issues. Hygiene owns code/graph health. Overview, Guide, and Route Proof Trace should read the same current setup route projection so they cannot drift. Intent remains the steering surface for what the developer wants to wire next. Facts should remain read-only evidence and vocabulary.

Map and Hygiene export tab-specific JSON snapshots for diagnostics. Map exports current setup reality only. Guide and Hygiene can also export a Route Proof Trace that follows the same current route working projection Guide renders: current action, ordered fresh-scene steps, blockers, direct proof support, source owners, contracts, and diagnostic questions. Route Proof Trace deliberately separates `currentAction` from the first `orderedSteps` row so a ready foundation card does not masquerade as the next move. It also excludes scene-surface and scene-readiness repair work, and it does not promote broad later capabilities as direct support for a small first proof. Hygiene separates actionable Cleanup Focus from Watch List pressure so large expected scripts remain visible without reading as failures. These exports are read-only evidence for humans and agents; incorrect output should be fixed in contracts, dependency reflection, validators, graph projection, or grammar rather than patched in the export.

## Current Health

The codebase is in a solid consolidation phase. The authoring graph is carrying real setup guidance, and the runtime setup path is increasingly understandable from contracts, dependency reflection, and validators.

The biggest remaining risks are not the basic architecture. They are:

- large coordinator files that may need owner-based partials or extracted policies when they stop being readable
- presentation/animation and camera edges accumulating too much mixed responsibility
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
