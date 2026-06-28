# Hygiene Lenses

Hygiene is an audit projection. It should make maintenance and ownership pressure visible without becoming a setup guide.

Every Hygiene row has one owning lens. The Overview lens is the aggregate view and contains every row.

## Overview

Question: What needs attention first?

Owns:

- aggregate row list
- total review, warning, and error counts

Does not own:

- independent findings
- alternate scoring logic

## Ownership

Question: Do owners and responsibilities stay clear?

Owns:

- duplicate ownership claims
- unclear responsibility evidence
- source ownership leaks represented by graph evidence

Does not own:

- missing scene setup
- raw source lists without an ownership issue

## Dependencies

Question: How are files, assemblies, and systems connected?

Owns:

- assembly dependency edges
- grouped assembly reference rows by source assembly
- namespace fanout pressure
- broad source dependency pressure

Does not own:

- product setup instructions
- contract metadata gaps

## Contracts

Question: Are machine-readable contracts complete enough to steer projections?

Owns:

- missing contract metadata
- duplicate stable IDs with all declaring source types and files
- incomplete selectable capability metadata
- contract coverage pressure

Does not own:

- labels supplied only by prose
- runtime behavior decisions

## Runtime Flow

Question: Are requests, facts, state, and handlers explicit?

Owns:

- event or command flow pressure when represented as typed evidence
- unclear runtime coordination evidence

Does not own:

- invented runtime architecture
- product-specific flow names not present in evidence

## Projection Integrity

Question: Do projections have enough typed evidence to display and export the same view?

Owns:

- validation records missing structured fields
- issue rows without enough metadata for UI/export parity
- projection packet shape warnings

Does not own:

- scene repair instructions
- target-project product wording

## Docs And Claims

Question: Do prose claims stay separate from typed evidence?

Owns:

- documentation claims represented as typed evidence
- summary/commentary pressure when a project supplies it as audit evidence

Does not own:

- treating code comments as truth
- replacing contracts, reflection, dependencies, or validation records
