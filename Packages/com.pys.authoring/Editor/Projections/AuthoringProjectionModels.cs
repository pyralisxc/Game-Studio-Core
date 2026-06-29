using System.Collections.Generic;

namespace Pys.Authoring.Editor.Projections
{
    public sealed class IntentProjection
    {
        public IntentProjection()
        {
            Rows = new List<IntentRow>();
        }

        public List<IntentRow> Rows { get; }

        public int SelectableCount;

        public string SelectedContractId;

        public string SelectedDisplayName;

        public string SelectedDisabledReason;

        public string SelectedFeatureToggles;

        public string SelectedLane;

        public string SelectedCompositionSummary;
    }

    public sealed class IntentRow
    {
        public string ContractId;
        public string DisplayName;
        public string Category;
        public string CapabilityPath;
        public string Surface;
        public string Summary;
        public bool Selectable;
        public string DisabledReason;
        public string StableId;
        public string SourceType;
        public string SourcePath;
        public string OrganizationPattern;
        public int DependencyCount;
        public string IntentToggles;
        public string IntentLanes;
        public string CompatibleStableIds;
        public string SupportingStableIds;
        public string HoverExplanations;
        public string SuccessDescription;
        public string ReadinessHint;
        public string ExpectedEvidence;
        public string CompletionSignals;
        public string ValidationOwnerStableId;
        public string IntentSource;
        public int Priority;
    }

    public sealed class FactsProjection
    {
        public FactsProjection()
        {
            Rows = new List<FactRow>();
        }

        public List<FactRow> Rows { get; }

        public int AssemblyCount;
        public int NamespaceCount;
        public int TypeCount;
        public int ScriptCount;
        public int FieldCount;
        public int ContractCount;
        public int ValidatorCount;
        public int SceneObjectCount;
        public int PrefabCount;
        public int AssetCount;
        public int IssueCount;
    }

    public sealed class FactRow
    {
        public string Kind;
        public string Label;
        public string Detail;
        public string SourcePath;
        public int SourceCount;
        public string Confidence;
    }

    public sealed class MapProjection
    {
        public MapProjection()
        {
            Rows = new List<MapRow>();
        }

        public List<MapRow> Rows { get; }
    }

    public sealed class MapRow
    {
        public string Id;
        public string Label;
        public string Kind;
        public string SourcePath;
        public int ComponentCount;
        public int IssueCount;
        public bool CanPing;
        public bool CanSelect;
        public string NavigationKind;
        public string NavigationLabel;
    }

    public sealed class OverviewProjection
    {
        public OverviewProjection()
        {
            NextActions = new List<OverviewActionRow>();
        }

        public List<OverviewActionRow> NextActions { get; }

        public string Summary;
        public string NextAction;
        public string Reason;
        public string SelectedIntent;
        public string ProofTarget;
        public string Readiness;
        public int IssueCount;
    }

    public sealed class OverviewActionRow
    {
        public int Order;
        public string Title;
        public string Detail;
        public string ActionKind;
        public string ActionLabel;
        public string NativeAction;
        public string SourceRole;
        public string OwnerId;
        public bool BlocksReadiness;
    }

    public sealed class GuideProjection
    {
        public GuideProjection()
        {
            Rows = new List<GuideRow>();
        }

        public List<GuideRow> Rows { get; }

        public string SelectedContractId;

        public string SelectedDisplayName;

        public string ProofTarget;

        public bool ProofReady;
    }

    public sealed class GuideRow
    {
        public int Order;
        public string Role;
        public string OwnerId;
        public string Title;
        public string Detail;
        public string ActionKind;
        public string ActionLabel;
        public string NativeAction;
        public string SuccessCheck;
        public bool BlocksProof;
        public string StableId;
        public string RouteStage;
        public int RouteOrder;
        public string SetupDomain;
    }
}
