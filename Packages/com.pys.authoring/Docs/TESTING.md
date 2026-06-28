# Testing

This package is embedded at `Packages/com.pys.authoring` and linked from the Unity package manifest.

Package validation:

1. Run `Tools/Validate-PysAuthoringPackage.ps1`.
2. Let Unity import and compile.
3. Run Unity Test Runner Editor tests for `Pys.Authoring.Editor.Tests`.
4. Open `Tools/PYS/Authoring`.
5. Select a scripts folder in Settings.
6. Scan.
7. Select an Intent contract when contracts are present.
8. Confirm Overview reads from the active Guide path.
9. Confirm Guide shows selected intent, proof target, readiness, ordered rows, and blocking status.
10. Export Graph, Hygiene, Facts, Intent, Map, Overview, and Guide JSON.

Current prepared tests cover:

- contract resolver normalization
- contract metadata gap detection
- display-only vocabulary lookup
- vocabulary provider discovery
- generic Unity action vocabulary
- Facts and Overview projections
- Facts typed rows with provenance, confidence, and source counts
- selected Intent projection
- duplicate StableId Intent select-blocking and Hygiene provenance
- Map asset rows
- reflected fields excluded from Map asset rows
- Guide action kind labels and selected proof path rows
- Hygiene lens projection shape
- grouped assembly reference Hygiene rows
- projection/export parity
