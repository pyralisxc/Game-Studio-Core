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
        Relevance = "Installs runtime feature prefabs declared by actor definitions or profiles.",
        NativeSetup = new[]
        {
            "Add one host to the actor root.",
            "Modules are assigned by the actor definition during initialization.",
            "Runtime feature prefabs should contain IFeatureModuleRuntime components."
        },
        FirstProof = "Authored feature prefabs are installed on the actor at runtime.",
        FirstProofTargetId = "proof.custom-object-effect",
        ExpertAdvice = "The ActorFeatureHost is the central manager for dynamic actor capabilities. It handles dependency injection (VContainer) for newly instantiated feature prefabs.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/composition"
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Composition/Actor Feature Host")]
    [DisallowMultipleComponent]
    public class ActorFeatureHost : MonoBehaviour, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (GetComponents<ActorFeatureHost>().Length > 1)
                yield return "This actor has multiple ActorFeatureHost components. Keep one host on the actor root.";

            if (GetComponent("PawnRoot") == null
                && GetComponent("Motor3D") == null
                && GetComponent("EnemyAI") == null)
            {
                yield return "No known actor bootstrap component found. Ensure something calls InitializeFeatures at runtime.";
            }

            if (Application.isPlaying && InstalledModules.Count == 0)
                yield return "No feature modules are currently installed. Check the actor definition/profile feature list.";
        }
        private readonly List<IFeatureModuleRuntime> _featureModules = new List<IFeatureModuleRuntime>();
        private readonly List<GameObject> _featureInstances = new List<GameObject>();
        public IReadOnlyList<IFeatureModuleRuntime> InstalledModules => _featureModules;

        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void InitializeFeatures(FeatureHostInitializationContext initializationContext, FeatureModuleDefinition[] definitions)
        {
            ShutdownFeatures();

            if (definitions == null || initializationContext == null || initializationContext.ActorContext == null)
                return;

            FeatureModuleDefinition[] orderedDefinitions = (FeatureModuleDefinition[])definitions.Clone();
            System.Array.Sort(orderedDefinitions, CompareDefinitions);

            IObjectResolver resolver = initializationContext.Resolver ?? _resolver;

            foreach (FeatureModuleDefinition definition in orderedDefinitions)
            {
                if (definition == null || !definition.enabledByDefault || definition.runtimePrefab == null)
                    continue;

                if (!definition.SupportsPresentationMode(initializationContext.ActorContext.PresentationMode))
                    continue;

                GameObject instance = resolver != null 
                    ? resolver.Instantiate(definition.runtimePrefab, transform)
                    : Instantiate(definition.runtimePrefab, transform);
                _featureInstances.Add(instance);
                
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
