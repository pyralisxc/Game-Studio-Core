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
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void IntentProjection_SmokeCapabilityIngredientTogglesAreUnique()
        {
            PyralisAuthoringWindow window = ScriptableObject.CreateInstance<PyralisAuthoringWindow>();
            try
            {
                System.Reflection.MethodInfo method = typeof(PyralisAuthoringWindow).GetMethod(
                    "BuildIntentCapabilityGroups",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null);

                Dictionary<string, List<AuthoringCapability>> groups =
                    (Dictionary<string, List<AuthoringCapability>>)method.Invoke(window, null);
                AuthoringCapability[] capabilities = groups.Values.SelectMany(group => group).ToArray();

                Assert.That(capabilities.Count(capability => capability == AuthoringCapability.Movement), Is.EqualTo(1));
                Assert.That(capabilities.Count(capability => capability == AuthoringCapability.Input), Is.EqualTo(1));
                Assert.That(capabilities.Count(capability => capability == AuthoringCapability.Camera), Is.EqualTo(1));
                Assert.That(capabilities.Count(capability => capability == AuthoringCapability.Rpg), Is.EqualTo(1));
                Assert.That(capabilities.Distinct().Count(), Is.EqualTo(capabilities.Length));
                Assert.That(groups.TryGetValue("Core Setup", out List<AuthoringCapability> coreGroup), Is.True);
                Assert.That(coreGroup, Does.Contain(AuthoringCapability.Input));
                Assert.That(coreGroup, Does.Contain(AuthoringCapability.Participants));
                Assert.That(groups.TryGetValue("Actor & Action", out List<AuthoringCapability> actorGroup), Is.True);
                Assert.That(actorGroup, Does.Contain(AuthoringCapability.Combat));
                Assert.That(actorGroup, Does.Contain(AuthoringCapability.CombatSensors));
                Assert.That(actorGroup, Does.Contain(AuthoringCapability.Movement));
                Assert.That(groups.TryGetValue("RPG & Narrative", out List<AuthoringCapability> rpgGroup), Is.True);
                Assert.That(rpgGroup, Does.Contain(AuthoringCapability.Rpg));
                Assert.That(rpgGroup, Does.Contain(AuthoringCapability.Puzzle));
                Assert.That(groups.ContainsKey("Combat Sensors"), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(window);
            }
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

            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                null,
                null,
                new[] { missingSetup, proof, sceneIssue },
                new[] { new PyralisAuthoringGraphEdge("proof.1p", "setup.session", PyralisAuthoringGraphEdgeKind.BlockedBy, "missing setup") });

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
            Assert.That(mapJson, Does.Contain("\"currentRoute\""));
            Assert.That(mapJson, Does.Contain("\"mapRows\""));
            Assert.That(mapJson, Does.Contain("\"mapConnections\""));
            Assert.That(mapJson, Does.Contain("\"sceneSetupIssues\""));
            Assert.That(mapJson, Does.Contain("validation.input-profile"));
            Assert.That(mapJson, Does.Not.Contain("\"hygieneSections\""));

            Assert.That(hygieneJson, Does.Contain("pyralis.authoring.hygieneSnapshot.v1"));
            Assert.That(hygieneJson, Does.Contain("\"view\": \"Hygiene\""));
            Assert.That(hygieneJson, Does.Contain("\"graphSummary\""));
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
            Assert.That(hygieneJson, Does.Not.Contain("\"mapRows\""));

            Assert.That(noRouteHygieneJson, Does.Contain("pyralis.authoring.hygieneSnapshot.v1"));
            Assert.That(noRouteHygieneJson, Does.Contain("\"routeName\": \"No setup route selected\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"dependencyPressureSummary\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"dependencyPressure\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"nodeCount\": 0"));
            Assert.That(noRouteHygieneJson, Does.Not.Contain("\"mapRows\""));

            Assert.That(routeProofTraceJson, Does.Contain("pyralis.authoring.routeProofTrace.v1"));
            Assert.That(routeProofTraceJson, Does.Contain("\"view\": \"RouteProofTrace\""));
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
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);

            Assert.That(graph.TryFindNode("route.participant-input-profile", out PyralisAuthoringGraphNode inputNode), Is.True);
            Assert.That(inputNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(inputNode.Guidance, Does.Contain("ParticipantDefinition.inputProfile"));
            Assert.That(inputNode.AssignmentFields, Does.Contain("ParticipantDefinition.inputProfile"));
            Assert.That(string.Join(" ", inputNode.NativeSetup), Does.Contain("Sync Action Names From Asset"));
            Assert.That(string.Join(" ", inputNode.NativeSetup), Does.Not.Contain("add/remove Gameplay Action rows"));
            Assert.That(string.Join(" ", inputNode.NativeSetup), Does.Not.Contain("SessionDefinition or ParticipantDefinition"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildOverviewIssues(graph, session)
                .Any(issue => issue.Label == "Assign Input Profile" && issue.Lane == PyralisAuthoringOverviewLane.DoNow), Is.True);
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildProofBlockerRows(graph)
                .Any(row => row.To != null && row.To.StableId == "route.participant-input-profile"), Is.True);

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(pawn);
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
