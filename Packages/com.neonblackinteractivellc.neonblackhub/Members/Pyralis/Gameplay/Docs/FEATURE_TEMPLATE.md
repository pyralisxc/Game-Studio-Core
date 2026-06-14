# Pyralis Feature Template

Use this checklist when adding a new gameplay domain.

## Required Layout

- `Features/<FeatureName>/Runtime/Shared`
- `Features/<FeatureName>/Runtime/2D`
- `Features/<FeatureName>/Runtime/3D`
- `Features/<FeatureName>/Data`
- `Features/<FeatureName>/Editor`
- `Features/<FeatureName>/Tests`
- `Features/<FeatureName>/Docs`

## Required Contracts

- Define authored assets in the feature's `Data` area unless they are truly cross-feature.
- Implement runtime behavior through `IFeatureModuleRuntime`.
- Declare `FeatureNetworkRole`, `authoringCategory`, and `gizmoMode` on each `FeatureModuleDefinition`.
- Use `PyralisGameplayLifetimeScope`, explicit initialization context, participant/session services, or feature-owned runtime context instead of new global lookups.

## Required Validation

- Validate required profiles and runtime prefab contracts.
- Validate presentation compatibility and network-role assumptions.
- Add at least one authoring test and one runtime test.
