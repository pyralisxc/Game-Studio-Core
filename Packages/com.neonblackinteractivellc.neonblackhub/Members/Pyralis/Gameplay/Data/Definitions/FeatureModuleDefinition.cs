using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    public enum FeatureNetworkRole
    {
        OfflineOnly,
        Replicated,
        Predicted,
        ServerAuthoritative,
        CosmeticOnly
    }

    public enum FeatureAuthoringGizmoMode
    {
        None,
        Optional,
        Required
    }

    /// <summary>
    /// Authoring definition for an attachable runtime feature module.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Setup, 
        Relevance = "Authoring container for attachable runtime logic, used to extend Pawns or Game Modes with modular functionality.",
        AssignmentFields = new[] { nameof(moduleId), nameof(displayName), nameof(profileAsset), nameof(runtimePrefab) },
        FirstProof = "Add this Feature Module to the 'Required Feature Modules' list on a Game Mode or Pawn Definition.",
        NativeSetup = new[] { "Define Module ID.", "Assign Runtime Prefab and Profile Asset." },
        ExpertAdvice = "Module ID must be unique across the project. Use 'OfflineOnly' network role for purely visual or local-state modules.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/composition"
    )]
[CreateAssetMenu(menuName = "NeonBlack/Definitions/Feature Module Definition", fileName = "FeatureModuleDefinition", order = 50)]
    public class FeatureModuleDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

        private const string FeatureRuntimeInterfaceName = "NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime";

        public string moduleId = "feature.module";
        public string displayName = "Feature Module";
        public int installOrder = 100;
        public bool enabledByDefault = true;
        public bool requiredForMode = false;
        public string[] featureTags;
        public ActorPresentationMode[] supportedPresentationModes;
        public FeatureNetworkRole networkRole = FeatureNetworkRole.OfflineOnly;
        public string replicationPolicyId = string.Empty;
        public bool requiresOwnership;
        public bool requiresAuthority;
        public bool requiresPrediction;
        public bool requiresServerExecution;
        public string authoringCategory = "General";
        public FeatureAuthoringGizmoMode gizmoMode = FeatureAuthoringGizmoMode.Optional;

        [Tooltip("Optional authored profile consumed by the runtime module.")]
        public ScriptableObject profileAsset;

        [Tooltip("Optional runtime prefab instantiated under a PawnRoot when the feature is enabled.")]
        public GameObject runtimePrefab;

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public void Sanitize()
        {
            replicationPolicyId = replicationPolicyId != null ? replicationPolicyId.Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(authoringCategory))
                authoringCategory = "General";

            if (gizmoMode == FeatureAuthoringGizmoMode.None)
                gizmoMode = FeatureAuthoringGizmoMode.Optional;

            if (networkRole == FeatureNetworkRole.OfflineOnly)
            {
                replicationPolicyId = string.Empty;
                requiresOwnership = false;
                requiresAuthority = false;
                requiresPrediction = false;
                requiresServerExecution = false;
            }
        }

        public bool SupportsPresentationMode(ActorPresentationMode mode)
        {
            if (supportedPresentationModes == null || supportedPresentationModes.Length == 0)
                return true;

            for (int i = 0; i < supportedPresentationModes.Length; i++)
            {
                if (supportedPresentationModes[i] == mode)
                    return true;
            }

            return false;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(authoringCategory))
                issues.Add("Feature modules should declare an authoring category for designer-facing tooling.");

            if (networkRole == FeatureNetworkRole.OfflineOnly)
            {
                if (!string.IsNullOrWhiteSpace(replicationPolicyId)
                    || requiresOwnership
                    || requiresAuthority
                    || requiresPrediction
                    || requiresServerExecution)
                {
                    issues.Add("OfflineOnly modules should not declare replication policies or authority/prediction requirements.");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(replicationPolicyId))
                    issues.Add("Networked feature modules should declare a replication policy id.");

                if (networkRole == FeatureNetworkRole.CosmeticOnly
                    && (requiresOwnership || requiresAuthority || requiresPrediction || requiresServerExecution))
                {
                    issues.Add("CosmeticOnly modules cannot require ownership, authority, prediction, or server execution.");
                }

                if (networkRole == FeatureNetworkRole.Predicted && !requiresPrediction)
                    issues.Add("Predicted modules should declare prediction support.");

                if (networkRole == FeatureNetworkRole.Predicted && !requiresOwnership)
                    issues.Add("Predicted modules should require ownership so local prediction has an authority source.");

                if (networkRole == FeatureNetworkRole.ServerAuthoritative && !requiresServerExecution)
                    issues.Add("ServerAuthoritative modules should require server execution.");
            }

            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId(moduleId);

            if (runtimePrefab == null)
                issues.Add(GetMissingRuntimePrefabMessage(contract));
            else
            {
                bool hasFeatureRuntime = HasFeatureRuntime(runtimePrefab);
                bool matchesContractRuntime = AppendContractRuntimePrefabIssues(runtimePrefab, contract, issues);
                if (runtimePrefab.GetComponentsInChildren<MonoBehaviour>(true).Length == 0 || !hasFeatureRuntime)
                    issues.Add(GetRuntimePrefabMissingRuntimeMessage(contract));

                if (hasFeatureRuntime && matchesContractRuntime)
                    AppendRuntimeValidationProviderIssues(runtimePrefab, issues);
            }

            AppendContractProfileIssue(contract, issues);

            return issues;
        }

        public List<string> GetActorCompatibilityIssues(GameObject actorRoot, ActorPresentationMode presentationMode, bool isEnemyActor = false)
        {
            List<string> issues = new List<string>();
            if (actorRoot == null)
                return issues;

            // Reflective Contract Validation
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId(moduleId);
            if (contract != null)
            {
                // Validate Required Components
                if (contract.RequiredComponentNames != null)
                {
                    foreach (var typeName in contract.RequiredComponentNames)
                    {
                        if (!HasComponentOfType(actorRoot, typeName))
                            issues.Add($"`{moduleId}` expects a {GetShortTypeName(typeName)} on the actor root.");
                    }
                }

                // Validate Required Interfaces (from attribute)
                if (contract.RequiredRuntimeInterfaceNames != null)
                {
                    foreach (var interfaceName in contract.RequiredRuntimeInterfaceNames)
                    {
                        // Skip the base runtime interface which is checked separately or is on the module itself
                        if (string.Equals(interfaceName, FeatureRuntimeInterfaceName, StringComparison.Ordinal))
                            continue;

                        // Specific exception for combat modifiers on enemies if needed (legacy parity)
                        if (moduleId == "actor.status" && isEnemyActor && interfaceName.Contains("IActorCombatModifierReceiver"))
                            continue;

                        // Conditional check for interaction bridge
                        if (moduleId == "actor.interaction" && presentationMode != ActorPresentationMode.Sprite2D && interfaceName.Contains("IActorInteractionInputReceiver2D"))
                            continue;

                        if (!HasComponentImplementing(actorRoot, interfaceName))
                            issues.Add($"`{moduleId}` expects a component implementing {GetShortTypeName(interfaceName)} on the actor root.");
                    }
                }

                // Presentation Lane Validation
                if (contract.IsExplicitlyUnsupported(presentationMode))
                {
                    issues.Add(!string.IsNullOrWhiteSpace(contract.UnsupportedLaneMessage) 
                        ? contract.UnsupportedLaneMessage 
                        : $"`{moduleId}` is explicitly unsupported for {presentationMode} presentation.");
                }
            }

            return issues;
        }

        private static string GetShortTypeName(string fullTypeName)
        {
            if (string.IsNullOrWhiteSpace(fullTypeName)) return string.Empty;
            int lastDot = fullTypeName.LastIndexOf('.');
            return lastDot >= 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
        }

        private static bool HasFeatureRuntime(GameObject prefab)
        {
            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (ImplementsTypeName(behaviours[i], FeatureRuntimeInterfaceName))
                    return true;
            }

            return false;
        }

        private string GetMissingRuntimePrefabMessage(ResolvedAuthoringContract contract)
        {
            if (contract != null && contract.SourceType != null)
                return $"Runtime Prefab is required. Create or assign a prefab with `{contract.SourceType.Name}` on its root, then assign it to FeatureModuleDefinition.runtimePrefab.";

            return "Runtime Prefab is required for a feature module definition. Create or assign a prefab with a component that implements IFeatureModuleRuntime.";
        }

        private string GetRuntimePrefabMissingRuntimeMessage(ResolvedAuthoringContract contract)
        {
            if (contract != null && contract.SourceType != null)
                return $"Runtime Prefab must contain `{contract.SourceType.Name}` or another component that implements IFeatureModuleRuntime for module `{moduleId}`. Create an empty prefab, add `{contract.SourceType.Name}`, then assign that prefab to FeatureModuleDefinition.runtimePrefab.";

            return "Runtime Prefab must contain at least one component that implements IFeatureModuleRuntime. Add the runtime component for this module to the prefab root.";
        }

        private void AppendContractProfileIssue(ResolvedAuthoringContract contract, List<string> issues)
        {
            if (contract == null || contract.RequiredProfileType == null)
                return;

            if (profileAsset == null)
            {
                issues.Add($"Profile Asset is required. Create or assign `{contract.RequiredProfileType.Name}` to FeatureModuleDefinition.profileAsset for module `{moduleId}`.");
                return;
            }

            if (!contract.RequiredProfileType.IsInstanceOfType(profileAsset))
                issues.Add($"Profile Asset must be `{contract.RequiredProfileType.Name}` for module `{moduleId}`.");
        }

        private bool AppendContractRuntimePrefabIssues(GameObject prefab, ResolvedAuthoringContract contract, List<string> issues)
        {
            if (prefab == null || contract == null)
                return true;

            if (contract.SourceType != null
                && typeof(MonoBehaviour).IsAssignableFrom(contract.SourceType)
                && !HasComponentOfTypeOnRoot(prefab, contract.SourceType.FullName))
            {
                issues.Add($"Runtime Prefab `{prefab.name}` is not the expected feature runtime prefab for module `{moduleId}`. Create or assign a prefab whose root has `{contract.SourceType.Name}`; do not assign the pawn prefab here unless it intentionally contains that feature runtime component.");
                return false;
            }

            if (contract.RequiredRuntimeInterfaceNames == null)
                return true;

            bool ready = true;

            for (int i = 0; i < contract.RequiredRuntimeInterfaceNames.Length; i++)
            {
                string interfaceName = contract.RequiredRuntimeInterfaceNames[i];
                if (string.IsNullOrWhiteSpace(interfaceName))
                    continue;

                if (!HasComponentImplementing(prefab, interfaceName))
                {
                    issues.Add($"Runtime Prefab should expose `{GetShortTypeName(interfaceName)}` for module `{contract.ModuleId}`.");
                    ready = false;
                }
            }

            return ready;
        }

        private static bool HasComponentOfTypeOnRoot(GameObject target, string fullTypeName)
        {
            if (target == null)
                return false;

            Component[] components = target.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                    continue;

                if (string.Equals(components[i].GetType().FullName, fullTypeName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool HasComponentOfType(GameObject target, string fullTypeName)
        {
            if (target == null)
                return false;

            Component[] components = target.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                    continue;

                if (string.Equals(components[i].GetType().FullName, fullTypeName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool HasComponentImplementing(GameObject target, string interfaceFullTypeName)
        {
            if (target == null)
                return false;

            MonoBehaviour[] behaviours = target.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (ImplementsTypeName(behaviours[i], interfaceFullTypeName))
                    return true;
            }

            return false;
        }

        private static bool ImplementsTypeName(MonoBehaviour behaviour, string interfaceFullTypeName)
        {
            if (behaviour == null || string.IsNullOrWhiteSpace(interfaceFullTypeName))
                return false;

            Type[] interfaces = behaviour.GetType().GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (string.Equals(interfaces[i].FullName, interfaceFullTypeName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static void AppendRuntimeValidationProviderIssues(GameObject prefab, List<string> issues)
        {
            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IRuntimeValidationProvider provider)
                    continue;

                foreach (string issue in provider.GetRuntimeValidationIssues())
                {
                    if (!string.IsNullOrWhiteSpace(issue))
                        issues.Add(issue);
                }
            }
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
