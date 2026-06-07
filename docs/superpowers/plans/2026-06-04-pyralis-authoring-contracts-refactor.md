# Pyralis Authoring Contracts Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move Pyralis authoring from curated cards and module-id switch cases toward feature-owned, contract-backed guidance that separates intent, evidence, and proof.

**Architecture:** Keep runtime composition authoritative in definitions, profiles, prefabs, feature modules, `GameplaySessionBootstrap`, and `PyralisGameplayLifetimeScope`. Add an editor/data-facing authoring contract layer that describes how features are authored, validated, customized, and proven; it must feed existing facts, validators, inspectors, and the Authoring Window without becoming a second runtime composition system. Feature-owned contract providers are discovered reflectively through `PyralisAuthoringContractRegistry`, with no manual registry tables or legacy compatibility path.

**Tech Stack:** Unity 6000 project, C#, Unity Editor tooling, ScriptableObject definitions/profiles, existing `NeonBlack.Gameplay.Editor` authoring fact pipeline, NUnit EditMode tests, project gate `.\Tools\Validation\Run-PreSceneValidation.ps1`.

---

## Phase Map

1. **Foundation Split:** Extract the typed authoring fact primitives out of `PyralisRuntimeCapabilityCatalog.cs` so contracts can grow without making the catalog file heavier.
2. **Contract Core:** Add the minimal `PyralisAuthoringContract` model, provider interface, registry, and fact adapter. Use reflection-based provider discovery as the durable design.
3. **TopDownHop Pilot:** Add a contract provider for `actor.traversal.topdown-hop` and move its profile/interface/lane checks out of hardcoded `FeatureModuleDefinition` switch logic.
4. **Comparison Feature:** Add one non-hop provider, starting with `actor.interaction`, to prove the model is not special-cased around traversal.
5. **Validation Integration:** Make `FeatureModuleDefinition.GetValidationIssues()` consume contract metadata for profile type, runtime prefab interfaces, supported/unsupported lanes, issue wording, and proof target ids.
6. **Authoring Window Integration:** Feed the Facts tab and feature-module inspector from contract facts while keeping visible UX stable.
7. **Proof And Docs:** Add contract tests, update active docs, run compile/Unity validation, then perform a native Unity authoring pass for the Sprite2D pawn route.
8. **Expansion Policy:** Add a durable rule that new feature modules are not authoring-ready until they declare an authoring contract.

## File Structure

Current local build note: Unity-generated `.csproj` files in this workspace can lag behind newly created package editor files. Do not edit generated `.csproj` files to force compile coverage. New package editor files must include `.meta` files, and Unity refresh/regeneration should run before relying on CLI compile gates.

Create:

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactTypes.cs`
  - Owns `PyralisAuthoringFactKind`, `PyralisAuthoringFactSourceKind`, `PyralisAuthoringConfidence`, `PyralisAuthoringIssueSeverity`, `PyralisAuthoringFact`, `PyralisAuthoringIssue`, and any existing shared fact value types currently embedded in `PyralisRuntimeCapabilityCatalog.cs`.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactRegistry.cs`
  - Owns registry aggregation, duplicate-id checks, lookup, filtering, and provider collection.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContract.cs`
  - Owns the smallest contract data model and helper methods.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/IAuthoringContractProvider.cs`
  - Defines `IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()`.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContractRegistry.cs`
  - Discovers feature-owned contract providers, detects duplicate stable ids, and finds contracts by module id.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContractFacts.cs`
  - Converts contracts into `PyralisAuthoringFact` records.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisFeatureModuleContractValidator.cs`
  - Applies contract metadata to `FeatureModuleDefinition` validation without the data assembly depending on editor-only code.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Traversal/Editor/TopDownHopAuthoringContractProvider.cs`
  - Feature-owned contract provider for `actor.traversal.topdown-hop`.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Interaction/Editor/InteractionAuthoringContractProvider.cs`
  - Feature-owned contract provider for `actor.interaction`.
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`
  - Contract registry, duplicate id, fact parity, feature validation, and supported-lane tests.

Modify:

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisRuntimeCapabilityCatalog.cs`
  - Remove embedded shared fact primitives after extraction; keep catalog card content and catalog-specific helpers.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/FeatureModuleDefinition.cs`
  - Keep runtime/data validation that is safe outside editor. Remove feature-specific profile/interface/lane switch cases only after editor contract validation covers them.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/FeatureModuleDefinitionEditor.cs`
  - Draw contract-backed facts and contract-backed validation issues in the guided inspector.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringWindow.cs`
  - Ensure Facts tab includes `FeatureContract`/contract-backed facts and labels them as intent/evidence/proof-targets, not proof results.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_CONTRACTS_HANDOFF.md`
  - Mark this plan as the execution path.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_BLUEPRINT.md`
  - Add authoring contracts as the next fact-pipeline layer.
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`
  - Update Authoring 2.0 rollout status after the pilot lands.

Do not modify:

- Runtime feature initialization contracts unless the implementation discovers an actual runtime bug.
- Scene or prefab YAML as proof. Manual Unity proof must use native authoring flows.
- Starter-pack generators as validation evidence.

---

### Task 1: Extract Shared Authoring Fact Types

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactTypes.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactRegistry.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisRuntimeCapabilityCatalog.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringSourceContractTests.cs`

- [ ] **Step 1: Write the extraction guard test**

Add assertions to `PyralisEditor_Source_ExposesGuideOnlyRuntimeCapabilityCatalog` that the catalog no longer owns the registry implementation after extraction:

```csharp
string factTypesPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisAuthoringFactTypes.cs");
string factRegistryPath = Path.Combine(editorRoot, "Authoring", "Facts", "PyralisAuthoringFactRegistry.cs");

Assert.That(File.Exists(factTypesPath), Is.True);
Assert.That(File.Exists(factRegistryPath), Is.True);

string factTypesSource = File.ReadAllText(factTypesPath);
string factRegistrySource = File.ReadAllText(factRegistryPath);

Assert.That(factTypesSource.Contains("public sealed class PyralisAuthoringFact"), Is.True);
Assert.That(factTypesSource.Contains("public sealed class PyralisAuthoringIssue"), Is.True);
Assert.That(factRegistrySource.Contains("public static class PyralisAuthoringFactRegistry"), Is.True);
Assert.That(catalogSource.Contains("public static class PyralisAuthoringFactRegistry"), Is.False);
```

- [ ] **Step 2: Run the focused test and verify it fails**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringSourceContractTests.PyralisEditor_Source_ExposesGuideOnlyRuntimeCapabilityCatalog"
```

Expected: fails because the new files do not exist or the registry is still embedded in `PyralisRuntimeCapabilityCatalog.cs`.

- [ ] **Step 3: Move only shared fact types**

Move these existing declarations from `PyralisRuntimeCapabilityCatalog.cs` into `PyralisAuthoringFactTypes.cs` without changing public names or constructor signatures:

```csharp
namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringFactKind { RouteFamily, RuntimeCapability, SetupNode, Definition, Profile, SceneComponent, PrefabComponent, AssignmentField, CustomizationMoment, Issue, Proof }
    public enum PyralisAuthoringFactSourceKind { HandAuthoredGuideCard, SetupFlow, Validator, InspectorGuide, Reflection, Convention, SceneEvidence }
    public enum PyralisAuthoringConfidence { Unknown, Inferred, ConventionDerived, Explicit }
    public enum PyralisAuthoringIssueSeverity { Info, Optional, Recommended, Required, Blocked }
    public sealed class PyralisAuthoringFact { /* exact existing implementation */ }
    public sealed class PyralisAuthoringIssue { /* exact existing implementation */ }
}
```

Keep `RuntimeCapabilityGoalTag`, `RuntimeCapabilityLaneTag`, and `RuntimeCapabilityCard` in `PyralisRuntimeCapabilityCatalog.cs` unless later tasks prove they are broadly shared.

- [ ] **Step 4: Move only registry aggregation**

Move the existing `PyralisAuthoringFactRegistry` class from `PyralisRuntimeCapabilityCatalog.cs` into `PyralisAuthoringFactRegistry.cs` without changing its public methods:

```csharp
namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringFactRegistry
    {
        // Preserve existing All, Find, GetFacts, and HasDuplicateStableIds behavior.
    }
}
```

Do not add contract behavior in this task.

- [ ] **Step 5: Compile-check the editor assembly**

Run:

```powershell
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: build passes with the same known external-package warnings only.

- [ ] **Step 6: Commit**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisRuntimeCapabilityCatalog.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactTypes.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactRegistry.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringSourceContractTests.cs"
git commit -m "refactor: split pyralis authoring fact spine"
```

---

### Task 2: Add Minimal Authoring Contract Model

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContract.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/IAuthoringContractProvider.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContractRegistry.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`

- [ ] **Step 1: Write failing registry tests**

Create `AuthoringContractsContractTests.cs`:

```csharp
using NeonBlack.Gameplay.Editor;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public class AuthoringContractsContractTests : PyralisEditorTestSupport
    {
        [Test]
        public void AuthoringContracts_DoNotContainDuplicateStableIds()
        {
            Assert.That(PyralisAuthoringContractRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);
        }

        [Test]
        public void AuthoringContracts_CanFindContractByModuleId()
        {
            PyralisAuthoringContract contract = PyralisAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.StableId, Is.EqualTo("feature.actor.traversal.topdown-hop"));
            Assert.That(contract.ModuleId, Is.EqualTo("actor.traversal.topdown-hop"));
        }
    }
}
```

- [ ] **Step 2: Run the tests and verify they fail**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringContractsContractTests"
```

Expected: compile failure because contract types do not exist yet.

- [ ] **Step 3: Add the smallest contract type**

Create `PyralisAuthoringContract.cs`:

```csharp
using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringContract
    {
        public PyralisAuthoringContract(
            string stableId,
            string moduleId,
            string displayName,
            string authoringCategory,
            Type requiredProfileType,
            string[] requiredRuntimeInterfaceNames,
            ActorPresentationMode[] supportedPresentationModes,
            ActorPresentationMode[] unsupportedPresentationModes,
            string unsupportedLaneMessage,
            string[] consumedActionRoles,
            string nativeSetup,
            string firstProofTargetId,
            PyralisAuthoringConfidence confidence,
            string[] assignmentFields = null,
            string[] customizationMoments = null)
        {
            StableId = stableId ?? string.Empty;
            ModuleId = moduleId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            AuthoringCategory = authoringCategory ?? string.Empty;
            RequiredProfileType = requiredProfileType;
            RequiredRuntimeInterfaceNames = requiredRuntimeInterfaceNames ?? Array.Empty<string>();
            SupportedPresentationModes = supportedPresentationModes ?? Array.Empty<ActorPresentationMode>();
            UnsupportedPresentationModes = unsupportedPresentationModes ?? Array.Empty<ActorPresentationMode>();
            UnsupportedLaneMessage = unsupportedLaneMessage ?? string.Empty;
            ConsumedActionRoles = consumedActionRoles ?? Array.Empty<string>();
            NativeSetup = nativeSetup ?? string.Empty;
            FirstProofTargetId = firstProofTargetId ?? string.Empty;
            Confidence = confidence;
            AssignmentFields = assignmentFields ?? Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? Array.Empty<string>();
        }

        public string StableId { get; }
        public string ModuleId { get; }
        public string DisplayName { get; }
        public string AuthoringCategory { get; }
        public Type RequiredProfileType { get; }
        public string[] RequiredRuntimeInterfaceNames { get; }
        public ActorPresentationMode[] SupportedPresentationModes { get; }
        public ActorPresentationMode[] UnsupportedPresentationModes { get; }
        public string UnsupportedLaneMessage { get; }
        public string[] ConsumedActionRoles { get; }
        public string NativeSetup { get; }
        public string FirstProofTargetId { get; }
        public PyralisAuthoringConfidence Confidence { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }

        public bool MatchesModuleId(string moduleId)
        {
            return string.Equals(ModuleId, moduleId, StringComparison.Ordinal);
        }

        public bool SupportsPresentationMode(ActorPresentationMode mode)
        {
            if (SupportedPresentationModes.Length == 0)
                return true;

            for (int i = 0; i < SupportedPresentationModes.Length; i++)
            {
                if (SupportedPresentationModes[i] == mode)
                    return true;
            }

            return false;
        }

        public bool IsExplicitlyUnsupported(ActorPresentationMode mode)
        {
            for (int i = 0; i < UnsupportedPresentationModes.Length; i++)
            {
                if (UnsupportedPresentationModes[i] == mode)
                    return true;
            }

            return false;
        }
    }
}
```

- [ ] **Step 4: Add the provider interface**

Create `IAuthoringContractProvider.cs`:

```csharp
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public interface IAuthoringContractProvider
    {
        IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts();
    }
}
```

- [ ] **Step 5: Add a reflection-ready empty registry**

Create `PyralisAuthoringContractRegistry.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringContractRegistry
    {
        private static readonly Lazy<IReadOnlyList<PyralisAuthoringContract>> LazyContracts =
            new Lazy<IReadOnlyList<PyralisAuthoringContract>>(BuildContracts);

        public static IReadOnlyList<PyralisAuthoringContract> All => LazyContracts.Value;

        public static PyralisAuthoringContract FindByModuleId(string moduleId)
        {
            return All.FirstOrDefault(contract => contract.MatchesModuleId(moduleId));
        }

        public static bool HasDuplicateStableIds(out string duplicateStableId)
        {
            duplicateStableId = string.Empty;
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < All.Count; i++)
            {
                string id = All[i].StableId;
                if (!seen.Add(id))
                {
                    duplicateStableId = id;
                    return true;
                }
            }

            return false;
        }

        private static IReadOnlyList<PyralisAuthoringContract> BuildContracts()
        {
            return DiscoverProviders()
                .SelectMany(provider => provider.GetAuthoringContracts())
                .ToArray();
        }

        private static IEnumerable<IAuthoringContractProvider> DiscoverProviders()
        {
            return typeof(IAuthoringContractProvider).Assembly
                .GetTypes()
                .Where(type => typeof(IAuthoringContractProvider).IsAssignableFrom(type))
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .Where(type => type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type => (IAuthoringContractProvider)System.Activator.CreateInstance(type));
        }
    }
}
```

This should compile but keep the second test failing until the TopDownHop provider is added.

- [ ] **Step 6: Run focused tests**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringContractsContractTests"
```

Expected: duplicate test passes; find-by-module test fails because no provider exists.

- [ ] **Step 7: Commit**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs"
git commit -m "feat: add pyralis authoring contract model"
```

---

### Task 3: Add TopDownHop Contract Provider

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Traversal/Editor/TopDownHopAuthoringContractProvider.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContractRegistry.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`

- [ ] **Step 1: Expand the failing TopDownHop test**

Add:

```csharp
[Test]
public void TopDownHopContract_DeclaresProfileRuntimeLanesAndProof()
{
    PyralisAuthoringContract contract = PyralisAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop");

    Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo("NeonBlack.Gameplay.Data.Profiles.TopDownHopProfile"));
    Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Core.Contracts.IActorGameplayActionReceiver"));
    Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.True);
    Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
    Assert.That(contract.IsExplicitlyUnsupported(ActorPresentationMode.Rigged3D), Is.True);
    Assert.That(contract.ConsumedActionRoles, Does.Contain("Jump"));
    Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.1p-pawn-movement"));
}
```

- [ ] **Step 2: Add the provider**

Create `TopDownHopAuthoringContractProvider.cs`:

```csharp
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class TopDownHopAuthoringContractProvider : IAuthoringContractProvider
    {
        private static readonly PyralisAuthoringContract[] Contracts =
        {
            new PyralisAuthoringContract(
                stableId: "feature.actor.traversal.topdown-hop",
                moduleId: "actor.traversal.topdown-hop",
                displayName: "Top Down Hop",
                authoringCategory: "Traversal",
                requiredProfileType: typeof(TopDownHopProfile),
                requiredRuntimeInterfaceNames: new[]
                {
                    "NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime",
                    "NeonBlack.Gameplay.Core.Contracts.IActorGameplayActionReceiver"
                },
                supportedPresentationModes: new[]
                {
                    ActorPresentationMode.Sprite2D,
                    ActorPresentationMode.Billboard2_5D
                },
                unsupportedPresentationModes: new[]
                {
                    ActorPresentationMode.Rigged3D
                },
                unsupportedLaneMessage: "Rigged3D actors should use the 3D traversal jump path instead of the top-down visual-hop module.",
                consumedActionRoles: new[] { "Jump" },
                nativeSetup: "Create a TopDownHopProfile, create a FeatureModuleDefinition, assign a runtime prefab with TopDownHopFeatureRuntime, assign the profile asset, and add the module to PawnDefinition.featureModules.",
                firstProofTargetId: "proof.1p-pawn-movement",
                confidence: PyralisAuthoringConfidence.Explicit,
                assignmentFields: new[]
                {
                    "FeatureModuleDefinition.moduleId",
                    "FeatureModuleDefinition.runtimePrefab",
                    "FeatureModuleDefinition.profileAsset",
                    "PawnDefinition.featureModules",
                    "InputProfile.gameplayActions"
                },
                customizationMoments: new[]
                {
                    "TopDownHopProfile.actionRole",
                    "TopDownHopProfile.duration",
                    "TopDownHopProfile.height",
                    "TopDownHopProfile.cooldown",
                    "TopDownHopFeatureRuntime.visualTransform"
                })
        };

        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return Contracts;
        }
    }
}
```

- [ ] **Step 3: Confirm reflective discovery picks up the provider**

After adding `TopDownHopAuthoringContractProvider`, do not add it to a central list. The registry should discover it through `IAuthoringContractProvider` reflection. If the provider does not appear, fix discovery rather than adding a switch or central list.

Run the focused tests below. The find-by-module test is the proof that reflective provider discovery is working.

- [ ] **Step 4: Run focused contract tests**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringContractsContractTests"
```

Expected: all contract tests pass.

- [ ] **Step 5: Commit**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Traversal/Editor/TopDownHopAuthoringContractProvider.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContractRegistry.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs"
git commit -m "feat: declare top-down hop authoring contract"
```

---

### Task 4: Convert Contract Metadata Into Facts

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContractFacts.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactTypes.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Facts/PyralisAuthoringFactRegistry.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`

- [ ] **Step 1: Add a fact kind for feature contracts**

Add enum member:

```csharp
public enum PyralisAuthoringFactKind
{
    RouteFamily,
    RuntimeCapability,
    FeatureContract,
    SetupNode,
    Definition,
    Profile,
    SceneComponent,
    PrefabComponent,
    AssignmentField,
    CustomizationMoment,
    Issue,
    Proof
}
```

- [ ] **Step 2: Write failing fact parity test**

Add:

```csharp
[Test]
public void TopDownHopContract_ContributesAuthoringFact()
{
    PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.traversal.topdown-hop");

    Assert.That(fact, Is.Not.Null);
    Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
    Assert.That(fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.Validator));
    Assert.That(fact.RequiredProfiles, Does.Contain("TopDownHopProfile"));
    Assert.That(fact.RequiredPrefabComponents, Does.Contain("IActorGameplayActionReceiver"));
    Assert.That(fact.LaneTags, Does.Contain("Sprite2D"));
    Assert.That(fact.LaneTags, Does.Contain("Billboard2_5D"));
    Assert.That(fact.UnsupportedLaneTags, Does.Contain("Rigged3D"));
    Assert.That(fact.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));
}
```

- [ ] **Step 3: Implement contract-to-fact conversion**

Create `PyralisAuthoringContractFacts.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringContractFacts
    {
        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            return PyralisAuthoringContractRegistry.All
                .Select(CreateFact)
                .ToArray();
        }

        private static PyralisAuthoringFact CreateFact(PyralisAuthoringContract contract)
        {
            return new PyralisAuthoringFact(
                stableId: contract.StableId,
                displayName: contract.DisplayName,
                kind: PyralisAuthoringFactKind.FeatureContract,
                sourceKind: PyralisAuthoringFactSourceKind.Validator,
                confidence: contract.Confidence,
                summary: contract.NativeSetup,
                routeRelevance: contract.AuthoringCategory,
                firstProof: contract.FirstProofTargetId,
                laneTags: contract.SupportedPresentationModes.Select(mode => mode.ToString()).ToArray(),
                unsupportedLaneTags: contract.UnsupportedPresentationModes.Select(mode => mode.ToString()).ToArray(),
                requiredProfiles: contract.RequiredProfileType != null
                    ? new[] { contract.RequiredProfileType.Name }
                    : System.Array.Empty<string>(),
                requiredPrefabComponents: contract.RequiredRuntimeInterfaceNames.Select(ShortName).ToArray(),
                assignmentFields: contract.AssignmentFields,
                customizationMoments: contract.CustomizationMoments,
                nativeActions: new[]
                {
                    PyralisAuthoringNativeAction.ProjectCreateAsset,
                    PyralisAuthoringNativeAction.InspectorAssignField,
                    PyralisAuthoringNativeAction.InspectorAddComponent,
                    PyralisAuthoringNativeAction.EnterPlayMode
                },
                workIntent: "RequiredSetup",
                relatedStableIds: new[] { contract.FirstProofTargetId });
        }

        private static string ShortName(string typeName)
        {
            int index = typeName.LastIndexOf('.');
            return index >= 0 ? typeName.Substring(index + 1) : typeName;
        }
    }
}
```

- [ ] **Step 4: Add contract facts to the registry aggregation**

In `PyralisAuthoringFactRegistry`, add the contract facts to the existing fact list:

```csharp
facts.AddRange(PyralisAuthoringContractFacts.GetAuthoringFacts());
```

Place it after hand-authored catalog facts and before convention/reflection facts so explicit feature contracts win conceptually over lower-confidence convention facts.

- [ ] **Step 5: Run focused tests**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringContractsContractTests"
```

Expected: all pass.

- [ ] **Step 6: Commit**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs"
git commit -m "feat: expose feature authoring contracts as facts"
```

---

### Task 5: Add Contract-Backed Feature Module Validation

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisFeatureModuleContractValidator.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/FeatureModuleDefinitionEditor.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/FeatureModuleDefinition.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`

- [ ] **Step 1: Write failing validation tests**

Add tests that create in-memory feature modules and prefab objects:

```csharp
[Test]
public void TopDownHopContract_ReportsWrongProfileType()
{
    FeatureModuleDefinition module = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
    module.moduleId = "actor.traversal.topdown-hop";
    module.profileAsset = ScriptableObject.CreateInstance<InteractionFeatureProfile>();

    List<string> issues = PyralisFeatureModuleContractValidator.GetValidationIssues(module);

    Assert.That(issues, Has.Some.Contains("TopDownHopProfile"));
}

[Test]
public void TopDownHopContract_ReportsUnsupportedRigged3DLane()
{
    FeatureModuleDefinition module = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
    module.moduleId = "actor.traversal.topdown-hop";
    module.supportedPresentationModes = new[] { ActorPresentationMode.Rigged3D };

    List<string> issues = PyralisFeatureModuleContractValidator.GetValidationIssues(module);

    Assert.That(issues, Has.Some.Contains("Rigged3D actors should use the 3D traversal jump path"));
}
```

Add `using System.Collections.Generic;`, `using NeonBlack.Gameplay.Data.Definitions;`, `using NeonBlack.Gameplay.Data.Profiles;`, and `using UnityEngine;`.

- [ ] **Step 2: Implement editor-only validator**

Create `PyralisFeatureModuleContractValidator.cs`:

```csharp
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisFeatureModuleContractValidator
    {
        public static List<string> GetValidationIssues(FeatureModuleDefinition definition)
        {
            List<string> issues = new List<string>();
            if (definition == null)
                return issues;

            PyralisAuthoringContract contract = PyralisAuthoringContractRegistry.FindByModuleId(definition.moduleId);
            if (contract == null)
                return issues;

            if (contract.RequiredProfileType != null
                && definition.profileAsset != null
                && !contract.RequiredProfileType.IsInstanceOfType(definition.profileAsset))
            {
                issues.Add($"`{definition.moduleId}` expects a {contract.RequiredProfileType.Name} profile asset.");
            }

            ActorPresentationMode[] supportedModes = definition.supportedPresentationModes;
            if (supportedModes != null)
            {
                for (int i = 0; i < supportedModes.Length; i++)
                {
                    ActorPresentationMode mode = supportedModes[i];
                    if (contract.IsExplicitlyUnsupported(mode))
                        issues.Add(contract.UnsupportedLaneMessage);
                    else if (!contract.SupportsPresentationMode(mode))
                        issues.Add($"`{definition.moduleId}` does not declare support for {mode} presentation.");
                }
            }

            if (definition.runtimePrefab != null)
            {
                for (int i = 0; i < contract.RequiredRuntimeInterfaceNames.Length; i++)
                {
                    string interfaceName = contract.RequiredRuntimeInterfaceNames[i];
                    if (!HasComponentImplementing(definition.runtimePrefab, interfaceName))
                        issues.Add($"`{definition.moduleId}` runtime prefab should expose {ShortName(interfaceName)}.");
                }
            }

            return issues;
        }

        private static bool HasComponentImplementing(GameObject prefab, string interfaceName)
        {
            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                System.Type[] interfaces = behaviour.GetType().GetInterfaces();
                for (int j = 0; j < interfaces.Length; j++)
                {
                    if (interfaces[j].FullName == interfaceName)
                        return true;
                }
            }

            return false;
        }

        private static string ShortName(string typeName)
        {
            int index = typeName.LastIndexOf('.');
            return index >= 0 ? typeName.Substring(index + 1) : typeName;
        }
    }
}
```

- [ ] **Step 3: Feed the guided inspector**

In `FeatureModuleDefinitionEditor.OnInspectorGUI()`, replace:

```csharp
List<string> issues = definition.GetValidationIssues();
```

with:

```csharp
List<string> issues = definition.GetValidationIssues();
issues.AddRange(PyralisFeatureModuleContractValidator.GetValidationIssues(definition));
```

- [ ] **Step 4: Avoid duplicate data-layer checks for migrated modules**

In `FeatureModuleDefinition.GetValidationIssues()`, remove the `case "actor.traversal.topdown-hop"` profile/lane case only after the editor validator test passes. Leave generic data-layer checks, network checks, runtime prefab existence, and `IFeatureModuleRuntime` checks in place. Do not remove `AppendRuntimeContractIssues` yet if other code paths still rely on it.

- [ ] **Step 5: Run focused tests and build**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringContractsContractTests"
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: tests pass; build passes.

- [ ] **Step 6: Commit**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisFeatureModuleContractValidator.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/FeatureModuleDefinitionEditor.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/FeatureModuleDefinition.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs"
git commit -m "feat: validate feature modules from authoring contracts"
```

---

### Task 6: Add Interaction Comparison Contract

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Interaction/Editor/InteractionAuthoringContractProvider.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContractRegistry.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/FeatureModuleDefinition.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`

- [ ] **Step 1: Write comparison tests**

Add:

```csharp
[Test]
public void InteractionContract_DeclaresProfileRuntimeAndProof()
{
    PyralisAuthoringContract contract = PyralisAuthoringContractRegistry.FindByModuleId("actor.interaction");

    Assert.That(contract, Is.Not.Null);
    Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo("NeonBlack.Gameplay.Data.Profiles.InteractionFeatureProfile"));
    Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Interaction.IActorInteractionFeature"));
    Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.action-selection"));
}
```

- [ ] **Step 2: Add provider**

Create:

```csharp
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class InteractionAuthoringContractProvider : IAuthoringContractProvider
    {
        private static readonly PyralisAuthoringContract[] Contracts =
        {
            new PyralisAuthoringContract(
                stableId: "feature.actor.interaction",
                moduleId: "actor.interaction",
                displayName: "Actor Interaction",
                authoringCategory: "Interaction",
                requiredProfileType: typeof(InteractionFeatureProfile),
                requiredRuntimeInterfaceNames: new[]
                {
                    "NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime",
                    "NeonBlack.Gameplay.Features.Interaction.IActorInteractionFeature"
                },
                supportedPresentationModes: System.Array.Empty<ActorPresentationMode>(),
                unsupportedPresentationModes: System.Array.Empty<ActorPresentationMode>(),
                unsupportedLaneMessage: string.Empty,
                consumedActionRoles: new[] { "Interact" },
                nativeSetup: "Create an InteractionFeatureProfile, create a FeatureModuleDefinition, assign a runtime prefab with an interaction feature runtime, assign the profile asset, and add the module to PawnDefinition.featureModules.",
                firstProofTargetId: "proof.action-selection",
                confidence: PyralisAuthoringConfidence.Explicit,
                assignmentFields: new[]
                {
                    "FeatureModuleDefinition.moduleId",
                    "FeatureModuleDefinition.runtimePrefab",
                    "FeatureModuleDefinition.profileAsset",
                    "PawnDefinition.featureModules",
                    "InputProfile.gameplayActions"
                },
                customizationMoments: new[]
                {
                    "InteractionFeatureProfile.interactionRadius",
                    "InteractionFeatureProfile.cooldownSeconds",
                    "InputProfile Interact binding"
                })
        };

        public IReadOnlyList<PyralisAuthoringContract> GetAuthoringContracts()
        {
            return Contracts;
        }
    }
}
```

- [ ] **Step 3: Confirm reflective discovery picks up the provider**

Do not add `InteractionAuthoringContractProvider` to a central provider list. The existing registry discovery should find it automatically.

- [ ] **Step 4: Remove migrated data-layer switch cases**

Remove the `actor.interaction` profile case and the `actor.interaction` runtime interface case from `FeatureModuleDefinition` after the editor contract tests pass. Leave generic runtime prefab checks in the data layer.

- [ ] **Step 5: Run tests**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringContractsContractTests"
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: passes.

- [ ] **Step 6: Commit**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Interaction/Editor/InteractionAuthoringContractProvider.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Contracts/PyralisAuthoringContractRegistry.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/FeatureModuleDefinition.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs"
git commit -m "feat: add interaction authoring contract"
```

---

### Task 7: Surface Contracts In The Inspector And Facts Tab

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/FeatureModuleDefinitionEditor.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringWindow.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringSourceContractTests.cs`

- [ ] **Step 1: Add source contract assertions**

Add assertions:

```csharp
string featureModuleEditorSource = File.ReadAllText(Path.Combine(editorRoot, "FeatureModuleDefinitionEditor.cs"));

Assert.That(featureModuleEditorSource.Contains("PyralisAuthoringContractRegistry.FindByModuleId"), Is.True);
Assert.That(featureModuleEditorSource.Contains("First Proof Target"), Is.True);
Assert.That(featureModuleEditorSource.Contains("Consumed Action Roles"), Is.True);
Assert.That(authoringWindow.Contains("FeatureContract"), Is.True);
```

- [ ] **Step 2: Add inspector contract summary**

In `FeatureModuleDefinitionEditor`, after platform metadata and before validation:

```csharp
PyralisAuthoringContract contract = PyralisAuthoringContractRegistry.FindByModuleId(definition.moduleId);
if (contract != null)
{
    EditorGUILayout.Space(6f);
    EditorGUILayout.LabelField("Authoring Contract", EditorStyles.boldLabel);
    EditorGUILayout.LabelField("Stable Id", contract.StableId);
    EditorGUILayout.LabelField("First Proof Target", contract.FirstProofTargetId);
    EditorGUILayout.LabelField("Consumed Action Roles", string.Join(", ", contract.ConsumedActionRoles));
    EditorGUILayout.HelpBox(contract.NativeSetup, MessageType.Info);
}
```

Keep this read-only. Do not add create-and-assign buttons in this task.

- [ ] **Step 3: Make Facts tab visibly classify contracts**

In the Facts tab grouping logic, ensure `PyralisAuthoringFactKind.FeatureContract` renders with the same generic fact-card path as other facts and does not get filtered out. If the tab already groups every enum value automatically, no UI change is needed beyond the source assertion.

- [ ] **Step 4: Run tests and open compile gate**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringSourceContractTests.PyralisEditor_Source_ExposesGuideOnlyRuntimeCapabilityCatalog"
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: passes.

- [ ] **Step 5: Commit**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/FeatureModuleDefinitionEditor.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/PyralisAuthoringWindow.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringSourceContractTests.cs"
git commit -m "feat: show authoring contracts in pyralis editor surfaces"
```

---

### Task 8: Add Coverage Rules For New Feature Modules

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_BLUEPRINT.md`

- [ ] **Step 1: Add coverage test for known module switch cases**

Add a test that protects the current migration surface:

```csharp
[Test]
public void MigratedFeatureModules_HaveContractCoverage()
{
    string[] requiredModuleIds =
    {
        "actor.traversal.topdown-hop",
        "actor.interaction"
    };

    for (int i = 0; i < requiredModuleIds.Length; i++)
    {
        Assert.That(PyralisAuthoringContractRegistry.FindByModuleId(requiredModuleIds[i]), Is.Not.Null, requiredModuleIds[i]);
    }
}
```

Do not require every existing module id yet. Expand this list only as modules complete contract coverage.

- [ ] **Step 2: Add roadmap rule**

In `FEATURE_DEVELOPMENT_ROADMAP.md`, add:

```markdown
New runtime features should not be considered authoring-ready until they contribute an authoring contract. The contract should name module id, profile type, runtime prefab/interface requirements, supported and unsupported presentation lanes, semantic action roles, native Unity setup actions, validation issue codes, first proof target, customization fields, and provenance/confidence.
```

- [ ] **Step 3: Add blueprint rule**

In `AUTHORING_BLUEPRINT.md`, add:

```markdown
Feature-owned authoring contracts are the next layer above guide cards and convention facts. A feature contract is intent and setup evidence, not proof. It may feed Facts, Overview, Validate, and inspectors, but it must not replace runtime definitions/profiles or claim Play Mode success without test or manual proof evidence.
```

- [ ] **Step 4: Run docs/source tests**

Run:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringContractsContractTests|FullyQualifiedName~SetupDocsContractTests"
```

Expected: passes.

- [ ] **Step 5: Commit**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md" `
        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_BLUEPRINT.md"
git commit -m "docs: define feature authoring contract coverage rule"
```

---

### Task 9: Native Unity Proof And Full Gate

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_CONTRACTS_HANDOFF.md`
- Optional evidence output: `Logs/Codex/*` local only unless project policy says otherwise.

- [ ] **Step 1: Run compile and Unity refresh**

If new package editor sources were added in this phase, verify each has a `.meta` file and confirm a Unity refresh/reimport happened before using CLI build as proof.

Run:

```powershell
dotnet restore "Game Studio Core.slnx"
dotnet build "Game Studio Core.slnx" --no-restore
```

Expected: compile passes with no project-code errors.

If the GUI Unity Editor is open, use the existing refresh helper from `unity-project-stewardship` in attach mode. If the Editor is closed and a full gate is intended, continue to Step 2.

- [ ] **Step 2: Run full pre-scene validation gate**

Close the GUI Unity Editor first. Then run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected: dotnet restore/build, Unity EditMode, Unity PlayMode, final restore/build, and residue scan complete. Record XML summary counts from `Logs\Codex`.

- [ ] **Step 3: Run manual native authoring proof**

Use Computer Use in the Unity Editor. Do not add scene generators, YAML edits, or hidden auto-wire scripts.

Proof route:

1. Open `NeonBlack/Gameplay/Pyralis Authoring Window`.
2. Select or pin the Sprite2D pawn setup route.
3. Create or inspect a `TopDownHopProfile` from the Project window.
4. Create or inspect a `FeatureModuleDefinition` with module id `actor.traversal.topdown-hop`.
5. Assign a runtime prefab containing `TopDownHopFeatureRuntime`.
6. Assign `TopDownHopProfile` to `FeatureModuleDefinition.profileAsset`.
7. Add the module to `PawnDefinition.featureModules`.
8. Confirm Facts tab shows `feature.actor.traversal.topdown-hop`.
9. Confirm Validate/Inspector reports contract-backed issues when profile, prefab interface, or lane setup is wrong.
10. Enter Play Mode only after Do Now is clear.
11. Press the configured Jump action.
12. Observe the map-plane body staying grounded while the visual child hops.

- [ ] **Step 4: Update handoff with completion evidence**

Append:

```markdown
## Authoring Contracts Refactor Status

- Contract spine: implemented for `actor.traversal.topdown-hop` and `actor.interaction`.
- Contract facts: visible in the Facts tab as intent/evidence/proof-target metadata.
- Feature-module validation: contract-backed for migrated modules.
- Proof status: [record exact Unity proof result here with date].
- Validation: [record dotnet build and Unity XML counts here].
```

- [ ] **Step 5: Commit final docs**

```powershell
git add "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/AUTHORING_CONTRACTS_HANDOFF.md"
git commit -m "docs: record authoring contracts validation evidence"
```

---

## Validation Strategy

Fast checks after each task:

```powershell
dotnet test "NeonBlack.Gameplay.Editor.Tests.csproj" --filter "FullyQualifiedName~AuthoringContractsContractTests"
dotnet build "Game Studio Core.slnx" --no-restore
```

Project gate at the end:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Manual proof:

- Use native Unity authoring workflows only.
- Use the Authoring Window as route guidance and facts/validation surface.
- Do not use generated proof scenes or hidden auto-wiring as evidence.
- Keep proof language precise: contracts create intent/evidence/proof targets; Play Mode or tests create proof.

## Risks And Controls

- **Risk:** Contract layer becomes a second runtime composition system.
  - **Control:** Keep contracts editor-facing. Runtime continues reading definitions, profiles, prefabs, feature modules, and services.
- **Risk:** Facts tab becomes noisy.
  - **Control:** Start with two contracts and source/fact tests; do not scrape the whole project yet.
- **Risk:** Data assembly loses validation coverage when switch cases move out.
  - **Control:** Remove only migrated module-specific cases after editor contract tests cover equivalent behavior.
- **Risk:** Claims overrun proof.
  - **Control:** UI/docs must use “proof target” or “ready to attempt” until Unity Play Mode proof runs.
- **Risk:** Reflection discovery overclaims or picks up unintended providers.
  - **Control:** Discover only concrete `IAuthoringContractProvider` types with public parameterless constructors in the editor assembly, keep contracts explicit and feature-owned, and rely on duplicate-id/provenance tests.

## Completion Criteria

The refactor is complete when:

- Shared fact primitives live in focused files.
- `PyralisAuthoringContract` and provider registry exist.
- `TopDownHop` and `Interaction` have feature-owned contracts.
- Contract facts appear in `PyralisAuthoringFactRegistry` and the Authoring Window Facts tab.
- `FeatureModuleDefinitionEditor` displays contract metadata and contract-backed validation.
- Module-specific hardcoded validation has been removed for migrated modules.
- Tests protect duplicate contract ids, contract fact parity, profile type checks, runtime interface checks, unsupported-lane messaging, and migrated module coverage.
- Active docs explain that feature contracts are required for authoring-ready features.
- Dotnet build and Unity validation have passing evidence.
- A native Unity authoring proof confirms Sprite2D top-down hop can be authored without scene generators or hidden auto-wire scripts.

## Execution Recommendation

Use subagent-driven execution with one worker per phase after Task 1. Task 1 should be integrated carefully by the main agent or one narrow worker because it touches central fact types. Keep final integration, validation, docs, and checkpoint classification in the main agent.
