using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using System.Collections.Generic;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Primary authored definition for a controllable or simulated pawn.
    /// </summary>
    [AuthoringContract(
        StableId = "pawn.definition",
        Category = "Movement, Combat",
        CapabilityPath = "Character / Pawn Gameplay/Pawn Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Core definition for a controllable entity, linking its prefab to movement, combat, and animation profiles.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/pawn",
        RequiredFields = new[] { nameof(pawnPrefab), nameof(movementProfile), nameof(combatProfile), nameof(traversalProfile), nameof(presentationProfile), nameof(animationProfile) },
        SetupSteps = new[] { "PawnRoot" },
        SuccessChecks = new[] { "Assign this Pawn Definition to the controlling ParticipantDefinition, then let ParticipantSpawnService place the spawned pawn at an authored spawn point." },
        RoleTags = new[] { "PawnDefinition", "PawnPrefab", "ActorBody" },
        Tags = new[] { "capability:Movement", "capability:Combat", "runtime:CharacterPawnGameplay", "runtime:Combat", "lane:Entity", "priority:Primary" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Pawn Definition", fileName = "PawnDefinition", order = 30)]
    public class PawnDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        private const string ActorAnimationDriverTypeFullName = "NeonBlack.Gameplay.Presentation.Animation.ActorAnimationDriver";
        private const string PawnRootTypeFullName = "NeonBlack.Gameplay.Modules.Character.PawnRoot";
        private const string PawnMotorInterfaceFullName = "NeonBlack.Gameplay.Modules.Character.IPawnMotor";
        private const string PawnInputModuleInterfaceFullName = "NeonBlack.Gameplay.Data.Participants.IPawnInputModule";
        private const string PawnPresentationModuleInterfaceFullName = "NeonBlack.Gameplay.Modules.Character.IPawnPresentationModule";
        private const string TopDownHopTypeFullName = "NeonBlack.Gameplay.Modules.Traversal.TopDownHopComponent";

        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return BuildRuntimeValidationIssues();
        }

        public GameObject pawnPrefab;
        public PawnMovementProfile movementProfile;
        public PawnCombatProfile combatProfile;
        public PawnTraversalProfile traversalProfile;
        public PawnPresentationProfile presentationProfile;
        public PawnAnimationProfile animationProfile;

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();
            List<PyralisRuntimeValidationIssue> runtimeIssues = BuildRuntimeValidationIssues();
            for (int i = 0; i < runtimeIssues.Count; i++)
            {
                if (runtimeIssues[i] != null && !string.IsNullOrWhiteSpace(runtimeIssues[i].Message))
                    issues.Add(runtimeIssues[i].Message);
            }

            return issues;
        }

        private List<PyralisRuntimeValidationIssue> BuildRuntimeValidationIssues()
        {
            List<PyralisRuntimeValidationIssue> issues = new List<PyralisRuntimeValidationIssue>();

            if (pawnPrefab == null)
            {
                AddRequired(
                    issues,
                    "Assign a pawn prefab. PawnDefinition is the primary authored unit for runtime-controlled entities.",
                    "PawnDefinition.PawnPrefab.Missing",
                    nameof(pawnPrefab),
                    "Create a prefab with PawnRoot and the lane-specific pawn components, then assign it to PawnDefinition.pawnPrefab.",
                    "PawnDefinition.pawnPrefab references the prefab spawned for this participant.");
            }

            if (pawnPrefab != null)
            {
                AppendPawnPrefabCompositionIssues(pawnPrefab, issues);
                AppendPawnPrefabValidationProviderIssues(pawnPrefab, issues);
            }

            AddIfPresent(issues, GetTopDownJumpFeatureIssue());

            return issues;
        }

        private void AppendPawnPrefabCompositionIssues(GameObject prefab, List<PyralisRuntimeValidationIssue> issues)
        {
            if (!HasComponentOfTypeName(prefab, PawnRootTypeFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Add PawnRoot to the prefab root.", "PawnDefinition.PawnRoot.Missing");
            else if (!HasEnabledComponentOfTypeName(prefab, PawnRootTypeFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Enable PawnRoot on the prefab root before Play Mode.", "PawnDefinition.PawnRoot.Disabled");

            if (!HasComponentImplementing(prefab, PawnMotorInterfaceFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Add the lane motor component that implements IPawnMotor.", "PawnDefinition.PawnMotor.Missing");
            else if (!HasEnabledComponentImplementing(prefab, PawnMotorInterfaceFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Enable the lane motor component before Play Mode.", "PawnDefinition.PawnMotor.Disabled");

            if (!HasComponentImplementing(prefab, PawnInputModuleInterfaceFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Add the lane input adapter that implements IPawnInputModule so the participant InputProfile can reach the pawn.", "PawnDefinition.PawnInput.Missing");
            else if (!HasEnabledComponentImplementing(prefab, PawnInputModuleInterfaceFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Enable the lane input adapter before Play Mode.", "PawnDefinition.PawnInput.Disabled");

            if (!HasComponentImplementing(prefab, PawnPresentationModuleInterfaceFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Add the lane presentation component that implements IPawnPresentationModule.", "PawnDefinition.PawnPresentation.Missing");
            else if (!HasEnabledComponentImplementing(prefab, PawnPresentationModuleInterfaceFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Enable the lane presentation component before Play Mode.", "PawnDefinition.PawnPresentation.Disabled");
        }

        private void AppendPawnPrefabValidationProviderIssues(GameObject pawnPrefab, List<PyralisRuntimeValidationIssue> issues)
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
                    {
                        string issueCode = !string.IsNullOrWhiteSpace(issue.IssueCode)
                            ? issue.IssueCode
                            : $"PawnDefinition.Nested.{i}";
                        issues.Add(new PyralisRuntimeValidationIssue(
                            $"Pawn Prefab `{pawnPrefab.name}`: {issue.Message}",
                            issue.FieldPath,
                            !string.IsNullOrWhiteSpace(issue.TargetLabel) ? issue.TargetLabel : nameof(PawnDefinition),
                            issue.NativeAction,
                            issue.SuccessCheck,
                            issue.Severity,
                            issueCode));
                    }
                }
            }
        }

        private PyralisRuntimeValidationIssue GetTopDownJumpFeatureIssue()
        {
            if (movementProfile == null
                || movementProfile.Effective2DMovementStyle != Pawn2DMovementStyle.TopDownNoGravity
                || !movementProfile.allow2DJump)
            {
                return null;
            }

            if (pawnPrefab != null && HasComponentOfTypeName(pawnPrefab, TopDownHopTypeFullName))
                return null;

            return PyralisRuntimeValidationIssue.Required(
                $"PawnDefinition `{name}`: Top-down/no-gravity Jump is enabled, but the pawn prefab has no TopDownHopComponent component. Add TopDownHopComponent to the pawn root when Jump should lift the visual child, or turn off Allow 2D Jump when this pawn has no top-down hop action.",
                nameof(pawnPrefab),
                nameof(PawnDefinition),
                "Add TopDownHopComponent to the pawn prefab root, assign its TopDownHopProfile, or disable Allow 2D Jump on the movement profile.",
                "Top-down/no-gravity jump has a direct pawn component that can animate the visual child without world gravity.",
                "PawnDefinition.TopDownHop.Missing");
        }

        private void AppendActorAnimationDriverIssues(GameObject pawnPrefab, MonoBehaviour animationDriver, List<PyralisRuntimeValidationIssue> issues)
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
                AddRequired(issues, $"Pawn Prefab `{pawnPrefab.name}`: Add an Animator to the pawn root or visual child so ActorAnimationDriver can drive animation signals.", "PawnDefinition.Animator.Missing");

            if (!hasPresentationProfile)
                AddRequired(issues, $"Pawn Prefab `{pawnPrefab.name}`: Assign PawnDefinition.presentationProfile for participant-spawned pawns, or ActorAnimationDriver.presentationProfile for direct scene actors.", "PawnDefinition.PresentationProfile.Missing", nameof(presentationProfile));

            if (!hasAnimationProfile)
                AddRequired(issues, $"Pawn Prefab `{pawnPrefab.name}`: Assign PawnDefinition.animationProfile so the spawned pawn can apply animation signal bindings.", "PawnDefinition.AnimationProfile.Missing", nameof(animationProfile));
        }

        private static bool IsActorAnimationDriver(MonoBehaviour behaviour)
        {
            return behaviour != null && behaviour.GetType().FullName == ActorAnimationDriverTypeFullName;
        }

        private static bool HasComponentOfTypeName(GameObject root, string fullTypeName)
        {
            if (root == null || string.IsNullOrWhiteSpace(fullTypeName))
                return false;

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && string.Equals(behaviour.GetType().FullName, fullTypeName, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool HasEnabledComponentOfTypeName(GameObject root, string fullTypeName)
        {
            if (root == null || string.IsNullOrWhiteSpace(fullTypeName))
                return false;

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null
                    && behaviour.enabled
                    && behaviour.gameObject.activeSelf
                    && string.Equals(behaviour.GetType().FullName, fullTypeName, System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasComponentImplementing(GameObject root, string interfaceFullTypeName)
        {
            if (root == null || string.IsNullOrWhiteSpace(interfaceFullTypeName))
                return false;

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (ImplementsInterface(behaviours[i], interfaceFullTypeName))
                    return true;
            }

            return false;
        }

        private static bool HasEnabledComponentImplementing(GameObject root, string interfaceFullTypeName)
        {
            if (root == null || string.IsNullOrWhiteSpace(interfaceFullTypeName))
                return false;

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null
                    && behaviour.enabled
                    && behaviour.gameObject.activeSelf
                    && ImplementsInterface(behaviour, interfaceFullTypeName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ImplementsInterface(MonoBehaviour behaviour, string interfaceFullTypeName)
        {
            if (behaviour == null || string.IsNullOrWhiteSpace(interfaceFullTypeName))
                return false;

            System.Type[] interfaces = behaviour.GetType().GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (string.Equals(interfaces[i].FullName, interfaceFullTypeName, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static void AddIfPresent(List<PyralisRuntimeValidationIssue> issues, PyralisRuntimeValidationIssue issue)
        {
            if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
                issues.Add(issue);
        }

        private static void AddRequired(
            List<PyralisRuntimeValidationIssue> issues,
            string message,
            string issueCode,
            string fieldPath = null,
            string nativeAction = null,
            string successCheck = null)
        {
            if (issues == null || string.IsNullOrWhiteSpace(message))
                return;

            issues.Add(PyralisRuntimeValidationIssue.Required(
                message,
                fieldPath,
                nameof(PawnDefinition),
                !string.IsNullOrWhiteSpace(nativeAction)
                    ? nativeAction
                    : "Open PawnDefinition and its assigned pawn prefab, then resolve the named field or component issue.",
                !string.IsNullOrWhiteSpace(successCheck)
                    ? successCheck
                    : "PawnDefinition and its pawn prefab report no validation issues.",
                issueCode));
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
