# Pyralis PYS Authoring Model

The supported model is:

```text
Pyralis scripts and assets
  -> PYS contracts / validation providers / reflection evidence
    -> PYS graph
      -> PYS projections and exports
```

Pyralis scripts should declare authoring metadata only where the script owns real gameplay meaning. Keep metadata concise and structural:

- `StableId` only for stable setup/proof identities
- `Category` and `CapabilityPath` for selectable capability shape
- `Surface` for projection ownership
- `RequiredFields`, `RequiredComponents`, and `RequiredInterfaces` for setup evidence
- `SetupSteps` and `SuccessChecks` for contract-owned setup/proof checks
- `OwnershipClaims` and `RoleTags` for stable non-UI meaning

Avoid stuffing UI prose, old route policy, or projection behavior into contracts. If reflection can infer a field, component, or interface, let reflection own it. If a script owns a semantic readiness rule, expose it through a PYS validation provider.

The PYS projection contract is the source of truth for tab behavior. Pyralis must not export a different packet than the UI renders.
