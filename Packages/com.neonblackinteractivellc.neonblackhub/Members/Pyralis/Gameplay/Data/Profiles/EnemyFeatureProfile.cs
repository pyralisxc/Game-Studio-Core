using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        Relevance = "The central configuration for an enemy; binds combat and reaction profiles together.",
        NativeSetup = new[] { "Create Asset.", "Assign Combat and Reaction profiles.", "Add optional Feature Modules (Ambient, etc)." },
        AssignmentFields = new[] { nameof(combatProfile), nameof(reactionProfile) },
        FirstProof = "Confirm the enemy uses all assigned profiles in its runtime behavior.",
        ExpertAdvice = "Use modular profiles to share behaviors across multiple enemy types while keeping the root profile unique per archetype.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/enemies"
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Feature Profile", fileName = "EnemyFeatureProfile")]
    public class EnemyFeatureProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (combatProfile == null) yield return "Combat Profile is missing.";
            if (reactionProfile == null) yield return "Reaction Profile is missing.";

            foreach (var issue in GetValidationIssues())
                yield return issue;
        }

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
