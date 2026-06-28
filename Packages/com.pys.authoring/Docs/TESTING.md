# Testing

This package is embedded at `Packages/com.pys.authoring` and linked from the Unity package manifest.

Package validation:

1. Run `Tools/Validate-PysAuthoringPackage.ps1`.
2. Let Unity import and compile.
3. Run Unity Test Runner Editor tests for `Pys.Authoring.Editor.Tests`.
4. Open `Tools/PYS/Authoring`.
5. Select a scripts folder in Settings.
6. Scan.
7. Confirm Settings shows Observer-mode evidence counts and authoring-guide readiness.
8. Select an Intent contract when contracts are present.
9. Confirm Overview reads from the active Guide path.
10. Confirm Guide shows selected intent, proof target, readiness, ordered rows, and blocking status.
11. Toggle tab-local filters and confirm exports match the rendered packet.
12. Export Graph, Hygiene, Facts, Intent, Map, Overview, and Guide JSON.

Current prepared tests cover:

- contract resolver normalization
- contract metadata gap detection
- display-only vocabulary lookup
- vocabulary provider discovery
- generic Unity action vocabulary
- Facts and Overview projections
- Facts typed rows with provenance, confidence, and source counts
- selected Intent projection from explicit goal/proof organization patterns
- route-terminal Intent fallback when no explicit goal exists
- reflective runtime validation method discovery and issue normalization
- observed validation evidence flowing into Guide, Overview, Facts, and Hygiene
- duplicate StableId Intent select-blocking and Hygiene provenance
- no-selected-Intent Guide/Overview blocked state
- selected Guide dependency closure ordering from generic route metadata
- validator issue rows scoped to the selected route dependency closure
- Map asset rows
- reflected fields excluded from Map asset rows
- Guide action kind labels and selected proof path rows
- Hygiene lens projection shape
- grouped assembly reference Hygiene rows
- projection/export parity
