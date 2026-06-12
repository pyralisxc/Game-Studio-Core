using NeonBlack.Gameplay.Data.Definitions;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Composition
{
    [AuthoringContract(
        Capability = AuthoringCapability.Setup | AuthoringCapability.Session, 
        Relevance = "The runtime entry point for custom game features and modular logic.", 
        Axioms = AuthoringWorldAxiom.None,
        AssignmentFields = new[] { nameof(IFeatureModuleRuntime.ModuleId) },
        FirstProof = "proof.custom-object-effect",
        NativeSetup = new[] { "Implement interface in a feature module component" }
    ,
        ExpertAdvice = "Implement this interface on any component that needs to participate in the feature-host lifecycle. It provides access to shared services via the InitializationContext.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/composition")]
    public interface IFeatureModuleRuntime
{
        string ModuleId { get; }
        void InitializeFeature(FeatureRuntimeInitializationContext initializationContext);
        void ShutdownFeature();
    }
}
