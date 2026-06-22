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

    public enum PyralisSourceDependencyPressureKind
    {
        RuntimeOwnership,
        ReferenceAssembly,
        AcceptedComposition,
        PawnCoordinator,
        PawnCapabilitySibling,
        LocalPresentationSurface,
        SceneZoneSurface,
        InputRoutingSurface,
        EnemyCapabilityModule,
        EnemyCoordinator,
        CombatContactSurface,
        PawnRuntimeHelper,
        NetworkAdapterSurface,
        SceneNavigationSurface,
        RpgDomainCore,
        RpgSceneSurface,
        ScoringRuntimeSurface,
        SpawningRuntimeSurface,
        GameFlowRuntimeSurface,
        ContractReflectionSurface,
        PersistenceDataSurface,
        ActorFeatureContext,
        SceneCameraRig,
        AuthoredDataAsset,
        HazardRuntimeSurface,
        DomainUtility,
        FeatureModule,
        AuthoredRuntimeSurface,
        EditorAudit,
        GrammarVocabulary,
        DirectSceneQuerySurface,
        ReflectionMeaningLeak,
        ValidatorGuideLeak,
        InspectorRouteGuideLeak,
        ExportTruthLeak,
        TabRendererLogicLeak,
        LegacyDocTruthLeak,
        CompatibilityBridge,
        OldOwnerName,
        ScannerImplementation
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
        public int LocalComponentLookupCount { get; }
        public int BroadUnityDiscoveryCount { get; }
        public int StaticAccessCount { get; }
        public int ReflectionOrStringLookupCount { get; }
        public int RiskScore { get; }
        public PyralisSourceDependencyRisk Risk { get; }
        public PyralisSourceDependencyPressureKind PressureKind { get; }
        public string ReviewHint { get; }
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
            int localComponentLookupCount,
            int broadUnityDiscoveryCount,
            int staticAccessCount,
            int reflectionOrStringLookupCount,
            int riskScore,
            PyralisSourceDependencyRisk risk,
            PyralisSourceDependencyPressureKind pressureKind,
            string reviewHint,
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
            LocalComponentLookupCount = localComponentLookupCount;
            BroadUnityDiscoveryCount = broadUnityDiscoveryCount;
            StaticAccessCount = staticAccessCount;
            ReflectionOrStringLookupCount = reflectionOrStringLookupCount;
            RiskScore = riskScore;
            Risk = risk;
            PressureKind = pressureKind;
            ReviewHint = reviewHint ?? string.Empty;
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
        private static readonly Regex LocalComponentLookupRegex = new Regex(@"\bGetComponents?(InChildren|InParent)?\b", RegexOptions.Compiled);
        private static readonly Regex BroadUnityDiscoveryRegex = new Regex(@"\b(FindObjectOfType|FindObjectsOfType|FindFirstObjectByType|FindAnyObjectByType|FindObjectsByType|GameObject\.Find|Resources\.Load)\b", RegexOptions.Compiled);
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
            foreach (string file in EnumerateScannableFiles(normalizedRoot))
            {
                if (ShouldSkip(file))
                    continue;

                string source = File.ReadAllText(file);
                string assetPath = ToProjectPath(file);
                records.Add(AnalyzeSource(assetPath, source));
            }

            return records
                .OrderBy(record => GetCleanupPriority(record.PressureKind))
                .ThenByDescending(record => record.RiskScore)
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
            int localComponentLookupCount = LocalComponentLookupRegex.Matches(safeSource).Count;
            int broadUnityDiscoveryCount = BroadUnityDiscoveryRegex.Matches(safeSource).Count;
            int unityLookupCount = localComponentLookupCount + broadUnityDiscoveryCount;
            int staticAccessCount = StaticAccessRegex.Matches(safeSource).Count;
            int reflectionOrStringLookupCount = ReflectionOrStringLookupRegex.Matches(safeSource).Count;
            int dependencyCount = neonBlackUsingCount + serializedFieldCount + unityLookupCount + reflectionOrStringLookupCount;
            PyralisSourceDependencyPressureKind pressureKind = ResolvePressureKind(safePath, safeSource, ownerDomain);
            int riskScore = CalculateRiskScore(domains.Count, concreteCrossDomainCount, serializedFieldCount, localComponentLookupCount, broadUnityDiscoveryCount, staticAccessCount, reflectionOrStringLookupCount);
            riskScore = ApplyPressureKindRiskFloor(riskScore, pressureKind);
            List<string> reasons = BuildReasons(domains.Count, concreteCrossDomainCount, serializedFieldCount, localComponentLookupCount, broadUnityDiscoveryCount, staticAccessCount, reflectionOrStringLookupCount);
            AddPressureKindReason(reasons, pressureKind);

            return new PyralisSourceDependencyHygieneRecord(
                safePath,
                string.IsNullOrWhiteSpace(safePath) ? "Unknown.cs" : Path.GetFileName(safePath),
                ownerDomain,
                domains.OrderBy(domain => domain, StringComparer.Ordinal).ToArray(),
                dependencyCount,
                concreteCrossDomainCount,
                serializedFieldCount,
                unityLookupCount,
                localComponentLookupCount,
                broadUnityDiscoveryCount,
                staticAccessCount,
                reflectionOrStringLookupCount,
                riskScore,
                ResolveRisk(riskScore),
                pressureKind,
                BuildReviewHint(pressureKind),
                reasons);
        }

        public static int GetCleanupPriority(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind switch
            {
                PyralisSourceDependencyPressureKind.RuntimeOwnership => 0,
                PyralisSourceDependencyPressureKind.DirectSceneQuerySurface => 1,
                PyralisSourceDependencyPressureKind.ReflectionMeaningLeak => 2,
                PyralisSourceDependencyPressureKind.ValidatorGuideLeak => 3,
                PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak => 4,
                PyralisSourceDependencyPressureKind.ExportTruthLeak => 5,
                PyralisSourceDependencyPressureKind.TabRendererLogicLeak => 6,
                PyralisSourceDependencyPressureKind.CompatibilityBridge => 7,
                PyralisSourceDependencyPressureKind.LegacyDocTruthLeak => 8,
                PyralisSourceDependencyPressureKind.OldOwnerName => 9,
                PyralisSourceDependencyPressureKind.AcceptedComposition => 10,
                PyralisSourceDependencyPressureKind.PawnCoordinator => 11,
                PyralisSourceDependencyPressureKind.PawnCapabilitySibling => 12,
                PyralisSourceDependencyPressureKind.LocalPresentationSurface => 13,
                PyralisSourceDependencyPressureKind.SceneZoneSurface => 14,
                PyralisSourceDependencyPressureKind.InputRoutingSurface => 15,
                PyralisSourceDependencyPressureKind.EnemyCapabilityModule => 16,
                PyralisSourceDependencyPressureKind.EnemyCoordinator => 17,
                PyralisSourceDependencyPressureKind.CombatContactSurface => 18,
                PyralisSourceDependencyPressureKind.PawnRuntimeHelper => 19,
                PyralisSourceDependencyPressureKind.NetworkAdapterSurface => 20,
                PyralisSourceDependencyPressureKind.SceneNavigationSurface => 21,
                PyralisSourceDependencyPressureKind.RpgDomainCore => 22,
                PyralisSourceDependencyPressureKind.RpgSceneSurface => 23,
                PyralisSourceDependencyPressureKind.ScoringRuntimeSurface => 24,
                PyralisSourceDependencyPressureKind.SpawningRuntimeSurface => 25,
                PyralisSourceDependencyPressureKind.GameFlowRuntimeSurface => 26,
                PyralisSourceDependencyPressureKind.ContractReflectionSurface => 27,
                PyralisSourceDependencyPressureKind.PersistenceDataSurface => 28,
                PyralisSourceDependencyPressureKind.ActorFeatureContext => 29,
                PyralisSourceDependencyPressureKind.SceneCameraRig => 30,
                PyralisSourceDependencyPressureKind.AuthoredDataAsset => 31,
                PyralisSourceDependencyPressureKind.HazardRuntimeSurface => 32,
                PyralisSourceDependencyPressureKind.DomainUtility => 33,
                PyralisSourceDependencyPressureKind.FeatureModule => 34,
                PyralisSourceDependencyPressureKind.AuthoredRuntimeSurface => 35,
                PyralisSourceDependencyPressureKind.ReferenceAssembly => 36,
                PyralisSourceDependencyPressureKind.EditorAudit => 37,
                PyralisSourceDependencyPressureKind.GrammarVocabulary => 38,
                PyralisSourceDependencyPressureKind.ScannerImplementation => 39,
                _ => 6
            };
        }

        private static IEnumerable<string> EnumerateScannableFiles(string normalizedRoot)
        {
            foreach (string file in Directory.GetFiles(normalizedRoot, "*.cs", SearchOption.AllDirectories))
                yield return file;

            foreach (string file in Directory.GetFiles(normalizedRoot, "*.md", SearchOption.AllDirectories))
            {
                string normalized = file.Replace('\\', '/');
                if (normalized.Contains("/Docs/", StringComparison.Ordinal)
                    || normalized.EndsWith("/README.md", StringComparison.Ordinal))
                {
                    yield return file;
                }
            }
        }

        private static bool ShouldSkip(string file)
        {
            string normalized = file.Replace('\\', '/');
            return normalized.Contains("/Tests/", StringComparison.Ordinal)
                || normalized.Contains("/_Archive/", StringComparison.Ordinal)
                || normalized.Contains("/Archive/", StringComparison.Ordinal)
                || normalized.Contains("/TempGraphs/", StringComparison.Ordinal)
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
                || usingNamespace.Contains(".Core.Contracts", StringComparison.Ordinal))
                return false;

            return true;
        }

        private static int CalculateRiskScore(
            int domainCount,
            int concreteCrossDomainCount,
            int serializedFieldCount,
            int localComponentLookupCount,
            int broadUnityDiscoveryCount,
            int staticAccessCount,
            int reflectionOrStringLookupCount)
        {
            int score = Math.Max(0, domainCount - 2);
            score += concreteCrossDomainCount * 2;
            score += localComponentLookupCount / 3;
            score += broadUnityDiscoveryCount * 4;
            score += staticAccessCount * 2;
            score += reflectionOrStringLookupCount * 2;
            score += serializedFieldCount / 4;
            return score;
        }

        private static List<string> BuildReasons(
            int domainCount,
            int concreteCrossDomainCount,
            int serializedFieldCount,
            int localComponentLookupCount,
            int broadUnityDiscoveryCount,
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
            if (localComponentLookupCount >= 6)
                reasons.Add(localComponentLookupCount + " local component lookup/cache call(s).");
            if (broadUnityDiscoveryCount > 0)
                reasons.Add(broadUnityDiscoveryCount + " broad Unity scene/resource discovery call(s).");
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

        private static int ApplyPressureKindRiskFloor(int riskScore, PyralisSourceDependencyPressureKind pressureKind)
        {
            if (IsOwnershipLeakPressure(pressureKind))
                return Math.Max(riskScore, 8);

            if (pressureKind == PyralisSourceDependencyPressureKind.LegacyDocTruthLeak
                || pressureKind == PyralisSourceDependencyPressureKind.OldOwnerName)
            {
                return Math.Max(riskScore, 4);
            }

            return riskScore;
        }

        private static void AddPressureKindReason(List<string> reasons, PyralisSourceDependencyPressureKind pressureKind)
        {
            if (reasons == null)
                return;

            string reason = pressureKind switch
            {
                PyralisSourceDependencyPressureKind.ReflectionMeaningLeak => "Reflection surface appears to contain vocabulary, proof, or route meaning.",
                PyralisSourceDependencyPressureKind.ValidatorGuideLeak => "Validator surface appears to contain route-guide or first-proof wording.",
                PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak => "Inspector surface appears to contain route-guide or first-proof wording.",
                PyralisSourceDependencyPressureKind.ExportTruthLeak => "Export surface appears to compute authoring truth instead of serializing a projection.",
                PyralisSourceDependencyPressureKind.TabRendererLogicLeak => "Tab renderer appears to classify readiness or route logic while rendering.",
                PyralisSourceDependencyPressureKind.LegacyDocTruthLeak => "Active documentation appears to preserve legacy or deprecated setup truth.",
                PyralisSourceDependencyPressureKind.CompatibilityBridge => "Source appears to contain compatibility or fallback repair behavior.",
                PyralisSourceDependencyPressureKind.OldOwnerName => "Active file or source wording appears to reference an old ownership model.",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(reason) && !reasons.Contains(reason))
                reasons.Add(reason);
        }

        private static PyralisSourceDependencyPressureKind ResolvePressureKind(string assetPath, string source, string ownerDomain)
        {
            string normalized = (assetPath ?? string.Empty).Replace('\\', '/');
            string fileName = Path.GetFileName(normalized);
            string safeSource = source ?? string.Empty;
            if (IsScannerImplementationPath(normalized))
                return PyralisSourceDependencyPressureKind.ScannerImplementation;

            if (IsLegacyDocTruthLeak(normalized, safeSource))
                return PyralisSourceDependencyPressureKind.LegacyDocTruthLeak;

            if (IsCompatibilityBridge(normalized, fileName, safeSource))
                return PyralisSourceDependencyPressureKind.CompatibilityBridge;

            if (IsOldOwnerName(normalized, safeSource))
                return PyralisSourceDependencyPressureKind.OldOwnerName;

            if (IsReflectionMeaningLeak(normalized, safeSource))
                return PyralisSourceDependencyPressureKind.ReflectionMeaningLeak;

            if (IsValidatorGuideLeak(normalized, fileName, safeSource))
                return PyralisSourceDependencyPressureKind.ValidatorGuideLeak;

            if (IsInspectorRouteGuideLeak(normalized, fileName, safeSource))
                return PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak;

            if (IsExportTruthLeak(normalized, fileName, safeSource))
                return PyralisSourceDependencyPressureKind.ExportTruthLeak;

            if (IsTabRendererLogicLeak(normalized, fileName, safeSource))
                return PyralisSourceDependencyPressureKind.TabRendererLogicLeak;

            if (normalized.EndsWith(".md", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.EditorAudit;

            if (normalized.Contains("/Editor/Authoring/Grammar/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.GrammarVocabulary;

            if (normalized.Contains("/Core/AuthoringContracts/", StringComparison.Ordinal)
                || string.Equals(fileName, "ResolvedAuthoringContractRegistry.cs", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.ContractReflectionSurface;
            }

            if (normalized.Contains("/Editor/Authoring/Spine/Validation/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Authoring/Spine/Evidence/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Authoring/Spine/Graph/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Authoring/Surfaces/", StringComparison.Ordinal)
                || normalized.Contains("/Features/Rpg/Editor/", StringComparison.Ordinal)
                || string.Equals(ownerDomain, "Editor", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.EditorAudit;
            }

            if (normalized.Contains("/Features/Platform/Composition/", StringComparison.Ordinal)
                || normalized.Contains("/Features/Rpg/Runtime/Composition/", StringComparison.Ordinal)
                || normalized.Contains("/Features/Platform/Session/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.AcceptedComposition;
            }

            if (fileName.Contains("SaveData", StringComparison.Ordinal)
                || fileName.Contains("Snapshot", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.PersistenceDataSurface;
            }

            if (string.Equals(fileName, "SceneGuard.cs", StringComparison.Ordinal)
                || normalized.Contains("/Core/Navigation/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.SceneNavigationSurface;
            }

            if (fileName.StartsWith("PawnRoot", StringComparison.Ordinal)
                || string.Equals(fileName, "Motor2D.cs", StringComparison.Ordinal)
                || string.Equals(fileName, "Motor3D.cs", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.PawnCoordinator;
            }

            if (normalized.EndsWith(".Config.cs", StringComparison.Ordinal)
                || normalized.EndsWith(".Profiles.cs", StringComparison.Ordinal)
                || normalized.EndsWith(".Profile.cs", StringComparison.Ordinal)
                || normalized.EndsWith(".Validation.cs", StringComparison.Ordinal)
                || normalized.EndsWith(".Gizmos.cs", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.AuthoredRuntimeSurface;
            }

            if (normalized.Contains("/Data/Definitions/", StringComparison.Ordinal)
                || normalized.Contains("/Data/Profiles/", StringComparison.Ordinal)
                || normalized.Contains("/Features/", StringComparison.Ordinal) && normalized.Contains("/Data/", StringComparison.Ordinal)
                || fileName.EndsWith("Definition.cs", StringComparison.Ordinal)
                || fileName.EndsWith("Profile.cs", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.AuthoredDataAsset;
            }

            if (string.Equals(fileName, "ActorFeatureContext.cs", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.ActorFeatureContext;

            if (string.Equals(fileName, "CinemachineCameraRigController.cs", StringComparison.Ordinal)
                || normalized.Contains("/Presentation/Camera/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.SceneCameraRig;
            }

            if (normalized.Contains("/Networking/", StringComparison.Ordinal)
                || fileName.StartsWith("Network", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.NetworkAdapterSurface;
            }

            if (fileName.StartsWith("Pawn", StringComparison.Ordinal)
                && (fileName.Contains("MovementComponent", StringComparison.Ordinal)
                    || fileName.Contains("TraversalComponent", StringComparison.Ordinal)
                    || fileName.Contains("CombatBehaviour", StringComparison.Ordinal)
                    || fileName.Contains("PresentationComponent", StringComparison.Ordinal)
                    || fileName.Contains("InputModule", StringComparison.Ordinal)
                    || fileName.Contains("WeaponModule", StringComparison.Ordinal)
                    || fileName.Contains("ProjectileModule", StringComparison.Ordinal)))
            {
                return PyralisSourceDependencyPressureKind.PawnCapabilitySibling;
            }

            if (string.Equals(fileName, "PawnComboProcessor.cs", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.PawnRuntimeHelper;

            if (normalized.Contains("/Features/Feedback/", StringComparison.Ordinal)
                || normalized.Contains("/Features/Combat/UI/", StringComparison.Ordinal)
                || normalized.Contains("/Presentation/Animation/", StringComparison.Ordinal)
                || normalized.Contains("/Presentation/Visuals/", StringComparison.Ordinal)
                || fileName.Contains("Feedback", StringComparison.Ordinal)
                || fileName.Contains("HealthBar", StringComparison.Ordinal)
                || fileName.Contains("AnimationDriver", StringComparison.Ordinal)
                || fileName.Contains("ShadowDriver", StringComparison.Ordinal)
                || fileName.Contains("DamageNumber", StringComparison.Ordinal)
                || fileName.Contains("Presenter", StringComparison.Ordinal)
                || fileName.Contains("Presentation", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.LocalPresentationSurface;
            }

            if (normalized.Contains("/Features/Zones/", StringComparison.Ordinal)
                || fileName.Contains("Zone", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.SceneZoneSurface;
            }

            if (string.Equals(fileName, "ParticipantInputRouter.cs", StringComparison.Ordinal)
                || fileName.Contains("InputBridge", StringComparison.Ordinal)
                || normalized.Contains("/Features/Input/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.InputRoutingSurface;
            }

            if (normalized.Contains("/Features/Enemies/", StringComparison.Ordinal)
                && fileName.Contains("Module", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.EnemyCapabilityModule;
            }

            if (normalized.Contains("/Features/Enemies/", StringComparison.Ordinal)
                && (fileName.Contains("AI", StringComparison.Ordinal)
                    || fileName.Contains("Processor", StringComparison.Ordinal)))
            {
                return PyralisSourceDependencyPressureKind.EnemyCoordinator;
            }

            if (normalized.Contains("/Core/Rpg/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.RpgDomainCore;
            }

            if (normalized.Contains("/Features/Rpg/UI/", StringComparison.Ordinal)
                || normalized.Contains("/Features/Rpg/", StringComparison.Ordinal)
                    && (fileName.Contains("SceneController", StringComparison.Ordinal)
                        || fileName.Contains("Panel", StringComparison.Ordinal)
                        || fileName.Contains("Router", StringComparison.Ordinal)
                        || fileName.Contains("Presenter", StringComparison.Ordinal)))
            {
                return PyralisSourceDependencyPressureKind.RpgSceneSurface;
            }

            if (normalized.Contains("/Features/Scoring/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.ScoringRuntimeSurface;

            if (normalized.Contains("/Features/Spawning/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.SpawningRuntimeSurface;

            if (normalized.Contains("/Features/Combat/", StringComparison.Ordinal)
                && (fileName.Contains("HitBox", StringComparison.Ordinal)
                    || fileName.Contains("HurtBox", StringComparison.Ordinal)))
            {
                return PyralisSourceDependencyPressureKind.CombatContactSurface;
            }

            if (fileName.EndsWith("Utility.cs", StringComparison.Ordinal)
                || fileName.EndsWith("Utilities.cs", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.DomainUtility;
            }

            if (normalized.Contains("/Features/Hazards/", StringComparison.Ordinal)
                || fileName.Contains("Hazard", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.HazardRuntimeSurface;
            }

            if (fileName.Contains("FeatureRuntime", StringComparison.Ordinal)
                || safeSource.Contains("IFeatureModuleRuntime", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.FeatureModule;
            }

            if (normalized.EndsWith("RuntimeReferences.cs", StringComparison.Ordinal)
                || normalized.EndsWith("RuntimeReferenceCache.cs", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.ReferenceAssembly;
            }

            if (normalized.Contains("/Features/GameFlow/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.GameFlowRuntimeSurface;

            if (normalized.Contains("/PlayerRegistry", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.DirectSceneQuerySurface;

            return PyralisSourceDependencyPressureKind.RuntimeOwnership;
        }

        public static bool IsOwnershipLeakPressure(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind == PyralisSourceDependencyPressureKind.ReflectionMeaningLeak
                || pressureKind == PyralisSourceDependencyPressureKind.ValidatorGuideLeak
                || pressureKind == PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak
                || pressureKind == PyralisSourceDependencyPressureKind.ExportTruthLeak
                || pressureKind == PyralisSourceDependencyPressureKind.TabRendererLogicLeak
                || pressureKind == PyralisSourceDependencyPressureKind.CompatibilityBridge;
        }

        private static bool IsLegacyDocTruthLeak(string normalizedPath, string source)
        {
            if (!normalizedPath.EndsWith(".md", StringComparison.Ordinal)
                || !normalizedPath.Contains("/Docs/", StringComparison.Ordinal))
            {
                return false;
            }

            bool hasLegacySetupLanguage = ContainsAny(source,
                "we used to",
                "old setup",
                "legacy setup",
                "deprecated path",
                "deprecated setup",
                "no longer do",
                "migration residue");
            if (hasLegacySetupLanguage)
                return true;

            return ContainsAny(source, "compatibility bridge")
                && !IsProtectivePolicyText(source);
        }

        private static bool IsOldOwnerName(string normalizedPath, string source)
        {
            if (IsScannerImplementationPath(normalizedPath))
                return false;

            if (normalizedPath.EndsWith(".md", StringComparison.Ordinal)
                && IsProtectivePolicyText(source))
            {
                return false;
            }

            return ContainsAny(normalizedPath,
                    "Legacy",
                    "Deprecated",
                    "OldOwner")
                || ContainsAny(source,
                    "LegacyAuthoring",
                    "OldAuthoring",
                    "OldOwner",
                    "DeprecatedOwner");
        }

        private static bool IsReflectionMeaningLeak(string normalizedPath, string source)
        {
            return normalizedPath.Contains("/Editor/Authoring/Spine/Reflection/", StringComparison.Ordinal)
                && ContainsAny(source,
                    "proof.",
                    "FirstProof",
                    "Route Proof",
                    "CapabilityPath",
                    "RuntimeCapabilityFamily",
                    "NativeSetup",
                    "Guide",
                    "Overview");
        }

        private static bool IsValidatorGuideLeak(string normalizedPath, string fileName, string source)
        {
            bool validatorSurface = normalizedPath.Contains("/Validation/", StringComparison.Ordinal)
                || fileName.Contains("Validation", StringComparison.Ordinal)
                || fileName.Contains("Validator", StringComparison.Ordinal);
            return validatorSurface && ContainsRouteGuideWording(source);
        }

        private static bool IsInspectorRouteGuideLeak(string normalizedPath, string fileName, string source)
        {
            bool inspectorSurface = normalizedPath.Contains("/Surfaces/Inspectors/", StringComparison.Ordinal)
                || normalizedPath.Contains("/Editor/Inspectors/", StringComparison.Ordinal)
                || fileName.EndsWith("Editor.cs", StringComparison.Ordinal)
                || fileName.Contains("Inspector", StringComparison.Ordinal);
            return inspectorSurface
                && ContainsRouteGuideWording(source)
                && !IsInspectorAuthoringHandoffOnly(source);
        }

        private static bool IsExportTruthLeak(string normalizedPath, string fileName, string source)
        {
            bool exportSurface = fileName.Contains("Exporter", StringComparison.Ordinal)
                || fileName.Contains("JsonExport", StringComparison.Ordinal)
                || normalizedPath.Contains("/TempGraphs/", StringComparison.Ordinal);
            if (!exportSurface)
                return false;

            if (IsExportControlChrome(normalizedPath, fileName, source))
                return false;

            return ContainsAny(source,
                    "BuildRouteWorkingProjection(",
                    "FindCurrentProofNode(",
                    "ResolveProofReadiness(",
                    "BuildMapSceneSetupIssueRows(")
                || (source.Contains(".OrderBy(", StringComparison.Ordinal)
                    && source.Contains("EvidenceState", StringComparison.Ordinal));
        }

        private static bool IsExportControlChrome(string normalizedPath, string fileName, string source)
        {
            if (!normalizedPath.Contains("/Surfaces/AuthoringWindow/", StringComparison.Ordinal)
                || !fileName.Contains("JsonExportControl", StringComparison.Ordinal))
            {
                return false;
            }

            return !ContainsAny(source,
                "BuildRouteWorkingProjection(",
                "FindCurrentProofNode(",
                "ResolveProofReadiness(",
                "BuildMapSceneSetupIssueRows(",
                "PyralisAuthoringGraphEvidenceState.",
                "IssueCode.StartsWith(");
        }

        private static bool IsTabRendererLogicLeak(string normalizedPath, string fileName, string source)
        {
            bool tabRenderer = normalizedPath.Contains("/Surfaces/AuthoringWindow/", StringComparison.Ordinal)
                && (fileName.Contains("Renderer", StringComparison.Ordinal)
                    || fileName.Contains("ExportControl", StringComparison.Ordinal)
                    || fileName.Contains("Window", StringComparison.Ordinal));
            if (!tabRenderer)
                return false;

            return ContainsAny(source,
                    "PyralisAuthoringGraphEvidenceState.",
                    "BuildRouteWorkingProjection(",
                    "FindCurrentProofNode(",
                    "IssueCode.StartsWith(")
                || (source.Contains(".Where(", StringComparison.Ordinal)
                    && source.Contains("EvidenceState", StringComparison.Ordinal));
        }

        private static bool IsCompatibilityBridge(string normalizedPath, string fileName, string source)
        {
            if (fileName.Contains("CodexUnityValidationRefreshBridge", StringComparison.Ordinal))
                return false;
            if (normalizedPath.EndsWith(".md", StringComparison.Ordinal))
                return false;
            if (IsScannerImplementationPath(normalizedPath))
                return false;
            if (normalizedPath.Contains("/Spine/Reflection/", StringComparison.Ordinal)
                && source.Contains("ReflectionTypeLoadException", StringComparison.Ordinal))
            {
                return false;
            }

            bool compatibilityLanguage = ContainsAny(normalizedPath, "Compatibility", "Legacy", "Bridge")
                || ContainsAny(source, "compatibility", "legacy", "obsolete", "[Obsolete");
            bool repairLanguage = ContainsAny(source, "fallback", "repair", "auto-wire", "auto wire", "auto-create", "auto create", "quietly");
            return compatibilityLanguage && repairLanguage;
        }

        private static bool IsScannerImplementationPath(string normalizedPath)
        {
            return !string.IsNullOrWhiteSpace(normalizedPath)
                && normalizedPath.EndsWith("/Editor/Authoring/Spine/Hygiene/PyralisSourceDependencyHygieneScanner.cs", StringComparison.Ordinal);
        }

        private static bool ContainsRouteGuideWording(string source)
        {
            return ContainsAny(source,
                "Route Proof",
                "first proof",
                "FirstProof",
                "Do Now",
                "Overview",
                "Guide",
                "Map owns",
                "Route Trace",
                "proof.");
        }

        private static bool IsInspectorAuthoringHandoffOnly(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            bool handoff = ContainsAny(source,
                "Use Pyralis Authoring",
                "Open Pyralis Authoring",
                "PyralisInspectorHandoff.DrawAuthoringButton",
                "Authoring owns route",
                "Authoring owns first-proof",
                "Authoring owns first proof");
            if (!handoff)
                return false;

            bool ownsGuidance = ContainsAny(source,
                "PyralisGuideContent",
                "PyralisGuideSection",
                "PyralisInspectorValidationIssue",
                "CreateAssignmentFact",
                "relatedStableIds",
                "BuildChecklist",
                "FeatureModuleSetup",
                "Do Now:",
                "Route Proof",
                "proof.");
            return !ownsGuidance;
        }

        private static bool IsProtectivePolicyText(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            return ContainsAny(source,
                "Fallback policy is strict",
                "Do not recover by parsing",
                "should not quietly repair",
                "must not quietly repair",
                "must not auto-wire",
                "must not auto wire",
                "must not auto-create",
                "must not auto create",
                "compatibility bridges are cleanup smells",
                "source-ownership residue",
                "Hygiene pressure kinds are not all cleanup commands");
        }

        private static bool ContainsAny(string value, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(value) || needles == null)
                return false;

            for (int i = 0; i < needles.Length; i++)
            {
                string needle = needles[i];
                if (!string.IsNullOrWhiteSpace(needle)
                    && value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildReviewHint(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind switch
            {
                PyralisSourceDependencyPressureKind.ReflectionMeaningLeak => "Reflection should expose structure only. Move vocabulary, route labels, proof meaning, and setup prose to contracts, grammar, validators, or projection.",
                PyralisSourceDependencyPressureKind.ValidatorGuideLeak => "Validators should witness local semantic readiness. Move route setup cards, first-proof wording, and guide sequencing to graph projection or grammar.",
                PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak => "Inspectors should stay field-local. Move route guide, first-proof, and setup-path wording to Authoring projections or grammar.",
                PyralisSourceDependencyPressureKind.ExportTruthLeak => "Exports should serialize tab projections. Move ranking, readiness classification, and route truth back to projection/model builders.",
                PyralisSourceDependencyPressureKind.TabRendererLogicLeak => "Tab renderers should render projection models. Move readiness and route decisions back to graph projection.",
                PyralisSourceDependencyPressureKind.LegacyDocTruthLeak => "Active docs should state current ownership directly. Delete or archive stale legacy/deprecated setup truth.",
                PyralisSourceDependencyPressureKind.CompatibilityBridge => "Compatibility bridges should not quietly repair authoring setup. Prefer explicit contract/reflection/validator evidence and Map guidance.",
                PyralisSourceDependencyPressureKind.OldOwnerName => "Old owner names make future work follow stale seams. Rename or document why the old concept is still active.",
                PyralisSourceDependencyPressureKind.ReferenceAssembly => "Expected pressure for a focused reference/context assembly helper; review only if gameplay decisions move into it.",
                PyralisSourceDependencyPressureKind.AcceptedComposition => "Expected pressure for bootstrap/composition code; review only if it starts owning feature behavior instead of wiring services.",
                PyralisSourceDependencyPressureKind.PawnCoordinator => "Expected pressure for an explicit pawn coordinator. Review only if it starts constructing optional features or owning movement/combat/presentation behavior directly.",
                PyralisSourceDependencyPressureKind.PawnCapabilitySibling => "Expected pressure for an explicit pawn capability sibling. Review if it stops being visible prefab identity and starts replacing participant/session ownership.",
                PyralisSourceDependencyPressureKind.LocalPresentationSurface => "Expected pressure for local presentation, feedback, or HUD-adjacent behavior. Review if it starts owning gameplay state instead of rendering or reacting to it.",
                PyralisSourceDependencyPressureKind.SceneZoneSurface => "Expected pressure for a scene-authored trigger/zone surface. Review if it becomes the owner of combat, camera, or participant policy instead of requesting those effects.",
                PyralisSourceDependencyPressureKind.InputRoutingSurface => "Expected pressure for Unity Input System join/routing code. Review if it starts owning input profiles instead of applying ParticipantDefinition.inputProfile.",
                PyralisSourceDependencyPressureKind.EnemyCapabilityModule => "Expected pressure for an explicit NPC capability module. Review if it starts owning session, participant, or scene service policy instead of enemy-local behavior.",
                PyralisSourceDependencyPressureKind.EnemyCoordinator => "Expected pressure for an NPC tactical coordinator or processor. Review if it starts owning session, participant, or scene service policy instead of enemy-local state and actions.",
                PyralisSourceDependencyPressureKind.CombatContactSurface => "Expected pressure for combat contact surfaces such as hitboxes. Review if they start owning combat policy instead of applying authored hit data to detected targets.",
                PyralisSourceDependencyPressureKind.PawnRuntimeHelper => "Expected pressure for a focused pawn runtime helper. Review if it starts reaching into scene, session, or participant ownership.",
                PyralisSourceDependencyPressureKind.NetworkAdapterSurface => "Expected pressure for a networking adapter. Review if transport-specific code leaks back into the transport-agnostic gameplay assembly.",
                PyralisSourceDependencyPressureKind.SceneNavigationSurface => "Expected pressure for scene navigation and shell guard surfaces. Review if they become gameplay flow owners instead of scene-readiness safeguards.",
                PyralisSourceDependencyPressureKind.RpgDomainCore => "Expected pressure for RPG domain models and services. Review if this area mixes scene UI, Unity object discovery, or presentation behavior into domain/runtime services.",
                PyralisSourceDependencyPressureKind.RpgSceneSurface => "Expected pressure for RPG scene/UI controller surfaces. Review if they become platform session owners instead of feature-local scene presenters/controllers.",
                PyralisSourceDependencyPressureKind.ScoringRuntimeSurface => "Expected pressure for a scoring feature surface. Review if it becomes a session owner instead of applying authored scoring rules through scoring/session services.",
                PyralisSourceDependencyPressureKind.SpawningRuntimeSurface => "Expected pressure for scene or participant spawning surfaces. Review if pawn identity moves here instead of staying with ParticipantDefinition, PawnDefinition, and ParticipantSpawnService.",
                PyralisSourceDependencyPressureKind.GameFlowRuntimeSurface => "Expected pressure for a mode-specific game-flow surface. Review if it starts owning participant identity, pawn spawning, or shared session state instead of coordinating authored arcade flow.",
                PyralisSourceDependencyPressureKind.ContractReflectionSurface => "Expected pressure for the reflective authoring contract spine. Review if feature-specific setup truth moves here instead of staying on contracts/reflection.",
                PyralisSourceDependencyPressureKind.PersistenceDataSurface => "Expected pressure for serializable save/snapshot data. Review if runtime service behavior or scene discovery moves into the data surface.",
                PyralisSourceDependencyPressureKind.ActorFeatureContext => "Expected pressure for the read-only context object passed into optional feature modules. Review if it begins resolving services or mutating gameplay state.",
                PyralisSourceDependencyPressureKind.SceneCameraRig => "Expected pressure for the scene-owned camera rig. Review if pawns or zones become camera owners instead of target/profile providers.",
                PyralisSourceDependencyPressureKind.AuthoredDataAsset => "Expected pressure for definitions/profiles that describe authored data. Review if runtime behavior or scene discovery moves into the asset.",
                PyralisSourceDependencyPressureKind.HazardRuntimeSurface => "Expected pressure for authored hazard runtime surfaces. Review if hazards own participant/session policy instead of applying configured hazard effects.",
                PyralisSourceDependencyPressureKind.DomainUtility => "Expected pressure for a stateless domain helper. Review if it starts storing state, discovering scene objects broadly, or becoming a hidden service.",
                PyralisSourceDependencyPressureKind.FeatureModule => "Expected pressure for an optional feature module or feature contract. Review if it becomes required pawn identity instead of an installable capability.",
                PyralisSourceDependencyPressureKind.AuthoredRuntimeSurface => "Expected pressure for authored runtime fields, profiles, validation, or gizmos. Review if setup meaning duplicates contracts or graph guidance.",
                PyralisSourceDependencyPressureKind.EditorAudit => "Expected pressure for graph, evidence, or validator code; review for duplicated setup truth before splitting.",
                PyralisSourceDependencyPressureKind.GrammarVocabulary => "Vocabulary pressure is acceptable when it is wording only; move feature-specific setup meaning back to contracts/reflection.",
                PyralisSourceDependencyPressureKind.DirectSceneQuerySurface => "Direct scene query pressure should stay explicit and shrink when participant/session-native paths can provide the reference.",
                PyralisSourceDependencyPressureKind.ScannerImplementation => "Scanner pressure describes the audit tool itself; tune false positives before treating this as runtime architecture risk.",
                _ => "Runtime ownership pressure; check whether this script owns too many domains or should delegate to a feature service/profile/presenter."
            };
        }
    }
}
