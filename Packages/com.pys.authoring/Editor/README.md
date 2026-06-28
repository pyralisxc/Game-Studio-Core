# Editor

Editor contains the generic Unity observer.

It may inspect:

- target script folders
- asmdefs
- C# using directives
- MonoBehaviours
- ScriptableObjects
- serialized fields
- required components
- implemented interfaces
- optional `Pys.Authoring.Contracts` metadata
- optional `IAuthoringValidationProvider` records

It must not reference target project gameplay assemblies directly.
