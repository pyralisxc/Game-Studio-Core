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
        NativeSetup = new[] { "Create Asset.", "Define Module ID.", "Assign Runtime Prefab and Profile Asset." },
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
        private static readonly string RuntimeValidationProviderInterfaceName = typeof(IRuntimeValidationProvider).FullName;

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

            if (runtimePrefab == null)
                issues.Add("Runtime Prefab is required for a feature module definition.");
            else
            {
                if (runtimePrefab.GetComponentsInChildren<MonoBehaviour>(true).Length == 0
                    || !HasFeatureRuntime(runtimePrefab))
                    issues.Add("Runtime Prefab must contain at least one component that implements IFeatureModuleRuntime.");

                AppendRuntimeValidationProviderIssues(runtimePrefab, issues);
            }

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

            // Support for generic IRuntimeValidationProvider components on the actor root.
            // This allows modules to perform custom actor-specific validation without hardcoding logic here.
            AppendActorValidationProviderIssues(actorRoot, issues);

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

        private static bool HasComponentOfType(GameObject target, string fullTypeName)
        {
            if (target == null)
                return false;

            MonoBehaviour[] behaviours = target.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null)
                    continue;

                if (string.Equals(behaviours[i].GetType().FullName, fullTypeName, StringComparison.Ordinal))
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
                if (behaviours[i] == null || !ImplementsTypeName(behaviours[i], RuntimeValidationProviderInterfaceName))
                    continue;

                if (!TryGetRuntimeValidationIssues(behaviours[i], out IEnumerable<string> providerIssues))
                    continue;

                foreach (string issue in providerIssues)
                {
                    if (!string.IsNullOrWhiteSpace(issue))
                        issues.Add(issue);
                }
            }
        }

        private static void AppendActorValidationProviderIssues(GameObject actorRoot, List<string> issues)
        {
            MonoBehaviour[] behaviours = actorRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null || !ImplementsTypeName(behaviours[i], RuntimeValidationProviderInterfaceName))
                    continue;

                if (!TryGetRuntimeValidationIssues(behaviours[i], out IEnumerable<string> providerIssues))
                    continue;

                foreach (string issue in providerIssues)
                {
                    if (!string.IsNullOrWhiteSpace(issue))
                        issues.Add(issue);
                }
            }
        }

        private static bool TryGetRuntimeValidationIssues(MonoBehaviour behaviour, out IEnumerable<string> issues)
        {
            issues = null;
            if (behaviour == null)
                return false;

            var method = behaviour.GetType().GetMethod("GetRuntimeValidationIssues", Type.EmptyTypes);
            if (method == null)
                return false;

            issues = method.Invoke(behaviour, null) as IEnumerable<string>;
            return issues != null;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
