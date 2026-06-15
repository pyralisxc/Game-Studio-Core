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
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Pickups;
using NeonBlack.Gameplay.Features.Scoring;
using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Presentation.Camera;
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
        public void SourceDependencyHygiene_SmokeScoresCrossDomainPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Platform.Session
{
    public sealed class HygieneFixture : MonoBehaviour
    {
        [SerializeField] private InputProfile inputProfile;
        [SerializeField] private CameraRigProfile cameraRigProfile;

        private void Awake()
        {
            GetComponent<PlayerInputHandler>();
            Type.GetType(""NeonBlack.Gameplay.Features.Combat.CombatService"");
        }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Platform/Session/HygieneFixture.cs",
                    source);

            Assert.That(record.OwnerDomain, Is.EqualTo("Platform"));
            Assert.That(record.Domains, Does.Contain("Input"));
            Assert.That(record.Domains, Does.Contain("Combat"));
            Assert.That(record.ConcreteCrossDomainCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(record.UnityLookupCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(record.ReflectionOrStringLookupCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(record.Risk, Is.Not.EqualTo(PyralisSourceDependencyRisk.Low));
            Assert.That(record.Reasons, Is.Not.Empty);
        }

        [Test]
        public void HygieneProjection_SmokeAuditsGraphEvidenceWithoutPlayReadinessBuckets()
        {
            PyralisAuthoringGraphNode missingSetup = new PyralisAuthoringGraphNode(
                "setup.session",
                "Session",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                PyralisAuthoringGraphEvidenceState.Missing);
            PyralisAuthoringGraphNode unknownContract = new PyralisAuthoringGraphNode(
                "contract.custom",
                "Custom Contract",
                PyralisAuthoringGraphNodeKind.Contract,
                PyralisAuthoringGraphSourceKind.AuthoringContract,
                PyralisAuthoringGraphEvidenceState.Unknown);
            PyralisAuthoringGraphNode runtimeEvidence = new PyralisAuthoringGraphNode(
                "validation.input",
                "Input Validation",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.RuntimeValidation,
                PyralisAuthoringGraphEvidenceState.Missing);
            PyralisAuthoringGraphNode proof = new PyralisAuthoringGraphNode(
                "proof.1p",
                "1P Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.ProofVocabulary,
                PyralisAuthoringGraphEvidenceState.Missing);

            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                null,
                null,
                new[] { missingSetup, unknownContract, runtimeEvidence, proof },
                new[] { new PyralisAuthoringGraphEdge("proof.1p", "setup.session", PyralisAuthoringGraphEdgeKind.BlockedBy, "missing setup") });

            IReadOnlyList<PyralisAuthoringValidationGraphSection> sections =
                PyralisAuthoringSetupGraphProjection.BuildHygieneSections(graph);

            Assert.That(sections.Select(section => section.Label), Does.Contain("Unvalidated Graph Nodes"));
            Assert.That(sections.Select(section => section.Label), Does.Contain("Explicit Runtime / Scene Findings"));
            Assert.That(sections.Select(section => section.Label), Does.Contain("Proof Blocker Links"));
            Assert.That(sections.Select(section => section.Label), Does.Not.Contain("Required Before Play"));
            Assert.That(sections.SelectMany(section => section.Rows).Select(row => row.NodeId), Does.Contain("contract.custom"));
            Assert.That(sections.SelectMany(section => section.Rows).Select(row => row.NodeId), Does.Contain("validation.input"));
            Assert.That(sections.SelectMany(section => section.Rows).Select(row => row.NodeId), Does.Contain("setup.session"));
        }

        [Test]
        public void ContractNativeSetup_SmokeDoesNotDuplicateReflectedUnityMetadata()
        {
            string gameplayRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Packages", "com.neonblackinteractivellc.neonblackhub", "Members", "Pyralis", "Gameplay"));
            string[] sourceFiles = Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories);
            string[] duplicateCreateSetupMarkers =
            {
                "NativeSetup = new[] { \"Create Asset\" }",
                "NativeSetup = new[] { \"Create Asset.\" }",
                "NativeSetup = new[] { \"Create asset in Project window.\" }",
                "\"Create Asset.\", ",
                "\"Create asset in Project window.\", "
            };
            string[] duplicateAddComponentSetupMarkers =
            {
                "NativeSetup = new[] { \"Add Component\" }",
                "NativeSetup = new[] { \"Add Component.\" }",
                "\"Add Component\", ",
                "\"Add Component.\", "
            };

            foreach (string sourceFile in sourceFiles)
            {
                string source = File.ReadAllText(sourceFile);
                if (source.Contains("[CreateAssetMenu"))
                {
                    for (int i = 0; i < duplicateCreateSetupMarkers.Length; i++)
                    {
                        Assert.That(
                            source.Contains(duplicateCreateSetupMarkers[i]),
                            Is.False,
                            $"{sourceFile} should let CreateAssetMenu reflection generate the Project create action instead of duplicating it in NativeSetup.");
                    }
                }

                if (source.Contains("[AddComponentMenu"))
                {
                    for (int i = 0; i < duplicateAddComponentSetupMarkers.Length; i++)
                    {
                        Assert.That(
                            source.Contains(duplicateAddComponentSetupMarkers[i]),
                            Is.False,
                            $"{sourceFile} should let AddComponentMenu reflection generate the Inspector add-component action instead of duplicating it in NativeSetup.");
                    }
                }
            }
        }

        [Test]
        public void ContractNativeSetup_SmokePrefersReflectedCreateActionBeforeFallbackSteps()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByType(typeof(PawnMovementProfile));
            Assert.That(contract, Is.Not.Null);

            PyralisAuthoringFact fact = PyralisReflectiveFactScanner.CreateFactFromContract(contract);
            Assert.That(fact.NativeActions.Length, Is.GreaterThanOrEqualTo(2));

            PyralisAuthoringNativeAction first = fact.NativeActions[0];
            Assert.That(first.Verb, Is.EqualTo("Create"));
            Assert.That(first.Surface, Is.EqualTo(PyralisAuthoringActionSurface.ProjectWindow));
            Assert.That(first.Target, Is.EqualTo("Pawn Movement Profile"));
            Assert.That(first.FieldOrComponent, Does.Contain("Create -> NeonBlack/Profiles/Pawn Movement Profile"));
            Assert.That(fact.NativeActions.Any(action => action.Target == "contract NativeSetup fallback"), Is.False);
            Assert.That(fact.NativeActions.Any(action => action.FieldOrComponent == "Create asset in Project window."), Is.False);
            Assert.That(fact.NativeActions.Any(action => action.FieldOrComponent == "Assign to a PawnDefinition."), Is.True);
        }

        [Test]
        public void ContractNativeSetup_SmokeUnityMetadataPreventsGeneratedFallbackSetup()
        {
            ResolvedAuthoringContract settingsContract = ResolvedAuthoringContractRegistry.FindByType(typeof(SettingsProfile));
            Assert.That(settingsContract, Is.Not.Null);
            Assert.That(settingsContract.NativeSetup, Is.Empty);

            PyralisAuthoringFact settingsFact = PyralisReflectiveFactScanner.CreateFactFromContract(settingsContract);
            Assert.That(settingsFact.NativeActions.Length, Is.EqualTo(1));
            Assert.That(settingsFact.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.ProjectWindow));
            Assert.That(settingsFact.NativeActions[0].FieldOrComponent, Does.Contain("Create -> NeonBlack/Profiles/Settings Profile"));

            ResolvedAuthoringContract inputAdapterContract = ResolvedAuthoringContractRegistry.FindByType(typeof(Motor2DInputAdapter));
            Assert.That(inputAdapterContract, Is.Not.Null);
            Assert.That(inputAdapterContract.NativeSetup, Is.Empty);

            PyralisAuthoringFact inputAdapterFact = PyralisReflectiveFactScanner.CreateFactFromContract(inputAdapterContract);
            Assert.That(inputAdapterFact.NativeActions.Length, Is.EqualTo(1));
            Assert.That(inputAdapterFact.NativeActions[0].Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));
            Assert.That(inputAdapterFact.NativeActions[0].FieldOrComponent, Does.Contain("Add Component -> NeonBlack/Gameplay/Input/2D Motor Input Adapter"));
            Assert.That(inputAdapterFact.RequiredUnitySurfaces, Does.Contain(nameof(Motor2D)));
        }

        [Test]
        public void GameplaySeams_SmokeKeepSingleRuntimeOwners()
        {
            string gameplayRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Packages", "com.neonblackinteractivellc.neonblackhub", "Members", "Pyralis", "Gameplay"));
            string sessionSource = File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "SessionDefinition.cs"));
            string pawnSource = File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "PawnDefinition.cs"));
            string inputProfileSource = File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "InputProfile.cs"));
            string participantInputUtilitySource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Input", "ParticipantInputProfileUtility.cs"));
            string participantRosterInterfaceSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "Runtime", "Shared", "Participants", "IParticipantRoster.cs"));
            string participantRosterSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "Runtime", "Shared", "Participants", "ParticipantRosterService.cs"));
            string runtimeContextSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Platform", "Composition", "GameplayRuntimeContext.cs"));
            string bootstrapSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Platform", "Session", "GameplaySessionBootstrap.cs"));
            string spawnSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "Runtime", "Shared", "Services", "ParticipantSpawnService.cs"));
            string spawnerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Spawning", "3D", "PlayerSpawner.cs"));
            string hudTargetBindingSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Feedback", "UI", "ParticipantHudTargetBinding.cs"));
            string movementSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "2D", "Pawn2DMovementComponent.cs"));
            string hazardSpawnerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "HazardSpawner.cs"));
            string collectibleSpawnerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Pickups", "2D", "CollectibleSpawner2D.cs"));
            string playfieldSource = File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Profiles", "PlayfieldProfile.cs"));
            string pawn2DMovementSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "2D", "Pawn2DMovementComponent.cs"));
            string cameraRigSource = File.ReadAllText(Path.Combine(gameplayRoot, "Presentation", "Camera", "CinemachineCameraRigController.cs"));
            string featureServicePolicySource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Platform", "Composition", "PyralisRuntimeFeatureServicePolicy.cs"));

            Assert.That(sessionSource, Does.Not.Contain("defaultInputProfile"));
            Assert.That(pawnSource, Does.Not.Contain("defaultInputProfile"));
            Assert.That(sessionSource, Does.Not.Contain("InputProfile inputProfile"));
            Assert.That(pawnSource, Does.Not.Contain("InputProfile inputProfile"));
            Assert.That(inputProfileSource, Does.Not.Contain("participant or pawn definition"));
            Assert.That(participantInputUtilitySource, Does.Not.Contain("SessionDefinition"));
            Assert.That(participantInputUtilitySource, Does.Not.Contain("PawnDefinition"));
            Assert.That(participantRosterInterfaceSource, Does.Contain("TryGetParticipantBySeat"));
            Assert.That(participantRosterSource, Does.Contain("TryGetParticipantBySeat"));
            Assert.That(runtimeContextSource, Does.Not.Contain("DefaultInputProfile"));
            Assert.That(runtimeContextSource, Does.Not.Contain("DefaultInputActions"));
            Assert.That(bootstrapSource, Does.Not.Contain("GetOrCreatePersistentService"));
            Assert.That(bootstrapSource, Does.Contain("ConfigureRuntime("));
            Assert.That(bootstrapSource, Does.Contain("sceneNavigatorSource"));
            Assert.That(bootstrapSource, Does.Not.Contain("SceneLoader sceneLoader"));
            Assert.That(spawnSource, Does.Not.Contain("GetMethod("));
            Assert.That(spawnSource, Does.Contain("IPawnRuntimeServicesReceiver"));
            Assert.That(spawnerSource, Does.Not.Contain("playerPrefab"));
            Assert.That(spawnerSource, Does.Not.Contain("currentPlayer"));
            Assert.That(spawnerSource, Does.Contain("ParticipantSpawnService"));
            Assert.That(spawnerSource, Does.Contain("TryGetParticipantBySeat"));
            Assert.That(hudTargetBindingSource, Does.Contain("TryGetParticipantBySeat"));
            Assert.That(bootstrapSource, Does.Not.Contain("TrySetMember"));
            Assert.That(bootstrapSource, Does.Not.Contain("System.Reflection"));
            Assert.That(bootstrapSource, Does.Not.Contain("cameraBoundsSource"));
            Assert.That(movementSource, Does.Not.Contain("cameraBoundsSource"));
            Assert.That(movementSource, Does.Not.Contain("private Camera targetCamera"));
            Assert.That(hazardSpawnerSource, Does.Not.Contain("_cameraBoundsSource"));
            Assert.That(hazardSpawnerSource, Does.Not.Contain("_targetCamera"));
            Assert.That(collectibleSpawnerSource, Does.Not.Contain("_cameraBoundsSource"));
            Assert.That(collectibleSpawnerSource, Does.Not.Contain("_targetCamera"));
            Assert.That(playfieldSource, Does.Contain("IPlayfieldBoundsProvider"));
            Assert.That(playfieldSource, Does.Contain("AuthoringCapability.Movement"));
            Assert.That(pawn2DMovementSource.IndexOf("TryGetPlayfieldBounds2D", System.StringComparison.Ordinal), Is.LessThan(pawn2DMovementSource.IndexOf("TryGetCameraBounds", System.StringComparison.Ordinal)));
            Assert.That(cameraRigSource, Does.Contain("ICameraBoundsProvider"));
            Assert.That(featureServicePolicySource, Does.Contain("AppendContractSignals"));
            Assert.That(featureServicePolicySource, Does.Not.Contain("AppendUncontractedModuleSignals"));
            Assert.That(featureServicePolicySource, Does.Not.Contain("JoinSignals"));
            Assert.That(featureServicePolicySource, Does.Not.Contain("IndexOf(token"));

            string inputRouterSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Input", "ParticipantInputRouter.cs"));
            string gameManagerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "GameFlow", "2D", "GameManager.cs"));
            Assert.That(inputRouterSource, Does.Not.Contain("PlayerInputManager.instance"));
            Assert.That(gameManagerSource, Does.Not.Contain("private GameObject player"));
            Assert.That(gameManagerSource, Does.Not.Contain("private Motor2D primaryPlayerController"));
            Assert.That(gameManagerSource, Does.Contain("Standalone Compatibility"));
        }

        [Test]
        public void ReflectiveContracts_SmokeDoNotPromoteRuntimeServiceFallbackFields()
        {
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(Pawn2DMovementComponent));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(HazardSpawner));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(CollectibleSpawner2D));

            ResolvedAuthoringContract gameManagerContract = ResolvedAuthoringContractRegistry.FindByType(typeof(GameManager));
            Assert.That(gameManagerContract, Is.Not.Null);
            Assert.That(gameManagerContract.AssignmentFields, Does.Contain("scoreManager"));
            Assert.That(gameManagerContract.AssignmentFields, Does.Contain("hazardSpawner"));
            Assert.That(gameManagerContract.AssignmentFields, Does.Not.Contain("playerRoot"));
            Assert.That(gameManagerContract.AssignmentFields, Does.Not.Contain("scoreService"));

            ResolvedAuthoringContract cameraRigContract = ResolvedAuthoringContractRegistry.FindByType(typeof(CinemachineCameraRigController));
            PyralisAuthoringFact cameraRigFact = PyralisReflectiveFactScanner.CreateFactFromContract(cameraRigContract);
            Assert.That(cameraRigFact.AssignmentFields, Has.Some.Contains("targetCamera"));
        }

        private static void AssertContractFactDoesNotExposeRuntimeServiceFields(System.Type sourceType)
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByType(sourceType);
            Assert.That(contract, Is.Not.Null);

            PyralisAuthoringFact fact = PyralisReflectiveFactScanner.CreateFactFromContract(contract);
            Assert.That(fact.AssignmentFields, Has.None.Contains("gameplayStateSource"));
            Assert.That(fact.AssignmentFields, Has.None.Contains("cameraBoundsSource"));
            Assert.That(fact.AssignmentFields, Has.None.Contains("hazardOutcomeSource"));
            Assert.That(fact.AssignmentFields, Has.None.Contains("pickupBurstSurfaceSource"));
            Assert.That(fact.AssignmentFields, Has.None.Contains("scoreAwardSource"));
            Assert.That(fact.AssignmentFields, Has.None.Contains("targetCamera"));
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
        public void SetupGraph_SmokeStandalonePawnDefinitionCompilesPawnPrefabGuidance()
        {
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(pawn, intent);

            Assert.That(graph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(pawnNode.BlockingReason, Does.Contain("pawn prefab"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildProofBlockerRows(graph)
                .Any(row => row.To != null && row.To.StableId == "pawn.definition"), Is.True);

            Object.DestroyImmediate(pawn);
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
        public void IntentProjection_SmokeMovementInputDoesNotInferCombatRoute()
        {
            RuntimeCapabilityFamily[] families = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                AuthoringCapability.Movement | AuthoringCapability.Input | AuthoringCapability.Participants | AuthoringCapability.KineticMotor2D,
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.BoundedSpace);

            Assert.That(families.Any(family => family == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.True);
            Assert.That(families.Any(family => family == RuntimeCapabilityFamily.Combat), Is.False);
            Assert.That(families.Any(family => family == RuntimeCapabilityFamily.GunsProjectiles), Is.False);
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
