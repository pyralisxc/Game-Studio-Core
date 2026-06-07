using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Characters;
using Unity.Netcode;
using UnityEngine;

namespace NeonBlack.Gameplay.Networking.Participants
{
    /// <summary>
    /// Drop-in replacement for <see cref="ParticipantSpawnService"/> in online sessions.
    /// Registers spawned pawns with NGO and despawns them cleanly on removal.
    /// </summary>
    public class NetworkedParticipantSpawnService : ParticipantSpawnService
    {
        public override GameObject SpawnParticipantPawn(ParticipantHandle participant)
        {
            GameObject instance = base.SpawnParticipantPawn(participant);
            if (instance == null)
                return null;

            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                networkObject.Spawn(true);

            return instance;
        }

        protected override void DestroyPawnInstance(GameObject go)
        {
            if (go == null)
                return;

            NetworkObject networkObject = go.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                networkObject.Despawn(true);
                return;
            }

            base.DestroyPawnInstance(go);
        }
    }
}
