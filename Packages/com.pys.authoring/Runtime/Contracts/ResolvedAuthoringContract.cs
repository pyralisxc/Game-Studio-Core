using System.Collections.Generic;

namespace Pys.Authoring.Contracts
{
    public sealed class ResolvedAuthoringContract
    {
        public ResolvedAuthoringContract(string stableId, string sourceTypeName)
        {
            StableId = stableId ?? string.Empty;
            SourceTypeName = sourceTypeName ?? string.Empty;
            DisplayName = string.Empty;
            Category = string.Empty;
            CapabilityPath = string.Empty;
            Surface = AuthoringSurface.Auto;
            Summary = string.Empty;
            DocumentationUrl = string.Empty;
            Selectable = true;
            Tags = new List<string>();
            RequiredFields = new List<string>();
            RequiredComponents = new List<string>();
            RequiredInterfaces = new List<string>();
            PrerequisiteStableIds = new List<string>();
            RouteStage = string.Empty;
            RouteOrder = 0;
            SetupDomain = string.Empty;
            ProofTarget = string.Empty;
            SuccessDescription = string.Empty;
            ReadinessHint = string.Empty;
            ValidationOwnerStableId = string.Empty;
            ExpectedEvidence = new List<string>();
            CompletionSignals = new List<string>();
            IntentToggles = new List<string>();
            IntentLanes = new List<string>();
            CompatibleStableIds = new List<string>();
            SupportingStableIds = new List<string>();
            HoverExplanations = new List<string>();
            NativeActionKind = AuthoringActionKind.None;
            SetupSteps = new List<string>();
            SuccessChecks = new List<string>();
            OwnershipClaims = new List<string>();
            RoleTags = new List<string>();
            MetadataGaps = new List<string>();
        }

        public string StableId { get; }

        public string SourceTypeName { get; }

        public string DisplayName { get; set; }

        public string Category { get; set; }

        public string CapabilityPath { get; set; }

        public AuthoringSurface Surface { get; set; }

        public string Summary { get; set; }

        public string DocumentationUrl { get; set; }

        public bool Selectable { get; set; }

        public List<string> Tags { get; }

        public List<string> RequiredFields { get; }

        public List<string> RequiredComponents { get; }

        public List<string> RequiredInterfaces { get; }

        public List<string> PrerequisiteStableIds { get; }

        public string RouteStage { get; set; }

        public int RouteOrder { get; set; }

        public string SetupDomain { get; set; }

        public string ProofTarget { get; set; }

        public string SuccessDescription { get; set; }

        public string ReadinessHint { get; set; }

        public string ValidationOwnerStableId { get; set; }

        public List<string> ExpectedEvidence { get; }

        public List<string> CompletionSignals { get; }

        public List<string> IntentToggles { get; }

        public List<string> IntentLanes { get; }

        public List<string> CompatibleStableIds { get; }

        public List<string> SupportingStableIds { get; }

        public List<string> HoverExplanations { get; }

        public AuthoringActionKind NativeActionKind { get; set; }

        public List<string> SetupSteps { get; }

        public List<string> SuccessChecks { get; }

        public List<string> OwnershipClaims { get; }

        public List<string> RoleTags { get; }

        public List<string> MetadataGaps { get; }
    }
}
