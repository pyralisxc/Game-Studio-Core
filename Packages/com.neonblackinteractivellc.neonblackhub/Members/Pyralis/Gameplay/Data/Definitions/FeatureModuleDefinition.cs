using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
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
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.Custom },
        CapabilityPath = "Core Setup/Feature Modules/Feature Module Definition",
        Relevance = "Authoring container for attachable runtime logic, used to extend Pawns or Game Modes with modular functionality.",
        RoleTags = new[] { AuthoringContractRoleTags.IntentRouteEssential, AuthoringContractRoleTags.FeatureModuleRouteSupport },
        AssignmentFields = new[] { nameof(moduleId), nameof(displayName), nameof(profileAsset), nameof(runtimePrefab) },
        Proof = "Add this Feature Module to the 'Required Feature Modules' list on a Game Mode or Pawn Definition.",
        NativeSetup = new[] { "Define Module ID.", "Assign Runtime Prefab and Profile Asset." },
        Surface = AuthoringContractSurface.RouteEssential,
        ExpertAdvice = "Module ID must be unique across the project. Use 'OfflineOnly' network role for purely visual or local-state modules.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/composition"
    )]
[CreateAssetMenu(menuName = "NeonBlack/Definitions/Feature Module Definition", fileName = "FeatureModuleDefinition", order = 50)]
    public class FeatureModuleDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return BuildRuntimeValidationIssues();
        }

        private const string FeatureRuntimeInterfaceName = "NeonBlack.Gameplay.Modules.Actor.Composition.IFeatureModuleRuntime";

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
            List<PyralisRuntimeValidationIssue> runtimeIssues = BuildRuntimeValidationIssues();
            for (int i = 0; i < runtimeIssues.Count; i++)
            {
                if (runtimeIssues[i] != null && !string.IsNullOrWhiteSpace(runtimeIssues[i].Message))
                    issues.Add(runtimeIssues[i].Message);
            }

            return issues;
        }

        private List<PyralisRuntimeValidationIssue> BuildRuntimeValidationIssues()
        {
            List<PyralisRuntimeValidationIssue> issues = new List<PyralisRuntimeValidationIssue>();

            if (string.IsNullOrWhiteSpace(authoringCategory))
            {
                issues.Add(PyralisRuntimeValidationIssue.Recommended(
                    "Feature modules should declare an authoring category for designer-facing tooling.",
                    nameof(authoringCategory),
                    nameof(FeatureModuleDefinition),
                    "Set FeatureModuleDefinition.authoringCategory to a stable designer-facing category.",
                    "FeatureModuleDefinition has an authoring category.",
                    "FeatureModuleDefinition.AuthoringCategory.Missing"));
            }

            if (networkRole == FeatureNetworkRole.OfflineOnly)
            {
                if (!string.IsNullOrWhiteSpace(replicationPolicyId)
                    || requiresOwnership
                    || requiresAuthority
                    || requiresPrediction
                    || requiresServerExecution)
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        "OfflineOnly modules should not declare replication policies or authority/prediction requirements.",
                        nameof(networkRole),
                        nameof(FeatureModuleDefinition),
                        "Clear replication policy, ownership, authority, prediction, and server execution fields for OfflineOnly modules.",
                        "OfflineOnly FeatureModuleDefinition has no network authority flags.",
                        "FeatureModuleDefinition.NetworkRole.OfflineOnlyContradiction"));
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(replicationPolicyId))
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        "Networked feature modules should declare a replication policy id.",
                        nameof(replicationPolicyId),
                        nameof(FeatureModuleDefinition),
                        "Set FeatureModuleDefinition.replicationPolicyId for this networked module.",
                        "Networked FeatureModuleDefinition has a replication policy id.",
                        "FeatureModuleDefinition.ReplicationPolicy.Missing"));
                }

                if (networkRole == FeatureNetworkRole.CosmeticOnly
                    && (requiresOwnership || requiresAuthority || requiresPrediction || requiresServerExecution))
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        "CosmeticOnly modules cannot require ownership, authority, prediction, or server execution.",
                        nameof(networkRole),
                        nameof(FeatureModuleDefinition),
                        "Clear ownership, authority, prediction, and server execution flags for CosmeticOnly modules.",
                        "CosmeticOnly FeatureModuleDefinition is presentation-only.",
                        "FeatureModuleDefinition.NetworkRole.CosmeticAuthorityContradiction"));
                }

                if (networkRole == FeatureNetworkRole.Predicted && !requiresPrediction)
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        "Predicted modules should declare prediction support.",
                        nameof(requiresPrediction),
                        nameof(FeatureModuleDefinition),
                        "Enable FeatureModuleDefinition.requiresPrediction for a Predicted module.",
                        "Predicted FeatureModuleDefinition declares prediction support.",
                        "FeatureModuleDefinition.Prediction.Required"));
                }

                if (networkRole == FeatureNetworkRole.Predicted && !requiresOwnership)
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        "Predicted modules should require ownership so local prediction has an authority source.",
                        nameof(requiresOwnership),
                        nameof(FeatureModuleDefinition),
                        "Enable FeatureModuleDefinition.requiresOwnership for a Predicted module.",
                        "Predicted FeatureModuleDefinition requires ownership.",
                        "FeatureModuleDefinition.Ownership.RequiredForPrediction"));
                }

                if (networkRole == FeatureNetworkRole.ServerAuthoritative && !requiresServerExecution)
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        "ServerAuthoritative modules should require server execution.",
                        nameof(requiresServerExecution),
                        nameof(FeatureModuleDefinition),
                        "Enable FeatureModuleDefinition.requiresServerExecution for a ServerAuthoritative module.",
                        "Server-authoritative FeatureModuleDefinition requires server execution.",
                        "FeatureModuleDefinition.ServerExecution.Required"));
                }
            }

            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId(moduleId);

            if (runtimePrefab == null)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    GetMissingRuntimePrefabMessage(contract),
                    nameof(runtimePrefab),
                    nameof(FeatureModuleDefinition),
                    BuildMissingRuntimePrefabNativeAction(contract),
                    "FeatureModuleDefinition.runtimePrefab references a prefab with the expected feature runtime.",
                    "FeatureModuleDefinition.RuntimePrefab.Missing"));
            }
            else
            {
                bool hasFeatureRuntime = HasFeatureRuntime(runtimePrefab);
                bool matchesContractRuntime = AppendContractRuntimePrefabIssues(runtimePrefab, contract, issues);
                if (runtimePrefab.GetComponentsInChildren<MonoBehaviour>(true).Length == 0 || !hasFeatureRuntime)
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        GetRuntimePrefabMissingRuntimeMessage(contract),
                        nameof(runtimePrefab),
                        nameof(FeatureModuleDefinition),
                        BuildRuntimePrefabMissingRuntimeNativeAction(contract),
                        "FeatureModuleDefinition.runtimePrefab contains an IFeatureModuleRuntime component.",
                        "FeatureModuleDefinition.RuntimePrefab.RuntimeMissing"));
                }

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

                        // Enemy actors can receive status effects without exposing the pawn-facing combat modifier surface.
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

        private string BuildMissingRuntimePrefabNativeAction(ResolvedAuthoringContract contract)
        {
            if (contract != null && contract.SourceType != null)
                return $"Create a prefab with {contract.SourceType.Name} on the root, then assign it to FeatureModuleDefinition.runtimePrefab.";

            return "Create a prefab with a component that implements IFeatureModuleRuntime, then assign it to FeatureModuleDefinition.runtimePrefab.";
        }

        private string BuildRuntimePrefabMissingRuntimeNativeAction(ResolvedAuthoringContract contract)
        {
            if (contract != null && contract.SourceType != null)
                return $"Open the runtime prefab and add {contract.SourceType.Name} or another IFeatureModuleRuntime component.";

            return "Open the runtime prefab and add the runtime component for this feature module.";
        }

        private void AppendContractProfileIssue(ResolvedAuthoringContract contract, List<PyralisRuntimeValidationIssue> issues)
        {
            if (contract == null || contract.RequiredProfileType == null)
                return;

            if (profileAsset == null)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    $"Profile Asset is required. Create or assign `{contract.RequiredProfileType.Name}` to FeatureModuleDefinition.profileAsset for module `{moduleId}`.",
                    nameof(profileAsset),
                    nameof(FeatureModuleDefinition),
                    $"Create or assign {contract.RequiredProfileType.Name}, then set FeatureModuleDefinition.profileAsset.",
                    "FeatureModuleDefinition.profileAsset references the contract-required profile type.",
                    "FeatureModuleDefinition.ProfileAsset.Missing"));
                return;
            }

            if (!contract.RequiredProfileType.IsInstanceOfType(profileAsset))
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    $"Profile Asset must be `{contract.RequiredProfileType.Name}` for module `{moduleId}`.",
                    nameof(profileAsset),
                    nameof(FeatureModuleDefinition),
                    $"Assign a {contract.RequiredProfileType.Name} asset to FeatureModuleDefinition.profileAsset.",
                    "FeatureModuleDefinition.profileAsset matches the contract-required profile type.",
                    "FeatureModuleDefinition.ProfileAsset.TypeMismatch"));
            }
        }

        private bool AppendContractRuntimePrefabIssues(GameObject prefab, ResolvedAuthoringContract contract, List<PyralisRuntimeValidationIssue> issues)
        {
            if (prefab == null || contract == null)
                return true;

            if (contract.SourceType != null
                && typeof(MonoBehaviour).IsAssignableFrom(contract.SourceType)
                && !HasComponentOfTypeOnRoot(prefab, contract.SourceType.FullName))
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    $"Runtime Prefab `{prefab.name}` is not the expected feature runtime prefab for module `{moduleId}`. Create or assign a prefab whose root has `{contract.SourceType.Name}`; do not assign the pawn prefab here unless it intentionally contains that feature runtime component.",
                    nameof(runtimePrefab),
                    nameof(FeatureModuleDefinition),
                    $"Assign a prefab whose root has {contract.SourceType.Name} to FeatureModuleDefinition.runtimePrefab.",
                    "FeatureModuleDefinition.runtimePrefab root has the contract source runtime component.",
                    "FeatureModuleDefinition.RuntimePrefab.ContractRuntimeMismatch"));
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
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        $"Runtime Prefab should expose `{GetShortTypeName(interfaceName)}` for module `{contract.ModuleId}`.",
                        nameof(runtimePrefab),
                        nameof(FeatureModuleDefinition),
                        $"Open the runtime prefab and add a component implementing {GetShortTypeName(interfaceName)}.",
                        "FeatureModuleDefinition.runtimePrefab exposes the contract-required runtime interface.",
                        "FeatureModuleDefinition.RuntimePrefab.InterfaceMissing." + GetShortTypeName(interfaceName)));
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

        private static void AppendRuntimeValidationProviderIssues(GameObject prefab, List<PyralisRuntimeValidationIssue> issues)
        {
            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IRuntimeValidationProvider provider)
                    continue;

                foreach (PyralisRuntimeValidationIssue issue in provider.GetRuntimeValidationIssues())
                {
                    if (issue != null && !string.IsNullOrWhiteSpace(issue.Message))
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
