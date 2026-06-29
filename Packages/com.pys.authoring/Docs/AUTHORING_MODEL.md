# PYS Authoring Model

PYS Authoring is a generic Unity observer that can grow into an authoring guide when the target project supplies evidence.

Core model:

```text
compiled evidence graph
-> tab-owned projections
-> projection controls
-> exports that mirror the rendered projection packet
```

PYS must not invent target-project meaning. It may render target concepts only when they enter through contracts, reflection, scene/assets, dependency evidence, runtime validation records, or vocabulary providers.

## First Use

A fresh codebase starts in Observer mode.

Useful immediately:

- Settings readiness
- Facts evidence ledger
- Hygiene structural pressure
- Map scene/prefab/asset reality

Unlocked by target evidence:

- Intent composition from goal contracts, toggles, lanes, compatibility, and supporting metadata
- Guide routes from prerequisites, route metadata, setup steps, validation records, expected evidence, and completion signals
- Overview next actions from the rendered Guide path or observed Map/setup issues

Available as generic Unity help:

- built-in Unity setup guides for native Unity workflows that do not require target-project code
- guides for Camera, Cinemachine, Animation, Animator, Timeline, Audio, VFX, Particle System, UI, Input Actions, Prefab, and Lighting setup
- guide rows are fallback or user-enabled assistance, not target-project gameplay meaning

## Contract Metadata

Contracts are hints and authoring surfaces, not proof.

Developer-settable fields for Intent composition:

- `IntentToggles`
- `IntentLanes`
- `CompatibleStableIds`
- `SupportingStableIds`
- `HoverExplanations`

Developer-settable fields for readiness evidence:

- `SuccessDescription`
- `ReadinessHint`
- `ExpectedEvidence`
- `CompletionSignals`
- `ValidationOwnerStableId`

Fallback wording:

- `ProofTarget` is supported as fallback wording, but current integrations should prefer `SuccessDescription`, `ExpectedEvidence`, and `CompletionSignals`.

## Tab Ownership

- Settings owns scan scope, persistence, stale state, and readiness summary.
- Intent owns selected authoring intent composition. Target-project contracts have priority; Unity setup guides are lower-priority fallback/user-enabled options.
- Overview owns the next small action summary.
- Guide owns the full selected route.
- Map owns current scene, prefab, and asset reality.
- Hygiene owns graph-backed audit pressure: contract hygiene, dependency pressure, validation evidence quality, projection integrity, ownership/honesty, runtime flow, docs/claims, and dependency graph seed rows.
- Facts owns source evidence, provenance, and compiled graph data.
