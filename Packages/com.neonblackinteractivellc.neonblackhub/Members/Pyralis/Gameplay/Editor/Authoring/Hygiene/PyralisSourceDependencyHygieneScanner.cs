using System;
using NeonBlack.Gameplay.Data.Definitions.Combat;
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
        ActionRuntimeSurface,
        TabletopRuntimeSurface,
        RpgRuntimeSurface,
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
        Vocabulary,
        DirectSceneQuerySurface,
        ReflectionMeaningLeak,
        ValidatorGuideLeak,
        InspectorRouteGuideLeak,
        ExportTruthLeak,
        TabRendererLogicLeak,
        LegacyDocTruthLeak,
        CompatibilityBridge,
        OldOwnerName,
        NamespaceDependencyFanout,
        DirectModuleCommunication,
        LifecycleBooleanCluster,
        StateMachineMissing,
        EventChannelOveruse,
        ManagerBehaviorLeak,
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
            "Glue",
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
        private static readonly Regex LifecycleBooleanRegex = new Regex(@"\b(?:private|protected|public)\s+bool\s+[_A-Za-z0-9]*(?:Is|Has|Can|Should|Enabled|Locked|Dead|Grounded|Dashing|Attacking|Moving|Playing|Paused|Active|Started|Finished)[_A-Za-z0-9]*", RegexOptions.Compiled);

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
            int namespaceBudget = ResolveNamespaceBudget(safePath, ownerDomain);
            PyralisSourceDependencyPressureKind pressureKind = ResolvePressureKind(safePath, safeSource, ownerDomain, neonBlackUsingCount, namespaceBudget);
            int riskScore = CalculateRiskScore(domains.Count, concreteCrossDomainCount, serializedFieldCount, localComponentLookupCount, broadUnityDiscoveryCount, staticAccessCount, reflectionOrStringLookupCount, neonBlackUsingCount, namespaceBudget);
            riskScore = ApplyPressureKindRiskFloor(riskScore, pressureKind, safePath, ownerDomain, neonBlackUsingCount, namespaceBudget);
            List<string> reasons = BuildReasons(domains.Count, concreteCrossDomainCount, serializedFieldCount, localComponentLookupCount, broadUnityDiscoveryCount, staticAccessCount, reflectionOrStringLookupCount, neonBlackUsingCount, namespaceBudget);
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
                PyralisSourceDependencyPressureKind.NamespaceDependencyFanout => 10,
                PyralisSourceDependencyPressureKind.DirectModuleCommunication => 11,
                PyralisSourceDependencyPressureKind.LifecycleBooleanCluster => 12,
                PyralisSourceDependencyPressureKind.StateMachineMissing => 13,
                PyralisSourceDependencyPressureKind.EventChannelOveruse => 14,
                PyralisSourceDependencyPressureKind.ManagerBehaviorLeak => 15,
                PyralisSourceDependencyPressureKind.AcceptedComposition => 16,
                PyralisSourceDependencyPressureKind.PawnCoordinator => 17,
                PyralisSourceDependencyPressureKind.PawnCapabilitySibling => 18,
                PyralisSourceDependencyPressureKind.LocalPresentationSurface => 19,
                PyralisSourceDependencyPressureKind.SceneZoneSurface => 20,
                PyralisSourceDependencyPressureKind.InputRoutingSurface => 21,
                PyralisSourceDependencyPressureKind.EnemyCapabilityModule => 22,
                PyralisSourceDependencyPressureKind.EnemyCoordinator => 23,
                PyralisSourceDependencyPressureKind.CombatContactSurface => 24,
                PyralisSourceDependencyPressureKind.PawnRuntimeHelper => 25,
                PyralisSourceDependencyPressureKind.NetworkAdapterSurface => 26,
                PyralisSourceDependencyPressureKind.SceneNavigationSurface => 27,
                PyralisSourceDependencyPressureKind.ActionRuntimeSurface => 28,
                PyralisSourceDependencyPressureKind.TabletopRuntimeSurface => 29,
                PyralisSourceDependencyPressureKind.RpgRuntimeSurface => 30,
                PyralisSourceDependencyPressureKind.RpgSceneSurface => 31,
                PyralisSourceDependencyPressureKind.ScoringRuntimeSurface => 32,
                PyralisSourceDependencyPressureKind.SpawningRuntimeSurface => 33,
                PyralisSourceDependencyPressureKind.GameFlowRuntimeSurface => 34,
                PyralisSourceDependencyPressureKind.ContractReflectionSurface => 35,
                PyralisSourceDependencyPressureKind.PersistenceDataSurface => 36,
                PyralisSourceDependencyPressureKind.ActorFeatureContext => 37,
                PyralisSourceDependencyPressureKind.SceneCameraRig => 38,
                PyralisSourceDependencyPressureKind.AuthoredDataAsset => 39,
                PyralisSourceDependencyPressureKind.HazardRuntimeSurface => 40,
                PyralisSourceDependencyPressureKind.DomainUtility => 41,
                PyralisSourceDependencyPressureKind.FeatureModule => 42,
                PyralisSourceDependencyPressureKind.AuthoredRuntimeSurface => 43,
                PyralisSourceDependencyPressureKind.ReferenceAssembly => 44,
                PyralisSourceDependencyPressureKind.EditorAudit => 45,
                PyralisSourceDependencyPressureKind.Vocabulary => 46,
                PyralisSourceDependencyPressureKind.ScannerImplementation => 45,
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
            int reflectionOrStringLookupCount,
            int neonBlackUsingCount,
            int namespaceBudget)
        {
            int score = Math.Max(0, domainCount - 2);
            score += Math.Max(0, neonBlackUsingCount - namespaceBudget) * 2;
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
            int reflectionOrStringLookupCount,
            int neonBlackUsingCount,
            int namespaceBudget)
        {
            List<string> reasons = new List<string>();
            if (neonBlackUsingCount > namespaceBudget)
                reasons.Add("Imports " + neonBlackUsingCount + " Pyralis namespace(s); budget is " + namespaceBudget + ".");
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

        private static int ApplyPressureKindRiskFloor(
            int riskScore,
            PyralisSourceDependencyPressureKind pressureKind,
            string assetPath,
            string ownerDomain,
            int neonBlackUsingCount,
            int namespaceBudget)
        {
            if (IsOwnershipLeakPressure(pressureKind))
                return Math.Max(riskScore, 8);

            if (pressureKind == PyralisSourceDependencyPressureKind.NamespaceDependencyFanout)
            {
                int floor = IsGlueCompositionPath(assetPath, ownerDomain) ? 4 : 4;
                if (!IsGlueCompositionPath(assetPath, ownerDomain) && neonBlackUsingCount > 5)
                    floor = 8;
                if (neonBlackUsingCount >= namespaceBudget + 6)
                    floor = 12;
                return Math.Max(riskScore, floor);
            }

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
                PyralisSourceDependencyPressureKind.NamespaceDependencyFanout => "Source imports more Pyralis namespaces than its owner budget allows.",
                PyralisSourceDependencyPressureKind.DirectModuleCommunication => "Runtime source directly imports another feature module instead of a stable contract/event/state reader.",
                PyralisSourceDependencyPressureKind.LifecycleBooleanCluster => "Runtime source contains clustered lifecycle booleans without a focused state machine surface.",
                PyralisSourceDependencyPressureKind.StateMachineMissing => "Runtime source appears to own explicit states without a focused state machine owner.",
                PyralisSourceDependencyPressureKind.EventChannelOveruse => "Runtime source publishes or subscribes to many event-channel messages; verify the channel is not becoming hidden control flow.",
                PyralisSourceDependencyPressureKind.ManagerBehaviorLeak => "Manager-like runtime source combines broad discovery/static access with non-composition ownership.",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(reason) && !reasons.Contains(reason))
                reasons.Add(reason);
        }

        private static PyralisSourceDependencyPressureKind ResolvePressureKind(string assetPath, string source, string ownerDomain, int neonBlackUsingCount, int namespaceBudget)
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

            if (IsManagerBehaviorLeak(normalized, fileName, safeSource, ownerDomain))
                return PyralisSourceDependencyPressureKind.ManagerBehaviorLeak;

            if (IsEventChannelOveruse(normalized, safeSource, ownerDomain))
                return PyralisSourceDependencyPressureKind.EventChannelOveruse;

            if (IsLifecycleBooleanCluster(normalized, fileName, safeSource))
                return PyralisSourceDependencyPressureKind.LifecycleBooleanCluster;

            if (IsStateMachineMissing(normalized, fileName, safeSource))
                return PyralisSourceDependencyPressureKind.StateMachineMissing;

            if (IsDirectModuleCommunication(normalized, safeSource, ownerDomain))
                return PyralisSourceDependencyPressureKind.DirectModuleCommunication;

            if (neonBlackUsingCount > namespaceBudget)
                return PyralisSourceDependencyPressureKind.NamespaceDependencyFanout;

            if (normalized.EndsWith(".md", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.EditorAudit;

            if (normalized.Contains("/Editor/Authoring/Vocabulary/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.Vocabulary;

            if (normalized.Contains("/Core/Contracts/Authoring/", StringComparison.Ordinal)
                || string.Equals(fileName, "ResolvedAuthoringContractRegistry.cs", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.ContractReflectionSurface;
            }

            if (normalized.Contains("/Editor/Authoring/Validation/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Authoring/Evidence/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Authoring/Graph/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Authoring/Projections/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Authoring/Exports/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Authoring/Window/", StringComparison.Ordinal)
                || normalized.Contains("/Editor/Inspectors/Pyralis/", StringComparison.Ordinal)
                || normalized.Contains("/Modules/Rpg/Editor/", StringComparison.Ordinal)
                || string.Equals(ownerDomain, "Editor", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.EditorAudit;
            }

            if (normalized.Contains("/Glue/Bootstrap/", StringComparison.Ordinal)
                || normalized.Contains("/Glue/InputRouting/", StringComparison.Ordinal)
                || normalized.Contains("/Glue/Lifetime/", StringComparison.Ordinal)
                || normalized.Contains("/Glue/Ownership/", StringComparison.Ordinal)
                || normalized.Contains("/Glue/Participants/", StringComparison.Ordinal)
                || normalized.Contains("/Glue/SceneServices/", StringComparison.Ordinal)
                || normalized.Contains("/Glue/ServiceRegistration/", StringComparison.Ordinal)
                || normalized.Contains("/Glue/Session/", StringComparison.Ordinal)
                || normalized.Contains("/Glue/Spawning/", StringComparison.Ordinal)
                || normalized.Contains("/Modules/Rpg/Runtime/Composition/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.AcceptedComposition;
            }

            if (fileName.Contains("SaveData", StringComparison.Ordinal)
                || fileName.Contains("Snapshot", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.PersistenceDataSurface;
            }

            if (string.Equals(fileName, "SceneGuard.cs", StringComparison.Ordinal)
                || normalized.Contains("/Glue/SceneFlow/Navigation/", StringComparison.Ordinal))
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
                || normalized.Contains("/Modules/", StringComparison.Ordinal) && normalized.Contains("/Data/", StringComparison.Ordinal)
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

            if (normalized.Contains("/Modules/Feedback/", StringComparison.Ordinal)
                || normalized.Contains("/Modules/Combat/UI/", StringComparison.Ordinal)
                || normalized.Contains("/Presentation/HUD/", StringComparison.Ordinal)
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

            if (normalized.Contains("/Modules/Hazards/Zones/", StringComparison.Ordinal)
                || fileName.Contains("Zone", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.SceneZoneSurface;
            }

            if (string.Equals(fileName, "ParticipantInputRouter.cs", StringComparison.Ordinal)
                || fileName.Contains("InputBridge", StringComparison.Ordinal)
                || normalized.Contains("/Modules/Input/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.InputRoutingSurface;
            }

            if (normalized.Contains("/Modules/Enemies/", StringComparison.Ordinal)
                && fileName.Contains("Module", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.EnemyCapabilityModule;
            }

            if (normalized.Contains("/Modules/Enemies/", StringComparison.Ordinal)
                && (fileName.Contains("AI", StringComparison.Ordinal)
                    || fileName.Contains("Processor", StringComparison.Ordinal)))
            {
                return PyralisSourceDependencyPressureKind.EnemyCoordinator;
            }

            if (normalized.Contains("/Modules/Rpg/Runtime/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.RpgRuntimeSurface;
            }

            if (normalized.Contains("/Modules/Actions/Runtime/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.ActionRuntimeSurface;
            }

            if (normalized.Contains("/Modules/Tabletop/Runtime/", StringComparison.Ordinal)
                || normalized.Contains("/Modules/Tabletop/", StringComparison.Ordinal))
            {
                return PyralisSourceDependencyPressureKind.TabletopRuntimeSurface;
            }

            if (normalized.Contains("/Modules/Rpg/UI/", StringComparison.Ordinal)
                || normalized.Contains("/Modules/Rpg/", StringComparison.Ordinal)
                    && (fileName.Contains("SceneController", StringComparison.Ordinal)
                        || fileName.Contains("Panel", StringComparison.Ordinal)
                        || fileName.Contains("Router", StringComparison.Ordinal)
                        || fileName.Contains("Presenter", StringComparison.Ordinal)))
            {
                return PyralisSourceDependencyPressureKind.RpgSceneSurface;
            }

            if (normalized.Contains("/Modules/Scoring/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.ScoringRuntimeSurface;

            if (normalized.Contains("/Modules/Spawning/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.SpawningRuntimeSurface;

            if (normalized.Contains("/Modules/Combat/", StringComparison.Ordinal)
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

            if (normalized.Contains("/Modules/Hazards/", StringComparison.Ordinal)
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

            if (normalized.Contains("/Glue/SceneFlow/", StringComparison.Ordinal)
                || normalized.Contains("/Modules/Encounters/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.GameFlowRuntimeSurface;

            if (normalized.Contains("/PlayerRegistry", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.DirectSceneQuerySurface;

            return PyralisSourceDependencyPressureKind.RuntimeOwnership;
        }

        private static int ResolveNamespaceBudget(string assetPath, string ownerDomain)
        {
            string normalized = (assetPath ?? string.Empty).Replace('\\', '/');
            if (normalized.EndsWith(".md", StringComparison.Ordinal))
                return 100;
            if (normalized.Contains("/Editor/", StringComparison.Ordinal))
                return 8;
            if (IsGlueCompositionPath(normalized, ownerDomain))
                return 6;
            if (string.Equals(ownerDomain, "Core", StringComparison.Ordinal)
                || string.Equals(ownerDomain, "Data", StringComparison.Ordinal))
            {
                return 2;
            }

            if (string.Equals(ownerDomain, "Presentation", StringComparison.Ordinal))
                return 2;

            return 3;
        }

        private static bool IsGlueCompositionPath(string assetPath, string ownerDomain)
        {
            string normalized = (assetPath ?? string.Empty).Replace('\\', '/');
            return string.Equals(ownerDomain, "Glue", StringComparison.Ordinal)
                || normalized.Contains("/Glue/", StringComparison.Ordinal)
                || normalized.Contains("/Runtime/Composition/", StringComparison.Ordinal);
        }

        public static bool IsOwnershipLeakPressure(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind == PyralisSourceDependencyPressureKind.ReflectionMeaningLeak
                || pressureKind == PyralisSourceDependencyPressureKind.ValidatorGuideLeak
                || pressureKind == PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak
                || pressureKind == PyralisSourceDependencyPressureKind.ExportTruthLeak
                || pressureKind == PyralisSourceDependencyPressureKind.TabRendererLogicLeak
                || pressureKind == PyralisSourceDependencyPressureKind.CompatibilityBridge
                || pressureKind == PyralisSourceDependencyPressureKind.NamespaceDependencyFanout
                || pressureKind == PyralisSourceDependencyPressureKind.DirectModuleCommunication
                || pressureKind == PyralisSourceDependencyPressureKind.LifecycleBooleanCluster
                || pressureKind == PyralisSourceDependencyPressureKind.StateMachineMissing
                || pressureKind == PyralisSourceDependencyPressureKind.EventChannelOveruse
                || pressureKind == PyralisSourceDependencyPressureKind.ManagerBehaviorLeak;
        }

        private static bool IsDirectModuleCommunication(string normalizedPath, string source, string ownerDomain)
        {
            if (!IsRuntimeSourcePressurePath(normalizedPath, ownerDomain)
                || !normalizedPath.Contains("/Modules/", StringComparison.Ordinal))
            {
                return false;
            }

            foreach (Match match in UsingRegex.Matches(source ?? string.Empty))
            {
                string usingNamespace = match.Groups[1].Value;
                if (!usingNamespace.StartsWith("NeonBlack.Gameplay.Modules.", StringComparison.Ordinal))
                    continue;

                string dependencyDomain = ResolveDomainFromNamespace(usingNamespace);
                if (string.IsNullOrWhiteSpace(dependencyDomain)
                    || string.Equals(dependencyDomain, ownerDomain, StringComparison.Ordinal)
                    || IsAcceptedDirectModuleCommunication(normalizedPath, source, ownerDomain, dependencyDomain))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static bool IsAcceptedDirectModuleCommunication(string normalizedPath, string source, string ownerDomain, string dependencyDomain)
        {
            if (string.Equals(dependencyDomain, "Actor", StringComparison.Ordinal)
                || string.Equals(dependencyDomain, "Composition", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(ownerDomain, "Character", StringComparison.Ordinal)
                && string.Equals(dependencyDomain, "Combat", StringComparison.Ordinal)
                && ContainsAny(normalizedPath, "PawnCombat", "PawnDamage", "PawnBlock", "PawnWeapon", "PawnProjectile"))
            {
                return true;
            }

            if (string.Equals(ownerDomain, "Enemies", StringComparison.Ordinal)
                && string.Equals(dependencyDomain, "Combat", StringComparison.Ordinal)
                && ContainsAny(normalizedPath, "EnemyCombat", "EnemyMovementModule", "EnemyAnimationModule", "EnemyReaction"))
            {
                return true;
            }

            if (string.Equals(ownerDomain, "Hazards", StringComparison.Ordinal)
                && string.Equals(dependencyDomain, "Combat", StringComparison.Ordinal)
                && ContainsAny(source, "IActorStatusEffectReceiver", "StatusEffectDefinition", "KnockbackReceiver"))
            {
                return true;
            }

            if (string.Equals(ownerDomain, "Feedback", StringComparison.Ordinal)
                && string.Equals(dependencyDomain, "Combat", StringComparison.Ordinal)
                && ContainsAny(source, "StatusEffectDefinition", "IActorHealthState"))
            {
                return true;
            }

            return false;
        }

        private static bool IsLifecycleBooleanCluster(string normalizedPath, string fileName, string source)
        {
            if (!IsRuntimeSourcePressurePath(normalizedPath, ResolveDomainFromPath(normalizedPath))
                || fileName.Contains("StateMachine", StringComparison.Ordinal))
            {
                return false;
            }

            int lifecycleBooleanCount = LifecycleBooleanRegex.Matches(source ?? string.Empty).Count;
            if (lifecycleBooleanCount < 4)
                return false;

            return !ContainsAny(source, "StateMachine", "LocomotionState", "ActionState", "LifecycleState");
        }

        private static bool IsStateMachineMissing(string normalizedPath, string fileName, string source)
        {
            if (!IsRuntimeSourcePressurePath(normalizedPath, ResolveDomainFromPath(normalizedPath))
                || fileName.Contains("StateMachine", StringComparison.Ordinal))
            {
                return false;
            }

            bool ownsExplicitState = Regex.IsMatch(source ?? string.Empty, @"\benum\s+[A-Za-z0-9_]*State\b", RegexOptions.Multiline)
                || Regex.IsMatch(source ?? string.Empty, @"\b[A-Za-z0-9_]*State\s+[_A-Za-z0-9]+\s*=", RegexOptions.Multiline);
            return ownsExplicitState && !ContainsAny(source, "StateMachine");
        }

        private static bool IsEventChannelOveruse(string normalizedPath, string source, string ownerDomain)
        {
            if (!IsRuntimeSourcePressurePath(normalizedPath, ownerDomain)
                || !source.Contains("IGameplayEventChannel", StringComparison.Ordinal))
            {
                return false;
            }

            int publishCount = CountOccurrences(source, ".Publish(");
            int subscribeCount = CountOccurrences(source, ".Subscribe<");
            return publishCount + subscribeCount >= 4;
        }

        private static bool IsManagerBehaviorLeak(string normalizedPath, string fileName, string source, string ownerDomain)
        {
            if (!IsRuntimeSourcePressurePath(normalizedPath, ownerDomain)
                || IsGlueCompositionPath(normalizedPath, ownerDomain))
            {
                return false;
            }

            bool managerName = ContainsAny(fileName, "Manager", "Service", "Controller", "Coordinator");
            if (!managerName)
                return false;

            int broadDiscovery = BroadUnityDiscoveryRegex.Matches(source ?? string.Empty).Count;
            int staticAccess = StaticAccessRegex.Matches(source ?? string.Empty).Count;
            int localLookup = LocalComponentLookupRegex.Matches(source ?? string.Empty).Count;
            return broadDiscovery > 0 || staticAccess > 1 || localLookup >= 8;
        }

        private static bool IsRuntimeSourcePressurePath(string normalizedPath, string ownerDomain)
        {
            if (string.IsNullOrWhiteSpace(normalizedPath)
                || normalizedPath.EndsWith(".md", StringComparison.Ordinal)
                || normalizedPath.Contains("/Editor/", StringComparison.Ordinal)
                || normalizedPath.Contains("/Tests/", StringComparison.Ordinal))
            {
                return false;
            }

            return !string.Equals(ownerDomain, "Core", StringComparison.Ordinal)
                && !string.Equals(ownerDomain, "Data", StringComparison.Ordinal);
        }

        private static int CountOccurrences(string value, string needle)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(needle))
                return 0;

            int count = 0;
            int index = 0;
            while ((index = value.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += needle.Length;
            }

            return count;
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
            return (normalizedPath.Contains("/Editor/Authoring/Reflection/", StringComparison.Ordinal)
                    || normalizedPath.Contains("/Editor/Authoring/Evidence/", StringComparison.Ordinal))
                && ContainsAny(source,
                    "proof.",
                    "Proof",
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
            bool inspectorSurface = normalizedPath.Contains("/Editor/Inspectors/Pyralis/", StringComparison.Ordinal)
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
            if (!normalizedPath.Contains("/Editor/Authoring/Window/", StringComparison.Ordinal)
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
            bool tabRenderer = normalizedPath.Contains("/Editor/Authoring/Window/", StringComparison.Ordinal)
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
            if ((normalizedPath.Contains("/Authoring/Reflection/", StringComparison.Ordinal)
                    || normalizedPath.Contains("/Authoring/Evidence/", StringComparison.Ordinal))
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
                && normalizedPath.EndsWith("/Editor/Authoring/Hygiene/PyralisSourceDependencyHygieneScanner.cs", StringComparison.Ordinal);
        }

        private static bool ContainsRouteGuideWording(string source)
        {
            return ContainsAny(source,
                "Route Proof",
                "Do Now",
                "Overview",
                "Guide",
                "Map owns",
                "Guide Trace",
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
                "Authoring owns proof");
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
                PyralisSourceDependencyPressureKind.NamespaceDependencyFanout => "This script imports too many Pyralis namespaces for its owner. Split behavior, depend on smaller contracts/data/events, or move wiring to Glue instead of hiding it in a broad manager.",
                PyralisSourceDependencyPressureKind.DirectModuleCommunication => "A runtime module directly imports another feature module. Prefer stable contracts, events, state readers, or a documented same-capability composition edge.",
                PyralisSourceDependencyPressureKind.LifecycleBooleanCluster => "Several lifecycle booleans are clustered in one runtime source. Consider a focused state machine or state reader owned by this feature lane.",
                PyralisSourceDependencyPressureKind.StateMachineMissing => "This source owns explicit state vocabulary without an obvious state machine owner. Extract transition rules into a plain state machine when lifecycle rules grow.",
                PyralisSourceDependencyPressureKind.EventChannelOveruse => "The event channel should report typed facts, not hide control flow. Split broad publishers/subscribers into narrower handlers or state readers when pressure stays high.",
                PyralisSourceDependencyPressureKind.ManagerBehaviorLeak => "Manager-like runtime code is doing discovery or broad ownership outside Glue. Move composition to Glue or move feature behavior to the real module owner.",
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
                PyralisSourceDependencyPressureKind.ActionRuntimeSurface => "Expected pressure for queued gameplay action runtime. Review if broad feature semantics move into the queue instead of staying with feature-local resolvers.",
                PyralisSourceDependencyPressureKind.TabletopRuntimeSurface => "Expected pressure for tabletop board, turn, and selection runtime. Review if it starts owning platform session, participant identity, or unrelated scene services.",
                PyralisSourceDependencyPressureKind.RpgRuntimeSurface => "Expected pressure for RPG runtime domain, contracts, and services. Review if this area mixes scene UI, Unity object discovery, or platform composition behavior into feature-local runtime services.",
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
                PyralisSourceDependencyPressureKind.Vocabulary => "Vocabulary pressure is acceptable when it is wording only; move feature-specific setup meaning back to contracts/reflection.",
                PyralisSourceDependencyPressureKind.DirectSceneQuerySurface => "Direct scene query pressure should stay explicit and shrink when participant/session-native paths can provide the reference.",
                PyralisSourceDependencyPressureKind.ScannerImplementation => "Scanner pressure describes the audit tool itself; tune false positives before treating this as runtime architecture risk.",
                _ => "Runtime ownership pressure; check whether this script owns too many domains or should delegate to a feature service/profile/presenter."
            };
        }
    }
}
