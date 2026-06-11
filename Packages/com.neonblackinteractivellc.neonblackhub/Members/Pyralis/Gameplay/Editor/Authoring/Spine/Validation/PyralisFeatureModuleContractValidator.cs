using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    /// <summary>
    /// Specialized validator for FeatureModuleDefinition that checks for moduleId-specific contracts.
    /// This was previously part of the manual Guided Authoring system.
    /// </summary>
    public static class PyralisFeatureModuleContractValidator
    {
        public static List<string> GetValidationIssues(FeatureModuleDefinition definition)
        {
            List<string> issues = new List<string>();
            if (definition == null) return issues;

            switch (definition.moduleId)
            {
                case "actor.feedback":
                    ValidateProfile<ActorFeedbackProfile>(definition, issues);
                    break;
                
                case "actor.traversal.topdown-hop":
                    ValidateProfile<TopDownHopProfile>(definition, issues);
                    if (SupportsLane(definition, ActorPresentationMode.ThirdPerson3D))
                        issues.Add("Rigged3D actors should use the 3D traversal jump path instead of topdown-hop.");
                    break;

                case "enemy.reaction":
                    ValidateProfile<EnemyReactionProfile>(definition, issues);
                    break;

                case "actor.combat.reaction":
                    ValidateProfile<ActorCombatReactionProfile>(definition, issues);
                    break;

                case "actor.status":
                    ValidateProfile<ActorStatusEffectProfile>(definition, issues);
                    break;
            }

            return issues;
        }

        private static void ValidateProfile<T>(FeatureModuleDefinition definition, List<string> issues) where T : ScriptableObject
        {
            if (definition.profileAsset == null)
            {
                issues.Add($"Feature module `{definition.moduleId}` requires a `{typeof(T).Name}` profile asset.");
            }
            else if (!(definition.profileAsset is T))
            {
                issues.Add($"Feature module `{definition.moduleId}` profile asset is not `{typeof(T).Name}`.");
            }
        }

        private static bool SupportsLane(FeatureModuleDefinition definition, ActorPresentationMode mode)
        {
            if (definition.supportedPresentationModes == null || definition.supportedPresentationModes.Length == 0)
                return true;

            foreach (var m in definition.supportedPresentationModes)
                if (m == mode) return true;

            return false;
        }
    }
}