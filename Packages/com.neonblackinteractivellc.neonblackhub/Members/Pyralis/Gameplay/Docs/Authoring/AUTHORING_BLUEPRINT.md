# Pyralis PYS Authoring Blueprint

Pyralis no longer owns a project-specific Authoring Window, graph compiler, projection renderer, hygiene scanner, or export system. Those surfaces belong to the standalone `com.pys.authoring` package.

Pyralis owns only the target-project evidence:

- feature contracts on gameplay scripts
- local validation providers on scripts/assets that own semantic readiness
- Unity-native fields, components, interfaces, and assets that reflection can observe
- tactical inspector handoffs into PYS Authoring

PYS owns the authoring workbenches:

- Settings
- Intent
- Overview
- Guide
- Map
- Hygiene
- Facts
- projection exports

Projection contracts, export rules, hygiene lenses, package limits, and validation rules live in `Packages/com.pys.authoring/Docs`.

Hard rule: do not reintroduce a Pyralis-specific authoring spine under `Editor/Authoring` or runtime authoring metadata under `Core/Contracts/Authoring`. If PYS cannot express a needed graph concept, fix or extend the PYS package in its own lane.
