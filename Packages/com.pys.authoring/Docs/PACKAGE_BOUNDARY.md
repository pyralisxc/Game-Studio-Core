# PYS Authoring Package Boundary

`Pys.Authoring` is a generic Unity codebase observer.

It can scan a target scripts root and infer:

- assemblies
- namespace using relationships
- C# types
- MonoBehaviours
- ScriptableObjects
- serialized fields
- required components
- implemented interfaces
- optional authoring contracts
- optional validation providers
- scene, prefab, and asset evidence

It does not carry target-project product language. Product-specific wording may enter the graph only as observed evidence from:

- reflection
- contract metadata
- dependency edges
- validation records
- scene or asset names
- optional target-project vocabulary packages

## Package Rules

- Generic scanner, graph, projection, export, and window infrastructure may live here.
- Target-project behavior must stay in the target project.
- Runtime behavior must never depend on authoring metadata.
- Runtime contract types must stay neutral and small.
- Settings owns only the target scripts folder, manual scan action, and export folder visibility.
- Contract metadata is accepted only from scripts inside the selected scripts folder.
- Vocabulary dictionaries are display helpers. They must not decide setup validity, runtime behavior, or graph ownership.
- Generic Unity nouns and actions live in `Editor/Vocabulary`.
- Target-project wording must enter through reflection, contracts, dependency edges, validation records, scene or asset names, or vocabulary providers.

## Vocabulary Subsystem

`Editor/Vocabulary` is the only built-in wording subsystem.

It contains:

- stable vocabulary keys
- a default vocabulary facade
- Unity object vocabulary
- Unity graph vocabulary
- Unity action vocabulary
- projection vocabulary
- Hygiene lens vocabulary
- optional target-project vocabulary providers

Package code should use stable keys for built-in Unity concepts. Target projects may add or override labels through `IAuthoringVocabularyProvider`, but providers remain display-only.

## Embedded Package Path

1. Keep the package linked from `Packages/manifest.json`.
2. Let Unity import the embedded package and preserve any `.meta` files Unity creates.
3. Compile `Pys.Authoring.Contracts` without Editor dependencies.
4. Compile `Pys.Authoring.Editor` without target-project assembly references.
5. Run `Pys.Authoring.Editor.Tests`.
6. Scan a target scripts root and export graph, Hygiene, Facts, Intent, Map, Overview, and Guide JSON.
7. Add target-project contract annotations using `Pys.Authoring.Contracts`.
8. Confirm the package treats the target project as observed data, not as an internal dependency.

## Validation

Package validation:

- run `Tools/Validate-PysAuthoringPackage.ps1`
- confirm no package C# file imports target-project namespaces
- confirm the Editor assembly references only the contracts assembly

- import package in Unity
- run Editor tests for `Pys.Authoring.Editor.Tests`
- open `Tools/PYS/Authoring`
- scan a target scripts root
- export and compare each displayed projection with its JSON packet
