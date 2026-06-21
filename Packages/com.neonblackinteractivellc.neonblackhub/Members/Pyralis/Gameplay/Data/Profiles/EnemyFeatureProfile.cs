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
        NativeSetup = new[] { "Assign Combat and Reaction profiles.", "Add optional Feature Modules (Ambient, etc)." },
        AssignmentFields = new[] { nameof(combatProfile), nameof(reactionProfile) },
        FirstProof = "Confirm the enemy uses all assigned profiles in its runtime behavior.",
        ExpertAdvice = "Use modular profiles to share behaviors across multiple enemy types while keeping the root profile unique per archetype.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/enemies"
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Feature Profile", fileName = "EnemyFeatureProfile")]
    public class EnemyFeatureProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return BuildRuntimeValidationIssues();
        }

        public EnemyCombatProfile combatProfile;
        public EnemyReactionProfile reactionProfile;
        public FeatureModuleDefinition[] featureModules;

        public List<string> GetValidationIssues(GameObject actorRoot = null, ActorPresentationMode presentationMode = ActorPresentationMode.Billboard2_5D)
        {
            List<string> issues = new List<string>();
            List<PyralisRuntimeValidationIssue> runtimeIssues = BuildRuntimeValidationIssues(presentationMode);
            for (int i = 0; i < runtimeIssues.Count; i++)
            {
                if (runtimeIssues[i] != null && !string.IsNullOrWhiteSpace(runtimeIssues[i].Message))
                    issues.Add(runtimeIssues[i].Message);
            }

            if (actorRoot == null)
                return issues;

            AppendActorCompatibilityMessages(issues, actorRoot, presentationMode);
            return issues;
        }

        private List<PyralisRuntimeValidationIssue> BuildRuntimeValidationIssues(ActorPresentationMode presentationMode = ActorPresentationMode.Billboard2_5D)
        {
            List<PyralisRuntimeValidationIssue> issues = new List<PyralisRuntimeValidationIssue>();

            if (combatProfile == null)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    "Combat Profile is missing.",
                    nameof(combatProfile),
                    nameof(EnemyFeatureProfile),
                    "Assign an EnemyCombatProfile to EnemyFeatureProfile.combatProfile.",
                    "EnemyFeatureProfile has a combat profile.",
                    "EnemyFeatureProfile.CombatProfile.Missing"));
            }
            else
            {
                AppendChildIssues(
                    issues,
                    combatProfile.GetRuntimeValidationIssues(),
                    "Combat profile: ",
                    "EnemyFeatureProfile.CombatProfile",
                    nameof(combatProfile));
            }

            if (reactionProfile == null)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    "Reaction Profile is missing.",
                    nameof(reactionProfile),
                    nameof(EnemyFeatureProfile),
                    "Assign an EnemyReactionProfile to EnemyFeatureProfile.reactionProfile.",
                    "EnemyFeatureProfile has a reaction profile.",
                    "EnemyFeatureProfile.ReactionProfile.Missing"));
            }
            else
            {
                AppendChildIssues(
                    issues,
                    reactionProfile.GetRuntimeValidationIssues(),
                    "Reaction profile: ",
                    "EnemyFeatureProfile.ReactionProfile",
                    nameof(reactionProfile));
            }

            HashSet<string> moduleIds = new HashSet<string>();

            if (featureModules == null)
                return issues;

            for (int i = 0; i < featureModules.Length; i++)
            {
                FeatureModuleDefinition module = featureModules[i];
                if (module == null)
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        $"Feature Modules[{i}] is null.",
                        $"{nameof(featureModules)}[{i}]",
                        nameof(EnemyFeatureProfile),
                        "Assign a FeatureModuleDefinition or remove the empty array entry.",
                        "EnemyFeatureProfile feature modules contain no empty entries.",
                        "EnemyFeatureProfile.FeatureModule.Null"));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(module.moduleId) && !moduleIds.Add(module.moduleId))
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        $"Feature module `{module.moduleId}` is assigned more than once.",
                        nameof(featureModules),
                        nameof(EnemyFeatureProfile),
                        "Remove the duplicate feature module or give each module a unique id.",
                        "EnemyFeatureProfile feature module ids are unique.",
                        "EnemyFeatureProfile.FeatureModule.Duplicate"));
                }

                if (!module.SupportsPresentationMode(presentationMode))
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        $"Feature module `{module.moduleId}` does not support `{presentationMode}` presentation mode.",
                        nameof(featureModules),
                        nameof(EnemyFeatureProfile),
                        "Choose a FeatureModuleDefinition that supports this enemy presentation lane or update its supported presentation modes.",
                        "EnemyFeatureProfile feature modules support the selected presentation mode.",
                        "EnemyFeatureProfile.FeatureModule.UnsupportedPresentationMode"));
                }

                AppendChildIssues(
                    issues,
                    module.GetRuntimeValidationIssues(),
                    $"Feature `{module.moduleId}`: ",
                    "EnemyFeatureProfile.FeatureModule." + GetSafeIssueSegment(module.moduleId),
                    $"{nameof(featureModules)}[{i}]");
            }

            return issues;
        }

        private void AppendActorCompatibilityMessages(List<string> issues, GameObject actorRoot, ActorPresentationMode presentationMode)
        {
            if (featureModules == null || actorRoot == null)
                return;

            for (int i = 0; i < featureModules.Length; i++)
            {
                FeatureModuleDefinition module = featureModules[i];
                if (module == null)
                    continue;

                List<string> actorIssues = module.GetActorCompatibilityIssues(actorRoot, presentationMode, isEnemyActor: true);
                for (int issueIndex = 0; issueIndex < actorIssues.Count; issueIndex++)
                    issues.Add($"Feature `{module.moduleId}`: {actorIssues[issueIndex]}");
            }
        }

        private static void AppendChildIssues(
            List<PyralisRuntimeValidationIssue> issues,
            IEnumerable<PyralisRuntimeValidationIssue> childIssues,
            string messagePrefix,
            string issueCodePrefix,
            string fieldPath)
        {
            if (childIssues == null)
                return;

            foreach (PyralisRuntimeValidationIssue issue in childIssues)
            {
                PyralisRuntimeValidationIssue contextualIssue =
                    PyralisRuntimeValidationIssueUtility.WithParentContext(
                        issue,
                        messagePrefix,
                        issueCodePrefix,
                        fieldPath,
                        nameof(EnemyFeatureProfile),
                        "Open the referenced EnemyFeatureProfile child asset and resolve the named issue.",
                        "EnemyFeatureProfile child assets report no validation issues.");

                if (contextualIssue != null && !string.IsNullOrWhiteSpace(contextualIssue.Message))
                    issues.Add(contextualIssue);
            }
        }

        private static string GetSafeIssueSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unnamed";

            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}
