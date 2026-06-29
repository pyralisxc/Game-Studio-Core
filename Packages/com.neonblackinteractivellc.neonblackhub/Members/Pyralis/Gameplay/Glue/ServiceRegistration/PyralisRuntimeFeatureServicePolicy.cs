using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
{
    internal readonly struct PyralisRuntimeFeatureServicePolicy
    {
        public PyralisRuntimeFeatureServicePolicy(
            bool usesCombatServices,
            bool usesEnemyServices,
            bool usesRpgServices,
            bool usesGameFlowServices,
            bool usesScoringServices,
            bool usesFeedbackServices)
        {
            UsesCombatServices = usesCombatServices;
            UsesEnemyServices = usesEnemyServices;
            UsesRpgServices = usesRpgServices;
            UsesGameFlowServices = usesGameFlowServices;
            UsesScoringServices = usesScoringServices;
            UsesFeedbackServices = usesFeedbackServices;
        }

        public bool UsesCombatServices { get; }
        public bool UsesEnemyServices { get; }
        public bool UsesRpgServices { get; }
        public bool UsesGameFlowServices { get; }
        public bool UsesScoringServices { get; }
        public bool UsesFeedbackServices { get; }

        public PyralisRuntimeFeatureServicePolicy WithLoadedSceneEvidence(
            bool usesCombatServices,
            bool usesEnemyServices,
            bool usesRpgServices,
            bool usesGameFlowServices,
            bool usesScoringServices,
            bool usesFeedbackServices)
        {
            return new PyralisRuntimeFeatureServicePolicy(
                UsesCombatServices || usesCombatServices,
                UsesEnemyServices || usesEnemyServices,
                UsesRpgServices || usesRpgServices,
                UsesGameFlowServices || usesGameFlowServices,
                UsesScoringServices || usesScoringServices,
                UsesFeedbackServices || usesFeedbackServices);
        }

        public static PyralisRuntimeFeatureServicePolicy Resolve(SessionDefinition sessionDefinition)
        {
            bool usesCombat = false;
            bool usesEnemy = false;
            bool usesRpg = false;
            bool usesGameFlow = false;
            bool usesScoring = false;
            bool usesFeedback = false;

            GameModeDefinition mode = sessionDefinition != null ? sessionDefinition.defaultGameMode : null;
            if (mode != null)
            {
                usesCombat |= mode.enableCombat || mode.enableHazards;
                usesGameFlow |= mode.enablePickups || mode.enableHazards || mode.enableScore || mode.enableRespawn;
                usesScoring |= mode.enablePickups || mode.enableScore;
                usesFeedback |= mode.enableCombat || mode.enablePickups || mode.enableHazards || mode.enableScore;
            }

            ParticipantDefinition[] participants = sessionDefinition != null ? sessionDefinition.defaultParticipants : null;
            if (participants != null)
            {
                for (int i = 0; i < participants.Length; i++)
                {
                    PawnDefinition pawn = participants[i] != null ? participants[i].defaultPawn : null;
                    if (pawn == null)
                        continue;

                    usesCombat |= pawn.combatProfile != null && pawn.combatProfile.enableCombat;
                }
            }

            return new PyralisRuntimeFeatureServicePolicy(
                usesCombat,
                usesEnemy,
                usesRpg,
                usesGameFlow,
                usesScoring,
                usesFeedback);
        }

        public static PyralisRuntimeFeatureServicePolicy ResolveWithLoadedSceneEvidence(SessionDefinition sessionDefinition)
        {
            return Resolve(sessionDefinition).WithLoadedSceneEvidence(
                PyralisCombatServiceInstaller.ContainsLoadedSceneEvidence(),
                PyralisEnemyServiceInstaller.ContainsLoadedSceneEvidence(),
                PyralisRuntimeSceneSearch.ContainsComponentInNamespace("NeonBlack.Gameplay.Modules.Rpg"),
                PyralisRuntimeSceneSearch.ContainsComponent<GameManager>()
                || PyralisRuntimeSceneSearch.ContainsComponentInNamespace("NeonBlack.Gameplay.Glue.SceneFlow.Arcade2D"),
                PyralisScoringServiceInstaller.ContainsLoadedSceneEvidence(),
                PyralisFeedbackServiceInstaller.ContainsLoadedSceneEvidence());
        }

    }
}
