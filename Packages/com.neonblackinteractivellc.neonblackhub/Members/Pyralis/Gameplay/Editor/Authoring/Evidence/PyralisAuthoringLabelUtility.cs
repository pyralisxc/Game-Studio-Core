using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringLabelUtility
    {
        public static readonly PyralisAuthoringSemanticTag[] BeginnerLegendTags =
        {
            PyralisAuthoringSemanticTag.Project,
            PyralisAuthoringSemanticTag.Hierarchy,
            PyralisAuthoringSemanticTag.Inspector,
            PyralisAuthoringSemanticTag.Component,
            PyralisAuthoringSemanticTag.Prefab,
            PyralisAuthoringSemanticTag.Definition,
            PyralisAuthoringSemanticTag.Profile,
            PyralisAuthoringSemanticTag.Input,
            PyralisAuthoringSemanticTag.UI,
            PyralisAuthoringSemanticTag.Animation,
            PyralisAuthoringSemanticTag.Audio,
            PyralisAuthoringSemanticTag.PlayMode
        };

        public static string GetSurfaceLabel(PyralisAuthoringActionSurface surface)
        {
            switch (surface)
            {
                case PyralisAuthoringActionSurface.ProjectWindow:
                    return "Project";
                case PyralisAuthoringActionSurface.Hierarchy:
                    return "Hierarchy";
                case PyralisAuthoringActionSurface.Inspector:
                    return "Inspector";
                case PyralisAuthoringActionSurface.PlayMode:
                    return "Play Mode";
                default:
                    return "Authoring";
            }
        }

        public static string GetFieldOrComponentLabel(string fieldOrComponent)
        {
            if (string.IsNullOrWhiteSpace(fieldOrComponent))
                return string.Empty;

            string value = fieldOrComponent.Trim();
            value = StripNativeActionInstructionPrefix(value);
            value = StripNativeActionExtraInstructions(value);
            int memberSeparator = value.LastIndexOf('.');
            if (memberSeparator >= 0 && memberSeparator + 1 < value.Length)
                value = value.Substring(memberSeparator + 1);

            switch (value)
            {
                case "autoRegisterDefaultParticipantsWithoutPlayerInput":
                    return "Auto Register Defaults Without Player Input";
                case "playerInputManager":
                    return "Player Input Manager";
                case "spawnOnRegister":
                    return "Spawn On Register";
                default:
                    return NeonBlack.Gameplay.Core.Contracts.AuthoringCapabilityRegistry.PrettifyTypeName(value);
            }
        }

        public static string GetNativeActionFieldOrComponentName(PyralisAuthoringNativeAction action)
        {
            string value = action.FieldOrComponent ?? string.Empty;
            value = StripNativeActionInstructionPrefix(value.Trim());
            value = StripNativeActionExtraInstructions(value);
            value = GetLastMenuSegment(value);
            return value;
        }

        public static string GetNativeActionInstructionLabel(string fieldOrComponent)
        {
            if (string.IsNullOrWhiteSpace(fieldOrComponent))
                return string.Empty;

            string value = fieldOrComponent.Trim();
            const string addComponentPrefix = "Add Component -> ";
            if (value.StartsWith(addComponentPrefix, StringComparison.Ordinal))
                return AppendInstructionExtra(
                    "Add Component > " + GetFieldOrComponentLabel(value.Substring(addComponentPrefix.Length)),
                    value);

            const string createPrefix = "Create -> ";
            if (value.StartsWith(createPrefix, StringComparison.Ordinal))
            {
                string menuPath = StripNativeActionExtraInstructions(value.Substring(createPrefix.Length)).Replace("->", ">").Trim();
                return AppendInstructionExtra("Create > " + menuPath, value);
            }

            const string createSceneObjectPrefix = "create or select a scene object and add ";
            if (value.StartsWith(createSceneObjectPrefix, StringComparison.Ordinal))
            {
                string componentName = StripNativeActionExtraInstructions(value.Substring(createSceneObjectPrefix.Length));
                return AppendInstructionExtra(
                    "Create/select scene object + Add Component > " + GetFieldOrComponentLabel(componentName),
                    value);
            }

            return GetFieldOrComponentLabel(value);
        }

        public static string GetNativeActionDisplayLabel(PyralisAuthoringNativeAction action)
        {
            string verb = action.Verb ?? string.Empty;
            string semanticName = GetNativeActionFieldOrComponentName(action);
            if (verb.StartsWith("Create", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(action.Target))
                semanticName = action.Target;

            if (string.IsNullOrWhiteSpace(semanticName))
                semanticName = action.Target ?? string.Empty;

            return FirstNonEmpty(JoinNonEmpty(" ", verb, semanticName), action.Target, verb);
        }

        public static string GetNativeActionOwnerLabel(PyralisAuthoringNativeAction action)
        {
            string target = action.Surface == PyralisAuthoringActionSurface.ProjectWindow
                ? GetSurfaceLabel(action.Surface)
                : FirstNonEmpty(action.Target, GetSurfaceLabel(action.Surface));
            string semanticName = GetNativeActionFieldOrComponentName(action);
            return !string.IsNullOrWhiteSpace(semanticName)
                ? target + "." + semanticName
                : target;
        }

        public static string FormatNativeAction(PyralisAuthoringNativeAction action)
        {
            List<string> parts = new List<string>();
            AddIfPresent(parts, action.Verb);
            AddIfPresent(parts, action.Target);
            AddIfPresent(parts, GetNativeActionFieldOrComponentName(action));
            AddIfPresent(parts, action.SuccessCheck);
            return string.Join(" - ", parts);
        }

        private static string StripNativeActionInstructionPrefix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            const string addComponentPrefix = "Add Component -> ";
            if (value.StartsWith(addComponentPrefix, StringComparison.Ordinal))
                return value.Substring(addComponentPrefix.Length).Trim();

            const string createPrefix = "Create -> ";
            if (value.StartsWith(createPrefix, StringComparison.Ordinal))
                return value.Substring(createPrefix.Length).Trim();

            const string createSceneObjectPrefix = "create or select a scene object and add ";
            if (value.StartsWith(createSceneObjectPrefix, StringComparison.Ordinal))
                return value.Substring(createSceneObjectPrefix.Length).Trim();

            return value.Trim();
        }

        private static string StripNativeActionExtraInstructions(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            int extraSeparator = value.IndexOf(';');
            return extraSeparator >= 0 ? value.Substring(0, extraSeparator).Trim() : value.Trim();
        }

        private static string AppendInstructionExtra(string instruction, string originalValue)
        {
            if (string.IsNullOrWhiteSpace(originalValue))
                return instruction;

            int extraSeparator = originalValue.IndexOf(';');
            if (extraSeparator < 0 || extraSeparator + 1 >= originalValue.Length)
                return instruction;

            string extra = originalValue.Substring(extraSeparator + 1).Trim();
            return string.IsNullOrWhiteSpace(extra) ? instruction : instruction + "; " + extra;
        }

        private static string GetLastMenuSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string[] parts = value.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return value.Trim();

            return parts[parts.Length - 1].Trim();
        }

        private static string JoinNonEmpty(string separator, params string[] values)
        {
            if (values == null)
                return string.Empty;

            List<string> parts = new List<string>();
            for (int i = 0; i < values.Length; i++)
                AddIfPresent(parts, values[i]);

            return string.Join(separator, parts);
        }

        private static void AddIfPresent(List<string> parts, string value)
        {
            if (parts == null || string.IsNullOrWhiteSpace(value))
                return;

            parts.Add(value.Trim());
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
                return string.Empty;

            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                    return values[i];
            }

            return string.Empty;
        }

        public static PyralisAuthoringSemanticTag GetSemanticTag(PyralisAuthoringActionSurface surface)
        {
            switch (surface)
            {
                case PyralisAuthoringActionSurface.ProjectWindow:
                    return PyralisAuthoringSemanticTag.Project;
                case PyralisAuthoringActionSurface.Hierarchy:
                    return PyralisAuthoringSemanticTag.Hierarchy;
                case PyralisAuthoringActionSurface.Inspector:
                    return PyralisAuthoringSemanticTag.Inspector;
                case PyralisAuthoringActionSurface.PlayMode:
                    return PyralisAuthoringSemanticTag.PlayMode;
                default:
                    return PyralisAuthoringSemanticTag.Authoring;
            }
        }

        public static string GetSemanticTagLabel(PyralisAuthoringSemanticTag tag)
        {
            switch (tag)
            {
                case PyralisAuthoringSemanticTag.Project:
                    return "Project";
                case PyralisAuthoringSemanticTag.Hierarchy:
                    return "Hierarchy";
                case PyralisAuthoringSemanticTag.Inspector:
                    return "Inspector";
                case PyralisAuthoringSemanticTag.Component:
                    return "Component";
                case PyralisAuthoringSemanticTag.Prefab:
                    return "Prefab";
                case PyralisAuthoringSemanticTag.Definition:
                    return "Definition";
                case PyralisAuthoringSemanticTag.Profile:
                    return "Profile";
                case PyralisAuthoringSemanticTag.Input:
                    return "Input";
                case PyralisAuthoringSemanticTag.UI:
                    return "UI";
                case PyralisAuthoringSemanticTag.Animation:
                    return "Animation";
                case PyralisAuthoringSemanticTag.Audio:
                    return "Audio";
                case PyralisAuthoringSemanticTag.PlayMode:
                    return "Play Mode";
                default:
                    return "Authoring";
            }
        }

        public static UnityEngine.Color GetSemanticTagColor(PyralisAuthoringSemanticTag tag)
        {
            switch (tag)
            {
                case PyralisAuthoringSemanticTag.Project:
                    return new UnityEngine.Color(0.18f, 0.48f, 0.82f);
                case PyralisAuthoringSemanticTag.Hierarchy:
                    return new UnityEngine.Color(0.22f, 0.62f, 0.44f);
                case PyralisAuthoringSemanticTag.Inspector:
                    return new UnityEngine.Color(0.83f, 0.53f, 0.18f);
                case PyralisAuthoringSemanticTag.Component:
                    return new UnityEngine.Color(0.58f, 0.46f, 0.86f);
                case PyralisAuthoringSemanticTag.Prefab:
                    return new UnityEngine.Color(0.22f, 0.68f, 0.76f);
                case PyralisAuthoringSemanticTag.Definition:
                    return new UnityEngine.Color(0.72f, 0.38f, 0.72f);
                case PyralisAuthoringSemanticTag.Profile:
                    return new UnityEngine.Color(0.52f, 0.66f, 0.25f);
                case PyralisAuthoringSemanticTag.Input:
                    return new UnityEngine.Color(0.77f, 0.45f, 0.28f);
                case PyralisAuthoringSemanticTag.UI:
                    return new UnityEngine.Color(0.38f, 0.58f, 0.9f);
                case PyralisAuthoringSemanticTag.Animation:
                    return new UnityEngine.Color(0.84f, 0.46f, 0.58f);
                case PyralisAuthoringSemanticTag.Audio:
                    return new UnityEngine.Color(0.42f, 0.7f, 0.42f);
                case PyralisAuthoringSemanticTag.PlayMode:
                    return new UnityEngine.Color(0.88f, 0.22f, 0.42f);
                default:
                    return new UnityEngine.Color(0.52f, 0.52f, 0.52f);
            }
        }

        public static string GetEvidenceLabel(PyralisAuthoringEvidenceState state)
        {
            switch (state)
            {
                case PyralisAuthoringEvidenceState.Missing:
                    return "Needs setup";
                case PyralisAuthoringEvidenceState.CandidateDetected:
                    return "Found candidate surface";
                case PyralisAuthoringEvidenceState.LinkedToActiveSetup:
                    return "Linked to active setup";
                case PyralisAuthoringEvidenceState.Validated:
                    return "Validated";
                case PyralisAuthoringEvidenceState.PlayProven:
                    return "Play-proven";
                default:
                    return "Not needed for this proof";
            }
        }

        public static string GetProofLabel(PyralisAuthoringProofState state)
        {
            switch (state)
            {
                case PyralisAuthoringProofState.ReadyToAttempt:
                    return "Ready to attempt proof";
                case PyralisAuthoringProofState.NotRun:
                    return "Play proof not run";
                case PyralisAuthoringProofState.Passed:
                    return "Play proof passed";
                case PyralisAuthoringProofState.Stale:
                    return "Play proof stale";
                default:
                    return "Not ready for proof";
            }
        }
    }
}
