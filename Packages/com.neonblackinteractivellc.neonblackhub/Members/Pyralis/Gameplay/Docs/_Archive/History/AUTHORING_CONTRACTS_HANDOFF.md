# Pyralis Authoring Contracts Handoff

## Goal

Move Pyralis Authoring from mostly curated cards toward contract-backed guidance.

Do not make the Authoring Window a freeform generator or route authority. The target is a clean-break contract model:

- curated route contracts for major route families
- feature-owned authoring contracts for modules, profiles, validators, and proof targets
- reflection/convention facts as lower-confidence evidence
- live Unity route proofs as the final proof layer

The product outcome is clearer beginner/adaptable authoring. The developer outcome is better modular code: a feature is not complete until it declares how it is authored, validated, customized, and proven.

## Why This Matters

The live Sprite2D/top-down pawn pass exposed a shared seam: input actions should be semantic requests, not hardcoded behavior. `Jump` can mean side-view Rigidbody2D jump, top-down visual hop, 3D jump, animation-only action, or a custom ability depending on the selected route.

The resulting `IActorGameplayActionReceiver` and `TopDownHopFeatureRuntime` are the first concrete examples of the direction:

- input action role
- feature module runtime
- required profile type
- supported presentation lanes
- runtime prefab contract
- inspector handoff
- validator issue
- first proof target

That should become a reusable contract pattern instead of repeated hand-coded guidance. No legacy compatibility layer remains for feature authoring contracts.

## Core Rule

Separate these states everywhere:

- `Intent`: the route or feature says it should support something.
- `Evidence`: assets, fields, components, interfaces, and scene objects appear to be present.
- `Proof`: EditMode, PlayMode, or Computer Use route proof demonstrated the behavior.

Authoring metadata can say a route is ready to attempt. It must not claim Play Mode proof unless proof actually ran.

Feature contracts are required for feature module authoring in this model. A feature contract is declared in-package via `IAuthoringContractProvider` and found only through `ResolvedAuthoringContractRegistry` discovery.

## Minimum Viable Authoring Contract

Start small. Do not build a giant ontology.

Suggested shape:

```text
AuthoringContract
- stableId
- owner type or asset kind
- route/capability ids
- supported presentation lanes
- unsupported lanes and wording
- required profile type
- required definition/profile assets
- required prefab components or interfaces
- required scene components/evidence
- assignment/customization fields
- semantic input/action roles
- native Unity action hint
- validator issue codes
- first proof target id
- confidence/provenance
```

Suggested provider:

```text
IAuthoringContractProvider
- GetAuthoringContracts()
```

Keep this editor/data-facing. It should describe existing architecture, not become a second runtime composition system.

## First Implementation Slice

Pilot on feature modules, not the entire package.

1. Add the smallest `AuthoringContract` model and provider interface in an editor/data-safe location.
2. Convert `actor.traversal.topdown-hop` to emit a contract:
   - stable id: `feature.actor.traversal.topdown-hop`
   - profile type: `TopDownHopProfile`
   - runtime interface: `IActorGameplayActionReceiver`
   - module id: `actor.traversal.topdown-hop`
   - supported lanes: `Sprite2D`, `Billboard2_5D`
   - unsupported lane: `Rigged3D` should use 3D traversal jump
   - consumed action: `Jump` by default, profile-customizable
   - native setup: create profile, create `FeatureModuleDefinition`, assign runtime prefab, add module to `PawnDefinition`
   - proof target: Jump action produces visible top-down hop while map-plane body remains grounded
3. Convert one existing non-hop feature module as a comparison case, such as interaction or pickups.
4. Make `FeatureModuleDefinition.GetValidationIssues()` consume contract metadata where practical instead of growing more module-id switch cases.
5. Feed the Facts tab from contracts without changing the visible UX dramatically.
6. Add tests for:
   - duplicate contract ids
   - required profile type checks
   - missing runtime prefab/interface checks
   - unsupported lane messaging
   - facts/validator parity for contract-backed modules

## What Should Stay Curated

Keep these human-authored or explicitly contract-authored:

- route taxonomy
- first proof definitions
- `Do Now`, `Proof Enhancer`, and `Feature Card` priority
- issue severity and success criteria
- unsupported-lane wording
- network/authority claims
- what can wait
- customization moments
- starter pack/sample intent
- polished product language

Use generated/reflection facts only for low-level evidence:

- menu paths
- serialized fields
- tooltips
- required components
- implemented interfaces
- asset/component availability
- possible relationships

## Canonical Live Proof Strategy

Do not manually explore every possible combination.

Maintain a small set of canonical live proofs:

- Game Shell proof
- `Sprite2D` pawn-backed action proof
- `Billboard2_5D` pawn-backed action proof
- `Rigged3D` pawn-backed action proof
- non-pawn tabletop proof
- network ownership proof

Add focused seam proofs when a new shared contract appears. The semantic input action dispatch into `TopDownHopFeatureRuntime` is a seam proof inside the `Sprite2D` lane, not a whole new route family.

## Current Validation Notes

Recent completed validation for the semantic action/top-down hop slice:

- `dotnet build "Game Studio Core.slnx" --no-restore` passed with 0 errors.
- Unity Editor refresh/import passed with fresh Tundra build success and assembly reload.
- Full pre-scene gate was not run because the GUI Unity Editor was open.

Preferred full gate when the Editor is closed:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

## Guardrails For The Next Agent

- Use native Unity authoring workflows for proof: Project Create menu, Hierarchy, Inspector Add Component, object picker, Scene view, Play Mode.
- Do not create proof scenes, scene generators, helper scripts, YAML edits, or hidden auto-wire paths as evidence that authoring works.
- Do not edit scenes/assets while in Play Mode.
- Do not edit generated `.csproj` files. New package editor assets/scripts must include `.meta` files and a Unity refresh before relying on CLI build gates.
- Keep `GameplaySessionBootstrap`, `PyralisGameplayLifetimeScope`, definitions, profiles, feature modules, and runtime patterns as the source of truth.
- Prefer shared contracts and validators over adding another bespoke guide card.
- Do not let metadata claim proof. Proof requires tests or manual Play Mode evidence.

## Recommended Next Move

Build the minimal Authoring Contract layer and pilot it on `TopDownHop` plus one existing feature module. Then run compile/Unity refresh and follow with a fresh native Unity authoring pass through the Sprite2D pawn route.
