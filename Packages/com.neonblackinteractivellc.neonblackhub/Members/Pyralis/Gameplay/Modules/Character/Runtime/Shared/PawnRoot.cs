using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Character
{
    /// <summary>
    /// Composition root for participant-owned pawns.
    /// </summary>
    [AuthoringContract(
        StableId = "pawn.root",
        Category = "Movement, Session",
        CapabilityPath = "Character/Pawn Gameplay/Pawn Root",
        Surface = AuthoringSurface.Goal,
        Summary = "The root coordinator for participant-owned pawns. Handles profile application after a participant spawns the pawn prefab.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/pawns",
        PrerequisiteStableIds = new[] { "pawn.definition" },
        RouteStage = "Pawn Prefab",
        RouteOrder = 80,
        SetupDomain = "Pawn",
        ProofTarget = "Pawn prefab has a PawnRoot that receives participant-owned profile setup.",
        NativeActionKind = AuthoringActionKind.AddComponent,
        SetupSteps = new[] { "Add to Pawn prefab root" },
        SuccessChecks = new[] { "Pawn spawns and receives its defined movement/combat profiles." },
        RoleTags = new[] { "IntentRouteEssential", "ParticipantRouteSupport", "PawnRoot" },
        Tags = new[] { "capability:Movement", "capability:Session", "runtime:CharacterPawnGameplay" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Characters/Pawn Root")]
    public partial class PawnRoot : MonoBehaviour, IPawnParticipantInitializer, IPawnParticipantStateReader
    {
        [Tooltip("Optional prefab-local fallback. Spawned pawns receive this from ParticipantDefinition.defaultPawn, so beginner prefab setup usually leaves this empty.")]
        [SerializeField] private PawnDefinition pawnDefinition;
        public PawnDefinition PawnDefinition => pawnDefinition;
        public ParticipantHandle Participant { get; private set; }
        public GameModeDefinition ActiveGameMode { get; private set; }

        private PawnRootRuntimeReferences _runtime;
        public void InitializeForParticipant(ParticipantHandle participant, GameModeDefinition gameMode)
        {
            Participant = participant;
            ActiveGameMode = gameMode;

            if (participant != null && participant.PawnDefinition != null)
                pawnDefinition = participant.PawnDefinition;

            ApplyProfiles();
        }
    }
}
