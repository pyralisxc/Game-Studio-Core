# Hygiene Lenses

Hygiene is an audit projection. It should make maintenance and ownership pressure visible without becoming a setup guide.

Every Hygiene row has one owning lens. The Overview lens is the aggregate view and contains every row.

Rows carry:

- lens
- issue code
- severity
- owner ID
- source kind/path
- evidence IDs
- claim
- observed evidence
- recommendation
- confidence/source class
- navigation availability

## Overview

Question: What needs attention first?

Owns:

- aggregate row list
- total review, warning, and error counts

Does not own:

- independent findings
- alternate scoring logic

## Contract Hygiene

Question: Are machine-readable contracts complete and honest enough to steer projections?

Owns:

- missing contract metadata
- duplicate stable IDs with all declaring source types and files
- incomplete selectable capability metadata
- missing readiness hints for goal contracts
- contract coverage pressure

Does not own:

- labels supplied only by prose
- runtime behavior decisions

## Dependency Pressure

Question: How are files, assemblies, and systems connected?

Owns:

- assembly dependency edges
- grouped assembly reference rows by source assembly
- namespace fanout pressure
- broad source dependency pressure

Does not own:

- product setup instructions
- contract metadata gaps

## Validation Evidence

Question: Are validation records structured enough to witness readiness?

Owns:

- validation records missing structured fields
- validation owner stable IDs that do not match observed contracts
- goal contracts that cannot name a validation owner when validation evidence exists
- issue rows without enough metadata for Guide/Overview/Facts parity

Does not own:

- target-project validation APIs
- setup-flow invention

## Projection Integrity

Question: Do projections have enough typed evidence to display and export the same view?

Owns:

- projection packet shape warnings
- display/export parity findings
- rows that would make UI and exports disagree

Does not own:

- scene repair instructions
- target-project product wording

## Ownership & Honesty

Question: Do claims stay backed by observed evidence and clear ownership?

Owns:

- duplicate ownership claims
- unclear responsibility evidence
- source ownership leaks represented by graph evidence
- contract expected-evidence hints that are not observed in the compiled graph

Does not own:

- missing scene setup
- raw source lists without an ownership or honesty issue

## Runtime Flow

Question: Are requests, facts, state, and handlers explicit?

Owns:

- event or command flow pressure when represented as typed evidence
- unclear runtime coordination evidence

Does not own:

- invented runtime architecture
- product-specific flow names not present in evidence

## Docs And Claims

Question: Do prose claims stay separate from typed evidence?

Owns:

- documentation claims represented as typed evidence
- summary/commentary pressure when a project supplies it as audit evidence

Does not own:

- treating code comments as truth
- replacing contracts, reflection, dependencies, or validation records

## Dependency Graph

Question: What graph node and edge groups would a visual dependency view render?

Owns:

- grouped node-kind rows
- grouped edge-kind rows
- textual backing data for a future visual dependency graph
- graph drilldown seeds by node kind and edge kind

Does not own:

- a separate proof engine
- desired setup state
- target-specific graph adapters
