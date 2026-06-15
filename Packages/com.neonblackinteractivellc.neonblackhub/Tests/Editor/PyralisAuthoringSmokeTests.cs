using System.Collections.Generic;
using System.IO;
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
        public void GameplaySeams_SmokeKeepSingleRuntimeOwners()
        {
            string gameplayRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Packages", "com.neonblackinteractivellc.neonblackhub", "Members", "Pyralis", "Gameplay"));
            string sessionSource = File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "SessionDefinition.cs"));
            string pawnSource = File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "PawnDefinition.cs"));
            string runtimeContextSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Platform", "Composition", "GameplayRuntimeContext.cs"));
            string spawnSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "Runtime", "Shared", "Services", "ParticipantSpawnService.cs"));
            string spawnerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Spawning", "3D", "PlayerSpawner.cs"));
            string bootstrapSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "GameplaySessionBootstrap.cs"));
            string playfieldSource = File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "PlayfieldProfile.cs"));
            string pawn2DMovementSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "2D", "Pawn2DMovementComponent.cs"));
            string cameraRigSource = File.ReadAllText(Path.Combine(gameplayRoot, "Presentation", "Camera", "CinemachineCameraRigController.cs"));

            Assert.That(sessionSource, Does.Not.Contain("defaultInputProfile"));
            Assert.That(pawnSource, Does.Not.Contain("defaultInputProfile"));
            Assert.That(runtimeContextSource, Does.Not.Contain("DefaultInputProfile"));
            Assert.That(runtimeContextSource, Does.Not.Contain("DefaultInputActions"));
            Assert.That(spawnSource, Does.Not.Contain("GetMethod("));
            Assert.That(spawnSource, Does.Contain("IPawnRuntimeServicesReceiver"));
            Assert.That(spawnerSource, Does.Not.Contain("playerPrefab"));
            Assert.That(spawnerSource, Does.Not.Contain("currentPlayer"));
            Assert.That(spawnerSource, Does.Contain("ParticipantSpawnService"));
            Assert.That(bootstrapSource, Does.Not.Contain("TrySetMember"));
            Assert.That(bootstrapSource, Does.Not.Contain("System.Reflection"));
            Assert.That(playfieldSource, Does.Contain("IPlayfieldBoundsProvider"));
            Assert.That(playfieldSource, Does.Contain("AuthoringCapability.Movement"));
            Assert.That(pawn2DMovementSource.IndexOf("TryGetPlayfieldBounds2D", System.StringComparison.Ordinal), Is.LessThan(pawn2DMovementSource.IndexOf("TryGetCameraBounds", System.StringComparison.Ordinal)));
            Assert.That(cameraRigSource, Does.Contain("ICameraBoundsProvider"));
        }

        [Test]
        public void PlayfieldProfile_SmokeProvidesLegalMovementBounds()
        {
            PlayfieldProfile playfield = ScriptableObject.CreateInstance<PlayfieldProfile>();
            playfield.clampToBounds = true;
            playfield.allowScreenWrap = true;
            playfield.minBounds = new Vector2(-5f, -3f);
            playfield.maxBounds = new Vector2(5f, 3f);

            Assert.That(playfield.TryGetPlayfieldBounds2D(0.5f, out PlayfieldBounds2D bounds), Is.True);
            Assert.That(bounds.Min, Is.EqualTo(new Vector2(-4.5f, -2.5f)));
            Assert.That(bounds.Max, Is.EqualTo(new Vector2(4.5f, 2.5f)));
            Assert.That(bounds.AllowScreenWrap, Is.True);

            Object.DestroyImmediate(playfield);
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
        public void SetupGraph_SmokePawnRouteCreatesMovementProofAndBlocksOnPawnValidation()
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
            Assert.That(proofNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(proofNode.SourceOrigin, Is.EqualTo(PyralisAuthoringGraphSourceOrigin.GrammarFallback));
            Assert.That(graph.Edges.Any(edge =>
                edge.ToNodeId == "proof.1p-pawn-movement"
                && edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof), Is.True);
            Assert.That(graph.Nodes.Any(node =>
                node.SourceKind == PyralisAuthoringGraphSourceKind.RuntimeValidation
                && node.Guidance.Contains("pawn prefab")), Is.True);
            Assert.That(graph.Edges.Any(edge =>
                edge.FromNodeId == "proof.1p-pawn-movement"
                && edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy
                && edge.ToNodeId.StartsWith("runtimevalidation.", System.StringComparison.Ordinal)), Is.True);
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph)
                .Any(row => row.Label == "Pawn / No Pawn" && row.IsMissing && row.Message.Contains("pawn prefab")), Is.True);

            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokeProofReadinessIsBlockedByMissingRequiredSetup()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);

            PyralisAuthoringGraphNode proofNode = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            Assert.That(proofNode, Is.Not.Null);
            Assert.That(proofNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(graph.Edges.Any(edge =>
                edge.FromNodeId == proofNode.StableId
                && edge.ToNodeId == "session.definition"
                && edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy), Is.True);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetupGraph_SmokeIntentFocusGuidesPawnBeforePawnExists()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);

            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.True);
            Assert.That(graph.TryFindNode("route.shape", out PyralisAuthoringGraphNode routeShape), Is.True);
            Assert.That(routeShape.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.RouteShape));
            Assert.That(routeShape.Label, Is.EqualTo("Participant With Pawn"));
            Assert.That(routeShape.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(routeShape.Guidance, Does.Contain("PawnDefinition"));
            Assert.That(graph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(pawnNode.BlockingReason, Does.Contain("ParticipantDefinition.defaultPawn"));
            Assert.That(graph.TryFindNode("proof.1p-pawn-movement", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(graph.Edges.Any(edge =>
                edge.FromNodeId == "proof.1p-pawn-movement"
                && edge.ToNodeId == "pawn.definition"
                && edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy), Is.True);
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildIntentFocusSummary(graph), Does.Contain("pawn movement/control"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildFirstProofPrioritySummary(graph), Does.Contain("fix this first"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildProofBlockerRows(graph)
                .Any(row => row.To != null && row.To.StableId == "pawn.definition"), Is.True);
            IReadOnlyList<PyralisAuthoringRouteStepRow> routeSteps = PyralisAuthoringSetupGraphProjection.BuildRouteStepRows(graph);
            Assert.That(routeSteps.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(routeSteps[0].Role, Is.EqualTo(PyralisAuthoringRouteStepRole.DoThisFirst));
            Assert.That(routeSteps.Any(row => row.StableId == "route.shape"
                && (row.Role == PyralisAuthoringRouteStepRole.BlocksProof || row.Role == PyralisAuthoringRouteStepRole.RouteContext)), Is.True);
            Assert.That(routeSteps.Any(row => row.StableId == "pawn.definition"
                && (row.Role == PyralisAuthoringRouteStepRole.DoThisFirst || row.Role == PyralisAuthoringRouteStepRole.BlocksProof)), Is.True);
            Assert.That(routeSteps.Any(row => row.StableId == "proof.1p-pawn-movement"
                && row.Role == PyralisAuthoringRouteStepRole.ProofTarget), Is.True);
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(graph), Does.Contain("Participant With Pawn"));

            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokeCurrentSetupGraphDoesNotTreatIntentAsAuthoredTruth()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph currentGraph = PyralisAuthoringSetupGraphBuilder.Build(session);
            PyralisAuthoringSetupGraph intentGraph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);

            Assert.That(currentGraph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.False);
            Assert.That(currentGraph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode currentPawn), Is.True);
            Assert.That(currentPawn.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildProofBlockerRows(currentGraph)
                .Any(row => row.To != null && row.To.StableId == "pawn.definition"), Is.False);

            Assert.That(intentGraph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.True);
            Assert.That(intentGraph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode intentPawn), Is.True);
            Assert.That(intentPawn.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildProofBlockerRows(intentGraph)
                .Any(row => row.To != null && row.To.StableId == "pawn.definition"), Is.True);

            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void IntentProjection_SmokeInputAloneDoesNotInferCameraRoute()
        {
            RuntimeCapabilityFamily[] families = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                AuthoringCapability.Input,
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime);

            Assert.That(families.Any(family => family == RuntimeCapabilityFamily.ActionTargeting), Is.True);
            Assert.That(families.Any(family => family == RuntimeCapabilityFamily.CameraInput), Is.False);

            PyralisAuthoringCapabilityDescriptor descriptor =
                PyralisAuthoringCapabilityDescriptorRegistry.FindBestForCapability(
                    AuthoringCapability.Input,
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);
            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor.Family, Is.Not.EqualTo(RuntimeCapabilityFamily.CameraInput));
        }

        [Test]
        public void SetupRouteAnalysis_SmokeInputProfileDoesNotDefineCameraRoute()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            session.defaultGameMode = mode;
            participant.inputProfile = inputProfile;
            session.defaultParticipants = new[] { participant };
            participant.inputProfile = inputProfile;

            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.Build(session);

            Assert.That(route.CapabilityFamilies.Any(family => family == RuntimeCapabilityFamily.CameraInput), Is.False);
            Assert.That(route.CapabilityFamilies.Any(family => family == RuntimeCapabilityFamily.PlatformCore), Is.True);

            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokePlatformCoreOnlyReadsAsFoundation()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            session.defaultGameMode = mode;

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);

            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily != RuntimeCapabilityFamily.PlatformCore), Is.False);
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildIntentFocusSummary(graph), Does.Contain("setup foundation only"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(graph), Does.Contain("Setup Foundation"));

            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupRouteAnalysis_SmokeFeatureModulesDoNotInferCapabilitiesFromDisplayName()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            FeatureModuleDefinition module = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            module.moduleId = "project.uncontracted-combat-module";
            module.displayName = "Uncontracted Combat Module";
            module.authoringCategory = "combat";
            mode.requiredFeatureModules = new[] { module };
            session.defaultGameMode = mode;

            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.Build(session);

            Assert.That(route.CapabilityFamilies.Any(family => family == RuntimeCapabilityFamily.Combat), Is.False);
            Assert.That(route.CapabilityFamilies.Any(family => family == RuntimeCapabilityFamily.PlatformCore), Is.True);

            Object.DestroyImmediate(module);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokeTabletopRouteStaysNoPawn()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            BoardDefinition board = ScriptableObject.CreateInstance<BoardDefinition>();
            mode.boardDefinition = board;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);

            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.BoardCardTabletop), Is.True);
            Assert.That(graph.TryFindNode("route.shape", out PyralisAuthoringGraphNode routeShape), Is.True);
            Assert.That(routeShape.Label, Is.EqualTo("Participant Without Pawn"));
            Assert.That(routeShape.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(graph), Does.Contain("Participant Without Pawn"));
            Assert.That(graph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(pawnNode.Guidance, Does.Contain("No-pawn route"));
            Assert.That(graph.TryFindNode("proof.board-card-action", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Proof));

            Object.DestroyImmediate(board);
            Object.DestroyImmediate(participant);
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
            Assert.That(model.DoNow.Count, Is.LessThanOrEqualTo(3));
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
