using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Features.Characters;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class PyralisAuthoringSmokeTests : PyralisEditorTestSupport
    {
        [Test]
        public void ContractRegistry_SmokeResolvesStableFeatureContract()
        {
            Assert.That(ResolvedAuthoringContractRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            ResolvedAuthoringContract contract =
                ResolvedAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.StableId, Is.EqualTo("feature.actor.traversal.topdown-hop"));
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void IntentProjection_SmokeUsesCapabilityDescriptors()
        {
            PyralisAuthoringIntentModel model = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringCapability.Movement | AuthoringCapability.Input,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone));

            Assert.That(model.Summary, Does.Contain("Active focus"));
            Assert.That(model.Recommendations.Select(row => row.Fact.StableId), Does.Contain("intent.2d-top-down-plane"));
            Assert.That(model.Recommendations.Any(row => row.Fact.SourceKind == PyralisAuthoringFactSourceKind.RuntimeCapability), Is.True);
        }

        [Test]
        public void SetupFlow_SmokeEmptyBootstrapReportsFirstBlockingStep()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);

            Assert.That(report.FirstBlockingStep.Label, Is.EqualTo("Assign Session Definition"));
            Assert.That(report.FirstBlockingStep.Status, Is.EqualTo(PyralisSetupFlowStepStatus.Missing));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetupGraph_SmokePawnRouteCreatesMovementProof()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay }
            };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);

            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.True);
            Assert.That(graph.TryFindNode("proof.1p-pawn-movement", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Proof));
            Assert.That(graph.Edges.Any(edge =>
                edge.ToNodeId == "proof.1p-pawn-movement"
                && edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof), Is.True);

            Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void SetupGraph_SmokeTabletopRouteStaysNoPawn()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.BoardCardTabletop }
            };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);

            Assert.That(graph.RouteAnalysis.RequiresPawn, Is.False);
            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.BoardCardTabletop), Is.True);
            Assert.That(graph.TryFindNode("proof.board-card-action", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Proof));

            Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void OverviewProjection_SmokeReadsGraphNextAction()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model, Is.Not.Null);
            Assert.That(model.DoNow.Count, Is.GreaterThan(0));
            Assert.That(model.BestNextAction, Is.Not.Empty);

            Object.DestroyImmediate(root);
        }
    }
}
