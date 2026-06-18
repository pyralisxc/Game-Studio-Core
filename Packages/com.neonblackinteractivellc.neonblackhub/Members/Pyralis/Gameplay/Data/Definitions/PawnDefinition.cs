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
        Capability = AuthoringCapability.Movement | AuthoringCapability.Combat, 
        Priority = AuthoringPriority.Primary,
        SetupNodeId = "pawn.definition",
        Lane = "Entity",
        Relevance = "Core definition for a controllable entity, linking its prefab to movement, combat, and animation profiles.",
        AssignmentFields = new[] { nameof(pawnPrefab), nameof(movementProfile), nameof(combatProfile), nameof(animationProfile), nameof(featureModules) },
        NativeSetup = new[] { "PawnRoot" },
        FirstProof = "Assign this Pawn Definition to a Participant Definition or a Spawner in the scene.",
        ExpertAdvice = "PawnDefinition describes the actor body and prefab composition. ParticipantDefinition.inputProfile owns who controls this pawn; keep input off pawn definitions so seats, AI, hands, cursors, and pawn routes share one ownership rule.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/pawn"
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Pawn Definition", fileName = "PawnDefinition", order = 30)]
    public class PawnDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        private const string ActorAnimationDriverTypeFullName = "NeonBlack.Gameplay.Presentation.Animation.ActorAnimationDriver";

        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return PyralisRuntimeValidationIssueUtility.RequiredFrom(GetValidationIssues());
        }

        public GameObject pawnPrefab;
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

            if (pawnPrefab != null)
                AppendPawnPrefabValidationProviderIssues(pawnPrefab, issues);

            return issues;
        }

        private void AppendPawnPrefabValidationProviderIssues(GameObject pawnPrefab, List<string> issues)
        {
            MonoBehaviour[] behaviours = pawnPrefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (IsActorAnimationDriver(behaviours[i]))
                {
                    AppendActorAnimationDriverIssues(pawnPrefab, behaviours[i], issues);
                    continue;
                }

                if (behaviours[i] is not IRuntimeValidationProvider provider)
                    continue;

                foreach (PyralisRuntimeValidationIssue issue in provider.GetRuntimeValidationIssues())
                {
                    if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                        issues.Add($"Pawn Prefab `{pawnPrefab.name}`: {issue.Message}");
                }
            }
        }

        private void AppendActorAnimationDriverIssues(GameObject pawnPrefab, MonoBehaviour animationDriver, List<string> issues)
        {
            if (animationDriver == null)
                return;

            bool hasAnimator = GetObjectProperty<Animator>(animationDriver, "Animator") != null
                || animationDriver.GetComponentInChildren<Animator>(true) != null;
            bool hasPresentationProfile = presentationProfile != null
                || GetObjectProperty<PawnPresentationProfile>(animationDriver, "PresentationProfile") != null;
            bool hasAnimationProfile = animationProfile != null
                || GetObjectProperty<PawnAnimationProfile>(animationDriver, "AnimationProfile") != null;

            if (!hasAnimator)
                issues.Add($"Pawn Prefab `{pawnPrefab.name}`: Add an Animator to the pawn root or visual child so ActorAnimationDriver can drive animation signals.");

            if (!hasPresentationProfile)
                issues.Add($"Pawn Prefab `{pawnPrefab.name}`: Assign PawnDefinition.presentationProfile for participant-spawned pawns, or ActorAnimationDriver.presentationProfile for direct scene actors.");

            if (!hasAnimationProfile)
                issues.Add($"Pawn Prefab `{pawnPrefab.name}`: Assign PawnDefinition.animationProfile so the spawned pawn can apply animation signal bindings.");
        }

        private static bool IsActorAnimationDriver(MonoBehaviour behaviour)
        {
            return behaviour != null && behaviour.GetType().FullName == ActorAnimationDriverTypeFullName;
        }

        private static T GetObjectProperty<T>(object instance, string propertyName) where T : Object
        {
            if (instance == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            System.Reflection.PropertyInfo property = instance.GetType().GetProperty(propertyName);
            return property != null ? property.GetValue(instance) as T : null;
        }
    }
}
