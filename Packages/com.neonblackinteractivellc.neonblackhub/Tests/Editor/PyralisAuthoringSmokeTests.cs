using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Scoring;
using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Presentation.Animation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public void ContractRegistry_SmokeReflectsCodeProvenRequirements()
        {
            ResolvedAuthoringContract contract =
                ResolvedAuthoringContractRegistry.FindByType(typeof(ContractReflectionRequirementFixture));

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain(typeof(IContractReflectionRequirementFixture).FullName));
            Assert.That(contract.RequiredComponentNames, Does.Contain(typeof(RectTransform).FullName));
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
            Assert.That(model.Recommendations.Any(row => row.Fact.Kind == PyralisAuthoringFactKind.RuntimeCapability), Is.True);
        }

        [Test]
        public void IntentAxioms_SmokeComeFromAuthoringContractVocabulary()
        {
            System.Collections.Generic.IReadOnlyList<AuthoringWorldAxiomGroup> groups =
                AuthoringWorldAxiomRegistry.GetIntentGroups();

            Assert.That(groups.Select(group => group.DisplayName), Does.Contain("Dimensionality"));
            Assert.That(groups.Select(group => group.DisplayName), Does.Contain("Physics Gravity"));
            Assert.That(groups.Select(group => group.DisplayName), Does.Contain("Sequence Timeline"));
            Assert.That(groups.Select(group => group.DisplayName), Does.Contain("Spatial Topology"));
            Assert.That(AuthoringWorldAxiomRegistry.HasCompleteCoreAxioms(
                AuthoringWorldAxiom.Dimensions2D
                | AuthoringWorldAxiom.GravityNone
                | AuthoringWorldAxiom.Realtime
                | AuthoringWorldAxiom.BoundedSpace), Is.True);
        }

        [Test]
        public void CapabilityDescriptor_SmokeDoesNotMergeFallbackSetupIntoContractDescriptors()
        {
            PyralisAuthoringCapabilityDescriptor descriptor =
                PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(RuntimeCapabilityFamily.CharacterPawnGameplay);

            Assert.That(descriptor, Is.Not.Null);
            Assert.That(
                descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract
                || descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Reflection,
                Is.True);
            Assert.That(descriptor.RequiredSetup, Does.Not.Contain("ParticipantDefinition"));
            Assert.That(descriptor.RequiredSetup, Does.Not.Contain("PawnDefinition"));
            Assert.That(
                descriptor.AssignmentFields.Any(field => field.Contains("ParticipantDefinition.defaultPawn")),
                Is.False);
        }

        [Test]
        public void FeatureModuleDefinition_SmokeValidatesRequiredUnityComponentsBeyondMonoBehaviours()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "test.required-box-collider";

            GameObject actor = new GameObject("Actor With Box Collider");
            actor.AddComponent<BoxCollider>();

            System.Collections.Generic.List<string> issues =
                definition.GetActorCompatibilityIssues(actor, ActorPresentationMode.Sprite2D);

            Assert.That(issues.Any(issue => issue.Contains("BoxCollider")), Is.False);

            Object.DestroyImmediate(actor);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void SceneEvidence_SmokeFindsTypedRuntimeAndSurfaceComponents()
        {
            GameObject root = new GameObject("Scene Evidence Root");
            try
            {
                root.AddComponent<GameplaySessionBootstrap>();
                root.AddComponent<ParticipantScoreService>();
                root.AddComponent<ProjectileLauncher2D>();
                root.AddComponent<TabletopBoardGridPresenter>();
                root.AddComponent<Canvas>();

                PyralisAuthoringSceneEvidence evidence =
                    PyralisAuthoringSceneEvidence.Build(root.GetComponent<GameplaySessionBootstrap>());

                Assert.That(evidence.HasScoreService, Is.True);
                Assert.That(evidence.ScoreServiceCount, Is.EqualTo(1));
                Assert.That(evidence.HasProjectileLauncher, Is.True);
                Assert.That(evidence.HasTabletopGridPresenter, Is.True);
                Assert.That(evidence.HasCanvas, Is.True);
                Assert.That(evidence.CanvasCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
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
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            participant.defaultPawn = pawn;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);

            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.True);
            Assert.That(graph.TryFindNode("proof.1p-pawn-movement", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Proof));
            Assert.That(graph.Edges.Any(edge =>
                edge.ToNodeId == "proof.1p-pawn-movement"
                && edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof), Is.True);

            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokeTabletopRouteStaysNoPawn()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            BoardDefinition board = ScriptableObject.CreateInstance<BoardDefinition>();
            mode.boardDefinition = board;
            session.defaultGameMode = mode;

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);

            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.BoardCardTabletop), Is.True);
            Assert.That(graph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(pawnNode.Guidance, Does.Contain("No-pawn route"));
            Assert.That(graph.TryFindNode("proof.board-card-action", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Proof));

            Object.DestroyImmediate(board);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
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

        [Test]
        public void InputProfileSync_SmokeMapsUnityInputActionsToGameplayRows()
        {
            InputActionAsset actions = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap player = actions.AddActionMap("Player");
            player.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            player.AddAction("Attack", InputActionType.Button, expectedControlLayout: "Button");
            player.AddAction("Emote", InputActionType.Button, expectedControlLayout: "Button");

            InputProfile profile = ScriptableObject.CreateInstance<InputProfile>();
            profile.actions = actions;
            profile.primaryActionMap = "Player";
            profile.actionBindings = System.Array.Empty<GameplayInputActionBinding>();

            bool changed = InputProfileInputActionSync.SyncFromAssignedActions(profile, includeCustomActions: true, out string summary);

            Assert.That(changed, Is.True, summary);
            Assert.That(profile.FindBinding(GameplayInputActionRole.Move)?.actionName, Is.EqualTo("Move"));
            Assert.That(profile.FindBinding(GameplayInputActionRole.Move)?.requiredForGameplay, Is.True);
            Assert.That(profile.FindBinding(GameplayInputActionRole.AttackPrimary)?.actionName, Is.EqualTo("Attack"));
            Assert.That(profile.FindCustomBinding("Emote")?.actionName, Is.EqualTo("Emote"));

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actions);
        }
    }

    internal interface IContractReflectionRequirementFixture
    {
    }

    [RequireComponent(typeof(RectTransform))]
    [AuthoringContract(
        Capability = AuthoringCapability.Setup,
        Relevance = "Editor smoke fixture for reflected contract requirements.")]
    internal sealed class ContractReflectionRequirementFixture : MonoBehaviour, IContractReflectionRequirementFixture
    {
    }

    [AuthoringContract(
        ModuleId = "test.required-box-collider",
        Capability = AuthoringCapability.Setup,
        Relevance = "Editor smoke fixture for required Unity component validation.",
        RequiredComponents = new[] { typeof(BoxCollider) })]
    internal sealed class RequiredBoxColliderContractFixture
    {
    }
}
