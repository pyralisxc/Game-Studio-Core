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

        public AuthoringActionKind NativeActionKind { get; set; }

        public List<string> SetupSteps { get; }

        public List<string> SuccessChecks { get; }

        public List<string> OwnershipClaims { get; }

        public List<string> RoleTags { get; }

        public List<string> MetadataGaps { get; }
    }
}
