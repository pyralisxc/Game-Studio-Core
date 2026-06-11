using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Helper queries for gameplay systems that need active participant transforms
    /// without assuming there is only one player in the session.
    /// </summary>
    public static class ParticipantQueryUtility
    {
        private static IParticipantRoster _roster;
        private static IPlayerProvider _playerProvider;
        private static readonly List<Transform> CachedTransforms = new List<Transform>(8);

        public static void Initialize(IParticipantRoster roster, IPlayerProvider playerProvider)
        {
            _roster = roster;
            _playerProvider = playerProvider;
        }

        public static bool TryResolveRoster(out IParticipantRoster roster)
        {
            roster = _roster;
            return roster != null;
        }

        public static bool TryResolvePlayerProvider(out IPlayerProvider playerProvider)
        {
            playerProvider = _playerProvider;
            return playerProvider != null;
        }

        public static bool TryGetClosestParticipantTransform(Vector3 worldPosition, out Transform closest, out float distance)
        {
            CachedTransforms.Clear();
            GetParticipantTransforms(CachedTransforms);

            closest = null;
            distance = float.MaxValue;

            for (int i = 0; i < CachedTransforms.Count; i++)
            {
                Transform candidate = CachedTransforms[i];
                if (candidate == null)
                    continue;

                float candidateDistance = Vector3.Distance(worldPosition, candidate.position);
                if (candidateDistance >= distance)
                    continue;

                closest = candidate;
                distance = candidateDistance;
            }

            return closest != null;
        }

        public static void GetParticipantTransforms(List<Transform> results)
        {
            if (results == null)
                return;

            results.Clear();

            if (TryResolveRoster(out IParticipantRoster roster))
            {
                for (int i = 0; i < roster.Participants.Count; i++)
                {
                    ParticipantHandle participant = roster.Participants[i];
                    if (participant?.PawnInstance == null)
                        continue;

                    results.Add(participant.PawnInstance.transform);
                }

                if (results.Count > 0)
                    return;
            }

            if (TryResolvePlayerProvider(out IPlayerProvider playerProvider))
            {
                Transform playerTransform = playerProvider.GetPlayerTransform();
                if (playerTransform != null)
                    results.Add(playerTransform);
            }
        }

        public static bool TryResolveParticipant(GameObject actor, out ParticipantHandle participant)
        {
            participant = null;
            if (actor == null)
                return false;

            if (!TryResolveRoster(out IParticipantRoster roster))
                return false;

            Transform actorTransform = actor.transform;
            for (int i = 0; i < roster.Participants.Count; i++)
            {
                ParticipantHandle candidate = roster.Participants[i];
                if (candidate?.PawnInstance == null)
                    continue;

                Transform pawnTransform = candidate.PawnInstance.transform;
                if (actorTransform == pawnTransform || actorTransform.IsChildOf(pawnTransform))
                {
                    participant = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
