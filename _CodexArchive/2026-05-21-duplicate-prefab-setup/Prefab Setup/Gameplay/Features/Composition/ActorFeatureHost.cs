using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Runtime;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Features.Composition
{
    /// <summary>
    /// Installs and manages authored feature runtimes for an actor.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Features/Actor Feature Host")]
    public class ActorFeatureHost : MonoBehaviour
    {
        private readonly List<IFeatureModuleRuntime> _featureModules = new List<IFeatureModuleRuntime>();
        public IReadOnlyList<IFeatureModuleRuntime> InstalledModules => _featureModules;

        public void InitializeFeatures(FeatureHostInitializationContext initializationContext, FeatureModuleDefinition[] definitions)
        {
            ShutdownFeatures();

            if (definitions == null || initializationContext == null || initializationContext.ActorContext == null)
                return;

            FeatureModuleDefinition[] orderedDefinitions = (FeatureModuleDefinition[])definitions.Clone();
            System.Array.Sort(orderedDefinitions, CompareDefinitions);

            foreach (FeatureModuleDefinition definition in orderedDefinitions)
            {
                if (definition == null || !definition.enabledByDefault || definition.runtimePrefab == null)
                    continue;

                if (!definition.SupportsPresentationMode(initializationContext.ActorContext.PresentationMode))
                    continue;

                GameObject instance = Instantiate(definition.runtimePrefab, transform);
                if (initializationContext.Services != null
                    && initializationContext.Services.TryResolve(out IObjectResolver resolver)
                    && resolver != null)
                {
                    resolver.InjectGameObject(instance);
                }

                MonoBehaviour[] behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] is not IFeatureModuleRuntime featureRuntime)
                        continue;

                    featureRuntime.InitializeFeature(new FeatureRuntimeInitializationContext(
                        initializationContext.ActorContext,
                        definition,
                        initializationContext.Services));
                    _featureModules.Add(featureRuntime);
                }
            }
        }

        public void InitializeFeatures(ActorFeatureContext context, FeatureModuleDefinition[] definitions)
        {
            PlatformServiceRegistry services = GameplayPlatformContext.Current != null
                ? GameplayPlatformContext.Current.Services
                : new PlatformServiceRegistry();

            InitializeFeatures(new FeatureHostInitializationContext(context, services), definitions);
        }

        public bool TryGetInstalledFeature<T>(out T feature) where T : class
        {
            for (int i = 0; i < _featureModules.Count; i++)
            {
                if (_featureModules[i] is T typedFeature)
                {
                    feature = typedFeature;
                    return true;
                }
            }

            feature = null;
            return false;
        }

        public void ShutdownFeatures()
        {
            for (int i = _featureModules.Count - 1; i >= 0; i--)
                _featureModules[i].ShutdownFeature();

            _featureModules.Clear();
        }

        private void OnDestroy()
        {
            ShutdownFeatures();
        }

        private static int CompareDefinitions(FeatureModuleDefinition left, FeatureModuleDefinition right)
        {
            int order = (left != null ? left.installOrder : 0).CompareTo(right != null ? right.installOrder : 0);
            if (order != 0)
                return order;

            return string.CompareOrdinal(left != null ? left.moduleId : string.Empty, right != null ? right.moduleId : string.Empty);
        }
    }
}
