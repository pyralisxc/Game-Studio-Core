using System;
using System.Collections.Generic;
using Pys.Authoring.Contracts;

namespace Pys.Authoring.Editor.Contracts
{
    public static class AuthoringContractResolver
    {
        public static IReadOnlyList<ResolvedAuthoringContract> Resolve(Type type)
        {
            List<ResolvedAuthoringContract> resolved = new List<ResolvedAuthoringContract>();
            if (type == null)
                return resolved;

            object[] attributes = type.GetCustomAttributes(typeof(AuthoringContractAttribute), true);
            for (int i = 0; i < attributes.Length; i++)
            {
                AuthoringContractAttribute attribute = attributes[i] as AuthoringContractAttribute;
                if (attribute == null)
                    continue;

                resolved.Add(Resolve(type, attribute, i));
            }

            return resolved;
        }

        private static ResolvedAuthoringContract Resolve(Type type, AuthoringContractAttribute attribute, int index)
        {
            string stableId = !string.IsNullOrWhiteSpace(attribute.StableId)
                ? attribute.StableId.Trim()
                : type.FullName + "#" + index;

            ResolvedAuthoringContract contract = new ResolvedAuthoringContract(stableId, type.FullName)
            {
                DisplayName = FirstNonEmpty(attribute.DisplayName, Prettify(type.Name)),
                Category = attribute.Category ?? string.Empty,
                CapabilityPath = NormalizePath(attribute.CapabilityPath),
                Surface = attribute.Surface,
                Summary = attribute.Summary ?? string.Empty,
                DocumentationUrl = attribute.DocumentationUrl ?? string.Empty,
                RouteStage = attribute.RouteStage ?? string.Empty,
                RouteOrder = attribute.RouteOrder,
                SetupDomain = attribute.SetupDomain ?? string.Empty,
                ProofTarget = attribute.ProofTarget ?? string.Empty,
                SuccessDescription = attribute.SuccessDescription ?? string.Empty,
                ReadinessHint = attribute.ReadinessHint ?? string.Empty,
                ValidationOwnerStableId = attribute.ValidationOwnerStableId ?? string.Empty,
                NativeActionKind = attribute.NativeActionKind,
                Selectable = attribute.Selectable
            };

            AddRange(contract.Tags, attribute.Tags);
            AddRange(contract.PrerequisiteStableIds, attribute.PrerequisiteStableIds);
            AddRange(contract.ExpectedEvidence, attribute.ExpectedEvidence);
            AddRange(contract.CompletionSignals, attribute.CompletionSignals);
            AddRange(contract.IntentToggles, attribute.IntentToggles);
            AddRange(contract.IntentLanes, attribute.IntentLanes);
            AddRange(contract.CompatibleStableIds, attribute.CompatibleStableIds);
            AddRange(contract.SupportingStableIds, attribute.SupportingStableIds);
            AddRange(contract.HoverExplanations, attribute.HoverExplanations);
            AddRange(contract.RequiredFields, attribute.RequiredFields);
            AddTypeRange(contract.RequiredComponents, attribute.RequiredComponents);
            AddRange(contract.RequiredComponents, attribute.RequiredComponentNames);
            AddTypeRange(contract.RequiredInterfaces, attribute.RequiredInterfaces);
            AddRange(contract.RequiredInterfaces, attribute.RequiredInterfaceNames);
            AddRange(contract.SetupSteps, attribute.SetupSteps);
            AddRange(contract.SuccessChecks, attribute.SuccessChecks);
            AddRange(contract.OwnershipClaims, attribute.OwnershipClaims);
            AddRange(contract.RoleTags, attribute.RoleTags);
            AddMetadataGaps(contract);
            return contract;
        }

        private static void AddMetadataGaps(ResolvedAuthoringContract contract)
        {
            if (string.IsNullOrWhiteSpace(contract.Category))
                contract.MetadataGaps.Add("category");

            if (contract.Surface == AuthoringSurface.Auto)
                contract.MetadataGaps.Add("surface");

            if (string.IsNullOrWhiteSpace(contract.CapabilityPath) && contract.Selectable)
                contract.MetadataGaps.Add("capabilityPath");
        }

        private static void AddRange(List<string> target, IEnumerable<string> values)
        {
            if (target == null || values == null)
                return;

            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    target.Add(value.Trim());
            }
        }

        private static void AddTypeRange(List<string> target, IEnumerable<Type> values)
        {
            if (target == null || values == null)
                return;

            foreach (Type value in values)
            {
                if (value != null && !string.IsNullOrWhiteSpace(value.FullName))
                    target.Add(value.FullName);
            }
        }

        private static string FirstNonEmpty(string preferred, string fallback)
        {
            return !string.IsNullOrWhiteSpace(preferred) ? preferred.Trim() : fallback ?? string.Empty;
        }

        private static string NormalizePath(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().Replace('\\', '/').Trim('/');
        }

        public static string Prettify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            List<char> output = new List<char>(value.Length + 8);
            for (int i = 0; i < value.Length; i++)
            {
                char current = NormalizeSeparator(value[i]);
                if (current == ' ')
                {
                    AppendSpace(output);
                    continue;
                }

                char previous = PreviousNonSpace(output);
                char next = i + 1 < value.Length ? NormalizeSeparator(value[i + 1]) : '\0';
                if (ShouldInsertSpace(previous, current, next))
                    output.Add(' ');

                output.Add(current);
            }

            return FormatWords(new string(output.ToArray()).Trim());
        }

        private static char NormalizeSeparator(char value)
        {
            return value == '_'
                || value == '-'
                || value == '.'
                || value == '/'
                || value == '\\'
                || value == ':'
                    ? ' '
                    : value;
        }

        private static void AppendSpace(List<char> output)
        {
            if (output.Count > 0 && output[output.Count - 1] != ' ')
                output.Add(' ');
        }

        private static char PreviousNonSpace(List<char> output)
        {
            for (int i = output.Count - 1; i >= 0; i--)
            {
                if (output[i] != ' ')
                    return output[i];
            }

            return '\0';
        }

        private static bool ShouldInsertSpace(char previous, char current, char next)
        {
            if (previous == '\0' || current == '\0' || previous == ' ')
                return false;

            if (char.IsLower(previous) && char.IsUpper(current))
                return true;

            if (char.IsUpper(previous) && char.IsUpper(current) && char.IsLower(next))
                return true;

            if (char.IsLetter(previous) && char.IsDigit(current))
                return true;

            if (char.IsDigit(previous) && char.IsLetter(current))
                return !IsDimensionLetter(current);

            return false;
        }

        private static bool IsDimensionLetter(char value)
        {
            return value == 'd' || value == 'D';
        }

        private static string FormatWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string[] words = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
                words[i] = FormatWord(words[i]);

            return string.Join(" ", words);
        }

        private static string FormatWord(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (IsAllUpper(value))
                return SplitKnownAcronymRun(value);

            if (value.Length == 2 && char.IsDigit(value[0]) && char.IsLetter(value[1]))
                return value[0] + char.ToUpperInvariant(value[1]).ToString();

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string SplitKnownAcronymRun(string value)
        {
            if (value == "UIVFX")
                return "UI VFX";

            return value;
        }

        private static bool IsAllUpper(string value)
        {
            bool hasLetter = false;
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsLetter(value[i]))
                    continue;

                hasLetter = true;
                if (!char.IsUpper(value[i]))
                    return false;
            }

            return hasLetter;
        }
    }
}
