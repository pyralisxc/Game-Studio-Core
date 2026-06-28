using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Pys.Authoring.Editor.Scanning
{
    internal static class SourceDependencyScanner
    {
        private static readonly Regex UsingRegex = new Regex(
            @"^\s*using\s+([^;]+);",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex AsmdefNameRegex = new Regex(
            @"""name""\s*:\s*""([^""]+)""",
            RegexOptions.Compiled);

        private static readonly Regex AsmdefReferencesRegex = new Regex(
            @"""references""\s*:\s*\[(.*?)\]",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex QuotedStringRegex = new Regex(
            @"""([^""]+)""",
            RegexOptions.Compiled);

        public static IReadOnlyList<AssemblyDefinitionObservation> ScanAssemblyDefinitions(UnityCodebaseScanRequest request)
        {
            List<AssemblyDefinitionObservation> observations = new List<AssemblyDefinitionObservation>();
            string absoluteRoot = Path.GetFullPath(UnityScanPathUtility.NormalizeRoot(request));
            if (!Directory.Exists(absoluteRoot))
                return observations;

            foreach (string file in Directory.GetFiles(absoluteRoot, "*.asmdef", SearchOption.AllDirectories))
            {
                string text = File.ReadAllText(file);
                AssemblyDefinitionObservation observation = new AssemblyDefinitionObservation(
                    UnityScanPathUtility.ToAssetPath(file),
                    MatchValue(AsmdefNameRegex, text));

                Match referenceBlock = AsmdefReferencesRegex.Match(text);
                if (referenceBlock.Success)
                {
                    foreach (Match referenceMatch in QuotedStringRegex.Matches(referenceBlock.Groups[1].Value))
                        observation.References.Add(referenceMatch.Groups[1].Value);
                }

                observations.Add(observation);
            }

            return observations;
        }

        public static IReadOnlyList<SourceDependencyObservation> ScanSourceDependencies(UnityCodebaseScanRequest request)
        {
            List<SourceDependencyObservation> observations = new List<SourceDependencyObservation>();
            string absoluteRoot = Path.GetFullPath(UnityScanPathUtility.NormalizeRoot(request));
            if (!Directory.Exists(absoluteRoot))
                return observations;

            foreach (string file in Directory.GetFiles(absoluteRoot, "*.cs", SearchOption.AllDirectories))
            {
                SourceDependencyObservation observation = new SourceDependencyObservation(UnityScanPathUtility.ToAssetPath(file));
                string text = File.ReadAllText(file);
                foreach (Match match in UsingRegex.Matches(text))
                {
                    string namespaceName = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(namespaceName))
                        observation.Namespaces.Add(namespaceName);
                }

                observations.Add(observation);
            }

            return observations;
        }

        private static string MatchValue(Regex regex, string text)
        {
            Match match = regex.Match(text ?? string.Empty);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
