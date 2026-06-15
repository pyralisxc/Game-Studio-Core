using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Core.Runtime
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
                AppendModuleSignals(
                    mode.requiredFeatureModules,
                    ref usesCombat,
                    ref usesEnemy,
                    ref usesRpg,
                    ref usesGameFlow,
                    ref usesScoring,
                    ref usesFeedback);
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
                    AppendModuleSignals(
                        pawn.featureModules,
                        ref usesCombat,
                        ref usesEnemy,
                        ref usesRpg,
                        ref usesGameFlow,
                        ref usesScoring,
                        ref usesFeedback);
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

        private static void AppendModuleSignals(
            FeatureModuleDefinition[] modules,
            ref bool usesCombat,
            ref bool usesEnemy,
            ref bool usesRpg,
            ref bool usesGameFlow,
            ref bool usesScoring,
            ref bool usesFeedback)
        {
            if (modules == null)
                return;

            for (int i = 0; i < modules.Length; i++)
            {
                FeatureModuleDefinition module = modules[i];
                if (module == null)
                    continue;

                string moduleText = JoinSignals(module.moduleId, module.authoringCategory, module.featureTags);
                usesCombat |= Contains(moduleText, "combat")
                    || Contains(moduleText, "weapon")
                    || Contains(moduleText, "projectile")
                    || Contains(moduleText, "hazard");
                usesEnemy |= Contains(moduleText, "enemy")
                    || Contains(moduleText, "npc");
                usesRpg |= Contains(moduleText, "rpg")
                    || Contains(moduleText, "inventory")
                    || Contains(moduleText, "quest")
                    || Contains(moduleText, "dialogue")
                    || Contains(moduleText, "vendor")
                    || Contains(moduleText, "equipment")
                    || Contains(moduleText, "skill");
                usesGameFlow |= Contains(moduleText, "gameflow")
                    || Contains(moduleText, "game flow")
                    || Contains(moduleText, "sessionflow")
                    || Contains(moduleText, "session flow")
                    || Contains(moduleText, "arcade")
                    || Contains(moduleText, "loop")
                    || Contains(moduleText, "respawn")
                    || Contains(moduleText, "hazard")
                    || Contains(moduleText, "pickup")
                    || Contains(moduleText, "collectible")
                    || Contains(moduleText, "score");
                usesScoring |= Contains(moduleText, "score")
                    || Contains(moduleText, "scoring")
                    || Contains(moduleText, "leaderboard")
                    || Contains(moduleText, "objective")
                    || Contains(moduleText, "pickup")
                    || Contains(moduleText, "collectible")
                    || Contains(moduleText, "reward");
                usesFeedback |= Contains(moduleText, "feedback")
                    || Contains(moduleText, "hud")
                    || Contains(moduleText, "ui")
                    || Contains(moduleText, "floating")
                    || Contains(moduleText, "popup")
                    || Contains(moduleText, "damage")
                    || Contains(moduleText, "heal")
                    || Contains(moduleText, "status")
                    || Contains(moduleText, "alert");

                ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId(module.moduleId);
                if (contract == null)
                    continue;

                usesCombat |= contract.Capability.HasFlag(AuthoringCapability.Combat)
                    || contract.Capability.HasFlag(AuthoringCapability.CombatState)
                    || contract.Capability.HasFlag(AuthoringCapability.CombatSensors)
                    || contract.Capability.HasFlag(AuthoringCapability.RangedFlow);
                usesEnemy |= Contains(contract.AuthoringLane, "enemy")
                    || Contains(contract.AuthoringCategory, "enemy");
                usesRpg |= Contains(contract.AuthoringLane, "rpg")
                    || Contains(contract.AuthoringCategory, "rpg")
                    || Contains(contract.Relevance, "inventory")
                    || Contains(contract.Relevance, "quest")
                    || Contains(contract.Relevance, "dialogue")
                    || Contains(contract.Relevance, "vendor")
                    || Contains(contract.Relevance, "equipment")
                    || Contains(contract.Relevance, "skill");
                usesGameFlow |= Contains(contract.AuthoringLane, "gameflow")
                    || Contains(contract.AuthoringLane, "game flow")
                    || Contains(contract.AuthoringCategory, "gameflow")
                    || Contains(contract.AuthoringCategory, "game flow")
                    || Contains(contract.Relevance, "session flow")
                    || Contains(contract.Relevance, "game loop")
                    || Contains(contract.Relevance, "respawn");
                usesScoring |= contract.Capability.HasFlag(AuthoringCapability.Scoring)
                    || Contains(contract.AuthoringLane, "scoring")
                    || Contains(contract.AuthoringCategory, "scoring")
                    || Contains(contract.Relevance, "score")
                    || Contains(contract.Relevance, "leaderboard")
                    || Contains(contract.Relevance, "objective");
                usesFeedback |= contract.Capability.HasFlag(AuthoringCapability.UI)
                    || contract.Capability.HasFlag(AuthoringCapability.VFX)
                    || Contains(contract.AuthoringLane, "feedback")
                    || Contains(contract.AuthoringCategory, "feedback")
                    || Contains(contract.Relevance, "feedback")
                    || Contains(contract.Relevance, "hud")
                    || Contains(contract.Relevance, "popup");
            }
        }

        private static string JoinSignals(string first, string second, string[] rest)
        {
            string result = (first ?? string.Empty) + " " + (second ?? string.Empty);
            if (rest == null)
                return result;

            for (int i = 0; i < rest.Length; i++)
                result += " " + (rest[i] ?? string.Empty);

            return result;
        }

        private static bool Contains(string value, string token)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
