using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Primary authored definition for a controllable or simulated pawn.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Definitions/Pawn Definition", fileName = "PawnDefinition")]
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
            if (movementProfile == null)
                issues.Add("Movement Profile is missing. Shared module composition works best when movement is data-authored.");
            if (defaultInputProfile == null)
                issues.Add("Assign an Input Profile if this pawn should be driven by participant-owned inputs.");
            if (animationProfile == null)
                issues.Add("Assign an Animation Profile. The clean-slate pawn stack expects a data-driven animation asset.");
            if (presentationProfile == null)
                issues.Add("Assign a Presentation Profile so the animation driver knows whether this pawn is 2D, 2.5D, or rigged 3D.");

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
