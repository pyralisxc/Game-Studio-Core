using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Primary authored definition for a controllable or simulated pawn.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Movement, 
        Relevance = "Core definition for a controllable entity, linking its prefab to movement, combat, and animation profiles.",
        AssignmentFields = new[] { nameof(pawnPrefab), nameof(movementProfile), nameof(combatProfile), nameof(animationProfile), nameof(featureModules) },
        NativeSetup = new[] { "PawnRoot" },
        FirstProof = "Assign this Pawn Definition to a Participant Definition or a Spawner in the scene."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Pawn Definition", fileName = "PawnDefinition", order = 30)]
    public class PawnDefinition : ScriptableObject
    {
        public GameObject pawnPrefab;
        public InputProfile defaultInputProfile;
        public PawnMovementProfile movementProfile;
        public PawnCombatProfile combatProfile;
        public PawnTraversalProfile traversalProfile;
        public PawnPresentationProfile presentationProfile;
        public PawnAnimationProfile animationProfile;
        public FeatureModuleDefinition[] featureModules;

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (pawnPrefab == null)
                issues.Add("Assign a pawn prefab. PawnDefinition is the primary authored unit for runtime-controlled entities.");

            ActorPresentationMode? mode = presentationProfile != null ? presentationProfile.presentationMode : null;
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

                if (mode.HasValue && !module.SupportsPresentationMode(mode.Value))
                    issues.Add($"Feature module `{module.moduleId}` does not support `{mode.Value}` presentation mode.");

                List<string> moduleIssues = module.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < moduleIssues.Count; issueIndex++)
                    issues.Add($"Feature `{module.moduleId}`: {moduleIssues[issueIndex]}");

                if (pawnPrefab != null && mode.HasValue)
                {
                    List<string> actorIssues = module.GetActorCompatibilityIssues(pawnPrefab, mode.Value);
                    for (int issueIndex = 0; issueIndex < actorIssues.Count; issueIndex++)
                        issues.Add($"Feature `{module.moduleId}`: {actorIssues[issueIndex]}");
                }
            }

            return issues;
        }
    }
}
