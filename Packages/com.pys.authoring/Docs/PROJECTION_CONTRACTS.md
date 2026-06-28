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

Forbidden payload:

- graph rows
- setup guidance
- target-project conclusions
- code audit findings

Native actions: yes, only settings/window actions.

Desired target data: no.

Current scene/setup reality: no.

Code audit pressure: no.

Export rule: Settings does not export a separate packet. It supplies the scripts root included in every exported projection.

## Intent

Question: What selectable authoring goal is the user steering toward?

Allowed payload:

- selectable contract rows
- selected contract ID and display name
- disabled reason when selected metadata is incomplete
- category, capability path, surface, and summary from contract evidence
- stable ID, source type, and source file provenance for each contract row
- duplicate stable ID select-block reason when graph evidence reports a collision

Forbidden payload:

- scene repair steps
- proof instructions
- raw graph rows that are not selectable contract evidence
- product-specific meaning not supplied by contracts or vocabulary providers

Native actions: no.

Desired target data: yes, only as contract evidence.

Current scene/setup reality: no.

Code audit pressure: no.

Export rule: export mirrors the Intent projection fields, including the selected contract.

## Overview

Question: What should the user inspect next?

Allowed payload:

- concise scan summary
- issue count
- selected intent name
- proof target
- readiness
- next action from the active Guide projection evidence
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

Question: How do I resolve the selected intent proof path?

Allowed payload:

- selected contract ID and display name
- proof target
- proof readiness
- ordered rows
- row role
- proof-blocking status
- issue rows
- owner IDs
- issue details
- generic Unity action kind
- generic Unity action label
- native actions
- success checks
- selected contract setup steps
- contract metadata completion rows

Forbidden payload:

- local setup invention
- target-project workflow assumptions
- raw graph nodes without a guide role
- Hygiene-only ownership pressure

Native actions: yes, from validation records or generic contract metadata gaps.

Desired target data: only when supplied through contracts or validation records.

Current scene/setup reality: issue-linked only.

Code audit pressure: no, except contract metadata rows needed for projection readiness.

Export rule: export mirrors the Guide projection fields and rows.

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

Forbidden payload:

- desired target setup
- future workflow steps
- code ownership commentary
- product-specific meaning not present in observed evidence

Native actions: no.

Desired target data: no.

Current scene/setup reality: yes.

Code audit pressure: no.

Export rule: export mirrors the Map projection rows.

## Hygiene

Question: Is the observed authoring evidence structurally healthy?

Allowed payload:

- lens packets
- dependency pressure
- contract metadata pressure
- duplicate stable ID pressure with all source types/files
- projection integrity pressure
- source ownership pressure when inferred from graph evidence
- docs/claims pressure when represented as typed evidence

Forbidden payload:

- scene setup walkthroughs
- target-project product guidance
- raw graph dumps as rows without a Hygiene issue role
- prose claims treated as truth

Native actions: no, except generic inspect/review wording encoded in Hygiene rows.

Desired target data: no.

Current scene/setup reality: only as audit evidence.

Code audit pressure: yes.

Export rule: export mirrors the Hygiene projection, including all lens packets and rows.

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
- source path/provenance when observed
- confidence label and source/edge count

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

Export rule: export mirrors the Facts projection counts and rows.
