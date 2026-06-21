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
        CapabilityPath = "Character / Pawn Gameplay/Pawn Definition",
        Priority = AuthoringPriority.Primary,
        SetupNodeId = "pawn.definition",
        Lane = "Entity",
        Relevance = "Core definition for a controllable entity, linking its prefab to movement, combat, and animation profiles.",
        RoleTags = new[] { "PawnDefinition", "PawnPrefab", "ActorBody" },
        AssignmentFields = new[] { nameof(pawnPrefab), nameof(movementProfile), nameof(combatProfile), nameof(animationProfile), nameof(featureModules) },
        NativeSetup = new[] { "PawnRoot" },
        FirstProof = "Assign this Pawn Definition to the controlling ParticipantDefinition, then let ParticipantSpawnService place the spawned pawn at an authored spawn point.",
        ExpertAdvice = "PawnDefinition describes the actor body and prefab composition. ParticipantDefinition.inputProfile owns who controls this pawn; keep input off pawn definitions so seats, AI, hands, cursors, and pawn routes share one ownership rule.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/pawn"
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Pawn Definition", fileName = "PawnDefinition", order = 30)]
    public class PawnDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        private const string ActorAnimationDriverTypeFullName = "NeonBlack.Gameplay.Presentation.Animation.ActorAnimationDriver";
        private const string ActorFeatureHostTypeFullName = "NeonBlack.Gameplay.Features.Composition.ActorFeatureHost";
        private const string PawnRootTypeFullName = "NeonBlack.Gameplay.Characters.PawnRoot";
        private const string PawnMotorInterfaceFullName = "NeonBlack.Gameplay.Characters.IPawnMotor";
        private const string PawnInputModuleInterfaceFullName = "NeonBlack.Gameplay.Characters.IPawnInputModule";
        private const string PawnPresentationModuleInterfaceFullName = "NeonBlack.Gameplay.Characters.IPawnPresentationModule";
        private const string TopDownHopModuleId = "actor.traversal.topdown-hop";

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
        public FeatureModuleDefinition[] featureModules;

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

            ActorPresentationMode? mode = presentationProfile != null ? presentationProfile.presentationMode : null;
            HashSet<string> moduleIds = new HashSet<string>();
            if (featureModules != null)
            {
                for (int i = 0; i < featureModules.Length; i++)
                {
                    FeatureModuleDefinition module = featureModules[i];
                    if (module == null)
                    {
                        AddRequired(
                            issues,
                            $"Feature Modules[{i}] is null.",
                            "PawnDefinition.FeatureModule.Null",
                            $"featureModules[{i}]");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(module.moduleId) && !moduleIds.Add(module.moduleId))
                    {
                        AddRequired(
                            issues,
                            $"Feature module `{module.moduleId}` is assigned more than once.",
                            "PawnDefinition.FeatureModule.Duplicate",
                            nameof(featureModules));
                    }

                    if (mode.HasValue && !module.SupportsPresentationMode(mode.Value))
                    {
                        AddRequired(
                            issues,
                            $"Feature module `{module.moduleId}` does not support `{mode.Value}` presentation mode.",
                            "PawnDefinition.FeatureModule.UnsupportedPresentationMode",
                            nameof(featureModules));
                    }

                    List<string> moduleIssues = module.GetValidationIssues();
                    for (int issueIndex = 0; issueIndex < moduleIssues.Count; issueIndex++)
                    {
                        AddRequired(
                            issues,
                            $"Feature `{module.moduleId}`: {moduleIssues[issueIndex]}",
                            $"PawnDefinition.FeatureModule.{GetSafeIssueSegment(module.moduleId)}.{issueIndex}",
                            nameof(featureModules));
                    }

                    if (pawnPrefab != null && mode.HasValue)
                    {
                        List<string> actorIssues = module.GetActorCompatibilityIssues(pawnPrefab, mode.Value);
                        for (int issueIndex = 0; issueIndex < actorIssues.Count; issueIndex++)
                        {
                            AddRequired(
                                issues,
                                $"Feature `{module.moduleId}`: {actorIssues[issueIndex]}",
                                $"PawnDefinition.FeatureModule.Compatibility.{GetSafeIssueSegment(module.moduleId)}.{issueIndex}",
                                nameof(featureModules));
                        }
                    }
                }
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

            if (HasEnabledFeatureModules() && !HasComponentOfTypeName(prefab, ActorFeatureHostTypeFullName))
                AddRequired(issues, $"Pawn Prefab `{prefab.name}`: Add ActorFeatureHost to the prefab root because PawnDefinition.featureModules contains enabled optional modules.", "PawnDefinition.ActorFeatureHost.Missing");

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

            if (HasFeatureModule(TopDownHopModuleId))
                return null;

            return PyralisRuntimeValidationIssue.Required(
                $"PawnDefinition `{name}`: Top-down/no-gravity Jump is enabled, but no TopDownHop feature module is assigned to PawnDefinition.featureModules. Add a FeatureModuleDefinition with module id `{TopDownHopModuleId}` when Jump should lift the visual child, or turn off Allow 2D Jump when this pawn has no top-down hop action.",
                nameof(featureModules),
                nameof(PawnDefinition),
                "Assign a TopDownHop FeatureModuleDefinition to PawnDefinition.featureModules, or disable Allow 2D Jump on the movement profile.",
                "Top-down/no-gravity jump has a feature module that can animate the visual child without world gravity.",
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

        private bool HasEnabledFeatureModules()
        {
            if (featureModules == null)
                return false;

            for (int i = 0; i < featureModules.Length; i++)
            {
                FeatureModuleDefinition module = featureModules[i];
                if (module != null && module.enabledByDefault)
                    return true;
            }

            return false;
        }

        private bool HasFeatureModule(string moduleId)
        {
            if (featureModules == null || string.IsNullOrWhiteSpace(moduleId))
                return false;

            for (int i = 0; i < featureModules.Length; i++)
            {
                FeatureModuleDefinition module = featureModules[i];
                if (module != null && string.Equals(module.moduleId, moduleId, System.StringComparison.Ordinal))
                    return true;
            }

            return false;
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

        private static T GetObjectProperty<T>(object instance, string propertyName) where T : Object
        {
            if (instance == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            System.Reflection.PropertyInfo property = instance.GetType().GetProperty(propertyName);
            return property != null ? property.GetValue(instance) as T : null;
        }
    }
}
