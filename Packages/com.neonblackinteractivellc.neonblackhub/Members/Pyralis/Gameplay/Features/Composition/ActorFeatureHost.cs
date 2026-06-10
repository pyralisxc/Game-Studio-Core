using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace NeonBlack.Gameplay.Features.Composition
{
    /// <summary>
    /// Installs and manages authored feature runtimes for an actor.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup | AuthoringCapability.Session, 
        Relevance = "Inspector Add Component path for installing custom feature modules on an actor.",
        AssignmentFields = new[] { "InstalledModules" },
        FirstProof = "Assign feature module definitions and verify they are instantiated at runtime.",
        NativeSetup = new[] { "Add Component" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Features/Actor Feature Host")]
    public class ActorFeatureHost : MonoBehaviour
    {
        private readonly List<IFeatureModuleRuntime> _featureModules = new List<IFeatureModuleRuntime>();
        private readonly List<GameObject> _featureInstances = new List<GameObject>();
        public IReadOnlyList<IFeatureModuleRuntime> InstalledModules => _featureModules;

        public void InitializeFeatures(FeatureHostInitializationContext initializationContext, FeatureModuleDefinition[] definitions)
        {
            ShutdownFeatures();

            if (definitions == null || initializationContext == null || initializationContext.ActorContext == null)
                return;

            FeatureModuleDefinition[] orderedDefinitions = (FeatureModuleDefinition[])definitions.Clone();
            System.Array.Sort(orderedDefinitions, CompareDefinitions);

            IObjectResolver resolver = null;
            if (initializationContext.Services != null)
                initializationContext.Services.TryResolve(out resolver);

            foreach (FeatureModuleDefinition definition in orderedDefinitions)
            {
                if (definition == null || !definition.enabledByDefault || definition.runtimePrefab == null)
                    continue;

                if (!definition.SupportsPresentationMode(initializationContext.ActorContext.PresentationMode))
                    continue;

                GameObject instance = Instantiate(definition.runtimePrefab, transform);
                _featureInstances.Add(instance);
                
                if (resolver != null)
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
                        resolver));
                    _featureModules.Add(featureRuntime);
                }
            }
        }

        public void InitializeFeatures(ActorFeatureContext context, FeatureModuleDefinition[] definitions)
        {
            IObjectResolver resolver = null;
            if (GameplayPlatformContext.TryResolve(out IObjectResolver currentResolver))
                resolver = currentResolver;

            InitializeFeatures(new FeatureHostInitializationContext(context, resolver), definitions);
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

            for (int i = _featureInstances.Count - 1; i >= 0; i--)
                DestroyInstance(_featureInstances[i]);

            _featureInstances.Clear();
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

        private static void DestroyInstance(GameObject instance)
        {
            if (instance == null)
                return;

            instance.SetActive(false);
            instance.transform.SetParent(null, false);

            if (Application.isPlaying)
                Destroy(instance);
            else
                DestroyImmediate(instance);
        }
    }
}
