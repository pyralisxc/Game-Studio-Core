# Read-Only Authoring Setup Graph Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a read-only resolved authoring setup graph that normalizes existing Pyralis authoring contracts, facts, route analysis, setup-flow evidence, and scene-readiness evidence without changing visible Authoring Window behavior.

**Architecture:** Create a focused `Editor/Authoring/Spine/Graph` model and builder. The builder consumes existing spine sources and emits graph nodes/edges/evidence; current tabs keep using their existing models until later phases. Tests prove the graph reflects a pawn route and reflected feature contracts.

**Tech Stack:** Unity 6000.4, C#, NUnit EditMode tests, Pyralis authoring spine, `ResolvedAuthoringContractRegistry`, `PyralisSetupRouteAnalysis`, `PyralisSetupFlowValidator`, `PyralisSceneReadinessValidator`.

---

## File Structure

- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraphTypes.cs`
  - Owns graph enums and immutable node/edge records.
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraph.cs`
  - Owns read-only graph container and lookup helpers.
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraphBuilder.cs`
  - Builds the graph from existing route, contract, fact, setup-flow, and scene-readiness sources.
- Create meta files for the new folder and scripts.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`
  - Adds graph coverage tests.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/AUTHORING_MODEL.md`
  - Documents the graph as the next spine layer.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/AUTHORING_BLUEPRINT.md`
  - Adds the graph owner to the implementation responsibility table.

## Task 1: Graph Types

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraphTypes.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph.meta`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraphTypes.cs.meta`

- [ ] **Step 1: Create the graph folder and meta file**

Create `Editor/Authoring/Spine/Graph` with a Unity `.meta` file. Use a fresh GUID.

```yaml
fileFormatVersion: 2
guid: 11111111111111111111111111111111
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```

- [ ] **Step 2: Add the graph type model**

Create `PyralisAuthoringSetupGraphTypes.cs`:

```csharp
using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringGraphNodeKind
    {
        Unknown,
        SetupChain,
        Capability,
        Contract,
        Proof,
        SceneSurface,
        PrefabRequirement,
        AssignmentField,
        ValidationEvidence
    }

    public enum PyralisAuthoringGraphSourceKind
    {
        Unknown,
        SetupProfile,
        RuntimeCapabilityCatalog,
        RuntimePattern,
        AuthoringContract,
        FactRegistry,
        SetupFlow,
        SceneReadiness,
        RouteProof
    }

    public enum PyralisAuthoringGraphEvidenceState
    {
        Unknown,
        Optional,
        Missing,
        CandidateDetected,
        Ready,
        Blocked
    }

    public enum PyralisAuthoringGraphEdgeKind
    {
        RelatesTo,
        DependsOn,
        Satisfies,
        Recommends,
        BlocksProof
    }

    public sealed class PyralisAuthoringGraphNode
    {
        public PyralisAuthoringGraphNode(
            string stableId,
            string label,
            PyralisAuthoringGraphNodeKind kind,
            PyralisAuthoringGraphSourceKind sourceKind,
            PyralisAuthoringGraphEvidenceState evidenceState = PyralisAuthoringGraphEvidenceState.Unknown,
            RuntimeCapabilityFamily capabilityFamily = RuntimeCapabilityFamily.PlatformCore,
            AuthoringCapability authoringCapability = AuthoringCapability.None,
            string proofTargetId = null,
            string guidance = null,
            string[] nativeSetup = null,
            string[] assignmentFields = null,
            string[] customizationMoments = null,
            string blockingReason = null,
            ResolvedAuthoringContract sourceContract = null,
            UnityEngine.Object sourceObject = null)
        {
            StableId = stableId ?? string.Empty;
            Label = label ?? string.Empty;
            Kind = kind;
            SourceKind = sourceKind;
            EvidenceState = evidenceState;
            CapabilityFamily = capabilityFamily;
            AuthoringCapability = authoringCapability;
            ProofTargetId = proofTargetId ?? string.Empty;
            Guidance = guidance ?? string.Empty;
            NativeSetup = nativeSetup ?? Array.Empty<string>();
            AssignmentFields = assignmentFields ?? Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? Array.Empty<string>();
            BlockingReason = blockingReason ?? string.Empty;
            SourceContract = sourceContract;
            SourceObject = sourceObject;
        }

        public string StableId { get; }
        public string Label { get; }
        public PyralisAuthoringGraphNodeKind Kind { get; }
        public PyralisAuthoringGraphSourceKind SourceKind { get; }
        public PyralisAuthoringGraphEvidenceState EvidenceState { get; }
        public RuntimeCapabilityFamily CapabilityFamily { get; }
        public AuthoringCapability AuthoringCapability { get; }
        public string ProofTargetId { get; }
        public string Guidance { get; }
        public string[] NativeSetup { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }
        public string BlockingReason { get; }
        public ResolvedAuthoringContract SourceContract { get; }
        public UnityEngine.Object SourceObject { get; }
    }

    public sealed class PyralisAuthoringGraphEdge
    {
        public PyralisAuthoringGraphEdge(string fromNodeId, string toNodeId, PyralisAuthoringGraphEdgeKind kind, string label = null)
        {
            FromNodeId = fromNodeId ?? string.Empty;
            ToNodeId = toNodeId ?? string.Empty;
            Kind = kind;
            Label = label ?? string.Empty;
        }

        public string FromNodeId { get; }
        public string ToNodeId { get; }
        public PyralisAuthoringGraphEdgeKind Kind { get; }
        public string Label { get; }
    }
}
```

- [ ] **Step 3: Add script meta**

Create `PyralisAuthoringSetupGraphTypes.cs.meta` with a fresh GUID:

```yaml
fileFormatVersion: 2
guid: 22222222222222222222222222222222
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```

- [ ] **Step 4: Refresh Unity**

Run:

```powershell
$UnityValidation = "C:\Users\camer\.codex\skills\unity-project-stewardship\scripts\Invoke-UnityValidation.ps1"
& $UnityValidation -ProjectPath (Get-Location).Path -Mode Refresh -ReimportPath "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph" -TimeoutMinutes 5
```

Expected: Unity refresh completes without compiler errors.

## Task 2: Graph Container

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraph.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraph.cs.meta`

- [ ] **Step 1: Write the graph container**

Create `PyralisAuthoringSetupGraph.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringSetupGraph
    {
        private readonly List<PyralisAuthoringGraphNode> _nodes;
        private readonly List<PyralisAuthoringGraphEdge> _edges;
        private readonly Dictionary<string, PyralisAuthoringGraphNode> _nodeById;

        public PyralisAuthoringSetupGraph(
            UnityEngine.Object source,
            PyralisSetupRouteAnalysis routeAnalysis,
            IEnumerable<PyralisAuthoringGraphNode> nodes,
            IEnumerable<PyralisAuthoringGraphEdge> edges)
        {
            Source = source;
            RouteAnalysis = routeAnalysis;
            _nodes = nodes != null ? nodes.Where(node => node != null).ToList() : new List<PyralisAuthoringGraphNode>();
            _edges = edges != null ? edges.Where(edge => edge != null).ToList() : new List<PyralisAuthoringGraphEdge>();
            _nodeById = new Dictionary<string, PyralisAuthoringGraphNode>(StringComparer.Ordinal);

            for (int i = 0; i < _nodes.Count; i++)
            {
                PyralisAuthoringGraphNode node = _nodes[i];
                if (!string.IsNullOrWhiteSpace(node.StableId) && !_nodeById.ContainsKey(node.StableId))
                    _nodeById.Add(node.StableId, node);
            }
        }

        public UnityEngine.Object Source { get; }
        public PyralisSetupRouteAnalysis RouteAnalysis { get; }
        public IReadOnlyList<PyralisAuthoringGraphNode> Nodes => _nodes;
        public IReadOnlyList<PyralisAuthoringGraphEdge> Edges => _edges;

        public bool TryFindNode(string stableId, out PyralisAuthoringGraphNode node)
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                node = null;
                return false;
            }

            return _nodeById.TryGetValue(stableId, out node);
        }

        public IReadOnlyList<PyralisAuthoringGraphNode> FindNodes(PyralisAuthoringGraphNodeKind kind)
        {
            return _nodes.Where(node => node.Kind == kind).ToArray();
        }

        public IReadOnlyList<PyralisAuthoringGraphEdge> FindOutgoing(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                return Array.Empty<PyralisAuthoringGraphEdge>();

            return _edges.Where(edge => string.Equals(edge.FromNodeId, stableId, StringComparison.Ordinal)).ToArray();
        }
    }
}
```

- [ ] **Step 2: Add script meta**

Create `PyralisAuthoringSetupGraph.cs.meta` with a fresh GUID.

- [ ] **Step 3: Run compile smoke**

Run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1" -Phase Smoke
```

Expected: dotnet build succeeds.

## Task 3: Graph Builder

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraphBuilder.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph/PyralisAuthoringSetupGraphBuilder.cs.meta`

- [ ] **Step 1: Write graph builder skeleton**

Create `PyralisAuthoringSetupGraphBuilder.cs`:

```csharp
using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringSetupGraphBuilder
    {
        public static PyralisAuthoringSetupGraph Build(UnityEngine.Object source)
        {
            PyralisSetupRouteAnalysis route = BuildRoute(source);
            List<PyralisAuthoringGraphNode> nodes = new List<PyralisAuthoringGraphNode>();
            List<PyralisAuthoringGraphEdge> edges = new List<PyralisAuthoringGraphEdge>();

            AddSetupChainNodes(source, route, nodes, edges);
            AddCapabilityNodes(route, nodes, edges);
            AddProofNode(route, nodes, edges);
            AddContractNodes(route, nodes, edges);
            AddSetupFlowEvidence(source, nodes, edges);
            AddSceneReadinessEvidence(source, nodes, edges);

            return new PyralisAuthoringSetupGraph(source, route, nodes, edges);
        }

        private static PyralisSetupRouteAnalysis BuildRoute(UnityEngine.Object source)
        {
            if (source is GameplaySessionBootstrap bootstrap)
                return PyralisSetupRouteAnalysis.Build(bootstrap);
            if (source is SessionDefinition session)
                return PyralisSetupRouteAnalysis.Build(session);
            if (source is GameModeDefinition mode)
                return PyralisSetupRouteAnalysis.Build(mode);
            if (source is GameSetupProfile setupProfile)
                return PyralisSetupRouteAnalysis.Build(setupProfile);

            return PyralisSetupRouteAnalysis.Build((GameSetupProfile)null);
        }
    }
}
```

- [ ] **Step 2: Add setup chain nodes**

Add this method to the builder:

```csharp
        private static void AddSetupChainNodes(
            UnityEngine.Object source,
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            AddNode(nodes, new PyralisAuthoringGraphNode(
                "bootstrap.root",
                "Gameplay Root",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                source is GameplaySessionBootstrap ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Unknown,
                sourceObject: source as GameplaySessionBootstrap));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "session.definition",
                "Session Definition",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                route != null && route.SessionDefinition != null ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                sourceObject: route?.SessionDefinition));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "mode.definition",
                "Game Mode Definition",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                route != null && route.GameModeDefinition != null ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                sourceObject: route?.GameModeDefinition));

            AddNode(nodes, new PyralisAuthoringGraphNode(
                "setup.profile",
                "Game Setup Profile",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupProfile,
                route != null && route.SetupProfile != null ? PyralisAuthoringGraphEvidenceState.Ready : PyralisAuthoringGraphEvidenceState.Missing,
                sourceObject: route?.SetupProfile));

            AddEdge(edges, "bootstrap.root", "session.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "reads");
            AddEdge(edges, "session.definition", "mode.definition", PyralisAuthoringGraphEdgeKind.DependsOn, "default mode");
            AddEdge(edges, "mode.definition", "setup.profile", PyralisAuthoringGraphEdgeKind.DependsOn, "setup profile");
        }
```

- [ ] **Step 3: Add capability nodes**

Add this method:

```csharp
        private static void AddCapabilityNodes(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>();
            for (int i = 0; i < families.Length; i++)
            {
                RuntimeCapabilityFamily family = families[i];
                RuntimeCapabilityCard card = PyralisRuntimeCapabilityCatalog.FindPrimaryByFamily(family);
                string nodeId = "capability." + family.ToString().ToLowerInvariant();
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    card != null ? card.Title : family.ToString(),
                    PyralisAuthoringGraphNodeKind.Capability,
                    card != null ? PyralisAuthoringGraphSourceKind.RuntimeCapabilityCatalog : PyralisAuthoringGraphSourceKind.SetupProfile,
                    PyralisAuthoringGraphEvidenceState.Ready,
                    family,
                    card != null ? card.AuthoringCapability : AuthoringCapability.None,
                    card != null && card.PrimaryProofCandidate ? card.ProofTargetId : string.Empty,
                    card != null ? card.Description : string.Empty,
                    card != null ? new[] { card.SetupSummary } : Array.Empty<string>(),
                    customizationMoments: card != null ? new[] { card.CustomizationSummary } : Array.Empty<string>()));

                AddEdge(edges, "setup.profile", nodeId, PyralisAuthoringGraphEdgeKind.Satisfies, "selected capability");
            }
        }
```

- [ ] **Step 4: Add proof node**

Add this method:

```csharp
        private static void AddProofNode(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            PyralisAuthoringRouteDescriptor descriptor = PyralisAuthoringRouteDescriptor.Build(route);
            PyralisAuthoringRouteProof proof = PyralisAuthoringRouteProof.Build(descriptor);
            AddNode(nodes, new PyralisAuthoringGraphNode(
                "proof.current",
                proof.Label,
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.RouteProof,
                PyralisAuthoringGraphEvidenceState.Unknown,
                proofTargetId: proof.ProofTargetId,
                guidance: proof.Guidance,
                nativeSetup: new[] { proof.SetupSurface },
                blockingReason: proof.SuccessCriteria));

            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? Array.Empty<RuntimeCapabilityFamily>();
            for (int i = 0; i < families.Length; i++)
            {
                AddEdge(edges, "capability." + families[i].ToString().ToLowerInvariant(), "proof.current", PyralisAuthoringGraphEdgeKind.BlocksProof, "supports proof");
            }
        }
```

- [ ] **Step 5: Add contract nodes**

Add this method:

```csharp
        private static void AddContractNodes(
            PyralisSetupRouteAnalysis route,
            List<PyralisAuthoringGraphNode> nodes,
            List<PyralisAuthoringGraphEdge> edges)
        {
            foreach (ResolvedAuthoringContract contract in ResolvedAuthoringContractRegistry.All)
            {
                if (contract == null || string.IsNullOrWhiteSpace(contract.StableId))
                    continue;

                AddNode(nodes, new PyralisAuthoringGraphNode(
                    "contract." + contract.StableId,
                    contract.DisplayName,
                    PyralisAuthoringGraphNodeKind.Contract,
                    PyralisAuthoringGraphSourceKind.AuthoringContract,
                    PyralisAuthoringGraphEvidenceState.Unknown,
                    authoringCapability: contract.Capability,
                    proofTargetId: contract.FirstProofTargetId,
                    guidance: contract.Relevance,
                    nativeSetup: contract.NativeSetup,
                    assignmentFields: contract.AssignmentFields,
                    customizationMoments: contract.CustomizationMoments,
                    sourceContract: contract));

                if (!string.IsNullOrWhiteSpace(contract.FirstProofTargetId))
                    AddEdge(edges, "contract." + contract.StableId, "proof.current", PyralisAuthoringGraphEdgeKind.Recommends, "proof guidance");
            }
        }
```

- [ ] **Step 6: Add setup-flow and scene-readiness evidence wrappers**

Add these methods:

```csharp
        private static void AddSetupFlowEvidence(UnityEngine.Object source, List<PyralisAuthoringGraphNode> nodes, List<PyralisAuthoringGraphEdge> edges)
        {
            if (source is not GameplaySessionBootstrap bootstrap)
                return;

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            IReadOnlyList<PyralisSetupFlowStep> steps = report.Steps;
            for (int i = 0; i < steps.Count; i++)
            {
                PyralisSetupFlowStep step = steps[i];
                string nodeId = "setupflow." + NormalizeId(step.Label);
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    step.Label,
                    PyralisAuthoringGraphNodeKind.ValidationEvidence,
                    PyralisAuthoringGraphSourceKind.SetupFlow,
                    ConvertSetupFlowStatus(step.Status),
                    guidance: step.Message,
                    blockingReason: step.Status == PyralisSetupFlowStepStatus.Blocked ? step.Message : string.Empty));
                AddEdge(edges, "bootstrap.root", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "setup evidence");
            }
        }

        private static void AddSceneReadinessEvidence(UnityEngine.Object source, List<PyralisAuthoringGraphNode> nodes, List<PyralisAuthoringGraphEdge> edges)
        {
            if (source is not GameplaySessionBootstrap bootstrap)
                return;

            PyralisSceneReadinessReport report = PyralisSceneReadinessValidator.BuildReport(bootstrap);
            IReadOnlyList<PyralisSceneReadinessIssue> issues = report.Issues;
            for (int i = 0; i < issues.Count; i++)
            {
                PyralisSceneReadinessIssue issue = issues[i];
                string nodeId = "scenereadiness." + NormalizeId(issue.Code);
                AddNode(nodes, new PyralisAuthoringGraphNode(
                    nodeId,
                    issue.Title,
                    PyralisAuthoringGraphNodeKind.ValidationEvidence,
                    PyralisAuthoringGraphSourceKind.SceneReadiness,
                    issue.RequiredBeforePlay ? PyralisAuthoringGraphEvidenceState.Blocked : PyralisAuthoringGraphEvidenceState.Missing,
                    guidance: issue.Message,
                    blockingReason: issue.RequiredBeforePlay ? issue.Message : string.Empty));
                AddEdge(edges, "bootstrap.root", nodeId, PyralisAuthoringGraphEdgeKind.RelatesTo, "scene readiness");
            }
        }
```

- [ ] **Step 7: Add helper methods**

Add these methods:

```csharp
        private static void AddNode(List<PyralisAuthoringGraphNode> nodes, PyralisAuthoringGraphNode node)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (string.Equals(nodes[i].StableId, node.StableId, StringComparison.Ordinal))
                    return;
            }

            nodes.Add(node);
        }

        private static void AddEdge(List<PyralisAuthoringGraphEdge> edges, string fromNodeId, string toNodeId, PyralisAuthoringGraphEdgeKind kind, string label)
        {
            if (string.IsNullOrWhiteSpace(fromNodeId) || string.IsNullOrWhiteSpace(toNodeId))
                return;

            edges.Add(new PyralisAuthoringGraphEdge(fromNodeId, toNodeId, kind, label));
        }

        private static PyralisAuthoringGraphEvidenceState ConvertSetupFlowStatus(PyralisSetupFlowStepStatus status)
        {
            return status switch
            {
                PyralisSetupFlowStepStatus.Ready => PyralisAuthoringGraphEvidenceState.Ready,
                PyralisSetupFlowStepStatus.Recommended => PyralisAuthoringGraphEvidenceState.CandidateDetected,
                PyralisSetupFlowStepStatus.Optional => PyralisAuthoringGraphEvidenceState.Optional,
                PyralisSetupFlowStepStatus.Blocked => PyralisAuthoringGraphEvidenceState.Blocked,
                _ => PyralisAuthoringGraphEvidenceState.Unknown
            };
        }

        private static string NormalizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            char[] chars = value.ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '-';
            }

            return new string(chars).Trim('-');
        }
```

- [ ] **Step 8: Add script meta and refresh**

Create `PyralisAuthoringSetupGraphBuilder.cs.meta` with a fresh GUID, then run Unity refresh for the Graph folder.

Expected: Unity refresh completes without compiler errors.

## Task 4: Graph Tests

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/AuthoringContractsContractTests.cs`

- [ ] **Step 1: Add contract graph test**

Add this test inside `AuthoringContractsContractTests`:

```csharp
        [Test]
        public void AuthoringSetupGraph_ReflectsFeatureContractsAsNodes()
        {
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null);

            Assert.That(graph.TryFindNode("contract.feature.actor.traversal.topdown-hop", out PyralisAuthoringGraphNode node), Is.True);
            Assert.That(node.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Contract));
            Assert.That(node.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.AuthoringContract));
            Assert.That(node.ProofTargetId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(node.NativeSetup, Is.Not.Empty);
        }
```

- [ ] **Step 2: Add setup profile graph test**

Add this test:

```csharp
        [Test]
        public void AuthoringSetupGraph_GameSetupProfileCreatesCapabilityAndProofNodes()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimeCapabilities = new[] { RuntimeCapabilityFamily.CharacterPawnGameplay, RuntimeCapabilityFamily.Combat };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);

            Assert.That(graph.TryFindNode("setup.profile", out PyralisAuthoringGraphNode setupNode), Is.True);
            Assert.That(setupNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(graph.TryFindNode("capability.characterpawngameplay", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Capability));
            Assert.That(graph.TryFindNode("proof.current", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Proof));
            Assert.That(graph.Edges.Any(edge => edge.ToNodeId == "proof.current"), Is.True);

            Object.DestroyImmediate(setupProfile);
        }
```

- [ ] **Step 3: Run targeted editor test build**

Run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1" -Phase Smoke
```

Expected: build succeeds. If Unity tests are required for new graph behavior, close Unity and run the full pre-scene gate.

## Task 5: Docs

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/AUTHORING_MODEL.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/AUTHORING_BLUEPRINT.md`

- [ ] **Step 1: Update AUTHORING_MODEL graph language**

Replace the paragraph that says tabs are projections from the same cookbook with language that says the current transition target is the resolved setup graph:

```markdown
The **Intent**, **Guide**, **Overview**, **Map**, **Validate**, and **Facts** tabs are projections from the same authoring spine. The current target spine is a read-only resolved setup graph built from contracts, cookbook facts, route analysis, setup-flow evidence, scene-readiness evidence, and selected Unity context. The graph does not create assets or apply presets; it explains what the selected intent and current setup imply.
```

- [ ] **Step 2: Update AUTHORING_BLUEPRINT owner table**

Add a row:

```markdown
| `PyralisAuthoringSetupGraph` | read-only resolved graph of setup nodes, edges, evidence, proof targets, and source contracts |
```

- [ ] **Step 3: Run docs stale-reference scan**

Run:

```powershell
rg -n "PyralisAuthoringFeatureAdvisor|IAuthoringContractProvider|Docs/Setup|Docs\\Setup" Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring -g "*.md" --glob "!**/_Archive/**"
```

Expected: no active-doc matches for deleted authoring/provider paths.

## Task 6: Final Verification

**Files:**
- No new files unless validation exposes a compile issue.

- [ ] **Step 1: Refresh Unity graph folder**

Run:

```powershell
$UnityValidation = "C:\Users\camer\.codex\skills\unity-project-stewardship\scripts\Invoke-UnityValidation.ps1"
& $UnityValidation -ProjectPath (Get-Location).Path -Mode Refresh -ReimportPath "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Spine/Graph" -TimeoutMinutes 5
```

Expected: Unity refresh completes and Editor log has no fresh compiler errors.

- [ ] **Step 2: Run smoke gate**

Run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1" -Phase Smoke
```

Expected: pre-scene validation passes.

- [ ] **Step 3: Run full gate when Unity is closed**

Run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected: full validation passes. If Unity is open, report that full gate is deferred and name smoke plus Unity refresh evidence.

## Self-Review

- Spec coverage: The plan implements a read-only graph wrapper, graph nodes/edges/evidence, tests, docs, and verification without migrating tabs.
- Placeholder scan: No task contains open-ended placeholder work. GUID values in the snippets are examples; the implementation step explicitly requires fresh Unity GUIDs.
- Type consistency: Graph type names, builder names, node/edge enums, and tests use the same identifiers across tasks.
