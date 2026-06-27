using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Modules.Character
{
    /// <summary>
    /// Composition root for participant-owned pawns.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Movement | AuthoringCapability.Session,
        SetupNodeId = "pawn.definition",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.CharacterPawnGameplay },
        CapabilityPath = "Character/Pawn Gameplay/Pawn Root",
        Relevance = "The root coordinator for participant-owned pawns. Handles profile application and feature installation after a participant spawns the pawn prefab.",
        RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.ParticipantRouteSupport, "PawnRoot" },
        NativeSetup = new[] { "Add to Pawn prefab root" },
        Proof = "Pawn spawns and receives its defined movement/combat profiles.",
        ExpertAdvice = "PawnDefinition owns the prefab reference. PawnRoot receives the participant's PawnDefinition during spawn; assign the local field only when placing a pawn directly in a scene without ParticipantSpawnService.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/pawns"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Characters/Pawn Root")]
    public partial class PawnRoot : MonoBehaviour, IPawnParticipantInitializer
    {
        [Tooltip("Optional prefab-local fallback. Spawned pawns receive this from ParticipantDefinition.defaultPawn, so beginner prefab setup usually leaves this empty.")]
        [SerializeField] private PawnDefinition pawnDefinition;
        public PawnDefinition PawnDefinition => pawnDefinition;
        public ParticipantHandle Participant { get; private set; }
        public GameModeDefinition ActiveGameMode { get; private set; }

        private PawnRootRuntimeReferences _runtime;
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
    }
}
