using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Input;
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
            Assert.That(contract.CapabilityPath, Is.EqualTo("Movement/Traversal/FakeGravityJump"));
            Assert.That(contract.RoleTags, Does.Contain("FakeGravityJump"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void IntentProjection_SmokeCapabilityIngredientTogglesAreUnique()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime);

            string[] stableIds = descriptors.Select(descriptor => descriptor.StableId).ToArray();
            Assert.That(stableIds.Distinct().Count(), Is.EqualTo(stableIds.Length));
            Assert.That(descriptors.Any(descriptor =>
                descriptor.Capability.HasFlag(AuthoringCapability.Input)
                && descriptor.CapabilityPath.StartsWith("Core Setup/Input", System.StringComparison.Ordinal)), Is.True);
            Assert.That(descriptors.Any(descriptor =>
                descriptor.Capability.HasFlag(AuthoringCapability.Movement)
                && descriptor.CapabilityPath.StartsWith("Movement", System.StringComparison.Ordinal)), Is.True);
            Assert.That(descriptors.Any(descriptor =>
                descriptor.Capability.HasFlag(AuthoringCapability.Camera)
                && descriptor.CapabilityPath.StartsWith("World & Meta/Camera", System.StringComparison.Ordinal)), Is.True);
        }

        [Test]
        public void IntentProjection_SmokeUsesReflectedContractDescriptorPaths()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringCapabilityDescriptor fakeGravityJump = descriptors.FirstOrDefault(descriptor =>
                descriptor != null
                && string.Equals(descriptor.StableId, "feature.actor.traversal.topdown-hop", System.StringComparison.Ordinal));

            Assert.That(fakeGravityJump, Is.Not.Null);
            Assert.That(fakeGravityJump.CapabilityPath, Is.EqualTo("Movement/Traversal/FakeGravityJump"));
            Assert.That(fakeGravityJump.RoleTags, Does.Contain("FakeGravityJump"));
            Assert.That(fakeGravityJump.SelectableIntent, Is.True);
        }

        [Test]
        public void IntentProjection_SmokeInfersDomainPathsAndHidesInterfaceContracts()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime);

            Assert.That(descriptors.Any(descriptor =>
                descriptor.CapabilityPath.StartsWith("RPG & Narrative", System.StringComparison.Ordinal)), Is.True);
            Assert.That(descriptors.Any(descriptor =>
                descriptor.CapabilityPath.StartsWith("Strategy & Board", System.StringComparison.Ordinal)), Is.True);
            Assert.That(descriptors.Any(descriptor =>
                descriptor.CapabilityPath.StartsWith("Interaction", System.StringComparison.Ordinal)), Is.True);
            Assert.That(descriptors.Any(descriptor =>
                descriptor.DisplayName.Contains("IFeatureModuleRuntime", System.StringComparison.Ordinal)), Is.False);
        }

        [Test]
        public void SceneReadiness_SmokeRequiresAuthoredFeatureHostForEnabledPawnModules()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            GameObject pawnPrefab = new GameObject("Pawn Prefab");
            pawnPrefab.AddComponent<PawnRoot>();
            pawnPrefab.AddComponent<SmokePawnMotor>();
            pawnPrefab.AddComponent<SmokePawnInput>();
            pawnPrefab.AddComponent<SmokePawnPresentation>();

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            FeatureModuleDefinition module = ScriptableObject.CreateInstance<FeatureModuleDefinition>();

            module.moduleId = "feature.test";
            module.enabledByDefault = true;
            pawn.pawnPrefab = pawnPrefab;
            pawn.featureModules = new[] { module };
            participant.defaultPawn = pawn;
            session.defaultParticipants = new[] { participant };
            SetPrivateField(bootstrap, "sessionDefinition", session);

            PyralisSceneReadinessReport report = PyralisSceneReadinessValidator.BuildReport(bootstrap);

            Assert.That(report.RequiredIssues.Any(issue =>
                issue.Contains("missing ActorFeatureHost", System.StringComparison.Ordinal)), Is.True);

            Object.DestroyImmediate(module);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(pawnPrefab);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void MapProjection_DoesNotLetSceneSurfaceIssuesContaminateGameplayRootRow()
        {
            PyralisAuthoringGraphNode gameplayRoot = new PyralisAuthoringGraphNode(
                "bootstrap.root",
                "Gameplay Root",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                PyralisAuthoringGraphEvidenceState.Ready,
                guidance: "Gameplay Root is assigned.");
            PyralisAuthoringGraphNode sceneSurfaces = new PyralisAuthoringGraphNode(
                "scene.surfaces",
                "Scene Surfaces",
                PyralisAuthoringGraphNodeKind.SceneSurface,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "One scene surface is missing.");
            PyralisAuthoringGraphNode missingUi = new PyralisAuthoringGraphNode(
                "scene.ui",
                "UI / HUD / Menus",
                PyralisAuthoringGraphNodeKind.SceneSurface,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "No Canvas");
            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                null,
                null,
                new[] { gameplayRoot, sceneSurfaces, missingUi },
                new[]
                {
                    new PyralisAuthoringGraphEdge("bootstrap.root", "scene.ui", PyralisAuthoringGraphEdgeKind.RelatesTo, "scene surface"),
                    new PyralisAuthoringGraphEdge("bootstrap.root", "scene.surfaces", PyralisAuthoringGraphEdgeKind.RelatesTo, "scene surface summary")
                });

            IReadOnlyList<PyralisAuthoringSetupGraphRow> rows = PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph);
            PyralisAuthoringSetupGraphRow rootRow = rows.First(row => row.Label == "Gameplay Root");
            PyralisAuthoringSetupGraphRow sceneRow = rows.First(row => row.Label == "Scene Surfaces");

            Assert.That(rootRow.EffectiveEvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(rootRow.Message, Does.Not.Contain("No Canvas"));
            Assert.That(sceneRow.EffectiveEvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(sceneRow.Message, Does.Contain("One scene surface"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildMapSceneSetupIssueRows(graph).Select(row => row.NodeId), Does.Contain("scene.ui"));
        }

        [Test]
        public void SetupGraphJsonExport_SmokeSerializesGraphAndTabProjections()
        {
            PyralisAuthoringGraphNode missingSetup = new PyralisAuthoringGraphNode(
                "setup.session",
                "Session",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "Assign a SessionDefinition.");
            PyralisAuthoringGraphNode proof = new PyralisAuthoringGraphNode(
                "proof.1p",
                "1P Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.ProofVocabulary,
                PyralisAuthoringGraphEvidenceState.Missing);
            PyralisAuthoringGraphNode sceneIssue = new PyralisAuthoringGraphNode(
                "validation.input-profile",
                "Input Profile",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "Assign the participant input profile.");
            PyralisAuthoringGraphNode graphIssue = new PyralisAuthoringGraphNode(
                "graph.assignment.coverage",
                "Graph Assignment Coverage",
                PyralisAuthoringGraphNodeKind.AssignmentField,
                PyralisAuthoringGraphSourceKind.GrammarRegistry,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "Graph projection lacks assignment coverage.");

            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                null,
                null,
                new[] { missingSetup, proof, sceneIssue, graphIssue },
                new[]
                {
                    new PyralisAuthoringGraphEdge("proof.1p", "setup.session", PyralisAuthoringGraphEdgeKind.BlockedBy, "missing setup"),
                    new PyralisAuthoringGraphEdge("proof.1p", "graph.assignment.coverage", PyralisAuthoringGraphEdgeKind.BlockedBy, "graph coverage")
                });

            string mapJson = PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);
            string hygieneJson = PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(
                graph,
                new[]
                {
                    PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Platform/Session/HeavyRuntime.cs",
                        "using NeonBlack.Gameplay.Features.Input; using NeonBlack.Gameplay.Features.Combat; class HeavyRuntime { void Tick() { UnityEngine.Object.FindAnyObjectByType<UnityEngine.Transform>(); } }")
                });
            string noRouteHygieneJson = PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(
                null,
                new[]
                {
                    PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                        "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Platform/Session/NoRoutePressure.cs",
                        "using NeonBlack.Gameplay.Features.Input; using NeonBlack.Gameplay.Features.Combat; class NoRoutePressure { }")
                });
            string routeProofTraceJson = PyralisAuthoringSetupGraphJsonExporter.ToRouteProofTraceJson(graph);

            Assert.That(mapJson, Does.Contain("pyralis.authoring.mapSnapshot.v1"));
            Assert.That(mapJson, Does.Contain("\"view\": \"Map\""));
            Assert.That(mapJson, Does.Contain("\"nodes\""));
            Assert.That(mapJson, Does.Contain("\"edges\""));
            Assert.That(mapJson, Does.Contain("\"summary\""));
            Assert.That(mapJson, Does.Contain("\"mapRowCount\""));
            Assert.That(mapJson, Does.Contain("\"sceneSetupIssueCount\""));
            Assert.That(mapJson, Does.Contain("\"currentRoute\""));
            Assert.That(mapJson, Does.Contain("\"mapRows\""));
            Assert.That(mapJson, Does.Contain("\"mapConnections\""));
            Assert.That(mapJson, Does.Contain("\"sceneSetupIssues\""));
            Assert.That(mapJson, Does.Contain("validation.input-profile"));
            Assert.That(mapJson, Does.Not.Contain("\"hygieneSections\""));

            Assert.That(hygieneJson, Does.Contain("pyralis.authoring.hygieneSnapshot.v2"));
            Assert.That(hygieneJson, Does.Contain("\"view\": \"Hygiene\""));
            Assert.That(hygieneJson, Does.Contain("\"graphContext\""));
            Assert.That(hygieneJson, Does.Contain("\"graphSummary\""));
            Assert.That(hygieneJson, Does.Contain("\"summary\""));
            Assert.That(hygieneJson, Does.Contain("\"hygieneRowCount\""));
            Assert.That(hygieneJson, Does.Contain("\"cleanupFocusCount\""));
            Assert.That(hygieneJson, Does.Contain("\"hygieneSections\""));
            Assert.That(hygieneJson, Does.Contain("\"hygieneRows\""));
            Assert.That(hygieneJson, Does.Contain("\"proofBlockers\""));
            Assert.That(hygieneJson, Does.Contain("\"sourceOriginCounts\""));
            Assert.That(hygieneJson, Does.Contain("\"dependencyPressureSummary\""));
            Assert.That(hygieneJson, Does.Contain("\"pressureKindCounts\""));
            Assert.That(hygieneJson, Does.Contain("\"cleanupFocus\""));
            Assert.That(hygieneJson, Does.Contain("\"watchList\""));
            Assert.That(hygieneJson, Does.Contain("\"dependencyPressure\""));
            Assert.That(hygieneJson, Does.Contain("\"pressureKind\""));
            Assert.That(hygieneJson, Does.Contain("\"reviewHint\""));
            Assert.That(hygieneJson, Does.Contain("\"localComponentLookupCount\""));
            Assert.That(hygieneJson, Does.Contain("\"broadUnityDiscoveryCount\""));
            Assert.That(hygieneJson, Does.Contain("\"contractSourcePressure\""));
            Assert.That(hygieneJson, Does.Not.Contain("validation.input-profile"));
            Assert.That(hygieneJson, Does.Not.Contain("setup.session"));
            Assert.That(hygieneJson, Does.Contain("graph.assignment.coverage"));
            Assert.That(hygieneJson, Does.Not.Contain("\"mapRows\""));

            Assert.That(noRouteHygieneJson, Does.Contain("pyralis.authoring.hygieneSnapshot.v2"));
            Assert.That(noRouteHygieneJson, Does.Contain("\"hasGraph\": false"));
            Assert.That(noRouteHygieneJson, Does.Contain("\"graphName\": \"No active setup graph\""));
            Assert.That(noRouteHygieneJson, Does.Not.Contain("\"routeName\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"dependencyPressureSummary\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"dependencyPressure\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"nodeCount\": 0"));
            Assert.That(noRouteHygieneJson, Does.Not.Contain("\"mapRows\""));

            Assert.That(routeProofTraceJson, Does.Contain("pyralis.authoring.routeProofTrace.v1"));
            Assert.That(routeProofTraceJson, Does.Contain("\"view\": \"RouteProofTrace\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"summary\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"currentActionLabel\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"criticalPathCount\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"proof\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"currentAction\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"orderedSteps\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"criticalPath\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"proofEnhancers\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"canWait\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"proofBlockers\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"proofSupport\""));
            Assert.That(routeProofTraceJson, Does.Contain("\"diagnosticQuestions\""));
            Assert.That(routeProofTraceJson, Does.Contain("setup.session"));
            Assert.That(routeProofTraceJson, Does.Not.Contain("validation.input-profile"));
            Assert.That(routeProofTraceJson, Does.Not.Contain("\"mapRows\""));
            Assert.That(routeProofTraceJson, Does.Not.Contain("\"hygieneSections\""));
        }

        [Test]
        public void SetupGraph_SmokePawnIntentBlocksWhenParticipantInputProfileIsMissing()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            CameraRigProfile cameraProfile = ScriptableObject.CreateInstance<CameraRigProfile>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<SmokePawnMotor>();
            prefab.AddComponent<SmokePawnPresentation>();
            prefab.AddComponent<SmokePawnInput>();
            pawn.pawnPrefab = prefab;
            participant.defaultPawn = pawn;
            session.defaultGameMode = mode;
            mode.cameraRigProfile = cameraProfile;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);

            Assert.That(graph.TryFindNode("route.participant-input-profile", out _), Is.False);
            Assert.That(graph.TryFindNode("dependency.participant.input-profile", out PyralisAuthoringGraphNode inputNode), Is.True);
            Assert.That(inputNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(inputNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.AssignmentField));
            Assert.That(inputNode.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.Reflection));
            Assert.That(inputNode.Guidance, Does.Contain("ParticipantDefinition.inputProfile"));
            Assert.That(inputNode.AssignmentFields, Does.Contain("ParticipantDefinition.inputProfile"));
            Assert.That(inputNode.NativeAction.HasValue, Is.True);
            Assert.That(inputNode.NativeAction.Value.Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));
            Assert.That(string.Join(" ", inputNode.NativeSetup), Does.Not.Contain("add/remove Gameplay Action rows"));
            Assert.That(string.Join(" ", inputNode.NativeSetup), Does.Not.Contain("SessionDefinition or ParticipantDefinition"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph).CurrentAction.Node.StableId,
                Is.EqualTo("dependency.participant.input-profile"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildOverviewIssues(graph, session)
                .Any(issue => issue.Label == "Assign Input Profile" && issue.Lane == PyralisAuthoringOverviewLane.DoNow), Is.True);
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph).CriticalPath
                .Any(row => row.Node != null && row.Node.StableId == "dependency.participant.input-profile"), Is.True);
            Assert.That(graph.TryFindNode("route.camera-focus", out PyralisAuthoringGraphNode cameraFocusNode), Is.True);
            Assert.That(cameraFocusNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.CandidateDetected));
            Assert.That(cameraFocusNode.Guidance, Does.Contain("PawnCameraTarget"));

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(cameraProfile);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokePlayfieldCameraFocusDoesNotRequirePawnTarget()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            CameraRigProfile cameraProfile = ScriptableObject.CreateInstance<CameraRigProfile>();
            cameraProfile.focusMode = CameraRigProfile.CameraFocusMode.PlayfieldCenter;
            mode.cameraRigProfile = cameraProfile;
            session.defaultGameMode = mode;

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);

            Assert.That(graph.TryFindNode("route.camera-focus", out PyralisAuthoringGraphNode cameraFocusNode), Is.True);
            Assert.That(cameraFocusNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(cameraFocusNode.Guidance, Does.Contain("Playfield Center"));
            Assert.That(cameraFocusNode.Guidance, Does.Not.Contain("PawnCameraTarget"));

            Object.DestroyImmediate(cameraProfile);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void RouteProofTrace_SmokeTranslatesSprite2DPawnInterfacesIntoConcreteSetupCards()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            pawn.pawnPrefab = prefab;
            participant.defaultPawn = pawn;
            participant.inputProfile = inputProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Session
                | AuthoringCapability.Input
                | AuthoringCapability.Movement
                | AuthoringCapability.Participants
                | AuthoringCapability.KineticMotor2D,
                AuthoringWorldAxiom.Dimensions2D
                | AuthoringWorldAxiom.GravityNone
                | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);
            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);

            Assert.That(route.CurrentAction, Is.Not.Null);
            Assert.That(route.CurrentAction.Label, Is.EqualTo("Add Motor2D"));
            Assert.That(route.CurrentAction.StableId, Is.EqualTo("pawn.definition"));
            Assert.That(route.CriticalPath.Any(row => row.Label == "Add Motor2D"), Is.True);
            Assert.That(route.CriticalPath.Any(row => row.UnityActionLabel.Contains("Motor2D")), Is.True);
            PyralisAuthoringRouteStepRow routeShape = route.CriticalPath
                .FirstOrDefault(row => row.StableId == "route.shape");
            Assert.That(routeShape, Is.Not.Null);
            Assert.That(routeShape.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(routeShape.Message, Does.Not.Contain("Add Motor2D"));
            Assert.That(routeShape.NativeSetup.Any(step => step.Contains("Add Motor2D")), Is.False);
            Assert.That(routeShape.AssignmentFields, Does.Not.Contain("PawnDefinition.pawnPrefab"));
            Assert.That(routeShape.AssignmentFields, Does.Not.Contain("ParticipantDefinition.inputProfile"));
            Assert.That(route.CriticalPath.Any(row => row.Label == "Scene And Prefab Readiness"), Is.False);
            Assert.That(route.ProofEnhancers.Any(row => row.Node.SourceKind == PyralisAuthoringGraphSourceKind.AuthoringContract), Is.False);
            Assert.That(route.ProofEnhancers.Select(row => row.Label).Distinct().Count(), Is.EqualTo(route.ProofEnhancers.Count));
            Assert.That(route.Proof.NativeSetup.Length, Is.EqualTo(1));
            Assert.That(route.Proof.AssignmentFields.Length, Is.EqualTo(0));
            Assert.That(route.Proof.CustomizationMoments.Length, Is.EqualTo(0));
            Assert.That(route.ProofSupport.Any(row => row.FromLabel.Contains("Top Down Hop")), Is.False);
            Assert.That(route.ProofSupport.Any(row => row.FromLabel.Contains("Pawn Camera Target")), Is.False);

            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToRouteProofTraceJson(graph);
            Assert.That(traceJson, Does.Contain("Add Motor2D"));
            Assert.That(traceJson, Does.Contain("\"currentActionLabel\": \"Add Motor2D\""));
            Assert.That(traceJson.Contains("Motor2DInputAdapter") || traceJson.Contains("Pawn2DMovementComponent"), Is.True);
            Assert.That(traceJson, Does.Not.Contain("Scene And Prefab Readiness"));
            Assert.That(traceJson, Does.Not.Contain("Add Lane Motor"));
            Assert.That(traceJson, Does.Not.Contain("the lane motor component"));
            Assert.That(traceJson, Does.Not.Contain("implements IPawnMotor"));
            Assert.That(traceJson, Does.Not.Contain("TopDownHopProfile"));
            Assert.That(traceJson, Does.Not.Contain("Pawn Camera Target"));

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokeBeginnerMovementIntentUsesPawnMovementProof()
        {
            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Session
                | AuthoringCapability.Input
                | AuthoringCapability.Movement
                | AuthoringCapability.Participants
                | AuthoringCapability.KineticMotor2D,
                AuthoringWorldAxiom.Dimensions2D
                | AuthoringWorldAxiom.GravityNone
                | AuthoringWorldAxiom.Realtime
                | AuthoringWorldAxiom.BoundedSpace);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null, intent);

            PyralisAuthoringGraphNode proof = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.StableId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(proof.Label, Does.Contain("Pawn Movement"));
            Assert.That(proof.Label, Does.Not.Contain("Custom Object"));
            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.Combat), Is.False);
            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.GunsProjectiles), Is.False);
            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.True);

            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("bootstrap.root"));
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("session.definition"));
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("mode.definition"));
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("participant.default"));
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("pawn.definition"));
            Assert.That(route.CurrentAction, Is.Not.Null);
            Assert.That(route.CurrentAction.Node.StableId, Is.EqualTo("bootstrap.root"));

            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToRouteProofTraceJson(graph);
            Assert.That(traceJson, Does.Contain("\"orderedSteps\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"bootstrap.root\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"session.definition\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"participant.default\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"proof.1p-pawn-movement\""));
        }

        [Test]
        public void OverviewProjection_SmokeReadsGraphNextAction()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model, Is.Not.Null);
            Assert.That(model.DoNow.Count, Is.GreaterThan(0));
            Assert.That(model.DoNow.Count, Is.LessThanOrEqualTo(3));
            Assert.That(route.CurrentAction, Is.Not.Null);
            Assert.That(model.DoNow[0].Label, Is.EqualTo(route.CurrentAction.Label));
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

        private sealed class SmokePawnMotor : MonoBehaviour, IPawnMotor
        {
            public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile)
            {
            }
        }

        private sealed class SmokePawnPresentation : MonoBehaviour, IPawnPresentationModule
        {
            public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
            {
            }
        }

        private sealed class SmokePawnInput : MonoBehaviour, IPawnInputModule
        {
            public void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile inputProfile)
            {
            }
        }
    }
}
