using System;
using System.Collections.Generic;
using System.IO;
using Pys.Authoring.Editor.Projections;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Exports
{
    public static class ProjectionJsonExporter
    {
        public static string ExportIntent(IntentProjection projection, string scriptsRoot)
        {
            return Write("intent", scriptsRoot, ToIntentJson(projection, scriptsRoot));
        }

        public static string ExportFacts(FactsProjection projection, string scriptsRoot)
        {
            return Write("facts", scriptsRoot, ToFactsJson(projection, scriptsRoot));
        }

        public static string ExportMap(MapProjection projection, string scriptsRoot)
        {
            return Write("map", scriptsRoot, ToMapJson(projection, scriptsRoot));
        }

        public static string ExportOverview(OverviewProjection projection, string scriptsRoot)
        {
            return Write("overview", scriptsRoot, ToOverviewJson(projection, scriptsRoot));
        }

        public static string ExportGuide(GuideProjection projection, string scriptsRoot)
        {
            return Write("guide", scriptsRoot, ToGuideJson(projection, scriptsRoot));
        }

        public static string ToIntentJson(IntentProjection projection, string scriptsRoot)
        {
            return JsonUtility.ToJson(IntentDto.FromProjection(projection, scriptsRoot), true);
        }

        public static string ToFactsJson(FactsProjection projection, string scriptsRoot)
        {
            return JsonUtility.ToJson(FactsDto.FromProjection(projection, scriptsRoot), true);
        }

        public static string ToMapJson(MapProjection projection, string scriptsRoot)
        {
            return JsonUtility.ToJson(MapDto.FromProjection(projection, scriptsRoot), true);
        }

        public static string ToOverviewJson(OverviewProjection projection, string scriptsRoot)
        {
            return JsonUtility.ToJson(OverviewDto.FromProjection(projection, scriptsRoot), true);
        }

        public static string ToGuideJson(GuideProjection projection, string scriptsRoot)
        {
            return JsonUtility.ToJson(GuideDto.FromProjection(projection, scriptsRoot), true);
        }

        private static string Write(string suffix, string scriptsRoot, string json)
        {
            string folder = Path.Combine(Path.GetFullPath("."), AuthoringGraphJsonExporter.DefaultExportFolder);
            Directory.CreateDirectory(folder);

            string safeRoot = string.IsNullOrWhiteSpace(scriptsRoot)
                ? "Project"
                : scriptsRoot.Replace('/', '_').Replace('\\', '_').Replace(':', '_');

            string path = Path.Combine(folder, safeRoot + "_" + suffix + ".json");
            File.WriteAllText(path, json);
            EditorUtility.RevealInFinder(path);
            return path;
        }

        [Serializable]
        private sealed class IntentDto
        {
            public string scriptsRoot;
            public int selectableCount;
            public string selectedContractId;
            public string selectedDisplayName;
            public string selectedDisabledReason;
            public string selectedFeatureToggles;
            public string selectedLane;
            public string selectedCompositionSummary;
            public List<IntentRowDto> rows = new List<IntentRowDto>();

            public static IntentDto FromProjection(IntentProjection projection, string scriptsRoot)
            {
                IntentDto dto = new IntentDto
                {
                    scriptsRoot = scriptsRoot ?? string.Empty,
                    selectableCount = projection != null ? projection.SelectableCount : 0,
                    selectedContractId = projection != null ? projection.SelectedContractId : string.Empty,
                    selectedDisplayName = projection != null ? projection.SelectedDisplayName : string.Empty,
                    selectedDisabledReason = projection != null ? projection.SelectedDisabledReason : string.Empty,
                    selectedFeatureToggles = projection != null ? projection.SelectedFeatureToggles : string.Empty,
                    selectedLane = projection != null ? projection.SelectedLane : string.Empty,
                    selectedCompositionSummary = projection != null ? projection.SelectedCompositionSummary : string.Empty
                };

                if (projection == null)
                    return dto;

                for (int i = 0; i < projection.Rows.Count; i++)
                    dto.rows.Add(IntentRowDto.FromRow(projection.Rows[i]));

                return dto;
            }
        }

        [Serializable]
        private sealed class IntentRowDto
        {
            public string contractId;
            public string displayName;
            public string category;
            public string capabilityPath;
            public string surface;
            public string summary;
            public bool selectable;
            public string disabledReason;
            public string stableId;
            public string sourceType;
            public string sourcePath;
            public string organizationPattern;
            public int dependencyCount;
            public string intentToggles;
            public string intentLanes;
            public string compatibleStableIds;
            public string supportingStableIds;
            public string hoverExplanations;
            public string successDescription;
            public string readinessHint;
            public string expectedEvidence;
            public string completionSignals;
            public string validationOwnerStableId;
            public string intentSource;
            public int priority;

            public static IntentRowDto FromRow(IntentRow row)
            {
                return new IntentRowDto
                {
                    contractId = row != null ? row.ContractId : string.Empty,
                    displayName = row != null ? row.DisplayName : string.Empty,
                    category = row != null ? row.Category : string.Empty,
                    capabilityPath = row != null ? row.CapabilityPath : string.Empty,
                    surface = row != null ? row.Surface : string.Empty,
                    summary = row != null ? row.Summary : string.Empty,
                    selectable = row != null && row.Selectable,
                    disabledReason = row != null ? row.DisabledReason : string.Empty,
                    stableId = row != null ? row.StableId : string.Empty,
                    sourceType = row != null ? row.SourceType : string.Empty,
                    sourcePath = row != null ? row.SourcePath : string.Empty,
                    organizationPattern = row != null ? row.OrganizationPattern : string.Empty,
                    dependencyCount = row != null ? row.DependencyCount : 0,
                    intentToggles = row != null ? row.IntentToggles : string.Empty,
                    intentLanes = row != null ? row.IntentLanes : string.Empty,
                    compatibleStableIds = row != null ? row.CompatibleStableIds : string.Empty,
                    supportingStableIds = row != null ? row.SupportingStableIds : string.Empty,
                    hoverExplanations = row != null ? row.HoverExplanations : string.Empty,
                    successDescription = row != null ? row.SuccessDescription : string.Empty,
                    readinessHint = row != null ? row.ReadinessHint : string.Empty,
                    expectedEvidence = row != null ? row.ExpectedEvidence : string.Empty,
                    completionSignals = row != null ? row.CompletionSignals : string.Empty,
                    validationOwnerStableId = row != null ? row.ValidationOwnerStableId : string.Empty,
                    intentSource = row != null ? row.IntentSource : string.Empty,
                    priority = row != null ? row.Priority : 0
                };
            }
        }

        [Serializable]
        private sealed class FactsDto
        {
            public string scriptsRoot;
            public int assemblyCount;
            public int namespaceCount;
            public int typeCount;
            public int scriptCount;
            public int fieldCount;
            public int contractCount;
            public int validatorCount;
            public int sceneObjectCount;
            public int prefabCount;
            public int assetCount;
            public int issueCount;
            public List<FactRowDto> rows = new List<FactRowDto>();

            public static FactsDto FromProjection(FactsProjection projection, string scriptsRoot)
            {
                FactsDto dto = new FactsDto
                {
                    scriptsRoot = scriptsRoot ?? string.Empty,
                    assemblyCount = projection != null ? projection.AssemblyCount : 0,
                    namespaceCount = projection != null ? projection.NamespaceCount : 0,
                    typeCount = projection != null ? projection.TypeCount : 0,
                    scriptCount = projection != null ? projection.ScriptCount : 0,
                    fieldCount = projection != null ? projection.FieldCount : 0,
                    contractCount = projection != null ? projection.ContractCount : 0,
                    validatorCount = projection != null ? projection.ValidatorCount : 0,
                    sceneObjectCount = projection != null ? projection.SceneObjectCount : 0,
                    prefabCount = projection != null ? projection.PrefabCount : 0,
                    assetCount = projection != null ? projection.AssetCount : 0,
                    issueCount = projection != null ? projection.IssueCount : 0
                };

                if (projection == null)
                    return dto;

                for (int i = 0; i < projection.Rows.Count; i++)
                    dto.rows.Add(FactRowDto.FromRow(projection.Rows[i]));

                return dto;
            }
        }

        [Serializable]
        private sealed class FactRowDto
        {
            public string kind;
            public string label;
            public string detail;
            public string sourcePath;
            public int sourceCount;
            public string confidence;

            public static FactRowDto FromRow(FactRow row)
            {
                return new FactRowDto
                {
                    kind = row != null ? row.Kind : string.Empty,
                    label = row != null ? row.Label : string.Empty,
                    detail = row != null ? row.Detail : string.Empty,
                    sourcePath = row != null ? row.SourcePath : string.Empty,
                    sourceCount = row != null ? row.SourceCount : 0,
                    confidence = row != null ? row.Confidence : string.Empty
                };
            }
        }

        [Serializable]
        private sealed class MapDto
        {
            public string scriptsRoot;
            public List<MapRowDto> rows = new List<MapRowDto>();

            public static MapDto FromProjection(MapProjection projection, string scriptsRoot)
            {
                MapDto dto = new MapDto { scriptsRoot = scriptsRoot ?? string.Empty };
                if (projection == null)
                    return dto;

                for (int i = 0; i < projection.Rows.Count; i++)
                    dto.rows.Add(MapRowDto.FromRow(projection.Rows[i]));

                return dto;
            }
        }

        [Serializable]
        private sealed class MapRowDto
        {
            public string id;
            public string label;
            public string kind;
            public string sourcePath;
            public int componentCount;
            public int issueCount;
            public bool canPing;
            public bool canSelect;
            public string navigationKind;
            public string navigationLabel;

            public static MapRowDto FromRow(MapRow row)
            {
                return new MapRowDto
                {
                    id = row != null ? row.Id : string.Empty,
                    label = row != null ? row.Label : string.Empty,
                    kind = row != null ? row.Kind : string.Empty,
                    sourcePath = row != null ? row.SourcePath : string.Empty,
                    componentCount = row != null ? row.ComponentCount : 0,
                    issueCount = row != null ? row.IssueCount : 0,
                    canPing = row != null && row.CanPing,
                    canSelect = row != null && row.CanSelect,
                    navigationKind = row != null ? row.NavigationKind : string.Empty,
                    navigationLabel = row != null ? row.NavigationLabel : string.Empty
                };
            }
        }

        [Serializable]
        private sealed class OverviewDto
        {
            public string scriptsRoot;
            public string summary;
            public string nextAction;
            public string reason;
            public string selectedIntent;
            public string proofTarget;
            public string readiness;
            public int issueCount;
            public List<OverviewActionRowDto> nextActions = new List<OverviewActionRowDto>();

            public static OverviewDto FromProjection(OverviewProjection projection, string scriptsRoot)
            {
                OverviewDto dto = new OverviewDto
                {
                    scriptsRoot = scriptsRoot ?? string.Empty,
                    summary = projection != null ? projection.Summary : string.Empty,
                    nextAction = projection != null ? projection.NextAction : string.Empty,
                    reason = projection != null ? projection.Reason : string.Empty,
                    selectedIntent = projection != null ? projection.SelectedIntent : string.Empty,
                    proofTarget = projection != null ? projection.ProofTarget : string.Empty,
                    readiness = projection != null ? projection.Readiness : string.Empty,
                    issueCount = projection != null ? projection.IssueCount : 0
                };

                if (projection == null)
                    return dto;

                for (int i = 0; i < projection.NextActions.Count; i++)
                    dto.nextActions.Add(OverviewActionRowDto.FromRow(projection.NextActions[i]));

                return dto;
            }
        }

        [Serializable]
        private sealed class OverviewActionRowDto
        {
            public int order;
            public string title;
            public string detail;
            public string actionKind;
            public string actionLabel;
            public string nativeAction;
            public string sourceRole;
            public string ownerId;
            public bool blocksReadiness;

            public static OverviewActionRowDto FromRow(OverviewActionRow row)
            {
                return new OverviewActionRowDto
                {
                    order = row != null ? row.Order : 0,
                    title = row != null ? row.Title : string.Empty,
                    detail = row != null ? row.Detail : string.Empty,
                    actionKind = row != null ? row.ActionKind : string.Empty,
                    actionLabel = row != null ? row.ActionLabel : string.Empty,
                    nativeAction = row != null ? row.NativeAction : string.Empty,
                    sourceRole = row != null ? row.SourceRole : string.Empty,
                    ownerId = row != null ? row.OwnerId : string.Empty,
                    blocksReadiness = row != null && row.BlocksReadiness
                };
            }
        }

        [Serializable]
        private sealed class GuideDto
        {
            public string scriptsRoot;
            public string selectedContractId;
            public string selectedDisplayName;
            public string proofTarget;
            public bool proofReady;
            public List<GuideRowDto> rows = new List<GuideRowDto>();

            public static GuideDto FromProjection(GuideProjection projection, string scriptsRoot)
            {
                GuideDto dto = new GuideDto
                {
                    scriptsRoot = scriptsRoot ?? string.Empty,
                    selectedContractId = projection != null ? projection.SelectedContractId : string.Empty,
                    selectedDisplayName = projection != null ? projection.SelectedDisplayName : string.Empty,
                    proofTarget = projection != null ? projection.ProofTarget : string.Empty,
                    proofReady = projection != null && projection.ProofReady
                };
                if (projection == null)
                    return dto;

                for (int i = 0; i < projection.Rows.Count; i++)
                    dto.rows.Add(GuideRowDto.FromRow(projection.Rows[i]));

                return dto;
            }
        }

        [Serializable]
        private sealed class GuideRowDto
        {
            public int order;
            public string role;
            public string ownerId;
            public string title;
            public string detail;
            public string actionKind;
            public string actionLabel;
            public string nativeAction;
            public string successCheck;
            public bool blocksProof;
            public string stableId;
            public string routeStage;
            public int routeOrder;
            public string setupDomain;

            public static GuideRowDto FromRow(GuideRow row)
            {
                return new GuideRowDto
                {
                    order = row != null ? row.Order : 0,
                    role = row != null ? row.Role : string.Empty,
                    ownerId = row != null ? row.OwnerId : string.Empty,
                    title = row != null ? row.Title : string.Empty,
                    detail = row != null ? row.Detail : string.Empty,
                    actionKind = row != null ? row.ActionKind : string.Empty,
                    actionLabel = row != null ? row.ActionLabel : string.Empty,
                    nativeAction = row != null ? row.NativeAction : string.Empty,
                    successCheck = row != null ? row.SuccessCheck : string.Empty,
                    blocksProof = row != null && row.BlocksProof,
                    stableId = row != null ? row.StableId : string.Empty,
                    routeStage = row != null ? row.RouteStage : string.Empty,
                    routeOrder = row != null ? row.RouteOrder : 0,
                    setupDomain = row != null ? row.SetupDomain : string.Empty
                };
            }
        }
    }
}
