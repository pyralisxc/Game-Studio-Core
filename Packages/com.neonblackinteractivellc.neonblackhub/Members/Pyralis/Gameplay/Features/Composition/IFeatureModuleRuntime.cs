using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Features.Composition
{
    public interface IFeatureModuleRuntime
    {
        string ModuleId { get; }
        void InitializeFeature(FeatureRuntimeInitializationContext initializationContext);
        void ShutdownFeature();
    }
}
