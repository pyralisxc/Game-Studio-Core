using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using System.Collections.Generic;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Category = "Combat",
        CapabilityPath = "Combat/Actions/Enemy Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "The central configuration for an enemy; binds combat and reaction profiles together.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/enemies",
        RequiredFields = new[] { nameof(combatProfile), nameof(reactionProfile) },
        SetupSteps = new[] { "Assign Combat and Reaction profiles.", "Add optional enemy module components directly to the enemy prefab." },
        SuccessChecks = new[] { "Confirm the enemy uses all assigned profiles in its runtime behavior." },
        Tags = new[] { "capability:Combat", "runtime:Combat" },
        Selectable = false
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Profile", fileName = "EnemyProfile")]
    public class EnemyProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return BuildRuntimeValidationIssues();
        }

        public EnemyCombatProfile combatProfile;
        public EnemyReactionProfile reactionProfile;

        public List<string> GetValidationIssues(GameObject actorRoot = null, ActorPresentationMode presentationMode = ActorPresentationMode.Billboard2_5D)
        {
            List<string> issues = new List<string>();
            List<RuntimeValidationIssue> runtimeIssues = BuildRuntimeValidationIssues(presentationMode);
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

        private List<RuntimeValidationIssue> BuildRuntimeValidationIssues(ActorPresentationMode presentationMode = ActorPresentationMode.Billboard2_5D)
        {
            List<RuntimeValidationIssue> issues = new List<RuntimeValidationIssue>();

            if (combatProfile == null)
            {
                issues.Add(RuntimeValidationIssue.Required(
                    "Combat Profile is missing.",
                    nameof(combatProfile),
                    nameof(EnemyProfile),
                    "Assign an EnemyCombatProfile to EnemyProfile.combatProfile.",
                    "EnemyProfile has a combat profile.",
                    "EnemyProfile.CombatProfile.Missing"));
            }
            else
            {
                AppendChildIssues(
                    issues,
                    combatProfile.GetRuntimeValidationIssues(),
                    "Combat profile: ",
                    "EnemyProfile.CombatProfile",
                    nameof(combatProfile));
            }

            if (reactionProfile == null)
            {
                issues.Add(RuntimeValidationIssue.Required(
                    "Reaction Profile is missing.",
                    nameof(reactionProfile),
                    nameof(EnemyProfile),
                    "Assign an EnemyReactionProfile to EnemyProfile.reactionProfile.",
                    "EnemyProfile has a reaction profile.",
                    "EnemyProfile.ReactionProfile.Missing"));
            }
            else
            {
                AppendChildIssues(
                    issues,
                    reactionProfile.GetRuntimeValidationIssues(),
                    "Reaction profile: ",
                    "EnemyProfile.ReactionProfile",
                    nameof(reactionProfile));
            }

            return issues;
        }

        private void AppendActorCompatibilityMessages(List<string> issues, GameObject actorRoot, ActorPresentationMode presentationMode)
        {
        }

        private static void AppendChildIssues(
            List<RuntimeValidationIssue> issues,
            IEnumerable<RuntimeValidationIssue> childIssues,
            string messagePrefix,
            string issueCodePrefix,
            string fieldPath)
        {
            if (childIssues == null)
                return;

            foreach (RuntimeValidationIssue issue in childIssues)
            {
                RuntimeValidationIssue contextualIssue =
                    RuntimeValidationIssueUtility.WithParentContext(
                        issue,
                        messagePrefix,
                        issueCodePrefix,
                        fieldPath,
                        nameof(EnemyProfile),
                        "Open the referenced EnemyProfile child asset and resolve the named issue.",
                        "EnemyProfile child assets report no validation issues.");

                if (contextualIssue != null && !string.IsNullOrWhiteSpace(contextualIssue.Message))
                    issues.Add(contextualIssue);
            }
        }

    }
}
