using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using NeonBlack.Gameplay.Features.Spawning;
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
        public void IntentProjection_SmokeKeeps2DGravityStylesDistinct()
        {
            PyralisAuthoringIntentModel topDown = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringCapability.Movement | AuthoringCapability.Input | AuthoringCapability.KineticMotor2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone));

            PyralisAuthoringIntentModel sideView = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringCapability.Movement | AuthoringCapability.Input | AuthoringCapability.KineticMotor2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityVertical));

            Assert.That(topDown.MatchingIntents.Select(fact => fact.StableId), Does.Contain("intent.2d-top-down-plane"));
            Assert.That(topDown.MatchingIntents.Select(fact => fact.StableId), Does.Not.Contain("intent.2d-side-view-action"));
            Assert.That(sideView.MatchingIntents.Select(fact => fact.StableId), Does.Contain("intent.2d-side-view-action"));
            Assert.That(sideView.MatchingIntents.Select(fact => fact.StableId), Does.Not.Contain("intent.2d-top-down-plane"));
        }

        [Test]
        public void IntentProjection_SmokeScoresHighBitCapabilities()
        {
            PyralisAuthoringFact steeringFact = new PyralisAuthoringFact(
                "test.intent.steering2d",
                "2D Steering Test",
                PyralisAuthoringFactKind.RuntimeCapability,
                PyralisAuthoringFactSourceKind.Reflection,
                PyralisAuthoringConfidence.Inferred,
                "A reflected 2D steering provider.",
                "Used to prove high-bit capability scoring.",
                string.Empty,
                goalTags: new[] { "2D Steering" },
                laneTags: new[] { RuntimeCapabilityLaneTag.Sprite2D.ToString() },
                axioms: AuthoringWorldAxiom.Dimensions2D,
                capability: AuthoringCapability.Steering2D);

            PyralisAuthoringIntentModel model = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringCapability.Steering2D,
                    AuthoringWorldAxiom.Dimensions2D),
                new[] { steeringFact });

            PyralisAuthoringIntentRow row = model.Recommendations.SingleOrDefault(recommendation =>
                recommendation.Fact.StableId == "test.intent.steering2d");

            Assert.That(row, Is.Not.Null);
            Assert.That(row.Score, Is.GreaterThan(0));
            Assert.That(row.Reason, Is.EqualTo(PyralisAuthoringGuidance.MatchesCapabilities));
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
            Assert.That(record.LocalComponentLookupCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(record.BroadUnityDiscoveryCount, Is.EqualTo(0));
            Assert.That(record.ReflectionOrStringLookupCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(record.Risk, Is.Not.EqualTo(PyralisSourceDependencyRisk.Low));
            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.AcceptedComposition));
            Assert.That(record.ReviewHint, Does.Contain("composition"));
            Assert.That(record.Reasons, Is.Not.Empty);
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesRuntimeOwnershipPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public sealed class EnemyPressureFixture : MonoBehaviour
    {
        [SerializeField] private InputProfile inputProfile;
        [SerializeField] private CameraRigProfile cameraRigProfile;

        private void Awake()
        {
            GetComponent<PlayerInputHandler>();
        }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Enemies/3D/EnemyPressureFixture.cs",
                    source);

            Assert.That(record.OwnerDomain, Is.EqualTo("Enemies"));
            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.RuntimeOwnership));
            Assert.That(record.LocalComponentLookupCount, Is.EqualTo(1));
            Assert.That(record.BroadUnityDiscoveryCount, Is.EqualTo(0));
            Assert.That(record.ReviewHint, Does.Contain("Runtime ownership"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesFocusedRuntimeReferenceAssembly()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    internal sealed class EnemyActorRuntimeReferences
    {
        public void Resolve(GameObject target)
        {
            target.GetComponent<PlayerInputHandler>();
            target.GetComponentInChildren<Camera>();
        }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Enemies/3D/EnemyActorRuntimeReferences.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.ReferenceAssembly));
            Assert.That(record.LocalComponentLookupCount, Is.EqualTo(2));
            Assert.That(record.ReviewHint, Does.Contain("reference/context assembly"));
            Assert.That(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind),
                Is.GreaterThan(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(PyralisSourceDependencyPressureKind.CompatibilitySurface)));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesPawnCoordinatorAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Interaction;
using NeonBlack.Gameplay.Features.Traversal;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed class Motor3D : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<ActorFeatureHost>();
        }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/3D/Motor3D.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.PawnCoordinator));
            Assert.That(record.ReviewHint, Does.Contain("pawn coordinator"));
            Assert.That(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind),
                Is.GreaterThan(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(PyralisSourceDependencyPressureKind.CompatibilitySurface)));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesFeatureRuntimeAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
    public sealed class PawnTraversalFeatureRuntime3D : MonoBehaviour, IFeatureModuleRuntime
    {
        public string ModuleId => ""actor.traversal.3d"";
        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext) {}
        public void ShutdownFeature() {}
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Traversal/Runtime/3D/PawnTraversalFeatureRuntime3D.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.FeatureModule));
            Assert.That(record.ReviewHint, Does.Contain("optional feature module"));
            Assert.That(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind),
                Is.GreaterThan(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(PyralisSourceDependencyPressureKind.CompatibilitySurface)));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesPawnCapabilitySiblingAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed class PawnCombatBehaviour2D : MonoBehaviour
    {
        [SerializeField] private HitBox2D hitBox;
        private void Awake() { GetComponent<Motor2D>(); }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/2D/PawnCombatBehaviour2D.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.PawnCapabilitySibling));
            Assert.That(record.ReviewHint, Does.Contain("pawn capability sibling"));
            Assert.That(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind),
                Is.GreaterThan(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(PyralisSourceDependencyPressureKind.PawnCoordinator)));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesLocalPresentationSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Combat;
using TMPro;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public sealed class WorldHealthBar : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        private void Awake() { GetComponent<HealthComponent>(); }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Combat/UI/WorldHealthBar.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.LocalPresentationSurface));
            Assert.That(record.ReviewHint, Does.Contain("local presentation"));
            Assert.That(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind),
                Is.GreaterThan(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(PyralisSourceDependencyPressureKind.PawnCapabilitySibling)));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesSceneZoneSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Zones
{
    public sealed class CameraZone : MonoBehaviour
    {
        [SerializeField] private CameraRigProfile profile;
        private void Awake() { GetComponent<BoxCollider>(); }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Zones/3D/CameraZone.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.SceneZoneSurface));
            Assert.That(record.ReviewHint, Does.Contain("scene-authored"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesDomainUtilityAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
    public static class HazardImpactUtility
    {
        public static bool TryApply(GameObject target)
        {
            return target.GetComponentInParent<HealthComponent>() != null;
        }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Hazards/HazardImpactUtility.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.DomainUtility));
            Assert.That(record.ReviewHint, Does.Contain("stateless domain helper"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesInputRoutingSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Features.Input
{
    public sealed class ParticipantInputRouter : MonoBehaviour
    {
        [SerializeField] private SessionDefinition sessionDefinition;
        [SerializeField] private PlayerInputManager playerInputManager;
        private void Apply(PlayerInput input, ParticipantHandle participant)
        {
            InputProfile profile = participant.Definition.inputProfile;
        }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Input/ParticipantInputRouter.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.InputRoutingSurface));
            Assert.That(record.ReviewHint, Does.Contain("ParticipantDefinition.inputProfile"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesEnemyCapabilityModuleAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public sealed class EnemyCombatModule : MonoBehaviour
    {
        [SerializeField] private EnemyCombatProfile combatProfile;
        private void Awake() { GetComponent<ActorAnimationDriver>(); }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Enemies/3D/EnemyCombatModule.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.EnemyCapabilityModule));
            Assert.That(record.ReviewHint, Does.Contain("NPC capability module"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesEnemyCoordinatorAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public sealed class EnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyCombatProfile combatProfile;
        private void Tick() { }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Enemies/3D/EnemyAI.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.EnemyCoordinator));
            Assert.That(record.ReviewHint, Does.Contain("NPC tactical coordinator"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesCombatContactSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    public sealed class HitBox : MonoBehaviour
    {
        [SerializeField] private GameObject owner;
        private void Awake() { GetComponent<Collider>(); }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Combat/HitBox.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.CombatContactSurface));
            Assert.That(record.ReviewHint, Does.Contain("combat contact"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesNetworkAdapterAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Characters;
using Unity.Netcode;
using UnityEngine;

namespace NeonBlack.Gameplay.Networking.Characters
{
    public sealed class NetworkMotor3D : NetworkBehaviour
    {
        [SerializeField] private Motor3D motor;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Networking/Characters/NetworkMotor3D.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.NetworkAdapterSurface));
            Assert.That(record.ReviewHint, Does.Contain("networking adapter"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesPersistenceDataSurfaceAsExpectedPressure()
        {
            const string source = @"
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [System.Serializable]
    public sealed class RpgOwnerSaveData
    {
        [SerializeField] private string ownerId;
        [SerializeField] private int level;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/RpgOwnerSaveData.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.PersistenceDataSurface));
            Assert.That(record.ReviewHint, Does.Contain("save/snapshot data"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesSceneNavigationSurfaceAsExpectedPressure()
        {
            const string source = @"
using UnityEngine;
using UnityEngine.EventSystems;

namespace NeonBlack.Gameplay.Core.Navigation
{
    public sealed class SceneGuard : MonoBehaviour
    {
        private void Awake() { FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude); }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Navigation/UI/SceneGuard.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.SceneNavigationSurface));
            Assert.That(record.ReviewHint, Does.Contain("scene navigation"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesRpgSceneSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Core.Rpg;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    public sealed class HubInteractionSceneController : MonoBehaviour
    {
        [SerializeField] private RpgOwnerKey owner;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Rpg/UI/HubInteractionSceneController.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.RpgSceneSurface));
            Assert.That(record.ReviewHint, Does.Contain("RPG scene"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesRpgPanelRouterAsRpgSceneSurface()
        {
            const string source = @"
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    public sealed class RpgHubPanelRouter : MonoBehaviour
    {
        [SerializeField] private HubInteractionResult result;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Rpg/UI/RpgHubPanelRouter.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.RpgSceneSurface));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesScoringRuntimeSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Scoring
{
    public sealed class StillnessBonus2D : MonoBehaviour
    {
        [SerializeField] private Motor2D motor;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Scoring/2D/StillnessBonus2D.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.ScoringRuntimeSurface));
            Assert.That(record.ReviewHint, Does.Contain("scoring feature"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesSpawningRuntimeSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Spawning
{
    public sealed class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private ParticipantSpawnService participantSpawnService;
        [SerializeField] private Transform[] spawnPoints;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Spawning/3D/PlayerSpawner.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.SpawningRuntimeSurface));
            Assert.That(record.ReviewHint, Does.Contain("spawning surfaces"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesGameFlowRuntimeSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Scoring;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.GameFlow
{
    public sealed class GameManager : MonoBehaviour, IGameplaySessionFlow, IHazardOutcomeSink
    {
        [SerializeField] private ParticipantScoreService scoreManager;
        [SerializeField] private HazardSpawner hazardSpawner;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/GameFlow/2D/GameManager.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.GameFlowRuntimeSurface));
            Assert.That(record.ReviewHint, Does.Contain("game-flow surface"));
            Assert.That(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(record.PressureKind),
                Is.GreaterThan(PyralisSourceDependencyHygieneScanner.GetCleanupPriority(PyralisSourceDependencyPressureKind.CompatibilitySurface)));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesContractReflectionSurfaceAsExpectedPressure()
        {
            const string source = @"
using System;
using System.Reflection;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public static class ResolvedAuthoringContractRegistry
    {
        public static Type Resolve(Type type) => type.GetTypeInfo().AsType();
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Authoring/ResolvedAuthoringContractRegistry.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.ContractReflectionSurface));
            Assert.That(record.ReviewHint, Does.Contain("contract spine"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesPawnProjectileModuleAsCapabilitySibling()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed class PawnProjectileModule : MonoBehaviour
    {
        [SerializeField] private ProjectileLauncher3D projectileLauncher;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/PawnProjectileModule.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.PawnCapabilitySibling));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesActorFeatureContextAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Composition
{
    public sealed class ActorFeatureContext
    {
        public GameObject ActorObject { get; }
        public PawnDefinition PawnDefinition { get; }
        public ActorPresentationMode PresentationMode { get; }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Composition/ActorFeatureContext.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.ActorFeatureContext));
            Assert.That(record.ReviewHint, Does.Contain("read-only context"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesSceneCameraRigAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using Unity.Cinemachine;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Camera
{
    public sealed class CinemachineCameraRigController : MonoBehaviour
    {
        [SerializeField] private CameraRigProfile cameraRigProfile;
        [SerializeField] private CinemachineCamera sharedCamera;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Presentation/Camera/CinemachineCameraRigController.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.SceneCameraRig));
            Assert.That(record.ReviewHint, Does.Contain("scene-owned camera rig"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesAuthoredDataAssetAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    public sealed class PawnDefinition : ScriptableObject
    {
        [SerializeField] private PawnCombatProfile combatProfile;
        [SerializeField] private WeaponData startingWeapon;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/PawnDefinition.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.AuthoredDataAsset));
            Assert.That(record.ReviewHint, Does.Contain("authored data"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesHazardRuntimeSurfaceAsExpectedPressure()
        {
            const string source = @"
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
    public sealed class Hazard : MonoBehaviour
    {
        [SerializeField] private HazardData data;
        private void Awake() { GetComponent<Rigidbody2D>(); }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Hazards/2D/Hazard.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.HazardRuntimeSurface));
            Assert.That(record.ReviewHint, Does.Contain("hazard runtime"));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeClassifiesAuthoredRuntimeSurfaceAsExpectedPressure()
        {
            const string source = @"
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn3DMovementComponent
    {
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float jumpImpulse;
        [SerializeField] private float gravity;
        [SerializeField] private float acceleration;
        [SerializeField] private float deceleration;
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/Runtime/Shared/Components/3D/Pawn3DMovementComponent.Config.cs",
                    source);

            Assert.That(record.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.AuthoredRuntimeSurface));
            Assert.That(record.ReviewHint, Does.Contain("authored runtime fields"));
            Assert.That(record.SerializedFieldCount, Is.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void SourceDependencyHygiene_SmokeScoresBroadUnityDiscoveryAboveLocalComponentCaching()
        {
            const string localSource = @"
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed class LocalLookupFixture : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Motor2D>();
            GetComponentInChildren<Renderer>();
            GetComponentInParent<PawnRoot>();
        }
    }
}";
            const string broadSource = @"
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed class BroadLookupFixture : MonoBehaviour
    {
        private void Awake()
        {
            Object.FindAnyObjectByType<Camera>();
            GameObject.Find(""Main Camera"");
            Resources.Load<GameObject>(""Pawn"");
        }
    }
}";

            PyralisSourceDependencyHygieneRecord local =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/LocalLookupFixture.cs",
                    localSource);
            PyralisSourceDependencyHygieneRecord broad =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Characters/BroadLookupFixture.cs",
                    broadSource);

            Assert.That(local.LocalComponentLookupCount, Is.EqualTo(3));
            Assert.That(local.BroadUnityDiscoveryCount, Is.EqualTo(0));
            Assert.That(broad.LocalComponentLookupCount, Is.EqualTo(0));
            Assert.That(broad.BroadUnityDiscoveryCount, Is.EqualTo(3));
            Assert.That(broad.RiskScore, Is.GreaterThan(local.RiskScore));
            Assert.That(string.Join(" ", broad.Reasons), Does.Contain("broad Unity"));
        }

        [Test]
        public void SourceDependencyHygiene_DoesNotTreatInspectorPropertyBindingAsReflectionPressure()
        {
            const string source = @"
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies.Editor.Inspectors
{
    public sealed class InspectorFixture : UnityEditor.Editor
    {
        private SerializedProperty _profile;

        private void OnEnable()
        {
            _profile = serializedObject.FindProperty(nameof(_profile));
        }
    }
}";

            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Enemies/Editor/Inspectors/InspectorFixture.cs",
                    source);

            Assert.That(record.ReflectionOrStringLookupCount, Is.EqualTo(0));
        }

        [Test]
        public void HygieneProjection_SmokeAuditsGraphHealthWithoutSceneRepairBuckets()
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

            IReadOnlyList<PyralisAuthoringGraphAuditSection> sections =
                PyralisAuthoringSetupGraphProjection.BuildHygieneSections(graph);

            Assert.That(sections.Select(section => section.Label), Does.Contain("Unvalidated Graph Nodes"));
            Assert.That(sections.Select(section => section.Label), Does.Contain("Contract Inventory / Not Route-Evaluated"));
            Assert.That(sections.Select(section => section.Label), Does.Contain("Proof Blocker Links"));
            Assert.That(sections.Select(section => section.Label), Does.Not.Contain("Explicit Runtime / Scene Findings"));
            Assert.That(sections.Select(section => section.Label), Does.Not.Contain("Required Before Play"));
            PyralisAuthoringGraphAuditSection inventorySection =
                sections.First(section => section.Label == "Contract Inventory / Not Route-Evaluated");
            Assert.That(inventorySection.Rows.Select(row => row.NodeId), Does.Contain("contract.custom"));
            PyralisAuthoringGraphAuditSection unvalidatedSection =
                sections.First(section => section.Label == "Unvalidated Graph Nodes");
            Assert.That(unvalidatedSection.Rows.Select(row => row.NodeId), Does.Not.Contain("contract.custom"));
            Assert.That(sections.SelectMany(section => section.Rows).Select(row => row.NodeId), Does.Contain("setup.session"));
            Assert.That(sections.SelectMany(section => section.Rows).Select(row => row.NodeId), Does.Not.Contain("validation.input"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildMapSceneSetupIssueRows(graph).Select(row => row.NodeId), Does.Contain("validation.input"));
        }

        [Test]
        public void SetupGraph_SmokeNoRouteDoesNotSelectCustomObjectProof()
        {
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build((UnityEngine.Object)null);

            PyralisAuthoringGraphNode proof = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.StableId, Is.EqualTo("proof.unresolved-route"));
            Assert.That(proof.Label, Is.EqualTo("No Active Proof Target"));
            Assert.That(proof.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Optional));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildProofBlockerRows(graph), Is.Empty);
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildFirstProofPrioritySummary(graph), Does.Contain("No first proof target yet"));
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
        public void SetupGraphProjection_FocusedProofSupportDoesNotPromoteLaterCapabilities()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement
                    | AuthoringCapability.Input
                    | AuthoringCapability.Camera
                    | AuthoringCapability.Animation
                    | AuthoringCapability.Networking
                    | AuthoringCapability.Environment,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> allProofSupport =
                PyralisAuthoringSetupGraphProjection.BuildProofSupportRows(graph);
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> directProofSupport =
                PyralisAuthoringSetupGraphProjection.BuildDirectProofSupportRows(graph);
            string routeTraceJson = PyralisAuthoringSetupGraphJsonExporter.ToRouteProofTraceJson(graph);

            Assert.That(allProofSupport.Any(row => row.From?.CapabilityFamily == RuntimeCapabilityFamily.Networking), Is.True);
            Assert.That(directProofSupport.Any(row => row.From?.CapabilityFamily == RuntimeCapabilityFamily.Networking), Is.False);
            Assert.That(directProofSupport.Any(row => row.From?.CapabilityFamily == RuntimeCapabilityFamily.ProceduralGeneration), Is.False);
            Assert.That(directProofSupport.Any(row => row.From?.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.True);
            Assert.That(routeTraceJson, Does.Not.Contain("\"from\": \"Networking\""));
            Assert.That(routeTraceJson, Does.Not.Contain("\"from\": \"Procedural Generation\""));

            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraphProjection_RouteProofTraceSeparatesRequiredEnhancerAndCanWaitCards()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            PyralisAuthoringGraphNode lifetimeScope = new PyralisAuthoringGraphNode(
                "setupflow.setup-visible-lifetime-scope",
                "Visible Lifetime Scope",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                PyralisAuthoringGraphEvidenceState.Missing,
                workIntent: PyralisAuthoringGraphWorkIntent.RequiredSetup);
            PyralisAuthoringGraphNode scoring = new PyralisAuthoringGraphNode(
                "setupflow.setup-enable-scoring-route",
                "Enable Scoring Route",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                PyralisAuthoringGraphEvidenceState.Optional,
                workIntent: PyralisAuthoringGraphWorkIntent.Optional);
            PyralisAuthoringGraphNode settings = new PyralisAuthoringGraphNode(
                "setupflow.setup-assign-settings-manager",
                "Assign Settings Manager",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                PyralisAuthoringGraphEvidenceState.CandidateDetected);
            PyralisAuthoringGraphNode cameraEnhancer = new PyralisAuthoringGraphNode(
                "setupflow.setup-tune-camera-framing",
                "Tune Camera Framing",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.SetupFlow,
                PyralisAuthoringGraphEvidenceState.CandidateDetected,
                workIntent: PyralisAuthoringGraphWorkIntent.ProofEnhancer);
            PyralisAuthoringGraphNode proof = new PyralisAuthoringGraphNode(
                "proof.1p-pawn-movement",
                "1P Pawn Movement Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.ProofVocabulary,
                PyralisAuthoringGraphEvidenceState.Missing);
            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                session,
                null,
                new[] { lifetimeScope, scoring, settings, cameraEnhancer, proof },
                System.Array.Empty<PyralisAuthoringGraphEdge>());

            IReadOnlyList<PyralisAuthoringRouteStepRow> criticalPath = PyralisAuthoringSetupGraphProjection.BuildRouteCriticalPathRows(graph);
            IReadOnlyList<PyralisAuthoringRouteStepRow> proofEnhancers = PyralisAuthoringSetupGraphProjection.BuildRouteProofEnhancerRows(graph);
            IReadOnlyList<PyralisAuthoringRouteStepRow> canWait = PyralisAuthoringSetupGraphProjection.BuildRouteCanWaitRows(graph);
            IReadOnlyList<PyralisAuthoringRouteStepRow> orderedSteps = PyralisAuthoringSetupGraphProjection.BuildRouteStepRows(graph);
            string routeTraceJson = PyralisAuthoringSetupGraphJsonExporter.ToRouteProofTraceJson(graph);

            Assert.That(criticalPath.Select(row => row.Label), Does.Contain("Visible Lifetime Scope"));
            Assert.That(criticalPath.Select(row => row.Label), Does.Not.Contain("Enable Scoring Route"));
            Assert.That(proofEnhancers.Select(row => row.Label), Does.Contain("Tune Camera Framing"));
            Assert.That(canWait.Select(row => row.Label), Does.Contain("Enable Scoring Route"));
            Assert.That(canWait.Select(row => row.Label), Does.Contain("Assign Settings Manager"));
            Assert.That(orderedSteps.Select(row => row.Label), Does.Contain("Visible Lifetime Scope"));
            Assert.That(orderedSteps.Select(row => row.Label), Does.Contain("Tune Camera Framing"));
            Assert.That(orderedSteps.Select(row => row.Label), Does.Not.Contain("Enable Scoring Route"));
            Assert.That(orderedSteps.Last().StableId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(routeTraceJson, Does.Contain("\"criticalPath\""));
            Assert.That(routeTraceJson, Does.Contain("\"proofEnhancers\""));
            Assert.That(routeTraceJson, Does.Contain("\"canWait\""));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraphJsonExport_SmokeMapExportsAuthoredRouteWithoutIntent()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            participant.defaultPawn = pawn;

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);
            string mapJson = PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);

            Assert.That(mapJson, Does.Contain("\"currentRoute\""));
            Assert.That(mapJson, Does.Contain("\"hasSelectedCapabilities\": true"));
            Assert.That(mapJson, Does.Contain("\"requiresPawn\": true"));
            Assert.That(mapJson, Does.Contain("\"hasParticipants\": true"));
            Assert.That(mapJson, Does.Contain("\"capabilityFamilies\""));
            Assert.That(mapJson, Does.Contain("CharacterPawnGameplay"));
            Assert.That(mapJson, Does.Contain("\"routeFacts\""));
            Assert.That(mapJson, Does.Contain("\"participantPawnIssueKind\": \"MissingPawnPrefab\""));
            Assert.That(mapJson, Does.Not.Contain("\"hygieneSections\""));

            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
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
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Assert.That(typeof(SessionDefinition).GetField("defaultInputProfile", fields), Is.Null);
            Assert.That(typeof(SessionDefinition).GetField("inputProfile", fields), Is.Null);
            Assert.That(typeof(PawnDefinition).GetField("defaultInputProfile", fields), Is.Null);
            Assert.That(typeof(PawnDefinition).GetField("inputProfile", fields), Is.Null);
            Assert.That(typeof(ParticipantDefinition).GetField(nameof(ParticipantDefinition.inputProfile), fields), Is.Not.Null);

            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            participant.inputProfile = inputProfile;

            Assert.That(ParticipantInputProfileUtility.ResolveEffectiveInputProfile(participant), Is.SameAs(inputProfile));
            Assert.That(typeof(IParticipantRoster).GetMethod("TryGetParticipantBySeat"), Is.Not.Null);
            Assert.That(typeof(ParticipantRosterService).GetInterfaces(), Has.Member(typeof(IParticipantRoster)));
            Assert.That(typeof(PlayfieldProfile).GetInterfaces(), Has.Member(typeof(IPlayfieldBoundsProvider)));
            Assert.That(typeof(CinemachineCameraRigController).GetInterfaces(), Has.Member(typeof(ICameraBoundsProvider)));
            Assert.That(typeof(GameplaySessionBootstrap).GetField("sceneNavigatorSource", fields), Is.Not.Null);
            Assert.That(typeof(GameplaySessionBootstrap).GetField("cameraBoundsSource", fields), Is.Null);
            Assert.That(typeof(PlayerSpawner).GetField("participantSpawnService", fields), Is.Not.Null);
            Assert.That(typeof(PlayerSpawner).GetField("targetSeatIndex", fields), Is.Not.Null);
            Assert.That(typeof(PlayerSpawner).GetField("playerPrefab", fields), Is.Null);
            Assert.That(typeof(PlayerSpawner).GetField("currentPlayer", fields), Is.Null);
            Assert.That(typeof(GameManager).GetInterfaces(), Has.Member(typeof(IGameplaySessionFlow)));
            Assert.That(typeof(GameManager).GetInterfaces(), Has.No.Member(typeof(IGameplayStateReader)));
            Assert.That(typeof(GameManager).GetField("playerControllers", fields), Is.Not.Null);

            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participant);
        }

        [Test]
        public void ReflectiveContracts_SmokeDoNotPromoteRuntimeServiceFallbackFields()
        {
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(Pawn2DMovementComponent));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(Hazard));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(HazardSpawner));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(CollectibleSpawner2D));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(Collectible2D));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(Collectible3D));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(CollectibleFeedback2D));
            AssertContractFactDoesNotExposeRuntimeServiceFields(typeof(StillnessBonus2D));

            ResolvedAuthoringContract gameManagerContract = ResolvedAuthoringContractRegistry.FindByType(typeof(GameManager));
            Assert.That(gameManagerContract, Is.Not.Null);
            Assert.That(gameManagerContract.AssignmentFields, Does.Contain("scoreManager"));
            Assert.That(gameManagerContract.AssignmentFields, Does.Contain("hazardSpawner"));
            Assert.That(gameManagerContract.AssignmentFields, Does.Not.Contain("playerControllers"));
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
            Assert.That(fact.AssignmentFields, Has.None.Contains("awardSinkSource"));
            Assert.That(fact.AssignmentFields, Has.None.Contains("targetCamera"));
            Assert.That(fact.AssignmentFields, Has.None.Contains("cameraShakeSink"));
            Assert.That(fact.AssignmentFields, Has.None.Contains("settingsSource"));
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
                .Any(row => row.Label == "Pawn Setup" && row.IsMissing && row.Message.Contains("pawn prefab")), Is.True);

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
            Assert.That(routeSteps[0].Phase, Is.EqualTo(PyralisAuthoringRouteStepPhase.Foundation));
            Assert.That(routeSteps.Any(row => row.StableId == "route.shape"
                && (row.Role == PyralisAuthoringRouteStepRole.BlocksProof || row.Role == PyralisAuthoringRouteStepRole.RouteContext)), Is.True);
            Assert.That(routeSteps.Any(row => row.StableId == "pawn.definition"
                && (row.Role == PyralisAuthoringRouteStepRole.DoThisFirst || row.Role == PyralisAuthoringRouteStepRole.BlocksProof)), Is.True);
            Assert.That(routeSteps.Any(row => row.StableId == "proof.1p-pawn-movement"
                && row.Role == PyralisAuthoringRouteStepRole.ProofTarget), Is.True);
            Assert.That(routeSteps.Last().StableId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(routeSteps.Select(row => row.StableId).ToArray(), Does.Contain("route.participant-input-profile"));
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
        public void SetupGraph_IntentPawnRoute_DoesNotSatisfySessionParticipants()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            session.defaultGameMode = mode;

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input | AuthoringCapability.Participants,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);
            string mapJson = PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);

            Assert.That(mapJson, Does.Contain("\"requiresPawn\": true"));
            Assert.That(mapJson, Does.Contain("\"hasParticipants\": false"));
            Assert.That(mapJson, Does.Contain("\"hasAnyDefaultPawn\": false"));
            Assert.That(mapJson, Does.Contain("\"participantPawnIssueKind\": \"MissingParticipants\""));
            Assert.That(graph.TryFindNode("participant.default", out PyralisAuthoringGraphNode participantsNode), Is.True);
            Assert.That(participantsNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(graph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));

            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(session, graph);
            Assert.That(model.ReadyToPressPlay, Is.False);
            Assert.That(model.DoNow.Select(issue => issue.Label), Does.Contain("Assign Default Participants"));

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
