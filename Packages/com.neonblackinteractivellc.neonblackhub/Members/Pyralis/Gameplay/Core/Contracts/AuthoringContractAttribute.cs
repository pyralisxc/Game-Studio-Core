using System;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Core.Contracts
{
    /// <summary>
    /// Metadata attribute used to tag classes and interfaces for reflective discovery by the Pyralis Authoring Window.
    /// This allows the system to automatically surface features in the Guide and Facts tabs without manual registration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class AuthoringContractAttribute : Attribute
    {
        /// <summary>
        /// The formal engine capability this contract belongs to (e.g., Combat, Movement).
        /// Use Flags to support compositional intent (e.g., Combat | Puzzle).
        /// </summary>
        public AuthoringCapability Capability { get; set; }

        /// <summary>
        /// Categorized priority for this contract. 
        /// Dynamic priority is calculated at runtime based on Axiom matches using this as the base.
        /// </summary>
        public AuthoringPriority Priority { get; set; } = AuthoringPriority.Unspecified;

        /// <summary>
        /// Allows 1-99 for precise Auxiliary sorting. If set, this overrides the default enum value.
        /// </summary>
        public int PriorityValueOverride { get; set; } = -1;

        /// <summary>
        /// The package version string where this contract was marked as deprecated.
        /// Surfaces HYG006 warnings.
        /// </summary>
        public string DeprecatedInVersion { get; set; }

        /// <summary>
        /// The package version string where this contract is scheduled for removal.
        /// Enforced by automated unit tests.
        /// </summary>
        public string RemovableInVersion { get; set; }

        /// <summary>
        /// Stable feature module identity used by reflective authoring and proof-card mapping.
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// The presentation lane this contract belongs to (e.g., "Sprite2D", "Rigged3D").
        /// </summary>
        public string Lane { get; set; }

        /// <summary>
        /// A beginner-friendly description of why this contract matters and what it does.
        /// </summary>
        public string Relevance { get; set; }

        /// <summary>
        /// A direct link to the technical documentation or wiki for this contract.
        /// </summary>
        public string DocumentationURL { get; set; }

        /// <summary>
        /// A relative path to a manual file within the package documentation folder.
        /// </summary>
        public string ManualPath { get; set; }

        /// <summary>
        /// Context-sensitive advice or pro-tips for using this contract effectively.
        /// </summary>
        public string ExpertAdvice { get; set; }

        /// <summary>
        /// Granular world properties that this contract is designed for. 
/// Use this to match against the granular intent in the Authoring Window.
        /// </summary>
        public AuthoringWorldAxiom Axioms { get; set; }

        /// <summary>
        /// Comma-separated search keywords for the authoring advisor.
        /// </summary>
        public string AxiomKeywords { get; set; }

        /// <summary>
        /// Interfaces that must be implemented for this contract to be considered satisfied.
        /// </summary>
        public Type[] RequiredInterfaces { get; set; }

        /// <summary>
        /// Fully qualified names of interfaces that must be implemented. 
        /// Use this for cross-assembly references where the source assembly cannot reference the interface type directly.
        /// </summary>
        public string[] RequiredInterfaceNames { get; set; }

        /// <summary>
        /// Components that must be present on a GameObject for this contract to be considered satisfied.
        /// </summary>
        public Type[] RequiredComponents { get; set; }

        /// <summary>
        /// Fully qualified names of components that must be present.
        /// Use this for cross-assembly references where the source assembly cannot reference the component type directly.
        /// </summary>
        public string[] RequiredComponentNames { get; set; }

        /// <summary>
        /// A human-readable explanation of why these requirements satisfy the contract.
        /// </summary>
        public string SatisfactionReason { get; set; }

        /// <summary>
        /// The type of profile (ScriptableObject) used to configure this contract.
        /// </summary>
        public Type ProfileType { get; set; }

        /// <summary>
        /// Presentation lanes that this contract explicitly supports.
        /// </summary>
        public ActorPresentationMode[] SupportedLanes { get; set; }

        /// <summary>
        /// Presentation lanes that this contract explicitly does NOT support.
        /// </summary>
        public ActorPresentationMode[] UnsupportedLanes { get; set; }

        /// <summary>
        /// A custom message to display when this contract is used in an unsupported lane.
        /// </summary>
        public string UnsupportedLaneMessage { get; set; }

        /// <summary>
        /// Roles or tags consumed by this contract (e.g., "MainCamera", "PlayerPawn").
        /// </summary>
        public string[] ConsumedRoles { get; set; }

        /// <summary>
        /// Native setup steps or components that this contract provides.
        /// </summary>
        public string[] NativeSetup { get; set; }

        /// <summary>
        /// The first thing a user should do to prove this contract is working.
        /// </summary>
        public string FirstProof { get; set; }

        /// <summary>
        /// Fields or properties that should be highlighted for assignment in the authoring UI.
        /// </summary>
        public string[] AssignmentFields { get; set; }

        /// <summary>
        /// Key moments where this contract can be customized or extended.
        /// </summary>
        public string[] CustomizationMoments { get; set; }

        public AuthoringContractAttribute() { }

        public AuthoringContractAttribute(AuthoringCapability capability, string relevance, AuthoringWorldAxiom axioms)
        {
            Capability = capability;
            Relevance = relevance;
            Axioms = axioms;
        }

        public AuthoringContractAttribute(string relevance, AuthoringWorldAxiom axioms)
        {
            Relevance = relevance;
            Axioms = axioms;
        }

        public AuthoringContractAttribute(string relevance, AuthoringWorldAxiom axioms, Type[] requiredInterfaces = null, Type[] requiredComponents = null, string satisfactionReason = null)
        {
            Relevance = relevance;
            Axioms = axioms;
            RequiredInterfaces = requiredInterfaces;
            RequiredComponents = requiredComponents;
            SatisfactionReason = satisfactionReason;
        }
    }
}
