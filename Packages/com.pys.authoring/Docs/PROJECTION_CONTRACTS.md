# Projection Contracts

PYS Authoring follows one rule for every tab:

The tab renders a projection packet, and export saves that same projection packet.

The graph may contain broad evidence. A projection is narrower: it answers one question for one work surface.

## Settings

Question: What source folder should this scan observe?

Allowed payload:

- selected scripts folder
- scan command
- export folder display/open command
- observer evidence counts
- authoring guide readiness status
- missing evidence hints such as goal contracts, route metadata, or runtime validation methods

Forbidden payload:

- graph rows
- setup guidance
- target-project conclusions beyond observed-evidence readiness
- code audit findings

Native actions: yes, only settings/window actions.

Desired target data: only readiness hints derived from observed evidence.

Current scene/setup reality: summary counts only.

Code audit pressure: no.

Export rule: Settings does not export a separate packet. It supplies the scripts root included in every exported projection.

Projection controls: tab-owned controls may filter the rendered packet for that tab. If a control changes visible rows, export writes that same filtered packet.

## Intent

Question: What selectable authoring goal is the user steering toward?

Allowed payload:

- selectable goal option rows inferred from contract organization evidence
- selected contract ID and display name
- selected feature toggles, selected lane, and selected composition summary rendered by the tab
- disabled reason when selected metadata is incomplete
- category, capability path, surface, and summary from contract evidence
- stable ID, source type, and source file provenance for each contract row
- duplicate stable ID select-block reason when graph evidence reports a collision
- organization pattern and dependency count for each projected Intent candidate
- developer-settable intent toggles, lanes, compatibility/supporting stable IDs, hover explanations, success descriptions, readiness hints, expected evidence, completion signals, and validation owner stable ID

Intent candidate inference:

- `Surface = Goal`, `SuccessDescription`, `ExpectedEvidence`, `CompletionSignals`, `ProofTarget`, or `SuccessChecks` make an explicit goal candidate.
- If no explicit goal exists, the terminal contract of a prerequisite chain may become the route goal candidate.
- If no organization pattern exists, Intent falls back to selectable contract rows so sparse target projects remain usable.
- Setup, profile, service, presenter, adapter, runtime component, and vocabulary contracts are supporting route evidence unless they match one of the goal patterns above.
- Built-in Unity setup guides may appear only when no target Intent candidates exist, or when the user enables Unity Setup Guides. They are always lower priority than target-project contracts and must be labeled as `BuiltInUnitySetup`.

Forbidden payload:

- scene repair steps
- readiness instructions
- raw graph rows that are not goal or fallback contract evidence
- full visible contract inventory tables
- product-specific meaning not supplied by contracts or vocabulary providers
- built-in Unity setup guides presented as target-project gameplay contracts

Native actions: no.

Desired target data: yes, only as contract evidence.

Current scene/setup reality: no.

Code audit pressure: no.

Export rule: export mirrors the rendered Intent projection fields, including active candidate visibility controls, the selected contract, selected feature toggles, selected lane, and selected composition summary.

## Overview

Question: What should the user inspect next?

Allowed payload:

- concise scan summary
- issue count
- selected intent name
- readiness target
- readiness
- next action from the active Guide projection evidence
- up to three next-action rows from ordered blocking Guide rows
- reason code from graph issue metadata

Forbidden payload:

- full graph dumps
- full dependency lists
- Hygiene lens details
- target-project policy not present in contracts or validation records

Native actions: yes, when supplied by issue metadata or generic scan state.

Desired target data: only when supplied through contracts or validation records.

Current scene/setup reality: summary only.

Code audit pressure: summary only.

Export rule: export mirrors the Overview projection fields.

## Guide

Question: How do I resolve the selected intent readiness path?

Allowed payload:

- selected contract ID and display name
- readiness target
- readiness state
- success descriptions, readiness hints, expected evidence, and completion signals from selected route contracts
- built-in Unity setup readiness evidence rows derived from observed native scene components, component field assignments, and assets
- selected contract dependency closure, ordered by generic route metadata
- ordered rows
- row role
- blocking status
- route stable ID, route stage, route order, and setup domain per row
- issue rows
- owner IDs
- issue details
- generic Unity action kind
- generic Unity action label
- native actions
- success checks
- selected contract setup steps
- prerequisite contract setup steps
- contract metadata completion rows

Forbidden payload:

- local setup invention
- target-project workflow assumptions
- raw graph nodes outside the selected contract dependency closure
- Hygiene-only ownership pressure

Native actions: yes, from validation records or generic contract metadata gaps.

Desired target data: only when supplied through contracts or validation records.

Current scene/setup reality: issue-linked only.

Code audit pressure: no, except contract metadata rows needed for projection readiness.

Export rule: export mirrors the rendered Guide projection fields and rows, including active row filters.

## Map

Question: What exists in the current Unity scene and assets?

Allowed payload:

- scene object rows
- prefab rows
- asset rows
- object IDs
- labels
- source paths
- component counts
- issue counts
- packet-backed navigation fields: `CanSelect`, `CanPing`, `NavigationKind`, and `NavigationLabel`
- native inspection commands when the row packet says they are available: select loaded scene object or ping project asset

Forbidden payload:

- desired target setup
- future workflow steps
- code ownership commentary
- product-specific meaning not present in observed evidence

Native actions: yes, only current-reality inspection actions backed by the rendered row packet. Map actions may select a hierarchy object or ping a project asset. They must not create, repair, or configure desired setup.

Desired target data: no.

Current scene/setup reality: yes.

Code audit pressure: no.

Export rule: export mirrors the rendered Map projection rows, including active row filters and navigation/action fields.

## Hygiene

Question: Is the observed authoring evidence structurally healthy?

Allowed payload:

- lens packets
- dependency pressure
- contract metadata pressure
- duplicate stable ID pressure with all source types/files
- goal readiness-hint pressure
- validation record structure and owner pressure
- expected-evidence honesty pressure
- projection integrity pressure
- source ownership pressure when inferred from graph evidence
- docs/claims pressure when represented as typed evidence
- textual dependency graph edge groups
- textual dependency graph node groups

Forbidden payload:

- scene setup walkthroughs
- target-project product guidance
- raw graph dumps as rows without a Hygiene issue role
- prose claims treated as truth

Native actions: no, except generic inspect/review wording encoded in Hygiene rows.

Desired target data: no.

Current scene/setup reality: only as audit evidence.

Code audit pressure: yes.

Export rule: export mirrors the rendered Hygiene projection, including the active lens packet and rows.

## Facts

Question: What evidence does the scanner know about?

Allowed payload:

- assembly count
- namespace count
- type count
- script count
- contract count
- validator count
- scene object count
- prefab count
- asset count
- issue count
- fact rows for observed assemblies, namespaces, types, scripts, fields, contracts, validators, scene objects, prefabs, assets, and issues
- built-in Unity setup fact details including readiness state when native scene/asset evidence was observed
- Unity-native vocabulary labels for setup domains, component fields, bindings, and readiness states when those labels explain observed evidence
- source path/provenance when observed
- confidence label and source/edge count
- active kind and search filters through the rendered row packet

Forbidden payload:

- next actions
- guide instructions
- setup repair
- ownership judgment
- target-project product meaning not represented as observed evidence

Native actions: no.

Desired target data: no.

Current scene/setup reality: counts and observed fact rows only.

Code audit pressure: no.

Export rule: export mirrors the rendered Facts projection counts and rows, including active kind/search filters.
