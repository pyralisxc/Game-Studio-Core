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
            foreach (string file in Directory.GetFiles(normalizedRoot, "*.cs", SearchOption.AllDirectories))
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
            int riskScore = CalculateRiskScore(domains.Count, concreteCrossDomainCount, serializedFieldCount, localComponentLookupCount, broadUnityDiscoveryCount, staticAccessCount, reflectionOrStringLookupCount);
            PyralisSourceDependencyPressureKind pressureKind = ResolvePressureKind(safePath, safeSource, ownerDomain);
            List<string> reasons = BuildReasons(domains.Count, concreteCrossDomainCount, serializedFieldCount, localComponentLookupCount, broadUnityDiscoveryCount, staticAccessCount, reflectionOrStringLookupCount);

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
                PyralisSourceDependencyPressureKind.AcceptedComposition => 2,
                PyralisSourceDependencyPressureKind.PawnCoordinator => 3,
                PyralisSourceDependencyPressureKind.PawnCapabilitySibling => 4,
                PyralisSourceDependencyPressureKind.LocalPresentationSurface => 5,
                PyralisSourceDependencyPressureKind.SceneZoneSurface => 6,
                PyralisSourceDependencyPressureKind.InputRoutingSurface => 7,
                PyralisSourceDependencyPressureKind.EnemyCapabilityModule => 8,
                PyralisSourceDependencyPressureKind.EnemyCoordinator => 9,
                PyralisSourceDependencyPressureKind.CombatContactSurface => 10,
                PyralisSourceDependencyPressureKind.PawnRuntimeHelper => 11,
                PyralisSourceDependencyPressureKind.NetworkAdapterSurface => 12,
                PyralisSourceDependencyPressureKind.SceneNavigationSurface => 13,
                PyralisSourceDependencyPressureKind.RpgDomainCore => 14,
                PyralisSourceDependencyPressureKind.RpgSceneSurface => 15,
                PyralisSourceDependencyPressureKind.ScoringRuntimeSurface => 16,
                PyralisSourceDependencyPressureKind.SpawningRuntimeSurface => 17,
                PyralisSourceDependencyPressureKind.GameFlowRuntimeSurface => 18,
                PyralisSourceDependencyPressureKind.ContractReflectionSurface => 19,
                PyralisSourceDependencyPressureKind.PersistenceDataSurface => 20,
                PyralisSourceDependencyPressureKind.ActorFeatureContext => 21,
                PyralisSourceDependencyPressureKind.SceneCameraRig => 22,
                PyralisSourceDependencyPressureKind.AuthoredDataAsset => 23,
                PyralisSourceDependencyPressureKind.HazardRuntimeSurface => 24,
                PyralisSourceDependencyPressureKind.DomainUtility => 25,
                PyralisSourceDependencyPressureKind.FeatureModule => 26,
                PyralisSourceDependencyPressureKind.AuthoredRuntimeSurface => 27,
                PyralisSourceDependencyPressureKind.ReferenceAssembly => 28,
                PyralisSourceDependencyPressureKind.EditorAudit => 29,
                PyralisSourceDependencyPressureKind.GrammarVocabulary => 30,
                PyralisSourceDependencyPressureKind.ScannerImplementation => 31,
                _ => 6
            };
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

        private static PyralisSourceDependencyPressureKind ResolvePressureKind(string assetPath, string source, string ownerDomain)
        {
            string normalized = (assetPath ?? string.Empty).Replace('\\', '/');
            string fileName = Path.GetFileName(normalized);
            string safeSource = source ?? string.Empty;
            if (normalized.Contains("/Editor/Authoring/Spine/Hygiene/", StringComparison.Ordinal))
                return PyralisSourceDependencyPressureKind.ScannerImplementation;

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

        private static string BuildReviewHint(PyralisSourceDependencyPressureKind pressureKind)
        {
            return pressureKind switch
            {
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
