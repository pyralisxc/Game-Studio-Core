using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Actor-level feature composition profile for enemies.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Enemy Feature Profile", fileName = "EnemyFeatureProfile")]
    public class EnemyFeatureProfile : ScriptableObject
    {
        public EnemyCombatProfile combatProfile;
        public EnemyReactionProfile reactionProfile;
        public FeatureModuleDefinition[] featureModules;

        public List<string> GetValidationIssues(GameObject actorRoot = null, ActorPresentationMode presentationMode = ActorPresentationMode.Billboard2_5D)
        {
            List<string> issues = new List<string>();
            HashSet<string> moduleIds = new HashSet<string>();

            if (featureModules == null)
                return issues;

            for (int i = 0; i < featureModules.Length; i++)
            {
                FeatureModuleDefinition module = featureModules[i];
                if (module == null)
                {
                    issues.Add($"Feature Modules[{i}] is null.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(module.moduleId) && !moduleIds.Add(module.moduleId))
                    issues.Add($"Feature module `{module.moduleId}` is assigned more than once.");

                if (!module.SupportsPresentationMode(presentationMode))
                    issues.Add($"Feature module `{module.moduleId}` does not support `{presentationMode}` presentation mode.");

                List<string> moduleIssues = module.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < moduleIssues.Count; issueIndex++)
                    issues.Add($"Feature `{module.moduleId}`: {moduleIssues[issueIndex]}");

                if (actorRoot == null)
                    continue;

                List<string> actorIssues = module.GetActorCompatibilityIssues(actorRoot, presentationMode, isEnemyActor: true);
                for (int issueIndex = 0; issueIndex < actorIssues.Count; issueIndex++)
                    issues.Add($"Feature `{module.moduleId}`: {actorIssues[issueIndex]}");
            }

            return issues;
        }
    }
}
