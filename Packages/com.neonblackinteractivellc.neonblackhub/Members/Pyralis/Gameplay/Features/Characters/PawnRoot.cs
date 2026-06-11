using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Config;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Input;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Composition root for participant-owned pawns.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Movement | AuthoringCapability.Session,
        Relevance = "The root coordinator for participant-owned pawns. Handles profile application and feature installation.",
        NativeSetup = new[] { "Add to Pawn prefab root", "Assign PawnDefinition" },
        AssignmentFields = new[] { nameof(pawnDefinition) },
        FirstProof = "Pawn spawns and receives its defined movement/combat profiles.",
        ExpertAdvice = "The PawnRoot is the composition root. It reads the PawnDefinition and installs requested feature modules (Combat, Traversal, etc.) at runtime. Pawn prefabs should not carry their own scene cameras.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/pawns"
    )]
[AddComponentMenu("NeonBlack/Gameplay/Characters/Pawn Root")]
    public class PawnRoot : MonoBehaviour, IPawnParticipantInitializer
    {
        [SerializeField] private PawnDefinition pawnDefinition;
        public PawnDefinition PawnDefinition => pawnDefinition;
        public ParticipantHandle Participant { get; private set; }
        public GameModeDefinition ActiveGameMode { get; private set; }

        private ActorFeatureHost _featureHost;
        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void InitializeForParticipant(ParticipantHandle participant, GameModeDefinition gameMode)
{
            Participant = participant;
            ActiveGameMode = gameMode;

            if (participant != null && participant.PawnDefinition != null)
                pawnDefinition = participant.PawnDefinition;

            ApplyProfiles();
            InstallFeatureModules();
        }

        private void ApplyProfiles()
        {
            if (pawnDefinition == null)
                return;

            PawnProfileApplicationContext profileContext = new PawnProfileApplicationContext(gameObject, pawnDefinition, Participant);
            InputProfile inputProfile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(
                Participant != null ? Participant.Definition : null,
                pawnDefinition,
                GameplayRuntimeContext.DefaultInputProfile);

            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IPawnInputModule inputModule)
                    inputModule.ApplyInputProfile(profileContext, inputProfile);
                if (behaviour is IPawnMotor motor)
                    motor.ApplyMovementProfile(profileContext, pawnDefinition.movementProfile);
                if (behaviour is IPawnCombatModule combatModule)
                    combatModule.ApplyCombatProfile(profileContext, pawnDefinition.combatProfile);
                if (behaviour is IPawnTraversalModule traversalModule)
                    traversalModule.ApplyTraversalProfile(profileContext, pawnDefinition.traversalProfile);
                if (behaviour is IPawnPresentationModule presentationModule)
                    presentationModule.ApplyPresentationProfile(profileContext, pawnDefinition.presentationProfile);
            }
        }

        private void InstallFeatureModules()
        {
            _featureHost ??= GetComponent<ActorFeatureHost>();
            if (_featureHost == null)
                _featureHost = gameObject.AddComponent<ActorFeatureHost>();

            _featureHost.InitializeFeatures(
                new FeatureHostInitializationContext(BuildFeatureContext(), _resolver),
                pawnDefinition != null ? pawnDefinition.featureModules : null);
        }

        private ActorFeatureContext BuildFeatureContext()
        {
            return new ActorFeatureContext(
                gameObject,
                participant: Participant,
                pawnDefinition: pawnDefinition,
                gameMode: ActiveGameMode,
                health: GetComponent<HealthComponent>(),
                animation: GetComponent<ActorAnimationDriver>(),
                knockback: GetComponent<KnockbackReceiver>(),
                presentationMode: pawnDefinition != null && pawnDefinition.presentationProfile != null
                    ? pawnDefinition.presentationProfile.presentationMode
                    : ActorPresentationMode.Sprite2D,
                authoredProfiles: new ScriptableObject[]
                {
                    pawnDefinition != null ? pawnDefinition.defaultInputProfile : null,
                    pawnDefinition != null ? pawnDefinition.movementProfile : null,
                    pawnDefinition != null ? pawnDefinition.combatProfile : null,
                    pawnDefinition != null ? pawnDefinition.traversalProfile : null,
                    pawnDefinition != null ? pawnDefinition.presentationProfile : null,
                    pawnDefinition != null ? pawnDefinition.animationProfile : null
                });
        }

        private void OnDestroy()
        {
            _featureHost?.ShutdownFeatures();
        }
    }
}
