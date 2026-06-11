using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public readonly struct PawnAnimationParameterInfo
    {
        public readonly string Name;
        public readonly AnimatorControllerParameterType ParameterType;

        public PawnAnimationParameterInfo(string name, AnimatorControllerParameterType parameterType)
        {
            Name = name;
            ParameterType = parameterType;
        }

        public override string ToString()
        {
            return $"{Name} ({ParameterType})";
        }
    }

    public readonly struct PawnAnimationMappingSummary
    {
        public readonly bool HasDefinition;
        public readonly bool HasController;
        public readonly int ControllerParameterCount;
        public readonly int BindingCount;
        public readonly int MappedSignalCount;
        public readonly int SupportedSignalCount;
        public readonly int CustomChannelCount;
        public readonly int IssueCount;

        public PawnAnimationMappingSummary(
            bool hasDefinition,
            bool hasController,
            int controllerParameterCount,
            int bindingCount,
            int mappedSignalCount,
            int supportedSignalCount,
            int customChannelCount,
            int issueCount)
        {
            HasDefinition = hasDefinition;
            HasController = hasController;
            ControllerParameterCount = controllerParameterCount;
            BindingCount = bindingCount;
            MappedSignalCount = mappedSignalCount;
            SupportedSignalCount = supportedSignalCount;
            CustomChannelCount = customChannelCount;
            IssueCount = issueCount;
        }

        public float Coverage01
        {
            get
            {
                if (SupportedSignalCount <= 0)
                    return 1f;

                return Mathf.Clamp01((float)MappedSignalCount / SupportedSignalCount);
            }
        }

        public string ReadinessLabel
        {
            get
            {
                if (!HasController)
                    return "Needs base Animator Controller";

                if (BindingCount == 0)
                    return "Ready for suggested bindings";

                if (IssueCount > 0)
                    return "Needs binding review";

                if (!HasDefinition)
                    return "Usable, but definition should be assigned";

                return "Ready";
            }
        }
    }

    public static class PawnAnimationProfileValidation
    {
        private static readonly IReadOnlyDictionary<ActorAnimationSignal, string[]> ParameterAliases = new Dictionary<ActorAnimationSignal, string[]>
        {
            { ActorAnimationSignal.Idle, new[] { "Idle", "IsIdle" } },
            { ActorAnimationSignal.Move, new[] { "Move", "Moving", "IsMoving" } },
            { ActorAnimationSignal.Sprint, new[] { "Sprint", "Sprinting", "IsSprinting" } },
            { ActorAnimationSignal.Crouch, new[] { "Crouch", "Crouching", "IsCrouching" } },
            { ActorAnimationSignal.Jump, new[] { "Jump" } },
            { ActorAnimationSignal.Fall, new[] { "Fall", "Falling", "IsFalling", "IsInAir" } },
            { ActorAnimationSignal.Land, new[] { "Land", "Landed" } },
            { ActorAnimationSignal.Dash, new[] { "Dash", "Dodge", "DodgeFwd", "DiveRoll" } },
            { ActorAnimationSignal.Slide, new[] { "Slide", "IsSliding" } },
            { ActorAnimationSignal.AttackPrimary, new[] { "AttackPrimary", "Attack", "RightPunch", "LeftPunch" } },
            { ActorAnimationSignal.AttackSecondary, new[] { "AttackSecondary", "Attack2", "RightKick", "LeftKick" } },
            { ActorAnimationSignal.AttackAerial, new[] { "AttackAerial", "AerialAttack", "Knee" } },
            { ActorAnimationSignal.BlockStart, new[] { "BlockStart", "Block" } },
            { ActorAnimationSignal.BlockLoop, new[] { "BlockLoop", "Block", "IsBlocking" } },
            { ActorAnimationSignal.BlockEnd, new[] { "BlockEnd" } },
            { ActorAnimationSignal.Hurt, new[] { "Hurt", "Hit", "KnockedBack" } },
            { ActorAnimationSignal.Stagger, new[] { "Stagger", "Stunned" } },
            { ActorAnimationSignal.Death, new[] { "Death", "Die", "Dead", "IsDead" } },
            { ActorAnimationSignal.ClimbStart, new[] { "ClimbStart", "ClimbUp" } },
            { ActorAnimationSignal.ClimbLoop, new[] { "ClimbLoop", "Climb" } },
            { ActorAnimationSignal.ClimbEnd, new[] { "ClimbEnd" } },
            { ActorAnimationSignal.Hang, new[] { "Hang", "Hanging", "IsHanging" } },
            { ActorAnimationSignal.Shimmy, new[] { "Shimmy", "ShimmySpeed" } },
            { ActorAnimationSignal.Interact, new[] { "Interact", "Use" } },
            { ActorAnimationSignal.LookAround, new[] { "LookAround", "Look" } },
            { ActorAnimationSignal.Spawn, new[] { "Spawn", "Respawn" } },
            { ActorAnimationSignal.Despawn, new[] { "Despawn" } },
            { ActorAnimationSignal.SideClimb, new[] { "SideClimb" } },
            { ActorAnimationSignal.ForwardClimb, new[] { "ForwardClimb", "FwdClimb" } },
            { ActorAnimationSignal.LedgeDrop, new[] { "LedgeDrop" } }
        };

        private static readonly IReadOnlyDictionary<string, string[]> BlendTreeFloatAliases = new Dictionary<string, string[]>
        {
            { "Speed", new[] { "Speed", "MoveSpeed", "MovementSpeed", "GroundSpeed", "LocomotionSpeed" } },
            { "NormalizedSpeed", new[] { "NormalizedSpeed", "NormalizedMoveSpeed", "MoveSpeed01", "Speed01", "SpeedNormalized" } },
            { "MoveX", new[] { "MoveX", "Horizontal", "InputX", "DirectionX" } },
            { "MoveY", new[] { "MoveY", "Vertical", "InputY", "DirectionY" } },
            { "MoveZ", new[] { "MoveZ", "Forward", "InputZ", "DirectionZ" } },
            { "VelocityX", new[] { "VelocityX" } },
            { "VelocityY", new[] { "VelocityY", "VerticalVelocity", "YVelocity" } },
            { "VelocityZ", new[] { "VelocityZ", "ForwardVelocity", "ZVelocity" } }
        };

        public static List<string> GetValidationIssues(PawnAnimationProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile == null)
                return issues;

            if (profile.animationDefinition == null)
                issues.Add("Assign an Actor Animation Definition so supported signals are explicit.");

            if (profile.baseController == null)
                issues.Add("Assign a base Animator Controller. The animation stack is Unity-Animator-driven.");

            Dictionary<string, AnimatorControllerParameterType> controllerParameters = profile.baseController != null
                ? GetControllerParameters(profile.baseController)
                : null;
            bool canInspectController = profile.baseController == null || controllerParameters != null;

            if (profile.baseController != null && !canInspectController)
                issues.Add($"Controller '{profile.baseController.name}' cannot be inspected for parameters. Use a standard Animator Controller or Animator Override Controller for editor validation.");

            AddMissingSpriteFrameIssues(profile.baseController, issues);

            if (profile.bindings == null || profile.bindings.Length == 0)
            {
                issues.Add("Add bindings so supported animation signals can drive Animator parameters.");
            }
            else
            {
                HashSet<string> seenBindingKeys = new HashSet<string>();

                foreach (ActorAnimationBinding binding in profile.bindings)
                {
                    if (binding == null)
                        continue;

                    string bindingLabel = GetBindingLabel(binding);
                    string bindingKey = $"{binding.signal}|{binding.customKey}|{binding.bindingType}|{binding.parameterName}";
                    if (!seenBindingKeys.Add(bindingKey))
                        issues.Add($"Binding '{bindingLabel}' is duplicated.");

                    if (binding.signal != ActorAnimationSignal.Custom
                        && profile.animationDefinition != null
                        && !profile.animationDefinition.SupportsSignal(binding.signal))
                        issues.Add($"Binding '{binding.parameterName}' uses {binding.signal}, but that signal is not listed on the assigned definition.");

                    if (binding.signal == ActorAnimationSignal.Custom && string.IsNullOrWhiteSpace(binding.customKey))
                        issues.Add($"Binding '{bindingLabel}' uses Custom but has no custom key.");

                    if (string.IsNullOrWhiteSpace(binding.parameterName))
                    {
                        issues.Add($"Binding '{bindingLabel}' has no Animator parameter name.");
                        continue;
                    }

                    if (controllerParameters == null)
                        continue;

                    if (!controllerParameters.TryGetValue(binding.parameterName.Trim(), out AnimatorControllerParameterType parameterType))
                    {
                        issues.Add($"Binding '{bindingLabel}' targets missing Animator parameter '{binding.parameterName}'.");
                        continue;
                    }

                    if (!IsBindingTypeCompatible(binding.bindingType, parameterType))
                        issues.Add($"Binding '{bindingLabel}' is {binding.bindingType}, but Animator parameter '{binding.parameterName}' is {parameterType}.");
                }
            }

            return issues;
        }

        public static Dictionary<string, List<string>> GetValidationIssueGroups(PawnAnimationProfile profile)
        {
            Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
            List<string> issues = GetValidationIssues(profile);

            foreach (string issue in issues)
            {
                if (string.IsNullOrWhiteSpace(issue))
                    continue;

                AddIssue(groups, GetIssueGroup(issue), issue);
            }

            return groups;
        }

        public static IReadOnlyList<PawnAnimationParameterInfo> GetInspectableParameters(PawnAnimationProfile profile)
        {
            Dictionary<string, AnimatorControllerParameterType> parameters = profile != null
                ? GetControllerParameters(profile.baseController)
                : null;

            if (parameters == null)
                return Array.Empty<PawnAnimationParameterInfo>();

            return parameters
                .OrderBy(pair => pair.Key)
                .Select(pair => new PawnAnimationParameterInfo(pair.Key, pair.Value))
                .ToArray();
        }

        public static IReadOnlyList<PawnAnimationParameterInfo> GetCompatibleParameters(
            PawnAnimationProfile profile,
            ActorAnimationBindingType bindingType)
        {
            return GetInspectableParameters(profile)
                .Where(parameter => IsBindingTypeCompatible(bindingType, parameter.ParameterType))
                .ToArray();
        }

        public static PawnAnimationMappingSummary GetMappingSummary(PawnAnimationProfile profile)
        {
            if (profile == null)
                return new PawnAnimationMappingSummary(false, false, 0, 0, 0, 0, 0, 0);

            IReadOnlyList<PawnAnimationParameterInfo> parameters = GetInspectableParameters(profile);
            List<string> issues = GetValidationIssues(profile);
            HashSet<ActorAnimationSignal> supportedSignals = GetSupportedSignals(profile);
            HashSet<ActorAnimationSignal> mappedSignals = new HashSet<ActorAnimationSignal>(
                (profile.bindings ?? Array.Empty<ActorAnimationBinding>())
                    .Where(binding => binding != null && binding.signal != ActorAnimationSignal.Custom && !string.IsNullOrWhiteSpace(binding.parameterName))
                    .Select(binding => binding.signal));

            int mappedSupportedCount = mappedSignals.Count(signal => supportedSignals.Contains(signal));
            int customChannelCount = (profile.bindings ?? Array.Empty<ActorAnimationBinding>())
                .Count(binding => binding != null && binding.signal == ActorAnimationSignal.Custom && !string.IsNullOrWhiteSpace(binding.customKey));

            return new PawnAnimationMappingSummary(
                profile.animationDefinition != null,
                profile.baseController != null,
                parameters.Count,
                profile.bindings != null ? profile.bindings.Length : 0,
                mappedSupportedCount,
                supportedSignals.Count,
                customChannelCount,
                issues.Count);
        }

        public static void AppendSuggestedBindings(PawnAnimationProfile profile)
        {
            if (profile == null)
                return;

            Dictionary<string, AnimatorControllerParameterType> parameters = GetControllerParameters(profile.baseController);
            if (parameters == null || parameters.Count == 0)
                return;

            List<ActorAnimationBinding> bindings = profile.bindings != null
                ? profile.bindings.Where(binding => binding != null).ToList()
                : new List<ActorAnimationBinding>();

            HashSet<ActorAnimationSignal> existingSignals = new HashSet<ActorAnimationSignal>(
                bindings
                    .Where(binding => binding.signal != ActorAnimationSignal.Custom)
                    .Select(binding => binding.signal));
            HashSet<ActorAnimationSignal> supportedSignals = GetSupportedSignals(profile);

            foreach (KeyValuePair<ActorAnimationSignal, string[]> signalAliases in ParameterAliases)
            {
                if (existingSignals.Contains(signalAliases.Key))
                    continue;

                if (!supportedSignals.Contains(signalAliases.Key))
                    continue;

                if (!TryFindParameter(parameters, signalAliases.Value, out string parameterName, out AnimatorControllerParameterType parameterType))
                    continue;

                bindings.Add(new ActorAnimationBinding
                {
                    signal = signalAliases.Key,
                    parameterName = parameterName,
                    bindingType = ToBindingType(parameterType)
                });
            }

            HashSet<string> existingCustomFloatKeys = new HashSet<string>(
                bindings
                    .Where(binding => binding.signal == ActorAnimationSignal.Custom && binding.bindingType == ActorAnimationBindingType.Float)
                    .Select(binding => binding.customKey),
                StringComparer.Ordinal);

            foreach (KeyValuePair<string, string[]> floatAliases in BlendTreeFloatAliases)
            {
                if (existingCustomFloatKeys.Contains(floatAliases.Key))
                    continue;

                if (!TryFindParameter(parameters, floatAliases.Value, out string parameterName, out AnimatorControllerParameterType parameterType))
                    continue;

                if (parameterType != AnimatorControllerParameterType.Float)
                    continue;

                bindings.Add(new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = floatAliases.Key,
                    parameterName = parameterName,
                    bindingType = ActorAnimationBindingType.Float
                });
            }

            profile.bindings = bindings.ToArray();
        }

        public static void ReplaceWithSuggestedBindings(PawnAnimationProfile profile)
        {
            if (profile == null)
                return;

            profile.bindings = Array.Empty<ActorAnimationBinding>();
            AppendSuggestedBindings(profile);
        }

        public static ActorAnimationBindingType ToBindingType(AnimatorControllerParameterType parameterType)
        {
            return parameterType switch
            {
                AnimatorControllerParameterType.Bool => ActorAnimationBindingType.Bool,
                AnimatorControllerParameterType.Float => ActorAnimationBindingType.Float,
                AnimatorControllerParameterType.Int => ActorAnimationBindingType.Int,
                AnimatorControllerParameterType.Trigger => ActorAnimationBindingType.Trigger,
                _ => ActorAnimationBindingType.Bool
            };
        }

        public static bool IsBindingTypeCompatible(ActorAnimationBindingType bindingType, AnimatorControllerParameterType parameterType)
        {
            return bindingType switch
            {
                ActorAnimationBindingType.Bool => parameterType == AnimatorControllerParameterType.Bool,
                ActorAnimationBindingType.Float => parameterType == AnimatorControllerParameterType.Float,
                ActorAnimationBindingType.Int => parameterType == AnimatorControllerParameterType.Int,
                ActorAnimationBindingType.Trigger => parameterType == AnimatorControllerParameterType.Trigger,
                _ => false
            };
        }

        private static Dictionary<string, AnimatorControllerParameterType> GetControllerParameters(RuntimeAnimatorController controller)
        {
            AnimatorController animatorController = GetAnimatorController(controller);
            if (animatorController == null)
                return controller == null ? new Dictionary<string, AnimatorControllerParameterType>() : null;

            Dictionary<string, AnimatorControllerParameterType> parameters = new Dictionary<string, AnimatorControllerParameterType>();
            foreach (AnimatorControllerParameter parameter in animatorController.parameters)
            {
                if (!parameters.ContainsKey(parameter.name))
                    parameters.Add(parameter.name, parameter.type);
            }

            return parameters;
        }

        private static AnimatorController GetAnimatorController(RuntimeAnimatorController controller)
        {
            if (controller is AnimatorController animatorController)
                return animatorController;

            if (controller is AnimatorOverrideController overrideController)
                return overrideController.runtimeAnimatorController as AnimatorController;

            return null;
        }

        private static void AddMissingSpriteFrameIssues(RuntimeAnimatorController controller, List<string> issues)
        {
            if (controller == null)
                return;

            AnimationClip[] clips = controller.animationClips;
            if (clips == null || clips.Length == 0)
                return;

            int missingFrameCount = 0;
            HashSet<string> affectedClips = new HashSet<string>();

            foreach (AnimationClip clip in clips)
            {
                if (clip == null)
                    continue;

                EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                foreach (EditorCurveBinding binding in bindings)
                {
                    if (binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite")
                        continue;

                    ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    foreach (ObjectReferenceKeyframe keyframe in keyframes)
                    {
                        if (keyframe.value != null)
                            continue;

                        missingFrameCount++;
                        affectedClips.Add(clip.name);
                    }
                }
            }

            if (missingFrameCount == 0)
                return;

            string clipList = string.Join(", ", affectedClips.OrderBy(name => name).Take(6));
            if (affectedClips.Count > 6)
                clipList += $", and {affectedClips.Count - 6} more";

            issues.Add($"Controller '{controller.name}' has {missingFrameCount} missing SpriteRenderer sprite frame reference(s) in clip(s): {clipList}. Reimport or restore the sprite sheet/art package used by this controller before using it on a Sprite2D pawn.");
        }

        private static bool TryFindParameter(
            IReadOnlyDictionary<string, AnimatorControllerParameterType> parameters,
            IReadOnlyList<string> aliases,
            out string parameterName,
            out AnimatorControllerParameterType parameterType)
        {
            foreach (string alias in aliases)
            {
                if (parameters.TryGetValue(alias, out parameterType))
                {
                    parameterName = alias;
                    return true;
                }
            }

            foreach (KeyValuePair<string, AnimatorControllerParameterType> parameter in parameters)
            {
                string normalizedParameter = NormalizeName(parameter.Key);
                if (aliases.Any(alias => NormalizeName(alias) == normalizedParameter))
                {
                    parameterName = parameter.Key;
                    parameterType = parameter.Value;
                    return true;
                }
            }

            parameterName = string.Empty;
            parameterType = AnimatorControllerParameterType.Bool;
            return false;
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            char[] characters = value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray();

            return new string(characters);
        }

        private static HashSet<ActorAnimationSignal> GetSupportedSignals(PawnAnimationProfile profile)
        {
            if (profile != null
                && profile.animationDefinition != null
                && profile.animationDefinition.supportedSignals != null
                && profile.animationDefinition.supportedSignals.Length > 0)
            {
                return new HashSet<ActorAnimationSignal>(
                    profile.animationDefinition.supportedSignals.Where(signal => signal != ActorAnimationSignal.Custom));
            }

            return new HashSet<ActorAnimationSignal>(
                Enum.GetValues(typeof(ActorAnimationSignal))
                    .Cast<ActorAnimationSignal>()
                    .Where(signal => signal != ActorAnimationSignal.Custom));
        }

        private static void AddIssue(Dictionary<string, List<string>> groups, string group, string issue)
        {
            if (!groups.TryGetValue(group, out List<string> groupIssues))
            {
                groupIssues = new List<string>();
                groups.Add(group, groupIssues);
            }

            groupIssues.Add(issue);
        }

        private static string GetIssueGroup(string issue)
        {
            if (issue.Contains("Assign ") || issue.Contains("Add bindings") || issue.Contains("cannot be inspected"))
                return "Setup";

            if (issue.Contains("missing Animator parameter") || issue.Contains("but Animator parameter"))
                return "Animator Parameter Mismatch";

            if (issue.Contains("missing SpriteRenderer sprite frame"))
                return "Sprite Frame References";

            if (issue.Contains("duplicated"))
                return "Duplicate Bindings";

            if (issue.Contains("not listed on the assigned definition"))
                return "Unsupported Signals";

            if (issue.Contains("Custom") || issue.Contains("custom key"))
                return "Custom Channels";

            return "Other";
        }

        private static string GetBindingLabel(ActorAnimationBinding binding)
        {
            if (binding == null)
                return "Null binding";

            if (binding.signal == ActorAnimationSignal.Custom && !string.IsNullOrWhiteSpace(binding.customKey))
                return $"{binding.signal}:{binding.customKey}";

            return binding.signal.ToString();
        }
    }
}
