# Runtime

Runtime contains only safe contract types that gameplay assemblies may reference.

It must stay small:

- attributes
- validation records
- generic authoring action kinds
- graph DTOs
- neutral enums

It must not contain:

- editor code
- project-specific product vocabulary
- product workflow logic
- service registration or runtime behavior
