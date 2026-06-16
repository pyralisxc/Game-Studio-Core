using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisSourceDependencyRisk
    {
        Low,
        Watch,
        Heavy,
        BoundaryRisk
    }

    public sealed class PyralisSourceDependencyHygieneRecord
    {
        public string AssetPath { get; }
        public string FileName { get; }
        public string OwnerDomain { get; }
        public IReadOnlyList<string> Domains { get; }
        public int DependencyCount { get; }
        public int ConcreteCrossDomainCount { get; }
        public int SerializedFieldCount { get; }
        public int UnityLookupCount { get; }
        public int StaticAccessCount { get; }
        public int ReflectionOrStringLookupCount { get; }
        public int RiskScore { get; }
        public PyralisSourceDependencyRisk Risk { get; }
        public IReadOnlyList<string> Reasons { get; }

        public PyralisSourceDependencyHygieneRecord(
            string assetPath,
            string fileName,
            string ownerDomain,
            IReadOnlyList<string> domains,
            int dependencyCount,
            int concreteCrossDomainCount,
            int serializedFieldCount,
            int unityLookupCount,
            int staticAccessCount,
            int reflectionOrStringLookupCount,
            int riskScore,
            PyralisSourceDependencyRisk risk,
            IReadOnlyList<string> reasons)
        {
            AssetPath = assetPath;
            FileName = fileName;
            OwnerDomain = ownerDomain;
            Domains = domains ?? Array.Empty<string>();
            DependencyCount = dependencyCount;
            ConcreteCrossDomainCount = concreteCrossDomainCount;
            SerializedFieldCount = serializedFieldCount;
            UnityLookupCount = unityLookupCount;
            StaticAccessCount = staticAccessCount;
            ReflectionOrStringLookupCount = reflectionOrStringLookupCount;
            RiskScore = riskScore;
            Risk = risk;
            Reasons = reasons ?? Array.Empty<string>();
        }
    }

    public static class PyralisSourceDependencyHygieneScanner
    {
        private static readonly string[] KnownDomains =
        {
            "Core",
            "Data",
            "Editor",
            "Presentation",
            "Networking",
            "Characters",
            "Combat",
            "Composition",
            "Encounters",
            "Enemies",
            "Environment",
            "Feedback",
            "GameFlow",
            "Hazards",
            "Input",
            "Interaction",
            "Pickups",
            "Platform",
            "Rpg",
            "Scoring",
            "Settings",
            "Spawning",
            "Tabletop",
            "Traversal",
            "UI",
            "Zones"
        };

        private static readonly Regex UsingRegex = new Regex(@"^\s*using\s+([A-Za-z0-9_.]+)\s*;", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex SerializedFieldRegex = new Regex(@"\[(SerializeField|SerializeReference)\]", RegexOptions.Compiled);
        private static readonly Regex UnityLookupRegex = new Regex(@"\b(GetComponent|GetComponents|FindObjectOfType|FindObjectsOfType|FindFirstObjectByType|FindAnyObjectByType|FindObjectsByType|GameObject\.Find|Resources\.Load)\b", RegexOptions.Compiled);
        private static readonly Regex StaticAccessRegex = new Regex(@"\b(Instance|Current|Default|Shared|Main)\s*\.", RegexOptions.Compiled);
        private static readonly Regex ReflectionOrStringLookupRegex = new Regex(@"\b(Type\.GetType|GetType\(\)|GetMethod|GetField|GetProperty)\b", RegexOptions.Compiled);

        public static IReadOnlyList<PyralisSourceDependencyHygieneRecord> ScanPackage()
        {
            string packageRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay"));

            if (!Directory.Exists(packageRoot))
                return Array.Empty<PyralisSourceDependencyHygieneRecord>();

            return ScanDirectory(packageRoot);
        }

        public static IReadOnlyList<PyralisSourceDependencyHygieneRecord> ScanDirectory(string root)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                return Array.Empty<PyralisSourceDependencyHygieneRecord>();

            string normalizedRoot = Path.GetFullPath(root);
            List<PyralisSourceDependencyHygieneRecord> records = new List<PyralisSourceDependencyHygieneRecord>();
            foreach (string file in Directory.GetFiles(normalizedRoot, "*.cs", SearchOption.AllDirectories))
            {
                if (ShouldSkip(file))
                    continue;

                string source = File.ReadAllText(file);
                string assetPath = ToProjectPath(file);
                records.Add(AnalyzeSource(assetPath, source));
            }

            return records
                .OrderByDescending(record => record.RiskScore)
                .ThenBy(record => record.FileName, StringComparer.Ordinal)
                .ToArray();
        }

        public static PyralisSourceDependencyHygieneRecord AnalyzeSource(string assetPath, string source)
        {
            string safePath = assetPath ?? string.Empty;
            string safeSource = source ?? string.Empty;
            string ownerDomain = ResolveOwnerDomain(safePath, safeSource);
            HashSet<string> domains = new HashSet<string>(StringComparer.Ordinal);
            if (!string.IsNullOrWhiteSpace(ownerDomain))
                domains.Add(ownerDomain);

            MatchCollection usingMatches = UsingRegex.Matches(safeSource);
            int neonBlackUsingCount = 0;
            int concreteCrossDomainCount = 0;
            foreach (Match match in usingMatches)
            {
                string usingNamespace = match.Groups[1].Value;
                if (!usingNamespace.StartsWith("NeonBlack.Gameplay", StringComparison.Ordinal))
                    continue;

                neonBlackUsingCount++;
                string domain = ResolveDomainFromNamespace(usingNamespace);
                if (string.IsNullOrWhiteSpace(domain))
                    continue;

                domains.Add(domain);
                if (IsConcreteCrossDomain(ownerDomain, domain, usingNamespace))
                    concreteCrossDomainCount++;
            }

            int serializedFieldCount = SerializedFieldRegex.Matches(safeSource).Count;
            int unityLookupCount = UnityLookupRegex.Matches(safeSource).Count;
            int staticAccessCount = StaticAccessRegex.Matches(safeSource).Count;
            int reflectionOrStringLookupCount = ReflectionOrStringLookupRegex.Matches(safeSource).Count;
            int dependencyCount = neonBlackUsingCount + serializedFieldCount + unityLookupCount + reflectionOrStringLookupCount;
            int riskScore = CalculateRiskScore(domains.Count, concreteCrossDomainCount, serializedFieldCount, unityLookupCount, staticAccessCount, reflectionOrStringLookupCount);
            List<string> reasons = BuildReasons(domains.Count, concreteCrossDomainCount, serializedFieldCount, unityLookupCount, staticAccessCount, reflectionOrStringLookupCount);

            return new PyralisSourceDependencyHygieneRecord(
                safePath,
                string.IsNullOrWhiteSpace(safePath) ? "Unknown.cs" : Path.GetFileName(safePath),
                ownerDomain,
                domains.OrderBy(domain => domain, StringComparer.Ordinal).ToArray(),
                dependencyCount,
                concreteCrossDomainCount,
                serializedFieldCount,
                unityLookupCount,
                staticAccessCount,
                reflectionOrStringLookupCount,
                riskScore,
                ResolveRisk(riskScore),
                reasons);
        }

        private static bool ShouldSkip(string file)
        {
            string normalized = file.Replace('\\', '/');
            return normalized.Contains("/Docs/", StringComparison.Ordinal)
                || normalized.Contains("/Tests/", StringComparison.Ordinal)
                || normalized.Contains("/_Archive/", StringComparison.Ordinal)
                || normalized.EndsWith(".g.cs", StringComparison.Ordinal)
                || normalized.EndsWith(".Designer.cs", StringComparison.Ordinal);
        }

        private static string ToProjectPath(string file)
        {
            string normalized = file.Replace('\\', '/');
            int packagesIndex = normalized.IndexOf("/Packages/", StringComparison.Ordinal);
            if (packagesIndex >= 0)
                return normalized.Substring(packagesIndex + 1);

            int assetsIndex = normalized.IndexOf("/Assets/", StringComparison.Ordinal);
            if (assetsIndex >= 0)
                return normalized.Substring(assetsIndex + 1);

            return normalized;
        }

        private static string ResolveOwnerDomain(string assetPath, string source)
        {
            string pathDomain = ResolveDomainFromPath(assetPath);
            if (!string.IsNullOrWhiteSpace(pathDomain))
                return pathDomain;

            Match namespaceMatch = Regex.Match(source ?? string.Empty, @"namespace\s+([A-Za-z0-9_.]+)");
            if (namespaceMatch.Success)
                return ResolveDomainFromNamespace(namespaceMatch.Groups[1].Value);

            return "Unknown";
        }

        private static string ResolveDomainFromPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return string.Empty;

            string normalized = assetPath.Replace('\\', '/');
            for (int i = 0; i < KnownDomains.Length; i++)
            {
                string domain = KnownDomains[i];
                if (normalized.Contains("/" + domain + "/", StringComparison.Ordinal))
                    return domain;
            }

            return string.Empty;
        }

        private static string ResolveDomainFromNamespace(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                return string.Empty;

            string[] parts = namespaceName.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                for (int j = 0; j < KnownDomains.Length; j++)
                {
                    if (string.Equals(parts[i], KnownDomains[j], StringComparison.Ordinal))
                        return KnownDomains[j];
                }
            }

            return string.Empty;
        }

        private static bool IsConcreteCrossDomain(string ownerDomain, string dependencyDomain, string usingNamespace)
        {
            if (string.IsNullOrWhiteSpace(dependencyDomain)
                || string.Equals(ownerDomain, dependencyDomain, StringComparison.Ordinal))
                return false;

            if (string.Equals(dependencyDomain, "Core", StringComparison.Ordinal)
                || string.Equals(dependencyDomain, "Data", StringComparison.Ordinal)
                || usingNamespace.Contains(".Core.ContractInterfaces", StringComparison.Ordinal))
                return false;

            return true;
        }

        private static int CalculateRiskScore(
            int domainCount,
            int concreteCrossDomainCount,
            int serializedFieldCount,
            int unityLookupCount,
            int staticAccessCount,
            int reflectionOrStringLookupCount)
        {
            int score = Math.Max(0, domainCount - 2);
            score += concreteCrossDomainCount * 2;
            score += unityLookupCount * 3;
            score += staticAccessCount * 2;
            score += reflectionOrStringLookupCount * 2;
            score += serializedFieldCount / 4;
            return score;
        }

        private static List<string> BuildReasons(
            int domainCount,
            int concreteCrossDomainCount,
            int serializedFieldCount,
            int unityLookupCount,
            int staticAccessCount,
            int reflectionOrStringLookupCount)
        {
            List<string> reasons = new List<string>();
            if (domainCount > 2)
                reasons.Add("Touches " + domainCount + " Pyralis domains.");
            if (concreteCrossDomainCount > 0)
                reasons.Add(concreteCrossDomainCount + " concrete cross-domain reference(s).");
            if (serializedFieldCount >= 6)
                reasons.Add(serializedFieldCount + " serialized field/reference marker(s).");
            if (unityLookupCount > 0)
                reasons.Add(unityLookupCount + " Unity lookup/discovery call(s).");
            if (staticAccessCount > 0)
                reasons.Add(staticAccessCount + " static/global access marker(s).");
            if (reflectionOrStringLookupCount > 0)
                reasons.Add(reflectionOrStringLookupCount + " reflection/string lookup marker(s).");
            if (reasons.Count == 0)
                reasons.Add("No obvious dependency pressure.");

            return reasons;
        }

        private static PyralisSourceDependencyRisk ResolveRisk(int riskScore)
        {
            if (riskScore >= 12)
                return PyralisSourceDependencyRisk.BoundaryRisk;
            if (riskScore >= 8)
                return PyralisSourceDependencyRisk.Heavy;
            if (riskScore >= 4)
                return PyralisSourceDependencyRisk.Watch;
            return PyralisSourceDependencyRisk.Low;
        }
    }
}
