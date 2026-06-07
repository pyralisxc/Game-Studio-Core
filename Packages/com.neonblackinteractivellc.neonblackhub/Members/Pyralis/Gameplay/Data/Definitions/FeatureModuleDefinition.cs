using System;
using System.Collections.Generic;
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
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Feature Module Definition", fileName = "FeatureModuleDefinition", order = 50)]
    public class FeatureModuleDefinition : ScriptableObject
    {
        private const string FeatureRuntimeInterfaceName = "NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime";
        private const string RuntimeValidationProviderInterfaceName = "NeonBlack.Gameplay.Features.Composition.IRuntimeValidationProvider";

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

            switch (moduleId)
            {
                case "actor.interaction":
                    if (presentationMode == ActorPresentationMode.Sprite2D
                        && !HasComponentImplementing(actorRoot, "NeonBlack.Gameplay.Features.Characters.IActorInteractionInputReceiver2D"))
                    {
                        issues.Add("`actor.interaction` on Sprite2D actors should expose an IActorInteractionInputReceiver2D bridge.");
                    }
                    break;

                case "actor.traversal.3d":
                    if (!HasComponentOfType(actorRoot, "NeonBlack.Gameplay.Features.Characters.Motor3D"))
                        issues.Add("`actor.traversal.3d` expects a Motor3D on the actor root.");
                    if (!HasComponentOfType(actorRoot, "NeonBlack.Gameplay.Features.Traversal.Pawn3DTraversalComponent"))
                        issues.Add("`actor.traversal.3d` expects a Pawn3DTraversalComponent on the actor root.");
                    break;

                case "actor.pickups.2d":
                    if (actorRoot.GetComponent<Collider2D>() == null)
                        issues.Add("`actor.pickups.2d` expects a Collider2D on the actor root.");
                    break;

                case "actor.pickups.3d":
                    if (actorRoot.GetComponent<Collider>() == null && actorRoot.GetComponent<CharacterController>() == null)
                        issues.Add("`actor.pickups.3d` expects a Collider or CharacterController on the actor root.");
                    break;

                case "actor.combat.reaction":
                    if (!HasComponentOfType(actorRoot, "NeonBlack.Gameplay.Features.Combat.HealthComponent"))
                        issues.Add("`actor.combat.reaction` expects a HealthComponent on the actor root.");
                    if (!HasComponentImplementing(actorRoot, "NeonBlack.Gameplay.Features.Combat.IActorReactionResponder"))
                        issues.Add("`actor.combat.reaction` expects an IActorReactionResponder on the actor root.");
                    break;

                case "actor.status":
                    if (!HasComponentOfType(actorRoot, "NeonBlack.Gameplay.Features.Combat.HealthComponent"))
                        issues.Add("`actor.status` expects a HealthComponent on the actor root.");
                    if (!HasComponentImplementing(actorRoot, "NeonBlack.Gameplay.Features.Combat.IActorMovementModifierReceiver"))
                        issues.Add("`actor.status` expects an IActorMovementModifierReceiver on the actor root.");
                    if (!HasComponentImplementing(actorRoot, "NeonBlack.Gameplay.Features.Combat.IActorCombatModifierReceiver") && !isEnemyActor)
                        issues.Add("`actor.status` expects an IActorCombatModifierReceiver on the actor root.");
                    if (!HasComponentImplementing(actorRoot, "NeonBlack.Gameplay.Features.Combat.IActorHealthModifierReceiver"))
                        issues.Add("`actor.status` expects an IActorHealthModifierReceiver on the actor root.");
                    break;

                case "actor.feedback":
                    if (!HasComponentOfType(actorRoot, "NeonBlack.Gameplay.Features.Combat.HealthComponent"))
                        issues.Add("`actor.feedback` should usually be paired with a HealthComponent on the actor root.");
                    if (!HasComponentImplementing(actorRoot, "NeonBlack.Gameplay.Features.Feedback.IActorFeedbackReceiver"))
                        issues.Add("`actor.feedback` should usually have at least one IActorFeedbackReceiver in the actor hierarchy.");
                    AppendActorValidationProviderIssues(actorRoot, issues);
                    break;

                case "enemy.reaction":
                    if (!HasComponentOfType(actorRoot, "NeonBlack.Gameplay.Features.Enemies.EnemyAI"))
                        issues.Add("`enemy.reaction` expects an EnemyAI on the actor root.");
                    if (!HasComponentOfType(actorRoot, "NeonBlack.Gameplay.Features.Combat.HealthComponent"))
                        issues.Add("`enemy.reaction` expects a HealthComponent on the actor root.");
                    break;

                case "enemy.ambient":
                    if (!HasComponentOfType(actorRoot, "NeonBlack.Gameplay.Features.Enemies.EnemyAI"))
                        issues.Add("`enemy.ambient` expects an EnemyAI on the actor root.");
                    break;
            }

            return issues;
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
