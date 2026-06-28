using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// Shared validation for the NGO-backed Pyralis runtime lane.
    /// </summary>
    [AuthoringContract(
        Category = "Networking",
        Surface = AuthoringSurface.RequiredSetup,
        Summary = "Shared validation for the NGO-backed Pyralis runtime lane.",
        RequiredComponentNames = new[] { nameof(NetworkManager), nameof(UnityTransport) },
        Tags = new[] { "capability:Networking" },
        Selectable = false
    )]
    public static class PyralisNetworkSetupValidator
    {
        public static List<string> GetIssues(SessionDefinition sessionDefinition, NetworkManager networkManager)
        {
            List<PyralisRuntimeValidationIssue> validationIssues = GetValidationIssues(sessionDefinition, networkManager);
            List<string> issues = new List<string>();
            for (int i = 0; i < validationIssues.Count; i++)
            {
                if (validationIssues[i] != null && !string.IsNullOrWhiteSpace(validationIssues[i].Message))
                    issues.Add(validationIssues[i].Message);
            }

            return issues;
        }

        public static List<PyralisRuntimeValidationIssue> GetValidationIssues(SessionDefinition sessionDefinition, NetworkManager networkManager)
        {
            List<PyralisRuntimeValidationIssue> issues = new List<PyralisRuntimeValidationIssue>();

            if (sessionDefinition == null)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    "SessionDefinition is required before validating network setup.",
                    targetLabel: nameof(SessionDefinition),
                    nativeAction: "Assign a SessionDefinition before validating network setup.",
                    successCheck: "Network setup has a SessionDefinition.",
                    issueCode: "NetworkSetup.SessionDefinition.Missing"));
                return issues;
            }

            if (sessionDefinition.networkMode == GameplayNetworkMode.LocalOnly)
                return issues;

            if (networkManager == null)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    "Networked sessions require a scene NetworkManager.",
                    targetLabel: nameof(NetworkManager),
                    nativeAction: "Create or assign a NetworkManager in the scene for the networked route.",
                    successCheck: "Scene has a NetworkManager for networked sessions.",
                    issueCode: "NetworkSetup.NetworkManager.Missing"));
                return issues;
            }

            if (networkManager.NetworkConfig == null)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    "NetworkManager has no NetworkConfig.",
                    targetLabel: nameof(NetworkManager),
                    nativeAction: "Inspect NetworkManager and restore its NetworkConfig.",
                    successCheck: "NetworkManager has a NetworkConfig.",
                    issueCode: "NetworkSetup.NetworkConfig.Missing"));
                return issues;
            }

            if (networkManager.NetworkConfig.NetworkTransport == null)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    "NetworkManager requires a NetworkTransport. Add UnityTransport for the supported MVP lane.",
                    targetLabel: nameof(NetworkManager),
                    nativeAction: "Add UnityTransport and assign it to NetworkManager.NetworkConfig.NetworkTransport.",
                    successCheck: "NetworkManager uses UnityTransport.",
                    issueCode: "NetworkSetup.Transport.Missing"));
            }
            else if (networkManager.NetworkConfig.NetworkTransport is not UnityTransport)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    "NetworkManager uses a non-UnityTransport transport. Pyralis MVP networking is validated against UnityTransport.",
                    targetLabel: nameof(NetworkManager),
                    nativeAction: "Replace the NetworkTransport with UnityTransport for the supported MVP lane.",
                    successCheck: "NetworkManager uses UnityTransport.",
                    issueCode: "NetworkSetup.Transport.Unsupported"));
            }

            AppendParticipantPawnIssues(sessionDefinition, networkManager, issues);
            return issues;
        }

        public static bool IsNetworkReady(SessionDefinition sessionDefinition, NetworkManager networkManager)
        {
            List<PyralisRuntimeValidationIssue> issues = GetValidationIssues(sessionDefinition, networkManager);
            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i] != null && issues[i].Severity == PyralisRuntimeValidationSeverity.Required)
                    return false;
            }

            return true;
        }

        private static void AppendParticipantPawnIssues(SessionDefinition sessionDefinition, NetworkManager networkManager, List<PyralisRuntimeValidationIssue> issues)
        {
            if (sessionDefinition.defaultParticipants == null)
                return;

            for (int i = 0; i < sessionDefinition.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = sessionDefinition.defaultParticipants[i];
                GameObject pawnPrefab = participant != null && participant.defaultPawn != null
                    ? participant.defaultPawn.pawnPrefab
                    : null;

                if (pawnPrefab == null)
                    continue;

                if (!pawnPrefab.TryGetComponent(out NetworkObject _))
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        $"Participant slot {i} pawn prefab `{pawnPrefab.name}` needs a NetworkObject for networked spawning.",
                        targetLabel: nameof(NetworkObject),
                        nativeAction: "Open the pawn prefab and add NetworkObject for networked spawning.",
                        successCheck: "Networked pawn prefab has NetworkObject.",
                        issueCode: "NetworkSetup.PawnPrefab.NetworkObjectMissing." + i));
                    continue;
                }

                if (!IsRegisteredNetworkPrefab(networkManager, pawnPrefab))
                {
                    issues.Add(PyralisRuntimeValidationIssue.Recommended(
                        $"Participant slot {i} pawn prefab `{pawnPrefab.name}` is not registered in NetworkManager Network Prefabs.",
                        targetLabel: nameof(NetworkManager),
                        nativeAction: "Inspect NetworkManager Network Prefabs and register the pawn prefab.",
                        successCheck: "NetworkManager Network Prefabs contains each networked pawn prefab.",
                        issueCode: "NetworkSetup.PawnPrefab.NotRegistered." + i));
                }
            }
        }

        private static bool IsRegisteredNetworkPrefab(NetworkManager networkManager, GameObject prefab)
        {
            IReadOnlyList<NetworkPrefab> prefabs = networkManager.NetworkConfig?.Prefabs?.Prefabs;
            if (prefabs == null)
                return false;

            for (int i = 0; i < prefabs.Count; i++)
            {
                NetworkPrefab networkPrefab = prefabs[i];
                if (networkPrefab == null)
                    continue;

                if (networkPrefab.Prefab == prefab
                    || networkPrefab.SourcePrefabToOverride == prefab
                    || networkPrefab.OverridingTargetPrefab == prefab)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
