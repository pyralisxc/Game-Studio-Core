using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace NeonBlack.Gameplay.Networking.Runtime
{
    /// <summary>
    /// Shared validation for the NGO-backed Pyralis runtime lane.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Networking,
        Relevance = "Shared validation for the NGO-backed Pyralis runtime lane.",
        FirstProof = "Start a networked session and verify transport, pawn ownership, and prefab registration.",
        FirstProofTargetId = "proof.network-ownership",
        NativeSetup = new[] { "Add NetworkManager to the scene.", "Assign UnityTransport.", "Register networked pawn prefabs." },
        RequiredComponentNames = new[] { nameof(NetworkManager), nameof(UnityTransport) }
    )]
    public static class PyralisNetworkSetupValidator
    {
        public static List<string> GetIssues(SessionDefinition sessionDefinition, NetworkManager networkManager)
        {
            List<string> issues = new List<string>();

            if (sessionDefinition == null)
            {
                issues.Add("SessionDefinition is required before validating network setup.");
                return issues;
            }

            if (sessionDefinition.networkMode == GameplayNetworkMode.LocalOnly)
                return issues;

            if (networkManager == null)
            {
                issues.Add("Networked sessions require a scene NetworkManager.");
                return issues;
            }

            if (networkManager.NetworkConfig == null)
            {
                issues.Add("NetworkManager has no NetworkConfig.");
                return issues;
            }

            if (networkManager.NetworkConfig.NetworkTransport == null)
                issues.Add("NetworkManager requires a NetworkTransport. Add UnityTransport for the supported MVP lane.");
            else if (networkManager.NetworkConfig.NetworkTransport is not UnityTransport)
                issues.Add("NetworkManager uses a non-UnityTransport transport. Pyralis MVP networking is validated against UnityTransport.");

            AppendParticipantPawnIssues(sessionDefinition, networkManager, issues);
            return issues;
        }

        public static bool IsNetworkReady(SessionDefinition sessionDefinition, NetworkManager networkManager)
        {
            return GetIssues(sessionDefinition, networkManager).Count == 0;
        }

        private static void AppendParticipantPawnIssues(SessionDefinition sessionDefinition, NetworkManager networkManager, List<string> issues)
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
                    issues.Add($"Participant slot {i} pawn prefab `{pawnPrefab.name}` needs a NetworkObject for networked spawning.");
                    continue;
                }

                if (!IsRegisteredNetworkPrefab(networkManager, pawnPrefab))
                    issues.Add($"Participant slot {i} pawn prefab `{pawnPrefab.name}` is not registered in NetworkManager Network Prefabs.");
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
