using System;

namespace Pys.Authoring.Contracts
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class AuthoringContractAttribute : Attribute
    {
        public string StableId { get; set; }

        public string DisplayName { get; set; }

        public string Category { get; set; }

        public string CapabilityPath { get; set; }

        public string[] Tags { get; set; }

        public AuthoringSurface Surface { get; set; } = AuthoringSurface.Auto;

        public string Summary { get; set; }

        public string DocumentationUrl { get; set; }

        public string[] RequiredFields { get; set; }

        public Type[] RequiredComponents { get; set; }

        public string[] RequiredComponentNames { get; set; }

        public Type[] RequiredInterfaces { get; set; }

        public string[] RequiredInterfaceNames { get; set; }

        public string[] PrerequisiteStableIds { get; set; }

        public string RouteStage { get; set; }

        public int RouteOrder { get; set; }

        public string SetupDomain { get; set; }

        public string ProofTarget { get; set; }

        public string SuccessDescription { get; set; }

        public string ReadinessHint { get; set; }

        public string ValidationOwnerStableId { get; set; }

        public string[] ExpectedEvidence { get; set; }

        public string[] CompletionSignals { get; set; }

        public string[] IntentToggles { get; set; }

        public string[] IntentLanes { get; set; }

        public string[] CompatibleStableIds { get; set; }

        public string[] SupportingStableIds { get; set; }

        public string[] HoverExplanations { get; set; }

        public AuthoringActionKind NativeActionKind { get; set; } = AuthoringActionKind.None;

        public string[] SetupSteps { get; set; }

        public string[] SuccessChecks { get; set; }

        public string[] OwnershipClaims { get; set; }

        public string[] RoleTags { get; set; }

        public bool Selectable { get; set; } = true;
    }
}
